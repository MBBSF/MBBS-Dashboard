using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using CsvHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using CsvHelper.Configuration;
using MBBS.Dashboard.web.Mappings;
using MBBS.Dashboard.web.Models;


public class UploadFileController : Controller
{
    private readonly ApplicationDbContext _context;

    public UploadFileController(ApplicationDbContext context)
    {
        _context = context;
    }

    private readonly Dictionary<string, List<string>> ExpectedHeaders = new()
    {
        { "Coursera", new List<string> { "Name", "Email", "External Id", "Specialization", "Specialization Slug", "University", "Enrollment Time", "Last Specialization Activity Time", "# Completed Courses", "# Courses in Specialization", "Completed", "Removed From Program", "Program Slug", "Program Name", "Enrollment Source", "Specialization Completion Time", "Specialization Certificate URL", "Job Title", "Job Type", "Location City", "Location Region", "Location Country" } },
        { "Cognito", new List<string> { "MARIEBARNEYBOSTONSCHOLARSHIPFOU_Id", "Name_First", "Name_Middle", "Name_Last", "Phone", "Age", "Address_Line1", "Address_Line2", "Address_City", "Address_State", "Address_PostalCode", "IntendedMajor", "ExpectedGraduation", "CollegePlanToAttend_Name", "CollegePlanToAttend_CityState", "HighSchoolCollegeData_CurrentStudent", "HighSchoolCollegeData_HighSchoolCollegeInformation", "HighSchoolCollegeData_Phone", "HighSchoolCollegeData_HighSchoolGraduation", "HighSchoolCollegeData_CumulativeGPA", "HighSchoolCollegeData_ACTCompositeScore", "HighSchoolCollegeData_SATCompositeScore", "HighSchoolCollegeData_SchoolCommunityRelatedActivities", "HighSchoolCollegeData_HonorsAndSpecialRecognition", "HighSchoolCollegeData_ExplainYourNeedForAssistance", "WriteYourNameAsFormOfSignature", "Date", "Entry_Status", "Entry_DateCreated", "Entry_DateSubmitted", "Entry_DateUpdated" } },
        { "GoogleForms", new List<string> { "Timestamp", "Mentor", "Mentee", "Date", "Time", "Method of Contact", "Comment" } }
    };

    private async Task<bool> ValidateFileHeaders(IFormFile file, string source, string fileExtension)
    {
        if (!ExpectedHeaders.ContainsKey(source)) return false;

        var expectedHeaders = ExpectedHeaders[source];

        if (fileExtension == ".csv")
        {
            using (var streamReader = new StreamReader(file.OpenReadStream()))
            using (var csv = new CsvReader(streamReader, new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true }))
            {
                await csv.ReadAsync();
                csv.ReadHeader();
                var fileHeaders = csv.HeaderRecord?.ToList();

                if (fileHeaders == null || !fileHeaders.SequenceEqual(expectedHeaders))
                {
                    return false; // Headers do not match
                }
            }
        }
        else if (fileExtension == ".xlsx")
        {
            return ValidateExcelHeaders(file, expectedHeaders);
            //return !expectedHeaders.Except(fileHeaders).Any();
        }

