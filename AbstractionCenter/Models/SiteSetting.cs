using System.ComponentModel.DataAnnotations;

namespace AbstractionCenter.Models.Entities
{
    /// <summary>
    /// هذا النموذج سيجعل كل حرف في الموقع قابل للتعديل من لوحة الإدارة
    /// يعتمد على فكرة (المفتاح والقيمة) مثل: Key = "ContactPhone", Value = "010000000"
    /// </summary>
    public class SiteSetting
    {
        [Key]
        [MaxLength(100)]
        public string Key { get; set; } // مثال: HeroTitle, AboutUsText, FacebookLink

        [Required]
        [Display(Name = "القيمة / النص")]
        public string Value { get; set; } // النص الذي سيظهر للناس

        [MaxLength(50)]
        public string Group { get; set; } // لتصنيفها في الإدارة: General, Home, Contact, About

        [Display(Name = "اسم توضيحي للمدير")]
        public string DisplayName { get; set; } // مثال: "عنوان الصفحة الرئيسية"
    }
}