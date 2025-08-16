using CoreAdminWeb.Model.Base;

namespace CoreAdminWeb.Model.KhamSucKhoes
{
    public class KhamSucKhoeNgheNghiepModel : BaseModel<int>
    {
        public string? ma_luot_kham { get; set; }
        public string? nghe_nghiep { get; set; }
        public string? noi_cong_tac { get; set; }
        public DateTime? ngay_vao_lam { get; set; }
        public DateTime? den_ngay { get; set; }
        public new bool deleted { get; set; } = false;

        public SoKhamSucKhoeModel? luot_kham { get; set; }
    }

    public class KhamSucKhoeNgheNghiepCRUDModel : BaseDetailModel
    {
        public string? ma_luot_kham { get; set; }
        public string? nghe_nghiep { get; set; }
        public string? noi_cong_tac { get; set; }
        public DateTime? ngay_vao_lam { get; set; }
        public DateTime? den_ngay { get; set; }
        public new bool deleted { get; set; } = false;

        public int? luot_kham { get; set; }
        public new string status { set; get; } = Status.published.ToString();
    }
}