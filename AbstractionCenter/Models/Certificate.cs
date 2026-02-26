using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AbstractionCenter.Models.Entities
{
    /// <summary>
    /// تم تحديث الشهادة لترتبط بالدفعة (BatchId)
    /// </summary>
    public class Certificate
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

        [Required]
        [Display(Name = "رقم الشهادة المرجعي")]
        public string UniqueSerialNumber { get; set; } = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();

        [Display(Name = "تاريخ الإصدار")]
        public DateTime IssueDate { get; set; } = DateTime.Now;

        [Display(Name = "اعتماد الإدارة")]
        public bool IsApproved { get; set; } = false;
    }
}