using CoreAdminWeb.Model.RequestHttps;

namespace CoreAdminWeb.Services.BaseServices
{
    public interface IBaseService<T>
    {
        Task<RequestHttpResponse<List<T>>> GetAllAsync(string query);
        Task<RequestHttpResponse<T>> GetByIdAsync(string id);
        Task<RequestHttpResponse<T>> CreateAsync(T model);
        Task<RequestHttpResponse<bool>> UpdateAsync(T model);
        Task<RequestHttpResponse<bool>> DeleteAsync(T model);
    }
} 