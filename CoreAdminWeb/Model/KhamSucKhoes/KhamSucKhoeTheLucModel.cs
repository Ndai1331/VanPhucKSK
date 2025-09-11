using CoreAdminWeb.Model.Base;

namespace CoreAdminWeb.Model.KhamSucKhoes
{
    public class KhamSucKhoeTheLucModel : BaseModel<int>
    {
        public string? ma_luot_kham { get; set; }
        public decimal? chieu_cao { get; set; }
        public decimal? can_nang { get; set; }
        public decimal? bmi { get; set; }
        public decimal? nhip_tho { get; set; }
        public int? mach { get; set; }
        public string? huyet_ap { get; set; }
        public decimal? abi { get; set; }
        public PhanLoaiSucKhoeModel? phan_loai { get; set; }

        public SoKhamSucKhoeModel? luot_kham { get; set; }
    }

    public class KhamSucKhoeTheLucCRUDModel
    {
        public bool? deleted { get; set; } = false;
        public int? sort { get; set; } = 0;
        public string? ma_luot_kham { get; set; }
        public decimal? chieu_cao { get; set; }
        public decimal? can_nang { get; set; }
        public decimal? bmi { get; set; }
        public decimal? nhip_tho { get; set; }
        public int? mach { get; set; }
        public string? huyet_ap { get; set; }
        public decimal? abi { get; set; }

        public int? phan_loai { get; set; }

        public int? luot_kham { get; set; }
        public string status { set; get; } = Status.published.ToString();
    }
}