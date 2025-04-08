using System;
using System.Collections.Generic;

namespace MBBS.Dashboard.web.Models
{
    public class DashboardViewModel
    {
        public KpiDataViewModel KpiData { get; set; }
        public KpiDataViewModel.GoogleCertificationKPIsViewModel GoogleCertificationKPIs { get; set; }
        public MentoringProgramKPIsViewModel MentoringProgramKPIs { get; set; }
        public ScholarshipApplicationKPIsViewModel ScholarshipApplicationKPIs { get; set; }
        public List<KpiDataViewModel.CourseraMembershipReportViewModel> CourseraMembershipReports { get; set; }
        public List<KpiDataViewModel.CourseraPivotLocationCityReportViewModel> CourseraPivotLocationCityReports { get; set; }
        public List<KpiDataViewModel.CourseraUsageReportViewModel> CourseraUsageReports { get; set; }
        public List<ExcelDataCourseraSpecialization> CourseraData { get; set; }
        public List<ExcelDataCognitoMasterList> CognitoData { get; set; }
        public List<ExcelDataGoogleFormsVolunteerProgram> GoogleFormsData { get; set; }
        public List<ActivityLogViewModel> ActivityLogs { get; set; }
        public int PlatformId { get; set; }
        public List<object> PlatformData { get; set; }
        public string CurrentSortBy { get; set; }
        public string CurrentSortOrder { get; set; }
    }

    public class KpiDataViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalCourseraUsers { get; set; }
        public int TotalCognitoUsers { get; set; }
        public int TotalGoogleFormsUsers { get; set; }

        public class GoogleCertificationKPIsViewModel
        {
            public int TotalParticipants { get; set; }
            public int CompletedCertifications { get; set; }
            public double CompletionRate { get; set; }
            public Dictionary<string, int> SpecializationDistribution { get; set; }
            public Dictionary<string, int> LocationDistribution { get; set; }
            public int ActiveLearners { get; set; }
        }

        public class CourseraMembershipReportViewModel
        {
            public string Name { get; set; }
            public string Email { get; set; }
            public string ProgramName { get; set; }
            public int EnrolledCourses { get; set; }
            public int CompletedCourses { get; set; }
            public string MemberState { get; set; }
        }

        public class CourseraPivotLocationCityReportViewModel
        {
            public string LocationCity { get; set; }
            public int CurrentMembers { get; set; }
            public int CurrentLearners { get; set; }
            public int TotalEnrollments { get; set; }
            public int TotalCompletedCourses { get; set; }
            public double? AverageProgress { get; set; }
        }

        public class CourseraUsageReportViewModel
        {
            public string Name { get; set; }
            public string Course { get; set; }
            public double? OverallProgress { get; set; }
            public string Completed { get; set; }
            public double EstimatedLearningHours { get; set; }
        }
    }

    public class MentoringProgramKPIsViewModel
    {
        public int TotalMentoringSessions { get; set; }
        public Dictionary<string, int> ContactMethodDistribution { get; set; }
        public List<string> TopMentors { get; set; }
        public int UniqueMentees { get; set; }
        public double AverageSessionsPerMentee { get; set; }
    }

    public class ScholarshipApplicationKPIsViewModel
    {
        public int TotalApplications { get; set; }
        public Dictionary<string, int> IntendedMajorDistribution { get; set; }
        public double PhoneNumberProvisionRate { get; set; }
        public Dictionary<string, int> SchoolDistribution { get; set; }
        public double AverageGPA { get; set; }
    }

    public class ActivityLogViewModel
    {
        public string UserName { get; set; }
        public string Action { get; set; }
        public DateTime Timestamp { get; set; }
        public string Details { get; set; }
    }
}