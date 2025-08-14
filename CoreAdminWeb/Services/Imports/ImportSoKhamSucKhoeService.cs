using CoreAdminWeb.Enums;
using CoreAdminWeb.Extensions;
using CoreAdminWeb.Hubs;
using CoreAdminWeb.Model;
using CoreAdminWeb.Model.DanhSachDoan;
using CoreAdminWeb.Model.RequestHttps;
using CoreAdminWeb.Model.User;
using CoreAdminWeb.Services.BaseServices;
using CoreAdminWeb.Services.Users;
using Microsoft.AspNetCore.SignalR;
using OfficeOpenXml;
using System.Globalization;

namespace CoreAdminWeb.Services.Imports
{
    public class ImportSoKhamSucKhoeService
    {
        private readonly IHubContext<ImportProgressHub> _hubContext;
        private readonly IBaseDetailService<SoKhamSucKhoeModel> _soKhamSucKhoeService;
        private readonly IUserService _userService;
        private readonly IBaseService<TinhModel> _tinhService;
        private readonly IBaseService<XaPhuongModel> _xaService;
        public ImportSoKhamSucKhoeService(IHubContext<ImportProgressHub> hubContext, IServiceScopeFactory serviceScopeFactory)
        {
            _hubContext = hubContext;
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
                                                           string? accessToken,
                                                           CancellationToken cancellationToken)
        {
            try
            {
                ExcelPackage.License.SetNonCommercialOrganization("NonCommercial");

                // 1. Đọc Excel streaming -> list model
                List<ImportDoanKhamModel> result;
                int percent = 0;
                using (var ms = new MemoryStream(fileBytes))
                using (var package = new ExcelPackage(ms))
                {
                    var worksheet = package.Workbook.Worksheets[0];
                    var rowCount = worksheet.Dimension.Rows;
                    var colCount = worksheet.Dimension.Columns;

                    result = new List<ImportDoanKhamModel>(rowCount - 1);
                    for (int row = 2; row <= rowCount; row++)
                    {
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

                        if (!string.IsNullOrWhiteSpace(model.MaBenhNhan) || !string.IsNullOrWhiteSpace(model.MaLuotKham))
                        {
                            int nextPercent = (int)Math.Round((double)(row - 1) * 100 / (rowCount - 1));
                            if (row == rowCount || percent != nextPercent)
                            {
                                percent = nextPercent;
                                await _hubContext.Clients.Client(connectionId)
                                    .SendAsync("ImportProgress", $"Đang đọc dữ liệu import {percent}%", cancellationToken);
                            }
                            result.Add(model);
                        }
                    }
                }

                if (result.Count == 0)
                {
                    await _hubContext.Clients.Client(connectionId)
                        .SendAsync("ImportCompleted", "Không có dữ liệu để import!");
                    return;
                }

                var maBenhNhans = result.Select(c => c.MaBenhNhan).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList();
                var maTinhs = result.Select(c => c.MaTinh).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList();
                var maXas = result.Select(c => c.MaXa).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList();
                var maLuotKhams = result.Select(c => c.MaLuotKham).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList();

                await _hubContext.Clients.Client(connectionId)
                    .SendAsync("ImportProgress", "Kiểm tra dữ liệu bênh nhân đã có...", cancellationToken);

                // 3. Batch query song song
                var userTask = BatchQueryAsync(
                    ids => _userService.GetAllAsync($"filter[_and][][status][_eq]=active&filter[_and][][ma_benh_nhan][_in]={string.Join(",", ids)}"),
                    maBenhNhans
                );
                var tinhTask = BatchQueryAsync(
                    ids => _tinhService.GetAllAsync($"filter[_and][][ma][_in]={string.Join(",", ids)}"),
                    maTinhs.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x!).ToList()
                );
                var xaTask = BatchQueryAsync(
                    ids => _xaService.GetAllAsync($"filter[_and][][ma][_in]={string.Join(",", ids)}"),
                    maXas.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x!).ToList()
                );

                await Task.WhenAll(userTask, tinhTask, xaTask);

                var existingUsers = userTask.Result;
                var existingTinhs = tinhTask.Result;
                var existingXas = xaTask.Result;

