using CoreAdminWeb.Enums;
using CoreAdminWeb.Extensions;

namespace CoreAdminWeb.Commons.Utils
{
    public static class TrangThaiXuLyUtil
    {
        public static string GetTrangThai(this TrangThaiXuLy trangThai)
        {
            string description = trangThai.GetDescription();
            string strClass = trangThai switch
            {
                TrangThaiXuLy.DaXuLy => "shadow-success/50 bg-success text-white inline-flex items-center rounded shadow-md text-xs justify-center px-1.5 py-0.5",
                TrangThaiXuLy.ChuaXuLy => "shadow-warning/50 bg-warning text-white inline-flex items-center rounded shadow-md text-xs justify-center px-1.5 py-0.5",
                TrangThaiXuLy.DangXuLy => "shadow-danger/50 bg-danger text-white inline-flex items-center rounded shadow-md text-xs justify-center px-1.5 py-0.5",
                _ => ""
            };
            return $"<span class=\"{strClass}\">{description}</span>";
        }
    }
}
