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

        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            ViewData["StudentName"] = user.FullName ?? user.UserName;

            var enrolledCourses = await _context.StudentCourses
                .Include(sc => sc.Course)
                .Where(sc => sc.StudentId == user.Id)
                .OrderByDescending(sc => sc.EnrollmentDate)
                .ToListAsync();

            return View(enrolledCourses);
        }

        public async Task<IActionResult> CourseDetails(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            var isEnrolled = await _context.StudentCourses
                .AnyAsync(sc => sc.CourseId == id && sc.StudentId == user.Id);

            if (!isEnrolled)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            // تم التحديث هنا لجلب الوحدات (Lessons) وما بداخلها من محتويات (Contents)
            var course = await _context.Courses
                .Include(c => c.Lessons)
                    .ThenInclude(l => l.Contents)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null) return NotFound();

            var assignments = await _context.Assignments
                .Where(a => a.CourseId == id)
                .OrderBy(a => a.DueDate)
                .ToListAsync();

            ViewBag.Assignments = assignments;

            return View(course);
        }

        // =========================================================
        // --- نظام الاختبار النهائي للطالب ---
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> TakeExam(int id) // id = courseId
        {
            var user = await _userManager.GetUserAsync(User);
            var isEnrolled = await _context.StudentCourses.AnyAsync(sc => sc.CourseId == id && sc.StudentId == user.Id);
            if (!isEnrolled) return RedirectToAction("AccessDenied", "Account");

            var course = await _context.Courses
                .Include(c => c.FinalExam)
                    .ThenInclude(e => e.Questions)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course?.FinalExam == null || !course.FinalExam.Questions.Any())
            {
                TempData["InfoMessage"] = "الاختبار النهائي غير متاح لهذه الدورة حالياً.";
                return RedirectToAction("CourseDetails", new { id });
            }

            return View(course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitExam(int courseId, System.Collections.Generic.Dictionary<int, int> answers)
        {
            var user = await _userManager.GetUserAsync(User);
            var course = await _context.Courses.Include(c => c.FinalExam).ThenInclude(e => e.Questions).FirstOrDefaultAsync(c => c.Id == courseId);
            if (course == null) return NotFound();

            int correctCount = 0;
            int totalQuestions = course.FinalExam.Questions.Count;

            foreach (var q in course.FinalExam.Questions)
            {
                if (answers.ContainsKey(q.Id) && answers[q.Id] == q.CorrectOption)
                {
                    correctCount++;
                }
            }

            double scorePercentage = ((double)correctCount / totalQuestions) * 100;
            bool passed = scorePercentage >= course.FinalExam.PassingScorePercentage;

            if (passed)
            {
                // 1. تحديث حالة الدورة للطالب إلى مكتملة
                var studentCourse = await _context.StudentCourses.FirstOrDefaultAsync(sc => sc.CourseId == courseId && sc.StudentId == user.Id);
                if (studentCourse != null) studentCourse.Status = EnrollmentStatus.Completed;

                // 2. إصدار الشهادة (بانتظار الاعتماد الإداري)
                var existingCert = await _context.Certificates.FirstOrDefaultAsync(c => c.CourseId == courseId && c.StudentId == user.Id);
                if (existingCert == null)
                {
                    _context.Certificates.Add(new Certificate { CourseId = courseId, StudentId = user.Id, IsApproved = false });
                }
                await _context.SaveChangesAsync();

                TempData["ExamResult"] = $"مبروك! لقد اجتزت الاختبار بنجاح. نتيجتك: {scorePercentage:F1}%. تم إرسال طلب إصدار شهادتك للإدارة.";
                TempData["IsSuccess"] = true;
            }
            else
            {
                TempData["ExamResult"] = $"للأسف، لم تجتز الاختبار. نتيجتك: {scorePercentage:F1}%. درجة النجاح المطلوبة هي {course.FinalExam.PassingScorePercentage}%. راجع المادة العلمية وحاول مرة أخرى.";
                TempData["IsSuccess"] = false;
            }

            return RedirectToAction("CourseDetails", new { id = courseId });
        }
    }
}