using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WarsztatCar.Data;
using WarsztatCar.Models;
using WarsztatCar.Services;

namespace WarsztatCar
{
    public class Program
    {
        public static async Task Main(string[] args) // Zmienione na async Task dla Seeder'a
        {
            var builder = WebApplication.CreateBuilder(args);

            // Konfiguracja bazy danych SQLite
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? "Data Source=WarsztatCar.db";

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(connectionString));

            // Konfiguracja Identity (Logowanie i Role)
            builder.Services.AddDefaultIdentity<IdentityUser>(options => {
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequireDigit = false; // U³atwienie na czas testów
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
            })
            .AddRoles<IdentityRole>() // W³¹czenie obs³ugi ról
            .AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.AddControllersWithViews();

            builder.Services.AddTransient<IEmailSender, EmailSender>();

            var app = builder.Build();

            // SEEDOWANIE DANYCH (Tworzenie ról Admin/User na starcie)
            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                string[] roleNames = { "Admin", "User" };
                foreach (var roleName in roleNames)
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        await roleManager.CreateAsync(new IdentityRole(roleName));
                    }
                }
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // Wa¿na kolejnoœæ: najpierw Authentication, potem Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapRazorPages(); // Wymagane dla Identity (logowanie/rejestracja)

            var cultureInfo = new System.Globalization.CultureInfo("pl-PL");
            cultureInfo.DateTimeFormat.ShortDatePattern = "dd.MM.yyyy";
            cultureInfo.DateTimeFormat.LongTimePattern = "HH:mm";

            System.Globalization.CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

            app.Run();
        }
    }
}
