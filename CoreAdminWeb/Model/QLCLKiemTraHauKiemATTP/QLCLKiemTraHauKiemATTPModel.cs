using CoreAdminWeb.Model.Base;
using System.Text.Json.Serialization;

namespace CoreAdminWeb.Model
{
    public class QLCLKiemTraHauKiemATTPModel : BaseModel<int>
    {
        public QLCLCoSoNLTSDuDieuKienATTPModel? co_so { get; set; }
        public TinhModel? province { get; set; }
        public XaPhuongModel? ward { get; set; }
        public QLCLDotKiemTraHauKiemATTPModel? dot_kiem_tra { get; set; }
        public string? dia_chi_san_xuat_kinh_doanh { get; set; }
        public Enums.LoaiHinhKiemTra? loai_hinh_kiem_tra { get; set; } = Enums.LoaiHinhKiemTra.KiemTraBanDau;
        public Enums.HinhThucXetNghiem? hinh_thuc_xet_nghiem { get; set; } = Enums.HinhThucXetNghiem.XetNghiemTaiPhongKiemNghiem;
        public DateTime? ngay_kiem_tra { get; set; }
        public string? co_quan_kiem_tra { get; set; }
        public string? noi_dung_kiem_tra { get; set; }
        public string? bien_phap_xu_ly { get; set; }
        public Enums.KetQuaKiemTraDinhKy? ket_qua_kiem_tra { get; set; } = Enums.KetQuaKiemTraDinhKy.Dat;
        public Enums.TinhHinhViPham? tinh_hinh_vi_pham { get; set; } = Enums.TinhHinhViPham.Khong;

        [JsonIgnore]
        public Enums.LoaiCoSo? loai_co_so { get; set; } = Enums.LoaiCoSo.DuDieuKien;
        public List<QLCLKiemTraHauKiemATTPChiTietModel>? chi_tiet { get; set; }

    }
    public class QLCLKiemTraHauKiemATTPCRUDModel : BaseDetailModel
    {
        public new string status { get; set; } = Status.active.ToString();
        public int? co_so { get; set; }
        public int? province { get; set; }
        public int? ward { get; set; }
        public int? dot_kiem_tra { get; set; }
        public string? dia_chi_san_xuat_kinh_doanh { get; set; }
        public Enums.LoaiHinhKiemTra? loai_hinh_kiem_tra { get; set; } = Enums.LoaiHinhKiemTra.KiemTraBanDau;
        public Enums.HinhThucXetNghiem? hinh_thuc_xet_nghiem { get; set; } = Enums.HinhThucXetNghiem.XetNghiemTaiPhongKiemNghiem;
        public DateTime? ngay_kiem_tra { get; set; }
        public string? co_quan_kiem_tra { get; set; }
        public string? noi_dung_kiem_tra { get; set; }
        public Enums.KetQuaKiemTraDinhKy? ket_qua_kiem_tra { get; set; } = Enums.KetQuaKiemTraDinhKy.Dat;
        public Enums.TinhHinhViPham? tinh_hinh_vi_pham { get; set; } = Enums.TinhHinhViPham.Khong;
        public string? bien_phap_xu_ly { get; set; }
    }


    public class QLCLKiemTraHauKiemATTPCRUDResponseModel
    {
        public int id { get; set; }
    }
}
