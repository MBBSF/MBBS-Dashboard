using Microsoft.EntityFrameworkCore;
using MBBS.Dashboard.web.Controllers;

namespace MBBS.Dashboard.web.Models
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

        public DbSet<ExcelDataCourseraMembershipReport> ExcelDataCourseraMembershipReports { get; set; }

        public DbSet<ExcelDataCourseraPivotLocationCityReport> ExcelDataCourseraPivotLocationCityReports { get; set; }
        public DbSet<ExcelDataCourseraUsageReport> ExcelDataCourseraUsageReports { get; set; }

        // log test
        public DbSet<ActivityLog> ActivityLogs { get; set; } // new DbSet for ActivityLog



    }
}