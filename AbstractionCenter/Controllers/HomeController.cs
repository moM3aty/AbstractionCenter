using Microsoft.AspNetCore.Mvc;

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
        public IActionResult Staff()
        {
            ViewData["Title"] = "هيئة التدريس";
            return View();
        }

        // تواصل معنا
        public IActionResult Contact()
        {
            ViewData["Title"] = "تواصل معنا";
            return View();
        }

        // صفحة التحقق من الشهادات (سنبنيها لاحقاً)
        public IActionResult VerifyCertificate()
        {
            ViewData["Title"] = "التحقق من الشهادة";
            return View();
        }
    }
}