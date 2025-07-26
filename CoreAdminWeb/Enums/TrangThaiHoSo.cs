using System.ComponentModel;

namespace CoreAdminWeb.Enums
{
    public enum TrangThaiHoSo
    {
        [Description("Mới")]
        Moi,
        [Description("Đang xử lý")]
        DangXuLy,
        [Description("Hoàn thành")]
        HoanThanh,
        [Description("Hủy")]
        Huy
    }
}
