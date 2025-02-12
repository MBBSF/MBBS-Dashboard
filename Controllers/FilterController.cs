using Microsoft.AspNetCore.Mvc;

namespace MBBS.Dashboard.web.Controllers
{
    public class FilterController : Controller
    {
        [HttpPost]
        public ActionResult ReadAndApplyFilter(string filterCriteria)
        {
            // Logic to apply filter on the dashboard data
            ViewBag.FilterApplied = filterCriteria;
            return View("Dashboard");
        }
    }
}
