using MBBS.Dashboard.web.Models;
using Microsoft.AspNetCore.Mvc;

namespace MBBS.Dashboard.web.Controllers
{
    public class DashboardController : Controller
    {
        public ActionResult Index()
        {
            // Load initial dashboard view
            return View();
        }

        [HttpPost]
        public ActionResult ApplyFilter(string filterCriteria)
        {
            ViewBag.Filter = filterCriteria;
            return View("Index");
        }

        public ActionResult ViewDataByPlatform(int platformId)
        {
            // Example: Fetch data based on selected platform
            ViewBag.PlatformId = platformId;
            return View("Index");
        }
    }
}
