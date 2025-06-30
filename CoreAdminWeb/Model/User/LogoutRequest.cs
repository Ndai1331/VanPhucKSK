using System.Text.Json.Serialization;

namespace CoreAdminWeb.Model.RequestHttps
{
    public class LogoutRequest
    {
        [JsonPropertyName("refresh_token")]
        public string refresh_token { get; set; }
    }
}