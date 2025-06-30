namespace CoreAdminWeb.Model.Reports
{
    /// <summary>
    /// Model for QLCL Bao Cao Kiem Tra Hau Kiem ATTP
    /// </summary>
    public class ReportDashboardModel
    {
        public int so_luong_co_so { get; set; }
        public int so_luong_co_so_dat_chung_nhan { get; set; }
        public int so_luong_vu_vi_pham { get; set; }
        public int so_dot_kiem_tra { get; set; }
        public List<LoaiHinhCoSoModel> loai_hinh_co_so { get; set; } = new List<LoaiHinhCoSoModel>();
        public List<GcnSapHetHanModel> gcn_sap_het_han { get; set; } = new List<GcnSapHetHanModel>();
        public List<SoDotKiemTraTheoThangModel> so_dot_kiem_tra_theo_thang { get; set; } = new List<SoDotKiemTraTheoThangModel>();
    }

    /// <summary>
    /// Model for Loai Hinh Co So
    /// </summary>
    public class LoaiHinhCoSoModel
    {
        public string code { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public int so_luong_co_so { get; set; }
    }

    /// <summary>
    /// Model for GCN Sap Het Han
    /// </summary>
    public class GcnSapHetHanModel
    {
        public int id { get; set; }
        public string name { get; set; } = string.Empty;
        public string so_giay_chung_nhan { get; set; } = string.Empty;
        public DateTime ngay_het_hieu_luc { get; set; }
        public int so_ngay_con_lai { get; set; }
    }

    /// <summary>
    /// Model for So Dot Kiem Tra Theo Thang
    /// </summary>
    public class SoDotKiemTraTheoThangModel
    {
        public int t1 { get; set; }
        public int t2 { get; set; }
        public int t3 { get; set; }
        public int t4 { get; set; }
        public int t5 { get; set; }
        public int t6 { get; set; }
        public int t7 { get; set; }
        public int t8 { get; set; }
        public int t9 { get; set; }
        public int t10 { get; set; }
        public int t11 { get; set; }
        public int t12 { get; set; }
    }
} 