using CoreAdminWeb.Commons.Utils;
using CoreAdminWeb.Enums;
using CoreAdminWeb.Extensions;
using CoreAdminWeb.Helpers;
using CoreAdminWeb.Model;
using CoreAdminWeb.Model.KhamSucKhoes;
using CoreAdminWeb.Model.RequestHttps;
using CoreAdminWeb.Model.Settings;
using CoreAdminWeb.Services.BaseServices;
using CoreAdminWeb.Services.PDFService;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.Extensions.Caching.Memory;
using System.IO.Compression;
using System.Text;


namespace CoreAdminWeb.Services.Exports
{
    public class ExportKSKDataService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IBaseDetailService<SoKhamSucKhoeModel> _soKhamSucKhoeService;
        private readonly IBaseDetailService<KhamSucKhoeChuyenKhoaModel> _khamSucKhoeChuyenKhoaService;
        private readonly IBaseDetailService<KhamSucKhoeSanPhuKhoaModel> _khamSucKhoeSanPhuKhoaService;
        private readonly IBaseDetailService<KhamSucKhoeKetLuanModel> _khamSucKhoeKetLuanService;
        private readonly IBaseDetailService<KhamSucKhoeTheLucModel> _khamSucKhoeTheLucService;
        private readonly IBaseDetailService<KhamSucKhoeKetQuaCanLamSangModel> _khamSucKhoeKetQuaCanLamSangService;
        private readonly IBaseDetailService<KhamSucKhoeNgheNghiepModel> _khamSucKhoeNgheNghiepService;
        private readonly IBaseDetailService<KhamSucKhoeTienSuModel> _khamSucKhoeTienSuService;
        private readonly IPdfService _pdfService;
        public ExportKSKDataService(IServiceScopeFactory serviceScopeFactory, IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
            using (var scope = serviceScopeFactory.CreateScope())
            {
                _soKhamSucKhoeService = scope.ServiceProvider.GetRequiredService<IBaseDetailService<SoKhamSucKhoeModel>>();
                _khamSucKhoeChuyenKhoaService = scope.ServiceProvider.GetRequiredService<IBaseDetailService<KhamSucKhoeChuyenKhoaModel>>();
                _khamSucKhoeKetLuanService = scope.ServiceProvider.GetRequiredService<IBaseDetailService<KhamSucKhoeKetLuanModel>>();
                _khamSucKhoeSanPhuKhoaService = scope.ServiceProvider.GetRequiredService<IBaseDetailService<KhamSucKhoeSanPhuKhoaModel>>();
                _khamSucKhoeTheLucService = scope.ServiceProvider.GetRequiredService<IBaseDetailService<KhamSucKhoeTheLucModel>>();
                _khamSucKhoeKetQuaCanLamSangService = scope.ServiceProvider.GetRequiredService<IBaseDetailService<KhamSucKhoeKetQuaCanLamSangModel>>();
                _khamSucKhoeNgheNghiepService = scope.ServiceProvider.GetRequiredService<IBaseDetailService<KhamSucKhoeNgheNghiepModel>>();
                _khamSucKhoeTienSuService = scope.ServiceProvider.GetRequiredService<IBaseDetailService<KhamSucKhoeTienSuModel>>();
                _pdfService = scope.ServiceProvider.GetRequiredService<IPdfService>();
            }
        }

        public async Task ExportFromExaminationWithProgressAsync(
            string connectionId,
            List<int> soKSKIds,
            SettingModel setting,
            HoSoKhamSucKhoeExportType exportType,
            string baseUrl,
            Func<ProcessingModel, Task> updateProgress,
            CancellationToken cancellationToken)
        {
            try
            {
                await updateProgress.Invoke(new ProcessingModel()
                {
                    ProcessId = connectionId,
                    Status = TrangThaiXuLyNen.Processing,
                    Value = "Đang chuẩn bị dữ liệu..."
                });

                byte[]? docTemplate1Bytes = null;
                byte[]? docTemplate2Bytes = null;
                string templateContent = string.Empty;

                await updateProgress.Invoke(new ProcessingModel()
                {
                    ProcessId = connectionId,
                    Status = TrangThaiXuLyNen.Processing,
                    Value = "Đang kiểm tra mẫu xuất dữ liệu..."
                });

                switch (exportType)
                {
                    case HoSoKhamSucKhoeExportType.CheckListKsk:
                    case HoSoKhamSucKhoeExportType.ConsultationSlip:
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

                            await updateProgress.Invoke(new ProcessingModel()
                            {
                                ProcessId = connectionId,
                                Status = TrangThaiXuLyNen.Error,
                                Value = $"Mẫu {errorBuilder} không tồn tại."
                            });
                            return;
                        }

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

                            await updateProgress.Invoke(new ProcessingModel()
                            {
                                ProcessId = connectionId,
                                Status = TrangThaiXuLyNen.Error,
                                Value = $"Mẫu {errorBuilder} không tồn tại hoặc đã bị xóa."
                            });
                            return;
                        }
                        break;

                    case HoSoKhamSucKhoeExportType.HealthCheckupBook:
                        string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "templates", "hsksk-tt32.html");
                        if (File.Exists(templatePath))
                        {
                            templateContent = await File.ReadAllTextAsync(templatePath, cancellationToken);
                        }
                        else
                        {
                            await updateProgress.Invoke(new ProcessingModel()
                            {
                                ProcessId = connectionId,
                                Status = TrangThaiXuLyNen.Error,
                                Value = $"Mẫu '{exportType.GetDescription()}' không tồn tại."
                            });
                            return;
                        }
                        break;
                    default:
                        await updateProgress.Invoke(new ProcessingModel()
                        {
                            ProcessId = connectionId,
                            Status = TrangThaiXuLyNen.Error,
                            Value = "Chưa hỗ trợ xuất mẫu này."
                        });
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
                List<KhamSucKhoeNgheNghiepModel>? khamSucKhoeNgheNghieps = null;
                List<KhamSucKhoeTienSuModel>? khamSucKhoeTienSus = null;

