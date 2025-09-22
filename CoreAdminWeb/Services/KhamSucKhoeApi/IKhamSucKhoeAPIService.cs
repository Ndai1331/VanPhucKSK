using CoreAdminWeb.Model.RequestHttps;

namespace CoreAdminWeb.Services.KhamSucKhoeApi
{
    public interface IKhamSucKhoeAPIService<T>
    {
        Task<RequestHttpResponse<List<T>>> GetAllAsync(string query);
    }
}