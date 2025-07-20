using CoreAdminWeb.Model.Base;

namespace CoreAdminWeb.Model.Contract
{
    public class ContractDinhMucModel : BaseModel<int>
    {
        public ContractModel? contract { get; set; }
        public DinhMucModel? dinh_muc { get; set; }
        public decimal? so_luong { get; set; }
        public decimal? don_gia_tt { get; set; }
        public decimal? thanh_tien_tt { get; set; }
        public decimal? don_gia_dm { get; set; }
        public decimal? thanh_tien_dm { get; set; }
        public decimal? chi_phi_thuc_te { get; set; }
    }
    public class ContractDinhMucCRUDModel : BaseDetailModel
    {
        public new string status { set; get; } = Status.published.ToString();
        public int? contract { get; set; }
        public int? dinh_muc { get; set; }
        public decimal? so_luong { get; set; }
        public decimal? don_gia_tt { get; set; }
        public decimal? thanh_tien_tt { get; set; }
        public decimal? don_gia_dm { get; set; }
        public decimal? thanh_tien_dm { get; set; }
        public decimal? chi_phi_thuc_te { get; set; }
    }
}