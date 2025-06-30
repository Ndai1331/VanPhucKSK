using CoreAdminWeb.Model.Base;
using CoreAdminWeb.Enums;

namespace CoreAdminWeb.Model
{
    public class QLCLDotKiemTraHauKiemATTPModel : BaseModel<int>
    {
        public DateTime? bat_dau_tu { get; set; }
        public DateTime? den { get; set; }
        public string? so_quyet_dinh { get; set; }
        public string? don_vi_chu_tri { get; set; }
        public string? noi_dung_kiem_tra { get; set; }

    }
    public class QLCLDotKiemTraHauKiemATTPCRUDModel : BaseDetailModel
    {
        public new string status { get; set; } = Status.active.ToString();
        public DateTime? bat_dau_tu { get; set; }
        public DateTime? den { get; set; }
        public string? so_quyet_dinh { get; set; }
        public string? don_vi_chu_tri { get; set; }
        public string? noi_dung_kiem_tra { get; set; }
    }
}
