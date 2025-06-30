using System.ComponentModel;

namespace CoreAdminWeb.Enums
{
    public enum LoaiHinhKiemTra
    {
        [Description("Kiểm tra ban đầu")]
        KiemTraBanDau = 1,
        [Description("Kiểm tra định kỳ")]
        KiemTraDinhKy,
        [Description("Kiểm tra đột xuất")]
        KiemTraDotXuat,
        [Description("Kiểm tra theo đơn yêu cầu")]
        KiemTraTheoDonYeuCau,
        [Description("Kiểm tra do có dấu hiệu vi phạm")]
        KiemTraDoCoDauHieuViPham,
    }
}