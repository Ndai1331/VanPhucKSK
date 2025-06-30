using CoreAdminWeb.Model.RequestHttps;
using CoreAdminWeb.Model.Settings;
using CoreAdminWeb.Services.BaseServices;
using CoreAdminWeb.RequestHttp;
using System.Net;
using CoreAdminWeb.Model;

namespace CoreAdminWeb.Services
{
    /// <summary>
    /// Service for managing fertilizer production facilities
    /// </summary>
    public class KhamSucKhoeTienSuService : IBaseGetService<KhamSucKhoeTienSuModel>
    {
        private readonly string _collection = "kham_suc_khoe_tien_su";
        private const string Fields = "*,user_created.last_name,user_created.first_name,user_updated.last_name,user_updated.first_name";

        /// <summary>
        /// Gets all fertilizer production facilities
        /// </summary>
        public async Task<RequestHttpResponse<List<KhamSucKhoeTienSuModel>>> GetAllAsync(string query)
        {
            try
            {
                string url = $"items/{_collection}?fields={Fields}&{query}";
                var response = await RequestClient.GetAPIAsync<RequestHttpResponse<List<KhamSucKhoeTienSuModel>>>(url);
                
                return response.IsSuccess 
                    ? new RequestHttpResponse<List<KhamSucKhoeTienSuModel>> { Data = response.Data.Data, Meta = response.Data.Meta }
                    : new RequestHttpResponse<List<KhamSucKhoeTienSuModel>> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return IBaseGetService<KhamSucKhoeTienSuModel>.CreateErrorResponse<List<KhamSucKhoeTienSuModel>>(ex);
            }
        }

        /// <summary>
        /// Gets a fertilizer production facility by ID
        /// </summary>
        public async Task<RequestHttpResponse<KhamSucKhoeTienSuModel>> GetByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new RequestHttpResponse<KhamSucKhoeTienSuModel>
                {
                    Errors = new List<ErrorResponse> { new() { Message = "ID không được để trống" } },
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            try
            {
                var response = await RequestClient.GetAPIAsync<RequestHttpResponse<KhamSucKhoeTienSuModel>>($"items/{_collection}/{id}?fields={Fields}");
                
                return response.IsSuccess
                    ? new RequestHttpResponse<KhamSucKhoeTienSuModel> { Data = response.Data.Data, Meta = response.Data.Meta }
                    : new RequestHttpResponse<KhamSucKhoeTienSuModel> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return IBaseGetService<KhamSucKhoeTienSuModel>.CreateErrorResponse<KhamSucKhoeTienSuModel>(ex);
            }
        }
    }
}