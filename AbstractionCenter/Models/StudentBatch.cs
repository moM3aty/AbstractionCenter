using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AbstractionCenter.Models.Entities
{
    /// <summary>
    /// تم استبدال StudentCourse بهذا النموذج لربط الطالب بالدفعة بدلاً من الدورة العامة.
    /// وتم تحديث الحالات (Status) بناءً على طلبك.
    /// </summary>
    public class StudentBatch
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string StudentId { get; set; }
        [ForeignKey("StudentId")]
        public ApplicationUser Student { get; set; }

        [Required]
        public int BatchId { get; set; }
        [ForeignKey("BatchId")]
        public Batch Batch { get; set; }

        public DateTime EnrollmentDate { get; set; } = DateTime.Now;

        // الحالات الأكاديمية الجديدة للطالب
        [Display(Name = "حالة الطالب الأكاديمية")]
        public StudentAcademicStatus Status { get; set; } = StudentAcademicStatus.Registered;
    }

    public enum StudentAcademicStatus
    {
        [Display(Name = "مسجّل")]
        Registered,

        [Display(Name = "قيد الدراسة")]
        Studying,

        [Display(Name = "مكتمل (مؤهل للشهادة)")]
        Completed,

        [Display(Name = "غير مكتمل")]
        Incomplete
    }
}