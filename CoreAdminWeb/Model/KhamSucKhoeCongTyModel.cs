using CoreAdminWeb.Model.Base;

namespace CoreAdminWeb.Model
{
    public class KhamSucKhoeCongTyModel : BaseModel<int>
    {
        public int? ma_don_vi { get; set; }
        public int? ma_hop_dong_ksk { get; set; }
        public int? so_luong_du_kien { get; set; }
        public int? so_luong_thuc_te { get; set; }
        public DateTime? ngay_du_kien_kham { get; set; }
        public DateTime? ngay_ket_thuc { get; set; }
        public string? bs_tuan_hoan { get; set; }
        public string? chu_ky_tuan_hoan { get; set; }
        public string? bs_ho_hap { get; set; }
        public string? chu_ky_ho_hap { get; set; }
        public string? bs_tieu_hoa { get; set; }
        public string? chu_ky_tieu_hoa { get; set; }
        public string? bs_than_tiet_nieu { get; set; }
        public string? chu_ky_than_tiet_nieu { get; set; }
        public string? bs_noi_tiet { get; set; }
        public string? chu_ky_noi_tiet { get; set; }
        public string? bs_co_xuong_khop { get; set; }
        public string? chu_ky_co_xuong_khop { get; set; }
        public string? bs_than_kinh { get; set; }
        public string? chu_ky_than_kinh { get; set; }
        public string? bs_tam_than { get; set; }
        public string? chu_ky_tam_than { get; set; }
        public string? bs_ngoai_khoa { get; set; }
        public string? chu_ky_ngoai_khoa { get; set; }
        public string? bs_mat { get; set; }
        public string? chu_ky_mat { get; set; }
        public string? bs_tai_mui_hong { get; set; }
        public string? chu_ky_tai_mui_hong { get; set; }
        public string? bs_rang_ham_mat { get; set; }
        public string? chu_ky_rang_ham_mat { get; set; }
        public string? bs_san_phu_khoa { get; set; }
        public string? chu_ky_san_phu_khoa { get; set; }
        public string? bs_ket_luan { get; set; }
        public string? chu_ky_ket_luan { get; set; }
        public string? Ksk_status { get; set; }
    }
}