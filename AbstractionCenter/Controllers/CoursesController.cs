using Microsoft.AspNetCore.Mvc;
using AbstractionCenter.Models.Entities;
using System;
using System.Collections.Generic;

namespace AbstractionCenter.Controllers
{
    public class CoursesController : Controller
    {
        // عرض الدورات المفتوحة للتسجيل
        public IActionResult Open()
        {
            ViewData["Title"] = "الدورات المتاحة للتسجيل";

            // بيانات وهمية (Mock Data) لتجربة الواجهة والشكل حتى يتم ربط قاعدة البيانات لاحقاً
            var openCourses = new List<Course>
            {
                new Course
                {
                    Id = 1,
                    Title = "دبلومة تطوير واجهات الويب (Front-End)",
                    Description = "تعلم بناء مواقع تفاعلية حديثة باستخدام HTML5, CSS3, و JavaScript المتقدمة لتصبح جاهزاً لسوق العمل.",
                    RegistrarName = "أ. أحمد محمود",
                    RegistrarWhatsApp = "201000000000", // أرقام وهمية للتجربة
                    RegistrationFormUrl = "https://forms.google.com/...", // رابط الفورم
                    StartDate = DateTime.Now.AddDays(10),
                    Status = CourseStatus.OpenForRegistration
                },
                new Course
                {
                    Id = 2,
                    Title = "دورة التسويق الرقمي المتكامل",
                    Description = "احترف التسويق عبر منصات التواصل الاجتماعي، تحسين محركات البحث (SEO)، وإدارة الحملات الإعلانية.",
                    RegistrarName = "أ. سارة خالد",
                    RegistrarWhatsApp = "201111111111",
                    RegistrationFormUrl = "https://forms.google.com/...",
                    StartDate = DateTime.Now.AddDays(15),
                    Status = CourseStatus.OpenForRegistration
                },
                new Course
                {
                    Id = 3,
                    Title = "مستوى متقدم في تحليل البيانات",
                    Description = "استخدم Python و PowerBI لتحليل البيانات المعقدة واستخراج رؤى قابلة للتنفيذ للشركات.",
                    RegistrarName = "م. عمر طارق",
                    RegistrarWhatsApp = "201222222222",
                    RegistrationFormUrl = "https://forms.google.com/...",
                    StartDate = DateTime.Now.AddDays(5),
                    Status = CourseStatus.OpenForRegistration
                }
            };

            return View(openCourses);
        }
    }
}