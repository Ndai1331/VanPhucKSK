using CoreAdminWeb.Helpers;
using CoreAdminWeb.Model;
using CoreAdminWeb.Model.DanhSachDoan;
using CoreAdminWeb.Services.BaseServices;
// using CoreAdminWeb.Services.DanhSachDoan;
using CoreAdminWeb.Shared.Base;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using CoreAdminWeb.Services.Files;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using CoreAdminWeb.Commons;

namespace CoreAdminWeb.Pages.Admins.DanhSachDoan
{
    public partial class Index(
        // IBaseService<DanhSachDoanModel> MainService,
        // IDanhSachDoanChiTietService DanhSachDoanChiTietService,
        IBaseService<CongTyModel> CongTyService,
        IBaseService<KhamSucKhoeCongTyModel> KhamSucKhoeCongTyService
        // IBaseService<DanhSachDoanTypeModel> DanhSachDoanTypeService,
        // IBaseService<DinhMucModel> DinhMucService,
        // IFileService _fileService
    ) : BlazorCoreBase
    {
        private List<DanhSachDoanModel> MainModels { get; set; } = new();
        private bool openDeleteModal = false;
        private bool openAddOrUpdateModal = false;
        private bool openDetailDeleteModal = false;
        private DanhSachDoanModel SelectedItem { get; set; } = new DanhSachDoanModel();
        private List<DanhSachDoanChiTietModel> SelectedItemsDetail { get; set; } = new List<DanhSachDoanChiTietModel>();
        private DanhSachDoanChiTietModel? SelectedItemDetail { get; set; } = default;
        private string _searchString = "";
        private string _searchStatusString = "";
        private string _titleAddOrUpdate = "Thêm mới";
        private string activeDefTab { get; set; } = "tab1";
        private IBrowserFile SelectedFile { get; set; } = default!;
        private string fileContent { get; set; } = string.Empty;
        private FileCRUDModel UploadFileCRUD { get; set; } = new FileCRUDModel();


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
            // IsLoading = true;

            // BuildPaginationQuery(Page, PageSize);
            // BuilderQuery += $"&filter[_and][0][deleted][_eq]=false";
            // if (!string.IsNullOrEmpty(_searchString))
            // {
            //     BuilderQuery += $"&filter[_and][1][_or][0][code][_contains]={_searchString}";
            //     BuilderQuery += $"&filter[_and][1][_or][1][name][_contains]={_searchString}";
            // }
            // if (!string.IsNullOrEmpty(_searchStatusString))
            // {
            //     BuilderQuery += $"&filter[_and][2][status][_eq]={_searchStatusString}";
            // }
            // var result = await MainService.GetAllAsync(BuilderQuery);
            // if (result.IsSuccess)
            // {
            //     MainModels = result.Data ?? new List<DanhSachDoanModel>();
            //     if (result.Meta != null)
            //     {
            //         TotalItems = result.Meta.filter_count ?? 0;
            //         TotalPages = (int)Math.Ceiling((double)TotalItems / PageSize);
            //     }
            // }
            // else
            // {
                MainModels = new List<DanhSachDoanModel>();
            // }
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

        private async Task LoadDetailData()
        {
            var buildQuery = $"sort=sort";
            buildQuery += $"&filter[_and][][danh_sach_doan][_eq]={SelectedItem.id}";
            buildQuery += $"&filter[_and][][deleted][_eq]=false";
            // var result = await DanhSachDoanChiTietService.GetAllAsync(buildQuery);
            // SelectedItemsDetail = result.Data ?? new List<DanhSachDoanChiTietModel>();
            SelectedItemsDetail = new List<DanhSachDoanChiTietModel>();
        }

        private async Task<IEnumerable<CongTyModel>> LoadCongTyData(string searchText)
        {
            return await LoadBlazorTypeaheadData(searchText, CongTyService);
        }

        private async Task<IEnumerable<KhamSucKhoeCongTyModel>> LoadKhamSucKhoeCongTyData(string searchText)
        {
            return await LoadBlazorTypeaheadData(searchText, KhamSucKhoeCongTyService);
        }

        // private async Task<IEnumerable<DanhSachDoanTypeModel>> LoadDanhSachDoanTypeData(string searchText)
        // {
        //     return await LoadBlazorTypeaheadData(searchText, DanhSachDoanTypeService);
        // }

        // private async Task<IEnumerable<DinhMucModel>> LoadDinhMucData(string searchText)
        // {
        //     return await LoadBlazorTypeaheadData(searchText, DinhMucService);
        // }

        private void OpenDeleteModal(DanhSachDoanModel item)
        {
            SelectedItem = item;
            openDeleteModal = true;
        }

        private async Task OnDelete()
        {
            // var result = await MainService.DeleteAsync(SelectedItem);
            // if (result.IsSuccess && result.Data)
            // {
            //     await LoadData();
            //     AlertService.ShowAlert("Xoá thành công!", "success");
            //     openDeleteModal = false;
            // }
            // else
            // {
            //     AlertService.ShowAlert(result.Message ?? "Lỗi khi xóa dữ liệu", "danger");
            // }
        }

        private void CloseDeleteModal()
        {
            SelectedItem = new DanhSachDoanModel();
            openDeleteModal = false;
        }

        private void OpenDetailDeleteModal(DanhSachDoanChiTietModel item)
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

            if (!SelectedItemsDetail.Any(c => c.deleted == null || c.deleted == false))
            {
                SelectedItemsDetail.Add(new DanhSachDoanChiTietModel()
                {
                    danh_sach_doan = SelectedItem,
                    sort = (SelectedItemsDetail.Max(c => c.sort) ?? 0) + 1,
                    code = string.Empty,
                    name = string.Empty
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
                SelectedItemsDetail = new List<DanhSachDoanChiTietModel>();
            }

            SelectedItemsDetail.Add(new DanhSachDoanChiTietModel
            {
                danh_sach_doan = SelectedItem,
                sort = (SelectedItemsDetail.Max(c => c.sort) ?? 0) + 1,
                code = string.Empty,
                name = string.Empty
            });
        }

        private async Task OpenAddOrUpdateModal(DanhSachDoanModel? item)
        {
            _titleAddOrUpdate = item != null ? "Sửa" : "Thêm mới";
            SelectedItem = item != null ? item.DeepClone() : new DanhSachDoanModel();

            SelectedItemsDetail = new List<DanhSachDoanChiTietModel>();
            activeDefTab = "tab1";
            // ClearImageUpload();

            if (SelectedItem.id > 0)
            {
                await LoadDetailData();
            }

            if (!SelectedItemsDetail.Any())
            {
                SelectedItemsDetail.Add(new DanhSachDoanChiTietModel()
                {
                    danh_sach_doan = SelectedItem,
                    sort = (SelectedItemsDetail.Max(c => c.sort) ?? 0) + 1,
                    code = string.Empty,
                    name = string.Empty
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
            // if (!FormValidation())
            // {
            //     return;
            // }
            // if (SelectedItem.id == 0)
            // {
            //     var result = await MainService.CreateAsync(SelectedItem);
            //     if (result.IsSuccess)
            //     {
            //         var chiTietList = SelectedItemsDetail
            //             .Where(c => c.deleted == false || c.deleted == null)
            //             .Select(c =>
            //             {
            //                 c.danh_sach_doan = result.Data;
            //                 return c;
            //             })
            //             .ToList();

            //         var detailResult = await DanhSachDoanChiTietService.CreateAsync(chiTietList);
            //         if (!detailResult.IsSuccess)
            //         {
            //             AlertService.ShowAlert(detailResult.Message ?? "Lỗi khi thêm mới chi tiết dữ liệu", "danger");
            //             return;
            //         }
            //         await LoadData();
            //         openAddOrUpdateModal = false;
            //         AlertService.ShowAlert("Thêm mới thành công!", "success");
            //     }
            //     else
            //     {
            //         AlertService.ShowAlert(result.Message ?? "Lỗi khi thêm mới dữ liệu", "danger");
            //     }
            // }
            // else
            // {
            //     var result = await MainService.UpdateAsync(SelectedItem);
            //     if (result.IsSuccess)
            //     {
            //         var addNewChiTietList = SelectedItemsDetail
            //             .Where(c => (c.deleted == false || c.deleted == null) && c.id == 0)
            //             .Select(c =>
            //             {
            //                 c.danh_sach_doan = SelectedItem;
            //                 return c;
            //             }).ToList();
            //         var removeChiTietList = SelectedItemsDetail
            //             .Where(c => c.deleted == true && c.id > 0)
            //             .Select(c =>
            //             {
            //                 c.danh_sach_doan = SelectedItem;
            //                 c.deleted = true;
            //                 return c;
            //             }).ToList();
            //         var updateChiTietList = SelectedItemsDetail
            //             .Where(c => (c.deleted == false || c.deleted == null) && c.id > 0)
            //             .Select(c =>
            //             {
            //                 c.danh_sach_doan = SelectedItem;
            //                 return c;
            //             }).ToList();

            //         if (addNewChiTietList.Any())
            //         {
            //             var detailResult = await DanhSachDoanChiTietService.CreateAsync(addNewChiTietList);
            //             if (!detailResult.IsSuccess)
            //             {
            //                 AlertService.ShowAlert(detailResult.Message ?? "Lỗi khi thêm mới chi tiết dữ liệu", "danger");
            //                 return;
            //             }
            //         }

            //         if (removeChiTietList.Any())
            //         {
            //             var detailResult = await DanhSachDoanChiTietService.DeleteAsync(removeChiTietList);
            //             if (!detailResult.IsSuccess)
            //             {
            //                 AlertService.ShowAlert(detailResult.Message ?? "Lỗi khi xóa chi tiết dữ liệu", "danger");
            //                 return;
            //             }
            //         }

            //         if (updateChiTietList.Any())
            //         {
            //             var detailResult = await DanhSachDoanChiTietService.UpdateAsync(updateChiTietList);
            //             if (!detailResult.IsSuccess)
            //             {
            //                 AlertService.ShowAlert(detailResult.Message ?? "Lỗi khi cập nhật chi tiết dữ liệu", "danger");
            //                 return;
            //             }
            //         }

            //         await LoadData();
            //         openAddOrUpdateModal = false;
            //         AlertService.ShowAlert("Cập nhật thành công!", "success");
            //     }
            //     else
            //     {
            //         AlertService.ShowAlert(result.Message ?? "Lỗi khi cập nhật dữ liệu", "danger");
            //     }
            // }
        }

        private bool FormValidation()
        {
            if (SelectedItem.code == null)
            {
                AlertService.ShowAlert("Mã là bắt buộc", "danger");
                return false;
            }

            if (string.IsNullOrEmpty(SelectedItem.name))
            {
                AlertService.ShowAlert("Tên là bắt buộc", "danger");
                return false;
            }

            return true;
        }

        private void CloseAddOrUpdateModal()
        {
            SelectedItem = new DanhSachDoanModel();
            openAddOrUpdateModal = false;
        }

        private async Task OnStatusFilterChanged(ChangeEventArgs? selected)
        {
            _searchStatusString = selected?.Value?.ToString() ?? string.Empty;

            await LoadData();
        }

        private void OnCongTyChanged(CongTyModel? selected)
        {
            SelectedItem.cong_ty = selected;
        }

        private void OnKhamSucKhoeCongTyChanged(KhamSucKhoeCongTyModel? selected)
        {
            SelectedItem.ma_ho_so_kham_suc_khoe = selected;
        }

        // private void OnDanhSachDoanTypeChanged(DanhSachDoanTypeModel? selected)
        // {
        //     SelectedItem.danh_sach_doan_type = selected;
        // }

        private void OnDateChanged(ChangeEventArgs e, string fieldName)
        {
            try
            {
                var dateStr = e.Value?.ToString();
                if (string.IsNullOrEmpty(dateStr))
                {
                    switch (fieldName)
                    {
                        case "ngay_kham":
                            SelectedItem.ngay_kham = null;
                            break;

                        case "ngay_lap_so":
                            SelectedItem.ngay_lap_so = null;
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
                        case "ngay_kham":
                            SelectedItem.ngay_kham = date;
                            break;

                        case "ngay_lap_so":
                            SelectedItem.ngay_lap_so = date;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                AlertService.ShowAlert($"Lỗi khi xử lý ngày: {ex.Message}", "danger");
            }
        }

        private void OnDateChangedDetail(ChangeEventArgs e, DanhSachDoanChiTietModel item, string fieldName)
        {
            try
            {
                var dateStr = e.Value?.ToString();
                if (string.IsNullOrEmpty(dateStr))
                {
                    switch (fieldName)
                    {
                        case "ngay_kham":
                            item.ngay_kham = null;
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
                        case "ngay_kham":
                            item.ngay_kham = date;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                AlertService.ShowAlert($"Lỗi khi xử lý ngày: {ex.Message}", "danger");
            }
        }

        private void OnCheckedChanged(ChangeEventArgs e, string fieldName)
        {
            try
            {
                var isChecked = e.Value != null && e.Value.ToString() == "true";
                switch (fieldName)
                {
                    case "da_gui_thong_bao":
                        SelectedItem.da_gui_thong_bao = isChecked;
                        break;
                    case "da_gui_mail":
                        SelectedItem.da_gui_mail = isChecked;
                        break;
                }
            }
            catch (Exception ex)
            {
                AlertService.ShowAlert($"Lỗi khi xử lý checkbox: {ex.Message}", "danger");
            }
        }

        private void OnTabChanged(string tab)
        {
            activeDefTab = tab;

            if (activeDefTab == "tab1")
            {
                // Wait for modal to render
                _ = Task.Run(async () =>
                {
                    await Task.Delay(500);
                    await JsRuntime.InvokeVoidAsync("initializeDatePicker");
                });
            }
        }
        // private void OnDinhMucChanged(DanhSachDoanChiTietModel item, DinhMucModel? selected)
        // {
        //     if (selected == null)
        //     {
        //         item.dinh_muc = null;
        //         item.don_gia_tt = null;
        //         item.don_gia_dm = null;
        //         item.thanh_tien_tt = null;
        //         item.thanh_tien_dm = null;
        //         return;
        //     }
        //     item.don_gia_tt = selected.DonGia ?? 0;
        //     item.don_gia_dm = selected.DinhMuc ?? 0;
        //     item.dinh_muc = selected;

        //     if (item.so_luong.HasValue && item.don_gia_tt.HasValue)
        //     {
        //         item.thanh_tien_tt = item.so_luong * item.don_gia_tt;
        //     }

        //     if (item.so_luong.HasValue && item.don_gia_dm.HasValue)
        //     {
        //         item.thanh_tien_dm = item.so_luong * item.don_gia_dm;
        //     }
        // }

        // private void UpdateThanhTien(ChangeEventArgs e, DanhSachDoanChiTietModel item, string field)
        // {
        //     if (!decimal.TryParse(e.Value?.ToString(), out decimal value))
        //     {
        //         AlertService.ShowAlert("Dữ liệu không hợp lệ", "warning");
        //         return;
        //     }
        //     if (value < 0)
        //     {
        //         AlertService.ShowAlert("Số lượng không thể nhỏ hơn 0", "warning");
        //         return;
        //     }

        //     switch (field)
        //     {
        //         case nameof(item.so_luong):
        //             item.so_luong = value;
        //             break;
        //         case nameof(item.don_gia_tt):
        //             item.don_gia_tt = value;
        //             break;
        //         case nameof(item.don_gia_dm):
        //             item.don_gia_dm = value;
        //             break;
        //     }

        //     item.thanh_tien_tt = null;
        //     item.thanh_tien_dm = null;

        //     if (item.so_luong.HasValue && item.don_gia_tt.HasValue)
        //     {
        //         item.thanh_tien_tt = item.so_luong * item.don_gia_tt;
        //     }

        //     if (item.so_luong.HasValue && item.don_gia_dm.HasValue)
        //     {
        //         item.thanh_tien_dm = item.so_luong * item.don_gia_dm;
        //     }
        // }

        // private async Task HandleFileSelect(InputFileChangeEventArgs e)
        // {
        //     var files = e.GetMultipleFiles();
        //     if (files != null && files.Any())
        //     {
        //         await ProcessFile(files[0]);
        //     }
        // }

        // private async Task HandleDrop(DragEventArgs e)
        // {
        //     var files = await JsRuntime.InvokeAsync<IReadOnlyList<IBrowserFile>>("getDroppedFiles", e);
        //     if (files?.Count > 0)
        //     {
        //         await ProcessFile(files[0]);
        //     }
        // }

        // private async Task ProcessFile(IBrowserFile file)
        // {
        //     SelectedFile = file;

        //     if (SelectedFile != null)
        //     {
        //         var maxAllowSize = 5 * 1024 * 1024;
        //         if (SelectedFile.Size <= maxAllowSize) // 5MB max size
        //         {
        //             try
        //             {
        //                 var buffer = new byte[SelectedFile.Size];
        //                 await SelectedFile.OpenReadStream(maxAllowSize).ReadExactlyAsync(buffer);
        //                 var base64 = Convert.ToBase64String(buffer);
        //                 fileContent = $"data:{SelectedFile.ContentType};base64,{base64}";
        //             }
        //             catch (Exception ex)
        //             {
        //                 await JsRuntime.InvokeVoidAsync("alert", $"Error processing image: {ex.Message}");
        //             }
        //             finally
        //             {
        //                 StateHasChanged();
        //             }
        //         }
        //         else
        //         {
        //             await JsRuntime.InvokeVoidAsync("alert", "File size exceeds 5MB limit");
        //         }
        //     }
        //     else
        //     {
        //         await JsRuntime.InvokeVoidAsync("alert", "Invalid image format");
        //     }
        // }

        // async Task DownloadFile()
        // {
        //     await JsRuntime.InvokeVoidAsync("downloadFile", fileContent, SelectedFile.Name);
        // }

        // private Task HandleDragOver(DragEventArgs e)
        // {
        //     e.DataTransfer.DropEffect = "copy";
        //     return Task.CompletedTask;
        // }

        // private Task HandleDragEnter(DragEventArgs e)
        // {
        //     // Optional: Add visual feedback for drag enter
        //     return Task.CompletedTask;
        // }

        // private Task HandleDragLeave(DragEventArgs e)
        // {
        //     // Optional: Remove visual feedback for drag leave
        //     return Task.CompletedTask;
        // }
        // private async Task UpdateImageAsync()
        // {
        //     if (SelectedFile != null)
        //     {
        //         try
        //         {
        //             UploadFileCRUD.folder = Guid.Parse(GlobalConstant.CONTRACT_FOLDER_ID);
        //             var fileUploaded = await _fileService.UploadFileAsync(SelectedFile, UploadFileCRUD);

        //             if (
        //                 fileUploaded != null
        //                 && fileUploaded.IsSuccess
        //                 && fileUploaded.Data != null
        //                 && !string.IsNullOrEmpty(fileUploaded.Data.filename_download)
        //             )
        //             {
        //                 SelectedItem.file_hd = fileUploaded.Data;
        //                 // await MainService.UpdateAsync(SelectedItem);;

        //                 // fileContent = string.Empty;
        //                 // SelectedFile = default!;
        //                 // await LoadData();
        //             }
        //             else
        //             {
        //                 await JsRuntime.InvokeVoidAsync("alert", "Failed to upload image.");
        //             }
        //         }
        //         catch (Exception ex)
        //         {
        //             await JsRuntime.InvokeVoidAsync("alert", $"Error saving image: {ex.Message}");
        //         }
        //         finally
        //         {
        //             StateHasChanged();
        //         }
        //     }
        // }

        // private void ClearImageUpload()
        // {
        //     fileContent = string.Empty;
        //     SelectedFile = default!;
        // }
    }
}