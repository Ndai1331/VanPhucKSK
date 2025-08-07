using CoreAdminWeb.Enums;
using CoreAdminWeb.Model.Base;
using CoreAdminWeb.Model.Contract;
using CoreAdminWeb.Model.User;

namespace CoreAdminWeb.Model
{
    public class KhamSucKhoeCongTyModel : BaseModel<int>
    {
        public string? ma_don_vi { get; set; }
        public int? so_luong_du_kien { get; set; }
        public int? so_luong_thuc_te { get; set; }
        public DateTime? ngay_du_kien_kham { get; set; }
        public DateTime? ngay_ket_thuc { get; set; }
        public UserModel? bs_tuan_hoan { get; set; }
        public string? chu_ky_tuan_hoan { get; set; }
        public UserModel? bs_ho_hap { get; set; }
        public string? chu_ky_ho_hap { get; set; }
        public UserModel? bs_tieu_hoa { get; set; }
        public string? chu_ky_tieu_hoa { get; set; }
        public UserModel? bs_than_tiet_nieu { get; set; }
        public string? chu_ky_than_tiet_nieu { get; set; }
        public UserModel? bs_noi_tiet { get; set; }
        public string? chu_ky_noi_tiet { get; set; }
        public UserModel? bs_co_xuong_khop { get; set; }
        public string? chu_ky_co_xuong_khop { get; set; }
        public UserModel? bs_than_kinh { get; set; }
        public string? chu_ky_than_kinh { get; set; }
        public UserModel? bs_tam_than { get; set; }
        public string? chu_ky_tam_than { get; set; }
        public UserModel? bs_ngoai_khoa { get; set; }
        public string? chu_ky_ngoai_khoa { get; set; }
        public UserModel? bs_mat { get; set; }
        public string? chu_ky_mat { get; set; }
        public UserModel? bs_tai_mui_hong { get; set; }
        public string? chu_ky_tai_mui_hong { get; set; }
        public UserModel? bs_rang_ham_mat { get; set; }
        public string? chu_ky_rang_ham_mat { get; set; }
        public UserModel? bs_san_phu_khoa { get; set; }
        public string? chu_ky_san_phu_khoa { get; set; }
        public UserModel? bs_ket_luan { get; set; }
        public string? chu_ky_ket_luan { get; set; }
        public string? Ksk_status { get; set; }

        public ContractModel? ma_hop_dong_ksk { get; set; }
        public string? chu_ky_lap_so { get; set; }
        public UserModel? nguoi_lap_so { get; set; }
        public new Status status { get; set; } = Status.published;
    }
    public class KhamSucKhoeCongTyCRUDModel : BaseDetailModel
    {
        public string? ma_don_vi { get; set; }
        public int? so_luong_du_kien { get; set; }
        public int? so_luong_thuc_te { get; set; }
        public DateTime? ngay_du_kien_kham { get; set; }
        public DateTime? ngay_ket_thuc { get; set; }
        public Guid? bs_tuan_hoan { get; set; }
        public string? chu_ky_tuan_hoan { get; set; }
        public Guid? bs_ho_hap { get; set; }
        public string? chu_ky_ho_hap { get; set; }
        public Guid? bs_tieu_hoa { get; set; }
        public string? chu_ky_tieu_hoa { get; set; }
        public Guid? bs_than_tiet_nieu { get; set; }
        public string? chu_ky_than_tiet_nieu { get; set; }
        public Guid? bs_noi_tiet { get; set; }
        public string? chu_ky_noi_tiet { get; set; }
        public Guid? bs_co_xuong_khop { get; set; }
        public string? chu_ky_co_xuong_khop { get; set; }
        public Guid? bs_than_kinh { get; set; }
        public string? chu_ky_than_kinh { get; set; }
        public Guid? bs_tam_than { get; set; }
        public string? chu_ky_tam_than { get; set; }
        public Guid? bs_ngoai_khoa { get; set; }
        public string? chu_ky_ngoai_khoa { get; set; }
        public Guid? bs_mat { get; set; }
        public string? chu_ky_mat { get; set; }
        public Guid? bs_tai_mui_hong { get; set; }
        public string? chu_ky_tai_mui_hong { get; set; }
        public Guid? bs_rang_ham_mat { get; set; }
        public string? chu_ky_rang_ham_mat { get; set; }
        public Guid? bs_san_phu_khoa { get; set; }
        public string? chu_ky_san_phu_khoa { get; set; }
        public Guid? bs_ket_luan { get; set; }
        public string? chu_ky_ket_luan { get; set; }
        public string? Ksk_status { get; set; }
        public new string status { get; set; } = Status.published.ToString();
        public int? ma_hop_dong_ksk { get; set; }
        public string? chu_ky_lap_so { get; set; }
        public Guid? nguoi_lap_so { get; set; }
    }
}