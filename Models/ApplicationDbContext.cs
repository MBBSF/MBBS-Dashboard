using Microsoft.EntityFrameworkCore;
using FirstIterationProductRelease.Models;
using FirstIterationProductRelease.Controllers;

namespace FirstIterationProductRelease.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) :
        base(options)
        { }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<ExcelDataCourseraSpecialization> ExcelDataCourseraSpecialization { get; set; }
        public DbSet<ExcelDataCognitoMasterList> ExcelDataCognitoMasterList { get; set; }
        public DbSet<ExcelDataGoogleFormsVolunteerProgram> ExcelDataGoogleFormsVolunteerProgram { get; set; }

        // log test
        public DbSet<ActivityLog> ActivityLogs { get; set; } // new DbSet for ActivityLog



    }
}