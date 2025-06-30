using Microsoft.AspNetCore.Components.Forms;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Text;
using CoreAdminWeb.Model;
using CoreAdminWeb.Model.RequestHttps;
using CoreAdminWeb.Model.User;
using Blazored.LocalStorage;
using CoreAdminWeb.Services.Users;
using LoginResponse = CoreAdminWeb.Model.User.LoginResponse;

namespace CoreAdminWeb.RequestHttp
{
    /// <summary>
    /// Client for making authenticated API requests
    /// </summary>
    public static class RequestClient
    {
        private static HttpClient? _client;

        private static ILocalStorageService _localStorage;
        private static readonly CancellationTokenSource _tokenSource = new();
        private const long UploadLimit = 25214400; // ~24MB
        private static string? _accessToken;
        private static IUserService? _userService;

        // Event để thông báo khi cần logout
        public static event EventHandler? OnLogoutRequired;

        /// <summary>
        /// Initialize the client with a new HttpClient instance
        /// </summary>
        public static void Initialize(HttpClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        /// <summary>
        /// Cancel any ongoing requests
        /// </summary>
        public static void CancelToken()
        {
            _tokenSource.Cancel();
        }


        public static void InjectServices(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }


        /// <summary>
        /// Attach authentication token to the client
        /// </summary>
        public static void AttachToken(string token)
        {
            _accessToken = token;
            EnsureClientInitialized();
            
            if (!string.IsNullOrEmpty(_accessToken))
            {
                _client!.DefaultRequestHeaders.Clear();
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            }
        }

        /// <summary>
        /// Remove authentication token from the client
        /// </summary>
        public static void RemoveToken()
        {
            EnsureClientInitialized();
            _client!.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Authorization = null;
        }

        /// <summary>
        /// Trigger logout event and remove token
        /// </summary>
        private static void TriggerLogout()
        {
            RemoveToken();
            OnLogoutRequired?.Invoke(null, EventArgs.Empty);
        }

        /// <summary>
        /// Make a GET request to the specified URL
        /// </summary>
        public static async Task<RequestHttpResponse<T>> GetAPIAsync<T>([Required] string URL)
        {
            try
            {
                EnsureClientInitialized();
                EnsureTokenAttached();
                var response = await _client!.GetAsync(URL, _tokenSource.Token);
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
        public static async Task<RequestHttpResponse<T>> GetAPIWithoutAuthAsync<T>([Required] string URL)
        {
            try
            {
                EnsureClientInitialized();
                RemoveToken();
                var response = await _client!.GetAsync(URL, _tokenSource.Token);
                AttachToken(_accessToken ?? "");
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
        public static async Task PostAPIAsync([Required] string URL, object input)
        {
            try
            {
                EnsureClientInitialized();
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
                    EnsureTokenAttached();
                }

                var response = await _client!.PostAsync(URL, content, _tokenSource.Token);
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
        public static async Task<RequestHttpResponse<T>> PostAPIAsync<T>([Required] string URL, object input, bool notifyOk = true)
        {
            try
            {
                EnsureClientInitialized();
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
                    EnsureTokenAttached();
                }

                var response = await _client!.PostAsync(URL, content, _tokenSource.Token);
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
        public static async Task<RequestHttpResponse<T>> PostAPIWithFileAsync<T>([Required] string URL, IBrowserFile file, FileCRUDModel? fileCRUDModel = null)
        {
            try
            {
                EnsureClientInitialized();
                EnsureTokenAttached();

                using var content = new MultipartFormDataContent();
                using var stream = file.OpenReadStream(UploadLimit);
                var streamContent = new StreamContent(stream);
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                content.Add(streamContent, "file", file.Name);

                if (fileCRUDModel != null)
                {
                    AddFileMetadata(content, fileCRUDModel);
                }

                var response = await _client!.PostAsync(URL, content, _tokenSource.Token);
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
        public static async Task<RequestHttpResponse<T>> PostAPIWithMultipleFileAsync<T>([Required] string URL, List<IBrowserFile> files)
        {
            try
            {
                EnsureClientInitialized();
                EnsureTokenAttached();

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

                var response = await _client!.PostAsync(URL, content, _tokenSource.Token);

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
        public static async Task<RequestHttpResponse<T>> PatchAPIAsync<T>([Required] string URL, object input, bool notifyOk = true)
        {
            try
            {
                EnsureClientInitialized();
                EnsureTokenAttached();

                var content = new StringContent(
                    JsonConvert.SerializeObject(input),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _client!.PatchAsync(URL, content, _tokenSource.Token);
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
        public static async Task<RequestHttpResponse<T>> PutAPIAsync<T>([Required] string URL, object input)
        {
            try
            {
                EnsureClientInitialized();
                EnsureTokenAttached();

                var content = new StringContent(
                    JsonConvert.SerializeObject(input),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _client!.PutAsync(URL, content, _tokenSource.Token);
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
        public static async Task<RequestHttpResponse<T>> DeleteAPIAsync<T>([Required] string URL)
        {
            try
            {
                EnsureClientInitialized();
                EnsureTokenAttached();

                var response = await _client!.DeleteAsync(URL, _tokenSource.Token);
                return await ReturnApiResponse<T>(response);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<T>($"Request failed: {ex.Message}", "REQUEST_FAILED");
            }
        }

        private static void EnsureClientInitialized()
        {
            if (_client == null)
            {
                throw new InvalidOperationException("Client has not been initialized. Call Initialize() first.");
            }
        }

        private static void EnsureTokenAttached()
        {
            if (_client!.DefaultRequestHeaders.Authorization == null)
            {
                AttachToken(_accessToken ?? "");
            }
        }

        private static void AddFileMetadata(MultipartFormDataContent content, FileCRUDModel model)
        {
            void AddTextIfNotNull(object? value, string key)
            {
                if (value == null) return;

                string stringValue = value switch
                {
                    DateTime dt => dt.ToString("yyyy-MM-dd"),
                    Guid guid => guid.ToString(),
                    _ => value.ToString() ?? ""
                };

                content.Add(new StringContent(stringValue), key);
            }

            AddTextIfNotNull(model.phan_loai_vb, "phan_loai_vb");
            AddTextIfNotNull(model.linh_vuc_vb, "linh_vuc_vb");
            AddTextIfNotNull(model.folder, "folder");
            AddTextIfNotNull(model.ngay_ban_hanh, "ngay_ban_hanh");
            AddTextIfNotNull(model.ngay_hieu_luc, "ngay_hieu_luc");
            AddTextIfNotNull(model.co_quan_ban_hanh, "co_quan_ban_hanh");
            AddTextIfNotNull(model.system, "system");
            AddTextIfNotNull(model.so_van_ban, "so_van_ban");
            AddTextIfNotNull(model.so_ky_hieu, "so_ky_hieu");
            AddTextIfNotNull(model.so_luu_tru, "so_luu_tru");
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

        private static async Task<RequestHttpResponse<T>> ReturnApiResponse<T>(HttpResponseMessage response, int retryCount = 0)
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
                        // Retry the original request
                        var originalRequest = response.RequestMessage;
                        if (originalRequest != null)
                        {
                            var newResponse = await _client!.SendAsync(originalRequest, _tokenSource.Token);
                            return await ReturnApiResponse<T>(newResponse, retryCount + 1);
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
    }
}