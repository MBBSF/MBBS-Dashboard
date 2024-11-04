using System.Web;

namespace FirstIterationProductRelease.Models
{
    public class UploadFile
    {
        public IFormFile File { get; set; }
        public string Source { get; set; }
    }

    public enum DataSource
    {
        Coursera,
        GoogleForms,
        Cognito
    }
}
