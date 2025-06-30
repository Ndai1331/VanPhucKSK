using System.ComponentModel;

namespace CoreAdminWeb.Enums;
public enum NhomCayTrong
{
    [Description("Cây đầu dòng")]
    CayDauDong = 1,
    [Description("Cây trồng chính")]
    CayTrongChinh = 2,
    [Description("Cây được bảo hộ")]
    CayDuocBaoHo = 3
}

public enum NguonGoc
{
    [Description("Nội địa")]
    NoiDia = 1,
    [Description("Nhập khẩu")]
    NhapKhau = 2,
    [Description("Khác")]
    Khac = 3
}