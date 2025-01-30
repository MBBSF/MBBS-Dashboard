using FirstIterationProductRelease.Controllers;
using FirstIterationProductRelease.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Register application services
builder.Services.AddControllersWithViews();

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
app.UseAuthorization();

// Set the default landing page to login ---abdel
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=LogInPage}/{id?}"
);

app.Run();
