using Microsoft.AspNetCore.Mvc;
using AbstractionCenter.Models.Entities;
using AbstractionCenter.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using AbstractionCenter.Services;
using System;

namespace AbstractionCenter.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileUploaderService _fileUploader;

        public HomeController(ApplicationDbContext context, IFileUploaderService fileUploader)
        {
            _context = context;
            _fileUploader = fileUploader;
        }

        public IActionResult Index()
        {
            return View();
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
            // جلب المدربين النشطين فقط
            var instructors = await _context.Users
                .Where(u => u.IsActive)
                .ToListAsync();

            // تصفية إضافية عبر الـ Role إذا لزم الأمر، لكننا نعتمد على IsActive حالياً
            // var instructorRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Instructor");

            return View(instructors);
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

                if (certificate != null)
                {
                    ViewBag.Certificate = certificate;
                    ViewBag.IsValid = true;
                }
                else
                {
                    ViewBag.IsValid = false;
                }
            }
            return View();
        }
    }
}