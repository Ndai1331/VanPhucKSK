using System.ComponentModel;

namespace CoreAdminWeb.Enums
{
    public enum LoaiGiayChungNhan
    {
        [Description("An toàn thực phẩm")]
        AnToanThucPham = 1,
        [Description("VietGap")]
        VietGap,
        [Description("Chứng nhận hữu cơ")]
        ChungNhanHuuCo,
        [Description("khác")]
        Khac
    }
}
