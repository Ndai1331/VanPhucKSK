using CoreAdminWeb.Http;
using CoreAdminWeb.Model.KhamSucKhoes;
using CoreAdminWeb.Model.RequestHttps;
using CoreAdminWeb.Services.BaseServices;
using CoreAdminWeb.Services.Http;
using System.Net;

namespace CoreAdminWeb.Services.KhamSucKhoe
{
    /// <summary>
    /// Service for managing fertilizer production facilities
    /// </summary>
    public class KetQuaCanLamSangFileService : IBaseGetService<KetQuaCanLamSangFileModel>
    {
        private readonly string _collection = "ket_qua_can_lam_sang_file";
        private readonly IHttpClientService _httpClientService;

        private readonly string Fields = "*,user_created.last_name,user_created.first_name,user_updated.last_name,user_updated.first_name";

        public KetQuaCanLamSangFileService(IHttpClientService httpClientService)
        {
            _httpClientService = httpClientService;
        }

        /// <summary>
        /// Gets all fertilizer production facilities
        /// </summary>
        public async Task<RequestHttpResponse<List<KetQuaCanLamSangFileModel>>> GetAllAsync(string query, bool isPublic = false)
        {
            try
            {
                string url = $"items/{_collection}?fields={Fields}&{query}";
                var response = isPublic
                    ? await PublicRequestClient.GetAPIAsync<RequestHttpResponse<List<KetQuaCanLamSangFileModel>>>(url)
                    : await _httpClientService.GetAPIAsync<RequestHttpResponse<List<KetQuaCanLamSangFileModel>>>(url);

                return response.IsSuccess
                    ? new RequestHttpResponse<List<KetQuaCanLamSangFileModel>> { Data = response.Data?.Data, Meta = response.Data?.Meta }
                    : new RequestHttpResponse<List<KetQuaCanLamSangFileModel>> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return IBaseGetService<KetQuaCanLamSangFileModel>.CreateErrorResponse<List<KetQuaCanLamSangFileModel>>(ex);
            }
        }

        /// <summary>
        /// Gets a fertilizer production facility by ID
        /// </summary>
        public async Task<RequestHttpResponse<KetQuaCanLamSangFileModel>> GetByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new RequestHttpResponse<KetQuaCanLamSangFileModel>
                {
                    Errors = new List<ErrorResponse> { new() { Message = "ID không được để trống" } },
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            try
            {
                var response = await _httpClientService.GetAPIAsync<RequestHttpResponse<KetQuaCanLamSangFileModel>>($"items/{_collection}/{id}?fields={Fields}");

                return response.IsSuccess
                    ? new RequestHttpResponse<KetQuaCanLamSangFileModel> { Data = response.Data?.Data, Meta = response.Data?.Meta }
                    : new RequestHttpResponse<KetQuaCanLamSangFileModel> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return IBaseGetService<KetQuaCanLamSangFileModel>.CreateErrorResponse<KetQuaCanLamSangFileModel>(ex);
            }
        }
    }
}