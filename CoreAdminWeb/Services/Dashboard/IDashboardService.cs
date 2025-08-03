using CoreAdminWeb.Model.RequestHttps;

namespace CoreAdminWeb.Services.IDashboardService
{
    public interface IDashboardService<T>
    {
        Task<RequestHttpResponse<T>> GeDataAsync(string query);
    }
}
