using Microsoft.AspNetCore.Components.Forms;
using CoreAdminWeb.Model;
using CoreAdminWeb.Model.RequestHttps;

namespace CoreAdminWeb.Services.Http
{
    /// <summary>
    /// Interface for HTTP client service that makes authenticated API requests
    /// </summary>
    public interface IHttpClientService
    {
        /// <summary>
        /// Attach authentication token to the client
        /// </summary>
        void AttachToken(string token);

        /// <summary>
        /// Remove authentication token from the client
        /// </summary>
        void RemoveToken();

        /// <summary>
        /// Make a GET request to the specified URL
        /// </summary>
        Task<RequestHttpResponse<T>> GetAPIAsync<T>(string URL);

        /// <summary>
        /// Make a GET request without authentication
        /// </summary>
        Task<RequestHttpResponse<T>> GetAPIWithoutAuthAsync<T>(string URL);

        /// <summary>
        /// Make a POST request with JSON data without return type
        /// </summary>
        Task PostAPIAsync(string URL, object input);

        /// <summary>
        /// Make a POST request with JSON data and return type
        /// </summary>
        Task<RequestHttpResponse<T>> PostAPIAsync<T>(string URL, object input, bool notifyOk = true);

        /// <summary>
        /// Make a POST request with a file
        /// </summary>
        Task<RequestHttpResponse<T>> PostAPIWithFileAsync<T>(string URL, IBrowserFile file, FileCRUDModel? fileCRUDModel = null);

        /// <summary>
        /// Make a POST request with multiple files
        /// </summary>
        Task<RequestHttpResponse<T>> PostAPIWithMultipleFileAsync<T>(string URL, List<IBrowserFile> files);

        /// <summary>
        /// Make a PATCH request with JSON data
        /// </summary>
        Task<RequestHttpResponse<T>> PatchAPIAsync<T>(string URL, object input, bool notifyOk = true);

        /// <summary>
        /// Make a PUT request with JSON data
        /// </summary>
        Task<RequestHttpResponse<T>> PutAPIAsync<T>(string URL, object input);

        /// <summary>
        /// Make a DELETE request
        /// </summary>
        Task<RequestHttpResponse<T>> DeleteAPIAsync<T>(string URL);

        /// <summary>
        /// Event để thông báo khi cần logout
        /// </summary>
        event EventHandler? OnLogoutRequired;
    }
} 