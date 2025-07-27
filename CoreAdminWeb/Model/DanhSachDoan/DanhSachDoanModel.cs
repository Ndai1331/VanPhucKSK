using CoreAdminWeb.Model.Base;

namespace CoreAdminWeb.Model.DanhSachDoan
{
    public class DanhSachDoanModel : BaseModel<int>
    {
        // public CongTyModel? cong_ty { get; set; }
        public KhamSucKhoeCongTyModel? ma_ho_so_kham_suc_khoe { get; set; }
        // public DateTime? ngay_kham { get; set; }
        // public DateTime? ngay_lap_so { get; set; }
        // public string? nguoi_lap_so { get; set; }
        // public string? nguoi_xac_nhan { get; set; }
        // public bool? da_gui_thong_bao { get; set; }
        // public bool? da_gui_mail { get; set; }
    }
    public class CDanhSachDoanCRUDModel : BaseDetailModel
    {
        public new string status { set; get; } = Status.published.ToString();
        // public int? cong_ty { get; set; }
        public int? MaHoSoKhamSucKhoe { get; set; }
        // public DateTime? ngay_kham { get; set; }
        // public DateTime? ngay_lap_so { get; set; }
        // public string? nguoi_lap_so { get; set; }
        // public string? nguoi_xac_nhan { get; set; }
        // public bool? da_gui_thong_bao { get; set; }
        // public bool? da_gui_mail { get; set; }
    }
}