using CsvHelper.Configuration;
using MBBS.Dashboard.web.Models;

namespace MBBS.Dashboard.web.Mappings
{
    public class ExcelDataCourseraPivotLocationCountryReportMap : ClassMap<ExcelDataCourseraPivotLocationCountryReport>
    {
        public ExcelDataCourseraPivotLocationCountryReportMap()
        {
            Map(m => m.AccountId).Ignore();
            Map(m => m.LocationCountry).Name("Location Country");
            Map(m => m.CurrentMembers).Name("Current Members");
            Map(m => m.CurrentLearners).Name("Current Learners");
            Map(m => m.TotalEnrollments).Name("Total Enrollments");
            Map(m => m.TotalCompletedCourses).Name("Total Completed Courses");
            Map(m => m.AverageProgress).Name("Average Progress");
            Map(m => m.TotalEstimatedLearningHours).Name("Total Estimated Learning Hours");
            Map(m => m.AverageEstimatedLearningHours).Name("Average Estimated Learning Hours");
            Map(m => m.DeletedMembers).Name("Deleted Members");
        }
    }
}