using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AbstractionCenter.Data;
using AbstractionCenter.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Http;
using AbstractionCenter.Services;
using System.Collections.Generic;
using System;

namespace AbstractionCenter.Controllers
{
    /// <summary>
    /// المتحكم الرئيسي لإدارة شؤون المحاضرين، المناهج، التقييمات، والطلاب.
    /// </summary>
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

        // --- 1. لوحة تحكم المحاضر الرئيسية ---
        public async Task<IActionResult> Dashboard()
        {
            ViewData["Title"] = "الأكاديمية - لوحة المحاضر";
            var user = await _userManager.GetUserAsync(User);

            var myBatches = await _context.Batches
                .Include(b => b.Course)
                .Where(b => b.InstructorId == user.Id)
                .OrderByDescending(b => b.StartDate)
                .ToListAsync();

            ViewData["InstructorName"] = user.FullName ?? user.Email;
            return View(myBatches);
        }

        // --- 2. إدارة الدفعة (المنهج، الطلاب، الاختبارات) ---
        [HttpGet]
        public async Task<IActionResult> ManageBatch(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            var batch = await _context.Batches
                .Include(b => b.Course).ThenInclude(c => c.CustomQuestions)
                .Include(b => b.Instructor)
                .Include(b => b.Lessons)
                .Include(b => b.EnrolledStudents).ThenInclude(es => es.Student)
                .Include(b => b.FinalExam).ThenInclude(e => e.Questions)
                .FirstOrDefaultAsync(b => b.Id == id && b.InstructorId == user.Id);

            if (batch == null) return RedirectToAction("AccessDenied", "Account");
            return View(batch);
        }

        // --- 3. إدارة الدروس والوحدات (AJAX Support) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddLessonModule(int batchId, string title, int order)
        {
            var user = await _userManager.GetUserAsync(User);
            if (!await _context.Batches.AnyAsync(b => b.Id == batchId && b.InstructorId == user.Id))
                return Unauthorized();

            var lesson = new Lesson { BatchId = batchId, Title = title, Order = order };
            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync();

            return Json(new { success = true, id = lesson.Id, title = lesson.Title, order = lesson.Order });
        }

