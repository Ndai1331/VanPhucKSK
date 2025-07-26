using CoreAdminWeb.Model.Base;

namespace CoreAdminWeb.Model
{
    public class KhamSucKhoeTheLucModel : BaseModel<int>
    {
        public string? ma_luot_kham { get; set; }
        public decimal? chieu_cao { get; set; }
        public decimal? can_nang { get; set; }
        public decimal? bmi { get; set; }
        public int? mach { get; set; }
        public string? huyet_ap { get; set; }
        public string? phan_loai { get; set; }

        public SoKhamSucKhoeModel? luot_kham { get; set; }
    }
}