using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AbstractionCenter.Models.Entities
{
    /// <summary>
    /// هذا النموذج يمثل رفع الطالب لحل الواجب وتقييم المحاضر له.
    /// </summary>
    public class AssignmentSubmission
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string StudentId { get; set; }
        [ForeignKey("StudentId")]
        public ApplicationUser Student { get; set; }

        [Required]
        public int LessonContentId { get; set; }
        [ForeignKey("LessonContentId")]
        public LessonContent LessonContent { get; set; }

        [Required]
        [Display(Name = "ملف الحل")]
        public string FilePath { get; set; }

        [Display(Name = "تاريخ التسليم")]
        public DateTime SubmissionDate { get; set; } = DateTime.Now;

        // --- قسم التقييم الخاص بالمحاضر ---
        [Display(Name = "الدرجة")]
        public double? Grade { get; set; }

        [Display(Name = "ملاحظات المحاضر")]
        public string? InstructorFeedback { get; set; }

        public bool IsGraded { get; set; } = false;
    }
}