using CoreAdminWeb.Enums;
using CoreAdminWeb.Extensions;

namespace CoreAdminWeb.Commons.Utils
{
    public static class TrangThaiCapPhepUtil
    {
        public static string GetTrangThai(this TrangThaiGiayPhep trangThai)
        {
            string description = trangThai.GetDescription();
            (string bgColor, string textColor) = trangThai switch
            {
                TrangThaiGiayPhep.DangHoatDong => ("#28a745", "#fff"),      // green bg, white text
                TrangThaiGiayPhep.HetHan => ("#ffc107", "#212529"),         // yellow bg, dark text
                TrangThaiGiayPhep.DinhChi => ("#dc3545", "#fff"),           // red bg, white text
                TrangThaiGiayPhep.ThuHoi => ("#6c757d", "#fff"),    // gray bg, white text
                _ => ("#343a40", "#fff")                                    // dark bg, white text
            };
            return $"<span class=\"inline-flex items-center rounded shadow-md text-xs justify-center px-1.5 py-0.5\" style=\"background-color: {bgColor}; color: {textColor};\">{description}</span>";
        }
    }
}
