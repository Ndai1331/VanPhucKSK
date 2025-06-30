using Microsoft.AspNetCore.Components.Forms;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Text;
using CoreAdminWeb.Model.RequestHttps;

namespace CoreAdminWeb.RequestHttp
{
    /// <summary>
    /// Client for making unauthenticated API requests
    /// </summary>
    public static class PublicRequestClient
    {
        private static HttpClient? _client;
        private static readonly CancellationTokenSource _tokenSource = new();
        private const long UploadLimit = 25214400; // ~24MB

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

        /// <summary>
        /// Make a GET request to the specified URL
        /// </summary>
        public static async Task<RequestHttpResponse<T>> GetAPIAsync<T>([Required] string URL)
        {
            try
            {
                EnsureClientInitialized();
                var response = await _client!.GetAsync(URL, _tokenSource.Token);
                return await ReturnApiResponse<T>(response);
            }
            catch (Exception ex)
            {
                return new RequestHttpResponse<T>
                {
                    Errors = new List<ErrorResponse>
                    {
                        new()
                        {
                            Message = $"Request failed: {ex.Message}",
                            Code = "REQUEST_FAILED"
                        }
                    }
                };
            }
        }

        /// <summary>
        /// Make a POST request with JSON data
        /// </summary>
        public static async Task<RequestHttpResponse<T>> PostAPIAsync<T>([Required] string URL, object input)
        {
            try
            {
                EnsureClientInitialized();
                var content = new StringContent(
                    JsonConvert.SerializeObject(input),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _client!.PostAsync(URL, content, _tokenSource.Token);
                return await ReturnApiResponse<T>(response);
            }
            catch (Exception ex)
            {
                return new RequestHttpResponse<T>
                {
                    Errors = new List<ErrorResponse>
                    {
                        new()
                        {
                            Message = $"Request failed: {ex.Message}",
                            Code = "REQUEST_FAILED"
                        }
                    }
                };
            }
        }



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
        /// Make a POST request with a file
        /// </summary>
        public static async Task<RequestHttpResponse<T>> PostAPIWithFileAsync<T>([Required] string URL, IBrowserFile file)
        {
            try
            {
                EnsureClientInitialized();
                using var content = new MultipartFormDataContent();
                using var stream = file.OpenReadStream(UploadLimit);
                var streamContent = new StreamContent(stream);
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                content.Add(streamContent, "file", file.Name);

                var response = await _client!.PostAsync(URL, content, _tokenSource.Token);
                return await ReturnApiResponse<T>(response);
            }
            catch (Exception ex)
            {
                return new RequestHttpResponse<T>
                {
                    Errors = new List<ErrorResponse>
                    {
                        new()
                        {
                            Message = $"Request failed: {ex.Message}",
                            Code = "REQUEST_FAILED"
                        }
                    }
                };
            }
        }

        private static void EnsureClientInitialized()
        {
            if (_client == null)
            {
                throw new InvalidOperationException("Client has not been initialized. Call Initialize() first.");
            }
        }

        private static async Task<RequestHttpResponse<T>> ReturnApiResponse<T>(HttpResponseMessage response)
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