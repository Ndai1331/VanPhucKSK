using CoreAdminWeb.Model.Base;

namespace CoreAdminWeb.Model
{
    public class TinhModel : BaseModel<int>
    {
        public string? ma { get; set; }
        public string? ten { get; set; }
    }
    public class TinhCRUDModel : BaseDetailModel
    {
        public string? ma { get; set; }
        public string? ten { get; set; }
    }
    public class XaPhuongModel : BaseModel<int>
    {
        public string? ma { get; set; }
        public string? ten { get; set; }
        public TinhModel? tinh { get; set; }
    }
    public class XaPhuongCRUDModel : BaseDetailModel
    {
        public string? ma { get; set; }
        public string? ten { get; set; }
        public int? tinh { get; set; }
    }
    public class LoaiDinhMucModel : BaseModel<int>
    {
        public new bool? active { get; set; } = true;
        public bool? la_dich_vu_ky_thuat { get; set; } = false;
    }
    public class LoaiDinhMucCRUDModel
    {
        public string? code { get; set; }
        public string? name { get; set; }
        public bool? deleted { get; set; } = false;
        public string? description { get; set; }
        public bool? active { get; set; } = true;
        public string status { get; set; } = Status.published.ToString();
        public bool? la_dich_vu_ky_thuat { get; set; } = false;
    }
    public class PhanLoaiSucKhoeModel : BaseModel<int>
    {
        public new bool? active { get; set; } = true;
    }
    public class PhanLoaiSucKhoeCRUDModel : BaseDetailModel
    {
        public new bool? active { get; set; } = true;
        public new string status { get; set; } = Status.published.ToString();
    }
    public class DinhMucModel : BaseModel<int>
    {
        public decimal? DinhMuc { get; set; }
        public decimal? DonGia { get; set; }
        public new bool? active { get; set; } = true;
        public LoaiDinhMucModel? loai_dinh_muc { get; set; }
    }
    public class DinhMucCRUDModel : BaseDetailModel
    {
        public decimal? DinhMuc { get; set; }
        public decimal? DonGia { get; set; }
        public new bool? active { get; set; } = true;
        public int? loai_dinh_muc { get; set; }
        public new string status { get; set; } = Status.published.ToString();
    }
    public class ContractTypeModel : BaseModel<int>
    {
    }
    public class ContractTypeCRUDModel
    {
        public string? code { get; set; }
        public string? name { get; set; }
        public bool? deleted { get; set; } = false;
        public string? description { get; set; }
        public string status { get; set; } = Status.published.ToString();
    }
}