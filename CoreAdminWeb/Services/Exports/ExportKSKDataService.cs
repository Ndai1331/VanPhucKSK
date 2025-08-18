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

namespace CoreAdminWeb.Services.Imports
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

                if (exportType == HoSoKhamSucKhoeExportType.CónultationSlip)
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
                    HoSoKhamSucKhoeExportType.CónultationSlip => setting.thcn_nu,
                    HoSoKhamSucKhoeExportType.CheckListKsk => setting.phieu_ksk_nu,
                    _ => throw new NotSupportedException("Unsupported export type")
                };

                var exportTemplateNam = exportType switch
                {
                    HoSoKhamSucKhoeExportType.CónultationSlip => setting.thcn_nam,
                    HoSoKhamSucKhoeExportType.CheckListKsk => setting.phieu_ksk_nam,
                    _ => throw new NotSupportedException("Unsupported export type")
                };

                if (
                    (
                        exportType == HoSoKhamSucKhoeExportType.CónultationSlip
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
                    using (var inStream = new MemoryStream(usingDocTemplate, writable: false))
                    {
                        var spDoc = new Spire.Doc.Document();
                        spDoc.LoadFromStream(inStream, Spire.Doc.FileFormat.Docx);

                        DocumentHelper.FindAndReplaceString(ref spDoc, "<<HoVaTen>>", $"{item.benh_nhan?.full_name}");
                        DocumentHelper.FindAndReplaceString(ref spDoc, "<<GioiTinh>>", $"{item.benh_nhan?.gioi_tinh?.GetDescription()}");
                        switch (exportType)
                        {
                            case HoSoKhamSucKhoeExportType.CheckListKsk:
                                DocumentHelper.FindAndReplaceString(ref spDoc, "<<STT_KSK>>", $"{item.sort}");
                                DocumentHelper.FindAndReplaceString(ref spDoc, "<<MaLuotKham>>", $"{item.ma_luot_kham}");
                                DocumentHelper.FindAndReplaceString(ref spDoc, "<<NgaySinh>>", $"{item.benh_nhan?.ngay_sinh:dd/MM/yyyy}");
                                DocumentHelper.FindAndReplaceString(ref spDoc, "<<X>>", $"{DateTimeUtil.CalculateAge(item.benh_nhan?.ngay_sinh?.Date)}");
                                DocumentHelper.FindAndReplaceString(ref spDoc, "<<X >>", $"{DateTimeUtil.CalculateAge(item.benh_nhan?.ngay_sinh?.Date)}");
                                DocumentHelper.FindAndReplaceString(ref spDoc, "<<SoDinhDanh>>", $"{item.benh_nhan?.so_dinh_danh}");
                                DocumentHelper.FindAndReplaceString(ref spDoc, "<<SoDinhDanh >>", $"{item.benh_nhan?.so_dinh_danh}");
                                DocumentHelper.FindAndReplaceString(ref spDoc, "<<SoDienThoai>>", $"{item.benh_nhan?.so_dien_thoai}");
                                DocumentHelper.FindAndReplaceString(ref spDoc, DocumentHelper.QRCodeReplaceCode, $"{item.ma_luot_kham}");
                                break;
                            case HoSoKhamSucKhoeExportType.CónultationSlip:
                                DocumentHelper.FindAndReplaceString(ref spDoc, "<<CongTy>>", $"{item.MaDotKham?.ma_hop_dong_ksk?.cong_ty?.code}");
                                DocumentHelper.FindAndReplaceString(ref spDoc, "<<NamSinh>>", $"{item.benh_nhan?.ngay_sinh:yyyy}");

                                var theLuc = khamSucKhoeTheLucs?.FirstOrDefault(x => x.ma_luot_kham == item.ma_luot_kham);
                                DocumentHelper.FindAndReplaceString(ref spDoc, "<<ChieuCao>>", $"{theLuc?.chieu_cao}");
                                DocumentHelper.FindAndReplaceString(ref spDoc, "<<CanNang>>", $"{theLuc?.can_nang}");
                                DocumentHelper.FindAndReplaceString(ref spDoc, "<<BMI>>", $"{theLuc?.bmi}");

                                var cls = khamSucKhoeChuyenKhoas?.FirstOrDefault(x => x.ma_luot_kham == item.ma_luot_kham);
                                DocumentHelper.FindAndReplaceString(ref spDoc, "<<kq_tuanhoan>>", $"{cls?.kq_nk_tuan_hoan}");
                                DocumentHelper.FindAndReplaceString(ref spDoc, "<<kq_hohap>>", $"{cls?.kq_nk_ho_hap}");
                                DocumentHelper.FindAndReplaceString(ref spDoc, "<<kq_tieuhoa>>", $"{cls?.kq_nk_tieu_hoa}");
                                DocumentHelper.FindAndReplaceString(ref spDoc, "<<kq_noitiet>>", $"{cls?.kq_nk_noi_tiet}");
                                DocumentHelper.FindAndReplaceString(ref spDoc, "<<kq_thantietnieu>>", $"{cls?.kq_nk_than_tiet_nieu}");
                                DocumentHelper.FindAndReplaceString(ref spDoc, "<<kq_coxuongkhop>>", $"{cls?.kq_nk_co_xuong_khop}");
                                DocumentHelper.FindAndReplaceString(ref spDoc, "<<kq_thankinh>>", $"{cls?.kq_nk_than_kinh}");
                                DocumentHelper.FindAndReplaceString(ref spDoc, "<<kq_ngoaikhoa>>", $"{cls?.kq_ngoai_khoa}");
                                DocumentHelper.FindAndReplaceString(ref spDoc, "<<kq_taimuihong>>", $"{cls?.benh_tai_mui_hong}");
                                DocumentHelper.FindAndReplaceString(ref spDoc, "<<kq_dalieu>>", $"{cls?.kq_da_lieu}");
                                DocumentHelper.FindAndReplaceString(ref spDoc, "<<kq_mat>>", $"{cls?.benh_mat}");
                                DocumentHelper.FindAndReplaceString(ref spDoc, "<<kq_ranghammat>>", $"{cls?.benh_rhm}");
                                DocumentHelper.FindAndReplaceString(ref spDoc, "<<kq_tamthan>>", $"{cls?.kq_nk_tam_than}");

                                var sanPhuKhoa = khamSucKhoeSanPhuKhoas?.FirstOrDefault(x => x.ma_luot_kham == item.ma_luot_kham);
                                DocumentHelper.FindAndReplaceString(ref spDoc, "<<kq_sanphukhoa>>", $"{sanPhuKhoa?.ket_qua}");

                                var kqcls = khamSucKhoeKetQuaCanLamSangs?.Where(x => x.luot_kham?.ma_luot_kham == item.ma_luot_kham && !string.IsNullOrEmpty(x.ket_qua)).ToList();
                                if (kqcls != null && kqcls.Any())
                                {
                                    var ds = kqcls.Select(c => new CanLamSangItem(c.type?.GetDescriptionFromString<KetQuaCanLamSang>() ?? string.Empty, c.ket_qua ?? string.Empty));
                                    DocumentHelper.FillChiDinhKetQuaTable(ref spDoc, ds);
                                }
                                else
                                {
                                    DocumentHelper.FindAndReplaceString(ref spDoc, "<<TenChiDinh>>", "");
                                    DocumentHelper.FindAndReplaceString(ref spDoc, "<<kq_canlamsang>>", "");
                                }

                                var ketLuan = khamSucKhoeKetLuans?.FirstOrDefault(x => x.ma_luot_kham == item.ma_luot_kham);
                                DocumentHelper.FindAndReplaceString(ref spDoc, "<<kl_phanloai>>", $"{ketLuan?.phan_loai_suc_khoe?.name}");
                                DocumentHelper.FindAndReplaceString(ref spDoc, "<<kl_ketluan>>", $"{ketLuan?.benh_tat_ket_luan}");
                                DocumentHelper.FindAndReplaceString(ref spDoc, "<<kl_denghi>>", $"{ketLuan?.de_nghi}");
                                break;
                        }

                        string filename = $"{item.benh_nhan?.full_name}_{item.ma_luot_kham}_{exportType.GetDescription()}_{(item.benh_nhan?.gioi_tinh == GioiTinh.Nam ? "Nam" : "Nu")}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf".ToNormalChar();
                        var savePath = Path.Combine(baseFolder, $"{exportType}_{dateNow:yyyyMMddHHmmssfff}");
                        if (!Directory.Exists(savePath))
                        {
                            Directory.CreateDirectory(savePath);
                        }
                        spDoc.SaveToFile(Path.Combine(savePath, filename), Spire.Doc.FileFormat.PDF);
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
                .SendAsync("ExportExaminationError", $"Lỗi khi import: {ex.Message}", cancellationToken);
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
