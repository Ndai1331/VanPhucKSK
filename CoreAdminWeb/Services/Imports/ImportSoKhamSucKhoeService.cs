using CoreAdminWeb.Commons;
using CoreAdminWeb.Commons.Validations;
using CoreAdminWeb.Enums;
using CoreAdminWeb.Extensions;
using CoreAdminWeb.Model;
using CoreAdminWeb.Model.DanhSachDoan;
using CoreAdminWeb.Model.KhamSucKhoes;
using CoreAdminWeb.Model.RequestHttps;
using CoreAdminWeb.Model.Settings;
using CoreAdminWeb.Model.User;
using CoreAdminWeb.Services.BaseServices;
using CoreAdminWeb.Services.Users;
using OfficeOpenXml;
using System.Globalization;
using System.Text;

namespace CoreAdminWeb.Services.Imports
{
    public class ImportSoKhamSucKhoeService
    {
        private readonly IBaseDetailService<SoKhamSucKhoeModel> _soKhamSucKhoeService;
        private readonly IUserService _userService;
        private readonly IBaseService<TinhModel> _tinhService;
        private readonly IBaseService<XaPhuongModel> _xaService;
        public ImportSoKhamSucKhoeService(IServiceScopeFactory serviceScopeFactory)
        {
            using (var scope = serviceScopeFactory.CreateScope())
            {
                _soKhamSucKhoeService = scope.ServiceProvider.GetRequiredService<IBaseDetailService<SoKhamSucKhoeModel>>();
                _userService = scope.ServiceProvider.GetRequiredService<IUserService>();
                _tinhService = scope.ServiceProvider.GetRequiredService<IBaseService<TinhModel>>();
                _xaService = scope.ServiceProvider.GetRequiredService<IBaseService<XaPhuongModel>>();
            }
        }


