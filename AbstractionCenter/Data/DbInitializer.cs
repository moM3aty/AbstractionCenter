using AbstractionCenter.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace AbstractionCenter.Data
{
    /// <summary>
    /// كلاس مسؤول عن زرع الأدوار (Roles) الأساسية وحساب الإدارة (Admin) 
    /// في قاعدة البيانات عند تشغيل المشروع لأول مرة.
    /// </summary>
    public static class DbInitializer
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // 1. تعريف الأدوار الأساسية في المنصة
            string[] roleNames = { "Admin", "Instructor", "Student" };
            IdentityResult roleResult;

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    // إنشاء الدور إذا لم يكن موجوداً
                    roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2. إنشاء حساب المدير الافتراضي (Admin)
            var adminEmail = "admin@abstraction.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var newAdmin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "مدير النظام",
                    EmailConfirmed = true
                };

                // كلمة المرور الافتراضية للمدير
                var createPowerUser = await userManager.CreateAsync(newAdmin, "Admin@123");
                if (createPowerUser.Succeeded)
                {
                    // ربط حساب المدير بدور الـ Admin
                    await userManager.AddToRoleAsync(newAdmin, "Admin");
                }
            }
        }
    }
}