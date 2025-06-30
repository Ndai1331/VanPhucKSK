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
    public class KhamSucKhoeNgheNghiepService : IBaseGetService<KhamSucKhoeNgheNghiepModel>
    {
        private readonly string _collection = "kham_suc_khoe_nghe_nghiep";
        private const string Fields = "*,user_created.last_name,user_created.first_name,user_updated.last_name,user_updated.first_name";

        /// <summary>
        /// Gets all fertilizer production facilities
        /// </summary>
        public async Task<RequestHttpResponse<List<KhamSucKhoeNgheNghiepModel>>> GetAllAsync(string query)
        {
            try
            {
                string url = $"items/{_collection}?fields={Fields}&{query}";
                var response = await RequestClient.GetAPIAsync<RequestHttpResponse<List<KhamSucKhoeNgheNghiepModel>>>(url);
                
                return response.IsSuccess 
                    ? new RequestHttpResponse<List<KhamSucKhoeNgheNghiepModel>> { Data = response.Data.Data, Meta = response.Data.Meta }
                    : new RequestHttpResponse<List<KhamSucKhoeNgheNghiepModel>> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return IBaseGetService<KhamSucKhoeNgheNghiepModel>.CreateErrorResponse<List<KhamSucKhoeNgheNghiepModel>>(ex);
            }
        }

        /// <summary>
        /// Gets a fertilizer production facility by ID
        /// </summary>
        public async Task<RequestHttpResponse<KhamSucKhoeNgheNghiepModel>> GetByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new RequestHttpResponse<KhamSucKhoeNgheNghiepModel>
                {
                    Errors = new List<ErrorResponse> { new() { Message = "ID không được để trống" } },
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            try
            {
                var response = await RequestClient.GetAPIAsync<RequestHttpResponse<KhamSucKhoeNgheNghiepModel>>($"items/{_collection}/{id}?fields={Fields}");
                
                return response.IsSuccess
                    ? new RequestHttpResponse<KhamSucKhoeNgheNghiepModel> { Data = response.Data.Data, Meta = response.Data.Meta }
                    : new RequestHttpResponse<KhamSucKhoeNgheNghiepModel> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return IBaseGetService<KhamSucKhoeNgheNghiepModel>.CreateErrorResponse<KhamSucKhoeNgheNghiepModel>(ex);
            }
        }
    }
}