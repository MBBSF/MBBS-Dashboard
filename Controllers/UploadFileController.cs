using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CsvHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using CsvHelper.Configuration;
using MBBS.Dashboard.web.Mappings;
using MBBS.Dashboard.web.Models;
using Microsoft.EntityFrameworkCore;
using MBBS.Dashboard.web.Controllers;

public class UploadFileController : Controller
{
    private readonly ApplicationDbContext _context;

    public UploadFileController(ApplicationDbContext context)
    {
        _context = context;
    }

    private readonly Dictionary<string, List<string>> ExpectedHeaders = new()
    {
        { "Coursera", new List<string>
            {
                "Name", "Email", "External Id", "Specialization", "Specialization Slug", "University",
                "Enrollment Time", "Last Specialization Activity Time", "# Completed Courses", "# Courses in Specialization",
                "Completed", "Removed From Program", "Program Slug", "Program Name", "Enrollment Source",
                "Specialization Completion Time", "Specialization Certificate URL", "Job Title", "Job Type",
                "Location City", "Location Region", "Location Country"
            }
        },
        { "Cognito", new List<string>
            {
                "MARIEBARNEYBOSTONSCHOLARSHIPFOU_Id", "Name_First", "Name_Middle", "Name_Last", "Phone", "Age",
                "Address_Line1", "Address_Line2", "Address_City", "Address_State", "Address_PostalCode",
                "IntendedMajor", "ExpectedGraduation", "CollegePlanToAttend_Name", "CollegePlanToAttend_CityState",
                "HighSchoolCollegeData_CurrentStudent", "HighSchoolCollegeData_HighSchoolCollegeInformation",
                "HighSchoolCollegeData_Phone", "HighSchoolCollegeData_HighSchoolGraduation", "HighSchoolCollegeData_CumulativeGPA",
                "HighSchoolCollegeData_ACTCompositeScore", "HighSchoolCollegeData_SATCompositeScore",
                "HighSchoolCollegeData_SchoolCommunityRelatedActivities", "HighSchoolCollegeData_HonorsAndSpecialRecognition",
                "HighSchoolCollegeData_ExplainYourNeedForAssistance", "WriteYourNameAsFormOfSignature",
                "Date", "Entry_Status", "Entry_DateCreated", "Entry_DateSubmitted", "Entry_DateUpdated"
            }
        },
        { "GoogleForms", new List<string>
            {
                "Timestamp", "Mentor", "Mentee", "Date", "Time", "Method of Contact", "Comment"
            }
        },
        { "Coursera-membership-report", new List<string>
            {
                "Name", "Email", "External Id", "Program Name", "Program Slug",
                "# Enrolled Courses", "# Completed Courses", "Member State",
                "Join Date", "Invitation Date", "Latest Program Activity Date",
                "Job Title", "Job Type", "Location City", "Location Region", "Location Country"
            }
        },
        { "Coursera-pivot-location-city-report", new List<string>
            {
                "Location City", "Current Members", "Current Learners", "Total Enrollments", "Total Completed Courses",
                "Average Progress", "Total Estimated Learning Hours", "Average Estimated Learning Hours", "Deleted Members"
            }
        },
        { "Coursera-usage-report", new List<string>
            {
                "Name", "Email", "External Id", "Course", "Course ID", "Course Slug", "University",
                "Enrollment Time", "Class Start Time", "Class End Time", "Last Course Activity Time", "Overall Progress",
                "Estimated Learning Hours", "Completed", "Removed From Program", "Program Slug", "Program Name",
                "Enrollment Source", "Completion Time", "Course Grade", "Course Certificate URL", "For Credit",
                "Course Type", "Job Title", "Job Type", "Location City", "Location Region", "Location Country"
            }
        }
    };

    private string NormalizeHeader(string header)
    {
        // Trim the header and replace any sequence of whitespace with a single space.
        return Regex.Replace(header.Trim(), @"\s+", " ");
    }

