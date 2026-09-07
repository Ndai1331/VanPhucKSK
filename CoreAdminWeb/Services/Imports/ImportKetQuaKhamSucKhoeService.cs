using CoreAdminWeb.Enums;
using CoreAdminWeb.Model;
using CoreAdminWeb.Model.Base;
using CoreAdminWeb.Model.DanhSachDoan;
using CoreAdminWeb.Model.KhamSucKhoes;
using CoreAdminWeb.Model.RequestHttps;
using CoreAdminWeb.Services.BaseServices;
using OfficeOpenXml;
using System.Globalization;
using System.Text;

namespace CoreAdminWeb.Services.Imports
{
    public class ImportKetQuaKhamSucKhoeService
    {
        private readonly IBaseDetailService<SoKhamSucKhoeModel> _soKhamSucKhoeService;
        private readonly IBaseService<KhamSucKhoeCongTyModel> _khamSucKhoeCongTyService;
        private readonly IBaseService<PhanLoaiSucKhoeModel> _phanLoaiSucKhoeService;
        private readonly IBaseDetailService<KhamSucKhoeKetLuanModel> _khamSucKhoeKetLuanService;
        public ImportKetQuaKhamSucKhoeService(
            IBaseDetailService<SoKhamSucKhoeModel> soKhamSucKhoeService,
            IBaseService<KhamSucKhoeCongTyModel> khamSucKhoeCongTyService,
            IBaseService<PhanLoaiSucKhoeModel> phanLoaiSucKhoeService,
            IBaseDetailService<KhamSucKhoeKetLuanModel> khamSucKhoeKetLuanService)
        {
            _soKhamSucKhoeService = soKhamSucKhoeService;
            _khamSucKhoeCongTyService = khamSucKhoeCongTyService;
            _phanLoaiSucKhoeService = phanLoaiSucKhoeService;
            _khamSucKhoeKetLuanService = khamSucKhoeKetLuanService;
        }


