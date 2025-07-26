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
    public class KetQuaCanLamSangService : IBaseGetService<KetQuaCanLamSangModel>
    {
        private readonly string _collection = "kham_suc_khoe_can_lam_sang";
        private const string Fields = "*,user_created.last_name,user_created.first_name,user_updated.last_name,user_updated.first_name"
            + ",luot_kham.id,luot_kham.ma_luot_kham";
        private readonly IHttpClientService _httpClientService;

        public KetQuaCanLamSangService(IHttpClientService httpClientService)
        {
            _httpClientService = httpClientService;
        }

        /// <summary>
        /// Gets all fertilizer production facilities
        /// </summary>
        public async Task<RequestHttpResponse<List<KetQuaCanLamSangModel>>> GetAllAsync(string query, bool isPublic = false)
        {
            try
            {
                string url = $"items/{_collection}?fields={Fields}&{query}";
                var response = isPublic ? await PublicRequestClient.GetAPIAsync<RequestHttpResponse<List<KetQuaCanLamSangModel>>>(url) : await _httpClientService.GetAPIAsync<RequestHttpResponse<List<KetQuaCanLamSangModel>>>(url);

                return response.IsSuccess
                    ? new RequestHttpResponse<List<KetQuaCanLamSangModel>> { Data = response.Data?.Data, Meta = response.Data?.Meta }
                    : new RequestHttpResponse<List<KetQuaCanLamSangModel>> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return IBaseGetService<KetQuaCanLamSangModel>.CreateErrorResponse<List<KetQuaCanLamSangModel>>(ex);
            }
        }

        /// <summary>
        /// Gets a fertilizer production facility by ID
        /// </summary>
        public async Task<RequestHttpResponse<KetQuaCanLamSangModel>> GetByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new RequestHttpResponse<KetQuaCanLamSangModel>
                {
                    Errors = new List<ErrorResponse> { new() { Message = "ID không được để trống" } },
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            try
            {
                var response = await _httpClientService.GetAPIAsync<RequestHttpResponse<KetQuaCanLamSangModel>>($"items/{_collection}/{id}?fields={Fields}");

                return response.IsSuccess
                    ? new RequestHttpResponse<KetQuaCanLamSangModel> { Data = response.Data?.Data, Meta = response.Data?.Meta }
                    : new RequestHttpResponse<KetQuaCanLamSangModel> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return IBaseGetService<KetQuaCanLamSangModel>.CreateErrorResponse<KetQuaCanLamSangModel>(ex);
            }
        }
    }
}