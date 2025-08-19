using CoreAdminWeb.Model.RequestHttps;

namespace CoreAdminWeb.Services.Reports
{
    public interface IReportService<T>
    {
        Task<RequestHttpResponse<List<T>>> GeDataAsync(string query);
    }
}
