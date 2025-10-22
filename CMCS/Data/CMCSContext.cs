using CMCS.Models;
using Microsoft.EntityFrameworkCore;

namespace CMCS.Data
{
    public class CMCSContext : DbContext
    {
        public CMCSContext(DbContextOptions<CMCSContext> options) : base(options)
        {
        }

        // Tables
        public DbSet<Lecturer> Lecturers { get; set; }
        public DbSet<Claim> Claims { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<AuditTrail> AuditTrails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Claim ↔ Lecturer (1-to-many) - Relies on the clean model now
            modelBuilder.Entity<Claim>()
                .HasOne<Lecturer>()
                .WithMany()
                .HasForeignKey(c => c.LecturerID)
                .OnDelete(DeleteBehavior.Cascade);

            // Document ↔ Claim (1-to-many)
            modelBuilder.Entity<Document>()
                .HasOne<Claim>()
                .WithMany()
                .HasForeignKey(d => d.ClaimID)
                .OnDelete(DeleteBehavior.Cascade);

            // Notification ↔ Claim (1-to-many)
            modelBuilder.Entity<Notification>()
                .HasOne<Claim>()
                .WithMany()
                .HasForeignKey(n => n.ClaimID)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}