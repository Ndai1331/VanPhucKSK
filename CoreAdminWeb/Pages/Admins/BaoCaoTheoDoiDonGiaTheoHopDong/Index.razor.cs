using CoreAdminWeb.Helpers;
using CoreAdminWeb.Model;
using CoreAdminWeb.Model.Contract;
using CoreAdminWeb.Model.Reports;
using CoreAdminWeb.Services;
using CoreAdminWeb.Services.BaseServices;
using CoreAdminWeb.Services.Reports;
using CoreAdminWeb.Shared.Base;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CoreAdminWeb.Pages.Admins.BaoCaoTheoDoiDonGiaTheoHopDong
{
    public partial class Index(
        IReportService<ReportBaoCaoTheoDoiDonGiaTheoHopDongModel> MainService,
        IBaseService<CongTyModel> CongTyService,
        IBaseService<ContractModel> ContractService,
        IExportExcelService<dynamic> ExportExcelService
    ) : BlazorCoreBase
    {
        private List<ReportBaoCaoTheoDoiDonGiaTheoHopDongModel> MainModels { get; set; } = new();
        private string _searchStatusString = "";
        private DateTime? _fromDate = null;
        private DateTime? _toDate = null;
        private CongTyModel? _selectedCongTyFilter = null;
        private ContractModel? _selectedContractFilter = null;

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

        private async Task LoadData(bool isReset = false)
        {
            IsLoading = true;

            if (isReset)
            {
                ResetPage();
                await Task.Delay(100);
            }

            BuilderQuery = $"Report/contract-unit-prices?limit={PageSize}&offset={(Page - 1) * PageSize}";

            if (_selectedContractFilter == null)
            {
                AlertService.ShowAlert("Vui lòng chọn hợp đồng để xem báo cáo", "warning");
                IsLoading = false;
                return;
            }

            BuilderQuery += $"&contract={_selectedContractFilter.id}";

            var result = await MainService.GeDataAsync(BuilderQuery);
            if (result.IsSuccess)
            {
                MainModels = result.Data ?? new List<ReportBaoCaoTheoDoiDonGiaTheoHopDongModel>();
                if (result.Meta != null)
                {
                    TotalItems = result.Meta.total_count ?? 0;
                    TotalPages = (int)Math.Ceiling((double)TotalItems / PageSize);

                    if (Page > TotalPages)
                    {
                        await SelectedPage(TotalPages);
                    }
                }
            }
            else
            {
                MainModels = new List<ReportBaoCaoTheoDoiDonGiaTheoHopDongModel>();
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

        private async Task<IEnumerable<ContractModel>> LoaContractFilterData(string searchText)
        {
            string query = "";
            if (!string.IsNullOrEmpty(_searchStatusString))
            {
                query += $"&filter[_and][][status][_eq]={_searchStatusString}";
            }
            if (_selectedCongTyFilter != null)
            {
                query += $"&filter[_and][][cong_ty][_eq]={_selectedCongTyFilter.id}";
            }
            if (_fromDate != null)
            {
                query += $"&filter[_and][][ngay_hop_dong][_gte]={_fromDate:yyyy-MM-dd}";
            }
            if (_toDate != null)
            {
                query += $"&filter[_and][][ngay_hop_dong][_lte]={_toDate:yyyy-MM-dd}";
            }
            return await LoadBlazorTypeaheadData(searchText, ContractService, query);
        }

        private void OnStatusFilterChanged(ChangeEventArgs? selected)
        {
            _searchStatusString = selected?.Value?.ToString() ?? string.Empty;
            _selectedContractFilter = null;
        }

        private void OnCongTyFilterChanged(CongTyModel? selected)
        {
            _selectedCongTyFilter = selected;
            _selectedContractFilter = null;
        }

        private async Task OnContractFilterChanged(ContractModel? selected)
        {
            _selectedContractFilter = selected;
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

                if (isFilter)
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
                IsLoading = true;

                BuilderQuery = $"Report/contract-unit-prices?limit={int.MaxValue}&offset={0}";

                if (_fromDate.HasValue)
                {
                    BuilderQuery += $"&fromDate={_fromDate.Value:yyyy-MM-dd}";
                }
                if (_toDate.HasValue)
                {
                    BuilderQuery += $"&toDate={_toDate.Value:yyyy-MM-dd}";
                }
                if (_selectedContractFilter != null)
                {
                    BuilderQuery += $"&contract={_selectedContractFilter.id}";
                }


                var fields = new List<string>()
                {
                    "MaDinhMuc",
                    "TenDinhMuc",
                    "SoLuong",
                    "DonGiaDM",
                    "ThanhTienDM",
                    "DonGiaTT",
                    "ThanhTienTT",
                    "ChenhLech",
                    "TyLe",
                };
                var labels = new List<string>()
                {
                    "Mã định mức",
                    "Tên định mức",
                    "Số lượng",
                    "Đơn giá",
                    "Thành tiền",
                    "Đơn giá hợp đồng",
                    "Thành tiền hợp đồng",
                    "Chênh lệch",
                    "Tỷ lệ (%)",
                };

                var result = await MainService.GeDataAsync(BuilderQuery);
                if (result.IsSuccess)
                {
                    var fileBytes = await ExportExcelService.ExportToExcelAsync(result.Data?.Cast<dynamic>().ToList() ?? new List<dynamic>(), fields, labels);

                    var fileName = $"{"BaoCaoTheoDoiDonGiaTheoHopDong"}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
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
                IsLoading = false;
            }
        }
    }
}
