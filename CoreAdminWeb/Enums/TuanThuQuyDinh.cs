using System.ComponentModel;

namespace CoreAdminWeb.Enums
{
    public enum TuanThuQuyDinh
    {
        [Description("Ký cam kết")]
        KyCamKet     = 1,
        [Description("Chưa ký cam kết")]
        ChuaKyCamKet,
    }

    public enum KiemTraTuanThu
    {
        [Description("Đã được kiểm tra")]
        DaDuocKiemTra    = 1,
        [Description("Chưa được kiểm tra")]
        ChuaDuocKiemTra,
    }
    
}
