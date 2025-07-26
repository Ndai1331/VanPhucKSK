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
    public partial class Index(IBaseService<SoKhamSucKhoeModel> MainService,
                               IBaseService<PhanLoaiSucKhoeModel> PhanLoaiSucKhoaService,
                               IUserService UserService,
                               IBaseService<ContractModel> ContractService,
                               IBaseService<KhamSucKhoeCongTyModel> KhamSucKhoeCongTyService) : BlazorCoreBase
    {
        private List<SoKhamSucKhoeModel> MainModels { get; set; } = new();
        private string activeDefTab = "tab1";

        private SoKhamSucKhoeModel SelectedItem { get; set; } = new SoKhamSucKhoeModel();

        private DateTime? _startDateFilter = default;
        private DateTime? _endDateFilter = default;
        private ContractModel? _contractFilter = default;
        private KhamSucKhoeCongTyModel? _khamSucKhoeCongTyFilter = default;
        private string _maDieuTriString = "";
        private string _maBenhNhanString = "";
        private string _tenBenhNhanString = "";

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
            if (!string.IsNullOrEmpty(_tenBenhNhanString))
            {
                BuilderQuery += $"&filter[_and][1][_or][0][benh_nhan][first_name][_contains]={_tenBenhNhanString}";
                BuilderQuery += $"&filter[_and][1][_or][1][benh_nhan][last_name][_contains]={_tenBenhNhanString}";
            }
            if (!string.IsNullOrEmpty(_maBenhNhanString))
            {
                BuilderQuery += $"&filter[_and][][ma_benh_nhan][_contains]={_maBenhNhanString}";
            }
            if (!string.IsNullOrEmpty(_maDieuTriString))
            {
                BuilderQuery += $"&filter[_and][][ma_luot_kham][_contains]={_maDieuTriString}";
            }
            if (_startDateFilter.HasValue)
            {
                BuilderQuery += $"&filter[_and][][MaDotKham][ngay_du_kien_kham][_gte]={_startDateFilter:yyyy-MM-dd}";
            }
            if (_endDateFilter.HasValue)
            {
                BuilderQuery += $"&filter[_and][][MaDotKham][ngay_du_kien_kham][_lte]={_endDateFilter:yyyy-MM-dd}";
            }
            if (_khamSucKhoeCongTyFilter != null && _khamSucKhoeCongTyFilter.id > 0)
            {
                BuilderQuery += $"&filter[_and][][MaDotKham][_eq]={_khamSucKhoeCongTyFilter.id}";
            }
            if (_contractFilter != null && _contractFilter.id > 0)
            {
                BuilderQuery += $"&filter[_and][][MaDotKham][ma_hop_dong_ksk][_eq]={_contractFilter.id}";
            }
            var result = await MainService.GetAllAsync(BuilderQuery);
            if (result.IsSuccess)
            {
                MainModels = result.Data ?? new List<SoKhamSucKhoeModel>();
                if (result.Meta != null)
                {
                    TotalItems = result.Meta.filter_count ?? 0;
                    TotalPages = (int)Math.Ceiling((double)TotalItems / PageSize);
                }
            }
            else
            {
                MainModels = new List<SoKhamSucKhoeModel>();
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

            return true;
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

        private async Task OnValueChanged(ChangeEventArgs e, string fieldName, bool isFilter = false, bool isDate = true)
        {
            try
            {
                if (!isDate)
                {
                    if (e.Value == null || string.IsNullOrEmpty(e.Value.ToString()))
                    {
                        ReflectionHelper.SetFieldValue(this, SelectedItem, fieldName, null);
                        return;
                    }
                    var value = e.Value.ToString();
                    ReflectionHelper.SetFieldValue(this, SelectedItem, fieldName, value);
                    return;
                }

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

        private async Task ContractFilterChanged(ContractModel? contract)
        {
            _contractFilter = contract;

            await LoadData();
        }

        private async Task KhamSucKhoeCongTyChanged(KhamSucKhoeCongTyModel? khamSucKhoeCongTy)
        {
            _khamSucKhoeCongTyFilter = khamSucKhoeCongTy;

            await LoadData();
        }
    }
}
