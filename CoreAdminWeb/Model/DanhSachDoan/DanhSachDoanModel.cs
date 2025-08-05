using CoreAdminWeb.Model.Base;

namespace CoreAdminWeb.Model
{
    public class MedicalExaminationDto
    {
        public string? user_id { get; set; }
        public string? ma_luot_kham { get; set; }
        public string? code { get; set; }
        public string? last_name { get; set; }
        public string? first_name { get; set; }
        public DateTime? ngay_sinh { get; set; }
        public string? gioi_tinh { get; set; }
        
        // Tiền sử bệnh
        public string? ten_benh { get; set; }
        public string? tien_su_gia_dinh { get; set; }
        
        // Thể lực
        public decimal? chieu_cao { get; set; }
        public decimal? can_nang { get; set; }
        public decimal? bmi { get; set; }
        public int? mach { get; set; }
        public string? huyet_ap { get; set; }
        
        // Khám chuyên khoa - Nội khoa
        public string? kq_nk_tuan_hoan { get; set; }
        public string? kq_nk_ho_hap { get; set; }
        public string? kq_nk_tieu_hoa { get; set; }
        public string? kq_nk_than_tiet_nieu { get; set; }
        public string? kq_nk_noi_tiet { get; set; }
        public string? kq_nk_co_xuong_khop { get; set; }
        public string? kq_nk_than_kinh { get; set; }
        public string? kq_nk_tam_than { get; set; }
        public string? kq_ngoai_khoa { get; set; }
        
        // Sản phụ khoa
        public string? ket_qua_san_phu_khoa { get; set; }
        
        // Các khám khác
        public string? benh_mat { get; set; }
        public string? benh_tai_mui_hong { get; set; }
        public string? benh_rhm { get; set; }
        public string? kq_da_lieu { get; set; }
        
        // Kết luận
        public string? benh_tat_ket_luan { get; set; }
        public string? de_nghi { get; set; }
        public string? phan_loai_suc_khoe { get; set; }
        
        // Cận lâm sàng (gộp tất cả kết quả)
        public string? can_lam_sang_results { get; set; }
    }

}