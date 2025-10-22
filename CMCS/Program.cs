using CMCS.Data;
using CMCS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using System.Globalization; // 1. REQUIRED for CultureInfo

namespace CMCS
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ==============================
            // Database Connection (In-Memory)
            // ==============================
            builder.Services.AddDbContext<CMCSContext>(options =>
                options.UseInMemoryDatabase("CMCS_InMemoryDB"));

            // Keep Identity system connected to the same In-Memory DB
            builder.Services.AddDefaultIdentity<IdentityUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireDigit = false;
            })
            .AddEntityFrameworkStores<CMCSContext>();

            // ==============================
            // 2. CULTURE CONFIGURATION (Forces dot '.' as decimal separator)
            // This fixes the validation error on HoursWorked and HourlyRate.
            // ==============================
            builder.Services.Configure<RequestLocalizationOptions>(options =>
            {
                // Use en-US culture, which expects the dot (.) as the decimal separator.
                var cultureInfo = new CultureInfo("en-US");

                options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture(cultureInfo);
                options.SupportedCultures = new List<CultureInfo> { cultureInfo };
                options.SupportedUICultures = new List<CultureInfo> { cultureInfo };
            });

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // ==============================
            // Seed In-Memory Lecturers (Your Original Logic Restored)
            // ==============================
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<CMCSContext>();

                if (!context.Lecturers.Any())
                {
                    context.Lecturers.AddRange(
                        new Lecturer
                        {
                            LecturerID = 1,
                            FirstName = "John",
                            LastName = "Doe",
                            Email = "john.doe@iie.ac.za",
                            Phone = "0712345678",
                            StaffNumber = "L1001"
                        },
                        new Lecturer
                        {
                            LecturerID = 2,
                            FirstName = "Sarah",
                            LastName = "Naidoo",
                            Email = "sarah.naidoo@iie.ac.za",
                            Phone = "0723456789",
                            StaffNumber = "L1002"
                        },
                        new Lecturer
                        {
                            LecturerID = 3,
                            FirstName = "Michael",
                            LastName = "Mokoena",
                            Email = "michael.mokoena@iie.ac.za",
                            Phone = "0734567890",
                            StaffNumber = "L1003"
                        }
                    );

                    context.SaveChanges();
                }
            }

            // ==============================
            // Configure HTTP Pipeline
            // ==============================
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            // 3. ADD CULTURE MIDDLEWARE HERE
            app.UseRequestLocalization();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            // ==============================
            // Route Mapping
            // ==============================
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapRazorPages();

            app.Run();
        }
    }
}
