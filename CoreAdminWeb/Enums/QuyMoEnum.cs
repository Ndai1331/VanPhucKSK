using System.ComponentModel;

namespace CoreAdminWeb.Enums;
public enum QuyMoEnum
{
    [Description("Quy mô nhỏ")]
    QuyMoNho = 1,
    [Description("Quy mô vừa")]
    QuyMoVua = 2,
    [Description("Quy mô lớn")]
    QuyMoLon = 3
}
