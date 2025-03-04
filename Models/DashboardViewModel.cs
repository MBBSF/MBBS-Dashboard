using System.Collections.Generic;
using MBBS.Dashboard.web.Controllers;

namespace MBBS.Dashboard.web.Models
{
    public class DashboardViewModel
    {
        // Key Performance Indicators
        public KpiData KpiData { get; set; }

        // Activity Logs
        public List<ActivityLog> ActivityLogs { get; set; }

        // Detailed Platform Data
        public List<ExcelDataCourseraSpecialization> CourseraData { get; set; }
        public List<ExcelDataCognitoMasterList> CognitoData { get; set; }
        public List<ExcelDataGoogleFormsVolunteerProgram> GoogleFormsData { get; set; }
    }

    public class KpiData
    {
        public int TotalUsers { get; set; }
        public int TotalCourseraUsers { get; set; }
        public int TotalCognitoUsers { get; set; }
        public int TotalGoogleFormsUsers { get; set; }
        public int CompletedCourseraSpecializations { get; set; }
        public double AverageCoursesCompleted { get; set; }
        public List<string> TopMentors { get; set; }
    }
}


