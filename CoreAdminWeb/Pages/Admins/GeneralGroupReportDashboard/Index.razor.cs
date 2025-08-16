using CoreAdminWeb.Commons;
using CoreAdminWeb.Helpers;
using CoreAdminWeb.Model.Dashboard.General;
using CoreAdminWeb.Model.KhamSucKhoes;
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

        private List<dynamic>? theoDoiChiPhiDoanKhamSeries { get; set; }
        private List<string>? theoDoiChiPhiDoanKhamLabels { get; set; }
        private List<dynamic>? theoDoiNhomChiPhiSeries { get; set; }
        private List<string>? theoDoiNhomChiPhiLabels { get; set; }
        private List<decimal>? phanBoChiPhiTheoNhomDinhMucSeries { get; set; }
        private List<string>? phanBoChiPhiTheoNhomDinhMucLabels { get; set; }
        private List<dynamic>? theoDoiLoiNhuanHopDongSeries { get; set; }
        private List<string>? theoDoiLoiNhuanHopDongLabels { get; set; }
        private List<dynamic>? dienTienLuotKhamTheoThoiGianSeries { get; set; }
        private List<string>? dienTienLuotKhamTheoThoiGianLabels { get; set; }
        private List<dynamic>? soNguoiKhamTheoDonViSeries { get; set; }
        private List<string>? soNguoiKhamTheoDonViLabels { get; set; }

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

            var chartTasks = new[]
            {
                OnLoadTheoDoiChiPhiDoanKhamChart(),
                OnLoadTheoDoiNhomChiPhiChart(),
                OnLoadPhanBoChiPhiTheoNhomDinhMucChart(),
                OnLoadTheoDoiLoiNhuanHopDongChart(),
                OnLoadDienTienLuotKhamTheoThoiGianChart(),
                OnLoadSoNguoiKhamTheoDonViChart()
            };
            await Task.WhenAll(chartTasks);

            IsLoading = false;
        }

        private async Task OnLoadTheoDoiChiPhiDoanKhamChart()
        {
            try
            {
                theoDoiChiPhiDoanKhamSeries = new List<dynamic>();
                theoDoiChiPhiDoanKhamLabels = new List<string>();

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
        }

        private async Task OnLoadTheoDoiNhomChiPhiChart()
        {
            try
            {
                theoDoiNhomChiPhiSeries = new List<dynamic>();
                theoDoiNhomChiPhiLabels = new List<string>();

                var nhomChiPhis = MainModel.Revenues
                    .GroupBy(c => c.DinhMuc)
                    .Select(g => new
                    {
                        NhomChiPhi = g.Key,
                        ChiPhiDuKien = g.Sum(c => c.ChiPhiDuKien),
                        ChiPhiThucTe = g.Sum(c => c.ChiPhiThucTe)
                    }).ToList();

                for (int i = 0; i < 2; i++)
                {
                    dynamic seriData = new
                    {
                        name = i switch
                        {
                            0 => "Chi phí dự kiến",
                            _ => "Chi phí thực tế"
                        },
                        data = new List<decimal>()
                    };

                    foreach (var item in nhomChiPhis)
                    {
                        seriData.data.Add(i switch
                        {
                            0 => item.ChiPhiDuKien,
                            _ => item.ChiPhiThucTe
                        });

                        if (!theoDoiNhomChiPhiLabels.Any(c => c.Equals(item.NhomChiPhi)))
                        {
                            theoDoiNhomChiPhiLabels.Add(item.NhomChiPhi);
                        }
                    }

                    theoDoiNhomChiPhiSeries.Add(seriData);
                }

                await JsRuntime.InvokeVoidAsync("initLineChart", "#theoDoiCacNhomChiPhiChart", theoDoiNhomChiPhiSeries, theoDoiNhomChiPhiLabels, GlobalConstant.PefaultChartColors.Take(2), false, true);
            }
            catch (Exception ex)
            {
                AlertService.ShowAlert($"Lỗi khi tải dữ liệu biểu đồ: {ex.Message}", "danger");
            }
        }

        private async Task OnLoadPhanBoChiPhiTheoNhomDinhMucChart()
        {
            try
            {
                phanBoChiPhiTheoNhomDinhMucSeries = new List<decimal>();
                phanBoChiPhiTheoNhomDinhMucLabels = new List<string>();

                var nhomChiPhis = MainModel.Revenues
                    .GroupBy(c => c.DinhMuc)
                    .Select(g => new
                    {
                        NhomChiPhi = g.Key,
                        ChiPhiDuKien = g.Sum(c => c.ChiPhiDuKien),
                        ChiPhiThucTe = g.Sum(c => c.ChiPhiThucTe)
                    }).ToList();

                foreach (var item in nhomChiPhis)
                {
                    phanBoChiPhiTheoNhomDinhMucSeries.Add(item.ChiPhiThucTe);

                    if (!phanBoChiPhiTheoNhomDinhMucLabels.Any(c => c.Equals(item.NhomChiPhi)))
                    {
                        phanBoChiPhiTheoNhomDinhMucLabels.Add(item.NhomChiPhi);
                    }
                }

                await JsRuntime.InvokeVoidAsync("initPieChart", "#phanBoChiPhiTheoNhomDinhMucChart", phanBoChiPhiTheoNhomDinhMucSeries, phanBoChiPhiTheoNhomDinhMucLabels, GlobalConstant.PefaultChartColors.Take(phanBoChiPhiTheoNhomDinhMucLabels.Count), false, true);
            }
            catch (Exception ex)
            {
                AlertService.ShowAlert($"Lỗi khi tải dữ liệu biểu đồ: {ex.Message}", "danger");
            }
        }

        private async Task OnLoadTheoDoiLoiNhuanHopDongChart()
        {
            try
            {
                theoDoiLoiNhuanHopDongSeries = new List<dynamic>();
                theoDoiLoiNhuanHopDongLabels = new List<string>();

                var hopDongKhamSucKhoes = MainModel.Revenues
                    .GroupBy(c => c.MaHopDong)
                    .Select(g => new
                    {
                        MaHopDong = g.Key,
                        LoiNhuan = g.First().GiaTriHopDong - g.Sum(c => c.ChiPhiThucTe)
                    }).ToList();

                dynamic seriData = new
                {
                    name = "Lợi nhuận",
                    data = new List<decimal>()
                };

                foreach (var item in hopDongKhamSucKhoes)
                {
                    seriData.data.Add(item.LoiNhuan);

                    if (!theoDoiLoiNhuanHopDongLabels.Any(c => c.Equals(item.MaHopDong)))
                    {
                        theoDoiLoiNhuanHopDongLabels.Add(item.MaHopDong);
                    }
                }

                theoDoiLoiNhuanHopDongSeries.Add(seriData);

                await JsRuntime.InvokeVoidAsync("initSimpleBarChart", "#theoDoiLoiNhuanHopDongChart", theoDoiLoiNhuanHopDongSeries, theoDoiLoiNhuanHopDongLabels, GlobalConstant.PefaultChartColors.Take(theoDoiLoiNhuanHopDongLabels.Count), false, true);
            }
            catch (Exception ex)
            {
                AlertService.ShowAlert($"Lỗi khi tải dữ liệu biểu đồ: {ex.Message}", "danger");
            }
        }

        private async Task OnLoadDienTienLuotKhamTheoThoiGianChart()
        {
            try
            {
                dienTienLuotKhamTheoThoiGianSeries = new List<dynamic>();
                dienTienLuotKhamTheoThoiGianLabels = new List<string>();

                var nhomChiPhis = MainModel.NoteSummaries
                    .GroupBy(c => c.NgayKham?.Date)
                    .Select(g => new
                    {
                        NgayKham = g.Key?.ToString("dd/MM/yy") ?? "Unknown",
                        Count = g.Sum(c => c.Count)
                    }).ToList();

                dynamic seriData = new
                {
                    name = "Lượt khám",
                    data = new List<decimal>()
                };

                foreach (var item in nhomChiPhis)
                {
                    seriData.data.Add(item.Count);

                    if (!dienTienLuotKhamTheoThoiGianLabels.Any(c => c.Equals(item.NgayKham)))
                    {
                        dienTienLuotKhamTheoThoiGianLabels.Add(item.NgayKham);
                    }
                }

                dienTienLuotKhamTheoThoiGianSeries.Add(seriData);

                await JsRuntime.InvokeVoidAsync("initAreaChart", "#dienTienLuotKhamTheoThoiGianChart", dienTienLuotKhamTheoThoiGianSeries, dienTienLuotKhamTheoThoiGianLabels, GlobalConstant.PefaultChartColors.Take(1), false, true);
            }
            catch (Exception ex)
            {
                AlertService.ShowAlert($"Lỗi khi tải dữ liệu biểu đồ: {ex.Message}", "danger");
            }
        }

        private async Task OnLoadSoNguoiKhamTheoDonViChart()
        {
            try
            {
                soNguoiKhamTheoDonViSeries = new List<dynamic>();
                soNguoiKhamTheoDonViLabels = new List<string>();

                var hopDongKhamSucKhoes = MainModel.NoteSummaries
                    .GroupBy(c => c.MaDonVi)
                    .Select(g => new
                    {
                        MaDonVi = g.Key,
                        Count = g.Sum(c => c.Count)
                    }).ToList();

                dynamic seriData = new
                {
                    name = "Số lượt khám",
                    data = new List<decimal>()
                };

                foreach (var item in hopDongKhamSucKhoes)
                {
                    seriData.data.Add(item.Count);

                    if (!soNguoiKhamTheoDonViLabels.Any(c => c.Equals(item.MaDonVi)))
                    {
                        soNguoiKhamTheoDonViLabels.Add(item.MaDonVi);
                    }
                }

                soNguoiKhamTheoDonViSeries.Add(seriData);

                await JsRuntime.InvokeVoidAsync("initSimpleBarChart", "#soNguoiKhamTheoDonViChart", soNguoiKhamTheoDonViSeries, soNguoiKhamTheoDonViLabels, GlobalConstant.PefaultChartColors.Take(soNguoiKhamTheoDonViLabels.Count), false, true);
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
