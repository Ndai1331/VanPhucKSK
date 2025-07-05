using CoreAdminWeb.Model.Base;

namespace CoreAdminWeb.Model
{
    public class KhamSucKhoeKetLuanModel : BaseModel<int>
    {
        public string? ma_luot_kham { get; set; }
        public string? phan_loai_suc_khoe { get; set; }
        public string? benh_tat_ket_luan { get; set; }
        public string? nguoi_ket_luan { get; set; }
        public DateTime? ngay_ket_luan { get; set; }
        public string? chu_ky { get; set; }
        public string? de_nghi { get; set; }
        public Guid? file_url { get; set; }
    }
}