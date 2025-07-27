using CoreAdminWeb.Services.BaseServices;

namespace CoreAdminWeb.Helpers
{
    public static class BaseServiceHelper
    {
        public static async Task<TModel> GetFirstOrDefaultAsync<TModel>(IBaseDetailService<TModel> service, string? query = default)
            where TModel : class, new()
        {
            var res = await service.GetAllAsync(query ?? string.Empty);
            return res?.IsSuccess == true && res.Data != null
                ? res.Data.FirstOrDefault() ?? new TModel()
                : new TModel();
        }
    }
}
