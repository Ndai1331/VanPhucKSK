using CoreAdminWeb.Model;
using CoreAdminWeb.Services.BaseServices;
using CoreAdminWeb.Shared.Base;
using Microsoft.JSInterop;
using CoreAdminWeb.Services.Files;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace CoreAdminWeb.Pages.Folder
{
    public partial class FileDetail(
                                IFileService FileService,
                                IBaseService<FolderModel> FolderService,
                                 IBaseService<LinhVucVanBanModel> LinhVucVanBanService,
                                 IBaseService<PhanLoaiVanBanModel> PhanLoaiVanBanService) : BlazorCoreBase
    {
        [Parameter]
        public string FileId { get; set; } = string.Empty;
        private FileModel SelectedFile { get; set; } = new FileModel();
        private FileCRUDModel UploadFileCRUD { get; set; } = new FileCRUDModel();
        private PhanLoaiVanBanModel _selectedPhanLoaiVanBan { get; set; } = new PhanLoaiVanBanModel();
        private LinhVucVanBanModel _selectedLinhVucVanBan { get; set; } = new LinhVucVanBanModel();
        private IBrowserFile UploadFile { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await LoadFile(FileId);


                _ = Task.Run(async () =>
                {
                    await Task.Delay(500);
                    await JsRuntime.InvokeVoidAsync("initializeDatePicker");
                });
                await JsRuntime.InvokeAsync<IJSObjectReference>("import", "/assets/js/pages/flatpickr.js");
                StateHasChanged();
            }
        }

        private async Task LoadFile(string fileId)
        {
            var result = await FileService.GetFileAsync(fileId);
            if(result.IsSuccess)
            {
                SelectedFile = result.Data;
                if(SelectedFile.phan_loai_vb != null)
                {
                    _selectedPhanLoaiVanBan = SelectedFile.phan_loai_vb;
                }
                if(SelectedFile.linh_vuc_vb != null)
                {
                    _selectedLinhVucVanBan = SelectedFile.linh_vuc_vb;  
                }
                SelectedFile.GetFileType();
                UploadFileCRUD = MapToCRUDModel(SelectedFile);
            }
            else
            {
                AlertService.ShowAlert(result.Message ?? "Lỗi khi tải file", "danger");
            }
        }
        private async Task<IEnumerable<PhanLoaiVanBanModel>> LoadPhanLoaiVanBanData(string searchText)
        {
            return await LoadBlazorTypeaheadData(searchText, PhanLoaiVanBanService);
        }
        private async Task<IEnumerable<LinhVucVanBanModel>> LoadLinhVucVanBanData(string searchText)
        {
            return await LoadBlazorTypeaheadData(searchText, LinhVucVanBanService);
        }

        private void OnPhanLoaiVanBanChanged(PhanLoaiVanBanModel selected)
        {
            _selectedPhanLoaiVanBan = selected;
            UploadFileCRUD.phan_loai_vb = selected.id;
        }
        private void OnLinhVucVanBanChanged(LinhVucVanBanModel selected)
        {
            _selectedLinhVucVanBan = selected;
            UploadFileCRUD.linh_vuc_vb = selected.id;
        }


        private async Task OnUploadFile()
        {
            try
            {
                var result = await FileService.EditFileAsync(FileId, UploadFileCRUD);
                if(result.IsSuccess)
                {
                    AlertService.ShowAlert("Cập nhật file thành công!", "success");
                }
                else
                {
                    AlertService.ShowAlert(result.Message ?? "Lỗi khi cập nhật file", "danger");
                }
            }
            catch (Exception ex)
            {
                AlertService.ShowAlert(ex.Message ?? "Lỗi khi cập nhật file", "danger");
            }
            finally
            {
                StateHasChanged();
            }
        }

        private void OnDateChanged(ChangeEventArgs e, string fieldName)
        {
            try
            {
                var dateStr = e.Value?.ToString();
                if (string.IsNullOrEmpty(dateStr))
                {
                    switch (fieldName)
                    {
                        case nameof(UploadFileCRUD.ngay_ban_hanh):
                            UploadFileCRUD.ngay_ban_hanh = null;
                            break;
                        case nameof(UploadFileCRUD.ngay_hieu_luc):
                            UploadFileCRUD.ngay_hieu_luc = null;
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
                        case nameof(UploadFileCRUD.ngay_ban_hanh):
                            UploadFileCRUD.ngay_ban_hanh = date;
                            break;
                        case nameof(UploadFileCRUD.ngay_hieu_luc):
                            UploadFileCRUD.ngay_hieu_luc = date;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                AlertService.ShowAlert($"Lỗi khi xử lý ngày: {ex.Message}", "danger");
            }
        }

        private void OnBack()
        {
            NavigationManager?.NavigateTo($"/danh-muc-vbqppl");
        }


        private FileCRUDModel MapToCRUDModel(FileModel file)
        {
            FileCRUDModel fileCRUD = new FileCRUDModel();

            fileCRUD.icon_file = file.icon_file;
            fileCRUD.storage = file.storage;
            fileCRUD.filename_disk = file.filename_disk;
            fileCRUD.filename_download = file.filename_download;
            fileCRUD.type_file = file.type_file;
            fileCRUD.phan_loai_vb = file.phan_loai_vb?.id;
            fileCRUD.linh_vuc_vb = file.linh_vuc_vb?.id;
            fileCRUD.ngay_ban_hanh = file.ngay_ban_hanh;
            fileCRUD.ngay_hieu_luc = file.ngay_hieu_luc;
            fileCRUD.co_quan_ban_hanh = file.co_quan_ban_hanh;
            fileCRUD.so_van_ban = file.so_van_ban;
            fileCRUD.so_ky_hieu = file.so_ky_hieu;
            fileCRUD.so_luu_tru = file.so_luu_tru;
            fileCRUD.folder = file.folder;
            fileCRUD.system = file.system;
            fileCRUD.title = file.title;
            fileCRUD.type = file.type;
            fileCRUD.charset = file.charset;
            fileCRUD.filesize = file.filesize;
            fileCRUD.width = file.width;
            fileCRUD.height = file.height;
            fileCRUD.duration = file.duration;
            fileCRUD.embed = file.embed;
            fileCRUD.description = file.description;
            fileCRUD.location = file.location;
            fileCRUD.tags = file.tags;
            //fileCRUD.metadata = file.metadata;
            fileCRUD.focal_point_x = file.focal_point_x;
            fileCRUD.focal_point_y = file.focal_point_y;
            fileCRUD.tus_id = file.tus_id;
            fileCRUD.tus_data = file.tus_data;
            return fileCRUD;
        }
    }
}
