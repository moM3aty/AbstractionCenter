using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AbstractionCenter.Models.Entities
{
    /// <summary>
    /// نموذج يمثل طلب انضمام الطالب لدورة معينة (البديل الاحترافي لـ Google Forms)
    /// </summary>
    public class RegistrationRequest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string StudentId { get; set; }
        [ForeignKey("StudentId")]
        public ApplicationUser Student { get; set; }

        [Required]
        public int CourseId { get; set; }
        [ForeignKey("CourseId")]
        public Course Course { get; set; }

        [Display(Name = "رسالة أو ملاحظة من الطالب")]
        public string Message { get; set; }

        [Display(Name = "تاريخ تقديم الطلب")]
        public DateTime RequestDate { get; set; } = DateTime.Now;

        [Display(Name = "حالة الطلب")]
        public RequestStatus Status { get; set; } = RequestStatus.Pending;
    }

    public enum RequestStatus
    {
        [Display(Name = "قيد المراجعة")]
        Pending,

        [Display(Name = "تمت الموافقة")]
        Approved,

        [Display(Name = "مرفوض")]
        Rejected
    }
}