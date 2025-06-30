using CoreAdminWeb.Model.RequestHttps;
using CoreAdminWeb.RequestHttp;
using CoreAdminWeb.Model.Reports;

namespace CoreAdminWeb.Services.Reports
{
    public class ReportBaoCaoKiemTraHauKiemATTPService : IReportService<ReportBaoCaoKiemTraHauKiemATTPModel>
    {
       
        public async Task<RequestHttpResponse<List<ReportBaoCaoKiemTraHauKiemATTPModel>>> GetAllAsync(string query)
        {
            try
            {
                var response = await ReportRequestClient.GetAPIAsync<RequestHttpResponse<List<ReportBaoCaoKiemTraHauKiemATTPModel>>>(query);

                return response.IsSuccess
                    ? new RequestHttpResponse<List<ReportBaoCaoKiemTraHauKiemATTPModel>> { Data = response.Data?.Data, Meta = response.Data?.Meta }
                    : new RequestHttpResponse<List<ReportBaoCaoKiemTraHauKiemATTPModel>> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<List<ReportBaoCaoKiemTraHauKiemATTPModel>>(ex);
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
