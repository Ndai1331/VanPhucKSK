using CoreAdminWeb.Commons;
using CoreAdminWeb.Helpers;
using CoreAdminWeb.Model.Dashboard.General;
using CoreAdminWeb.Model.User;
using CoreAdminWeb.Services.IDashboardService;
using CoreAdminWeb.Services.Users;
using CoreAdminWeb.Shared.Base;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace CoreAdminWeb.Pages.Admins.Dashboard
{
    public partial class Index(IDashboardService<GeneralDashboardModel> MainService, IUserService UserService) : BlazorCoreBase
    {
        private GeneralDashboardModel MainModel { get; set; } = new();
        private DateTime? _startDateFilter = default;
        private DateTime? _endDateFilter = default;
        private UserModel? CurrentUser { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                DateTime dateNow = DateTime.Now;
                _startDateFilter = new DateTime(dateNow.Year, dateNow.Month, 1, 0, 0, 0, DateTimeKind.Local);
                _endDateFilter = new DateTime(dateNow.Year, dateNow.Month, DateTime.DaysInMonth(dateNow.Year, dateNow.Month), 0, 0, 0, DateTimeKind.Local);

                var resUser = await UserService.GetCurrentUserAsync();
                if (resUser.IsSuccess)
                {
                    CurrentUser = resUser.Data;
                }
                await LoadData();
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
            if (CurrentUser == null)
            {
                return;
            }

            DateTime dateNow = DateTime.Now;
            if (_startDateFilter.HasValue)
            {
                dateNow = _startDateFilter.Value;
            }

            if (_endDateFilter.HasValue)
            {
                dateNow = _endDateFilter.Value;
            }

            if (_startDateFilter == null)
            {
                _startDateFilter = new DateTime(dateNow.Year, dateNow.Month, 1, 0, 0, 0, DateTimeKind.Local);
            }

            if (_endDateFilter == null)
            {
                _endDateFilter = new DateTime(dateNow.Year, dateNow.Month, DateTime.DaysInMonth(dateNow.Year, dateNow.Month), 0, 0, 0, DateTimeKind.Local);
            }

            IsLoading = true;

            BuilderQuery = $"Dashboard/company-medical-data?company={CurrentUser.ma_don_vi}&fromDate={_startDateFilter:yyyy-MM-dd}&toDate={_endDateFilter:yyyy-MM-dd}";

            var result = await MainService.GetInfomationAsync(BuilderQuery);
            if (result.IsSuccess)
            {
                MainModel = result.Data ?? new GeneralDashboardModel();
                if (result.Meta != null)
                {
                    TotalItems = result.Meta.filter_count ?? 0;
                    TotalPages = (int)Math.Ceiling((double)TotalItems / PageSize);
                }
            }
            else
            {
                MainModel = new GeneralDashboardModel();
            }
            IsLoading = false;

            try
            {
                var phanLoaiucKhoeNames = MainModel.HealthClassifications.Select(c => c.Name).Distinct();
                var phanLoaiucKhoeDates = MainModel.HealthClassifications.Select(c => c.Date.ToString("yyyy-MM")).Distinct();
                var benhs = MainModel.CommonDiseases.OrderByDescending(c => c.Count).Select(c => c.Name).Distinct();

                List<dynamic> timeLinePhanLoaiSucKhoeSeries = new List<dynamic>();
                List<dynamic> phanLoaiSucKhoeSeries = new List<dynamic>();
                List<dynamic> benhPhoBienSeries = new List<dynamic>();
                dynamic phanLoaiSucKhoeSeri = new
                {
                    name = "Phân loại sức khỏe",
                    data = new List<int>()
                };
                dynamic benhSeri = new
                {
                    name = "Nhóm bệnh phổ biến",
                    data = new List<int>()
                };

                foreach (var name in phanLoaiucKhoeNames)
                {
                    dynamic item = new
                    {
                        name = name,
                        data = new List<int>()
                    };

                    foreach (var date in phanLoaiucKhoeDates)
                    {
                        var count = MainModel.HealthClassifications
                            .Where(c => c.Name == name && c.Date.ToString("yyyy-MM") == date)
                            .Sum(c => c.Count);
                        item.data.Add(count);
                    }

                    timeLinePhanLoaiSucKhoeSeries.Add(item);

                    phanLoaiSucKhoeSeri.data.Add(
                        MainModel.HealthClassifications
                            .Where(c => c.Name == name)
                            .Sum(c => c.Count)
                    );
                }


                foreach (var item in benhs)
                {
                    benhSeri.data.Add(
                        MainModel.CommonDiseases
                            .Where(c => c.Name == item)
                            .Sum(c => c.Count)
                    );
                }

                phanLoaiSucKhoeSeries.Add(phanLoaiSucKhoeSeri);
                benhPhoBienSeries.Add(benhSeri);

                await JsRuntime.InvokeVoidAsync("initSimpleBarChart", "#phanLoaiSucKhoeChart", phanLoaiSucKhoeSeries, phanLoaiucKhoeNames, GlobalConstant.PefaultChartColors.Take(phanLoaiucKhoeNames.Count()), false);
                await JsRuntime.InvokeVoidAsync("initLineBarChart", "#timelinePhanLoaiSucKhoeChart", timeLinePhanLoaiSucKhoeSeries, phanLoaiucKhoeDates, GlobalConstant.PefaultChartColors.Take(phanLoaiucKhoeDates.Count()));
                await JsRuntime.InvokeVoidAsync("initSimpleBarChart", "#nhomBenhPhoBienChart", benhPhoBienSeries, benhs, GlobalConstant.PefaultChartColors.Take(benhs.Count()));
            }
            catch (Exception ex)
            {
                AlertService.ShowAlert($"Lỗi khi tải dữ liệu biểu đồ: {ex.Message}", "danger");
            }
        }

        private async Task OnValueChanged(ChangeEventArgs e, string fieldName)
        {
            try
            {
                var dateStr = e.Value?.ToString();
                if (string.IsNullOrEmpty(dateStr))
                {
                    ReflectionHelper.SetFieldValue(this, fieldName, null);
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
                        ReflectionHelper.SetFieldValue(this, fieldName, date);
                    }
                }

                await LoadData();
            }
            catch (Exception ex)
            {
                AlertService.ShowAlert($"Lỗi khi xử lý ngày: {ex.Message}", "danger");
            }
        }
    }
}
