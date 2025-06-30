using CoreAdminWeb.Model.Base;
using CoreAdminWeb.Model.User;
using System.Text.Json.Serialization;

namespace CoreAdminWeb.Model
{
    /// <summary>
    /// Model representing a fertilizer production facility
    /// </summary>
    public class QLCLTinhHinhSXKDNLTSModel
    {
        public UserModel? user_created { get; set; }
        public DateTime date_created { get; set; } = DateTime.Now;
        public UserModel? user_updated { get; set; }
        public DateTime? date_updated { get; set; }
        public int? id { get; set; }
        public DateTime? ngay_ghi_nhan { get; set; }
        public DateTime? thoi_gian_bat_dau { get; set; }
        public DateTime? thoi_gian_ket_thuc { get; set; }
        public QLCLCoSoCheBienNLTSModel? qlcl_co_so_che_bien_nlts { get; set; }
        public string? su_co_an_toan { get; set; }
        public string? bien_phap_xu_ly_chat_thai { get; set; }
        public bool? deleted { get; set; } = false;
        public int? sort { get; set; } = 0;
    }

    /// <summary>
    /// Model for CRUD operations on fertilizer production facilities
    /// </summary>
    public class QLCLTinhHinhSXKDNLTSCRUDModel
    {
        public int? qlcl_co_so_che_bien_nlts { get; set; }
        public DateTime? ngay_ghi_nhan { get; set; }
        public DateTime? thoi_gian_bat_dau { get; set; }
        public DateTime? thoi_gian_ket_thuc { get; set; }
        public string? su_co_an_toan { get; set; }
        public string? bien_phap_xu_ly_chat_thai { get; set; }
        public bool? deleted { get; set; } = false;
        public int? sort { get; set; } = 0;

    }

    public class QLCLTinhHinhSXKDNLTSCRUDResponseModel
    {
        public int? id { get; set; }
    }

}
