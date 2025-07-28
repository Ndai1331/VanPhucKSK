using CoreAdminWeb.Model.Base;
using CoreAdminWeb.Model.User;

namespace CoreAdminWeb.Model.Contract
{
    public class ContractModel : BaseModel<int>
    {
        public CongTyModel? cong_ty { get; set; }
        public ContractTypeModel? contract_type { get; set; }
        public DateTime? ngay_hop_dong { get; set; }
        public DateTime? ngay_hieu_luc { get; set; }
        public DateTime? ngay_het_han { get; set; }
        public decimal? gia_tri_hop_dong { get; set; }
        public decimal? so_tien_tam_ung { get; set; }
        public decimal? ti_le_chap_nhan_thanh_toan { get; set; }
        public int? phe_duyet { get; set; }
        public int? han_che { get; set; }
        // public string? ten_khoa_phong { get; set; }
        public string? nguoi_theo_doi { get; set; }
        public string? so_dien_thoai_lien_he { get; set; }
        // public string? nguoi_gioi_thieu { get; set; }
        // public string? dien_thoai { get; set; }
        // public string? email { get; set; }
        public FileModel? file_hd { get; set; }
        public UserModel? nhan_vien_id { get; set; }
        public List<ContractDinhMucModel>? chi_tiet { get; set; }
    }
    public class ContractCRUDModel : BaseDetailModel
    {
        public new string status { set; get; } = Status.published.ToString();
        public int? cong_ty { get; set; }
        public int? contract_type { get; set; }
        public DateTime? ngay_hop_dong { get; set; }
        public DateTime? ngay_hieu_luc { get; set; }
        public DateTime? ngay_het_han { get; set; }
        public decimal? gia_tri_hop_dong { get; set; }
        public decimal? so_tien_tam_ung { get; set; }
        public decimal? ti_le_chap_nhan_thanh_toan { get; set; }
        public int? phe_duyet { get; set; }
        public int? han_che { get; set; }
        public string? ten_khoa_phong { get; set; }
        public string? nguoi_theo_doi { get; set; }
        public string? so_dien_thoai_lien_he { get; set; }
        public string? nguoi_gioi_thieu { get; set; }
        public string? dien_thoai { get; set; }
        public string? email { get; set; }
        public string? file_hd { get; set; }
        public Guid? nhan_vien_id { get; set; }
    }
}