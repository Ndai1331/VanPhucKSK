using CoreAdminWeb.Model.RequestHttps;
using CoreAdminWeb.RequestHttp;
using CoreAdminWeb.Model;

namespace CoreAdminWeb.Services.Reports
{
    public class ReportBaoCaoChiTietKiemTraHauKiemATTPService : IReportService<QLCLCoSoNLTSDuDieuKienATTPModel>
    {
       
        public async Task<RequestHttpResponse<List<QLCLCoSoNLTSDuDieuKienATTPModel>>> GetAllAsync(string query)
        {
            try
            {
                var response = await ReportRequestClient.GetAPIAsync<RequestHttpResponse<List<QLCLCoSoNLTSDuDieuKienATTPModel>>>(query);

                return response.IsSuccess
                    ? new RequestHttpResponse<List<QLCLCoSoNLTSDuDieuKienATTPModel>> { Data = response.Data?.Data, Meta = response.Data?.Meta }
                    : new RequestHttpResponse<List<QLCLCoSoNLTSDuDieuKienATTPModel>> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<List<QLCLCoSoNLTSDuDieuKienATTPModel>>(ex);
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
