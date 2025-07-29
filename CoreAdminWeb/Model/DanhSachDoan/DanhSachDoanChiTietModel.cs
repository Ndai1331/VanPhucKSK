using CoreAdminWeb.Model.Base;

namespace CoreAdminWeb.Model.DanhSachDoan
{
    public class DanhSachDoanChiTietModel : BaseModel<int>
    {
        public KhamSucKhoeCongTyModel? ma_ho_so_kham_suc_khoe { get; set; }
        public string? ma_khach { get; set; }
        public string? ten_khach { get; set; }
        public string? ma_dieu_tri { get; set; }
        public DateTime? ngay_kham { get; set; }
    }
    public class DanhSachDoanChiTietCRUDModel : BaseDetailModel
    {
        public new string status { set; get; } = Status.published.ToString();
        public int? ma_ho_so_kham_suc_khoe { get; set; }
        public string? ma_khach { get; set; }
        public string? ten_khach { get; set; }
        public string? ma_dieu_tri { get; set; }
        public DateTime? ngay_kham { get; set; }
    }
}