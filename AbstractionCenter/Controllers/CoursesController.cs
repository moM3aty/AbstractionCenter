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
            var openBatches = await _context.Batches
                .Include(b => b.Course)
                .Include(b => b.Instructor)
                .Where(b => b.Status == BatchStatus.OpenForRegistration)
                .OrderByDescending(b => b.StartDate)
                .ToListAsync();

            return View(openBatches);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Register(int id)
        {
            var batch = await _context.Batches
                .Include(b => b.Course).ThenInclude(c => c.CustomQuestions)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (batch == null || batch.Status != BatchStatus.OpenForRegistration)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);

            var isAlreadyEnrolled = await _context.StudentBatches.AnyAsync(sb => sb.BatchId == id && sb.StudentId == user.Id);
            if (isAlreadyEnrolled)
            {
                TempData["InfoMessage"] = "أنت مسجل بالفعل في هذه الدفعة.";
                return RedirectToAction("Open");
            }

            var hasPendingRequest = await _context.RegistrationRequests.AnyAsync(r => r.BatchId == id && r.StudentId == user.Id && r.Status == RequestStatus.Pending);
            if (hasPendingRequest)
            {
                TempData["InfoMessage"] = "لديك طلب مسبق قيد المراجعة لهذه الدفعة.";
                return RedirectToAction("Open");
            }

            return View(batch);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitRegistration(int batchId, string fullName, string specialization, string level, string whatsAppNumber, string telegramNumber, Dictionary<int, string> customAnswers)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var newRequest = new RegistrationRequest
            {
                BatchId = batchId,
                StudentId = user.Id,
                FullName = fullName,
                Specialization = specialization,
                Level = level,
                WhatsAppNumber = whatsAppNumber,
                TelegramNumber = telegramNumber,
                Status = RequestStatus.Pending,
                RequestDate = System.DateTime.Now
            };

            _context.RegistrationRequests.Add(newRequest);
            await _context.SaveChangesAsync();

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

            TempData["SuccessMessage"] = "تم إرسال طلب التسجيل بنجاح! سيتم مراجعة بياناتك وإضافتك للدفعة قريباً.";
            return RedirectToAction("Open");
        }
    }
}