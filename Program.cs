using MBBS.Dashboard.web.Controllers;
using MBBS.Dashboard.web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), sqlOptions =>
    {
        // Enable connection resiliency for SQL Server with automatic retries on transient failures
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    })
);

// ?? Dependency injection ??
builder.Services.AddScoped<IAccountRepository, EFAccountRepository>();
builder.Services.AddTransient<IActivityLogRepository, EFActivityLogRepository>();

var app = builder.Build();

// ?? HTTP request pipeline ??
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
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

    context.Database.Migrate();

    if (!context.Accounts.Any())
    {
        var accountRepository = services.GetRequiredService<IAccountRepository>();

        var defaultAdmin = new Account
        {
            LegalName = "Default Admin",
            Username = "admin",
            Email = "admin@example.com",
            Password = "Admin@123",
            UserRole = "Admin",
            IsActive = true
        };

        context.Accounts.Add(defaultAdmin);
        context.SaveChanges();
    }
}

app.Run();
