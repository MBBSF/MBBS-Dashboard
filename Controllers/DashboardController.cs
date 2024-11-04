using FirstIterationProductRelease.Models;
using Microsoft.AspNetCore.Mvc;

namespace FirstIterationProductRelease.Controllers
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
        }
    }
}
