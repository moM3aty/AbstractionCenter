using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AbstractionCenter.Models.Entities;

namespace AbstractionCenter.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Course> Courses { get; set; }
        public DbSet<StudentCourse> StudentCourses { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<Certificate> Certificates { get; set; }
        public DbSet<RegistrationRequest> RegistrationRequests { get; set; }

        // الجدول الجديد الخاص بالتحكم الكامل في نصوص الموقع
        public DbSet<SiteSetting> SiteSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<StudentCourse>()
                .HasOne(sc => sc.Course)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}