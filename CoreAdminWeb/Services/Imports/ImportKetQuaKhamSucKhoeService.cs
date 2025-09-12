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
        public ImportKetQuaKhamSucKhoeService(IServiceScopeFactory serviceScopeFactory)
        {
            using (var scope = serviceScopeFactory.CreateScope())
            {
                _soKhamSucKhoeService = scope.ServiceProvider.GetRequiredService<IBaseDetailService<SoKhamSucKhoeModel>>();
                _khamSucKhoeCongTyService = scope.ServiceProvider.GetRequiredService<IBaseService<KhamSucKhoeCongTyModel>>();
                _phanLoaiSucKhoeService = scope.ServiceProvider.GetRequiredService<IBaseService<PhanLoaiSucKhoeModel>>();
                _khamSucKhoeKetLuanService = scope.ServiceProvider.GetRequiredService<IBaseDetailService<KhamSucKhoeKetLuanModel>>();
            }
        }


        public async Task ImportFromExcelWithProgressAsync(byte[] fileBytes,
                                                           string connectionId,
                                                           string currentUserId,
                                                           Func<ProcessingModel, Task> updateProgress,
                                                           CancellationToken cancellationToken)
        {
            try
            {
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
                    for (int row = 3; row <= rowCount; row++)
                    {
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
                            NgayKetLuan = colCount > 30 ? worksheet.Cells[row, 31].Text : string.Empty
                        };

                        var validate = ValidateImportData(model);
                        if (!string.IsNullOrEmpty(validate))
                        {
                            errorBuilder.Append($"\nDòng {row}: Các trường {validate} bị rỗng hoặc không đúng định dạng");
                        }
                        result.Add(model);

                        int nextPercent = (int)Math.Round((double)(row - 2) * 100 / (rowCount - 2));
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
                    phanLoaiSKs, batchSize
                );
                var klTask = BatchQueryAsync(
                    ids => _khamSucKhoeKetLuanService.GetAllAsync($"filter[_and][][ma_luot_kham][_in]={string.Join(",", ids)}"),
                    maLuotKhams, batchSize
                );
                var skskTask = BatchQueryAsync(
                    ids => _soKhamSucKhoeService.GetAllAsync($"filter[_and][][ma_luot_kham][_in]={string.Join(",", ids)}"),
                    maLuotKhams, batchSize
                );

                await Task.WhenAll(plSucKhoeTask, klTask, skskTask);

                var phanLoaiSucKhoes = plSucKhoeTask.Result;
                var existingKLs = klTask.Result;
                var soKhamSucKhoes = skskTask.Result;

                var existingUserMap = existingKLs
                    .DistinctBy(c => c.ma_luot_kham ?? c.luot_kham?.ma_luot_kham ?? string.Empty)
                    .ToDictionary(c => c.ma_luot_kham ?? c.luot_kham?.ma_luot_kham ?? string.Empty, c => c);

                var notExistingSKSKs = soKhamSucKhoes.Where(c => !maLuotKhams.Any(x => x == c.ma_luot_kham));
                if (notExistingSKSKs.Any())
                {
                    await updateProgress.Invoke(new ProcessingModel()
                    {
                        ProcessId = connectionId,
                        Status = TrangThaiXuLyNen.Error,
                        Value = $"Không tồn tại sổ khám sức khỏe: {string.Join(", ", notExistingSKSKs.Select(c => $"'{c.ma_luot_kham}'"))}",
                        AdditionalParams = new { ShowPopup = true }
                    });
                    return;
                }

                var dotKhamIds = soKhamSucKhoes.Select(c => c.MaDotKham).Where(x => x != null && x.id > 0).Select(x => (x?.id ?? 0).ToString()).Distinct().ToList();
                var dotKhamTask = BatchQueryAsync(
                    ids => _khamSucKhoeCongTyService.GetAllAsync($"filter[_and][][id][_in]={string.Join(",", ids)}"),
                    dotKhamIds, batchSize
                );
                var dotKhams = await dotKhamTask;

                var updatingKLs = new List<KhamSucKhoeKetLuanModel>();
                var newKLs = new List<KhamSucKhoeKetLuanModel>();

                percent = 0;
                int totalRow = result.Count;
                int rowIndex = 0;
                foreach (var item in result)
                {
                    DateTime? ngayKetLuan = null;
                    var selectedSoKham = soKhamSucKhoes.FirstOrDefault(c => c.ma_luot_kham == item.MaLuotKham);
                    var selectedDotKham = dotKhams.FirstOrDefault(c => c.id == (selectedSoKham?.MaDotKham?.id ?? 0));
                    if (!string.IsNullOrEmpty(item.NgayKetLuan) && DateTime.TryParseExact(item.NgayKetLuan, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                    {
                        ngayKetLuan = parsedDate;
                    }

                    if (existingUserMap.TryGetValue(item.MaLuotKham, out var existingUser))
                    {
                        existingUser.de_nghi = item.DeNghi;
                        existingUser.benh_tat_ket_luan = item.CacLoaiBenhTat;
                        existingUser.phan_loai_suc_khoe = phanLoaiSucKhoes.FirstOrDefault(c => c.name == item.PhanLoaiSucKhoe);
                        existingUser.ngay_ket_luan = ngayKetLuan;
                        existingUser.nguoi_ket_luan = $"{selectedDotKham?.bs_ket_luan?.first_name} {selectedDotKham?.bs_ket_luan?.last_name}";

                        updatingKLs.Add(existingUser);
                    }
                    else if (!newKLs.Any(c => c.ma_luot_kham == item.MaLuotKham))
                    {
                        newKLs.Add(new KhamSucKhoeKetLuanModel
                        {
                            benh_tat_ket_luan = item.CacLoaiBenhTat,
                            de_nghi = item.DeNghi,
                            ma_luot_kham = item.MaLuotKham,
                            phan_loai_suc_khoe = phanLoaiSucKhoes.FirstOrDefault(c => c.name == item.PhanLoaiSucKhoe),
                            status = Status.draft,
                            isAbnormal = false,
                            active = true,
                            date_created = DateTime.Now,
                            date_updated = DateTime.Now,
                            deleted = false,
                            luot_kham = selectedSoKham,
                            ngay_ket_luan = ngayKetLuan,
                            nguoi_ket_luan = $"{selectedDotKham?.bs_ket_luan?.first_name} {selectedDotKham?.bs_ket_luan?.last_name}"
                        });
                    }

                    int nextPercent = (int)Math.Round((double)(rowIndex - 1) * 100 / (totalRow - 1));
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
                await BatchExecuteAsync(updatingKLs, _khamSucKhoeKetLuanService.UpdateAsync, batchSize);
                await BatchExecuteAsync(newKLs, _khamSucKhoeKetLuanService.CreateAsync, batchSize);

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

        static async Task<List<T>> BatchQueryAsync<T>(Func<List<string>, Task<RequestHttpResponse<List<T>>>> queryFunc, List<string> ids, int batchSize = 200)
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

        static async Task BatchExecuteAsync<T>(List<T> items, Func<List<T>, Task<RequestHttpResponse<List<T>>>> execFunc, int batchSize = 100)
        {
            foreach (var batch in items.Chunk(batchSize))
            {
                await execFunc(batch.ToList());
            }
        }

        static async Task BatchExecuteAsync<T>(List<T> items, Func<List<T>, Task<RequestHttpResponse<bool>>> execFunc, int batchSize = 100)
        {
            foreach (var batch in items.Chunk(batchSize))
            {
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