    private async Task<bool> ValidateFileHeaders(IFormFile file, string source, string fileExtension, string fileType = null)
    {
        // Determine the key for ExpectedHeaders.
        string headerKey = source;
        if (source == "Coursera")
        {
            if (fileType == "membership-report")
                headerKey = "Coursera-membership-report";
            else if (fileType == "pivot-location-city-report")
                headerKey = "Coursera-pivot-location-city-report";
            else if (fileType == "usage-report")
                headerKey = "Coursera-usage-report";
            // Otherwise, assume specialization-report or default.
        }

        if (!ExpectedHeaders.ContainsKey(headerKey))
            return false;

        var expectedHeaders = ExpectedHeaders[headerKey].Select(NormalizeHeader).ToList();

        if (fileExtension == ".csv")
        {
            using var streamReader = new StreamReader(file.OpenReadStream());
            using var csv = new CsvReader(streamReader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true
            });
            await csv.ReadAsync();
            csv.ReadHeader();
            var fileHeaders = csv.HeaderRecord?.Select(NormalizeHeader).ToList();
            // Remove trailing empty headers.
            while (fileHeaders != null && fileHeaders.Count > 0 && string.IsNullOrEmpty(fileHeaders.Last()))
            {
                fileHeaders.RemoveAt(fileHeaders.Count - 1);
            }
            if (fileHeaders == null || !fileHeaders.SequenceEqual(expectedHeaders, StringComparer.OrdinalIgnoreCase))
            {
                return false;
            }
        }
        else if (fileExtension == ".xlsx")
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var stream = new MemoryStream();
            file.CopyTo(stream);
            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets[0];
            int colCount = worksheet.Dimension.Columns;
            var fileHeaders = new List<string>();
            for (int col = 1; col <= colCount; col++)
            {
                fileHeaders.Add(NormalizeHeader(worksheet.Cells[1, col].Text));
            }
            while (fileHeaders.Count > 0 && string.IsNullOrEmpty(fileHeaders.Last()))
            {
                fileHeaders.RemoveAt(fileHeaders.Count - 1);
            }
            if (!fileHeaders.SequenceEqual(expectedHeaders, StringComparer.OrdinalIgnoreCase))
            {
                return false;
            }
        }
        return true;
    }

    [HttpGet]
    public IActionResult Upload()
    {
        return View();
    }

    [TempData]
    public string DuplicateMessage { get; set; }

    [HttpPost]
    public async Task<IActionResult> Upload(UploadFile model)
    {
        // Check if a file, data source, and file type have been provided.
        if (model.File == null || model.File.Length == 0 ||
            string.IsNullOrEmpty(model.Source) ||
            string.IsNullOrEmpty(model.FileType))
        {
            ModelState.AddModelError("", "Please select a file, data source, and file type.");
            model.Source = "";
            model.FileType = "";
            ModelState.Remove("Source");
            ModelState.Remove("FileType");
            return View(model);
        }

        // Get the file extension in lower case.
        var fileExtension = Path.GetExtension(model.File.FileName).ToLower();

        // Check if the file extension is either .csv or .xlsx.
        if (fileExtension != ".csv" && fileExtension != ".xlsx")
        {
            ModelState.AddModelError("", "Only .csv and .xlsx files are supported.");
            model.Source = "";
            model.FileType = "";
            ModelState.Remove("Source");
            ModelState.Remove("FileType");
            return View(model);
        }

        int duplicateCount = 0;
        try
        {
            // Validate headers against the expected format.
            if (!await ValidateFileHeaders(model.File, model.Source, fileExtension, model.FileType))
            {
                ModelState.AddModelError("", "The uploaded file does not match the expected format for the selected data source.");
                model.Source = "";
                model.FileType = "";
                ModelState.Remove("Source");
                ModelState.Remove("FileType");
                return View(model);
            }

            // Process based on the selected source.
            switch (model.Source)
            {
                case "Coursera":
                    switch (fileExtension)
                    {
                        case ".csv":
                            switch (model.FileType)
                            {
                                case "specialization-report":
                                    duplicateCount = await ProcessCourseraSpecializationCsvData(model.File);
                                    break;
                                case "membership-report":
                                    duplicateCount = await ProcessCourseraMembershipCsvData(model.File);
                                    break;
                                case "pivot-location-city-report":
                                    duplicateCount = await ProcessCourseraPivotLocationCityCsvData(model.File);
                                    break;
                                case "usage-report":
                                    duplicateCount = await ProcessCourseraUsageCsvData(model.File);
                                    break;
                                default:
                                    ModelState.AddModelError("", "Invalid file type selected for Coursera.");
                                    model.Source = "";
                                    model.FileType = "";
                                    ModelState.Remove("Source");
                                    ModelState.Remove("FileType");
                                    return View(model);
                            }
                            break;
                        case ".xlsx":
                            switch (model.FileType)
                            {
                                case "specialization-report":
                                    duplicateCount = await ProcessCourseraSpecializationXlsxData(model.File);
                                    break;
                                case "membership-report":
                                    duplicateCount = await ProcessCourseraMembershipXlsxData(model.File);
                                    break;
                                case "pivot-location-city-report":
                                    duplicateCount = await ProcessCourseraPivotLocationCityXlsxData(model.File);
                                    break;
                                case "usage-report":
                                    duplicateCount = await ProcessCourseraUsageXlsxData(model.File);
                                    break;
                                default:
                                    ModelState.AddModelError("", "Invalid file type selected for Coursera.");
                                    model.Source = "";
                                    model.FileType = "";
                                    ModelState.Remove("Source");
                                    ModelState.Remove("FileType");
                                    return View(model);
                            }
                            break;
                        default:
                            ModelState.AddModelError("", "Only .csv and .xlsx files are supported for Coursera.");
                            model.Source = "";
                            model.FileType = "";
                            ModelState.Remove("Source");
                            ModelState.Remove("FileType");
                            return View(model);
                    }
                    break;
                case "GoogleForms":
                    switch (fileExtension)
                    {
                        case ".csv":
                            duplicateCount = await ProcessGoogleFormsVolunteerProgramCsvData(model.File);
                            break;
                        case ".xlsx":
                            duplicateCount = await ProcessGoogleFormsVolunteerProgramXlsxData(model.File);
                            break;
                        default:
                            ModelState.AddModelError("", "Only .csv and .xlsx files are supported for Google Forms.");
                            model.Source = "";
                            model.FileType = "";
                            ModelState.Remove("Source");
                            ModelState.Remove("FileType");
                            return View(model);
                    }
                    break;
                case "Cognito":
                    switch (fileExtension)
                    {
                        case ".csv":
                            duplicateCount = await ProcessCognitoMasterListCsvData(model.File);
                            break;
                        case ".xlsx":
                            duplicateCount = await ProcessCognitoMasterListXlsxData(model.File);
                            break;
                        default:
                            ModelState.AddModelError("", "Only .csv and .xlsx files are supported for Cognito.");
                            model.Source = "";
                            model.FileType = "";
                            ModelState.Remove("Source");
                            ModelState.Remove("FileType");
                            return View(model);
                    }
                    break;
                default:
                    ModelState.AddModelError("", "Invalid data source selected.");
                    model.Source = "";
                    model.FileType = "";
                    ModelState.Remove("Source");
                    ModelState.Remove("FileType");
                    return View(model);
            }

            DuplicateMessage = duplicateCount > 0
                ? $"{duplicateCount} duplicate record(s) were detected and skipped."
                : "No duplicate records were detected.";

            return RedirectToAction("UploadSuccess");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Error processing file: {ex.Message}");
            model.Source = "";
            model.FileType = "";
            ModelState.Remove("Source");
            ModelState.Remove("FileType");
            return View(model);
        }
    }

    #region File Processing Methods

    private async Task<int> ProcessCourseraSpecializationCsvData(IFormFile file)
    {
        int dbDuplicateCount = 0, fileDuplicateCount = 0;
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            HeaderValidated = null,
            MissingFieldFound = null
        };
        using var streamReader = new StreamReader(file.OpenReadStream());
        using var csv = new CsvReader(streamReader, config);
        csv.Context.RegisterClassMap<ExcelDataCourseraSpecializationMap>();
        var dataList = new List<ExcelDataCourseraSpecialization>();
        var fileKeys = new HashSet<string>();
        int currentAccountId = AccountController.ActiveAccount?.Id ?? 0;
        await foreach (var record in csv.GetRecordsAsync<ExcelDataCourseraSpecialization>())
        {
            // Set defaults as needed.
            record.EnrollmentSource ??= "Unknown";
            record.ProgramName ??= "Not Specified";
            record.LocationRegion ??= "Not Specified";
            record.AccountId = currentAccountId;
            // Remove AccountId from the composite key.
            string key = $"{record.Email?.Trim()}|{record.ExternalId?.Trim()}";
            if (!fileKeys.Add(key))
            {
                fileDuplicateCount++;
                continue;
            }
            bool exists = await _context.ExcelDataCourseraSpecialization.AnyAsync(r =>
                r.Email == record.Email &&
                (string.IsNullOrEmpty(record.ExternalId) || r.ExternalId == record.ExternalId)
            );
            if (exists)
                dbDuplicateCount++;
            else
                dataList.Add(record);
        }
        _context.ExcelDataCourseraSpecialization.AddRange(dataList);
        await _context.SaveChangesAsync();
        return dbDuplicateCount + fileDuplicateCount;
    }

    private async Task<int> ProcessCourseraSpecializationXlsxData(IFormFile file)
    {
        int dbDuplicateCount = 0, fileDuplicateCount = 0;
        OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
        int currentAccountId = AccountController.ActiveAccount?.Id ?? 0;
        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        using var package = new ExcelPackage(stream);
        var worksheet = package.Workbook.Worksheets[0];
        int rowCount = worksheet.Dimension.Rows;
        var dataList = new List<ExcelDataCourseraSpecialization>();
        var fileKeys = new HashSet<string>();
        // Process rows starting at row 2 (header is in row 1).
        for (int row = 2; row <= rowCount; row++)
        {
            var record = new ExcelDataCourseraSpecialization
            {
                Name = worksheet.Cells[row, 1].Text,
                Email = worksheet.Cells[row, 2].Text,
                ExternalId = worksheet.Cells[row, 3].Text,
                Specialization = worksheet.Cells[row, 4].Text,
                SpecializationSlug = worksheet.Cells[row, 5].Text,
                University = worksheet.Cells[row, 6].Text,
                EnrollmentTime = DateTime.TryParse(worksheet.Cells[row, 7].Text, out var enrollmentTime)
                                  ? enrollmentTime : (DateTime?)null,
                LastSpecializationActivityTime = DateTime.TryParse(worksheet.Cells[row, 8].Text, out var lastActivity)
                                  ? lastActivity : (DateTime?)null,
                CompletedCourses = int.TryParse(worksheet.Cells[row, 9].Text, out var completedCourses)
                                  ? completedCourses : (int?)null,
                CoursesInSpecialization = int.TryParse(worksheet.Cells[row, 10].Text, out var coursesInSpec)
                                  ? coursesInSpec : (int?)null,
                Completed = worksheet.Cells[row, 11].Text,
                RemovedFromProgram = worksheet.Cells[row, 12].Text,
                ProgramSlug = worksheet.Cells[row, 13].Text,
                ProgramName = worksheet.Cells[row, 14].Text,
                EnrollmentSource = worksheet.Cells[row, 15].Text,
                SpecializationCompletionTime = DateTime.TryParse(worksheet.Cells[row, 16].Text, out var specCompletion)
                                  ? specCompletion : (DateTime?)null,
                SpecializationCertificateURL = worksheet.Cells[row, 17].Text,
                JobTitle = worksheet.Cells[row, 18].Text,
                JobType = worksheet.Cells[row, 19].Text,
                LocationCity = worksheet.Cells[row, 20].Text,
                LocationRegion = worksheet.Cells[row, 21].Text,
                LocationCountry = worksheet.Cells[row, 22].Text,
                AccountId = currentAccountId
            };
            // Remove AccountId from the duplicate key.
            string key = $"{record.Email?.Trim()}|{record.ExternalId?.Trim()}";
            if (!fileKeys.Add(key))
            {
                fileDuplicateCount++;
                continue;
            }
            bool exists = await _context.ExcelDataCourseraSpecialization.AnyAsync(r =>
                r.Email == record.Email &&
                (string.IsNullOrEmpty(record.ExternalId) || r.ExternalId == record.ExternalId)
            );
            if (exists)
                dbDuplicateCount++;
            else
                dataList.Add(record);
        }
        _context.ExcelDataCourseraSpecialization.AddRange(dataList);
        await _context.SaveChangesAsync();
        return dbDuplicateCount + fileDuplicateCount;
    }

    private async Task<int> ProcessCourseraMembershipCsvData(IFormFile file)
    {
        int dbDuplicateCount = 0, fileDuplicateCount = 0;
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            HeaderValidated = null,
            MissingFieldFound = null
        };
        using var streamReader = new StreamReader(file.OpenReadStream());
        using var csv = new CsvReader(streamReader, config);
        csv.Context.RegisterClassMap<ExcelDataCourseraMembershipReportMap>();
        var dataList = new List<ExcelDataCourseraMembershipReport>();
        var fileKeys = new HashSet<string>();
        int currentAccountId = AccountController.ActiveAccount?.Id ?? 0;
        await foreach (var record in csv.GetRecordsAsync<ExcelDataCourseraMembershipReport>())
        {
            record.AccountId = currentAccountId;
            // Remove AccountId from the composite key.
            string key = $"{record.Email?.Trim()}|{record.ExternalId?.Trim()}";
            if (!fileKeys.Add(key))
            {
                fileDuplicateCount++;
                continue;
            }
            bool exists = await _context.ExcelDataCourseraMembershipReports.AnyAsync(r =>
                r.Email == record.Email &&
                (string.IsNullOrEmpty(record.ExternalId) || r.ExternalId == record.ExternalId)
            );
            if (!exists)
                dataList.Add(record);
            else
                dbDuplicateCount++;
        }
        _context.ExcelDataCourseraMembershipReports.AddRange(dataList);
        await _context.SaveChangesAsync();
        return dbDuplicateCount + fileDuplicateCount;
    }

    private async Task<int> ProcessCourseraMembershipXlsxData(IFormFile file)
    {
        int dbDuplicateCount = 0, fileDuplicateCount = 0;
        OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
        int currentAccountId = AccountController.ActiveAccount?.Id ?? 0;
        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        using var package = new ExcelPackage(stream);
        var worksheet = package.Workbook.Worksheets[0];
        int rowCount = worksheet.Dimension.Rows;
        var dataList = new List<ExcelDataCourseraMembershipReport>();
        var fileKeys = new HashSet<string>();
        for (int row = 2; row <= rowCount; row++)
        {
            var record = new ExcelDataCourseraMembershipReport
            {
                Name = worksheet.Cells[row, 1].Text,
                Email = worksheet.Cells[row, 2].Text,
                ExternalId = worksheet.Cells[row, 3].Text,
                ProgramName = worksheet.Cells[row, 4].Text,
                ProgramSlug = worksheet.Cells[row, 5].Text,
                EnrolledCourses = int.TryParse(worksheet.Cells[row, 6].Text, out var enrolledCourses) ? enrolledCourses : (int?)null,
                CompletedCourses = int.TryParse(worksheet.Cells[row, 7].Text, out var completedCourses) ? completedCourses : (int?)null,
                MemberState = worksheet.Cells[row, 8].Text,
                JoinDate = DateTime.TryParse(worksheet.Cells[row, 9].Text, out var joinDate) ? joinDate : (DateTime?)null,
                InvitationDate = DateTime.TryParse(worksheet.Cells[row, 10].Text, out var invitationDate) ? invitationDate : (DateTime?)null,
                LatestProgramActivityDate = DateTime.TryParse(worksheet.Cells[row, 11].Text, out var latestActivityDate) ? latestActivityDate : (DateTime?)null,
                JobTitle = worksheet.Cells[row, 12].Text,
                JobType = worksheet.Cells[row, 13].Text,
                LocationCity = worksheet.Cells[row, 14].Text,
                LocationRegion = worksheet.Cells[row, 15].Text,
                LocationCountry = worksheet.Cells[row, 16].Text,
                AccountId = currentAccountId
            };
            string key = $"{record.Email?.Trim()}|{record.ExternalId?.Trim()}";
            if (!fileKeys.Add(key))
            {
                fileDuplicateCount++;
                continue;
            }
            bool exists = await _context.ExcelDataCourseraMembershipReports.AnyAsync(r =>
                r.Email == record.Email &&
                (string.IsNullOrEmpty(record.ExternalId) || r.ExternalId == record.ExternalId)
            );
            if (!exists)
                dataList.Add(record);
            else
                dbDuplicateCount++;
        }
        _context.ExcelDataCourseraMembershipReports.AddRange(dataList);
        await _context.SaveChangesAsync();
        return dbDuplicateCount + fileDuplicateCount;
    }

    private async Task<int> ProcessCourseraUsageCsvData(IFormFile file)
    {
        int dbDuplicateCount = 0, fileDuplicateCount = 0;
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            HeaderValidated = null,
            MissingFieldFound = null
        };

        int currentAccountId = AccountController.ActiveAccount?.Id ?? 0;
        using var streamReader = new StreamReader(file.OpenReadStream());
        using var csv = new CsvReader(streamReader, config);
        csv.Context.RegisterClassMap<ExcelDataCourseraUsageReportMap>();
        var dataList = new List<ExcelDataCourseraUsageReport>();
        var fileKeys = new HashSet<string>();

        await foreach (var record in csv.GetRecordsAsync<ExcelDataCourseraUsageReport>())
        {
            record.AccountId = currentAccountId;
            // Build a composite key without AccountId.
            string key = string.Join("|", new string[]
            {
                record.Name?.Trim() ?? "",
                record.Email?.Trim() ?? "",
                record.ExternalId?.Trim() ?? "",
                record.Course?.Trim() ?? "",
                record.CourseId?.Trim() ?? "",
                record.CourseSlug?.Trim() ?? "",
                record.University?.Trim() ?? "",
                record.EnrollmentTime?.ToString("o") ?? "",
                record.ClassStartTime?.ToString("o") ?? "",
                record.ClassEndTime?.ToString("o") ?? "",
                record.LastCourseActivityTime?.ToString("o") ?? "",
                record.OverallProgress?.ToString() ?? "",
                record.EstimatedLearningHours?.ToString() ?? "",
                record.Completed?.Trim() ?? "",
                record.RemovedFromProgram?.Trim() ?? "",
                record.ProgramSlug?.Trim() ?? "",
                record.ProgramName?.Trim() ?? "",
                record.EnrollmentSource?.Trim() ?? "",
                record.CompletionTime?.ToString("o") ?? "",
                record.CourseGrade?.Trim() ?? "",
                record.CourseCertificateURL?.Trim() ?? "",
                record.ForCredit?.Trim() ?? "",
                record.CourseType?.Trim() ?? "",
                record.JobTitle?.Trim() ?? "",
                record.JobType?.Trim() ?? "",
                record.LocationCity?.Trim() ?? "",
                record.LocationRegion?.Trim() ?? "",
                record.LocationCountry?.Trim() ?? ""
            });
            if (!fileKeys.Add(key))
            {
                fileDuplicateCount++;
                continue;
            }
            bool exists = await _context.ExcelDataCourseraUsageReports.AnyAsync(r =>
                r.Name == record.Name &&
                r.Email == record.Email &&
                r.ExternalId == record.ExternalId &&
                r.Course == record.Course &&
                r.CourseId == record.CourseId &&
                r.CourseSlug == record.CourseSlug &&
                r.University == record.University &&
                r.EnrollmentTime == record.EnrollmentTime &&
                r.ClassStartTime == record.ClassStartTime &&
                r.ClassEndTime == record.ClassEndTime &&
                r.LastCourseActivityTime == record.LastCourseActivityTime &&
                r.OverallProgress == record.OverallProgress &&
                r.EstimatedLearningHours == record.EstimatedLearningHours &&
                r.Completed == record.Completed &&
                r.RemovedFromProgram == record.RemovedFromProgram &&
                r.ProgramSlug == record.ProgramSlug &&
                r.ProgramName == record.ProgramName &&
                r.EnrollmentSource == record.EnrollmentSource &&
                r.CompletionTime == record.CompletionTime &&
                r.CourseGrade == record.CourseGrade &&
                r.CourseCertificateURL == record.CourseCertificateURL &&
                r.ForCredit == record.ForCredit &&
                r.CourseType == record.CourseType &&
                r.JobTitle == record.JobTitle &&
                r.JobType == record.JobType &&
                r.LocationCity == record.LocationCity &&
                r.LocationRegion == record.LocationRegion &&
                r.LocationCountry == record.LocationCountry
            );
            if (exists)
                dbDuplicateCount++;
            else
                dataList.Add(record);
        }
        _context.ExcelDataCourseraUsageReports.AddRange(dataList);
        await _context.SaveChangesAsync();
        return dbDuplicateCount + fileDuplicateCount;
    }

    private async Task<int> ProcessCourseraUsageXlsxData(IFormFile file)
    {
        int dbDuplicateCount = 0, fileDuplicateCount = 0;
        OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
        int currentAccountId = AccountController.ActiveAccount?.Id ?? 0;
        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        using var package = new ExcelPackage(stream);
        var worksheet = package.Workbook.Worksheets[0];
        int rowCount = worksheet.Dimension.Rows;
        var dataList = new List<ExcelDataCourseraUsageReport>();
        var fileKeys = new HashSet<string>();
        for (int row = 2; row <= rowCount; row++)
        {
            var record = new ExcelDataCourseraUsageReport
            {
                Name = worksheet.Cells[row, 1].Text,
                Email = worksheet.Cells[row, 2].Text,
                ExternalId = worksheet.Cells[row, 3].Text,
                Course = worksheet.Cells[row, 4].Text,
                CourseId = worksheet.Cells[row, 5].Text,
                CourseSlug = worksheet.Cells[row, 6].Text,
                University = worksheet.Cells[row, 7].Text,
                EnrollmentTime = DateTime.TryParse(worksheet.Cells[row, 8].Text, out var enrollmentTime) ? enrollmentTime : (DateTime?)null,
                ClassStartTime = DateTime.TryParse(worksheet.Cells[row, 9].Text, out var classStart) ? classStart : (DateTime?)null,
                ClassEndTime = DateTime.TryParse(worksheet.Cells[row, 10].Text, out var classEnd) ? classEnd : (DateTime?)null,
                LastCourseActivityTime = DateTime.TryParse(worksheet.Cells[row, 11].Text, out var lastActivity) ? lastActivity : (DateTime?)null,
                OverallProgress = decimal.TryParse(worksheet.Cells[row, 12].Text, out var overallProgress) ? overallProgress : (decimal?)null,
                EstimatedLearningHours = decimal.TryParse(worksheet.Cells[row, 13].Text, out var estHours) ? estHours : (decimal?)null,
                Completed = worksheet.Cells[row, 14].Text,
                RemovedFromProgram = worksheet.Cells[row, 15].Text,
                ProgramSlug = worksheet.Cells[row, 16].Text,
                ProgramName = worksheet.Cells[row, 17].Text,
                EnrollmentSource = worksheet.Cells[row, 18].Text,
                CompletionTime = DateTime.TryParse(worksheet.Cells[row, 19].Text, out var completionTime) ? completionTime : (DateTime?)null,
                CourseGrade = worksheet.Cells[row, 20].Text,
                CourseCertificateURL = worksheet.Cells[row, 21].Text,
                ForCredit = worksheet.Cells[row, 22].Text,
                CourseType = worksheet.Cells[row, 23].Text,
                JobTitle = worksheet.Cells[row, 24].Text,
                JobType = worksheet.Cells[row, 25].Text,
                LocationCity = worksheet.Cells[row, 26].Text,
                LocationRegion = worksheet.Cells[row, 27].Text,
                LocationCountry = worksheet.Cells[row, 28].Text,
                AccountId = currentAccountId
            };
            // Build composite key without including AccountId.
            string key = string.Join("|", new string[]
            {
                record.Name?.Trim() ?? "",
                record.Email?.Trim() ?? "",
                record.ExternalId?.Trim() ?? "",
                record.Course?.Trim() ?? "",
                record.CourseId?.Trim() ?? "",
                record.CourseSlug?.Trim() ?? "",
                record.University?.Trim() ?? "",
                record.EnrollmentTime?.ToString("o") ?? "",
                record.ClassStartTime?.ToString("o") ?? "",
                record.ClassEndTime?.ToString("o") ?? "",
                record.LastCourseActivityTime?.ToString("o") ?? "",
                record.OverallProgress?.ToString() ?? "",
                record.EstimatedLearningHours?.ToString() ?? "",
                record.Completed?.Trim() ?? "",
                record.RemovedFromProgram?.Trim() ?? "",
                record.ProgramSlug?.Trim() ?? "",
                record.ProgramName?.Trim() ?? "",
                record.EnrollmentSource?.Trim() ?? "",
                record.CompletionTime?.ToString("o") ?? "",
                record.CourseGrade?.Trim() ?? "",
                record.CourseCertificateURL?.Trim() ?? "",
                record.ForCredit?.Trim() ?? "",
                record.CourseType?.Trim() ?? "",
                record.JobTitle?.Trim() ?? "",
                record.JobType?.Trim() ?? "",
                record.LocationCity?.Trim() ?? "",
                record.LocationRegion?.Trim() ?? "",
                record.LocationCountry?.Trim() ?? ""
            });
            if (!fileKeys.Add(key))
            {
                fileDuplicateCount++;
                continue;
            }
            bool exists = await _context.ExcelDataCourseraUsageReports.AnyAsync(r =>
                r.Name == record.Name &&
                r.Email == record.Email &&
                r.ExternalId == record.ExternalId &&
                r.Course == record.Course &&
                r.CourseId == record.CourseId &&
                r.CourseSlug == record.CourseSlug &&
                r.University == record.University &&
                r.EnrollmentTime == record.EnrollmentTime &&
                r.ClassStartTime == record.ClassStartTime &&
                r.ClassEndTime == record.ClassEndTime &&
                r.LastCourseActivityTime == record.LastCourseActivityTime &&
                r.OverallProgress == record.OverallProgress &&
                r.EstimatedLearningHours == record.EstimatedLearningHours &&
                r.Completed == record.Completed &&
                r.RemovedFromProgram == record.RemovedFromProgram &&
                r.ProgramSlug == record.ProgramSlug &&
                r.ProgramName == record.ProgramName &&
                r.EnrollmentSource == record.EnrollmentSource &&
                r.CompletionTime == record.CompletionTime &&
                r.CourseGrade == record.CourseGrade &&
                r.CourseCertificateURL == record.CourseCertificateURL &&
                r.ForCredit == record.ForCredit &&
                r.CourseType == record.CourseType &&
                r.JobTitle == record.JobTitle &&
                r.JobType == record.JobType &&
                r.LocationCity == record.LocationCity &&
                r.LocationRegion == record.LocationRegion &&
                r.LocationCountry == record.LocationCountry
            );
            if (exists)
                dbDuplicateCount++;
            else
                dataList.Add(record);
        }
        _context.ExcelDataCourseraUsageReports.AddRange(dataList);
        await _context.SaveChangesAsync();
        return dbDuplicateCount + fileDuplicateCount;
    }

    private async Task<int> ProcessCourseraPivotLocationCityCsvData(IFormFile file)
    {
        int dbDuplicateCount = 0, fileDuplicateCount = 0;
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            HeaderValidated = null,
            MissingFieldFound = null
        };
        using var streamReader = new StreamReader(file.OpenReadStream());
        using var csv = new CsvReader(streamReader, config);
        csv.Context.RegisterClassMap<ExcelDataCourseraPivotLocationCityReportMap>();
        var dataList = new List<ExcelDataCourseraPivotLocationCityReport>();
        var fileKeys = new HashSet<string>();
        int currentAccountId = AccountController.ActiveAccount?.Id ?? 0;
        await foreach (var record in csv.GetRecordsAsync<ExcelDataCourseraPivotLocationCityReport>())
        {
            record.AccountId = currentAccountId;
            string key = record.LocationCity?.Trim(); // Composite key excludes account id.
            if (!fileKeys.Add(key))
            {
                fileDuplicateCount++;
                continue;
            }
            bool exists = await _context.ExcelDataCourseraPivotLocationCityReports.AnyAsync(r =>
                r.LocationCity == record.LocationCity
            );
            if (!exists)
                dataList.Add(record);
            else
                dbDuplicateCount++;
        }
        _context.ExcelDataCourseraPivotLocationCityReports.AddRange(dataList);
        await _context.SaveChangesAsync();
        return dbDuplicateCount + fileDuplicateCount;
    }

    private async Task<int> ProcessCourseraPivotLocationCityXlsxData(IFormFile file)
    {
        int dbDuplicateCount = 0, fileDuplicateCount = 0;
        OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
        int currentAccountId = AccountController.ActiveAccount?.Id ?? 0;
        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        using var package = new ExcelPackage(stream);
        var worksheet = package.Workbook.Worksheets[0]; // Assume row 1 contains headers.
        int rowCount = worksheet.Dimension.Rows;
        var dataList = new List<ExcelDataCourseraPivotLocationCityReport>();
        var fileKeys = new HashSet<string>();
        for (int row = 2; row <= rowCount; row++)
        {
            var record = new ExcelDataCourseraPivotLocationCityReport
            {
                LocationCity = worksheet.Cells[row, 1].Text,
                CurrentMembers = int.TryParse(worksheet.Cells[row, 2].Text, out var currentMembers) ? currentMembers : (int?)null,
                CurrentLearners = int.TryParse(worksheet.Cells[row, 3].Text, out var currentLearners) ? currentLearners : (int?)null,
                TotalEnrollments = int.TryParse(worksheet.Cells[row, 4].Text, out var totalEnrollments) ? totalEnrollments : (int?)null,
                TotalCompletedCourses = int.TryParse(worksheet.Cells[row, 5].Text, out var totalCompleted) ? totalCompleted : (int?)null,
                AverageProgress = decimal.TryParse(worksheet.Cells[row, 6].Text, out var avgProgress) ? avgProgress : (decimal?)null,
                TotalEstimatedLearningHours = decimal.TryParse(worksheet.Cells[row, 7].Text, out var totalEstHours) ? totalEstHours : (decimal?)null,
                AverageEstimatedLearningHours = decimal.TryParse(worksheet.Cells[row, 8].Text, out var avgEstHours) ? avgEstHours : (decimal?)null,
                DeletedMembers = int.TryParse(worksheet.Cells[row, 9].Text, out var deletedMembers) ? deletedMembers : (int?)null,
                AccountId = currentAccountId
            };
            string key = record.LocationCity?.Trim(); // Duplicate key does not include account id.
            if (!fileKeys.Add(key))
            {
                fileDuplicateCount++;
                continue;
            }
            bool exists = await _context.ExcelDataCourseraPivotLocationCityReports.AnyAsync(r =>
                r.LocationCity == record.LocationCity
            );
            if (!exists)
                dataList.Add(record);
            else
                dbDuplicateCount++;
        }
        _context.ExcelDataCourseraPivotLocationCityReports.AddRange(dataList);
        await _context.SaveChangesAsync();
        return dbDuplicateCount + fileDuplicateCount;
    }

    private async Task<int> ProcessCognitoMasterListCsvData(IFormFile file)
    {
        int dbDuplicateCount = 0, fileDuplicateCount = 0;
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            HeaderValidated = null,
            MissingFieldFound = null
        };
        using var streamReader = new StreamReader(file.OpenReadStream());
        using var csv = new CsvReader(streamReader, config);
        csv.Context.RegisterClassMap<ExcelDataCognitoMasterListMap>();
        var dataList = new List<ExcelDataCognitoMasterList>();
        var fileKeys = new HashSet<string>();
        int currentAccountId = AccountController.ActiveAccount?.Id ?? 0;
        await foreach (var record in csv.GetRecordsAsync<ExcelDataCognitoMasterList>())
        {
            record.Name_Middle ??= "Not Provided";
            record.Address_Line2 ??= "N/A";
            record.AccountId = currentAccountId;
            string key = $"{record.Name_First?.Trim()}|{record.Name_Last?.Trim()}|{record.Phone?.Trim()}";
            if (!fileKeys.Add(key))
            {
                fileDuplicateCount++;
                continue;
            }
            bool exists = await _context.ExcelDataCognitoMasterList.AnyAsync(r =>
                r.Name_First == record.Name_First &&
                r.Name_Last == record.Name_Last &&
                r.Phone == record.Phone
            );
            if (!exists)
                dataList.Add(record);
            else
                dbDuplicateCount++;
        }
        _context.ExcelDataCognitoMasterList.AddRange(dataList);
        await _context.SaveChangesAsync();
        return dbDuplicateCount + fileDuplicateCount;
    }

    private async Task<int> ProcessCognitoMasterListXlsxData(IFormFile file)
    {
        int dbDuplicateCount = 0, fileDuplicateCount = 0;
        OfficeOpenXml.ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        int currentAccountId = AccountController.ActiveAccount?.Id ?? 0;
        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        using var package = new ExcelPackage(stream);
        var worksheet = package.Workbook.Worksheets[0];
        int rowCount = worksheet.Dimension.Rows;
        var dataList = new List<ExcelDataCognitoMasterList>();
        var fileKeys = new HashSet<string>();
        for (int row = 2; row <= rowCount; row++)
        {
            var record = new ExcelDataCognitoMasterList
            {
                Name_First = worksheet.Cells[row, 2].Text,
                Name_Middle = worksheet.Cells[row, 3].Text,
                Name_Last = worksheet.Cells[row, 4].Text,
                Phone = worksheet.Cells[row, 5].Text,
                Age = int.TryParse(worksheet.Cells[row, 6].Text, out var age) ? age : (int?)null,
                Address_Line1 = worksheet.Cells[row, 7].Text,
                Address_Line2 = worksheet.Cells[row, 8].Text,
                Address_City = worksheet.Cells[row, 9].Text,
                Address_State = worksheet.Cells[row, 10].Text,
                Address_PostalCode = worksheet.Cells[row, 11].Text,
                IntendedMajor = worksheet.Cells[row, 12].Text,
                ExpectedGraduation = worksheet.Cells[row, 13].Text,
                CollegePlanToAttend_Name = worksheet.Cells[row, 14].Text,
                CollegePlanToAttend_CityState = worksheet.Cells[row, 15].Text,
                HighSchoolCollegeData_CurrentStudent = worksheet.Cells[row, 16].Text,
                HighSchoolCollegeData_HighSchoolCollegeInformation = worksheet.Cells[row, 17].Text,
                HighSchoolCollegeData_Phone = worksheet.Cells[row, 18].Text,
                HighSchoolCollegeData_HighSchoolGraduation = DateTime.TryParse(worksheet.Cells[row, 19].Text, out var hsGradDate) ? hsGradDate : (DateTime?)null,
                HighSchoolCollegeData_CumulativeGPA = decimal.TryParse(worksheet.Cells[row, 20].Text, out var gpa) ? gpa : (decimal?)null,
                HighSchoolCollegeData_ACTCompositeScore = int.TryParse(worksheet.Cells[row, 21].Text, out var actScore) ? actScore : (int?)null,
                HighSchoolCollegeData_SATCompositeScore = int.TryParse(worksheet.Cells[row, 22].Text, out var satScore) ? satScore : (int?)null,
                HighSchoolCollegeData_SchoolCommunityRelatedActivities = worksheet.Cells[row, 23].Text,
                HighSchoolCollegeData_HonorsAndSpecialRecognition = worksheet.Cells[row, 24].Text,
                HighSchoolCollegeData_ExplainYourNeedForAssistance = worksheet.Cells[row, 25].Text,
                WriteYourNameAsFormOfSignature = worksheet.Cells[row, 26].Text,
                Date = DateTime.TryParse(worksheet.Cells[row, 27].Text, out var date) ? date : (DateTime?)null,
                Entry_Status = worksheet.Cells[row, 28].Text,
                Entry_DateCreated = DateTime.TryParse(worksheet.Cells[row, 29].Text, out var dateCreated) ? dateCreated : (DateTime?)null,
                Entry_DateSubmitted = DateTime.TryParse(worksheet.Cells[row, 30].Text, out var dateSubmitted) ? dateSubmitted : (DateTime?)null,
                Entry_DateUpdated = DateTime.TryParse(worksheet.Cells[row, 31].Text, out var dateUpdated) ? dateUpdated : (DateTime?)null,
                AccountId = currentAccountId
            };
            string key = $"{record.Name_First?.Trim()}|{record.Name_Last?.Trim()}|{record.Phone?.Trim()}";
            if (!fileKeys.Add(key))
            {
                fileDuplicateCount++;
                continue;
            }
            bool exists = await _context.ExcelDataCognitoMasterList.AnyAsync(r =>
                r.Name_First == record.Name_First &&
                r.Name_Last == record.Name_Last &&
                r.Phone == record.Phone
            );
            if (!exists)
                dataList.Add(record);
            else
                dbDuplicateCount++;
        }
        _context.ExcelDataCognitoMasterList.AddRange(dataList);
        await _context.SaveChangesAsync();
        return dbDuplicateCount + fileDuplicateCount;
    }

    private async Task<int> ProcessGoogleFormsVolunteerProgramCsvData(IFormFile file)
    {
        int dbDuplicateCount = 0, fileDuplicateCount = 0;
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            HeaderValidated = null,
            MissingFieldFound = null
        };

        int currentAccountId = AccountController.ActiveAccount?.Id ?? 0;

        using (var streamReader = new StreamReader(file.OpenReadStream()))
        using (var csv = new CsvReader(streamReader, config))
        {
            csv.Context.RegisterClassMap<ExcelDataGoogleFormsVolunteerProgramMap>();
            var dataList = new List<ExcelDataGoogleFormsVolunteerProgram>();
            var fileKeys = new HashSet<string>();

            await foreach (var record in csv.GetRecordsAsync<ExcelDataGoogleFormsVolunteerProgram>())
            {
                // Normalize fields and set defaults.
                record.Timestamp = DateTime.TryParse(record.Timestamp.ToString(), out var timestamp)
                    ? timestamp
                    : DateTime.MinValue;
                record.Date = DateTime.TryParse(record.Date.ToString(), out var date)
                    ? date
                    : null;
                record.MethodOfContact ??= "Unknown";
                record.Comment ??= "No comment provided";
                record.AccountId = currentAccountId;
                string key = $"{record.Timestamp.ToString("o")}|{record.Mentor?.Trim()}|{record.Mentee?.Trim()}|{(record.Date.HasValue ? record.Date.Value.ToString("yyyy-MM-dd") : "")}";
                if (!fileKeys.Add(key))
                {
                    fileDuplicateCount++;
                    continue;
                }
                bool exists = await _context.ExcelDataGoogleFormsVolunteerProgram.AnyAsync(r =>
                    r.Timestamp == record.Timestamp &&
                    r.Mentor == record.Mentor &&
                    r.Mentee == record.Mentee &&
                    r.Date == record.Date
                );
                if (!exists)
                    dataList.Add(record);
                else
                    dbDuplicateCount++;
            }
            _context.ExcelDataGoogleFormsVolunteerProgram.AddRange(dataList);
            await _context.SaveChangesAsync();
        }
        return dbDuplicateCount + fileDuplicateCount;
    }

    private async Task<int> ProcessGoogleFormsVolunteerProgramXlsxData(IFormFile file)
    {
        int dbDuplicateCount = 0, fileDuplicateCount = 0;
        OfficeOpenXml.ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        int currentAccountId = AccountController.ActiveAccount?.Id ?? 0;
        using (var stream = new MemoryStream())
        {
            await file.CopyToAsync(stream);
            using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.Rows;
                var dataList = new List<ExcelDataGoogleFormsVolunteerProgram>();
                var fileKeys = new HashSet<string>();
                for (int row = 2; row <= rowCount; row++)
                {
                    if (string.IsNullOrWhiteSpace(worksheet.Cells[row, 2].Text))
                    {
                        break;
                    }
                    var timestamp = DateTime.TryParse(worksheet.Cells[row, 1].Text, out var parsedTimestamp)
                        ? parsedTimestamp
                        : DateTime.MinValue;
                    string mentor = worksheet.Cells[row, 2].Text;
                    string mentee = worksheet.Cells[row, 3].Text;
                    var date = DateTime.TryParse(worksheet.Cells[row, 4].Text, out var parsedDate)
                        ? parsedDate
                        : (DateTime?)null;
                    string time = worksheet.Cells[row, 5].Text;
                    string methodOfContact = worksheet.Cells[row, 6].Text;
                    string comment = worksheet.Cells[row, 7].Text;
                    string key = $"{timestamp.ToString("o")}|{mentor.Trim()}|{mentee.Trim()}|{(date.HasValue ? date.Value.ToString("yyyy-MM-dd") : "")}";
                    if (!fileKeys.Add(key))
                    {
                        fileDuplicateCount++;
                        continue;
                    }
                    var record = new ExcelDataGoogleFormsVolunteerProgram
                    {
                        Timestamp = timestamp,
                        Mentor = mentor,
                        Mentee = mentee,
                        Date = date,
                        Time = time,
                        MethodOfContact = string.IsNullOrWhiteSpace(methodOfContact) ? "Unknown" : methodOfContact,
                        Comment = string.IsNullOrWhiteSpace(comment) ? "No comment provided" : comment,
                        AccountId = currentAccountId
                    };
                    bool exists = await _context.ExcelDataGoogleFormsVolunteerProgram.AnyAsync(r =>
                        r.Timestamp == record.Timestamp &&
                        r.Mentor == record.Mentor &&
                        r.Mentee == record.Mentee &&
                        r.Date == record.Date
                    );
                    if (!exists)
                        dataList.Add(record);
                    else
                        dbDuplicateCount++;
                }
                _context.ExcelDataGoogleFormsVolunteerProgram.AddRange(dataList);
                await _context.SaveChangesAsync();
            }
        }
        return dbDuplicateCount + fileDuplicateCount;
    }

    #endregion

    public IActionResult UploadSuccess()
    {
        ViewBag.DuplicateMessage = DuplicateMessage;
        return View();
    }
}