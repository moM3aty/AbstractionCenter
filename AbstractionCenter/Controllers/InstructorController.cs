using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AbstractionCenter.Data;
using AbstractionCenter.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;

namespace AbstractionCenter.Controllers
{
    [Authorize(Roles = "Instructor")]
    public class InstructorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public InstructorController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // لوحة تحكم المحاضر - تعرض الدورات المسندة إليه
        public async Task<IActionResult> Dashboard()
        {
            ViewData["Title"] = "لوحة تحكم المحاضر";

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // جلب الدورات التي يديرها هذا المحاضر تحديداً
            var myCourses = await _context.Courses
                .Where(c => c.RegistrarUserId == user.Id)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            // تمرير اسم المحاضر للواجهة
            ViewData["InstructorName"] = user.FullName ?? user.Email;

            return View(myCourses);
        }

        // إدارة دورة معينة (رفع محتوى وإضافة واجبات)
        [HttpGet]
        public async Task<IActionResult> ManageCourse(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            // التحقق أن الدورة تخص هذا المحاضر
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == id && c.RegistrarUserId == user.Id);

            if (course == null) return RedirectToAction("AccessDenied", "Account");

            return View(course);
        }

        // دالة استقبال الواجب الجديد وحفظه في قاعدة البيانات
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAssignment(int courseId, string title, string description, System.DateTime dueDate)
        {
            var user = await _userManager.GetUserAsync(User);

            // التأكد للمرة الثانية كإجراء أمني أن المحاضر يملك الدورة
            var ownsCourse = await _context.Courses.AnyAsync(c => c.Id == courseId && c.RegistrarUserId == user.Id);
            if (!ownsCourse) return Unauthorized();

            var assignment = new Assignment
            {
                CourseId = courseId,
                Title = title,
                Description = description,
                DueDate = dueDate,
                CreatedAt = System.DateTime.Now
            };

            _context.Assignments.Add(assignment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم إضافة الواجب بنجاح وإرساله للطلاب.";

            return RedirectToAction("ManageCourse", new { id = courseId });
        }
    }
}