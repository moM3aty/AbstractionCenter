using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AbstractionCenter.Models.Entities;
using AbstractionCenter.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AbstractionCenter.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFileUploaderService _fileUploader; // خدمة رفع الملفات

        public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IFileUploaderService fileUploader)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _fileUploader = fileUploader;
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
                    // --- تنفيذ ميزة الطرد للجهزة الأخرى ---
                    // بتحديث الـ SecurityStamp يتم إبطال مفعول كل الجلسات السابقة (Cookies) المفتوحة لهذا الحساب
                    await _userManager.UpdateSecurityStampAsync(user);

                    var result = await _signInManager.PasswordSignInAsync(user, password, rememberMe, lockoutOnFailure: false);

                    if (result.Succeeded)
                    {
                        var roles = await _userManager.GetRolesAsync(user);
                        if (roles.Contains("Admin")) return RedirectToAction("Index", "Admin");
                        if (roles.Contains("Instructor")) return RedirectToAction("Dashboard", "Instructor");
                        return RedirectToAction("Dashboard", "Student");
                    }
                }
                ModelState.AddModelError(string.Empty, "البريد الإلكتروني أو كلمة المرور غير صحيحة.");
            }
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated) return RedirectToDashboard();
            ViewData["Title"] = "إنشاء حساب جديد";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // تم إضافة استقبال الصورة الشخصية ProfilePictureFile
        public async Task<IActionResult> Register(string fullName, string email, string password, string confirmPassword, IFormFile? profilePictureFile)
        {
            if (password != confirmPassword)
            {
                ModelState.AddModelError(string.Empty, "كلمتا المرور غير متطابقتين.");
                return View();
            }

            if (ModelState.IsValid)
            {
                // رفع الصورة إذا وجدت
                string profilePicPath = null;
                if (profilePictureFile != null)
                {
                    profilePicPath = await _fileUploader.UploadFileAsync(profilePictureFile, "profiles");
                }

                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = fullName,
                    ProfilePicture = profilePicPath, // حفظ مسار الصورة
                    CreatedAt = System.DateTime.Now
                };

                var result = await _userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Student");
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Dashboard", "Student");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View();
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