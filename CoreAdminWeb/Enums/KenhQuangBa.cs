using System.ComponentModel;

namespace CoreAdminWeb.Enums
{
    public enum KenhQuangBa
    {
        [Description("Hội chợ")]
        HoiCho = 1,
        [Description("Triển lãm")]
        TrienLam,
        [Description("Sàn TMĐT")]
        SanTMDT,
        [Description("Website")]
        Website,
        [Description("Mạng xã hội")]
        MangXaHoi,
        [Description("Báo chí")]
        BaoChi,
        [Description("Truyền hình")]
        TruyenHinh,
        [Description("Khác")]
        Khac
    }
}
