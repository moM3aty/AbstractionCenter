using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AbstractionCenter.Models.Entities;
using System.Threading.Tasks;

namespace AbstractionCenter.Controllers
{
    /// <summary>
    /// متحكم تسجيل الدخول وتوجيه المستخدمين حسب صلاحياتهم
    /// </summary>
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
            ViewData["Title"] = "تسجيل الدخول";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, bool rememberMe)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(email, password, rememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    // جلب بيانات المستخدم لمعرفة صلاحيته (Role) وتوجيهه للوحة المناسبة
                    var user = await _userManager.FindByEmailAsync(email);
                    var roles = await _userManager.GetRolesAsync(user);

                    if (roles.Contains("Admin"))
                    {
                        return RedirectToAction("Index", "Admin"); // توجيه للوحة الإدارة
                    }
                    else if (roles.Contains("Instructor"))
                    {
                        return RedirectToAction("Dashboard", "Instructor"); // توجيه للوحة هيئة التدريس
                    }
                    else
                    {
                        return RedirectToAction("Dashboard", "Student"); // توجيه لمنصة الطالب
                    }
                }

                ModelState.AddModelError(string.Empty, "البريد الإلكتروني أو كلمة المرور غير صحيحة.");
            }

            return View(); // إعادة عرض صفحة الدخول مع رسالة الخطأ
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}