using System.ComponentModel;

namespace CoreAdminWeb.Enums
{
    public enum TrangThaiBanGhiChiTiet
    {
        [Description("Mới")]
        New = 1,
        [Description("Đã lưu")]
        Saved,
        [Description("Đã xóa")]
        Removed
    }
}
