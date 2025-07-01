using System.Text.Json.Serialization;
using CoreAdminWeb.Enums;

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

        [JsonPropertyName("so_dien_thoai")]
        public string so_dien_thoai { get; set; }

        [JsonPropertyName("ma_benh_nhan")]
        public string ma_benh_nhan { get; set; }
        [JsonPropertyName("gioi_tinh")]
        public GioiTinh? gioi_tinh { get; set; }
        [JsonPropertyName("ngay_sinh")]
        public DateTime? ngay_sinh { get; set; }
        [JsonPropertyName("so_dinh_danh")]
        public string so_dinh_danh { get; set; }
        [JsonPropertyName("ngay_cap")]
        public DateTime? ngay_cap { get; set; }
        [JsonPropertyName("noi_cap")]
        public string noi_cap { get; set; }
        [JsonPropertyName("dia_chi")]
        public string dia_chi { get; set; }

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