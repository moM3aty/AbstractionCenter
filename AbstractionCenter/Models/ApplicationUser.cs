using Microsoft.AspNetCore.Identity;

namespace AbstractionCenter.Models.Entities
{
    /// <summary>
    /// نموذج المستخدم الأساسي الذي يرث من IdentityUser الخاص بـ ASP.NET Core
    /// سنستخدمه لكل من (الطالب، عضو هيئة التدريس، والمدير)
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        // إضافة حقول مخصصة غير موجودة في Identity الافتراضي
        public string FullName { get; set; }

        public string ProfilePicture { get; set; }

        // التخصص (مفيد لأعضاء هيئة التدريس)
        public string Specialization { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}