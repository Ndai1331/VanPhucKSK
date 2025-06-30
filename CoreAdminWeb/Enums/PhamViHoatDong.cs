using System.ComponentModel;

namespace CoreAdminWeb.Enums
{
    public enum PhamViHoatDong
    {
        [Description("Toàn quốc")]
        ToanQuoc = 1, 
        [Description("Toàn tỉnh")]
        ToanTinh,
        [Description("Khu vực miền trung")]
        KhuVucMienTrung,
        [Description("Xuất khẩu")]
        XuatKhau,
    }
}
