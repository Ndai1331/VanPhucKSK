
namespace CoreAdminWeb.Model.Reports
{
    /// <summary>
    /// Model for QLCL Bao Cao Kiem Tra Hau Kiem ATTP
    /// </summary>
    public class ReportBaoCaoThamDinhCapGCNModel
    {
        public string thang { get; set; } = string.Empty;
        
        public int tong_co_so_tham_dinh { get; set; }
        
        public int so_dat { get; set; }
        
        public int so_khong_dat { get; set; }
        
        public int so_co_so_duoc_cap_gcn { get; set; }
        
        public double ty_le_co_so_duoc_cap_gcn { get; set; }
    }
} 