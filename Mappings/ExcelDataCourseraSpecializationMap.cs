using CsvHelper.Configuration;
using FirstIterationProductRelease.Models;

public class ExcelDataCourseraSpecializationMap : ClassMap<ExcelDataCourseraSpecialization>
{
    public ExcelDataCourseraSpecializationMap()
    {
 
        Map(m => m.Id).Ignore();
       
        Map(m => m.Name).Name("Name");
        Map(m => m.Email).Name("Email");
        Map(m => m.ExternalId).Name("ExternalId");
        Map(m => m.Specialization).Name("Specialization");
        Map(m => m.SpecializationSlug).Name("SpecializationSlug");
        Map(m => m.University).Name("University");
        Map(m => m.EnrollmentTime).Name("EnrollmentTime");
        Map(m => m.LastSpecializationActivityTime).Name("LastSpecializationActivityTime");
        Map(m => m.CompletedCourses).Name("CompletedCourses");
        Map(m => m.CoursesInSpecialization).Name("CoursesInSpecialization");
        Map(m => m.Completed).Name("Completed");
        Map(m => m.RemovedFromProgram).Name("RemovedFromProgram");
        Map(m => m.ProgramSlug).Name("ProgramSlug");
        Map(m => m.ProgramName).Name("ProgramName");
        Map(m => m.EnrollmentSource).Name("EnrollmentSource");
        Map(m => m.SpecializationCompletionTime).Name("SpecializationCompletionTime");
        Map(m => m.SpecializationCertificateURL).Name("SpecializationCertificateURL");
        Map(m => m.JobTitle).Name("JobTitle");
        Map(m => m.JobType).Name("JobType");
        Map(m => m.LocationCity).Name("LocationCity");
        Map(m => m.LocationRegion).Name("LocationRegion");
        Map(m => m.LocationCountry).Name("LocationCountry");
    }
}