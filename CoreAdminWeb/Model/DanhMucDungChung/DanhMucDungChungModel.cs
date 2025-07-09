using CoreAdminWeb.Model.Base;

namespace CoreAdminWeb.Model
{
    public class TinhModel : BaseModel<int>
    {
    }
    public class TinhCRUDModel : BaseDetailModel
    {

    }
    public class XaPhuongModel : BaseModel<int>
    {
        public TinhModel? tinh { get; set; }
    }
    public class XaPhuongCRUDModel : BaseDetailModel
    {
        public int? tinh { get; set; }
    }
    public class LoaiDinhMucModel : BaseModel<int>
    {
        public bool? active { get; set; } = true;
    }
    public class LoaiDinhMucCRUDModel : BaseDetailModel
    {
        public bool? active { get; set; } = true;
        public string status { get; set; } = Status.published.ToString();
    }
    public class DinhMucModel : BaseModel<int>
    {
        public decimal? du_kien_chi_phi { get; set; }
        public bool? active { get; set; } = true;
        public LoaiDinhMucModel? loai_dinh_muc { get; set; }
    }
    public class DinhMucCRUDModel : BaseDetailModel
    {
        public decimal? du_kien_chi_phi { get; set; }
        public bool? active { get; set; } = true;
        public int? loai_dinh_muc { get; set; }
        public string status { get; set; } = Status.published.ToString();
    }
    public class ContractTypeModel : BaseModel<int>
    {
    }
    public class ContractTypeCRUDModel : BaseDetailModel
    {
        public bool? active { get; set; } = true;
        public string status { get; set; } = Status.published.ToString();
    }
}