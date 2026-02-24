using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AbstractionCenter.Models.Entities
{
    /// <summary>
    /// جدول الربط (Junction Table) بين الطالب والدورة
    /// يمثل اشتراك الطالب في دورة معينة وحالة تقدمه فيها
    /// </summary>
    public class StudentCourse
    {
        [Key]
        public int Id { get; set; }

        // معرف الطالب (Foreign Key من جدول Identity)
        [Required]
        public string StudentId { get; set; }
        [ForeignKey("StudentId")]
        public ApplicationUser Student { get; set; }

        // معرف الدورة
        [Required]
        public int CourseId { get; set; }
        [ForeignKey("CourseId")]
        public Course Course { get; set; }

        public DateTime EnrollmentDate { get; set; } = DateTime.Now;

        // حالة التقدم: قيد التنفيذ أم مكتملة (تمهيداً لإصدار الشهادة)
        public EnrollmentStatus Status { get; set; } = EnrollmentStatus.InProgress;
    }

    public enum EnrollmentStatus
    {
        [Display(Name = "قيد التنفيذ")]
        InProgress,

        [Display(Name = "مكتملة")]
        Completed
    }
}