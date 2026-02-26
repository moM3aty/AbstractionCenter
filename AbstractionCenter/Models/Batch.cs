using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AbstractionCenter.Models.Entities
{
    /// <summary>
    /// الدفعة (Batch) هي أساس النظام الآن.
    /// يتم ربط المحاضر والطلاب والدروس والشهادات بالدفعة وليس بالدورة العامة.
    /// </summary>
    public class Batch
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "اسم الدفعة (مثال: دفعة 2025)")]
        public string BatchName { get; set; }

        // ربط الدفعة بالدورة الأم
        [Required]
        public int CourseId { get; set; }
        [ForeignKey("CourseId")]
        public Course Course { get; set; }

        // ربط الدفعة بمحاضر واحد فقط (لكي لا يرى إلا طلابه ومحتواه)
        [Required]
        public string InstructorId { get; set; }
        [ForeignKey("InstructorId")]
        public ApplicationUser Instructor { get; set; }

        [Display(Name = "تاريخ بدء الدفعة")]
        public DateTime StartDate { get; set; }

        [Display(Name = "حالة الدفعة")]
        public BatchStatus Status { get; set; } = BatchStatus.OpenForRegistration;

        // جميع العمليات التعليمية مرتبطة بالدفعة
        public ICollection<StudentBatch>? EnrolledStudents { get; set; }
        public ICollection<Lesson>? Lessons { get; set; }

        // تم إضافة FinalExamId لدعم الارتباط البرمجي وحل خطأ التعريف
        public int? FinalExamId { get; set; }
        [ForeignKey("FinalExamId")]
        public FinalExam? FinalExam { get; set; }
    }

    public enum BatchStatus
    {
        [Display(Name = "مفتوحة للتسجيل")] OpenForRegistration,
        [Display(Name = "قيد التنفيذ")] InProgress,
        [Display(Name = "مكتملة")] Completed,
        [Display(Name = "مغلقة")] Closed
    }
}