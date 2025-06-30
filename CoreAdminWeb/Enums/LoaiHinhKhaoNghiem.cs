using System.ComponentModel;

namespace CoreAdminWeb.Enums
{
    public enum LoaiHinhKhaoNghiem
    {
        [Description("Nhà nước")]
        NhaNuoc = 1,
        [Description("Doanh nghiệp")]
        DoanhNghiep,
        [Description("Viện nghiên cứu")]
        VienNghienCuu,
        [Description("Khác")]
        Khac
    }
}
