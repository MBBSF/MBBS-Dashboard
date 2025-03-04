using CsvHelper.Configuration;
using MBBS.Dashboard.web.Models;

namespace MBBS.Dashboard.web.Mappings
{
    public class ExcelDataCourseraMembershipReportMap : ClassMap<ExcelDataCourseraMembershipReport>
    {
        public ExcelDataCourseraMembershipReportMap()
        {
            Map(m => m.Name).Name("Name");
            Map(m => m.Email).Name("Email");
            Map(m => m.ExternalId).Name("External Id");
            Map(m => m.ProgramName).Name("Program Name");
            Map(m => m.ProgramSlug).Name("Program Slug");
            Map(m => m.EnrolledCourses).Name("# Enrolled Courses");
            Map(m => m.CompletedCourses).Name("# Completed Courses");
            Map(m => m.MemberState).Name("Member State");
            Map(m => m.JoinDate).Name("Join Date");
            Map(m => m.InvitationDate).Name("Invitation Date");
            Map(m => m.LatestProgramActivityDate).Name("Latest Program Activity Date");
            Map(m => m.JobTitle).Name("Job Title");
            Map(m => m.JobType).Name("Job Type");
            Map(m => m.LocationCity).Name("Location City");
            Map(m => m.LocationRegion).Name("Location Region");
            Map(m => m.LocationCountry).Name("Location Country");
        }
    }
}