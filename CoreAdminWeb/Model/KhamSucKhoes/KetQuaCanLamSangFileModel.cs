using CoreAdminWeb.Model.Base;

namespace CoreAdminWeb.Model.KhamSucKhoes
{
    public class KetQuaCanLamSangFileModel : BaseModel<int>
    {
        public string? ma_benh_nhan { get; set; }
        public string? ma_luot_kham { get; set; }
        public string? loai { get; set; }
        public string? url_file { get; set; }
        public Int64 HIS_ID { get; set; }
    }
}
