using VolunteerManagementSystem.Models;
using System.Collections.Generic;
using System.Linq;

namespace VolunteerManagementSystem.Repositories
{
    public class EFVolunteerRepository : IVolunteerRepository
    {
        private readonly ApplicationDbContext _context;

        public EFVolunteerRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Volunteer> GetAllVolunteers()
        {
            return _context.Volunteers.ToList();
        }

        public Volunteer GetVolunteerById(int id)
        {
            return _context.Volunteers.Find(id);
        }

        public void AddVolunteer(Volunteer volunteer)
        {
            _context.Volunteers.Add(volunteer);
            _context.SaveChanges();
        }

        public void UpdateVolunteer(Volunteer volunteer)
        {
            _context.Volunteers.Update(volunteer);
            _context.SaveChanges();
        }

        public void DeleteVolunteer(int id)
        {
            var volunteer = _context.Volunteers.Find(id);
            if (volunteer != null)
            {
                _context.Volunteers.Remove(volunteer);
                _context.SaveChanges();
            }
        }
    }
}