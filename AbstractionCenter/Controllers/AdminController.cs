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

        // --- النظرة العامة للوحة القيادة ---
        public async Task<IActionResult> Index()
        {
            var students = await _userManager.GetUsersInRoleAsync("Student");
            var instructors = await _userManager.GetUsersInRoleAsync("Instructor");

            ViewBag.TotalStudents = students.Count;
            ViewBag.ActiveInstructors = instructors.Count(i => i.IsActive);
            ViewBag.TotalCourses = await _context.Courses.CountAsync();
            ViewBag.PendingCertificates = await _context.Certificates.CountAsync(c => !c.IsApproved);

            return View();
        }

        // ==========================================
        // 1. إدارة المحاضرين (الجديدة)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> ManageInstructors()
        {
            var instructors = await _userManager.GetUsersInRoleAsync("Instructor");
            return View(instructors);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteInstructor(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // فحص أمان: منع حذف محاضر يمتلك دفعات لحماية قاعدة البيانات
            bool hasBatches = await _context.Batches.AnyAsync(b => b.InstructorId == id);
            if (hasBatches)
            {
                TempData["ErrorMessage"] = $"لا يمكن حذف المحاضر ({user.FullName}) لوجود دفعات دراسية مسندة إليه. يرجى نقل الدفعات لمحاضر آخر أو تعطيل حسابه.";
                return RedirectToAction(nameof(ManageInstructors));
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"تم حذف المحاضر ({user.FullName}) نهائياً.";
            }
            else
            {
                TempData["ErrorMessage"] = "حدث خطأ أثناء محاولة الحذف.";
            }

            return RedirectToAction(nameof(ManageInstructors));
        }

        // ==========================================
        // 2. إدارة المستخدمين والصلاحيات
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> ManageUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserStatus(string userId, bool isActive)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.IsActive = isActive;
                await _userManager.UpdateAsync(user);
                TempData["SuccessMessage"] = $"تم {(isActive ? "تفعيل" : "تعطيل")} حساب المستخدم بنجاح.";
            }
            return RedirectToAction(nameof(ManageUsers));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeUserRole(string userId, string newRole)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, newRole);
                TempData["SuccessMessage"] = $"تم ترقية/تغيير رتبة المستخدم إلى {newRole}.";
            }
            return RedirectToAction(nameof(ManageUsers));
        }

        [HttpGet]
        public IActionResult CreateStudent() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStudent(string fullName, string phoneNumber, string email, string password)
        {
            var user = new ApplicationUser { UserName = email, Email = email, FullName = fullName, PhoneNumber = phoneNumber, EmailConfirmed = true };
            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Student");
                TempData["SuccessMessage"] = "تم إنشاء حساب الطالب بنجاح.";
                return RedirectToAction(nameof(ManageUsers));
            }

            foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> EditStudent(string userId)
        {
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

            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            user.Specialization = model.Specialization;

            var updateResult = await _userManager.UpdateAsync(user);

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
                return RedirectToAction(nameof(ManageUsers));
            }

            return View(user);
        }

        // ==========================================
        // 3. إدارة الكورسات والدفعات
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> ManageCourses()
        {
            var courses = await _context.Courses.Include(c => c.Batches).OrderByDescending(c => c.CreatedAt).ToListAsync();
            return View(courses);
        }

        [HttpGet]
        public IActionResult CreateCourse() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCourse(Course course)
        {
            if (ModelState.IsValid)
            {
                // يمكن إضافة لوجيك رفع الصورة هنا باستخدام IFileUploaderService
                _context.Courses.Add(course);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم إنشاء المسار التدريبي بنجاح.";
                return RedirectToAction(nameof(ManageCourses));
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
                existingCourse.Title = course.Title;
                existingCourse.Description = course.Description;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم تحديث بيانات المسار بنجاح.";
                return RedirectToAction(nameof(ManageCourses));
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

        [HttpGet]
        public async Task<IActionResult> ManageBatches(int courseId)
        {
            var course = await _context.Courses
                .Include(c => c.Batches).ThenInclude(b => b.Instructor)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null) return NotFound();

            ViewBag.Instructors = await _userManager.GetUsersInRoleAsync("Instructor");
            return View(course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBatch(Batch batch)
        {
            _context.Batches.Add(batch);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "تم فتح الدفعة الجديدة وتعيين المحاضر.";
            return RedirectToAction(nameof(ManageBatches), new { courseId = batch.CourseId });
        }

        [HttpGet]
        public async Task<IActionResult> EditBatch(int id)
        {
            var batch = await _context.Batches.Include(b => b.Course).FirstOrDefaultAsync(b => b.Id == id);
            if (batch == null) return NotFound();

            ViewBag.Instructors = await _userManager.GetUsersInRoleAsync("Instructor");
            return View(batch);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBatch(Batch model, List<string> InstructorIds)
        {
            var batch = await _context.Batches.FindAsync(model.Id);
            if (batch == null) return NotFound();

            // تحديث البيانات الأساسية
            batch.BatchName = model.BatchName;
            batch.StartDate = model.StartDate;
            batch.Status = model.Status;

            // معالجة تعدد المحاضرين
            if (InstructorIds != null && InstructorIds.Any())
            {
                batch.InstructorId = InstructorIds.First(); // المحاضر الأساسي
                if (InstructorIds.Count > 1)
                {
                    // حفظ الباقي كنص مفصول بفاصلة للاستخدام الداخلي
                    batch.AdditionalInstructorIds = string.Join(",", InstructorIds.Skip(1));
                }
                else
                {
                    batch.AdditionalInstructorIds = null;
                }
            }

            // تحديث إعدادات العرض والتسعير
            batch.ExecutionNote = model.ExecutionNote ?? "Delivered by Abstraction Training Team";
            batch.ShowExecutionNote = model.ShowExecutionNote;
            batch.Price = model.Price;
            batch.ShowPrice = model.ShowPrice;
            batch.DiscountPercentage = model.DiscountPercentage;
            batch.ShowDiscount = model.ShowDiscount;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "تم حفظ إعدادات الدفعة، التسعير، والمحاضرين بنجاح.";
            return RedirectToAction(nameof(ManageBatches), new { courseId = batch.CourseId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBatch(int id)
        {
            var batch = await _context.Batches.Include(b => b.EnrolledStudents).FirstOrDefaultAsync(b => b.Id == id);
            if (batch == null) return NotFound();

            // حماية: منع حذف الدفعة إذا كان بها طلاب مسجلين
            if (batch.EnrolledStudents != null && batch.EnrolledStudents.Any())
            {
                TempData["ErrorMessage"] = "لا يمكن حذف هذه الدفعة لوجود طلاب مسجلين بها. يرجى نقل الطلاب لدفعة أخرى أو إلغاء تسجيلهم أولاً.";
                return RedirectToAction(nameof(ManageBatches), new { courseId = batch.CourseId });
            }

            int courseId = batch.CourseId;
            _context.Batches.Remove(batch);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم حذف الدفعة بشكل نهائي بنجاح.";
            return RedirectToAction(nameof(ManageBatches), new { courseId = courseId });
        }

        // ==========================================
        // 4. طلبات التسجيل للطلاب
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> RegistrationRequests()
        {
            var requests = await _context.RegistrationRequests
                .Include(r => r.Student)
                .Include(r => r.Batch).ThenInclude(b => b.Course)
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();
            return View(requests);
        }

        [HttpGet]
        public async Task<IActionResult> RegistrationDetails(int id)
        {
            var req = await _context.RegistrationRequests
                .Include(r => r.Student)
                .Include(r => r.Batch).ThenInclude(b => b.Course)
                .Include(r => r.Answers).ThenInclude(a => a.CourseQuestion)
                .FirstOrDefaultAsync(r => r.Id == id);
            return View(req);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveRequest(int requestId)
        {
            var req = await _context.RegistrationRequests.FindAsync(requestId);
            if (req != null && req.Status == RequestStatus.Pending)
            {
                req.Status = RequestStatus.Approved;

                // 1. إنشاء حساب للطالب إذا لم يكن موجوداً
                var user = await _userManager.FindByEmailAsync(req.Email);
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = req.Email,
                        Email = req.Email,
                        FullName = req.FullName,
                        PhoneNumber = req.WhatsAppNumber,
                        Specialization = req.Specialization,
                        EmailConfirmed = true,
                        IsActive = true
                    };
                    // إنشاء الحساب بدون باسورد مبدئياً
                    await _userManager.CreateAsync(user);
                    await _userManager.AddToRoleAsync(user, "Student");
                }

                // 2. تسجيل الطالب في الدفعة
                var enrollment = new StudentBatch { BatchId = req.BatchId, StudentId = user.Id };
                _context.StudentBatches.Add(enrollment);
                await _context.SaveChangesAsync();

                // 3. توليد رابط "إعداد كلمة المرور"
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.Action("SetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Scheme);

                // إظهار الرابط للأدمن لنسخه وإرساله واتساب
                TempData["SuccessMessage"] = "تم اعتماد الطلب بنجاح.";
                TempData["PasswordLink"] = callbackUrl; // حفظ الرابط لعرضه للأدمن
            }
            return RedirectToAction(nameof(RegistrationRequests));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectRequest(int requestId)
        {
            var req = await _context.RegistrationRequests.FindAsync(requestId);
            if (req != null)
            {
                req.Status = RequestStatus.Rejected;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم رفض الطلب.";
            }
            return RedirectToAction(nameof(RegistrationRequests));
        }

        // ==========================================
        // 5. طلبات توظيف المحاضرين
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> InstructorApplications()
        {
            var apps = await _context.InstructorApplications.OrderByDescending(a => a.AppliedAt).ToListAsync();
            return View(apps);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveInstructor(int id)
        {
            var app = await _context.InstructorApplications.FindAsync(id);
            if (app != null && app.Status == RequestStatus.Pending)
            {
                // إنشاء حساب المحاضر آلياً
                var newUser = new ApplicationUser
                {
                    UserName = app.Email,
                    Email = app.Email,
                    FullName = app.FullName,
                    PhoneNumber = app.PhoneNumber,
                    Specialization = app.Specialization,
                    ProfilePicture = app.ProfilePicturePath,
                    EmailConfirmed = true,
                    IsActive = true
                };

                var result = await _userManager.CreateAsync(newUser, "Instructor@123");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(newUser, "Instructor");
                    app.Status = RequestStatus.Approved;
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"تم اعتماد {app.FullName} كمحاضر في المنصة بكلمة مرور مبدئية: Instructor@123";
                }
            }
            return RedirectToAction(nameof(InstructorApplications));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectInstructor(int id)
        {
            var app = await _context.InstructorApplications.FindAsync(id);
            if (app != null) { app.Status = RequestStatus.Rejected; await _context.SaveChangesAsync(); }
            return RedirectToAction(nameof(InstructorApplications));
        }

        // ==========================================
        // 6. اعتماد الشهادات
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Certificates()
        {
            var certs = await _context.Certificates
                .Include(c => c.Student)
                .Include(c => c.Batch).ThenInclude(b => b.Course)
                .OrderByDescending(c => c.IssueDate)
                .ToListAsync();
            return View(certs);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveCertificate(int certificateId)
        {
            var cert = await _context.Certificates.Include(c => c.Student).FirstOrDefaultAsync(c => c.Id == certificateId);
            if (cert != null)
            {
                cert.IsApproved = true;

                _context.Notifications.Add(new Notification
                {
                    UserId = cert.StudentId,
                    Title = "تم اعتماد شهادتك",
                    Message = "تم إصدار واعتماد شهادتك من قبل الإدارة. يمكنك الآن تحميلها من لوحة التحكم الخاصة بك."
                });

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم اعتماد الشهادة بنجاح.";
            }
            return RedirectToAction(nameof(Certificates));
        }
    }
}