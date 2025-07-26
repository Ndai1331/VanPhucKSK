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
    public class SoKhamSucKhoeService : IBaseGetService<SoKhamSucKhoeModel>
    {
        private readonly string _collection = "SoKhamSucKhoe";
        private readonly IHttpClientService _httpClientService;

        private readonly string Fields = "*,user_created.last_name,user_created.first_name,user_updated.last_name,user_updated.first_name"
            + ",benh_nhan.id,benh_nhan.first_name,benh_nhan.last_name,benh_nhan.so_dien_thoai"
            + ",benh_nhan.ngay_sinh,benh_nhan.gioi_tinh,benh_nhan.so_dinh_danh,benh_nhan.dia_chi"
            + ",MaDotKham.ma_hop_dong_ksk.code";

        public SoKhamSucKhoeService(IHttpClientService httpClientService)
        {
            _httpClientService = httpClientService;
        }

        /// <summary>
        /// Gets all fertilizer production facilities
        /// </summary>
        public async Task<RequestHttpResponse<List<SoKhamSucKhoeModel>>> GetAllAsync(string query, bool isPublic = false)
        {
            try
            {
                string url = $"items/{_collection}?fields={Fields}&{query}";
                var response = isPublic
                    ? await PublicRequestClient.GetAPIAsync<RequestHttpResponse<List<SoKhamSucKhoeModel>>>(url)
                    : await _httpClientService.GetAPIAsync<RequestHttpResponse<List<SoKhamSucKhoeModel>>>(url);

                return response.IsSuccess
                    ? new RequestHttpResponse<List<SoKhamSucKhoeModel>> { Data = response.Data?.Data, Meta = response.Data?.Meta }
                    : new RequestHttpResponse<List<SoKhamSucKhoeModel>> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return IBaseGetService<SoKhamSucKhoeModel>.CreateErrorResponse<List<SoKhamSucKhoeModel>>(ex);
            }
        }

        /// <summary>
        /// Gets a fertilizer production facility by ID
        /// </summary>
        public async Task<RequestHttpResponse<SoKhamSucKhoeModel>> GetByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new RequestHttpResponse<SoKhamSucKhoeModel>
                {
                    Errors = new List<ErrorResponse> { new() { Message = "ID không được để trống" } },
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            try
            {
                var response = await _httpClientService.GetAPIAsync<RequestHttpResponse<SoKhamSucKhoeModel>>($"items/{_collection}/{id}?fields={Fields}");

                return response.IsSuccess
                    ? new RequestHttpResponse<SoKhamSucKhoeModel> { Data = response.Data?.Data }
                    : new RequestHttpResponse<SoKhamSucKhoeModel> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return IBaseGetService<SoKhamSucKhoeModel>.CreateErrorResponse<SoKhamSucKhoeModel>(ex);
            }
        }
    }
}