using CoreAdminWeb.Http;
using CoreAdminWeb.Model.KhamSucKhoes;
using CoreAdminWeb.Model.RequestHttps;

namespace CoreAdminWeb.Services.KhamSucKhoeApi
{
    public class KhamSucKhoeCongTyAPIService : IKhamSucKhoeAPIService<KhamSucKhoeCongTyModel>
    {
        public async Task<RequestHttpResponse<List<KhamSucKhoeCongTyModel>>> GetAllAsync(string query)
        {
            try
            {
                var response = await LocalRequestClientService.GetAPIAsync<RequestHttpResponse<List<KhamSucKhoeCongTyModel>>>(query);

                return response.IsSuccess
                    ? new RequestHttpResponse<List<KhamSucKhoeCongTyModel>> { Data = response.Data?.Data, Meta = response.Data?.Meta }
                    : new RequestHttpResponse<List<KhamSucKhoeCongTyModel>> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<List<KhamSucKhoeCongTyModel>>(ex);
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
