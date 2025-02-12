using MBBS.Dashboard.web.Controllers;

namespace MBBS.Dashboard.web.Models
{
    public class DashboardViewModel
    {
        public KpiData KpiData { get; set; }
        public IEnumerable<ActivityLog> ActivityLogs { get; set; }
    }

    public class KpiData
    {
        public int TotalUsers { get; set; }

    }
}
