using AbstractionCenter.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AbstractionCenter.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // الصفحة الرئيسية
        public IActionResult Index()
        {
            ViewData["Title"] = "الرئيسية";
            return View();
        }

        // من نحن
        public IActionResult About()
        {
            ViewData["Title"] = "من نحن";
            return View();
        }

        // المسارات التدريبية
        public IActionResult Tracks()
        {
            ViewData["Title"] = "المسارات التدريبية";
            return View();
        }

        // هيئة التدريس
        public async Task<IActionResult> Staff([FromServices] UserManager<ApplicationUser> userManager)
        {
            ViewData["Title"] = "هيئة التدريس";
            // جلب المحاضرين لعرضهم للزوار
            var instructors = await userManager.GetUsersInRoleAsync("Instructor");
            return View(instructors);
        }

        // تواصل معنا
        public IActionResult Contact()
        {
            ViewData["Title"] = "تواصل معنا";
            return View();
        }

        // صفحة التحقق من الشهادات
        [HttpGet]
        public IActionResult VerifyCertificate(string serialNumber)
        {
            ViewData["Title"] = "التحقق من الشهادة";
            // هنا في التطبيق الفعلي يمكنك البحث عن الشهادة في قاعدة البيانات وتمريرها للـ View
            ViewBag.SerialNumber = serialNumber;
            return View();
        }
    }
}