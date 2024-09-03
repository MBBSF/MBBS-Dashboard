using VolunteerManagementSystem.Models;
using System.Collections.Generic;
using System.Linq;

namespace VolunteerManagementSystem.Models
{
    public class EFVolunteerOppurtunityRepository : IVolunteerOppurtunityRepository
    {
        private readonly ApplicationDbContext _context;

        public EFVolunteerOppurtunityRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<VolunteerOppurtunity> GetAllVolunteerOppurtunities()
        {
            return _context.volunteerOppurtunities.ToList();
        }

        public VolunteerOppurtunity GetVolunteerOppurtunityById(int id)
        {
            return _context.volunteerOppurtunities.Find(id);
        }

        public void AddVolunteerOppurtunity(VolunteerOppurtunity volunteerOppurtunity)
        {
            _context.volunteerOppurtunities.Add(volunteerOppurtunity);
            _context.SaveChanges();
        }

        public void UpdateVolunteerOppurtunity(VolunteerOppurtunity volunteerOppurtunity)
        {
            _context.volunteerOppurtunities.Update(volunteerOppurtunity);
            _context.SaveChanges();
        }

        public void DeleteVolunteerOppurtunity(int id)
        {
            var volunteerOppurtunity = _context.volunteerOppurtunities.Find(id);
            if (volunteerOppurtunity != null)
            {
                _context.volunteerOppurtunities.Remove(volunteerOppurtunity);
                _context.SaveChanges();
            }
        }
    }
}
