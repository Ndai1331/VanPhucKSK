using CoreAdminWeb.Model.Base;
using CoreAdminWeb.Model.User;

namespace CoreAdminWeb.Model.KhamSucKhoes
{
    public class SoKhamSucKhoeModel : BaseModel<int>
    {
        public DateTime? ngay_kham { get; set; }
        public DateTime? ngay_lap_so { get; set; }
        public string? ma_benh_nhan { get; set; }
        public string? ma_luot_kham { get; set; }
        public string? nguoi_lap { get; set; }
        public string? chu_ky_nls { get; set; }
        public string? xac_nhan_nld { get; set; }
        public bool send_mail { get; set; } = false;
        public int? ma_cong_ty { get; set; }
        public string? GhiChu { get; set; }
        public string? ChuanDoan { get; set; }
        public bool? Type { get; set; } = false;
        public new bool deleted { get; set; } = false;
        public UserModel? benh_nhan { get; set; }
        public KhamSucKhoeCongTyModel? MaDotKham { get; set; }
        public new Status status { set; get; } = Status.draft;
    }

    public class SoKhamSucKhoeCRUDModel : BaseDetailModel
    {
        public new string status { set; get; } = Status.draft.ToString();
        public DateTime? ngay_kham { get; set; }
        public DateTime? ngay_lap_so { get; set; }
        public string? ma_benh_nhan { get; set; }
        public string? ma_luot_kham { get; set; }
        public string? nguoi_lap { get; set; }
        public string? chu_ky_nls { get; set; }
        public string? xac_nhan_nld { get; set; }
        public bool send_mail { get; set; } = false;
        public int? ma_cong_ty { get; set; }
        public string? GhiChu { get; set; }
        public string? ChuanDoan { get; set; }
        public bool? Type { get; set; } = false;
        public new bool deleted { get; set; } = false;
        public Guid? benh_nhan { get; set; }
        public int? MaDotKham { get; set; }
    }
}