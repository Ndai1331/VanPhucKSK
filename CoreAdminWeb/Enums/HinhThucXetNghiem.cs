using System.ComponentModel;

namespace CoreAdminWeb.Enums
{
    public enum HinhThucXetNghiem
    {
        [Description("Xét nghiệm tại phòng kiểm nghiệm")]
        XetNghiemTaiPhongKiemNghiem = 1,
        [Description("Xét nghiệm nhanh")]
        XetNghiemNhanh,
    }
}