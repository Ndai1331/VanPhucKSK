using CoreAdminWeb.Model.RequestHttps;
using CoreAdminWeb.Model;
using CoreAdminWeb.Http;
using CoreAdminWeb.Services.ICaNhanSoKhamSucKhoeService;

namespace CoreAdminWeb.Services.CaNhanSoKhamSucKhoe
{
    public class CaNhanSoKhamSucKhoeService: ICaNhanSoKhamSucKhoeService<HoSoKhamSucKhoeTT32Model>
    {
        public async Task<RequestHttpResponse<List<HoSoKhamSucKhoeTT32Model>>> GetAllAsync(string query)
        {
            try
            {
                var response = await LocalRequestClientService.GetAPIAsync<RequestHttpResponse<List<HoSoKhamSucKhoeTT32Model>>>(query);

                return response.IsSuccess
                    ? new RequestHttpResponse<List<HoSoKhamSucKhoeTT32Model>> { Data = response.Data?.Data, Meta = response.Data?.Meta }
                    : new RequestHttpResponse<List<HoSoKhamSucKhoeTT32Model>> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<List<HoSoKhamSucKhoeTT32Model>>(ex);
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
