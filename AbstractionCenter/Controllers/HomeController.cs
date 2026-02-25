using AbstractionCenter.Data;
using AbstractionCenter.Models.Entities;
using AbstractionCenter.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace AbstractionCenter.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IFileUploaderService _fileUploader;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, IFileUploaderService fileUploader)
        {
            _logger = logger;
            _context = context;
            _fileUploader = fileUploader;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "الرئيسية";
            return View();
        }

        public IActionResult About()
        {
            ViewData["Title"] = "من نحن";
            return View();
        }

        public IActionResult Tracks()
        {
            ViewData["Title"] = "المسارات التدريبية";
            return View();
        }

        public async Task<IActionResult> Staff([FromServices] UserManager<ApplicationUser> userManager)
        {
            ViewData["Title"] = "هيئة التدريس";
            var instructors = await userManager.GetUsersInRoleAsync("Instructor");
            return View(instructors);
        }

        public IActionResult Contact()
        {
            ViewData["Title"] = "تواصل معنا";
            return View();
        }

        [HttpGet]
        public IActionResult VerifyCertificate(string serialNumber)
        {
            ViewData["Title"] = "التحقق من الشهادة";
            ViewBag.SerialNumber = serialNumber;
            return View();
        }

        // --- الميزة الجديدة: فورم تسجيل المحاضرين ---
        [HttpGet]
        public IActionResult JoinAsInstructor()
        {
            ViewData["Title"] = "انضم كمدرب / محاضر";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> JoinAsInstructor(InstructorApplication application)
        {
            if (ModelState.IsValid)
            {
                // رفع السيرة الذاتية (CV)
                if (application.CVFile != null)
                {
                    application.CVPath = await _fileUploader.UploadFileAsync(application.CVFile, "cvs");
                }

                // رفع الصورة الشخصية
                if (application.ProfilePictureFile != null)
                {
                    application.ProfilePicturePath = await _fileUploader.UploadFileAsync(application.ProfilePictureFile, "profiles");
                }

                application.AppliedAt = System.DateTime.Now;
                application.Status = RequestStatus.Pending;

                _context.InstructorApplications.Add(application);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "تم إرسال طلبك بنجاح. سيتم مراجعة بياناتك والتواصل معك قريباً.";
                return RedirectToAction(nameof(Index));
            }

            return View(application);
        }
    }
}