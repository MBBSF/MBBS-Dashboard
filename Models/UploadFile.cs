using System.Web;

namespace MBBS.Dashboard.web.Models
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
