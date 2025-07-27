using Blazored.LocalStorage;
using CoreAdminWeb.Helpers;
using CoreAdminWeb.Http;
using CoreAdminWeb.Model.RequestHttps;
using CoreAdminWeb.Model.User;
using CoreAdminWeb.Providers;
using CoreAdminWeb.Services.BaseServices;
using CoreAdminWeb.Services.Http;
using Microsoft.AspNetCore.Components.Authorization;
using LoginResponse = CoreAdminWeb.Model.User.LoginResponse;

namespace CoreAdminWeb.Services.Users
{
    public interface IUserService
    {
        Task<RequestHttpResponse<LoginResponse>> LoginAsync(string email, string password);
        Task<RequestHttpResponse<LoginResponse>> LoginAdminAsync(string email, string password);
        Task<bool> LogoutAsync();
        Task<RequestHttpResponse<UserModel>> GetCurrentUserAsync();
        Task<RequestHttpResponse<UserModel>> UpdateUserAsync(UserModel req);
        Task<RequestHttpResponse<UserModel>> UpdateCurrentUserAsync(UserModel req);
        Task<RequestHttpResponse<UserModel>> UpdateUserAvatarAsync(UserModel req);
        Task<RequestHttpResponse<UserModel>> GetUserByCCCDAsync(string cccd);
        string GetAccessTokenAsync();
        string GetRefreshTokenAsync();
        Task<bool> RefreshTokenAsync();
        Task<RequestHttpResponse<List<UserModel>>> GetAllAsync(string query, bool isPublic = false);
    }

    public class UserService : IUserService
    {

        private readonly ILocalStorageService _localStorage;
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly IHttpClientService _httpClientService;
        private string _accessToken;
        private string _refreshToken;

        public UserService(
            ILocalStorageService localStorage,
            AuthenticationStateProvider authenticationStateProvider,
            IHttpClientService httpClientService
        )
        {
            _localStorage = localStorage;
            _authenticationStateProvider = authenticationStateProvider;
            _httpClientService = httpClientService;

            // Set up circular reference for token refresh
            if (_httpClientService is HttpClientService httpService)
            {
                httpService.SetUserService(this);
            }
        }

        private async Task<string> GetEmailFromPhone(string phone)
        {
            var response = await PublicRequestClient.GetAPIAsync<RequestHttpResponse<List<UserModel>>>($"users?filter[_and][0][_and][0][so_dien_thoai][_eq]={phone}");
            if (response.IsSuccess && response.Data != null && response.Data.Data != null && response.Data.Data.Count > 0)
            {
                return response.Data.Data[0].email;
            }
            return string.Empty;
        }

