using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AbstractionCenter.Models.Entities
{
    /// <summary>
    /// نموذج الواجبات التي يضيفها عضو هيئة التدريس (المسجل) داخل الدورة
    /// </summary>
    public class Assignment
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "عنوان الواجب مطلوب")]
        [Display(Name = "عنوان الواجب")]
        public string Title { get; set; }

        [Display(Name = "وصف أو تفاصيل الواجب")]
        public string Description { get; set; }

        [Display(Name = "تاريخ التسليم (Deadline)")]
        public DateTime DueDate { get; set; }

        // ربط الواجب بالدورة الخاصة به
        [Required]
        public int CourseId { get; set; }
        [ForeignKey("CourseId")]
        public Course Course { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}