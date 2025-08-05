namespace CoreAdminWeb.Model.Dashboard.General
{
    public class CompanySummaryReportDashboardModel
    {
        public CompanySummaryReportDashboardSummaryModel Summary { get; set; } = new CompanySummaryReportDashboardSummaryModel();
        public CompanySummaryReportDashboardSummaryFeatureModel Feature { get; set; } = new CompanySummaryReportDashboardSummaryFeatureModel();
        public List<CompanySummaryReportDashboardRevenueModel> Revenues { get; set; } = new List<CompanySummaryReportDashboardRevenueModel>();
        public List<CompanySummaryReportDashboardNoteSummaryModel> NoteSummaries { get; set; } = new List<CompanySummaryReportDashboardNoteSummaryModel>();
    }

    public class CompanySummaryReportDashboardSummaryModel
    {
        public int Count { get; set; }
        public int DoneCount { get; set; }
        public int ProcessingCount { get; set; }
        public int PatientDoneCount { get; set; }
        public int PatientProcessingCount { get; set; }
        public decimal ChiPhiDuKien { get; set; }
        public decimal ChiPhiThucTe { get; set; }

        public float PatientDonePercentage
        {
            get
            {
                if (PatientDoneCount + PatientProcessingCount == 0)
                {
                    return 0;
                }
                return (float)PatientDoneCount / (PatientDoneCount + PatientProcessingCount) * 100;
            }
        }
    }

    public class CompanySummaryReportDashboardSummaryFeatureModel
    {
        public int Count { get; set; }
        public int PatientCount { get; set; }
    }

    public class CompanySummaryReportDashboardRevenueModel
    {
        public string MaHopDong { get; set; } = string.Empty;
        public string DinhMuc { get; set; } = string.Empty;
        public decimal GiaTriHopDong { get; set; }
        public decimal ChiPhiThucTe { get; set; }
        public decimal ChiPhiDuKien { get; set; }
    }

    public class CompanySummaryReportDashboardNoteSummaryModel
    {
        public string MaDonVi { get; set; } = string.Empty;
        public DateTime? NgayKham { get; set; }
        public int Count { get; set; }
    }
}
