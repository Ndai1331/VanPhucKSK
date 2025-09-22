using System.ComponentModel;

namespace CoreAdminWeb.Enums
{
    public enum HoSoKhamSucKhoeExportType
    {
        [Description("Phiếu khám sức khỏe")]
        CheckListKsk = 1,
        [Description("Sổ KSK - TT32")]
        HealthCheckupBook,
        [Description("Phiếu tư vấn")]
        ConsultationSlip,
        [Description("Hồ sơ KSK")]
        MedicalExamination
    }
}
