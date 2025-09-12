using CoreAdminWeb.Model.RequestHttps;
using CoreAdminWeb.Model;
using CoreAdminWeb.Http;
using CoreAdminWeb.Model.KhamSucKhoes;

namespace CoreAdminWeb.Services.KhamSucKhoeApi
{
    public class KhamSucKhoeKetLuanAPIService: IKhamSucKhoeAPIService<KhamSucKhoeKetLuanModel>
    {
        public async Task<RequestHttpResponse<List<KhamSucKhoeKetLuanModel>>> GetAllAsync(string query)
        {
            try
            {
                var response = await LocalRequestClientService.GetAPIAsync<RequestHttpResponse<List<KhamSucKhoeKetLuanModel>>>(query);

                return response.IsSuccess
                    ? new RequestHttpResponse<List<KhamSucKhoeKetLuanModel>> { Data = response.Data?.Data, Meta = response.Data?.Meta }
                    : new RequestHttpResponse<List<KhamSucKhoeKetLuanModel>> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<List<KhamSucKhoeKetLuanModel>>(ex);
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
