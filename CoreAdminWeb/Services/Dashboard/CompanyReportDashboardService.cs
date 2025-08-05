using CoreAdminWeb.Http;
using CoreAdminWeb.Model.Dashboard.General;
using CoreAdminWeb.Model.RequestHttps;
using CoreAdminWeb.Services.IDashboardService;

namespace CoreAdminWeb.Services.Dashboard
{
    public class CompanyReportDashboardService : IDashboardService<CompanySummaryReportDashboardModel>
    {
        public async Task<RequestHttpResponse<CompanySummaryReportDashboardModel>> GeDataAsync(string query)
        {
            try
            {
                var response = await LocalRequestClientService.GetAPIAsync<RequestHttpResponse<CompanySummaryReportDashboardModel>>(query);

                return response.IsSuccess
                    ? new RequestHttpResponse<CompanySummaryReportDashboardModel> { Data = response.Data?.Data, Meta = response.Data?.Meta }
                    : new RequestHttpResponse<CompanySummaryReportDashboardModel> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<CompanySummaryReportDashboardModel>(ex);
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
