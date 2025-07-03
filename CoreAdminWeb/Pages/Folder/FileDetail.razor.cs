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
                                 IBaseService<PostModel> PostService) : BlazorCoreBase
    {
        [Parameter]
        public string FileId { get; set; } = string.Empty;
        private FileModel SelectedFile { get; set; } = new FileModel();
        private FileCRUDModel UploadFileCRUD { get; set; } = new FileCRUDModel();
        private PostModel _selectedPost { get; set; } = new PostModel();
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
                SelectedFile.GetFileType();
                UploadFileCRUD = MapToCRUDModel(SelectedFile);
            }
            else
            {
                AlertService.ShowAlert(result.Message ?? "Lỗi khi tải file", "danger");
            }
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
            fileCRUD.folder = file.folder;
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
