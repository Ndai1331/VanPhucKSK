using CoreAdminWeb.Model.Base;

namespace CoreAdminWeb.Model
{
    public class CongTyConTractModel : BaseModel<int>
    {
        public int? ma_don_vi { get; set; }
        public string? so_hop_dong { get; set; }
        public decimal? gia_tri_hop_dong { get; set; }
        public decimal? gia_tri_quyet_toan { get; set; }
        public DateTime? ngay_bat_dau { get; set; }
        public DateTime? ngay_ket_thuc_hop_dong { get; set; }
        public string? file_hop_dong { get; set; }
        public Guid? nguoi_thuc_hien { get; set; }
        public string? nguoi_gioi_thieu { get; set; }
        public string? dien_thoai { get; set; }
        public string? email { get; set; }
    }
}