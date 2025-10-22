using CMCS.Data; // Assume your context is here
using CMCS.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.InMemory;

namespace CMCS.Tests.Mocks
{
    public static class DbContextMocker
    {
        public static CMCSContext GetCMCSContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<CMCSContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            var context = new CMCSContext(options);

            // Ensure a clean state
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // Add Test Claims
            context.Claims.AddRange(new List<Claim>
            {
                new Claim { ClaimID = 1, LecturerID = 101, Status = "Pending", HoursWorked = 10.0, HourlyRate = 100.00m, ClaimDate = DateTime.Parse("2025-10-01") },
                new Claim { ClaimID = 2, LecturerID = 102, Status = "Approved", HoursWorked = 5.0, HourlyRate = 200.00m, ClaimDate = DateTime.Parse("2025-10-02") },
                new Claim { ClaimID = 3, LecturerID = 101, Status = "Rejected", HoursWorked = 2.0, HourlyRate = 150.00m, ClaimDate = DateTime.Parse("2025-10-03") },
                new Claim { ClaimID = 4, LecturerID = 103, Status = "Pending", HoursWorked = 1.0, HourlyRate = 120.00m, ClaimDate = DateTime.Parse("2025-10-04") }
            });

            // Add Test Lecturers (Needed for Controller tests that access Lecturers)
            // NOTE: We assume a 'Lecturer' model exists in CMCS.Models
            // This is vital for your join/include logic to not crash.
            context.Lecturers.AddRange(new List<dynamic>
            {
                new { LecturerID = 101, FirstName = "John", LastName = "Doe" },
                new { LecturerID = 102, FirstName = "Jane", LastName = "Smith" },
                new { LecturerID = 103, FirstName = "Bob", LastName = "Jones" }
            }.Select(l =>
            {
                // This logic is a hack to create a Lecturer instance without knowing its full definition
                // If you provide the Lecturer model, this part should be updated to use the real type.
                var lecturer = Activator.CreateInstance(typeof(Lecturer));
                lecturer.GetType().GetProperty("LecturerID").SetValue(lecturer, l.LecturerID);
                lecturer.GetType().GetProperty("FirstName").SetValue(lecturer, l.FirstName);
                lecturer.GetType().GetProperty("LastName").SetValue(lecturer, l.LastName);
                return (Lecturer)lecturer;
            }));

            // The AuditTrail and Notification models are not provided, so we'll skip adding test data for them,
            // but the controller SaveChanges() calls will still work in the mock environment.

            context.SaveChanges();

            return context;
        }
    }
}