using System.ComponentModel;

namespace CoreAdminWeb.Enums
{
    public enum HinhThucCap
    {
        [Description("Cấp mới")]
        CapMoi = 1,
        [Description("Cấp lại")]
        CapLai,
        [Description("Gia hạn")]
        GiaHan,
        [Description("Điều chỉnh")]
        DieuChinh,
        [Description("Cấp tạm")]
        CapTam,
        [Description("Cấp đổi")]
        CapDoi
    }
}
