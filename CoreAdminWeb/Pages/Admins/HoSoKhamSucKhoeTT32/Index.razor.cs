using CoreAdminWeb.Helpers;
using CoreAdminWeb.Model;
using CoreAdminWeb.Model.User;
using CoreAdminWeb.Services.BaseServices;
using CoreAdminWeb.Services.ICaNhanSoKhamSucKhoeService;
using CoreAdminWeb.Services.Users;
using CoreAdminWeb.Shared.Base;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CoreAdminWeb.Pages.Admins.HoSoKhamSucKhoeTT32
{
    public partial class Index(
        ICaNhanSoKhamSucKhoeService<HoSoKhamSucKhoeTT32Model> MainService,
        IBaseService<CongTyModel> CongTyService,
        IBaseService<KhamSucKhoeCongTyModel> KhamSucKhoeCongTyService,
        IUserService UserService
    ) : BlazorCoreBase
    {
        [Parameter] public int? Id { get; set; }
        private List<HoSoKhamSucKhoeTT32Model> MainModels { get; set; } = new();
        private DateTime? _fromDate = null;
        private DateTime? _toDate = null;
        private CongTyModel? _selectedCongTyFilter = null;
        private KhamSucKhoeCongTyModel? _selectedKhamSucKhoeCongTyFilter = null;
        private UserModel? _benhNhanFilter { get; set; } = null;

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
            // var benhNhan = _benhNhanFilter != null ? _benhNhanFilter.id.ToString() : "";
            // var congTy = _selectedCongTyFilter != null ? _selectedCongTyFilter.id.ToString() : "";

            BuilderQuery = $"CaNhan/medical-data?limit={PageSize}&offset={(Page - 1) * PageSize}";
            if (_benhNhanFilter != null)
            {
                BuilderQuery += $"&maBenhNhan={_benhNhanFilter.ma_benh_nhan.ToString()}";
            }
            if (_selectedCongTyFilter != null)
            {
                BuilderQuery += $"&congTy={_selectedCongTyFilter.id.ToString()}";
            }
            if (_fromDate.HasValue)
            {
                BuilderQuery += $"&fromDate={_fromDate.Value.ToString("yyyy-MM-dd")}";
            }
            if (_toDate.HasValue)
            {
                BuilderQuery += $"&toDate={_toDate.Value.ToString("yyyy-MM-dd")}";
            }

            var result = await MainService.GetAllAsync(BuilderQuery);
            if (result.IsSuccess)
            {
                MainModels = result.Data ?? new List<HoSoKhamSucKhoeTT32Model>();
                if (result.Meta != null)
                {
                    TotalItems = result.Meta.filter_count ?? 0;
                    TotalPages = result.Meta.page_count ?? 0;
                }
            }
            else
            {
                MainModels = new List<HoSoKhamSucKhoeTT32Model>();
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

        private async Task<IEnumerable<UserModel>> LoadBenhNhanFilterData(string searchText)
        {
            string queryBenhNhan = "role=patient";
            if (!string.IsNullOrEmpty(searchText))
            {
                queryBenhNhan = $"&filter[_and][][ma_benh_nhan][_eq]={searchText}";
            }
            var res = await UserService.GetAllAsync(queryBenhNhan);
            return res?.IsSuccess == true && res.Data != null ? res.Data : new List<UserModel>();
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

        private async Task OnBenhNhanFilterChanged(UserModel? selected)
        {
            if (selected != null)
            {
                _benhNhanFilter = selected;
            } else
            {
                _benhNhanFilter = null;
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
    }
}
