using System.Text.Json.Serialization;

namespace CoreAdminWeb.Model.User
{
    public class UserModel
    {
        [JsonPropertyName("id")]
        public string id { get; set; }

        [JsonPropertyName("first_name")]
        public string first_name { get; set; }

        [JsonPropertyName("last_name")]
        public string last_name { get; set; }

        [JsonIgnore]
        public string full_name => $"{last_name} {first_name}";

        [JsonPropertyName("email")]
        public string email { get; set; }

        [JsonPropertyName("password")]
        public string password { get; set; }
        
        [JsonPropertyName("location")]
        public string location { get; set; }

        [JsonPropertyName("title")]
        public string title { get; set; }
        
        [JsonPropertyName("description")]
        public string description { get; set; }

        [JsonPropertyName("tags")]
        public List<string>? tags { get; set; }
        
        [JsonPropertyName("avatar")]
        public string avatar { get; set; }

        [JsonPropertyName("language")]
        public string language { get; set; }

        [JsonPropertyName("status")]
        public string status { get; set; }
        
        [JsonPropertyName("role")]
        public string role { get; set; }
        
        [JsonPropertyName("token")]
        public string token { get; set; }
        
        [JsonPropertyName("last_access")]
        public string last_access { get; set; }

        [JsonPropertyName("last_page")]
        public string last_page { get; set; }

        [JsonPropertyName("policies")]
        public List<string> policies { get; set; }
    }
} 