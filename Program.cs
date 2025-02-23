using MBBS.Dashboard.web.Controllers;
using MBBS.Dashboard.web.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Register application services
builder.Services.AddControllersWithViews();

// Configure database context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Use InMemoryAccountRepository instead of EFAccountRepository
builder.Services.AddScoped<IAccountRepository, EFAccountRepository>();
builder.Services.AddTransient<IActivityLogRepository, EFActivityLogRepository>(); // abdel EFActivityLogRepository test
var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// Set the default landing page to login ---abdel
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=LogInPage}/{id?}"
);
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();

    // Apply any pending migrations.
    context.Database.Migrate();

    // Check if any accounts exist.
    if (!context.Accounts.Any())
    {
        // Create a default admin account.
        var accountRepository = services.GetRequiredService<IAccountRepository>();

        var defaultAdmin = new Account
        {
            LegalName = "Default Admin",
            Username = "admin",
            Email = "admin@example.com",
            // You must hash the password; for demonstration, we'll use a plaintext password then hash it.
            Password = accountRepository.HashPassword("Admin@123"), // Replace with a secure default
            UserRole = "Admin"
        };

        context.Accounts.Add(defaultAdmin);
        context.SaveChanges();
    }
}
app.Run();
