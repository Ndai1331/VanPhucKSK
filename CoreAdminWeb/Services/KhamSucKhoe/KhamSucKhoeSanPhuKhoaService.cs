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
    public class KhamSucKhoeSanPhuKhoaService : IBaseGetService<KhamSucKhoeSanPhuKhoaModel>
    {
        private readonly string _collection = "kham_suc_khoe_san_phu_khoa";
        private readonly IHttpClientService _httpClientService;

        private string Fields = "*,user_created.last_name,user_created.first_name,user_updated.last_name,user_updated.first_name";

        public KhamSucKhoeSanPhuKhoaService(IHttpClientService httpClientService)
        {
            _httpClientService = httpClientService;
        }

        /// <summary>
        /// Gets all fertilizer production facilities
        /// </summary>
        public async Task<RequestHttpResponse<List<KhamSucKhoeSanPhuKhoaModel>>> GetAllAsync(string query, bool isPublic = false)
        {
            try
            {
                string url = $"items/{_collection}?fields={Fields}&{query}";
                var response = isPublic
                    ? await PublicRequestClient.GetAPIAsync<RequestHttpResponse<List<KhamSucKhoeSanPhuKhoaModel>>>(url)
                    : await _httpClientService.GetAPIAsync<RequestHttpResponse<List<KhamSucKhoeSanPhuKhoaModel>>>(url);

                return response.IsSuccess
                    ? new RequestHttpResponse<List<KhamSucKhoeSanPhuKhoaModel>> { Data = response.Data?.Data, Meta = response.Data?.Meta }
                    : new RequestHttpResponse<List<KhamSucKhoeSanPhuKhoaModel>> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return IBaseGetService<KhamSucKhoeSanPhuKhoaModel>.CreateErrorResponse<List<KhamSucKhoeSanPhuKhoaModel>>(ex);
            }
        }

        /// <summary>
        /// Gets a fertilizer production facility by ID
        /// </summary>
        public async Task<RequestHttpResponse<KhamSucKhoeSanPhuKhoaModel>> GetByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new RequestHttpResponse<KhamSucKhoeSanPhuKhoaModel>
                {
                    Errors = new List<ErrorResponse> { new() { Message = "ID không được để trống" } },
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            try
            {
                var response = await _httpClientService.GetAPIAsync<RequestHttpResponse<KhamSucKhoeSanPhuKhoaModel>>($"items/{_collection}/{id}?fields={Fields}");

                return response.IsSuccess
                    ? new RequestHttpResponse<KhamSucKhoeSanPhuKhoaModel> { Data = response.Data?.Data }
                    : new RequestHttpResponse<KhamSucKhoeSanPhuKhoaModel> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return IBaseGetService<KhamSucKhoeSanPhuKhoaModel>.CreateErrorResponse<KhamSucKhoeSanPhuKhoaModel>(ex);
            }
        }
    }
}