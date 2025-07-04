using CoreAdminWeb.Model;
using CoreAdminWeb.Model.RequestHttps;
using CoreAdminWeb.Services.BaseServices;
using CoreAdminWeb.Services.Http;
using System.Net;
using CoreAdminWeb.Http;
namespace CoreAdminWeb.Services.KhamSucKhoe
{
    /// <summary>
    /// Service for managing fertilizer production facilities
    /// </summary>
    public class KhamSucKhoeKetLuanService : IBaseGetService<KhamSucKhoeKetLuanModel>
    {
        private readonly string _collection = "kham_suc_khoe_ket_luan";
        private readonly IHttpClientService _httpClientService;

        private string Fields = "*,user_created.last_name,user_created.first_name,user_updated.last_name,user_updated.first_name";

        public KhamSucKhoeKetLuanService(IHttpClientService httpClientService)
        {
            _httpClientService = httpClientService;
        }

        /// <summary>
        /// Gets all fertilizer production facilities
        /// </summary>
        public async Task<RequestHttpResponse<List<KhamSucKhoeKetLuanModel>>> GetAllAsync(string query, bool isPublic = false)
        {
            try
            {
                string url = $"items/{_collection}?fields={Fields}&{query}";
                var response = isPublic 
                    ? await PublicRequestClient.GetAPIAsync<RequestHttpResponse<List<KhamSucKhoeKetLuanModel>>>(url) 
                    : await _httpClientService.GetAPIAsync<RequestHttpResponse<List<KhamSucKhoeKetLuanModel>>>(url);

                return response.IsSuccess
                    ? new RequestHttpResponse<List<KhamSucKhoeKetLuanModel>> { Data = response.Data.Data, Meta = response.Data.Meta }
                    : new RequestHttpResponse<List<KhamSucKhoeKetLuanModel>> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return IBaseGetService<KhamSucKhoeKetLuanModel>.CreateErrorResponse<List<KhamSucKhoeKetLuanModel>>(ex);
            }
        }

        /// <summary>
        /// Gets a fertilizer production facility by ID
        /// </summary>
        public async Task<RequestHttpResponse<KhamSucKhoeKetLuanModel>> GetByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new RequestHttpResponse<KhamSucKhoeKetLuanModel>
                {
                    Errors = new List<ErrorResponse> { new() { Message = "ID không được để trống" } },
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            try
            {
                var response = await _httpClientService.GetAPIAsync<RequestHttpResponse<KhamSucKhoeKetLuanModel>>($"items/{_collection}/{id}?fields={Fields}");
                
                return response.IsSuccess
                    ? new RequestHttpResponse<KhamSucKhoeKetLuanModel> { Data = response.Data.Data }
                    : new RequestHttpResponse<KhamSucKhoeKetLuanModel> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return IBaseGetService<KhamSucKhoeKetLuanModel>.CreateErrorResponse<KhamSucKhoeKetLuanModel>(ex);
            }
        }
    }
}