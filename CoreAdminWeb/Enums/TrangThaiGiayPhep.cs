using System.ComponentModel;

namespace CoreAdminWeb.Enums
{
    public enum TrangThaiGiayPhep
    {
        [Description("Đang hoạt động")]
        DangHoatDong = 1,
        [Description("Hết hạn")]
        HetHan,
        [Description("Đình chỉ")]
        DinhChi,
        [Description("Thu hồi")]
        ThuHoi
    }
}
