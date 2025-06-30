using System.Text.Json.Serialization;

namespace CoreAdminWeb.Model.User
{
    public class LoginRequest
    {
        [JsonPropertyName("email")]
        public string email { get; set; }

        [JsonPropertyName("password")]
        public string password { get; set; }
    }

    public class LoginResponse
    {
        [JsonPropertyName("expires")]    
        public int expires { get; set; }
        
        [JsonPropertyName("access_token")]
        public string access_token { get; set; }

        [JsonPropertyName("refresh_token")]
        public string refresh_token { get; set; }
    }
   
} 