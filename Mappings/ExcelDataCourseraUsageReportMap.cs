using CsvHelper.Configuration;
using MBBS.Dashboard.web.Models;

namespace MBBS.Dashboard.web.Mappings
{
    public class ExcelDataCourseraUsageReportMap : ClassMap<ExcelDataCourseraUsageReport>
    {
        public ExcelDataCourseraUsageReportMap()
        {
            Map(m => m.AccountId).Ignore();
            Map(m => m.Name).Name("Name");
            Map(m => m.Email).Name("Email");
            Map(m => m.ExternalId).Name("External Id");
            Map(m => m.Course).Name("Course");
            Map(m => m.CourseId).Name("Course ID");
            Map(m => m.CourseSlug).Name("Course Slug");
            Map(m => m.University).Name("University");
            Map(m => m.EnrollmentTime).Name("Enrollment Time");
            Map(m => m.ClassStartTime).Name("Class Start Time");
            Map(m => m.ClassEndTime).Name("Class End Time");
            Map(m => m.LastCourseActivityTime).Name("Last Course Activity Time");
            Map(m => m.OverallProgress).Name("Overall Progress");
            Map(m => m.EstimatedLearningHours).Name("Estimated Learning Hours");
            Map(m => m.Completed).Name("Completed");
            Map(m => m.RemovedFromProgram).Name("Removed From Program");
            Map(m => m.ProgramSlug).Name("Program Slug");
            Map(m => m.ProgramName).Name("Program Name");
            Map(m => m.EnrollmentSource).Name("Enrollment Source");
            Map(m => m.CompletionTime).Name("Completion Time");
            Map(m => m.CourseGrade).Name("Course Grade");
            Map(m => m.CourseCertificateURL).Name("Course Certificate URL");
            Map(m => m.ForCredit).Name("For Credit");
            Map(m => m.CourseType).Name("Course Type");
            Map(m => m.JobTitle).Name("Job Title");
            Map(m => m.JobType).Name("Job Type");
            Map(m => m.LocationCity).Name("Location City");
            Map(m => m.LocationRegion).Name("Location Region");
            Map(m => m.LocationCountry).Name("Location Country");
        }
    }
}