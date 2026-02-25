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
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public StudentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // لوحة تحكم الطالب (عرض الدورات المسجل بها)
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            ViewData["StudentName"] = user.FullName ?? user.UserName;

            // جلب الدورات المسجل بها الطالب من قاعدة البيانات الفعلية
            var enrolledCourses = await _context.StudentCourses
                .Include(sc => sc.Course)
                .Where(sc => sc.StudentId == user.Id)
                .OrderByDescending(sc => sc.EnrollmentDate)
                .ToListAsync();

            return View(enrolledCourses);
        }

        // شاشة الدخول إلى الدورة وعرض المحتوى والواجبات
        public async Task<IActionResult> CourseDetails(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            // التحقق من أن الطالب مسجل فعلياً في الدورة لمنع الدخول غير المصرح
            var isEnrolled = await _context.StudentCourses
                .AnyAsync(sc => sc.CourseId == id && sc.StudentId == user.Id);

            if (!isEnrolled)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == id);
            if (course == null) return NotFound();

            // جلب الواجبات الخاصة بهذه الدورة
            var assignments = await _context.Assignments
                .Where(a => a.CourseId == id)
                .OrderBy(a => a.DueDate)
                .ToListAsync();

            ViewBag.Assignments = assignments;

            return View(course);
        }
    }
}