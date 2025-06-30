using System.ComponentModel;

namespace CoreAdminWeb.Enums
{
    public enum DoiTuongTiepCan
    {
        [Description("Doanh nghiệp")]
        DoanhNghiep = 1,
        [Description("Nhà phân phối")]
        NhaPhanPhoi,
        [Description("Người tiêu dùng")]
        NguoiTieuDung,
        [Description("Khác")]
        Khac,
    }
}
