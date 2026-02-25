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

        // لوحة الإدارة الشاملة بالإحصائيات الحقيقية
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "لوحة تحكم الإدارة";

            ViewBag.TotalCourses = await _context.Courses.CountAsync();
            ViewBag.TotalStudents = await _context.StudentCourses.Select(sc => sc.StudentId).Distinct().CountAsync();
            ViewBag.PendingCertificates = await _context.Set<Certificate>().CountAsync(c => !c.IsApproved);

            var instructors = await _userManager.GetUsersInRoleAsync("Instructor");
            ViewBag.ActiveInstructors = instructors.Count;

            return View();
        }

        // ----------------- إدارة الدورات -----------------
        public async Task<IActionResult> ManageCourses()
        {
            var courses = await _context.Courses.OrderByDescending(c => c.CreatedAt).ToListAsync();
            return View(courses);
        }
        [HttpGet]
        public async Task<IActionResult> CreateCourse()
        {
            // جلب جميع المستخدمين الذين يمتلكون صلاحية "محاضر"
            var instructors = await _userManager.GetUsersInRoleAsync("Instructor");
            ViewBag.Instructors = instructors;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCourse(Course course)
        {
            // تعبئة اسم المحاضر تلقائياً بناءً على الـ ID المختار
            var instructor = await _userManager.FindByIdAsync(course.RegistrarUserId);
            if (instructor != null)
            {
                course.RegistrarName = instructor.FullName ?? instructor.Email;
                course.RegistrarWhatsApp = instructor.PhoneNumber ?? "غير متوفر"; // تأكد من إضافة رقم الهاتف في حساب المحاضر
            }

            course.CreatedAt = System.DateTime.Now;
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ManageCourses));
        }

        // ----------------- إدارة المستخدمين والصلاحيات -----------------
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
                // إزالة الصلاحيات القديمة
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

                // إضافة الصلاحية الجديدة
                await _userManager.AddToRoleAsync(user, newRole);
            }
            return RedirectToAction("ManageUsers");
        }

        // ----------------- إدارة طلبات التسجيل الداخلية -----------------
        public async Task<IActionResult> RegistrationRequests()
        {
            var requests = await _context.Set<RegistrationRequest>()
                .Include(r => r.Student)
                .Include(r => r.Course)
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();

            return View(requests);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveRequest(int requestId)
        {
            var request = await _context.Set<RegistrationRequest>().FindAsync(requestId);
            if (request != null && request.Status == RequestStatus.Pending)
            {
                // تغيير حالة الطلب إلى موافق عليه
                request.Status = RequestStatus.Approved;

                // إضافة الطالب رسمياً للدورة
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
            ViewData["Title"] = "إدارة الشهادات";
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
            var cert = await _context.Set<Certificate>().FindAsync(certificateId);
            if (cert != null)
            {
                cert.IsApproved = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}