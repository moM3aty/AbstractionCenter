using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using AbstractionCenter.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AbstractionCenter.Data
{
    /// <summary>
    /// مركز قاعدة البيانات الخاص بالمنصة
    /// يرث من IdentityDbContext ليدعم نظام تسجيل الدخول والصلاحيات تلقائياً
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // تسجيل الجداول المخصصة في قاعدة البيانات
        public DbSet<Course> Courses { get; set; }
        public DbSet<StudentCourse> StudentCourses { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        // سيتم إضافة جدول الشهادات (Certificates) لاحقاً

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            // تهيئة إضافية إذا لزم الأمر، مثل منع الحذف المتسلسل (Cascade Delete) لتجنب أخطاء قاعدة البيانات
            builder.Entity<StudentCourse>()
                .HasOne(sc => sc.Course)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}