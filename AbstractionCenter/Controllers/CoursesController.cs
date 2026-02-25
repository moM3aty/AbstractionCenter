using Microsoft.AspNetCore.Mvc;
using AbstractionCenter.Models.Entities;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using AbstractionCenter.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

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

        // استقبال طلب التسجيل الداخلي
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SubmitRegistration(int courseId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // منع الطالب من تقديم طلب لدورة هو مسجل فيها بالفعل
            var isAlreadyEnrolled = await _context.StudentCourses
                .AnyAsync(sc => sc.CourseId == courseId && sc.StudentId == user.Id);

            if (isAlreadyEnrolled)
            {
                TempData["InfoMessage"] = "أنت مسجل بالفعل في هذه الدورة ويمكنك الوصول إليها عبر لوحة التحكم الخاصة بك.";
                return RedirectToAction("Open");
            }

            // التحقق من عدم وجود طلب مسبق قيد المراجعة
            var existingRequest = await _context.Set<RegistrationRequest>()
                .FirstOrDefaultAsync(r => r.CourseId == courseId && r.StudentId == user.Id);

            if (existingRequest == null)
            {
                var newRequest = new RegistrationRequest
                {
                    CourseId = courseId,
                    StudentId = user.Id,
                    Message = "أرغب في الانضمام إلى هذه الدورة."
                };
                _context.Set<RegistrationRequest>().Add(newRequest);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "تم إرسال طلب التسجيل بنجاح! سيتم مراجعته وإضافتك للدورة قريباً.";
            }
            else
            {
                TempData["InfoMessage"] = "لديك طلب مسبق قيد المراجعة لهذه الدورة.";
            }

            return RedirectToAction("Open");
        }
    }
}