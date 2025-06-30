using System.ComponentModel;

namespace CoreAdminWeb.Enums
{
    public enum KhaNangAnhHuong
    {
        [Description("Mức nhẹ")]
        MucNhe = 1,
        [Description("Mức trung bình")]
        MucTrungBinh,
        [Description("Mức nặng")]
        MucNang,
        [Description("Mất trắng")]
        MatTrang
    }
}
