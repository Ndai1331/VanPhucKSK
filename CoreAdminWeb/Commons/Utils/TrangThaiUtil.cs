using CoreAdminWeb.Enums;
using CoreAdminWeb.Extensions;
using CoreAdminWeb.Model.Base;

namespace CoreAdminWeb.Commons.Utils
{
    public static class TrangThaiUtil
    {
        public static string GetTrangThai(this Status trangThai)
        {
            string description = trangThai.GetDescription();
            string strClass = trangThai switch
            {
                Status.published => "text-success",
                Status.draft => "text-danger",
                _ => ""
            };
            return $"<span class=\"{strClass}\">{description}</span>";
        }
        public static string GetTrangThai(this TrangThaiHoatDong trangThai)
        {
            string description = trangThai.GetDescription();
            string strStyle = trangThai switch
            {
                TrangThaiHoatDong.active => "background: #e0f7e9;color: #1abc9c;",
                TrangThaiHoatDong.deactive => "background: #ffeaea;color: #e74c3c;",
                _ => ""
            };
            return $"<span style=\"padding: 4px 12px;border-radius: 12px;font-size: 13px;font-weight: 600;{strStyle}\">{description}</span>";
        }
    }
}
