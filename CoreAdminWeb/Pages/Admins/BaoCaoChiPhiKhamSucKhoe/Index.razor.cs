using CoreAdminWeb.Enums;
using CoreAdminWeb.Model.Contract;
using CoreAdminWeb.Services;
using CoreAdminWeb.Services.BaseServices;
using CoreAdminWeb.Shared.Base;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CoreAdminWeb.Pages.Admins.BaoCaoChiPhiKhamSucKhoe
{
    public partial class Index(
        IBaseService<ContractModel> MainService,
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

            var result = await MainService.GetAllAsync(BuilderQuery);
            if (result.IsSuccess)
            {
                MainModels = result.Data ?? new List<ContractModel>();
                if (result.Meta != null)
                {
                    TotalItems = result.Meta.filter_count ?? 0;
                    TotalPages = result.Meta.page_count ?? 0;
                }
            }
            else
            {
                MainModels = new List<ContractModel>();
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

        private async Task OnStatusFilterChanged(ChangeEventArgs? selected)
        {
            _searchStatusString = selected?.Value?.ToString() ?? string.Empty;

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
        
        private async Task OnExcelExport()
        {
            try
            {
                IsLoading = true;

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

                var fields = new List<string>()
                {
                    "stt",
                    "code",
                    "cong_ty",
                    "gia_tri_hop_dong",
                    "tong_chi_phi_dm",
                    "tong_chi_phi_thuc_te",
                    "chenh_lech",
                    "ket_qua",
                };
                var labels = new List<string>()
                {
                    "STT",
                    "Mã hợp đồng",
                    "Công ty",
                    "Giá trị hợp đồng (VNĐ)",
                    "Tổng chi phí định mức (VNĐ)",
                    "Tổng chi phí thực tế (VNĐ)",
                    "Chênh lệch (VNĐ)",
                    "Kết quả"
                };

                var result = await MainService.GetAllAsync(BuilderQuery);
                if (result.IsSuccess)
                {
                    var prepareData = result.Data?.Select(item =>
                        (dynamic)new
                        {
                            stt = ((Page - 1) * PageSize) + result.Data.IndexOf(item) + 1,
                            code = item.code,
                            cong_ty = item.cong_ty?.name,
                            gia_tri_hop_dong = item.gia_tri_hop_dong,
                            tong_chi_phi_dm = item.chi_tiet?.Sum(x => x.thanh_tien_dm),
                            tong_chi_phi_thuc_te = item.chi_tiet?.Sum(x => x.chi_phi_thuc_te),
                            chenh_lech = item.gia_tri_hop_dong - item.chi_tiet?.Sum(x => x.chi_phi_thuc_te),
                            ket_qua = item.chi_tiet?.Sum(x => x.chi_phi_thuc_te) > item.gia_tri_hop_dong ? "Vượt hợp đồng" : item.chi_tiet?.Sum(x => x.chi_phi_thuc_te) > item.chi_tiet?.Sum(x => x.thanh_tien_dm) ? "Vượt định mức" : "Đạt",
                        }
                    ).ToList() ?? new List<dynamic>();

                    var fileBytes = await ExportExcelService.ExportToExcelAsync(prepareData, fields, labels);

                    var fileName = $"{"BaoCaoChiPhiKhamSucKhoe"}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
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
