using MBBS.Dashboard.web.Models;
using Microsoft.AspNetCore.Mvc;

namespace MBBS.Dashboard.web.Controllers
{
    public class PlatformController : Controller
    {
        // Simulating a platform repository
        private static List<Platform> Platforms = new List<Platform>
    {
        new Platform { PlatformId = 1, Name = "Coursera" },
        new Platform { PlatformId = 2, Name = "Cognito" },
        new Platform { PlatformId = 2, Name = "Google Forms" }
    };

        [HttpPost]
        public ActionResult ChangePlatform(int platformId)
        {
            var selectedPlatform = Platforms.Find(p => p.PlatformId == platformId);
            if (selectedPlatform != null)
            {
                // Logic to switch the dashboard data source
                ViewBag.Message = $"Platform switched to {selectedPlatform.Name}";
            }
            else
            {
                ViewBag.Message = "Platform not found.";
            }
            return View("Dashboard");
        }
    }
}
