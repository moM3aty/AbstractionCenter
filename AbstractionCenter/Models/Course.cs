using System;
using System.ComponentModel.DataAnnotations;

namespace AbstractionCenter.Models.Entities
{
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

        [Display(Name = "اسم مسجل الدورة")]
        public string RegistrarName { get; set; }

        [Display(Name = "رقم واتساب المسجل")]
        public string RegistrarWhatsApp { get; set; }

        [Display(Name = "رابط فورم التسجيل")]
        public string RegistrationFormUrl { get; set; }

        public string RegistrarUserId { get; set; }

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