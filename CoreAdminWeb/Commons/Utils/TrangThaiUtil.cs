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
                Status.active => "text-success",
                Status.deactive => "text-danger",
                _ => ""
            };
            return $"<span class=\"{strClass}\">{description}</span>";
        }
    }
}
