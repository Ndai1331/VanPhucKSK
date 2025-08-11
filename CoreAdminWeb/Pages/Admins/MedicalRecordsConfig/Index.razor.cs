using CoreAdminWeb.Commons;
using CoreAdminWeb.Enums;
using CoreAdminWeb.Helpers;
using CoreAdminWeb.Model;
using CoreAdminWeb.Model.Contract;
using CoreAdminWeb.Model.User;
using CoreAdminWeb.Services;
using CoreAdminWeb.Services.BaseServices;
using CoreAdminWeb.Services.Users;
using CoreAdminWeb.Shared.Base;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CoreAdminWeb.Pages.Admins.MedicalRecordsConfig
{
    public partial class Index(IBaseService<KhamSucKhoeCongTyModel> MainService,
                               IBaseService<CongTyModel> CongTyService,
                               IBaseService<ContractModel> HopDongService,
                               IUserService UserService) : BlazorCoreBase
    {
        private List<KhamSucKhoeCongTyModel> MainModels { get; set; } = new();
        private bool openDeleteModal = false;
        private bool openAddOrUpdateModal = false;
        private KhamSucKhoeCongTyModel SelectedItem { get; set; } = new KhamSucKhoeCongTyModel();
        private CongTyModel? SelectedCongTy { get; set; } = null;
        private string _searchString = "";
        private string _searchStatusString = "";
        private string _titleAddOrUpdate = "Thêm mới";


        private bool readOnly { get; set; } = false;

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
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
                BuilderQuery += $"&filter[_and][1][_or][0][ma_hop_dong_ksk][code][_eq]={_searchString}";
                BuilderQuery += $"&filter[_and][1][_or][1][ma_hop_dong_ksk][cong_ty][code][_eq]={_searchString}";
                BuilderQuery += $"&filter[_and][1][_or][2][ma_hop_dong_ksk][name][_contains]={_searchString}";
                BuilderQuery += $"&filter[_and][1][_or][3][ma_hop_dong_ksk][cong_ty][name][_contains]={_searchString}";
                BuilderQuery += $"&filter[_and][1][_or][4][code][_eq]={_searchString}";
            }
            if (!string.IsNullOrEmpty(_searchStatusString))
            {
                BuilderQuery += $"&filter[_and][2][status][_eq]={_searchStatusString}";
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

        private async Task<IEnumerable<CongTyModel>> LoadCongTyData(string searchText)
        {
            return await LoadBlazorTypeaheadData(searchText, CongTyService, "filter[_and][][status][_eq]=published");
        }

        private async Task<IEnumerable<ContractModel>> LoadHopDongData(string searchText)
        {
            string query = "";
            if (SelectedCongTy != null)
            {
                query = $"filter[_and][][cong_ty][_eq]={SelectedCongTy.id}";
            }
            return await LoadBlazorTypeaheadData(searchText, HopDongService, query);
        }

        private async Task<IEnumerable<UserModel>> LoadBacSiData(string searchText)
        {
            try
            {
                var query = "sort=-id";

                query += "&filter[_and][][status][_eq]=active";
                query += $"&filter[_and][][role][_eq]={GlobalConstant.DOCTOR_ROLE_ID}";

                if (!string.IsNullOrEmpty(searchText))
                {
                    query += $"&filter[_and][0][_or][0][first_name][_contains]={Uri.EscapeDataString(searchText)}";
                    query += $"&filter[_and][0][_or][1][last_name][_contains]={Uri.EscapeDataString(searchText)}";
                    query += $"&filter[_and][0][_or][2][ma_benh_nhan][_contains]={Uri.EscapeDataString(searchText)}";
                    query += $"&filter[_and][0][_or][3][so_dinh_danh][_contains]={Uri.EscapeDataString(searchText)}";
                }

                var result = await UserService.GetAllAsync(query);
                return result?.IsSuccess == true ? result.Data ?? Enumerable.Empty<UserModel>() : Enumerable.Empty<UserModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading typeahead data: {ex.Message}");
                return Enumerable.Empty<UserModel>();
            }
        }

        private void OpenDeleteModal(KhamSucKhoeCongTyModel item)
        {
            SelectedItem = item;
            openDeleteModal = true;
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

        private async Task OpenAddOrUpdateModal(KhamSucKhoeCongTyModel? item, bool isReadOnly = false)
        {
            readOnly = isReadOnly;
            _titleAddOrUpdate = item != null ? "Sửa" : "Thêm mới";
            if (readOnly)
            {
                _titleAddOrUpdate = "Thông tin hồ sơ: ";
            }

            SelectedItem = item != null ? item.DeepClone() : new KhamSucKhoeCongTyModel();
            SelectedCongTy = SelectedItem.ma_hop_dong_ksk?.cong_ty;

            if (SelectedCongTy == null && SelectedItem.ma_don_vi != null)
            {
                var congTyResult = await CongTyService.GetAllAsync($"filter[_and][][status][_eq]=published&filter[_and][][code][_eq]={SelectedItem.ma_don_vi}");
                if (congTyResult.IsSuccess && congTyResult.Data != null && congTyResult.Data.Any())
                {
                    SelectedCongTy = congTyResult.Data.FirstOrDefault();
                    SelectedItem.ma_don_vi = SelectedCongTy?.code;
                }
            }

            if (readOnly)
            {
                _titleAddOrUpdate = $"Thông tin hồ sơ: {SelectedItem.code}";
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
            if (string.IsNullOrEmpty(SelectedItem.code))
            {
                AlertService.ShowAlert("Mã hồ sơ là bắt buộc", "danger");
                return;
            }

            if (SelectedItem.ma_hop_dong_ksk == null)
            {
                AlertService.ShowAlert("Mã hợp đồng là bắt buộc", "danger");
                return;
            }

            if (SelectedItem.ma_don_vi == null)
            {
                AlertService.ShowAlert("Tên công ty là bắt buộc", "danger");
                return;
            }

            if (SelectedItem.id == 0)
            {
                var result = await MainService.CreateAsync(SelectedItem);
                if (result.IsSuccess)
                {
                    await LoadData();
                    openAddOrUpdateModal = false;
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
                    await LoadData();
                    openAddOrUpdateModal = false;
                    AlertService.ShowAlert("Cập nhật thành công!", "success");
                }
                else
                {
                    AlertService.ShowAlert(result.Message ?? "Lỗi khi cập nhật dữ liệu", "danger");
                }
            }
        }

        private void CloseAddOrUpdateModal()
        {
            SelectedItem = new KhamSucKhoeCongTyModel();
            openAddOrUpdateModal = false;
        }

        private async Task OnDateChanged(ChangeEventArgs e, string fieldName, bool isFilter = false)
        {
            try
            {
                var dateStr = e.Value?.ToString();
                if (string.IsNullOrEmpty(dateStr))
                {
                    ReflectionHelper.SetFieldValue(this, SelectedItem, fieldName, null);
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
                        ReflectionHelper.SetFieldValue(this, SelectedItem, fieldName, date);
                    }
                }

                if (isFilter)
                {
                    await LoadData();
                }
            }
            catch (Exception ex)
            {
                AlertService.ShowAlert($"Lỗi khi xử lý ngày: {ex.Message}", "danger");
            }
        }

        private async Task OnStatusFilterChanged(ChangeEventArgs? selected)
        {
            _searchStatusString = selected?.Value?.ToString() ?? string.Empty;

            await LoadData();
        }

        // private void OnStatusChanged(ChangeEventArgs? selected)
        // {
        //     _selectedStatusString = selected?.Value?.ToString() ?? string.Empty;
        //     SelectedItem.active = true;
        //     if (bool.TryParse(_selectedStatusString, out bool activeValue))
        //     {
        //         SelectedItem.active = activeValue;
        //     }
        // }

        private void OnCongTyChanged(CongTyModel? selected)
        {
            SelectedCongTy = selected;
            SelectedItem.ma_hop_dong_ksk = null;
        }

        private void OnHopDongChanged(ContractModel? selected)
        {
            SelectedItem.ma_hop_dong_ksk = selected;
            if (selected?.cong_ty != null)
            {
                SelectedCongTy = selected.cong_ty;
                SelectedItem.ma_don_vi = SelectedCongTy?.code;
            }
        }
    }
}
