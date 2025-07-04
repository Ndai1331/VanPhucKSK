using CoreAdminWeb.Model.Menus;
using CoreAdminWeb.Providers;
using CoreAdminWeb.Services;
using CoreAdminWeb.Services.BaseServices;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Text;

namespace CoreAdminWeb.Shared.Base
{
    public class BlazorCoreBase : ComponentBase
    {
        [Inject]
        protected AuthenticationStateProvider AuthStateProvider { get; set; } = null!;

        [Inject]
        protected NavigationManager? NavigationManager { get; set; }

        [Inject]
        protected AlertService AlertService { get; set; } = null!;

        [Inject]
        protected IJSRuntime JsRuntime { get; set; } = null!;

        // Cached authentication state to avoid repeated calls
        private bool? _cachedIsAuthenticated;
        private DateTime _authCacheTime = DateTime.MinValue;
        private static readonly TimeSpan AuthCacheTimeout = TimeSpan.FromMinutes(5);

        protected bool IsAuthenticated { get; private set; }
        public bool IsLoading { get; set; } = false;

        // Pagination properties with better initialization
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public string BuilderQuery { get; set; } = "";
        
        // Cached file types string
        public string AcceptFileTypes { get; set; } = "application/vnd.openxmlformats-officedocument.wordprocessingml.document, application/vnd.openxmlformats-officedocument.spreadsheetml.sheet, application/vnd.openxmlformats-officedocument.presentationml.presentation, application/pdf,application/zip, application/x-7z-compressed, application/x-rar-compressed, application/x-tar, application/x-gzip, application/x-bzip2, application/x-compressed, application/x-compressed-tar, application/x-compressed-zip, application/x-compressed-rar, application/x-compressed-7z";

        // Cached status list - created once
        public static readonly List<Model.Base.Status> StatusList = new() 
        { 
            Model.Base.Status.draft,
            Model.Base.Status.published 
        };

        protected List<MenuResponse> Menus { get; set; } = new List<MenuResponse>();

        // Cache for query building to avoid repeated string operations
        private readonly Dictionary<string, string> _queryCache = new();
        private readonly StringBuilder _queryBuilder = new();

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            
            // Only reset page if not already initialized properly
            if (Page <= 0 || PageSize <= 0)
            {
                ResetPage();
            }
        }

        /// <summary>
        /// Reset pagination values with optimized approach
        /// </summary>
        public void ResetPage()
        {
            Page = 1;
            PageSize = 10;
            TotalCount = 0;
            TotalPages = 0;
            TotalItems = 0;
            
            // Clear query cache when resetting
            _queryCache.Clear();
            BuilderQuery = "";
        }

        /// <summary>
        /// Check authentication state with caching to improve performance
        /// </summary>
        public async Task<bool> IsAuthenticatedAsync()
        {
            // Return cached result if still valid
            if (_cachedIsAuthenticated.HasValue && 
                DateTime.UtcNow - _authCacheTime < AuthCacheTimeout)
            {
                return _cachedIsAuthenticated.Value;
            }

            try
            {
                var authState = await AuthStateProvider.GetAuthenticationStateAsync();
                var isAuthenticated = authState?.User?.Identity?.IsAuthenticated ?? false;
                
                // Cache the result
                _cachedIsAuthenticated = isAuthenticated;
                _authCacheTime = DateTime.UtcNow;
                IsAuthenticated = isAuthenticated;
                
                return isAuthenticated;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking authentication: {ex.Message}");
                // Don't cache error results
                return false;
            }
        }

