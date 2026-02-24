using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AbstractionCenter.Data;
using AbstractionCenter.Models.Entities;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace AbstractionCenter.Controllers
{
    [Authorize(Roles = "Instructor")]
    public class InstructorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public InstructorController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            ViewData["Title"] = "لوحة تحكم المحاضر";

            // 1. الحصول على حساب المحاضر الحالي
            var user = await _userManager.GetUserAsync(User);

            // 2. جلب الدورات التي يعتبر هذا المحاضر هو "المسجل" الخاص بها
            var myCourses = await _context.Courses
                .Where(c => c.RegistrarUserId == user.Id)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(myCourses);
        }

        [HttpPost]
        public async Task<IActionResult> AddAssignment(int courseId, string title, string description, System.DateTime dueDate)
        {
            var assignment = new Assignment
            {
                CourseId = courseId,
                Title = title,
                Description = description,
                DueDate = dueDate,
                CreatedAt = System.DateTime.Now
            };

            _context.Assignments.Add(assignment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Dashboard");
        }
    }
}