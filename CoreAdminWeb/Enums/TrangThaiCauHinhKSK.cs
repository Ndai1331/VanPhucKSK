using System.ComponentModel;

namespace CoreAdminWeb.Enums
{
    public enum TrangThaiCauHinhKSK
    {
        // Base Status values
        [Description("Bản nháp")]
        draft,
        [Description("Xuất bản")]
        published,
        [Description("Đã xóa")]
        removed,
        
        // Extended values
        // [Description("Locked")]
        // locked
    }
}
