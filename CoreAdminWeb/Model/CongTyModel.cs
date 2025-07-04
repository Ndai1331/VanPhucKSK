using CoreAdminWeb.Model.Base;

namespace CoreAdminWeb.Model
{
    public class CongTyModel : BaseModel<int>
    {
        public string? dia_chi { get; set; }
        public string? email { get; set; }
        public string? dien_thoai { get; set; }
        public int? parent_id { get; set; }
    }
    public class CongTyCRUDModel : BaseDetailModel
    {
        public new string status { set; get; } = Status.published.ToString();
        public string? dia_chi { get; set; }
        public string? email { get; set; }
        public string? dien_thoai { get; set; }
        public int? parent_id { get; set; }
    }
}