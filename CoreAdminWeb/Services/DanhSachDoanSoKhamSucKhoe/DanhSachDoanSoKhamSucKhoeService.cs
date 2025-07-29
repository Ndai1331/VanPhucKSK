using CoreAdminWeb.Model.RequestHttps;
using CoreAdminWeb.Model;
using CoreAdminWeb.Http;
using CoreAdminWeb.Services.IDanhSachDoanSoKhamSucKhoeService;

namespace CoreAdminWeb.Services.DanhSachDoanSoKhamSucKhoe
{
    public class DanhSachDoanSoKhamSucKhoeService: IDanhSachDoanSoKhamSucKhoeService<DanhSachDoanSoKhamSucKhoeModel>
    {
        public async Task<RequestHttpResponse<List<DanhSachDoanSoKhamSucKhoeModel>>> GetAllAsync(string query)
        {
            try
            {
                var response = await LocalRequestClientService.GetAPIAsync<RequestHttpResponse<List<DanhSachDoanSoKhamSucKhoeModel>>>(query);

                return response.IsSuccess
                    ? new RequestHttpResponse<List<DanhSachDoanSoKhamSucKhoeModel>> { Data = response.Data?.Data, Meta = response.Data?.Meta }
                    : new RequestHttpResponse<List<DanhSachDoanSoKhamSucKhoeModel>> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<List<DanhSachDoanSoKhamSucKhoeModel>>(ex);
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
