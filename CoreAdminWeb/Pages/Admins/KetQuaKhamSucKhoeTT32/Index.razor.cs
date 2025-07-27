using CoreAdminWeb.Enums;
using CoreAdminWeb.Helpers;
using CoreAdminWeb.Model;
using CoreAdminWeb.Model.Contract;
using CoreAdminWeb.Model.User;
using CoreAdminWeb.Services.BaseServices;
using CoreAdminWeb.Services.Files;
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
                               IBaseDetailService<KhamSucKhoeCanLamSangModel> KhamSucKhoeCanLamSangService) : BlazorCoreBase
    {
        private List<SoKhamSucKhoeModel> MainModels { get; set; } = new();
        private string activeDefTab = "tab1";

        private SoKhamSucKhoeModel SelectedItem { get; set; } = new SoKhamSucKhoeModel();
        private UserModel SelectedUser { get; set; } = new UserModel();
        private KhamSucKhoeChuyenKhoaModel SelectedKhamSucKhoeChuyenKhoa { get; set; } = new KhamSucKhoeChuyenKhoaModel();
        private KhamSucKhoeKetLuanModel SelectedKhamSucKhoeKetLuan { get; set; } = new KhamSucKhoeKetLuanModel();
        private KhamSucKhoeSanPhuKhoaModel SelectedKhamSucKhoeSanPhuKhoa { get; set; } = new KhamSucKhoeSanPhuKhoaModel();
        private KhamSucKhoeTheLucModel SelectedKhamSucKhoeTheLuc { get; set; } = new KhamSucKhoeTheLucModel();
        private KhamSucKhoeTienSuModel SelectedKhamSucKhoeTienSu { get; set; } = new KhamSucKhoeTienSuModel();
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
                BuilderQuery += $"&filter[_and][][MaDotKham][ngay_du_kien_kham][_gte]={_startDateFilter:yyyy-MM-dd}";
            }
            if (_endDateFilter.HasValue)
            {
                BuilderQuery += $"&filter[_and][][MaDotKham][ngay_du_kien_kham][_lte]={_endDateFilter:yyyy-MM-dd}";
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

        private async Task LoadDetailData(int soKhamSKId)
        {
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

                string query = "&filter[_and][0][deleted][_eq]=false" +
                               $"&filter[_and][][ma_luot_kham][_contains]={SelectedItem.ma_luot_kham}";

                SelectedKhamSucKhoeChuyenKhoa = await BaseServiceHelper.GetFirstOrDefaultAsync(KhamSucKhoeChuyenKhoaService, query);
                SelectedKhamSucKhoeKetLuan = await BaseServiceHelper.GetFirstOrDefaultAsync(KhamSucKhoeKetLuanService, query);
                SelectedKhamSucKhoeSanPhuKhoa = await BaseServiceHelper.GetFirstOrDefaultAsync(KhamSucKhoeSanPhuKhoaService, query);
                SelectedKhamSucKhoeTheLuc = await BaseServiceHelper.GetFirstOrDefaultAsync(KhamSucKhoeTheLucService, query);
                SelectedKhamSucKhoeTienSu = await BaseServiceHelper.GetFirstOrDefaultAsync(KhamSucKhoeTienSuService, query);

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
            }
            else
            {
                AlertService.ShowAlert("Không tìm thấy thông tin khám!", "danger");
            }
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
            return await LoadBlazorTypeaheadData(searchText, ContractService, "filter[_and][][active][_eq]=true");
        }

        private async Task<IEnumerable<KhamSucKhoeCongTyModel>> LoadKhamSucKhoeCongTyData(string searchText)
        {
            return await LoadBlazorTypeaheadData(searchText, KhamSucKhoeCongTyService, "filter[_and][][active][_eq]=true");
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
    }
}
