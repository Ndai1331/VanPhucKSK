using System.ComponentModel;

namespace CoreAdminWeb.Enums
{
    public enum KhacPhuc
    {
        [Description("Buộc thu hồi")]
        BuocThuHoi = 1,
        [Description("Buộc tiêu hủy")]
        BuocTieuHuy,
        [Description("Khác")]
        Khac,
    }
}
