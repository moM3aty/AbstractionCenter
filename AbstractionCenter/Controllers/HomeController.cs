using Microsoft.AspNetCore.Mvc;
using AbstractionCenter.Models.Entities;
using AbstractionCenter.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using AbstractionCenter.Services;
using System;
using Microsoft.AspNetCore.Identity;

namespace AbstractionCenter.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileUploaderService _fileUploader;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ApplicationDbContext context, IFileUploaderService fileUploader, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _fileUploader = fileUploader;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // جلب المستخدمين الذين يمتلكون رتبة "محاضر" فقط، ثم فلترة النشطين منهم
            var instructorsInRole = await _userManager.GetUsersInRoleAsync("Instructor");
            var activeInstructors = instructorsInRole.Where(u => u.IsActive).Take(3).ToList();

            return View(activeInstructors);
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Tracks()
        {
            return View();
        }

        public async Task<IActionResult> Staff()
        {
            // جلب جميع المحاضرين النشطين لصفحة فريق الخبراء
            var instructorsInRole = await _userManager.GetUsersInRoleAsync("Instructor");
            var activeInstructors = instructorsInRole.Where(u => u.IsActive).ToList();

            return View(activeInstructors);
        }

        public IActionResult Contact()
        {
            return View();
        }

        [HttpGet]
        public IActionResult JoinAsInstructor()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> JoinAsInstructor(InstructorApplication application)
        {
            if (ModelState.IsValid)
            {
                if (application.ProfilePictureFile != null)
                {
                    application.ProfilePicturePath = await _fileUploader.UploadFileAsync(application.ProfilePictureFile, "profiles");
                }
                if (application.CVFile != null)
                {
                    application.CVPath = await _fileUploader.UploadFileAsync(application.CVFile, "cvs");
                }

                application.AppliedAt = DateTime.Now;
                application.Status = RequestStatus.Pending;

                _context.InstructorApplications.Add(application);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "تم إرسال طلبك بنجاح! ستقوم الإدارة بمراجعته والتواصل معك قريباً.";
                return RedirectToAction(nameof(JoinAsInstructor));
            }

            return View(application);
        }

        [HttpGet]
        public async Task<IActionResult> VerifyCertificate(string serialNumber)
        {
            ViewBag.SerialNumber = serialNumber;
            if (!string.IsNullOrEmpty(serialNumber))
            {
                var certificate = await _context.Certificates
                    .Include(c => c.Student)
                    .Include(c => c.Batch).ThenInclude(b => b.Course)
                    .FirstOrDefaultAsync(c => c.UniqueSerialNumber == serialNumber && c.IsApproved);

                ViewBag.Certificate = certificate;
                ViewBag.IsValid = certificate != null;
            }
            return View();
        }
    }
}