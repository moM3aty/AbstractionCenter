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
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFileUploaderService _fileUploader; // لحفظ غلاف الدورة

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IFileUploaderService fileUploader)
        {
            _context = context;
            _userManager = userManager;
            _fileUploader = fileUploader;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "لوحة تحكم الإدارة";
            ViewBag.TotalCourses = await _context.Courses.CountAsync();
            ViewBag.TotalStudents = await _context.StudentCourses.Select(sc => sc.StudentId).Distinct().CountAsync();
            ViewBag.PendingCertificates = await _context.Set<Certificate>().CountAsync(c => !c.IsApproved);

            var instructors = await _userManager.GetRolesAsync(new ApplicationUser()); // مجرد تهيئة مبدئية
            var instCount = await _userManager.GetUsersInRoleAsync("Instructor");
            ViewBag.ActiveInstructors = instCount.Count;

            return View();
        }

        // ----------------- إدارة طلبات المحاضرين -----------------
        public async Task<IActionResult> InstructorApplications()
        {
            ViewData["Title"] = "طلبات انضمام المحاضرين";
            var apps = await _context.InstructorApplications.OrderByDescending(a => a.AppliedAt).ToListAsync();
            return View(apps);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveInstructor(int id)
        {
            var app = await _context.InstructorApplications.FindAsync(id);
            if (app != null && app.Status == RequestStatus.Pending)
            {
                // 1. تغيير حالة الطلب
                app.Status = RequestStatus.Approved;

                // 2. التحقق من عدم وجود حساب مسبق بهذا الإيميل
                var existingUser = await _userManager.FindByEmailAsync(app.Email);
                if (existingUser == null)
                {
                    // إنشاء حساب جديد للمحاضر تلقائياً
                    var newUser = new ApplicationUser
                    {
                        UserName = app.Email,
                        Email = app.Email,
                        FullName = app.FullName,
                        PhoneNumber = app.PhoneNumber,
                        Specialization = app.Specialization,
                        ProfilePicture = app.ProfilePicturePath,
                        EmailConfirmed = true
                    };

                    // كلمة مرور افتراضية (يمكن إرسالها بالإيميل لاحقاً)
                    var result = await _userManager.CreateAsync(newUser, "Instructor@123");
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(newUser, "Instructor");
                    }
                }
                else
                {
                    // إذا كان مسجل مسبقاً (كطالب مثلاً)، نرقيه لمحاضر
                    await _userManager.AddToRoleAsync(existingUser, "Instructor");
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم قبول المحاضر وإنشاء حسابه بنجاح!";
            }
            return RedirectToAction(nameof(InstructorApplications));
        }

        [HttpPost]
        public async Task<IActionResult> RejectInstructor(int id)
        {
            var app = await _context.InstructorApplications.FindAsync(id);
            if (app != null)
            {
                app.Status = RequestStatus.Rejected;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(InstructorApplications));
        }

        // ----------------- إدارة الدورات (تم تحديثها لدعم الرفع) -----------------
        public async Task<IActionResult> ManageCourses()
        {
            var courses = await _context.Courses.OrderByDescending(c => c.CreatedAt).ToListAsync();
            return View(courses);
        }

        [HttpGet]
        public async Task<IActionResult> CreateCourse()
        {
            var instructors = await _userManager.GetUsersInRoleAsync("Instructor");
            ViewBag.Instructors = instructors;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCourse(Course course)
        {
            // رفع الصورة الفعلية
            if (course.ImageFile != null)
            {
                course.ImageUrl = await _fileUploader.UploadFileAsync(course.ImageFile, "courses");
            }
            else
            {
                // صورة افتراضية في حالة عدم رفع صورة
                course.ImageUrl = "https://via.placeholder.com/800x600?text=No+Cover";
            }

            var instructor = await _userManager.FindByIdAsync(course.RegistrarUserId);
            if (instructor != null)
            {
                course.RegistrarName = instructor.FullName ?? instructor.Email;
                course.RegistrarWhatsApp = instructor.PhoneNumber ?? "غير متوفر";
            }

            course.CreatedAt = System.DateTime.Now;
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "تم إنشاء الدورة ونشرها بنجاح.";
            return RedirectToAction(nameof(ManageCourses));
        }

        // --- الدوال الجديدة الخاصة بتعديل الدورة ---
        [HttpGet]
        public async Task<IActionResult> EditCourse(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();

            // جلب المحاضرين لملء الـ Dropdown
            var instructors = await _userManager.GetUsersInRoleAsync("Instructor");
            ViewBag.Instructors = instructors;

            return View(course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCourse(int id, Course updatedCourse, IFormFile? ImageFile)
        {
            if (id != updatedCourse.Id) return NotFound();

            var existingCourse = await _context.Courses.FindAsync(id);
            if (existingCourse == null) return NotFound();

            // تحديث البيانات الأساسية
            existingCourse.Title = updatedCourse.Title;
            existingCourse.Description = updatedCourse.Description;
            existingCourse.StartDate = updatedCourse.StartDate;
            existingCourse.Status = updatedCourse.Status;

            // تحديث الصورة فقط إذا تم رفع صورة جديدة
            if (ImageFile != null)
            {
                existingCourse.ImageUrl = await _fileUploader.UploadFileAsync(ImageFile, "courses");
            }

            // تحديث بيانات المحاضر إذا تم تغييره
            if (existingCourse.RegistrarUserId != updatedCourse.RegistrarUserId)
            {
                existingCourse.RegistrarUserId = updatedCourse.RegistrarUserId;
                var instructor = await _userManager.FindByIdAsync(updatedCourse.RegistrarUserId);
                if (instructor != null)
                {
                    existingCourse.RegistrarName = instructor.FullName ?? instructor.Email;
                    existingCourse.RegistrarWhatsApp = instructor.PhoneNumber ?? "غير متوفر";
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "تم حفظ التعديلات على الدورة بنجاح.";
            return RedirectToAction(nameof(ManageCourses));
        }

        // =========================================================================
        // --- دوال إدارة المحتوى المتقدمة للإدارة (Admin) ---
        // =========================================================================

        [HttpGet]
        public async Task<IActionResult> ManageCourseDetails(int id)
        {
            // الإدارة تستطيع الدخول لأي دورة دون التحقق من الـ RegistrarUserId
            var course = await _context.Courses
                .Include(c => c.Lessons)
                .Include(c => c.CustomQuestions)
                .Include(c => c.FinalExam)
                    .ThenInclude(e => e.Questions)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null) return NotFound();

            // نستخدم نفس View المحاضر لمنع تكرار الأكواد (DRY)
            return View("~/Views/Instructor/ManageCourse.cshtml", course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddLessonModule(int courseId, string title, int order)
        {
            _context.Lessons.Add(new Lesson { CourseId = courseId, Title = title, Order = order });
            await _context.SaveChangesAsync();
            return RedirectToAction("ManageCourseDetails", new { id = courseId });
        }

        [HttpGet]
        public async Task<IActionResult> ManageLessonDetails(int id)
        {
            var lesson = await _context.Lessons
                .Include(l => l.Course)
                .Include(l => l.Contents)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lesson == null) return NotFound();
            return View("~/Views/Instructor/ManageLessonDetails.cshtml", lesson);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddLessonContent(int lessonId, string title, ContentType type, string? videoUrl, string? description, int order, IFormFile? uploadedFile)
        {
            string filePath = null;
            if (uploadedFile != null && type == ContentType.Pdf)
                filePath = await _fileUploader.UploadFileAsync(uploadedFile, "lessons_content");

            _context.LessonContents.Add(new LessonContent { LessonId = lessonId, Title = title, Type = type, VideoUrl = videoUrl, Description = description, FilePath = filePath, Order = order });
            await _context.SaveChangesAsync();
            return RedirectToAction("ManageLessonDetails", new { id = lessonId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCourseQuestion(int courseId, string questionText, bool isRequired, int order)
        {
            _context.CourseQuestions.Add(new CourseQuestion { CourseId = courseId, QuestionText = questionText, IsRequired = isRequired, Order = order });
            await _context.SaveChangesAsync();
            return RedirectToAction("ManageCourseDetails", new { id = courseId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCourseQuestion(int id)
        {
            var question = await _context.CourseQuestions.FindAsync(id);
            if (question != null) { _context.CourseQuestions.Remove(question); await _context.SaveChangesAsync(); return Ok(); }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> ReorderCourseQuestion(int id, string direction)
        {
            var question = await _context.CourseQuestions.FindAsync(id);
            if (question == null) return NotFound();

            var courseQuestions = await _context.CourseQuestions.Where(q => q.CourseId == question.CourseId).OrderBy(q => q.Order).ToListAsync();
            int index = courseQuestions.FindIndex(q => q.Id == id);

            if (direction == "up" && index > 0) { var prev = courseQuestions[index - 1]; int temp = prev.Order; prev.Order = question.Order; question.Order = temp; }
            else if (direction == "down" && index < courseQuestions.Count - 1) { var next = courseQuestions[index + 1]; int temp = next.Order; next.Order = question.Order; question.Order = temp; }

            await _context.SaveChangesAsync();
            return RedirectToAction("ManageCourseDetails", new { id = question.CourseId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveExamSettings(int courseId, double passingScorePercentage)
        {
            var exam = await _context.FinalExams.FirstOrDefaultAsync(e => e.CourseId == courseId);
            if (exam == null) _context.FinalExams.Add(new FinalExam { CourseId = courseId, PassingScorePercentage = passingScorePercentage });
            else exam.PassingScorePercentage = passingScorePercentage;
            await _context.SaveChangesAsync();
            return RedirectToAction("ManageCourseDetails", new { id = courseId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddExamQuestion(int courseId, string questionText, string option1, string option2, string option3, string option4, int correctOption)
        {
            var exam = await _context.FinalExams.FirstOrDefaultAsync(e => e.CourseId == courseId);
            if (exam == null) { exam = new FinalExam { CourseId = courseId, PassingScorePercentage = 70 }; _context.FinalExams.Add(exam); await _context.SaveChangesAsync(); }
            _context.ExamQuestions.Add(new ExamQuestion { FinalExamId = exam.Id, QuestionText = questionText, Option1 = option1, Option2 = option2, Option3 = option3, Option4 = option4, CorrectOption = correctOption });
            await _context.SaveChangesAsync();
            return RedirectToAction("ManageCourseDetails", new { id = courseId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteExamQuestion(int id)
        {
            var eq = await _context.ExamQuestions.FindAsync(id);
            if (eq != null) { _context.ExamQuestions.Remove(eq); await _context.SaveChangesAsync(); return Ok(); }
            return NotFound();
        }

        // ----------------- إدارة المستخدمين -----------------
        public async Task<IActionResult> ManageUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeUserRole(string userId, string newRole)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, newRole);
            }
            return RedirectToAction("ManageUsers");
        }

        // ----------------- إدارة طلبات التسجيل والشهادات -----------------
        public async Task<IActionResult> RegistrationRequests()
        {
            var requests = await _context.RegistrationRequests
                .Include(r => r.Student)
                .Include(r => r.Course)
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();
            return View(requests);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveRequest(int requestId)
        {
            var request = await _context.RegistrationRequests.FindAsync(requestId);
            if (request != null && request.Status == RequestStatus.Pending)
            {
                request.Status = RequestStatus.Approved;
                var studentCourse = new StudentCourse
                {
                    StudentId = request.StudentId,
                    CourseId = request.CourseId,
                    Status = EnrollmentStatus.InProgress
                };
                _context.StudentCourses.Add(studentCourse);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("RegistrationRequests");
        }

        public async Task<IActionResult> Certificates()
        {
            var certificates = await _context.Certificates
                .Include(c => c.Student)
                .Include(c => c.Course)
                .OrderByDescending(c => c.IssueDate)
                .ToListAsync();
            return View(certificates);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveCertificate(int certificateId)
        {
            var cert = await _context.Certificates.FindAsync(certificateId);
            if (cert != null)
            {
                cert.IsApproved = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Certificates");
        }
    }
}