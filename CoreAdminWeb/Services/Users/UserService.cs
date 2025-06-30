using CoreAdminWeb.Model.RequestHttps;
using CoreAdminWeb.Model.User;
using CoreAdminWeb.RequestHttp;
using LoginResponse = CoreAdminWeb.Model.User.LoginResponse;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using CoreAdminWeb.Providers;
using CoreAdminWeb.Helpers;

namespace CoreAdminWeb.Services.Users
{
    public interface IUserService
    {
        Task<RequestHttpResponse<LoginResponse>> LoginAsync(string email, string password);
        Task<bool> LogoutAsync(string refreshToken);
        Task<RequestHttpResponse<UserModel>> GetCurrentUserAsync();
        Task<RequestHttpResponse<UserModel>> UpdateUserAsync(UserModel req);
        Task<RequestHttpResponse<UserModel>> UpdateCurrentUserAsync(UserModel req);
        string GetAccessTokenAsync();
        string GetRefreshTokenAsync();
        Task<bool> RefreshTokenAsync();
    }

    public class UserService : IUserService
    {

        private readonly ILocalStorageService _localStorage;
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private string _accessToken;
        private string _refreshToken;
        public UserService(
            ILocalStorageService localStorage,
            AuthenticationStateProvider authenticationStateProvider
        )
        {
            _localStorage = localStorage;
            _authenticationStateProvider = authenticationStateProvider;
        }
        public async Task<RequestHttpResponse<LoginResponse>> LoginAsync(string email, string password)
        {
            var response = new RequestHttpResponse<LoginResponse>();
            try
            {
                var result = await PublicRequestClient.PostAPIAsync<RequestHttpResponse<LoginResponse>>("auth/login", new LoginRequest { email = email, password = password });
                if (result.IsSuccess)
                {
                    response.Data = result.Data.Data;
                    _accessToken = result.Data.Data.access_token;
                    _refreshToken = result.Data.Data.refresh_token;

                    RequestClient.AttachToken(_accessToken);
                    RequestClient.InjectServices(_localStorage);
                    var claim = ClaimHepler.GetListClaim(_accessToken);

                    var currentUserAsync = await GetCurrentUserAsync();
                    if(currentUserAsync.Data != null)
                    {
                        await _localStorage.SetItemAsync("accessToken", _accessToken);
                        await _localStorage.SetItemAsync("userName", currentUserAsync.Data.email);
                        await _localStorage.SetItemAsync("userId", currentUserAsync.Data.id);
                        await _localStorage.SetItemAsync("role", currentUserAsync.Data.role);
                        await _localStorage.SetItemAsync("claims", claim);
                        (
                            (ApiAuthenticationStateProvider)_authenticationStateProvider
                        ).MarkUserAsAuthenticated(currentUserAsync.Data.email);
                    }else{
                        response.Errors = new List<ErrorResponse> { new ErrorResponse { Message = "Không tìm thấy user" } };
                    }
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

        public async Task<bool> LogoutAsync(string refreshToken)
        {
            try
            {
                if (string.IsNullOrEmpty(_accessToken))
                    return true;

                await PublicRequestClient.PostAPIAsync("auth/logout", new LogoutRequest { refresh_token = refreshToken });
                _accessToken = null;
                _refreshToken = null;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<RequestHttpResponse<UserModel>> GetCurrentUserAsync()
        {
            var response = new RequestHttpResponse<UserModel>();
            try
            {
                var result = await RequestClient.GetAPIAsync<RequestHttpResponse<UserModel>>("users/me");
                if (result.IsSuccess)
                {
                    response.Data = result.Data.Data;
                }
                else
                {
                    response.Errors = result.Errors;
                }
            }
            catch (Exception ex)
            {
                response.Errors = new List<ErrorResponse> { new ErrorResponse { Message = ex.Message } };
            }
            return response;
        }

        public async Task<RequestHttpResponse<UserModel>> UpdateUserAsync(UserModel req)
        {
            var response = new RequestHttpResponse<UserModel>();
            try
            {
                var request = new
                {
                    req.first_name,
                    req.last_name,
                    req.email,
                    req.location,
                    req.title,
                    req.description,
                    req.avatar,
                    req.language
                };

                var result = await RequestClient.PatchAPIAsync<RequestHttpResponse<UserModel>>($"users/{req.id}", request);
                if (result.IsSuccess)
                {
                    response.Data = result.Data.Data;
                }
                else
                {
                    response.Errors = result.Errors;
                }
            }
            catch (Exception ex)
            {
                response.Errors = new List<ErrorResponse> { new ErrorResponse { Message = ex.Message } };
            }
            return response;
        }

        public async Task<RequestHttpResponse<UserModel>> UpdateCurrentUserAsync(UserModel req)
        {
            var response = new RequestHttpResponse<UserModel>();
            try
            {
                dynamic request;

                if (!string.IsNullOrEmpty(req.password))
                    request = new { req.password };
                else
                    request = new
                    {
                        req.id,
                        req.first_name,
                        req.last_name,
                        req.email,
                        req.location,
                        req.title,
                        req.description,
                        req.avatar,
                        req.language
                    };

                var result = await RequestClient.PatchAPIAsync<RequestHttpResponse<UserModel>>("users/me", request);
                if (result.IsSuccess)
                {
                    response.Data = result.Data.Data;
                }
                else
                {
                    response.Errors = result.Errors;
                }
            }
            catch (Exception ex)
            {
                response.Errors = new List<ErrorResponse> { new ErrorResponse { Message = ex.Message } };
            }
            return response;
        }

        public string GetAccessTokenAsync()
        {
            return _accessToken;
        }
        public string GetRefreshTokenAsync()
        {
            return _refreshToken;
        }

        public async Task<bool> RefreshTokenAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(_refreshToken))
                    return false;

                // Call refresh token endpoint
                var response = await PublicRequestClient.PostAPIAsync<RequestHttpResponse<LoginResponse>>(
                    "auth/refresh",
                    new { refresh_token = _refreshToken }
                );

                if (response.IsSuccess && response.Data?.Data != null)
                {
                    // Update tokens
                    _accessToken = response.Data.Data.access_token;
                    _refreshToken = response.Data.Data.refresh_token;
                    
                    // Update token in RequestClient
                    RequestClient.AttachToken(_accessToken);
                    return true;
                }
            }
            catch
            {
                // Ignore refresh errors
            }
            return false;
        }
    }
}