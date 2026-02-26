using System.ComponentModel.DataAnnotations;

namespace AbstractionCenter.Models.Entities
{
    public class SiteSetting
    {
        [Key]
        public string Key { get; set; }

        [Display(Name = "القيمة (بالعربية)")]
        public string Value { get; set; }

        // --- التعديل الجذري: إضافة حقل الترجمة الإنجليزية ---
        [Display(Name = "القيمة (بالإنجليزية)")]
        public string? ValueEn { get; set; }

        public string Group { get; set; }
        public string DisplayName { get; set; }
    }
}