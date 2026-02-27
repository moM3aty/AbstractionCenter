using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AbstractionCenter.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using AbstractionCenter.Models.Entities;

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
            if (settings == null) return RedirectToAction("Index");

            foreach (var key in settings.Keys)
            {
                var settingToUpdate = await _context.SiteSettings.FirstOrDefaultAsync(s => s.Key == key);

                // إضافة المعالجة للقيم الفارغة لمنع خطأ الـ SQL (NULL)
                string val = settings[key] ?? "";
                string valEn = (settingsEn != null && settingsEn.ContainsKey(key)) ? (settingsEn[key] ?? "") : "";

                if (settingToUpdate != null)
                {
                    settingToUpdate.Value = val;
                    settingToUpdate.ValueEn = valEn;
                }
                else
                {
                    var newSetting = new SiteSetting
                    {
                        Key = key,
                        Value = val,
                        ValueEn = valEn,
                        Group = key.StartsWith("Track") ? "Tracks" : "General",
                        DisplayName = key.Contains("Title") ? "عنوان/نص" : (key.Contains("Desc") ? "وصف" : "إعداد إضافي")
                    };
                    _context.SiteSettings.Add(newSetting);
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "تم حفظ التعديلات ونشرها على الموقع بنجاح.";
            return RedirectToAction("Index");
        }
    }
}