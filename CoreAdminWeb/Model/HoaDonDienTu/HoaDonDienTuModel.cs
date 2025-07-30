using CoreAdminWeb.Model.Base;

namespace CoreAdminWeb.Model
{
    public class HoaDonDienTuModel 
    {
        public string? ma_luot_kham { get; set; }
        public string? so_hoa_don { get; set; }
        public DateTime? ngay_hoa_don { get; set; }
        public string? ma_hoa_don { get; set; }
        public string? ma_giao_dich { get; set; }
        public string? ma_so_thue { get; set; }
        public string? url_tra_cuu { get; set; }
    }
}