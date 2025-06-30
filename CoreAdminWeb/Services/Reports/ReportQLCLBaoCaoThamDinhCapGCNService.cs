using CoreAdminWeb.Model.RequestHttps;
using CoreAdminWeb.RequestHttp;
using CoreAdminWeb.Model.Reports;

namespace CoreAdminWeb.Services.Reports
{
    public class ReportBaoCaoThamDinhCapGCNService : IReportService<ReportBaoCaoThamDinhCapGCNModel>
    {
       
        public async Task<RequestHttpResponse<List<ReportBaoCaoThamDinhCapGCNModel>>> GetAllAsync(string query)
        {
            try
            {
                var response = await ReportRequestClient.GetAPIAsync<RequestHttpResponse<List<ReportBaoCaoThamDinhCapGCNModel>>>(query);

                return response.IsSuccess
                    ? new RequestHttpResponse<List<ReportBaoCaoThamDinhCapGCNModel>> { Data = response.Data?.Data, Meta = response.Data?.Meta }
                    : new RequestHttpResponse<List<ReportBaoCaoThamDinhCapGCNModel>> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<List<ReportBaoCaoThamDinhCapGCNModel>>(ex);
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