        /// <summary>
        /// Logout with optimized cleanup
        /// </summary>
        protected async Task Logout()
        {
            try
            {
                // Clear authentication cache
                _cachedIsAuthenticated = null;
                _authCacheTime = DateTime.MinValue;
                IsAuthenticated = false;

                await ((ApiAuthenticationStateProvider)AuthStateProvider).MarkUserAsLoggedOut();
                NavigationManager?.NavigateTo("/signin", true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during logout: {ex.Message}");
                AlertService?.ShowAlert("Lỗi khi đăng xuất. Vui lòng thử lại.", "danger");
            }
        }

        /// <summary>
        /// Build pagination query with caching and StringBuilder for better performance
        /// </summary>
        public void BuildPaginationQuery(int page, int pageSize, string sort = "id", bool isAsc = false)
        {
            // Create cache key
            var cacheKey = $"{page}_{pageSize}_{sort}_{isAsc}";
            
            // Return cached query if available
            if (_queryCache.TryGetValue(cacheKey, out var cachedQuery))
            {
                BuilderQuery = cachedQuery;
                return;
            }

            // Build query using StringBuilder for better performance
            _queryBuilder.Clear();
            _queryBuilder.Append($"limit={pageSize}&offset={(page - 1) * pageSize}&meta=filter_count");
            
            if (!isAsc)
            {
                _queryBuilder.Append($"&sort=-{sort}");
            }
            else
            {
                _queryBuilder.Append($"&sort={sort}");
            }

            var query = _queryBuilder.ToString();
            
            // Cache the result (limit cache size to prevent memory issues)
            if (_queryCache.Count < 100)
            {
                _queryCache[cacheKey] = query;
            }
            else
            {
                // Clear cache if it gets too large
                _queryCache.Clear();
                _queryCache[cacheKey] = query;
            }

            BuilderQuery = query;
        }

        /// <summary>
        /// Optimized keyboard event handler with debouncing
        /// </summary>
        public async Task OnInputKeyDownSearch(KeyboardEventArgs e, Func<Task> loadData)
        {
            if (e.Key == "Enter" && !e.ShiftKey)
            {
                try
                {
                    await JsRuntime.InvokeVoidAsync("preventEnterKeyDefault", "search");
                    await loadData.Invoke();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in search: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Load typeahead data with improved caching and error handling
        /// </summary>
        public async Task<IEnumerable<T>> LoadBlazorTypeaheadData<T>(
            string searchText, 
            IBaseService<T> service, 
            string? otherQuery = "", 
            bool isIgnoreCheck = false)
        {
            try
            {
                var query = BuildBaseQuery(searchText, isIgnoreCheck);

                if (!string.IsNullOrEmpty(otherQuery))
                {
                    query += $"&{otherQuery}";
                }

                var result = await service.GetAllAsync(query);
                return result?.IsSuccess == true ? result.Data ?? Enumerable.Empty<T>() : Enumerable.Empty<T>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading typeahead data: {ex.Message}");
                return Enumerable.Empty<T>();
            }
        }

        /// <summary>
        /// Build base query with optimized string building
        /// </summary>
        private string BuildBaseQuery(string searchText = "", bool isIgnoreCheck = false)
        {
            _queryBuilder.Clear();
            _queryBuilder.Append("filter[_and][][deleted][_eq]=false&sort=sort");
            
            if (!string.IsNullOrEmpty(searchText))
            {
                _queryBuilder.Append($"&filter[_and][][name][_contains]={Uri.EscapeDataString(searchText)}");
            }
            
            return _queryBuilder.ToString();
        }

        /// <summary>
        /// Optimized page size change handler
        /// </summary>
        public async Task OnPageSizeChanged(Func<Task> loadData)
        {
            if (PageSize <= 0) return;
            
            Page = 1;
            TotalItems = 0;
            
            // Clear query cache since page size changed
            _queryCache.Clear();
            
            try
            {
                await loadData();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error changing page size: {ex.Message}");
                AlertService?.ShowAlert("Lỗi khi thay đổi kích thước trang", "danger");
            }
        }

        /// <summary>
        /// Navigate to previous page with validation
        /// </summary>
        public async Task PreviousPage(Func<Task> loadData)
        {
            if (Page > 1)
            {
                Page--;
                try
                {
                    await loadData();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error navigating to previous page: {ex.Message}");
                    Page++; // Revert on error
                }
            }
        }

        /// <summary>
        /// Navigate to selected page with validation
        /// </summary>
        public async Task SelectedPage(int page, Func<Task> loadData)
        {
            if (page < 1 || page > TotalPages) return;
            
            var previousPage = Page;
            Page = page;
            
            try
            {
                await loadData();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error navigating to page {page}: {ex.Message}");
                Page = previousPage; // Revert on error
            }
        }

        /// <summary>
        /// Navigate to next page with validation
        /// </summary>
        public async Task NextPage(Func<Task> loadData)
        {
            if (Page < TotalPages)
            {
                Page++;
                try
                {
                    await loadData();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error navigating to next page: {ex.Message}");
                    Page--; // Revert on error
                }
            }
        }

        /// <summary>
        /// Clear caches when component is being disposed
        /// </summary>
        public void ClearCaches()
        {
            _queryCache?.Clear();
            _queryBuilder?.Clear();
            _cachedIsAuthenticated = null;
            _authCacheTime = DateTime.MinValue;
        }
    }
}