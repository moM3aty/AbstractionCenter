using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AbstractionCenter.Models.Entities
{
    /// <summary>
    /// تم تحديث هذا النموذج ليتم ربط طلب تسجيل الطالب بـ (الدفعة) بدلاً من الدورة العامة
    /// </summary>
    public class RegistrationRequest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string StudentId { get; set; }
        [ForeignKey("StudentId")]
        public ApplicationUser Student { get; set; }

        // التعديل: الربط بالدفعة (Batch)
        [Required]
        public int BatchId { get; set; }
        [ForeignKey("BatchId")]
        public Batch Batch { get; set; }

        [Required(ErrorMessage = "الاسم الرباعي مطلوب")]
        [Display(Name = "الاسم رباعي")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "التخصص مطلوب")]
        [Display(Name = "التخصص")]
        public string Specialization { get; set; }

        [Required(ErrorMessage = "المستوى مطلوب")]
        [Display(Name = "المستوى")]
        public string Level { get; set; }

        [Required(ErrorMessage = "رقم الواتساب مطلوب")]
        [Display(Name = "رقم الواتساب")]
        public string WhatsAppNumber { get; set; }

        [Display(Name = "رقم تلغرام")]
        public string? TelegramNumber { get; set; }

        [Display(Name = "رسالة أو ملاحظة إضافية")]
        public string? Message { get; set; }

        [Display(Name = "تاريخ تقديم الطلب")]
        public DateTime RequestDate { get; set; } = DateTime.Now;

        [Display(Name = "حالة الطلب")]
        public RequestStatus Status { get; set; } = RequestStatus.Pending;

        public ICollection<RegistrationAnswer>? Answers { get; set; }
    }

    public enum RequestStatus
    {
        [Display(Name = "قيد المراجعة")] Pending,
        [Display(Name = "تمت الموافقة")] Approved,
        [Display(Name = "مرفوض")] Rejected
    }
}