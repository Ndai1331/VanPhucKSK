using System.Text.Json.Serialization;
using CoreAdminWeb.Helpers;

namespace CoreAdminWeb.Model.Menus
{
    public class MenuResponse
    {
        [JsonPropertyName("id")]    
        public int id { get; set; }
        
        [JsonPropertyName("status")]
        public string status { get; set; }

        [JsonPropertyName("sort")]
        public int sort { get; set; }

        [JsonPropertyName("code")]
        public string code { get; set; }

        [JsonPropertyName("name")]
        public string name { get; set; }
        [JsonPropertyName("icon")]
        public string icon { get; set; }

        [JsonPropertyName("parent_code")]
        public string? parent_code { get; set; }

        [JsonPropertyName("parent_id")]
        public int? parent_id { get; set; }

        [JsonPropertyName("external_system_id")]
        public int external_system_id { get; set; }
        
        [JsonIgnore]
        public List<MenuResponse> sub_menus { get; set; } = new List<MenuResponse>();

        [JsonIgnore]
        public string url { get{
            return MenuHelper.CreateUrlByName(name, id);
        }}
    }
} 