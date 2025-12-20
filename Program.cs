using MBBS.Dashboard.web.Controllers;
using MBBS.Dashboard.web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// ?? Configure logging to console only (removes EventLog provider) ??
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// ?? Register application services ??
builder.Services.AddControllersWithViews();

// ?? Configure session services ??
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ?? Configure database context ??
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// ?? Dependency injection ??
builder.Services.AddScoped<IAccountRepository, EFAccountRepository>();
builder.Services.AddTransient<IActivityLogRepository, EFActivityLogRepository>();
builder.Services.AddScoped<IPasswordHasher<Account>, PasswordHasher<Account>>();

var app = builder.Build();

// ?? HTTP request pipeline ??
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}
else
{
    // Skip HTTPS redirection in development
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

// ?? Default route ??
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=LogInPage}/{id?}"
);

// ?? Apply migrations and seed default admin ??
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    var passwordHasher = services.GetRequiredService<IPasswordHasher<Account>>();

    context.Database.Migrate();

    // Hash any existing plain-text passwords (migration safety)
    var accountsWithPlainTextPasswords = context.Accounts
        .Where(a => a.Password.Length < 50) // Hashed passwords are typically longer
        .ToList();

    foreach (var account in accountsWithPlainTextPasswords)
    {
        string originalPassword = account.Password;
        account.Password = passwordHasher.HashPassword(account, originalPassword);
        Console.WriteLine($"Hashed password for user: {account.Username}");
    }

    if (accountsWithPlainTextPasswords.Any())
    {
        context.SaveChanges();
        Console.WriteLine($"Updated {accountsWithPlainTextPasswords.Count} accounts with hashed passwords");
    }

    // Create default admin if no accounts exist
    if (!context.Accounts.Any())
    {
        var accountRepository = services.GetRequiredService<IAccountRepository>();

        var defaultAdmin = new Account
        {
            LegalName = "Default Admin",
            Username = "admin",
            Email = "admin@example.com",
            UserRole = "Admin",
            IsActive = true
        };

        // Hash the password before saving
        defaultAdmin.Password = passwordHasher.HashPassword(defaultAdmin, "Admin@123");

        context.Accounts.Add(defaultAdmin);
        context.SaveChanges();
        Console.WriteLine("Created default admin account with hashed password");
    }
}

app.Run();
