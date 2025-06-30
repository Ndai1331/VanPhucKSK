
namespace CoreAdminWeb.Model.Reports
{
    /// <summary>
    /// Model for QLCL Bao Cao Kiem Tra Hau Kiem ATTP
    /// </summary>
    public class ReportBaoCaoKiemTraHauKiemATTPModel
    {
        /// <summary>
        /// Tháng báo cáo (yyyy-MM format)
        /// </summary>
        public string thang { get; set; } = string.Empty;
        
        /// <summary>
        /// Tỉnh/thành phố
        /// </summary>
        public int? province { get; set; }
        
        /// <summary>
        /// Quận/huyện
        /// </summary>
        public int? ward { get; set; }
        
        /// <summary>
        /// Tổng số đợt kiểm tra
        /// </summary>
        public int tong_dot_kiem_tra { get; set; }
        
        /// <summary>
        /// Tổng số cơ sở kiểm tra
        /// </summary>
        public int tong_co_so_kiem_tra { get; set; }
        
        /// <summary>
        /// Số cơ sở vi phạm
        /// </summary>
        public int so_vi_pham { get; set; }
        
        /// <summary>
        /// Số cơ sở chấp hành
        /// </summary>
        public int so_chap_hanh { get; set; }
        
        /// <summary>
        /// Số cơ sở đạt
        /// </summary>
        public int so_dat { get; set; }
        
        /// <summary>
        /// Số cơ sở không đạt
        /// </summary>
        public int so_khong_dat { get; set; }
    }
} 