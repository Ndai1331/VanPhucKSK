using CoreAdminWeb.Model.Base;
using CoreAdminWeb.Model.User;

namespace CoreAdminWeb.Model
{
    public class QLCLCoSoVatTuNongNghiepSanPhamModel 
    {
        public int id { get; set; }
        public UserModel? user_created { get; set; }
        public DateTime date_created { get; set; } = DateTime.Now;
        public UserModel? user_updated { get; set; }
        public DateTime? date_updated { get; set; } = DateTime.Now;
        public QLCLCoSoVatTuNongNghiepModel? co_so_vat_tu_nong_nghiep { get; set; }
        public QLCLSanPhamSanXuatModel? san_pham { get; set; }
        public bool? deleted { get; set; } = false;
        public int? sort { get; set; } = 0;
    }
    public class QLCLCoSoVatTuNongNghiepSanPhamCRUDModel
    {
        public int? co_so_vat_tu_nong_nghiep { get; set; }
        public int? san_pham { get; set; }
        public int? sort { get; set; } = 0;
        public bool? deleted { get; set; } = false;

    }
}
