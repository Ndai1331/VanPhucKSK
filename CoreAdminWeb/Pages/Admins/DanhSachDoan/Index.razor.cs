using CoreAdminWeb.Helpers;
using CoreAdminWeb.Model.KhamSucKhoes;
using CoreAdminWeb.Model.User;
using CoreAdminWeb.Services.BaseServices;
using CoreAdminWeb.Services.Imports;
using CoreAdminWeb.Services.Users;
using CoreAdminWeb.Shared.Base;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;

namespace CoreAdminWeb.Pages.Admins.DanhSachDoan
{
    public partial class Index(IBaseService<KhamSucKhoeCongTyModel> MainService,
                               IBaseDetailService<SoKhamSucKhoeModel> SoKhamSucKhoeService,
                               IUserService UserService,
                               ImportSoKhamSucKhoeService importSoKhamSucKhoeService,
                               NavigationManager NavManager) : BlazorCoreBase
    {
        private List<KhamSucKhoeCongTyModel> MainModels { get; set; } = new();
        private bool openDeleteModal = false;
        private bool openAddOrUpdateModal = false;
        private bool openDetailDeleteModal = false;
        private KhamSucKhoeCongTyModel SelectedItem { get; set; } = new KhamSucKhoeCongTyModel();
        private List<SoKhamSucKhoeModel> SelectedItemsDetail { get; set; } = new List<SoKhamSucKhoeModel>();
        private SoKhamSucKhoeModel? SelectedItemDetail { get; set; } = default;
        private string _searchString = "";
        private string _titleAddOrUpdate = "Thêm mới";

        // Select in table
        private CancellationTokenSource? _filterCancellationTokenSource;
        private Dictionary<int, List<UserModel>> SelectedUserItems { get; set; } = new();
        private List<UserModel> UserItems { get; set; } = new();

        private const long MaxExcelFileSize = 25 * 1024 * 1024; // 25MB, adjust as needed

        private HubConnection? connection;
        private string? connectionId = "";
        private string? importProcessingMessage { get; set; }
        public bool isImportDone { get; set; }
        public bool isErrorPopup { get; set; }
        public bool isImportError { get; set; }
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            try
            {
                connection = new HubConnectionBuilder()
                .WithUrl(NavManager.ToAbsoluteUri("/importProgressHub"))
                .Build();

                connection.On<string>("ImportProgress", message =>
                {
                    isImportError = false;
                    isImportDone = false;
                    importProcessingMessage = message;
                    InvokeAsync(StateHasChanged);
                });

                connection.On<string>("ImportCompleted", async message =>
                {
                    isImportDone = true;
                    isImportError = false;
                    importProcessingMessage = message;

                    await LoadDetailData();
                    await InvokeAsync(StateHasChanged);
                });

                connection.On<string, bool>("ImportError", (message, isPopup) =>
                {
                    isImportError = true;
                    isImportDone = isPopup;
                    isErrorPopup = isPopup;
                    importProcessingMessage = message;
                    InvokeAsync(StateHasChanged);
                });

                await connection.StartAsync();
                connectionId = connection.ConnectionId;
            }
            catch
            {
                AlertService.ShowAlert("Lỗi khi khởi tạo socket", "danger");
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await LoadData();
                await JsRuntime.InvokeAsync<IJSObjectReference>("import", "/assets/js/pages/flatpickr.js");
                StateHasChanged();
            }
        }

        private async Task LoadData()
        {
            IsLoading = true;

            BuildPaginationQuery(Page, PageSize);
            BuilderQuery += $"&filter[_and][0][deleted][_eq]=false";
            if (!string.IsNullOrEmpty(_searchString))
            {
                BuilderQuery += $"&filter[_and][1][_or][0][code][_contains]={_searchString}";
            }
            var result = await MainService.GetAllAsync(BuilderQuery);
            if (result.IsSuccess)
            {
                MainModels = result.Data ?? new List<KhamSucKhoeCongTyModel>();
                if (result.Meta != null)
                {
                    TotalItems = result.Meta.filter_count ?? 0;
                    TotalPages = (int)Math.Ceiling((double)TotalItems / PageSize);
                }
            }
            else
            {
                MainModels = new List<KhamSucKhoeCongTyModel>();
            }
            IsLoading = false;
        }

        private async Task OnPageSizeChanged(int newSize)
        {
            Page = 1;
            PageSize = newSize;
            await LoadData();
        }

