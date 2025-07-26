using CoreAdminWeb.Helpers;
using CoreAdminWeb.Model;
using CoreAdminWeb.Model.Contract;
using CoreAdminWeb.Model.User;
using CoreAdminWeb.Services;
using CoreAdminWeb.Services.BaseServices;
using CoreAdminWeb.Services.Users;
using CoreAdminWeb.Shared.Base;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CoreAdminWeb.Pages.Admins.MedicalRecordsConfig
{
    public partial class Index(IBaseService<KhamSucKhoeCongTyModel> MainService,
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

        private string _selectedStatusString = "";

        private bool readOnly { get; set; } = false;
        private UserModel? SelectedBacSiTuanHoan { get; set; } = null;
        private UserModel? SelectedBacSiHoHap { get; set; } = null;
        private UserModel? SelectedBacSiTieuHoa { get; set; } = null;
        private UserModel? SelectedBacSiThanTietNieu { get; set; } = null;
        private UserModel? SelectedBacSiNoiTiet { get; set; } = null;
        private UserModel? SelectedBacSiCoXuongKhop { get; set; } = null;
        private UserModel? SelectedBacSiThanKinh { get; set; } = null;
        private UserModel? SelectedBacSiTamThan { get; set; } = null;
        private UserModel? SelectedBacSiNgoaiKhoa { get; set; } = null;
        private UserModel? SelectedBacSiMat { get; set; } = null;
        private UserModel? SelectedBacSiTaiMuiHong { get; set; } = null;
        private UserModel? SelectedBacSiRangHamMat { get; set; } = null;
        private UserModel? SelectedBacSiSanPhuKhoa { get; set; } = null;
        private UserModel? SelectedBacSiKetLuan { get; set; } = null;

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
                BuilderQuery += $"&filter[_and][1][_or][0][contract][code][_eq]={_searchString}";
                BuilderQuery += $"&filter[_and][1][_or][1][contract][cong_ty][code][_eq]={_searchString}";
                BuilderQuery += $"&filter[_and][1][_or][2][contract][name][_contains]={_searchString}";
                BuilderQuery += $"&filter[_and][1][_or][3][contract][cong_ty][name][_contains]={_searchString}";
                BuilderQuery += $"&filter[_and][1][_or][4][code][_eq]={_searchString}";
            }
            if (!string.IsNullOrEmpty(_searchStatusString))
            {
                BuilderQuery += $"&filter[_and][2][active][_eq]={_searchStatusString}";
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

        private async Task<IEnumerable<CongTyModel>> LoadCongTyData(string searchText)
        {
            return await LoadBlazorTypeaheadData(searchText, CongTyService, "filter[_and][][status][_eq]=active");
        }

        private async Task<IEnumerable<ContractModel>> LoadHopDongData(string searchText)
        {
            string query = "";
            if (SelectedCongTy != null)
            {
                query = $"filter[_and][][cong_ty][_eq]={SelectedCongTy.id}";
            }
            return await LoadBlazorTypeaheadData(searchText, HopDongService, query);
        }

        private async Task<IEnumerable<UserModel>> LoadBacSiData(string searchText)
        {
            try
            {
                var query = "sort=-id";

                query += "&filter[_and][][status][_eq]=active";
                query += "&filter[_and][][role][_eq]=87D650A9-0BD2-41DC-ADF2-B0A248AD9A3B";

                if (!string.IsNullOrEmpty(searchText))
                {
                    query += $"&filter[_and][0][_or][0][first_name][_contains]={Uri.EscapeDataString(searchText)}";
                    query += $"&filter[_and][0][_or][1][last_name][_contains]={Uri.EscapeDataString(searchText)}";
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

        private async Task LoadUserInfo()
        {
            try
            {
                List<UserModel> users = new List<UserModel>();
                if (SelectedItem.id > 0)
                {
                    var query = "sort=-id";
                    var result = await UserService.GetAllAsync(query);
                    users = result.Data ?? new List<UserModel>();
                }

                OnUserChange(users.FirstOrDefault(c => c.full_name.Equals(SelectedItem.bs_tuan_hoan)), nameof(SelectedItem.bs_tuan_hoan));
                OnUserChange(users.FirstOrDefault(c => c.full_name.Equals(SelectedItem.bs_ho_hap)), nameof(SelectedItem.bs_ho_hap));
                OnUserChange(users.FirstOrDefault(c => c.full_name.Equals(SelectedItem.bs_tieu_hoa)), nameof(SelectedItem.bs_tieu_hoa));
                OnUserChange(users.FirstOrDefault(c => c.full_name.Equals(SelectedItem.bs_than_tiet_nieu)), nameof(SelectedItem.bs_than_tiet_nieu));
                OnUserChange(users.FirstOrDefault(c => c.full_name.Equals(SelectedItem.bs_noi_tiet)), nameof(SelectedItem.bs_noi_tiet));
                OnUserChange(users.FirstOrDefault(c => c.full_name.Equals(SelectedItem.bs_co_xuong_khop)), nameof(SelectedItem.bs_co_xuong_khop));
                OnUserChange(users.FirstOrDefault(c => c.full_name.Equals(SelectedItem.bs_than_kinh)), nameof(SelectedItem.bs_than_kinh));
                OnUserChange(users.FirstOrDefault(c => c.full_name.Equals(SelectedItem.bs_tam_than)), nameof(SelectedItem.bs_tam_than));
                OnUserChange(users.FirstOrDefault(c => c.full_name.Equals(SelectedItem.bs_ngoai_khoa)), nameof(SelectedItem.bs_ngoai_khoa));
                OnUserChange(users.FirstOrDefault(c => c.full_name.Equals(SelectedItem.bs_mat)), nameof(SelectedItem.bs_mat));
                OnUserChange(users.FirstOrDefault(c => c.full_name.Equals(SelectedItem.bs_tai_mui_hong)), nameof(SelectedItem.bs_tai_mui_hong));
                OnUserChange(users.FirstOrDefault(c => c.full_name.Equals(SelectedItem.bs_rang_ham_mat)), nameof(SelectedItem.bs_rang_ham_mat));
                OnUserChange(users.FirstOrDefault(c => c.full_name.Equals(SelectedItem.bs_san_phu_khoa)), nameof(SelectedItem.bs_san_phu_khoa));
                OnUserChange(users.FirstOrDefault(c => c.full_name.Equals(SelectedItem.bs_ket_luan)), nameof(SelectedItem.bs_ket_luan));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading typeahead data: {ex.Message}");
            }
        }

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
                var congTyResult = await CongTyService.GetAllAsync($"filter[_and][][status][_eq]=active&filter[_and][][code][_eq]={SelectedItem.ma_don_vi}");
                if (congTyResult.IsSuccess && congTyResult.Data != null && congTyResult.Data.Any())
                {
                    SelectedCongTy = congTyResult.Data.FirstOrDefault();
                    SelectedItem.ma_don_vi = SelectedCongTy?.code;
                }
            }

            _selectedStatusString = SelectedItem.active?.ToString() ?? "True";
            if (readOnly)
            {
                _titleAddOrUpdate = $"Thông tin hồ sơ: {SelectedItem.code}";
            }

            openAddOrUpdateModal = true;

            await LoadUserInfo();

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
                    AlertService.ShowAlert(result.Message ?? "Lỗi khi thêm mới dữ liệu", "danger");
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
                    ReflectionHelper.SetDateFieldValue(this, SelectedItem, fieldName, null);
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
                        ReflectionHelper.SetDateFieldValue(this, SelectedItem, fieldName, date);
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

        private async Task OnStatusFilterChanged(ChangeEventArgs? selected)
        {
            _searchStatusString = selected?.Value?.ToString() ?? string.Empty;

            await LoadData();
        }

        private void OnStatusChanged(ChangeEventArgs? selected)
        {
            _selectedStatusString = selected?.Value?.ToString() ?? string.Empty;
            SelectedItem.active = true;
            if (bool.TryParse(_selectedStatusString, out bool activeValue))
            {
                SelectedItem.active = activeValue;
            }
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

        #region Doctors
        private void OnBacSiTuanHoanChanged(UserModel? user)
        {
            OnUserChange(user, nameof(SelectedItem.bs_tuan_hoan));
        }
        private void OnBacSiHoHapChanged(UserModel? user)
        {
            OnUserChange(user, nameof(SelectedItem.bs_ho_hap));
        }
        private void OnBacSiTieuHoaChanged(UserModel? user)
        {
            OnUserChange(user, nameof(SelectedItem.bs_tieu_hoa));
        }
        private void OnBacSiThanTietNieuChanged(UserModel? user)
        {
            OnUserChange(user, nameof(SelectedItem.bs_than_tiet_nieu));
        }
        private void OnBacSiNoiTietChanged(UserModel? user)
        {
            OnUserChange(user, nameof(SelectedItem.bs_noi_tiet));
        }
        private void OnBacSiCoXuongKhopChanged(UserModel? user)
        {
            OnUserChange(user, nameof(SelectedItem.bs_co_xuong_khop));
        }
        private void OnBacSiThanKinhChanged(UserModel? user)
        {
            OnUserChange(user, nameof(SelectedItem.bs_than_kinh));
        }
        private void OnBacSiTamThanChanged(UserModel? user)
        {
            OnUserChange(user, nameof(SelectedItem.bs_tam_than));
        }
        private void OnBacSiNgoaiKhoaChanged(UserModel? user)
        {
            OnUserChange(user, nameof(SelectedItem.bs_ngoai_khoa));
        }
        private void OnBacSiMatChanged(UserModel? user)
        {
            OnUserChange(user, nameof(SelectedItem.bs_mat));
        }
        private void OnBacSiTaiMuiHongChanged(UserModel? user)
        {
            OnUserChange(user, nameof(SelectedItem.bs_tai_mui_hong));
        }
        private void OnBacSiRangHamMatChanged(UserModel? user)
        {
            OnUserChange(user, nameof(SelectedItem.bs_rang_ham_mat));
        }
        private void OnBacSiSanPhuKhoaChanged(UserModel? user)
        {
            OnUserChange(user, nameof(SelectedItem.bs_san_phu_khoa));
        }
        private void OnBacSiKetLuanChanged(UserModel? user)
        {
            OnUserChange(user, nameof(SelectedItem.bs_ket_luan));
        }

        private void OnUserChange(UserModel? user, string fieldName)
        {
            try
            {
                switch (fieldName)
                {
                    case nameof(SelectedItem.bs_tuan_hoan):
                        SelectedBacSiTuanHoan = user;
                        SelectedItem.bs_tuan_hoan = user?.full_name;
                        SelectedItem.chu_ky_tuan_hoan = user?.chu_ky_bac_si;
                        break;
                    case nameof(SelectedItem.bs_ho_hap):
                        SelectedBacSiHoHap = user;
                        SelectedItem.bs_ho_hap = user?.full_name;
                        SelectedItem.chu_ky_ho_hap = user?.chu_ky_bac_si;
                        break;
                    case nameof(SelectedItem.bs_tieu_hoa):
                        SelectedBacSiTieuHoa = user;
                        SelectedItem.bs_tieu_hoa = user?.full_name;
                        SelectedItem.chu_ky_tieu_hoa = user?.chu_ky_bac_si;
                        break;
                    case nameof(SelectedItem.bs_than_tiet_nieu):
                        SelectedBacSiThanTietNieu = user;
                        SelectedItem.bs_than_tiet_nieu = user?.full_name;
                        SelectedItem.chu_ky_than_tiet_nieu = user?.chu_ky_bac_si;
                        break;
                    case nameof(SelectedItem.bs_noi_tiet):
                        SelectedBacSiNoiTiet = user;
                        SelectedItem.bs_noi_tiet = user?.full_name;
                        SelectedItem.chu_ky_noi_tiet = user?.chu_ky_bac_si;
                        break;
                    case nameof(SelectedItem.bs_co_xuong_khop):
                        SelectedBacSiCoXuongKhop = user;
                        SelectedItem.bs_co_xuong_khop = user?.full_name;
                        SelectedItem.chu_ky_co_xuong_khop = user?.chu_ky_bac_si;
                        break;
                    case nameof(SelectedItem.bs_than_kinh):
                        SelectedBacSiThanKinh = user;
                        SelectedItem.bs_than_kinh = user?.full_name;
                        SelectedItem.chu_ky_than_kinh = user?.chu_ky_bac_si;
                        break;
                    case nameof(SelectedItem.bs_tam_than):
                        SelectedBacSiTamThan = user;
                        SelectedItem.bs_tam_than = user?.full_name;
                        SelectedItem.chu_ky_tam_than = user?.chu_ky_bac_si;
                        break;
                    case nameof(SelectedItem.bs_ngoai_khoa):
                        SelectedBacSiNgoaiKhoa = user;
                        SelectedItem.bs_ngoai_khoa = user?.full_name;
                        SelectedItem.chu_ky_ngoai_khoa = user?.chu_ky_bac_si;
                        break;
                    case nameof(SelectedItem.bs_mat):
                        SelectedBacSiMat = user;
                        SelectedItem.bs_mat = user?.full_name;
                        SelectedItem.chu_ky_mat = user?.chu_ky_bac_si;
                        break;
                    case nameof(SelectedItem.bs_tai_mui_hong):
                        SelectedBacSiTaiMuiHong = user;
                        SelectedItem.bs_tai_mui_hong = user?.full_name;
                        SelectedItem.chu_ky_tai_mui_hong = user?.chu_ky_bac_si;
                        break;
                    case nameof(SelectedItem.bs_rang_ham_mat):
                        SelectedBacSiRangHamMat = user;
                        SelectedItem.bs_rang_ham_mat = user?.full_name;
                        SelectedItem.chu_ky_rang_ham_mat = user?.chu_ky_bac_si;
                        break;
                    case nameof(SelectedItem.bs_san_phu_khoa):
                        SelectedBacSiSanPhuKhoa = user;
                        SelectedItem.bs_san_phu_khoa = user?.full_name;
                        SelectedItem.chu_ky_san_phu_khoa = user?.chu_ky_bac_si;
                        break;
                    case nameof(SelectedItem.bs_ket_luan):
                        SelectedBacSiKetLuan = user;
                        SelectedItem.bs_ket_luan = user?.full_name;
                        SelectedItem.chu_ky_ket_luan = user?.chu_ky_bac_si;
                        break;
                }
            }
            catch (Exception ex)
            {
                AlertService.ShowAlert($"Lỗi khi xử lý thông tin bác sĩ: {ex.Message}", "danger");
            }
        }
        #endregion
    }
}
