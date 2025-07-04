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
    public class KhamSucKhoeChuyenKhoaService : IBaseGetService<KhamSucKhoeChuyenKhoaModel>
    {
        private readonly string _collection = "kham_suc_khoe_kham_chuyen_khoa";
        private const string Fields = "*,user_created.last_name,user_created.first_name,user_updated.last_name,user_updated.first_name";

        /// <summary>
        /// Gets all fertilizer production facilities
        /// </summary>
        public async Task<RequestHttpResponse<List<KhamSucKhoeChuyenKhoaModel>>> GetAllAsync(string query, bool isPublic = false)
        {
            try
            {
                string url = $"items/{_collection}?fields={Fields}&{query}";
                var response = isPublic ? await PublicRequestClient.GetAPIAsync<RequestHttpResponse<List<KhamSucKhoeChuyenKhoaModel>>>(url) : await RequestClient.GetAPIAsync<RequestHttpResponse<List<KhamSucKhoeChuyenKhoaModel>>>(url);

                return response.IsSuccess
                    ? new RequestHttpResponse<List<KhamSucKhoeChuyenKhoaModel>> { Data = response.Data?.Data, Meta = response.Data?.Meta }
                    : new RequestHttpResponse<List<KhamSucKhoeChuyenKhoaModel>> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return IBaseGetService<KhamSucKhoeChuyenKhoaModel>.CreateErrorResponse<List<KhamSucKhoeChuyenKhoaModel>>(ex);
            }
        }

        /// <summary>
        /// Gets a fertilizer production facility by ID
        /// </summary>
        public async Task<RequestHttpResponse<KhamSucKhoeChuyenKhoaModel>> GetByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new RequestHttpResponse<KhamSucKhoeChuyenKhoaModel>
                {
                    Errors = new List<ErrorResponse> { new() { Message = "ID không được để trống" } },
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            try
            {
                var response = await RequestClient.GetAPIAsync<RequestHttpResponse<KhamSucKhoeChuyenKhoaModel>>($"items/{_collection}/{id}?fields={Fields}");

                return response.IsSuccess
                    ? new RequestHttpResponse<KhamSucKhoeChuyenKhoaModel> { Data = response.Data?.Data, Meta = response.Data?.Meta }
                    : new RequestHttpResponse<KhamSucKhoeChuyenKhoaModel> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return IBaseGetService<KhamSucKhoeChuyenKhoaModel>.CreateErrorResponse<KhamSucKhoeChuyenKhoaModel>(ex);
            }
        }
    }
}