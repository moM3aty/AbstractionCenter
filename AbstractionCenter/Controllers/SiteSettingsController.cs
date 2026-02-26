using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AbstractionCenter.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace AbstractionCenter.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SiteSettingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SiteSettingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var settings = await _context.SiteSettings.ToListAsync();
            return View(settings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSettings(Dictionary<string, string> settings, Dictionary<string, string> settingsEn)
        {
            foreach (var key in settings.Keys)
            {
                var settingToUpdate = await _context.SiteSettings.FirstOrDefaultAsync(s => s.Key == key);
                if (settingToUpdate != null)
                {
                    settingToUpdate.Value = settings[key];
                    if (settingsEn.ContainsKey(key))
                    {
                        settingToUpdate.ValueEn = settingsEn[key];
                    }
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "تم تحديث إعدادات ومحتوى الموقع بنجاح.";
            return RedirectToAction("Index");
        }
    }
}