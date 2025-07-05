using CoreAdminWeb.Http;
using CoreAdminWeb.Model;
using CoreAdminWeb.Model.RequestHttps;
using CoreAdminWeb.Services.BaseServices;
using CoreAdminWeb.Services.Http;
using System.Net;

namespace CoreAdminWeb.Services
{
    /// <summary>
    /// Service for managing fertilizer production facilities
    /// </summary>
    public class CongTyConTractService : IBaseGetService<CongTyConTractModel>
    {
        private readonly string _collection = "company_contract";
        private const string Fields = "*,user_created.last_name,user_created.first_name,user_updated.last_name,user_updated.first_name";
        private readonly IHttpClientService _httpClientService;

        public CongTyConTractService(IHttpClientService httpClientService)
        {
            _httpClientService = httpClientService;
        }

        /// <summary>
        /// Gets all fertilizer production facilities
        /// </summary>
        public async Task<RequestHttpResponse<List<CongTyConTractModel>>> GetAllAsync(string query, bool isPublic = false)
        {
            try
            {
                string url = $"items/{_collection}?fields={Fields}&{query}";
                var response = isPublic ? await PublicRequestClient.GetAPIAsync<RequestHttpResponse<List<CongTyConTractModel>>>(url) : await _httpClientService.GetAPIAsync<RequestHttpResponse<List<CongTyConTractModel>>>(url);

                return response.IsSuccess
                    ? new RequestHttpResponse<List<CongTyConTractModel>> { Data = response.Data?.Data, Meta = response.Data?.Meta }
                    : new RequestHttpResponse<List<CongTyConTractModel>> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return IBaseGetService<CongTyConTractModel>.CreateErrorResponse<List<CongTyConTractModel>>(ex);
            }
        }

        /// <summary>
        /// Gets a fertilizer production facility by ID
        /// </summary>
        public async Task<RequestHttpResponse<CongTyConTractModel>> GetByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new RequestHttpResponse<CongTyConTractModel>
                {
                    Errors = new List<ErrorResponse> { new() { Message = "ID không được để trống" } },
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            try
            {
                var response = await _httpClientService.GetAPIAsync<RequestHttpResponse<CongTyConTractModel>>($"items/{_collection}/{id}?fields={Fields}");

                return response.IsSuccess
                    ? new RequestHttpResponse<CongTyConTractModel> { Data = response.Data?.Data, Meta = response.Data?.Meta }
                    : new RequestHttpResponse<CongTyConTractModel> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return IBaseGetService<CongTyConTractModel>.CreateErrorResponse<CongTyConTractModel>(ex);
            }
        }
    }
}