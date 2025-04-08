using MBBS.Dashboard.web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
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

        public async Task<IActionResult> ViewDataByPlatform(int platformId, string sortBy, string sortOrder, string searchQuery = null)
        {
            var viewModel = await GetDashboardViewModel();
            viewModel.PlatformId = platformId;
            await GetPlatformData(viewModel, platformId);
            viewModel.SearchQuery = searchQuery;

            if (!string.IsNullOrEmpty(searchQuery))
            {
                switch (platformId)
                {
                    case 1:
                        viewModel.CourseraPlatformData = viewModel.CourseraPlatformData
                            .Where(p => p.Name?.ToLower().Contains(searchQuery.ToLower()) == true ||
                                       p.Email?.ToLower().Contains(searchQuery.ToLower()) == true ||
                                       p.Specialization?.ToLower().Contains(searchQuery.ToLower()) == true ||
                                       p.Completed?.ToLower().Contains(searchQuery.ToLower()) == true)
                            .ToList();
                        break;
                    case 2:
                        viewModel.CognitoPlatformData = viewModel.CognitoPlatformData
                            .Where(p => p.Name_First?.ToLower().Contains(searchQuery.ToLower()) == true ||
                                       p.Name_Last?.ToLower().Contains(searchQuery.ToLower()) == true ||
                                       p.Phone?.ToLower().Contains(searchQuery.ToLower()) == true ||
                                       p.IntendedMajor?.ToLower().Contains(searchQuery.ToLower()) == true)
                            .ToList();
                        break;
                    case 3:
                        viewModel.GoogleFormsPlatformData = viewModel.GoogleFormsPlatformData
                            .Where(p => p.Mentor?.ToLower().Contains(searchQuery.ToLower()) == true ||
                                       p.Mentee?.ToLower().Contains(searchQuery.ToLower()) == true ||
                                       p.Date?.ToString().ToLower().Contains(searchQuery.ToLower()) == true ||
                                       p.MethodOfContact?.ToLower().Contains(searchQuery.ToLower()) == true)
                            .ToList();
                        break;
                }
            }

            if (!string.IsNullOrEmpty(sortBy))
            {
                switch (platformId)
                {
                    case 1:
                        viewModel.CourseraPlatformData = sortOrder == "desc"
                            ? viewModel.CourseraPlatformData.OrderByDescending(x => x.GetType().GetProperty(sortBy)?.GetValue(x)).ToList()
                            : viewModel.CourseraPlatformData.OrderBy(x => x.GetType().GetProperty(sortBy)?.GetValue(x)).ToList();
                        break;
                    case 2:
                        viewModel.CognitoPlatformData = sortOrder == "desc"
                            ? viewModel.CognitoPlatformData.OrderByDescending(x => x.GetType().GetProperty(sortBy)?.GetValue(x)).ToList()
                            : viewModel.CognitoPlatformData.OrderBy(x => x.GetType().GetProperty(sortBy)?.GetValue(x)).ToList();
                        break;
                    case 3:
                        viewModel.GoogleFormsPlatformData = sortOrder == "desc"
                            ? viewModel.GoogleFormsPlatformData.OrderByDescending(x => x.GetType().GetProperty(sortBy)?.GetValue(x)).ToList()
                            : viewModel.GoogleFormsPlatformData.OrderBy(x => x.GetType().GetProperty(sortBy)?.GetValue(x)).ToList();
                        break;
                }
                viewModel.CurrentSortOrder = sortOrder == "asc" ? "desc" : "asc";
                viewModel.CurrentSortBy = sortBy;
            }

            return View("Dashboard", viewModel);
        }

        public async Task<IActionResult> CourseraReports(int platformId = 1, string reportType = "membership")
        {
            var viewModel = await GetDashboardViewModel();
            viewModel.PlatformId = platformId;
            await GetPlatformData(viewModel, platformId);
            return View("Dashboard", viewModel);
        }

        private async Task GetPlatformData(DashboardViewModel viewModel, int platformId)
        {
            switch (platformId)
            {
                case 1:
                    viewModel.CourseraPlatformData = await _context.ExcelDataCourseraSpecialization
                        .Select(x => new PlatformDataViewModel.CourseraSpecializationData
                        {
                            Name = x.Name,
                            Email = x.Email,
                            Specialization = x.Specialization,
                            Completed = x.Completed
                        }).ToListAsync();
                    break;
                case 2:
                    viewModel.CognitoPlatformData = await _context.ExcelDataCognitoMasterList
                        .Select(x => new PlatformDataViewModel.CognitoData
                        {
                            Name_First = x.Name_First,
                            Name_Last = x.Name_Last,
                            Phone = x.Phone,
                            IntendedMajor = x.IntendedMajor
                        }).ToListAsync();
                    break;
                case 3:
                    viewModel.GoogleFormsPlatformData = await _context.ExcelDataGoogleFormsVolunteerProgram
                        .Select(x => new PlatformDataViewModel.GoogleFormsData
                        {
                            Mentor = x.Mentor,
                            Mentee = x.Mentee,
                            Date = x.Date,
                            MethodOfContact = x.MethodOfContact
                        }).ToListAsync();
                    break;
            }
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
                    MemberState = x.MemberState,
                    Name = x.Name,
                    Email = x.Email,
                    ProgramName = x.ProgramName,
                    EnrolledCourses = x.EnrolledCourses ?? 0,
                    CompletedCourses = x.CompletedCourses ?? 0
                }).ToList(),
                CourseraPivotLocationCityReports = pivotReports.Select(x => new KpiDataViewModel.CourseraPivotLocationCityReportViewModel
                {
                    LocationCity = x.LocationCity,
                    CurrentMembers = x.CurrentMembers ?? 0,
                    CurrentLearners = x.CurrentLearners ?? 0,
                    TotalEnrollments = x.TotalEnrollments ?? 0,
                    TotalCompletedCourses = x.TotalCompletedCourses ?? 0,
                    AverageProgress = (double?)x.AverageProgress
                }).ToList(),
                CourseraUsageReports = usageReports.Select(x => new KpiDataViewModel.CourseraUsageReportViewModel
                {
                    Name = x.Name,
                    Course = x.Course,
                    OverallProgress = (double?)x.OverallProgress,
                    Completed = x.Completed,
                    EstimatedLearningHours = (double)(x.EstimatedLearningHours ?? 0)
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