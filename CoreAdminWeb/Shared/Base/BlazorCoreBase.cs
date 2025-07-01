using CoreAdminWeb.Model.Menus;
using CoreAdminWeb.Model.User;
using CoreAdminWeb.Providers;
using CoreAdminWeb.Services;
using CoreAdminWeb.Services.BaseServices;
using CoreAdminWeb.Services.Users;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace CoreAdminWeb.Shared.Base
{
    public class BlazorCoreBase : ComponentBase
    {
        [Inject]
        protected AuthenticationStateProvider AuthStateProvider { get; set; }

        [Inject]
        protected IUserService UserService { get; set; }

        [Inject]
        protected NavigationManager? NavigationManager { get; set; }

        [Inject]
        protected AlertService AlertService { get; set; }

        [Inject]
        protected IJSRuntime JsRuntime { get; set; }

        protected UserModel? CurrentUser { get; private set; }
        protected bool IsAuthenticated { get; private set; }
        public bool IsLoading { get; set; } = false;

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public string BuilderQuery { get; set; } = "";
        public string AcceptFileTypes { get; set; } = "application/vnd.openxmlformats-officedocument.wordprocessingml.document, application/vnd.openxmlformats-officedocument.spreadsheetml.sheet, application/vnd.openxmlformats-officedocument.presentationml.presentation, application/pdf,application/zip, application/x-7z-compressed, application/x-rar-compressed, application/x-tar, application/x-gzip, application/x-bzip2, application/x-compressed, application/x-compressed-tar, application/x-compressed-zip, application/x-compressed-rar, application/x-compressed-7z";

        public static List<Model.Base.Status> StatusList = new List<
        Model.Base.Status> { Model.Base.Status.draft,
         Model.Base.Status.published };
        protected List<MenuResponse> Menus { get; set; } = new List<MenuResponse>();

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            ResetPage();
            CurrentUser = await GetCurrentUser();
        }
        public void ResetPage()
        {
            Page = 1;
            PageSize = 10;
            TotalCount = 0;
            TotalPages = 0;
            TotalItems = 0;
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            return authState?.User?.Identity?.IsAuthenticated ?? false;
        }

        protected async Task Logout()
        {
            try
            {
                await ((ApiAuthenticationStateProvider)AuthStateProvider).MarkUserAsLoggedOut();
                NavigationManager?.NavigateTo("/signin", true);
            }
            catch (Exception ex)
            {
                // Handle error
                Console.WriteLine($"Error during logout: {ex.Message}");
            }
        }

        protected bool HasRole(string roleName)
        {
            return CurrentUser?.role == roleName;
        }

        protected bool HasAnyRole(params string[] roleNames)
        {
            return roleNames.Contains(CurrentUser?.role);
        }
        protected async Task<UserModel?> GetCurrentUser()
        {
            var resUser = await UserService.GetCurrentUserAsync();
            if (resUser.IsSuccess)
            {
                CurrentUser = resUser.Data;
            }
            return CurrentUser;
        }


        public void BuildPaginationQuery(int page, int pageSize, string sort = "id", bool isAsc = false)
        {
            BuilderQuery = $"limit={pageSize}&offset={(page - 1) * pageSize}&meta=filter_count";
            if (!isAsc)
            {
                BuilderQuery += $"&sort=-{sort}";
            }
            else
            {
                BuilderQuery += $"&sort={sort}";
            }
        }



        public async Task OnInputKeyDownSearch(KeyboardEventArgs e, Func<Task> loadData)
        {
            if (e.Key == "Enter" && !e.ShiftKey)
            {
                await JsRuntime.InvokeVoidAsync("preventEnterKeyDefault", "search");
                await loadData.Invoke();
            }
        }


        public async Task<IEnumerable<T>> LoadBlazorTypeaheadData<T>(string searchText, IBaseService<T> service, string? otherQuery = "", bool isIgnoreCheck = false)
        {
            var query = BuildBaseQuery(searchText, isIgnoreCheck);

            if (!string.IsNullOrEmpty(otherQuery))
                query += $"&{otherQuery}";

            var result = await service.GetAllAsync(query);
            return result.IsSuccess ? result.Data ?? new List<T>() : new List<T>();
        }


        private string BuildBaseQuery(string searchText = "", bool isIgnoreCheck = false)
        {
            var query = "filter[_and][][deleted][_eq]=false&sort=sort";
            if (!string.IsNullOrEmpty(searchText))
            {
                if (!string.IsNullOrEmpty(query))
                    query += "&";
                query += $"filter[_and][][name][_contains]={searchText}";
            }
            return query;
        }


        public async Task OnPageSizeChanged(Func<Task> loadData)
        {
            Page = 1;
            TotalItems = 0;
            await loadData();
        }

        public async Task PreviousPage(Func<Task> loadData)
        {
            if (Page > 1)
            {
                Page--;
                await loadData();
            }
        }

        public async Task SelectedPage(int page, Func<Task> loadData)
        {
            Page = page;
            await loadData();
        }

        public async Task NextPage(Func<Task> loadData)
        {
            if (Page < TotalPages)
            {
                Page++;
                await loadData();
            }
        }
    }
}