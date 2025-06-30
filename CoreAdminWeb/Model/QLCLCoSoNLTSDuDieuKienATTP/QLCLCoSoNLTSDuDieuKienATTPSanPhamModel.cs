using CoreAdminWeb.Model.Base;
using CoreAdminWeb.Model.User;

namespace CoreAdminWeb.Model
{
    public class QLCLCoSoNLTSDuDieuKienATTPSanPhamModel 
    {
        public int id { get; set; }
        public UserModel? user_created { get; set; }
        public DateTime date_created { get; set; } = DateTime.Now;
        public UserModel? user_updated { get; set; }
        public DateTime? date_updated { get; set; } = DateTime.Now;
        public QLCLCoSoNLTSDuDieuKienATTPModel? co_so_nlts_du_dieu_kien_attp { get; set; }
        public QLCLSanPhamSanXuatModel? san_pham { get; set; }
        public int? san_luong_tan { get; set; }
        public bool? deleted { get; set; } = false;
        public int? sort { get; set; } = 0;
    }
    public class QLCLCoSoNLTSDuDieuKienATTPSanPhamCRUDModel : BaseDetailModel
    {
        public int? co_so_nlts_du_dieu_kien_attp { get; set; }
        public int? san_pham { get; set; }
        public int? sort { get; set; } = 0;
        public bool? deleted { get; set; } = false;

    }
}
