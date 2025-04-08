using MBBS.Dashboard.web.Controllers;
using System.Collections.Generic;

namespace MBBS.Dashboard.web.Models
{
    public class DashboardViewModel
    {
        public KpiData KpiData { get; set; }
        public List<ActivityLog> ActivityLogs { get; set; }
        public List<object> PlatformData { get; set; }
        public int? PlatformId { get; set; }
        public List<ExcelDataCourseraSpecialization> CourseraData { get; set; }
        public List<ExcelDataCognitoMasterList> CognitoData { get; set; }
        public List<ExcelDataGoogleFormsVolunteerProgram> GoogleFormsData { get; set; }
        public string SearchQuery { get; set; }
        public string CurrentSortOrder { get; internal set; }
        public string CurrentSortBy { get; internal set; }
    }

    public class KpiData
    {
        public int TotalUsers { get; set; }
        public int TotalCourseraUsers { get; set; }
        public int TotalCognitoUsers { get; set; }
        public int TotalGoogleFormsUsers { get; set; }
        public List<string> TopMentors { get; set; }
        public double MentorCompletionRate { get; set; }
        public Dictionary<string, int> SpecializationDistribution { get; set; }
        public Dictionary<string, int> IntendedMajorDistribution { get; set; }
        public double PhoneNumberProvisionRate { get; set; }
        public int TotalMentoringSessions { get; set; }
        public Dictionary<string, int> ContactMethodPreference { get; set; }
    }
}
