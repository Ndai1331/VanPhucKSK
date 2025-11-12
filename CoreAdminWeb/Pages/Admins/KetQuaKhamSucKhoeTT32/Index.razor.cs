using CoreAdminWeb.Enums;
using CoreAdminWeb.Extensions;
using CoreAdminWeb.Helpers;
using CoreAdminWeb.Model;
using CoreAdminWeb.Model.Contract;
using CoreAdminWeb.Model.KhamSucKhoes;
using CoreAdminWeb.Model.User;
using CoreAdminWeb.Services.BaseServices;
using CoreAdminWeb.Services.Files;
using CoreAdminWeb.Services.PDFService;
using CoreAdminWeb.Services.Users;
using CoreAdminWeb.Shared.Base;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using MudBlazor;
using System.Dynamic;

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
                               IBaseGetService<KetQuaCanLamSangModel> KetQuaCanLamSangService,
                               IBaseDetailService<KhamSucKhoeNgheNghiepModel> KhamSucKhoeNgheNghiepService,
                               IPdfService PdfService,
                               IConfiguration Configuration,
                               IWebHostEnvironment WebHostEnvironment) : BlazorCoreBase
    {
        #region Constants
        private const string DEFAULT_PROFILE_IMAGE = "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='120' height='160' viewBox='0 0 120 160'%3E%3Crect width='120' height='160' fill='%23f8f9fa' stroke='%23dee2e6' stroke-width='1'/%3E%3Ctext x='60' y='80' text-anchor='middle' fill='%236c757d' font-size='12'%3EẢnh 4x6 cm%3C/text%3E%3C/svg%3E";
        #endregion

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
        private bool openSoKhamSucKhoeModal { get; set; } = false;
        private bool onReadonly => SelectedItem.status == Model.Base.Status.published || SelectedItem.MaDotKham?.ma_hop_dong_ksk?.status == TrangThaiHopDong.locked;

        private bool isShowOnlyMe { get; set; } = false;
        private bool onBS { get; set; } = false;
        private bool onBSHoHap => CurrentUser != null && SelectedKhamSucKhoeCongTy.bs_ho_hap?.id == CurrentUser.id || SelectedKhamSucKhoeCongTy.kham_noi_vien == true && onBS;
        private bool onBSTuanHoan => CurrentUser != null && SelectedKhamSucKhoeCongTy.bs_tuan_hoan?.id == CurrentUser.id || SelectedKhamSucKhoeCongTy.kham_noi_vien == true && onBS;
        private bool onBSTieuHoa => CurrentUser != null && SelectedKhamSucKhoeCongTy.bs_tieu_hoa?.id == CurrentUser.id || SelectedKhamSucKhoeCongTy.kham_noi_vien == true && onBS;
        private bool onBSThanTietNieu => CurrentUser != null && SelectedKhamSucKhoeCongTy.bs_than_tiet_nieu?.id == CurrentUser.id || SelectedKhamSucKhoeCongTy.kham_noi_vien == true && onBS;
        private bool onBSNoiTiet => CurrentUser != null && SelectedKhamSucKhoeCongTy.bs_noi_tiet?.id == CurrentUser.id || SelectedKhamSucKhoeCongTy.kham_noi_vien == true && onBS;
        private bool onBSCoXuongKhop => CurrentUser != null && SelectedKhamSucKhoeCongTy.bs_co_xuong_khop?.id == CurrentUser.id || SelectedKhamSucKhoeCongTy.kham_noi_vien == true && onBS;
        private bool onBSThanKinh => CurrentUser != null && SelectedKhamSucKhoeCongTy.bs_than_kinh?.id == CurrentUser.id || SelectedKhamSucKhoeCongTy.kham_noi_vien == true && onBS;
        private bool onBSTamThan => CurrentUser != null && SelectedKhamSucKhoeCongTy.bs_tam_than?.id == CurrentUser.id || SelectedKhamSucKhoeCongTy.kham_noi_vien == true && onBS;
        private bool onBSNgoaiKhoa => CurrentUser != null && SelectedKhamSucKhoeCongTy.bs_ngoai_khoa?.id == CurrentUser.id || SelectedKhamSucKhoeCongTy.kham_noi_vien == true && onBS;
        private bool onBSMat => CurrentUser != null && SelectedKhamSucKhoeCongTy.bs_mat?.id == CurrentUser.id || SelectedKhamSucKhoeCongTy.kham_noi_vien == true && onBS;
        private bool onBSTaiMuiHong => CurrentUser != null && SelectedKhamSucKhoeCongTy.bs_tai_mui_hong?.id == CurrentUser.id || SelectedKhamSucKhoeCongTy.kham_noi_vien == true && onBS;
        private bool onBSRangHamMat => CurrentUser != null && SelectedKhamSucKhoeCongTy.bs_rang_ham_mat?.id == CurrentUser.id || SelectedKhamSucKhoeCongTy.kham_noi_vien == true && onBS;
        private bool onBSSanPhuKhoa => CurrentUser != null && SelectedKhamSucKhoeCongTy.bs_san_phu_khoa?.id == CurrentUser.id || SelectedKhamSucKhoeCongTy.kham_noi_vien == true && onBS;
        private bool onBSKetLuan => CurrentUser != null && SelectedKhamSucKhoeCongTy.bs_ket_luan?.id == CurrentUser.id || SelectedKhamSucKhoeCongTy.kham_noi_vien == true && onBS;

        private string imageWebRootPath { get; set; } = string.Empty;

        private Dictionary<int, List<PhanLoaiSucKhoeModel>> SelectedPhanLoaiSucKhoes { get; set; } = new();
        private List<PhanLoaiSucKhoeModel> PhanLoaiSucKhoes { get; set; } = new();

        private Dictionary<int, List<UserModel>> SelectedUsers { get; set; } = new();
        private List<UserModel> Users { get; set; } = new();
        private string currentFilterPhanLoaiSucKhoe { get; set; } = string.Empty;

        
        
        private int renderKey { get; set; } = 0;

        private dynamic dynamicTheLucObj = new ExpandoObject();
        private dynamic dynamicSanPhuKhoaObj = new ExpandoObject();
        private dynamic dynamicChuyenKhoaObj = new ExpandoObject();
        private dynamic dynamicKetLuanObj = new ExpandoObject();
        private dynamic dynamicTienSuObj = new ExpandoObject();
        private List<dynamic> dynamicKhamCLSObj = new List<dynamic>();

        private dynamic dynamicTheLucObjOriginal = new ExpandoObject();
        private dynamic dynamicSanPhuKhoaObjOriginal = new ExpandoObject();
        private dynamic dynamicChuyenKhoaObjOriginal = new ExpandoObject();
        private dynamic dynamicKetLuanObjOriginal = new ExpandoObject();
        private dynamic dynamicTienSuObjOriginal = new ExpandoObject();
        private List<dynamic> dynamicKhamCLSObjOriginal = new List<dynamic>();

        private bool isShowConfirmModal { get; set; }
        private string confirmMessage { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                var resUser = await UserService.GetCurrentUserAsync();
                if (resUser.IsSuccess)
                {
                    CurrentUser = resUser.Data;

                    onBS = CurrentUser?.role?.ToLower() == CurrentSetting.doctor_role_id?.ToLower().ToString();
                }

                await LoadPhanLoaiSucKhoeSelect2(PhanLoaiSucKhoes, string.Empty, CancellationToken.None);

                SetProfileImagePlaceholder();
                //await LoadData(true);
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

        private async Task LoadData(bool isReset = false)
        {
            Loading.Show();

            if (isReset)
            {
                ResetPage();
            }

            BuildPaginationQuery(Page, PageSize);
            BuilderQuery += $"&filter[_and][0][deleted][_eq]=false";
            BuilderQuery += $"&filter[_and][1][MaDotKham][deleted][_eq]=false";

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

                    if (Page > TotalPages)
                    {
                        await SelectedPage(TotalPages);
                    }
                }

                if (MainModels.Any())
                {
                    openSoKhamSucKhoeModal = true;
                }
                else
                {
                    AlertService.ShowAlert("Không tìm thấy thông tin sổ khám sức khỏe!", "danger");
                }
            }
            else
            {
                MainModels = new List<SoKhamSucKhoeModel>();
            }
            OnResetData();
            Loading.Hide();
            StateHasChanged();
        }

        private async Task OnRowClick(int soKhamSKId)
        {
            if (SelectedItem.id != soKhamSKId)
            {
                openSoKhamSucKhoeModal = false;
                OnResetData();

                onBS = CurrentUser?.role?.ToLower() == CurrentSetting.doctor_role_id?.ToLower().ToString();

                if (!isShowOnlyMe || isShowOnlyMe && onBS)
                {
                    activeDefTab = "tab1";
                }
                else if (!isShowOnlyMe || isShowOnlyMe && (onBS || onBSTuanHoan || onBSHoHap || onBSTieuHoa || onBSThanTietNieu || onBSNoiTiet || onBSCoXuongKhop || onBSThanKinh || onBSThanKinh || onBSTamThan || onBSNgoaiKhoa || onBSSanPhuKhoa || onBSMat || onBSTaiMuiHong || onBSRangHamMat))
                {
                    activeDefTab = "tab2";
                }
                else if (!isShowOnlyMe || isShowOnlyMe && onBSKetLuan)
                {
                    activeDefTab = "tab3";
                }
                else
                {
                    activeDefTab = "";
                }

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
            Loading.Show();
            dynamicTheLucObj = new ExpandoObject();
            dynamicSanPhuKhoaObj = new ExpandoObject();
            dynamicChuyenKhoaObj = new ExpandoObject();
            dynamicKetLuanObj = new ExpandoObject();
            dynamicTienSuObj = new ExpandoObject();
            dynamicKhamCLSObj = new List<dynamic>();

            var resSoKhamSK = await MainService.GetByIdAsync(soKhamSKId.ToString());
            if (resSoKhamSK?.IsSuccess == true && resSoKhamSK.Data != null)
            {
                SelectedItem = resSoKhamSK.Data;

                if (SelectedItem.benh_nhan == null)
                {
                    string queryBenhNhan = $"&filter[_and][][ma_tai_khoan][_eq]={SelectedItem.ma_benh_nhan}";
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
                string query = $"filter[_and][][luot_kham][_eq]={SelectedItem.id}";

                var tasks = new[]
                {
                    BaseServiceHelper.LoadSingleRecordAsync(KhamSucKhoeChuyenKhoaService, query, r => SelectedKhamSucKhoeChuyenKhoa = r ?? new KhamSucKhoeChuyenKhoaModel()),
                    BaseServiceHelper.LoadSingleRecordAsync(KhamSucKhoeKetLuanService, query, r => SelectedKhamSucKhoeKetLuan = r ?? new KhamSucKhoeKetLuanModel()),
                    BaseServiceHelper.LoadSingleRecordAsync(KhamSucKhoeSanPhuKhoaService, query, r => SelectedKhamSucKhoeSanPhuKhoa = r ?? new KhamSucKhoeSanPhuKhoaModel()),
                    BaseServiceHelper.LoadSingleRecordAsync(KhamSucKhoeTheLucService, query, r => SelectedKhamSucKhoeTheLuc = r ?? new KhamSucKhoeTheLucModel()),
                    BaseServiceHelper.LoadSingleRecordAsync(KhamSucKhoeTienSuService, query, r => SelectedKhamSucKhoeTienSu = r ?? new KhamSucKhoeTienSuModel()),
                    BaseServiceHelper.LoadSingleRecordAsync(KhamSucKhoeCongTyService, $"filter[_and][][id][_eq]={SelectedItem.MaDotKham?.id}", r => SelectedKhamSucKhoeCongTy = r ?? new KhamSucKhoeCongTyModel()),
                    BaseServiceHelper.LoadSingleRecordAsync(KhamSucKhoeNgheNghiepService, query, r => SelectedKhamSucKhoeNgheNghiep = r ?? new KhamSucKhoeNgheNghiepModel()),
                    BaseServiceHelper.LoadMultipleRecordAsync(KhamSucKhoeKetQuaCanLamSangService, $"filter[_and][][ma_luot_kham][_eq]={SelectedItem.ma_luot_kham}", r => SelectedKhamSucKhoeKetQuaCanLamSangs = r ?? new List<KhamSucKhoeKetQuaCanLamSangModel>()),
                };

                await Task.WhenAll(tasks);

                if (string.IsNullOrEmpty(SelectedItem.nguoi_lap))
                {
                    SelectedItem.nguoi_lap = SelectedKhamSucKhoeCongTy.nguoi_lap_so?.full_name;
                }

                var ketQuas = new List<KhamSucKhoeKetQuaCanLamSangModel>
                    {
                        new KhamSucKhoeKetQuaCanLamSangModel { type = KetQuaCanLamSang.CDHATDCN.ToString(), sort = 0 },
                        new KhamSucKhoeKetQuaCanLamSangModel { type = KetQuaCanLamSang.XNCongThucMau.ToString(), sort = 1 },
                        new KhamSucKhoeKetQuaCanLamSangModel { type = KetQuaCanLamSang.XNNuocTieu.ToString(), sort = 2 },
                        new KhamSucKhoeKetQuaCanLamSangModel { type = KetQuaCanLamSang.XNKhac.ToString(), sort = 3 }
                    };

                foreach (var item in ketQuas)
                {
                    var existing = SelectedKhamSucKhoeKetQuaCanLamSangs.FirstOrDefault(c => c.type == item.type);
                    if (existing == null)
                    {
                        SelectedKhamSucKhoeKetQuaCanLamSangs.Add(item);
                    }
                    else
                    {
                        existing.sort = item.sort;
                    }
                }

                if (!string.IsNullOrEmpty(SelectedKhamSucKhoeSanPhuKhoa.para))
                {
                    var paraSplit = SelectedKhamSucKhoeSanPhuKhoa.para
                        .Split(new[] { '|', '-' }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
                    if (paraSplit.Length == 1)
                    {
                        paraSplit = SelectedKhamSucKhoeSanPhuKhoa.para.SplitStringByTwoChars();
                    }

                    para1 = paraSplit.Length > 0 ? paraSplit[0].Trim() : string.Empty;
                    para2 = paraSplit.Length > 1 ? paraSplit[1].Trim() : string.Empty;
                    para3 = paraSplit.Length > 2 ? paraSplit[2].Trim() : string.Empty;
                    para4 = paraSplit.Length > 3 ? paraSplit[3].Trim() : string.Empty;
                }

                if (SelectedKhamSucKhoeKetLuan.phan_loai_suc_khoe != null && !PhanLoaiSucKhoes.Any(c => c.id == SelectedKhamSucKhoeKetLuan.phan_loai_suc_khoe.id))
                {
                    PhanLoaiSucKhoes.Add(SelectedKhamSucKhoeKetLuan.phan_loai_suc_khoe);
                }

                if (SelectedKhamSucKhoeSanPhuKhoa.phan_loai != null && !PhanLoaiSucKhoes.Any(c => c.id == SelectedKhamSucKhoeSanPhuKhoa.phan_loai.id))
                {
                    PhanLoaiSucKhoes.Add(SelectedKhamSucKhoeSanPhuKhoa.phan_loai);
                }

                if (SelectedKhamSucKhoeChuyenKhoa.pl_nk_tuan_hoan != null && !PhanLoaiSucKhoes.Any(c => c.id == SelectedKhamSucKhoeChuyenKhoa.pl_nk_tuan_hoan.id))
                {
                    PhanLoaiSucKhoes.Add(SelectedKhamSucKhoeChuyenKhoa.pl_nk_tuan_hoan);
                }

                if (SelectedKhamSucKhoeChuyenKhoa.pl_nk_ho_hap != null && !PhanLoaiSucKhoes.Any(c => c.id == SelectedKhamSucKhoeChuyenKhoa.pl_nk_ho_hap.id))
                {
                    PhanLoaiSucKhoes.Add(SelectedKhamSucKhoeChuyenKhoa.pl_nk_ho_hap);
                }

                if (SelectedKhamSucKhoeChuyenKhoa.pl_nk_tieu_hoa != null && !PhanLoaiSucKhoes.Any(c => c.id == SelectedKhamSucKhoeChuyenKhoa.pl_nk_tieu_hoa.id))
                {
                    PhanLoaiSucKhoes.Add(SelectedKhamSucKhoeChuyenKhoa.pl_nk_tieu_hoa);
                }

                if (SelectedKhamSucKhoeChuyenKhoa.pl_nk_than_tiet_nieu != null && !PhanLoaiSucKhoes.Any(c => c.id == SelectedKhamSucKhoeChuyenKhoa.pl_nk_than_tiet_nieu.id))
                {
                    PhanLoaiSucKhoes.Add(SelectedKhamSucKhoeChuyenKhoa.pl_nk_than_tiet_nieu);
                }

                if (SelectedKhamSucKhoeChuyenKhoa.pl_nk_noi_tiet != null && !PhanLoaiSucKhoes.Any(c => c.id == SelectedKhamSucKhoeChuyenKhoa.pl_nk_noi_tiet.id))
                {
                    PhanLoaiSucKhoes.Add(SelectedKhamSucKhoeChuyenKhoa.pl_nk_noi_tiet);
                }

                if (SelectedKhamSucKhoeChuyenKhoa.pl_nk_co_xuong_khop != null && !PhanLoaiSucKhoes.Any(c => c.id == SelectedKhamSucKhoeChuyenKhoa.pl_nk_co_xuong_khop.id))
                {
                    PhanLoaiSucKhoes.Add(SelectedKhamSucKhoeChuyenKhoa.pl_nk_co_xuong_khop);
                }

                if (SelectedKhamSucKhoeChuyenKhoa.pl_nk_than_kinh != null && !PhanLoaiSucKhoes.Any(c => c.id == SelectedKhamSucKhoeChuyenKhoa.pl_nk_than_kinh.id))
                {
                    PhanLoaiSucKhoes.Add(SelectedKhamSucKhoeChuyenKhoa.pl_nk_than_kinh);
                }

                if (SelectedKhamSucKhoeChuyenKhoa.pl_nk_tam_than != null && !PhanLoaiSucKhoes.Any(c => c.id == SelectedKhamSucKhoeChuyenKhoa.pl_nk_tam_than.id))
                {
                    PhanLoaiSucKhoes.Add(SelectedKhamSucKhoeChuyenKhoa.pl_nk_tam_than);
                }

                if (SelectedKhamSucKhoeChuyenKhoa.pl_ngoai_khoa != null && !PhanLoaiSucKhoes.Any(c => c.id == SelectedKhamSucKhoeChuyenKhoa.pl_ngoai_khoa.id))
                {
                    PhanLoaiSucKhoes.Add(SelectedKhamSucKhoeChuyenKhoa.pl_ngoai_khoa);
                }

                if (SelectedKhamSucKhoeChuyenKhoa.pl_da_lieu != null && !PhanLoaiSucKhoes.Any(c => c.id == SelectedKhamSucKhoeChuyenKhoa.pl_da_lieu.id))
                {
                    PhanLoaiSucKhoes.Add(SelectedKhamSucKhoeChuyenKhoa.pl_da_lieu);
                }

                if (SelectedKhamSucKhoeChuyenKhoa.pl_mat != null && !PhanLoaiSucKhoes.Any(c => c.id == SelectedKhamSucKhoeChuyenKhoa.pl_mat.id))
                {
                    PhanLoaiSucKhoes.Add(SelectedKhamSucKhoeChuyenKhoa.pl_mat);
                }

                if (SelectedKhamSucKhoeChuyenKhoa.pl_tmh != null && !PhanLoaiSucKhoes.Any(c => c.id == SelectedKhamSucKhoeChuyenKhoa.pl_tmh.id))
                {
                    PhanLoaiSucKhoes.Add(SelectedKhamSucKhoeChuyenKhoa.pl_tmh);
                }

                if (SelectedKhamSucKhoeChuyenKhoa.pl_rhm != null && !PhanLoaiSucKhoes.Any(c => c.id == SelectedKhamSucKhoeChuyenKhoa.pl_rhm.id))
                {
                    PhanLoaiSucKhoes.Add(SelectedKhamSucKhoeChuyenKhoa.pl_rhm);
                }

                dynamicTheLucObjOriginal = SelectedKhamSucKhoeTheLuc.DeepClone();
                dynamicSanPhuKhoaObjOriginal = SelectedKhamSucKhoeSanPhuKhoa.DeepClone();
                dynamicChuyenKhoaObjOriginal = SelectedKhamSucKhoeChuyenKhoa.DeepClone();
                dynamicKetLuanObjOriginal = SelectedKhamSucKhoeKetLuan.DeepClone();
                dynamicTienSuObjOriginal = SelectedKhamSucKhoeTienSu.DeepClone();
                dynamicKhamCLSObjOriginal = SelectedKhamSucKhoeKetQuaCanLamSangs.DeepClone().Cast<dynamic>().ToList();
            }
            else
            {
                AlertService.ShowAlert("Không tìm thấy thông tin khám!", "danger");
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

        private async Task<List<PhanLoaiSucKhoeModel>> LoadPhanLoaiSucKhoeSelect2(IEnumerable<PhanLoaiSucKhoeModel> allItems, string filter, CancellationToken token)
        {
            if (PhanLoaiSucKhoes != null && PhanLoaiSucKhoes.Count > 0 && currentFilterPhanLoaiSucKhoe == filter)
            {
                return PhanLoaiSucKhoes;
            }

            currentFilterPhanLoaiSucKhoe = filter;
            await Task.Delay(300);

            try
            {
                ArgumentNullException.ThrowIfNull(allItems);

                PhanLoaiSucKhoes = await LoadDataInTable(allItems, filter, token, PhanLoaiSucKhoaService, "filter[_and][][active][_eq]=true");
                StateHasChanged();
                return PhanLoaiSucKhoes;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in filterFunction: {ex.Message}");
                return new List<PhanLoaiSucKhoeModel>();
            }
        }

        private async Task<List<UserModel>> LoadDoctorSelect2(IEnumerable<UserModel> allItems, string filter, CancellationToken token)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(allItems);

                // Debouncing - wait 300ms before making API call
                await Task.Delay(300, token);

                var query = "sort=-id";
                query += "&filter[_and][][status][_eq]=active";
                query += $"&filter[_and][][role][_eq]={CurrentSetting.doctor_role_id}";

                if (!string.IsNullOrEmpty(filter))
                {
                    query += $"&filter[_and][0][_or][0][first_name][_contains]={Uri.EscapeDataString(filter)}";
                    query += $"&filter[_and][0][_or][1][last_name][_contains]={Uri.EscapeDataString(filter)}";
                }

                var result = await UserService.GetAllAsync(query);

                if (!result.IsSuccess)
                {
                    Console.WriteLine($"Error loading users: {string.Join(", ", result.Errors.Select(e => e.Message))}");
                    Users = new List<UserModel>();
                }
                else
                {
                    Console.WriteLine($"Loaded {result.Data?.Count} users successfully.");
                    Users = result.Data ?? new List<UserModel>();
                }

                StateHasChanged();
                return Users;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in filterFunction: {ex.Message}");
                return new List<UserModel>();
            }
        }

        private async Task<IEnumerable<ContractModel>> LoadContractData(string searchText)
        {
            return await LoadBlazorTypeaheadData(searchText, ContractService);
        }

        private async Task<IEnumerable<KhamSucKhoeCongTyModel>> LoadKhamSucKhoeCongTyData(string searchText)
        {
            return await LoadBlazorTypeaheadData(searchText, KhamSucKhoeCongTyService);
        }

        private async Task<IEnumerable<KetQuaCanLamSangModel>> LoadKetQuaCanLamSangData(string searchText)
        {
            try
            {
                var query = "sort=-id";
                query += $"&filter[_and][][ma_luot_kham][_eq]={SelectedItem.ma_luot_kham}";

                if (!string.IsNullOrEmpty(searchText))
                {
                    query += $"&filter[_and][0][_or][0][ma_can_lam_sang][_contains]={Uri.EscapeDataString(searchText)}";
                    query += $"&filter[_and][0][_or][1][ten_can_lam_san][_contains]={Uri.EscapeDataString(searchText)}";
                    query += $"&filter[_and][0][_or][1][ket_luan_can_lam_sang][_contains]={Uri.EscapeDataString(searchText)}";
                }

                var result = await KetQuaCanLamSangService.GetAllAsync(query);
                return result?.IsSuccess == true ? result.Data ?? Enumerable.Empty<KetQuaCanLamSangModel>() : Enumerable.Empty<KetQuaCanLamSangModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading typeahead data: {ex.Message}");
                return Enumerable.Empty<KetQuaCanLamSangModel>();
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
            StateHasChanged();
        }
        private void CloseSyncKetQuaCanLamSangModal()
        {
            AlertService.ShowAlert("Đồng bộ kết quả cận lâm sàng thành công!", "success");
            openSyncKetQuaCanLamSangModal = false;
        }

        private void CloseSoKhamSucKhoeModal()
        {
            openSoKhamSucKhoeModal = false;
        }

        private void CloseConfirmModal()
        {
            isShowConfirmModal = false;
        }

        private async Task OnValidSubmit(bool isConfirm = false)
        {
            try
            {
                isShowConfirmModal = false;
                if (SelectedItem.id <= 0)
                {
                    return;
                }

                Loading.Show();

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

                if (SelectedKhamSucKhoeCongTy.kham_noi_vien == null || SelectedKhamSucKhoeCongTy.kham_noi_vien == false)
                {
                    if (SelectedKhamSucKhoeSanPhuKhoa.ap_dung_bptt != true)
                    {
                        SelectedKhamSucKhoeSanPhuKhoa.bptt_ghi_ro = string.Empty;
                    }

                    if ((SelectedKhamSucKhoeSanPhuKhoa.so_lan_mo_san_phu_khoa ?? 0) <= 0)
                    {
                        SelectedKhamSucKhoeSanPhuKhoa.mo_san_phu_khoa_ghi_ro = string.Empty;
                    }

                    PhanLoaiSucKhoeModel? plDefault = default;
                    if (CurrentSetting.phan_loai_sk_mac_dinh.HasValue && CurrentSetting.phan_loai_sk_mac_dinh > 0)
                    {
                        var plResult = await PhanLoaiSucKhoaService.GetByIdAsync($"{CurrentSetting.phan_loai_sk_mac_dinh}");
                        plDefault = plResult.Data;
                    }

                    if (onBS)
                    {
                        SelectedKhamSucKhoeTheLuc.phan_loai ??= plDefault;
                    }

                    if (onBSHoHap)
                    {
                        SelectedKhamSucKhoeChuyenKhoa.pl_nk_ho_hap ??= plDefault;
                        if (string.IsNullOrEmpty(SelectedKhamSucKhoeChuyenKhoa.kq_nk_ho_hap?.Trim()))
                        {
                            SelectedKhamSucKhoeChuyenKhoa.kq_nk_ho_hap = CurrentSetting.ket_qua_ksk_mac_dinh;
                        }

                        SelectedKhamSucKhoeChuyenKhoa.bs_ho_hap = $"{CurrentUser?.chuc_danh} {CurrentUser?.full_name}";
                        SelectedKhamSucKhoeChuyenKhoa.chu_ky_ho_hap = CurrentUser?.chu_ky_bac_si;
                    }
                    if (onBSTuanHoan)
                    {
                        SelectedKhamSucKhoeChuyenKhoa.pl_nk_tuan_hoan ??= plDefault;
                        if (string.IsNullOrEmpty(SelectedKhamSucKhoeChuyenKhoa.kq_nk_tuan_hoan?.Trim()))
                        {
                            SelectedKhamSucKhoeChuyenKhoa.kq_nk_tuan_hoan = CurrentSetting.ket_qua_ksk_mac_dinh;
                        }

                        SelectedKhamSucKhoeChuyenKhoa.bs_tuan_hoan = $"{CurrentUser?.chuc_danh} {CurrentUser?.full_name}";
                        SelectedKhamSucKhoeChuyenKhoa.chu_ky_tuan_hoan = CurrentUser?.chu_ky_bac_si;
                    }
                    if (onBSTieuHoa)
                    {
                        SelectedKhamSucKhoeChuyenKhoa.pl_nk_tieu_hoa ??= plDefault;
                        if (string.IsNullOrEmpty(SelectedKhamSucKhoeChuyenKhoa.kq_nk_tieu_hoa?.Trim()))
                        {
                            SelectedKhamSucKhoeChuyenKhoa.kq_nk_tieu_hoa = CurrentSetting.ket_qua_ksk_mac_dinh;
                        }

                        SelectedKhamSucKhoeChuyenKhoa.bs_tieu_hoa = $"{CurrentUser?.chuc_danh} {CurrentUser?.full_name}";
                        SelectedKhamSucKhoeChuyenKhoa.chu_ky_tieu_hoa = CurrentUser?.chu_ky_bac_si;
                    }
                    if (onBSThanTietNieu)
                    {
                        SelectedKhamSucKhoeChuyenKhoa.pl_nk_than_tiet_nieu ??= plDefault;
                        if (string.IsNullOrEmpty(SelectedKhamSucKhoeChuyenKhoa.kq_nk_than_tiet_nieu?.Trim()))
                        {
                            SelectedKhamSucKhoeChuyenKhoa.kq_nk_than_tiet_nieu = CurrentSetting.ket_qua_ksk_mac_dinh;
                        }

                        SelectedKhamSucKhoeChuyenKhoa.bs_than_tiet_nieu = $"{CurrentUser?.chuc_danh} {CurrentUser?.full_name}";
                        SelectedKhamSucKhoeChuyenKhoa.chu_ky_than_tiet_nieu = CurrentUser?.chu_ky_bac_si;
                    }
                    if (onBSNoiTiet)
                    {
                        SelectedKhamSucKhoeChuyenKhoa.pl_nk_noi_tiet ??= plDefault;
                        if (string.IsNullOrEmpty(SelectedKhamSucKhoeChuyenKhoa.kq_nk_noi_tiet?.Trim()))
                        {
                            SelectedKhamSucKhoeChuyenKhoa.kq_nk_noi_tiet = CurrentSetting.ket_qua_ksk_mac_dinh;
                        }

                        SelectedKhamSucKhoeChuyenKhoa.bs_noi_tiet = $"{CurrentUser?.chuc_danh} {CurrentUser?.full_name}";
                        SelectedKhamSucKhoeChuyenKhoa.chu_ky_noi_tiet = CurrentUser?.chu_ky_bac_si;
                    }
                    if (onBSCoXuongKhop)
                    {
                        SelectedKhamSucKhoeChuyenKhoa.pl_nk_co_xuong_khop ??= plDefault;
                        if (string.IsNullOrEmpty(SelectedKhamSucKhoeChuyenKhoa.kq_nk_co_xuong_khop?.Trim()))
                        {
                            SelectedKhamSucKhoeChuyenKhoa.kq_nk_co_xuong_khop = CurrentSetting.ket_qua_ksk_mac_dinh;
                        }

                        SelectedKhamSucKhoeChuyenKhoa.bs_co_xuong_khop = $"{CurrentUser?.chuc_danh} {CurrentUser?.full_name}";
                        SelectedKhamSucKhoeChuyenKhoa.chu_ky_co_xuong_khop = CurrentUser?.chu_ky_bac_si;
                    }
                    if (onBSThanKinh)
                    {
                        SelectedKhamSucKhoeChuyenKhoa.pl_nk_than_kinh ??= plDefault;
                        if (string.IsNullOrEmpty(SelectedKhamSucKhoeChuyenKhoa.kq_nk_than_kinh?.Trim()))
                        {
                            SelectedKhamSucKhoeChuyenKhoa.kq_nk_than_kinh = CurrentSetting.ket_qua_ksk_mac_dinh;
                        }

                        SelectedKhamSucKhoeChuyenKhoa.bs_than_kinh = $"{CurrentUser?.chuc_danh} {CurrentUser?.full_name}";
                        SelectedKhamSucKhoeChuyenKhoa.chu_ky_than_kinh = CurrentUser?.chu_ky_bac_si;
                    }
                    if (onBSTamThan)
                    {
                        SelectedKhamSucKhoeChuyenKhoa.pl_nk_tam_than ??= plDefault;
                        if (string.IsNullOrEmpty(SelectedKhamSucKhoeChuyenKhoa.kq_nk_tam_than?.Trim()))
                        {
                            SelectedKhamSucKhoeChuyenKhoa.kq_nk_tam_than = CurrentSetting.ket_qua_ksk_mac_dinh;
                        }

                        SelectedKhamSucKhoeChuyenKhoa.bs_tam_than = $"{CurrentUser?.chuc_danh} {CurrentUser?.full_name}";
                        SelectedKhamSucKhoeChuyenKhoa.chu_ky_tam_than = CurrentUser?.chu_ky_bac_si;
                    }
                    if (onBSNgoaiKhoa)
                    {
                        SelectedKhamSucKhoeChuyenKhoa.pl_ngoai_khoa ??= plDefault;
                        if (string.IsNullOrEmpty(SelectedKhamSucKhoeChuyenKhoa.kq_ngoai_khoa?.Trim()))
                        {
                            SelectedKhamSucKhoeChuyenKhoa.kq_ngoai_khoa = CurrentSetting.ket_qua_ksk_mac_dinh;
                        }

                        SelectedKhamSucKhoeChuyenKhoa.pl_da_lieu ??= plDefault;
                        if (string.IsNullOrEmpty(SelectedKhamSucKhoeChuyenKhoa.kq_da_lieu?.Trim()))
                        {
                            SelectedKhamSucKhoeChuyenKhoa.kq_da_lieu = CurrentSetting.ket_qua_ksk_mac_dinh;
                        }

                        SelectedKhamSucKhoeChuyenKhoa.bs_ngoai_khoa = $"{CurrentUser?.chuc_danh} {CurrentUser?.full_name}";
                        SelectedKhamSucKhoeChuyenKhoa.chu_ky_ngoai_khoa = CurrentUser?.chu_ky_bac_si;
                    }
                    if (onBSMat)
                    {
                        SelectedKhamSucKhoeChuyenKhoa.pl_mat ??= plDefault;
                        if (string.IsNullOrEmpty(SelectedKhamSucKhoeChuyenKhoa.benh_mat?.Trim()))
                        {
                            SelectedKhamSucKhoeChuyenKhoa.benh_mat = CurrentSetting.ket_qua_ksk_mac_dinh;
                        }

                        SelectedKhamSucKhoeChuyenKhoa.bs_mat = CurrentUser?.full_name;
                        SelectedKhamSucKhoeChuyenKhoa.chu_ky_mat = CurrentUser?.chu_ky_bac_si;
                    }
                    if (onBSTaiMuiHong)
                    {
                        SelectedKhamSucKhoeChuyenKhoa.pl_tmh ??= plDefault;
                        if (string.IsNullOrEmpty(SelectedKhamSucKhoeChuyenKhoa.benh_tai_mui_hong?.Trim()))
                        {
                            SelectedKhamSucKhoeChuyenKhoa.benh_tai_mui_hong = CurrentSetting.ket_qua_ksk_mac_dinh;
                        }

                        SelectedKhamSucKhoeChuyenKhoa.bs_tmh = $"{CurrentUser?.chuc_danh} {CurrentUser?.full_name}";
                        SelectedKhamSucKhoeChuyenKhoa.chu_ky_tmh = CurrentUser?.chu_ky_bac_si;
                    }
                    if (onBSRangHamMat)
                    {
                        SelectedKhamSucKhoeChuyenKhoa.pl_rhm ??= plDefault;
                        if (string.IsNullOrEmpty(SelectedKhamSucKhoeChuyenKhoa.kq_rhm_ham_tren?.Trim()))
                        {
                            SelectedKhamSucKhoeChuyenKhoa.kq_rhm_ham_tren = CurrentSetting.ket_qua_ksk_mac_dinh;
                        }
                        if (string.IsNullOrEmpty(SelectedKhamSucKhoeChuyenKhoa.kq_rhm_ham_duoi?.Trim()))
                        {
                            SelectedKhamSucKhoeChuyenKhoa.kq_rhm_ham_duoi = CurrentSetting.ket_qua_ksk_mac_dinh;
                        }
                        if (string.IsNullOrEmpty(SelectedKhamSucKhoeChuyenKhoa.benh_rhm?.Trim()))
                        {
                            SelectedKhamSucKhoeChuyenKhoa.benh_rhm = CurrentSetting.ket_qua_ksk_mac_dinh;
                        }

                        SelectedKhamSucKhoeChuyenKhoa.bs_rhm = $"{CurrentUser?.chuc_danh} {CurrentUser?.full_name}";
                        SelectedKhamSucKhoeChuyenKhoa.chu_ky_rhm = CurrentUser?.chu_ky_bac_si;
                    }
                    if (onBSSanPhuKhoa && SelectedUser.gioi_tinh == GioiTinh.Nu)
                    {
                        SelectedKhamSucKhoeSanPhuKhoa.phan_loai ??= plDefault;
                        if (string.IsNullOrEmpty(SelectedKhamSucKhoeSanPhuKhoa.ket_qua?.Trim()))
                        {
                            SelectedKhamSucKhoeSanPhuKhoa.ket_qua = CurrentSetting.ket_qua_ksk_mac_dinh;
                        }

                        SelectedKhamSucKhoeSanPhuKhoa.nguoi_ket_luan = $"{CurrentUser?.chuc_danh} {CurrentUser?.full_name}";
                        SelectedKhamSucKhoeSanPhuKhoa.chu_ky = CurrentUser?.chu_ky_bac_si;
                    }

                    SelectedKhamSucKhoeKetQuaCanLamSangs = SelectedKhamSucKhoeKetQuaCanLamSangs.Select(c =>
                            {
                                c.luot_kham = SelectedItem;
                                c.ma_luot_kham = SelectedItem.ma_luot_kham;

                                return c;
                            }).ToList();


                    if (onBSKetLuan)
                    {
                        SelectedKhamSucKhoeKetLuan.phan_loai_suc_khoe ??= plDefault;
                        if (string.IsNullOrEmpty(SelectedKhamSucKhoeKetLuan.benh_tat_ket_luan?.Trim()))
                        {
                            SelectedKhamSucKhoeKetLuan.benh_tat_ket_luan = CurrentSetting.ket_qua_ksk_mac_dinh;
                        }

                        SelectedKhamSucKhoeKetLuan.nguoi_ket_luan = $"{CurrentUser?.chuc_danh} {CurrentUser?.full_name}";
                        SelectedKhamSucKhoeKetLuan.chu_ky = CurrentUser?.chu_ky_bac_si;
                        SelectedKhamSucKhoeKetLuan.bs_ket_luan = CurrentUser;
                        SelectedKhamSucKhoeKetLuan.ngay_ket_luan ??= DateTime.Now;
                    }

                    if (SelectedKhamSucKhoeTienSu.id > 0)
                    {
                        var result = await KhamSucKhoeTienSuService.UpdateAsync(new List<dynamic>() { SelectedKhamSucKhoeTienSu });
                        if (result == null || !result.IsSuccess)
                        {
                            AlertService.ShowAlert("Đã có lỗi xảy ra khi lưu tiền sử bệnh tật!", "danger");
                            return;
                        }
                    }
                    else
                    {
                        var result = await KhamSucKhoeTienSuService.CreateAsync(new List<dynamic>() { SelectedKhamSucKhoeTienSu });
                        if (result == null || !result.IsSuccess)
                        {
                            AlertService.ShowAlert("Đã có lỗi xảy ra khi lưu tiền sử bệnh tật!", "danger");
                            return;
                        }
                    }

                    if (SelectedKhamSucKhoeSanPhuKhoa.id > 0)
                    {
                        var result = await KhamSucKhoeSanPhuKhoaService.UpdateAsync(new List<dynamic>() { SelectedKhamSucKhoeSanPhuKhoa });
                        if (result == null || !result.IsSuccess)
                        {
                            AlertService.ShowAlert("Đã có lỗi xảy ra khi lưu thông tin khám phụ khoa!", "danger");
                            return;
                        }
                    }
                    else
                    {
                        var result = await KhamSucKhoeSanPhuKhoaService.CreateAsync(new List<dynamic>() { SelectedKhamSucKhoeSanPhuKhoa });
                        if (result == null || !result.IsSuccess)
                        {
                            AlertService.ShowAlert("Đã có lỗi xảy ra khi lưu thông tin khám phụ khoa!", "danger");
                            return;
                        }
                    }

                    if (SelectedKhamSucKhoeTheLuc.id > 0)
                    {
                        var result = await KhamSucKhoeTheLucService.UpdateAsync(new List<dynamic>() { SelectedKhamSucKhoeTheLuc });
                        if (result == null || !result.IsSuccess)
                        {
                            AlertService.ShowAlert("Đã có lỗi xảy ra khi lưu khám thể lực!", "danger");
                            return;
                        }
                    }
                    else
                    {
                        var result = await KhamSucKhoeTheLucService.CreateAsync(new List<dynamic>() { SelectedKhamSucKhoeTheLuc });
                        if (result == null || !result.IsSuccess)
                        {
                            AlertService.ShowAlert("Đã có lỗi xảy ra khi lưu khám thể lực!", "danger");
                            return;
                        }
                    }

                    if (SelectedKhamSucKhoeChuyenKhoa.id > 0)
                    {
                        var result = await KhamSucKhoeChuyenKhoaService.UpdateAsync(new List<dynamic>() { SelectedKhamSucKhoeChuyenKhoa });
                        if (result == null || !result.IsSuccess)
                        {
                            AlertService.ShowAlert("Đã có lỗi xảy ra khi lưu khám chuyên khoa!", "danger");
                            return;
                        }
                    }
                    else
                    {
                        var result = await KhamSucKhoeChuyenKhoaService.CreateAsync(new List<dynamic>() { SelectedKhamSucKhoeChuyenKhoa });
                        if (result == null || !result.IsSuccess)
                        {
                            AlertService.ShowAlert("Đã có lỗi xảy ra khi lưu khám chuyên khoa!", "danger");
                            return;
                        }
                    }

                    var updateCls = SelectedKhamSucKhoeKetQuaCanLamSangs.Where(c => c.id > 0).ToList();
                    var addCls = SelectedKhamSucKhoeKetQuaCanLamSangs.Where(c => c.id == 0 && !string.IsNullOrEmpty(c.ket_qua)).ToList();

                    if (updateCls != null && updateCls.Count() > 0)
                    {
                        var result = await KhamSucKhoeKetQuaCanLamSangService.UpdateAsync(updateCls.Cast<dynamic>().ToList());
                        if (result == null || !result.IsSuccess)
                        {
                            AlertService.ShowAlert("Đã có lỗi xảy ra khi lưu khám cận lâm sàng!", "danger");
                            return;
                        }
                    }
                    if (addCls != null && addCls.Count() > 0)
                    {
                        var result = await KhamSucKhoeKetQuaCanLamSangService.CreateAsync(addCls.Cast<dynamic>().ToList());
                        if (result == null || !result.IsSuccess)
                        {
                            AlertService.ShowAlert("Đã có lỗi xảy ra khi lưu khám cận lâm sàng!", "danger");
                            return;
                        }
                    }

                    if (SelectedKhamSucKhoeKetLuan.id > 0)
                    {
                        var result = await KhamSucKhoeKetLuanService.UpdateAsync(new List<dynamic>() { SelectedKhamSucKhoeKetLuan });
                        if (result == null || !result.IsSuccess)
                        {
                            AlertService.ShowAlert("Đã có lỗi xảy ra khi lưu kết luận!", "danger");
                            return;
                        }
                    }
                    else
                    {
                        var result = await KhamSucKhoeKetLuanService.CreateAsync(new List<dynamic>() { SelectedKhamSucKhoeKetLuan });
                        if (result == null || !result.IsSuccess)
                        {
                            AlertService.ShowAlert("Đã có lỗi xảy ra khi lưu kết luận!", "danger");
                            return;
                        }
                    }
                }
                else
                {
                    if (
                        !isConfirm
                        && (
                            (dynamicTheLucObj as IDictionary<string, object?>)?.Count > 0
                            || (dynamicSanPhuKhoaObj as IDictionary<string, object?>)?.Count > 0
                            || (dynamicChuyenKhoaObj as IDictionary<string, object?>)?.Count > 0
                            || (dynamicKetLuanObj as IDictionary<string, object?>)?.Count > 0
                            || (dynamicTienSuObj as IDictionary<string, object?>)?.Count > 0
                            || dynamicKhamCLSObj.Any()
                        )
                    )
                    {
                        confirmMessage = GenerateChangeNoteTable();
                        isShowConfirmModal = true;

                        return;
                    }

                    #region Tien su
                    dynamic updateDynamicObj = new ExpandoObject();
                    var updateFields = (IDictionary<string, object?>)updateDynamicObj;
                    var dict = dynamicTienSuObj as IDictionary<string, object?>;
                    if (dict != null && dict.Count > 0)
                    {
                        var props = typeof(KhamSucKhoeTienSuModel).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                        foreach (var kv in dict.Where(kv => props.Contains(kv.Key)))
                        {
                            updateFields[kv.Key] = kv.Value?.GetCustumFieldOrValue();
                        }

                        if (updateFields.Any())
                        {
                            if (SelectedKhamSucKhoeTienSu.id > 0)
                            {
                                updateFields[nameof(SelectedKhamSucKhoeTienSu.id)] = SelectedKhamSucKhoeTienSu.id;
                            }

                            updateFields[nameof(SelectedKhamSucKhoeTienSu.luot_kham)] = SelectedItem.id;
                            updateFields[nameof(SelectedKhamSucKhoeTienSu.ma_luot_kham)] = SelectedItem.ma_luot_kham ?? string.Empty;
                        }
                    }

                    if (updateFields.Any())
                    {
                        if (updateFields.ContainsKey(nameof(SelectedKhamSucKhoeTienSu.id)))
                        {
                            var result = await KhamSucKhoeTienSuService.UpdateAsync(new List<dynamic>() { updateDynamicObj });
                            if (result == null || !result.IsSuccess)
                            {
                                AlertService.ShowAlert("Đã có lỗi xảy ra khi lưu tiền sử bệnh tật!", "danger");
                                return;
                            }
                        }
                        else
                        {
                            var result = await KhamSucKhoeTienSuService.CreateAsync(new List<dynamic>() { updateDynamicObj });
                            if (result == null || !result.IsSuccess)
                            {
                                AlertService.ShowAlert("Đã có lỗi xảy ra khi lưu tiền sử bệnh tật!", "danger");
                                return;
                            }
                        }
                    }
                    #endregion

                    #region San phu khoa
                    updateDynamicObj = new ExpandoObject();
                    updateFields = (IDictionary<string, object?>)updateDynamicObj;
                    dict = dynamicSanPhuKhoaObj as IDictionary<string, object?>;
                    if (dict != null && dict.Count > 0)
                    {
                        var props = typeof(KhamSucKhoeSanPhuKhoaModel).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                        foreach (var kv in dict.Where(kv => props.Contains(kv.Key)))
                        {
                            updateFields[kv.Key] = kv.Value?.GetCustumFieldOrValue();
                        }

                        if (updateFields.Any())
                        {
                            if (SelectedKhamSucKhoeSanPhuKhoa.id > 0)
                            {
                                updateFields[nameof(SelectedKhamSucKhoeSanPhuKhoa.id)] = SelectedKhamSucKhoeSanPhuKhoa.id;
                            }
                            updateFields[nameof(SelectedKhamSucKhoeSanPhuKhoa.luot_kham)] = SelectedItem.id;
                            updateFields[nameof(SelectedKhamSucKhoeSanPhuKhoa.ma_luot_kham)] = SelectedItem.ma_luot_kham ?? string.Empty;

                            if (updateFields.ContainsKey(nameof(SelectedKhamSucKhoeSanPhuKhoa.phan_loai)) || updateFields.ContainsKey(nameof(SelectedKhamSucKhoeSanPhuKhoa.ket_qua)))
                            {
                                SelectedKhamSucKhoeSanPhuKhoa.nguoi_ket_luan = $"{CurrentUser?.chuc_danh} {CurrentUser?.full_name}";
                                SelectedKhamSucKhoeSanPhuKhoa.chu_ky = CurrentUser?.chu_ky_bac_si;
                                updateFields[nameof(SelectedKhamSucKhoeSanPhuKhoa.nguoi_ket_luan)] = SelectedKhamSucKhoeSanPhuKhoa.nguoi_ket_luan;
                                updateFields[nameof(SelectedKhamSucKhoeSanPhuKhoa.chu_ky)] = SelectedKhamSucKhoeSanPhuKhoa.chu_ky;
                            }
                        }
                    }

                    if (updateFields.Any())
                    {
                        if (updateFields.ContainsKey(nameof(SelectedKhamSucKhoeSanPhuKhoa.id)))
                        {
                            var result = await KhamSucKhoeSanPhuKhoaService.UpdateAsync(new List<dynamic>() { updateDynamicObj });
                            if (result == null || !result.IsSuccess)
                            {
                                AlertService.ShowAlert("Đã có lỗi xảy ra khi lưu thông tin khám phụ khoa!", "danger");
                                return;
                            }
                        }
                        else
                        {
                            var result = await KhamSucKhoeSanPhuKhoaService.CreateAsync(new List<dynamic>() { updateDynamicObj });
                            if (result == null || !result.IsSuccess)
                            {
                                AlertService.ShowAlert("Đã có lỗi xảy ra khi lưu thông tin khám phụ khoa!", "danger");
                                return;
                            }
                        }
                    }
                    #endregion

                    #region The luc
                    updateDynamicObj = new ExpandoObject();
                    updateFields = (IDictionary<string, object?>)updateDynamicObj;
                    dict = dynamicTheLucObj as IDictionary<string, object?>;
                    if (dict != null && dict.Count > 0)
                    {
                        var props = typeof(KhamSucKhoeTheLucModel).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                        foreach (var kv in dict.Where(kv => props.Contains(kv.Key)))
                        {
                            updateFields[kv.Key] = kv.Value?.GetCustumFieldOrValue();
                        }

                        if (updateFields.Any())
                        {
                            if (SelectedKhamSucKhoeTheLuc.id > 0)
                            {
                                updateFields[nameof(SelectedKhamSucKhoeTheLuc.id)] = SelectedKhamSucKhoeTheLuc.id;
                            }

                            updateFields[nameof(SelectedKhamSucKhoeTheLuc.luot_kham)] = SelectedItem.id;
                            updateFields[nameof(SelectedKhamSucKhoeTheLuc.ma_luot_kham)] = SelectedItem.ma_luot_kham ?? string.Empty;
                        }
                    }

                    if (updateFields.Any())
                    {
                        if (updateFields.ContainsKey(nameof(SelectedKhamSucKhoeTheLuc.id)))
                        {
                            var result = await KhamSucKhoeTheLucService.UpdateAsync(new List<dynamic>() { updateDynamicObj });
                            if (result == null || !result.IsSuccess)
                            {
                                AlertService.ShowAlert("Đã có lỗi xảy ra khi lưu thông tin khám thể lực!", "danger");
                                return;
                            }
                        }
                        else
                        {
                            var result = await KhamSucKhoeTheLucService.CreateAsync(new List<dynamic>() { updateDynamicObj });
                            if (result == null || !result.IsSuccess)
                            {
                                AlertService.ShowAlert("Đã có lỗi xảy ra khi lưu thông tin khám thể lực!", "danger");
                                return;
                            }
                        }
                    }
                    #endregion

                    #region Chuyen khoa
                    updateDynamicObj = new ExpandoObject();
                    updateFields = (IDictionary<string, object?>)updateDynamicObj;
                    dict = dynamicChuyenKhoaObj as IDictionary<string, object?>;
                    if (dict != null && dict.Count > 0)
                    {
                        var props = typeof(KhamSucKhoeChuyenKhoaModel).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                        foreach (var kv in dict.Where(kv => props.Contains(kv.Key)))
                        {
                            updateFields[kv.Key] = kv.Value?.GetCustumFieldOrValue();
                        }

                        if (updateFields.Any())
                        {
                            if (SelectedKhamSucKhoeChuyenKhoa.id > 0)
                            {
                                updateFields[nameof(SelectedKhamSucKhoeChuyenKhoa.id)] = SelectedKhamSucKhoeChuyenKhoa.id;
                            }

                            updateFields[nameof(SelectedKhamSucKhoeChuyenKhoa.luot_kham)] = SelectedItem.id;
                            updateFields[nameof(SelectedKhamSucKhoeChuyenKhoa.ma_luot_kham)] = SelectedItem.ma_luot_kham ?? string.Empty;

                            if (
                                updateFields.ContainsKey(nameof(SelectedKhamSucKhoeChuyenKhoa.kq_nk_tuan_hoan))
                                || updateFields.ContainsKey(nameof(SelectedKhamSucKhoeChuyenKhoa.pl_nk_tuan_hoan))
                            )
                            {
                                SelectedKhamSucKhoeChuyenKhoa.bs_tuan_hoan = $"{CurrentUser?.chuc_danh} {CurrentUser?.full_name}";
                                SelectedKhamSucKhoeChuyenKhoa.chu_ky_tuan_hoan = CurrentUser?.chu_ky_bac_si;
                                updateFields[nameof(SelectedKhamSucKhoeChuyenKhoa.bs_tuan_hoan)] = SelectedKhamSucKhoeChuyenKhoa.bs_tuan_hoan;
                                updateFields[nameof(SelectedKhamSucKhoeChuyenKhoa.chu_ky_tuan_hoan)] = SelectedKhamSucKhoeChuyenKhoa.chu_ky_tuan_hoan;
                            }

                            if (
                                updateFields.ContainsKey(nameof(SelectedKhamSucKhoeChuyenKhoa.kq_nk_ho_hap))
                                || updateFields.ContainsKey(nameof(SelectedKhamSucKhoeChuyenKhoa.pl_nk_ho_hap))
                            )
                            {
                                SelectedKhamSucKhoeChuyenKhoa.bs_ho_hap = $"{CurrentUser?.chuc_danh} {CurrentUser?.full_name}";
                                SelectedKhamSucKhoeChuyenKhoa.chu_ky_ho_hap = CurrentUser?.chu_ky_bac_si;
                                updateFields[nameof(SelectedKhamSucKhoeChuyenKhoa.bs_ho_hap)] = SelectedKhamSucKhoeChuyenKhoa.bs_ho_hap;
                                updateFields[nameof(SelectedKhamSucKhoeChuyenKhoa.chu_ky_ho_hap)] = SelectedKhamSucKhoeChuyenKhoa.chu_ky_ho_hap;
                            }

                            if (
                                updateFields.ContainsKey(nameof(SelectedKhamSucKhoeChuyenKhoa.kq_nk_tieu_hoa))
                                || updateFields.ContainsKey(nameof(SelectedKhamSucKhoeChuyenKhoa.pl_nk_tieu_hoa))
                            )
                            {
                                SelectedKhamSucKhoeChuyenKhoa.bs_tieu_hoa = $"{CurrentUser?.chuc_danh} {CurrentUser?.full_name}";
                                SelectedKhamSucKhoeChuyenKhoa.chu_ky_tieu_hoa = CurrentUser?.chu_ky_bac_si;
                                updateFields[nameof(SelectedKhamSucKhoeChuyenKhoa.bs_tieu_hoa)] = SelectedKhamSucKhoeChuyenKhoa.bs_tieu_hoa;
                                updateFields[nameof(SelectedKhamSucKhoeChuyenKhoa.chu_ky_tieu_hoa)] = SelectedKhamSucKhoeChuyenKhoa.chu_ky_tieu_hoa;
                            }

                            if (
                                updateFields.ContainsKey(nameof(SelectedKhamSucKhoeChuyenKhoa.kq_nk_than_tiet_nieu))
                                || updateFields.ContainsKey(nameof(SelectedKhamSucKhoeChuyenKhoa.pl_nk_than_tiet_nieu))
                            )
                            {
                                SelectedKhamSucKhoeChuyenKhoa.bs_than_tiet_nieu = $"{CurrentUser?.chuc_danh} {CurrentUser?.full_name}";
                                SelectedKhamSucKhoeChuyenKhoa.chu_ky_than_tiet_nieu = CurrentUser?.chu_ky_bac_si;
                                updateFields[nameof(SelectedKhamSucKhoeChuyenKhoa.bs_than_tiet_nieu)] = SelectedKhamSucKhoeChuyenKhoa.bs_than_tiet_nieu;
                                updateFields[nameof(SelectedKhamSucKhoeChuyenKhoa.chu_ky_than_tiet_nieu)] = SelectedKhamSucKhoeChuyenKhoa.chu_ky_than_tiet_nieu;
                            }

                            if (
                                updateFields.ContainsKey(nameof(SelectedKhamSucKhoeChuyenKhoa.kq_nk_noi_tiet))
                                || updateFields.ContainsKey(nameof(SelectedKhamSucKhoeChuyenKhoa.pl_nk_noi_tiet))
                            )
                            {
                                SelectedKhamSucKhoeChuyenKhoa.bs_noi_tiet = $"{CurrentUser?.chuc_danh} {CurrentUser?.full_name}";
                                SelectedKhamSucKhoeChuyenKhoa.chu_ky_noi_tiet = CurrentUser?.chu_ky_bac_si;
                                updateFields[nameof(SelectedKhamSucKhoeChuyenKhoa.bs_noi_tiet)] = SelectedKhamSucKhoeChuyenKhoa.bs_noi_tiet;
                                updateFields[nameof(SelectedKhamSucKhoeChuyenKhoa.chu_ky_noi_tiet)] = SelectedKhamSucKhoeChuyenKhoa.chu_ky_noi_tiet;
                            }

                            if (
                                updateFields.ContainsKey(nameof(SelectedKhamSucKhoeChuyenKhoa.kq_nk_co_xuong_khop))
                                || updateFields.ContainsKey(nameof(SelectedKhamSucKhoeChuyenKhoa.pl_nk_co_xuong_khop))
                            )
                            {
                                SelectedKhamSucKhoeChuyenKhoa.bs_co_xuong_khop = $"{CurrentUser?.chuc_danh} {CurrentUser?.full_name}";
                                SelectedKhamSucKhoeChuyenKhoa.chu_ky_co_xuong_khop = CurrentUser?.chu_ky_bac_si;
                                updateFields[nameof(SelectedKhamSucKhoeChuyenKhoa.bs_co_xuong_khop)] = SelectedKhamSucKhoeChuyenKhoa.bs_co_xuong_khop;
                                updateFields[nameof(SelectedKhamSucKhoeChuyenKhoa.chu_ky_co_xuong_khop)] = SelectedKhamSucKhoeChuyenKhoa.chu_ky_co_xuong_khop;
                            }

                            if (
                                updateFields.ContainsKey(nameof(SelectedKhamSucKhoeChuyenKhoa.kq_nk_than_kinh))
                                || updateFields.ContainsKey(nameof(SelectedKhamSucKhoeChuyenKhoa.pl_nk_than_kinh))
                            )
                            {
                                SelectedKhamSucKhoeChuyenKhoa.bs_than_kinh = $"{CurrentUser?.chuc_danh} {CurrentUser?.full_name}";
                                SelectedKhamSucKhoeChuyenKhoa.chu_ky_than_kinh = CurrentUser?.chu_ky_bac_si;
                                updateFields[nameof(SelectedKhamSucKhoeChuyenKhoa.bs_than_kinh)] = SelectedKhamSucKhoeChuyenKhoa.bs_than_kinh;
                                updateFields[nameof(SelectedKhamSucKhoeChuyenKhoa.chu_ky_than_kinh)] = SelectedKhamSucKhoeChuyenKhoa.chu_ky_than_kinh;
                            }

                            if (
                                updateFields.ContainsKey(nameof(SelectedKhamSucKhoeChuyenKhoa.kq_nk_tam_than))
                                || updateFields.ContainsKey(nameof(SelectedKhamSucKhoeChuyenKhoa.pl_nk_tam_than))
                            )
                            {
                                SelectedKhamSucKhoeChuyenKhoa.bs_tam_than = $"{CurrentUser?.chuc_danh} {CurrentUser?.full_name}";
                                SelectedKhamSucKhoeChuyenKhoa.chu_ky_tam_than = CurrentUser?.chu_ky_bac_si;
                                updateFields[nameof(SelectedKhamSucKhoeChuyenKhoa.bs_tam_than)] = SelectedKhamSucKhoeChuyenKhoa.bs_tam_than;
                                updateFields[nameof(SelectedKhamSucKhoeChuyenKhoa.chu_ky_tam_than)] = SelectedKhamSucKhoeChuyenKhoa.chu_ky_tam_than;
                            }

                            if (
                                updateFields.ContainsKey(nameof(SelectedKhamSucKhoeChuyenKhoa.kq_ngoai_khoa))
                                || updateFields.ContainsKey(nameof(SelectedKhamSucKhoeChuyenKhoa.pl_ngoai_khoa))
                                || updateFields.ContainsKey(nameof(SelectedKhamSucKhoeChuyenKhoa.kq_da_lieu))
                                || updateFields.ContainsKey(nameof(SelectedKhamSucKhoeChuyenKhoa.pl_da_lieu))
                            )
                            {
                                SelectedKhamSucKhoeChuyenKhoa.bs_ngoai_khoa = $"{CurrentUser?.chuc_danh} {CurrentUser?.full_name}";
                                SelectedKhamSucKhoeChuyenKhoa.chu_ky_ngoai_khoa = CurrentUser?.chu_ky_bac_si;
                                updateFields[nameof(SelectedKhamSucKhoeChuyenKhoa.bs_ngoai_khoa)] = SelectedKhamSucKhoeChuyenKhoa.bs_ngoai_khoa;
                                updateFields[nameof(SelectedKhamSucKhoeChuyenKhoa.chu_ky_ngoai_khoa)] = SelectedKhamSucKhoeChuyenKhoa.chu_ky_ngoai_khoa;
                            }

                            if (
                                updateFields.ContainsKey(nameof(SelectedKhamSucKhoeChuyenKhoa.benh_mat))
                                || updateFields.ContainsKey(nameof(SelectedKhamSucKhoeChuyenKhoa.pl_mat))
                                || updateFields.ContainsKey(nameof(SelectedKhamSucKhoeChuyenKhoa.thi_luc_khong_kinh_phai))
                                || updateFields.ContainsKey(nameof(SelectedKhamSucKhoeChuyenKhoa.thi_luc_khong_kinh_trai))
                                || updateFields.ContainsKey(nameof(SelectedKhamSucKhoeChuyenKhoa.thi_luc_co_kinh_phai))
                                || updateFields.ContainsKey(nameof(SelectedKhamSucKhoeChuyenKhoa.thi_luc_co_kinh_trai))
                            )
                            {
                                SelectedKhamSucKhoeChuyenKhoa.bs_mat = $"{CurrentUser?.chuc_danh} {CurrentUser?.full_name}";
                                SelectedKhamSucKhoeChuyenKhoa.chu_ky_mat = CurrentUser?.chu_ky_bac_si;
                                updateFields[nameof(SelectedKhamSucKhoeChuyenKhoa.bs_mat)] = SelectedKhamSucKhoeChuyenKhoa.bs_mat;
                                updateFields[nameof(SelectedKhamSucKhoeChuyenKhoa.chu_ky_mat)] = SelectedKhamSucKhoeChuyenKhoa.chu_ky_mat;
                            }

                            if (
                                updateFields.ContainsKey(nameof(SelectedKhamSucKhoeChuyenKhoa.kq_rhm_ham_tren))
                                || updateFields.ContainsKey(nameof(SelectedKhamSucKhoeChuyenKhoa.pl_rhm))
                                || updateFields.ContainsKey(nameof(SelectedKhamSucKhoeChuyenKhoa.kq_rhm_ham_duoi))
                                || updateFields.ContainsKey(nameof(SelectedKhamSucKhoeChuyenKhoa.benh_rhm))
                            )
                            {
                                SelectedKhamSucKhoeChuyenKhoa.bs_rhm = $"{CurrentUser?.chuc_danh} {CurrentUser?.full_name}";
                                SelectedKhamSucKhoeChuyenKhoa.chu_ky_rhm = CurrentUser?.chu_ky_bac_si;
                                updateFields[nameof(SelectedKhamSucKhoeChuyenKhoa.bs_rhm)] = SelectedKhamSucKhoeChuyenKhoa.bs_rhm;
                                updateFields[nameof(SelectedKhamSucKhoeChuyenKhoa.chu_ky_rhm)] = SelectedKhamSucKhoeChuyenKhoa.chu_ky_rhm;
                            }
                        }
                    }

                    if (updateFields.Any())
                    {
                        if (updateFields.ContainsKey(nameof(SelectedKhamSucKhoeChuyenKhoa.id)))
                        {
                            var result = await KhamSucKhoeChuyenKhoaService.UpdateAsync(new List<dynamic>() { updateDynamicObj });
                            if (result == null || !result.IsSuccess)
                            {
                                AlertService.ShowAlert("Đã có lỗi xảy ra khi lưu thông tin khám chuyên khoa!", "danger");
                                return;
                            }
                        }
                        else
                        {
                            var result = await KhamSucKhoeChuyenKhoaService.CreateAsync(new List<dynamic>() { updateDynamicObj });
                            if (result == null || !result.IsSuccess)
                            {
                                AlertService.ShowAlert("Đã có lỗi xảy ra khi lưu thông tin khám chuyên khoa!", "danger");
                                return;
                            }
                        }
                    }
                    #endregion

                    #region Ket qua CLS
                    List<dynamic> updateDynamicListObj = new List<dynamic>();
                    List<dynamic> addDynamicListObj = new List<dynamic>();
                    if (dynamicKhamCLSObj.Any())
                    {
                        foreach (var kqCLS in SelectedKhamSucKhoeKetQuaCanLamSangs)
                        {
                            var clsDynamicObj = dynamicKhamCLSObj.FirstOrDefault(c => c.type == kqCLS.type);
                            bool isAddEmptyValue = false;
                            if (clsDynamicObj != null)
                            {
                                updateDynamicObj = new ExpandoObject();
                                updateFields = (IDictionary<string, object?>)updateDynamicObj;
                                dict = clsDynamicObj as IDictionary<string, object?>;

                                if (dict != null && dict.Count > 0)
                                {
                                    var props = typeof(KhamSucKhoeKetQuaCanLamSangModel).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                                    foreach (var kv in dict.Where(kv => props.Contains(kv.Key)))
                                    {
                                        updateFields[kv.Key] = kv.Value?.GetCustumFieldOrValue();
                                    }

                                    if (updateFields.Any())
                                    {
                                        updateFields[nameof(kqCLS.luot_kham)] = SelectedItem.id;

                                        if (kqCLS.id > 0)
                                        {
                                            updateFields[nameof(kqCLS.id)] = kqCLS.id;
                                            updateDynamicListObj.Add(updateDynamicObj);
                                        }
                                        else
                                        {
                                            addDynamicListObj.Add(updateDynamicObj);
                                        }
                                    }
                                    else
                                    {
                                        isAddEmptyValue = true;
                                    }
                                }
                                else
                                {
                                    isAddEmptyValue = true;
                                }
                            }
                            else
                            {
                                isAddEmptyValue = true;
                            }

                            if (isAddEmptyValue && kqCLS.id <= 0)
                            {
                                updateDynamicObj = new ExpandoObject();
                                updateFields = (IDictionary<string, object?>)updateDynamicObj;
                                updateFields[nameof(kqCLS.id)] = kqCLS.id;
                                updateFields[nameof(kqCLS.type)] = kqCLS.type;
                                addDynamicListObj.Add(updateDynamicObj);
                            }
                        }
                    }

                    if (updateDynamicListObj.Any())
                    {
                        var result = await KhamSucKhoeKetQuaCanLamSangService.UpdateAsync(updateDynamicListObj);
                        if (result == null || !result.IsSuccess)
                        {
                            AlertService.ShowAlert("Đã có lỗi xảy ra khi lưu thông tin khám cận lâm sàng!", "danger");
                            return;
                        }
                    }
                    if (addDynamicListObj.Any())
                    {
                        var result = await KhamSucKhoeKetQuaCanLamSangService.CreateAsync(new List<dynamic>() { updateDynamicObj });
                        if (result == null || !result.IsSuccess)
                        {
                            AlertService.ShowAlert("Đã có lỗi xảy ra khi lưu thông tin khám cận lâm sàng!", "danger");
                            return;
                        }
                    }
                    #endregion

                    #region Ket luan
                    updateDynamicObj = new ExpandoObject();
                    updateFields = (IDictionary<string, object?>)updateDynamicObj;
                    dict = dynamicKetLuanObj as IDictionary<string, object?>;
                    if (dict != null && dict.Count > 0)
                    {
                        var props = typeof(KhamSucKhoeKetLuanModel).GetProperties().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                        foreach (var kv in dict.Where(kv => props.Contains(kv.Key)))
                        {
                            updateFields[kv.Key] = kv.Value?.GetCustumFieldOrValue();
                        }

                        if (updateFields.Any())
                        {
                            if (SelectedKhamSucKhoeKetLuan.id > 0)
                            {
                                updateFields[nameof(SelectedKhamSucKhoeKetLuan.id)] = SelectedKhamSucKhoeKetLuan.id;
                            }

                            SelectedKhamSucKhoeKetLuan.nguoi_ket_luan = $"{CurrentUser?.chuc_danh} {CurrentUser?.full_name}";
                            updateFields[nameof(SelectedKhamSucKhoeKetLuan.luot_kham)] = SelectedItem.id;
                            updateFields[nameof(SelectedKhamSucKhoeKetLuan.ma_luot_kham)] = SelectedItem.ma_luot_kham ?? string.Empty;
                            updateFields[nameof(SelectedKhamSucKhoeKetLuan.nguoi_ket_luan)] = SelectedKhamSucKhoeKetLuan.nguoi_ket_luan;
                            updateFields[nameof(SelectedKhamSucKhoeKetLuan.ngay_ket_luan)] = SelectedKhamSucKhoeKetLuan.ngay_ket_luan;
                            SelectedKhamSucKhoeKetLuan.ngay_ket_luan ??= DateTime.Now;
                            if (CurrentUser != null && CurrentUser.id != Guid.Empty)
                            {
                                updateFields[nameof(SelectedKhamSucKhoeKetLuan.bs_ket_luan)] = CurrentUser.id;
                                SelectedKhamSucKhoeKetLuan.bs_ket_luan = CurrentUser;
                            }
                        }
                    }

                    if (updateFields.Any())
                    {
                        if (updateFields.ContainsKey(nameof(SelectedKhamSucKhoeKetLuan.id)))
                        {
                            var result = await KhamSucKhoeKetLuanService.UpdateAsync(new List<dynamic>() { updateDynamicObj });
                            if (result == null || !result.IsSuccess)
                            {
                                AlertService.ShowAlert("Đã có lỗi xảy ra khi lưu kết luận!", "danger");
                                return;
                            }
                        }
                        else
                        {
                            var result = await KhamSucKhoeKetLuanService.CreateAsync(new List<dynamic>() { updateDynamicObj });
                            if (result == null || !result.IsSuccess)
                            {
                                AlertService.ShowAlert("Đã có lỗi xảy ra khi lưu kết luận!", "danger");
                                return;
                            }
                        }
                    }
                    #endregion
                }

                AlertService.ShowAlert("Lưu thông tin khám sức khỏe thành công!", "success");
                renderKey++;
                if (renderKey == int.MaxValue)
                {
                    renderKey = 0;
                }

                dynamicTheLucObj = new ExpandoObject();
                dynamicSanPhuKhoaObj = new ExpandoObject();
                dynamicChuyenKhoaObj = new ExpandoObject();
                dynamicKetLuanObj = new ExpandoObject();
                dynamicTienSuObj = new ExpandoObject();
                dynamicKhamCLSObj = new List<dynamic>();

                await LoadDetailData(SelectedItem.id);

                dynamicTheLucObjOriginal = SelectedKhamSucKhoeTheLuc.DeepClone();
                dynamicSanPhuKhoaObjOriginal = SelectedKhamSucKhoeSanPhuKhoa.DeepClone();
                dynamicChuyenKhoaObjOriginal = SelectedKhamSucKhoeChuyenKhoa.DeepClone();
                dynamicKetLuanObjOriginal = SelectedKhamSucKhoeKetLuan.DeepClone();
                dynamicTienSuObjOriginal = SelectedKhamSucKhoeTienSu.DeepClone();
                dynamicKhamCLSObjOriginal = SelectedKhamSucKhoeKetQuaCanLamSangs.DeepClone().Cast<dynamic>().ToList();
            }
            catch
            {
                AlertService.ShowAlert("Đã có lỗi xảy ra khi lưu thông tin!", "danger");
            }
            finally
            {
                Loading.Hide();
            }
        }

        public async Task OnEndSubmit()
        {
            try
            {
                Loading.Show();

                SelectedItem.status = Model.Base.Status.published;

                var result = await MainService.UpdateAsync(new List<dynamic>() { SelectedItem });
                if (result == null || !result.IsSuccess)
                {
                    AlertService.ShowAlert("Đã có lỗi xảy ra khi kết thúc!", "danger");
                    return;
                }
                else
                {
                    AlertService.ShowAlert("Kết thúc thành công!", "success");
                }
            }
            catch
            {
                AlertService.ShowAlert("Đã có lỗi xảy ra khi lưu thông tin!", "danger");
            }
            finally
            {
                Loading.Hide();
            }
        }

        public async Task OnCancelEndSubmit()
        {
            try
            {
                Loading.Show();

                SelectedItem.status = Model.Base.Status.draft;

                var result = await MainService.UpdateAsync(new List<dynamic>() { SelectedItem });
                if (result == null || !result.IsSuccess)
                {
                    AlertService.ShowAlert("Đã có lỗi xảy ra khi hủy kết thúc!", "danger");
                    return;
                }
                else
                {
                    AlertService.ShowAlert("Hủy kết thúc thành công!", "success");
                }
            }
            catch
            {
                AlertService.ShowAlert("Đã có lỗi xảy ra khi hủy kết thúc!", "danger");
            }
            finally
            {
                Loading.Hide();
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
                    await InvokeAsync(StateHasChanged);
                }
            }
            else
            {
                await JsRuntime.InvokeVoidAsync("alert", "File size exceeds 5MB limit");
            }
        }

        private async Task OnValueChanged(ChangeEventArgs e, string fieldName, bool isFilter = false, bool isDate = true, object item = default!)
        {
            try
            {
                void UpdateDynamicField(object dynamicObj, object dynamicObjOriginal, object? value, Type type, object selected)
                {
                    UpdateField(dynamicObj, dynamicObjOriginal, fieldName, value, type, selected);
                }

                void HandleModelUpdate(object? value, object dynamicObj, object dynamicObjOriginal, Type type, object selected)
                {
                    UpdateDynamicField(dynamicObj, dynamicObjOriginal, value, type, selected);
                }

                if (!isDate)
                {
                    var value = e.Value;
                    if (value == null || string.IsNullOrEmpty(value.ToString()))
                    {
                        ReflectionHelper.SetFieldValue(this, item, fieldName, null);
                        if (item != default)
                        {
                            switch (item)
                            {
                                case KhamSucKhoeChuyenKhoaModel:
                                    HandleModelUpdate(null, dynamicChuyenKhoaObj, dynamicChuyenKhoaObjOriginal, item.GetType(), item);
                                    break;
                                case KhamSucKhoeSanPhuKhoaModel:
                                    HandleModelUpdate(null, dynamicSanPhuKhoaObj, dynamicSanPhuKhoaObjOriginal, item.GetType(), item);
                                    break;
                                case KhamSucKhoeTheLucModel:
                                    HandleModelUpdate(null, dynamicTheLucObj, dynamicTheLucObjOriginal, item.GetType(), item);
                                    break;
                                case KhamSucKhoeKetLuanModel:
                                    HandleModelUpdate(null, dynamicKetLuanObj, dynamicKetLuanObjOriginal, item.GetType(), item);
                                    break;
                                case KhamSucKhoeTienSuModel:
                                    HandleModelUpdate(null, dynamicTienSuObj, dynamicTienSuObjOriginal, item.GetType(), item);
                                    break;
                            }
                        }
                    }
                    else
                    {
                        var valueType = value.GetType();
                        if (valueType.IsClass && valueType != typeof(string))
                        {
                            ReflectionHelper.SetFieldValue(this, item, fieldName, value);
                            if (item != default)
                            {
                                switch (item)
                                {
                                    case KhamSucKhoeChuyenKhoaModel:
                                        HandleModelUpdate(value, dynamicChuyenKhoaObj, dynamicChuyenKhoaObjOriginal, item.GetType(), item);
                                        break;
                                    case KhamSucKhoeSanPhuKhoaModel:
                                        HandleModelUpdate(value, dynamicSanPhuKhoaObj, dynamicSanPhuKhoaObjOriginal, item.GetType(), item);
                                        break;
                                    case KhamSucKhoeTheLucModel:
                                        HandleModelUpdate(value, dynamicTheLucObj, dynamicTheLucObjOriginal, item.GetType(), item);
                                        break;
                                    case KhamSucKhoeKetLuanModel:
                                        HandleModelUpdate(value, dynamicKetLuanObj, dynamicKetLuanObjOriginal, item.GetType(), item);
                                        break;
                                    case KhamSucKhoeTienSuModel:
                                        HandleModelUpdate(value, dynamicTienSuObj, dynamicTienSuObjOriginal, item.GetType(), item);
                                        break;
                                }
                            }
                        }
                        else
                        {
                            var strValue = value.ToString();
                            ReflectionHelper.SetFieldValue(this, item, fieldName, strValue);
                            if (item != default)
                            {
                                switch (item)
                                {
                                    case KhamSucKhoeChuyenKhoaModel:
                                        HandleModelUpdate(strValue, dynamicChuyenKhoaObj, dynamicChuyenKhoaObjOriginal, item.GetType(), item);
                                        break;
                                    case KhamSucKhoeSanPhuKhoaModel:
                                        HandleModelUpdate(strValue, dynamicSanPhuKhoaObj, dynamicSanPhuKhoaObjOriginal, item.GetType(), item);
                                        break;
                                    case KhamSucKhoeTheLucModel:
                                        HandleModelUpdate(strValue, dynamicTheLucObj, dynamicTheLucObjOriginal, item.GetType(), item);
                                        break;
                                    case KhamSucKhoeKetLuanModel:
                                        HandleModelUpdate(strValue, dynamicKetLuanObj, dynamicKetLuanObjOriginal, item.GetType(), item);
                                        break;
                                    case KhamSucKhoeTienSuModel:
                                        HandleModelUpdate(strValue, dynamicTienSuObj, dynamicTienSuObjOriginal, item.GetType(), item);
                                        break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    var dateStr = e.Value?.ToString();
                    if (string.IsNullOrEmpty(dateStr))
                    {
                        ReflectionHelper.SetFieldValue(this, item, fieldName, null);
                        if (item != default)
                        {
                            switch (item)
                            {
                                case KhamSucKhoeChuyenKhoaModel:
                                    HandleModelUpdate(null, dynamicChuyenKhoaObj, dynamicChuyenKhoaObjOriginal, item.GetType(), item);
                                    break;
                                case KhamSucKhoeSanPhuKhoaModel:
                                    HandleModelUpdate(null, dynamicSanPhuKhoaObj, dynamicSanPhuKhoaObjOriginal, item.GetType(), item);
                                    break;
                                case KhamSucKhoeTheLucModel:
                                    HandleModelUpdate(null, dynamicTheLucObj, dynamicTheLucObjOriginal, item.GetType(), item);
                                    break;
                                case KhamSucKhoeKetLuanModel:
                                    HandleModelUpdate(null, dynamicKetLuanObj, dynamicKetLuanObjOriginal, item.GetType(), item);
                                    break;
                                case KhamSucKhoeTienSuModel:
                                    HandleModelUpdate(null, dynamicTienSuObj, dynamicTienSuObjOriginal, item.GetType(), item);
                                    break;
                            }
                        }
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
                            ReflectionHelper.SetFieldValue(this, item, fieldName, date);
                            if (item != default)
                            {
                                switch (item)
                                {
                                    case KhamSucKhoeChuyenKhoaModel:
                                        HandleModelUpdate(date, dynamicChuyenKhoaObj, dynamicChuyenKhoaObjOriginal, item.GetType(), item);
                                        break;
                                    case KhamSucKhoeSanPhuKhoaModel:
                                        HandleModelUpdate(date, dynamicSanPhuKhoaObj, dynamicSanPhuKhoaObjOriginal, item.GetType(), item);
                                        break;
                                    case KhamSucKhoeTheLucModel:
                                        HandleModelUpdate(date, dynamicTheLucObj, dynamicTheLucObjOriginal, item.GetType(), item);
                                        break;
                                    case KhamSucKhoeKetLuanModel:
                                        HandleModelUpdate(date, dynamicKetLuanObj, dynamicKetLuanObjOriginal, item.GetType(), item);
                                        break;
                                    case KhamSucKhoeTienSuModel:
                                        HandleModelUpdate(date, dynamicTienSuObj, dynamicTienSuObjOriginal, item.GetType(), item);
                                        break;
                                }
                            }
                        }
                    }
                }

                if (isFilter && !Loading.IsBusy)
                {
                    await LoadData(true);
                }

                if (nameof(SelectedKhamSucKhoeTheLuc.chieu_cao).Equals(fieldName) || nameof(SelectedKhamSucKhoeTheLuc.can_nang).Equals(fieldName))
                {
                    var chieuCao = (SelectedKhamSucKhoeTheLuc.chieu_cao ?? 0) / 100;
                    var canNang = SelectedKhamSucKhoeTheLuc.can_nang ?? 0;
                    SelectedKhamSucKhoeTheLuc.bmi = chieuCao > 0 ? Math.Round(canNang / (chieuCao * chieuCao), 2) : 0;
                    UpdateField(
                        dynamicTheLucObj,
                        dynamicTheLucObjOriginal,
                        nameof(SelectedKhamSucKhoeTheLuc.bmi),
                        SelectedKhamSucKhoeTheLuc.bmi,
                        SelectedKhamSucKhoeTheLuc.GetType(),
                        SelectedKhamSucKhoeTheLuc
                    );
                }
                await InvokeAsync(StateHasChanged);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                AlertService.ShowAlert("Lỗi khi xử lý dữ liệu nhập", "danger");
            }
        }

        private void OnKhamCLSChanged(ChangeEventArgs e, KetQuaCanLamSang? type)
        {
            if (type == null)
            {
                return;
            }

            var value = e.Value?.ToString();
            var kqCLS = SelectedKhamSucKhoeKetQuaCanLamSangs.FirstOrDefault(c => c.type == type.ToString());
            if (kqCLS != null)
            {
                kqCLS.ket_qua = value;
                kqCLS.kq_cls = null;

                if (!dynamicKhamCLSObj.Any(c => c.type == kqCLS.type))
                {
                    var newObj = new ExpandoObject();
                    var newDict = newObj as IDictionary<string, object?>;
                    newDict[nameof(kqCLS.type)] = kqCLS.type;
                    dynamicKhamCLSObj.Add(newObj);
                }
                var selectedKqCls = dynamicKhamCLSObj.FirstOrDefault(c => c.type == kqCLS.type);
                if (selectedKqCls != null)
                {
                    UpdateField(selectedKqCls, dynamicKhamCLSObjOriginal.FirstOrDefault(c => c.type == kqCLS.type), nameof(kqCLS.kq_cls), null, kqCLS.GetType(), kqCLS);
                    UpdateField(selectedKqCls, dynamicKhamCLSObjOriginal.FirstOrDefault(c => c.type == kqCLS.type), nameof(kqCLS.ket_qua), kqCLS.ket_qua, kqCLS.GetType(), kqCLS);

                    InvokeAsync(StateHasChanged);
                }
            }
        }

        private void ContractFilterChanged(ContractModel? contract)
        {
            _contractFilter = contract;
        }

        private void KhamSucKhoeCongTyChanged(KhamSucKhoeCongTyModel? khamSucKhoeCongTy)
        {
            _khamSucKhoeCongTyFilter = khamSucKhoeCongTy;
        }

        private void OnTinhChatKinhChanged(ChangeEventArgs args, string value)
        {
            bool isChecked = false;
            if (args.Value is bool b)
            {
                isChecked = b;
            }
            else if (args.Value is string s && bool.TryParse(s, out var parsed))
            {
                isChecked = parsed;
            }

            if (isChecked)
            {
                SelectedKhamSucKhoeSanPhuKhoa.tinh_chat_kinh = value;
            }
            else
            {
                SelectedKhamSucKhoeSanPhuKhoa.tinh_chat_kinh = "";
            }

            UpdateField(
                    dynamicSanPhuKhoaObj,
                    dynamicSanPhuKhoaObjOriginal,
                    nameof(SelectedKhamSucKhoeSanPhuKhoa.tinh_chat_kinh),
                    SelectedKhamSucKhoeSanPhuKhoa.tinh_chat_kinh,
                    SelectedKhamSucKhoeSanPhuKhoa.GetType(),
                    SelectedKhamSucKhoeSanPhuKhoa
                );
        }

        private void OnDauBungKinhChanged(ChangeEventArgs args, string value)
        {
            bool isChecked = false;
            if (args.Value is bool b)
            {
                isChecked = b;
            }
            else if (args.Value is string s && bool.TryParse(s, out var parsed))
            {
                isChecked = parsed;
            }

            if (isChecked)
            {
                SelectedKhamSucKhoeSanPhuKhoa.dau_bung_kinh = value == YesNo.Co.ToString();
            }
            else
            {
                SelectedKhamSucKhoeSanPhuKhoa.dau_bung_kinh = null;
            }

            UpdateField(
                dynamicSanPhuKhoaObj,
                dynamicSanPhuKhoaObjOriginal,
                nameof(SelectedKhamSucKhoeSanPhuKhoa.dau_bung_kinh),
                SelectedKhamSucKhoeSanPhuKhoa.dau_bung_kinh,
                SelectedKhamSucKhoeSanPhuKhoa.GetType(),
                SelectedKhamSucKhoeSanPhuKhoa
            );
        }

        private void OnSoLanMoPhuKhoaChanged(ChangeEventArgs args, int value)
        {
            bool isChecked = false;
            if (args.Value is bool b)
            {
                isChecked = b;
            }
            else if (args.Value is string s && bool.TryParse(s, out var parsed))
            {
                isChecked = parsed;
            }

            if (isChecked)
            {
                SelectedKhamSucKhoeSanPhuKhoa.so_lan_mo_san_phu_khoa = value;
            }
            else
            {
                SelectedKhamSucKhoeSanPhuKhoa.so_lan_mo_san_phu_khoa = null;
            }

            UpdateField(
                dynamicSanPhuKhoaObj,
                dynamicSanPhuKhoaObjOriginal,
                nameof(SelectedKhamSucKhoeSanPhuKhoa.so_lan_mo_san_phu_khoa),
                SelectedKhamSucKhoeSanPhuKhoa.so_lan_mo_san_phu_khoa,
                SelectedKhamSucKhoeSanPhuKhoa.GetType(),
                SelectedKhamSucKhoeSanPhuKhoa
            );
        }

        private void OnApDungBPPTChanged(ChangeEventArgs args, string value)
        {
            bool isChecked = false;
            if (args.Value is bool b)
            {
                isChecked = b;
            }
            else if (args.Value is string s && bool.TryParse(s, out var parsed))
            {
                isChecked = parsed;
            }

            if (isChecked)
            {
                SelectedKhamSucKhoeSanPhuKhoa.ap_dung_bptt = value == YesNo.Co.ToString();
            }
            else
            {
                SelectedKhamSucKhoeSanPhuKhoa.ap_dung_bptt = null;
            }

            UpdateField(
                dynamicSanPhuKhoaObj,
                dynamicSanPhuKhoaObjOriginal,
                nameof(SelectedKhamSucKhoeSanPhuKhoa.ap_dung_bptt),
                SelectedKhamSucKhoeSanPhuKhoa.ap_dung_bptt,
                SelectedKhamSucKhoeSanPhuKhoa.GetType(),
                SelectedKhamSucKhoeSanPhuKhoa
            );
        }

        private async Task OnShowAll()
        {
            isShowOnlyMe = !isShowOnlyMe;

            onBS = CurrentUser?.role?.ToLower() == CurrentSetting.doctor_role_id?.ToLower().ToString();

            if (!isShowOnlyMe || isShowOnlyMe && onBS)
            {
                activeDefTab = "tab1";
            }
            else if (!isShowOnlyMe || isShowOnlyMe && (onBS || onBSTuanHoan || onBSHoHap || onBSTieuHoa || onBSThanTietNieu || onBSNoiTiet || onBSCoXuongKhop || onBSThanKinh || onBSThanKinh || onBSTamThan || onBSNgoaiKhoa || onBSSanPhuKhoa || onBSMat || onBSTaiMuiHong || onBSRangHamMat))
            {
                activeDefTab = "tab2";
            }
            else if (!isShowOnlyMe || isShowOnlyMe && onBSKetLuan)
            {
                activeDefTab = "tab3";
            }
            else
            {
                activeDefTab = "";
            }

            await InvokeAsync(StateHasChanged);
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

            UpdateField(
                dynamicSanPhuKhoaObj,
                dynamicSanPhuKhoaObjOriginal,
                nameof(SelectedKhamSucKhoeSanPhuKhoa.para),
                SelectedKhamSucKhoeSanPhuKhoa.para,
                SelectedKhamSucKhoeSanPhuKhoa.GetType(),
                SelectedKhamSucKhoeSanPhuKhoa
            );
        }

        private void OnKetQuaCanLamSangChanged(KetQuaCanLamSangModel? selected, KhamSucKhoeKetQuaCanLamSangModel item)
        {
            try
            {
                item.kq_cls = selected;
                item.ket_qua = selected?.ket_luan_can_lam_sang;

                if (!dynamicKhamCLSObj.Any(c => c.type == item.type))
                {
                    var newObj = new ExpandoObject();
                    var newDict = newObj as IDictionary<string, object?>;
                    newDict[nameof(item.type)] = item.type;
                    dynamicKhamCLSObj.Add(newObj);
                }
                var selectedKqCls = dynamicKhamCLSObj.FirstOrDefault(c => c.type == item.type);
                if (selectedKqCls != null)
                {
                    UpdateField(selectedKqCls, dynamicKhamCLSObjOriginal.FirstOrDefault(c => c.type == item.type), nameof(item.kq_cls), selected?.ket_luan_can_lam_sang, item.GetType(), item);
                    UpdateField(selectedKqCls, dynamicKhamCLSObjOriginal.FirstOrDefault(c => c.type == item.type), nameof(item.ket_qua), selected, item.GetType(), item);
                }
            }
            catch (Exception ex)
            {
                AlertService.ShowAlert($"Lỗi khi xử lý dữ liệu: {ex.Message}", "danger");
            }
            finally
            {
                InvokeAsync(StateHasChanged);
            }
        }
        private void UpdateField(object dynamicObj, object dynamicOriginal, string fieldName, object? value, Type type, object selected)
        {
            var prop = type.GetProperty(fieldName);
            var dict = dynamicObj as IDictionary<string, object>;
            if (dict != null && fieldName != null)
            {
                object? selectedValue = null;
                object? convertedValue = value;
                if (prop != null)
                {
                    if (prop.CanRead)
                    {
                        selectedValue = prop.GetValue(selected);
                    }

                    if (selectedValue != null && value != null && value.GetType() != selectedValue.GetType())
                    {
                        try
                        {
                            var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                            convertedValue = Convert.ChangeType(value, targetType);
                        }
                        catch
                        {
                            convertedValue = value;
                        }
                    }
                }
                if (
                    dict.ContainsKey(fieldName)
                    && Equals(dynamicOriginal.GetPropertyValue(fieldName), selected.GetPropertyValue(fieldName))
                )
                {
                    if (dynamicOriginal is KhamSucKhoeKetQuaCanLamSangModel && fieldName.Equals(nameof(KhamSucKhoeKetQuaCanLamSangModel.ket_qua)))
                    {
                        dynamicKhamCLSObj = dynamicKhamCLSObj.Where(c => c.type != ((dynamic)dynamicObj).type).ToList();
                    }
                    else
                    {
                        dict.Remove(fieldName);
                    }

                    return;
                }
                dict[fieldName] = convertedValue!;
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
                if (!string.IsNullOrEmpty(fallbackText))
                {
                    return new MarkupString(fallbackText);
                }

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

        /// <summary>
        /// Generate a table (HTML) showing changed fields: field name, current value, changed value.
        /// </summary>
        private string GenerateChangeNoteTable()
        {
            string yesVal = YesNo.Co.GetDescription();
            string noVal = YesNo.Khong.GetDescription();

            var tableBuilder = new System.Text.StringBuilder();
            tableBuilder.AppendLine("<table class='table table-bordered table-sm table-fixed'><thead style='position:sticky;top:0;background: aliceblue;' class='whitespace-pre-wrap'><tr><th style='width: 20%;'>Thông tin</th><th style='width: 40%;' class='whitespace-pre-wrap'>Giá trị hiện tại</th><th style='width: 40%;' class='whitespace-pre-wrap'>Giá trị mới</th></tr></thead><tbody>");

            void AppendChangesFromDynamic(object dynamicObj, object? originalObj, Type modelType, string sectionName)
            {
                var dict = dynamicObj as IDictionary<string, object?>;
                if (dict == null || dict.Count == 0)
                {
                    return;
                }

                tableBuilder.AppendLine($"<tr><td colspan='3'><strong>{sectionName}</strong></td></tr>");
                foreach (var kv in dict)
                {
                    var prop = modelType.GetProperty(kv.Key);
                    if (prop == null)
                    {
                        continue;
                    }

                    var displayNameAttr = prop.GetCustomAttributes(typeof(System.ComponentModel.DisplayNameAttribute), true)
                        .FirstOrDefault() as System.ComponentModel.DisplayNameAttribute;
                    var displayName = displayNameAttr?.DisplayName ?? prop.Name;

                    object? currentValueObj = originalObj?.GetPropertyValue(kv.Key, "name");
                    object? changedValueObj = kv.Value?.GetCustumFieldOrValue("name");

                    string currentValueStr = "";
                    string changedValueStr = "";

                    if (kv.Key == "tinh_chat_kinh")
                    {
                        currentValueStr = currentValueObj?.ToString()?.GetDescriptionFromString<TinhChatKinh>() ?? "";
                        changedValueStr = changedValueObj?.ToString()?.GetDescriptionFromString<TinhChatKinh>() ?? "";
                    }
                    else if (kv.Key == "so_lan_mo_san_phu_khoa")
                    {
                        int currVal;
                        if (int.TryParse(currentValueObj?.ToString(), out currVal))
                        {
                            currentValueStr = currVal == 1 ? yesVal : noVal;
                        }

                        if (int.TryParse(changedValueObj?.ToString(), out currVal))
                        {
                            changedValueStr = currVal == 1 ? yesVal : noVal;
                        }
                    }
                    else if (kv.Key == "para")
                    {
                        currentValueStr = currentValueObj?.ToString()?.Replace("|", "").Replace("-", "") ?? "";
                        changedValueStr = changedValueObj?.ToString()?.Replace("|", "").Replace("-", "") ?? "";
                    }
                    else if (kv.Key == "ngay_ket_luan")
                    {
                        if (currentValueObj is DateTime currentDate)
                        {
                            currentValueStr = currentDate.ToString("dd/MM/yyyy");
                        }
                        else
                        {
                            currentValueStr = currentValueObj?.ToString() ?? "";
                        }
                        if (changedValueObj is DateTime changedDate)
                        {
                            changedValueStr = changedDate.ToString("dd/MM/yyyy");
                        }
                        else
                        {
                            changedValueStr = changedValueObj?.ToString() ?? "";
                        }
                    }
                    else if (prop.PropertyType == typeof(bool) || prop.PropertyType == typeof(bool?))
                    {
                        bool? currentBool = null;
                        bool? changedBool = null;
                        if (currentValueObj is bool b1 || bool.TryParse(currentValueObj?.ToString(), out b1))
                        {
                            currentBool = b1;
                        }

                        if (changedValueObj is bool b2 || bool.TryParse(changedValueObj?.ToString(), out b2))
                        {
                            changedBool = b2;
                        }

                        if (currentBool != null)
                        {
                            currentValueStr = currentBool == true ? yesVal : noVal;
                        }

                        if (changedBool != null)
                        {
                            changedValueStr = changedBool == true ? yesVal : noVal;
                        }
                    }
                    else
                    {
                        currentValueStr = currentValueObj?.ToString() ?? "";
                        changedValueStr = changedValueObj?.ToString() ?? "";
                    }

                    tableBuilder.AppendLine($"<tr><td class='whitespace-pre-wrap'>{displayName}</td><td class='whitespace-pre-wrap'>{currentValueStr}</td><td class='whitespace-pre-wrap text-danger'>{changedValueStr}</td></tr>");
                }
            }

            AppendChangesFromDynamic(dynamicTheLucObj, dynamicTheLucObjOriginal, typeof(KhamSucKhoeTheLucModel), "Thể lực");
            AppendChangesFromDynamic(dynamicSanPhuKhoaObj, dynamicSanPhuKhoaObjOriginal, typeof(KhamSucKhoeSanPhuKhoaModel), "Sản phụ khoa");
            AppendChangesFromDynamic(dynamicChuyenKhoaObj, dynamicChuyenKhoaObjOriginal, typeof(KhamSucKhoeChuyenKhoaModel), "Chuyên khoa");
            AppendChangesFromDynamic(dynamicKetLuanObj, dynamicKetLuanObjOriginal, typeof(KhamSucKhoeKetLuanModel), "Kết luận");
            AppendChangesFromDynamic(dynamicTienSuObj, dynamicTienSuObjOriginal, typeof(KhamSucKhoeTienSuModel), "Tiền sử");
            if (dynamicKhamCLSObj.Any())
            {
                tableBuilder.AppendLine($"<tr><td colspan='3'><strong>Kết quả khám CLS</strong></td></tr>");
                foreach (var cls in dynamicKhamCLSObj)
                {
                    var originalCls = dynamicKhamCLSObjOriginal.FirstOrDefault(c => c.type == cls.type);

                    var displayName = ((object)cls).GetPropertyValue("type")?.GetEnumDescription(typeof(KetQuaCanLamSang));
                    var ketQuaOriginal = ((object?)originalCls)?.GetPropertyValue("ket_qua");
                    var ketQuaChanged = ((object)cls).GetPropertyValue("ket_qua");
                    tableBuilder.AppendLine($"<tr><td class='whitespace-pre-wrap'>{displayName}</td><td class='whitespace-pre-wrap'>{ketQuaOriginal}</td><td class='whitespace-pre-wrap text-danger'>{ketQuaChanged}</td></tr>");
                }
            }

            tableBuilder.AppendLine("</tbody></table>");
            return tableBuilder.ToString();
        }
    }
}
