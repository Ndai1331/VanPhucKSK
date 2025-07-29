using CoreAdminWeb.Helpers;
using CoreAdminWeb.Model;
using CoreAdminWeb.Services.BaseServices;
using CoreAdminWeb.Services.IDanhSachDoanSoKhamSucKhoeService;
using CoreAdminWeb.Shared.Base;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CoreAdminWeb.Pages.Admins.DanhSachDoanSoKhamSucKhoe
{
    public partial class Index(
        IDanhSachDoanSoKhamSucKhoeService<DanhSachDoanSoKhamSucKhoeModel> MainService,
        IBaseService<CongTyModel> CongTyService,
        IBaseService<KhamSucKhoeCongTyModel> KhamSucKhoeCongTyService
    ) : BlazorCoreBase
    {
        [Parameter] public int? Id { get; set; }
        private List<DanhSachDoanSoKhamSucKhoeModel> MainModels { get; set; } = new();
        private DateTime? _fromDate = null;
        private DateTime? _toDate = null;
        private CongTyModel? _selectedCongTyFilter = null;
        private KhamSucKhoeCongTyModel? _selectedKhamSucKhoeCongTyFilter = null;

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            
            // Load the KhamSucKhoeCongTy by ID if provided
            if (Id.HasValue)
            {
                await LoadKhamSucKhoeCongTyById(Id.Value);
            }
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
            var maDotKham = 0;
            if (_selectedKhamSucKhoeCongTyFilter != null)
            {
                maDotKham = _selectedKhamSucKhoeCongTyFilter.id;
            }
            BuilderQuery = $"DanhSachDoan/medical-data?maDotKham={maDotKham}&limit={PageSize}&offset={(Page - 1) * PageSize}";

            // BuildPaginationQuery(Page, PageSize);
            // BuilderQuery += $"&filter[_and][0][deleted][_eq]=false";

            // if (_fromDate.HasValue)
            // {
            //     var fromDate = _fromDate.Value.ToString("yyyy-MM-dd");
            //     BuilderQuery += $"&filter[_and][][ngay_kham][_gte]={fromDate}";
            // }
            // if (_toDate.HasValue)
            // {
            //     var toDate = _toDate.Value.ToString("yyyy-MM-dd");
            //     BuilderQuery += $"&filter[_and][][ngay_kham][_lte]={toDate}";
            // }
            // if (_selectedCongTyFilter != null)
            // {
            //     BuilderQuery += $"&filter[_and][][MaDotKham][ma_hop_dong_ksk][cong_ty][_eq]={_selectedCongTyFilter.id}";
            // }
            // if (_selectedKhamSucKhoeCongTyFilter != null)
            // {
            //     BuilderQuery += $"&filter[_and][][MaDotKham][_eq]={_selectedKhamSucKhoeCongTyFilter.id}";
            // }

            var result = await MainService.GetAllAsync(BuilderQuery);
            if (result.IsSuccess)
            {
                MainModels = result.Data ?? new List<DanhSachDoanSoKhamSucKhoeModel>();
                if (result.Meta != null)
                {
                    TotalItems = result.Meta.filter_count ?? 0;
                    TotalPages = (int)Math.Ceiling((double)TotalItems / PageSize);
                }
            }
            else
            {
                MainModels = new List<DanhSachDoanSoKhamSucKhoeModel>();
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
                    switch (fieldName)
                    {
                        case "_fromDate":
                            _fromDate = null;
                            break;

                        case "_toDate":
                            _toDate = null;
                            break;
                    }
                    return;
                }

                var parts = dateStr.Split('/');
                if (parts.Length == 3 &&
                    int.TryParse(parts[0], out int day) &&
                    int.TryParse(parts[1], out int month) &&
                    int.TryParse(parts[2], out int year))
                {
                    var date = new DateTime(year, month, day);

                    switch (fieldName)
                    {
                        case "_fromDate":
                            _fromDate = date;
                            break;

                        case "_toDate":
                            _toDate = date;
                            break;
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

        private async Task LoadKhamSucKhoeCongTyById(int id)
        {
            try
            {
                var result = await KhamSucKhoeCongTyService.GetByIdAsync(id.ToString());
                if (result?.IsSuccess == true && result.Data != null)
                {
                    _selectedKhamSucKhoeCongTyFilter = result.Data;
                }
            }
            catch (Exception ex)
            {
                AlertService.ShowAlert($"Lỗi khi tải dữ liệu: {ex.Message}", "danger");
            }
        }
    }
}
