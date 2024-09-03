using Microsoft.EntityFrameworkCore;
using VolunteerManagementSystem.Models;

namespace VolunteerManagementSystem.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) :
        base(options)
        { }
        public DbSet<Account> Accounts { get; set; }

        public DbSet<Volunteer> Volunteers { get; set; }

        public DbSet<VolunteerOppurtunity> volunteerOppurtunities { get; set; }

    }
}