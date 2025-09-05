using CoreAdminWeb.Enums;
using CoreAdminWeb.Helpers;
using CoreAdminWeb.Model;
using CoreAdminWeb.Model.Contract;
using CoreAdminWeb.Services;
using CoreAdminWeb.Services.BaseServices;
using CoreAdminWeb.Shared.Base;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CoreAdminWeb.Pages.Admins.BaoCaoThoiGianThuHienHopDong
{
    public partial class Index(
        IBaseService<ContractModel> MainService,
        IBaseService<CongTyModel> CongTyService,
        IExportExcelService<dynamic> ExportExcelService
    ) : BlazorCoreBase
    {
        [Parameter] public int? Id { get; set; }
        private List<TrangThaiHopDong> TrangThaiHopDongList { get; set; } = Enum.GetValues(typeof(TrangThaiHopDong)).Cast<TrangThaiHopDong>().ToList();
        private List<ContractModel> MainModels { get; set; } = new();
        private string _searchString = "";
        private string _searchStatusString = "";
        private DateTime? _fromDate = null;
        private DateTime? _toDate = null;
        private CongTyModel? _selectedCongTyFilter = null;

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await LoadData(true);
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

        private async Task LoadData(bool isResetPage = false)
        {
            Loading.Show();

            if (isResetPage)
            {
                ResetPage();
            }

            BuildPaginationQuery(Page, PageSize);
            BuilderQuery += $"&filter[_and][0][deleted][_eq]=false";
            if (!string.IsNullOrEmpty(_searchString))
            {
                BuilderQuery += $"&filter[_and][1][_or][0][code][_contains]={_searchString}";
                BuilderQuery += $"&filter[_and][1][_or][1][name][_contains]={_searchString}";
            }
            if (!string.IsNullOrEmpty(_searchStatusString))
            {
                BuilderQuery += $"&filter[_and][2][status][_eq]={_searchStatusString}";
            }
            if (_fromDate.HasValue)
            {
                var fromDate = _fromDate.Value.ToString("yyyy-MM-dd");
                BuilderQuery += $"&filter[_and][][ngay_hop_dong][_gte]={fromDate}";
            }
            if (_toDate.HasValue)
            {
                var toDate = _toDate.Value.ToString("yyyy-MM-dd");
                BuilderQuery += $"&filter[_and][][ngay_hop_dong][_lte]={toDate}";
            }
            if (_selectedCongTyFilter != null)
            {
                BuilderQuery += $"&filter[_and][][cong_ty][_eq]={_selectedCongTyFilter.id}";
            }

            var result = await MainService.GetAllAsync(BuilderQuery);
            if (result.IsSuccess)
            {
                MainModels = result.Data ?? new List<ContractModel>();
                if (result.Meta != null)
                {
                    TotalItems = result.Meta.filter_count ?? 0;
                    TotalPages = (int)Math.Ceiling((double)TotalItems / PageSize);

                    if (Page > TotalPages)
                    {
                        await SelectedPage(TotalPages);
                    }
                }
            }
            else
            {
                MainModels = new List<ContractModel>();
            }
            Loading.Hide();
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

        private async Task OnStatusFilterChanged(ChangeEventArgs? selected)
        {
            _searchStatusString = selected?.Value?.ToString() ?? string.Empty;

            await LoadData(true);
        }

        private async Task OnCongTyFilterChanged(CongTyModel? selected)
        {
            _selectedCongTyFilter = selected;

            await LoadData(true);
        }

        private async Task OnDateChanged(ChangeEventArgs e, string fieldName, bool isFilter = false)
        {
            try
            {
                var dateStr = e.Value?.ToString();
                var olderData = ReflectionHelper.GetFieldValue(this, fieldName);

                DateTime? newDate = null;
                if (!string.IsNullOrEmpty(dateStr))
                {
                    var parts = dateStr.Split('/');
                    if (parts.Length == 3 &&
                        int.TryParse(parts[0], out int day) &&
                        int.TryParse(parts[1], out int month) &&
                        int.TryParse(parts[2], out int year))
                    {
                        newDate = new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Local);
                    }
                }

                // Compare old and new value, do nothing if unchanged
                if ((olderData == null && newDate == null) ||
                    (olderData is DateTime oldDate && newDate.HasValue && oldDate == newDate.Value))
                {
                    return;
                }

                ReflectionHelper.SetFieldValue(this, fieldName, newDate);

                if (isFilter && !Loading.IsBusy)
                {
                    await LoadData(true);
                }
            }
            catch (Exception ex)
            {
                AlertService.ShowAlert($"Lỗi khi xử lý ngày: {ex.Message}", "danger");
            }
        }

        private async Task OnExcelExport()
        {
            try
            {
                Loading.Show();

                BuildPaginationQuery(1, int.MaxValue);
                BuilderQuery += $"&filter[_and][0][deleted][_eq]=false";
                if (!string.IsNullOrEmpty(_searchString))
                {
                    BuilderQuery += $"&filter[_and][1][_or][0][code][_contains]={_searchString}";
                    BuilderQuery += $"&filter[_and][1][_or][1][name][_contains]={_searchString}";
                }
                if (!string.IsNullOrEmpty(_searchStatusString))
                {
                    BuilderQuery += $"&filter[_and][2][status][_eq]={_searchStatusString}";
                }
                if (_fromDate.HasValue)
                {
                    var fromDate = _fromDate.Value.ToString("yyyy-MM-dd");
                    BuilderQuery += $"&filter[_and][][ngay_hop_dong][_gte]={fromDate}";
                }
                if (_toDate.HasValue)
                {
                    var toDate = _toDate.Value.ToString("yyyy-MM-dd");
                    BuilderQuery += $"&filter[_and][][ngay_hop_dong][_lte]={toDate}";
                }
                if (_selectedCongTyFilter != null)
                {
                    BuilderQuery += $"&filter[_and][][cong_ty][_eq]={_selectedCongTyFilter.id}";
                }

                var fields = new List<string>()
                {
                    "stt",
                    "cong_ty",
                    "code",
                    "ngay_hop_dong",
                    "ngay_het_han",
                    "ngay_nghiem_thu_hd",
                    "ngay_thanh_toan",
                    "ngay_du_kien_kham",
                    "ngay_ket_thuc",
                    "so_sanh_kham_nghiem_thu",
                    "so_sanh_kham_thanh_toan",
                };
                var labels = new List<string>()
                {
                    "STT",
                    "Công ty",
                    "Mã hợp đồng",
                    "Ngày ký",
                    "Ngày hết hạn HĐ",
                    "Ngày nghiệm thu HĐ",
                    "Ngày thanh toán",
                    "Ngày dự kiến khám",
                    "Ngày kết thúc",
                    "So sánh khám - nghiệm thu (ngày)",
                    "So sánh khám - thanh toán (ngày)",
                };

                var result = await MainService.GetAllAsync(BuilderQuery);
                if (result.IsSuccess)
                {
                    var prepareData = result.Data?.Select(item =>
                    {
                        var ngayDuKienKham = item.cau_hinh_ho_so_ksk?
                            .OrderByDescending(x => x.ngay_du_kien_kham)
                            .FirstOrDefault()?.ngay_du_kien_kham;

                        var ngayNghiemThu = item.ngay_nghiem_thu_hd ?? DateTime.Now;
                        var ngayThanhToan = item.ngay_thanh_toan ?? DateTime.Now;

                        int soSanhKhamNghiemThu = 0;
                        int soSanhKhamThanhToan = 0;
                        if (ngayDuKienKham.HasValue)
                        {
                            soSanhKhamNghiemThu = (ngayNghiemThu - ngayDuKienKham.Value).Days;
                        }
                        if (ngayDuKienKham.HasValue)
                        {
                            soSanhKhamThanhToan = (ngayThanhToan - ngayDuKienKham.Value).Days;
                        }

                        return (dynamic)new
                        {
                            stt = ((Page - 1) * PageSize) + result.Data.IndexOf(item) + 1,
                            cong_ty = item.cong_ty?.name,
                            code = item.code,
                            ngay_hop_dong = item.ngay_hop_dong?.ToString("dd/MM/yyyy"),
                            ngay_het_han = item.ngay_het_han?.ToString("dd/MM/yyyy"),
                            ngay_nghiem_thu_hd = item.ngay_nghiem_thu_hd?.ToString("dd/MM/yyyy"),
                            ngay_thanh_toan = item.ngay_thanh_toan?.ToString("dd/MM/yyyy"),
                            ngay_du_kien_kham = ngayDuKienKham?.ToString("dd/MM/yyyy"),
                            ngay_ket_thuc = item.cau_hinh_ho_so_ksk?.OrderByDescending(x => x.ngay_ket_thuc).FirstOrDefault()?.ngay_ket_thuc?.ToString("dd/MM/yyyy"),
                            so_sanh_kham_nghiem_thu = soSanhKhamNghiemThu,
                            so_sanh_kham_thanh_toan = soSanhKhamThanhToan,
                        };
                    }).ToList() ?? new List<dynamic>();

                    var fileBytes = await ExportExcelService.ExportToExcelAsync(prepareData, fields, labels);

                    var fileName = $"{"BaoCaoThoiGianThuHienHopDong"}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                    await JsRuntime.InvokeVoidAsync("saveAsFile", fileName, Convert.ToBase64String(fileBytes));
                }
                else
                {
                    AlertService.ShowAlert(result.Message ?? "Lỗi khi lấy dữ liệu để xuất excel", "danger");
                }
            }
            catch
            {
                AlertService.ShowAlert("Lỗi khi xuất excel", "danger");
            }
            finally
            {
                Loading.Hide();
            }
        }
    }
}
