using CoreAdminWeb.Enums;
using CoreAdminWeb.Model.Base;

namespace CoreAdminWeb.Model
{
    public class QLCLKenhQuangBaXucTienThuongMaiModel : BaseModel<int>
    {
        public DateTime? ngay_to_chuc { get; set; }
        public string? dia_diem_to_chuc { get; set; }
        public TinhModel? province { get; set; }
        public XaPhuongModel? ward { get; set; }
        public KenhQuangBa? kenh_quang_ba { get; set; }
        public HinhThucQuangBa? hinh_thuc_quang_ba { get; set; }
        public PhamViTiepCan? pham_vi_tiep_can { get; set; }
        public DoiTuongTiepCan? doi_tuong_tiep_can { get; set; }
        public int? so_luong_chu_the_tham_gia { get; set; }
        public int? luot_khach_tham_quan { get; set; }
        public int? so_hop_dong_ky_ket { get; set; }
        public decimal? gia_tri_giao_dich { get; set; }
        public string? san_pham_tham_gia { get; set; }
        public string? nguon_kinh_phi_thuc_hien { get; set; }
        public string? noi_dung_chuong_trinh { get; set; }
    }
    public class QLCLKenhQuangBaXucTienThuongMaiCRUDModel : BaseDetailModel
    {
        public new string status { get; set; } = Status.active.ToString();
        public DateTime? ngay_to_chuc { get; set; }
        public string? dia_diem_to_chuc { get; set; }
        public int? province { get; set; }
        public int? ward { get; set; }
        public KenhQuangBa? kenh_quang_ba { get; set; }
        public HinhThucQuangBa? hinh_thuc_quang_ba { get; set; }
        public PhamViTiepCan? pham_vi_tiep_can { get; set; }
        public DoiTuongTiepCan? doi_tuong_tiep_can { get; set; }
        public int? so_luong_chu_the_tham_gia { get; set; }
        public int? luot_khach_tham_quan { get; set; }
        public int? so_hop_dong_ky_ket { get; set; }
        public decimal? gia_tri_giao_dich { get; set; }
        public string? san_pham_tham_gia { get; set; }
        public string? nguon_kinh_phi_thuc_hien { get; set; }
        public string? noi_dung_chuong_trinh { get; set; }

    }
}
