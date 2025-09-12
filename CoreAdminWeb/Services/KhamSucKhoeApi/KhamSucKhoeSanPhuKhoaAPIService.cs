using CoreAdminWeb.Model.RequestHttps;
using CoreAdminWeb.Model;
using CoreAdminWeb.Http;
using CoreAdminWeb.Model.KhamSucKhoes;

namespace CoreAdminWeb.Services.KhamSucKhoeApi
{
    public class KhamSucKhoeSanPhuKhoaAPIService: IKhamSucKhoeAPIService<KhamSucKhoeSanPhuKhoaModel>
    {
        public async Task<RequestHttpResponse<List<KhamSucKhoeSanPhuKhoaModel>>> GetAllAsync(string query)
        {
            try
            {
                var response = await LocalRequestClientService.GetAPIAsync<RequestHttpResponse<List<KhamSucKhoeSanPhuKhoaModel>>>(query);

                return response.IsSuccess
                    ? new RequestHttpResponse<List<KhamSucKhoeSanPhuKhoaModel>> { Data = response.Data?.Data, Meta = response.Data?.Meta }
                    : new RequestHttpResponse<List<KhamSucKhoeSanPhuKhoaModel>> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<List<KhamSucKhoeSanPhuKhoaModel>>(ex);
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
