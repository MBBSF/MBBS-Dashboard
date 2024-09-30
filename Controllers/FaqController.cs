using FirstIterationProductRelease.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

//test

namespace FirstIterationProductRelease.Controllers
{
    public class FaqController : Controller
    {
        private readonly ILogger<FaqController> _logger;

        public FaqController(ILogger<FaqController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public ViewResult Faq()
        {
            return View();
        }
    }
}
