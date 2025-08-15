using CoreAdminWeb.Enums;
using CoreAdminWeb.Extensions;
using CoreAdminWeb.Helpers;
using CoreAdminWeb.Model;
using CoreAdminWeb.Model.Contract;
using CoreAdminWeb.Model.Settings;
using CoreAdminWeb.Model.User;
using CoreAdminWeb.Services.BaseServices;
using CoreAdminWeb.Services.Files;
using CoreAdminWeb.Services.PDFService;
using CoreAdminWeb.Services.Users;
using CoreAdminWeb.Shared.Base;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace CoreAdminWeb.Pages.Admins.KetQuaKhamSucKhoeTT32
{
    public partial class Index(IBaseDetailService<SoKhamSucKhoeModel> MainService,
                               IBaseService<PhanLoaiSucKhoeModel> PhanLoaiSucKhoaService,
                               IUserService UserService,
                               IBaseService<ContractModel> ContractService,
                               IBaseService<KhamSucKhoeCongTyModel> KhamSucKhoeCongTyService,
                               IFileService FileService,
                               IBaseDetailService<KhamSucKhoeChuyenKhoaModel> KhamSucKhoeChuyenKhoaService,
                               IBaseDetailService<KhamSucKhoeKetLuanModel> KhamSucKhoeKetLuanService,
                               IBaseDetailService<KhamSucKhoeSanPhuKhoaModel> KhamSucKhoeSanPhuKhoaService,
                               IBaseDetailService<KhamSucKhoeTheLucModel> KhamSucKhoeTheLucService,
                               IBaseDetailService<KhamSucKhoeTienSuModel> KhamSucKhoeTienSuService,
                               IBaseDetailService<KhamSucKhoeKetQuaCanLamSangModel> KhamSucKhoeKetQuaCanLamSangService,
                               IBaseDetailService<KhamSucKhoeCanLamSangModel> KhamSucKhoeCanLamSangService,
                               IBaseDetailService<KhamSucKhoeNgheNghiepModel> KhamSucKhoeNgheNghiepService,
                               IPdfService PdfService,
                               IConfiguration Configuration,
                               IWebHostEnvironment WebHostEnvironment) : BlazorCoreBase
    {
        #region Constants
        private const string DEFAULT_PROFILE_IMAGE = "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='120' height='160' viewBox='0 0 120 160'%3E%3Crect width='120' height='160' fill='%23f8f9fa' stroke='%23dee2e6' stroke-width='1'/%3E%3Ctext x='60' y='80' text-anchor='middle' fill='%236c757d' font-size='12'%3EẢnh 4x6 cm%3C/text%3E%3C/svg%3E";
        #endregion

        private string _logoPath = $"/images/Logo/logo.png";
        private readonly string _imagesFolder = $"/images/";
        private string _profileImageUrl = string.Empty;

        private UserModel? CurrentUser { get; set; }

        private List<SoKhamSucKhoeModel> MainModels { get; set; } = new();
        private string activeDefTab = "tab1";

        private SoKhamSucKhoeModel SelectedItem { get; set; } = new SoKhamSucKhoeModel();
        private UserModel SelectedUser { get; set; } = new UserModel();
        private KhamSucKhoeChuyenKhoaModel SelectedKhamSucKhoeChuyenKhoa { get; set; } = new KhamSucKhoeChuyenKhoaModel();
        private KhamSucKhoeKetLuanModel SelectedKhamSucKhoeKetLuan { get; set; } = new KhamSucKhoeKetLuanModel();
        private KhamSucKhoeSanPhuKhoaModel SelectedKhamSucKhoeSanPhuKhoa { get; set; } = new KhamSucKhoeSanPhuKhoaModel();
        private KhamSucKhoeTheLucModel SelectedKhamSucKhoeTheLuc { get; set; } = new KhamSucKhoeTheLucModel();
        private KhamSucKhoeTienSuModel SelectedKhamSucKhoeTienSu { get; set; } = new KhamSucKhoeTienSuModel();
        private KhamSucKhoeCongTyModel SelectedKhamSucKhoeCongTy { get; set; } = new KhamSucKhoeCongTyModel();
        private KhamSucKhoeNgheNghiepModel SelectedKhamSucKhoeNgheNghiep { get; set; } = new KhamSucKhoeNgheNghiepModel();
        private List<KhamSucKhoeKetQuaCanLamSangModel> SelectedKhamSucKhoeKetQuaCanLamSangs { get; set; } = new List<KhamSucKhoeKetQuaCanLamSangModel>() {
            new KhamSucKhoeKetQuaCanLamSangModel()
            {
                type = KetQuaCanLamSang.CDHATDCN.ToString(),
                sort = 0
            },
            new KhamSucKhoeKetQuaCanLamSangModel()
            {
                type = KetQuaCanLamSang.XNCongThucMau.ToString(),
                sort = 1
            },
            new KhamSucKhoeKetQuaCanLamSangModel()
            {
                type = KetQuaCanLamSang.XNNuocTieu.ToString(),
                sort = 2
            },
            new KhamSucKhoeKetQuaCanLamSangModel()
            {
                type = KetQuaCanLamSang.XNKhac.ToString(),
                sort = 3
            }
        };

        private FileCRUDModel UploadFileCRUD { get; set; } = new FileCRUDModel();

        private string para1 { get; set; } = string.Empty;
        private string para2 { get; set; } = string.Empty;
        private string para3 { get; set; } = string.Empty;
        private string para4 { get; set; } = string.Empty;

        private DateTime? _startDateFilter = default;
        private DateTime? _endDateFilter = default;
        private ContractModel? _contractFilter = default;
        private KhamSucKhoeCongTyModel? _khamSucKhoeCongTyFilter = default;
        private string _maDieuTriString = "";
        private string _maBenhNhanString = "";
        private string _tenBenhNhanString = "";

        private bool openSyncKetQuaCanLamSangModal { get; set; } = false;
        private bool onReadonly => SelectedItem.status == Model.Base.Status.published;

        private bool onBS { get; set; } = false;
        private bool onBSHoHap => CurrentUser != null && SelectedKhamSucKhoeCongTy.bs_ho_hap?.id == CurrentUser.id;
        private bool onBSTuanHoan => CurrentUser != null && SelectedKhamSucKhoeCongTy.bs_tuan_hoan?.id == CurrentUser.id;
        private bool onBSTieuHoa => CurrentUser != null && SelectedKhamSucKhoeCongTy.bs_tieu_hoa?.id == CurrentUser.id;
        private bool onBSThanTietNieu => CurrentUser != null && SelectedKhamSucKhoeCongTy.bs_than_tiet_nieu?.id == CurrentUser.id;
        private bool onBSNoiTiet => CurrentUser != null && SelectedKhamSucKhoeCongTy.bs_noi_tiet?.id == CurrentUser.id;
        private bool onBSCoXuongKhop => CurrentUser != null && SelectedKhamSucKhoeCongTy.bs_co_xuong_khop?.id == CurrentUser.id;
        private bool onBSThanKinh => CurrentUser != null && SelectedKhamSucKhoeCongTy.bs_than_kinh?.id == CurrentUser.id;
        private bool onBSTamThan => CurrentUser != null && SelectedKhamSucKhoeCongTy.bs_tam_than?.id == CurrentUser.id;
        private bool onBSNgoaiKhoa => CurrentUser != null && SelectedKhamSucKhoeCongTy.bs_ngoai_khoa?.id == CurrentUser.id;
        private bool onBSMat => CurrentUser != null && SelectedKhamSucKhoeCongTy.bs_mat?.id == CurrentUser.id;
        private bool onBSTaiMuiHong => CurrentUser != null && SelectedKhamSucKhoeCongTy.bs_tai_mui_hong?.id == CurrentUser.id;
        private bool onBSRangHamMat => CurrentUser != null && SelectedKhamSucKhoeCongTy.bs_rang_ham_mat?.id == CurrentUser.id;
        private bool onBSSanPhuKhoa => CurrentUser != null && SelectedKhamSucKhoeCongTy.bs_san_phu_khoa?.id == CurrentUser.id;
        private bool onBSKetLuan => CurrentUser != null && SelectedKhamSucKhoeCongTy.bs_ket_luan?.id == CurrentUser.id || true;

        private string imageWebRootPath { get; set; } = string.Empty;

        private SettingModel? Setting { get; set; } = default;
        private string? doctorRoleId { get; set; } = default;

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await LoadData();
                await LoadSettingsAsync();
                _logoPath = $"{Configuration["DrCoreApi:BaseUrlImage"]}/images/Logo/logo.png";
                await JsRuntime.InvokeAsync<IJSObjectReference>("import", "/assets/js/pages/flatpickr.js");
                SetProfileImagePlaceholder();
                StateHasChanged();

                _ = Task.Run(async () =>
                {
                    var resUser = await UserService.GetCurrentUserAsync();
                    if (resUser.IsSuccess)
                    {
                        CurrentUser = resUser.Data;
                    }
                });

                // Wait for modal to render
                _ = Task.Run(async () =>
                {
                    await Task.Delay(500);
                    await JsRuntime.InvokeVoidAsync("initializeDatePicker");
                });
            }
        }

        /// <summary>
        /// Set profile image placeholder
        /// </summary>
        private void SetProfileImagePlaceholder()
        {
            if (string.IsNullOrEmpty(_profileImageUrl))
            {
                _profileImageUrl = DEFAULT_PROFILE_IMAGE;
            }
        }

        private async Task LoadSettingsAsync()
        {
            try
            {
                if (Setting != null)
                {
                    return; // Skip if already loaded
                }

                var settingResults = await SettingService.GetCurrentSettingAsync();
                if (settingResults.IsSuccess)
                {
                    Setting = settingResults.Data;
                    doctorRoleId = Setting?.doctor_role_id ?? "";
                    onBS = CurrentUser?.role?.ToLower() == doctorRoleId?.ToLower().ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading settings: {ex.Message}");
            }
        }

        private async Task LoadData()
        {
            IsLoading = true;

            BuildPaginationQuery(Page, PageSize);
            BuilderQuery += $"&filter[_and][0][deleted][_eq]=false";
            if (!string.IsNullOrEmpty(_tenBenhNhanString))
            {
                BuilderQuery += $"&filter[_and][1][_or][0][benh_nhan][first_name][_contains]={_tenBenhNhanString}";
                BuilderQuery += $"&filter[_and][1][_or][1][benh_nhan][last_name][_contains]={_tenBenhNhanString}";
            }
            if (!string.IsNullOrEmpty(_maBenhNhanString))
            {
                BuilderQuery += $"&filter[_and][][ma_benh_nhan][_contains]={_maBenhNhanString}";
            }
            if (!string.IsNullOrEmpty(_maDieuTriString))
            {
                BuilderQuery += $"&filter[_and][][ma_luot_kham][_contains]={_maDieuTriString}";
            }
            if (_startDateFilter.HasValue)
            {
                BuilderQuery += $"&filter[_and][][ngay_kham][_gte]={_startDateFilter:yyyy-MM-dd}";
            }
            if (_endDateFilter.HasValue)
            {
                BuilderQuery += $"&filter[_and][][ngay_kham][_lte]={_endDateFilter:yyyy-MM-dd}";
            }
            if (_khamSucKhoeCongTyFilter != null && _khamSucKhoeCongTyFilter.id > 0)
            {
                BuilderQuery += $"&filter[_and][][MaDotKham][_eq]={_khamSucKhoeCongTyFilter.id}";
            }
            if (_contractFilter != null && _contractFilter.id > 0)
            {
                BuilderQuery += $"&filter[_and][][MaDotKham][ma_hop_dong_ksk][_eq]={_contractFilter.id}";
            }
            var result = await MainService.GetAllAsync(BuilderQuery);
            if (result.IsSuccess)
            {
                MainModels = result.Data ?? new List<SoKhamSucKhoeModel>();
                if (result.Meta != null)
                {
                    TotalItems = result.Meta.filter_count ?? 0;
                    TotalPages = (int)Math.Ceiling((double)TotalItems / PageSize);
                }
            }
            else
            {
                MainModels = new List<SoKhamSucKhoeModel>();
            }
            IsLoading = false;
        }

        private async Task OnRowClick(int soKhamSKId)
        {
            if (SelectedItem.id != soKhamSKId)
            {
                OnResetData();

                activeDefTab = "tab1";
                await LoadDetailData(soKhamSKId);
            }
        }

        private async Task OnCheckBoxChanged(ChangeEventArgs args, int soKhamSKId)
        {
            OnResetData();

            if (args.Value != null && !bool.TryParse(args.Value.ToString(), out bool boolVal) && boolVal)
            {
                await LoadDetailData(soKhamSKId);
            }
        }

        private void OnResetData()
        {
            SelectedItem = new SoKhamSucKhoeModel();
            SelectedUser = new UserModel();
            SelectedKhamSucKhoeChuyenKhoa = new KhamSucKhoeChuyenKhoaModel();
            SelectedKhamSucKhoeKetLuan = new KhamSucKhoeKetLuanModel();
            SelectedKhamSucKhoeSanPhuKhoa = new KhamSucKhoeSanPhuKhoaModel();
            SelectedKhamSucKhoeTheLuc = new KhamSucKhoeTheLucModel();
            SelectedKhamSucKhoeTienSu = new KhamSucKhoeTienSuModel();
            SelectedKhamSucKhoeCongTy = new KhamSucKhoeCongTyModel();
            SelectedKhamSucKhoeNgheNghiep = new KhamSucKhoeNgheNghiepModel();
            SelectedKhamSucKhoeKetQuaCanLamSangs = new List<KhamSucKhoeKetQuaCanLamSangModel>() {
                    new KhamSucKhoeKetQuaCanLamSangModel()
                    {
                        type = KetQuaCanLamSang.CDHATDCN.ToString(),
                        sort = 0
                    },
                    new KhamSucKhoeKetQuaCanLamSangModel()
                    {
                        type = KetQuaCanLamSang.XNCongThucMau.ToString(),
                        sort = 1
                    },
                    new KhamSucKhoeKetQuaCanLamSangModel()
                    {
                        type = KetQuaCanLamSang.XNNuocTieu.ToString(),
                        sort = 2
                    },
                    new KhamSucKhoeKetQuaCanLamSangModel()
                    {
                        type = KetQuaCanLamSang.XNKhac.ToString(),
                        sort = 3
                    }
                };

            para1 = string.Empty;
            para2 = string.Empty;
            para3 = string.Empty;
            para4 = string.Empty;
        }

        private async Task LoadDetailData(int soKhamSKId)
        {
            IsLoading = true;
            var resSoKhamSK = await MainService.GetByIdAsync(soKhamSKId.ToString());
            if (resSoKhamSK?.IsSuccess == true && resSoKhamSK.Data != null)
            {
                SelectedItem = resSoKhamSK.Data;

                if (SelectedItem.benh_nhan == null)
                {
                    string queryBenhNhan = $"&filter[_and][][ma_benh_nhan][_eq]={SelectedItem.ma_benh_nhan}";
                    var resBenhNhan = await UserService.GetUserByFilterAsync(queryBenhNhan);
                    SelectedUser = resBenhNhan?.IsSuccess == true && resBenhNhan.Data != null
                        ? resBenhNhan.Data
                        : new UserModel();
                }

                if (SelectedUser.id == Guid.Empty && SelectedItem.benh_nhan != null)
                {
                    var resBenhNhan = await UserService.GetUserByIdAsync(SelectedItem.benh_nhan.id);
                    SelectedUser = resBenhNhan?.IsSuccess == true && resBenhNhan.Data != null
                        ? resBenhNhan.Data
                        : new UserModel();
                }

                if (SelectedUser.id == Guid.Empty)
                {
                    AlertService.ShowAlert("Không tìm thấy thông tin bệnh nhân!", "danger");
                }

                SelectedItem.benh_nhan = SelectedUser;
                string query = $"filter[_and][][ma_luot_kham][_eq]={SelectedItem.ma_luot_kham}";

                var tasks = new[]
                {
                    BaseServiceHelper.LoadSingleRecordAsync(KhamSucKhoeChuyenKhoaService, query, r => SelectedKhamSucKhoeChuyenKhoa = r ?? new KhamSucKhoeChuyenKhoaModel()),
                    BaseServiceHelper.LoadSingleRecordAsync(KhamSucKhoeKetLuanService, query, r => SelectedKhamSucKhoeKetLuan = r ?? new KhamSucKhoeKetLuanModel()),
                    BaseServiceHelper.LoadSingleRecordAsync(KhamSucKhoeSanPhuKhoaService, query, r => SelectedKhamSucKhoeSanPhuKhoa = r ?? new KhamSucKhoeSanPhuKhoaModel()),
                    BaseServiceHelper.LoadSingleRecordAsync(KhamSucKhoeTheLucService, query, r => SelectedKhamSucKhoeTheLuc = r ?? new KhamSucKhoeTheLucModel()),
                    BaseServiceHelper.LoadSingleRecordAsync(KhamSucKhoeTienSuService, query, r => SelectedKhamSucKhoeTienSu = r ?? new KhamSucKhoeTienSuModel()),
                    BaseServiceHelper.LoadSingleRecordAsync(KhamSucKhoeCongTyService, $"filter[_and][][id][_eq]={SelectedItem.MaDotKham?.id}", r => SelectedKhamSucKhoeCongTy = r ?? new KhamSucKhoeCongTyModel()),
                    BaseServiceHelper.LoadSingleRecordAsync(KhamSucKhoeNgheNghiepService, query, r => SelectedKhamSucKhoeNgheNghiep = r ?? new KhamSucKhoeNgheNghiepModel()),
                    BaseServiceHelper.LoadMultipleRecordAsync(KhamSucKhoeKetQuaCanLamSangService, query, r => SelectedKhamSucKhoeKetQuaCanLamSangs = r ?? new List<KhamSucKhoeKetQuaCanLamSangModel>()),
                };

                await Task.WhenAll(tasks);

                if (!SelectedKhamSucKhoeKetQuaCanLamSangs.Any())
                {
                    SelectedKhamSucKhoeKetQuaCanLamSangs = new List<KhamSucKhoeKetQuaCanLamSangModel>
                    {
                        new KhamSucKhoeKetQuaCanLamSangModel { type = KetQuaCanLamSang.CDHATDCN.ToString(), sort = 0 },
                        new KhamSucKhoeKetQuaCanLamSangModel { type = KetQuaCanLamSang.XNCongThucMau.ToString(), sort = 1 },
                        new KhamSucKhoeKetQuaCanLamSangModel { type = KetQuaCanLamSang.XNNuocTieu.ToString(), sort = 2 },
                        new KhamSucKhoeKetQuaCanLamSangModel { type = KetQuaCanLamSang.XNKhac.ToString(), sort = 3 }
                    };
                }

                if (!string.IsNullOrEmpty(SelectedKhamSucKhoeSanPhuKhoa.para))
                {
                    var paraSplit = SelectedKhamSucKhoeSanPhuKhoa.para.Split('|');
                    para1 = paraSplit.Length > 0 ? paraSplit[0].Trim() : string.Empty;
                    para2 = paraSplit.Length > 1 ? paraSplit[1].Trim() : string.Empty;
                    para3 = paraSplit.Length > 2 ? paraSplit[2].Trim() : string.Empty;
                    para4 = paraSplit.Length > 3 ? paraSplit[3].Trim() : string.Empty;
                }
            }
            else
            {
                AlertService.ShowAlert("Không tìm thấy thông tin khám!", "danger");
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

        private async Task<IEnumerable<PhanLoaiSucKhoeModel>> LoadPhanLoaiSucKhoeData(string searchText)
        {
            return await LoadBlazorTypeaheadData(searchText, PhanLoaiSucKhoaService, "filter[_and][][active][_eq]=true");
        }

        private async Task<IEnumerable<ContractModel>> LoadContractData(string searchText)
        {
            return await LoadBlazorTypeaheadData(searchText, ContractService);
        }

        private async Task<IEnumerable<KhamSucKhoeCongTyModel>> LoadKhamSucKhoeCongTyData(string searchText)
        {
            return await LoadBlazorTypeaheadData(searchText, KhamSucKhoeCongTyService);
        }

        private async Task<IEnumerable<KhamSucKhoeCanLamSangModel>> LoadKetQuaCanLamSangData(string searchText)
        {
            try
            {
                var query = "sort=-id";

                query += "&filter[_and][][deleted][_eq]=false";

                if (!string.IsNullOrEmpty(searchText))
                {
                    query += $"&filter[_and][0][_or][0][ma_cls][_contains]={Uri.EscapeDataString(searchText)}";
                    query += $"&filter[_and][0][_or][1][ten_cls][_contains]={Uri.EscapeDataString(searchText)}";
                    query += $"&filter[_and][0][_or][1][danh_gia_cls][_contains]={Uri.EscapeDataString(searchText)}";
                }

                var result = await KhamSucKhoeCanLamSangService.GetAllAsync(query);
                return result?.IsSuccess == true ? result.Data ?? Enumerable.Empty<KhamSucKhoeCanLamSangModel>() : Enumerable.Empty<KhamSucKhoeCanLamSangModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading typeahead data: {ex.Message}");
                return Enumerable.Empty<KhamSucKhoeCanLamSangModel>();
            }
        }

        private async Task<IEnumerable<UserModel>> LoadBacSiData(string searchText)
        {
            try
            {
                var query = "sort=-id";

                query += "&filter[_and][][status][_eq]=active";
                query += $"&filter[_and][][role][_eq]={doctorRoleId}";

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

        private void OnLoadKetQuaCanLamSang()
        {
            if (SelectedKhamSucKhoeKetQuaCanLamSangs == null || !SelectedKhamSucKhoeKetQuaCanLamSangs.Any())
            {
                SelectedKhamSucKhoeKetQuaCanLamSangs = new List<KhamSucKhoeKetQuaCanLamSangModel>() {
                    new KhamSucKhoeKetQuaCanLamSangModel()
                    {
                        type = KetQuaCanLamSang.CDHATDCN.ToString(),
                        sort = 0
                    },
                    new KhamSucKhoeKetQuaCanLamSangModel()
                    {
                        type = KetQuaCanLamSang.XNCongThucMau.ToString(),
                        sort = 1
                    },
                    new KhamSucKhoeKetQuaCanLamSangModel()
                    {
                        type = KetQuaCanLamSang.XNNuocTieu.ToString(),
                        sort = 2
                    },
                    new KhamSucKhoeKetQuaCanLamSangModel()
                    {
                        type = KetQuaCanLamSang.XNKhac.ToString(),
                        sort = 3
                    }
                };
            }

            openSyncKetQuaCanLamSangModal = true;
        }
        private void CloseSyncKetQuaCanLamSangModal()
        {
            AlertService.ShowAlert("Đồng bộ kết quả cận lâm sàng thành công!", "success");
            openSyncKetQuaCanLamSangModal = false;
        }

        private async Task OnValidSubmit()
        {
            try
            {
                if (SelectedItem.id <= 0)
                {
                    return;
                }

                IsLoading = true;

                SelectedKhamSucKhoeTienSu.ma_luot_kham = SelectedItem.ma_luot_kham;
                SelectedKhamSucKhoeTienSu.luot_kham = SelectedItem;
                SelectedKhamSucKhoeChuyenKhoa.ma_luot_kham = SelectedItem.ma_luot_kham;
                SelectedKhamSucKhoeChuyenKhoa.luot_kham = SelectedItem;
                SelectedKhamSucKhoeKetLuan.ma_luot_kham = SelectedItem.ma_luot_kham;
                SelectedKhamSucKhoeKetLuan.luot_kham = SelectedItem;
                SelectedKhamSucKhoeTheLuc.ma_luot_kham = SelectedItem.ma_luot_kham;
                SelectedKhamSucKhoeTheLuc.luot_kham = SelectedItem;
                SelectedKhamSucKhoeSanPhuKhoa.ma_luot_kham = SelectedItem.ma_luot_kham;
                SelectedKhamSucKhoeSanPhuKhoa.luot_kham = SelectedItem;

                if (onBSHoHap)
                {
                    SelectedKhamSucKhoeChuyenKhoa.bs_ho_hap = CurrentUser?.full_name;
                    SelectedKhamSucKhoeChuyenKhoa.chu_ky_ho_hap = CurrentUser?.chu_ky_bac_si;
                }
                if (onBSTuanHoan)
                {
                    SelectedKhamSucKhoeChuyenKhoa.bs_tuan_hoan = CurrentUser?.full_name;
                    SelectedKhamSucKhoeChuyenKhoa.chu_ky_tuan_hoan = CurrentUser?.chu_ky_bac_si;
                }
                if (onBSTieuHoa)
                {
                    SelectedKhamSucKhoeChuyenKhoa.bs_tieu_hoa = CurrentUser?.full_name;
                    SelectedKhamSucKhoeChuyenKhoa.chu_ky_tieu_hoa = CurrentUser?.chu_ky_bac_si;
                }
                if (onBSThanTietNieu)
                {
                    SelectedKhamSucKhoeChuyenKhoa.bs_than_tiet_nieu = CurrentUser?.full_name;
                    SelectedKhamSucKhoeChuyenKhoa.chu_ky_than_tiet_nieu = CurrentUser?.chu_ky_bac_si;
                }
                if (onBSNoiTiet)
                {
                    SelectedKhamSucKhoeChuyenKhoa.bs_noi_tiet = CurrentUser?.full_name;
                    SelectedKhamSucKhoeChuyenKhoa.chu_ky_noi_tiet = CurrentUser?.chu_ky_bac_si;
                }
                if (onBSCoXuongKhop)
                {
                    SelectedKhamSucKhoeChuyenKhoa.bs_co_xuong_khop = CurrentUser?.full_name;
                    SelectedKhamSucKhoeChuyenKhoa.chu_ky_co_xuong_khop = CurrentUser?.chu_ky_bac_si;
                }
                if (onBSThanKinh)
                {
                    SelectedKhamSucKhoeChuyenKhoa.bs_than_kinh = CurrentUser?.full_name;
                    SelectedKhamSucKhoeChuyenKhoa.chu_ky_than_kinh = CurrentUser?.chu_ky_bac_si;
                }
                if (onBSTamThan)
                {
                    SelectedKhamSucKhoeChuyenKhoa.bs_tam_than = CurrentUser?.full_name;
                    SelectedKhamSucKhoeChuyenKhoa.chu_ky_tam_than = CurrentUser?.chu_ky_bac_si;
                }
                if (onBSNgoaiKhoa)
                {
                    SelectedKhamSucKhoeChuyenKhoa.bs_ngoai_khoa = CurrentUser?.full_name;
                    SelectedKhamSucKhoeChuyenKhoa.chu_ky_ngoai_khoa = CurrentUser?.chu_ky_bac_si;
                }
                if (onBSMat)
                {
                    SelectedKhamSucKhoeChuyenKhoa.bs_mat = CurrentUser?.full_name;
                    SelectedKhamSucKhoeChuyenKhoa.chu_ky_mat = CurrentUser?.chu_ky_bac_si;
                }
                if (onBSTaiMuiHong)
                {
                    SelectedKhamSucKhoeChuyenKhoa.bs_tmh = CurrentUser?.full_name;
                    SelectedKhamSucKhoeChuyenKhoa.chu_ky_tmh = CurrentUser?.chu_ky_bac_si;
                }
                if (onBSRangHamMat)
                {
                    SelectedKhamSucKhoeChuyenKhoa.bs_rhm = CurrentUser?.full_name;
                    SelectedKhamSucKhoeChuyenKhoa.chu_ky_rhm = CurrentUser?.chu_ky_bac_si;
                }
                if (onBSSanPhuKhoa)
                {
                    SelectedKhamSucKhoeSanPhuKhoa.nguoi_ket_luan = CurrentUser?.full_name;
                    SelectedKhamSucKhoeSanPhuKhoa.chu_ky = CurrentUser?.chu_ky_bac_si;
                }
                if (onBSKetLuan)
                {
                    SelectedKhamSucKhoeKetLuan.nguoi_ket_luan = CurrentUser?.full_name;
                    SelectedKhamSucKhoeKetLuan.chu_ky = CurrentUser?.chu_ky_bac_si;
                    SelectedKhamSucKhoeKetLuan.bs_ket_luan = CurrentUser;
                }

                SelectedKhamSucKhoeKetQuaCanLamSangs = SelectedKhamSucKhoeKetQuaCanLamSangs.Select(c =>
                        {
                            c.luot_kham = SelectedItem;

                            return c;
                        }).ToList();

                string query = "&filter[_and][0][deleted][_eq]=false" +
                               $"&filter[_and][][ma_luot_kham][_contains]={SelectedItem.ma_luot_kham}";

                switch (activeDefTab)
                {
                    case "tab1":
                        if (SelectedKhamSucKhoeTienSu.id > 0)
                        {
                            var result = await KhamSucKhoeTienSuService.UpdateAsync(new List<KhamSucKhoeTienSuModel>() { SelectedKhamSucKhoeTienSu });
                            if (result == null || !result.IsSuccess)
                            {
                                AlertService.ShowAlert("Đã có lỗi xảy ra khi lưu tiền sử bệnh tật!", "danger");
                                return;
                            }
                        }
                        else
                        {
                            var result = await KhamSucKhoeTienSuService.CreateAsync(new List<KhamSucKhoeTienSuModel>() { SelectedKhamSucKhoeTienSu });
                            if (result == null || !result.IsSuccess)
                            {
                                AlertService.ShowAlert("Đã có lỗi xảy ra khi lưu tiền sử bệnh tật!", "danger");
                                return;
                            }
                        }

                        if (SelectedKhamSucKhoeSanPhuKhoa.id > 0)
                        {
                            var result = await KhamSucKhoeSanPhuKhoaService.UpdateAsync(new List<KhamSucKhoeSanPhuKhoaModel>() { SelectedKhamSucKhoeSanPhuKhoa });
                            if (result == null || !result.IsSuccess)
                            {
                                AlertService.ShowAlert("Đã có lỗi xảy ra khi lưu tiền sử khám phụ khoa!", "danger");
                                return;
                            }
                        }
                        else
                        {
                            var result = await KhamSucKhoeSanPhuKhoaService.CreateAsync(new List<KhamSucKhoeSanPhuKhoaModel>() { SelectedKhamSucKhoeSanPhuKhoa });
                            if (result == null || !result.IsSuccess)
                            {
                                AlertService.ShowAlert("Đã có lỗi xảy ra khi lưu tiền sử khám phụ khoa!", "danger");
                                return;
                            }
                        }

                        SelectedKhamSucKhoeSanPhuKhoa = await BaseServiceHelper.GetFirstOrDefaultAsync(KhamSucKhoeSanPhuKhoaService, query);
                        SelectedKhamSucKhoeTienSu = await BaseServiceHelper.GetFirstOrDefaultAsync(KhamSucKhoeTienSuService, query);
                        break;

                    case "tab2":
                        if (SelectedKhamSucKhoeTheLuc.id > 0)
                        {
                            var result = await KhamSucKhoeTheLucService.UpdateAsync(new List<KhamSucKhoeTheLucModel>() { SelectedKhamSucKhoeTheLuc });
                            if (result == null || !result.IsSuccess)
                            {
                                AlertService.ShowAlert("Đã có lỗi xảy ra khi lưu khám thể lực!", "danger");
                                return;
                            }
                        }
                        else
                        {
                            var result = await KhamSucKhoeTheLucService.CreateAsync(new List<KhamSucKhoeTheLucModel>() { SelectedKhamSucKhoeTheLuc });
                            if (result == null || !result.IsSuccess)
                            {
                                AlertService.ShowAlert("Đã có lỗi xảy ra khi lưu khám thể lực!", "danger");
                                return;
                            }
                        }

                        if (SelectedKhamSucKhoeChuyenKhoa.id > 0)
                        {
                            var result = await KhamSucKhoeChuyenKhoaService.UpdateAsync(new List<KhamSucKhoeChuyenKhoaModel>() { SelectedKhamSucKhoeChuyenKhoa });
                            if (result == null || !result.IsSuccess)
                            {
                                AlertService.ShowAlert("Đã có lỗi xảy ra khi lưu khám chuyên khoa!", "danger");
                                return;
                            }
                        }
                        else
                        {
                            var result = await KhamSucKhoeChuyenKhoaService.CreateAsync(new List<KhamSucKhoeChuyenKhoaModel>() { SelectedKhamSucKhoeChuyenKhoa });
                            if (result == null || !result.IsSuccess)
                            {
                                AlertService.ShowAlert("Đã có lỗi xảy ra khi lưu khám chuyên khoa!", "danger");
                                return;
                            }
                        }

                        SelectedKhamSucKhoeChuyenKhoa = await BaseServiceHelper.GetFirstOrDefaultAsync(KhamSucKhoeChuyenKhoaService, query);
                        SelectedKhamSucKhoeTheLuc = await BaseServiceHelper.GetFirstOrDefaultAsync(KhamSucKhoeTheLucService, query);
                        break;

                    case "tab3":
                        if (SelectedKhamSucKhoeKetQuaCanLamSangs.Any(c => c.id > 0))
                        {
                            var result = await KhamSucKhoeKetQuaCanLamSangService.UpdateAsync(SelectedKhamSucKhoeKetQuaCanLamSangs);
                            if (result == null || !result.IsSuccess)
                            {
                                AlertService.ShowAlert("Đã có lỗi xảy ra khi lưu khám cận lâm sàng!", "danger");
                                return;
                            }
                        }
                        else
                        {
                            var result = await KhamSucKhoeKetQuaCanLamSangService.CreateAsync(SelectedKhamSucKhoeKetQuaCanLamSangs);
                            if (result == null || !result.IsSuccess)
                            {
                                AlertService.ShowAlert("Đã có lỗi xảy ra khi lưu khám cận lâm sàng!", "danger");
                                return;
                            }
                        }

                        if (SelectedKhamSucKhoeKetLuan.id > 0)
                        {
                            var result = await KhamSucKhoeKetLuanService.UpdateAsync(new List<KhamSucKhoeKetLuanModel>() { SelectedKhamSucKhoeKetLuan });
                            if (result == null || !result.IsSuccess)
                            {
                                AlertService.ShowAlert("Đã có lỗi xảy ra khi lưu kết luận!", "danger");
                                return;
                            }
                        }
                        else
                        {
                            var result = await KhamSucKhoeKetLuanService.CreateAsync(new List<KhamSucKhoeKetLuanModel>() { SelectedKhamSucKhoeKetLuan });
                            if (result == null || !result.IsSuccess)
                            {
                                AlertService.ShowAlert("Đã có lỗi xảy ra khi lưu kết luận!", "danger");
                                return;
                            }
                        }

                        SelectedKhamSucKhoeKetLuan = await BaseServiceHelper.GetFirstOrDefaultAsync(KhamSucKhoeKetLuanService, query);

                        var resKhamSucKhoeKetQuaCanLamSang = await KhamSucKhoeKetQuaCanLamSangService.GetAllAsync(query);
                        SelectedKhamSucKhoeKetQuaCanLamSangs = resKhamSucKhoeKetQuaCanLamSang?.IsSuccess == true && resKhamSucKhoeKetQuaCanLamSang.Data != null
                            ? resKhamSucKhoeKetQuaCanLamSang.Data
                            : new List<KhamSucKhoeKetQuaCanLamSangModel>();

                        if (!SelectedKhamSucKhoeKetQuaCanLamSangs.Any())
                        {
                            SelectedKhamSucKhoeKetQuaCanLamSangs = new List<KhamSucKhoeKetQuaCanLamSangModel>
                            {
                                new KhamSucKhoeKetQuaCanLamSangModel { type = KetQuaCanLamSang.CDHATDCN.ToString(), sort = 0 },
                                new KhamSucKhoeKetQuaCanLamSangModel { type = KetQuaCanLamSang.XNCongThucMau.ToString(), sort = 1 },
                                new KhamSucKhoeKetQuaCanLamSangModel { type = KetQuaCanLamSang.XNNuocTieu.ToString(), sort = 2 },
                                new KhamSucKhoeKetQuaCanLamSangModel { type = KetQuaCanLamSang.XNKhac.ToString(), sort = 3 }
                            };
                        }

                        break;
                }
            }
            catch
            {
                AlertService.ShowAlert("Đã có lỗi xảy ra khi lưu thông tin!", "danger");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task OnEndSubmit()
        {
            try
            {
                IsLoading = true;

                SelectedItem.status = Model.Base.Status.published;

                var result = await MainService.UpdateAsync(new List<SoKhamSucKhoeModel>() { SelectedItem });
                if (result == null || !result.IsSuccess)
                {
                    AlertService.ShowAlert("Đã có lỗi xảy ra khi lưu kết luận!", "danger");
                    return;
                }
            }
            catch
            {
                AlertService.ShowAlert("Đã có lỗi xảy ra khi lưu thông tin!", "danger");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Get HTML content of the medical form (optimized for large content)
        /// </summary>
        /// <returns>HTML string of the medical form content</returns>
        public async Task<string> GetMedicalFormHtmlAsync()
        {
            try
            {
                // Wait for DOM to render completely
                await Task.Delay(100);

                // Use shorter timeout and try chunked approach first
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

                // Try to get content size first
                var contentLength = await JsRuntime.InvokeAsync<int>("getMedicalFormContentLength", cts.Token);
                Console.WriteLine($"Medical form content length: {contentLength} characters");

                if (contentLength > 500000) // If content is larger than 500KB
                {
                    Console.WriteLine("Content is large, using extended timeout...");
                    // Use longer timeout for large content
                    using var largeCts = new CancellationTokenSource(TimeSpan.FromMinutes(2));
                    var largeHtmlContent = await JsRuntime.InvokeAsync<string>("getMedicalFormHtml", largeCts.Token);

                    if (!string.IsNullOrEmpty(largeHtmlContent))
                    {
                        Console.WriteLine($"Successfully retrieved large HTML content. Length: {largeHtmlContent.Length} characters");
                        return largeHtmlContent;
                    }

                    Console.WriteLine("Large content retrieval failed, trying normal approach...");
                }

                // For smaller content, use direct approach
                var htmlContent = await JsRuntime.InvokeAsync<string>("getMedicalFormHtml", cts.Token);

                if (string.IsNullOrEmpty(htmlContent))
                {
                    Console.WriteLine("ERROR: HTML content is null or empty!");
                    return string.Empty;
                }

                Console.WriteLine($"Successfully retrieved HTML content. Length: {htmlContent.Length} characters");
                return htmlContent;
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine($"ERROR: Timeout while getting HTML content - {ex.Message}");
                return string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Failed to get HTML content - {ex.Message}. Trying simple innerHTML...");

                // Fallback: Try to get just innerHTML without full styling
                try
                {
                    using var fallbackCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                    var innerHTML = await JsRuntime.InvokeAsync<string>("getMedicalFormInnerHTML", fallbackCts.Token);

                    if (!string.IsNullOrEmpty(innerHTML))
                    {
                        Console.WriteLine($"Fallback successful. Length: {innerHTML.Length}");
                        return innerHTML;
                    }
                }
                catch (Exception fallbackEx)
                {
                    Console.WriteLine($"ERROR: Fallback also failed - {fallbackEx.Message}");
                }

                return string.Empty;
            }
        }



        private async Task ExportPDF()
        {
            if (IsLoading || CurrentUser == null || SelectedItem.id <= 0)
            {
                return;
            }

            try
            {
                imageWebRootPath = Path.Combine(WebHostEnvironment.WebRootPath, _imagesFolder, $"{SelectedItem.ma_luot_kham}");

                // Hiển thị thông báo đang xử lý
                AlertService?.ShowAlert("Đang xử lý ảnh chữ ký và tạo PDF, vui lòng đợi...", "info");

                string htmlContent = await GetMedicalFormHtmlAsync();
                if (string.IsNullOrEmpty(htmlContent))
                {
                    Console.WriteLine("ERROR: HTML content is null or empty!");
                    AlertService?.ShowAlert("Không thể lấy nội dung để xuất PDF - HTML content empty", "danger");
                    return;
                }

                // Log HTML content details for debugging
                var htmlPreview = htmlContent.Length > 500 ? htmlContent.Substring(0, 500) + "..." : htmlContent;
                Console.WriteLine($"HTML preview (first 500 chars): {htmlPreview}");

                // Check for potential problematic content
                var hasImages = htmlContent.Contains("<img");
                var hasSvg = htmlContent.Contains("<svg");
                var hasLargeTable = htmlContent.Contains("ksk-table");
                Console.WriteLine($"HTML analysis: Images={hasImages}, SVG={hasSvg}, LargeTable={hasLargeTable}");

                // Log file size in different units
                var sizeKB = htmlContent.Length / 1024.0;
                Console.WriteLine($"HTML size: {htmlContent.Length} chars = {sizeKB:F2} KB");

                // Configure PDF settings
                Console.WriteLine("Step 3: Cấu hình PDF settings...");
                var pdfSettings = new PdfSettings
                {
                    FileName = $"{SelectedItem.ma_luot_kham}_{DateTime.Now:yyyyMMdd}.pdf",
                    PageSize = "A4",
                    Orientation = "Portrait",
                    MarginTop = 10,
                    MarginBottom = 10,
                    MarginLeft = 10,
                    MarginRight = 10
                };
                Console.WriteLine($"PDF filename: {pdfSettings.FileName}");

                // Generate PDF từ HTML content lấy từ client
                Console.WriteLine("Step 4: Đang tạo PDF với PuppeteerSharp...");

                byte[] pdfBytes;

                pdfBytes = PdfService.GeneratePdfFromHtml(htmlContent, pdfSettings);

                // Convert to base64 for download
                Console.WriteLine("Step 6: Chuyển đổi PDF sang base64...");
                var base64 = Convert.ToBase64String(pdfBytes);
                var dataUrl = $"data:application/pdf;base64,{base64}";
                Console.WriteLine($"Base64 length: {base64.Length}");

                // Trigger download via JavaScript
                Console.WriteLine("Step 7: Trigger download...");
                await JsRuntime.InvokeVoidAsync("downloadFile", dataUrl, pdfSettings.FileName);
                AlertService?.ShowAlert("Xuất PDF thành công!", "success");

                Console.WriteLine("Step 8: Hoàn thành thành công!");

                // Xóa ảnh chữ ký sau khi export PDF
                try
                {
                    // Xóa folder con chứa ảnh của mã lượt khám
                    string folderPath = WebHostEnvironment.WebRootPath + _imagesFolder;
                    if (Directory.Exists(folderPath))
                    {
                        // Xóa tất cả file trong folder
                        var files = Directory.GetFiles(folderPath);
                        foreach (var file in files)
                        {
                            File.Delete(file);
                        }
                        Console.WriteLine($"Step 9: Xóa folder và ảnh chữ ký thành công: {folderPath}");
                    }

                    // Xóa các ảnh có thể bị tạo nhầm ở thư mục gốc /images/
                    string rootImagesPath = Path.Combine(WebHostEnvironment.WebRootPath, "images");
                    if (Directory.Exists(rootImagesPath))
                    {
                        // Tìm và xóa các file có tên chứa mã lượt khám hoặc tên chữ ký
                        var signatureFiles = Directory.GetFiles(rootImagesPath, "*")
                            .Where(f => Path.GetFileName(f).Contains(SelectedItem.ma_luot_kham ?? string.Empty) ||
                                        Path.GetFileName(f).Contains("ket_luan") ||
                                        Path.GetFileName(f).Contains("tuan_hoan") ||
                                        Path.GetFileName(f).Contains("chu_ky"))
                            .ToArray();

                        foreach (var file in signatureFiles)
                        {
                            File.Delete(file);
                            Console.WriteLine($"Xóa file nhầm: {Path.GetFileName(file)}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi xóa ảnh: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== ERROR in ExportPDF ===");
                Console.WriteLine($"Error type: {ex.GetType().Name}");
                Console.WriteLine($"Error message: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }

                var errorMsg = $"Lỗi khi xuất PDF: {ex.Message}";
                AlertService?.ShowAlert(errorMsg, "danger");
            }
            finally
            {
                Console.WriteLine("=== Debug ExportPDF - Kết thúc ===");
                try
                {
                    CleanupSignatureImages();
                }
                catch (Exception cleanupEx)
                {
                    Console.WriteLine($"Lỗi khi dọn dẹp ảnh chữ ký: {cleanupEx.Message}");
                }
            }
        }

        private void OnTabChanged(string tab)
        {
            activeDefTab = tab;

            if (activeDefTab == "tab3")
            {
                // Wait for modal to render
                _ = Task.Run(async () =>
                {
                    await Task.Delay(500);
                    await JsRuntime.InvokeVoidAsync("initializeDatePicker");
                });
            }
        }

        private async Task HandleFileSelect(InputFileChangeEventArgs e)
        {
            var files = e.GetMultipleFiles();
            if (files != null && files.Any())
            {
                await ProcessFile(files[0]);
            }
        }

        private async Task ProcessFile(IBrowserFile file)
        {
            var maxAllowSize = 5 * 1024 * 1024;
            if (file.Size <= maxAllowSize) // 5MB max size
            {
                try
                {
                    var buffer = new byte[file.Size];
                    await file.OpenReadStream(maxAllowSize).ReadExactlyAsync(buffer);

                    var fileUploaded = await FileService.UploadFileAsync(file, UploadFileCRUD);

                    if (
                        fileUploaded != null
                        && fileUploaded.IsSuccess
                        && fileUploaded.Data != null
                        && !string.IsNullOrEmpty(fileUploaded.Data.filename_download)
                    )
                    {
                        SelectedUser.avatar = fileUploaded.Data.id.ToString();
                        var updateResult = await UserService.UpdateUserAvatarAsync(SelectedUser);
                        if (updateResult.IsSuccess)
                        {
                            AlertService.ShowAlert("Cập nhật ảnh đại diện thành công!", "success");
                        }
                        else
                        {
                            AlertService.ShowAlert(updateResult.Message ?? "Lỗi khi cập nhật ảnh đại diện", "danger");
                        }
                    }
                    else
                    {
                        await JsRuntime.InvokeVoidAsync("alert", "Failed to upload image.");
                    }
                }
                catch (Exception ex)
                {
                    await JsRuntime.InvokeVoidAsync("alert", $"Error processing image: {ex.Message}");
                }
                finally
                {
                    StateHasChanged();
                }
            }
            else
            {
                await JsRuntime.InvokeVoidAsync("alert", "File size exceeds 5MB limit");
            }
        }

        private async Task OnValueChanged(ChangeEventArgs e, string fieldName, bool isFilter = false, bool isDate = true)
        {
            try
            {
                if (!isDate)
                {
                    if (e.Value == null || string.IsNullOrEmpty(e.Value.ToString()))
                    {
                        ReflectionHelper.SetFieldValue(this, SelectedItem, fieldName, null);
                        return;
                    }
                    var value = e.Value.ToString();
                    ReflectionHelper.SetFieldValue(this, SelectedItem, fieldName, value);
                    return;
                }

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

        private async Task ContractFilterChanged(ContractModel? contract)
        {
            _contractFilter = contract;

            await LoadData();
        }

        private async Task KhamSucKhoeCongTyChanged(KhamSucKhoeCongTyModel? khamSucKhoeCongTy)
        {
            _khamSucKhoeCongTyFilter = khamSucKhoeCongTy;

            await LoadData();
        }

        private void OnTinhChatKinhChanged(string value)
        {
            SelectedKhamSucKhoeSanPhuKhoa.tinh_chat_kinh = value;
        }

        private void OnDauBungKinhChanged(string value)
        {
            SelectedKhamSucKhoeSanPhuKhoa.dau_bung_kinh = value == YesNo.Co.ToString();
        }

        private void OnSoLanMoPhuKhoaChanged(int value)
        {
            SelectedKhamSucKhoeSanPhuKhoa.so_lan_mo_san_phu_khoa = value;
        }

        private void OnApDungBPPTChanged(string value)
        {
            SelectedKhamSucKhoeSanPhuKhoa.ap_dung_bptt = value == YesNo.Co.ToString();
        }

        private void OnParaChanged(ChangeEventArgs value, int index)
        {
            switch (index)
            {
                case 0:
                    para1 = value.Value?.ToString() ?? string.Empty;
                    break;
                case 1:
                    para2 = value.Value?.ToString() ?? string.Empty;
                    break;
                case 2:
                    para3 = value.Value?.ToString() ?? string.Empty;
                    break;
                case 3:
                    para4 = value.Value?.ToString() ?? string.Empty;
                    break;
            }

            SelectedKhamSucKhoeSanPhuKhoa.para = $"{para1}|{para2}|{para3}|{para4}";
        }

        private void OnKetQuaCanLamSangChanged(KhamSucKhoeCanLamSangModel? selected, KhamSucKhoeKetQuaCanLamSangModel item)
        {
            try
            {
                item.ket_qua_cls = selected;
                item.ket_qua = $"{selected?.ten_cls} | {selected?.ket_qua_cls} | {selected?.danh_gia_cls} | {selected?.chi_so_cls}";
            }
            catch (Exception ex)
            {
                AlertService.ShowAlert($"Lỗi khi xử lý dữ liệu: {ex.Message}", "danger");
            }
        }


        /// <summary>
        /// Render signature as HTML - either image or text
        /// </summary>
        /// <param name="signatureData">Signature data (hex string or text)</param>
        /// <param name="fallbackText">Fallback text if signature is empty</param>
        /// <param name="fileName">File name for saving image</param>
        /// <param name="maxWidth">Maximum width for signature image</param>
        /// <param name="maxHeight">Maximum height for signature image</param>
        /// <returns>MarkupString containing signature HTML</returns>
        private MarkupString RenderSignature(string? signatureData, string? fallbackText = "", string? fileName = "", int maxWidth = 120, int maxHeight = 60)
        {
            if (string.IsNullOrEmpty(signatureData))
            {
                return new MarkupString();
            }

            var html = signatureData.GetSignatureDisplayHtml(fallbackText,
                                                             fileName,
                                                             maxWidth,
                                                             maxHeight,
                                                             imageWebRootPath,
                                                             Configuration["DrCoreApi:BaseUrlImage"]);
            return new MarkupString(html);
        }

        /// <summary>
        /// Delete all signature images in the component's folder
        /// </summary>
        private void CleanupSignatureImages()
        {
            if (string.IsNullOrEmpty(SelectedItem.ma_luot_kham))
            {
                return;
            }

            try
            {
                if (Directory.Exists(imageWebRootPath))
                {
                    // Delete all files in the folder
                    var files = Directory.GetFiles(imageWebRootPath);
                    foreach (var file in files)
                    {
                        File.Delete(file);
                    }

                    // Delete the folder itself
                    Directory.Delete(imageWebRootPath);
                    Console.WriteLine($"Cleanup: Deleted signature folder: {imageWebRootPath}");
                }

                // Also clean up any stray signature files in root images folder
                string rootImagesPath = Path.Combine(WebHostEnvironment.WebRootPath, "images", $"{SelectedItem.ma_luot_kham}");
                if (Directory.Exists(rootImagesPath))
                {
                    // Find and delete files that might be related to this medical record
                    var signatureFiles = Directory.GetFiles(rootImagesPath, "*");

                    foreach (var file in signatureFiles)
                    {
                        File.Delete(file);
                        Console.WriteLine($"Cleanup: Deleted stray signature file: {Path.GetFileName(file)}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cleaning up signature images: {ex.Message}");
            }
        }
    }
}
