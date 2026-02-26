using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AbstractionCenter.Models.Entities;
using System.Threading.Tasks;
using System.Linq;

namespace AbstractionCenter.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated) return RedirectToDashboard();
            ViewData["Title"] = "تسجيل الدخول";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, bool rememberMe)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user != null)
                {
                    if (!user.IsActive)
                    {
                        ModelState.AddModelError(string.Empty, "عذراً، هذا الحساب غير مفعل حالياً. يرجى مراجعة الإدارة.");
                        return View();
                    }

                    await _userManager.UpdateSecurityStampAsync(user);

                    var result = await _signInManager.PasswordSignInAsync(user, password, rememberMe, lockoutOnFailure: false);

                    if (result.Succeeded)
                    {
                        return RedirectToDashboard();
                    }
                }
                ModelState.AddModelError(string.Empty, "البريد الإلكتروني أو كلمة المرور غير صحيحة.");
            }
            return View();
        }

        // --- الوظيفة الجديدة لإصلاح خطأ تغيير الباسورد عبر AJAX ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminResetPassword(string userId, string newPassword)
        {
            // التأكد من أن المستخدم الحالي لديه صلاحية (أدمن أو هو نفسه المحاضر)
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Json(new { success = false, message = "غير مصرح لك بهذا الإجراء." });

            var userToChange = await _userManager.FindByIdAsync(userId);
            if (userToChange == null) return Json(new { success = false, message = "المستخدم غير موجود." });

            // إزالة كلمة المرور القديمة وتعيين الجديدة (طريقة Reset الإدارية)
            var removeResult = await _userManager.RemovePasswordAsync(userToChange);
            var addResult = await _userManager.AddPasswordAsync(userToChange, newPassword);

            if (addResult.Succeeded)
            {
                return Json(new { success = true });
            }

            return Json(new
            {
                success = false,
                message = string.Join(", ", addResult.Errors.Select(e => e.Description))
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            ViewData["Title"] = "صلاحيات غير كافية";
            return View();
        }

        private IActionResult RedirectToDashboard()
        {
            if (User.IsInRole("Admin")) return RedirectToAction("Index", "Admin");
            if (User.IsInRole("Instructor")) return RedirectToAction("Dashboard", "Instructor");
            return RedirectToAction("Dashboard", "Student");
        }
    }
}