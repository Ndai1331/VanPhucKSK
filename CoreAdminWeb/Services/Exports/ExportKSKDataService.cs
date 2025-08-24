using CoreAdminWeb.Commons.Utils;
using CoreAdminWeb.Enums;
using CoreAdminWeb.Extensions;
using CoreAdminWeb.Helpers;
using CoreAdminWeb.Hubs;
using CoreAdminWeb.Model.KhamSucKhoes;
using CoreAdminWeb.Model.RequestHttps;
using CoreAdminWeb.Model.Settings;
using CoreAdminWeb.Services.BaseServices;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using System.IO.Compression;
using System.Text;


namespace CoreAdminWeb.Services.Exports
{
    public class ExportKSKDataService
    {
        private readonly IHubContext<ProgressHub> _hubContext;
        private readonly IMemoryCache _memoryCache;
        private readonly IBaseDetailService<SoKhamSucKhoeModel> _soKhamSucKhoeService;
        private readonly IBaseDetailService<KhamSucKhoeChuyenKhoaModel> _khamSucKhoeChuyenKhoaService;
        private readonly IBaseDetailService<KhamSucKhoeSanPhuKhoaModel> _khamSucKhoeSanPhuKhoaService;
        private readonly IBaseDetailService<KhamSucKhoeKetLuanModel> _khamSucKhoeKetLuanService;
        private readonly IBaseDetailService<KhamSucKhoeTheLucModel> _khamSucKhoeTheLucService;
        private readonly IBaseDetailService<KhamSucKhoeKetQuaCanLamSangModel> _khamSucKhoeKetQuaCanLamSangService;
        public ExportKSKDataService(IHubContext<ProgressHub> hubContext, IServiceScopeFactory serviceScopeFactory, IMemoryCache memoryCache)
        {
            _hubContext = hubContext;
            _memoryCache = memoryCache;
            using (var scope = serviceScopeFactory.CreateScope())
            {
                _soKhamSucKhoeService = scope.ServiceProvider.GetRequiredService<IBaseDetailService<SoKhamSucKhoeModel>>();
                _khamSucKhoeChuyenKhoaService = scope.ServiceProvider.GetRequiredService<IBaseDetailService<KhamSucKhoeChuyenKhoaModel>>();
                _khamSucKhoeKetLuanService = scope.ServiceProvider.GetRequiredService<IBaseDetailService<KhamSucKhoeKetLuanModel>>();
                _khamSucKhoeSanPhuKhoaService = scope.ServiceProvider.GetRequiredService<IBaseDetailService<KhamSucKhoeSanPhuKhoaModel>>();
                _khamSucKhoeTheLucService = scope.ServiceProvider.GetRequiredService<IBaseDetailService<KhamSucKhoeTheLucModel>>();
                _khamSucKhoeKetQuaCanLamSangService = scope.ServiceProvider.GetRequiredService<IBaseDetailService<KhamSucKhoeKetQuaCanLamSangModel>>();
            }
        }

