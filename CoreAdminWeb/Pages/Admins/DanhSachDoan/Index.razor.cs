using CoreAdminWeb.Helpers;
using CoreAdminWeb.Model;
using CoreAdminWeb.Model.DanhSachDoan;
using CoreAdminWeb.Services.BaseServices;
// using CoreAdminWeb.Services.DanhSachDoan;
using CoreAdminWeb.Shared.Base;
using Microsoft.JSInterop;

namespace CoreAdminWeb.Pages.Admins.DanhSachDoan
{
    public partial class Index(
        IBaseService<KhamSucKhoeCongTyModel> MainService
        // IDanhSachDoanChiTietService DanhSachDoanChiTietService,
    ) : BlazorCoreBase
    {
        private List<KhamSucKhoeCongTyModel> MainModels { get; set; } = new();
        private bool openDeleteModal = false;
        private bool openAddOrUpdateModal = false;
        private bool openDetailDeleteModal = false;
        private KhamSucKhoeCongTyModel SelectedItem { get; set; } = new KhamSucKhoeCongTyModel();
        private List<DanhSachDoanChiTietModel> SelectedItemsDetail { get; set; } = new List<DanhSachDoanChiTietModel>();
        private DanhSachDoanChiTietModel? SelectedItemDetail { get; set; } = default;
        private string _searchString = "";
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
            }
            var result = await MainService.GetAllAsync(BuilderQuery);
            if (result.IsSuccess)
            {
                MainModels = result.Data ?? new List<KhamSucKhoeCongTyModel>();
                if (result.Meta != null)
                {
                    TotalItems = result.Meta.filter_count ?? 0;
                    TotalPages = (int)Math.Ceiling((double)TotalItems / PageSize);
                }
            }
            else
            {
                MainModels = new List<KhamSucKhoeCongTyModel>();
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
            buildQuery += $"&filter[_and][][ma_ho_so_kham_suc_khoe][_eq]={SelectedItem.id}";
            buildQuery += $"&filter[_and][][deleted][_eq]=false";
            // var result = await DanhSachDoanChiTietService.GetAllAsync(buildQuery);
            // SelectedItemsDetail = result.Data ?? new List<DanhSachDoanChiTietModel>();
            SelectedItemsDetail = new List<DanhSachDoanChiTietModel>();
        }

        // private async Task<IEnumerable<KhamSucKhoeCongTyModel>> LoadKhamSucKhoeCongTyData(string searchText)
        // {
        //     return await LoadBlazorTypeaheadData(searchText, KhamSucKhoeCongTyService);
        // }

