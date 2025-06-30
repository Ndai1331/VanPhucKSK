using System.ComponentModel;

namespace CoreAdminWeb.Enums
{
    public enum ChiTieu
    {
        [Description("Chỉ tiêu vi sinh")]
        ChiTieuViSinh = 1,
        [Description("Chỉ tiêu thuốc BVTV")]
        ChiTieuThuocBVTV = 2,
        [Description("Chỉ tiêu hóa chất, chất bảo quản")]
        ChiTieuHoaChat = 3,
    }
}