using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AbstractionCenter.Models.Entities
{
    /// <summary>
    /// نموذج التنبيهات: لإرسال إشعارات للطلاب (عند إضافة واجب أو اعتماد شهادة)
    /// </summary>
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Message { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // رابط اختياري لتوجيه الطالب (مثلاً رابط الشهادة أو الواجب)
        public string? LinkUrl { get; set; }
    }
}