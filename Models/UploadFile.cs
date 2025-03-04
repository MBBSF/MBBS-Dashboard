using Microsoft.AspNetCore.Http;

namespace MBBS.Dashboard.web.Models
{
    public class UploadFile
    {
        public IFormFile File { get; set; }
        public string Source { get; set; }
        public string FileType { get; set; }  // New property for the file type
    }

    public enum DataSource
    {
        Coursera,
        GoogleForms,
        Cognito
    }
}