using CoreAdminWeb.Enums;
using CoreAdminWeb.Model.Base;

namespace CoreAdminWeb.Model
{
    public class CongTyModel : BaseModel<int>
    {
        public new int? code { get; set; }
        public string? dia_chi { get; set; }
        public string? email { get; set; }
        public string? dien_thoai { get; set; }
        public string? nguoi_lien_he { get; set; }
        public int? parent_id { get; set; }
        public new Status status { get; set; } = Status.published;
    }
    public class CongTyCRUDModel : BaseDetailModel
    {
        public new int? code { get; set; }
        public string? dia_chi { get; set; }
        public string? email { get; set; }
        public string? dien_thoai { get; set; }
        public string? nguoi_lien_he { get; set; }
        public int? parent_id { get; set; }
        public new string status { get; set; } = Status.published.ToString();
    }
}