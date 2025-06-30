using CoreAdminWeb.Model.RequestHttps;
using CoreAdminWeb.RequestHttp;
using CoreAdminWeb.Model;
using CoreAdminWeb.Model.Reports;

namespace CoreAdminWeb.Services.Reports
{
    public class ReportBaoCaoKiemTraHauKiemLayMauATTPService : IReportService<ReportBaoCaoKiemTraHauKiemLayMauATTPModel>
    {
       
        public async Task<RequestHttpResponse<List<ReportBaoCaoKiemTraHauKiemLayMauATTPModel>>> GetAllAsync(string query)
        {
            try
            {
                var response = await ReportRequestClient.GetAPIAsync<RequestHttpResponse<List<ReportBaoCaoKiemTraHauKiemLayMauATTPModel>>>(query);

                return response.IsSuccess
                    ? new RequestHttpResponse<List<ReportBaoCaoKiemTraHauKiemLayMauATTPModel>> { Data = response.Data?.Data, Meta = response.Data?.Meta }
                    : new RequestHttpResponse<List<ReportBaoCaoKiemTraHauKiemLayMauATTPModel>> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<List<ReportBaoCaoKiemTraHauKiemLayMauATTPModel>>(ex);
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
