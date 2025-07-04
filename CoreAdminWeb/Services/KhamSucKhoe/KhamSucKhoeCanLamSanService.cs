using CoreAdminWeb.Model;
using CoreAdminWeb.Model.RequestHttps;
using CoreAdminWeb.RequestHttp;
using CoreAdminWeb.Services.BaseServices;
using System.Net;

namespace CoreAdminWeb.Services
{
    /// <summary>
    /// Service for managing fertilizer production facilities
    /// </summary>
    public class KhamSucKhoeCanLamSanService : IBaseGetService<KhamSucKhoeCanLamSanModel>
    {
        private readonly string _collection = "kham_suc_khoe_can_lam_sang";
        private const string Fields = "*,user_created.last_name,user_created.first_name,user_updated.last_name,user_updated.first_name";

        /// <summary>
        /// Gets all fertilizer production facilities
        /// </summary>
        public async Task<RequestHttpResponse<List<KhamSucKhoeCanLamSanModel>>> GetAllAsync(string query, bool isPublic = false)
        {
            try
            {
                string url = $"items/{_collection}?fields={Fields}&{query}";
                var response = isPublic ? await PublicRequestClient.GetAPIAsync<RequestHttpResponse<List<KhamSucKhoeCanLamSanModel>>>(url) : await RequestClient.GetAPIAsync<RequestHttpResponse<List<KhamSucKhoeCanLamSanModel>>>(url);

                return response.IsSuccess
                    ? new RequestHttpResponse<List<KhamSucKhoeCanLamSanModel>> { Data = response.Data?.Data, Meta = response.Data?.Meta }
                    : new RequestHttpResponse<List<KhamSucKhoeCanLamSanModel>> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return IBaseGetService<KhamSucKhoeCanLamSanModel>.CreateErrorResponse<List<KhamSucKhoeCanLamSanModel>>(ex);
            }
        }

        /// <summary>
        /// Gets a fertilizer production facility by ID
        /// </summary>
        public async Task<RequestHttpResponse<KhamSucKhoeCanLamSanModel>> GetByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new RequestHttpResponse<KhamSucKhoeCanLamSanModel>
                {
                    Errors = new List<ErrorResponse> { new() { Message = "ID không được để trống" } },
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            try
            {
                var response = await RequestClient.GetAPIAsync<RequestHttpResponse<KhamSucKhoeCanLamSanModel>>($"items/{_collection}/{id}?fields={Fields}");

                return response.IsSuccess
                    ? new RequestHttpResponse<KhamSucKhoeCanLamSanModel> { Data = response.Data?.Data, Meta = response.Data?.Meta }
                    : new RequestHttpResponse<KhamSucKhoeCanLamSanModel> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return IBaseGetService<KhamSucKhoeCanLamSanModel>.CreateErrorResponse<KhamSucKhoeCanLamSanModel>(ex);
            }
        }
    }
}