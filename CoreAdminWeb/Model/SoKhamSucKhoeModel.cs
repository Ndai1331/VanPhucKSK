using CoreAdminWeb.Enums;
using CoreAdminWeb.Model.Base;

namespace CoreAdminWeb.Model
{
    public class SoKhamSucKhoeModel : BaseModel<int>
    {
        public DateTime? ngay_kham { get; set; }
        public DateTime? ngay_lap_so { get; set; }
        public string ma_benh_nhan { get; set; }
        public string ma_luot_kham { get; set; }
        public string nguoi_lap { get; set; }
        public string chu_ky_nls { get; set; }
        public string xac_nhan_nld { get; set; }
        public bool send_mail { get; set; } = false;
        public int? ma_cong_ty { get; set; }
        public bool deleted { get; set; } = false;

    }

    public class SoKhamSucKhoeCRUDModel : BaseDetailModel
    {
        public new string status { set; get; } = Status.published.ToString();
        public DateTime? ngay_kham { get; set; }
        public DateTime? ngay_lap_so { get; set; }
        public int? ma_benh_nhan { get; set; }
        public int? ma_luot_kham { get; set; }
        public int? nguoi_lap { get; set; }
        public int? chu_ky_nls { get; set; }
        public int? xac_nhan_nld { get; set; }
        public bool send_mail { get; set; } = false;
        public int? ma_cong_ty { get; set; }
        public bool deleted { get; set; } = false;
    }
}