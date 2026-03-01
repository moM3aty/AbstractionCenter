using Microsoft.AspNetCore.Mvc;
using AbstractionCenter.Models.Entities;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using AbstractionCenter.Data;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Http;
using AbstractionCenter.Services;

namespace AbstractionCenter.Controllers
{
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFileUploaderService _fileUploader;

        public CoursesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IFileUploaderService fileUploader)
        {
            _context = context;
            _userManager = userManager;
            _fileUploader = fileUploader;
        }

        public async Task<IActionResult> Open()
        {
            var openBatches = await _context.Batches
                .Include(b => b.Course)
                .Where(b => b.Status == BatchStatus.OpenForRegistration)
                .OrderByDescending(b => b.StartDate)
                .ToListAsync();

            return View(openBatches);
        }

        // إزالة [Authorize] ليتمكن الزائر من التسجيل
        [HttpGet]
        public async Task<IActionResult> Register(int id)
        {
            var batch = await _context.Batches
                .Include(b => b.Course).ThenInclude(c => c.CustomQuestions)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (batch == null || batch.Status != BatchStatus.OpenForRegistration)
                return NotFound();

            // --- التحقق من حالة التسجيل العامة ---
            var isRegActiveSetting = await _context.SiteSettings.FirstOrDefaultAsync(s => s.Key == "Registration_IsActive");
            bool isRegistrationActive = isRegActiveSetting == null || isRegActiveSetting.Value == "true";
            ViewBag.IsRegistrationActive = isRegistrationActive;

            if (!isRegistrationActive)
            {
                ViewBag.ClosedMessageAr = (await _context.SiteSettings.FirstOrDefaultAsync(s => s.Key == "Registration_ClosedMessage"))?.Value ?? "عذراً، التسجيل مغلق حالياً.";
                ViewBag.ClosedMessageEn = (await _context.SiteSettings.FirstOrDefaultAsync(s => s.Key == "Registration_ClosedMessage"))?.ValueEn ?? "Registration is currently closed.";

                ViewBag.ContactInfoAr = (await _context.SiteSettings.FirstOrDefaultAsync(s => s.Key == "Registration_ContactInfo"))?.Value ?? "يرجى التواصل مع المُسجل.";
                ViewBag.ContactInfoEn = (await _context.SiteSettings.FirstOrDefaultAsync(s => s.Key == "Registration_ContactInfo"))?.ValueEn ?? "Please contact the registrar.";
            }

            return View(batch);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitRegistration(int batchId, string fullName, string fullNameEn, string email, string specialization, string level, string whatsAppNumber, string telegramNumber, Dictionary<string, string> customAnswers)
        {
            try
            {
                // فحص أمان إضافي
                if (batchId <= 0)
                {
                    TempData["ErrorMessage"] = "حدث خطأ في قراءة بيانات الدفعة. يرجى العودة لصفحة البرامج والمحاولة من جديد.";
                    return RedirectToAction("Open");
                }

                // التحقق مما إذا كان الإيميل مسجل مسبقاً في هذه الدفعة
                var existingRequest = await _context.RegistrationRequests.AnyAsync(r => r.BatchId == batchId && r.Email == email && r.Status != RequestStatus.Rejected);
                if (existingRequest)
                {
                    TempData["ErrorMessage"] = "يوجد طلب تسجيل مسبق بهذا البريد الإلكتروني لهذه الدفعة.";
                    return RedirectToAction("Register", new { id = batchId });
                }

                var newRequest = new RegistrationRequest
                {
                    BatchId = batchId,
                    FullName = fullName,
                    FullNameEn = fullNameEn, // حفظ الاسم الإنجليزي
                    Email = email,
                    Specialization = specialization ?? "غير محدد",
                    Level = level ?? "غير محدد",
                    WhatsAppNumber = whatsAppNumber,
                    TelegramNumber = telegramNumber,
                    Status = RequestStatus.Pending,
                    RequestDate = DateTime.Now
                };

                _context.RegistrationRequests.Add(newRequest);
                await _context.SaveChangesAsync();

                if (customAnswers != null && customAnswers.Any())
                {
                    foreach (var answer in customAnswers)
                    {
                        if (int.TryParse(answer.Key, out int questionId) && !string.IsNullOrWhiteSpace(answer.Value))
                        {
                            _context.RegistrationAnswers.Add(new RegistrationAnswer
                            {
                                RegistrationRequestId = newRequest.Id,
                                CourseQuestionId = questionId,
                                AnswerText = answer.Value
                            });
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                // تحويل الطالب لصفحة الدفع
                return RedirectToAction("Payment", new { requestId = newRequest.Id });
            }
            catch (Exception ex)
            {
                // طباعة الخطأ في واجهة المستخدم بدلاً من صفحة 500
                TempData["ErrorMessage"] = $"حدث خطأ غير متوقع أثناء معالجة الطلب. التفاصيل: {ex.Message}";
                return RedirectToAction("Register", new { id = batchId });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Payment(int requestId)
        {
            var request = await _context.RegistrationRequests
                .Include(r => r.Batch).ThenInclude(b => b.Course)
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null) return NotFound();

            return View(request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitPayment(int requestId, IFormFile receiptFile)
        {
            var request = await _context.RegistrationRequests.FindAsync(requestId);
            if (request == null) return NotFound();

            if (receiptFile != null)
            {
                request.ReceiptFilePath = await _fileUploader.UploadFileAsync(receiptFile, "receipts");
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("PaymentSuccess");
        }

        [HttpGet]
        public IActionResult PaymentSuccess()
        {
            return View();
        }
    }
}