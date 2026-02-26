using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AbstractionCenter.Models.Entities
{
    public class Course
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم الدورة مطلوب")]
        [Display(Name = "اسم الدورة")]
        public string Title { get; set; }

        [Display(Name = "وصف الدورة (للموقع التعريفي)")]
        public string Description { get; set; }

        [Display(Name = "صورة الدورة")]
        public string? ImageUrl { get; set; }

        [NotMapped]
        [Display(Name = "اختر صورة الدورة")]
        public IFormFile? ImageFile { get; set; }

        [Display(Name = "تاريخ الإنشاء")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<Batch>? Batches { get; set; }

        public ICollection<CourseQuestion>? CustomQuestions { get; set; }
    }
}