using CoreAdminWeb.Helpers;
using CoreAdminWeb.Model;
using CoreAdminWeb.Model.DanhSachDoan;
using CoreAdminWeb.Services.BaseServices;
// using CoreAdminWeb.Services.DanhSachDoan;
using CoreAdminWeb.Shared.Base;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CoreAdminWeb.Pages.Admins.DanhSachDoanHoSoKhamSucKhoe
{
    public partial class Index(
        // IBaseService<DanhSachDoanModel> MainService,
        // IDanhSachDoanChiTietService DanhSachDoanChiTietService,
        IBaseService<CongTyModel> CongTyService,
        IBaseService<KhamSucKhoeCongTyModel> KhamSucKhoeCongTyService
        // IBaseService<DanhSachDoanTypeModel> DanhSachDoanTypeService,
        // IBaseService<DinhMucModel> DinhMucService,
        // IFileService _fileService
    ) : BlazorCoreBase
    {
        private List<DanhSachDoanModel> MainModels { get; set; } = new();
        private DanhSachDoanModel SelectedItem { get; set; } = new DanhSachDoanModel();
        private DateTime? _fromDate = null;
        private DateTime? _toDate = null;
        private CongTyModel? _selectedCongTyFilter = null;
        private KhamSucKhoeCongTyModel? _selectedKhamSucKhoeCongTyFilter = null;

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
            // IsLoading = true;

            // BuildPaginationQuery(Page, PageSize);
            // BuilderQuery += $"&filter[_and][0][deleted][_eq]=false";
            // if (!string.IsNullOrEmpty(_searchString))
            // {
            //     BuilderQuery += $"&filter[_and][1][_or][0][code][_contains]={_searchString}";
            //     BuilderQuery += $"&filter[_and][1][_or][1][name][_contains]={_searchString}";
            // }
            // if (!string.IsNullOrEmpty(_searchStatusString))
            // {
            //     BuilderQuery += $"&filter[_and][2][status][_eq]={_searchStatusString}";
            // }
            // var result = await MainService.GetAllAsync(BuilderQuery);
            // if (result.IsSuccess)
            // {
            //     MainModels = result.Data ?? new List<DanhSachDoanModel>();
            //     if (result.Meta != null)
            //     {
            //         TotalItems = result.Meta.filter_count ?? 0;
            //         TotalPages = (int)Math.Ceiling((double)TotalItems / PageSize);
            //     }
            // }
            // else
            // {
                MainModels = new List<DanhSachDoanModel>();
            // }
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

        // private async Task LoadDetailData()
        // {
        //     var buildQuery = $"sort=sort";
        //     buildQuery += $"&filter[_and][][danh_sach_doan][_eq]={SelectedItem.id}";
        //     buildQuery += $"&filter[_and][][deleted][_eq]=false";
        //     // var result = await DanhSachDoanChiTietService.GetAllAsync(buildQuery);
        //     // SelectedItemsDetail = result.Data ?? new List<DanhSachDoanChiTietModel>();
        //     SelectedItemsDetail = new List<DanhSachDoanChiTietModel>();
        // }

        private async Task<IEnumerable<CongTyModel>> LoaCongTyFilterData(string searchText)
        {
            return await LoadBlazorTypeaheadData(searchText, CongTyService);
        }

        private async Task<IEnumerable<KhamSucKhoeCongTyModel>> LoadKhamSucKhoeCongTyData(string searchText)
        {
            return await LoadBlazorTypeaheadData(searchText, KhamSucKhoeCongTyService);
        }

        private async Task OnCongTyFilterChanged(CongTyModel? selected)
        {
            if (selected != null)
            {
                _selectedCongTyFilter = selected;
            } else
            {
                _selectedCongTyFilter = null;
            }

            await LoadData();
        }

        private async Task OnKhamSucKhoeCongTyFilterChanged(KhamSucKhoeCongTyModel? selected)
        {
            if (selected != null)
            {
                _selectedKhamSucKhoeCongTyFilter = selected;
            } else
            {
                _selectedKhamSucKhoeCongTyFilter = null;
            }

            await LoadData();
        }

        private async Task OnDateChanged(ChangeEventArgs e, string fieldName, bool isFilter = false)
        {
            try
            {
                var dateStr = e.Value?.ToString();
                if (string.IsNullOrEmpty(dateStr))
                {
                    ReflectionHelper.SetDateFieldValue(this, SelectedItem, fieldName, null);
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
                        ReflectionHelper.SetDateFieldValue(this, SelectedItem, fieldName, date);
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
    }
}