using Microsoft.AspNetCore.Identity;
using System;

namespace AbstractionCenter.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        // إضافة علامة ? تجعل الحقل يقبل قيمة Null في قاعدة البيانات
        public string? FullName { get; set; }

        public string? ProfilePicture { get; set; }

        public string? Specialization { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}