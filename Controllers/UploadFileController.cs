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
    private readonly ILogger _logger;

    public UploadFileController(ApplicationDbContext context, ILogger<UploadFileController> logger)
    {
        _context = context;
        _logger = logger;
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
                "#", "Status", "Date Submitted", "Name", "Phone", "Age", "Address", "Intended Major",
                "Expected graduation", "Name", "City, State", "Current Student?", "High School/ College Information",
                "Phone", "High School Graduation", "Cumulative GPA", "ACT Composite Score", "SAT Composite Score",
                "School/ Community- Related Activities", "Honors and Special Recognition", "Explain Your Need For Assistance",
                "Write Your Name As form of Signature", "Date"
            }
        },
        { "Cognito-Table", new List<string>
            {
                "MARIEBARNEYBOSTONSCHOLARSHIPFOU_Id", "Name_First", "Name_Middle", "Name_Last", "Phone", "Age",
                "Address_Line1", "Address_Line2", "Address_City", "Address_State", "Address_PostalCode",
                "IntendedMajor", "ExpectedGraduation", "CollegePlanToAttend_Name", "CollegePlanToAttend_CityState",
                "HighSchoolCollegeData_CurrentStudent", "HighSchoolCollegeData_HighSchoolCollegeInformation",
                "HighSchoolCollegeData_Phone", "HighSchoolCollegeData_HighSchoolGraduation",
                "HighSchoolCollegeData_CumulativeGPA", "HighSchoolCollegeData_ACTCompositeScore",
                "HighSchoolCollegeData_SATCompositeScore", "HighSchoolCollegeData_SchoolCommunityRelatedActivities",
                "HighSchoolCollegeData_HonorsAndSpecialRecognition", "HighSchoolCollegeData_ExplainYourNeedForAssistance",
                "WriteYourNameAsFormOfSignature", "Date", "Entry_Status", "Entry_DateCreated",
                "Entry_DateSubmitted", "Entry_DateUpdated"
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
        { "Coursera-pivot-location-report", new List<string>
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
        return header?.Trim().Replace(" ", "").Replace("-", "").Replace("?", "").Replace("/", "") ?? "";
    }

    private (string firstName, string middleName, string lastName) SplitName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return ("", "", "");

        var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1)
            return (parts[0], "", "");
        if (parts.Length == 2)
            return (parts[0], "", parts[1]);
        return (parts[0], string.Join(" ", parts.Skip(1).Take(parts.Length - 2)), parts[^1]);
    }

    private async Task<(bool isValid, string headerKey)> ValidateFileHeaders(IFormFile file, string source, string fileExtension, string fileType = null)
    {
        string headerKey = source;
        if (source == "Coursera")
        {
            if (fileType == "membership-report")
                headerKey = "Coursera-membership-report";
            else if (fileType == "pivot-location-city-report" || fileType == "pivot-location-country-report" || fileType == "location-country-report")
                headerKey = "Coursera-pivot-location-report"; // Merged both city and country reports
            else if (fileType == "usage-report")
                headerKey = "Coursera-usage-report";
        }

        List<string> rawFileHeaders = null;
        List<string> fileHeaders = null;

        if (fileExtension == ".csv")
        {
            using var streamReader = new StreamReader(file.OpenReadStream());
            using var csv = new CsvReader(streamReader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                TrimOptions = TrimOptions.Trim
            });
            await csv.ReadAsync();
            csv.ReadHeader();
            rawFileHeaders = csv.HeaderRecord?.Select(h => h?.Trim() ?? "").ToList();
            _logger.LogInformation("CSV headers with quotes preserved: {Headers}", string.Join(", ", rawFileHeaders));
            rawFileHeaders = rawFileHeaders?.Select(h => h.Trim('"')).ToList();
            fileHeaders = rawFileHeaders?.Select(NormalizeHeader).ToList();
            while (fileHeaders != null && fileHeaders.Count > 0 && string.IsNullOrEmpty(fileHeaders.Last()))
            {
                fileHeaders.RemoveAt(fileHeaders.Count - 1);
                rawFileHeaders.RemoveAt(rawFileHeaders.Count - 1);
            }
        }
        else if (fileExtension == ".xlsx")
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets[0];
            int colCount = worksheet.Dimension?.Columns ?? 0;
            rawFileHeaders = new List<string>();
            fileHeaders = new List<string>();
            for (int col = 1; col <= colCount; col++)
            {
                var rawHeader = worksheet.Cells[1, col].Text?.Trim() ?? "";
                rawFileHeaders.Add(rawHeader);
                fileHeaders.Add(NormalizeHeader(rawHeader));
            }
            while (fileHeaders.Count > 0 && string.IsNullOrEmpty(fileHeaders.Last()))
            {
                fileHeaders.RemoveAt(fileHeaders.Count - 1);
                rawFileHeaders.RemoveAt(rawFileHeaders.Count - 1);
            }
        }

        if (fileHeaders == null || !fileHeaders.Any())
        {
            _logger.LogError("File headers could not be read for {source} with fileType {fileType}.", source, fileType);
            return (false, headerKey);
        }
        _logger.LogInformation("Raw file headers (after quote stripping): {RawHeaders}", string.Join(", ", rawFileHeaders));
        _logger.LogInformation("Normalized file headers: {Headers}", string.Join(", ", fileHeaders));

        _logger.LogInformation("Validating headers for source: {Source}, fileType: {FileType}, headers: {Headers}", source, fileType, string.Join(", ", fileHeaders));

        if (headerKey == "Coursera-pivot-location-report")
        {
            var possibleFirstColumns = new[] { "LocationCity", "LocationCountry" };
            var fileFirstColumn = fileHeaders.FirstOrDefault();

            if (!possibleFirstColumns.Contains(fileFirstColumn, StringComparer.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Header validation failed for Coursera pivot-location-report. Expected first column to be LocationCity or LocationCountry, Actual: {ActualFirstColumn}",
                    fileFirstColumn);
                return (false, headerKey);
            }

            if (ExpectedHeaders[headerKey].Select(NormalizeHeader).ToList().Skip(1).All(h => fileHeaders.Skip(1).Contains(h, StringComparer.OrdinalIgnoreCase)))
            {
                _logger.LogInformation("Header validation passed for Coursera-pivot-location-report.");
                return (true, headerKey);
            }
            _logger.LogWarning("Header validation failed for Coursera pivot-location-report. Expected: {Expected}, Actual: {Actual}",
                string.Join(", ", ExpectedHeaders[headerKey].Select(NormalizeHeader).ToList()), string.Join(", ", fileHeaders));
            return (false, headerKey);
        }

        if (source == "Cognito")
        {
            var requiredHeaders = new List<string> { "#", "Phone" }.Select(NormalizeHeader).ToList();
            var tableHeaders = ExpectedHeaders["Cognito-Table"].Select(NormalizeHeader).ToList();
            var formHeaders = ExpectedHeaders["Cognito"].Select(NormalizeHeader).ToList();

            if (fileHeaders.SequenceEqual(tableHeaders, StringComparer.OrdinalIgnoreCase))
            {
                headerKey = "Cognito-Table";
                return (true, headerKey);
            }
            else if (requiredHeaders.All(h => fileHeaders.Contains(h, StringComparer.OrdinalIgnoreCase)))
            {
                headerKey = "Cognito";
                return (true, headerKey);
            }
            _logger.LogWarning("Cognito header validation failed. Expected Table: {TableHeaders}, Form: {FormHeaders}, Actual: {Actual}",
                string.Join(", ", tableHeaders), string.Join(", ", formHeaders), string.Join(", ", fileHeaders));
            return (false, headerKey);
        }

        if (!ExpectedHeaders.ContainsKey(headerKey))
        {
            _logger.LogError("Invalid header key: {HeaderKey}", headerKey);
            return (false, headerKey);
        }

        var expectedHeaders = ExpectedHeaders[headerKey].Select(NormalizeHeader).ToList();
        var isSubset = expectedHeaders.All(h => fileHeaders.Contains(h, StringComparer.OrdinalIgnoreCase));
        if (!isSubset)
        {
            _logger.LogWarning("Header mismatch for {HeaderKey}. Expected: {Expected}, Actual: {Actual}",
                headerKey, string.Join(", ", expectedHeaders), string.Join(", ", fileHeaders));
            return (false, headerKey);
        }
        return (true, headerKey);
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

        var fileExtension = Path.GetExtension(model.File.FileName).ToLower();
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
            var (isValid, headerKey) = await ValidateFileHeaders(model.File, model.Source, fileExtension, model.FileType);
            if (!isValid)
            {
                ModelState.AddModelError("", "The uploaded file does not match the expected format for the selected data source.");
                model.Source = "";
                model.FileType = "";
                ModelState.Remove("Source");
                ModelState.Remove("FileType");
                return View(model);
            }

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
                                case "pivot-location-country-report":
                                    duplicateCount = await ProcessCourseraPivotLocationCityCsvData(model.File); // Use the same method for both
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
                                case "pivot-location-country-report":
                                    duplicateCount = await ProcessCourseraPivotLocationCityXlsxData(model.File); // Use the same method for both
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
                            duplicateCount = await ProcessCognitoMasterListCsvData(model.File, headerKey);
                            break;
                        case ".xlsx":
                            duplicateCount = await ProcessCognitoMasterListXlsxData(model.File, headerKey);
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
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error occurred while processing file upload. Inner exception: {InnerException}", ex.InnerException?.Message);
            ModelState.AddModelError("", $"Database error: {ex.InnerException?.Message ?? ex.Message}");
            model.Source = "";
            model.FileType = "";
            ModelState.Remove("Source");
            ModelState.Remove("FileType");
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing file upload.");
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
            record.EnrollmentSource ??= "Unknown";
            record.ProgramName ??= "Not Specified";
            record.LocationRegion ??= "Not Specified";
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
        if (dataList.Any())
        {
            await _context.Database.BeginTransactionAsync();
            try
            {
                _context.ExcelDataCourseraSpecialization.AddRange(dataList);
                await _context.SaveChangesAsync();
                await _context.Database.CommitTransactionAsync();
            }
            catch
            {
                await _context.Database.RollbackTransactionAsync();
                throw;
            }
        }
        return dbDuplicateCount + fileDuplicateCount;
    }

    private async Task<int> ProcessCourseraSpecializationXlsxData(IFormFile file)
    {
        int dbDuplicateCount = 0, fileDuplicateCount = 0;
        OfficeOpenXml.ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        int currentAccountId = AccountController.ActiveAccount?.Id ?? 0;
        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        using var package = new ExcelPackage(stream);
        var worksheet = package.Workbook.Worksheets[0];
        int rowCount = worksheet.Dimension?.Rows ?? 0;
        var dataList = new List<ExcelDataCourseraSpecialization>();
        var fileKeys = new HashSet<string>();
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
                ProgramName = worksheet.Cells[row, 14].Text ?? "Not Specified",
                EnrollmentSource = worksheet.Cells[row, 15].Text ?? "Unknown",
                SpecializationCompletionTime = DateTime.TryParse(worksheet.Cells[row, 16].Text, out var specCompletion)
                                  ? specCompletion : (DateTime?)null,
                SpecializationCertificateURL = worksheet.Cells[row, 17].Text,
                JobTitle = worksheet.Cells[row, 18].Text,
                JobType = worksheet.Cells[row, 19].Text,
                LocationCity = worksheet.Cells[row, 20].Text,
                LocationRegion = worksheet.Cells[row, 21].Text ?? "Not Specified",
                LocationCountry = worksheet.Cells[row, 22].Text,
            };
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
        if (dataList.Any())
        {
            await _context.Database.BeginTransactionAsync();
            try
            {
                _context.ExcelDataCourseraSpecialization.AddRange(dataList);
                await _context.SaveChangesAsync();
                await _context.Database.CommitTransactionAsync();
            }
            catch
            {
                await _context.Database.RollbackTransactionAsync();
                throw;
            }
        }
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
        if (dataList.Any())
        {
            await _context.Database.BeginTransactionAsync();
            try
            {
                _context.ExcelDataCourseraMembershipReports.AddRange(dataList);
                await _context.SaveChangesAsync();
                await _context.Database.CommitTransactionAsync();
            }
            catch
            {
                await _context.Database.RollbackTransactionAsync();
                throw;
            }
        }
        return dbDuplicateCount + fileDuplicateCount;
    }

    private async Task<int> ProcessCourseraMembershipXlsxData(IFormFile file)
    {
        int dbDuplicateCount = 0, fileDuplicateCount = 0;
        OfficeOpenXml.ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        int currentAccountId = AccountController.ActiveAccount?.Id ?? 0;
        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        using var package = new ExcelPackage(stream);
        var worksheet = package.Workbook.Worksheets[0];
        int rowCount = worksheet.Dimension?.Rows ?? 0;
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
        if (dataList.Any())
        {
            await _context.Database.BeginTransactionAsync();
            try
            {
                _context.ExcelDataCourseraMembershipReports.AddRange(dataList);
                await _context.SaveChangesAsync();
                await _context.Database.CommitTransactionAsync();
            }
            catch
            {
                await _context.Database.RollbackTransactionAsync();
                throw;
            }
        }
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
            string key = $"{record.Email?.Trim()}|{record.ExternalId?.Trim()}|{record.CourseId?.Trim()}";
            if (!fileKeys.Add(key))
            {
                fileDuplicateCount++;
                continue;
            }
            bool exists = await _context.ExcelDataCourseraUsageReports.AnyAsync(r =>
                r.Email == record.Email &&
                r.ExternalId == record.ExternalId &&
                r.CourseId == record.CourseId
            );
            if (exists)
                dbDuplicateCount++;
            else
                dataList.Add(record);
        }
        if (dataList.Any())
        {
            await _context.Database.BeginTransactionAsync();
            try
            {
                _context.ExcelDataCourseraUsageReports.AddRange(dataList);
                await _context.SaveChangesAsync();
                await _context.Database.CommitTransactionAsync();
            }
            catch
            {
                await _context.Database.RollbackTransactionAsync();
                throw;
            }
        }
        return dbDuplicateCount + fileDuplicateCount;
    }

    private async Task<int> ProcessCourseraUsageXlsxData(IFormFile file)
    {
        int dbDuplicateCount = 0, fileDuplicateCount = 0;
        OfficeOpenXml.ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        int currentAccountId = AccountController.ActiveAccount?.Id ?? 0;
        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        using var package = new ExcelPackage(stream);
        var worksheet = package.Workbook.Worksheets[0];
        int rowCount = worksheet.Dimension?.Rows ?? 0;
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
            };
            string key = $"{record.Email?.Trim()}|{record.ExternalId?.Trim()}|{record.CourseId?.Trim()}";
            if (!fileKeys.Add(key))
            {
                fileDuplicateCount++;
                continue;
            }
            bool exists = await _context.ExcelDataCourseraUsageReports.AnyAsync(r =>
                r.Email == record.Email &&
                r.ExternalId == record.ExternalId &&
                r.CourseId == record.CourseId
            );
            if (exists)
                dbDuplicateCount++;
            else
                dataList.Add(record);
        }
        if (dataList.Any())
        {
            await _context.Database.BeginTransactionAsync();
            try
            {
                _context.ExcelDataCourseraUsageReports.AddRange(dataList);
                await _context.SaveChangesAsync();
                await _context.Database.CommitTransactionAsync();
            }
            catch
            {
                await _context.Database.RollbackTransactionAsync();
                throw;
            }
        }
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
            string key = record.LocationCity?.Trim();
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
        if (dataList.Any())
        {
            await _context.Database.BeginTransactionAsync();
            try
            {
                _context.ExcelDataCourseraPivotLocationCityReports.AddRange(dataList);
                await _context.SaveChangesAsync();
                await _context.Database.CommitTransactionAsync();
            }
            catch
            {
                await _context.Database.RollbackTransactionAsync();
                throw;
            }
        }
        return dbDuplicateCount + fileDuplicateCount;
    }

    private async Task<int> ProcessCourseraPivotLocationCityXlsxData(IFormFile file)
    {
        int dbDuplicateCount = 0, fileDuplicateCount = 0;
        OfficeOpenXml.ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        int currentAccountId = AccountController.ActiveAccount?.Id ?? 0;
        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        using var package = new ExcelPackage(stream);
        var worksheet = package.Workbook.Worksheets[0];
        int rowCount = worksheet.Dimension?.Rows ?? 0;
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
            };
            string key = record.LocationCity?.Trim();
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
        if (dataList.Any())
        {
            await _context.Database.BeginTransactionAsync();
            try
            {
                _context.ExcelDataCourseraPivotLocationCityReports.AddRange(dataList);
                await _context.SaveChangesAsync();
                await _context.Database.CommitTransactionAsync();
            }
            catch
            {
                await _context.Database.RollbackTransactionAsync();
                throw;
            }
        }
        return dbDuplicateCount + fileDuplicateCount;
    }

    private async Task<int> ProcessCognitoMasterListCsvData(IFormFile file, string headerKey)
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
        var dataList = new List<ExcelDataCognitoMasterList>();
        var fileKeys = new HashSet<string>();
        int currentAccountId = AccountController.ActiveAccount?.Id ?? 0;

        await csv.ReadAsync();
        csv.ReadHeader();
        int row = 1;

        if (headerKey == "Cognito-Table")
        {
            while (await csv.ReadAsync())
            {
                row++;
                var idText = csv.GetField("MARIEBARNEYBOSTONSCHOLARSHIPFOU_Id") ?? "";
                int idValue;
                if (!int.TryParse(idText, out idValue))
                {
                    idValue = !string.IsNullOrWhiteSpace(idText) ? idText.GetHashCode() : row;
                }

                var record = new ExcelDataCognitoMasterList
                {
                    MARIEBARNEYBOSTONSCHOLARSHIPFOU_Id = idValue,
                    AccountId = currentAccountId,
                    Name_First = csv.GetField("Name_First") ?? "",
                    Name_Middle = string.IsNullOrWhiteSpace(csv.GetField("Name_Middle")) ? "Not Provided" : csv.GetField("Name_Middle"),
                    Name_Last = csv.GetField("Name_Last") ?? "",
                    Phone = csv.GetField("Phone")?.Trim() ?? "",
                    Age = int.TryParse(csv.GetField("Age"), out var age) ? age : (int?)null,
                    Address_Line1 = csv.GetField("Address_Line1") ?? "",
                    Address_Line2 = string.IsNullOrWhiteSpace(csv.GetField("Address_Line2")) ? "N/A" : csv.GetField("Address_Line2"),
                    Address_City = csv.GetField("Address_City") ?? "",
                    Address_State = csv.GetField("Address_State") ?? "",
                    Address_PostalCode = csv.GetField("Address_PostalCode") ?? "",
                    IntendedMajor = csv.GetField("IntendedMajor") ?? "",
                    ExpectedGraduation = string.IsNullOrWhiteSpace(csv.GetField("ExpectedGraduation")) ? "Unknown" : csv.GetField("ExpectedGraduation"),
                    CollegePlanToAttend_Name = csv.GetField("CollegePlanToAttend_Name") ?? "",
                    CollegePlanToAttend_CityState = csv.GetField("CollegePlanToAttend_CityState") ?? "",
                    HighSchoolCollegeData_CurrentStudent = csv.GetField("HighSchoolCollegeData_CurrentStudent") ?? "",
                    HighSchoolCollegeData_HighSchoolCollegeInformation = csv.GetField("HighSchoolCollegeData_HighSchoolCollegeInformation") ?? "",
                    HighSchoolCollegeData_Phone = csv.GetField("HighSchoolCollegeData_Phone") ?? "",
                    HighSchoolCollegeData_HighSchoolGraduation = DateTime.TryParse(csv.GetField("HighSchoolCollegeData_HighSchoolGraduation"), out var hsGradDate) ? hsGradDate : (DateTime?)null,
                    HighSchoolCollegeData_CumulativeGPA = decimal.TryParse(csv.GetField("HighSchoolCollegeData_CumulativeGPA"), out var gpa) ? gpa : (decimal?)null,
                    HighSchoolCollegeData_ACTCompositeScore = int.TryParse(csv.GetField("HighSchoolCollegeData_ACTCompositeScore"), out var actScore) ? actScore : (int?)null,
                    HighSchoolCollegeData_SATCompositeScore = int.TryParse(csv.GetField("HighSchoolCollegeData_SATCompositeScore"), out var satScore) ? satScore : (int?)null,
                    HighSchoolCollegeData_SchoolCommunityRelatedActivities = csv.GetField("HighSchoolCollegeData_SchoolCommunityRelatedActivities") ?? "",
                    HighSchoolCollegeData_HonorsAndSpecialRecognition = csv.GetField("HighSchoolCollegeData_HonorsAndSpecialRecognition") ?? "",
                    HighSchoolCollegeData_ExplainYourNeedForAssistance = csv.GetField("HighSchoolCollegeData_ExplainYourNeedForAssistance") ?? "",
                    WriteYourNameAsFormOfSignature = csv.GetField("WriteYourNameAsFormOfSignature") ?? "",
                    Date = DateTime.TryParse(csv.GetField("Date"), out var date) ? date : (DateTime?)null,
                    Entry_Status = csv.GetField("Entry_Status") ?? "",
                    Entry_DateCreated = DateTime.TryParse(csv.GetField("Entry_DateCreated"), out var dateCreated) ? dateCreated : DateTime.Now,
                    Entry_DateSubmitted = DateTime.TryParse(csv.GetField("Entry_DateSubmitted"), out var dateSubmitted) ? dateSubmitted : (DateTime?)null,
                    Entry_DateUpdated = DateTime.TryParse(csv.GetField("Entry_DateUpdated"), out var dateUpdated) ? dateUpdated : DateTime.Now
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
        }
        else
        {
            while (await csv.ReadAsync())
            {
                row++;
                var idText = csv.GetField("#") ?? csv.GetField("MARIEBARNEYBOSTONSCHOLARSHIPFOU_Id") ?? "";
                int idValue;
                if (!int.TryParse(idText, out idValue))
                {
                    idValue = !string.IsNullOrWhiteSpace(idText) ? idText.GetHashCode() : row;
                }

                var addressText = csv.GetField("Address") ?? "";
                string addressLine1 = "", addressLine2 = "N/A", addressCity = "", addressState = "", addressPostalCode = "";
                if (!string.IsNullOrWhiteSpace(addressText))
                {
                    var addressParts = addressText.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    if (addressParts.Length > 0) addressLine1 = addressParts[0].Trim();
                    if (addressParts.Length > 1) addressLine2 = addressParts[1].Trim();
                    if (addressParts.Length > 2) addressCity = addressParts[2].Trim();
                    if (addressParts.Length > 3)
                    {
                        var cityStateZip = addressParts[3].Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        if (cityStateZip.Length > 0) addressState = cityStateZip[0].Trim();
                        if (cityStateZip.Length > 1) addressPostalCode = cityStateZip[1].Trim();
                    }
                }

                var record = new ExcelDataCognitoMasterList
                {
                    MARIEBARNEYBOSTONSCHOLARSHIPFOU_Id = idValue,
                    AccountId = currentAccountId,
                    Name_First = "",
                    Name_Middle = "Not Provided",
                    Name_Last = "",
                    Phone = csv.GetField("Phone")?.Trim() ?? "",
                    Age = int.TryParse(csv.GetField("Age"), out var age) ? age : (int?)null,
                    Address_Line1 = addressLine1,
                    Address_Line2 = addressLine2,
                    Address_City = addressCity,
                    Address_State = addressState,
                    Address_PostalCode = addressPostalCode,
                    IntendedMajor = csv.GetField("Intended Major") ?? "",
                    ExpectedGraduation = string.IsNullOrWhiteSpace(csv.GetField("Expected graduation")) ? "Unknown" : csv.GetField("Expected graduation"),
                    CollegePlanToAttend_Name = csv.GetField("Name", csv.HeaderRecord?.ToList().IndexOf("Expected graduation") + 1 ?? -1) ?? "",
                    CollegePlanToAttend_CityState = csv.GetField("City, State") ?? "",
                    HighSchoolCollegeData_CurrentStudent = csv.GetField("Current Student?") ?? "",
                    HighSchoolCollegeData_HighSchoolCollegeInformation = csv.GetField("High School/ College Information") ?? "",
                    HighSchoolCollegeData_Phone = csv.GetField("Phone", csv.HeaderRecord?.ToList().IndexOf("High School/ College Information") + 1 ?? -1) ?? "",
                    HighSchoolCollegeData_HighSchoolGraduation = DateTime.TryParse(csv.GetField("High School Graduation"), out var hsGradDate) ? hsGradDate : (DateTime?)null,
                    HighSchoolCollegeData_CumulativeGPA = decimal.TryParse(csv.GetField("Cumulative GPA"), out var gpa) ? gpa : (decimal?)null,
                    HighSchoolCollegeData_ACTCompositeScore = int.TryParse(csv.GetField("ACT Composite Score"), out var actScore) ? actScore : (int?)null,
                    HighSchoolCollegeData_SATCompositeScore = int.TryParse(csv.GetField("SAT Composite Score"), out var satScore) ? satScore : (int?)null,
                    HighSchoolCollegeData_SchoolCommunityRelatedActivities = csv.GetField("School/ Community- Related Activities") ?? "",
                    HighSchoolCollegeData_HonorsAndSpecialRecognition = csv.GetField("Honors and Special Recognition") ?? "",
                    HighSchoolCollegeData_ExplainYourNeedForAssistance = csv.GetField("Explain Your Need For Assistance") ?? "",
                    WriteYourNameAsFormOfSignature = csv.GetField("Write Your Name As form of Signature") ?? "",
                    Date = DateTime.TryParse(csv.GetField("Date"), out var date) ? date : (DateTime?)null,
                    Entry_Status = csv.GetField("Status") ?? "",
                    Entry_DateSubmitted = DateTime.TryParse(csv.GetField("Date Submitted"), out var dateSubmitted) ? dateSubmitted : (DateTime?)null,
                    Entry_DateCreated = DateTime.Now,
                    Entry_DateUpdated = DateTime.Now
                };

                var nameField = csv.GetField("Name");
                if (!string.IsNullOrWhiteSpace(nameField))
                {
                    var (firstName, middleName, lastName) = SplitName(nameField);
                    record.Name_First = firstName;
                    record.Name_Middle = middleName ?? "Not Provided";
                    record.Name_Last = lastName;
                }

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
        }

        if (dataList.Any())
        {
            await _context.Database.BeginTransactionAsync();
            try
            {
                _context.ExcelDataCognitoMasterList.AddRange(dataList);
                await _context.SaveChangesAsync();
                await _context.Database.CommitTransactionAsync();
            }
            catch
            {
                await _context.Database.RollbackTransactionAsync();
                throw;
            }
        }
        return dbDuplicateCount + fileDuplicateCount;
    }

    private async Task<int> ProcessCognitoMasterListXlsxData(IFormFile file, string headerKey)
    {
        int dbDuplicateCount = 0, fileDuplicateCount = 0;
        OfficeOpenXml.ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        int currentAccountId = AccountController.ActiveAccount?.Id ?? 0;
        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        using var package = new ExcelPackage(stream);
        var worksheet = package.Workbook.Worksheets[0];
        int rowCount = worksheet.Dimension?.Rows ?? 0;
        if (rowCount < 2)
            return 0;

        var dataList = new List<ExcelDataCognitoMasterList>();
        var fileKeys = new HashSet<string>();

        if (headerKey == "Cognito-Table")
        {
            for (int row = 2; row <= rowCount; row++)
            {
                var idText = worksheet.Cells[row, 1].Text;
                int idValue;
                if (!int.TryParse(idText, out idValue))
                {
                    idValue = !string.IsNullOrWhiteSpace(idText) ? idText.GetHashCode() : row;
                }

                var record = new ExcelDataCognitoMasterList
                {
                    MARIEBARNEYBOSTONSCHOLARSHIPFOU_Id = idValue,
                    AccountId = currentAccountId,
                    Name_First = worksheet.Cells[row, 2].Text ?? "",
                    Name_Middle = string.IsNullOrWhiteSpace(worksheet.Cells[row, 3].Text) ? "Not Provided" : worksheet.Cells[row, 3].Text,
                    Name_Last = worksheet.Cells[row, 4].Text ?? "",
                    Phone = worksheet.Cells[row, 5].Text?.Trim() ?? "",
                    Age = int.TryParse(worksheet.Cells[row, 6].Text, out var age) ? age : (int?)null,
                    Address_Line1 = worksheet.Cells[row, 7].Text ?? "",
                    Address_Line2 = string.IsNullOrWhiteSpace(worksheet.Cells[row, 8].Text) ? "N/A" : worksheet.Cells[row, 8].Text,
                    Address_City = worksheet.Cells[row, 9].Text ?? "",
                    Address_State = worksheet.Cells[row, 10].Text ?? "",
                    Address_PostalCode = worksheet.Cells[row, 11].Text ?? "",
                    IntendedMajor = worksheet.Cells[row, 12].Text ?? "",
                    ExpectedGraduation = string.IsNullOrWhiteSpace(worksheet.Cells[row, 13].Text) ? "Unknown" : worksheet.Cells[row, 13].Text,
                    CollegePlanToAttend_Name = worksheet.Cells[row, 14].Text ?? "",
                    CollegePlanToAttend_CityState = worksheet.Cells[row, 15].Text ?? "",
                    HighSchoolCollegeData_CurrentStudent = worksheet.Cells[row, 16].Text ?? "",
                    HighSchoolCollegeData_HighSchoolCollegeInformation = worksheet.Cells[row, 17].Text ?? "",
                    HighSchoolCollegeData_Phone = worksheet.Cells[row, 18].Text ?? "",
                    HighSchoolCollegeData_HighSchoolGraduation = DateTime.TryParse(worksheet.Cells[row, 19].Text, out var hsGradDate) ? hsGradDate : (DateTime?)null,
                    HighSchoolCollegeData_CumulativeGPA = decimal.TryParse(worksheet.Cells[row, 20].Text, out var gpa) ? gpa : (decimal?)null,
                    HighSchoolCollegeData_ACTCompositeScore = int.TryParse(worksheet.Cells[row, 21].Text, out var actScore) ? actScore : (int?)null,
                    HighSchoolCollegeData_SATCompositeScore = int.TryParse(worksheet.Cells[row, 22].Text, out var satScore) ? satScore : (int?)null,
                    HighSchoolCollegeData_SchoolCommunityRelatedActivities = worksheet.Cells[row, 23].Text ?? "",
                    HighSchoolCollegeData_HonorsAndSpecialRecognition = worksheet.Cells[row, 24].Text ?? "",
                    HighSchoolCollegeData_ExplainYourNeedForAssistance = worksheet.Cells[row, 25].Text ?? "",
                    WriteYourNameAsFormOfSignature = worksheet.Cells[row, 26].Text ?? "",
                    Date = DateTime.TryParse(worksheet.Cells[row, 27].Text, out var date) ? date : (DateTime?)null,
                    Entry_Status = worksheet.Cells[row, 28].Text ?? "",
                    Entry_DateCreated = DateTime.TryParse(worksheet.Cells[row, 29].Text, out var dateCreated) ? dateCreated : DateTime.Now,
                    Entry_DateSubmitted = DateTime.TryParse(worksheet.Cells[row, 30].Text, out var dateSubmitted) ? dateSubmitted : (DateTime?)null,
                    Entry_DateUpdated = DateTime.TryParse(worksheet.Cells[row, 31].Text, out var dateUpdated) ? dateUpdated : DateTime.Now
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
        }
        else
        {
            var headers = new List<string>();
            for (int col = 1; col <= (worksheet.Dimension?.Columns ?? 0); col++)
            {
                headers.Add(NormalizeHeader(worksheet.Cells[1, col].Text));
            }

            int idColumnIndex = headers.FindIndex(h => h == "#" || h == "MARIEBARNEYBOSTONSCHOLARSHIPFOU_Id") + 1;
            int statusColumnIndex = headers.FindIndex(h => h == "Status") + 1;
            int dateSubmittedColumnIndex = headers.FindIndex(h => h == "DateSubmitted") + 1;
            int nameColumnIndex = headers.FindIndex(h => h == "Name") + 1;
            int phoneColumnIndex = headers.FindIndex(h => h == "Phone" && headers.Take(h == "Phone" ? headers.IndexOf(h) : headers.Count).All(th => th != "HighSchoolCollegeInformation")) + 1;
            int ageColumnIndex = headers.FindIndex(h => h == "Age") + 1;
            int addressColumnIndex = headers.FindIndex(h => h == "Address") + 1;
            int intendedMajorColumnIndex = headers.FindIndex(h => h == "IntendedMajor") + 1;
            int expectedGraduationColumnIndex = headers.FindIndex(h => h == "Expectedgraduation") + 1;
            int collegeNameColumnIndex = headers.FindIndex(h => h == "Name" && headers.Take(h == "Name" ? headers.IndexOf(h) : headers.Count).Any(th => th == "Expectedgraduation")) + 1;
            int collegeCityStateColumnIndex = headers.FindIndex(h => h == "CityState") + 1;
            int currentStudentColumnIndex = headers.FindIndex(h => h == "CurrentStudent") + 1;
            int hsCollegeInfoColumnIndex = headers.FindIndex(h => h == "HighSchoolCollegeInformation") + 1;
            int hsPhoneColumnIndex = headers.FindIndex(h => h == "Phone" && headers.Take(h == "Phone" ? headers.IndexOf(h) : headers.Count).Any(th => th == "HighSchoolCollegeInformation")) + 1;
            int hsGraduationColumnIndex = headers.FindIndex(h => h == "HighSchoolGraduation") + 1;
            int gpaColumnIndex = headers.FindIndex(h => h == "CumulativeGPA") + 1;
            int actColumnIndex = headers.FindIndex(h => h == "ACTCompositeScore") + 1;
            int satColumnIndex = headers.FindIndex(h => h == "SATCompositeScore") + 1;
            int activitiesColumnIndex = headers.FindIndex(h => h == "SchoolCommunityRelatedActivities") + 1;
            int honorsColumnIndex = headers.FindIndex(h => h == "HonorsandSpecialRecognition") + 1;
            int needAssistanceColumnIndex = headers.FindIndex(h => h == "ExplainYourNeedForAssistance") + 1;
            int signatureColumnIndex = headers.FindIndex(h => h == "WriteYourNameAsformofSignature") + 1;
            int dateColumnIndex = headers.FindIndex(h => h == "Date") + 1;

            if (idColumnIndex <= 0 || phoneColumnIndex <= 0)
                throw new InvalidOperationException("Required columns '#' and 'Phone' are missing.");

            for (int row = 2; row <= rowCount; row++)
            {
                if (string.IsNullOrWhiteSpace(worksheet.Cells[row, idColumnIndex].Text) &&
                    string.IsNullOrWhiteSpace(worksheet.Cells[row, phoneColumnIndex].Text))
                    continue;

                var idText = idColumnIndex > 0 ? worksheet.Cells[row, idColumnIndex].Text : "";
                int idValue;
                if (!int.TryParse(idText, out idValue))
                {
                    idValue = !string.IsNullOrWhiteSpace(idText) ? idText.GetHashCode() : row;
                }

                string addressLine1 = "", addressLine2 = "N/A", addressCity = "", addressState = "", addressPostalCode = "";
                if (addressColumnIndex > 0 && !string.IsNullOrWhiteSpace(worksheet.Cells[row, addressColumnIndex].Text))
                {
                    var addressParts = worksheet.Cells[row, addressColumnIndex].Text.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    if (addressParts.Length > 0) addressLine1 = addressParts[0].Trim();
                    if (addressParts.Length > 1) addressLine2 = addressParts[1].Trim();
                    if (addressParts.Length > 2) addressCity = addressParts[2].Trim();
                    if (addressParts.Length > 3)
                    {
                        var cityStateZip = addressParts[3].Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        if (cityStateZip.Length > 0) addressState = cityStateZip[0].Trim();
                        if (cityStateZip.Length > 1) addressPostalCode = cityStateZip[1].Trim();
                    }
                }

                var record = new ExcelDataCognitoMasterList
                {
                    MARIEBARNEYBOSTONSCHOLARSHIPFOU_Id = idValue,
                    AccountId = currentAccountId,
                    Name_First = "",
                    Name_Middle = "Not Provided",
                    Name_Last = "",
                    Phone = phoneColumnIndex > 0 ? worksheet.Cells[row, phoneColumnIndex].Text?.Trim() : "",
                    Age = ageColumnIndex > 0 && int.TryParse(worksheet.Cells[row, ageColumnIndex].Text, out var age) ? age : (int?)null,
                    Address_Line1 = addressLine1,
                    Address_Line2 = addressLine2,
                    Address_City = addressCity,
                    Address_State = addressState,
                    Address_PostalCode = addressPostalCode,
                    IntendedMajor = intendedMajorColumnIndex > 0 ? worksheet.Cells[row, intendedMajorColumnIndex].Text : "",
                    ExpectedGraduation = expectedGraduationColumnIndex > 0 && !string.IsNullOrWhiteSpace(worksheet.Cells[row, expectedGraduationColumnIndex].Text)
                        ? worksheet.Cells[row, expectedGraduationColumnIndex].Text
                        : "Unknown",
                    CollegePlanToAttend_Name = collegeNameColumnIndex > 0 ? worksheet.Cells[row, collegeNameColumnIndex].Text : "",
                    CollegePlanToAttend_CityState = collegeCityStateColumnIndex > 0 ? worksheet.Cells[row, collegeCityStateColumnIndex].Text : "",
                    HighSchoolCollegeData_CurrentStudent = currentStudentColumnIndex > 0 ? worksheet.Cells[row, currentStudentColumnIndex].Text : "",
                    HighSchoolCollegeData_HighSchoolCollegeInformation = hsCollegeInfoColumnIndex > 0 ? worksheet.Cells[row, hsCollegeInfoColumnIndex].Text : "",
                    HighSchoolCollegeData_Phone = hsPhoneColumnIndex > 0 ? worksheet.Cells[row, hsPhoneColumnIndex].Text : "",
                    HighSchoolCollegeData_HighSchoolGraduation = hsGraduationColumnIndex > 0 && DateTime.TryParse(worksheet.Cells[row, hsGraduationColumnIndex].Text, out var hsGradDate) ? hsGradDate : (DateTime?)null,
                    HighSchoolCollegeData_CumulativeGPA = gpaColumnIndex > 0 && decimal.TryParse(worksheet.Cells[row, gpaColumnIndex].Text, out var gpa) ? gpa : (decimal?)null,
                    HighSchoolCollegeData_ACTCompositeScore = actColumnIndex > 0 && int.TryParse(worksheet.Cells[row, actColumnIndex].Text, out var actScore) ? actScore : (int?)null,
                    HighSchoolCollegeData_SATCompositeScore = satColumnIndex > 0 && int.TryParse(worksheet.Cells[row, satColumnIndex].Text, out var satScore) ? satScore : (int?)null,
                    HighSchoolCollegeData_SchoolCommunityRelatedActivities = activitiesColumnIndex > 0 ? worksheet.Cells[row, activitiesColumnIndex].Text : "",
                    HighSchoolCollegeData_HonorsAndSpecialRecognition = honorsColumnIndex > 0 ? worksheet.Cells[row, honorsColumnIndex].Text : "",
                    HighSchoolCollegeData_ExplainYourNeedForAssistance = needAssistanceColumnIndex > 0 ? worksheet.Cells[row, needAssistanceColumnIndex].Text : "",
                    WriteYourNameAsFormOfSignature = signatureColumnIndex > 0 ? worksheet.Cells[row, signatureColumnIndex].Text : "",
                    Date = dateColumnIndex > 0 && DateTime.TryParse(worksheet.Cells[row, dateColumnIndex].Text, out var date) ? date : (DateTime?)null,
                    Entry_Status = statusColumnIndex > 0 ? worksheet.Cells[row, statusColumnIndex].Text : "",
                    Entry_DateSubmitted = dateSubmittedColumnIndex > 0 && DateTime.TryParse(worksheet.Cells[row, dateSubmittedColumnIndex].Text, out var dateSubmitted) ? dateSubmitted : (DateTime?)null,
                    Entry_DateCreated = DateTime.Now,
                    Entry_DateUpdated = DateTime.Now
                };

                if (nameColumnIndex > 0)
                {
                    var (firstName, middleName, lastName) = SplitName(worksheet.Cells[row, nameColumnIndex].Text);
                    record.Name_First = firstName;
                    record.Name_Middle = middleName ?? "Not Provided";
                    record.Name_Last = lastName;
                }

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
        }

        if (dataList.Any())
        {
            await _context.Database.BeginTransactionAsync();
            try
            {
                _context.ExcelDataCognitoMasterList.AddRange(dataList);
                await _context.SaveChangesAsync();
                await _context.Database.CommitTransactionAsync();
            }
            catch
            {
                await _context.Database.RollbackTransactionAsync();
                throw;
            }
        }
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
        using var streamReader = new StreamReader(file.OpenReadStream());
        using var csv = new CsvReader(streamReader, config);
        csv.Context.RegisterClassMap<ExcelDataGoogleFormsVolunteerProgramMap>();
        var dataList = new List<ExcelDataGoogleFormsVolunteerProgram>();
        var fileKeys = new HashSet<string>();
        await foreach (var record in csv.GetRecordsAsync<ExcelDataGoogleFormsVolunteerProgram>())
        {
            record.Timestamp = DateTime.TryParse(record.Timestamp.ToString(), out var timestamp)
                ? timestamp
                : DateTime.MinValue;
            record.Date = DateTime.TryParse(record.Date.ToString(), out var date)
                ? date
                : null;
            record.MethodOfContact ??= "Unknown";
            record.Comment ??= "No comment provided";
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
        if (dataList.Any())
        {
            await _context.Database.BeginTransactionAsync();
            try
            {
                _context.ExcelDataGoogleFormsVolunteerProgram.AddRange(dataList);
                await _context.SaveChangesAsync();
                await _context.Database.CommitTransactionAsync();
            }
            catch
            {
                await _context.Database.RollbackTransactionAsync();
                throw;
            }
        }
        return dbDuplicateCount + fileDuplicateCount;
    }

    private async Task<int> ProcessGoogleFormsVolunteerProgramXlsxData(IFormFile file)
    {
        int dbDuplicateCount = 0, fileDuplicateCount = 0;
        OfficeOpenXml.ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        int currentAccountId = AccountController.ActiveAccount?.Id ?? 0;
        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        using var package = new ExcelPackage(stream);
        var worksheet = package.Workbook.Worksheets[0];
        int rowCount = worksheet.Dimension?.Rows ?? 0;
        var dataList = new List<ExcelDataGoogleFormsVolunteerProgram>();
        var fileKeys = new HashSet<string>();
        for (int row = 2; row <= rowCount; row++)
        {
            if (string.IsNullOrWhiteSpace(worksheet.Cells[row, 2].Text))
            {
                continue;
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
        if (dataList.Any())
        {
            await _context.Database.BeginTransactionAsync();
            try
            {
                _context.ExcelDataGoogleFormsVolunteerProgram.AddRange(dataList);
                await _context.SaveChangesAsync();
                await _context.Database.CommitTransactionAsync();
            }
            catch
            {
                await _context.Database.RollbackTransactionAsync();
                throw;
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