using CoreAdminWeb.Http;
using CoreAdminWeb.Model.Dashboard.General;
using CoreAdminWeb.Model.RequestHttps;
using CoreAdminWeb.Services.IDashboardService;

namespace CoreAdminWeb.Services.Dashboard
{
    public class DashboardService : IDashboardService<GeneralDashboardModel>
    {
        public async Task<RequestHttpResponse<GeneralDashboardModel>> GetInfomationAsync(string query)
        {
            try
            {
                var response = await LocalRequestClientService.GetAPIAsync<RequestHttpResponse<GeneralDashboardModel>>(query);

                return response.IsSuccess
                    ? new RequestHttpResponse<GeneralDashboardModel> { Data = response.Data?.Data, Meta = response.Data?.Meta }
                    : new RequestHttpResponse<GeneralDashboardModel> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<GeneralDashboardModel>(ex);
            }
        }
        private static RequestHttpResponse<T> CreateErrorResponse<T>(Exception ex)
        {
            return new RequestHttpResponse<T>
            {
                Errors = new List<ErrorResponse>
                {
                    new()
                    {
                        Message = ex.Message,
                        Code = "500"
                    }
                }
            };
        }
    }
}
