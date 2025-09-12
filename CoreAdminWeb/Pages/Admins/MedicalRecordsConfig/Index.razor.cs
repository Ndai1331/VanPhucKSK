using CoreAdminWeb.Enums;
using CoreAdminWeb.Helpers;
using CoreAdminWeb.Model;
using CoreAdminWeb.Model.Contract;
using CoreAdminWeb.Model.KhamSucKhoes;
using CoreAdminWeb.Model.User;
using CoreAdminWeb.Services;
using CoreAdminWeb.Services.BaseServices;
using CoreAdminWeb.Services.KhamSucKhoeApi;
using CoreAdminWeb.Services.Users;
using CoreAdminWeb.Shared.Base;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CoreAdminWeb.Pages.Admins.MedicalRecordsConfig
{
    public partial class Index(IKhamSucKhoeAPIService<KhamSucKhoeCongTyModel> MainService,
                               IBaseService<KhamSucKhoeCongTyModel> KhamSucKhoeCongTyService,
                               IBaseService<CongTyModel> CongTyService,
                               IBaseService<ContractModel> HopDongService,
                               IUserService UserService) : BlazorCoreBase
    {
        private List<KhamSucKhoeCongTyModel> MainModels { get; set; } = new();
        private bool openDeleteModal = false;
        private bool openAddOrUpdateModal = false;
        private KhamSucKhoeCongTyModel SelectedItem { get; set; } = new KhamSucKhoeCongTyModel();
        private CongTyModel? SelectedCongTy { get; set; } = null;
        private string _searchString = "";
        private string _searchStatusString = "";
        private string _titleAddOrUpdate = "Thêm mới";

        private bool readOnly { get; set; } = false;

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
            }
        }

        private async Task LoadData(bool isReset = false)
        {
            Loading.Show();

            if (isReset)
            {
                ResetPage();
                await Task.Delay(100);
            }

            string builderQuery = $"KhamSucKhoeCongTy/get-list?limit={PageSize}&offset={(Page - 1) * PageSize}";
            if (!string.IsNullOrEmpty(_searchString))
            {
                builderQuery += "&searchText={_searchString}";
            }
            if (!string.IsNullOrEmpty(_searchStatusString))
            {
                builderQuery += $"&status={_searchStatusString}";
            }
            var result = await MainService.GetAllAsync(builderQuery);
            if (result.IsSuccess)
            {
                MainModels = result.Data ?? new List<KhamSucKhoeCongTyModel>();
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
                MainModels = new List<KhamSucKhoeCongTyModel>();
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

        private async Task<IEnumerable<CongTyModel>> LoadCongTyData(string searchText)
        {
            return await LoadBlazorTypeaheadData(searchText, CongTyService, "filter[_and][][status][_eq]=published");
        }

        private async Task<IEnumerable<ContractModel>> LoadHopDongData(string searchText)
        {
            string query = $"filter[_and][][status][_eq]={TrangThaiHopDong.DangThucHien}";
            if (SelectedCongTy != null)
            {
                query += $"&filter[_and][][cong_ty][_eq]={SelectedCongTy.id}";
            }
            if (!string.IsNullOrEmpty(searchText))
            {
                query += $"&filter[_and][0][_or][0][code][_contains]={Uri.EscapeDataString(searchText)}";
                query += $"&filter[_and][0][_or][1][name][_contains]={Uri.EscapeDataString(searchText)}";
            }
            return await LoadBlazorTypeaheadData("", HopDongService, query);
        }

        private async Task<IEnumerable<UserModel>> LoadBacSiData(string searchText)
        {
            try
            {
                var query = "sort=-id";

                query += "&filter[_and][][status][_eq]=active";
                query += $"&filter[_and][][role][_eq]={CurrentSetting.doctor_role_id}";

                if (!string.IsNullOrEmpty(searchText))
                {
                    query += $"&filter[_and][0][_or][0][first_name][_contains]={Uri.EscapeDataString(searchText)}";
                    query += $"&filter[_and][0][_or][1][last_name][_contains]={Uri.EscapeDataString(searchText)}";
                    query += $"&filter[_and][0][_or][2][ma_tai_khoan][_contains]={Uri.EscapeDataString(searchText)}";
                    query += $"&filter[_and][0][_or][3][so_dinh_danh][_contains]={Uri.EscapeDataString(searchText)}";
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

        private void OpenDeleteModal(KhamSucKhoeCongTyModel item)
        {
            SelectedItem = item;
            openDeleteModal = true;
        }

        private async Task OnDelete()
        {
            var result = await KhamSucKhoeCongTyService.DeleteAsync(SelectedItem);
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

        private async Task OpenAddOrUpdateModal(KhamSucKhoeCongTyModel? item, bool isReadOnly = false)
        {
            readOnly = isReadOnly;
            _titleAddOrUpdate = item != null ? "Sửa" : "Thêm mới";
            if (readOnly)
            {
                _titleAddOrUpdate = "Thông tin hồ sơ: ";
            }

            SelectedItem = item != null ? item.DeepClone() : new KhamSucKhoeCongTyModel();
            SelectedCongTy = SelectedItem.ma_hop_dong_ksk?.cong_ty;

            if (SelectedCongTy == null && SelectedItem.ma_don_vi != null)
            {
                var congTyResult = await CongTyService.GetAllAsync($"filter[_and][][status][_eq]=published&filter[_and][][code][_eq]={SelectedItem.ma_don_vi}");
                if (congTyResult.IsSuccess && congTyResult.Data != null && congTyResult.Data.Any())
                {
                    SelectedCongTy = congTyResult.Data.FirstOrDefault();
                    SelectedItem.ma_don_vi = SelectedCongTy?.code;
                }
            }

            if (readOnly)
            {
                _titleAddOrUpdate = $"Thông tin hồ sơ: {SelectedItem.code}";
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
            if (string.IsNullOrEmpty(SelectedItem.code))
            {
                AlertService.ShowAlert("Mã hồ sơ là bắt buộc", "danger");
                return;
            }

            if (SelectedItem.ma_hop_dong_ksk == null)
            {
                AlertService.ShowAlert("Mã hợp đồng là bắt buộc", "danger");
                return;
            }

            if (SelectedItem.ma_don_vi == null)
            {
                AlertService.ShowAlert("Tên công ty là bắt buộc", "danger");
                return;
            }

            if (string.IsNullOrEmpty(SelectedItem.ma_bs_co_xuong_khop) && SelectedItem.bs_co_xuong_khop != null)
            {
                SelectedItem.ma_bs_co_xuong_khop = SelectedItem.bs_co_xuong_khop.ma_tai_khoan;
            }

            if (string.IsNullOrEmpty(SelectedItem.ma_bs_ho_hap) && SelectedItem.bs_ho_hap != null)
            {
                SelectedItem.ma_bs_ho_hap = SelectedItem.bs_ho_hap.ma_tai_khoan;
            }

            if (string.IsNullOrEmpty(SelectedItem.ma_bs_ket_luan) && SelectedItem.bs_ket_luan != null)
            {
                SelectedItem.ma_bs_ket_luan = SelectedItem.bs_ket_luan.ma_tai_khoan;
            }

            if (string.IsNullOrEmpty(SelectedItem.ma_bs_mat) && SelectedItem.bs_mat != null)
            {
                SelectedItem.ma_bs_mat = SelectedItem.bs_mat.ma_tai_khoan;
            }

            if (string.IsNullOrEmpty(SelectedItem.ma_bs_ngoai_khoa) && SelectedItem.bs_ngoai_khoa != null)
            {
                SelectedItem.ma_bs_ngoai_khoa = SelectedItem.bs_ngoai_khoa.ma_tai_khoan;
            }

            if (string.IsNullOrEmpty(SelectedItem.ma_bs_rang_ham_mat) && SelectedItem.bs_rang_ham_mat != null)
            {
                SelectedItem.ma_bs_rang_ham_mat = SelectedItem.bs_rang_ham_mat.ma_tai_khoan;
            }

            if (string.IsNullOrEmpty(SelectedItem.ma_bs_san_phu_khoa) && SelectedItem.bs_san_phu_khoa != null)
            {
                SelectedItem.ma_bs_san_phu_khoa = SelectedItem.bs_san_phu_khoa.ma_tai_khoan;
            }

            if (string.IsNullOrEmpty(SelectedItem.ma_bs_tai_mui_hong) && SelectedItem.bs_tai_mui_hong != null)
            {
                SelectedItem.ma_bs_tai_mui_hong = SelectedItem.bs_tai_mui_hong.ma_tai_khoan;
            }

            if (string.IsNullOrEmpty(SelectedItem.ma_bs_tam_than) && SelectedItem.bs_tam_than != null)
            {
                SelectedItem.ma_bs_tam_than = SelectedItem.bs_tam_than.ma_tai_khoan;
            }

            if (string.IsNullOrEmpty(SelectedItem.ma_bs_than_kinh) && SelectedItem.bs_than_kinh != null)
            {
                SelectedItem.ma_bs_than_kinh = SelectedItem.bs_than_kinh.ma_tai_khoan;
            }

            if (string.IsNullOrEmpty(SelectedItem.ma_bs_than_tiet_nieu) && SelectedItem.bs_than_tiet_nieu != null)
            {
                SelectedItem.ma_bs_than_tiet_nieu = SelectedItem.bs_than_tiet_nieu.ma_tai_khoan;
            }

            if (string.IsNullOrEmpty(SelectedItem.ma_bs_tuan_hoan) && SelectedItem.bs_tuan_hoan != null)
            {
                SelectedItem.ma_bs_tuan_hoan = SelectedItem.bs_tuan_hoan.ma_tai_khoan;
            }

            if (string.IsNullOrEmpty(SelectedItem.ma_bs_tieu_hoa) && SelectedItem.bs_tieu_hoa != null)
            {
                SelectedItem.ma_bs_tieu_hoa = SelectedItem.bs_tieu_hoa.ma_tai_khoan;
            }

            if (string.IsNullOrEmpty(SelectedItem.ma_bs_noi_tiet) && SelectedItem.bs_noi_tiet != null)
            {
                SelectedItem.ma_bs_noi_tiet = SelectedItem.bs_noi_tiet.ma_tai_khoan;
            }

            if (SelectedItem.id == 0)
            {
                var result = await KhamSucKhoeCongTyService.CreateAsync(SelectedItem);
                if (result.IsSuccess)
                {
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
                var result = await KhamSucKhoeCongTyService.UpdateAsync(SelectedItem);
                if (result.IsSuccess)
                {
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

        private void CloseAddOrUpdateModal()
        {
            SelectedItem = new KhamSucKhoeCongTyModel();
            openAddOrUpdateModal = false;
        }

        private async Task OnDateChanged(ChangeEventArgs e, string fieldName, bool isFilter = false)
        {
            try
            {
                var dateStr = e.Value?.ToString();
                if (string.IsNullOrEmpty(dateStr))
                {
                    ReflectionHelper.SetFieldValue(this, SelectedItem, fieldName, null);
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
                        ReflectionHelper.SetFieldValue(this, SelectedItem, fieldName, date);
                    }
                }

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

        private async Task OnStatusFilterChanged(ChangeEventArgs? selected)
        {
            _searchStatusString = selected?.Value?.ToString() ?? string.Empty;

            await LoadData(true);
        }

        private void OnCongTyChanged(CongTyModel? selected)
        {
            SelectedCongTy = selected;
            SelectedItem.ma_hop_dong_ksk = null;
        }

        private void OnHopDongChanged(ContractModel? selected)
        {
            SelectedItem.ma_hop_dong_ksk = selected;
            if (selected?.cong_ty != null)
            {
                SelectedCongTy = selected.cong_ty;
                SelectedItem.ma_don_vi = SelectedCongTy?.code;
            }
        }
    }
}
