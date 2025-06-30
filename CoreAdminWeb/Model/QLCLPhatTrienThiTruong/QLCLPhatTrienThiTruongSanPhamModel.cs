using CoreAdminWeb.Model.Base;
using CoreAdminWeb.Model.User;

namespace CoreAdminWeb.Model
{
    public class QLCLPhatTrienThiTruongSanPhamModel 
    {
        public int id { get; set; }
        public UserModel? user_created { get; set; }
        public DateTime date_created { get; set; } = DateTime.Now;
        public UserModel? user_updated { get; set; }
        public DateTime? date_updated { get; set; } = DateTime.Now;
        public QLCLPhatTrienThiTruongModel? phat_trien_thi_truong { get; set; }
        public QLCLSanPhamSanXuatModel? san_pham { get; set; }
        public int? so_luong { get; set; }
        public string? description { get; set; }
        public bool? deleted { get; set; } = false;
        public int? sort { get; set; } = 0;
    }
    public class QLCLPhatTrienThiTruongSanPhamCRUDModel : BaseDetailModel
    {
        public int? phat_trien_thi_truong { get; set; }
        public int? san_pham { get; set; }
        public int? so_luong { get; set; } = 0;
    }
}
