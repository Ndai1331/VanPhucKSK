using CoreAdminWeb.Model.Base;
using CoreAdminWeb.Model.User;

namespace CoreAdminWeb.Model
{
    /// <summary>
    /// Model representing a fertilizer production facility
    /// </summary>
    public class QLCLTinhHinhSXKDNLTSNguyenLieuModel
    {
        public int? id { get; set; }
        public UserModel? user_created { get; set; }
        public DateTime date_created { get; set; } = DateTime.Now;
        public UserModel? user_updated { get; set; }
        public DateTime? date_updated { get; set; }
        public QLCLTinhHinhSXKDNLTSModel? tinh_hinh_san_xuat_kinh_doanh_nlts { get; set; }
        public QLCLNguyenLieuCheBienModel? nguyen_lieu { get; set; }
        public decimal? khoi_luong_tan { get; set; } 
        public bool? deleted { get; set; } 
        public int? sort { get; set; } 

    }

    /// <summary>
    /// Model for CRUD operations on fertilizer production facilities
    /// </summary>
    public class QLCLTinhHinhSXKDNLTSNguyenLieuCRUDModel
    {
        public int? tinh_hinh_san_xuat_kinh_doanh_nlts { get; set; }
        public int? nguyen_lieu { get; set; }
        public decimal? khoi_luong_tan { get; set; }
        public bool? deleted { get; set; } 
        public int? sort { get; set; } 

    }

}
