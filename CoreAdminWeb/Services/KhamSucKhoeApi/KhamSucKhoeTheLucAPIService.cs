using CoreAdminWeb.Model.RequestHttps;
using CoreAdminWeb.Model;
using CoreAdminWeb.Http;
using CoreAdminWeb.Model.KhamSucKhoes;

namespace CoreAdminWeb.Services.KhamSucKhoeApi
{
    public class KhamSucKhoeTheLucAPIService: IKhamSucKhoeAPIService<KhamSucKhoeTheLucModel>
    {
        public async Task<RequestHttpResponse<List<KhamSucKhoeTheLucModel>>> GetAllAsync(string query)
        {
            try
            {
                var response = await LocalRequestClientService.GetAPIAsync<RequestHttpResponse<List<KhamSucKhoeTheLucModel>>>(query);

                return response.IsSuccess
                    ? new RequestHttpResponse<List<KhamSucKhoeTheLucModel>> { Data = response.Data?.Data, Meta = response.Data?.Meta }
                    : new RequestHttpResponse<List<KhamSucKhoeTheLucModel>> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<List<KhamSucKhoeTheLucModel>>(ex);
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
