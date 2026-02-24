using System;
using System.ComponentModel.DataAnnotations;

namespace AbstractionCenter.Models.Entities
{
    /// <summary>
    /// نموذج يمثل الدورة التدريبية، مصمم ليدعم طلبك الخاص بربط مسجل الدورة ورقم الواتساب والفورم الديناميكي.
    /// </summary>
    public class Course
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم الدورة مطلوب")]
        [Display(Name = "اسم الدورة")]
        public string Title { get; set; }

        [Display(Name = "وصف الدورة")]
        public string Description { get; set; }

        [Display(Name = "صورة الدورة")]
        public string ImageUrl { get; set; }

        // --- هذه الحقول مخصصة للميزة التي طلبتها (التواصل مع مسجل الدورة) ---

        [Display(Name = "اسم مسجل الدورة")]
        public string RegistrarName { get; set; }

        [Display(Name = "رقم واتساب المسجل")]
        [RegularExpression(@"^\+?\d{10,15}$", ErrorMessage = "صيغة رقم الواتساب غير صحيحة")]
        public string RegistrarWhatsApp { get; set; }

        [Display(Name = "رابط فورم التسجيل (جوجل فورم أو داخلي)")]
        public string RegistrationFormUrl { get; set; }

        // معرف حساب مسجل الدورة في لوحة التحكم (لكي تذهب طلبات التسجيل إلى صفحته الشخصية)
        public string RegistrarUserId { get; set; }

        // ------------------------------------------------------------------

        [Display(Name = "حالة الدورة")]
        public CourseStatus Status { get; set; }

        [Display(Name = "تاريخ البدء المتوقع")]
        public DateTime StartDate { get; set; }

        [Display(Name = "تاريخ الإنشاء")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public enum CourseStatus
    {
        [Display(Name = "مفتوحة للتسجيل")]
        OpenForRegistration,

        [Display(Name = "قيد التنفيذ")]
        InProgress,

        [Display(Name = "مكتملة")]
        Completed,

        [Display(Name = "مغلقة")]
        Closed
    }
}