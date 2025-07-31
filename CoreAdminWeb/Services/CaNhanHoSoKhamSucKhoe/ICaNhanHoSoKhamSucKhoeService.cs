using CoreAdminWeb.Model.RequestHttps;

namespace CoreAdminWeb.Services.ICaNhanSoKhamSucKhoeService
{
    public interface ICaNhanSoKhamSucKhoeService<T>
    {
        Task<RequestHttpResponse<List<T>>> GetAllAsync(string query);
    }
}
