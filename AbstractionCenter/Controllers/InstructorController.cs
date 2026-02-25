using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AbstractionCenter.Data;
using AbstractionCenter.Models.Entities;
using AbstractionCenter.Services;
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
        private readonly IFileUploaderService _fileUploader;

        public InstructorController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IFileUploaderService fileUploader)
        {
            _context = context;
            _userManager = userManager;
            _fileUploader = fileUploader;
        }

        public async Task<IActionResult> Dashboard()
        {
            ViewData["Title"] = "لوحة تحكم المحاضر";
            var user = await _userManager.GetUserAsync(User);
            var myCourses = await _context.Courses.Where(c => c.RegistrarUserId == user.Id).OrderByDescending(c => c.CreatedAt).ToListAsync();
            ViewData["InstructorName"] = user.FullName ?? user.Email;
            return View(myCourses);
        }

        [HttpGet]
        public async Task<IActionResult> ManageCourse(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            // التعديل هنا: قمنا بإضافة FinalExam.Questions لكي تظهر أسئلة الاختبار في الشاشة!
            var course = await _context.Courses
                .Include(c => c.Lessons)
                .Include(c => c.CustomQuestions)
                .Include(c => c.FinalExam)
                    .ThenInclude(e => e.Questions)
                .FirstOrDefaultAsync(c => c.Id == id && c.RegistrarUserId == user.Id);

            if (course == null) return RedirectToAction("AccessDenied", "Account");

            return View(course);
        }


[HttpPost]
public async Task<IActionResult> ReorderCourseQuestion(int id, string direction)
        {
            var question = await _context.CourseQuestions.FindAsync(id);
            if (question == null) return NotFound();

            var courseQuestions = await _context.CourseQuestions
                .Where(q => q.CourseId == question.CourseId)
                .OrderBy(q => q.Order)
                .ToListAsync();

            int index = courseQuestions.FindIndex(q => q.Id == id);

            if (direction == "up" && index > 0)
            {
                // التبديل مع السؤال الذي قبله
                var prev = courseQuestions[index - 1];
                int temp = prev.Order;
                prev.Order = question.Order;
                question.Order = temp;
            }
            else if (direction == "down" && index < courseQuestions.Count - 1)
            {
                // التبديل مع السؤال الذي بعده
                var next = courseQuestions[index + 1];
                int temp = next.Order;
                next.Order = question.Order;
                question.Order = temp;
            }

            await _context.SaveChangesAsync();
            // إعادة التوجيه للصفحة ليقوم الـ AJAX بسحب الترتيب الجديد
            return RedirectToAction("ManageCourse", new { id = question.CourseId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteExamQuestion(int id)
        {
            var eq = await _context.ExamQuestions.FindAsync(id);
            if (eq != null)
            {
                _context.ExamQuestions.Remove(eq);
                await _context.SaveChangesAsync();
                return Ok();
            }
            return NotFound();
        }

        
        // --- 1. إنشاء وحدة دراسية (Lesson Folder) ---
                [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddLessonModule(int courseId, string title, int order)
        {
            var user = await _userManager.GetUserAsync(User);
            if (!await _context.Courses.AnyAsync(c => c.Id == courseId && c.RegistrarUserId == user.Id)) return Unauthorized();

            _context.Lessons.Add(new Lesson { CourseId = courseId, Title = title, Order = order });
            await _context.SaveChangesAsync();
            return RedirectToAction("ManageCourse", new { id = courseId });
        }

        // --- 2. إدارة محتوى الوحدة من الداخل (الشاشة الجديدة) ---
        [HttpGet]
        public async Task<IActionResult> ManageLessonDetails(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var lesson = await _context.Lessons
                .Include(l => l.Course)
                .Include(l => l.Contents)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lesson == null || lesson.Course.RegistrarUserId != user.Id) return Unauthorized();
            return View(lesson);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddLessonContent(int lessonId, string title, ContentType type, string? videoUrl, string? description, int order, IFormFile? uploadedFile)
        {
            var lesson = await _context.Lessons.FindAsync(lessonId);
            if (lesson == null) return NotFound();

            string filePath = null;
            if (uploadedFile != null && type == ContentType.Pdf)
            {
                filePath = await _fileUploader.UploadFileAsync(uploadedFile, "lessons_content");
            }

            _context.LessonContents.Add(new LessonContent
            {
                LessonId = lessonId,
                Title = title,
                Type = type,
                VideoUrl = videoUrl,
                Description = description,
                FilePath = filePath,
                Order = order
            });

            await _context.SaveChangesAsync();
            return RedirectToAction("ManageLessonDetails", new { id = lessonId });
        }

        // --- 3. إدارة الفورم الديناميكي ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCourseQuestion(int courseId, string questionText, bool isRequired, int order)
        {
            _context.CourseQuestions.Add(new CourseQuestion { CourseId = courseId, QuestionText = questionText, IsRequired = isRequired, Order = order });
            await _context.SaveChangesAsync();
            return RedirectToAction("ManageCourse", new { id = courseId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCourseQuestion(int id)
        {
            var question = await _context.CourseQuestions.FindAsync(id);
            if (question != null)
            {
                _context.CourseQuestions.Remove(question);
                await _context.SaveChangesAsync();
                return Ok();
            }
            return NotFound();
        }

        // --- 4. إعدادات الاختبار النهائي ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveExamSettings(int courseId, double passingScorePercentage)
        {
            var exam = await _context.FinalExams.FirstOrDefaultAsync(e => e.CourseId == courseId);
            if (exam == null)
            {
                _context.FinalExams.Add(new FinalExam { CourseId = courseId, PassingScorePercentage = passingScorePercentage });
            }
            else
            {
                exam.PassingScorePercentage = passingScorePercentage;
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("ManageCourse", new { id = courseId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddExamQuestion(int courseId, string questionText, string option1, string option2, string option3, string option4, int correctOption)
        {
            var exam = await _context.FinalExams.FirstOrDefaultAsync(e => e.CourseId == courseId);
            if (exam == null)
            {
                exam = new FinalExam { CourseId = courseId, PassingScorePercentage = 70 };
                _context.FinalExams.Add(exam);
                await _context.SaveChangesAsync();
            }

            _context.ExamQuestions.Add(new ExamQuestion
            {
                FinalExamId = exam.Id,
                QuestionText = questionText,
                Option1 = option1,
                Option2 = option2,
                Option3 = option3,
                Option4 = option4,
                CorrectOption = correctOption
            });
            await _context.SaveChangesAsync();
            return RedirectToAction("ManageCourse", new { id = courseId });
        }
    }
}