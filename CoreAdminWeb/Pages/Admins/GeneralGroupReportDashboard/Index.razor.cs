using CoreAdminWeb.Commons;
using CoreAdminWeb.Helpers;
using CoreAdminWeb.Model;
using CoreAdminWeb.Model.Dashboard.General;
using CoreAdminWeb.Services;
using CoreAdminWeb.Services.BaseServices;
using CoreAdminWeb.Services.IDashboardService;
using CoreAdminWeb.Shared.Base;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CoreAdminWeb.Pages.Admins.GeneralGroupReportDashboard
{
    public partial class Index(IDashboardService<CompanySummaryReportDashboardModel> MainService,
                               IBaseService<KhamSucKhoeCongTyModel> KhamSucKhoeCongTyService) : BlazorCoreBase
    {
        private CompanySummaryReportDashboardModel MainModel { get; set; } = new();
        private DateTime? _startDateFilter = default;
        private DateTime? _endDateFilter = default;
        private KhamSucKhoeCongTyModel? _doanKhamFilter { get; set; } = default;

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

            BuilderQuery = $"Dashboard/company-summary-report?companyHelthCheckId={_doanKhamFilter?.id}&fromDate={_startDateFilter:yyyy-MM-dd}&toDate={_endDateFilter:yyyy-MM-dd}";

            var result = await MainService.GeDataAsync(BuilderQuery);
            MainModel = result.Data ?? new CompanySummaryReportDashboardModel();

            try
            {
                List<dynamic> theoDoiChiPhiDoanKhamSeries = new List<dynamic>();
                List<string> theoDoiChiPhiDoanKhamLabels = new List<string>();

                var hopDongKhamSucKhoes = MainModel.Revenues
                    .GroupBy(c => c.MaHopDong)
                    .Select(g => new
                    {
                        MaHopDong = g.Key,
                        GiaTriHopDong = g.First().GiaTriHopDong,
                        ChiPhiDuKien = g.Sum(c => c.ChiPhiDuKien),
                        ChiPhiThucTe = g.Sum(c => c.ChiPhiThucTe)
                    }).ToList();

                for (int i = 0; i < 3; i++)
                {
                    dynamic seriData = new
                    {
                        name = i switch
                        {
                            0 => "Giá trị hợp đồng",
                            1 => "Chi phí dự kiến",
                            _ => "Chi phí thực tế"
                        },
                        data = new List<decimal>()
                    };

                    foreach (var item in hopDongKhamSucKhoes)
                    {
                        seriData.data.Add(i switch
                        {
                            0 => item.GiaTriHopDong,
                            1 => item.ChiPhiDuKien,
                            _ => item.ChiPhiThucTe
                        });

                        if (!theoDoiChiPhiDoanKhamLabels.Any(c => c.Equals(item.MaHopDong)))
                        {
                            theoDoiChiPhiDoanKhamLabels.Add(item.MaHopDong);
                        }
                    }

                    theoDoiChiPhiDoanKhamSeries.Add(seriData);
                }


                await JsRuntime.InvokeVoidAsync("initSimpleBarChart", "#theoDoiChiPhiDoanKhamChart", theoDoiChiPhiDoanKhamSeries, theoDoiChiPhiDoanKhamLabels, GlobalConstant.PefaultChartColors.Take(3), false, true);
            }
            catch (Exception ex)
            {
                AlertService.ShowAlert($"Lỗi khi tải dữ liệu biểu đồ: {ex.Message}", "danger");
            }


            //try
            //{
            //    var phanLoaiucKhoeNames = MainModel.HealthClassifications.Select(c => c.Name).Distinct();
            //    var phanLoaiucKhoeDates = MainModel.HealthClassifications.Select(c => c.Date.ToString("yyyy-MM")).Distinct();
            //    var benhs = MainModel.CommonDiseases.OrderByDescending(c => c.Count).Select(c => c.Name).Distinct();

            //    List<dynamic> timeLinePhanLoaiSucKhoeSeries = new List<dynamic>();
            //    List<dynamic> phanLoaiSucKhoeSeries = new List<dynamic>();
            //    List<dynamic> benhPhoBienSeries = new List<dynamic>();
            //    dynamic phanLoaiSucKhoeSeri = new
            //    {
            //        name = "Phân loại sức khỏe",
            //        data = new List<int>()
            //    };
            //    dynamic benhSeri = new
            //    {
            //        name = "Nhóm bệnh phổ biến",
            //        data = new List<int>()
            //    };

            //    foreach (var name in phanLoaiucKhoeNames)
            //    {
            //        dynamic item = new
            //        {
            //            name = name,
            //            data = new List<int>()
            //        };

            //        foreach (var date in phanLoaiucKhoeDates)
            //        {
            //            var count = MainModel.HealthClassifications
            //                .Where(c => c.Name == name && c.Date.ToString("yyyy-MM") == date)
            //                .Sum(c => c.Count);
            //            item.data.Add(count);
            //        }

            //        timeLinePhanLoaiSucKhoeSeries.Add(item);

            //        phanLoaiSucKhoeSeri.data.Add(
            //            MainModel.HealthClassifications
            //                .Where(c => c.Name == name)
            //                .Sum(c => c.Count)
            //        );
            //    }


            //    foreach (var item in benhs)
            //    {
            //        benhSeri.data.Add(
            //            MainModel.CommonDiseases
            //                .Where(c => c.Name == item)
            //                .Sum(c => c.Count)
            //        );
            //    }

            //    phanLoaiSucKhoeSeries.Add(phanLoaiSucKhoeSeri);
            //    benhPhoBienSeries.Add(benhSeri);

            //    await JsRuntime.InvokeVoidAsync("initSimpleBarChart", "#phanLoaiSucKhoeChart", phanLoaiSucKhoeSeries, phanLoaiucKhoeNames, GlobalConstant.PefaultChartColors.Take(phanLoaiucKhoeNames.Count()), false);
            //    await JsRuntime.InvokeVoidAsync("initLineBarChart", "#timelinePhanLoaiSucKhoeChart", timeLinePhanLoaiSucKhoeSeries, phanLoaiucKhoeDates, GlobalConstant.PefaultChartColors.Take(phanLoaiucKhoeDates.Count()));
            //    await JsRuntime.InvokeVoidAsync("initSimpleBarChart", "#nhomBenhPhoBienChart", benhPhoBienSeries, benhs, GlobalConstant.PefaultChartColors.Take(benhs.Count()));
            //}
            //catch (Exception ex)
            //{
            //    AlertService.ShowAlert($"Lỗi khi tải dữ liệu biểu đồ: {ex.Message}", "danger");
            //}
            IsLoading = false;
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

        private async Task OnDoanKhamFilterChanged(KhamSucKhoeCongTyModel? selected)
        {
            _doanKhamFilter = selected;
            await LoadData();
        }

        private async Task<IEnumerable<KhamSucKhoeCongTyModel>> LoadKhamSucKhoeCongTyData(string searchText)
        {
            return await LoadBlazorTypeaheadData(searchText, KhamSucKhoeCongTyService);
        }
    }
}
