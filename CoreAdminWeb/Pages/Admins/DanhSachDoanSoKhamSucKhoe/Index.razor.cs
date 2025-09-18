using CoreAdminWeb.Enums;
using CoreAdminWeb.Extensions;
using CoreAdminWeb.Helpers;
using CoreAdminWeb.Model;
using CoreAdminWeb.Model.KhamSucKhoes;
using CoreAdminWeb.Services;
using CoreAdminWeb.Services.BaseServices;
using CoreAdminWeb.Services.Exports;
using CoreAdminWeb.Services.IDanhSachDoanSoKhamSucKhoeService;
using CoreAdminWeb.Services.Imports;
using CoreAdminWeb.Shared.Base;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;

namespace CoreAdminWeb.Pages.Admins.DanhSachDoanSoKhamSucKhoe
{
    public partial class Index(
        IDanhSachDoanSoKhamSucKhoeService<DanhSachDoanSoKhamSucKhoeModel> MainService,
        IBaseService<CongTyModel> CongTyService,
        IBaseService<KhamSucKhoeCongTyModel> KhamSucKhoeCongTyService,
        IExportExcelService<dynamic> ExportExcelService,
        IConfiguration Configuration,
        NavigationManager NavManager,
        ExportKSKDataService ExportKSKDataService,
        ImportKetQuaKhamSucKhoeService importKetQuaKhamSucKhoeService
    ) : BlazorCoreBase
    {
        [Parameter] public int? Id { get; set; }
        private List<DanhSachDoanSoKhamSucKhoeModel> MainModels { get; set; } = new();
        private DateTime? _fromDate = null;
        private DateTime? _toDate = null;
        private CongTyModel? _selectedCongTyFilter = null;
        private KhamSucKhoeCongTyModel? _selectedKhamSucKhoeCongTyFilter = null;
        private string? _searchMaDieuTriString = null;
        private int? _searchFromNumber = null;
        private int? _searchToNumber = null;

        private string? connectionExportId = "";
        private string? exportProcessingMessage { get; set; }
        public bool isExportDone { get; set; } = false;
        public bool isErrorPopup { get; set; } = false;
        public bool isExportError { get; set; } = false;
        public bool isDisabledExport { get; set; } = false;

        private bool IsShowExportModal { get; set; } = false;
        private HoSoKhamSucKhoeExportType exportType { get; set; } = HoSoKhamSucKhoeExportType.CheckListKsk;
        private List<DanhSachDoanSoKhamSucKhoeModel> selectedSoKhamSucKhoes { get; set; } = new();
        private bool isSelectAllChecked { get; set; } = false; // Track Select All state separately
        private bool IsAllRowsSelected => isSelectAllChecked;

        private const long MaxExcelFileSize = 25 * 1024 * 1024; // 25MB, adjust as needed

        private string? connectionImportId = "";
        private string? importProcessingMessage { get; set; }
        public bool isImportDone { get; set; }
        public bool isImportError { get; set; }
        public bool isDisabledImport { get; set; } = false;

        public bool onReadonly { get; set; } = false;


        private bool onShowViewContent { get; set; } = false;
        private string viewContentTitle { get; set; } = "";
        private string viewContentContent { get; set; } = "";

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                connectionImportId = $"{UserId}_import";
                connectionExportId = $"{UserId}_export";

                // Load the KhamSucKhoeCongTy by ID if provided
                if (Id.HasValue)
                {
                    await LoadKhamSucKhoeCongTyById(Id.Value);
                }
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

        private async Task LoadData(bool isReset = false)
        {
            Loading.Show();

            if (isReset)
            {
                ResetPage();
                await Task.Delay(100);
            }

            BuilderQuery = $"DanhSachDoan/medical-data?limit={PageSize}&offset={(Page - 1) * PageSize}";

            if (_fromDate.HasValue)
            {
                BuilderQuery += $"&fromDate={_fromDate.Value:yyyy-MM-dd}";
            }
            if (_toDate.HasValue)
            {
                BuilderQuery += $"&toDate={_toDate.Value:yyyy-MM-dd}";
            }
            if (_selectedCongTyFilter != null)
            {
                BuilderQuery += $"&congTy={_selectedCongTyFilter.id}";
            }
            if (_selectedKhamSucKhoeCongTyFilter != null)
            {
                BuilderQuery += $"&maDotKham={_selectedKhamSucKhoeCongTyFilter.id}";
            }
            if (!string.IsNullOrEmpty(_searchMaDieuTriString))
            {
                BuilderQuery += $"&maDieuTri={_searchMaDieuTriString}";
            }
            if (_searchFromNumber.HasValue)
            {
                BuilderQuery += $"&fromNumber={_searchFromNumber}";
            }
            if (_searchToNumber.HasValue)
            {
                BuilderQuery += $"&toNumber={_searchToNumber}";
            }

            var result = await MainService.GetAllAsync(BuilderQuery);
            if (result.IsSuccess)
            {
                selectedSoKhamSucKhoes = new List<DanhSachDoanSoKhamSucKhoeModel>();
                MainModels = result.Data ?? new List<DanhSachDoanSoKhamSucKhoeModel>();
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
                MainModels = new List<DanhSachDoanSoKhamSucKhoeModel>();
            }

            onReadonly = MainModels.Any(c => c.trang_thai_hop_dong == TrangThaiHopDong.locked);

            ResetSelectionState();

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

        private async Task<IEnumerable<CongTyModel>> LoaCongTyFilterData(string searchText)
        {
            return await LoadBlazorTypeaheadData(searchText, CongTyService);
        }

        private async Task<IEnumerable<KhamSucKhoeCongTyModel>> LoadKhamSucKhoeCongTyData(string searchText)
        {
            return await LoadBlazorTypeaheadData(searchText, KhamSucKhoeCongTyService);
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

            await LoadData(true);
        }

        private async Task OnKhamSucKhoeCongTyFilterChanged(KhamSucKhoeCongTyModel? selected)
        {
            if (selected != null)
            {
                _selectedKhamSucKhoeCongTyFilter = selected;
            }
            else
            {
                _selectedKhamSucKhoeCongTyFilter = null;
            }

            await LoadData(true);
        }

        private async Task OnValueChanged(ChangeEventArgs e, string fieldName, bool isFilter = false, bool isDate = true)
        {
            try
            {
                if (!isDate)
                {
                    if (e.Value == null || string.IsNullOrEmpty(e.Value.ToString()))
                    {
                        ReflectionHelper.SetFieldValue(this, fieldName, null);
                    }
                    else
                    {
                        var value = e.Value.ToString();
                        ReflectionHelper.SetFieldValue(this, fieldName, value);
                    }
                }
                else
                {
                    var dateStr = e.Value?.ToString();
                    if (string.IsNullOrEmpty(dateStr))
                    {
                        ReflectionHelper.SetFieldValue(this, fieldName, null);
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
                            ReflectionHelper.SetFieldValue(this, fieldName, date);
                        }
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

        private async Task LoadKhamSucKhoeCongTyById(int id)
        {
            Loading.Show();
            try
            {
                var result = await KhamSucKhoeCongTyService.GetByIdAsync(id.ToString());
                if (result?.IsSuccess == true && result.Data != null)
                {
                    _selectedKhamSucKhoeCongTyFilter = result.Data;
                    await LoadData();
                }
            }
            catch (Exception ex)
            {
                AlertService.ShowAlert($"Lỗi khi tải dữ liệu: {ex.Message}", "danger");
            }
            finally
            {
                Loading.Hide();
            }
        }

        private async Task OnExcelExport()
        {
            try
            {
                Loading.Show();

                BuilderQuery = $"DanhSachDoan/medical-data?limit={int.MaxValue}&offset={0}";

                if (_fromDate.HasValue)
                {
                    BuilderQuery += $"&fromDate={_fromDate.Value:yyyy-MM-dd}";
                }
                if (_toDate.HasValue)
                {
                    BuilderQuery += $"&toDate={_toDate.Value:yyyy-MM-dd}";
                }
                if (_selectedCongTyFilter != null)
                {
                    BuilderQuery += $"&congTy={_selectedCongTyFilter.id}";
                }
                if (_selectedKhamSucKhoeCongTyFilter != null)
                {
                    BuilderQuery += $"&maDotKham={_selectedKhamSucKhoeCongTyFilter.id}";
                }
                else
                {
                    BuilderQuery += $"&maDotKham={Id}";
                }

                var fields = new List<string>()
                {
                    "stt",
                    "ma_luot_kham",
                    "full_name",
                    "ngay_sinh",
                    "gioi_tinh",
                    "tien_su_gia_dinh",
                    "ten_benh",
                    "chieu_cao",
                    "can_nang",
                    "bmi",
                    "mach",
                    "huyet_ap",
                    "kq_nk_tuan_hoan",
                    "kq_nk_ho_hap",
                    "kq_nk_tieu_hoa",
                    "kq_nk_than_tiet_nieu",
                    "kq_nk_noi_tiet",
                    "kq_nk_co_xuong_khop",
                    "kq_nk_than_kinh",
                    "kq_nk_tam_than",
                    "kq_ngoai_khoa",
                    "ket_qua_san_phu_khoa",
                    "benh_mat",
                    "benh_tai_mui_hong",
                    "benh_rhm",
                    "kq_da_lieu",
                    "can_lam_sang_results",
                    "phan_loai_suc_khoe",
                    "benh_tat_ket_luan",
                    "de_nghi",
                    "ngay_ket_luan",
                };
                var labels = new List<string>()
                {
                    "STT",
                    "Mã đoàn",
                    "Họ và tên",
                    "Năm sinh",
                    "Giới tính",
                    "Tiền sử gia đình",
                    "Tiền sử bản thân",
                    "Chiều cao (cm)",
                    "Cân nặng (kg)",
                    "BMI",
                    "Mạch (lần/phút)",
                    "Huyết áp (mmHg)",
                    "Tuần hoàn",
                    "Hô hấp",
                    "Tiêu hóa",
                    "Thận-Tiết niệu",
                    "Nội tiết",
                    "Cơ-xương-khớp",
                    "Thần kinh",
                    "Tâm thần",
                    "Ngoại khoa",
                    "Sản phụ khoa",
                    "Mắt",
                    "Tai mũi họng",
                    "RHM",
                    "Đa liễu",
                    "Cận lâm sàng",
                    "Phân loại sức khỏe",
                    "Các bệnh tật",
                    "Đề nghị",
                    "Ngày kết luận",
                };

                var result = await MainService.GetAllAsync(BuilderQuery);
                if (result.IsSuccess)
                {
                    var prepareData = result.Data?.Select(item =>
                        (dynamic)new
                        {
                            stt = result.Data.IndexOf(item) + 1,
                            ma_luot_kham = item.ma_luot_kham,
                            full_name = $"{item.last_name} {item.first_name}",
                            ngay_sinh = item.ngay_sinh?.ToString("dd/MM/yyyy"),
                            gioi_tinh = item.gioi_tinh?.GetDescription(),
                            tien_su_gia_dinh = item.tien_su_gia_dinh,
                            ten_benh = item.ten_benh,
                            chieu_cao = item.chieu_cao,
                            can_nang = item.can_nang,
                            bmi = item.bmi,
                            mach = item.mach,
                            huyet_ap = item.huyet_ap,
                            kq_nk_tuan_hoan = item.kq_nk_tuan_hoan,
                            kq_nk_ho_hap = item.kq_nk_ho_hap,
                            kq_nk_tieu_hoa = item.kq_nk_tieu_hoa,
                            kq_nk_than_tiet_nieu = item.kq_nk_than_tiet_nieu,
                            kq_nk_noi_tiet = item.kq_nk_noi_tiet,
                            kq_nk_co_xuong_khop = item.kq_nk_co_xuong_khop,
                            kq_nk_than_kinh = item.kq_nk_than_kinh,
                            kq_nk_tam_than = item.kq_nk_tam_than,
                            kq_ngoai_khoa = item.kq_ngoai_khoa,
                            ket_qua_san_phu_khoa = item.ket_qua_san_phu_khoa,
                            benh_mat = item.benh_mat,
                            benh_tai_mui_hong = item.benh_tai_mui_hong,
                            benh_rhm = item.benh_rhm,
                            kq_da_lieu = item.kq_da_lieu,
                            can_lam_sang_results = item.can_lam_sang_results,
                            phan_loai_suc_khoe = item.phan_loai_suc_khoe,
                            benh_tat_ket_luan = item.benh_tat_ket_luan,
                            de_nghi = item.de_nghi,
                            ngay_ket_luan = item.ngay_ket_luan,
                        }
                    ).ToList() ?? new List<dynamic>();

                    var fileBytes = await ExportExcelService.ExportToExcelAsync(prepareData, fields, labels);

                    var fileName = $"{"DanhSachDoanHoSoKhamSucKhoe"}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                    await JsRuntime.InvokeVoidAsync("saveAsFile", fileName, Convert.ToBase64String(fileBytes));
                }
                else
                {
                    AlertService.ShowAlert(result.Message ?? "Lỗi khi lấy dữ liệu để xuất excel", "danger");
                }
            }
            catch
            {
                AlertService.ShowAlert("Lỗi khi xuất excel", "danger");
            }
            finally
            {
                Loading.Hide();
            }
        }

        private void ToggleRowSelection(DanhSachDoanSoKhamSucKhoeModel item, bool isSelected)
        {
            if (isSelected)
            {
                if (!selectedSoKhamSucKhoes.Any(x => x.id == item.id))
                {
                    selectedSoKhamSucKhoes.Add(item);
                }
            }
            else
            {
                selectedSoKhamSucKhoes.RemoveAll(x => x.id == item.id);
            }

            UpdateSelectAllState();
        }

        private void UpdateSelectAllState()
        {
            isSelectAllChecked = MainModels != null && MainModels.Count > 0 &&
                                selectedSoKhamSucKhoes.Count == MainModels.Count &&
                                MainModels.All(item => selectedSoKhamSucKhoes.Any(selected => selected.id == item.id));
        }

        private void ResetSelectionState()
        {
            selectedSoKhamSucKhoes.Clear();
            isSelectAllChecked = false;
        }

        private void ToggleSelectAllRows(ChangeEventArgs e)
        {
            var isChecked = e.Value is bool b && b;
            if (isChecked)
            {
                selectedSoKhamSucKhoes = MainModels.ToList();
                isSelectAllChecked = true;
            }
            else
            {
                selectedSoKhamSucKhoes.Clear();
                isSelectAllChecked = false;
            }
        }

        private void CloseExportModal()
        {
            IsShowExportModal = false;
        }

        private void OnShowExport()
        {
            if (selectedSoKhamSucKhoes == null || !selectedSoKhamSucKhoes.Any())
            {
                AlertService.ShowAlert("Không có sổ khám sức khỏe nào được chọn", "warning");
                return;
            }

            IsShowExportModal = true;
        }
        private async Task ExportFileSubmit()
        {
            List<int> ids = selectedSoKhamSucKhoes.Where(c => c.id.HasValue).Select(c => c.id ?? 0).Distinct().ToList();

            bool isAllRowsActuallySelected = MainModels != null && MainModels.Count > 0 &&
                                           selectedSoKhamSucKhoes.Count == MainModels.Count &&
                                           MainModels.All(item => selectedSoKhamSucKhoes.Any(selected => selected.id == item.id));

            if (isAllRowsActuallySelected)
            {
                BuilderQuery = $"DanhSachDoan/medical-data?limit={int.MaxValue}&offset=0";

                if (_fromDate.HasValue)
                {
                    BuilderQuery += $"&fromDate={_fromDate.Value:yyyy-MM-dd}";
                }
                if (_toDate.HasValue)
                {
                    BuilderQuery += $"&toDate={_toDate.Value:yyyy-MM-dd}";
                }
                if (_selectedCongTyFilter != null)
                {
                    BuilderQuery += $"&congTy={_selectedCongTyFilter.id}";
                }
                if (_selectedKhamSucKhoeCongTyFilter != null)
                {
                    BuilderQuery += $"&maDotKham={_selectedKhamSucKhoeCongTyFilter.id}";
                }

                var result = await MainService.GetAllAsync(BuilderQuery);
                if (result.IsSuccess)
                {
                    ids = (result.Data ?? new List<DanhSachDoanSoKhamSucKhoeModel>()).Where(c => c.id.HasValue).Select(c => c.id ?? 0).Distinct().ToList();
                }
            }

            _ = Task.Run(() => ExportKSKDataService.ExportFromExaminationWithProgressAsync(
                connectionExportId ?? string.Empty,
                ids,
                CurrentSetting,
                exportType,
                Configuration["DrCoreApi:BaseUrl"] ?? string.Empty,
                OnExportProcessing,
                CancellationToken.None)
            );
        }

        private async Task OnExportProcessing(ProcessingModel progress)
        {
            if (!progress.ProcessId.Equals(connectionExportId))
            {
                return;
            }

            switch (progress.Status)
            {
                case TrangThaiXuLyNen.Processing:
                    isDisabledExport = true;
                    isExportError = false;
                    isExportDone = false;
                    exportProcessingMessage = progress.Value?.ToString() ?? "";
                    await InvokeAsync(StateHasChanged);
                    break;

                case TrangThaiXuLyNen.Completed:
                    isDisabledExport = false;
                    isExportDone = true;
                    exportProcessingMessage = progress.Value?.ToString() ?? "";
                    if (progress.AdditionalParams != null && HasProperty(progress.AdditionalParams, "RelativeUrl"))
                    {
                        var url = progress.AdditionalParams?.RelativeUrl;
                        if (!string.IsNullOrWhiteSpace(url))
                        {
                            NavManager.NavigateTo(url, forceLoad: true);
                        }
                    }
                    await InvokeAsync(StateHasChanged);
                    break;

                case TrangThaiXuLyNen.Error:
                    isDisabledExport = false;
                    isExportError = true;
                    isExportDone = false;
                    exportProcessingMessage = progress.Value?.ToString() ?? "";
                    await InvokeAsync(StateHasChanged);
                    break;
                default:
                    break;
            }
        }

        static bool HasProperty(dynamic? obj, string propertyName)
        {
            if (obj == null || string.IsNullOrEmpty(propertyName))
            {
                return false;
            }

            if (obj is IDictionary<string, object> dict)
            {
                return dict.ContainsKey(propertyName);
            }

            return obj?.GetType().GetProperty(propertyName) != null;
        }


        private async Task OnExcelFileSelected(InputFileChangeEventArgs e)
        {
            try
            {
                var file = e.File;
                if (file == null)
                {
                    AlertService.ShowAlert("Vui lòng chọn file excel!", "warning");
                    return;
                }
                if (
                    file.ContentType != "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                    && file.ContentType != "application/vnd.ms-excel")
                {
                    AlertService.ShowAlert("Vui lòng chọn file Excel hợp lệ!", "warning");
                    return;
                }

                // Additional check to ensure file.Size is not greater than MaxExcelFileSize
                if (file.Size <= 0 || file.Size > MaxExcelFileSize)
                {
                    AlertService.ShowAlert("Kích thước file không hợp lệ hoặc vượt quá giới hạn cho phép!", "warning");
                    return;
                }

                using var stream = file.OpenReadStream(file.Size);
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                var fileBytes = ms.ToArray();

                // Gọi hàm import excel từ service
                _ = Task.Run(() => importKetQuaKhamSucKhoeService.ImportFromExcelWithProgressAsync(
                    fileBytes,
                    connectionImportId ?? string.Empty,
                    UserId,
                    OnImportProcessing,
                    CancellationToken.None)
                );
            }
            catch (Exception ex)
            {
                AlertService.ShowAlert($"Lỗi khi import file: {ex.Message}", "danger");
                await LoadData(true);
            }
        }

        private async Task OnImportProcessing(ProcessingModel progress)
        {
            if (!progress.ProcessId.Equals(connectionImportId))
            {
                return;
            }

            switch (progress.Status)
            {
                case Enums.TrangThaiXuLyNen.Processing:
                    isDisabledImport = true;
                    isImportError = false;
                    isImportDone = false;
                    importProcessingMessage = progress.Value?.ToString() ?? "";
                    await InvokeAsync(StateHasChanged);
                    break;

                case Enums.TrangThaiXuLyNen.Completed:
                    isDisabledImport = false;
                    isImportDone = true;
                    isImportError = false;
                    importProcessingMessage = progress.Value?.ToString() ?? "";

                    await LoadData();
                    await InvokeAsync(StateHasChanged);
                    break;

                case Enums.TrangThaiXuLyNen.Error:
                    isDisabledImport = false;
                    var isPopup = HasProperty(progress.AdditionalParams, "IsPopup") && progress.AdditionalParams?.IsPopup ?? false;

                    isImportError = true;
                    isImportDone = isPopup;
                    isErrorPopup = isPopup;
                    importProcessingMessage = progress.Value?.ToString() ?? "";
                    await InvokeAsync(StateHasChanged);
                    break;
                default:
                    break;
            }
        }

        private async Task OpenFileDialog()
        {
            // Clear the file input before processing
            await JsRuntime.InvokeVoidAsync("eval", "document.getElementById('excelFileInput').value = ''");
            await JsRuntime.InvokeVoidAsync("eval", "document.getElementById('excelFileInput').click()");
        }

        private void showDetail(string title, string? content)
        {
            onShowViewContent = true;
            viewContentTitle = title;
            viewContentContent = content ?? string.Empty;
        }

        private void closeDetail()
        {
            onShowViewContent = false;
        }
    }
}
