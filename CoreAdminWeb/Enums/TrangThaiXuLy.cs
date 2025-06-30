using System.ComponentModel;

namespace CoreAdminWeb.Enums
{
    public enum TrangThaiXuLy
    {
        [Description("Chưa xử lý")]
        ChuaXuLy = 1,
        [Description("Đang xử lý")]
        DangXuLy,
        [Description("Đã xử lý")]
        DaXuLy
    }
}
