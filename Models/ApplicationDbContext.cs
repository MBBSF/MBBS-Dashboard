using Microsoft.EntityFrameworkCore;
using FirstIterationProductRelease.Models;

namespace FirstIterationProductRelease.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) :
        base(options)
        { }
        public DbSet<Account> Accounts { get; set; }

        

    }
}