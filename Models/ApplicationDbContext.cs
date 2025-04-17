using Microsoft.EntityFrameworkCore;
using MBBS.Dashboard.web.Controllers;

namespace MBBS.Dashboard.web.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<ExcelDataCourseraSpecialization> ExcelDataCourseraSpecialization { get; set; }
        public DbSet<ExcelDataCognitoMasterList> ExcelDataCognitoMasterList { get; set; }
        public DbSet<ExcelDataGoogleFormsVolunteerProgram> ExcelDataGoogleFormsVolunteerProgram { get; set; }
        public DbSet<ExcelDataCourseraMembershipReport> ExcelDataCourseraMembershipReports { get; set; }
        public DbSet<ExcelDataCourseraPivotLocationCityReport> ExcelDataCourseraPivotLocationCityReports { get; set; }
        public DbSet<ExcelDataCourseraPivotLocationCountryReport> ExcelDataCourseraPivotLocationCountryReports { get; set; }
        public DbSet<ExcelDataCourseraUsageReport> ExcelDataCourseraUsageReports { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure decimal precision/scale to avoid silent truncation
            modelBuilder.Entity<ExcelDataCourseraPivotLocationCityReport>(b =>
            {
                b.Property(x => x.TotalEstimatedLearningHours)
                 .HasColumnType("decimal(18,2)");

                b.Property(x => x.AverageEstimatedLearningHours)
                 .HasColumnType("decimal(18,2)");

                b.Property(x => x.AverageProgress)
                 .HasColumnType("decimal(5,2)");
            });
        }
    }
}
