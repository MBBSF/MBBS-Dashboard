using MBBS.Dashboard.web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MBBS.Dashboard.web.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IActivityLogRepository _activityLogRepository;
        private readonly ApplicationDbContext _context;

        public DashboardController(
            IActivityLogRepository activityLogRepository,
            ApplicationDbContext context)
        {
            _activityLogRepository = activityLogRepository;
            _context = context; // Inject ApplicationDbContext
        }

        public async Task<IActionResult> Dashboard()
        {
            // Fetch data from the database
            var courseraData = await _context.ExcelDataCourseraSpecialization.ToListAsync();
            var cognitoData = await _context.ExcelDataCognitoMasterList.ToListAsync();
            var googleFormsData = await _context.ExcelDataGoogleFormsVolunteerProgram.ToListAsync();

            // Calculate KPIs
            var kpiData = new KpiData
            {
                TotalUsers = courseraData.Count + cognitoData.Count + googleFormsData.Count,
                TotalCourseraUsers = courseraData.Count,
                TotalCognitoUsers = cognitoData.Count,
                TotalGoogleFormsUsers = googleFormsData.Count,
                CompletedCourseraSpecializations = courseraData.Count(x => x.Completed == "Yes"),
                AverageCoursesCompleted = courseraData.Average(x => x.CompletedCourses) ?? 0,
                TopMentors = googleFormsData
                    .GroupBy(x => x.Mentor)
                    .OrderByDescending(g => g.Count())
                    .Take(5)
                    .Select(g => g.Key)
                    .ToList()
            };

            // Prepare the view model
            var model = new DashboardViewModel
            {
                KpiData = kpiData,
                ActivityLogs = _activityLogRepository.GetLogsForAccount(1).ToList(), // Convert to List
                CourseraData = courseraData,
                CognitoData = cognitoData,
                GoogleFormsData = googleFormsData
            };

            return View(model);
        }

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

        public async Task<IActionResult> ViewDataByPlatform(int platformId)
        {
            List<object> platformData = null;

            switch (platformId)
            {
                case 1: // Coursera
                    platformData = await _context.ExcelDataCourseraSpecialization
                        .Select(x => new { x.Name, x.Email, x.Specialization, x.Completed })
                        .ToListAsync<object>();
                    break;

                case 2: // Cognito
                    platformData = await _context.ExcelDataCognitoMasterList
                        .Select(x => new { x.Name_First, x.Name_Last, x.Phone, x.IntendedMajor })
                        .ToListAsync<object>();
                    break;

                case 3: // Google Forms
                    platformData = await _context.ExcelDataGoogleFormsVolunteerProgram
                        .Select(x => new { x.Mentor, x.Mentee, x.Date, x.MethodOfContact })
                        .ToListAsync<object>();
                    break;

                default:
                    return RedirectToAction("Dashboard");
            }

            ViewBag.PlatformId = platformId;
            ViewBag.PlatformData = platformData;

            return View("PlatformData");
        }
    }
}