        public async Task ExportFromExaminationWithProgressAsync(
            string connectionId,
            List<int> soKSKIds,
            SettingModel setting,
            HoSoKhamSucKhoeExportType exportType,
            string baseUrl,
            CancellationToken cancellationToken)
        {
            try
            {
                await _hubContext.Clients.Client(connectionId)
                    .SendAsync("ExportExaminationProgress", $"Đang chuẩn bị dữ liệu...", cancellationToken);

                var exportTemplateNu = exportType switch
                {
                    HoSoKhamSucKhoeExportType.ConsultationSlip => setting.thcn_nu,
                    HoSoKhamSucKhoeExportType.CheckListKsk => setting.phieu_ksk_nu,
                    _ => throw new NotSupportedException("Chưa hỗ trợ xuất template này")
                };

                var exportTemplateNam = exportType switch
                {
                    HoSoKhamSucKhoeExportType.ConsultationSlip => setting.thcn_nam,
                    HoSoKhamSucKhoeExportType.CheckListKsk => setting.phieu_ksk_nam,
                    _ => throw new NotSupportedException("Chưa hỗ trợ xuất template này")
                };

                await _hubContext.Clients.Client(connectionId)
                    .SendAsync("ExportExaminationProgress", "Đang kiểm tra mẫu xuất dữ liệu...", cancellationToken);

                if (
                    (
                        exportType == HoSoKhamSucKhoeExportType.ConsultationSlip
                        || exportType == HoSoKhamSucKhoeExportType.CheckListKsk
                    )
                    && (
                        exportTemplateNu == null
                        || exportTemplateNam == null
                    )
                )
                {
                    StringBuilder errorBuilder = new StringBuilder();
                    if (exportTemplateNam == null)
                    {
                        errorBuilder.AppendLine($"'{exportType.GetDescription()} cho Nam'");
                    }
                    if (exportTemplateNu == null)
                    {
                        if (errorBuilder.Length > 0)
                        {
                            errorBuilder.AppendLine(", ");
                        }
                        errorBuilder.AppendLine($"'{exportType.GetDescription()} cho Nữ'");
                    }

                    await _hubContext.Clients.Client(connectionId)
                    .SendAsync("ExportExaminationError", $"Mẫu {errorBuilder} không tồn tại.", cancellationToken);
                    return;
                }

                byte[]? docTemplate1Bytes = null;
                byte[]? docTemplate2Bytes = null;
                if (exportTemplateNam != null)
                {
                    using var http = new HttpClient();
                    docTemplate1Bytes = await http.GetByteArrayAsync($"{baseUrl}assets/{exportTemplateNam.id}");
                }
                if (exportTemplateNu != null)
                {
                    using var http = new HttpClient();
                    docTemplate2Bytes = await http.GetByteArrayAsync($"{baseUrl}assets/{exportTemplateNu.id}");
                }

                if (exportTemplateNam != null && docTemplate1Bytes == null || exportTemplateNu != null && docTemplate2Bytes == null)
                {
                    StringBuilder errorBuilder = new StringBuilder();
                    if (exportTemplateNam == null)
                    {
                        errorBuilder.AppendLine($"'{exportType.GetDescription()} cho Nam'");
                    }
                    if (exportTemplateNu == null)
                    {
                        if (errorBuilder.Length > 0)
                        {
                            errorBuilder.AppendLine(", ");
                        }
                        errorBuilder.AppendLine($"'{exportType.GetDescription()} cho Nữ'");
                    }

                    await _hubContext.Clients.Client(connectionId)
                    .SendAsync("ExportExaminationError", $"Mẫu {errorBuilder} không tồn tại hoặc đã bị xóa.", cancellationToken);
                    return;
                }

                // Tối ưu batch size khi truy vấn và ghi dữ liệu
                int batchSize = soKSKIds.Count switch
                {
                    >= 10000 => 1000,
                    >= 5000 => 500,
                    _ => 200
                };

                List<KhamSucKhoeChuyenKhoaModel>? khamSucKhoeChuyenKhoas = null;
                List<KhamSucKhoeSanPhuKhoaModel>? khamSucKhoeSanPhuKhoas = null;
                List<KhamSucKhoeKetLuanModel>? khamSucKhoeKetLuans = null;
                List<KhamSucKhoeTheLucModel>? khamSucKhoeTheLucs = null;
                List<KhamSucKhoeKetQuaCanLamSangModel>? khamSucKhoeKetQuaCanLamSangs = null;

                // Lấy dữ liệu chính theo batch lớn
                List<SoKhamSucKhoeModel> soKhamSucKhoes = await BatchQueryAsync(
                    ids => _soKhamSucKhoeService.GetAllAsync($"filter[_and][][id][_in]={string.Join(",", ids)}"),
                    soKSKIds, batchSize
                );

                if (soKhamSucKhoes == null || soKhamSucKhoes.Count == 0)
                {
                    await _hubContext.Clients.Client(connectionId)
                        .SendAsync("ExportExaminationError", "Không có dữ liệu khám sức khỏe để xuất.", cancellationToken);
                    return;
                }

                // Lấy dữ liệu liên quan song song (nếu cần)
                if (exportType == HoSoKhamSucKhoeExportType.ConsultationSlip)
                {
                    var chuyenKhoaTask = BatchQueryAsync(
                        ids => _khamSucKhoeChuyenKhoaService.GetAllAsync($"filter[_and][][ma_luot_kham][_in]={string.Join(",", ids)}"),
                        soKSKIds, batchSize
                    );
                    var sanPhuKhoaTask = BatchQueryAsync(
                        ids => _khamSucKhoeSanPhuKhoaService.GetAllAsync($"filter[_and][][ma_luot_kham][_in]={string.Join(",", ids)}"),
                        soKSKIds, batchSize
                    );
                    var ketLuanTask = BatchQueryAsync(
                        ids => _khamSucKhoeKetLuanService.GetAllAsync($"filter[_and][][ma_luot_kham][_in]={string.Join(",", ids)}"),
                        soKSKIds, batchSize
                    );
                    var theLucTask = BatchQueryAsync(
                        ids => _khamSucKhoeTheLucService.GetAllAsync($"filter[_and][][ma_luot_kham][_in]={string.Join(",", ids)}"),
                        soKSKIds, batchSize
                    );
                    var kqCLSTask = BatchQueryAsync(
                        ids => _khamSucKhoeKetQuaCanLamSangService.GetAllAsync($"filter[_and][][ma_luot_kham][_in]={string.Join(",", ids)}"),
                        soKSKIds, batchSize
                    );

                    await Task.WhenAll(chuyenKhoaTask, sanPhuKhoaTask, ketLuanTask, theLucTask, kqCLSTask);

                    khamSucKhoeChuyenKhoas = chuyenKhoaTask.Result;
                    khamSucKhoeSanPhuKhoas = sanPhuKhoaTask.Result;
                    khamSucKhoeKetLuans = ketLuanTask.Result;
                    khamSucKhoeTheLucs = theLucTask.Result;
                    khamSucKhoeKetQuaCanLamSangs = kqCLSTask.Result;
                }

                // Chuẩn bị thư mục lưu file
                var dateNow = DateTime.Now;
                var baseFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "exports", "kham_suc_khoe");
                var savePath = Path.Combine(baseFolder, $"{exportType}_{dateNow:yyyyMMddHHmmssfff}");
                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }

