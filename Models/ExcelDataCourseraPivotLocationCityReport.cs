using System;

namespace MBBS.Dashboard.web.Models
{
    public class ExcelDataCourseraPivotLocationCityReport
    {
        public int Id { get; set; } // Primary key

        public string LocationCity { get; set; }
        public int? CurrentMembers { get; set; }
        public int? CurrentLearners { get; set; }
        public int? TotalEnrollments { get; set; }
        public int? TotalCompletedCourses { get; set; }
        public decimal? AverageProgress { get; set; }
        public decimal? TotalEstimatedLearningHours { get; set; }
        public decimal? AverageEstimatedLearningHours { get; set; }
        public int? DeletedMembers { get; set; }
    }
}