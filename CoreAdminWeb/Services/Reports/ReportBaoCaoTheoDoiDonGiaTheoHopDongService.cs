using CoreAdminWeb.Http;
using CoreAdminWeb.Model.Reports;
using CoreAdminWeb.Model.RequestHttps;

namespace CoreAdminWeb.Services.Reports
{
    public class ReportBaoCaoTheoDoiDonGiaTheoHopDongService : IReportService<ReportBaoCaoTheoDoiDonGiaTheoHopDongModel>
    {
        public async Task<RequestHttpResponse<List<ReportBaoCaoTheoDoiDonGiaTheoHopDongModel>>> GeDataAsync(string query)
        {
            try
            {
                var response = await LocalRequestClientService.GetAPIAsync<RequestHttpResponse<List<ReportBaoCaoTheoDoiDonGiaTheoHopDongModel>>>(query);

                return response.IsSuccess
                    ? new RequestHttpResponse<List<ReportBaoCaoTheoDoiDonGiaTheoHopDongModel>> { Data = response.Data?.Data, Meta = response.Data?.Meta }
                    : new RequestHttpResponse<List<ReportBaoCaoTheoDoiDonGiaTheoHopDongModel>> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<List<ReportBaoCaoTheoDoiDonGiaTheoHopDongModel>>(ex);
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
