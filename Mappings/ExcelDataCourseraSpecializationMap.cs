using CsvHelper.Configuration;
using MBBS.Dashboard.web.Models;

public class ExcelDataCourseraSpecializationMap : ClassMap<ExcelDataCourseraSpecialization>
{
    public ExcelDataCourseraSpecializationMap()
    {
        //Map(m => m.AccountId).Ignore();
        Map(m => m.Id).Ignore();
        Map(m => m.Name).Name("Name");
        Map(m => m.Email).Name("Email");
        Map(m => m.ExternalId).Name("External Id");
        Map(m => m.Specialization).Name("Specialization");
        Map(m => m.SpecializationSlug).Name("Specialization Slug");
        Map(m => m.University).Name("University");
        Map(m => m.EnrollmentTime).Name("Enrollment Time");
        Map(m => m.LastSpecializationActivityTime).Name("Last Specialization Activity Time");
        Map(m => m.CompletedCourses).Name("# Completed Courses");
        Map(m => m.CoursesInSpecialization).Name("# Courses in Specialization");
        Map(m => m.Completed).Name("Completed");
        Map(m => m.RemovedFromProgram).Name("Removed From Program");
        Map(m => m.ProgramSlug).Name("Program Slug");
        Map(m => m.ProgramName).Name("Program Name");
        Map(m => m.EnrollmentSource).Name("Enrollment Source");
        Map(m => m.SpecializationCompletionTime).Name("Specialization Completion Time");
        Map(m => m.SpecializationCertificateURL).Name("Specialization Certificate URL");
        Map(m => m.JobTitle).Name("Job Title");
        Map(m => m.JobType).Name("Job Type");
        Map(m => m.LocationCity).Name("Location City");
        Map(m => m.LocationRegion).Name("Location Region");
        Map(m => m.LocationCountry).Name("Location Country");
    }
}