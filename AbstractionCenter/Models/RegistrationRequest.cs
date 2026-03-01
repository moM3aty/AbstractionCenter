using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AbstractionCenter.Models.Entities
{
    public class RegistrationRequest
    {
        public int Id { get; set; }

        public int BatchId { get; set; }
        public Batch Batch { get; set; }

        // تم جعل StudentId اختيارياً لأن الزائر ليس له حساب بعد
        public string? StudentId { get; set; }
        public ApplicationUser Student { get; set; }

        [Required]
        [Display(Name = "الاسم باللغة العربية")]
        public string FullName { get; set; }

        [Required]
        [Display(Name = "الاسم باللغة الإنجليزية")]
        public string FullNameEn { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } // حقل جديد للإيميل

        [Required]
        public string WhatsAppNumber { get; set; }

        public string? TelegramNumber { get; set; }
        public string Specialization { get; set; }
        public string Level { get; set; }

        // حقل جديد لملاحظات المتدرب الإضافية (لحل مشكلة الخطأ)
        public string? Message { get; set; }

        // حقل جديد لحفظ مسار صورة إيصال الدفع
        public string? ReceiptFilePath { get; set; }

        public RequestStatus Status { get; set; }
        public DateTime RequestDate { get; set; }

        public ICollection<RegistrationAnswer> Answers { get; set; }
    }

    // تعريف حالات الطلب لحل مشكلة (The name 'RequestStatus' does not exist)
    public enum RequestStatus
    {
        [Display(Name = "قيد المراجعة")]
        Pending = 0,

        [Display(Name = "تم القبول")]
        Approved = 1,

        [Display(Name = "مرفوض")]
        Rejected = 2
    }
}