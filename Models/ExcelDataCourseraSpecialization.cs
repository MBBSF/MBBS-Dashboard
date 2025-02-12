using System.Runtime.InteropServices;

namespace MBBS.Dashboard.web.Models
{
    public class ExcelDataCourseraSpecialization
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string? ExternalId { get; set; }
        public string Specialization { get; set; }
        public string SpecializationSlug { get; set; }
        public string University { get; set; }
        public DateTime? EnrollmentTime { get; set; }
        public DateTime? LastSpecializationActivityTime { get; set; }
        public int? CompletedCourses { get; set; }
        public int? CoursesInSpecialization { get; set; }
        public string Completed { get; set; }
        public string RemovedFromProgram { get; set; }
        public string ProgramSlug { get; set; }
        public string? ProgramName { get; set; } // Allow nulls
        public string? EnrollmentSource { get; set; } // Allow nulls
        public DateTime? SpecializationCompletionTime { get; set; }
        public string? SpecializationCertificateURL { get; set; }
        public string? JobTitle { get; set; } // Allow nulls
        public string? JobType { get; set; } // Allow nulls
        public string? LocationCity { get; set; } // Allow nulls
        public string? LocationRegion { get; set; } // Allow nulls
        public string? LocationCountry { get; set; } // Allow nulls
    }
}
