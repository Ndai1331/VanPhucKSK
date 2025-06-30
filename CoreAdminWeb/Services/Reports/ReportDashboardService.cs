using CoreAdminWeb.Model.RequestHttps;
using CoreAdminWeb.RequestHttp;
using CoreAdminWeb.Model.Reports;

namespace CoreAdminWeb.Services.Reports
{
    public class ReportDashboardService : IReportService<ReportDashboardModel>
    {
       
        public async Task<RequestHttpResponse<List<ReportDashboardModel>>> GetAllAsync(string query)
        {
            try
            {
                var response = await ReportRequestClient.GetAPIAsync<RequestHttpResponse<List<ReportDashboardModel>>>(query);

                return response.IsSuccess
                    ? new RequestHttpResponse<List<ReportDashboardModel>> { Data = response.Data?.Data, Meta = response.Data?.Meta }
                    : new RequestHttpResponse<List<ReportDashboardModel>> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<List<ReportDashboardModel>>(ex);
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
