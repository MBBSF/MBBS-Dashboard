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
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = await GetDashboardViewModel();
            return View("Dashboard", viewModel);
        }

        [HttpPost]
        public IActionResult ApplyFilter(string filterCriteria)
        {
            TempData["FilterCriteria"] = filterCriteria;
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ViewDataByPlatform(int platformId)
        {
            var viewModel = await GetDashboardViewModel();
            viewModel.PlatformId = platformId;
            viewModel.PlatformData = await GetPlatformData(platformId);

            return View("Dashboard", viewModel);
        }

        private async Task<DashboardViewModel> GetDashboardViewModel()
        {
            var courseraData = await _context.ExcelDataCourseraSpecialization.ToListAsync();
            var cognitoData = await _context.ExcelDataCognitoMasterList.ToListAsync();
            var googleFormsData = await _context.ExcelDataGoogleFormsVolunteerProgram.ToListAsync();

            var kpiData = new KpiData
            {
                TotalUsers = courseraData.Count + cognitoData.Count + googleFormsData.Count,
                TotalCourseraUsers = courseraData.Count,
                TotalCognitoUsers = cognitoData.Count,
                TotalGoogleFormsUsers = googleFormsData.Count,
                MentorCompletionRate = (double)courseraData.Count(x => x.Completed == "Yes") / courseraData.Count,
                SpecializationDistribution = courseraData.GroupBy(x => x.Specialization)
                    .ToDictionary(g => g.Key, g => g.Count()),
                IntendedMajorDistribution = cognitoData.GroupBy(x => x.IntendedMajor)
                    .ToDictionary(g => g.Key, g => g.Count()),
                PhoneNumberProvisionRate = (double)cognitoData.Count(x => !string.IsNullOrEmpty(x.Phone)) / cognitoData.Count,
                TotalMentoringSessions = googleFormsData.Count,
                ContactMethodPreference = googleFormsData.GroupBy(x => x.MethodOfContact)
                    .ToDictionary(g => g.Key, g => g.Count()),
                TopMentors = googleFormsData.GroupBy(x => x.Mentor)
                    .OrderByDescending(g => g.Count())
                    .Take(5)
                    .Select(g => g.Key)
                    .ToList()
            };

            return new DashboardViewModel
            {
                KpiData = kpiData,
                CourseraData = courseraData,
                CognitoData = cognitoData,
                GoogleFormsData = googleFormsData
            };
        }

        private async Task<List<object>> GetPlatformData(int platformId)
        {
            switch (platformId)
            {
                case 1:
                    return await _context.ExcelDataCourseraSpecialization
                        .Select(x => new { x.Name, x.Email, x.Specialization, x.Completed })
                        .Cast<object>().ToListAsync();
                case 2:
                    return await _context.ExcelDataCognitoMasterList
                        .Select(x => new { x.Name_First, x.Name_Last, x.Phone, x.IntendedMajor })
                        .Cast<object>().ToListAsync();
                case 3:
                    return await _context.ExcelDataGoogleFormsVolunteerProgram
                        .Select(x => new { x.Mentor, x.Mentee, x.Date, x.MethodOfContact })
                        .Cast<object>().ToListAsync();
                default:
                    return new List<object>();
            }
        }
    }
}
