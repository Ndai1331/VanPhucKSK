using CoreAdminWeb.Http;
using CoreAdminWeb.Model;
using CoreAdminWeb.Model.RequestHttps;
using CoreAdminWeb.Services.BaseServices;
using CoreAdminWeb.Services.Http;
using System.Net;
namespace CoreAdminWeb.Services.KhamSucKhoe
{
    /// <summary>
    /// Service for managing fertilizer production facilities
    /// </summary>
    public class KhamSucKhoeTheLucService : IBaseGetService<KhamSucKhoeTheLucModel>
    {
        private readonly string _collection = "kham_suc_khoe_the_luc";
        private readonly IHttpClientService _httpClientService;

        private string Fields = "*,user_created.last_name,user_created.first_name,user_updated.last_name,user_updated.first_name";

        public KhamSucKhoeTheLucService(IHttpClientService httpClientService)
        {
            _httpClientService = httpClientService;
        }

        /// <summary>
        /// Gets all fertilizer production facilities
        /// </summary>
        public async Task<RequestHttpResponse<List<KhamSucKhoeTheLucModel>>> GetAllAsync(string query, bool isPublic = false)
        {
            try
            {
                string url = $"items/{_collection}?fields={Fields}&{query}";
                var response = isPublic
                    ? await PublicRequestClient.GetAPIAsync<RequestHttpResponse<List<KhamSucKhoeTheLucModel>>>(url)
                    : await _httpClientService.GetAPIAsync<RequestHttpResponse<List<KhamSucKhoeTheLucModel>>>(url);

                return response.IsSuccess
                    ? new RequestHttpResponse<List<KhamSucKhoeTheLucModel>> { Data = response.Data?.Data, Meta = response.Data?.Meta }
                    : new RequestHttpResponse<List<KhamSucKhoeTheLucModel>> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return IBaseGetService<KhamSucKhoeTheLucModel>.CreateErrorResponse<List<KhamSucKhoeTheLucModel>>(ex);
            }
        }

        /// <summary>
        /// Gets a fertilizer production facility by ID
        /// </summary>
        public async Task<RequestHttpResponse<KhamSucKhoeTheLucModel>> GetByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new RequestHttpResponse<KhamSucKhoeTheLucModel>
                {
                    Errors = new List<ErrorResponse> { new() { Message = "ID không được để trống" } },
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            try
            {
                var response = await _httpClientService.GetAPIAsync<RequestHttpResponse<KhamSucKhoeTheLucModel>>($"items/{_collection}/{id}?fields={Fields}");

                return response.IsSuccess
                    ? new RequestHttpResponse<KhamSucKhoeTheLucModel> { Data = response.Data?.Data }
                    : new RequestHttpResponse<KhamSucKhoeTheLucModel> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return IBaseGetService<KhamSucKhoeTheLucModel>.CreateErrorResponse<KhamSucKhoeTheLucModel>(ex);
            }
        }
    }
}