using System.ComponentModel;

namespace CoreAdminWeb.Enums
{
    public enum PhamViTiepCan
    {
        [Description("Địa phương")]
        DiaPhuong = 1,
        [Description("Trong nước")]
        TrongNuoc,
        [Description("Quốc tế")]
        QuocTe,
    }
}
