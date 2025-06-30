using CoreAdminWeb.Enums;
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
}