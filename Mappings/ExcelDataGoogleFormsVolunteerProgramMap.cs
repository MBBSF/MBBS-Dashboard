using CsvHelper.Configuration;
using MBBS.Dashboard.web.Models;

namespace MBBS.Dashboard.web.Mappings
{
    public class ExcelDataGoogleFormsVolunteerProgramMap : ClassMap<ExcelDataGoogleFormsVolunteerProgram>
    {
        public ExcelDataGoogleFormsVolunteerProgramMap()
        {
            Map(m => m.Id).Ignore(); // Ignore the Id column as it's the primary key
            Map(m => m.Timestamp).Name("Timestamp");
            Map(m => m.Mentor).Name("Mentor");
            Map(m => m.Mentee).Name("Mentee");
            Map(m => m.Date).Name("Date");
            Map(m => m.Time).Name("Time");
            Map(m => m.MethodOfContact).Name("Method of Contact");
            Map(m => m.Comment).Name("Comment");
        }
    }
}