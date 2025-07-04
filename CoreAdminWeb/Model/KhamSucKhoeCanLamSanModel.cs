using CoreAdminWeb.Model.Base;

namespace CoreAdminWeb.Model
{
    public class KhamSucKhoeCanLamSanModel : BaseModel<int>
    {
        public string? ma_luot_kham { get; set; }
        public string? ket_qua_cls { get; set; }
        public string? danh_gia_cls { get; set; }
        public string? chi_so_cls { get; set; }
        public string? file_cls { get; set; }
        public string? url_cls { get; set; }
        public string? bs_cls { get; set; }
        public string? chu_ky_cls { get; set; }
        public new bool deleted { get; set; } = false;

    }

    public class KhamSucKhoeCanLamSanCRUDModel : BaseDetailModel
    {
        public new string status { set; get; } = Status.published.ToString();
        public string? ma_luot_kham { get; set; }
        public string? ket_qua_cls { get; set; }
        public string? danh_gia_cls { get; set; }
        public string? chi_so_cls { get; set; }
        public string? file_cls { get; set; }
        public string? url_cls { get; set; }
        public string? bs_cls { get; set; }
        public string? chu_ky_cls { get; set; }
        public new bool deleted { get; set; } = false;
    }
}