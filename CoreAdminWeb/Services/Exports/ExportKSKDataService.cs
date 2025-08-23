using CoreAdminWeb.Commons.Utils;
using CoreAdminWeb.Enums;
using CoreAdminWeb.Extensions;
using CoreAdminWeb.Hubs;
using CoreAdminWeb.Model.KhamSucKhoes;
using CoreAdminWeb.Model.RequestHttps;
using CoreAdminWeb.Model.Settings;
using CoreAdminWeb.Services.BaseServices;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using System.IO.Compression;
using System.Text;

using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;
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
                var qrFolderPath = Path.Combine(baseFolder, $"{exportType}_{dateNow:yyyyMMddHHmmssfff}_QR");
                if (!Directory.Exists(qrFolderPath))
                {
                    Directory.CreateDirectory(qrFolderPath);
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

                    // Create a temporary file for editing
                    var tempTemplatePath = Path.Combine(Path.GetTempPath(), $"{item.ma_luot_kham}_temp.docx");
                    File.WriteAllBytes(tempTemplatePath, usingDocTemplate);
                    
                    // Process document in a separate scope to ensure proper disposal
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
                                        doc.ReplaceImage("<<QR>>", qrCode, 1200000, 1200000);
                                       
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
                                    continue;
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
                                    continue;
                                }

                                break;
                        }
                    } // End of document processing scope - Document is now closed and file is unlocked
                    
                    // Now process the file outside of the using block
                    string filename = $"{item.benh_nhan?.full_name}_{item.ma_luot_kham}_{exportType.GetDescription()}_{(item.benh_nhan?.gioi_tinh == GioiTinh.Nam ? "Nam" : "Nu")}_{DateTime.Now:yyyyMMdd_HHmmss}.docx".ToNormalChar();
                    var savePath = Path.Combine(baseFolder, $"{exportType}_{dateNow:yyyyMMddHHmmssfff}");
                    if (!Directory.Exists(savePath))
                    {
                        Directory.CreateDirectory(savePath);
                    }
                    
                    try
                    {
                        // Wait a moment to ensure file system releases the lock completely
                        await Task.Delay(200, cancellationToken);
                        
                        // Save document to specific file path
                        var fullFilePath = Path.Combine(savePath, filename);
                        
                        // Now copy the modified temporary file to the final location
                        // The file should be unlocked after the using block ends
                        File.Copy(tempTemplatePath, fullFilePath, true);
                        
                        // Clean up temporary file
                        if (File.Exists(tempTemplatePath))
                        {
                            File.Delete(tempTemplatePath);
                        }
                        
                        // Verify file was created and has content
                        if (File.Exists(fullFilePath))
                        {
                            var fileInfo = new FileInfo(fullFilePath);
                            Console.WriteLine($"[Success] Document saved: {fullFilePath}, Size: {fileInfo.Length} bytes");
                            
                            if (fileInfo.Length == 0)
                            {
                                Console.WriteLine($"[Warning] File {fullFilePath} is empty!");
                            }
                            else if (fileInfo.Length < 1000) // Word files should be at least a few KB
                            {
                                Console.WriteLine($"[Warning] File {fullFilePath} seems too small ({fileInfo.Length} bytes) - might be corrupted");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"[Error] File {fullFilePath} was not created!");
                        }
                        
                        fileNames.Add(filename);
                    }
                    catch (Exception ex)
                    {
                        var errorDetails = $"[Document Save Error] Item: {item.ma_luot_kham}, Filename: {filename}, " +
                                         $"Line: {ex.StackTrace?.Split('\n').FirstOrDefault(x => x.Contains("ExportKSKDataService.cs"))?.Trim() ?? "Unknown"}, " +
                                         $"Method: {ex.TargetSite?.Name ?? "Unknown"}, " +
                                         $"Library: {ex.TargetSite?.DeclaringType?.Assembly.GetName().Name ?? "Unknown"}, " +
                                         $"Error: {ex.Message}, " +
                                         $"Inner Exception: {ex.InnerException?.Message ?? "None"}";
                        
                        Console.WriteLine(errorDetails);
                        await _hubContext.Clients.Client(connectionId)
                            .SendAsync("ExportExaminationError", $"Lỗi lưu file: {ex.Message}", cancellationToken);
                        continue;
                    }
                } // End of foreach loop

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
                
                try
                {
                    ZipFile.CreateFromDirectory(Path.Combine(baseFolder, $"{exportType}_{dateNow:yyyyMMddHHmmssfff}"), zipPath, CompressionLevel.Fastest, true);
                }
                catch (Exception ex)
                {
                    var errorDetails = $"[Zip Creation Error] " +
                                     $"Line: {ex.StackTrace?.Split('\n').FirstOrDefault(x => x.Contains("ExportKSKDataService.cs"))?.Trim() ?? "Unknown"}, " +
                                     $"Method: {ex.TargetSite?.Name ?? "Unknown"}, " +
                                     $"Library: {ex.TargetSite?.DeclaringType?.Assembly.GetName().Name ?? "Unknown"}, " +
                                     $"Error: {ex.Message}, " +
                                     $"Inner Exception: {ex.InnerException?.Message ?? "None"}";
                    
                    Console.WriteLine(errorDetails);
                    await _hubContext.Clients.Client(connectionId)
                        .SendAsync("ExportExaminationError", $"Lỗi tạo file nén: {ex.Message}", cancellationToken);
                    return;
                }

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
