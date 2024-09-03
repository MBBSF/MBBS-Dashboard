using Microsoft.AspNetCore.Mvc;
using VolunteerManagementSystem.Models;
using VolunteerManagementSystem.Repositories;

namespace VolunteerManagementSystem.Controllers
{
    public class VolunteersController : Controller
    {
        private readonly IVolunteerRepository _volunteerRepository;

        public VolunteersController(IVolunteerRepository volunteerRepository)
        {
            _volunteerRepository = volunteerRepository;
        }

        public IActionResult Index()
        {
            var volunteers = _volunteerRepository.GetAllVolunteers();
            return View(volunteers);
        }

        public IActionResult Details(int id)
        {
            var volunteer = _volunteerRepository.GetVolunteerById(id);
            if (volunteer == null)
                return NotFound();
            return View(volunteer);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Volunteer volunteer)
        {
            if (ModelState.IsValid)
            {
                _volunteerRepository.AddVolunteer(volunteer);
                return RedirectToAction(nameof(Index));
            }
            return View(volunteer);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var volunteer = _volunteerRepository.GetVolunteerById(id);
            if (volunteer == null)
                return NotFound();
            return View(volunteer);
        }

        [HttpPost]
        public IActionResult Edit(Volunteer volunteer)
        {
            if (ModelState.IsValid)
            {
                _volunteerRepository.UpdateVolunteer(volunteer);
                return RedirectToAction(nameof(Index));
            }
            return View(volunteer);
        }

        public IActionResult Delete(int id)
        {
            _volunteerRepository.DeleteVolunteer(id);
            return RedirectToAction(nameof(Index));
        }
    }
}