                int totalRecords = soKhamSucKhoes.Count;
                int processed = 0;
                List<string> fileNames = new List<string>(totalRecords);

                // Duyệt theo batch để giảm memory pressure
                foreach (var batch in soKhamSucKhoes.Chunk(batchSize))
                {
                    var batchList = batch.ToList();
                    var tasks = batchList.Select(async item =>
                    {
                        processed++;
                        if (cancellationToken.IsCancellationRequested)
                        {
                            await _hubContext.Clients.Client(connectionId)
                            .SendAsync("ExportExaminationError", "Xuất dữ liệu đã bị hủy.", cancellationToken);
                            return;
                        }

                        var usingDocTemplate = item.benh_nhan?.gioi_tinh == GioiTinh.Nam ? docTemplate1Bytes : docTemplate2Bytes;
                        if (usingDocTemplate == null)
                        {
                            await _hubContext.Clients.Client(connectionId)
                            .SendAsync("ExportExaminationError", $"Không tìm thấy mẫu xuất cho giới tính {item.benh_nhan?.gioi_tinh?.GetDescription()}.", cancellationToken);
                            return;
                        }

                        var tempTemplatePath = Path.Combine(Path.GetTempPath(), $"{item.ma_luot_kham}_temp.docx");
                        await File.WriteAllBytesAsync(tempTemplatePath, usingDocTemplate, cancellationToken);

                        // Xử lý file Word (có thể tối ưu hơn nếu dùng thư viện hỗ trợ stream)
                        using (var doc = WordprocessingDocument.Open(tempTemplatePath, true))
                        {
                            switch (exportType)
                            {
                                case HoSoKhamSucKhoeExportType.CheckListKsk:
                                    try
                                    {
                                        var qrCode = QRHelper.GenerateQRCode($"{item.ma_luot_kham}");
                                        doc.ReplaceText(new Dictionary<string, string>
                                        {
                                            { "<<HoVaTen>>", $"{item.benh_nhan?.full_name}" },
                                            { "<<GioiTinh>>", $"{item.benh_nhan?.gioi_tinh?.GetDescription()}" },
                                            { "<<STT_KSK>>", $"{item.sort}" },
                                            { "<<MaLuotKham>>", $"{item.ma_luot_kham}" },
                                            { "<<NgaySinh>>", $"{item.benh_nhan?.ngay_sinh:dd/MM/yyyy}" },
                                            { "<<X>>", $"{DateTimeUtil.CalculateAge(item.benh_nhan?.ngay_sinh?.Date)}" },
                                            { "<<SoDinhDanh>>", $"{item.benh_nhan?.so_dinh_danh}" },
                                            { "<<SoDienThoai>>", $"{item.benh_nhan?.so_dien_thoai}" }
                                        });
                                        doc.ReplaceImage("<<QR>>", qrCode, 500000, 500000);

                                    }
                                    catch (Exception ex)
                                    {
                                        var errorDetails = $"[CheckListKsk Processing Error] Item: {item.ma_luot_kham}, " +
                                                         $"Line: {ex.StackTrace?.Split('\n').FirstOrDefault(x => x.Contains("ExportKSKDataService.cs"))?.Trim() ?? "Unknown"}, " +
                                                         $"Method: {ex.TargetSite?.Name ?? "Unknown"}, " +
                                                         $"Library: {ex.TargetSite?.DeclaringType?.Assembly.GetName().Name ?? "Unknown"}, " +
                                                         $"Error: {ex.Message}, " +
                                                         $"Inner Exception: {ex.InnerException?.Message ?? "None"}";

                                        Console.WriteLine(errorDetails);
                                        await _hubContext.Clients.Client(connectionId)
                                            .SendAsync("ExportExaminationError", $"Lỗi xử lý CheckListKsk: {ex.Message}", cancellationToken);
                                        return;
                                    }
                                    break;
                                case HoSoKhamSucKhoeExportType.ConsultationSlip:
                                    try
                                    {
                                        var theLuc = khamSucKhoeTheLucs?.FirstOrDefault(x => x.ma_luot_kham == item.ma_luot_kham);
                                        var cls = khamSucKhoeChuyenKhoas?.FirstOrDefault(x => x.ma_luot_kham == item.ma_luot_kham);
                                        var sanPhuKhoa = khamSucKhoeSanPhuKhoas?.FirstOrDefault(x => x.ma_luot_kham == item.ma_luot_kham);
                                        var kqcls = khamSucKhoeKetQuaCanLamSangs?.Where(x => x.luot_kham?.ma_luot_kham == item.ma_luot_kham && !string.IsNullOrEmpty(x.ket_qua)).ToList();
                                        var ketLuan = khamSucKhoeKetLuans?.FirstOrDefault(x => x.ma_luot_kham == item.ma_luot_kham);


                                        doc.ReplaceText(new Dictionary<string, string>
                                    {
                                        { "<<HoVaTen>>", $"{item.benh_nhan?.full_name}" },
                                        { "<<GioiTinh>>", $"{item.benh_nhan?.gioi_tinh?.GetDescription()}" },
                                        { "<<CongTy>>", $"{item.MaDotKham?.ma_hop_dong_ksk?.cong_ty?.code}" },
                                        { "<<NamSinh>>", $"{item.benh_nhan?.ngay_sinh:yyyy}" },
                                        { "<<ChieuCao>>", $"{theLuc?.chieu_cao}" },
                                        { "<<CanNang>>", $"{theLuc?.can_nang}" },
                                        { "<<BMI>>", $"{theLuc?.bmi}" },
                                        { "<<kq_tuanhoan>>", $"{cls?.kq_nk_tuan_hoan}" },
                                        { "<<kq_hohap>>", $"{cls?.kq_nk_ho_hap}" },
                                        { "<<kq_tieuhoa>>", $"{cls?.kq_nk_tieu_hoa}" },
                                        { "<<kq_noitiet>>", $"{cls?.kq_nk_noi_tiet}" },
                                        { "<<kq_thantietnieu>>", $"{cls?.kq_nk_than_tiet_nieu}" },
                                        { "<<kq_coxuongkhop>>", $"{cls?.kq_nk_co_xuong_khop}" },
                                        { "<<kq_thankinh>>", $"{cls?.kq_nk_than_kinh}" },
                                        { "<<kq_ngoaikhoa>>", $"{cls?.kq_ngoai_khoa}" },
                                        { "<<kq_taimuihong>>", $"{cls?.benh_tai_mui_hong}" },
                                        { "<<kq_dalieu>>", $"{cls?.kq_da_lieu}" },
                                        { "<<kq_mat>>", $"{cls?.benh_mat}" },
                                        { "<<kq_ranghammat>>", $"{cls?.benh_rhm}" },
                                        { "<<kq_tamthan>>", $"{cls?.kq_nk_tam_than}" },
                                        { "<<kq_sanphukhoa>>", $"{sanPhuKhoa?.ket_qua}" },
                                        { "<<kl_phanloai>>", $"{ketLuan?.phan_loai_suc_khoe?.name}" },
                                        { "<<kl_ketluan>>", $"{ketLuan?.benh_tat_ket_luan}" },
                                        { "<<kl_denghi>>", $"{ketLuan?.de_nghi}" }

                                    });
                                        if (kqcls != null && kqcls.Any())
                                        {
                                            var items_kqcls = kqcls.Select(c => new CanLamSangItem(c.type?.GetDescriptionFromString<KetQuaCanLamSang>() ?? string.Empty, c.ket_qua ?? string.Empty)).ToList();

                                            // Build formatted text for chiDinh with bullet points
                                            StringBuilder chiDinhFormatted = new StringBuilder();
                                            for (int i = 0; i < items_kqcls.Count; i++)
                                            {
                                                chiDinhFormatted.AppendLine($"• {items_kqcls[i].TenChiDinh}");
                                            }

                                            // Build formatted text for ketQua with bullet points
                                            StringBuilder ketQuaFormatted = new StringBuilder();
                                            for (int i = 0; i < items_kqcls.Count; i++)
                                            {
                                                ketQuaFormatted.AppendLine($"• {items_kqcls[i].KetQua}");
                                            }

                                            // Replace placeholders with formatted text
                                            doc.ReplaceText(new Dictionary<string, string>
                                        {
                                            { "<<TenChiDinh>>", chiDinhFormatted.ToString() },
                                            { "<<kq_canlamsang>>", ketQuaFormatted.ToString() }
                                        });
                                        }
                                        else
                                        {
                                            doc.ReplaceText(new Dictionary<string, string>
                                        {
                                            { "<<TenChiDinh>>", "" },
                                            { "<<kq_canlamsang>>", "" }
                                        });
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        var errorDetails = $"[ConsultationSlip Processing Error] Item: {item.ma_luot_kham}, " +
                                                         $"Line: {ex.StackTrace?.Split('\n').FirstOrDefault(x => x.Contains("ExportKSKDataService.cs"))?.Trim() ?? "Unknown"}, " +
                                                         $"Method: {ex.TargetSite?.Name ?? "Unknown"}, " +
                                                         $"Library: {ex.TargetSite?.DeclaringType?.Assembly.GetName().Name ?? "Unknown"}, " +
                                                         $"Error: {ex.Message}, " +
                                                         $"Inner Exception: {ex.InnerException?.Message ?? "None"}";

                                        Console.WriteLine(errorDetails);
                                        await _hubContext.Clients.Client(connectionId)
                                            .SendAsync("ExportExaminationError", $"Lỗi xử lý ConsultationSlip: {ex.Message}", cancellationToken);
                                        return;
                                    }

                                    break;
                            }
                        }

                        string filename = $"{item.benh_nhan?.full_name}_{item.ma_luot_kham}_{exportType.GetDescription()}_{(item.benh_nhan?.gioi_tinh == GioiTinh.Nam ? "Nam" : "Nu")}_{DateTime.Now:yyyyMMdd_HHmmss}.docx".ToNormalChar();
                        var fullFilePath = Path.Combine(savePath, filename);
                        File.Copy(tempTemplatePath, fullFilePath, true);
                        if (File.Exists(tempTemplatePath))
                        {
                            File.Delete(tempTemplatePath);
                        }

                        fileNames.Add(filename);

                        // Chỉ gửi progress mỗi 100 bản ghi
                        if (processed % 100 == 0 || processed == totalRecords)
                        {
                            await _hubContext.Clients.Client(connectionId)
                                .SendAsync("ExportExaminationProgress", $"Đang xuất dữ liệu {processed}/{totalRecords}", cancellationToken);
                        }
                    });

                    // Chạy song song trong batch nhỏ (giới hạn số lượng để tránh quá tải IO)
                    await Task.WhenAll(tasks);
                }

                if (fileNames.Count == 0)
                {
                    await _hubContext.Clients.Client(connectionId)
                        .SendAsync("ExportExaminationError", "Không có dữ liệu để xuất.", cancellationToken);
                    return;
                }

                await _hubContext.Clients.Client(connectionId)
                    .SendAsync("ExportExaminationProgress", $"Đang chuẩn bị tập tin nén...", cancellationToken);

                await Task.Delay(200, cancellationToken);

                string zipFileName = $"{exportType.GetDescription()}_{DateTime.Now:yyyyMMdd_HHmmss}".ToNormalChar();
                string zipPath = Path.Combine(baseFolder, $"{zipFileName}.zip");
                ZipFile.CreateFromDirectory(savePath, zipPath, CompressionLevel.Fastest, true);

                if (Directory.Exists(savePath))
                {
                    GC.WaitForPendingFinalizers();
                    Directory.Delete(savePath, true);
                }

                var ticketId = Guid.NewGuid().ToString("N");
                _memoryCache.Set(ticketId, zipPath, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });

