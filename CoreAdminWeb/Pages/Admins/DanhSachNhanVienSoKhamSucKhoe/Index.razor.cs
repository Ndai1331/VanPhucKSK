using CoreAdminWeb.Helpers;
using CoreAdminWeb.Model;
using CoreAdminWeb.Services;
using CoreAdminWeb.Services.BaseServices;
using CoreAdminWeb.Services.IDanhSachDoanSoKhamSucKhoeService;
using CoreAdminWeb.Shared.Base;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using CoreAdminWeb.Extensions;
using CoreAdminWeb.Model.KhamSucKhoes;

namespace CoreAdminWeb.Pages.Admins.DanhSachNhanVienSoKhamSucKhoe
{
    public partial class Index(
        IDanhSachDoanSoKhamSucKhoeService<DanhSachDoanSoKhamSucKhoeModel> MainService,
        IBaseService<CongTyModel> CongTyService,
        IBaseService<KhamSucKhoeCongTyModel> KhamSucKhoeCongTyService,
        IExportExcelService<dynamic> ExportExcelService
    ) : BlazorCoreBase
    {
        [Parameter] public int? Id { get; set; }
        private List<DanhSachDoanSoKhamSucKhoeModel> MainModels { get; set; } = new();
        private DateTime? _fromDate = null;
        private DateTime? _toDate = null;
        private CongTyModel? _selectedCongTyFilter = null;
        private KhamSucKhoeCongTyModel? _selectedKhamSucKhoeCongTyFilter = null;

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            // Load the KhamSucKhoeCongTy by ID if provided
            // if (Id.HasValue)
            // {
            //     await LoadKhamSucKhoeCongTyById(Id.Value);
            // }
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
            BuilderQuery = $"DanhSachDoan/medical-data?limit={PageSize}&offset={(Page - 1) * PageSize}";
            if (_fromDate.HasValue)
            {
                BuilderQuery += $"&fromDate={_fromDate.Value.ToString("yyyy-MM-dd")}";
            }
            if (_toDate.HasValue)
            {
                BuilderQuery += $"&toDate={_toDate.Value.ToString("yyyy-MM-dd")}";
            }
            if (_selectedCongTyFilter != null)
            {
                BuilderQuery += $"&congTy={_selectedCongTyFilter.id.ToString()}";
            }
            if (_selectedKhamSucKhoeCongTyFilter != null)
            {
                BuilderQuery += $"&maDotKham={_selectedKhamSucKhoeCongTyFilter.id}";
            }

            var result = await MainService.GetAllAsync(BuilderQuery);
            if (result.IsSuccess)
            {
                MainModels = result.Data ?? new List<DanhSachDoanSoKhamSucKhoeModel>();
                if (result.Meta != null)
                {
                    TotalItems = result.Meta.filter_count ?? 0;
                    TotalPages = result.Meta.page_count ?? 0;
                }
            }
            else
            {
                MainModels = new List<DanhSachDoanSoKhamSucKhoeModel>();
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

            await LoadData();
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

        private async Task LoadKhamSucKhoeCongTyById(int id)
        {
            try
            {
                var result = await KhamSucKhoeCongTyService.GetByIdAsync(id.ToString());
                if (result?.IsSuccess == true && result.Data != null)
                {
                    _selectedKhamSucKhoeCongTyFilter = result.Data;
                }
            }
            catch (Exception ex)
            {
                AlertService.ShowAlert($"Lỗi khi tải dữ liệu: {ex.Message}", "danger");
            }
        }

        private Task GoToDetail(string? userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                AlertService.ShowAlert($"Không tìm thấy thông tin người dùng!", "danger");

            }
            else
            {
                NavigationManager?.NavigateTo($"/admin/ho-so-suc-khoe-chi-tiet/{userId}");
            }
            return Task.CompletedTask;

        }
        
        private async Task OnExcelExport()
        {
            try
            {
                IsLoading = true;

                BuilderQuery = $"DanhSachDoan/medical-data?limit={int.MaxValue}&offset={0}";
                if (_fromDate.HasValue)
                {
                    BuilderQuery += $"&fromDate={_fromDate.Value.ToString("yyyy-MM-dd")}";
                }
                if (_toDate.HasValue)
                {
                    BuilderQuery += $"&toDate={_toDate.Value.ToString("yyyy-MM-dd")}";
                }
                if (_selectedCongTyFilter != null)
                {
                    BuilderQuery += $"&congTy={_selectedCongTyFilter.id.ToString()}";
                }
                if (_selectedKhamSucKhoeCongTyFilter != null)
                {
                    BuilderQuery += $"&maDotKham={_selectedKhamSucKhoeCongTyFilter.id}";
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
                        }
                    ).ToList() ?? new List<dynamic>();

                    var fileBytes = await ExportExcelService.ExportToExcelAsync(prepareData, fields, labels);

                    var fileName = $"{"DanhSacNhanVienhHoSoKhamSucKhoe"}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
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
                IsLoading = false;
            }
        }
    }
}
