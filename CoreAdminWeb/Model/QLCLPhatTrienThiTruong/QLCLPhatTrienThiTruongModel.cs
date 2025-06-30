using CoreAdminWeb.Enums;
using CoreAdminWeb.Model.Base;

namespace CoreAdminWeb.Model
{
    public class QLCLPhatTrienThiTruongModel : BaseModel<int>
    {
        public TinhModel? province { get; set; }
        public XaPhuongModel? ward { get; set; }
        public string? dia_chi { get; set; }
        public string? ma_so_thue { get; set; }
        public string? so_giay_phep_dkkd { get; set; }
        public DateTime? ngay_cap { get; set; }
        public string? co_quan_cap { get; set; }
        public HinhThucBanHang? hinh_thuc_ban_hang { get; set; } = HinhThucBanHang.TruyenThong;
        public ThiTruong? thi_truong { get; set; } = ThiTruong.XuatKhau;
        public QuyMoEnum? quy_mo { get; set; } = QuyMoEnum.QuyMoNho;
        public decimal? doanh_thu_du_kien { get; set; }
        public List<QLCLPhatTrienThiTruongSanPhamModel>? chi_tiets { get; set; }
    }
    public class QLCLPhatTrienThiTruongCRUDModel : BaseDetailModel
    {
        public new string status { get; set; } = Status.active.ToString();
        public int? province { get; set; }
        public int? ward { get; set; }
        public string? dia_chi { get; set; }
        public string? ma_so_thue { get; set; }
        public string? so_giay_phep_dkkd { get; set; }
        public DateTime? ngay_cap { get; set; }
        public string? co_quan_cap { get; set; }
        public HinhThucBanHang? hinh_thuc_ban_hang { get; set; } = HinhThucBanHang.TruyenThong;
        public ThiTruong? thi_truong { get; set; } = ThiTruong.XuatKhau;
        public QuyMoEnum? quy_mo { get; set; } = QuyMoEnum.QuyMoNho;
        public decimal? doanh_thu_du_kien { get; set; }
    }

    public class QLCLPhatTrienThiTruongCRUDResponseModel
    {
        public int id { get; set; }
    }
}
