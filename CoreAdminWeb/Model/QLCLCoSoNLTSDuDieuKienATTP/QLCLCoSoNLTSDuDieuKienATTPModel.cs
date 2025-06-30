using CoreAdminWeb.Model.Base;

namespace CoreAdminWeb.Model
{
    public class QLCLCoSoNLTSDuDieuKienATTPModel : BaseModel<int>
    {
        public TinhModel? province { get; set; }
        public XaPhuongModel? ward { get; set; }
        public string? dia_chi { get; set; }
        public string? dien_thoai { get; set; }
        public string? dai_dien { get; set; }

        public Enums.LoaiCoSo? loai { get; set; } = Enums.LoaiCoSo.DuDieuKien;

        //đủ điều kiện
        public string? so_giay_chung_nhan { get; set; }
        public QLCLLoaiHinhKinhDoanhModel? loai_hinh_kinh_doanh { get; set; }
        public DateTime? ngay_cap { get; set; }
        public DateTime? ngay_het_hieu_luc { get; set; }
        public DateTime? ngay_tham_dinh { get; set; }
        public string? co_quan_cap { get; set; }
        public string? xu_ly_ket_qua { get; set; }
        public string? he_thong_quan_ly_chat_luong { get; set; }
        public Enums.KetQuaKiemTraDinhKy? ket_qua_tham_dinh { get; set; } = Enums.KetQuaKiemTraDinhKy.Dat;



        //không đủ điều kiện
        public Enums.TuanThuQuyDinh? tuan_thu_quy_dinh { get; set; } = Enums.TuanThuQuyDinh.KyCamKet;
        public Enums.KiemTraTuanThu? kiem_tra_tuan_thu { get; set; } = Enums.KiemTraTuanThu.ChuaDuocKiemTra;
        public Enums.TinhHinhViPham? tinh_hinh_vi_pham { get; set; } = Enums.TinhHinhViPham.Khong;
        public string? so_giay_phep_hoat_dong { get; set; }
        public DateTime? ngay_cap_giay_phep_hoat_dong { get; set; }
        public string? co_quan_cap_giay_phep_hoat_dong { get; set; }
        public Enums.QuyMoEnum? quy_mo { get; set; } = Enums.QuyMoEnum.QuyMoNho;

        public List<QLCLCoSoNLTSDuDieuKienATTPSanPhamModel>? chi_tiets { get; set; }


    }
    public class QLCLCoSoNLTSDuDieuKienATTPCRUDModel : BaseDetailModel
    {
        public new string status { get; set; } = Status.active.ToString();
        public int? province { get; set; }
        public int? ward { get; set; }
        public int? loai_hinh_kinh_doanh { get; set; }
        public string? dia_chi { get; set; }
        public string? dien_thoai { get; set; }
        public string? dai_dien { get; set; }

        public Enums.LoaiCoSo? loai { get; set; } = Enums.LoaiCoSo.DuDieuKien;


        //đủ điều kiện
        public string? so_giay_chung_nhan { get; set; }
        public DateTime? ngay_cap { get; set; }
        public DateTime? ngay_het_hieu_luc { get; set; }
        public DateTime? ngay_tham_dinh { get; set; }
        public string? co_quan_cap { get; set; }
        public string? xu_ly_ket_qua { get; set; }
        public string? he_thong_quan_ly_chat_luong { get; set; }
        public Enums.KetQuaKiemTraDinhKy? ket_qua_tham_dinh { get; set; } = Enums.KetQuaKiemTraDinhKy.Dat;


        //không đủ điều kiện
        public Enums.TuanThuQuyDinh? tuan_thu_quy_dinh { get; set; } = Enums.TuanThuQuyDinh.KyCamKet;
        public Enums.KiemTraTuanThu? kiem_tra_tuan_thu { get; set; } = Enums.KiemTraTuanThu.ChuaDuocKiemTra;
        public Enums.TinhHinhViPham? tinh_hinh_vi_pham { get; set; } = Enums.TinhHinhViPham.Khong;
        public string? so_giay_phep_hoat_dong { get; set; }
        public DateTime? ngay_cap_giay_phep_hoat_dong { get; set; }
        public string? co_quan_cap_giay_phep_hoat_dong { get; set; }
        public Enums.QuyMoEnum? quy_mo { get; set; } = Enums.QuyMoEnum.QuyMoNho;
    }


    public class QLCLCoSoNLTSDuDieuKienATTPCRUDResponseModel
    {
        public int id { get; set; }
    }
}
