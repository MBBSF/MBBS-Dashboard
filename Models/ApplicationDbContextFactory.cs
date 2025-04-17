using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Logging;
using System;

namespace MBBS.Dashboard.web.Models
{
    public class ApplicationDbContextFactory
        : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();

            // Use your real connection string here (or read from config)
            builder
                .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=MBBS.Dashboard.webDB;Trusted_Connection=True;MultipleActiveResultSets=true")
                .LogTo(Console.WriteLine, LogLevel.Warning);

            return new ApplicationDbContext(builder.Options);
        }
    }
}
