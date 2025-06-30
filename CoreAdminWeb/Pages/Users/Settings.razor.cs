using CoreAdminWeb.Model.User;
using CoreAdminWeb.Services.Files;
using CoreAdminWeb.Services.Users;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace CoreAdminWeb.Pages.Users
{
    public partial class Settings : ComponentBase
    {
        private readonly IUserService _userService;
        private readonly IFileService _fileService;

        public Settings(IUserService userService, IFileService fileService)
        {
            _userService = userService;
            _fileService = fileService;
        }

        private string previewImage { get; set; } = string.Empty;
        private UserModel CurrentUser { get; set; } = new UserModel();
        private bool IsLoading { get; set; } = true;

        private IBrowserFile SelectedFile { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await LoadCurrentUser();
                StateHasChanged();
            }
        }

        private async Task LoadCurrentUser()
        {
            try
            {
                var user = await _userService.GetCurrentUserAsync();
                if (user != null && user.IsSuccess)
                {
                    CurrentUser = user.Data ?? new UserModel();
                    CurrentUser.password = string.Empty;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching user profile: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
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

        private async Task HandleDrop(DragEventArgs e)
        {
            var files = await JsRuntime.InvokeAsync<IReadOnlyList<IBrowserFile>>("getDroppedFiles", e);
            if (files?.Count > 0)
            {
                await ProcessFile(files[0]);
            }
        }

        private async Task ProcessFile(IBrowserFile file)
        {
            SelectedFile = file;

            if (SelectedFile != null)
            {
                var maxAllowSize = 5 * 1024 * 1024;
                if (SelectedFile.Size <= maxAllowSize) // 5MB max size
                {
                    try
                    {
                        var buffer = new byte[SelectedFile.Size];
                        await SelectedFile.OpenReadStream(maxAllowSize).ReadExactlyAsync(buffer);
                        var base64 = Convert.ToBase64String(buffer);
                        previewImage = $"data:{SelectedFile.ContentType};base64,{base64}";
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
            else
            {
                await JsRuntime.InvokeVoidAsync("alert", "Invalid image format");
            }
        }

        private Task HandleDragOver(DragEventArgs e)
        {
            e.DataTransfer.DropEffect = "copy";
            return Task.CompletedTask;
        }

        private Task HandleDragEnter(DragEventArgs e)
        {
            // Optional: Add visual feedback for drag enter
            return Task.CompletedTask;
        }

        private Task HandleDragLeave(DragEventArgs e)
        {
            // Optional: Remove visual feedback for drag leave
            return Task.CompletedTask;
        }


        private async Task ChangeUserProfileAsync()
        {
            CurrentUser.password = string.Empty;
            await UpdateProfileAsync();
        }
        private async Task ChangePasswordAsync()
        {
            await UpdateProfileAsync();
        }
        private async Task UpdateProfileAsync()
        {
            try
            {
                await _userService.UpdateCurrentUserAsync(CurrentUser);
                await JsRuntime.InvokeVoidAsync("alert", "Updated profile successfully!");

                await LoadCurrentUser();
            }
            catch (Exception ex)
            {
                await JsRuntime.InvokeVoidAsync("alert", $"Error saving image: {ex.Message}");
            }
            finally
            {
                StateHasChanged();
            }
        }
        private async Task UpdateImageAsync()
        {
            if (SelectedFile != null)
            {
                try
                {
                    var fileUploaded = await _fileService.UploadFileAsync(SelectedFile);

                    if (
                        fileUploaded != null
                        && fileUploaded.IsSuccess
                        && fileUploaded.Data != null
                        && !string.IsNullOrEmpty(fileUploaded.Data.filename_download)
                    )
                    {
                        CurrentUser.avatar = fileUploaded.Data.id.ToString()!;
                        await _userService.UpdateCurrentUserAsync(CurrentUser);

                        previewImage = string.Empty;
                        SelectedFile = default!;
                        await LoadCurrentUser();
                    }
                    else
                    {
                        await JsRuntime.InvokeVoidAsync("alert", "Failed to upload image.");
                    }
                }
                catch (Exception ex)
                {
                    await JsRuntime.InvokeVoidAsync("alert", $"Error saving image: {ex.Message}");
                }
                finally
                {
                    StateHasChanged();
                }
            }
        }

        private void ClearImageUpload()
        {
            previewImage = string.Empty;
            SelectedFile = default!;
        }
    }
}
