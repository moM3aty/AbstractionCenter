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
        public DbSet<SiteSetting> SiteSettings { get; set; }

        public DbSet<InstructorApplication> InstructorApplications { get; set; }

        // الجداول الجديدة للدروس والمحتوى
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<LessonContent> LessonContents { get; set; }

        // جداول الفورم الديناميكي
        public DbSet<CourseQuestion> CourseQuestions { get; set; }
        public DbSet<RegistrationAnswer> RegistrationAnswers { get; set; }

        // جداول الاختبار النهائي
        public DbSet<FinalExam> FinalExams { get; set; }
        public DbSet<ExamQuestion> ExamQuestions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<StudentCourse>()
                .HasOne(sc => sc.Course)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<RegistrationAnswer>()
                .HasOne(ra => ra.RegistrationRequest)
                .WithMany(rr => rr.Answers)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<RegistrationAnswer>()
                .HasOne(ra => ra.CourseQuestion)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}