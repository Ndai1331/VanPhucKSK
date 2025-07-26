using CoreAdminWeb.Model.Base;

namespace CoreAdminWeb.Model
{
    public class KhamSucKhoeDinhMucThucTeModel : BaseModel<int>
    {
        public int? ma_dinh_muc { get; set; }
        public int? ma_dot_kham { get; set; }
        public decimal? du_kien_chi_phi { get; set; }
        public decimal? chi_phi_thuc_te { get; set; }
        public int? so_luong { get; set; }

        public SoKhamSucKhoeModel? luot_kham { get; set; }
    }
}