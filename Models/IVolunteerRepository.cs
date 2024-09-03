using System.Collections.Generic;
using VolunteerManagementSystem.Models;

namespace VolunteerManagementSystem.Repositories
{
    public interface IVolunteerRepository
    {
        IEnumerable<Volunteer> GetAllVolunteers();
        Volunteer GetVolunteerById(int id);
        void AddVolunteer(Volunteer volunteer);
        void UpdateVolunteer(Volunteer volunteer);
        void DeleteVolunteer(int id);
    }
}