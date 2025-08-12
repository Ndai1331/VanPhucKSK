using CoreAdminWeb.Model.Menus;
using CoreAdminWeb.Model.RequestHttps;
using CoreAdminWeb.Http;
using CoreAdminWeb.Services.Http;

namespace CoreAdminWeb.Services.Menus
{
    public interface IMenuService
    {
        Task<RequestHttpResponse<List<MenuResponse>>> GetMenusAsync();
    }

    public class MenuService : IMenuService
    {
        private readonly IHttpClientService _httpClientService;

        public MenuService(IHttpClientService httpClientService)
        {
            _httpClientService = httpClientService;
        }

        public async Task<RequestHttpResponse<List<MenuResponse>>> GetMenusAsync()
        {
            string url = $"items/Menu?fields=id,status,sort,code,name,parent_id,icon,url&sort=sort";
            var response = new RequestHttpResponse<List<MenuResponse>>();
            try
            {
                var result = await _httpClientService.GetAPIAsync<RequestHttpResponse<List<MenuResponse>>>(url);
                if (result.IsSuccess && result.Data != null)
                {
                    response.Data = result.Data.Data ?? new List<MenuResponse>();
                }else{
                    response.Errors = result.Errors;
                }
            }
            catch (Exception ex)
            {
                response.Errors = new List<ErrorResponse> { new ErrorResponse { Message = ex.Message } };
            }
            return response;
        }
    }
} 