using MBBS.Dashboard.web.Controllers;
using MBBS.Dashboard.web.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Register application services
builder.Services.AddControllersWithViews();

// Configure session services
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configure database context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Use InMemoryAccountRepository instead of EFAccountRepository
builder.Services.AddSingleton<IAccountRepository, InMemoryAccountRepository>();
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

// Enable session middleware (must be before UseAuthorization)
app.UseSession();

app.UseAuthorization();

// Set the default landing page to login ---abdel
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=LogInPage}/{id?}"
);

app.Run();