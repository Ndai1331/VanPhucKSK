using CoreAdminWeb.Helpers;
using CoreAdminWeb.Model;
using CoreAdminWeb.Model.User;
using CoreAdminWeb.Services;
using CoreAdminWeb.Services.BaseServices;
using CoreAdminWeb.Services.Users;
using CoreAdminWeb.Shared.Base;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CoreAdminWeb.Pages.Admins.HoSoKhamSucKhoeChiTiet
{
    public partial class Index(
        IBaseDetailService<SoKhamSucKhoeModel> MainService,
        IBaseDetailService<KhamSucKhoeKetLuanModel> KhamSucKhoeKetLuanService,
        IUserService UserService
    ) : BlazorCoreBase
    {
        [Parameter] public string? UserId { get; set; }
        private List<SoKhamSucKhoeModel> MainModels { get; set; } = new();
        private SoKhamSucKhoeModel SelectedItem { get; set; } = new SoKhamSucKhoeModel();
        private UserModel? CurrentUser { get; set; } = null;
        private DateTime? NgayKhamGanNhat = null;
        private string? MaLuotKhamGanNhat { get; set; }
        private KhamSucKhoeKetLuanModel? KetLuanGanNhat { get; set; }
        private string _searchMaKhachHang = "";
        private string _searchSoDienThoai = "";
        private string activeDefTab = "tab1";

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                // DateTime dateNow = DateTime.Now;
                // _startDateFilter = new DateTime(dateNow.Year, dateNow.Month, 1, 0, 0, 0, DateTimeKind.Local);
                // _endDateFilter = new DateTime(dateNow.Year, dateNow.Month, DateTime.DaysInMonth(dateNow.Year, dateNow.Month), 0, 0, 0, DateTimeKind.Local);

                // var resUser = await UserService.GetCurrentUserAsync();
                // if (resUser.IsSuccess)
                // {
                //     CurrentUser = resUser.Data;
                // }

                await LoadData();
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

            var res = await UserService.GetUserByIdAsync(Guid.Parse(UserId ?? ""));
            CurrentUser = res?.IsSuccess == true && res.Data != null
                        ? res.Data
                        : null;

            // if (result.IsSuccess)
            // {
            //     MainModels = result.Data ?? new List<SoKhamSucKhoeModel>();
            //     if (result.Meta != null)
            //     {
            //         TotalItems = result.Meta.filter_count ?? 0;
            //         TotalPages = (int)Math.Ceiling((double)TotalItems / PageSize);
            //     }
            // }
            // else
            // {
            //     MainModels = new List<SoKhamSucKhoeModel>();
            // }

            try
            {
                if (CurrentUser != null)
                {
                    // Load medical records and latest exam date in parallel
                    var medicalRecordsTask = LoadMedicalRecordsAsync();
                    var latestExamTask = LoadLatestExamDateAsync();
                    await Task.WhenAll(medicalRecordsTask, latestExamTask);

                    // Only load conclusion if we have the latest exam code
                    if (!string.IsNullOrEmpty(MaLuotKhamGanNhat))
                    {
                        await LoadKetLuanAsync();
                    }
                } else {
                    AlertService.ShowAlert($"Không tìm thấy bệnh nhân theo thông tin tìm kiếm!", "warning");
                }
            }
            catch (Exception ex)
            {
                AlertService.ShowAlert($"Lỗi khi tải dữ liệu hồ sơ khám sức khỏe: {ex.Message}", "danger");
            }

            IsLoading = false;
        }

        private async Task LoadMedicalRecordsAsync()
        {
            if (CurrentUser?.ma_benh_nhan == null) return;
            
            try
            {
                // Build query for paginated results
                BuildPaginationQuery(Page, PageSize, "ngay_kham");
                BuilderQuery += $"&filter[_and][0][deleted][_eq]=false";
                BuilderQuery += $"&filter[_and][1][ma_benh_nhan][_eq]={CurrentUser.ma_benh_nhan}";
                
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
            }
            catch (Exception ex)
            {
                AlertService.ShowAlert("Lỗi khi tải hồ sơ khám bệnh: " + ex.Message, "danger");
                MainModels = new List<SoKhamSucKhoeModel>();
            }
        }

        /// <summary>
        /// Load latest exam date separately (not affected by pagination)
        /// </summary>
        private async Task LoadLatestExamDateAsync()
        {
            if (CurrentUser?.ma_benh_nhan == null) return;
            
            try
            {
                // Separate query to get only the latest exam date
                string query = $"limit=1&offset=0&sort=-ngay_kham";
                query += $"&filter[_and][0][deleted][_eq]=false";
                query += $"&filter[_and][1][ma_benh_nhan][_eq]={CurrentUser.ma_benh_nhan}";
                query += $"&fields=ngay_kham,ma_luot_kham"; // Only get the date field for performance
                
                var result = await MainService.GetAllAsync(query);
                if (result.IsSuccess && result.Data?.Any() == true)
                {
                    NgayKhamGanNhat = result.Data.FirstOrDefault()?.ngay_kham;
                    MaLuotKhamGanNhat = result.Data.FirstOrDefault()?.ma_luot_kham;
                }
                else
                {
                    NgayKhamGanNhat = null;
                    MaLuotKhamGanNhat = null;
                }
            }
            catch (Exception ex)
            {
                AlertService.ShowAlert("Lỗi khi tải ngày khám gần nhất: " + ex.Message, "danger");
                NgayKhamGanNhat = null;
                MaLuotKhamGanNhat = null;
            }
        }

        /// <summary>
        /// Load medical records for pagination (wrapper method)
        /// </summary>
        private async Task LoadMedicalRecords()
        {
            await LoadMedicalRecordsAsync();
            await InvokeAsync(StateHasChanged);
        }

        private string GetStatusBadgeClass(string status)
        {
            return status switch
            {
                "published" => "bg-success text-white",
                "draft" => "bg-warning text-white",
                "deleted" => "bg-danger text-white",
                _ => "bg-gray-500 text-white"
            };
        }

        private void ViewRecordDetail(SoKhamSucKhoeModel record)
        {
            // Navigation.NavigateTo($"/record-detail-page/{record.ma_luot_kham}");
        }
        private void ViewSubClinicalDetail(SoKhamSucKhoeModel record)
        {
            // Navigation.NavigateTo($"/subclinical-result-detail-page/{record.ma_luot_kham}");
        }

        private async Task LoadKetLuanAsync()
        {
            if (CurrentUser?.ma_benh_nhan == null) return;
            
            try
            {
                // Separate query to get only the latest exam date
                string query = $"limit=1&offset=0&sort=-id";
                query += $"&filter[_and][0][ma_luot_kham][_eq]={MaLuotKhamGanNhat}";
                query += $"&fields=phan_loai_suc_khoe,ma_luot_kham"; // Only get the date field for performance
                
                var result = await KhamSucKhoeKetLuanService.GetAllAsync(query);
                if (result.IsSuccess && result.Data?.Any() == true)
                {
                    KetLuanGanNhat = result.Data.FirstOrDefault();
                }
                else
                {
                    KetLuanGanNhat = null;
                }
            }
            catch (Exception ex)
            {
                AlertService.ShowAlert("Lỗi khi tải kết luận gần nhất: " + ex.Message, "danger");
                KetLuanGanNhat = null;
            }
        }
    }
}
