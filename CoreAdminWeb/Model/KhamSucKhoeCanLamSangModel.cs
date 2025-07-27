using CoreAdminWeb.Model.Base;

namespace CoreAdminWeb.Model
{
    public class KhamSucKhoeCanLamSangModel : BaseModel<int>
    {
        public string? ma_luot_kham { get; set; }
        public string? ma_cls { get; set; }
        public string? ten_cls { get; set; }
        public string? ket_qua_cls { get; set; }
        public string? danh_gia_cls { get; set; }
        public string? chi_so_cls { get; set; }
        public FileModel? file_cls { get; set; }
        public string? url_cls { get; set; }
        public string? bs_cls { get; set; }
        public string? chu_ky_cls { get; set; }
        public new bool deleted { get; set; } = false;
        public bool SendMail { get; set; } = false;
        public bool SendNotification { get; set; } = false;
        public bool SendZaloOa { get; set; } = false;

        public SoKhamSucKhoeModel? luot_kham { get; set; }
    }

    public class KhamSucKhoeCanLamSangCRUDModel : BaseDetailModel
    {
        public new string status { set; get; } = Status.published.ToString();
        public string? ma_luot_kham { get; set; }
        public string? ma_cls { get; set; }
        public string? ten_cls { get; set; }
        public string? ket_qua_cls { get; set; }
        public string? danh_gia_cls { get; set; }
        public string? chi_so_cls { get; set; }
        public Guid? file_cls { get; set; }
        public string? url_cls { get; set; }
        public string? bs_cls { get; set; }
        public string? chu_ky_cls { get; set; }
        public new bool deleted { get; set; } = false;

        public int? luot_kham { get; set; }
        public bool SendMail { get; set; } = false;
        public bool SendNotification { get; set; } = false;
        public bool SendZaloOa { get; set; } = false;
    }
}