        public async Task ImportFromExcelWithProgressAsync(byte[] fileBytes,
                                                           string connectionId,
                                                           KhamSucKhoeCongTyModel SelectedItem,
                                                           SettingModel settings,
                                                           Func<ProcessingModel, Task> updateProgress,
                                                           CancellationToken cancellationToken)
        {
            try
            {
                ExcelPackage.License.SetNonCommercialOrganization("NonCommercial");

                List<ImportDoanKhamModel> result;
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

                    result = new List<ImportDoanKhamModel>(rowCount - 2);

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

                        var model = new ImportDoanKhamModel
                        {
                            MaLuotKham = worksheet.Cells[row, 1].Text,
                            SoThuTu = worksheet.Cells[row, 2].Text,
                            MaBenhNhan = worksheet.Cells[row, 3].Text,
                            TenBenhNhan = colCount > 3 ? worksheet.Cells[row, 4].Text : null,
                            GioiTinh = colCount > 4 ? worksheet.Cells[row, 5].Text : null,
                            NgaySinh = colCount > 5 ? worksheet.Cells[row, 6].Text : null,
                            SoDienThoai = colCount > 6 ? worksheet.Cells[row, 7].Text : null,
                            CCCD = colCount > 7 ? worksheet.Cells[row, 8].Text : null,
                            NoiCap = colCount > 8 ? worksheet.Cells[row, 9].Text : null,
                            MaXa = colCount > 9 ? worksheet.Cells[row, 10].Text : null,
                            MaTinh = colCount > 10 ? worksheet.Cells[row, 11].Text : null,
                            DiaChi = colCount > 11 ? worksheet.Cells[row, 12].Text : null,
                            Email = colCount > 12 ? worksheet.Cells[row, 13].Text : null,
                        };

                        var validate = ValidateImportData(model);
                        if (string.IsNullOrWhiteSpace(model.Email?.Trim()))
                        {
                            model.Email = $"{model.MaBenhNhan}{settings.email_prefix}";
                            if (!model.Email.IsValidEmail())
                            {
                                model.Email = string.Empty;
                                if (!string.IsNullOrEmpty(validate))
                                {
                                    validate += ", ";
                                }

                                validate += "Email";
                            }
                        }
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

                // Kiểm tra trùng lặp sử dụng HashSet để tăng tốc
                var emailSet = new HashSet<string>();
                var maBenhNhanSet = new HashSet<string>();
                var maLuotKhamSet = new HashSet<string>();
                var emailDuplicates = new List<string>();
                var maBenhNhanDuplicates = new List<string>();
                var maLuotKhamDuplicates = new List<string>();

                foreach (var item in result)
                {
                    if (!string.IsNullOrWhiteSpace(item.Email) && !emailSet.Add(item.Email))
                    {
                        emailDuplicates.Add(item.Email);
                    }

                    if (!string.IsNullOrWhiteSpace(item.MaBenhNhan) && !maBenhNhanSet.Add(item.MaBenhNhan))
                    {
                        maBenhNhanDuplicates.Add(item.MaBenhNhan);
                    }

                    if (!string.IsNullOrWhiteSpace(item.MaLuotKham) && !maLuotKhamSet.Add(item.MaLuotKham))
                    {
                        maLuotKhamDuplicates.Add(item.MaLuotKham);
                    }
                }

                if (emailDuplicates.Any())
                {
                    await updateProgress.Invoke(new ProcessingModel()
                    {
                        ProcessId = connectionId,
                        Status = TrangThaiXuLyNen.Error,
                        Value = $"Email bị trùng lặp: {string.Join("; ", emailDuplicates.Distinct())}",
                        AdditionalParams = new { ShowPopup = true }
                    });
                    return;
                }
                if (maBenhNhanDuplicates.Any())
                {
                    await updateProgress.Invoke(new ProcessingModel()
                    {
                        ProcessId = connectionId,
                        Status = TrangThaiXuLyNen.Error,
                        Value = $"Mã bệnh nhân bị trùng lặp: {string.Join("; ", maBenhNhanDuplicates.Distinct())}",
                        AdditionalParams = new { ShowPopup = true }
                    });
                    return;
                }
                if (maLuotKhamDuplicates.Any())
                {
                    await updateProgress.Invoke(new ProcessingModel()
                    {
                        ProcessId = connectionId,
                        Status = TrangThaiXuLyNen.Error,
                        Value = $"Mã lượt khám bị trùng lặp: {string.Join("; ", maLuotKhamDuplicates.Distinct())}",
                        AdditionalParams = new { ShowPopup = true }
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

                var maBenhNhans = result.Select(c => c.MaBenhNhan).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList();
                var maTinhs = result.Select(c => c.MaTinh ?? string.Empty).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList();
                var maXas = result.Select(c => c.MaXa ?? string.Empty).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList();
                var maLuotKhams = result.Select(c => c.MaLuotKham).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList();

                await updateProgress.Invoke(new ProcessingModel()
                {
                    ProcessId = connectionId,
                    Status = TrangThaiXuLyNen.Processing,
                    Value = $"Kiểm tra dữ liệu bênh nhân đã có..."
                });

                // Chạy các truy vấn batch song song
                var userTask = BatchQueryAsync(
                    ids => _userService.GetAllAsync($"filter[_and][][status][_eq]=active&filter[_and][][ma_tai_khoan][_in]={string.Join(",", ids)}"),
                    maBenhNhans, batchSize
                );
                var tinhTask = BatchQueryAsync(
                    ids => _tinhService.GetAllAsync($"filter[_and][][ma][_in]={string.Join(",", ids)}"),
                    maTinhs, batchSize
                );
                var xaTask = BatchQueryAsync(
                    ids => _xaService.GetAllAsync($"filter[_and][][ma][_in]={string.Join(",", ids)}"),
                    maXas, batchSize
                );

                await Task.WhenAll(userTask, tinhTask, xaTask);

                var existingUsers = userTask.Result;
                var existingTinhs = tinhTask.Result;
                var existingXas = xaTask.Result;

                var existingUserMap = existingUsers.DistinctBy(c => c.ma_tai_khoan).ToDictionary(c => c.ma_tai_khoan, c => c);
                var tinhMap = existingTinhs.DistinctBy(c => c.ma).ToDictionary(c => c.ma ?? "", c => c.id);
                var xaMap = existingXas.DistinctBy(c => $"{c.ma}|{c.tinh?.ma}").ToDictionary(c => $"{c.ma}|{c.tinh?.ma}", c => c.id);

                var updatingUsers = new List<UserModel>();
                var newUsers = new List<UserModel>();

                percent = 0;
                int totalRow = result.Count;
                int rowIndex = 0;
                foreach (var item in result)
                {
                    var splitName = StringExtension.SplitName(item.TenBenhNhan ?? string.Empty);

                    if (existingUserMap.TryGetValue(item.MaBenhNhan, out var existingUser))
                    {
                        existingUser.first_name = splitName.FirstName;
                        existingUser.last_name = splitName.LastName;
                        existingUser.gioi_tinh = item.GioiTinh switch
                        {
                            "Nam" => GioiTinh.Nam,
                            "Nữ" => GioiTinh.Nu,
                            _ => null
                        };
                        if (DateTime.TryParseExact(item.NgaySinh, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var ngaySinh))
                        {
                            existingUser.ngay_sinh = ngaySinh;
                        }

                        existingUser.so_dien_thoai = item.SoDienThoai ?? existingUser.so_dien_thoai;
                        existingUser.so_dinh_danh = item.CCCD ?? existingUser.so_dinh_danh;
                        existingUser.noi_cap = item.NoiCap ?? existingUser.noi_cap;
                        existingUser.dia_chi = item.DiaChi ?? existingUser.dia_chi;
                        existingUser.email = item.Email ?? existingUser.email;
                        existingUser.ma_don_vi = SelectedItem.ma_hop_dong_ksk?.cong_ty?.id;
                        existingUser.tinh = tinhMap.TryGetValue(item.MaTinh ?? "", out var tinhId) ? tinhId : null;
                        existingUser.xa = xaMap.TryGetValue($"{item.MaXa}|{item.MaTinh}", out var xaId) ? xaId : null;
                        existingUser.role = settings.patient_role_id ?? string.Empty;

                        updatingUsers.Add(existingUser);
                    }
                    else if (!newUsers.Any(c => c.ma_tai_khoan == item.MaBenhNhan))
                    {
                        newUsers.Add(new UserModel
                        {
                            first_name = splitName.FirstName,
                            last_name = splitName.LastName,
                            gioi_tinh = item.GioiTinh switch
                            {
                                "Nam" => GioiTinh.Nam,
                                "Nữ" => GioiTinh.Nu,
                                _ => null
                            },
                            ngay_sinh = DateTime.TryParseExact(item.NgaySinh, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var ns) ? ns : null,
                            so_dien_thoai = item.SoDienThoai ?? "",
                            so_dinh_danh = item.CCCD ?? "",
                            noi_cap = item.NoiCap ?? "",
                            dia_chi = item.DiaChi ?? "",
                            email = item.Email ?? "",
                            ma_tai_khoan = item.MaBenhNhan,
                            ma_don_vi = SelectedItem.ma_hop_dong_ksk?.cong_ty?.id,
                            tinh = tinhMap.TryGetValue(item.MaTinh ?? "", out var tinhId) ? tinhId : null,
                            xa = xaMap.TryGetValue($"{item.MaXa}|{item.MaTinh}", out var xaId) ? xaId : null,
                            role = settings.patient_role_id ?? string.Empty,
                            password = GlobalConstant.PwdDefault
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
                            Value = $"Đang xử lý thông tin bệnh nhân {percent}%"
                        });
                    }
                    rowIndex++;
                }

                await updateProgress.Invoke(new ProcessingModel()
                {
                    ProcessId = connectionId,
                    Status = TrangThaiXuLyNen.Processing,
                    Value = $"Đang kiểm tra mã lượt khám tồn tại..."
                });

                var existingRecordsOthers = await BatchQueryAsync(
                    ids => _soKhamSucKhoeService.GetAllAsync(
                        $"filter[_and][][deleted][_eq]=false&filter[_and][][MaDotKham][_neq]={SelectedItem.id}&filter[_and][][ma_luot_kham][_in]={string.Join(",", ids)}"
                    ),
                    maLuotKhams, batchSize
                );

                if (existingRecordsOthers.Any())
                {
                    await updateProgress.Invoke(new ProcessingModel()
                    {
                        ProcessId = connectionId,
                        Status = TrangThaiXuLyNen.Error,
                        Value = $"Mã lượt khám đã tồn tại trên hệ thống: {string.Join("; ", existingRecordsOthers.Select(c => $"'{c.ma_luot_kham}'"))}",
                        AdditionalParams = new { ShowPopup = true }
                    });
                    return;
                }

                await updateProgress.Invoke(new ProcessingModel()
                {
                    ProcessId = connectionId,
                    Status = TrangThaiXuLyNen.Processing,
                    Value = $"Đang cập nhật thông tin bệnh nhân..."
                });

                if (newUsers.Any())
                {
                    var existingByEmail = await BatchQueryAsync(
                        ids => _userService.GetAllAsync($"filter[_and][][status][_eq]=active&filter[_and][][email][_in]={string.Join(",", ids)}"),
                        newUsers.Select(c => c.email).Distinct().ToList(), batchSize
                    );

                    if (existingByEmail != null && existingByEmail.Any())
                    {
                        await updateProgress.Invoke(new ProcessingModel()
                        {
                            ProcessId = connectionId,
                            Status = TrangThaiXuLyNen.Error,
                            Value = $"Email đã tồn tại trên hệ thống: {string.Join("; ", existingByEmail.Select(c => $"'{c.email}'"))}",
                            AdditionalParams = new { ShowPopup = true }
                        });
                        return;
                    }
                }

                // Batch update/create users
                await BatchExecuteAsync(updatingUsers, _userService.UpdateAsync, batchSize);
                await BatchExecuteAsync(newUsers, _userService.CreateAsync, batchSize);

                if (newUsers.Any())
                {
                    existingUsers = await BatchQueryAsync(
                        ids => _userService.GetAllAsync($"filter[_and][][status][_eq]=active&filter[_and][][ma_tai_khoan][_in]={string.Join(",", ids)}"),
                        maBenhNhans, batchSize
                    );
                }

                var allUsers = existingUsers.DistinctBy(c => c.ma_tai_khoan).ToDictionary(c => c.ma_tai_khoan, c => c);

                var existingRecords = await BatchQueryAsync(
                    ids => _soKhamSucKhoeService.GetAllAsync(
                        $"filter[_and][][deleted][_eq]=false&filter[_and][][MaDotKham][_eq]={SelectedItem.id}&filter[_and][][ma_luot_kham][_in]={string.Join(",", ids)}"
                    ),
                    maLuotKhams, batchSize
                );

                var existingRecordKeys = new HashSet<string>(
                    existingRecords
                        .Where(r => !string.IsNullOrEmpty(r.ma_luot_kham))
                        .Select(r => r.ma_luot_kham!)
                );

                await updateProgress.Invoke(new ProcessingModel()
                {
                    ProcessId = connectionId,
                    Status = TrangThaiXuLyNen.Processing,
                    Value = $"Đang khởi tạo hồ sơ khám sức khỏe..."
                });

                var medicalRecordsToCreate = result
                    .Where(c => !existingRecordKeys.Contains(c.MaLuotKham))
                    .Select(item => new SoKhamSucKhoeModel
                    {
                        MaDotKham = SelectedItem,
                        ma_luot_kham = item.MaLuotKham,
                        sort = int.TryParse(item.SoThuTu, out var stt) ? stt : 0,
                        benh_nhan = allUsers.TryGetValue(item.MaBenhNhan, out var user) ? user : null,
                        ma_benh_nhan = item.MaBenhNhan,
                        ngay_kham = SelectedItem.ngay_du_kien_kham ?? DateTime.Now,
                        ngay_lap_so = DateTime.Now,
                        ma_cong_ty = SelectedItem.ma_hop_dong_ksk?.cong_ty?.id,
                        nguoi_lap = SelectedItem.nguoi_lap_so?.full_name,
                        chu_ky_nls = SelectedItem.nguoi_lap_so?.chu_ky_bac_si
                    }).ToList();

                await BatchExecuteAsync(medicalRecordsToCreate, _soKhamSucKhoeService.CreateAsync, batchSize);

                await updateProgress.Invoke(new ProcessingModel()
                {
                    ProcessId = connectionId,
                    Status = TrangThaiXuLyNen.Completed,
                    Value = $"Import Excel hoàn tất!"
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

        static string ValidateImportData(ImportDoanKhamModel import)
        {
            StringBuilder builder = new StringBuilder();
            if (string.IsNullOrEmpty(import.MaBenhNhan))
            {
                if (builder.Length > 0)
                {
                    builder.Append(", ");
                }

                builder.Append("Mã bệnh nhân");
            }

            if (string.IsNullOrEmpty(import.MaLuotKham))
            {
                if (builder.Length > 0)
                {
                    builder.Append(", ");
                }

                builder.Append("Mã lượt khám");
            }

            if (!string.IsNullOrEmpty(import.NgaySinh) && !DateTime.TryParseExact(import.NgaySinh, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            {
                if (builder.Length > 0)
                {
                    builder.Append(", ");
                }
                builder.Append("Ngày sinh");
            }

            return builder.ToString();
        }
    }
}