                // Lấy dữ liệu chính theo batch lớn
                List<SoKhamSucKhoeModel> soKhamSucKhoes = await BatchQueryAsync(
                    ids => _soKhamSucKhoeService.GetAllAsync($"filter[_and][][id][_in]={string.Join(",", ids)}"),
                    soKSKIds, batchSize
                );

                if (soKhamSucKhoes == null || soKhamSucKhoes.Count == 0)
                {
                    await updateProgress.Invoke(new ProcessingModel()
                    {
                        ProcessId = connectionId,
                        Status = TrangThaiXuLyNen.Error,
                        Value = "Không có dữ liệu khám sức khỏe để xuất."
                    });
                    return;
                }

                // Lấy dữ liệu liên quan song song (nếu cần)
                if (exportType == HoSoKhamSucKhoeExportType.ConsultationSlip || exportType == HoSoKhamSucKhoeExportType.HealthCheckupBook)
                {
                    var chuyenKhoaTask = BatchQueryAsync(
                        ids => _khamSucKhoeChuyenKhoaService.GetAllAsync($"filter[_and][][luot_kham][_in]={string.Join(",", ids)}"),
                        soKSKIds, batchSize
                    );
                    var sanPhuKhoaTask = BatchQueryAsync(
                        ids => _khamSucKhoeSanPhuKhoaService.GetAllAsync($"filter[_and][][luot_kham][_in]={string.Join(",", ids)}"),
                        soKSKIds, batchSize
                    );
                    var ketLuanTask = BatchQueryAsync(
                        ids => _khamSucKhoeKetLuanService.GetAllAsync($"filter[_and][][luot_kham][_in]={string.Join(",", ids)}"),
                        soKSKIds, batchSize
                    );
                    var theLucTask = BatchQueryAsync(
                        ids => _khamSucKhoeTheLucService.GetAllAsync($"filter[_and][][luot_kham][_in]={string.Join(",", ids)}"),
                        soKSKIds, batchSize
                    );
                    var kqCLSTask = BatchQueryAsync(
                        ids => _khamSucKhoeKetQuaCanLamSangService.GetAllAsync($"filter[_and][][luot_kham][_in]={string.Join(",", ids)}"),
                        soKSKIds, batchSize
                    );
                    var ngheNghiepTask = BatchQueryAsync(
                        ids => _khamSucKhoeNgheNghiepService.GetAllAsync($"filter[_and][][luot_kham][_in]={string.Join(",", ids)}"),
                        soKSKIds, batchSize
                    );
                    var tienSuTask = BatchQueryAsync(
                        ids => _khamSucKhoeTienSuService.GetAllAsync($"filter[_and][][luot_kham][_in]={string.Join(",", ids)}"),
                        soKSKIds, batchSize
                    );

                    await Task.WhenAll(chuyenKhoaTask, sanPhuKhoaTask, ketLuanTask, theLucTask, kqCLSTask, ngheNghiepTask, tienSuTask);

                    khamSucKhoeChuyenKhoas = chuyenKhoaTask.Result;
                    khamSucKhoeSanPhuKhoas = sanPhuKhoaTask.Result;
                    khamSucKhoeKetLuans = ketLuanTask.Result;
                    khamSucKhoeTheLucs = theLucTask.Result;
                    khamSucKhoeKetQuaCanLamSangs = kqCLSTask.Result;
                    khamSucKhoeNgheNghieps = ngheNghiepTask.Result;
                    khamSucKhoeTienSus = tienSuTask.Result;
                }

