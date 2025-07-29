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

        public static async Task<TModel> GetFirstOrDefaultAsync<TModel>(IBaseService<TModel> service, string? query = default)
            where TModel : class, new()
        {
            var res = await service.GetAllAsync(query ?? string.Empty);
            return res?.IsSuccess == true && res.Data != null
                ? res.Data.FirstOrDefault() ?? new TModel()
                : new TModel();
        }


        public static async Task LoadSingleRecordAsync<T>(IBaseGetService<T> service, string query, Action<T?> setResult) where T : class
        {
            try
            {
                var result = await service.GetAllAsync(query);
                if (result?.IsSuccess == true && result.Data != null)
                {
                    setResult(result.Data.FirstOrDefault());
                }
                else
                {
                    setResult(null);
                }
            }
            catch
            {
                setResult(null);
            }
        }

        public static async Task LoadSingleRecordAsync<T>(IBaseService<T> service, string query, Action<T?> setResult) where T : class
        {
            try
            {
                var result = await service.GetAllAsync(query);
                if (result?.IsSuccess == true && result.Data != null)
                {
                    setResult(result.Data.FirstOrDefault());
                }
                else
                {
                    setResult(null);
                }
            }
            catch
            {
                setResult(null);
            }
        }

        public static async Task LoadSingleRecordAsync<T>(IBaseDetailService<T> service, string query, Action<T?> setResult) where T : class
        {
            try
            {
                var result = await service.GetAllAsync(query);
                if (result?.IsSuccess == true && result.Data != null)
                {
                    setResult(result.Data.FirstOrDefault());
                }
                else
                {
                    setResult(null);
                }
            }
            catch
            {
                setResult(null);
            }
        }


        public static async Task LoadMultipleRecordAsync<T>(IBaseGetService<T> service, string query, Action<List<T>?> setResult) where T : class
        {
            try
            {
                var result = await service.GetAllAsync(query);
                if (result?.IsSuccess == true && result.Data != null)
                {
                    setResult(result.Data);
                }
                else
                {
                    setResult(null);
                }
            }
            catch
            {
                setResult(null);
            }
        }

        public static async Task LoadMultipleRecordAsync<T>(IBaseService<T> service, string query, Action<List<T>?> setResult) where T : class
        {
            try
            {
                var result = await service.GetAllAsync(query);
                if (result?.IsSuccess == true && result.Data != null)
                {
                    setResult(result.Data);
                }
                else
                {
                    setResult(null);
                }
            }
            catch
            {
                setResult(null);
            }
        }

        public static async Task LoadMultipleRecordAsync<T>(IBaseDetailService<T> service, string query, Action<List<T>?> setResult) where T : class
        {
            try
            {
                var result = await service.GetAllAsync(query);
                if (result?.IsSuccess == true && result.Data != null)
                {
                    setResult(result.Data);
                }
                else
                {
                    setResult(null);
                }
            }
            catch
            {
                setResult(null);
            }
        }
    }
}
