using CoreAdminWeb.Commons;
using CoreAdminWeb.Helpers;
using CoreAdminWeb.Model;
using CoreAdminWeb.Model.User;
using CoreAdminWeb.Services.BaseServices;
using CoreAdminWeb.Services.Users;
using CoreAdminWeb.Shared.Base;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CoreAdminWeb.Pages.Admins.DanhSachDoan
{
    public partial class Index(IBaseService<KhamSucKhoeCongTyModel> MainService,
                               IBaseDetailService<SoKhamSucKhoeModel> SoKhamSucKhoeService,
                               IUserService UserService) : BlazorCoreBase
    {
        private List<KhamSucKhoeCongTyModel> MainModels { get; set; } = new();
        private bool openDeleteModal = false;
        private bool openAddOrUpdateModal = false;
        private bool openDetailDeleteModal = false;
        private KhamSucKhoeCongTyModel SelectedItem { get; set; } = new KhamSucKhoeCongTyModel();
        private List<SoKhamSucKhoeModel> SelectedItemsDetail { get; set; } = new List<SoKhamSucKhoeModel>();
        private SoKhamSucKhoeModel? SelectedItemDetail { get; set; } = default;
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

        private async Task<IEnumerable<UserModel>> LoadBenhNhanData(string searchText)
        {
            try
            {
                var query = "sort=-id";

                query += "&filter[_and][][status][_eq]=active";
                query += $"&filter[_and][][role][_eq]={GlobalConstant.PATIENT_ROLE_ID}";
                query += $"&filter[_and][][ma_don_vi][_eq]={SelectedItem.ma_hop_dong_ksk?.cong_ty?.id}";

                if (!string.IsNullOrEmpty(searchText))
                {
                    query += $"&filter[_and][0][_or][0][first_name][_contains]={Uri.EscapeDataString(searchText)}";
                    query += $"&filter[_and][0][_or][1][last_name][_contains]={Uri.EscapeDataString(searchText)}";
                    query += $"&filter[_and][0][_or][2][so_dinh_danh][_contains]={Uri.EscapeDataString(searchText)}";
                    query += $"&filter[_and][0][_or][3][ma_benh_nhan][_contains]={Uri.EscapeDataString(searchText)}";
                }

                var result = await UserService.GetAllAsync(query);
                return result?.IsSuccess == true ? result.Data ?? Enumerable.Empty<UserModel>() : Enumerable.Empty<UserModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading typeahead data: {ex.Message}");
                return Enumerable.Empty<UserModel>();
            }
        }

        private async Task LoadDetailData()
        {
            SelectedItemsDetail = new List<SoKhamSucKhoeModel>();
            var buildQuery = $"sort=sort";
            buildQuery += $"&filter[_and][][MaDotKham][_eq]={SelectedItem.id}";
            buildQuery += $"&filter[_and][][deleted][_eq]=false";
            var result = await SoKhamSucKhoeService.GetAllAsync(buildQuery);
            if (result.IsSuccess)
            {
                SelectedItemsDetail = result.Data ?? new List<SoKhamSucKhoeModel>();
            }
            else
            {
                AlertService.ShowAlert(result.Message ?? "Lỗi khi tải dữ liệu chi tiết", "danger");
            }
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

        private void OpenDetailDeleteModal(SoKhamSucKhoeModel item)
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

            if (!SelectedItemsDetail.Any(c => !c.deleted))
            {
                SelectedItemsDetail.Add(new SoKhamSucKhoeModel()
                {
                    MaDotKham = SelectedItem,
                    sort = (SelectedItemsDetail.Max(c => c.sort) ?? 0) + 1,
                    code = string.Empty,
                    name = string.Empty,
                    ngay_kham = SelectedItem.ngay_du_kien_kham,
                    ngay_lap_so = DateTime.Now
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
                SelectedItemsDetail = new List<SoKhamSucKhoeModel>();
            }

            SelectedItemsDetail.Add(new SoKhamSucKhoeModel
            {
                MaDotKham = SelectedItem,
                sort = (SelectedItemsDetail.Max(c => c.sort) ?? 0) + 1,
                code = string.Empty,
                name = string.Empty,
                ngay_kham = SelectedItem.ngay_du_kien_kham,
                ngay_lap_so = DateTime.Now
            });

            // Wait for modal to render
            _ = Task.Run(async () =>
            {
                await Task.Delay(500);
                await JsRuntime.InvokeVoidAsync("initializeDatePicker");
            });
        }

        private async Task OpenAddOrUpdateModal(KhamSucKhoeCongTyModel? item)
        {
            _titleAddOrUpdate = item != null ? "Sửa" : "Thêm mới";
            SelectedItem = item != null ? item.DeepClone() : new KhamSucKhoeCongTyModel();

            SelectedItemsDetail = new List<SoKhamSucKhoeModel>();

            if (SelectedItem.id > 0)
            {
                await LoadDetailData();
            }

            if (!SelectedItemsDetail.Any())
            {
                SelectedItemsDetail.Add(new SoKhamSucKhoeModel()
                {
                    MaDotKham = SelectedItem,
                    sort = (SelectedItemsDetail.Max(c => c.sort) ?? 0) + 1,
                    code = string.Empty,
                    name = string.Empty,
                    ngay_kham = SelectedItem.ngay_du_kien_kham,
                    ngay_lap_so = DateTime.Now
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
                    var chiTietList = SelectedItemsDetail
                        .Where(c => !c.deleted)
                        .Select(c =>
                        {
                            c.MaDotKham = result.Data;
                            return c;
                        })
                        .ToList();

                    var detailResult = await SoKhamSucKhoeService.CreateAsync(chiTietList);
                    if (!detailResult.IsSuccess)
                    {
                        AlertService.ShowAlert(detailResult.Message ?? "Lỗi khi thêm mới chi tiết dữ liệu", "danger");
                        return;
                    }
                    await LoadDetailData();
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
                        .Where(c => (!c.deleted) && c.id == 0)
                        .Select(c =>
                        {
                            c.MaDotKham = SelectedItem;
                            return c;
                        }).ToList();
                    var removeChiTietList = SelectedItemsDetail
                        .Where(c => c.deleted && c.id > 0)
                        .Select(c =>
                        {
                            c.MaDotKham = SelectedItem;
                            c.deleted = true;
                            return c;
                        }).ToList();
                    var updateChiTietList = SelectedItemsDetail
                        .Where(c => (!c.deleted) && c.id > 0)
                        .Select(c =>
                        {
                            c.MaDotKham = SelectedItem;
                            return c;
                        }).ToList();

                    if (addNewChiTietList.Any())
                    {
                        var detailResult = await SoKhamSucKhoeService.CreateAsync(addNewChiTietList);
                        if (!detailResult.IsSuccess)
                        {
                            AlertService.ShowAlert(detailResult.Message ?? "Lỗi khi thêm mới chi tiết dữ liệu", "danger");
                            return;
                        }
                    }

                    if (removeChiTietList.Any())
                    {
                        var detailResult = await SoKhamSucKhoeService.DeleteAsync(removeChiTietList);
                        if (!detailResult.IsSuccess)
                        {
                            AlertService.ShowAlert(detailResult.Message ?? "Lỗi khi xóa chi tiết dữ liệu", "danger");
                            return;
                        }
                    }

                    if (updateChiTietList.Any())
                    {
                        var detailResult = await SoKhamSucKhoeService.UpdateAsync(updateChiTietList);
                        if (!detailResult.IsSuccess)
                        {
                            AlertService.ShowAlert(detailResult.Message ?? "Lỗi khi cập nhật chi tiết dữ liệu", "danger");
                            return;
                        }
                    }

                    await LoadDetailData();
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
            var indexValue = SelectedItemsDetail.FindIndex(c => !c.deleted && (c.benh_nhan == null || string.IsNullOrEmpty(c.ma_luot_kham)));
            if (indexValue >= 0)
            {
                AlertService.ShowAlert($"Thông tin bệnh nhân tại dòng {indexValue + 1} bị thiếu!", "warning");
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

        private void OnValueChanged(ChangeEventArgs e, string fieldName, SoKhamSucKhoeModel item)
        {
            try
            {
                var dateStr = e.Value?.ToString();
                if (string.IsNullOrEmpty(dateStr))
                {
                    ReflectionHelper.SetFieldValue(item, fieldName, null);
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
                        ReflectionHelper.SetFieldValue(item, fieldName, date);
                    }
                }
            }
            catch (Exception ex)
            {
                AlertService.ShowAlert($"Lỗi khi xử lý ngày: {ex.Message}", "danger");
            }
        }
    }
}