        private void OpenDeleteModal(KhamSucKhoeCongTyModel item)
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
            SelectedItem = new KhamSucKhoeCongTyModel();
            openDeleteModal = false;
        }

        private void OpenDetailDeleteModal(DanhSachDoanChiTietModel item)
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
                SelectedItemsDetail.Add(new DanhSachDoanChiTietModel()
                {
                    ma_ho_so_kham_suc_khoe = SelectedItem,
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
                SelectedItemsDetail = new List<DanhSachDoanChiTietModel>();
            }

            SelectedItemsDetail.Add(new DanhSachDoanChiTietModel
            {
                ma_ho_so_kham_suc_khoe = SelectedItem,
                sort = (SelectedItemsDetail.Max(c => c.sort) ?? 0) + 1,
                code = string.Empty,
                name = string.Empty
            });
        }

        private async Task OpenAddOrUpdateModal(KhamSucKhoeCongTyModel? item)
        {
            _titleAddOrUpdate = item != null ? "Sửa" : "Thêm mới";
            SelectedItem = item != null ? item.DeepClone() : new KhamSucKhoeCongTyModel();

            SelectedItemsDetail = new List<DanhSachDoanChiTietModel>();

            if (SelectedItem.id > 0)
            {
                await LoadDetailData();
            }

            if (!SelectedItemsDetail.Any())
            {
                SelectedItemsDetail.Add(new DanhSachDoanChiTietModel()
                {
                    ma_ho_so_kham_suc_khoe = SelectedItem,
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
            // if (!FormValidation())
            // {
            //     return;
            // }
            // if (SelectedItem.id == 0)
            // {
            //     var result = await MainService.CreateAsync(SelectedItem);
            //     if (result.IsSuccess)
            //     {
            //         var chiTietList = SelectedItemsDetail
            //             .Where(c => c.deleted == false || c.deleted == null)
            //             .Select(c =>
            //             {
            //                 c.ma_ho_so_kham_suc_khoe = result.Data;
            //                 return c;
            //             })
            //             .ToList();

            //         var detailResult = await DanhSachDoanChiTietService.CreateAsync(chiTietList);
            //         if (!detailResult.IsSuccess)
            //         {
            //             AlertService.ShowAlert(detailResult.Message ?? "Lỗi khi thêm mới chi tiết dữ liệu", "danger");
            //             return;
            //         }
            //         await LoadData();
            //         openAddOrUpdateModal = false;
            //         AlertService.ShowAlert("Thêm mới thành công!", "success");
            //     }
            //     else
            //     {
            //         AlertService.ShowAlert(result.Message ?? "Lỗi khi thêm mới dữ liệu", "danger");
            //     }
            // }
            // else
            // {
            //     var result = await MainService.UpdateAsync(SelectedItem);
            //     if (result.IsSuccess)
            //     {
            //         var addNewChiTietList = SelectedItemsDetail
            //             .Where(c => (c.deleted == false || c.deleted == null) && c.id == 0)
            //             .Select(c =>
            //             {
            //                 c.ma_ho_so_kham_suc_khoe = SelectedItem;
            //                 return c;
            //             }).ToList();
            //         var removeChiTietList = SelectedItemsDetail
            //             .Where(c => c.deleted == true && c.id > 0)
            //             .Select(c =>
            //             {
            //                 c.ma_ho_so_kham_suc_khoe = SelectedItem;
            //                 c.deleted = true;
            //                 return c;
            //             }).ToList();
            //         var updateChiTietList = SelectedItemsDetail
            //             .Where(c => (c.deleted == false || c.deleted == null) && c.id > 0)
            //             .Select(c =>
            //             {
            //                 c.ma_ho_so_kham_suc_khoe = SelectedItem;
            //                 return c;
            //             }).ToList();

            //         if (addNewChiTietList.Any())
            //         {
            //             var detailResult = await DanhSachDoanChiTietService.CreateAsync(addNewChiTietList);
            //             if (!detailResult.IsSuccess)
            //             {
            //                 AlertService.ShowAlert(detailResult.Message ?? "Lỗi khi thêm mới chi tiết dữ liệu", "danger");
            //                 return;
            //             }
            //         }

            //         if (removeChiTietList.Any())
            //         {
            //             var detailResult = await DanhSachDoanChiTietService.DeleteAsync(removeChiTietList);
            //             if (!detailResult.IsSuccess)
            //             {
            //                 AlertService.ShowAlert(detailResult.Message ?? "Lỗi khi xóa chi tiết dữ liệu", "danger");
            //                 return;
            //             }
            //         }

            //         if (updateChiTietList.Any())
            //         {
            //             var detailResult = await DanhSachDoanChiTietService.UpdateAsync(updateChiTietList);
            //             if (!detailResult.IsSuccess)
            //             {
            //                 AlertService.ShowAlert(detailResult.Message ?? "Lỗi khi cập nhật chi tiết dữ liệu", "danger");
            //                 return;
            //             }
            //         }

            //         await LoadData();
            //         openAddOrUpdateModal = false;
            //         AlertService.ShowAlert("Cập nhật thành công!", "success");
            //     }
            //     else
            //     {
            //         AlertService.ShowAlert(result.Message ?? "Lỗi khi cập nhật dữ liệu", "danger");
            //     }
            // }
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
            SelectedItem = new KhamSucKhoeCongTyModel();
            openAddOrUpdateModal = false;
        }

        private void GoToDetail(int id)
        {
            NavigationManager?.NavigateTo($"/admin/danh-sach-doan-ho-so-kham-suc-khoe/{id}");
        }
    }
}