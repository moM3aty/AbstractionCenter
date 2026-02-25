using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace AbstractionCenter.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }

        // المسار المحفوظ في قاعدة البيانات
        public string? ProfilePicture { get; set; }

        // خاصية غير مخزنة في الداتابيز، تُستخدم فقط لاستقبال الملف من الفورم
        [NotMapped]
        public IFormFile? ProfilePictureFile { get; set; }

        public string? Specialization { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // التعديل الرابع: خاصية لمنع الدخول من أكثر من جهاز (تسجيل الجلسة الحالية)
        public string? CurrentSessionId { get; set; }
    }
}