        [HttpGet]
        public async Task<IActionResult> ManageLessonDetails(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var lesson = await _context.Lessons
                .Include(l => l.Batch).ThenInclude(b => b.Course)
                .Include(l => l.Contents)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lesson == null || lesson.Batch.InstructorId != user.Id) return Unauthorized();
            return View(lesson);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddLessonContent(int lessonId, string title, ContentType type, string? videoUrl, string? description, string? quizUrl, int order, IFormFile? uploadedFile)
        {
            var lesson = await _context.Lessons.Include(l => l.Batch).FirstOrDefaultAsync(l => l.Id == lessonId);
            var user = await _userManager.GetUserAsync(User);

            if (lesson == null || lesson.Batch.InstructorId != user.Id) return Unauthorized();

            string? filePath = null;
            // معالجة رفع ملفات الـ PDF
            if (type == ContentType.Pdf && uploadedFile != null)
            {
                filePath = await _fileUploader.UploadFileAsync(uploadedFile, "course_materials");
            }

            var content = new LessonContent
            {
                LessonId = lessonId,
                Title = title,
                Type = type,
                VideoUrl = videoUrl,
                Description = description,
                QuizUrl = quizUrl,
                Order = order,
                FilePath = filePath
            };
            _context.LessonContents.Add(content);

            // إرسال إشعارات للطلاب المسجلين بالدفعة
            var studentIds = await _context.StudentBatches
                .Where(sb => sb.BatchId == lesson.BatchId)
                .Select(sb => sb.StudentId)
                .ToListAsync();

            foreach (var sId in studentIds)
            {
                _context.Notifications.Add(new Notification
                {
                    UserId = sId,
                    Title = "محتوى جديد",
                    Message = $"قام المحاضر بإضافة {title} في وحدة {lesson.Title}."
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("ManageLessonDetails", new { id = lessonId });
        }

        // --- 4. تقييم واجبات الطلاب ---
        [HttpGet]
        public async Task<IActionResult> ViewSubmissions(int lessonContentId)
        {
            var submissions = await _context.AssignmentSubmissions
                .Include(s => s.Student)
                .Include(s => s.LessonContent)
                .Where(s => s.LessonContentId == lessonContentId)
                .OrderByDescending(s => s.SubmissionDate)
                .ToListAsync();

            var content = await _context.LessonContents.FindAsync(lessonContentId);
            ViewBag.ContentTitle = content?.Title;
            ViewBag.LessonId = content?.LessonId;

            return View(submissions);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GradeSubmission(int submissionId, double grade, string feedback)
        {
            var sub = await _context.AssignmentSubmissions.Include(s => s.LessonContent).FirstOrDefaultAsync(s => s.Id == submissionId);
            if (sub == null) return NotFound();

            sub.Grade = grade;
            sub.InstructorFeedback = feedback;
            sub.IsGraded = true;

            _context.Notifications.Add(new Notification
            {
                UserId = sub.StudentId,
                Title = "تم تقييم واجبك",
                Message = $"قام المحاضر بتقييم حل الواجب الخاص بك ({sub.LessonContent.Title}). الدرجة: {grade}/100."
            });

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "تم حفظ التقييم وإرسال إشعار للطالب.";
            return RedirectToAction("ViewSubmissions", new { lessonContentId = sub.LessonContentId });
        }

        // --- 5. إدارة الطلاب داخل الدفعة (AJAX) ---
        [HttpPost]
        public async Task<IActionResult> AddStudentToBatch(int batchId, string studentEmail)
        {
            var user = await _userManager.FindByEmailAsync(studentEmail);
            if (user == null)
                return Json(new { success = false, message = "هذا البريد غير مسجل في المنصة كطالب." });

            var exists = await _context.StudentBatches.AnyAsync(sb => sb.BatchId == batchId && sb.StudentId == user.Id);
            if (exists)
                return Json(new { success = false, message = "هذا الطالب مسجل بالفعل في هذه الدفعة." });

            var enrollment = new StudentBatch
            {
                BatchId = batchId,
                StudentId = user.Id,
                Status = StudentAcademicStatus.Registered
            };
            _context.StudentBatches.Add(enrollment);
            await _context.SaveChangesAsync();

            return Json(new { success = true, fullName = user.FullName, email = user.Email });
        }

        // --- 6. الاختبار النهائي وبنك الأسئلة (AJAX) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveExamSettings(int batchId, double passingScorePercentage)
        {
            var batch = await _context.Batches.Include(b => b.FinalExam).FirstOrDefaultAsync(b => b.Id == batchId);
            if (batch != null)
            {
                if (batch.FinalExam == null)
                {
                    var newExam = new FinalExam { CourseId = batch.CourseId, PassingScorePercentage = passingScorePercentage };
                    _context.FinalExams.Add(newExam);
                    await _context.SaveChangesAsync();
                    batch.FinalExamId = newExam.Id;
                }
                else
                {
                    batch.FinalExam.PassingScorePercentage = passingScorePercentage;
                }
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("ManageBatch", new { id = batchId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddExamQuestion(int batchId, string questionText, string option1, string option2, string option3, string option4, int correctOption)
        {
            var batch = await _context.Batches.Include(b => b.FinalExam).FirstOrDefaultAsync(b => b.Id == batchId);
            if (batch == null) return Json(new { success = false });

            if (batch.FinalExam == null)
            {
                var newExam = new FinalExam { CourseId = batch.CourseId, PassingScorePercentage = 70 };
                _context.FinalExams.Add(newExam);
                await _context.SaveChangesAsync();
                batch.FinalExamId = newExam.Id;
                await _context.SaveChangesAsync();
            }

            var q = new ExamQuestion
            {
                FinalExamId = batch.FinalExamId.Value,
                QuestionText = questionText,
                Option1 = option1,
                Option2 = option2,
                Option3 = option3,
                Option4 = option4,
                CorrectOption = correctOption
            };
            _context.ExamQuestions.Add(q);
            await _context.SaveChangesAsync();

            return Json(new { success = true, id = q.Id, questionText = q.QuestionText, correctOption = q.CorrectOption });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteExamQuestion(int id)
        {
            var eq = await _context.ExamQuestions.FindAsync(id);
            if (eq != null) { _context.ExamQuestions.Remove(eq); await _context.SaveChangesAsync(); return Json(new { success = true }); }
            return Json(new { success = false });
        }

        // --- 7. تخصيص استمارة التسجيل ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCourseQuestion(int courseId, string questionText, bool isRequired, int order)
        {
            var q = new CourseQuestion { CourseId = courseId, QuestionText = questionText, IsRequired = isRequired, Order = order };
            _context.CourseQuestions.Add(q);
            await _context.SaveChangesAsync();

            return Json(new { success = true, id = q.Id, text = q.QuestionText, isRequired = q.IsRequired, order = q.Order });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCourseQuestion(int id)
        {
            var question = await _context.CourseQuestions.FindAsync(id);
            if (question != null) { _context.CourseQuestions.Remove(question); await _context.SaveChangesAsync(); return Json(new { success = true }); }
            return Json(new { success = false });
        }
    }
}