using System.ComponentModel;

namespace CoreAdminWeb.Enums
{
    public enum LoaiHinhSanXuat
    {
        [Description("Sản xuất hoạt chất")]
        SanXuatHoatChat = 1,
        [Description("Sản xuất thuốc kỹ thuật")]
        SanXuatThuocKyThuat = 2,
        [Description("Sản xuất thành phẩm")]
        SanXuatThanhPham = 3,
        [Description("Đóng gói")]
        DongGoi = 4,
        [Description("Khác")]
        Khac = 5
    }
}
