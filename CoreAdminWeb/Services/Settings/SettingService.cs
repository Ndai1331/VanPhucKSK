using CoreAdminWeb.Http;
using CoreAdminWeb.Model.RequestHttps;
using CoreAdminWeb.Model.Settings;
namespace CoreAdminWeb.Services.Settings
{
    public interface ISettingService
    {
        Task<RequestHttpResponse<SettingModel>> GetCurrentSettingAsync();
    }

    public class SettingService : ISettingService
    {
        private readonly string _collection = "settings";
        private const string Fields = "*"
            + ",phieu_ksk_nam.id,phieu_ksk_nam.filename_disk,phieu_ksk_nam.filename_download"
            + ",phieu_ksk_nu.id,phieu_ksk_nu.filename_disk,phieu_ksk_nu.filename_download"
            + ",thcn_nu.id,thcn_nu.filename_disk,thcn_nu.filename_download"
            + ",thcn_nam.id,thcn_nam.filename_disk,thcn_nam.filename_download";

        public async Task<RequestHttpResponse<SettingModel>> GetCurrentSettingAsync()
        {
            var response = new RequestHttpResponse<SettingModel>();
            try
            {
                var result = await PublicRequestClient.GetAPIAsync<RequestHttpResponse<SettingModel>>($"{_collection}?fields={Fields}");

                if (result.IsSuccess)
                {
                    response.Data = result.Data?.Data;
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