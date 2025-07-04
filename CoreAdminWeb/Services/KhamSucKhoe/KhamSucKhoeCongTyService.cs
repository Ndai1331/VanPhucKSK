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
    public class KhamSucKhoeCongTyService : IBaseGetService<KhamSucKhoeCongTyModel>
    {
        private readonly string _collection = "kham_suc_khoe_cong_ty";
        private const string Fields = "*,user_created.last_name,user_created.first_name,user_updated.last_name,user_updated.first_name";

        /// <summary>
        /// Gets all fertilizer production facilities
        /// </summary>
        public async Task<RequestHttpResponse<List<KhamSucKhoeCongTyModel>>> GetAllAsync(string query, bool isPublic = false)
        {
            try
            {
                string url = $"items/{_collection}?fields={Fields}&{query}";
                var response = isPublic ? await PublicRequestClient.GetAPIAsync<RequestHttpResponse<List<KhamSucKhoeCongTyModel>>>(url) : await RequestClient.GetAPIAsync<RequestHttpResponse<List<KhamSucKhoeCongTyModel>>>(url);

                return response.IsSuccess
                    ? new RequestHttpResponse<List<KhamSucKhoeCongTyModel>> { Data = response.Data?.Data, Meta = response.Data?.Meta }
                    : new RequestHttpResponse<List<KhamSucKhoeCongTyModel>> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return IBaseGetService<KhamSucKhoeCongTyModel>.CreateErrorResponse<List<KhamSucKhoeCongTyModel>>(ex);
            }
        }

        /// <summary>
        /// Gets a fertilizer production facility by ID
        /// </summary>
        public async Task<RequestHttpResponse<KhamSucKhoeCongTyModel>> GetByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new RequestHttpResponse<KhamSucKhoeCongTyModel>
                {
                    Errors = new List<ErrorResponse> { new() { Message = "ID không được để trống" } },
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            try
            {
                var response = await RequestClient.GetAPIAsync<RequestHttpResponse<KhamSucKhoeCongTyModel>>($"items/{_collection}/{id}?fields={Fields}");

                return response.IsSuccess
                    ? new RequestHttpResponse<KhamSucKhoeCongTyModel> { Data = response.Data?.Data, Meta = response.Data?.Meta }
                    : new RequestHttpResponse<KhamSucKhoeCongTyModel> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return IBaseGetService<KhamSucKhoeCongTyModel>.CreateErrorResponse<KhamSucKhoeCongTyModel>(ex);
            }
        }
    }
}