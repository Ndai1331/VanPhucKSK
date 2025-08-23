using CoreAdminWeb.Commons.Utils;
using CoreAdminWeb.Enums;
using CoreAdminWeb.Extensions;
using CoreAdminWeb.Helpers;
using CoreAdminWeb.Hubs;
using CoreAdminWeb.Model.KhamSucKhoes;
using CoreAdminWeb.Model.RequestHttps;
using CoreAdminWeb.Model.Settings;
using CoreAdminWeb.Services.BaseServices;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using System.IO.Compression;
using System.Text;
using Xceed.Document.NET;
using Xceed.Words.NET;
using CoreAdminWeb.Helpers;

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


        public async Task ExportFromExaminationWithProgressAsync(string connectionId,
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

                List<KhamSucKhoeChuyenKhoaModel>? khamSucKhoeChuyenKhoas = default;
                List<KhamSucKhoeSanPhuKhoaModel>? khamSucKhoeSanPhuKhoas = default;
                List<KhamSucKhoeKetLuanModel>? khamSucKhoeKetLuans = default;
                List<KhamSucKhoeTheLucModel>? khamSucKhoeTheLucs = default;
                List<KhamSucKhoeKetQuaCanLamSangModel>? khamSucKhoeKetQuaCanLamSangs = default;
                List<SoKhamSucKhoeModel> soKhamSucKhoes = await BatchQueryAsync(
                    ids => _soKhamSucKhoeService.GetAllAsync($"filter[_and][][id][_in]={string.Join(",", ids)}"),
                    soKSKIds
                );

                if (soKhamSucKhoes == null || !soKhamSucKhoes.Any())
                {
                    await _hubContext.Clients.Client(connectionId)
                    .SendAsync("ExportExaminationError", "Không có dữ liệu khám sức khỏe để xuất.", cancellationToken);
                    return;
                }

                if (exportType == HoSoKhamSucKhoeExportType.ConsultationSlip)
                {
                    var chuyenKhoaTask = BatchQueryAsync(
                        ids => _khamSucKhoeChuyenKhoaService.GetAllAsync($"filter[_and][][ma_luot_kham][_in]={string.Join(",", ids)}"),
                        soKSKIds
                    );
                    var sanPhuKhoaTask = BatchQueryAsync(
                        ids => _khamSucKhoeSanPhuKhoaService.GetAllAsync($"filter[_and][][ma_luot_kham][_in]={string.Join(",", ids)}"),
                        soKSKIds
                    );
                    var ketLuanTask = BatchQueryAsync(
                        ids => _khamSucKhoeKetLuanService.GetAllAsync($"filter[_and][][ma_luot_kham][_in]={string.Join(",", ids)}"),
                        soKSKIds
                    );
                    var theLucTask = BatchQueryAsync(
                        ids => _khamSucKhoeTheLucService.GetAllAsync($"filter[_and][][ma_luot_kham][_in]={string.Join(",", ids)}"),
                        soKSKIds
                    );
                    var kqCLSTask = BatchQueryAsync(
                        ids => _khamSucKhoeKetQuaCanLamSangService.GetAllAsync($"filter[_and][][ma_luot_kham][_in]={string.Join(",", ids)}"),
                        soKSKIds
                    );

                    await Task.WhenAll(chuyenKhoaTask, sanPhuKhoaTask, ketLuanTask, theLucTask, kqCLSTask);

                    khamSucKhoeChuyenKhoas = chuyenKhoaTask.Result;
                    khamSucKhoeSanPhuKhoas = sanPhuKhoaTask.Result;
                    khamSucKhoeKetLuans = ketLuanTask.Result;
                    khamSucKhoeTheLucs = theLucTask.Result;
                    khamSucKhoeKetQuaCanLamSangs = kqCLSTask.Result;
                }

                await _hubContext.Clients.Client(connectionId)
                    .SendAsync("ExportExaminationProgress", "Đang kiểm tra mẫu xuất dữ liệu...", cancellationToken);

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

                int startIndex = 0;
                int totalRecords = soKhamSucKhoes.Count;
                List<string> fileNames = new List<string>();
                var dateNow = DateTime.Now;
                var baseFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "exports", "kham_suc_khoe");
                if (!Directory.Exists(baseFolder))
                {
                    Directory.CreateDirectory(baseFolder);
                }
                foreach (var item in soKhamSucKhoes)
                {
                    startIndex++;
                    await _hubContext.Clients.Client(connectionId)
                        .SendAsync("ExportExaminationProgress", $"Đang thực hiện xuất dữ liệu {startIndex} trên tổng số {totalRecords}", cancellationToken);

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
                        continue;
                    }

                    using (var doc = DocX.Load(new MemoryStream(usingDocTemplate)))
                    {
                        doc.ReplaceText(new StringReplaceTextOptions()
                        {
                            SearchValue = "<<HoVaTen>>",
                            NewValue = $"{item.benh_nhan?.full_name}"
                        });
                        doc.ReplaceText(new StringReplaceTextOptions()
                        {
                            SearchValue = "<<GioiTinh>>",
                            NewValue = $"{item.benh_nhan?.gioi_tinh?.GetDescription()}"
                        });

                        switch (exportType)
                        {
                            case HoSoKhamSucKhoeExportType.CheckListKsk:
                                doc.ReplaceText(new StringReplaceTextOptions() { SearchValue = "<<STT_KSK>>", NewValue = $"{item.sort}" });
                                doc.ReplaceText(new StringReplaceTextOptions() { SearchValue = "<<MaLuotKham>>", NewValue = $"{item.ma_luot_kham}" });
                                doc.ReplaceText(new StringReplaceTextOptions() { SearchValue = "<<NgaySinh>>", NewValue = $"{item.benh_nhan?.ngay_sinh:dd/MM/yyyy}" });
                                doc.ReplaceText(new StringReplaceTextOptions() { SearchValue = "<<X>>", NewValue = $"{DateTimeUtil.CalculateAge(item.benh_nhan?.ngay_sinh?.Date)}" });
                                doc.ReplaceText(new StringReplaceTextOptions() { SearchValue = "<<X >>", NewValue = $"{DateTimeUtil.CalculateAge(item.benh_nhan?.ngay_sinh?.Date)}" });
                                doc.ReplaceText(new StringReplaceTextOptions() { SearchValue = "<<SoDinhDanh>>", NewValue = $"{item.benh_nhan?.so_dinh_danh}" });
                                doc.ReplaceText(new StringReplaceTextOptions() { SearchValue = "<<SoDinhDanh >>", NewValue = $"{item.benh_nhan?.so_dinh_danh}" });
                                doc.ReplaceText(new StringReplaceTextOptions() { SearchValue = "<<SoDienThoai>>", NewValue = $"{item.benh_nhan?.so_dien_thoai}" });

                                var qrCode = QRHelper.GenerateQRCode($"{item.ma_luot_kham}");
                                var filePath = Path.Combine(baseFolder, $"{exportType}_{dateNow:yyyyMMddHHmmssfff}_QR", $"{item.ma_luot_kham}.png");
                                await File.WriteAllBytesAsync(filePath, qrCode);

                                var image = doc.AddImage(filePath);
                                var picture = image.CreatePicture(); // Không truyền size, giữ mặc định
                                doc.ReplaceTextWithObject(new ObjectReplaceTextOptions { SearchValue = "<<QR>>", NewObject = picture });

                                //using (var ms = new MemoryStream(qrCode))
                                //{
                                //    var image = doc.AddImage(ms, "image/png");
                                //    var picture = image.CreatePicture(60, 60);
                                //    doc.ReplaceTextWithObject(new ObjectReplaceTextOptions() { SearchValue = "<<QR>>", NewObject = picture });
                                //}
                                break;
                            case HoSoKhamSucKhoeExportType.ConsultationSlip:
                                doc.ReplaceText(new StringReplaceTextOptions() { SearchValue = "<<CongTy>>", NewValue = $"{item.MaDotKham?.ma_hop_dong_ksk?.cong_ty?.code}" });
                                doc.ReplaceText(new StringReplaceTextOptions() { SearchValue = "<<NamSinh>>", NewValue = $"{item.benh_nhan?.ngay_sinh:yyyy}" });

                                var theLuc = khamSucKhoeTheLucs?.FirstOrDefault(x => x.ma_luot_kham == item.ma_luot_kham);
                                doc.ReplaceText(new StringReplaceTextOptions() { SearchValue = "<<ChieuCao>>", NewValue = $"{theLuc?.chieu_cao}" });
                                doc.ReplaceText(new StringReplaceTextOptions() { SearchValue = "<<CanNang>>", NewValue = $"{theLuc?.can_nang}" });
                                doc.ReplaceText(new StringReplaceTextOptions() { SearchValue = "<<BMI>>", NewValue = $"{theLuc?.bmi}" });

                                var cls = khamSucKhoeChuyenKhoas?.FirstOrDefault(x => x.ma_luot_kham == item.ma_luot_kham);
                                doc.ReplaceText(new StringReplaceTextOptions() { SearchValue = "<<kq_tuanhoan>>", NewValue = $"{cls?.kq_nk_tuan_hoan}" });
                                doc.ReplaceText(new StringReplaceTextOptions() { SearchValue = "<<kq_hohap>>", NewValue = $"{cls?.kq_nk_ho_hap}" });
                                doc.ReplaceText(new StringReplaceTextOptions() { SearchValue = "<<kq_tieuhoa>>", NewValue = $"{cls?.kq_nk_tieu_hoa}" });
                                doc.ReplaceText(new StringReplaceTextOptions() { SearchValue = "<<kq_noitiet>>", NewValue = $"{cls?.kq_nk_noi_tiet}" });
                                doc.ReplaceText(new StringReplaceTextOptions() { SearchValue = "<<kq_thantietnieu>>", NewValue = $"{cls?.kq_nk_than_tiet_nieu}" });
                                doc.ReplaceText(new StringReplaceTextOptions() { SearchValue = "<<kq_thantietnieu>>", NewValue = $"{cls?.kq_nk_than_tiet_nieu}" });
                                doc.ReplaceText(new StringReplaceTextOptions() { SearchValue = "<<kq_coxuongkhop>>", NewValue = $"{cls?.kq_nk_co_xuong_khop}" });
                                doc.ReplaceText(new StringReplaceTextOptions() { SearchValue = "<<kq_coxuongkhop>>", NewValue = $"{cls?.kq_nk_co_xuong_khop}" });
                                doc.ReplaceText(new StringReplaceTextOptions() { SearchValue = "<<kq_thankinh>>", NewValue = $"{cls?.kq_nk_than_kinh}" });
                                doc.ReplaceText(new StringReplaceTextOptions() { SearchValue = "<<kq_ngoaikhoa>>", NewValue = $"{cls?.kq_ngoai_khoa}" });
                                doc.ReplaceText(new StringReplaceTextOptions() { SearchValue = "<<kq_taimuihong>>", NewValue = $"{cls?.benh_tai_mui_hong}" });
                                doc.ReplaceText(new StringReplaceTextOptions() { SearchValue = "<<kq_dalieu>>", NewValue = $"{cls?.kq_da_lieu}" });
                                doc.ReplaceText(new StringReplaceTextOptions() { SearchValue = "<<kq_mat>>", NewValue = $"{cls?.benh_mat}" });
                                doc.ReplaceText(new StringReplaceTextOptions() { SearchValue = "<<kq_ranghammat>>", NewValue = $"{cls?.benh_rhm}" });
                                doc.ReplaceText(new StringReplaceTextOptions() { SearchValue = "<<kq_tamthan>>", NewValue = $"{cls?.kq_nk_tam_than}" });

                                var sanPhuKhoa = khamSucKhoeSanPhuKhoas?.FirstOrDefault(x => x.ma_luot_kham == item.ma_luot_kham);
                                doc.ReplaceText(new StringReplaceTextOptions() { SearchValue = "<<kq_sanphukhoa>>", NewValue = $"{sanPhuKhoa?.ket_qua}" });

                                var kqcls = khamSucKhoeKetQuaCanLamSangs?.Where(x => x.luot_kham?.ma_luot_kham == item.ma_luot_kham && !string.IsNullOrEmpty(x.ket_qua)).ToList();
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
                                    doc.ReplaceText(new StringReplaceTextOptions() { SearchValue = "<<TenChiDinh>>", NewValue = chiDinhFormatted.ToString() });
                                    doc.ReplaceText(new StringReplaceTextOptions() { SearchValue = "<<kq_canlamsang>>", NewValue = ketQuaFormatted.ToString() });
                                }
                                else
                                {
                                    doc.ReplaceText(new StringReplaceTextOptions() { SearchValue = "<<TenChiDinh>>", NewValue = "" });
                                    doc.ReplaceText(new StringReplaceTextOptions() { SearchValue = "<<kq_canlamsang>>", NewValue = "" });
                                }

                                var ketLuan = khamSucKhoeKetLuans?.FirstOrDefault(x => x.ma_luot_kham == item.ma_luot_kham);
                                doc.ReplaceText(new StringReplaceTextOptions() { SearchValue = "<<kl_phanloai>>", NewValue = $"{ketLuan?.phan_loai_suc_khoe?.name}" });
                                doc.ReplaceText(new StringReplaceTextOptions() { SearchValue = "<<kl_ketluan>>", NewValue = $"{ketLuan?.benh_tat_ket_luan}" });
                                doc.ReplaceText(new StringReplaceTextOptions() { SearchValue = "<<kl_denghi>>", NewValue = $"{ketLuan?.de_nghi}" });
                                break;
                        }


                        string filename = $"{item.benh_nhan?.full_name}_{item.ma_luot_kham}_{exportType.GetDescription()}_{(item.benh_nhan?.gioi_tinh == GioiTinh.Nam ? "Nam" : "Nu")}_{DateTime.Now:yyyyMMdd_HHmmss}.docx".ToNormalChar();
                        var savePath = Path.Combine(baseFolder, $"{exportType}_{dateNow:yyyyMMddHHmmssfff}");
                        if (!Directory.Exists(savePath))
                        {
                            Directory.CreateDirectory(savePath);
                        }
                        doc.SaveAs(Path.Combine(savePath, filename));
                        fileNames.Add(filename);
                    }

                }

                if (fileNames.Count == 0)
                {
                    await _hubContext.Clients.Client(connectionId)
                    .SendAsync("ExportExaminationError", "Không có dữ liệu để xuất.", cancellationToken);
                    return;
                }

                await _hubContext.Clients.Client(connectionId)
                    .SendAsync("ExportExaminationProgress", $"Đang chuẩn bị tập tin nén...", cancellationToken);

                string zipFileName = $"{exportType.GetDescription()}_{DateTime.Now:yyyyMMdd_HHmmss}".ToNormalChar();
                string zipPath = Path.Combine(baseFolder, $"{zipFileName}.zip");
                ZipFile.CreateFromDirectory(Path.Combine(baseFolder, $"{exportType}_{dateNow:yyyyMMddHHmmssfff}"), zipPath, CompressionLevel.Fastest, true);

                if (Directory.Exists(Path.Combine(baseFolder, $"{exportType}_{dateNow:yyyyMMddHHmmssfff}")))
                {
                    GC.WaitForPendingFinalizers();
                    Directory.Delete(Path.Combine(baseFolder, $"{exportType}_{dateNow:yyyyMMddHHmmssfff}"), true);
                    Directory.Delete(Path.Combine(baseFolder, $"{exportType}_{dateNow:yyyyMMddHHmmssfff}_QR"), true);
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
