using CoreAdminWeb.Helpers;
using CoreAdminWeb.Model;
using CoreAdminWeb.Model.Contract;
using CoreAdminWeb.Services.BaseServices;
using CoreAdminWeb.Services.Contract;
using CoreAdminWeb.Shared.Base;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CoreAdminWeb.Pages.Admins.Contract
{
    public partial class Index(
        IBaseService<ContractModel> MainService,
        IContractDinhMucService ContractDinhMucService,
        IBaseService<CongTyModel> CongTyService,
        IBaseService<ContractTypeModel> ContractTypeService,
        IBaseService<DinhMucModel> DinhMucService
    ) : BlazorCoreBase
    {
        private List<ContractModel> MainModels { get; set; } = new();
        private bool openDeleteModal = false;
        private bool openAddOrUpdateModal = false;
        private bool openDetailDeleteModal = false;
        private ContractModel SelectedItem { get; set; } = new ContractModel();
        private List<ContractDinhMucModel> SelectedItemsDetail { get; set; } = new List<ContractDinhMucModel>();
        private ContractDinhMucModel? SelectedItemDetail { get; set; } = default;
        private string _searchString = "";
        private string _searchStatusString = "";
        private string _titleAddOrUpdate = "Thêm mới";
        private string activeDefTab { get; set; } = "tab1";

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
            var result = await MainService.GetAllAsync(BuilderQuery);
            if (result.IsSuccess)
            {
                MainModels = result.Data ?? new List<ContractModel>();
                if (result.Meta != null)
                {
                    TotalItems = result.Meta.filter_count ?? 0;
                    TotalPages = (int)Math.Ceiling((double)TotalItems / PageSize);
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

        private async Task LoadDetailData()
        {
            var buildQuery = $"sort=sort";
            buildQuery += $"&filter[_and][][contract_id][_eq]={SelectedItem.id}";
            buildQuery += $"&filter[_and][][deleted][_eq]=false";
            var result = await ContractDinhMucService.GetAllAsync(buildQuery);
            SelectedItemsDetail = result.Data ?? new List<ContractDinhMucModel>();
        }


        private async Task<IEnumerable<CongTyModel>> LoadCongTyData(string searchText)
        {
            return await LoadBlazorTypeaheadData(searchText, CongTyService);
        }

        private async Task<IEnumerable<ContractTypeModel>> LoadContractTypeData(string searchText)
        {
            return await LoadBlazorTypeaheadData(searchText, ContractTypeService);
        }

        private async Task<IEnumerable<DinhMucModel>> LoadDinhMucData(string searchText)
        {
            return await LoadBlazorTypeaheadData(searchText, DinhMucService);
        }

        private void OpenDeleteModal(ContractModel item)
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
            SelectedItem = new ContractModel();
            openDeleteModal = false;
        }

        private void OpenDetailDeleteModal(ContractDinhMucModel item)
        {
            SelectedItemDetail = item;

            openDetailDeleteModal = true;
        }

        private void OnDetailDelete()
        {
            if (SelectedItemDetail == null)
            {
                AlertService.ShowAlert("Không có dữ liệu để xóa", "warning");
                return;
            }

            foreach (var item in SelectedItemsDetail)
            {
                if (item.id > 0 && item.id == SelectedItemDetail.id || item.sort > 0 && item.sort == SelectedItemDetail.sort)
                {
                    item.deleted = true;
                }
            }

            SelectedItemDetail = default;

            openDetailDeleteModal = false;

            if (!SelectedItemsDetail.Any(c => c.deleted == null || c.deleted == false))
            {
                SelectedItemsDetail.Add(new ContractDinhMucModel()
                {
                    contract = SelectedItem,
                    sort = (SelectedItemsDetail.Max(c => c.sort) ?? 0) + 1,
                    code = string.Empty,
                    name = string.Empty
                });
            }

            StateHasChanged();
        }

        private void CloseDetailDeleteModal()
        {
            SelectedItemDetail = default;

            openDetailDeleteModal = false;
        }

        private void OnAddChiTiet()
        {
            if (SelectedItemsDetail == null)
            {
                SelectedItemsDetail = new List<ContractDinhMucModel>();
            }

            SelectedItemsDetail.Add(new ContractDinhMucModel
            {
                contract = SelectedItem,
                sort = (SelectedItemsDetail.Max(c => c.sort) ?? 0) + 1,
                code = string.Empty,
                name = string.Empty
            });
        }

        private async Task OpenAddOrUpdateModal(ContractModel? item)
        {
            _titleAddOrUpdate = item != null ? "Sửa" : "Thêm mới";
            SelectedItem = item != null ? item.DeepClone() : new ContractModel();

            SelectedItemsDetail = new List<ContractDinhMucModel>();
            activeDefTab = "tab1";
            if (SelectedItem.id > 0)
            {
                await LoadDetailData();
            }

            if (!SelectedItemsDetail.Any())
            {
                SelectedItemsDetail.Add(new ContractDinhMucModel()
                {
                    contract = SelectedItem,
                    sort = (SelectedItemsDetail.Max(c => c.sort) ?? 0) + 1,
                    code = string.Empty,
                    name = string.Empty
                });
            }

            openAddOrUpdateModal = true;

            // Wait for modal to render
            _ = Task.Run(async () =>
            {
                await Task.Delay(500);
                await JsRuntime.InvokeVoidAsync("initializeDatePicker");
            });
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
                    var chiTieuChitietList = SelectedItemsDetail
                        .Where(c => c.deleted == false || c.deleted == null)
                        .Select(c =>
                        {
                            c.contract = result.Data;
                            return c;
                        })
                        .ToList();

                    var detailResult = await ContractDinhMucService.CreateAsync(chiTieuChitietList);
                    if (!detailResult.IsSuccess)
                    {
                        AlertService.ShowAlert(detailResult.Message ?? "Lỗi khi thêm mới chi tiết dữ liệu", "danger");
                        return;
                    }
                    await LoadData();
                    openAddOrUpdateModal = false;
                    AlertService.ShowAlert("Thêm mới thành công!", "success");
                }
                else
                {
                    AlertService.ShowAlert(result.Message ?? "Lỗi khi thêm mới dữ liệu", "danger");
                }
            }
            else
            {
                var result = await MainService.UpdateAsync(SelectedItem);
                if (result.IsSuccess)
                {
                    var addNewChiTietList = SelectedItemsDetail
                        .Where(c => (c.deleted == false || c.deleted == null) && c.id == 0)
                        .Select(c =>
                        {
                            c.contract = SelectedItem;
                            return c;
                        }).ToList();
                    var removeChiTietList = SelectedItemsDetail
                        .Where(c => c.deleted == true && c.id > 0)
                        .Select(c =>
                        {
                            c.contract = SelectedItem;
                            c.deleted = true;
                            return c;
                        }).ToList();
                    var updateChiTietList = SelectedItemsDetail
                        .Where(c => (c.deleted == false || c.deleted == null) && c.id > 0)
                        .Select(c =>
                        {
                            c.contract = SelectedItem;
                            return c;
                        }).ToList();

                    if (addNewChiTietList.Any())
                    {
                        var detailResult = await ContractDinhMucService.CreateAsync(addNewChiTietList);
                        if (!detailResult.IsSuccess)
                        {
                            AlertService.ShowAlert(detailResult.Message ?? "Lỗi khi thêm mới chi tiết dữ liệu", "danger");
                            return;
                        }
                    }

                    if (removeChiTietList.Any())
                    {
                        var detailResult = await ContractDinhMucService.DeleteAsync(removeChiTietList);
                        if (!detailResult.IsSuccess)
                        {
                            AlertService.ShowAlert(detailResult.Message ?? "Lỗi khi xóa chi tiết dữ liệu", "danger");
                            return;
                        }
                    }

                    if (updateChiTietList.Any())
                    {
                        var detailResult = await ContractDinhMucService.UpdateAsync(updateChiTietList);
                        if (!detailResult.IsSuccess)
                        {
                            AlertService.ShowAlert(detailResult.Message ?? "Lỗi khi cập nhật chi tiết dữ liệu", "danger");
                            return;
                        }
                    }

                    await LoadData();
                    openAddOrUpdateModal = false;
                    AlertService.ShowAlert("Cập nhật thành công!", "success");
                }
                else
                {
                    AlertService.ShowAlert(result.Message ?? "Lỗi khi cập nhật dữ liệu", "danger");
                }
            }
        }

        private bool FormValidation()
        {
            if (SelectedItem.code == null)
            {
                AlertService.ShowAlert("Mã là bắt buộc", "danger");
                return false;
            }

            if (string.IsNullOrEmpty(SelectedItem.name))
            {
                AlertService.ShowAlert("Tên là bắt buộc", "danger");
                return false;
            }

            return true;
        }

        private void CloseAddOrUpdateModal()
        {
            SelectedItem = new ContractModel();
            openAddOrUpdateModal = false;
        }

        private async Task OnStatusFilterChanged(ChangeEventArgs? selected)
        {
            _searchStatusString = selected?.Value?.ToString() ?? string.Empty;

            await LoadData();
        }

        private void OnCongTyChanged(CongTyModel? selected)
        {
            SelectedItem.cong_ty = selected;
        }

        private void OnContractTypeChanged(ContractTypeModel? selected)
        {
            SelectedItem.contract_type = selected;
        }

        private void OnDateChanged(ChangeEventArgs e, string fieldName)
        {
            try
            {
                var dateStr = e.Value?.ToString();
                if (string.IsNullOrEmpty(dateStr))
                {
                    switch (fieldName)
                    {
                        case "ngay_hop_dong":
                            SelectedItem.ngay_hop_dong = null;
                            break;

                        case "ngay_hieu_luc":
                            SelectedItem.ngay_hieu_luc = null;
                            break;
                        case "ngay_het_han":
                            SelectedItem.ngay_het_han = null;
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
                        case "ngay_hop_dong":
                            SelectedItem.ngay_hop_dong = date;
                            break;

                        case "ngay_hieu_luc":
                            SelectedItem.ngay_hieu_luc = date;
                            break;
                        case "ngay_het_han":
                            SelectedItem.ngay_het_han = date;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                AlertService.ShowAlert($"Lỗi khi xử lý ngày: {ex.Message}", "danger");
            }
        }

        private void OnTabChanged(string tab)
        {
            activeDefTab = tab;

            if (activeDefTab == "tab1")
            {
                // Wait for modal to render
                _ = Task.Run(async () =>
                {
                    await Task.Delay(500);
                    await JsRuntime.InvokeVoidAsync("initializeDatePicker");
                });
            }
        }
        private void UpdateThanhTien(ChangeEventArgs e, ContractDinhMucModel item, string field)
        {
            if (!decimal.TryParse(e.Value?.ToString(), out decimal value))
            {
                AlertService.ShowAlert("Dữ liệu không hợp lệ", "warning");
                return;
            }
            if (value < 0)
            {
                AlertService.ShowAlert("Số lượng không thể nhỏ hơn 0", "warning");
                return;
            }

            switch (field)
            {
                case nameof(item.so_luong):
                    item.so_luong = value;
                    break;
                case nameof(item.don_gia_tt):
                    item.don_gia_tt = value;
                    break;
                case nameof(item.don_gia_dm):
                    item.don_gia_dm = value;
                    break;
            }

            item.thanh_tien_tt = null;
            item.thanh_tien_dm = null;

            if (item.so_luong.HasValue && item.don_gia_tt.HasValue)
            {
                item.thanh_tien_tt = item.so_luong * item.don_gia_tt;
            }

            if (item.so_luong.HasValue && item.don_gia_dm.HasValue)
            {
                item.thanh_tien_dm = item.so_luong * item.don_gia_dm;
            }
        }
    }
}