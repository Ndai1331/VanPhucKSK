using CoreAdminWeb.Model.Base;

namespace CoreAdminWeb.Model.KhamSucKhoes
{
    public class KhamSucKhoeKetQuaCanLamSangModel : BaseModel<int>
    {
        public string? ket_qua { get; set; }
        public string? type { get; set; }

        public KhamSucKhoeCanLamSangModel? ket_qua_cls { get; set; }
        public KetQuaCanLamSangModel? kq_cls { get; set; }
        public SoKhamSucKhoeModel? luot_kham { get; set; }
    }

    public class KhamSucKhoeKetQuaCanLamSangCRUDModel : BaseDetailModel
    {
        public new string status { set; get; } = Status.published.ToString();
        public string? ket_qua { get; set; }
        public string? type { get; set; }

        public int? ket_qua_cls { get; set; }
        public int? kq_cls { get; set; }
        public int? luot_kham { get; set; }
    }
}