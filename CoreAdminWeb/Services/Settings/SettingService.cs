using CoreAdminWeb.Model.RequestHttps;
using CoreAdminWeb.Model.Settings;
using CoreAdminWeb.Services.Http;
using CoreAdminWeb.Http;
namespace CoreAdminWeb.Services.Settings
{
    public interface ISettingService
    {
        Task<RequestHttpResponse<SettingModel>> GetCurrentSettingAsync();
    }

    public class SettingService : ISettingService
    {
        private readonly IHttpClientService _httpClientService;

        public SettingService(IHttpClientService httpClientService)
        {
            _httpClientService = httpClientService;
        }

        public async Task<RequestHttpResponse<SettingModel>> GetCurrentSettingAsync()
        {
            var response = new RequestHttpResponse<SettingModel>();
            try
            {
                var result = await PublicRequestClient.GetAPIAsync<RequestHttpResponse<SettingModel>>("settings");

                if (result.IsSuccess)
                {
                    response.Data = result.Data.Data;
                }
                else
                {
                    response.Errors = result.Errors;
                }
            }
            catch (Exception ex)
            {
                response.Errors = new List<ErrorResponse> { new ErrorResponse { Message = ex.Message } };
            }
            return response;
        }
    }
} 