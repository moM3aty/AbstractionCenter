using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AbstractionCenter.Data;
using AbstractionCenter.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using QRCoder;
using System;
using Microsoft.AspNetCore.Http;
using AbstractionCenter.Services;

namespace AbstractionCenter.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFileUploaderService _fileUploader;

        public StudentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IFileUploaderService fileUploader)
        {
            _context = context;
            _userManager = userManager;
            _fileUploader = fileUploader;
        }

        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            ViewData["StudentName"] = user.FullName ?? user.UserName;

            var enrolledBatches = await _context.StudentBatches
                .Include(sb => sb.Batch).ThenInclude(b => b.Course)
                .Include(sb => sb.Batch).ThenInclude(b => b.Instructor)
                .Where(sb => sb.StudentId == user.Id)
                .OrderByDescending(sb => sb.EnrollmentDate)
                .ToListAsync();

            var myCertificates = await _context.Certificates
                .Where(c => c.StudentId == user.Id && c.IsApproved)
                .ToListAsync();
            ViewBag.MyCertificates = myCertificates;

            var notifications = await _context.Notifications
                .Where(n => n.UserId == user.Id)
                .OrderByDescending(n => n.CreatedAt)
                .Take(5)
                .ToListAsync();
            ViewBag.Notifications = notifications;

            return View(enrolledBatches);
        }

        public async Task<IActionResult> BatchDetails(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            var enrollment = await _context.StudentBatches
                .FirstOrDefaultAsync(sb => sb.BatchId == id && sb.StudentId == user.Id);

            if (enrollment == null) return RedirectToAction("AccessDenied", "Account");

            if (enrollment.Status == StudentAcademicStatus.Registered)
            {
                enrollment.Status = StudentAcademicStatus.Studying;
                await _context.SaveChangesAsync();
            }

            var batch = await _context.Batches
                .Include(b => b.Course)
                .Include(b => b.Instructor)
                .Include(b => b.Lessons).ThenInclude(l => l.Contents)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (batch == null) return NotFound();

            var mySubmissions = await _context.AssignmentSubmissions
                .Where(s => s.StudentId == user.Id && s.LessonContent.Lesson.BatchId == id)
                .ToListAsync();
            ViewBag.MySubmissions = mySubmissions;

            return View(batch);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitAssignment(int lessonContentId, int batchId, IFormFile solutionFile)
        {
            var user = await _userManager.GetUserAsync(User);
            if (solutionFile != null && solutionFile.Length > 0)
            {
                // استخدام خدمة الرفع التي صممناها سابقاً
                string filePath = await _fileUploader.UploadFileAsync(solutionFile, "student_solutions");

                var submission = new AssignmentSubmission
                {
                    StudentId = user.Id,
                    LessonContentId = lessonContentId,
                    FilePath = filePath,
                    SubmissionDate = DateTime.Now
                };

                _context.AssignmentSubmissions.Add(submission);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم تسليم الحل بنجاح.";
            }
            return RedirectToAction("BatchDetails", new { id = batchId });
        }

        [HttpGet]
        public async Task<IActionResult> Certificate(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            var certificate = await _context.Certificates
                .Include(c => c.Student)
                .Include(c => c.Batch).ThenInclude(b => b.Course)
                .Include(c => c.Batch).ThenInclude(b => b.Instructor)
                .FirstOrDefaultAsync(c => c.BatchId == id && c.StudentId == user.Id && c.IsApproved);

            if (certificate == null)
                return RedirectToAction("AccessDenied", "Account");

            string verifyUrl = Url.Action("VerifyCertificate", "Home", new { serialNumber = certificate.UniqueSerialNumber }, Request.Scheme);

            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(verifyUrl, QRCodeGenerator.ECCLevel.Q);
                using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
                {
                    byte[] qrCodeImage = qrCode.GetGraphic(20);
                    ViewBag.QRCodeBase64 = $"data:image/png;base64,{Convert.ToBase64String(qrCodeImage)}";
                }
            }

            return View(certificate);
        }

        [HttpGet]
        public async Task<IActionResult> TakeExam(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var isEnrolled = await _context.StudentBatches.AnyAsync(sb => sb.BatchId == id && sb.StudentId == user.Id);
            if (!isEnrolled) return RedirectToAction("AccessDenied", "Account");

            var batch = await _context.Batches
                .Include(b => b.Course)
                .Include(b => b.FinalExam).ThenInclude(e => e.Questions)
                .Include(b => b.Instructor)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (batch?.FinalExam == null || !batch.FinalExam.Questions.Any())
            {
                TempData["InfoMessage"] = "الاختبار النهائي غير متاح لهذه الدفعة حالياً.";
                return RedirectToAction("BatchDetails", new { id });
            }

            return View(batch);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitExam(int batchId, Dictionary<int, int> answers)
        {
            var user = await _userManager.GetUserAsync(User);
            var batch = await _context.Batches
                .Include(b => b.FinalExam).ThenInclude(e => e.Questions)
                .FirstOrDefaultAsync(b => b.Id == batchId);

            if (batch == null || batch.FinalExam == null) return NotFound();

            int correctCount = 0;
            int totalQuestions = batch.FinalExam.Questions.Count;

            foreach (var q in batch.FinalExam.Questions)
            {
                if (answers.ContainsKey(q.Id) && answers[q.Id] == q.CorrectOption)
                {
                    correctCount++;
                }
            }

            double scorePercentage = ((double)correctCount / totalQuestions) * 100;
            bool passed = scorePercentage >= batch.FinalExam.PassingScorePercentage;

            if (passed)
            {
                var studentBatch = await _context.StudentBatches.FirstOrDefaultAsync(sb => sb.BatchId == batchId && sb.StudentId == user.Id);
                if (studentBatch != null) studentBatch.Status = StudentAcademicStatus.Completed;

                var existingCert = await _context.Certificates.FirstOrDefaultAsync(c => c.BatchId == batchId && c.StudentId == user.Id);
                if (existingCert == null)
                {
                    _context.Certificates.Add(new Certificate { BatchId = batchId, StudentId = user.Id, IsApproved = false });
                }

                _context.Notifications.Add(new Notification
                {
                    UserId = user.Id,
                    Title = "نجاح في الاختبار",
                    Message = $"لقد اجتزت اختبار الدفعة بنجاح بنسبة {scorePercentage:F1}%. شهادتك الآن قيد الاعتماد من الإدارة."
                });

                await _context.SaveChangesAsync();

                TempData["ExamResult"] = $"مبروك! لقد اجتزت الاختبار بنجاح. نتيجتك: {scorePercentage:F1}%. تم إرسال طلب إصدار شهادتك للإدارة.";
                TempData["IsSuccess"] = true;
            }
            else
            {
                _context.Notifications.Add(new Notification
                {
                    UserId = user.Id,
                    Title = "نتيجة الاختبار",
                    Message = $"للأسف، لم تجتز اختبار الدفعة. نتيجتك: {scorePercentage:F1}%. نسبة الاجتياز المطلوبة هي {batch.FinalExam.PassingScorePercentage}%."
                });

                await _context.SaveChangesAsync();

                TempData["ExamResult"] = $"للأسف، لم تجتز الاختبار. نتيجتك: {scorePercentage:F1}%. درجة النجاح المطلوبة هي {batch.FinalExam.PassingScorePercentage}%. راجع المادة العلمية وحاول مرة أخرى.";
                TempData["IsSuccess"] = false;
            }

            return RedirectToAction("BatchDetails", new { id = batchId });
        }

        [HttpGet]
        public async Task<IActionResult> Notifications()
        {
            var user = await _userManager.GetUserAsync(User);
            var notifications = await _context.Notifications
                .Where(n => n.UserId == user.Id)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            foreach (var n in notifications.Where(n => !n.IsRead))
            {
                n.IsRead = true;
            }
            await _context.SaveChangesAsync();

            return View(notifications);
        }

        [HttpPost]
        public async Task<IActionResult> MarkNotificationsAsRead()
        {
            var user = await _userManager.GetUserAsync(User);
            var unreadNotifications = await _context.Notifications
                .Where(n => n.UserId == user.Id && !n.IsRead)
                .ToListAsync();

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}