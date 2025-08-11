using CoreAdminWeb.Extensions;
using CoreAdminWeb.Helpers;
using CoreAdminWeb.Model;
using CoreAdminWeb.Services.BaseServices;
using CoreAdminWeb.Shared.Base;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CoreAdminWeb.Pages.Admins.DanhMucDinhMuc
{
    public partial class Index(IBaseService<DinhMucModel> MainService, IBaseService<LoaiDinhMucModel> LoaiDinhMucService) : BlazorCoreBase
    {
        private List<DinhMucModel> MainModels { get; set; } = new();
        private bool openDeleteModal = false;
        private bool openAddOrUpdateModal = false;
        private DinhMucModel SelectedItem { get; set; } = new DinhMucModel();
        private string _searchString = "";
        private string _searchStatusString = "";
        private string _titleAddOrUpdate = "Thêm mới";

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await LoadData();
                StateHasChanged();
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
                BuilderQuery += $"&filter[_and][1][_or][2][loai_dinh_muc][name][_contains]={_searchString}";
            }
            if (!string.IsNullOrEmpty(_searchStatusString))
            {
                BuilderQuery += $"&filter[_and][2][status][_eq]={_searchStatusString}";
            }
            var result = await MainService.GetAllAsync(BuilderQuery);
            if (result.IsSuccess)
            {
                MainModels = result.Data ?? new List<DinhMucModel>();
                if (result.Meta != null)
                {
                    TotalItems = result.Meta.filter_count ?? 0;
                    TotalPages = (int)Math.Ceiling((double)TotalItems / PageSize);
                }
            }
            else
            {
                MainModels = new List<DinhMucModel>();
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

        private async Task<IEnumerable<LoaiDinhMucModel>> LoadLoaiDinhMucData(string searchText)
        {
            return await LoadBlazorTypeaheadData(searchText, LoaiDinhMucService);
        }


        private void OpenDeleteModal(DinhMucModel item)
        {
            SelectedItem = item;
            openDeleteModal = true;
        }

        private async Task OnDelete()
        {
            var result = await MainService.DeleteAsync(SelectedItem);
            if (result.IsSuccess && result.Data)
            {
                await LoadData();
                AlertService.ShowAlert("Xoá thành công!", "success");
                openDeleteModal = false;
            }
            else
            {
                AlertService.ShowAlert(result.Message ?? "Lỗi khi xóa dữ liệu", "danger");
            }
        }

        private void CloseDeleteModal()
        {
            SelectedItem = new DinhMucModel();
            openDeleteModal = false;
        }

        private void OpenAddOrUpdateModal(DinhMucModel? item)
        {
            _titleAddOrUpdate = item != null ? "Sửa" : "Thêm mới";
            SelectedItem = item != null ? item.DeepClone() : new DinhMucModel();
            openAddOrUpdateModal = true;
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
                    openAddOrUpdateModal = false;
                    AlertService.ShowAlert("Thêm mới thành công!", "success");
                }
                else
                {
                    AlertService.ShowAlert(result.Errors.GetErrorMessage(), "danger");
                }
            }
            else
            {
                var result = await MainService.UpdateAsync(SelectedItem);
                if (result.IsSuccess)
                {
                    await LoadData();
                    openAddOrUpdateModal = false;
                    AlertService.ShowAlert("Cập nhật thành công!", "success");
                }
                else
                {
                    AlertService.ShowAlert(result.Errors.GetErrorMessage(), "danger");
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

            if (SelectedItem.loai_dinh_muc == null)
            {
                AlertService.ShowAlert("Loại định mức là bắt buộc", "danger");
                return false;
            }

            return true;
        }

        private void CloseAddOrUpdateModal()
        {
            SelectedItem = new DinhMucModel();
            openAddOrUpdateModal = false;
        }

        private async Task OnStatusFilterChanged(ChangeEventArgs? selected)
        {
            _searchStatusString = selected?.Value?.ToString() ?? string.Empty;

            await LoadData();
        }

        #region Currency Formatting Methods

        /// <summary>
        /// Format currency with thousand separators
        /// </summary>
        private string FormatCurrency(decimal? value)
        {
            if (!value.HasValue) return string.Empty;
            return value.Value.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("vi-VN"));
        }

        /// <summary>
        /// Format currency with decimal places
        /// </summary>
        private string FormatCurrencyWithDecimals(decimal? value)
        {
            if (!value.HasValue) return string.Empty;
            return value.Value.ToString("N3", System.Globalization.CultureInfo.GetCultureInfo("vi-VN"));
        }

        /// <summary>
        /// Parse currency string back to decimal, removing thousand separators
        /// </summary>
        private decimal? ParseCurrency(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;
            
            // Remove thousand separators (dots) and replace comma with dot for decimal
            var cleanInput = input.Replace(".", "").Replace(",", ".");
            
            if (decimal.TryParse(cleanInput, System.Globalization.NumberStyles.Any, 
                System.Globalization.CultureInfo.InvariantCulture, out decimal result))
            {
                return result;
            }
            
            return null;
        }

        /// <summary>
        /// Get formatted currency display value
        /// </summary>
        private string GetCurrencyDisplayValue(decimal? value)
        {
            return FormatCurrency(value);
        }

        /// <summary>
        /// Update currency field with formatting and calculation
        /// </summary>
        private async Task UpdateCurrencyField(ChangeEventArgs e, string field)
        {
            var inputValue = e.Value?.ToString();
            var parsedValue = ParseCurrency(inputValue);

            if (parsedValue.HasValue && parsedValue < 0)
            {
                AlertService.ShowAlert("Giá trị không thể nhỏ hơn 0", "warning");
                return;
            }

            switch (field)
            {
                case nameof(SelectedItem.DinhMuc):
                    SelectedItem.DinhMuc = parsedValue;
                    break;
                case nameof(SelectedItem.DonGia):
                    SelectedItem.DonGia = parsedValue;
                    break;
            }

            // Format the input value and update UI
            await InvokeAsync(StateHasChanged);
        }
       
        public async void OnDinhMucChanged(ChangeEventArgs e)
        {
            await UpdateCurrencyField(e, nameof(SelectedItem.DinhMuc));
        }

        public async void OnDonGiaChanged(ChangeEventArgs e)
        {
            await UpdateCurrencyField(e, nameof(SelectedItem.DonGia));
        }

        /// <summary>
        /// Handle currency input real-time formatting with immediate JavaScript call
        /// </summary>
        public async void OnCurrencyInputRealTimeFormatting(ChangeEventArgs e)
        {
            try
            {
                var inputValue = e.Value?.ToString();
                if (!string.IsNullOrEmpty(inputValue))
                {
                    // Call JavaScript immediately to format the input
                    await JsRuntime.InvokeVoidAsync("formatCurrencyInputRealTime", inputValue);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in real-time currency formatting: {ex.Message}");
            }
        }

        /// <summary>
        /// Handle currency input formatting (legacy method for compatibility)
        /// </summary>
        public void OnCurrencyInputFormatting(ChangeEventArgs e)
        {
            // This will be handled by JavaScript
        }

        /// <summary>
        /// Format currency input using JavaScript
        /// </summary>
        public async Task FormatCurrencyInput(ChangeEventArgs e)
        {
            try
            {
                var inputValue = e.Value?.ToString();
                if (!string.IsNullOrEmpty(inputValue))
                {
                    // Let JavaScript handle the real-time formatting
                    await JsRuntime.InvokeVoidAsync("formatCurrencyInput", inputValue);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error formatting currency input: {ex.Message}");
            }
        }

        #endregion
    }
}
