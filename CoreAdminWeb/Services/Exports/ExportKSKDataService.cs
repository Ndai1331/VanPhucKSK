using CoreAdminWeb.Commons.Utils;
using CoreAdminWeb.Enums;
using CoreAdminWeb.Extensions;
using CoreAdminWeb.Helpers;
using CoreAdminWeb.Model;
using CoreAdminWeb.Model.KhamSucKhoes;
using CoreAdminWeb.Model.Settings;
using CoreAdminWeb.Services.BaseServices;
using CoreAdminWeb.Services.DocxToPdfConverter;
using CoreAdminWeb.Services.KhamSucKhoeApi;
using CoreAdminWeb.Services.PDFService;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.Extensions.Caching.Memory;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.IO.Compression;
using System.Text;


namespace CoreAdminWeb.Services.Exports
{
    public class ExportKSKDataService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IBaseDetailService<SoKhamSucKhoeModel> _soKhamSucKhoeService;
        private readonly IBaseDetailService<KhamSucKhoeChuyenKhoaModel> _khamSucKhoeChuyenKhoaService;
        private readonly IBaseDetailService<KhamSucKhoeSanPhuKhoaModel> _khamSucKhoeSanPhuKhoaService;
        private readonly IBaseDetailService<KhamSucKhoeKetLuanModel> _khamSucKhoeKetLuanService;
        private readonly IBaseDetailService<KhamSucKhoeTheLucModel> _khamSucKhoeTheLucService;
        private readonly IBaseDetailService<KhamSucKhoeKetQuaCanLamSangModel> _khamSucKhoeKetQuaCanLamSangService;
        private readonly IBaseDetailService<KhamSucKhoeNgheNghiepModel> _khamSucKhoeNgheNghiepService;
        private readonly IBaseDetailService<KhamSucKhoeTienSuModel> _khamSucKhoeTienSuService;
        private readonly IKhamSucKhoeAPIService<KetQuaCLSChiTietModel> _khamSucKhoeKQCLSDetailService;
        private readonly IBaseGetService<KetQuaCanLamSangFileModel> _ketQuaCanLamSangFileService;
        private readonly IPdfService _pdfService;
        private readonly IDocxToPdfConverter _docxToPdfConverter;
        public ExportKSKDataService(IServiceScopeFactory serviceScopeFactory, IMemoryCache memoryCache, IHttpClientFactory httpClientFactory)
        {
            _memoryCache = memoryCache;
            _httpClientFactory = httpClientFactory;
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
                _khamSucKhoeKQCLSDetailService = scope.ServiceProvider.GetRequiredService<IKhamSucKhoeAPIService<KetQuaCLSChiTietModel>>();
                _ketQuaCanLamSangFileService = scope.ServiceProvider.GetRequiredService<IBaseGetService<KetQuaCanLamSangFileModel>>();
                _pdfService = scope.ServiceProvider.GetRequiredService<IPdfService>();
                _docxToPdfConverter = scope.ServiceProvider.GetRequiredService<IDocxToPdfConverter>();
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
            var dateNow = DateTime.Now;
            var baseFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "exports", "kham_suc_khoe");
            var savePath = Path.Combine(baseFolder, $"{exportType}_{dateNow:yyyyMMddHHmmssfff}");

            try
            {
                await updateProgress.Invoke(new ProcessingModel()
                {
                    ProcessId = connectionId,
                    Status = TrangThaiXuLyNen.Processing,
                    Value = "Đang chuẩn bị dữ liệu..."
                });

                var prepareTemplate = await PrepareTemplateAsync(baseUrl, setting, exportType, connectionId, updateProgress, cancellationToken);
                if (!prepareTemplate.IsSuccess) return;

                // Tối ưu batch size khi truy vấn và ghi dữ liệu
                int batchSize = soKSKIds.Count switch
                {
                    >= 10000 => 1000,
                    >= 5000 => 500,
                    _ => 200
                };

                var prepareData = await PrepareData(soKSKIds, batchSize, connectionId, updateProgress);
                if (!prepareData.IsSuccess) return;

                // Chuẩn bị thư mục lưu file
                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }

                int totalRecords = prepareData.SoKhamSucKhoes.Count;
                int processed = 0;
                List<string> fileNames = new List<string>(totalRecords);

