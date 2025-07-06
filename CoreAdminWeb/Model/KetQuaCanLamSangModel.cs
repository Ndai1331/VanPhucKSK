using CoreAdminWeb.Model.Base;

namespace CoreAdminWeb.Model
{
    public class KetQuaCanLamSangModel : BaseModel<int>
    {
        public DateTime? ngay_kham { get; set; }
        public string? ma_can_lam_sang { get; set; }
        public string? ma_luot_kham { get; set; }
        public string? ten_can_lam_sang { get; set; }
        public string? ket_luan_can_lam_sang { get; set; }
        public string? ket_quan_url { get; set; }
    }
}