using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace AbstractionCenter.Models.Entities
{
    public class InstructorApplication
    {
        [Key] public int Id { get; set; }
        [Required, Display(Name = "الاسم بالكامل")] public string FullName { get; set; }
        [Required, EmailAddress] public string Email { get; set; }
        [Required] public string PhoneNumber { get; set; }
        [Required] public string Specialization { get; set; }
        public string? CVPath { get; set; }
        [NotMapped] public IFormFile? CVFile { get; set; }
        public string? ProfilePicturePath { get; set; }
        [NotMapped] public IFormFile? ProfilePictureFile { get; set; }
        public RequestStatus Status { get; set; } = RequestStatus.Pending;
        public DateTime AppliedAt { get; set; } = DateTime.Now;
    }

    // 1. الوحدة الدراسية (مجلد يحتوي على الدروس)
    public class Lesson
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BatchId { get; set; }
        [ForeignKey("BatchId")]
        public Batch Batch { get; set; }

        [Required]
        public string Title { get; set; }

        public int Order { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<LessonContent>? Contents { get; set; }
    }

    // 2. محتوى الدرس (فيديو، PDF، أو واجب)
    public class LessonContent
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int LessonId { get; set; }
        [ForeignKey("LessonId")]
        public Lesson Lesson { get; set; }

        [Required]
        public string Title { get; set; }

        public ContentType Type { get; set; }

        // للفيديوهات (سيتم عرضها في iframe محمي لمنع التحميل)
        public string? VideoUrl { get; set; }

        // للملفات (PDF)
        public string? FilePath { get; set; }
        [NotMapped]
        public IFormFile? UploadedFile { get; set; }

        // للواجبات
        public string? Description { get; set; }

        // --- التعديل الجديد ---
        // روابط الاختبارات الخارجية (Google Forms, Microsoft Forms)
        public string? QuizUrl { get; set; }

        public int Order { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    // إضافة نوع (ExternalQuiz) للروابط الخارجية
    public enum ContentType
    {
        Video,
        Pdf,
        Assignment,
        ExternalQuiz
    }

    // 3. أسئلة التسجيل المخصصة
    public class CourseQuestion
    {
        [Key] public int Id { get; set; }
        [Required] public int CourseId { get; set; }
        [ForeignKey("CourseId")] public Course Course { get; set; }
        [Required] public string QuestionText { get; set; }
        public bool IsRequired { get; set; } = true;
        public int Order { get; set; } // لدعم ترتيب الأسئلة
    }

    // 4. إجابات التسجيل
    public class RegistrationAnswer
    {
        [Key] public int Id { get; set; }
        [Required] public int RegistrationRequestId { get; set; }
        [ForeignKey("RegistrationRequestId")] public RegistrationRequest RegistrationRequest { get; set; }
        [Required] public int CourseQuestionId { get; set; }
        [ForeignKey("CourseQuestionId")] public CourseQuestion CourseQuestion { get; set; }
        [Required] public string AnswerText { get; set; }
    }

    // 5. إعدادات الاختبار النهائي
    public class FinalExam
    {
        [Key] public int Id { get; set; }
        [Required] public int CourseId { get; set; }
        [ForeignKey("CourseId")] public Course Course { get; set; }
        [Required] public double PassingScorePercentage { get; set; } = 70; // نسبة النجاح
        public ICollection<ExamQuestion>? Questions { get; set; }
    }

    // 6. أسئلة الاختبار النهائي (اختيار من متعدد)
    public class ExamQuestion
    {
        [Key] public int Id { get; set; }
        [Required] public int FinalExamId { get; set; }
        [ForeignKey("FinalExamId")] public FinalExam FinalExam { get; set; }
        [Required] public string QuestionText { get; set; }
        [Required] public string Option1 { get; set; }
        [Required] public string Option2 { get; set; }
        [Required] public string Option3 { get; set; }
        [Required] public string Option4 { get; set; }
        [Required, Range(1, 4)] public int CorrectOption { get; set; } // رقم الإجابة الصحيحة 1-4
    }
}