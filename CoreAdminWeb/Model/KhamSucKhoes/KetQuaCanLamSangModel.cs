using CoreAdminWeb.Model.Base;

namespace CoreAdminWeb.Model.KhamSucKhoes
{
    public class KetQuaCanLamSangModel : BaseModel<int>
    {
        public DateTime? ngay_kham { get; set; }
        public DateTime? ngay_ket_thuc { get; set; }
        public string? ma_luot_kham { get; set; }
        public string? ma_can_lam_sang { get; set; }
        public string? ten_can_lam_san { get; set; }
        public string? ket_luan_can_lam_sang { get; set; }
        public string? ma_benh_nhan { get; set; }
        public int? IdCLS { get; set; }
        public bool? SendMail { get; set; } = false;
        public bool? SendNotification { get; set; } = false;
        public bool? SendZaloOa { get; set; } = false;
        public string? ma_loai_chi_dinh { get; set; }
        public FileModel? file_cls { get; set; }
    }
}
