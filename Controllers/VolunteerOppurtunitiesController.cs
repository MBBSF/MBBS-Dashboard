using Microsoft.AspNetCore.Mvc;
using VolunteerManagementSystem.Models;
using VolunteerManagementSystem.Repositories;

namespace VolunteerManagementSystem.Controllers
{
    public class VolunteerOppurtunitiesController : Controller
    {
        private readonly IVolunteerOppurtunityRepository _volunteerOppurtunityRepository;

        public VolunteerOppurtunitiesController(IVolunteerOppurtunityRepository volunteerOppurtunityRepository)
        {
            _volunteerOppurtunityRepository = volunteerOppurtunityRepository;
        }

        public IActionResult Index()
        {
            var volunteerOppurtunities = _volunteerOppurtunityRepository.GetAllVolunteerOppurtunities();
            return View(volunteerOppurtunities);
        }

        public IActionResult Details(int id)
        {
            var volunteerOppurtunity = _volunteerOppurtunityRepository.GetVolunteerOppurtunityById(id);
            if (volunteerOppurtunity == null)
                return NotFound();
            return View(volunteerOppurtunity);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(VolunteerOppurtunity volunteerOppurtunity)
        {
            if (ModelState.IsValid)
            {
                _volunteerOppurtunityRepository.AddVolunteerOppurtunity(volunteerOppurtunity);
                return RedirectToAction(nameof(Index));
            }
            return View(volunteerOppurtunity);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var volunteerOppurtunity = _volunteerOppurtunityRepository.GetVolunteerOppurtunityById(id);
            if (volunteerOppurtunity == null)
                return NotFound();
            return View(volunteerOppurtunity);
        }

        [HttpPost]
        public IActionResult Edit(VolunteerOppurtunity volunteerOppurtunity)
        {
            if (ModelState.IsValid)
            {
                _volunteerOppurtunityRepository.UpdateVolunteerOppurtunity(volunteerOppurtunity);
                return RedirectToAction(nameof(Index));
            }
            return View(volunteerOppurtunity);
        }

        public IActionResult Delete(int id)
        {
            _volunteerOppurtunityRepository.DeleteVolunteerOppurtunity(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