                switch (exportType)
                {
                    case HoSoKhamSucKhoeExportType.CheckListKsk:
                    case HoSoKhamSucKhoeExportType.ConsultationSlip:
                        await updateProgress.Invoke(new ProcessingModel()
                        {
                            ProcessId = connectionId,
                            Status = TrangThaiXuLyNen.Processing,
                            Value = $"Đang xuất dữ liệu {processed}/{totalRecords}"
                        });

                        // Duyệt theo batch để giảm memory pressure
                        foreach (var batch in prepareData.SoKhamSucKhoes.Chunk(batchSize))
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

                                var usingDocTemplate = item.benh_nhan?.gioi_tinh == GioiTinh.Nam ? prepareTemplate.FemaleDocument : prepareTemplate.MaleDocument;
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
                                            var checkListResult = await ProcessingCheckListKskDocumentReplacement(doc, item, connectionId, updateProgress, cancellationToken);
                                            if (!checkListResult)
                                                return;

                                            break;
                                        case HoSoKhamSucKhoeExportType.ConsultationSlip:
                                            var consultationSlipResult = await ProcessingConsultationSlipDocumentReplacement(doc, item, setting, prepareData, connectionId, updateProgress, cancellationToken);
                                            if (!consultationSlipResult)
                                                return;
                                            break;
                                    }
                                }

                                var pdfBytes = await _docxToPdfConverter.ConvertFileAsync(tempTemplatePath, cancellationToken);
                                if (File.Exists(tempTemplatePath))
                                {
                                    File.Delete(tempTemplatePath);
                                }

                                string filename = $"{item.benh_nhan?.full_name}_{item.ma_luot_kham}_{exportType.GetDescription()}_{(item.benh_nhan?.gioi_tinh == GioiTinh.Nam ? "Nam" : "Nu")}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf".ToUnsign(spaceReplacement: "-");
                                string fullFilePath = Path.Combine(savePath, filename);
                                await File.WriteAllBytesAsync(fullFilePath, pdfBytes, cancellationToken);

                                fileNames.Add(filename);

                                // Chỉ gửi progress mỗi 50 bản ghi
                                if (processed % 50 == 0 || processed == totalRecords)
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
                        foreach (var item in prepareData.SoKhamSucKhoes)
                        {
                            // Chỉ gửi progress mỗi 50 bản ghi
                            if (processed % 50 == 0 || processed == totalRecords)
                            {
                                await updateProgress.Invoke(new ProcessingModel()
                                {
                                    ProcessId = connectionId,
                                    Status = TrangThaiXuLyNen.Processing,
                                    Value = $"Đang xuất dữ liệu {processed}/{totalRecords}"
                                });
                            }

                            string filename = $"{item.benh_nhan?.full_name}_{item.ma_luot_kham}_{exportType.GetDescription()}_{(item.benh_nhan?.gioi_tinh == GioiTinh.Nam ? "Nam" : "Nu")}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf".ToUnsign(spaceReplacement: "-");
                            var pdfBytes = await ProcessingHtmlData(prepareTemplate.TemplateContent, filename, item, prepareData);

                            string fullFilePath = Path.Combine(savePath, filename);
                            await File.WriteAllBytesAsync(fullFilePath, pdfBytes, cancellationToken);

                            fileNames.Add(filename);
                        }
                        break;

                    case HoSoKhamSucKhoeExportType.MedicalExamination:
                        {
                            decimal processPercent = 0;
                            await updateProgress.Invoke(new ProcessingModel()
                            {
                                ProcessId = connectionId,
                                Status = TrangThaiXuLyNen.Processing,
                                Value = $"Đang xuất dữ liệu {processPercent}%"
                            });

                            var cpuCount = Environment.ProcessorCount;
                            var maxParallel = Math.Clamp(Math.Min(cpuCount * 2, 16), 4, 16);
                            using var semaphore = new SemaphoreSlim(maxParallel, maxParallel);
                            var tasks = new List<Task>(prepareData.SoKhamSucKhoes.Count);
                            foreach (var item in prepareData.SoKhamSucKhoes)
                            {
                                await semaphore.WaitAsync(cancellationToken);
                                var t = Task.Run(async () =>
                                {
                                    try
                                    {
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

                                        var usingDocTemplate = item.benh_nhan?.gioi_tinh == GioiTinh.Nam ? prepareTemplate.FemaleDocument : prepareTemplate.MaleDocument;
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

                                        // Khởi tạo file PDF từ template HTML trước
                                        string filename = $"{item.benh_nhan?.full_name}_{item.ma_luot_kham}_{exportType.GetDescription()}_{(item.benh_nhan?.gioi_tinh == GioiTinh.Nam ? "Nam" : "Nu")}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf".ToUnsign(spaceReplacement: "-");
                                        var pdfBytes = await ProcessingHtmlData(prepareTemplate.TemplateContent, filename, item, prepareData);

                                        // Tạo file tạm cho mẫu Word
                                        var tempTemplatePath = Path.Combine(Path.GetTempPath(), $"{item.ma_luot_kham}_temp.docx");
                                        await File.WriteAllBytesAsync(tempTemplatePath, usingDocTemplate, cancellationToken);

                                        // Xử lý file Word
                                        using (var doc = WordprocessingDocument.Open(tempTemplatePath, true))
                                        {
                                            var consultationSlipResult = await ProcessingConsultationSlipDocumentReplacement(doc, item, setting, prepareData, connectionId, updateProgress, cancellationToken);
                                            if (!consultationSlipResult)
                                                return;
                                        }

                                        // Chuyển Word đã điền dữ liệu sang PDF
                                        var pdfContent = await _docxToPdfConverter.ConvertFileAsync(tempTemplatePath, cancellationToken);
                                        if (File.Exists(tempTemplatePath))
                                        {
                                            File.Delete(tempTemplatePath);
                                        }

                                        // --- New: merge generated pdfBytes with pdfContent from Word export ---
                                        if (pdfContent != null && pdfContent.Length > 0)
                                        {
                                            try
                                            {
                                                using var mergedDoc = new PdfDocument();
                                                // Append original generated PDF first
                                                using (var msMain = new MemoryStream(pdfBytes))
                                                {
                                                    var mainDoc = PdfReader.Open(msMain, PdfDocumentOpenMode.Import);
                                                    for (int p = 0; p < mainDoc.PageCount; p++)
                                                    {
                                                        mergedDoc.AddPage(mainDoc.Pages[p]);
                                                    }
                                                }

                                                // Only try to import if bytes look like a PDF (starts with %PDF)
                                                if (pdfContent.Length >= 4 && pdfContent[0] == '%' && pdfContent[1] == 'P' && pdfContent[2] == 'D' && pdfContent[3] == 'F')
                                                {
                                                    using var msExtra = new MemoryStream(pdfContent);
                                                    try
                                                    {
                                                        var extraDoc = PdfReader.Open(msExtra, PdfDocumentOpenMode.Import);
                                                        for (int p = 0; p < extraDoc.PageCount; p++)
                                                        {
                                                            mergedDoc.AddPage(extraDoc.Pages[p]);
                                                        }
                                                    }
                                                    catch
                                                    {
                                                        // skip corrupt/unreadable PDF
                                                    }
                                                }

                                                // Save merged PDF back to pdfBytes
                                                using var outMs = new MemoryStream();
                                                mergedDoc.Save(outMs);
                                                pdfBytes = outMs.ToArray();
                                            }
                                            catch (Exception ex)
                                            {
                                                // Log or ignore and continue with original pdfBytes
                                                Console.WriteLine($"Warning: failed to merge related PDFs for {item.ma_luot_kham}: {ex.Message}");
                                            }
                                        }

                                        var httpClient = _httpClientFactory.CreateClient();
                                        pdfBytes = await MergeWithRelatedPdfsAsync(
                                            pdfBytes,
                                            prepareData,
                                            item,
                                            baseUrl,
                                            httpClient,
                                            cancellationToken
                                        );

                                        string fullFilePath = Path.Combine(savePath, filename);
                                        await File.WriteAllBytesAsync(fullFilePath, pdfBytes, cancellationToken);
                                        fileNames.Add(filename);

                                        processed++;
                                        // Chỉ gửi progress mỗi 50 bản ghi

                                        var tempPercent = Math.Round(processed / (decimal)totalRecords * 100, 2);
                                        if (processPercent != tempPercent)
                                        {
                                            processPercent = tempPercent;
                                            await updateProgress.Invoke(new ProcessingModel()
                                            {
                                                ProcessId = connectionId,
                                                Status = TrangThaiXuLyNen.Processing,
                                                Value = $"Đang xuất dữ liệu {processPercent}%"
                                            });
                                        }
                                    }
                                    finally
                                    {
                                        semaphore.Release();
                                    }
                                }, cancellationToken);
                                tasks.Add(t);
                            }

                            await Task.WhenAll(tasks);
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

                    if (!string.IsNullOrWhiteSpace(savePath) && Directory.Exists(savePath))
                    {
                        GC.WaitForPendingFinalizers();
                        Directory.Delete(savePath, true);
                    }
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

                if (!string.IsNullOrWhiteSpace(savePath) && Directory.Exists(savePath))
                {
                    GC.WaitForPendingFinalizers();
                    Directory.Delete(savePath, true);
                }
            }
        }

        private async Task<PrepareDataModel> PrepareData(List<int> soKSKIds,
                           int batchSize,
                           string connectionId,
                           Func<ProcessingModel, Task> updateProgress)
        {
            try
            {
                // Lấy dữ liệu chính theo batch lớn
                List<SoKhamSucKhoeModel> soKhamSucKhoes = await ExportKSKHelpers.BatchQueryAsync(
                    ids => _soKhamSucKhoeService.GetAllAsync($"filter[_and][][id][_in]={string.Join(",", ids)}&limit={batchSize}"),
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
                    return new PrepareDataModel(
                        IsSuccess: false,
                        SoKhamSucKhoes: new List<SoKhamSucKhoeModel>(),
                        ChuyenKhoas: null,
                        SanPhuKhoas: null,
                        KetLuans: null,
                        TheLucs: null,
                        KetQuaCanLamSangs: null,
                        NgheNghieps: null,
                        TienSus: null,
                        KetQuaCLSChiTiets: null,
                        KetQuaCLSFiles: null);
                }

                var maLuotKhams = soKhamSucKhoes.Where(c => !string.IsNullOrEmpty(c.ma_luot_kham)).Select(c => c.ma_luot_kham!).ToList();

                List<KhamSucKhoeChuyenKhoaModel>? chuyenKhoas = null;
                List<KhamSucKhoeSanPhuKhoaModel>? sanPhuKhoas = null;
                List<KhamSucKhoeKetLuanModel>? ketLuans = null;
                List<KhamSucKhoeTheLucModel>? theLucs = null;
                List<KhamSucKhoeKetQuaCanLamSangModel>? kqCLSs = null;
                List<KhamSucKhoeNgheNghiepModel>? ngheNghieps = null;
                List<KhamSucKhoeTienSuModel>? tienSus = null;
                List<KetQuaCLSChiTietModel>? kqCLSCTs = null;
                List<KetQuaCanLamSangFileModel>? kqCLSFiles = null;
                var tasks = new List<Task>();

                // Lấy dữ liệu liên quan song song
                tasks.Add(Task.Run(async () =>
                {
                    chuyenKhoas = await ExportKSKHelpers.BatchQueryAsync(
                        ids => _khamSucKhoeChuyenKhoaService.GetAllAsync($"filter[_or][0][ma_luot_kham][_in]={string.Join(",", maLuotKhams)}&filter[_or][1][luot_kham][_in]={string.Join(",", ids)}&limit={batchSize}"),
                        soKSKIds, batchSize
                    );
                }));
                tasks.Add(Task.Run(async () =>
                {
                    sanPhuKhoas = await ExportKSKHelpers.BatchQueryAsync(
                        ids => _khamSucKhoeSanPhuKhoaService.GetAllAsync($"filter[_or][0][ma_luot_kham][_in]={string.Join(",", maLuotKhams)}&filter[_or][1][luot_kham][_in]={string.Join(",", ids)}&limit={batchSize}"),
                        soKSKIds, batchSize
                    );
                }));
                tasks.Add(Task.Run(async () =>
                {
                    ketLuans = await ExportKSKHelpers.BatchQueryAsync(
                        ids => _khamSucKhoeKetLuanService.GetAllAsync($"filter[_or][0][ma_luot_kham][_in]={string.Join(",", maLuotKhams)}&filter[_or][1][luot_kham][_in]={string.Join(",", ids)}&limit={batchSize}"),
                        soKSKIds, batchSize
                    );
                }));
                tasks.Add(Task.Run(async () =>
                {
                    theLucs = await ExportKSKHelpers.BatchQueryAsync(
                        ids => _khamSucKhoeTheLucService.GetAllAsync($"filter[_or][0][ma_luot_kham][_in]={string.Join(",", maLuotKhams)}&filter[_or][1][luot_kham][_in]={string.Join(",", ids)}&limit={batchSize}"),
                        soKSKIds, batchSize
                    );
                }));
                tasks.Add(Task.Run(async () =>
                {
                    kqCLSs = await ExportKSKHelpers.BatchQueryAsync(
                        ids => _khamSucKhoeKetQuaCanLamSangService.GetAllAsync($"filter[_or][0][ma_luot_kham][_in]={string.Join(",", maLuotKhams)}&filter[_or][1][luot_kham][_in]={string.Join(",", ids)}&limit={batchSize}"),
                        soKSKIds, batchSize
                    );
                }));
                tasks.Add(Task.Run(async () =>
                {
                    ngheNghieps = await ExportKSKHelpers.BatchQueryAsync(
                        ids => _khamSucKhoeNgheNghiepService.GetAllAsync($"filter[_or][0][ma_luot_kham][_in]={string.Join(",", maLuotKhams)}&filter[_or][1][luot_kham][_in]={string.Join(",", ids)}&limit={batchSize}"),
                        soKSKIds, batchSize
                    );
                }));
                tasks.Add(Task.Run(async () =>
                {
                    tienSus = await ExportKSKHelpers.BatchQueryAsync(
                        ids => _khamSucKhoeTienSuService.GetAllAsync($"filter[_or][0][ma_luot_kham][_in]={string.Join(",", maLuotKhams)}&filter[_or][1][luot_kham][_in]={string.Join(",", ids)}&limit={batchSize}"),
                        soKSKIds, batchSize
                    );
                }));
                tasks.Add(Task.Run(async () =>
                {
                    kqCLSCTs = await ExportKSKHelpers.BatchQueryAsync(
                        ids => _khamSucKhoeKQCLSDetailService.GetAllAsync($"KhamSucKhoeKQCLS/get-ket-qua-chi-tiet?{string.Join("&", ids.Select(c => $"maLuotKhams={c}"))}&loaiCLS=4"),
                        maLuotKhams, batchSize
                    );
                }));
                tasks.Add(Task.Run(async () =>
                {
                    kqCLSFiles = await ExportKSKHelpers.BatchQueryAsync(
                        ids => _ketQuaCanLamSangFileService.GetAllAsync($"filter[_and][][ma_luot_kham][_in]={string.Join(",", maLuotKhams)}&limit={batchSize}"),
                        soKSKIds, batchSize
                    );
                }));

                await Task.WhenAll(tasks);

                return new PrepareDataModel(
                    IsSuccess: true,
                    SoKhamSucKhoes: soKhamSucKhoes,
                    ChuyenKhoas: chuyenKhoas,
                    SanPhuKhoas: sanPhuKhoas,
                    KetLuans: ketLuans,
                    TheLucs: theLucs,
                    KetQuaCanLamSangs: kqCLSs,
                    NgheNghieps: ngheNghieps,
                    TienSus: tienSus,
                    KetQuaCLSChiTiets: kqCLSCTs,
                    KetQuaCLSFiles: kqCLSFiles
                );
            }
            catch (Exception ex)
            {
                await updateProgress.Invoke(new ProcessingModel()
                {
                    ProcessId = connectionId,
                    Status = TrangThaiXuLyNen.Error,
                    Value = $"Lỗi khi chuẩn bị dữ liệu xuất: {ex.Message}"
                });
                return new PrepareDataModel(
                    IsSuccess: false,
                    SoKhamSucKhoes: new List<SoKhamSucKhoeModel>(),
                    ChuyenKhoas: null,
                    SanPhuKhoas: null,
                    KetLuans: null,
                    TheLucs: null,
                    KetQuaCanLamSangs: null,
                    NgheNghieps: null,
                    TienSus: null,
                    KetQuaCLSChiTiets: null,
                    KetQuaCLSFiles: null);
            }
        }

        private async Task<(
            bool IsSuccess,
            byte[]? FemaleDocument,
            byte[]? MaleDocument,
            string TemplateContent
        )> PrepareTemplateAsync(string baseUrl,
                                SettingModel setting,
                                HoSoKhamSucKhoeExportType exportType,
                                string connectionId,
                                Func<ProcessingModel, Task> updateProgress,
                                CancellationToken cancellationToken)
        {
            var tasks = new List<Task>();
            byte[]? docTemplate1Bytes = null;
            byte[]? docTemplate2Bytes = null;
            string? templateContent = null;

            string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "templates", "hsksk-tt32.html");

            await updateProgress.Invoke(new ProcessingModel()
            {
                ProcessId = connectionId,
                Status = TrangThaiXuLyNen.Processing,
                Value = "Đang kiểm tra mẫu xuất dữ liệu..."
            });

            var exportTemplateNu = exportType switch
            {
                HoSoKhamSucKhoeExportType.ConsultationSlip => setting.thcn_nu,
                HoSoKhamSucKhoeExportType.CheckListKsk => setting.phieu_ksk_nu,
                HoSoKhamSucKhoeExportType.MedicalExamination => setting.thcn_nu,
                _ => null
            };
            var exportTemplateNam = exportType switch
            {
                HoSoKhamSucKhoeExportType.ConsultationSlip => setting.thcn_nam,
                HoSoKhamSucKhoeExportType.CheckListKsk => setting.phieu_ksk_nam,
                HoSoKhamSucKhoeExportType.MedicalExamination => setting.thcn_nam,
                _ => null
            };

            if (exportType is HoSoKhamSucKhoeExportType.MedicalExamination or HoSoKhamSucKhoeExportType.ConsultationSlip or HoSoKhamSucKhoeExportType.CheckListKsk)
            {
                StringBuilder errorBuilder = new StringBuilder();
                if (exportTemplateNam == null)
                {
                    errorBuilder.AppendLine($"'{exportType.GetDescription()} cho Nam'");
                }
                else
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        using var http = new HttpClient();
                        var res = await http.GetByteArrayAsync($"{baseUrl}assets/{exportTemplateNam.id}", cancellationToken);
                        docTemplate1Bytes = res;
                    }, cancellationToken));
                }

                if (exportTemplateNu == null)
                {
                    if (errorBuilder.Length > 0)
                    {
                        errorBuilder.AppendLine(", ");
                    }
                    errorBuilder.AppendLine($"'{exportType.GetDescription()} cho Nữ'");
                }
                else
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        using var http = new HttpClient();
                        var res = await http.GetByteArrayAsync($"{baseUrl}assets/{exportTemplateNu.id}", cancellationToken);
                        docTemplate2Bytes = res;
                    }, cancellationToken));
                }

                if (errorBuilder.Length > 0)
                {
                    await updateProgress.Invoke(new ProcessingModel()
                    {
                        ProcessId = connectionId,
                        Status = TrangThaiXuLyNen.Error,
                        Value = $"Mẫu {errorBuilder} không tồn tại."
                    });
                    return (IsSuccess: false, FemaleDocument: null, MaleDocument: null, TemplateContent: string.Empty);
                }
            }

            if (exportType is HoSoKhamSucKhoeExportType.MedicalExamination or HoSoKhamSucKhoeExportType.HealthCheckupBook)
            {
                if (File.Exists(templatePath))
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        var res = await File.ReadAllTextAsync(templatePath, cancellationToken);
                        templateContent = res;
                    }, cancellationToken));
                }
                else
                {
                    await updateProgress.Invoke(new ProcessingModel()
                    {
                        ProcessId = connectionId,
                        Status = TrangThaiXuLyNen.Error,
                        Value = $"Mẫu '{exportType.GetDescription()}' không tồn tại."
                    });
                    return (IsSuccess: false, FemaleDocument: null, MaleDocument: null, TemplateContent: string.Empty);
                }
            }

            await Task.WhenAll(tasks);

            if (exportTemplateNam != null && docTemplate1Bytes == null || exportTemplateNu != null && docTemplate2Bytes == null || templateContent == null)
            {
                StringBuilder errorBuilder = new StringBuilder();
                if (exportType is HoSoKhamSucKhoeExportType.MedicalExamination or HoSoKhamSucKhoeExportType.ConsultationSlip or HoSoKhamSucKhoeExportType.CheckListKsk)
                {
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
                }

                if (exportType is HoSoKhamSucKhoeExportType.MedicalExamination or HoSoKhamSucKhoeExportType.HealthCheckupBook && string.IsNullOrWhiteSpace(templateContent))
                {
                    if (errorBuilder.Length > 0)
                    {
                        errorBuilder.AppendLine(", ");
                    }
                    errorBuilder.AppendLine($"'{exportType.GetDescription()}'");
                }

                if (errorBuilder.Length > 0)
                {
                    await updateProgress.Invoke(new ProcessingModel()
                    {
                        ProcessId = connectionId,
                        Status = TrangThaiXuLyNen.Error,
                        Value = $"Mẫu {errorBuilder} không tồn tại hoặc đã bị xóa."
                    });

                    return (IsSuccess: false, FemaleDocument: null, MaleDocument: null, TemplateContent: string.Empty);
                }
            }

            return (IsSuccess: true, FemaleDocument: docTemplate1Bytes, MaleDocument: docTemplate2Bytes, TemplateContent: templateContent ?? string.Empty);
        }

        public record PrepareDataModel(
            bool IsSuccess,
            List<SoKhamSucKhoeModel> SoKhamSucKhoes,
            List<KhamSucKhoeChuyenKhoaModel>? ChuyenKhoas,
            List<KhamSucKhoeSanPhuKhoaModel>? SanPhuKhoas,
            List<KhamSucKhoeKetLuanModel>? KetLuans,
            List<KhamSucKhoeTheLucModel>? TheLucs,
            List<KhamSucKhoeKetQuaCanLamSangModel>? KetQuaCanLamSangs,
            List<KhamSucKhoeNgheNghiepModel>? NgheNghieps,
            List<KhamSucKhoeTienSuModel>? TienSus,
            List<KetQuaCLSChiTietModel>? KetQuaCLSChiTiets,
            List<KetQuaCanLamSangFileModel>? KetQuaCLSFiles
        );

        private async Task<bool> ProcessingCheckListKskDocumentReplacement(WordprocessingDocument doc,
                                                                    SoKhamSucKhoeModel soKhamSucKhoe,
                                                                    string connectionId,
                                                                    Func<ProcessingModel, Task> updateProgress,
                                                                    CancellationToken cancellationToken)
        {
            try
            {
                var qrCode = QRHelper.GenerateQRCode($"{soKhamSucKhoe.ma_luot_kham}");
                doc.ReplaceText(new Dictionary<string, string>
                                            {
                                                { "<<HoVaTen>>", $"{soKhamSucKhoe.benh_nhan?.full_name}" },
                                                { "<<GioiTinh>>", $"{soKhamSucKhoe.benh_nhan?.gioi_tinh?.GetDescription()}" },
                                                { "<<STT_KSK>>", $"{soKhamSucKhoe.sort}" },
                                                { "<<MaLuotKham>>", $"{soKhamSucKhoe.ma_luot_kham}" },
                                                { "<<NgaySinh>>", $"{soKhamSucKhoe.benh_nhan?.ngay_sinh:dd/MM/yyyy}" },
                                                { "<<X>>", $"{DateTimeUtil.CalculateAge(soKhamSucKhoe.benh_nhan?.ngay_sinh?.Date)}" },
                                                { "<<SoDinhDanh>>", $"{soKhamSucKhoe.benh_nhan?.so_dinh_danh}" },
                                                { "<<SoDienThoai>>", $"{soKhamSucKhoe.benh_nhan?.so_dien_thoai}" }
                                            });
                doc.ReplaceImage("<<QR>>", qrCode, null, 500000, 500000);
            }
            catch (Exception ex)
            {
                var errorDetails = $"[CheckListKsk Processing Error] Item: {soKhamSucKhoe.ma_luot_kham}, " +
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

                return false;
            }

            return true;
        }

        private async Task<bool> ProcessingConsultationSlipDocumentReplacement(WordprocessingDocument doc,
                                                                            SoKhamSucKhoeModel soKhamSucKhoe,
                                                                            SettingModel setting,
                                                                            PrepareDataModel prepareData,
                                                                            string connectionId,
                                                                            Func<ProcessingModel, Task> updateProgress,
                                                                            CancellationToken cancellationToken)
        {
            try
            {
                var theLuc = prepareData.TheLucs?.FirstOrDefault(x => x.ma_luot_kham == soKhamSucKhoe.ma_luot_kham);
                var cls = prepareData.ChuyenKhoas?.FirstOrDefault(x => x.ma_luot_kham == soKhamSucKhoe.ma_luot_kham);
                var sanPhuKhoa = prepareData.SanPhuKhoas?.FirstOrDefault(x => x.ma_luot_kham == soKhamSucKhoe.ma_luot_kham);
                var kqcls = prepareData.KetQuaCanLamSangs?.Where(x => x.ma_luot_kham == soKhamSucKhoe.ma_luot_kham && !string.IsNullOrEmpty(x.ket_qua)).ToList();
                var ketLuan = prepareData.KetLuans?.FirstOrDefault(x => x.ma_luot_kham == soKhamSucKhoe.ma_luot_kham);

                doc.ReplaceText(new Dictionary<string, string>
                                            {
                                                { "<<STT_KSK>>", $"{soKhamSucKhoe.sort}" },
                                                { "<<MaLuotKham>>", $"{soKhamSucKhoe.ma_luot_kham}" },
                                                { "<<HoVaTen>>", $"{soKhamSucKhoe.benh_nhan?.full_name}" },
                                                { "<<GioiTinh>>", $"{soKhamSucKhoe.benh_nhan?.gioi_tinh?.GetDescription()}" },
                                                { "<<CongTy>>", $"{soKhamSucKhoe.MaDotKham?.ma_hop_dong_ksk?.cong_ty?.code}" },
                                                { "<<NamSinh>>", $"{soKhamSucKhoe.benh_nhan?.ngay_sinh:yyyy}" },
                                                { "<<ChieuCao>>", $"{theLuc?.chieu_cao}" },
                                                { "<<CanNang>>", $"{theLuc?.can_nang}" },
                                                { "<<BMI>>", $"{theLuc?.bmi}" },
                                                { "<<Mach>>", $"{theLuc?.mach}" },
                                                { "<<HuyetAp>>", $"{theLuc?.huyet_ap}" },
                                                { "<<MaTaiKhoan>>", $"{soKhamSucKhoe.benh_nhan?.ma_tai_khoan}" },
                                                { "<<TenCongTy>>", $"{soKhamSucKhoe.MaDotKham?.ma_hop_dong_ksk?.cong_ty?.name}" },
                                                { "<<PhanLoaiSucKhoe>>", $"{ketLuan?.phan_loai_suc_khoe?.name}" },
                                                { "<<KetLuan>>", $"{ketLuan?.benh_tat_ket_luan}" },
                                                { "<<TuVan>>", $"{ketLuan?.tu_van}" },
                                                { "<<NgayKham>>", $"{soKhamSucKhoe.ngay_kham ?? soKhamSucKhoe.ngay_lap_so:dd}" },
                                                { "<<ThangKham>>", $"{soKhamSucKhoe.ngay_kham ?? soKhamSucKhoe.ngay_lap_so:MM}" },
                                                { "<<NamKham>>", $"{soKhamSucKhoe.ngay_kham ?? soKhamSucKhoe.ngay_lap_so:yyyy}" }
                                            });

                byte[]? signImg = null;
                string? chuKyBS = ketLuan?.bs_ket_luan?.chu_ky_bac_si ?? ketLuan?.chu_ky;
                if (!string.IsNullOrEmpty(chuKyBS))
                {
                    try
                    {
                        signImg = Convert.FromBase64String(chuKyBS);
                    }
                    catch { }
                }

                string fullNameBSKL = $"{ketLuan?.bs_ket_luan?.chuc_danh} {ketLuan?.bs_ket_luan?.full_name}";
                if (string.IsNullOrWhiteSpace(fullNameBSKL))
                    fullNameBSKL = $"{ketLuan?.nguoi_ket_luan}";
                doc.ReplaceImage("<<BSKetLuan>>", signImg, fullNameBSKL, 900000, 500000);

                StringBuilder stringBuilder = new StringBuilder();
                if (!string.IsNullOrEmpty(cls?.kq_nk_tuan_hoan) && cls.kq_nk_tuan_hoan != setting.ket_qua_ksk_mac_dinh)
                {
                    if (stringBuilder.Length > 0)
                    {
                        stringBuilder.Append("\n");
                    }
                    stringBuilder.Append(cls.kq_nk_tuan_hoan);
                }
                if (!string.IsNullOrEmpty(cls?.kq_nk_ho_hap) && cls.kq_nk_ho_hap != setting.ket_qua_ksk_mac_dinh)
                {
                    if (stringBuilder.Length > 0)
                    {
                        stringBuilder.Append("\n");
                    }
                    stringBuilder.Append(cls.kq_nk_ho_hap);
                }
                if (!string.IsNullOrEmpty(cls?.kq_nk_tieu_hoa) && cls.kq_nk_tieu_hoa != setting.ket_qua_ksk_mac_dinh)
                {
                    if (stringBuilder.Length > 0)
                    {
                        stringBuilder.Append("\n");
                    }
                    stringBuilder.Append(cls.kq_nk_tieu_hoa);
                }
                if (!string.IsNullOrEmpty(cls?.kq_nk_noi_tiet) && cls.kq_nk_noi_tiet != setting.ket_qua_ksk_mac_dinh)
                {
                    if (stringBuilder.Length > 0)
                    {
                        stringBuilder.Append("\n");
                    }
                    stringBuilder.Append(cls.kq_nk_noi_tiet);
                }
                if (!string.IsNullOrEmpty(cls?.kq_nk_than_tiet_nieu) && cls.kq_nk_than_tiet_nieu != setting.ket_qua_ksk_mac_dinh)
                {
                    if (stringBuilder.Length > 0)
                    {
                        stringBuilder.Append("\n");
                    }
                    stringBuilder.Append(cls.kq_nk_than_tiet_nieu);
                }
                if (!string.IsNullOrEmpty(cls?.kq_nk_co_xuong_khop) && cls.kq_nk_co_xuong_khop != setting.ket_qua_ksk_mac_dinh)
                {
                    if (stringBuilder.Length > 0)
                    {
                        stringBuilder.Append("\n");
                    }
                    stringBuilder.Append(cls.kq_nk_co_xuong_khop);
                }
                if (!string.IsNullOrEmpty(cls?.kq_nk_than_kinh) && cls.kq_nk_than_kinh != setting.ket_qua_ksk_mac_dinh)
                {
                    if (stringBuilder.Length > 0)
                    {
                        stringBuilder.Append("\n");
                    }
                    stringBuilder.Append(cls.kq_nk_than_kinh);
                }
                if (!string.IsNullOrEmpty(cls?.kq_ngoai_khoa) && cls.kq_ngoai_khoa != setting.ket_qua_ksk_mac_dinh)
                {
                    if (stringBuilder.Length > 0)
                    {
                        stringBuilder.Append("\n");
                    }
                    stringBuilder.Append(cls.kq_ngoai_khoa);
                }
                if (!string.IsNullOrEmpty(cls?.benh_tai_mui_hong) && cls.benh_tai_mui_hong != setting.ket_qua_ksk_mac_dinh)
                {
                    if (stringBuilder.Length > 0)
                    {
                        stringBuilder.Append("\n");
                    }
                    stringBuilder.Append(cls.benh_tai_mui_hong);
                }
                if (!string.IsNullOrEmpty(cls?.kq_da_lieu) && cls.kq_da_lieu != setting.ket_qua_ksk_mac_dinh)
                {
                    if (stringBuilder.Length > 0)
                    {
                        stringBuilder.Append("\n");
                    }
                    stringBuilder.Append(cls.kq_da_lieu);
                }
                if (!string.IsNullOrEmpty(cls?.benh_mat) && cls.benh_mat != setting.ket_qua_ksk_mac_dinh)
                {
                    if (stringBuilder.Length > 0)
                    {
                        stringBuilder.Append("\n");
                    }
                    stringBuilder.Append(cls.benh_mat);
                }
                if (!string.IsNullOrEmpty(cls?.benh_rhm) && cls.benh_rhm != setting.ket_qua_ksk_mac_dinh)
                {
                    if (stringBuilder.Length > 0)
                    {
                        stringBuilder.Append("\n");
                    }
                    stringBuilder.Append(cls.benh_rhm);
                }
                if (!string.IsNullOrEmpty(cls?.kq_nk_tam_than) && cls.kq_nk_tam_than != setting.ket_qua_ksk_mac_dinh)
                {
                    if (stringBuilder.Length > 0)
                    {
                        stringBuilder.Append("\n");
                    }
                    stringBuilder.Append(cls.kq_nk_tam_than);
                }
                if (!string.IsNullOrEmpty(sanPhuKhoa?.ket_qua) && sanPhuKhoa?.ket_qua != setting.ket_qua_ksk_mac_dinh)
                {
                    if (stringBuilder.Length > 0)
                    {
                        stringBuilder.Append("\n");
                    }
                    stringBuilder.Append(sanPhuKhoa?.ket_qua);
                }
                doc.ReplaceText(new Dictionary<string, string>
                                            {
                                                { "<<KhamTongQuat>>",stringBuilder.ToString() }
                                            });

                stringBuilder.Clear();
                var cdhatdcn = kqcls?.FirstOrDefault(c => c.type == KetQuaCanLamSang.CDHATDCN.ToString());
                stringBuilder.AppendLine($"{cdhatdcn?.ket_qua}");
                doc.ReplaceText(new Dictionary<string, string>
                                            {
                                                { "<<KetQuaCDHATDCN>>",stringBuilder.ToString() }
                                            });


                List<dynamic> kqxn = new List<dynamic>();
                if (kqcls != null && kqcls.Any())
                {
                    var kqxnMau = kqcls.FirstOrDefault(c => c.type == KetQuaCanLamSang.XNCongThucMau.ToString());
                    if (kqxnMau != null && kqxnMau.ket_qua != null)
                    {
                        kqxn.Add(new { TenXetNghiem = "Xét nghiệm công thức máu", KetQua = kqxnMau.ket_qua, ThamChieu = "Bình thường" });
                    }

                    var kqxnNuocTieu = kqcls.FirstOrDefault(c => c.type == KetQuaCanLamSang.XNNuocTieu.ToString());
                    if (kqxnNuocTieu != null && kqxnNuocTieu.ket_qua != null)
                    {
                        kqxn.Add(new { TenXetNghiem = "Xét nghiệm nước tiểu", KetQua = kqxnNuocTieu.ket_qua, ThamChieu = "Bình thường" });
                    }
                }

                var kqCLSCTs = prepareData.KetQuaCLSChiTiets?.Where(x => x.ma_luot_kham == soKhamSucKhoe.ma_luot_kham && !string.IsNullOrEmpty(x.ket_qua_chi_so)).ToList();
                if (kqCLSCTs != null && kqCLSCTs.Any())
                {
                    foreach (var kq in kqCLSCTs)
                    {
                        kqxn.Add(new
                        {
                            TenXetNghiem = kq.ten_cls,
                            KetQua = kq.ket_qua_chi_so,
                            ThamChieu = kq.gia_tri
                        });
                    }
                }
                if (!kqxn.Any())
                {
                    kqxn.Add(new { TenXetNghiem = "", KetQua = "", ThamChieu = "" });
                }
                doc.ReplaceTableRowsWithKqxn(kqxn);
            }
            catch (Exception ex)
            {
                var errorDetails = $"[ConsultationSlip Processing Error] Item: {soKhamSucKhoe.ma_luot_kham}, " +
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
                return false;
            }

            return true;
        }

        private async Task<byte[]> ProcessingHtmlData(string template, string filename, SoKhamSucKhoeModel item, PrepareDataModel prepareData)
        {
            var theLuc = prepareData.TheLucs?.FirstOrDefault(x => x.ma_luot_kham == item.ma_luot_kham);
            var cls = prepareData.ChuyenKhoas?.FirstOrDefault(x => x.ma_luot_kham == item.ma_luot_kham);
            var sanPhuKhoa = prepareData.SanPhuKhoas?.FirstOrDefault(x => x.ma_luot_kham == item.ma_luot_kham);
            var kqcls = prepareData.KetQuaCanLamSangs?.Where(x => x.ma_luot_kham == item.ma_luot_kham && !string.IsNullOrEmpty(x.ket_qua)).ToList();
            var ketLuan = prepareData.KetLuans?.FirstOrDefault(x => x.ma_luot_kham == item.ma_luot_kham);
            var ngheNghiep = prepareData.NgheNghieps?.FirstOrDefault(x => x.ma_luot_kham == item.ma_luot_kham);
            var tienSu = prepareData.TienSus?.FirstOrDefault(x => x.ma_luot_kham == item.ma_luot_kham);

            string fullNameBSKL = $"{ketLuan?.bs_ket_luan?.chuc_danh} {ketLuan?.bs_ket_luan?.full_name}";
            if (string.IsNullOrWhiteSpace(fullNameBSKL))
                fullNameBSKL = $"{ketLuan?.nguoi_ket_luan}";

            var tempTempalte = template
                .Replace("{{TenBenhNhan}}", $"{item.benh_nhan?.full_name.ToUpper()}")
                .Replace("{{GioiTinh_Nam}}", ExportKSKHelpers.CheckBoxHtmlBuilder(item.benh_nhan?.gioi_tinh == GioiTinh.Nam))
                .Replace("{{GioiTinh_Nu}}", ExportKSKHelpers.CheckBoxHtmlBuilder(item.benh_nhan?.gioi_tinh == GioiTinh.Nu))
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
                .Replace("{{TienSuBenhTatGiaDinh}}", ExportKSKHelpers.MultilineSpanHtmlBuilder(tienSu?.tien_su_gia_dinh ?? string.Empty, 4))
                .Replace("{{TenBenh}}", $"{tienSu?.ten_benh}")
                .Replace("{{NamPhatHien}}", $"{tienSu?.nam_phat_hien}")
                .Replace("{{BenhNgheNghiep}}", $"{tienSu?.benh_nghe_nghiep}")
                .Replace("{{NamPhatHienBenhNgheNghiep}}", $"{tienSu?.nam_phat_hien_benh_nghe_nghiep}")
                .Replace("{{ThoiGianLapSo_Ngay}}", $"{item.ngay_lap_so ?? DateTime.Now:dd}")
                .Replace("{{ThoiGianLapSo_Thang}}", $"{item.ngay_lap_so ?? DateTime.Now:MM}")
                .Replace("{{ThoiGianLapSo_Nam}}", $"{item.ngay_lap_so ?? DateTime.Now:yyyy}")
                .Replace("{{NguoiLapSo}}", ExportKSKHelpers.RenderSignature(item.chu_ky_nls ?? string.Empty, "<div style=\"height: 20mm\"></div>", 100, 50) + $"<br/><span class=\"bold\">{item.nguoi_lap}</span>")
                .Replace("{{NamBatDauKinhNguyet}}", ExportKSKHelpers.TableTdHtmlBuilder(item.benh_nhan?.gioi_tinh == GioiTinh.Nu ? sanPhuKhoa?.tuoi_bat_dau_kinh?.ToString() : ""))
                .Replace("{{KinhNguyet_Deu}}", ExportKSKHelpers.CheckBoxHtmlBuilder(item.benh_nhan?.gioi_tinh == GioiTinh.Nu && sanPhuKhoa?.tinh_chat_kinh == TinhChatKinh.Deu.ToString()))
                .Replace("{{KinhNguyet_KhongDeu}}", ExportKSKHelpers.CheckBoxHtmlBuilder(item.benh_nhan?.gioi_tinh == GioiTinh.Nu && sanPhuKhoa?.tinh_chat_kinh == TinhChatKinh.KhongDeu.ToString()))
                .Replace("{{ChuKyKinh}}", ExportKSKHelpers.TableTdHtmlBuilder(item.benh_nhan?.gioi_tinh == GioiTinh.Nu ? sanPhuKhoa?.chu_ky_kinh : ""))
                .Replace("{{LuongKinh}}", ExportKSKHelpers.TableTdHtmlBuilder(item.benh_nhan?.gioi_tinh == GioiTinh.Nu ? sanPhuKhoa?.luong_kinh : ""))
                .Replace("{{DauBungKinh_Co}}", ExportKSKHelpers.CheckBoxHtmlBuilder(item.benh_nhan?.gioi_tinh == GioiTinh.Nu && sanPhuKhoa?.dau_bung_kinh == true))
                .Replace("{{DauBungKinh_Khong}}", ExportKSKHelpers.CheckBoxHtmlBuilder(item.benh_nhan?.gioi_tinh == GioiTinh.Nu && sanPhuKhoa?.dau_bung_kinh == false))
                .Replace("{{DaLapGiaDinh_Co}}", ExportKSKHelpers.CheckBoxHtmlBuilder(item.benh_nhan?.gioi_tinh == GioiTinh.Nu && sanPhuKhoa?.da_lap_gia_dinh == true))
                .Replace("{{DaLapGiaDinh_Chua}}", ExportKSKHelpers.CheckBoxHtmlBuilder(item.benh_nhan?.gioi_tinh == GioiTinh.Nu && sanPhuKhoa?.da_lap_gia_dinh == false))
                .Replace("{{PARA}}", ExportKSKHelpers.TableTdHtmlBuilder(item.benh_nhan?.gioi_tinh == GioiTinh.Nu ? sanPhuKhoa?.para : "", 4))
                .Replace("{{MoSanPhuKhoa_Co}}", ExportKSKHelpers.CheckBoxHtmlBuilder(item.benh_nhan?.gioi_tinh == GioiTinh.Nu && sanPhuKhoa?.so_lan_mo_san_phu_khoa == (int)YesNo.Co))
                .Replace("{{MoSanPhuKhoa_GhiRo}}", $"{(item.benh_nhan?.gioi_tinh == GioiTinh.Nu ? sanPhuKhoa?.mo_san_phu_khoa_ghi_ro : "")}")
                .Replace("{{MoSanPhuKhoa_Khong}}", ExportKSKHelpers.CheckBoxHtmlBuilder(item.benh_nhan?.gioi_tinh == GioiTinh.Nu && sanPhuKhoa?.so_lan_mo_san_phu_khoa == (int)YesNo.Khong))
                .Replace("{{ApDungBienPhapPhongTranh_Co}}", ExportKSKHelpers.CheckBoxHtmlBuilder(item.benh_nhan?.gioi_tinh == GioiTinh.Nu && sanPhuKhoa?.ap_dung_bptt == true))
                .Replace("{{ApDungBienPhapPhongTranh_GhiRo}}", $"{(item.benh_nhan?.gioi_tinh == GioiTinh.Nu ? sanPhuKhoa?.bptt_ghi_ro : "")}")
                .Replace("{{ApDungBienPhapPhongTranh_Khong}}", ExportKSKHelpers.CheckBoxHtmlBuilder(item.benh_nhan?.gioi_tinh == GioiTinh.Nu && sanPhuKhoa?.ap_dung_bptt == false))
                .Replace("{{ChieuCao}}", $"{theLuc?.chieu_cao}")
                .Replace("{{CanNang}}", $"{theLuc?.can_nang}")
                .Replace("{{BMI}}", $"{theLuc?.bmi}")
                .Replace("{{Mach}}", $"{theLuc?.mach}")
                .Replace("{{HuyetAp}}", !string.IsNullOrEmpty(theLuc?.huyet_ap) ? $"<span class=\"dotted-ruled\" style=\"width: 30mm\">{theLuc?.huyet_ap}</span>" : "<span class=\"dotted-ruled\" style=\"width: 15mm\"></span><span>/</span><span class=\"dotted-ruled\" style=\"width: 15mm\"></span>")
                .Replace("{{PhanLoaiTheLuc}}", $"{theLuc?.phan_loai?.name}")
                .Replace("{{KetQuaCLS_TuanHoan}}", $"{cls?.kq_nk_tuan_hoan}")
                .Replace("{{KetQuaCLS_TuanHoan_ChuKy}}", ExportKSKHelpers.RenderSignature(cls?.chu_ky_tuan_hoan ?? string.Empty, "", 100, 50) + $"<br/>{cls?.bs_tuan_hoan}")
                .Replace("{{KetQuaCLS_TuanHoan_PhanLoai}}", $"{cls?.pl_nk_tuan_hoan?.name}")
                .Replace("{{KetQuaCLS_HoHap}}", $"{cls?.kq_nk_ho_hap}")
                .Replace("{{KetQuaCLS_HoHap_ChuKy}}", ExportKSKHelpers.RenderSignature(cls?.chu_ky_ho_hap ?? string.Empty, "", 100, 50) + $"<br/>{cls?.bs_ho_hap}")
                .Replace("{{KetQuaCLS_HoHap_PhanLoai}}", $"{cls?.pl_nk_ho_hap?.name}")
                .Replace("{{KetQuaCLS_TieuHoa}}", $"{cls?.kq_nk_tieu_hoa}")
                .Replace("{{KetQuaCLS_TieuHoa_ChuKy}}", ExportKSKHelpers.RenderSignature(cls?.chu_ky_tieu_hoa ?? string.Empty, "", 100, 50) + $"<br/>{cls?.bs_tieu_hoa}")
                .Replace("{{KetQuaCLS_TieuHoa_PhanLoai}}", $"{cls?.pl_nk_tieu_hoa?.name}")
                .Replace("{{KetQuaCLS_ThanTietNieu}}", $"{cls?.kq_nk_than_tiet_nieu}")
                .Replace("{{KetQuaCLS_ThanTietNieu_ChuKy}}", ExportKSKHelpers.RenderSignature(cls?.chu_ky_than_tiet_nieu ?? string.Empty, "", 100, 50) + $"<br/>{cls?.bs_than_tiet_nieu}")
                .Replace("{{KetQuaCLS_ThanTietNieu_PhanLoai}}", $"{cls?.pl_nk_than_tiet_nieu?.name}")
                .Replace("{{KetQuaCLS_NoiTiet}}", $"{cls?.kq_nk_noi_tiet}")
                .Replace("{{KetQuaCLS_NoiTiet_ChuKy}}", ExportKSKHelpers.RenderSignature(cls?.chu_ky_noi_tiet ?? string.Empty, "", 100, 50) + $"<br/>{cls?.bs_noi_tiet}")
                .Replace("{{KetQuaCLS_NoiTiet_PhanLoai}}", $"{cls?.pl_nk_noi_tiet?.name}")
                .Replace("{{KetQuaCLS_CoXuongKhops}}", $"{cls?.kq_nk_co_xuong_khop}")
                .Replace("{{KetQuaCLS_CoXuongKhops_ChuKy}}", ExportKSKHelpers.RenderSignature(cls?.chu_ky_co_xuong_khop ?? string.Empty, "", 100, 50) + $"<br/>{cls?.bs_co_xuong_khop}")
                .Replace("{{KetQuaCLS_CoXuongKhops_PhanLoai}}", $"{cls?.pl_nk_co_xuong_khop?.name}")
                .Replace("{{KetQuaCLS_ThanKinh}}", $"{cls?.kq_nk_than_kinh}")
                .Replace("{{KetQuaCLS_ThanKinh_ChuKy}}", ExportKSKHelpers.RenderSignature(cls?.chu_ky_than_kinh ?? string.Empty, "", 100, 50) + $"<br/>{cls?.bs_than_kinh}")
                .Replace("{{KetQuaCLS_ThanKinh_PhanLoai}}", $"{cls?.pl_nk_than_kinh?.name}")
                .Replace("{{KetQuaCLS_TamThan}}", $"{cls?.kq_nk_tam_than}")
                .Replace("{{KetQuaCLS_TamThan_ChuKy}}", ExportKSKHelpers.RenderSignature(cls?.chu_ky_tam_than ?? string.Empty, "", 100, 50) + $"<br/>{cls?.bs_tam_than}")
                .Replace("{{KetQuaCLS_TamThan_PhanLoai}}", $"{cls?.pl_nk_tam_than?.name}")
                .Replace("{{KetQuaCLS_NgoaiKhoa}}", $"{cls?.kq_ngoai_khoa}")
                .Replace("{{KetQuaCLS_NgoaiKhoa_PhanLoai}}", $"{cls?.pl_ngoai_khoa?.name}")
                .Replace("{{KetQuaCLS_DaLieu}}", $"{cls?.kq_da_lieu}")
                .Replace("{{KetQuaCLS_DaLieu_PhanLoai}}", $"{cls?.pl_da_lieu?.name}")
                .Replace("{{KetQuaCLS_NgoaiKhoa_ChuKy}}", ExportKSKHelpers.RenderSignature(cls?.chu_ky_ngoai_khoa ?? string.Empty, "", 100, 50) + $"<br/>{cls?.bs_ngoai_khoa}")
                .Replace("{{KetQuaCLS_SanPhuKhoa}}", $"{(!string.IsNullOrEmpty(sanPhuKhoa?.ket_qua) ? sanPhuKhoa?.ket_qua : "Chi tiết nội dung khám theo danh mục tại phụ lục XXV ban hành kèm theo Thông tư này.")}")
                .Replace("{{KetQuaCLS_SanPhuKhoa_PhanLoai}}", $"{sanPhuKhoa?.phan_loai?.name}")
                .Replace("{{KetQuaCLS_SanPhuKhoa_ChuKy}}", ExportKSKHelpers.RenderSignature(sanPhuKhoa?.chu_ky ?? string.Empty, "", 100, 50) + $"<br/>{sanPhuKhoa?.nguoi_ket_luan}")
                .Replace("{{KetQuaCLS_Mat_KhongKinh_Phai}}", $"{cls?.thi_luc_khong_kinh_phai}")
                .Replace("{{KetQuaCLS_Mat_KhongKinh_Trai}}", $"{cls?.thi_luc_khong_kinh_trai}")
                .Replace("{{KetQuaCLS_Mat_CoKinh_Phai}}", $"{cls?.thi_luc_co_kinh_phai}")
                .Replace("{{KetQuaCLS_Mat_CoKinh_Trai}}", $"{cls?.thi_luc_co_kinh_trai}")
                .Replace("{{KetQuaCLS_Mat_ChuKy}}", ExportKSKHelpers.RenderSignature(cls?.chu_ky_mat ?? string.Empty, "", 100, 50) + $"<br/>{cls?.bs_mat}")
                .Replace("{{KetQuaCLS_Mat_Benh}}", $"{cls?.benh_mat}")
                .Replace("{{KetQuaCLS_Mat_PhanLoai}}", $"{cls?.pl_mat?.name}")
                .Replace("{{KetQuaCLS_TaiMuiHong_TaiTrai_NoiThuong}}", $"{cls?.tmh_nt_trai}")
                .Replace("{{KetQuaCLS_TaiMuiHong_TaiTrai_NoiTham}}", $"{cls?.tmh_ntham_trai}")
                .Replace("{{KetQuaCLS_TaiMuiHong_TaiPhai_NoiThuong}}", $"{cls?.tmh_nt_phai}")
                .Replace("{{KetQuaCLS_TaiMuiHong_TaiPhai_NoiTham}}", $"{cls?.tmh_ntham_phai}")
                .Replace("{{KetQuaCLS_TaiMuiHong_ChuKy}}", ExportKSKHelpers.RenderSignature(cls?.chu_ky_tmh ?? string.Empty, "", 100, 50) + $"<br/>{cls?.bs_tmh}")
                .Replace("{{KetQuaCLS_TaiMuiHong_Benh}}", $"{cls?.benh_tai_mui_hong}")
                .Replace("{{KetQuaCLS_TaiMuiHong_PhanLoai}}", $"{cls?.pl_tmh?.name}")
                .Replace("{{KetQuaCLS_RangHamMat_HamTren}}", $"{cls?.kq_rhm_ham_tren}")
                .Replace("{{KetQuaCLS_RangHamMat_HamDuoi}}", $"{cls?.kq_rhm_ham_duoi}")
                .Replace("{{KetQuaCLS_RangHamMat_ChuKy}}", ExportKSKHelpers.RenderSignature(cls?.chu_ky_rhm ?? string.Empty, "", 100, 50) + $"<br/>{cls?.bs_rhm}")
                .Replace("{{KetQuaCLS_RangHamMat_Benh}}", $"{cls?.benh_rhm}")
                .Replace("{{KetQuaCLS_RangHamMat_PhanLoai}}", $"{cls?.pl_rhm?.name}")
                .Replace("{{KetQuaCLS}}", ExportKSKHelpers.RenderKQCLS(kqcls?.Where(c => !string.IsNullOrEmpty(c.ket_qua)).ToList(), ExportKSKHelpers.RenderSignature(ketLuan?.bs_ket_luan?.chu_ky_bac_si ?? ketLuan?.chu_ky ?? string.Empty, "", 100, 50) + $"<br/>{fullNameBSKL}"))
                .Replace("{{KetQuaCLS_CLS_PhanLoaiSucKhoe}}", $"{ketLuan?.phan_loai_suc_khoe?.name}")
                .Replace("{{KetLuan}}", ExportKSKHelpers.MultilineSpanHtmlBuilder(ketLuan?.benh_tat_ket_luan ?? string.Empty))
                .Replace("{{NgayKetLuan_Ngay}}", $"{ketLuan?.ngay_ket_luan:dd}")
                .Replace("{{NgayKetLuan_Thang}}", $"{ketLuan?.ngay_ket_luan:MM}")
                .Replace("{{NgayKetLuan_Nam}}", $"{ketLuan?.ngay_ket_luan:yyyy}")
                .Replace("{{NguoiKetLuan}}", ExportKSKHelpers.RenderSignature(ketLuan?.bs_ket_luan?.chu_ky_bac_si ?? ketLuan?.chu_ky ?? string.Empty, "<div style=\"height: 20mm\"></div>", 100, 50) + $"<br/><span class=\"bold\">{fullNameBSKL}</span>");

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

            return pdfBytes;
        }

        private static async Task<byte[]?> DownloadPdfBytesAsync(
            KetQuaCanLamSangFileModel fileModel,
            string baseUrl,
            HttpClient http,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(fileModel.url_file))
                return null;

            var fileUrl = fileModel.url_file.Trim();

            // Trường hợp data URI
            if (fileUrl.StartsWith("data:application/pdf;base64", StringComparison.OrdinalIgnoreCase))
            {
                var idx = fileUrl.IndexOf("base64,", StringComparison.OrdinalIgnoreCase);
                if (idx < 0) return null;

                var b64 = fileUrl[(idx + 7)..];
                try
                {
                    return Convert.FromBase64String(b64);
                }
                catch
                {
                    return null;
                }
            }

            // Trường hợp URL
            string fullUrl = fileUrl;
            if (!Uri.IsWellFormedUriString(fullUrl, UriKind.Absolute))
            {
                try
                {
                    var baseUri = new Uri(baseUrl.EndsWith("/") ? baseUrl : baseUrl + "/");
                    fullUrl = new Uri(baseUri, fileUrl.TrimStart('/')).ToString();
                }
                catch
                {
                    // nếu build baseUrl fail thì vẫn dùng nguyên fileUrl
                    fullUrl = fileUrl;
                }
            }

            try
            {
                return await http.GetByteArrayAsync(fullUrl, ct);
            }
            catch
            {
                // fallback thử lại với url gốc nếu khác
                if (!fullUrl.Equals(fileUrl, StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        return await http.GetByteArrayAsync(fileUrl, ct);
                    }
                    catch
                    {
                        return null;
                    }
                }

                return null;
            }
        }

        private static async Task<byte[]?> DownloadPdfBytesWithLimitAsync(
            KetQuaCanLamSangFileModel fileModel,
            string baseUrl,
            HttpClient http,
            SemaphoreSlim limiter,
            CancellationToken ct = default)
        {
            await limiter.WaitAsync(ct);
            try
            {
                return await DownloadPdfBytesAsync(fileModel, baseUrl, http, ct);
            }
            finally
            {
                limiter.Release();
            }
        }

        public async Task<byte[]> MergeWithRelatedPdfsAsync(
            byte[] pdfBytes,
            PrepareDataModel prepareData,
            SoKhamSucKhoeModel item,
            string baseUrl,
            HttpClient httpClient,
            CancellationToken ct = default)
        {
            // Tìm các file liên quan
            List<KetQuaCanLamSangFileModel> matchingFiles = new();
            if (!string.IsNullOrEmpty(item.ma_luot_kham))
            {
                matchingFiles = prepareData.KetQuaCLSFiles?
                    .Where(f => !string.IsNullOrEmpty(f.url_file) &&
                                f.url_file!.Contains(item.ma_luot_kham!, StringComparison.OrdinalIgnoreCase))
                    .ToList()
                    ?? new List<KetQuaCanLamSangFileModel>();
            }

            if (matchingFiles.Count == 0)
                return pdfBytes; // không có file kèm thì trả về nguyên bản

            try
            {
                var cpuCount = Environment.ProcessorCount;
                var maxParallel = Math.Clamp(Math.Min(cpuCount * 2, 16), 4, 16);
                var limiter = new SemaphoreSlim(maxParallel);
                var downloadTasks = matchingFiles.Select(f => DownloadPdfBytesWithLimitAsync(f, baseUrl, httpClient, limiter, ct));
                var downloaded = await Task.WhenAll(downloadTasks);

                // Lọc các file hợp lệ và là PDF
                var pdfExtraBytes = downloaded
                    .Where(b => b != null && b.Length > 4 && b[0] == '%' && b[1] == 'P' && b[2] == 'D' && b[3] == 'F')
                    .Cast<byte[]>()
                    .ToList();

                if (pdfExtraBytes.Count == 0)
                    return pdfBytes;

                // 2. Merge: main PDF + các PDF tải được
                using var mergedDoc = new PdfSharp.Pdf.PdfDocument();

                // main
                using (var msMain = new MemoryStream(pdfBytes, writable: false))
                {
                    var mainDoc = PdfSharp.Pdf.IO.PdfReader.Open(msMain, PdfSharp.Pdf.IO.PdfDocumentOpenMode.Import);
                    for (int p = 0; p < mainDoc.PageCount; p++)
                    {
                        mergedDoc.AddPage(mainDoc.Pages[p]);
                    }
                }

                // extra (duyệt tuần tự để tránh issues thread-unsafe của PdfSharp)
                foreach (var extraBytes in pdfExtraBytes)
                {
                    using var msExtra = new MemoryStream(extraBytes, writable: false);
                    try
                    {
                        var extraDoc = PdfSharp.Pdf.IO.PdfReader.Open(msExtra, PdfSharp.Pdf.IO.PdfDocumentOpenMode.Import);
                        for (int p = 0; p < extraDoc.PageCount; p++)
                        {
                            mergedDoc.AddPage(extraDoc.Pages[p]);
                        }
                    }
                    catch
                    {
                        // skip file lỗi
                        continue;
                    }
                }

                using var outMs = new MemoryStream();
                mergedDoc.Save(outMs);
                return outMs.ToArray();
            }
            catch (Exception ex)
            {
                // log và fallback về pdf gốc
                Console.WriteLine($"Warning: failed to merge related PDFs for {item.ma_luot_kham}: {ex.Message}");
                return pdfBytes;
            }
        }
    }
}
