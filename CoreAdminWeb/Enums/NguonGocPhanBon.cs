using System.ComponentModel;

namespace CoreAdminWeb.Enums
{
    public enum NguonGocPhanBon
    {
        [Description("Nhập khẩu")]
        NhapKhau = 1,
        [Description("Sản xuất")]
        SanXuat,
        [Description("Khác")]
        Khac
    }
}
