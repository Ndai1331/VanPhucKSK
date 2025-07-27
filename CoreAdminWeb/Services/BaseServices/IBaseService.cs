using CoreAdminWeb.Model.RequestHttps;
using System.Net;

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

    public interface IBaseGetService<T>
    {
        Task<RequestHttpResponse<List<T>>> GetAllAsync(string query, bool isPublic = false);
        Task<RequestHttpResponse<T>> GetByIdAsync(string id);

        public static RequestHttpResponse<TItem> CreateErrorResponse<TItem>(Exception ex)
        {
            return new RequestHttpResponse<TItem>
            {
                Errors = new List<ErrorResponse> { new() { Message = ex.Message } },
                StatusCode = HttpStatusCode.InternalServerError
            };
        }
    }

    public interface IBaseDetailService<T> : IBaseGetService<T>
    {
        Task<RequestHttpResponse<List<T>>> CreateAsync(List<T> model);
        Task<RequestHttpResponse<bool>> UpdateAsync(List<T> model);
        Task<RequestHttpResponse<bool>> DeleteAsync(List<T> model);
    }
    public interface IBaseAllService<T> : IBaseService<T>
    {
        Task<RequestHttpResponse<List<T>>> CreateAsync(List<T> model);
        Task<RequestHttpResponse<bool>> UpdateAsync(List<T> model);
        Task<RequestHttpResponse<bool>> DeleteAsync(List<T> model);
    }
}