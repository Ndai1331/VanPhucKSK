using CoreAdminWeb.Model.RequestHttps;

namespace CoreAdminWeb.Services.IDanhSachDoanSoKhamSucKhoeService
{
    public interface IDanhSachDoanSoKhamSucKhoeService<T>
    {
        Task<RequestHttpResponse<List<T>>> GetAllAsync(string query);
    }
}
