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
        public DbSet<Batch> Batches { get; set; }
        public DbSet<StudentBatch> StudentBatches { get; set; }
        public DbSet<SiteSetting> SiteSettings { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<LessonContent> LessonContents { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<AssignmentSubmission> AssignmentSubmissions { get; set; }
        public DbSet<FinalExam> FinalExams { get; set; }
        public DbSet<ExamQuestion> ExamQuestions { get; set; }
        public DbSet<Certificate> Certificates { get; set; }
        public DbSet<RegistrationRequest> RegistrationRequests { get; set; }
        public DbSet<InstructorApplication> InstructorApplications { get; set; }
        public DbSet<CourseQuestion> CourseQuestions { get; set; }
        public DbSet<RegistrationAnswer> RegistrationAnswers { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // --- منع الحذف المتسلسل (Cascade Delete) لتجنب أخطاء SQL Server ---

            builder.Entity<StudentBatch>()
                .HasOne(sb => sb.Batch)
                .WithMany(b => b.EnrolledStudents)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<StudentBatch>()
                .HasOne(sb => sb.Student)
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

            builder.Entity<AssignmentSubmission>()
                .HasOne(a => a.Student)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<AssignmentSubmission>()
                .HasOne(a => a.LessonContent)
                .WithMany()
                .OnDelete(DeleteBehavior.Cascade);

            // === التعديلات الجديدة لحل مشكلة الـ Certificates ===
            builder.Entity<Certificate>()
                .HasOne(c => c.Batch)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Certificate>()
                .HasOne(c => c.Student)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Batch>()
                .HasOne(b => b.Instructor)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<RegistrationRequest>()
                .HasOne(r => r.Batch)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}