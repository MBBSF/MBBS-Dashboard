using System.Collections.Generic;
using VolunteerManagementSystem.Models;

namespace VolunteerManagementSystem.Models
{
    public interface IVolunteerOppurtunityRepository
    {
        IEnumerable<VolunteerOppurtunity> GetAllVolunteerOppurtunities();
        VolunteerOppurtunity GetVolunteerOppurtunityById(int id);
        void AddVolunteerOppurtunity(VolunteerOppurtunity volunteerOppurtunity);
        void UpdateVolunteerOppurtunity(VolunteerOppurtunity volunteerOppurtunity);
        void DeleteVolunteerOppurtunity(int id);
    }
}
