namespace CoreAdminWeb.Model.Dashboard.General
{
    public class CompanyReportDashboardModel
    {
        public CompanyReportDashboardSummaryModel Summary { get; set; } = new CompanyReportDashboardSummaryModel();
        public CompanyReportDashboardSummaryFeatureModel Feature { get; set; } = new CompanyReportDashboardSummaryFeatureModel();
        public List<CompanyReportDashboardRevenueModel> Revenues { get; set; } = new List<CompanyReportDashboardRevenueModel>();
        public List<CompanyReportDashboardNoteSummaryModel> NoteSummaries { get; set; } = new List<CompanyReportDashboardNoteSummaryModel>();
    }

    public class CompanyReportDashboardSummaryModel
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

    public class CompanyReportDashboardSummaryFeatureModel
    {
        public int Count { get; set; }
        public int PatientCount { get; set; }
    }

    public class CompanyReportDashboardRevenueModel
    {
        public string MaHopDong { get; set; } = string.Empty;
        public string DinhMuc { get; set; } = string.Empty;
        public decimal ChiPhiThucTe { get; set; }
        public decimal ChiPhiDuKien { get; set; }
    }

    public class CompanyReportDashboardNoteSummaryModel
    {
        public string MaDonVi { get; set; } = string.Empty;
        public DateTime? NgayKham { get; set; }
        public int Count { get; set; }
    }
}
