using CoreAdminWeb.Model.RequestHttps;
using CoreAdminWeb.Model;
using CoreAdminWeb.Services.BaseServices;
using CoreAdminWeb.RequestHttp;
using System.Net;
using Microsoft.Extensions.Options;
using CoreAdminWeb.Model.Configuration;

namespace CoreAdminWeb.Services
{
    public class PostService : IBaseGetService<PostModel>
    {
        private readonly IOptions<DrCoreApi> _appSettings;
        public PostService(IOptions<DrCoreApi> appSettings)
        {
            _appSettings = appSettings;
        }
        private readonly string _collection = "Posts";
        private const string Fields = "*," +
        "post_images.modified_on,post_images.type,post_images.filename_disk,post_images.storage,post_images.id";
        public async Task<RequestHttpResponse<List<PostModel>>> GetAllAsync(string query, bool isPulic = false)
        {
            try
            {
                string url = $"items/{_collection}?fields={Fields}&{query}";
                var result = await PublicRequestClient.GetAPIAsync<RequestHttpResponse<List<PostModel>>>(url);
                

                var response = result.IsSuccess
                    ? new RequestHttpResponse<List<PostModel>> { Data = result.Data.Data, Meta = result.Data.Meta, Errors = result.Errors }
                    : new RequestHttpResponse<List<PostModel>> { Errors = result.Errors };

                if(response.IsSuccess && response.Data != null)
                {
                    foreach (var item in response.Data)
                    {
                        if(item.post_images != null)
                        {
                            item.post_images.filename_disk = $"{_appSettings.Value.BaseUrl}assets/{item.post_images.filename_disk}";
                        }
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                return IBaseGetService<PostModel>.CreateErrorResponse<List<PostModel>>(ex);
            }
        }

        /// <summary>
        /// Gets a fertilizer production facility by ID
        /// </summary>
        public async Task<RequestHttpResponse<PostModel>> GetByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new RequestHttpResponse<PostModel>
                {
                    Errors = new List<ErrorResponse> { new() { Message = "ID không được để trống" } },
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            try
            {
                var response = await RequestClient.GetAPIAsync<RequestHttpResponse<PostModel>>($"items/{_collection}/{id}?fields={Fields}");
                
                return response.IsSuccess
                    ? new RequestHttpResponse<PostModel> { Data = response.Data.Data, Meta = response.Data.Meta }
                    : new RequestHttpResponse<PostModel> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return IBaseGetService<PostModel>.CreateErrorResponse<PostModel>(ex);
            }
        }
    }
}