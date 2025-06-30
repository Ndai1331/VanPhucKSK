using CoreAdminWeb.Model.Base;
using CoreAdminWeb.Enums;
using CoreAdminWeb.Model.User;

namespace CoreAdminWeb.Model
{
    public class QLCLCoSoViPhamATTPModel : BaseModel<int>
    {
        public DateTime? ngay_ghi_nhan { get; set; }
        public DateTime? ngay_xu_ly { get; set; }
        public string? co_quan_xu_ly { get; set; }
        public LoaiCoSoNLTS? loai_co_so { get; set; } = LoaiCoSoNLTS.CoSoCheBien;
        public QLCLCoSoCheBienNLTSModel? co_so_che_bien_nlts { get; set; }
        public QLCLCoSoNLTSDuDieuKienATTPModel? co_so_nlts_du_dieu_kien_attp { get; set; }
        public string? noi_dung_vi_pham { get; set; }
        public string? san_pham_vi_pham { get; set; }
        public QLCLHanhViViPhamModel? hanh_vi_vi_pham { get; set; }
        public string? huong_xu_ly { get; set; }
        public decimal? phat_tien { get; set; }
        public QLCLHinhThucXuPhatModel? hinh_thuc_xu_phat { get; set; }
        public KhacPhuc khac_phuc { get; set; }
        public decimal? so_luong { get; set; }
        public DonViTinhModel? don_vi_tinh { get; set; }
        public decimal? gia_tri_tang_vat { get; set; }
        public TrangThaiXuLyKhac? xu_ly_khac { get; set; } = TrangThaiXuLyKhac.DinhChiLuuHanh;
        public new TrangThaiXuLy? status { get; set; } = TrangThaiXuLy.ChuaXuLy;
        public bool? da_cong_bo { get; set; } = false;
        public UserModel? user_cong_bo { get; set; }

    }
    public class QLCLCoSoViPhamATTPCRUDModel : BaseDetailModel
    {
        public new TrangThaiXuLy? status { get; set; } = TrangThaiXuLy.ChuaXuLy;
        public int? co_so_che_bien_nlts { get; set; }
        public int? co_so_nlts_du_dieu_kien_attp { get; set; }
        public string? noi_dung_vi_pham { get; set; }
        public string? san_pham_vi_pham { get; set; }
        public int? hanh_vi_vi_pham { get; set; }
        public string? huong_xu_ly { get; set; }
        public decimal? phat_tien { get; set; }
        public int? hinh_thuc_xu_phat { get; set; }
        public KhacPhuc khac_phuc { get; set; }
        public decimal? so_luong { get; set; }
        public int? don_vi_tinh { get; set; }
        public decimal? gia_tri_tang_vat { get; set; }
        public TrangThaiXuLyKhac? xu_ly_khac { get; set; } = TrangThaiXuLyKhac.DinhChiLuuHanh;
        public DateTime? ngay_ghi_nhan { get; set; }
        public DateTime? ngay_xu_ly { get; set; }
        public string? co_quan_xu_ly { get; set; }
        public LoaiCoSoNLTS? loai_co_so { get; set; } = LoaiCoSoNLTS.CoSoCheBien;
        public bool? da_cong_bo { get; set; } = false;
        public string? user_cong_bo { get; set; }
    }
}
