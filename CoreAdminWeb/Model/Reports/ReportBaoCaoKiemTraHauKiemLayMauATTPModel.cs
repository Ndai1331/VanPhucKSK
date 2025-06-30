
namespace CoreAdminWeb.Model.Reports
{
    /// <summary>
    /// Model for QLCL Bao Cao Kiem Tra Hau Kiem ATTP
    /// </summary>
    public class ReportBaoCaoKiemTraHauKiemLayMauATTPModel
    {
        public int id { get; set; }
        public string san_pham { get; set; } = string.Empty;
        public int so_luong_mau { get; set; }
        public string chi_tieu { get; set; } = string.Empty;
        public int? so_mau_khong_dat { get; set; }
        public string? chi_tieu_vi_pham { get; set; }
        public string? muc_phat_hien { get; set; }
    }
} 