                // Chuẩn bị thư mục lưu file
                var dateNow = DateTime.Now;
                var baseFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "exports", "kham_suc_khoe");
                var savePath = Path.Combine(baseFolder, $"{exportType}_{dateNow:yyyyMMddHHmmssfff}");
                string fullFilePath;
                string filename;
                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }

                int totalRecords = soKhamSucKhoes.Count;
                int processed = 0;
                List<string> fileNames = new List<string>(totalRecords);

                switch (exportType)
                {
                    case HoSoKhamSucKhoeExportType.CheckListKsk:
                    case HoSoKhamSucKhoeExportType.ConsultationSlip:
                        // Duyệt theo batch để giảm memory pressure
                        foreach (var batch in soKhamSucKhoes.Chunk(batchSize))
                        {
                            var batchList = batch.ToList();
                            var tasks = batchList.Select(async item =>
                            {
                                processed++;
                                if (cancellationToken.IsCancellationRequested)
                                {
                                    await updateProgress.Invoke(new ProcessingModel()
                                    {
                                        ProcessId = connectionId,
                                        Status = TrangThaiXuLyNen.Error,
                                        Value = "Xuất dữ liệu đã bị hủy."
                                    });
                                    return;
                                }

                                var usingDocTemplate = item.benh_nhan?.gioi_tinh == GioiTinh.Nam ? docTemplate1Bytes : docTemplate2Bytes;
                                if (usingDocTemplate == null)
                                {
                                    await updateProgress.Invoke(new ProcessingModel()
                                    {
                                        ProcessId = connectionId,
                                        Status = TrangThaiXuLyNen.Error,
                                        Value = $"Không tìm thấy mẫu xuất cho giới tính {item.benh_nhan?.gioi_tinh?.GetDescription()}."
                                    });
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
                                                await updateProgress.Invoke(new ProcessingModel()
                                                {
                                                    ProcessId = connectionId,
                                                    Status = TrangThaiXuLyNen.Error,
                                                    Value = $"Lỗi xử lý {HoSoKhamSucKhoeExportType.CheckListKsk.GetDescription()}: {ex.Message}"
                                                });
                                                return;
                                            }
                                            break;
                                        case HoSoKhamSucKhoeExportType.ConsultationSlip:
                                            try
                                            {
                                                var theLuc = khamSucKhoeTheLucs?.FirstOrDefault(x => x.luot_kham?.id == item.id);
                                                var cls = khamSucKhoeChuyenKhoas?.FirstOrDefault(x => x.luot_kham?.id == item.id);
                                                var sanPhuKhoa = khamSucKhoeSanPhuKhoas?.FirstOrDefault(x => x.luot_kham?.id == item.id);
                                                var kqcls = khamSucKhoeKetQuaCanLamSangs?.Where(x => x.luot_kham?.id == item.id && !string.IsNullOrEmpty(x.ket_qua)).ToList();
                                                var ketLuan = khamSucKhoeKetLuans?.FirstOrDefault(x => x.luot_kham?.id == item.id);

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
                                                    { "<<kl_phanloai>>", $"{ketLuan?.phan_loai_suc_khoe?.name}" },
                                                    { "<<kl_ketluan>>", $"{ketLuan?.benh_tat_ket_luan}" },
                                                    { "<<kl_denghi>>", $"{ketLuan?.de_nghi}" }
                                                });

                                                doc.ReplaceText(new Dictionary<string, string>
                                                {
                                                    { "<<kq_sanphukhoa>>",$"{sanPhuKhoa?.ket_qua}" }
                                                });

                                                if (kqcls != null && kqcls.Any())
                                                {
                                                    var items_kqcls = kqcls.Select(c => new CanLamSangItem(c.type?.GetDescriptionFromString<KetQuaCanLamSang>() ?? string.Empty, c.ket_qua ?? string.Empty)).ToList();
                                                    StringBuilder chiDinhFormatted = new StringBuilder();
                                                    for (int i = 0; i < items_kqcls.Count; i++)
                                                    {
                                                        var text = $"{i + 1} - {items_kqcls[i].TenChiDinh}:\n";
                                                        chiDinhFormatted.AppendLine(text);
                                                    }
                                                    StringBuilder ketQuaFormatted = new StringBuilder();
                                                    for (int i = 0; i < items_kqcls.Count; i++)
                                                    {
                                                        var text = $"Kết quả {items_kqcls[i].TenChiDinh} :{items_kqcls[i].KetQua}\n";
                                                        ketQuaFormatted.AppendLine(text);
                                                    }

                                                    doc.ReplaceText(new Dictionary<string, string>
                                                    {
                                                        { "<<TenChiDinh>>",$"{chiDinhFormatted}" },
                                                        { "<<kq_canlamsang>>",$"{ketQuaFormatted}" }
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
                                                await updateProgress.Invoke(new ProcessingModel()
                                                {
                                                    ProcessId = connectionId,
                                                    Status = TrangThaiXuLyNen.Error,
                                                    Value = $"Lỗi xử lý {HoSoKhamSucKhoeExportType.ConsultationSlip.GetDescription()}: {ex.Message}"
                                                });
                                                return;
                                            }

                                            break;
                                    }
                                }

                                filename = $"{item.benh_nhan?.full_name}_{item.ma_luot_kham}_{exportType.GetDescription()}_{(item.benh_nhan?.gioi_tinh == GioiTinh.Nam ? "Nam" : "Nu")}_{DateTime.Now:yyyyMMdd_HHmmss}.docx".ToUnsign(spaceReplacement: "-");
                                fullFilePath = Path.Combine(savePath, filename);
                                File.Copy(tempTemplatePath, fullFilePath, true);
                                if (File.Exists(tempTemplatePath))
                                {
                                    File.Delete(tempTemplatePath);
                                }
                                fileNames.Add(filename);

                                // Chỉ gửi progress mỗi 100 bản ghi
                                if (processed % 100 == 0 || processed == totalRecords)
                                {
                                    await updateProgress.Invoke(new ProcessingModel()
                                    {
                                        ProcessId = connectionId,
                                        Status = TrangThaiXuLyNen.Processing,
                                        Value = $"Đang xuất dữ liệu {processed}/{totalRecords}"
                                    });
                                }
                            });

                            // Chạy song song trong batch nhỏ (giới hạn số lượng để tránh quá tải IO)
                            await Task.WhenAll(tasks);
                        }

                        break;

                    case HoSoKhamSucKhoeExportType.HealthCheckupBook:
                        foreach (var item in soKhamSucKhoes)
                        {
                            // Chỉ gửi progress mỗi 100 bản ghi
                            if (processed % 100 == 0 || processed == totalRecords)
                            {
                                await updateProgress.Invoke(new ProcessingModel()
                                {
                                    ProcessId = connectionId,
                                    Status = TrangThaiXuLyNen.Processing,
                                    Value = $"Đang xuất dữ liệu {processed}/{totalRecords}"
                                });
                            }

                            var theLuc = khamSucKhoeTheLucs?.FirstOrDefault(x => x.luot_kham?.id == item.id);
                            var cls = khamSucKhoeChuyenKhoas?.FirstOrDefault(x => x.luot_kham?.id == item.id);
                            var sanPhuKhoa = khamSucKhoeSanPhuKhoas?.FirstOrDefault(x => x.luot_kham?.id == item.id);
                            var kqcls = khamSucKhoeKetQuaCanLamSangs?.Where(x => x.luot_kham?.id == item.id && !string.IsNullOrEmpty(x.ket_qua)).ToList();
                            var ketLuan = khamSucKhoeKetLuans?.FirstOrDefault(x => x.luot_kham?.id == item.id);
                            var ngheNghiep = khamSucKhoeNgheNghieps?.FirstOrDefault(x => x.luot_kham?.id == item.id);
                            var tienSu = khamSucKhoeTienSus?.FirstOrDefault(x => x.luot_kham?.id == item.id);

                            var tempTempalte = templateContent
                            .Replace("{{TenBenhNhan}}", $"{item.benh_nhan?.full_name.ToUpper()}")
                            .Replace("{{GioiTinh_Nam}}", CheckBoxHtmlBuilder(item.benh_nhan?.gioi_tinh == GioiTinh.Nam))
                            .Replace("{{GioiTinh_Nu}}", CheckBoxHtmlBuilder(item.benh_nhan?.gioi_tinh == GioiTinh.Nu))
                            .Replace("{{NgaySinh_Day}}", $"{item.benh_nhan?.ngay_sinh:dd}")
                            .Replace("{{NgaySinh_Month}}", $"{item.benh_nhan?.ngay_sinh:MM}")
                            .Replace("{{NgaySinh_Year}}", $"{item.benh_nhan?.ngay_sinh:yyyy}")
                            .Replace("{{Tuoi}}", $"{(item.benh_nhan?.ngay_sinh != null ? (DateTime.Now.Year - item.benh_nhan.ngay_sinh.Value.Year).ToString() : "")}")
                            .Replace("{{SoDinhDanh}}", $"{item.benh_nhan?.so_dinh_danh}")
                            .Replace("{{NgayCap}}", $"{item.benh_nhan?.ngay_cap}")
                            .Replace("{{NoiCap}}", $"{item.benh_nhan?.noi_cap}")
                            .Replace("{{DiaChi}}", $"{item.benh_nhan?.dia_chi}")
                            .Replace("{{SoDienThoai}}", $"{item.benh_nhan?.so_dien_thoai}")
                            .Replace("{{NgheNghiep}}", $"{ngheNghiep?.nghe_nghiep}")
                            .Replace("{{NoiCongTac}}", $"{ngheNghiep?.noi_cong_tac}")
                            .Replace("{{TienSuBenhTatGiaDinh}}", MultilineSpanHtmlBuilder(tienSu?.tien_su_gia_dinh ?? string.Empty, 4))
                            .Replace("{{TenBenh}}", $"{tienSu?.ten_benh}")
                            .Replace("{{NamPhatHien}}", $"{tienSu?.nam_phat_hien}")
                            .Replace("{{BenhNgheNghiep}}", $"{tienSu?.benh_nghe_nghiep}")
                            .Replace("{{NamPhatHienBenhNgheNghiep}}", $"{tienSu?.nam_phat_hien_benh_nghe_nghiep}")
                            .Replace("{{ThoiGianLapSo_Ngay}}", $"{item.ngay_lap_so ?? DateTime.Now:dd}")
                            .Replace("{{ThoiGianLapSo_Thang}}", $"{item.ngay_lap_so ?? DateTime.Now:MM}")
                            .Replace("{{ThoiGianLapSo_Nam}}", $"{item.ngay_lap_so ?? DateTime.Now:yyyy}")
                            .Replace("{{NguoiLapSo}}", $"{item.nguoi_lap}")
                            .Replace("{{NamBatDauKinhNguyet}}", TableTdHtmlBuilder(item.benh_nhan?.gioi_tinh == GioiTinh.Nu ? sanPhuKhoa?.tuoi_bat_dau_kinh?.ToString() : ""))
                            .Replace("{{KinhNguyet_Deu}}", CheckBoxHtmlBuilder(item.benh_nhan?.gioi_tinh == GioiTinh.Nu && sanPhuKhoa?.tinh_chat_kinh == TinhChatKinh.Deu.ToString()))
                            .Replace("{{KinhNguyet_KhongDeu}}", CheckBoxHtmlBuilder(item.benh_nhan?.gioi_tinh == GioiTinh.Nu && sanPhuKhoa?.tinh_chat_kinh == TinhChatKinh.KhongDeu.ToString()))
                            .Replace("{{ChuKyKinh}}", TableTdHtmlBuilder(item.benh_nhan?.gioi_tinh == GioiTinh.Nu ? sanPhuKhoa?.chu_ky_kinh : ""))
                            .Replace("{{LuongKinh}}", TableTdHtmlBuilder(item.benh_nhan?.gioi_tinh == GioiTinh.Nu ? sanPhuKhoa?.luong_kinh : ""))
                            .Replace("{{DauBungKinh_Co}}", CheckBoxHtmlBuilder(item.benh_nhan?.gioi_tinh == GioiTinh.Nu && sanPhuKhoa?.dau_bung_kinh == true))
                            .Replace("{{DauBungKinh_Khong}}", CheckBoxHtmlBuilder(item.benh_nhan?.gioi_tinh == GioiTinh.Nu && sanPhuKhoa?.dau_bung_kinh == false))
                            .Replace("{{DaLapGiaDinh_Co}}", CheckBoxHtmlBuilder(item.benh_nhan?.gioi_tinh == GioiTinh.Nu && sanPhuKhoa?.da_lap_gia_dinh == true))
                            .Replace("{{DaLapGiaDinh_Chua}}", CheckBoxHtmlBuilder(item.benh_nhan?.gioi_tinh == GioiTinh.Nu && sanPhuKhoa?.da_lap_gia_dinh == false))
                            .Replace("{{PARA}}", TableTdHtmlBuilder(item.benh_nhan?.gioi_tinh == GioiTinh.Nu ? sanPhuKhoa?.para : "", 4))
                            .Replace("{{MoSanPhuKhoa_Co}}", CheckBoxHtmlBuilder(item.benh_nhan?.gioi_tinh == GioiTinh.Nu && sanPhuKhoa?.so_lan_mo_san_phu_khoa == (int)YesNo.Co))
                            .Replace("{{MoSanPhuKhoa_GhiRo}}", $"{(item.benh_nhan?.gioi_tinh == GioiTinh.Nu ? sanPhuKhoa?.mo_san_phu_khoa_ghi_ro : "")}")
                            .Replace("{{MoSanPhuKhoa_Khong}}", CheckBoxHtmlBuilder(item.benh_nhan?.gioi_tinh == GioiTinh.Nu && sanPhuKhoa?.so_lan_mo_san_phu_khoa == (int)YesNo.Khong))
                            .Replace("{{ApDungBienPhapPhongTranh_Co}}", CheckBoxHtmlBuilder(item.benh_nhan?.gioi_tinh == GioiTinh.Nu && sanPhuKhoa?.ap_dung_bptt == true))
                            .Replace("{{ApDungBienPhapPhongTranh_GhiRo}}", $"{(item.benh_nhan?.gioi_tinh == GioiTinh.Nu ? sanPhuKhoa?.bptt_ghi_ro : "")}")
                            .Replace("{{ApDungBienPhapPhongTranh_Khong}}", CheckBoxHtmlBuilder(item.benh_nhan?.gioi_tinh == GioiTinh.Nu && sanPhuKhoa?.ap_dung_bptt == false))
                            .Replace("{{ChieuCao}}", $"{theLuc?.chieu_cao}")
                            .Replace("{{CanNang}}", $"{theLuc?.can_nang}")
                            .Replace("{{BMI}}", $"{theLuc?.bmi}")
                            .Replace("{{Mach}}", $"{theLuc?.mach}")
                            .Replace("{{HuyetAp}}", !string.IsNullOrEmpty(theLuc?.huyet_ap) ? $"<span class=\"dotted-ruled\" style=\"width: 30mm\">{theLuc?.huyet_ap}</span>" : "<span class=\"dotted-ruled\" style=\"width: 15mm\"></span><span>/</span><span class=\"dotted-ruled\" style=\"width: 15mm\"></span>")
                            .Replace("{{PhanLoaiTheLuc}}", $"{theLuc?.phan_loai?.name}")
                            .Replace("{{KetQuaCLS_TuanHoan}}", $"{cls?.kq_nk_tuan_hoan}")
                            .Replace("{{KetQuaCLS_TuanHoan_ChuKy}}", RenderSignature(cls?.chu_ky_tuan_hoan ?? string.Empty, "", 100, 50) + $"<br/>{cls?.bs_tuan_hoan}")
                            .Replace("{{KetQuaCLS_TuanHoan_PhanLoai}}", $"{cls?.pl_nk_tuan_hoan?.name}")
                            .Replace("{{KetQuaCLS_HoHap}}", $"{cls?.kq_nk_ho_hap}")
                            .Replace("{{KetQuaCLS_HoHap_ChuKy}}", RenderSignature(cls?.chu_ky_ho_hap ?? string.Empty, "", 100, 50) + $"<br/>{cls?.bs_ho_hap}")
                            .Replace("{{KetQuaCLS_HoHap_PhanLoai}}", $"{cls?.pl_nk_ho_hap?.name}")
                            .Replace("{{KetQuaCLS_TieuHoa}}", $"{cls?.kq_nk_tieu_hoa}")
                            .Replace("{{KetQuaCLS_TieuHoa_ChuKy}}", RenderSignature(cls?.chu_ky_tieu_hoa ?? string.Empty, "", 100, 50) + $"<br/>{cls?.bs_tieu_hoa}")
                            .Replace("{{KetQuaCLS_TieuHoa_PhanLoai}}", $"{cls?.pl_nk_tieu_hoa?.name}")
                            .Replace("{{KetQuaCLS_ThanTietNieu}}", $"{cls?.kq_nk_than_tiet_nieu}")
                            .Replace("{{KetQuaCLS_ThanTietNieu_ChuKy}}", RenderSignature(cls?.chu_ky_than_tiet_nieu ?? string.Empty, "", 100, 50) + $"<br/>{cls?.bs_than_tiet_nieu}")
                            .Replace("{{KetQuaCLS_ThanTietNieu_PhanLoai}}", $"{cls?.pl_nk_than_tiet_nieu?.name}")
                            .Replace("{{KetQuaCLS_NoiTiet}}", $"{cls?.kq_nk_noi_tiet}")
                            .Replace("{{KetQuaCLS_NoiTiet_ChuKy}}", RenderSignature(cls?.chu_ky_noi_tiet ?? string.Empty, "", 100, 50) + $"<br/>{cls?.bs_noi_tiet}")
                            .Replace("{{KetQuaCLS_NoiTiet_PhanLoai}}", $"{cls?.pl_nk_noi_tiet?.name}")
                            .Replace("{{KetQuaCLS_CoXuongKhops}}", $"{cls?.kq_nk_co_xuong_khop}")
                            .Replace("{{KetQuaCLS_CoXuongKhops_ChuKy}}", RenderSignature(cls?.chu_ky_co_xuong_khop ?? string.Empty, "", 100, 50) + $"<br/>{cls?.bs_co_xuong_khop}")
                            .Replace("{{KetQuaCLS_CoXuongKhops_PhanLoai}}", $"{cls?.pl_nk_co_xuong_khop?.name}")
                            .Replace("{{KetQuaCLS_ThanKinh}}", $"{cls?.kq_nk_than_kinh}")
                            .Replace("{{KetQuaCLS_ThanKinh_ChuKy}}", RenderSignature(cls?.chu_ky_than_kinh ?? string.Empty, "", 100, 50) + $"<br/>{cls?.bs_than_kinh}")
                            .Replace("{{KetQuaCLS_ThanKinh_PhanLoai}}", $"{cls?.pl_nk_than_kinh?.name}")
                            .Replace("{{KetQuaCLS_TamThan}}", $"{cls?.kq_nk_tam_than}")
                            .Replace("{{KetQuaCLS_TamThan_ChuKy}}", RenderSignature(cls?.chu_ky_tam_than ?? string.Empty, "", 100, 50) + $"<br/>{cls?.bs_tam_than}")
                            .Replace("{{KetQuaCLS_TamThan_PhanLoai}}", $"{cls?.pl_nk_tam_than?.name}")
                            .Replace("{{KetQuaCLS_NgoaiKhoa}}", $"{cls?.kq_ngoai_khoa}")
                            .Replace("{{KetQuaCLS_NgoaiKhoa_PhanLoai}}", $"{cls?.pl_ngoai_khoa?.name}")
                            .Replace("{{KetQuaCLS_DaLieu}}", $"{cls?.kq_da_lieu}")
                            .Replace("{{KetQuaCLS_DaLieu_PhanLoai}}", $"{cls?.pl_da_lieu?.name}")
                            .Replace("{{KetQuaCLS_NgoaiKhoa_ChuKy}}", RenderSignature(cls?.chu_ky_ngoai_khoa ?? string.Empty, "", 100, 50) + $"<br/>{cls?.bs_ngoai_khoa}")
                            .Replace("{{KetQuaCLS_SanPhuKhoa}}", $"{(!string.IsNullOrEmpty(sanPhuKhoa?.ket_qua) ? sanPhuKhoa?.ket_qua : "Chi tiết nội dung khám theo danh mục tại phụ lục XXV ban hành kèm theo Thông tư này.")}")
                            .Replace("{{KetQuaCLS_SanPhuKhoa_PhanLoai}}", $"{sanPhuKhoa?.phan_loai?.name}")
                            .Replace("{{KetQuaCLS_SanPhuKhoa_ChuKy}}", RenderSignature(sanPhuKhoa?.chu_ky ?? string.Empty, "", 100, 50) + $"<br/>{sanPhuKhoa?.nguoi_ket_luan}")
                            .Replace("{{KetQuaCLS_Mat_KhongKinh_Phai}}", $"{cls?.thi_luc_khong_kinh_phai}")
                            .Replace("{{KetQuaCLS_Mat_KhongKinh_Trai}}", $"{cls?.thi_luc_khong_kinh_trai}")
                            .Replace("{{KetQuaCLS_Mat_CoKinh_Phai}}", $"{cls?.thi_luc_co_kinh_phai}")
                            .Replace("{{KetQuaCLS_Mat_CoKinh_Trai}}", $"{cls?.thi_luc_co_kinh_trai}")
                            .Replace("{{KetQuaCLS_Mat_ChuKy}}", RenderSignature(cls?.chu_ky_mat ?? string.Empty, "", 100, 50) + $"<br/>{cls?.bs_mat}")
                            .Replace("{{KetQuaCLS_Mat_Benh}}", $"{cls?.benh_mat}")
                            .Replace("{{KetQuaCLS_Mat_PhanLoai}}", $"{cls?.pl_mat?.name}")
                            .Replace("{{KetQuaCLS_TaiMuiHong_TaiTrai_NoiThuong}}", $"{cls?.tmh_nt_trai}")
                            .Replace("{{KetQuaCLS_TaiMuiHong_TaiTrai_NoiTham}}", $"{cls?.tmh_ntham_trai}")
                            .Replace("{{KetQuaCLS_TaiMuiHong_TaiPhai_NoiThuong}}", $"{cls?.tmh_nt_phai}")
                            .Replace("{{KetQuaCLS_TaiMuiHong_TaiPhai_NoiTham}}", $"{cls?.tmh_ntham_phai}")
                            .Replace("{{KetQuaCLS_TaiMuiHong_ChuKy}}", RenderSignature(cls?.chu_ky_tmh ?? string.Empty, "", 100, 50) + $"<br/>{cls?.bs_tmh}")
                            .Replace("{{KetQuaCLS_TaiMuiHong_Benh}}", $"{cls?.benh_tai_mui_hong}")
                            .Replace("{{KetQuaCLS_TaiMuiHong_PhanLoai}}", $"{cls?.pl_tmh?.name}")
                            .Replace("{{KetQuaCLS_RangHamMat_HamTren}}", $"{cls?.kq_rhm_ham_tren}")
                            .Replace("{{KetQuaCLS_RangHamMat_HamDuoi}}", $"{cls?.kq_rhm_ham_duoi}")
                            .Replace("{{KetQuaCLS_RangHamMat_ChuKy}}", RenderSignature(cls?.chu_ky_rhm ?? string.Empty, "", 100, 50) + $"<br/>{cls?.bs_rhm}")
                            .Replace("{{KetQuaCLS_RangHamMat_Benh}}", $"{cls?.benh_rhm}")
                            .Replace("{{KetQuaCLS_RangHamMat_PhanLoai}}", $"{cls?.pl_rhm?.name}")
                            .Replace("{{KetQuaCLS}}", RenderKQCLS(kqcls?.Where(c => !string.IsNullOrEmpty(c.ket_qua)).ToList(), RenderSignature(ketLuan?.bs_ket_luan?.chu_ky_bac_si ?? string.Empty, "", 100, 50) + $"<br/>{ketLuan?.bs_ket_luan?.chuc_danh} {ketLuan?.bs_ket_luan?.full_name}"))
                            .Replace("{{KetQuaCLS_CLS_PhanLoaiSucKhoe}}", $"{ketLuan?.phan_loai_suc_khoe?.name}")
                            .Replace("{{KetLuan}}", MultilineSpanHtmlBuilder(ketLuan?.benh_tat_ket_luan ?? string.Empty))
                            .Replace("{{NgayKetLuan_Ngay}}", $"{ketLuan?.ngay_ket_luan:dd}")
                            .Replace("{{NgayKetLuan_Thang}}", $"{ketLuan?.ngay_ket_luan:MM}")
                            .Replace("{{NgayKetLuan_Nam}}", $"{ketLuan?.ngay_ket_luan:yyyy}")
                            .Replace("{{NguoiKetLuan}}", RenderSignature(ketLuan?.bs_ket_luan?.chu_ky_bac_si ?? string.Empty, "<div style=\"height: 20mm\"></div>", 100, 50) + $"<br/><span class=\"bold\">{ketLuan?.bs_ket_luan?.chuc_danh} {ketLuan?.bs_ket_luan?.full_name}</span>");

                            filename = $"{item.benh_nhan?.full_name}_{item.ma_luot_kham}_{exportType.GetDescription()}_{(item.benh_nhan?.gioi_tinh == GioiTinh.Nam ? "Nam" : "Nu")}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf".ToUnsign(spaceReplacement: "-");
                            var pdfBytes = _pdfService.GeneratePdfFromHtml(tempTempalte, new PdfSettings
                            {
                                FileName = filename,
                                PageSize = "A4",
                                Orientation = "Portrait",
                                MarginTop = 10,
                                MarginBottom = 10,
                                MarginLeft = 10,
                                MarginRight = 10
                            });

                            fullFilePath = Path.Combine(savePath, filename);
                            await File.WriteAllBytesAsync(fullFilePath, pdfBytes, cancellationToken);
                            fileNames.Add(filename);
                        }
                        break;

                    default:
                        await updateProgress.Invoke(new ProcessingModel()
                        {
                            ProcessId = connectionId,
                            Status = TrangThaiXuLyNen.Error,
                            Value = "Chưa hỗ trợ xuất mẫu này."
                        });
                        return;
                }

                if (fileNames.Count == 0)
                {
                    await updateProgress.Invoke(new ProcessingModel()
                    {
                        ProcessId = connectionId,
                        Status = TrangThaiXuLyNen.Error,
                        Value = "Không có dữ liệu để xuất."
                    });
                    return;
                }

                await updateProgress.Invoke(new ProcessingModel()
                {
                    ProcessId = connectionId,
                    Status = TrangThaiXuLyNen.Processing,
                    Value = $"Đang chuẩn bị tập tin nén..."
                });

                await Task.Delay(200, cancellationToken);

                string zipFileName = $"{exportType.GetDescription()}_{DateTime.Now:yyyyMMdd_HHmmss}".ToUnsign(spaceReplacement: "-");
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

                await updateProgress.Invoke(new ProcessingModel()
                {
                    ProcessId = connectionId,
                    Status = TrangThaiXuLyNen.Completed,
                    Value = "Tập tin nén đã được chuẩn bị xong. Đang chuẩn bị tải về.",
                    AdditionalParams = new
                    {
                        RelativeUrl = $"/downloads/{ticketId}",
                        TicketId = ticketId,
                        FileName = Path.GetFileName(zipPath),
                        Size = new FileInfo(zipPath).Length
                    }
                });
            }
            catch (Exception ex)
            {

                await updateProgress.Invoke(new ProcessingModel()
                {
                    ProcessId = connectionId,
                    Status = TrangThaiXuLyNen.Error,
                    Value = $"Lỗi khi xuất tập tin: {ex.Message}"
                });
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

        private static string CheckBoxHtmlBuilder(bool value)
        {
            if (value)
            {
                return "<span class=\"cb\" checked aria-hidden=\"true\"></span>";
            }
            else
            {
                return "<span class=\"cb\" aria-hidden=\"true\"></span>";
            }
        }

        private static string MultilineSpanHtmlBuilder(string value, int count = 1)
        {
            StringBuilder stringBuilder = new StringBuilder();
            int splitCount = 0;
            foreach (var splitStr in value.WrapToWidthMm())
            {
                splitCount++;
                stringBuilder.Append($"<span class=\"dotted-ruled\" style=\"width: 100%\">{splitStr}</span>");
            }

            if (splitCount < count)
            {
                for (int i = 0; i < 4 - splitCount; i++)
                {
                    stringBuilder.Append($"<span class=\"dotted-ruled\" style=\"width: 100%\"></span>");
                }
            }

            return stringBuilder.ToString();
        }

        private static string TableTdHtmlBuilder(string? value, int count = 2)
        {
            StringBuilder stringBuilder = new StringBuilder();

            if (!string.IsNullOrEmpty(value) && value.Contains("|"))
            {
                var splitStr = value.Split("|");
                foreach (var c in splitStr)
                {
                    stringBuilder.Append($"<td width=\"30\" height=\"30\" class=\"center v-middle\">{c}</td>");
                }

                return stringBuilder.ToString();
            }

            var charArray = value?.PadLeft(2, '0').ToCharArray();
            if (charArray == null || charArray.Length < count)
            {
                charArray = new char[count];
            }

            foreach (var c in charArray)
            {
                stringBuilder.Append($"<td width=\"30\" height=\"30\" class=\"center v-middle\">{c}</td>");
            }

            return stringBuilder.ToString();
        }

        private static string RenderSignature(string signatureData, string? fallbackText = "", int maxWidth = 120, int maxHeight = 60)
        {
            return signatureData.GetOptimizedSignatureDisplayHtml(fallbackText, maxWidth, maxHeight);
        }

        private static string RenderKQCLS(List<KhamSucKhoeKetQuaCanLamSangModel>? cls, string signature)
        {
            StringBuilder sb = new StringBuilder();
            if (cls == null || !cls.Any())
            {
                sb.Append("<tr><td>");
                sb.Append($"<span>** Xét nghiệm huyết học/sinh hóa/X.quang và các xét nghiệm khác khi có chỉ định của bác sỹ:</span>");
                sb.Append($"<div class=\"row\"><span>a) Kết quả:</span><span class=\"dotted-ruled\" style=\"width: 100%\"></span>");
                sb.Append($"</div><div class=\"row\"><span>b) Đánh giá:</span><span class=\"dotted-ruled\" style=\"width: 100%\"></span></div>");
                sb.Append("</td><td class=\"center v-middle\">" + signature);
                sb.Append("</td></tr>");
                return sb.ToString();
            }

            foreach (var item in cls.Select((v, i) => new { data = v, index = i }))
            {
                sb.Append("<tr><td>");
                sb.Append($"<span>* {item.data.type?.GetDescriptionFromString<KetQuaCanLamSang>()}:</span>");
                sb.Append($"<div class=\"row\"><span>a) Kết quả:</span>");
                sb.Append(MultilineSpanHtmlBuilder(item.data.ket_qua ?? string.Empty));
                sb.Append($"</div><div class=\"row\"><span>b) Đánh giá:</span><span class=\"dotted-ruled\" style=\"width: 100%\"></span></div>");

                if (item.index == 0)
                {
                    sb.Append($"</td><td rowspan=\"{cls.Count}\" class=\"center v-middle\">" + signature);
                }

                sb.Append("</td></tr>");
            }
            return sb.ToString();
        }
    }
}
