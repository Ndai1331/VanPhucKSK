using System.ComponentModel;

namespace CoreAdminWeb.Enums
{
    public enum HoSoKhamSucKhoeExportType
    {
        [Description("Checklist KSK")]
        CheckListKsk = 1,
        [Description("Báo cáo tổng")]
        SummaryReport,
        [Description("Sổ KSK - TT32")]
        HealthCheckupBook,
        [Description("Phiếu tư vấn")]
        CónultationSlip,
        [Description("Hồ sơ KSK")]
        MedicalExamination
    }
}
