using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using CsvHelper;
using FirstIterationProductRelease.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using CsvHelper.Configuration;


public class UploadFileController : Controller
{
    private readonly ApplicationDbContext _context;

    public UploadFileController(ApplicationDbContext context)
    {
        _context = context;
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

            if (fileExtension != ".csv")
            {
                ModelState.AddModelError("", "Only .csv files are supported.");
                return View(model);
            }


            try
            {
                switch (model.Source)
                {
                    case "Coursera":
                        await ProcessCourseraData(model.File);
                        break;
                    default:
                        ModelState.AddModelError("", "Invalid data source.");
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

    private async Task ProcessCourseraData(IFormFile file)
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
                dataList.Add(record);
            }

            _context.ExcelDataCourseraSpecialization.AddRange(dataList);
            await _context.SaveChangesAsync();

                
            
            
        }
    }




}