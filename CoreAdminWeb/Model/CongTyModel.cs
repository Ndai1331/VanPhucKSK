using CoreAdminWeb.Enums;
using CoreAdminWeb.Model.Base;

namespace CoreAdminWeb.Model
{
    public class CongTyModel : BaseModel<int>
    {
        public string? dia_chi { get; set; }
        public string? email { get; set; }
        public string? dien_thoai { get; set; }
        public string? nguoi_lien_he { get; set; }
        public int? parent_id { get; set; }
        public new Status status { get; set; } = Status.published;
    }
    public class CongTyCRUDModel
    {
        public string? code { get; set; }
        public string? name { get; set; }
        public bool? deleted { get; set; } = false;
        public string? description { get; set; }
        public int? sort { get; set; } = 0;
        public string? dia_chi { get; set; }
        public string? email { get; set; }
        public string? dien_thoai { get; set; }
        public string? nguoi_lien_he { get; set; }
        public int? parent_id { get; set; }
        public string status { get; set; } = Status.published.ToString();
    }
}