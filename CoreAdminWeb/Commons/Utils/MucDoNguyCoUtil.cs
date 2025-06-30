using CoreAdminWeb.Enums;
using CoreAdminWeb.Extensions;

namespace CoreAdminWeb.Commons.Utils
{
    public static class MucDoNguyCoUtil
    {
        public static string GetTrangThai(this MucDoNguyCo trangThai)
        {
            string description = trangThai.GetDescription();
            string strClass = trangThai switch
            {
                MucDoNguyCo.Thap => "shadow-success/50 bg-success text-white inline-flex items-center rounded shadow-md text-xs justify-center px-1.5 py-0.5",
                MucDoNguyCo.TrungBinh => "shadow-warning/50 bg-warning text-white inline-flex items-center rounded shadow-md text-xs justify-center px-1.5 py-0.5",
                MucDoNguyCo.Cao => "shadow-danger/50 bg-danger text-white inline-flex items-center rounded shadow-md text-xs justify-center px-1.5 py-0.5",
                _ => ""
            };
            return $"<span class=\"{strClass}\">{description}</span>";
        }
    }
}
