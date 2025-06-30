
using CoreAdminWeb.Model;
using CoreAdminWeb.Model.Configuration;
using CoreAdminWeb.Model.RequestHttps;
using CoreAdminWeb.RequestHttp;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Options;

namespace CoreAdminWeb.Services.Files
{
    public interface IFileService
    {
        Task<RequestHttpResponse<List<FileModel>>> GetAllFileAsync(string query);
        Task<RequestHttpResponse<FileModel>> GetFileAsync(string id);
        Task<RequestHttpResponse<FileModel>> UploadFileAsync(IBrowserFile file, FileCRUDModel? model = null);
        Task<RequestHttpResponse<FileCRUDModel>> EditFileAsync(string id,  FileCRUDModel? model = null);
        Task<RequestHttpResponse<FileModel>> GetPublicFileAsync(string id);
    }

    public class FileService : IFileService
    {
        private readonly IOptions<DrCoreApi> _appSettings;
        public FileService(IOptions<DrCoreApi> appSettings)
        {
            _appSettings = appSettings;
        }

        public async Task<RequestHttpResponse<List<FileModel>>> GetAllFileAsync(string query)
        {
            var response = new RequestHttpResponse<List<FileModel>>();
            try
            {
                string Fields = "id, storage,filename_disk,filename_download,"
                + "title,type,filesize,width,height,system,folder,"
                + "user_created.last_name,user_created.first_name,user_updated.last_name,user_updated.first_name,"
                +"co_quan_ban_hanh, so_van_ban, so_ky_hieu,ngay_ban_hanh,ngay_hieu_luc,so_luu_tru,"
                + "linh_vuc_vb.id, linh_vuc_vb.name,"
                + "phan_loai_vb.id, phan_loai_vb.name";
                var result = await RequestClient.GetAPIAsync<RequestHttpResponse<List<FileModel>>>($"files?fields={Fields}&sort=-uploaded_on&{query}");
                response = result.IsSuccess
                    ? new RequestHttpResponse<List<FileModel>> { Data = result.Data.Data, Meta = result.Data.Meta, Errors = result.Errors }
                    : new RequestHttpResponse<List<FileModel>> { Errors = result.Errors };

                if(response.IsSuccess && response.Data != null)
                {
                    foreach (var item in response.Data)
                    {
                        item.filename_disk = $"{_appSettings.Value.BaseUrl}assets/{item.filename_disk}";
                        item.filename_download = $"{_appSettings.Value.BaseUrl}assets/{item.id}";
                        item.url_download = $"{_appSettings.Value.BaseUrl}assets/{item.id}?download=";
                    }
                }
            }
            catch (Exception ex)
            {
                response.Errors = new List<ErrorResponse> { new ErrorResponse { Message = ex.Message } };
            }
            return response;
        }


        public async Task<RequestHttpResponse<FileModel>> GetFileAsync(string id)
        {
            var response = new RequestHttpResponse<FileModel>();
            try
            {
                string Fields = "*,user_created.last_name,user_created.first_name,user_updated.last_name,user_updated.first_name,"
                +"co_quan_ban_hanh, so_van_ban, so_ky_hieu,ngay_ban_hanh,ngay_hieu_luc,so_luu_tru,"
                + "linh_vuc_vb.id, linh_vuc_vb.name,"
                + "phan_loai_vb.id, phan_loai_vb.name";
                
                var result = await RequestClient.GetAPIAsync<RequestHttpResponse<FileModel>>($"files/{id}?fields={Fields}");
                if (result?.Data != null)
                {
                    response.Data = result.Data.Data;
                    response.Data.filename_disk = $"{_appSettings.Value.BaseUrl}assets/{result.Data.Data.filename_disk}";
                    response.Data.filename_download = $"{_appSettings.Value.BaseUrl}assets/{result.Data.Data.id}";
                }
                else if (result?.Errors != null)
                {
                    response.Errors = result.Errors;
                }
            }
            catch (Exception ex)
            {
                response.Errors = new List<ErrorResponse> { new ErrorResponse { Message = ex.Message } };
            }
            return response;
        }

        public async Task<RequestHttpResponse<FileModel>> GetPublicFileAsync(string id)
        {
            var response = new RequestHttpResponse<FileModel>();
            try
            {
                string Fields = "*,user_created.last_name,user_created.first_name,user_updated.last_name,user_updated.first_name,"
                +"co_quan_ban_hanh, so_van_ban, so_ky_hieu,ngay_ban_hanh,ngay_hieu_luc,so_luu_tru,"
                + "linh_vuc_vb.id, linh_vuc_vb.name,"
                + "phan_loai_vb.id, phan_loai_vb.name";
                
                var result = await PublicRequestClient.GetAPIAsync<RequestHttpResponse<FileModel>>($"files/{id}?fields={Fields}");
                if (result?.Data != null)
                {
                    response.Data = result.Data.Data;
                    response.Data.filename_disk = $"{_appSettings.Value.BaseUrl}assets/{result.Data.Data.filename_disk}";
                    response.Data.filename_download = $"{_appSettings.Value.BaseUrl}assets/{result.Data.Data.id}";
                }
                else if (result?.Errors != null)
                {
                    response.Errors = result.Errors;
                }
            }
            catch (Exception ex)
            {
                response.Errors = new List<ErrorResponse> { new ErrorResponse { Message = ex.Message } };
            }
            return response;
        }
        public async Task<RequestHttpResponse<FileModel>> UploadFileAsync(IBrowserFile file, FileCRUDModel? model = null)
        {
            var response = new RequestHttpResponse<FileModel>();
            try
            {
                var result = await RequestClient.PostAPIWithFileAsync<RequestHttpResponse<FileModel>>($"files", file);
                if (result.IsSuccess)
                {
                    response.Data = result.Data.Data;
                    response.Data.filename_disk = $"{_appSettings.Value.BaseUrl}assets/{result.Data.Data.filename_disk}";
                    response.Data.filename_download = $"{_appSettings.Value.BaseUrl}assets/{result.Data.Data.filename_download}";
                    if(model != null)
                    {
                        await RequestClient.PatchAPIAsync<RequestHttpResponse<FileModel>>($"files/{response.Data.id}", model);
                    }
                }
                else if (result?.Errors != null)
                {
                    response.Errors = result.Errors;
                }
            }
            catch (Exception ex)
            {
                response.Errors = new List<ErrorResponse> { new ErrorResponse { Message = ex.Message } };
            }
            return response;
        }

         public async Task<RequestHttpResponse<FileCRUDModel>> EditFileAsync(string id,  FileCRUDModel? model = null)
        {
            var response = new RequestHttpResponse<FileCRUDModel>();
            try
            {
                var result = await RequestClient.PatchAPIAsync<RequestHttpResponse<FileCRUDModel>>($"files/{id}", 
                new {
                    co_quan_ban_hanh = model.co_quan_ban_hanh,
                    so_van_ban = model.so_van_ban,
                    so_ky_hieu = model.so_ky_hieu,
                    ngay_ban_hanh = model.ngay_ban_hanh,
                    ngay_hieu_luc = model.ngay_hieu_luc,
                    so_luu_tru = model.so_luu_tru,
                    linh_vuc_vb = model.linh_vuc_vb,
                    phan_loai_vb = model.phan_loai_vb
                });
                if (result.IsSuccess)
                {
                    response.Data = result.Data.Data;
                }
                else if (result?.Errors != null)
                {
                    response.Errors = result.Errors;
                }
            }
            catch (Exception ex)
            {
                response.Errors = new List<ErrorResponse> { new ErrorResponse { Message = ex.Message } };
            }
            return response;
        }
    }
}