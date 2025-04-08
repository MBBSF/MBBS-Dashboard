using System;

namespace MBBS.Dashboard.web.Models
{
    public class ExcelDataCourseraMembershipReport
    {
        public int Id { get; set; } // Primary key

        public int? AccountId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string ExternalId { get; set; }
        public string ProgramName { get; set; }
        public string ProgramSlug { get; set; }
        public int? EnrolledCourses { get; set; }
        public int? CompletedCourses { get; set; }
        public string MemberState { get; set; }
        public DateTime? JoinDate { get; set; }
        public DateTime? InvitationDate { get; set; }
        public DateTime? LatestProgramActivityDate { get; set; }
        public string JobTitle { get; set; }
        public string JobType { get; set; }
        public string LocationCity { get; set; }
        public string LocationRegion { get; set; }
        public string LocationCountry { get; set; }
    }
}