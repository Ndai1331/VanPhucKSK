using CoreAdminWeb.Model.Base;
using CoreAdminWeb.Model.User;

namespace CoreAdminWeb.Model
{
    public class QLCLKiemTraHauKiemATTPChiTietModel 
    {
        public int id { get; set; }
        public UserModel? user_created { get; set; }
        public DateTime date_created { get; set; } = DateTime.Now;
        public UserModel? user_updated { get; set; }
        public DateTime? date_updated { get; set; } = DateTime.Now;
        public QLCLKiemTraHauKiemATTPModel? kiem_tra_hau_kiem_attp { get; set; }
        public QLCLSanPhamSanXuatModel? san_pham { get; set; }
        public int? so_luong_mau { get; set; }
        public Enums.LoaiXetNghiem? loai_xet_nghiem { get; set; } = Enums.LoaiXetNghiem.HoaSinh;
        public Enums.MauGoc? mau_goc { get; set; } = Enums.MauGoc.MauGoc;
        public Enums.ChiTieu? chi_tieu { get; set; } = Enums.ChiTieu.ChiTieuViSinh;
        public int? so_mau_khong_dat { get; set; }
        public string? chi_tieu_vi_pham { get; set; }
        public string? muc_phat_hien { get; set; }
        public bool? deleted { get; set; } = false;
        public int? sort { get; set; } = 0;
    }
    public class QLCLKiemTraHauKiemATTPChiTietCRUDModel : BaseDetailModel
    {
        public int? kiem_tra_hau_kiem_attp { get; set; }
        public int? san_pham { get; set; }
        public int? so_luong_mau { get; set; }
        public Enums.LoaiXetNghiem? loai_xet_nghiem { get; set; } = Enums.LoaiXetNghiem.HoaSinh;
        public Enums.MauGoc? mau_goc { get; set; } = Enums.MauGoc.MauGoc;
        public Enums.ChiTieu? chi_tieu { get; set; } = Enums.ChiTieu.ChiTieuViSinh;
        public int? so_mau_khong_dat { get; set; }
        public string? chi_tieu_vi_pham { get; set; }
        public string? muc_phat_hien { get; set; }
        public int? sort { get; set; } = 0;
        public bool? deleted { get; set; } = false;

    }
}
