using MBBS.Dashboard.web.Models;
using Microsoft.AspNetCore.Mvc;

namespace MBBS.Dashboard.web.Controllers
{
    public class DashboardController : Controller
    {

        private readonly IActivityLogRepository _activityLogRepository;

        public DashboardController(IActivityLogRepository activityLogRepository)
        {
            _activityLogRepository = activityLogRepository;
        }
        public IActionResult Dashboard()
        {
            var model = new DashboardViewModel
            {
                KpiData = new KpiData
                {
                    TotalUsers = 100 // will convert to database
                },
                ActivityLogs = _activityLogRepository.GetLogsForAccount(1) // Example account ID
            };

            return View(model);

        public IActionResult Index()
        {
            var viewModel = new DashboardViewModel
            {
                KpiData = new KpiData { TotalUsers = 100 }, // Example data
                ActivityLogs = new List<ActivityLog>() // Add some test activity logs if needed
            };

            return View("Dashboard", viewModel); // Explicitly loading Dashboard.cshtml
        }

        [HttpPost]
        public IActionResult ApplyFilter(string filterCriteria)
        {
            ViewBag.Filter = filterCriteria;
            return RedirectToAction("Index"); // Redirecting to avoid resubmission on refresh
        }

        public IActionResult ViewDataByPlatform(int platformId)
        {
            ViewBag.PlatformId = platformId;
            return RedirectToAction("Index"); // Redirecting for consistency

        }
    }
}