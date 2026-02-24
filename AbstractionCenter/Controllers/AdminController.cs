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

        // حقن DbContext و UserManager للتعامل مع قاعدة البيانات والمستخدمين
        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // لوحة الإدارة الشاملة بالإحصائيات الحقيقية
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "لوحة تحكم الإدارة";

            // جلب الإحصائيات الحقيقية من قاعدة البيانات
            ViewBag.TotalCourses = await _context.Courses.CountAsync();

            // حساب إجمالي الطلاب النشطين في الدورات
            ViewBag.TotalStudents = await _context.StudentCourses.Select(sc => sc.StudentId).Distinct().CountAsync();

            // حساب الشهادات التي تنتظر الاعتماد
            ViewBag.PendingCertificates = await _context.Set<Certificate>().CountAsync(c => !c.IsApproved);

            // حساب عدد أعضاء هيئة التدريس
            var instructors = await _userManager.GetUsersInRoleAsync("Instructor");
            ViewBag.ActiveInstructors = instructors.Count;

            return View();
        }

        // إدارة الدورات
        public async Task<IActionResult> ManageCourses()
        {
            var courses = await _context.Courses.OrderByDescending(c => c.CreatedAt).ToListAsync();
            return View(courses);
        }

        // إدارة المستخدمين
        public async Task<IActionResult> ManageUsers()
        {
            // جلب جميع المستخدمين المسجلين في النظام
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        // دالة لاعتماد الشهادات وإصدارها للطلاب
        [HttpPost]
        public async Task<IActionResult> ApproveCertificate(int certificateId)
        {
            // البحث عن الشهادة في قاعدة البيانات
            var cert = await _context.Set<Certificate>().FindAsync(certificateId);

            if (cert != null)
            {
                // تغيير حالة الشهادة إلى معتمدة
                cert.IsApproved = true;

                // حفظ التغييرات في قاعدة البيانات
                await _context.SaveChangesAsync();
            }

            // إعادة التوجيه للوحة التحكم بعد الاعتماد
            return RedirectToAction("Index");
        }
    }
}