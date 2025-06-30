using System.ComponentModel;

namespace CoreAdminWeb.Enums
{
    public enum HinhThucQuangBa
    {
        [Description("Trực tiếp")]
        TrucTiep = 1,
        [Description("Trực tuyến")]
        TrucTuyen,
        [Description("Kết hợp")]
        KetHop,
    }
}
