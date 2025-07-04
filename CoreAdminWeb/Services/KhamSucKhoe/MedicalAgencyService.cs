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
    public class MedicalAgencyService : IBaseGetService<MedicalAgencyModel>
    {
        private readonly string _collection = "medical_agency";
        private const string Fields = "*,user_created.last_name,user_created.first_name,user_updated.last_name,user_updated.first_name";

        /// <summary>
        /// Gets all fertilizer production facilities
        /// </summary>
        public async Task<RequestHttpResponse<List<MedicalAgencyModel>>> GetAllAsync(string query, bool isPublic = false)
        {
            try
            {
                string url = $"items/{_collection}?fields={Fields}&{query}";
                var response = isPublic ? await PublicRequestClient.GetAPIAsync<RequestHttpResponse<List<MedicalAgencyModel>>>(url) : await RequestClient.GetAPIAsync<RequestHttpResponse<List<MedicalAgencyModel>>>(url);

                return response.IsSuccess
                    ? new RequestHttpResponse<List<MedicalAgencyModel>> { Data = response.Data?.Data, Meta = response.Data?.Meta }
                    : new RequestHttpResponse<List<MedicalAgencyModel>> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return IBaseGetService<MedicalAgencyModel>.CreateErrorResponse<List<MedicalAgencyModel>>(ex);
            }
        }

        /// <summary>
        /// Gets a fertilizer production facility by ID
        /// </summary>
        public async Task<RequestHttpResponse<MedicalAgencyModel>> GetByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new RequestHttpResponse<MedicalAgencyModel>
                {
                    Errors = new List<ErrorResponse> { new() { Message = "ID không được để trống" } },
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            try
            {
                var response = await RequestClient.GetAPIAsync<RequestHttpResponse<MedicalAgencyModel>>($"items/{_collection}/{id}?fields={Fields}");

                return response.IsSuccess
                    ? new RequestHttpResponse<MedicalAgencyModel> { Data = response.Data?.Data, Meta = response.Data?.Meta }
                    : new RequestHttpResponse<MedicalAgencyModel> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return IBaseGetService<MedicalAgencyModel>.CreateErrorResponse<MedicalAgencyModel>(ex);
            }
        }
    }
}