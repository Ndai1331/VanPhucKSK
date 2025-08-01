using CoreAdminWeb.Enums;
using CoreAdminWeb.Helpers;
using CoreAdminWeb.Model;
using CoreAdminWeb.Model.User;
using CoreAdminWeb.Services.BaseServices;
using CoreAdminWeb.Services.ICaNhanSoKhamSucKhoeService;
using CoreAdminWeb.Services.Users;
using CoreAdminWeb.Shared.Base;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using CoreAdminWeb.Extensions;

namespace CoreAdminWeb.Pages.Admins.HoSoKhamSucKhoeTT32
{
    public partial class Index(
        ICaNhanSoKhamSucKhoeService<HoSoKhamSucKhoeTT32Model> MainService,
        IBaseService<CongTyModel> CongTyService,
        IUserService UserService,
        IConfiguration Configuration,
        IBaseDetailService<SoKhamSucKhoeModel> SoKhamSucKhoeService,
        IBaseDetailService<KhamSucKhoeChuyenKhoaModel> KhamSucKhoeChuyenKhoaService,
        IBaseDetailService<KhamSucKhoeKetLuanModel> KhamSucKhoeKetLuanService,
        IBaseDetailService<KhamSucKhoeSanPhuKhoaModel> KhamSucKhoeSanPhuKhoaService,
        IBaseDetailService<KhamSucKhoeTheLucModel> KhamSucKhoeTheLucService,
        IBaseDetailService<KhamSucKhoeTienSuModel> KhamSucKhoeTienSuService,
        IBaseDetailService<KhamSucKhoeKetQuaCanLamSangModel> KhamSucKhoeKetQuaCanLamSangService,
        IBaseDetailService<KhamSucKhoeNgheNghiepModel> KhamSucKhoeNgheNghiepService,
        IBaseService<KhamSucKhoeCongTyModel> KhamSucKhoeCongTyService,
        IWebHostEnvironment WebHostEnvironment
    ) : BlazorCoreBase
    {
        #region Constants
        private const string DEFAULT_PROFILE_IMAGE = "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='120' height='160' viewBox='0 0 120 160'%3E%3Crect width='120' height='160' fill='%23f8f9fa' stroke='%23dee2e6' stroke-width='1'/%3E%3Ctext x='60' y='80' text-anchor='middle' fill='%236c757d' font-size='12'%3EẢnh 4x6 cm%3C/text%3E%3C/svg%3E";
        #endregion

        private string _logoPath = $"/images/Logo/logo.png";
        private readonly string _imagesFolder = $"/images/";
        private string _profileImageUrl = string.Empty;

        [Parameter] public int? Id { get; set; }
        private List<HoSoKhamSucKhoeTT32Model> MainModels { get; set; } = new();
        private DateTime? _fromDate = null;
        private DateTime? _toDate = null;
        private CongTyModel? _selectedCongTyFilter = null;
        private UserModel? _benhNhanFilter { get; set; } = null;
        private bool openDetailModal = false;
        // private HoSoKhamSucKhoeTT32Model SelectedItem { get; set; } = new HoSoKhamSucKhoeTT32Model();
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
        private string para1 { get; set; } = string.Empty;
        private string para2 { get; set; } = string.Empty;
        private string para3 { get; set; } = string.Empty;
        private string para4 { get; set; } = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await LoadData();
                _logoPath = $"{Configuration["DrCoreApi:BaseUrlImage"]}/images/Logo/logo.png";
                await JsRuntime.InvokeAsync<IJSObjectReference>("import", "/assets/js/pages/flatpickr.js");
                SetProfileImagePlaceholder();
                StateHasChanged();

                // Wait for modal to render
                _ = Task.Run(async () =>
                {
                    await Task.Delay(500);
                    await JsRuntime.InvokeVoidAsync("initializeDatePicker");
                });
            }
        }

        private void SetProfileImagePlaceholder()
        {
            if (string.IsNullOrEmpty(_profileImageUrl))
            {
                _profileImageUrl = DEFAULT_PROFILE_IMAGE;
            }
        }

        private async Task LoadData()
        {
            IsLoading = true;
            // var benhNhan = _benhNhanFilter != null ? _benhNhanFilter.id.ToString() : "";
            // var congTy = _selectedCongTyFilter != null ? _selectedCongTyFilter.id.ToString() : "";

            BuilderQuery = $"CaNhan/medical-data?limit={PageSize}&offset={(Page - 1) * PageSize}";
            if (_benhNhanFilter != null)
            {
                BuilderQuery += $"&benhNhan={_benhNhanFilter.id}";
            }
            if (_selectedCongTyFilter != null)
            {
                BuilderQuery += $"&congTy={_selectedCongTyFilter.id.ToString()}";
            }
            if (_fromDate.HasValue)
            {
                BuilderQuery += $"&fromDate={_fromDate.Value.ToString("yyyy-MM-dd")}";
            }
            if (_toDate.HasValue)
            {
                BuilderQuery += $"&toDate={_toDate.Value.ToString("yyyy-MM-dd")}";
            }

            var result = await MainService.GetAllAsync(BuilderQuery);
            if (result.IsSuccess)
            {
                MainModels = result.Data ?? new List<HoSoKhamSucKhoeTT32Model>();
                if (result.Meta != null)
                {
                    TotalItems = result.Meta.filter_count ?? 0;
                    TotalPages = result.Meta.page_count ?? 0;
                }
            }
            else
            {
                MainModels = new List<HoSoKhamSucKhoeTT32Model>();
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

        private async Task<IEnumerable<UserModel>> LoadBenhNhanFilterData(string searchText)
        {
            string queryBenhNhan = "role=patient";
            if (!string.IsNullOrEmpty(searchText))
            {
                queryBenhNhan = $"&filter[_and][][ma_benh_nhan][_eq]={searchText}";
            }
            var res = await UserService.GetAllAsync(queryBenhNhan);
            return res?.IsSuccess == true && res.Data != null ? res.Data : new List<UserModel>();
        }

        private async Task OnCongTyFilterChanged(CongTyModel? selected)
        {
            if (selected != null)
            {
                _selectedCongTyFilter = selected;
            }
            else
            {
                _selectedCongTyFilter = null;
            }

            await LoadData();
        }

        private async Task OnBenhNhanFilterChanged(UserModel? selected)
        {
            if (selected != null)
            {
                _benhNhanFilter = selected;
            }
            else
            {
                _benhNhanFilter = null;
            }

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

        private async Task OpenDetailModal(HoSoKhamSucKhoeTT32Model item)
        {
            openDetailModal = true;
            await LoadDetailData(item);
        }

        private async Task LoadDetailData(HoSoKhamSucKhoeTT32Model item)
        {
            IsLoading = true;
            var resSoKhamSK = await SoKhamSucKhoeService.GetByIdAsync(item.id.ToString());
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

                string query = $"&filter[_and][][ma_luot_kham][_contains]={SelectedItem?.ma_luot_kham}";

                var tasks = new[]
                {
                    BaseServiceHelper.LoadSingleRecordAsync(KhamSucKhoeChuyenKhoaService, query, r => SelectedKhamSucKhoeChuyenKhoa = r ?? new KhamSucKhoeChuyenKhoaModel()),
                    BaseServiceHelper.LoadSingleRecordAsync(KhamSucKhoeKetLuanService, query, r => SelectedKhamSucKhoeKetLuan = r ?? new KhamSucKhoeKetLuanModel()),
                    BaseServiceHelper.LoadSingleRecordAsync(KhamSucKhoeSanPhuKhoaService, query, r => SelectedKhamSucKhoeSanPhuKhoa = r ?? new KhamSucKhoeSanPhuKhoaModel()),
                    BaseServiceHelper.LoadSingleRecordAsync(KhamSucKhoeTheLucService, query, r => SelectedKhamSucKhoeTheLuc = r ?? new KhamSucKhoeTheLucModel()),
                    BaseServiceHelper.LoadSingleRecordAsync(KhamSucKhoeTienSuService, query, r => SelectedKhamSucKhoeTienSu = r ?? new KhamSucKhoeTienSuModel()),
                    BaseServiceHelper.LoadSingleRecordAsync(KhamSucKhoeCongTyService, query, r => SelectedKhamSucKhoeCongTy = r ?? new KhamSucKhoeCongTyModel()),
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

        private MarkupString RenderSignature(string? signatureData, string? fallbackText = "", string? fileName = "", int maxWidth = 120, int maxHeight = 60)
        {
            if (string.IsNullOrEmpty(signatureData))
            {
                return new MarkupString();
            }

            var html = signatureData.GetSignatureDisplayHtml(fallbackText, fileName, maxWidth, maxHeight,
                WebHostEnvironment.WebRootPath + _imagesFolder + SelectedItem.ma_luot_kham, Configuration["DrCoreApi:BaseUrlImage"]);
            return new MarkupString(html);
        }

        private void CloseDetailModal()
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

            openDetailModal = false;
        }
    }

}
