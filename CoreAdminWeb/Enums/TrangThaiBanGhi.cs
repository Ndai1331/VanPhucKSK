using System.ComponentModel;

namespace CoreAdminWeb.Enums
{
    public enum TrangThaiBanGhi
    {
        [Description("Chờ lưu")]
        ChoLuu = 1,
        [Description("Đã lưu")]
        DaLuu,
        [Description("Đã xóa")]
        DaXoa
    }
}
