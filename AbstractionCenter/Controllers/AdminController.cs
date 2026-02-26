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
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "لوحة تحكم الإدارة العليا";
            ViewBag.TotalCourses = await _context.Courses.CountAsync();
            ViewBag.TotalBatches = await _context.Batches.CountAsync();
            ViewBag.TotalStudents = await _context.StudentBatches.Select(sb => sb.StudentId).Distinct().CountAsync();
            ViewBag.PendingCertificates = await _context.Certificates.CountAsync(c => !c.IsApproved);

            var instCount = await _userManager.GetUsersInRoleAsync("Instructor");
            ViewBag.ActiveInstructors = instCount.Count(i => i.IsActive);

            return View();
        }
        [HttpGet]
        public async Task<IActionResult> EditStudent(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return NotFound();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            return View(user);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStudent(ApplicationUser model, string? NewPassword, string? ConfirmPassword)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();

            // 1. تحديث البيانات الأساسية
            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            user.Specialization = model.Specialization;

            var updateResult = await _userManager.UpdateAsync(user);

            // 2. تحديث كلمة المرور إذا تم إدخالها
            if (updateResult.Succeeded && !string.IsNullOrEmpty(NewPassword))
            {
                if (NewPassword == ConfirmPassword)
                {
                    await _userManager.RemovePasswordAsync(user);
                    await _userManager.AddPasswordAsync(user, NewPassword);
                }
                else
                {
                    ModelState.AddModelError("", "كلمات المرور غير متطابقة.");
                    return View(user);
                }
            }

            if (updateResult.Succeeded)
            {
                TempData["SuccessMessage"] = $"تم تحديث بيانات {user.FullName} بنجاح.";
                return RedirectToAction("ManageUsers");
            }

            return View(user);
        }
        [HttpGet]
        public async Task<IActionResult> ManageInstructors()
        {
            // جلب جميع المستخدمين الذين يمتلكون صلاحية "محاضر"
            var instructors = await _userManager.GetUsersInRoleAsync("Instructor");
            return View(instructors);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteInstructor(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // فحص ما إذا كان المحاضر مرتبطاً بدفعات تدريبية لمنع أخطاء قاعدة البيانات
            bool hasBatches = await _context.Batches.AnyAsync(b => b.InstructorId == id);
            if (hasBatches)
            {
                TempData["ErrorMessage"] = $"لا يمكن حذف المحاضر ({user.FullName}) لوجود دفعات دراسية مسندة إليه. يرجى نقل الدفعات لمحاضر آخر أو تعطيل حسابه بدلاً من الحذف.";
                return RedirectToAction("ManageInstructors");
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"تم حذف المحاضر ({user.FullName}) بنجاح.";
            }
            else
            {
                TempData["ErrorMessage"] = "حدث خطأ أثناء محاولة الحذف.";
            }

            return RedirectToAction("ManageInstructors");
        }
        // 1. إدارة المستخدمين (الطلاب والمدربين)
        // ==========================================
        public async Task<IActionResult> ManageUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleUserStatus(string userId, bool isActive)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.IsActive = isActive;
                await _userManager.UpdateAsync(user);
                TempData["SuccessMessage"] = isActive ? "تم تفعيل الحساب بنجاح." : "تم إيقاف الحساب بنجاح.";
            }
            return RedirectToAction("ManageUsers");
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
                TempData["SuccessMessage"] = "تم تغيير الصلاحية بنجاح.";
            }
            return RedirectToAction("ManageUsers");
        }

        [HttpGet]
        public IActionResult CreateStudent()
        {
            ViewData["Title"] = "إنشاء حساب طالب جديد";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStudent(string fullName, string email, string password, string phoneNumber)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = fullName,
                    PhoneNumber = phoneNumber,
                    EmailConfirmed = true,
                    IsActive = true
                };

                var result = await _userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Student");
                    TempData["SuccessMessage"] = $"تم إنشاء حساب الطالب ({fullName}) بنجاح.";
                    return RedirectToAction("ManageUsers");
                }
                foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
            }
            ViewData["Title"] = "إنشاء حساب طالب جديد";
            return View();
        }

        // ==========================================
        // 2. إدارة الدورات والدفعات
        // ==========================================
        public async Task<IActionResult> ManageCourses()
        {
            var courses = await _context.Courses.Include(c => c.Batches).OrderByDescending(c => c.CreatedAt).ToListAsync();
            return View(courses);
        }

        [HttpGet]
        public IActionResult CreateCourse()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCourse(Course course)
        {
            if (ModelState.IsValid)
            {
                _context.Courses.Add(course);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم إنشاء الدورة بنجاح.";
                return RedirectToAction("ManageCourses");
            }
            return View(course);
        }

        [HttpGet]
        public async Task<IActionResult> EditCourse(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();
            return View(course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCourse(Course course)
        {
            if (ModelState.IsValid)
            {
                var existingCourse = await _context.Courses.FindAsync(course.Id);
                if (existingCourse != null)
                {
                    existingCourse.Title = course.Title;
                    existingCourse.Description = course.Description;
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "تم تحديث الدورة بنجاح.";
                    return RedirectToAction("ManageCourses");
                }
            }
            return View(course);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var course = await _context.Courses.Include(c => c.Batches).FirstOrDefaultAsync(c => c.Id == id);
            if (course == null) return NotFound();

            if (course.Batches != null && course.Batches.Any())
            {
                TempData["ErrorMessage"] = $"لا يمكن حذف الدورة ({course.Title}) لوجود دفعات دراسية مسجلة بها. يرجى حذف الدفعات أولاً.";
                return RedirectToAction(nameof(ManageCourses));
            }

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"تم حذف الدورة ({course.Title}) بنجاح.";
            return RedirectToAction(nameof(ManageCourses));
        }

        public async Task<IActionResult> ManageBatches(int courseId)
        {
            var course = await _context.Courses.Include(c => c.Batches).ThenInclude(b => b.Instructor).FirstOrDefaultAsync(c => c.Id == courseId);
            if (course == null) return NotFound();

            var instructors = await _userManager.GetUsersInRoleAsync("Instructor");
            ViewBag.Instructors = instructors.Where(i => i.IsActive).ToList();

            return View(course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBatch(Batch batch)
        {
            ModelState.Remove("Course");
            ModelState.Remove("Instructor");
            if (ModelState.IsValid)
            {
                _context.Batches.Add(batch);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم فتح الدفعة الجديدة بنجاح.";
            }
            return RedirectToAction("ManageBatches", new { courseId = batch.CourseId });
        }

        // ==========================================
        // 3. طلبات التوظيف (للمحاضرين)
        // ==========================================
        public async Task<IActionResult> InstructorApplications()
        {
            var apps = await _context.InstructorApplications.OrderByDescending(a => a.AppliedAt).ToListAsync();
            return View(apps);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveInstructor(int id)
        {
            var app = await _context.InstructorApplications.FindAsync(id);
            if (app != null && app.Status == RequestStatus.Pending)
            {
                app.Status = RequestStatus.Approved;
                var existingUser = await _userManager.FindByEmailAsync(app.Email);
                if (existingUser == null)
                {
                    var newUser = new ApplicationUser { UserName = app.Email, Email = app.Email, FullName = app.FullName, PhoneNumber = app.PhoneNumber, Specialization = app.Specialization, ProfilePicture = app.ProfilePicturePath, EmailConfirmed = true, IsActive = true };
                    var result = await _userManager.CreateAsync(newUser, "Instructor@123");
                    if (result.Succeeded) await _userManager.AddToRoleAsync(newUser, "Instructor");
                }
                else
                {
                    existingUser.IsActive = true; await _userManager.AddToRoleAsync(existingUser, "Instructor");
                }
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم تفعيل حساب المحاضر بنجاح.";
            }
            return RedirectToAction(nameof(InstructorApplications));
        }

        [HttpPost]
        public async Task<IActionResult> RejectInstructor(int id)
        {
            var app = await _context.InstructorApplications.FindAsync(id);
            if (app != null) { app.Status = RequestStatus.Rejected; await _context.SaveChangesAsync(); }
            return RedirectToAction(nameof(InstructorApplications));
        }

        // ==========================================
        // 4. طلبات تسجيل الطلاب
        // ==========================================
        public async Task<IActionResult> RegistrationRequests()
        {
            var requests = await _context.RegistrationRequests
                .Include(r => r.Student)
                .Include(r => r.Batch).ThenInclude(b => b.Course)
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();
            return View(requests);
        }

        public async Task<IActionResult> RegistrationDetails(int id)
        {
            var request = await _context.RegistrationRequests
                .Include(r => r.Student)
                .Include(r => r.Batch).ThenInclude(b => b.Course)
                .Include(r => r.Answers).ThenInclude(a => a.CourseQuestion)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null) return NotFound();
            return View(request);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveRequest(int requestId)
        {
            var request = await _context.RegistrationRequests.Include(r => r.Batch).FirstOrDefaultAsync(r => r.Id == requestId);
            if (request != null && request.Status == RequestStatus.Pending)
            {
                request.Status = RequestStatus.Approved;

                var studentBatch = new StudentBatch { StudentId = request.StudentId, BatchId = request.BatchId, Status = StudentAcademicStatus.Registered };
                _context.StudentBatches.Add(studentBatch);

                // --- إرسال تنبيه للطالب بقبوله في الدفعة ---
                _context.Notifications.Add(new Notification
                {
                    UserId = request.StudentId,
                    Title = "تم قبول انضمامك",
                    Message = $"تم الموافقة على طلب تسجيلك في الدفعة: {request.Batch.BatchName}. يرجى الدخول للقاعة الافتراضية للبدء."
                });

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم قبول الطالب وإضافته للدفعة.";
            }
            return RedirectToAction("RegistrationRequests");
        }

        [HttpPost]
        public async Task<IActionResult> RejectRequest(int requestId)
        {
            var request = await _context.RegistrationRequests.FindAsync(requestId);
            if (request != null) { request.Status = RequestStatus.Rejected; await _context.SaveChangesAsync(); }
            return RedirectToAction("RegistrationRequests");
        }

        // ==========================================
        // 5. اعتماد الشهادات
        // ==========================================
        public async Task<IActionResult> Certificates()
        {
            var certificates = await _context.Certificates
                .Include(c => c.Student)
                .Include(c => c.Batch).ThenInclude(b => b.Course)
                .OrderByDescending(c => c.IssueDate)
                .ToListAsync();
            return View(certificates);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveCertificate(int certificateId)
        {
            var cert = await _context.Certificates.Include(c => c.Batch).ThenInclude(b => b.Course).FirstOrDefaultAsync(c => c.Id == certificateId);
            if (cert != null)
            {
                cert.IsApproved = true;

                // --- إرسال تنبيه للطالب باعتماد الشهادة ليتمكن من طباعتها ---
                _context.Notifications.Add(new Notification
                {
                    UserId = cert.StudentId,
                    Title = "اعتماد الشهادة",
                    Message = $"تهانينا! لقد تم اعتماد شهادتك رسمياً من الإدارة لدورة '{cert.Batch.Course.Title}'. يمكنك تحميلها وطباعتها الآن من لوحة التحكم."
                });

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم اعتماد الشهادة بنجاح.";
            }
            return RedirectToAction("Certificates");
        }
    }
}