        private async Task SelectedPage(int page)
        {
            Page = page;
            await LoadData();
        }

        private async Task<List<UserModel>> LoadUserDataInTable(IEnumerable<UserModel> allItems, string filter, CancellationToken token)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(allItems);

                // Cancel previous filter operation
                if (_filterCancellationTokenSource != null)
                {
                    await _filterCancellationTokenSource.CancelAsync();
                    _filterCancellationTokenSource.Dispose();
                }

                _filterCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);

                // Debouncing - wait 300ms before making API call
                await Task.Delay(300, _filterCancellationTokenSource.Token);

                var query = "sort=-id";

                query += "&filter[_and][][status][_eq]=active";
                query += $"&filter[_and][][role][_eq]={CurrentSetting.patient_role_id}";
                query += $"&filter[_and][][ma_don_vi][_eq]={SelectedItem.ma_hop_dong_ksk?.cong_ty?.id}";

                if (!string.IsNullOrEmpty(filter))
                {
                    query += $"&filter[_and][0][_or][0][first_name][_contains]={Uri.EscapeDataString(filter)}";
                    query += $"&filter[_and][0][_or][1][last_name][_contains]={Uri.EscapeDataString(filter)}";
                    query += $"&filter[_and][0][_or][2][so_dinh_danh][_contains]={Uri.EscapeDataString(filter)}";
                    query += $"&filter[_and][0][_or][3][ma_benh_nhan][_contains]={Uri.EscapeDataString(filter)}";
                }

                Console.WriteLine($"Loading filter data from API for '{filter}'");
                var result = await UserService.GetAllAsync(query);
                UserItems = result?.Data ?? new List<UserModel>();
                StateHasChanged();
                return UserItems;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in filterFunction: {ex.Message}");
                return new List<UserModel>();
            }
        }


        private async Task LoadDetailData()
        {
            SelectedItemsDetail = new List<SoKhamSucKhoeModel>();
            var buildQuery = $"sort=sort";
            buildQuery += $"&filter[_and][][MaDotKham][_eq]={SelectedItem.id}";
            buildQuery += $"&filter[_and][][deleted][_eq]=false";
            var result = await SoKhamSucKhoeService.GetAllAsync(buildQuery);
            if (result.IsSuccess)
            {
                SelectedItemsDetail = result.Data ?? new List<SoKhamSucKhoeModel>();
            }
            else
            {
                AlertService.ShowAlert(result.Message ?? "Lỗi khi tải dữ liệu chi tiết", "danger");
            }
        }

        private async Task OnDelete()
        {
            var result = await MainService.DeleteAsync(SelectedItem);
            if (result.IsSuccess && result.Data)
            {
                await LoadData();
                AlertService.ShowAlert("Xoá thành công!", "success");
                openDeleteModal = false;
            }
            else
            {
                AlertService.ShowAlert(result.Message ?? "Lỗi khi xóa dữ liệu", "danger");
            }
        }

        private void CloseDeleteModal()
        {
            SelectedItem = new KhamSucKhoeCongTyModel();
            openDeleteModal = false;
        }

        private void OpenDetailDeleteModal(SoKhamSucKhoeModel item)
        {
            SelectedItemDetail = item;

            openDetailDeleteModal = true;
        }

        private void OnDetailDelete()
        {
            if (SelectedItemDetail == null)
            {
                AlertService.ShowAlert("Không có dữ liệu để xóa", "warning");
                return;
            }

            foreach (var item in SelectedItemsDetail)
            {
                if (item.id > 0 && item.id == SelectedItemDetail.id || item.sort > 0 && item.sort == SelectedItemDetail.sort)
                {
                    item.deleted = true;
                }
            }

            SelectedItemDetail = default;

            openDetailDeleteModal = false;

            if (!SelectedItemsDetail.Any(c => !c.deleted))
            {
                SelectedItemsDetail.Add(new SoKhamSucKhoeModel()
                {
                    MaDotKham = SelectedItem,
                    sort = (SelectedItemsDetail.Max(c => c.sort) ?? 0) + 1,
                    code = string.Empty,
                    name = string.Empty,
                    ngay_kham = SelectedItem.ngay_du_kien_kham,
                    ngay_lap_so = DateTime.Now
                });
            }

            StateHasChanged();
        }

        private void CloseDetailDeleteModal()
        {
            SelectedItemDetail = default;

            openDetailDeleteModal = false;
        }

        private void OnAddChiTiet()
        {
            if (SelectedItemsDetail == null)
            {
                SelectedItemsDetail = new List<SoKhamSucKhoeModel>();
            }

            SelectedItemsDetail.Add(new SoKhamSucKhoeModel
            {
                MaDotKham = SelectedItem,
                sort = (SelectedItemsDetail.Max(c => c.sort) ?? 0) + 1,
                code = string.Empty,
                name = string.Empty,
                ngay_kham = SelectedItem.ngay_du_kien_kham,
                ngay_lap_so = DateTime.Now
            });

            // Wait for modal to render
            _ = Task.Run(async () =>
            {
                await Task.Delay(500);
                await JsRuntime.InvokeVoidAsync("initializeDatePicker");
            });
        }

        private async Task OpenAddOrUpdateModal(KhamSucKhoeCongTyModel? item)
        {
            isImportDone = true;
            _titleAddOrUpdate = item != null ? "Sửa" : "Thêm mới";
            SelectedItem = item != null ? item.DeepClone() : new KhamSucKhoeCongTyModel();

            SelectedItemsDetail = new List<SoKhamSucKhoeModel>();

            if (SelectedItem.id > 0)
            {
                await LoadDetailData();
            }

            if (!SelectedItemsDetail.Any())
            {
                SelectedItemsDetail.Add(new SoKhamSucKhoeModel()
                {
                    MaDotKham = SelectedItem,
                    sort = (SelectedItemsDetail.Max(c => c.sort) ?? 0) + 1,
                    code = string.Empty,
                    name = string.Empty,
                    ngay_kham = SelectedItem.ngay_du_kien_kham,
                    ngay_lap_so = DateTime.Now
                });
            }

            openAddOrUpdateModal = true;

            // Wait for modal to render
            _ = Task.Run(async () =>
            {
                await Task.Delay(500);
                await JsRuntime.InvokeVoidAsync("initializeDatePicker");
            });
        }

        private async Task OnValidSubmit()
        {
            if (!FormValidation())
            {
                return;
            }
            if (SelectedItem.id == 0)
            {
                var result = await MainService.CreateAsync(SelectedItem);
                if (result.IsSuccess)
                {
                    var chiTietList = SelectedItemsDetail
                        .Where(c => !c.deleted)
                        .Select(c =>
                        {
                            c.MaDotKham = result.Data;
                            return c;
                        })
                        .ToList();

                    var detailResult = await SoKhamSucKhoeService.CreateAsync(chiTietList);
                    if (!detailResult.IsSuccess)
                    {
                        AlertService.ShowAlert(detailResult.Message ?? "Lỗi khi thêm mới chi tiết dữ liệu", "danger");
                        return;
                    }
                    await LoadDetailData();
                    AlertService.ShowAlert("Thêm mới thành công!", "success");
                }
                else
                {
                    AlertService.ShowAlert(result.Message ?? "Lỗi khi thêm mới dữ liệu", "danger");
                }
            }
            else
            {
                var result = await MainService.UpdateAsync(SelectedItem);
                if (result.IsSuccess)
                {
                    var addNewChiTietList = SelectedItemsDetail
                        .Where(c => (!c.deleted) && c.id == 0)
                        .Select(c =>
                        {
                            c.MaDotKham = SelectedItem;
                            return c;
                        }).ToList();
                    var removeChiTietList = SelectedItemsDetail
                        .Where(c => c.deleted && c.id > 0)
                        .Select(c =>
                        {
                            c.MaDotKham = SelectedItem;
                            c.deleted = true;
                            return c;
                        }).ToList();
                    var updateChiTietList = SelectedItemsDetail
                        .Where(c => (!c.deleted) && c.id > 0)
                        .Select(c =>
                        {
                            c.MaDotKham = SelectedItem;
                            return c;
                        }).ToList();

                    if (addNewChiTietList.Any())
                    {
                        var detailResult = await SoKhamSucKhoeService.CreateAsync(addNewChiTietList);
                        if (!detailResult.IsSuccess)
                        {
                            AlertService.ShowAlert(detailResult.Message ?? "Lỗi khi thêm mới chi tiết dữ liệu", "danger");
                            return;
                        }
                    }

                    if (removeChiTietList.Any())
                    {
                        var detailResult = await SoKhamSucKhoeService.DeleteAsync(removeChiTietList);
                        if (!detailResult.IsSuccess)
                        {
                            AlertService.ShowAlert(detailResult.Message ?? "Lỗi khi xóa chi tiết dữ liệu", "danger");
                            return;
                        }
                    }

                    if (updateChiTietList.Any())
                    {
                        var detailResult = await SoKhamSucKhoeService.UpdateAsync(updateChiTietList);
                        if (!detailResult.IsSuccess)
                        {
                            AlertService.ShowAlert(detailResult.Message ?? "Lỗi khi cập nhật chi tiết dữ liệu", "danger");
                            return;
                        }
                    }

                    await LoadDetailData();
                    AlertService.ShowAlert("Cập nhật thành công!", "success");
                }
                else
                {
                    AlertService.ShowAlert(result.Message ?? "Lỗi khi cập nhật dữ liệu", "danger");
                }
            }
        }

        private bool FormValidation()
        {
            var indexValue = SelectedItemsDetail.FindIndex(c => !c.deleted && (c.benh_nhan == null || string.IsNullOrEmpty(c.ma_luot_kham)));
            if (indexValue >= 0)
            {
                AlertService.ShowAlert($"Thông tin bệnh nhân tại dòng {indexValue + 1} bị thiếu!", "warning");
                return false;
            }
            return true;
        }

        private void CloseAddOrUpdateModal()
        {
            SelectedItem = new KhamSucKhoeCongTyModel();
            openAddOrUpdateModal = false;
        }

        private void GoToDetail(int id)
        {
            NavigationManager?.NavigateTo($"/admin/danh-sach-doan-ho-so-kham-suc-khoe/{id}");
        }

        private void OnValueChanged(ChangeEventArgs e, string fieldName, SoKhamSucKhoeModel item)
        {
            try
            {
                var dateStr = e.Value?.ToString();
                if (string.IsNullOrEmpty(dateStr))
                {
                    ReflectionHelper.SetFieldValue(item, fieldName, null);
                }
                else
                {
                    var parts = dateStr.Split('/');
                    if (parts.Length == 3 &&
                        int.TryParse(parts[0], out int day) &&
                        int.TryParse(parts[1], out int month) &&
                        int.TryParse(parts[2], out int year))
                    {
                        var date = new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Local);
                        ReflectionHelper.SetFieldValue(item, fieldName, date);
                    }
                }
            }
            catch (Exception ex)
            {
                AlertService.ShowAlert($"Lỗi khi xử lý ngày: {ex.Message}", "danger");
            }
        }

        private async Task OnExcelFileSelected(InputFileChangeEventArgs e)
        {
            try
            {
                var file = e.File;
                if (file == null)
                {
                    AlertService.ShowAlert("Vui lòng chọn file excel!", "warning");
                    return;
                }
                if (
                    file.ContentType != "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                    && file.ContentType != "application/vnd.ms-excel")
                {
                    AlertService.ShowAlert("Vui lòng chọn file Excel hợp lệ!", "warning");
                    return;
                }

                // Additional check to ensure file.Size is not greater than MaxExcelFileSize
                if (file.Size <= 0 || file.Size > MaxExcelFileSize)
                {
                    AlertService.ShowAlert("Kích thước file không hợp lệ hoặc vượt quá giới hạn cho phép!", "warning");
                    return;
                }

                using var stream = file.OpenReadStream(file.Size);
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                var fileBytes = ms.ToArray();

                SelectedItemsDetail = new List<SoKhamSucKhoeModel>();

                // Gọi hàm import excel từ service
                _ = Task.Run(() => importSoKhamSucKhoeService.ImportFromExcelWithProgressAsync(
                    fileBytes,
                    connectionId ?? string.Empty,
                    SelectedItem,
                    CurrentSetting.patient_role_id ?? string.Empty,
                    CancellationToken.None)
                );
            }
            catch (Exception ex)
            {
                AlertService.ShowAlert($"Lỗi khi import file: {ex.Message}", "danger");
                await LoadDetailData();
            }
        }

        private async Task OpenFileDialog()
        {
            // Clear the file input before processing
            await JsRuntime.InvokeVoidAsync("eval", "document.getElementById('excelFileInput').value = ''");
            await JsRuntime.InvokeVoidAsync("eval", "document.getElementById('excelFileInput').click()");
        }
    }
}