        public async Task<RequestHttpResponse<LoginResponse>> LoginAsync(string phone, string password)
        {
            var response = new RequestHttpResponse<LoginResponse>();
            try
            {
                var email = await GetEmailFromPhone(phone);
                if (string.IsNullOrEmpty(email))
                {
                    response.Errors = new List<ErrorResponse> { new ErrorResponse { Message = "Không tìm thấy user" } };
                    return response;
                }

                var result = await PublicRequestClient.PostAPIAsync<RequestHttpResponse<LoginResponse>>("auth/login", new LoginRequest { email = email, password = password });
                if (result.IsSuccess)
                {
                    response.Data = result.Data.Data;
                    _accessToken = result.Data.Data.access_token;
                    _refreshToken = result.Data.Data.refresh_token;

                    _httpClientService.AttachToken(_accessToken);
                    var claim = ClaimHepler.GetListClaim(_accessToken);

                    var currentUserAsync = await GetCurrentUserAsync();
                    if (currentUserAsync.Data != null)
                    {
                        await _localStorage.SetItemAsync("accessToken", _accessToken);
                        await _localStorage.SetItemAsync("userName", currentUserAsync.Data.email);
                        await _localStorage.SetItemAsync("userId", currentUserAsync.Data.id);
                        await _localStorage.SetItemAsync("role", currentUserAsync.Data.role);
                        await _localStorage.SetItemAsync("claims", claim);
                        (
                            (ApiAuthenticationStateProvider)_authenticationStateProvider
                        ).MarkUserAsAuthenticated(currentUserAsync.Data.email);
                    }
                    else
                    {
                        response.Errors = new List<ErrorResponse> { new ErrorResponse { Message = "Không tìm thấy user" } };
                    }
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

        public async Task<RequestHttpResponse<LoginResponse>> LoginAdminAsync(string email, string password)
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

                    _httpClientService.AttachToken(_accessToken);
                    var claim = ClaimHepler.GetListClaim(_accessToken);

                    var currentUserAsync = await GetCurrentUserAsync();
                    if (currentUserAsync.Data != null)
                    {
                        await _localStorage.SetItemAsync("accessToken", _accessToken);
                        await _localStorage.SetItemAsync("userName", currentUserAsync.Data.email);
                        await _localStorage.SetItemAsync("userId", currentUserAsync.Data.id);
                        await _localStorage.SetItemAsync("role", currentUserAsync.Data.role);
                        await _localStorage.SetItemAsync("claims", claim);
                        (
                            (ApiAuthenticationStateProvider)_authenticationStateProvider
                        ).MarkUserAsAuthenticated(currentUserAsync.Data.email);
                    }
                    else
                    {
                        response.Errors = new List<ErrorResponse> { new ErrorResponse { Message = "Không tìm thấy user" } };
                    }
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

        public async Task<bool> LogoutAsync()
        {
            try
            {
                _accessToken = null;
                _refreshToken = null;
                _httpClientService.RemoveToken();
                await _localStorage.RemoveItemAsync("accessToken");
                await _localStorage.RemoveItemAsync("userName");
                await _localStorage.RemoveItemAsync("userId");
                await _localStorage.RemoveItemAsync("role");
                await _localStorage.RemoveItemAsync("claims");
                (
                    (ApiAuthenticationStateProvider)_authenticationStateProvider
                ).MarkUserAsLoggedOut();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<RequestHttpResponse<UserModel>> GetUserByCCCDAsync(string strFilter)
        {
            var response = new RequestHttpResponse<UserModel>();
            try
            {
                string filter = $"filter[_or][0][so_dinh_danh][_eq]={strFilter}" +
                $"&filter[_or][1][so_dien_thoai][_eq]={strFilter}";
                var result = await PublicRequestClient.GetAPIAsync<RequestHttpResponse<List<UserModel>>>($"users?{filter}");
                if (result.IsSuccess)
                {
                    response.Data = result.Data.Data.FirstOrDefault();
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

        public async Task<RequestHttpResponse<UserModel>> GetCurrentUserAsync()
        {
            var response = new RequestHttpResponse<UserModel>();
            try
            {
                var result = await _httpClientService.GetAPIAsync<RequestHttpResponse<UserModel>>("users/me");
                if (result.IsSuccess)
                {
                    response.Data = result.Data.Data;
                }
                else
                {
                    response.Errors = result.Errors;
                    _accessToken = null;
                    _refreshToken = null;
                    _httpClientService.RemoveToken();
                    await _localStorage.RemoveItemAsync("accessToken");
                    await _localStorage.RemoveItemAsync("userName");
                    await _localStorage.RemoveItemAsync("userId");
                    await _localStorage.RemoveItemAsync("role");
                    await _localStorage.RemoveItemAsync("claims");
                    (
                        (ApiAuthenticationStateProvider)_authenticationStateProvider
                    ).MarkUserAsLoggedOut();
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

                var result = await _httpClientService.PatchAPIAsync<RequestHttpResponse<UserModel>>($"users/{req.id}", request);
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

        public async Task<RequestHttpResponse<UserModel>> UpdateUserAvatarAsync(UserModel req)
        {
            var response = new RequestHttpResponse<UserModel>();
            try
            {
                var request = new
                {
                    req.avatar
                };

                var result = await _httpClientService.PatchAPIAsync<RequestHttpResponse<UserModel>>($"users/{req.id}", request);
                if (result.IsSuccess)
                {
                    response.Data = result.Data?.Data;
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
                {
                    request = new { req.password };
                }
                else
                {
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
                }

                var result = await _httpClientService.PatchAPIAsync<RequestHttpResponse<UserModel>>("users/me", request);
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
                {
                    return false;
                }

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

                    // Update token in localStorage and HttpClientService
                    await _localStorage.SetItemAsync("accessToken", _accessToken);
                    _httpClientService.AttachToken(_accessToken);
                    return true;
                }
            }
            catch
            {
                // Ignore refresh errors
            }
            return false;
        }

        public async Task<RequestHttpResponse<List<UserModel>>> GetAllAsync(string query, bool isPublic = false)
        {
            try
            {
                string url = $"users?{query}";
                var response = isPublic
                    ? await PublicRequestClient.GetAPIAsync<RequestHttpResponse<List<UserModel>>>(url)
                    : await _httpClientService.GetAPIAsync<RequestHttpResponse<List<UserModel>>>(url);

                return response.IsSuccess
                    ? new RequestHttpResponse<List<UserModel>> { Data = response.Data?.Data, Meta = response.Data?.Meta }
                    : new RequestHttpResponse<List<UserModel>> { Errors = response.Errors };
            }
            catch (Exception ex)
            {
                return IBaseGetService<UserModel>.CreateErrorResponse<List<UserModel>>(ex);
            }
        }

    }
}