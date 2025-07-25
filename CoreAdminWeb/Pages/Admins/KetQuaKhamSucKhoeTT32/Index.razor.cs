using CoreAdminWeb.Helpers;
using CoreAdminWeb.Model;
using CoreAdminWeb.Model.Contract;
using CoreAdminWeb.Model.User;
using CoreAdminWeb.Services.BaseServices;
using CoreAdminWeb.Services.Users;
using CoreAdminWeb.Shared.Base;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CoreAdminWeb.Pages.Admins.KetQuaKhamSucKhoeTT32
{
    public partial class Index(IBaseService<DinhMucModel> MainService,
                               IBaseService<LoaiDinhMucModel> LoaiDinhMucService,
                               IBaseService<PhanLoaiSucKhoeModel> PhanLoaiSucKhoaService,
                               IUserService UserService,
                               IBaseService<ContractModel> ContractService,
                               IBaseService<KhamSucKhoeCongTyModel> KhamSucKhoeCongTyService) : BlazorCoreBase
    {
        private List<DinhMucModel> MainModels { get; set; } = new();
        private bool openDeleteModal = false;
        private bool openAddOrUpdateModal = false;
        private string activeDefTab = "tab1";

        private DinhMucModel SelectedItem { get; set; } = new DinhMucModel();
        private PhanLoaiSucKhoeModel? SelectedPhanLoaiItem { get; set; }
        private ContractModel? SelectedContractItem { get; set; }
        private KhamSucKhoeCongTyModel? SelectedKhamSucKhoeCongTyItem { get; set; }

        private UserModel? SelectedBacSiItem { get; set; }
        private string _searchString = "";
        private string _searchStatusString = "";
        private string _titleAddOrUpdate = "Thêm mới";

        private string _selectedStatusString = "";
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

                // Wait for modal to render
                _ = Task.Run(async () =>
                {
                    await Task.Delay(500);
                    await JsRuntime.InvokeVoidAsync("initializeDatePicker");
                });
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
                BuilderQuery += $"&filter[_and][1][_or][1][name][_contains]={_searchString}";
                BuilderQuery += $"&filter[_and][1][_or][2][loai_dinh_muc][name][_contains]={_searchString}";
            }
            if (!string.IsNullOrEmpty(_searchStatusString))
            {
                BuilderQuery += $"&filter[_and][2][active][_eq]={_searchStatusString}";
            }
            var result = await MainService.GetAllAsync(BuilderQuery);
            if (result.IsSuccess)
            {
                MainModels = result.Data ?? new List<DinhMucModel>();
                if (result.Meta != null)
                {
                    TotalItems = result.Meta.filter_count ?? 0;
                    TotalPages = (int)Math.Ceiling((double)TotalItems / PageSize);
                }
            }
            else
            {
                MainModels = new List<DinhMucModel>();
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

        private async Task<IEnumerable<LoaiDinhMucModel>> LoadLoaiDinhMucData(string searchText)
        {
            return await LoadBlazorTypeaheadData(searchText, LoaiDinhMucService, "filter[_and][][active][_eq]=true");
        }

        private async Task<IEnumerable<PhanLoaiSucKhoeModel>> LoadPhanLoaiSucKhoeData(string searchText)
        {
            return await LoadBlazorTypeaheadData(searchText, PhanLoaiSucKhoaService, "filter[_and][][active][_eq]=true");
        }

        private async Task<IEnumerable<ContractModel>> LoadContractData(string searchText)
        {
            return await LoadBlazorTypeaheadData(searchText, ContractService, "filter[_and][][active][_eq]=true");
        }

        private async Task<IEnumerable<KhamSucKhoeCongTyModel>> LoadKhamSucKhoeCongTyData(string searchText)
        {
            return await LoadBlazorTypeaheadData(searchText, KhamSucKhoeCongTyService, "filter[_and][][active][_eq]=true");
        }

        private async Task<IEnumerable<UserModel>> LoadBacSiData(string searchText)
        {
            try
            {
                var query = "sort=-id";

                query += "&filter[_and][][status][_eq]=active";
                query += "&filter[_and][][role][_eq]=87D650A9-0BD2-41DC-ADF2-B0A248AD9A3B";

                if (!string.IsNullOrEmpty(searchText))
                {
                    query += $"&filter[_and][0][_or][0][first_name][_contains]={Uri.EscapeDataString(searchText)}";
                    query += $"&filter[_and][0][_or][1][last_name][_contains]={Uri.EscapeDataString(searchText)}";
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

        private void OpenDeleteModal(DinhMucModel item)
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
            SelectedItem = new DinhMucModel();
            openDeleteModal = false;
        }

        private void OpenAddOrUpdateModal(DinhMucModel? item)
        {
            _titleAddOrUpdate = item != null ? "Sửa" : "Thêm mới";
            SelectedItem = item != null ? item.DeepClone() : new DinhMucModel();
            _selectedStatusString = SelectedItem.active?.ToString() ?? "True";
            openAddOrUpdateModal = true;
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

        private bool FormValidation()
        {

            if (string.IsNullOrEmpty(SelectedItem.code))
            {
                AlertService.ShowAlert("Mã là bắt buộc", "danger");
                return false;
            }

            if (string.IsNullOrEmpty(SelectedItem.name))
            {
                AlertService.ShowAlert("Tên là bắt buộc", "danger");
                return false;
            }

            if (SelectedItem.loai_dinh_muc == null)
            {
                AlertService.ShowAlert("Loại định mức là bắt buộc", "danger");
                return false;
            }

            return true;
        }

        private void CloseAddOrUpdateModal()
        {
            SelectedItem = new DinhMucModel();
            openAddOrUpdateModal = false;
        }

        private async Task OnStatusFilterChanged(ChangeEventArgs? selected)
        {
            _searchStatusString = selected?.Value?.ToString() ?? string.Empty;

            await LoadData();
        }

        private void OnStatusChanged(ChangeEventArgs? selected)
        {
            _selectedStatusString = selected?.Value?.ToString() ?? string.Empty;
            SelectedItem.active = true;
            if (bool.TryParse(_selectedStatusString, out bool activeValue))
            {
                SelectedItem.active = activeValue;
            }
        }

        private void OnTabChanged(string tab)
        {
            activeDefTab = tab;

            if (activeDefTab == "tab3")
            {
                // Wait for modal to render
                _ = Task.Run(async () =>
                {
                    await Task.Delay(500);
                    await JsRuntime.InvokeVoidAsync("initializeDatePicker");
                });
            }
        }
    }
}
