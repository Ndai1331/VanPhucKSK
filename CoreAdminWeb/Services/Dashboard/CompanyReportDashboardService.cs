using CoreAdminWeb.Http;
using CoreAdminWeb.Model.Dashboard.General;
using CoreAdminWeb.Model.RequestHttps;
using CoreAdminWeb.Services.IDashboardService;

namespace CoreAdminWeb.Services.Dashboard
{
    public class CompanyReportDashboardService : IDashboardService<CompanyReportDashboardModel>
    {
        public async Task<RequestHttpResponse<CompanyReportDashboardModel>> GeDataAsync(string query)
        {
            try
            {
                var response = await LocalRequestClientService.GetAPIAsync<RequestHttpResponse<CompanyReportDashboardModel>>(query);

                return response.IsSuccess
                    ? new RequestHttpResponse<CompanyReportDashboardModel> { Data = response.Data?.Data, Meta = response.Data?.Meta }
                    : new RequestHttpResponse<CompanyReportDashboardModel> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<CompanyReportDashboardModel>(ex);
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
