using Microsoft.AspNetCore.Mvc;
using AbstractionCenter.Models.Entities;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using AbstractionCenter.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;

namespace AbstractionCenter.Controllers
{
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CoursesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Open()
        {
            ViewData["Title"] = "الدورات المتاحة للتسجيل";

            var openCourses = await _context.Courses
                .Where(c => c.Status == CourseStatus.OpenForRegistration)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(openCourses);
        }

        // --- الميزة الجديدة: فتح فورم التسجيل الديناميكي للطالب ---
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Register(int id)
        {
            // جلب الدورة والأسئلة الخاصة بها
            var course = await _context.Courses
                .Include(c => c.CustomQuestions)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null || course.Status != CourseStatus.OpenForRegistration)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);

            // منع الطالب من فتح الفورم إذا كان مسجلاً بالفعل
            var isAlreadyEnrolled = await _context.StudentCourses.AnyAsync(sc => sc.CourseId == id && sc.StudentId == user.Id);
            if (isAlreadyEnrolled)
            {
                TempData["InfoMessage"] = "أنت مسجل بالفعل في هذه الدورة.";
                return RedirectToAction("Open");
            }

            var hasPendingRequest = await _context.RegistrationRequests.AnyAsync(r => r.CourseId == id && r.StudentId == user.Id && r.Status == RequestStatus.Pending);
            if (hasPendingRequest)
            {
                TempData["InfoMessage"] = "لديك طلب مسبق قيد المراجعة لهذه الدورة.";
                return RedirectToAction("Open");
            }

            return View(course); // إرسال الدورة (بما فيها الأسئلة) للفورم
        }

        // --- استقبال بيانات التسجيل والإجابات ---
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitRegistration(int courseId, string fullName, string specialization, string level, string whatsAppNumber, string telegramNumber, string message, Dictionary<int, string> customAnswers)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // إنشاء الطلب الأساسي
            var newRequest = new RegistrationRequest
            {
                CourseId = courseId,
                StudentId = user.Id,
                FullName = fullName,
                Specialization = specialization,
                Level = level,
                WhatsAppNumber = whatsAppNumber,
                TelegramNumber = telegramNumber,
                Message = message,
                Status = RequestStatus.Pending,
                RequestDate = System.DateTime.Now
            };

            _context.RegistrationRequests.Add(newRequest);
            await _context.SaveChangesAsync(); // للحصول على Id الطلب لربط الإجابات به

            // حفظ إجابات الأسئلة المخصصة إن وجدت
            if (customAnswers != null && customAnswers.Any())
            {
                foreach (var answer in customAnswers)
                {
                    if (!string.IsNullOrWhiteSpace(answer.Value))
                    {
                        _context.RegistrationAnswers.Add(new RegistrationAnswer
                        {
                            RegistrationRequestId = newRequest.Id,
                            CourseQuestionId = answer.Key,
                            AnswerText = answer.Value
                        });
                    }
                }
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "تم إرسال طلب التسجيل بنجاح! سيتم مراجعة بياناتك وإضافتك للدورة قريباً.";
            return RedirectToAction("Open");
        }
    }
}