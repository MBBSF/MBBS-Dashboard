namespace MBBS.Dashboard.web.Models
{
    public class Customer
    {
        public string CustomerID { get; set; }

        public int CardNumber { get; set; }
        public int CVC { get; set; }
        public string Expiration { get; set; }
    }
}
