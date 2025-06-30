using CoreAdminWeb.Model.Base;
using CoreAdminWeb.Enums;

namespace CoreAdminWeb.Model
{
    public class QLCLCoSoCheBienNLTSModel : BaseModel<int>
    {
        public TinhModel? province { get; set; }
        public XaPhuongModel? ward { get; set; }
        public Enums.PhamViHoatDong? pham_vi_hoat_dong { get; set; } = Enums.PhamViHoatDong.ToanQuoc;
        public QLCLLoaiHinhCoSoModel? loai_hinh_co_so { get; set; }
        public QLCLNguyenLieuCheBienModel? nguyen_lieu_che_bien { get; set; }
        public string? dia_chi { get; set; }
        public string? dien_thoai { get; set; }
        public string? dai_dien { get; set; }
        public decimal? cong_suat_tan_nam { get; set; }
        public decimal? san_luong_du_kien { get; set; }
        public QuyMoEnum quy_mo { get; set; } = QuyMoEnum.QuyMoNho;
        public string? so_giay_phep { get; set; }
        public DateTime? ngay_cap { get; set; }
        public string? co_quan_cap_phep { get; set; }
        public string? chung_nhan_attp { get; set; }

    }
    public class QLCLCoSoCheBienNLTSCRUDModel : BaseDetailModel
    {
        public new string status { get; set; } = Status.active.ToString();
        public int? province { get; set; }
        public int? ward { get; set; }
        public PhamViHoatDong? pham_vi_hoat_dong { get; set; } = PhamViHoatDong.ToanQuoc;
        public int? loai_hinh_co_so { get; set; }
        public int? nguyen_lieu_che_bien { get; set; }
        public string? dia_chi { get; set; }
        public string? dien_thoai { get; set; }
        public string? dai_dien { get; set; }
        public decimal? cong_suat_tan_nam { get; set; }
        public decimal? san_luong_du_kien { get; set; }
        public QuyMoEnum quy_mo { get; set; } = QuyMoEnum.QuyMoNho;
        public string? so_giay_phep { get; set; }
        public DateTime? ngay_cap { get; set; }
        public string? co_quan_cap_phep { get; set; }
        public string? chung_nhan_attp { get; set; }
    }
}
