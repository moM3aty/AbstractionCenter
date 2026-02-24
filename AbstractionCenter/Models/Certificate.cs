using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AbstractionCenter.Models.Entities
{
    /// <summary>
    /// نموذج الشهادة الإلكترونية
    /// يتم إصدارها للطالب عند اكتمال الدورة واعتمادها
    /// </summary>
    public class Certificate
    {
        [Key]
        public int Id { get; set; }

        // ربط الشهادة بالطالب
        [Required]
        public string StudentId { get; set; }
        [ForeignKey("StudentId")]
        public ApplicationUser Student { get; set; }

        // ربط الشهادة بالدورة
        [Required]
        public int CourseId { get; set; }
        [ForeignKey("CourseId")]
        public Course Course { get; set; }

        // رقم تسلسلي فريد للتحقق منه (يتم توليده تلقائياً بأحرف وأرقام)
        [Required]
        [Display(Name = "رقم الشهادة الفريد")]
        public string UniqueSerialNumber { get; set; } = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();

        [Display(Name = "تاريخ الإصدار")]
        public DateTime IssueDate { get; set; } = DateTime.Now;

        [Display(Name = "معتمدة من الإدارة؟")]
        public bool IsApproved { get; set; } = false; // يمكن تغييرها لتكون true مباشرة حسب سياسة المركز
    }
}