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
    public class ContractService : IBaseGetService<ContractModel>
    {
        private readonly string _collection = "contract";
        private const string Fields = "*,user_created.last_name,user_created.first_name,user_updated.last_name,user_updated.first_name";

        /// <summary>
        /// Gets all fertilizer production facilities
        /// </summary>
        public async Task<RequestHttpResponse<List<ContractModel>>> GetAllAsync(string query, bool isPublic = false)
        {
            try
            {
                string url = $"items/{_collection}?fields={Fields}&{query}";
                var response = isPublic ? await PublicRequestClient.GetAPIAsync<RequestHttpResponse<List<ContractModel>>>(url) : await RequestClient.GetAPIAsync<RequestHttpResponse<List<ContractModel>>>(url);

                return response.IsSuccess
                    ? new RequestHttpResponse<List<ContractModel>> { Data = response.Data?.Data, Meta = response.Data?.Meta }
                    : new RequestHttpResponse<List<ContractModel>> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return IBaseGetService<ContractModel>.CreateErrorResponse<List<ContractModel>>(ex);
            }
        }

        /// <summary>
        /// Gets a fertilizer production facility by ID
        /// </summary>
        public async Task<RequestHttpResponse<ContractModel>> GetByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new RequestHttpResponse<ContractModel>
                {
                    Errors = new List<ErrorResponse> { new() { Message = "ID không được để trống" } },
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            try
            {
                var response = await RequestClient.GetAPIAsync<RequestHttpResponse<ContractModel>>($"items/{_collection}/{id}?fields={Fields}");

                return response.IsSuccess
                    ? new RequestHttpResponse<ContractModel> { Data = response.Data?.Data, Meta = response.Data?.Meta }
                    : new RequestHttpResponse<ContractModel> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return IBaseGetService<ContractModel>.CreateErrorResponse<ContractModel>(ex);
            }
        }
    }
}