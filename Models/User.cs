namespace MBBS.Dashboard.web.Models
{
    public class User
    {
        public int UserID { get; set; }

        public string FName { get; set; }
        public string MInitial { get; set; }
        public string LName { get; set; }

        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public int ZipCode { get; set; }

        public string Email { get; set; }
    }
}
