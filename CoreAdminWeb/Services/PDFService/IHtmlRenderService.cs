namespace CoreAdminWeb.Services.PDFService
{
    /// <summary>
    /// Interface for rendering HTML templates
    /// </summary>
    public interface IHtmlRenderService
    {
        /// <summary>
        /// Render medical record to HTML for PDF generation
        /// </summary>
        /// <param name="model">Medical record data</param>
        /// <returns>HTML string</returns>
        string RenderMedicalRecordToPdf(MedicalRecordPdfModel model);
    }

    /// <summary>
    /// Model for medical record PDF generation
    /// </summary>
    public class MedicalRecordPdfModel
    {
        public string PatientName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string BirthDate { get; set; } = string.Empty;
        public string IdNumber { get; set; } = string.Empty;
        public string IssuedDate { get; set; } = string.Empty;
        public string IssuedPlace { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Occupation { get; set; } = string.Empty;
        public string WorkPlace { get; set; } = string.Empty;
        public string WorkStartDate { get; set; } = string.Empty;
        public string FamilyMedicalHistory { get; set; } = string.Empty;
        public string ProfileImageUrl { get; set; } = string.Empty;

        // Medical examination data
        public MedicalExamData? ExamData { get; set; }
        public PhysicalExamData? PhysicalData { get; set; }
        public LabTestData? LabData { get; set; }
        public ConclusionData? ConclusionData { get; set; }
        public SpecialtyExamData? SpecialtyData { get; set; }
        public GynecoExamData? GynecoData { get; set; }
    }

    public class MedicalExamData
    {
        public string MedicalHistory { get; set; } = string.Empty;
        public string CurrentDate { get; set; } = string.Empty;
    }

    public class PhysicalExamData
    {
        public string Height { get; set; } = string.Empty;
        public string Weight { get; set; } = string.Empty;
        public string BMI { get; set; } = string.Empty;
        public string Pulse { get; set; } = string.Empty;
        public string BloodPressure { get; set; } = string.Empty;
        public string Classification { get; set; } = string.Empty;
    }

    public class LabTestData
    {
        public string Results { get; set; } = string.Empty;
        public string Evaluation { get; set; } = string.Empty;
    }

    public class ConclusionData
    {
        public string HealthClassification { get; set; } = string.Empty;
        public string Diseases { get; set; } = string.Empty;
        public string Recommendations { get; set; } = string.Empty;
        public string ConclusionDate { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string DoctorSignature { get; set; } = string.Empty;
    }

    public class SpecialtyExamData
    {
        // Internal Medicine
        public string Circulation { get; set; } = string.Empty;
        public string Respiratory { get; set; } = string.Empty;
        public string Digestive { get; set; } = string.Empty;
        public string Urinary { get; set; } = string.Empty;
        public string Mental { get; set; } = string.Empty;

        // Surgery & Dermatology
        public string Surgery { get; set; } = string.Empty;
        public string Dermatology { get; set; } = string.Empty;

        // Ophthalmology
        public string EyeExam { get; set; } = string.Empty;

        // ENT
        public string ENTExam { get; set; } = string.Empty;

        // Dental
        public string DentalExam { get; set; } = string.Empty;
    }

    public class GynecoExamData
    {
        public string MenarcheAge { get; set; } = string.Empty;
        public string MenstrualCycle { get; set; } = string.Empty;
        public string MenstrualAmount { get; set; } = string.Empty;
        public string Dysmenorrhea { get; set; } = string.Empty;
        public string Married { get; set; } = string.Empty;
        public string PARA { get; set; } = string.Empty;
        public string Surgeries { get; set; } = string.Empty;
        public string Contraception { get; set; } = string.Empty;
        public string Results { get; set; } = string.Empty;
        public string Classification { get; set; } = string.Empty;
    }
} 