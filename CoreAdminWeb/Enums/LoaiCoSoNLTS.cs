using System.ComponentModel;

namespace CoreAdminWeb.Enums
{
    public enum LoaiCoSoNLTS
    {
        [Description("Cơ sở chế biến")]
        CoSoCheBien = 1,
        [Description("Cơ sở XSKD đủ điều kiện")]
        CoSoXSKDDuDieuKien,
        [Description("Cơ sở XSKD không đủ điều kiện")]
        CoSoXSKDKhongDuDieuKien,
    }
}
