using CoreAdminWeb.Model.Base;

namespace CoreAdminWeb.Model
{
    public class MedicalAgencyModel : BaseModel<int>
    {
        public int? ma_don_vi { get; set; }
        public Guid? y_te_co_quan { get; set; }
    }
}