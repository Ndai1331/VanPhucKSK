using CoreAdminWeb.Model.Base;

namespace CoreAdminWeb.Model
{
    public class ContractModel : BaseModel<int>
    {
        public int? cong_ty_id { get; set; }
        public DateTime? ngay_hop_dong { get; set; }
        public DateTime? ngay_hieu_luc { get; set; }
        public DateTime? ngay_het_han { get; set; }
        public decimal? gia_tri_hop_dong { get; set; }
        public decimal? so_tien_tam_ung { get; set; }
        public decimal? ti_le_chap_nhan_thanh_toan { get; set; }
        public int? phe_duyet { get; set; }
        public int? han_che { get; set; }
        public int? khoa_phong { get; set; }
        public string? nguoi_theo_doi { get; set; }
        public string? so_dien_thoai_lien_he { get; set; }
        public int? nhan_vien_id { get; set; }
    }
}