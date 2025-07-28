using CoreAdminWeb.Model.Base;

namespace CoreAdminWeb.Model
{
    public class KhamSucKhoeTienSuModel : BaseModel<int>
    {
        public string? ma_luot_kham { get; set; }
        public string? tien_su_gia_dinh { get; set; }
        public string? ten_benh { get; set; }
        public int? nam_phat_hien { get; set; }
        public string? benh_nghe_nghiep { get; set; }
        public int? nam_phat_hien_benh_nghe_nghiep { get; set; }

        public SoKhamSucKhoeModel? luot_kham { get; set; }
    }

    public class KhamSucKhoeTienSuCRUDModel : BaseDetailModel
    {
        public string? ma_luot_kham { get; set; }
        public string? tien_su_gia_dinh { get; set; }
        public string? ten_benh { get; set; }
        public int? nam_phat_hien { get; set; }
        public string? benh_nghe_nghiep { get; set; }
        public int? nam_phat_hien_benh_nghe_nghiep { get; set; }

        public int? luot_kham { get; set; }
        public new string status { set; get; } = Status.published.ToString();
    }
}