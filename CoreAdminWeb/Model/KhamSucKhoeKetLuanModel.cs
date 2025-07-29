using CoreAdminWeb.Model.Base;
using CoreAdminWeb.Model.User;

namespace CoreAdminWeb.Model
{
    public class KhamSucKhoeKetLuanModel : BaseModel<int>
    {
        public string? ma_luot_kham { get; set; }
        public PhanLoaiSucKhoeModel? phan_loai_suc_khoe { get; set; }
        public string? benh_tat_ket_luan { get; set; }
        public string? nguoi_ket_luan { get; set; }
        public DateTime? ngay_ket_luan { get; set; }
        public string? chu_ky { get; set; }
        public string? de_nghi { get; set; }
        public FileModel? file_url { get; set; }

        public SoKhamSucKhoeModel? luot_kham { get; set; }
        public UserModel? bs_ket_luan { get; set; }
    }
    public class KhamSucKhoeKetLuanCRUDModel : BaseDetailModel
    {
        public string? ma_luot_kham { get; set; }
        public int? phan_loai_suc_khoe { get; set; }
        public string? benh_tat_ket_luan { get; set; }
        public string? nguoi_ket_luan { get; set; }
        public DateTime? ngay_ket_luan { get; set; }
        public string? chu_ky { get; set; }
        public string? de_nghi { get; set; }
        public Guid? file_url { get; set; }

        public int? luot_kham { get; set; }
        public Guid? bs_ket_luan { get; set; }
        public new string status { set; get; } = Status.published.ToString();
    }
}