                var existingUserMap = existingUsers.ToDictionary(c => c.ma_benh_nhan, c => c);
                var tinhMap = existingTinhs.ToDictionary(c => c.code ?? "", c => c.id);
                var xaMap = existingXas.ToDictionary(c => $"{c.code}|{c.tinh?.code}", c => c.id);

                // 4. Xử lý danh sách cần update và thêm mới
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

                        updatingUsers.Add(existingUser);
                    }
                    else if (!newUsers.Any(c => c.ma_benh_nhan == item.MaBenhNhan))
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
                            ma_benh_nhan = item.MaBenhNhan,
                            ma_don_vi = SelectedItem.ma_hop_dong_ksk?.cong_ty?.id,
                            tinh = tinhMap.TryGetValue(item.MaTinh ?? "", out var tinhId) ? tinhId : null,
                            xa = xaMap.TryGetValue($"{item.MaXa}|{item.MaTinh}", out var xaId) ? xaId : null
                        });
                    }

                    int nextPercent = (int)Math.Round((double)(rowIndex - 1) * 100 / (totalRow - 1));
                    if (rowIndex == totalRow || percent != nextPercent)
                    {
                        percent = nextPercent;
                        await _hubContext.Clients.Client(connectionId)
                            .SendAsync("ImportProgress", $"Đang xử lý thông tin bệnh nhân {percent}%", cancellationToken);
                    }
                    rowIndex++;
                }

                await _hubContext.Clients.Client(connectionId)
                    .SendAsync("ImportProgress", $"Đang cập nhật thông tin bệnh nhân...", cancellationToken);

                // 5. Batch update / create user
                await BatchExecuteAsync(updatingUsers, _userService.UpdateAsync);
                await BatchExecuteAsync(newUsers, _userService.CreateAsync);

                // 6. Lấy danh sách user mới nhất
                var allUsers = existingUsers.Concat(newUsers).ToDictionary(c => c.ma_benh_nhan, c => c);

                // 7. Lấy hồ sơ khám đã tồn tại
                var existingRecords = await BatchQueryAsync(ids => _soKhamSucKhoeService.GetAllAsync(
                    $"filter[_and][][deleted][_eq]=false&filter[_and][][MaDotKham][_eq]={SelectedItem.id}&filter[_and][][ma_luot_kham][_in]={string.Join(",", ids)}"
                ), maLuotKhams);

                var existingRecordKeys = new HashSet<string>(
                    existingRecords
                        .Where(r => !string.IsNullOrEmpty(r.ma_luot_kham))
                        .Select(r => r.ma_luot_kham!)
                );

                await _hubContext.Clients.Client(connectionId)
                    .SendAsync("ImportProgress", $"Đang khởi tạo hồ sơ bệnh nhân...", cancellationToken);

                // 8. Tạo hồ sơ mới
                var medicalRecordsToCreate = result
                    .Where(c => !existingRecordKeys.Contains(c.MaLuotKham))
                    .Select(item => new SoKhamSucKhoeModel
                    {
                        MaDotKham = SelectedItem,
                        ma_luot_kham = item.MaLuotKham,
                        sort = int.TryParse(item.SoThuTu, out var stt) ? stt : 0,
                        benh_nhan = allUsers.TryGetValue(item.MaBenhNhan, out var user) ? user : null,
                        ngay_kham = SelectedItem.ngay_du_kien_kham,
                        ngay_lap_so = DateTime.Now,
                        ma_cong_ty = SelectedItem.ma_hop_dong_ksk?.cong_ty?.id,
                        nguoi_lap = SelectedItem.nguoi_lap_so?.full_name,
                        chu_ky_nls = SelectedItem.nguoi_lap_so?.chu_ky_bac_si
                    }).ToList();

                await BatchExecuteAsync(medicalRecordsToCreate, _soKhamSucKhoeService.CreateAsync);

                await _hubContext.Clients.Client(connectionId)
                .SendAsync("ImportCompleted", "Import Excel hoàn tất!");
            }
            catch (Exception ex)
            {
                await _hubContext.Clients.Client(connectionId)
                .SendAsync("ImportError", $"Lỗi khi import: {ex.Message}");
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
    }
}
