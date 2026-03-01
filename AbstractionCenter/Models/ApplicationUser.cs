using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace AbstractionCenter.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }

        // --- الحقل الجديد ---
        public string? FullNameEn { get; set; }

        public string? ProfilePicture { get; set; }

        [NotMapped]
        public IFormFile? ProfilePictureFile { get; set; }

        public string? Specialization { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string? CurrentSessionId { get; set; }

    
        public bool IsActive { get; set; } = true;
    }
}