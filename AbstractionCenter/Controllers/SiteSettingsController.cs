using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AbstractionCenter.Data;
using AbstractionCenter.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
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
            ViewData["Title"] = "تصميم ومحتوى الموقع";
            var settings = await _context.SiteSettings.ToListAsync();
            return View(settings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSettings(
            Dictionary<string, string> settings,
            List<string> NewSetting_Names,
            List<string> NewSetting_Values,
            List<string> NewSetting_Groups)
        {
            if (settings != null)
            {
                foreach (var item in settings)
                {
                    var setting = await _context.SiteSettings.FirstOrDefaultAsync(s => s.Key == item.Key);
                    if (setting != null)
                    {
                        setting.Value = item.Value;
                    }
                }
            }

            // إضافة الروابط الجديدة
            if (NewSetting_Names != null && NewSetting_Names.Count > 0)
            {
                for (int i = 0; i < NewSetting_Names.Count; i++)
                {
                    if (!string.IsNullOrWhiteSpace(NewSetting_Values[i]) && !string.IsNullOrWhiteSpace(NewSetting_Names[i]))
                    {
                        string uniqueKey = "Custom_" + Guid.NewGuid().ToString("N").Substring(0, 6);

                        var newSetting = new SiteSetting
                        {
                            Key = uniqueKey,
                            DisplayName = NewSetting_Names[i],
                            Value = NewSetting_Values[i],
                            Group = NewSetting_Groups != null && NewSetting_Groups.Count > i ? NewSetting_Groups[i] : "Contact"
                        };
                        _context.SiteSettings.Add(newSetting);
                    }
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "تم حفظ التعديلات بنجاح!";
            return RedirectToAction(nameof(Index));
        }

        // --- الدالة الجديدة لمسح الروابط ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSetting(string key)
        {
            var setting = await _context.SiteSettings.FirstOrDefaultAsync(s => s.Key == key);
            if (setting != null)
            {
                _context.SiteSettings.Remove(setting);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم حذف الرابط بنجاح!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}