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

        public async Task<IActionResult> ViewDataByPlatform(int platformId, string sortBy, string sortOrder)
        {
            var viewModel = await GetDashboardViewModel();
            viewModel.PlatformId = platformId;
            viewModel.PlatformData = await GetPlatformData(platformId);

            switch (sortBy)
            {
                case "Name":
                    viewModel.PlatformData = sortOrder == "desc" ? viewModel.PlatformData.OrderBy(p => (p as dynamic).Name).ToList() : viewModel.PlatformData.OrderByDescending(p => (p as dynamic).Name).ToList();
                    break;
                case "Email":
                    viewModel.PlatformData = sortOrder == "desc" ? viewModel.PlatformData.OrderBy(p => (p as dynamic).Email).ToList() : viewModel.PlatformData.OrderByDescending(p => (p as dynamic).Email).ToList();
                    break;
                case "Specialization":
                    viewModel.PlatformData = sortOrder == "desc" ? viewModel.PlatformData.OrderBy(p => (p as dynamic).Specialization).ToList() : viewModel.PlatformData.OrderByDescending(p => (p as dynamic).Specialization).ToList();
                    break;
                case "Completed":
                    viewModel.PlatformData = sortOrder == "desc" ? viewModel.PlatformData.OrderBy(p => (p as dynamic).Completed).ToList() : viewModel.PlatformData.OrderByDescending(p => (p as dynamic).Completed).ToList();
                    break;
                case "Name_First":
                    viewModel.PlatformData = sortOrder == "desc" ? viewModel.PlatformData.OrderBy(p => (p as dynamic).Name_First).ToList() : viewModel.PlatformData.OrderByDescending(p => (p as dynamic).Name_First).ToList();
                    break;
                case "Name_Last":
                    viewModel.PlatformData = sortOrder == "desc" ? viewModel.PlatformData.OrderBy(p => (p as dynamic).Name_Last).ToList() : viewModel.PlatformData.OrderByDescending(p => (p as dynamic).Name_Last).ToList();
                    break;
                case "Phone":
                    viewModel.PlatformData = sortOrder == "desc" ? viewModel.PlatformData.OrderBy(p => (p as dynamic).Phone).ToList() : viewModel.PlatformData.OrderByDescending(p => (p as dynamic).Phone).ToList();
                    break;
                case "IntendedMajor":
                    viewModel.PlatformData = sortOrder == "desc" ? viewModel.PlatformData.OrderBy(p => (p as dynamic).IntendedMajor).ToList() : viewModel.PlatformData.OrderByDescending(p => (p as dynamic).IntendedMajor).ToList();
                    break;
                case "Mentor":
                    viewModel.PlatformData = sortOrder == "desc" ? viewModel.PlatformData.OrderBy(p => (p as dynamic).Mentor).ToList() : viewModel.PlatformData.OrderByDescending(p => (p as dynamic).Mentor).ToList();
                    break;
                case "Mentee":
                    viewModel.PlatformData = sortOrder == "desc" ? viewModel.PlatformData.OrderBy(p => (p as dynamic).Mentee).ToList() : viewModel.PlatformData.OrderByDescending(p => (p as dynamic).Mentee).ToList();
                    break;
                case "Date":
                    viewModel.PlatformData = sortOrder == "desc" ? viewModel.PlatformData.OrderBy(p => (p as dynamic).Date).ToList() : viewModel.PlatformData.OrderByDescending(p => (p as dynamic).Date).ToList();
                    break;
                case "MethodOfContact":
                    viewModel.PlatformData = sortOrder == "desc" ? viewModel.PlatformData.OrderBy(p => (p as dynamic).MethodOfContact).ToList() : viewModel.PlatformData.OrderByDescending(p => (p as dynamic).MethodOfContact).ToList();
                    break;
                default:
                    viewModel.PlatformData = viewModel.PlatformData.OrderBy(p => (p as dynamic).Name).ToList();
                    break;
            }
            viewModel.CurrentSortOrder = sortOrder == "asc" ? "desc" : "asc";
            viewModel.CurrentSortBy = sortBy;

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
