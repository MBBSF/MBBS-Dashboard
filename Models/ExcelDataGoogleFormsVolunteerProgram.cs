namespace MBBS.Dashboard.web.Models
{
    public class ExcelDataGoogleFormsVolunteerProgram
    {
        public int Id { get; set; } // Primary key
        public DateTime Timestamp { get; set; }
        public string Mentor { get; set; }
        public string Mentee { get; set; }
        public DateTime? Date { get; set; }
        public string Time { get; set; }
        public string MethodOfContact { get; set; }
        public string Comment { get; set; }
    }
}