                await _hubContext.Clients.Client(connectionId).SendAsync(
                    "ExportExaminationCompleted",
                    new
                    {
                        Message = "Tập tin nén đã được chuẩn bị xong. Đang chuẩn bị tải về.",
                        RelativeUrl = $"/downloads/{ticketId}",
                        TicketId = ticketId,
                        FileName = Path.GetFileName(zipPath),
                        Size = new FileInfo(zipPath).Length
                    },
                    cancellationToken
                );
            }
            catch (Exception ex)
            {
                await _hubContext.Clients.Client(connectionId)
                    .SendAsync("ExportExaminationError", $"Lỗi khi xuất tập tin: {ex.Message}", cancellationToken);
            }
        }

        static async Task<List<T>> BatchQueryAsync<T, TValue>(Func<List<TValue>, Task<RequestHttpResponse<List<T>>>> queryFunc, List<TValue> ids, int batchSize = 200)
        {
            var results = new List<T>();
            foreach (var batch in ids.Chunk(batchSize))
            {
                var res = await queryFunc(batch.ToList());
                if (res.IsSuccess && res.Data != null)
                {
                    results.AddRange(res.Data);
                }
            }
            return results;
        }
        // Add a method to remove the file after download by ticketId
        public bool RemoveExportedFile(string ticketId)
        {
            if (_memoryCache.TryGetValue(ticketId, out string? filePath) && !string.IsNullOrEmpty(filePath))
            {
                try
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                    _memoryCache.Remove(ticketId);
                    return true;
                }
                catch
                {
                    // Optionally log error
                    return false;
                }
            }
            return false;
        }

        public record CanLamSangItem(string TenChiDinh, string KetQua);
    }
}