        return true; // Headers match
    }

    private bool ValidateExcelHeaders(IFormFile file, List<string> expectedHeaders)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using (var stream = new MemoryStream())
        {
            file.CopyTo(stream);
            using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets[0];
                int colCount = worksheet.Dimension.Columns;
                var fileHeaders = new List<string>();

                for (int col = 1; col <= colCount; col++)
                {
                    fileHeaders.Add(worksheet.Cells[1, col].Text.Trim());
                }

                return fileHeaders.SequenceEqual(expectedHeaders);
            }
        }
    }

    [HttpGet]
    public IActionResult Upload()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Upload(UploadFile model)
    {
        if (model.File != null && model.File.Length > 0 && !string.IsNullOrEmpty(model.Source))
        {
            var fileExtension = Path.GetExtension(model.File.FileName).ToLower();

            try
            {
                // Validate file headers
                if (!await ValidateFileHeaders(model.File, model.Source, fileExtension))
                {
                    ModelState.AddModelError("", "The uploaded file does not match the expected format for the selected data source.");
                    return View(model);
                }

                // Process the file if valid
                switch (fileExtension)
                {
                    case ".csv":
                        switch (model.Source)
                        {
                            case "Coursera":
                                await ProcessCourseraCsvData(model.File);
                                break;
                            case "Cognito":
                                await ProcessCognitoCsvData(model.File);
                                break;
                            case "GoogleForms":
                                await ProcessGoogleFormsCsvData(model.File);
                                break;
                            default:
                                ModelState.AddModelError("", "Invalid data source.");
                                return View(model);
                        }
                        break;

                    case ".xlsx":
                        switch (model.Source)
                        {
                            case "Coursera":
                                await ProcessCourseraXlsxData(model.File);
                                break;
                            case "Cognito":
                                await ProcessCognitoXlsxData(model.File);
                                break;
                            case "GoogleForms":
                                await ProcessGoogleFormsXlsxData(model.File);
                                break;
                            default:
                                ModelState.AddModelError("", "Invalid data source.");
                                return View(model);
                        }
                        break;

                    default:
                        ModelState.AddModelError("", "Only .csv and .xlsx files are supported.");
                        return View(model);
                }

                return RedirectToAction("UploadSuccess");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error processing file: {ex.Message}");
            }
        }

        ModelState.AddModelError("", "Please select a file and data source.");
        return View(model);
    }

    private async Task ProcessCourseraCsvData(IFormFile file)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            HeaderValidated = null,
            MissingFieldFound = null
        };

        using (var streamReader = new StreamReader(file.OpenReadStream()))
        using (var csv = new CsvReader(streamReader, config))
        {
            csv.Context.RegisterClassMap<ExcelDataCourseraSpecializationMap>();

            var dataList = new List<ExcelDataCourseraSpecialization>();

            await foreach (var record in csv.GetRecordsAsync<ExcelDataCourseraSpecialization>())
            {
                // Handle nulls or missing values
                record.EnrollmentSource ??= "Unknown"; // Default value
                record.ProgramName ??= "Not Specified"; // Default value
                record.LocationRegion ??= "Not Specified"; // Default value

                dataList.Add(record);
            }

            _context.ExcelDataCourseraSpecialization.AddRange(dataList);
            await _context.SaveChangesAsync();
        }
    }
    private async Task ProcessCognitoCsvData(IFormFile file)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            HeaderValidated = null,
            MissingFieldFound = null
        };

        using (var streamReader = new StreamReader(file.OpenReadStream()))
        using (var csv = new CsvReader(streamReader, config))
        {
            csv.Context.RegisterClassMap<ExcelDataCognitoMasterListMap>();

            var dataList = new List<ExcelDataCognitoMasterList>();

            await foreach (var record in csv.GetRecordsAsync<ExcelDataCognitoMasterList>())
            {
                // Handle null values
                record.Name_Middle ??= "Not Provided";
                record.Address_Line2 ??= "N/A";
                dataList.Add(record);
            }

            _context.ExcelDataCognitoMasterList.AddRange(dataList);
            await _context.SaveChangesAsync();
        }
    }
    private async Task ProcessCourseraXlsxData(IFormFile file)
    {
        // Ensure EPPlus license context is set
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using (var stream = new MemoryStream())
        {
            await file.CopyToAsync(stream);

            using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets[0]; // Assume the data is in the first worksheet
                int rowCount = worksheet.Dimension.Rows;

                var dataList = new List<ExcelDataCourseraSpecialization>();

                // Iterate through rows starting after the header (row 1 is assumed to be headers)
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
                        EnrollmentTime = DateTime.TryParse(worksheet.Cells[row, 7].Text, out var enrollmentTime) ? enrollmentTime : (DateTime?)null,
                        LastSpecializationActivityTime = DateTime.TryParse(worksheet.Cells[row, 8].Text, out var lastActivityTime) ? lastActivityTime : (DateTime?)null,
                        CompletedCourses = int.TryParse(worksheet.Cells[row, 9].Text, out var completedCourses) ? completedCourses : (int?)null,
                        CoursesInSpecialization = int.TryParse(worksheet.Cells[row, 10].Text, out var coursesInSpec) ? coursesInSpec : (int?)null,
                        Completed = worksheet.Cells[row, 11].Text,
                        RemovedFromProgram = worksheet.Cells[row, 12].Text,
                        ProgramSlug = worksheet.Cells[row, 13].Text,
                        ProgramName = worksheet.Cells[row, 14].Text,
                        EnrollmentSource = worksheet.Cells[row, 15].Text ?? "Unknown", // Default value
                        SpecializationCompletionTime = DateTime.TryParse(worksheet.Cells[row, 16].Text, out var completionTime) ? completionTime : (DateTime?)null,
                        SpecializationCertificateURL = worksheet.Cells[row, 17].Text,
                        JobTitle = worksheet.Cells[row, 18].Text,
                        JobType = worksheet.Cells[row, 19].Text,
                        LocationCity = worksheet.Cells[row, 20].Text,
                        LocationRegion = worksheet.Cells[row, 21].Text ?? "Not Specified", // Default value
                        LocationCountry = worksheet.Cells[row, 22].Text,
                    };

                    dataList.Add(record);
                }

                // Add records to the database and save changes
                _context.ExcelDataCourseraSpecialization.AddRange(dataList);
                await _context.SaveChangesAsync();
            }
        }
    }

    private async Task ProcessCognitoXlsxData(IFormFile file)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using (var stream = new MemoryStream())
        {
            await file.CopyToAsync(stream);
            using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.Rows;

                var dataList = new List<ExcelDataCognitoMasterList>();

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
                    };

                    dataList.Add(record);
                }

                _context.ExcelDataCognitoMasterList.AddRange(dataList);
                await _context.SaveChangesAsync();
            }
        }
    }

    private async Task ProcessGoogleFormsCsvData(IFormFile file)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            HeaderValidated = null,
            MissingFieldFound = null
        };

        using (var streamReader = new StreamReader(file.OpenReadStream()))
        using (var csv = new CsvReader(streamReader, config))
        {
            csv.Context.RegisterClassMap<ExcelDataGoogleFormsVolunteerProgramMap>();

            var dataList = new List<ExcelDataGoogleFormsVolunteerProgram>();

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

                dataList.Add(record);
            }

            _context.ExcelDataGoogleFormsVolunteerProgram.AddRange(dataList);
            await _context.SaveChangesAsync();
        }
    }

    private async Task ProcessGoogleFormsXlsxData(IFormFile file)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using (var stream = new MemoryStream())
        {
            await file.CopyToAsync(stream);

            using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets[0]; // Assume data is in the first worksheet
                int rowCount = worksheet.Dimension.Rows;

                var dataList = new List<ExcelDataGoogleFormsVolunteerProgram>();

                for (int row = 2; row <= rowCount; row++)
                {
                    var timestamp = DateTime.TryParse(worksheet.Cells[row, 1].Text, out var parsedTimestamp)
                        ? parsedTimestamp
                        : DateTime.MinValue; // Default if invalid

                    var date = DateTime.TryParse(worksheet.Cells[row, 4].Text, out var parsedDate)
                        ? parsedDate
                        : (DateTime?)null;

                    var record = new ExcelDataGoogleFormsVolunteerProgram
                    {
                        Timestamp = timestamp,
                        Mentor = worksheet.Cells[row, 2].Text,
                        Mentee = worksheet.Cells[row, 3].Text,
                        Date = date,
                        Time = worksheet.Cells[row, 5].Text,
                        MethodOfContact = worksheet.Cells[row, 6].Text ?? "Unknown", // Default for null
                        Comment = worksheet.Cells[row, 7].Text ?? "No comment provided", // Default for null
                    };

                    dataList.Add(record);
                }

                _context.ExcelDataGoogleFormsVolunteerProgram.AddRange(dataList);
                await _context.SaveChangesAsync();
            }
        }
    }

    public IActionResult UploadSuccess()
    {
        return View();
    }


}