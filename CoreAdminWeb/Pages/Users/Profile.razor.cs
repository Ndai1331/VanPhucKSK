using CoreAdminWeb.Model.User;
using CoreAdminWeb.Services.Users;
using Microsoft.AspNetCore.Components;

namespace CoreAdminWeb.Pages.Users
{
    public partial class Profile : ComponentBase
    {
        private IUserService _userService { get; set; }

        public Profile(IUserService userService)
        {
            _userService = userService;
        }

        private UserModel CurrentUser { get; set; } = new UserModel();
        private bool IsLoading { get; set; } = true;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                var user = await _userService.GetCurrentUserAsync();
                if (user != null && user.IsSuccess)
                {
                    CurrentUser = user.Data ?? new UserModel();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching user profile: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private string GetBackgroundImageUrl()
        {
            return "assets/images/images-2.jpg";
        }
    }
}