        public async Task ImportFromExcelWithProgressAsync(byte[] fileBytes,
                                                           string connectionId,
                                                           string currentUserId,
                                                           Func<ProcessingModel, Task> updateProgress,
                                                           CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                ExcelPackage.License.SetNonCommercialOrganization("NonCommercial");

                List<ImportKetQuaKhamSucKhoeModel> result;
                StringBuilder errorBuilder = new StringBuilder();
                int percent = 0;
                int rowCount, colCount;

                // Đọc dữ liệu Excel một lần, tránh tạo object không cần thiết
                using (var ms = new MemoryStream(fileBytes))
                using (var package = new ExcelPackage(ms))
                {
                    var worksheet = package.Workbook.Worksheets[0];
                    rowCount = worksheet.Dimension.Rows;
                    colCount = worksheet.Dimension.Columns;

                    result = new List<ImportKetQuaKhamSucKhoeModel>();

                    // Đọc dữ liệu theo batch để giảm memory pressure
                    for (int row = 2; row <= rowCount; row++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        bool isEmptyRow = true;
                        for (int col = 1; col <= colCount; col++)
                        {
                            if (!string.IsNullOrWhiteSpace(worksheet.Cells[row, col].Text))
                            {
                                isEmptyRow = false;
                                break;
                            }
                        }
                        if (isEmptyRow)
                        {
                            continue;
                        }

                        var model = new ImportKetQuaKhamSucKhoeModel
                        {
                            MaLuotKham = worksheet.Cells[row, 2].Text,
                            PhanLoaiSucKhoe = colCount > 27 ? worksheet.Cells[row, 28].Text : string.Empty,
                            CacLoaiBenhTat = colCount > 28 ? worksheet.Cells[row, 29].Text : string.Empty,
                            DeNghi = colCount > 29 ? worksheet.Cells[row, 30].Text : string.Empty,
                            TuVan = colCount > 30 ? worksheet.Cells[row, 31].Text : string.Empty,
                            NgayKetLuan = colCount > 31 ? worksheet.Cells[row, 32].Text : string.Empty
                        };

                        var validate = ValidateImportData(model);
                        if (!string.IsNullOrEmpty(validate))
                        {
                            errorBuilder.Append($"\nDòng {row}: Các trường {validate} bị rỗng hoặc không đúng định dạng");
                        }
                        result.Add(model);

                        int nextPercent = (int)Math.Round((double)(row - 1) * 100 / Math.Max(1, rowCount - 1));
                        if (nextPercent != percent)
                        {
                            percent = nextPercent;
                            await updateProgress.Invoke(new ProcessingModel()
                            {
                                ProcessId = connectionId,
                                Status = TrangThaiXuLyNen.Processing,
                                Value = $"Đang đọc dữ liệu import {percent}%"
                            });
                        }
                    }
                }

                if (result.Count == 0)
                {
                    await updateProgress.Invoke(new ProcessingModel()
                    {
                        ProcessId = connectionId,
                        Status = TrangThaiXuLyNen.Error,
                        Value = "Không có dữ liệu để import!"
                    });
                    return;
                }

                if (errorBuilder.Length > 0)
                {
                    await updateProgress.Invoke(new ProcessingModel()
                    {
                        ProcessId = connectionId,
                        Status = TrangThaiXuLyNen.Error,
                        Value = $"Dữ liệu import không hợp lệ:{errorBuilder}"
                    });
                    return;
                }

                // Tối ưu batch size khi truy vấn và ghi dữ liệu
                int batchSize = result.Count switch
                {
                    >= 10000 => 1000,
                    >= 5000 => 500,
                    _ => 200
                };

                await updateProgress.Invoke(new ProcessingModel()
                {
                    ProcessId = connectionId,
                    Status = TrangThaiXuLyNen.Processing,
                    Value = $"Chuẩn bị dữ liệu cập nhật..."
                });

                var maLuotKhams = result.Select(c => c.MaLuotKham).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList();
                var phanLoaiSKs = result.Select(c => c.PhanLoaiSucKhoe).Where(x => !string.IsNullOrWhiteSpace(x)).Select(c => c.Trim()).Distinct().ToList();

                // Chạy các truy vấn batch song song
                var plSucKhoeTask = BatchQueryAsync(
                    ids => _phanLoaiSucKhoeService.GetAllAsync($"filter[_and][][name][_in]={string.Join(",", ids)}"),
                    phanLoaiSKs, batchSize, cancellationToken
                );
                var klTask = BatchQueryAsync(
                    ids => _khamSucKhoeKetLuanService.GetAllAsync($"filter[_and][][ma_luot_kham][_in]={string.Join(",", ids)}"),
                    maLuotKhams, batchSize, cancellationToken
                );
                var skskTask = BatchQueryAsync(
                    ids => _soKhamSucKhoeService.GetAllAsync($"filter[_and][][ma_luot_kham][_in]={string.Join(",", ids)}"),
                    maLuotKhams, batchSize, cancellationToken
                );

                await Task.WhenAll(plSucKhoeTask, klTask, skskTask);

                var phanLoaiSucKhoes = plSucKhoeTask.Result;
                var existingKLs = klTask.Result;
                var soKhamSucKhoes = skskTask.Result;

                var existingUserMap = existingKLs
                    .DistinctBy(c => c.ma_luot_kham ?? c.luot_kham?.ma_luot_kham ?? string.Empty)
                    .ToDictionary(c => c.ma_luot_kham ?? c.luot_kham?.ma_luot_kham ?? string.Empty, c => c, StringComparer.OrdinalIgnoreCase);

                var existingVisitKeys = soKhamSucKhoes
                    .Where(item => !string.IsNullOrWhiteSpace(item.ma_luot_kham))
                    .Select(item => item.ma_luot_kham!.Trim())
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
                var missingVisitIds = maLuotKhams
                    .Where(id => !existingVisitKeys.Contains(id.Trim()))
                    .ToList();
                if (missingVisitIds.Count > 0)
                {
                    await updateProgress.Invoke(new ProcessingModel()
                    {
                        ProcessId = connectionId,
                        Status = TrangThaiXuLyNen.Error,
                        Value = $"Không tồn tại sổ khám sức khỏe: {string.Join(", ", missingVisitIds.Select(id => $"'{id}'"))}",
                        AdditionalParams = new { ShowPopup = true }
                    });
                    return;
                }

                var dotKhamIds = soKhamSucKhoes.Select(c => c.MaDotKham).Where(x => x != null && x.id > 0).Select(x => (x?.id ?? 0).ToString()).Distinct().ToList();
                var dotKhamTask = BatchQueryAsync(
                    ids => _khamSucKhoeCongTyService.GetAllAsync($"filter[_and][][id][_in]={string.Join(",", ids)}"),
                    dotKhamIds, batchSize, cancellationToken
                );
                var dotKhams = await dotKhamTask;
                var soKhamByVisitId = soKhamSucKhoes
                    .Where(item => !string.IsNullOrWhiteSpace(item.ma_luot_kham))
                    .GroupBy(item => item.ma_luot_kham!.Trim(), StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
                var dotKhamById = dotKhams
                    .GroupBy(item => item.id)
                    .ToDictionary(group => group.Key, group => group.First());
                var phanLoaiByName = phanLoaiSucKhoes
                    .Where(item => !string.IsNullOrWhiteSpace(item.name))
                    .GroupBy(item => item.name!.Trim(), StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

                var updatingKLs = new List<KhamSucKhoeKetLuanModel>();
                var newKLs = new List<KhamSucKhoeKetLuanModel>();
                var newVisitKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                percent = 0;
                int totalRow = result.Count;
                int rowIndex = 0;
                foreach (var item in result)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    DateTime? ngayKetLuan = null;
                    soKhamByVisitId.TryGetValue(item.MaLuotKham.Trim(), out var selectedSoKham);
                    dotKhamById.TryGetValue(selectedSoKham?.MaDotKham?.id ?? 0, out var selectedDotKham);
                    phanLoaiByName.TryGetValue(item.PhanLoaiSucKhoe.Trim(), out var selectedPhanLoai);
                    if (!string.IsNullOrEmpty(item.NgayKetLuan) && DateTime.TryParseExact(item.NgayKetLuan, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                    {
                        ngayKetLuan = parsedDate;
                    }

                    if (existingUserMap.TryGetValue(item.MaLuotKham, out var existingUser))
                    {
                        existingUser.de_nghi = item.DeNghi;
                        existingUser.tu_van = item.TuVan;
                        existingUser.benh_tat_ket_luan = item.CacLoaiBenhTat;
                        existingUser.phan_loai_suc_khoe = selectedPhanLoai;
                        existingUser.ngay_ket_luan = ngayKetLuan;
                        existingUser.nguoi_ket_luan = $"{selectedDotKham?.bs_ket_luan?.chuc_danh} {selectedDotKham?.bs_ket_luan?.full_name}";

                        updatingKLs.Add(existingUser);
                    }
                    else if (newVisitKeys.Add(item.MaLuotKham.Trim()))
                    {
                        newKLs.Add(new KhamSucKhoeKetLuanModel
                        {
                            benh_tat_ket_luan = item.CacLoaiBenhTat,
                            de_nghi = item.DeNghi,
                            tu_van = item.TuVan,
                            ma_luot_kham = item.MaLuotKham,
                            phan_loai_suc_khoe = selectedPhanLoai,
                            status = Status.draft,
                            isAbnormal = false,
                            active = true,
                            date_created = DateTime.Now,
                            date_updated = DateTime.Now,
                            deleted = false,
                            luot_kham = selectedSoKham,
                            ngay_ket_luan = ngayKetLuan,
                            nguoi_ket_luan = $"{selectedDotKham?.bs_ket_luan?.chuc_danh} {selectedDotKham?.bs_ket_luan?.full_name}"
                        });
                    }

                    int nextPercent = (int)Math.Round((double)(rowIndex + 1) * 100 / Math.Max(1, totalRow));
                    if (percent != nextPercent)
                    {
                        percent = nextPercent;

                        await updateProgress.Invoke(new ProcessingModel()
                        {
                            ProcessId = connectionId,
                            Status = TrangThaiXuLyNen.Processing,
                            Value = $"Đang xử lý kết quả khám {percent}%"
                        });
                    }
                    rowIndex++;
                }

                await updateProgress.Invoke(new ProcessingModel()
                {
                    ProcessId = connectionId,
                    Status = TrangThaiXuLyNen.Processing,
                    Value = $"Đang cập nhật kết quả khám..."
                });

                // Batch update/create users
                await BatchExecuteAsync(updatingKLs.Cast<dynamic>().ToList(), _khamSucKhoeKetLuanService.UpdateAsync, batchSize, cancellationToken);
                await BatchExecuteAsync(newKLs.Cast<dynamic>().ToList(), _khamSucKhoeKetLuanService.CreateAsync, batchSize, cancellationToken);

                await updateProgress.Invoke(new ProcessingModel()
                {
                    ProcessId = connectionId,
                    Status = TrangThaiXuLyNen.Completed,
                    Value = $"Hoàn tất cập nhật kết quả khám!"
                });
            }
            catch (Exception ex)
            {
                await updateProgress.Invoke(new ProcessingModel()
                {
                    ProcessId = connectionId,
                    Status = TrangThaiXuLyNen.Error,
                    Value = $"Lỗi khi import: {ex.Message}"
                });
            }
        }

        static async Task<List<T>> BatchQueryAsync<T>(Func<List<string>, Task<RequestHttpResponse<List<T>>>> queryFunc, List<string> ids, int batchSize = 200, CancellationToken cancellationToken = default)
        {
            var results = new List<T>();
            foreach (var batch in ids.Chunk(batchSize))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var res = await queryFunc(batch.ToList());
                if (res.IsSuccess && res.Data != null)
                {
                    results.AddRange(res.Data);
                }
            }
            return results;
        }

        static async Task BatchExecuteAsync<T, TValue>(List<T> items, Func<List<T>, Task<RequestHttpResponse<List<TValue>>>> execFunc, int batchSize = 100, CancellationToken cancellationToken = default)
        {
            foreach (var batch in items.Chunk(batchSize))
            {
                cancellationToken.ThrowIfCancellationRequested();
                await execFunc(batch.ToList());
            }
        }

        static async Task BatchExecuteAsync<T>(List<T> items, Func<List<T>, Task<RequestHttpResponse<List<T>>>> execFunc, int batchSize = 100, CancellationToken cancellationToken = default)
        {
            foreach (var batch in items.Chunk(batchSize))
            {
                cancellationToken.ThrowIfCancellationRequested();
                await execFunc(batch.ToList());
            }
        }

        static async Task BatchExecuteAsync<T>(List<T> items, Func<List<T>, Task<RequestHttpResponse<bool>>> execFunc, int batchSize = 100, CancellationToken cancellationToken = default)
        {
            foreach (var batch in items.Chunk(batchSize))
            {
                cancellationToken.ThrowIfCancellationRequested();
                await execFunc(batch.ToList());
            }
        }

        static string ValidateImportData(ImportKetQuaKhamSucKhoeModel import)
        {
            StringBuilder builder = new StringBuilder();
            if (string.IsNullOrEmpty(import.MaLuotKham))
            {
                if (builder.Length > 0)
                {
                    builder.Append(", ");
                }

                builder.Append("Mã lượt khám");
            }

            if (!string.IsNullOrEmpty(import.NgayKetLuan) && !DateTime.TryParseExact(import.NgayKetLuan, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            {
                if (builder.Length > 0)
                {
                    builder.Append(", ");
                }

                builder.Append("Ngày kết luận");
            }

            return builder.ToString();
        }
    }
}
