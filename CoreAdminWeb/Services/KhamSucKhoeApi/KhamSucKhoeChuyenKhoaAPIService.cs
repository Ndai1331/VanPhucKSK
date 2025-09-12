using CoreAdminWeb.Model.RequestHttps;
using CoreAdminWeb.Model;
using CoreAdminWeb.Http;
using CoreAdminWeb.Model.KhamSucKhoes;

namespace CoreAdminWeb.Services.KhamSucKhoeApi
{
    public class KhamSucKhoeChuyenKhoaAPIService: IKhamSucKhoeAPIService<KhamSucKhoeChuyenKhoaModel>
    {
        public async Task<RequestHttpResponse<List<KhamSucKhoeChuyenKhoaModel>>> GetAllAsync(string query)
        {
            try
            {
                var response = await LocalRequestClientService.GetAPIAsync<RequestHttpResponse<List<KhamSucKhoeChuyenKhoaModel>>>(query);

                return response.IsSuccess
                    ? new RequestHttpResponse<List<KhamSucKhoeChuyenKhoaModel>> { Data = response.Data?.Data, Meta = response.Data?.Meta }
                    : new RequestHttpResponse<List<KhamSucKhoeChuyenKhoaModel>> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<List<KhamSucKhoeChuyenKhoaModel>>(ex);
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
