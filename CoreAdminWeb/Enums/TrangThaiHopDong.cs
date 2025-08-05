using System.ComponentModel;

namespace CoreAdminWeb.Enums
{
    public enum TrangThaiHopDong
    {
        [Description("Chưa hiệu lực")]
        ChuaHieuLuc,
        [Description("Đang thực hiện")]
        DangThucHien,
        [Description("Đã hoàn thành")]
        HoanThanh,
        [Description("Khóa HĐ")]
        locked
    }
}
