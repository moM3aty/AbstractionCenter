using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AbstractionCenter.Models.Entities
{
    public class Batch
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public Course Course { get; set; }
        public string BatchName { get; set; }
        public DateTime StartDate { get; set; }
        public BatchStatus Status { get; set; }

        // المحاضر الأساسي
        public string InstructorId { get; set; }
        public ApplicationUser Instructor { get; set; }

        // --- الخصائص الجديدة ---

        // رابط مجموعة التلغرام
        public string? TelegramGroupUrl { get; set; }

        // المحاضرون الإضافيون (يتم حفظ الـ IDs الخاصة بهم كنص مفصول بفاصلة)
        public string? AdditionalInstructorIds { get; set; }

        // ملاحظة التنفيذ
        [MaxLength(200)]
        public string ExecutionNote { get; set; } = "Delivered by Abstraction Training Team";
        public bool ShowExecutionNote { get; set; } = true;

        // التسعير
        public decimal Price { get; set; } = 0;
        public bool ShowPrice { get; set; } = true;

        public double DiscountPercentage { get; set; } = 0;
        public bool ShowDiscount { get; set; } = false;

        // العلاقات
        public int? FinalExamId { get; set; }
        public FinalExam FinalExam { get; set; }
        public ICollection<StudentBatch> EnrolledStudents { get; set; }
        public ICollection<Lesson> Lessons { get; set; }
    }

    public enum BatchStatus
    {
        OpenForRegistration = 0,
        InProgress = 1,
        Completed = 2,
        Closed = 3
    }
}