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

        public DashboardController(IActivityLogRepository activityLogRepository, ApplicationDbContext context)
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

            if (!string.IsNullOrEmpty(sortBy))
            {
                viewModel.PlatformData = SortPlatformData(viewModel.PlatformData, sortBy, sortOrder);
                viewModel.CurrentSortOrder = sortOrder == "asc" ? "desc" : "asc";
                viewModel.CurrentSortBy = sortBy;
            }

            return View("Dashboard", viewModel);
        }

        public async Task<IActionResult> CourseraReports(int platformId = 1, string reportType = "membership")
        {
            var viewModel = await GetDashboardViewModel();
            viewModel.PlatformId = platformId;
            return View("Dashboard", viewModel);
        }

        private List<object> SortPlatformData(List<object> data, string sortBy, string sortOrder)
        {
            var sorted = data.OrderBy(x => 0); // default ordering

            try
            {
                sorted = sortOrder == "desc"
                    ? data.OrderBy(x => x?.GetType().GetProperty(sortBy)?.GetValue(x, null))
                    : data.OrderByDescending(x => x?.GetType().GetProperty(sortBy)?.GetValue(x, null));
            }
            catch
            {
                // log or ignore
            }

            return sorted.ToList();
        }

        private async Task<List<object>> GetPlatformData(int platformId)
        {
            return platformId switch
            {
                1 => await _context.ExcelDataCourseraSpecialization
                            .Select(x => new { x.Name, x.Email, x.Specialization, x.Completed })
                            .Cast<object>().ToListAsync(),
                2 => await _context.ExcelDataCognitoMasterList
                            .Select(x => new { x.Name_First, x.Name_Last, x.Phone, x.IntendedMajor })
                            .Cast<object>().ToListAsync(),
                3 => await _context.ExcelDataGoogleFormsVolunteerProgram
                            .Select(x => new { x.Mentor, x.Mentee, x.Date, x.MethodOfContact })
                            .Cast<object>().ToListAsync(),
                _ => new List<object>()
            };
        }

        private async Task<DashboardViewModel> GetDashboardViewModel()
        {
            var courseraData = await _context.ExcelDataCourseraSpecialization.ToListAsync();
            var cognitoData = await _context.ExcelDataCognitoMasterList.ToListAsync();
            var googleFormsData = await _context.ExcelDataGoogleFormsVolunteerProgram.ToListAsync();
            var membershipReports = await _context.ExcelDataCourseraMembershipReports.ToListAsync();
            var pivotReports = await _context.ExcelDataCourseraPivotLocationCityReports.ToListAsync();
            var usageReports = await _context.ExcelDataCourseraUsageReports.ToListAsync();
            var activityLogs = await _activityLogRepository.GetRecentActivityLogsAsync(50);

            var googleCertKPIs = new KpiDataViewModel.GoogleCertificationKPIsViewModel
            {
                TotalParticipants = courseraData.Count,
                CompletedCertifications = courseraData.Count(x => x.Completed == "Yes"),
                CompletionRate = courseraData.Count > 0 ?
                    (double)courseraData.Count(x => x.Completed == "Yes") / courseraData.Count : 0,
                SpecializationDistribution = courseraData.GroupBy(x => x.Specialization ?? "Unknown")
                    .ToDictionary(g => g.Key, g => g.Count()),
                LocationDistribution = courseraData.GroupBy(x => x.LocationCity ?? "Unknown")
                    .ToDictionary(g => g.Key, g => g.Count()),
                ActiveLearners = membershipReports.Count(x => x.MemberState == "active")
            };

            var mentoringKPIs = new MentoringProgramKPIsViewModel
            {
                TotalMentoringSessions = googleFormsData.Count,
                ContactMethodDistribution = googleFormsData.GroupBy(x => x.MethodOfContact ?? "Unknown")
                    .ToDictionary(g => g.Key, g => g.Count()),
                TopMentors = googleFormsData.GroupBy(x => x.Mentor)
                    .OrderByDescending(g => g.Count()).Take(5).Select(g => g.Key).ToList(),
                UniqueMentees = googleFormsData.Select(x => x.Mentee).Distinct().Count(),
                AverageSessionsPerMentee = googleFormsData.Select(x => x.Mentee).Distinct().Count() > 0 ?
                    (double)googleFormsData.Count / googleFormsData.Select(x => x.Mentee).Distinct().Count() : 0
            };

            var scholarshipKPIs = new ScholarshipApplicationKPIsViewModel
            {
                TotalApplications = cognitoData.Count,
                IntendedMajorDistribution = cognitoData.GroupBy(x => x.IntendedMajor ?? "Unknown")
                    .ToDictionary(g => g.Key, g => g.Count()),
                PhoneNumberProvisionRate = cognitoData.Count > 0 ?
                    (double)cognitoData.Count(x => !string.IsNullOrEmpty(x.Phone)) / cognitoData.Count : 0,
                SchoolDistribution = cognitoData.GroupBy(x => x.HighSchoolCollegeData_HighSchoolCollegeInformation ?? "Unknown")
                    .ToDictionary(g => g.Key, g => g.Count()),
                AverageGPA = (double)cognitoData.Where(x => x.HighSchoolCollegeData_CumulativeGPA.HasValue)
                    .Select(x => x.HighSchoolCollegeData_CumulativeGPA.Value).DefaultIfEmpty(0).Average()
            };

            return new DashboardViewModel
            {
                KpiData = new KpiDataViewModel
                {
                    TotalUsers = courseraData.Count + cognitoData.Count + googleFormsData.Count,
                    TotalCourseraUsers = courseraData.Count,
                    TotalCognitoUsers = cognitoData.Count,
                    TotalGoogleFormsUsers = googleFormsData.Count
                },
                GoogleCertificationKPIs = googleCertKPIs,
                MentoringProgramKPIs = mentoringKPIs,
                ScholarshipApplicationKPIs = scholarshipKPIs,
                CourseraMembershipReports = membershipReports.Select(x => new KpiDataViewModel.CourseraMembershipReportViewModel
                {
                    MemberState = x.MemberState
                }).ToList(),
                CourseraPivotLocationCityReports = pivotReports.Select(x => new KpiDataViewModel.CourseraPivotLocationCityReportViewModel
                {
                    // Map accordingly
                }).ToList(),
                CourseraUsageReports = usageReports.Select(x => new KpiDataViewModel.CourseraUsageReportViewModel
                {
                    // Map accordingly
                }).ToList(),
                CourseraData = courseraData,
                CognitoData = cognitoData,
                GoogleFormsData = googleFormsData,
                ActivityLogs = activityLogs.Select(x => new ActivityLogViewModel
                {
                    Action = x.Action,
                    Timestamp = x.Timestamp
                }).ToList()
            };
        }
    }
}
