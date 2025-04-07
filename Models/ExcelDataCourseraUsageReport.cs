using System;

namespace MBBS.Dashboard.web.Models
{
    public class ExcelDataCourseraUsageReport
    {
        public int Id { get; set; } // Primary key
        public int? AccountId { get; set; }

        public string Name { get; set; }
        public string Email { get; set; }
        public string ExternalId { get; set; }
        public string Course { get; set; }
        public string CourseId { get; set; }
        public string CourseSlug { get; set; }
        public string University { get; set; }
        public DateTime? EnrollmentTime { get; set; }
        public DateTime? ClassStartTime { get; set; }
        public DateTime? ClassEndTime { get; set; }
        public DateTime? LastCourseActivityTime { get; set; }
        public decimal? OverallProgress { get; set; }
        public decimal? EstimatedLearningHours { get; set; }
        public string Completed { get; set; }
        public string RemovedFromProgram { get; set; }
        public string ProgramSlug { get; set; }
        public string ProgramName { get; set; }
        public string EnrollmentSource { get; set; }
        public DateTime? CompletionTime { get; set; }
        public string CourseGrade { get; set; }
        public string CourseCertificateURL { get; set; }
        public string ForCredit { get; set; }
        public string CourseType { get; set; }
        public string JobTitle { get; set; }
        public string JobType { get; set; }
        public string LocationCity { get; set; }
        public string LocationRegion { get; set; }
        public string LocationCountry { get; set; }
    }
}