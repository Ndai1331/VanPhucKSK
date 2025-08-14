using Blazored.LocalStorage;
using CoreAdminWeb.Model;
using CoreAdminWeb.Model.RequestHttps;
using CoreAdminWeb.Services.Users;
using CoreAdminWeb.Shared.Layout;
using Microsoft.AspNetCore.Components.Forms;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Text;

namespace CoreAdminWeb.Services.Http
{
    /// <summary>
    /// Scoped service for making authenticated API requests - replaces static RequestClient
    /// </summary>
    public class HttpClientService : IHttpClientService
    {
        private readonly HttpClient _client;
        private readonly ILocalStorageService _localStorage;
        private readonly CancellationTokenSource _tokenSource = new();
        private const long UploadLimit = 25214400; // ~24MB
        private IUserService? _userService;

        // Event để thông báo khi cần logout
        public event EventHandler? OnLogoutRequired;

        public HttpClientService(IHttpClientFactory client, ILocalStorageService localStorage, IConfiguration configuration)
        {
            _localStorage = localStorage ?? throw new ArgumentNullException(nameof(localStorage));
            _client = client.CreateClient("DrCoreApi");
        }

        public void SetUserService(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Attach authentication token to the client
        /// </summary>
        public void AttachToken(string token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                _client.DefaultRequestHeaders.Clear();
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        /// <summary>
        /// Remove authentication token from the client
        /// </summary>
        public void RemoveToken()
        {
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Authorization = null;
        }

        /// <summary>
        /// Get current access token from localStorage
        /// </summary>
        private async Task<string?> GetCurrentTokenAsync()
        {
            try
            {
                return await _localStorage.GetItemAsync<string>("accessToken");
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Ensure token is attached before making request
        /// </summary>
        private async Task EnsureTokenAttachedAsync()
        {
            if (_client.DefaultRequestHeaders.Authorization == null)
            {
                var token = await GetCurrentTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    token = AdminLayout.AccessToken;
                }

                if (!string.IsNullOrEmpty(token))
                {
                    AttachToken(token);
                }
            }
        }

        /// <summary>
        /// Trigger logout event and remove token
        /// </summary>
        private void TriggerLogout()
        {
            RemoveToken();
            OnLogoutRequired?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Make a GET request to the specified URL
        /// </summary>
        public async Task<RequestHttpResponse<T>> GetAPIAsync<T>([Required] string URL)
        {
            try
            {
                await EnsureTokenAttachedAsync();
                var response = await _client.GetAsync(URL, _tokenSource.Token);
                return await ReturnApiResponse<T>(response);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<T>($"Request failed: {ex.Message}", "REQUEST_FAILED");
            }
        }

        /// <summary>
        /// Make a GET request without authentication
        /// </summary>
        public async Task<RequestHttpResponse<T>> GetAPIWithoutAuthAsync<T>([Required] string URL)
        {
            try
            {
                var originalAuth = _client.DefaultRequestHeaders.Authorization;
                RemoveToken();
                var response = await _client.GetAsync(URL, _tokenSource.Token);

                // Restore original auth header
                if (originalAuth != null)
                {
                    _client.DefaultRequestHeaders.Authorization = originalAuth;
                }

                return await ReturnApiResponse<T>(response);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<T>($"Request failed: {ex.Message}", "REQUEST_FAILED");
            }
        }

        /// <summary>
        /// Make a POST request with JSON data without return type
        /// </summary>
        public async Task PostAPIAsync([Required] string URL, object input)
        {
            try
            {
                var content = new StringContent(
                    JsonConvert.SerializeObject(input),
                    Encoding.UTF8,
                    "application/json"
                );

                if (URL.Contains("/signin"))
                {
                    RemoveToken();
                }
                else
                {
                    await EnsureTokenAttachedAsync();
                }

                var response = await _client.PostAsync(URL, content, _tokenSource.Token);
                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Request failed with status code {response.StatusCode}: {errorResponse}");
                }
            }
            catch (Exception ex)
            {
                throw new HttpRequestException($"Request failed: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Make a POST request with JSON data and return type
        /// </summary>
        public async Task<RequestHttpResponse<T>> PostAPIAsync<T>([Required] string URL, object input, bool notifyOk = true)
        {
            try
            {
                var content = new StringContent(
                    JsonConvert.SerializeObject(input),
                    Encoding.UTF8,
                    "application/json"
                );

                if (URL.Contains("/signin"))
                {
                    RemoveToken();
                }
                else
                {
                    await EnsureTokenAttachedAsync();
                }

                var response = await _client.PostAsync(URL, content, _tokenSource.Token);
                return await ReturnApiResponse<T>(response);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<T>($"Request failed: {ex.Message}", "REQUEST_FAILED");
            }
        }

        /// <summary>
        /// Make a POST request with a file
        /// </summary>
        public async Task<RequestHttpResponse<T>> PostAPIWithFileAsync<T>([Required] string URL, IBrowserFile file, FileCRUDModel? fileCRUDModel = null)
        {
            try
            {
                await EnsureTokenAttachedAsync();

                using var content = new MultipartFormDataContent();
                using var stream = file.OpenReadStream(UploadLimit);
                var streamContent = new StreamContent(stream);
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                content.Add(streamContent, "file", file.Name);

                if (fileCRUDModel != null)
                {
                    AddFileMetadata(content, fileCRUDModel);
                }

                var response = await _client.PostAsync(URL, content, _tokenSource.Token);
                return await ReturnApiResponse<T>(response);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<T>($"Request failed: {ex.Message}", "REQUEST_FAILED");
            }
        }

        /// <summary>
        /// Make a POST request with multiple files
        /// </summary>
        public async Task<RequestHttpResponse<T>> PostAPIWithMultipleFileAsync<T>([Required] string URL, List<IBrowserFile> files)
        {
            try
            {
                await EnsureTokenAttachedAsync();

                using var content = new MultipartFormDataContent();
                var streams = new List<MemoryStream>();

                foreach (var file in files)
                {
                    var ms = new MemoryStream();
                    await file.OpenReadStream(UploadLimit).CopyToAsync(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    content.Add(new StreamContent(ms), "files", file.Name);
                    streams.Add(ms);
                }

                var response = await _client.PostAsync(URL, content, _tokenSource.Token);

                foreach (var stream in streams)
                {
                    stream.Dispose();
                }

                return await ReturnApiResponse<T>(response);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<T>($"Request failed: {ex.Message}", "REQUEST_FAILED");
            }
        }

        /// <summary>
        /// Make a PATCH request with JSON data
        /// </summary>
        public async Task<RequestHttpResponse<T>> PatchAPIAsync<T>([Required] string URL, object input, bool notifyOk = true)
        {
            try
            {
                await EnsureTokenAttachedAsync();

                var content = new StringContent(
                    JsonConvert.SerializeObject(input),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _client.PatchAsync(URL, content, _tokenSource.Token);
                return await ReturnApiResponse<T>(response);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<T>($"Request failed: {ex.Message}", "REQUEST_FAILED");
            }
        }

        /// <summary>
        /// Make a PUT request with JSON data
        /// </summary>
        public async Task<RequestHttpResponse<T>> PutAPIAsync<T>([Required] string URL, object input)
        {
            try
            {
                await EnsureTokenAttachedAsync();

                var content = new StringContent(
                    JsonConvert.SerializeObject(input),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _client.PutAsync(URL, content, _tokenSource.Token);
                return await ReturnApiResponse<T>(response);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<T>($"Request failed: {ex.Message}", "REQUEST_FAILED");
            }
        }

        /// <summary>
        /// Make a DELETE request
        /// </summary>
        public async Task<RequestHttpResponse<T>> DeleteAPIAsync<T>([Required] string URL)
        {
            try
            {
                await EnsureTokenAttachedAsync();

                var response = await _client.DeleteAsync(URL, _tokenSource.Token);
                return await ReturnApiResponse<T>(response);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<T>($"Request failed: {ex.Message}", "REQUEST_FAILED");
            }
        }

        private static void AddFileMetadata(MultipartFormDataContent content, FileCRUDModel model)
        {
            void AddTextIfNotNull(object? value, string key)
            {
                if (value == null)
                {
                    return;
                }

                string stringValue = value switch
                {
                    DateTime dt => dt.ToString("yyyy-MM-dd"),
                    Guid guid => guid.ToString(),
                    _ => value.ToString() ?? ""
                };

                content.Add(new StringContent(stringValue), key);
            }

            AddTextIfNotNull(model.folder, "folder");
        }

        private static RequestHttpResponse<T> CreateErrorResponse<T>(string message, string code)
        {
            return new RequestHttpResponse<T>
            {
                Errors = new List<ErrorResponse>
                {
                    new()
                    {
                        Message = message,
                        Code = code
                    }
                }
            };
        }

        private async Task<RequestHttpResponse<T>> ReturnApiResponse<T>(HttpResponseMessage response, int retryCount = 0)
        {
            var result = new RequestHttpResponse<T>();

            try
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    result.Data = JsonConvert.DeserializeObject<T>(jsonResponse);
                    return result;
                }

                var errorResponse = JsonConvert.DeserializeObject<GraphQLErrorResponse>(jsonResponse);

                // Check if token is expired
                if (errorResponse?.errors?.Any(e => e.extensions?.code == "TOKEN_EXPIRED") == true)
                {
                    // Prevent infinite loop
                    if (retryCount >= 1)
                    {
                        TriggerLogout();
                        result.Errors = new List<ErrorResponse>
                        {
                            new()
                            {
                                Message = "Refresh token thất bại. Vui lòng đăng nhập lại.",
                                Code = "REFRESH_TOKEN_FAILED"
                            }
                        };
                        return result;
                    }

                    // Try to refresh token
                    if (_userService != null && await _userService.RefreshTokenAsync())
                    {
                        // Get updated token and retry
                        var newToken = await GetCurrentTokenAsync();
                        if (!string.IsNullOrEmpty(newToken))
                        {
                            AttachToken(newToken);

                            // Retry the original request
                            var originalRequest = response.RequestMessage;
                            if (originalRequest != null)
                            {
                                var newResponse = await _client.SendAsync(originalRequest, _tokenSource.Token);
                                return await ReturnApiResponse<T>(newResponse, retryCount + 1);
                            }
                        }
                    }
                    else
                    {
                        TriggerLogout();
                        result.Errors = new List<ErrorResponse>
                        {
                            new()
                            {
                                Message = "Refresh token thất bại. Vui lòng đăng nhập lại.",
                                Code = "REFRESH_TOKEN_FAILED"
                            }
                        };
                        return result;
                    }
                }

                result.Errors = errorResponse?.errors?.Select(e => new ErrorResponse
                {
                    Message = e.message,
                    Code = e.extensions?.code,
                    Reason = e.extensions?.reason
                }).ToList() ?? new List<ErrorResponse>
                {
                    new()
                    {
                        Message = "An unknown error occurred",
                        Code = "UNKNOWN_ERROR"
                    }
                };
            }
            catch (Exception ex)
            {
                result.Errors = new List<ErrorResponse>
                {
                    new()
                    {
                        Message = $"Failed to parse response: {ex.Message}",
                        Code = "PARSE_ERROR"
                    }
                };
            }

            return result;
        }

        public void Dispose()
        {
            _tokenSource?.Dispose();
        }
    }
}