using CoreAdminWeb.Http;
using CoreAdminWeb.Model.KhamSucKhoes;
using CoreAdminWeb.Model.RequestHttps;

namespace CoreAdminWeb.Services.KhamSucKhoeApi
{
    public class KhamSucKhoeKQCLSAPIService : IKhamSucKhoeAPIService<KetQuaCLSChiTietModel>
    {
        public async Task<RequestHttpResponse<List<KetQuaCLSChiTietModel>>> GetAllAsync(string query)
        {
            try
            {
                var response = await LocalRequestClientService.GetAPIAsync<RequestHttpResponse<List<KetQuaCLSChiTietModel>>>(query);

                return response.IsSuccess
                    ? new RequestHttpResponse<List<KetQuaCLSChiTietModel>> { Data = response.Data?.Data, Meta = response.Data?.Meta }
                    : new RequestHttpResponse<List<KetQuaCLSChiTietModel>> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<List<KetQuaCLSChiTietModel>>(ex);
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
