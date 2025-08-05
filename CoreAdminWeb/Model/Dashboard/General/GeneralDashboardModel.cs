namespace CoreAdminWeb.Model.Dashboard.General
{
    public class GeneralDashboardModel
    {
        public CompanyExaminationModel CompanyExamination { get; set; } = new CompanyExaminationModel();
        public CompanyHealthExaminationModel CompanyHealthExamination { get; set; } = new CompanyHealthExaminationModel();
        public List<HealthClassificationModel> HealthClassifications { get; set; } = new List<HealthClassificationModel>();
        public List<CommonDiseaseModel> CommonDiseases { get; set; } = new List<CommonDiseaseModel>();
    }

    public class CompanyExaminationModel
    {
        public int ToTalExaminations { get; set; }
        public int TotalExaminationRecords { get; set; }
        public decimal TotalCost { get; set; }
        public int AbnormalCases { get; set; }
    }

    public class HealthClassificationModel
    {
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
        public DateTime Date { get; set; }
    }

    public class CommonDiseaseModel
    {
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class CompanyHealthExaminationModel
    {
        public DateTime? LastDate { get; set; }
        public int? TotalExaminationRecords { get; set; }
        public int? AbnormalCases { get; set; }
        public float? PercentagePass
        {
            get
            {
                if (TotalExaminationRecords == null || TotalExaminationRecords == 0)
                {
                    return 0;
                }

                return (TotalExaminationRecords - AbnormalCases) / (float)TotalExaminationRecords * 100;
            }
        }
    }
}
