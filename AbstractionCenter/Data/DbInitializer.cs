using AbstractionCenter.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace AbstractionCenter.Data
{
    public static class DbInitializer
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            string[] roleNames = { "Admin", "Instructor", "Student" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            var adminEmail = "admin@abstraction.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var newAdmin = new ApplicationUser { UserName = adminEmail, Email = adminEmail, FullName = "مدير النظام", EmailConfirmed = true };
                var createPowerUser = await userManager.CreateAsync(newAdmin, "Admin@123");
                if (createPowerUser.Succeeded) await userManager.AddToRoleAsync(newAdmin, "Admin");
            }

            var defaultSettings = new List<SiteSetting>
            {
                // إعدادات عامة
                new SiteSetting { Key = "SiteName", Value = "مركز ابستراكشن", Group = "General", DisplayName = "اسم الموقع (البراند)" },
                new SiteSetting { Key = "FooterDesc", Value = "منصتك الأولى للتدريب وبناء القدرات المهنية. نصنع قادة المستقبل.", Group = "General", DisplayName = "وصف الفوتر" },
                new SiteSetting { Key = "Nav_About", Value = "من نحن", Group = "General", DisplayName = "اسم رابط: من نحن" },
                new SiteSetting { Key = "Nav_Tracks", Value = "المسارات", Group = "General", DisplayName = "اسم رابط: المسارات" },
                new SiteSetting { Key = "Nav_Staff", Value = "فريق العمل", Group = "General", DisplayName = "اسم رابط: فريق العمل" },
                new SiteSetting { Key = "Nav_Courses", Value = "الدورات", Group = "General", DisplayName = "اسم رابط: الدورات" },
                new SiteSetting { Key = "Nav_Verify", Value = "الشهادات", Group = "General", DisplayName = "اسم رابط: الشهادات" },
                new SiteSetting { Key = "Nav_Contact", Value = "تواصل معنا", Group = "General", DisplayName = "اسم رابط: تواصل معنا" },
                new SiteSetting { Key = "Staff_HeroTitle", Value = "هيئة التدريس", Group = "General", DisplayName = "عنوان صفحة فريق العمل" },
                new SiteSetting { Key = "Staff_HeroSubtitle", Value = "نفخر في ابستراكشن بنخبة من أفضل الخبراء الأكاديميين والمهنيين الذين ينقلون خبراتهم العملية لك لضمان أعلى جودة تعليمية.", Group = "General", DisplayName = "وصف صفحة فريق العمل" },

                new SiteSetting { Key = "Courses_HeroTitle", Value = "البرامج التدريبية", Group = "General", DisplayName = "عنوان صفحة الدورات" },
                new SiteSetting { Key = "Courses_HeroSubtitle", Value = "استثمر في مستقبلك وسجل الآن في أحدث الدورات المتاحة لتطوير مهاراتك ومواكبة سوق العمل.", Group = "General", DisplayName = "وصف صفحة الدورات" },

                new SiteSetting { Key = "Verify_HeroTitle", Value = "التحقق من صحة الشهادة", Group = "General", DisplayName = "عنوان صفحة الشهادات" },
                new SiteSetting { Key = "Verify_HeroSubtitle", Value = "أدخل رقم الشهادة الفريد (Serial Number) للتحقق من موثوقيتها واعتمادها من المركز.", Group = "General", DisplayName = "وصف صفحة الشهادات" },
                // الرئيسية
                new SiteSetting { Key = "HeroTitle", Value = "نصنع قادة المستقبل في", Group = "Home", DisplayName = "العنوان الرئيسي" },
                new SiteSetting { Key = "HeroSubtitle", Value = "رؤية عربية طموحة لتمكين الشباب بالمهارات التقنية والإدارية الحديثة. نجمع بين أصالة القيم وحداثة التكنولوجيا لنضعك على أول طريق النجاح المهني.", Group = "Home", DisplayName = "وصف الرئيسية" },
                new SiteSetting { Key = "HeroBadge", Value = "منصة الكفاءات العربية الأولى", Group = "Home", DisplayName = "الشريط التحفيزي" },
                new SiteSetting { Key = "HeroImage", Value = "https://images.unsplash.com/photo-1522071820081-009f0129c71c?auto=format&fit=crop&w=1920&q=80", Group = "Home", DisplayName = "صورة القسم الترحيبي" },

                new SiteSetting { Key = "FeaturesTitle", Value = "لماذا نحن خيارك الأفضل؟", Group = "Home", DisplayName = "عنوان المميزات" },
                new SiteSetting { Key = "FeaturesSubtitle", Value = "نقدم تجربة تعليمية فريدة صُممت خصيصاً لتلبي احتياجات سوق العمل بمعايير جودة عالمية.", Group = "Home", DisplayName = "وصف المميزات" },
                new SiteSetting { Key = "F1_Title", Value = "أصالة وابتكار", Group = "Home", DisplayName = "ميزة 1: العنوان" },
                new SiteSetting { Key = "F1_Desc", Value = "محتوى تدريبي يحترم هويتنا وقيمنا، ويُقدم بأحدث أساليب التكنولوجيا والذكاء الاصطناعي.", Group = "Home", DisplayName = "ميزة 1: الوصف" },
                new SiteSetting { Key = "F2_Title", Value = "نخبة الخبراء", Group = "Home", DisplayName = "ميزة 2: العنوان" },
                new SiteSetting { Key = "F2_Desc", Value = "تتلمذ على يد قامات علمية ومهنية من مختلف التخصصات، ينقلون لك خبراتهم الحقيقية.", Group = "Home", DisplayName = "ميزة 2: الوصف" },
                new SiteSetting { Key = "F3_Title", Value = "شهادات موثقة", Group = "Home", DisplayName = "ميزة 3: العنوان" },
                new SiteSetting { Key = "F3_Desc", Value = "احصل على شهادات إتمام قابلة للتحقق الفوري، تدعم سيرتك الذاتية في أكبر الشركات.", Group = "Home", DisplayName = "ميزة 3: الوصف" },

                new SiteSetting { Key = "CTATitle", Value = "جاهز للانطلاق نحو القمة؟", Group = "Home", DisplayName = "عنوان الدعوة (CTA)" },
                new SiteSetting { Key = "CTASubtitle", Value = "لا تضيع المزيد من الوقت. انضم إلى عائلتنا اليوم، وابدأ في بناء قدراتك لكي تكون الخيار الأول في سوق العمل التنافسي.", Group = "Home", DisplayName = "وصف الدعوة" },
                new SiteSetting { Key = "CTABtnText", Value = "سجل حسابك مجاناً", Group = "Home", DisplayName = "نص زر الدعوة" },
                new SiteSetting { Key = "CTAImage", Value = "https://images.unsplash.com/photo-1519389950473-47ba0277781c?auto=format&fit=crop&w=800&q=80", Group = "Home", DisplayName = "صورة قسم الدعوة" },

                // من نحن
                new SiteSetting { Key = "About_HeroTitle", Value = "قصتنا في أبستراكشن", Group = "About", DisplayName = "عنوان من نحن" },
                new SiteSetting { Key = "About_HeroSubtitle", Value = "نحن لسنا مجرد مركز تدريب، بل صرح تعليمي وُلد من رحم الحاجة العربية لبناء عقول شابة قادرة على قيادة المستقبل.", Group = "About", DisplayName = "الوصف الافتتاحي" },
                new SiteSetting { Key = "About_HeroImage", Value = "https://images.unsplash.com/photo-1541339907198-e08756dedf3f?auto=format&fit=crop&w=1920&q=80", Group = "About", DisplayName = "صورة الهيدر" },
                new SiteSetting { Key = "About_TeamImage", Value = "https://images.unsplash.com/photo-1531482615713-2afd69097998?auto=format&fit=crop&w=800&q=80", Group = "About", DisplayName = "صورة فريق العمل" },
                new SiteSetting { Key = "About_Vision", Value = "أن نكون المنصة العربية الأولى الموثوقة والمعتمدة محلياً ودولياً في سد الفجوة بين التعليم الأكاديمي والاحتياجات الفعلية لسوق العمل.", Group = "About", DisplayName = "نص الرؤية" },
                new SiteSetting { Key = "About_Mission", Value = "تقديم برامج تدريبية وتأهيلية مُبتكرة، مبنية على أسس التطبيق العملي والمشاريع الحقيقية، يديرها نخبة من أمهر الخبراء.", Group = "About", DisplayName = "نص الرسالة" },
                
                // المسارات (تمت إضافة الحقول التي طلبنا تغييرها ديناميكياً)
                new SiteSetting { Key = "Tracks_HeroTitle", Value = "المسارات التدريبية", Group = "Tracks", DisplayName = "عنوان صفحة المسارات" },
                new SiteSetting { Key = "Tracks_HeroSubtitle", Value = "نقدم مجموعة متنوعة من المسارات التي تغطي أهم متطلبات سوق العمل الحالي والمستقبلي، مصممة بعناية فائقة.", Group = "Tracks", DisplayName = "وصف صفحة المسارات" },
                new SiteSetting { Key = "Track1_Title", Value = "هندسة البرمجيات والتقنية", Group = "Tracks", DisplayName = "المسار الأول: العنوان" },
                new SiteSetting { Key = "Track1_Desc", Value = "تطوير الويب، الذكاء الاصطناعي، الأمن السيبراني، وبرمجة تطبيقات الهواتف الذكية بأحدث التقنيات.", Group = "Tracks", DisplayName = "المسار الأول: الوصف" },
                new SiteSetting { Key = "Track1_Icon", Value = "fa-solid fa-code", Group = "Tracks", DisplayName = "المسار الأول: الأيقونة" },
                new SiteSetting { Key = "Track2_Title", Value = "الإدارة والأعمال", Group = "Tracks", DisplayName = "المسار الثاني: العنوان" },
                new SiteSetting { Key = "Track2_Desc", Value = "التسويق الرقمي، إدارة المشاريع (PMP)، الموارد البشرية، وتحليل الأعمال وريادة المشاريع.", Group = "Tracks", DisplayName = "المسار الثاني: الوصف" },
                new SiteSetting { Key = "Track2_Icon", Value = "fa-solid fa-chart-line", Group = "Tracks", DisplayName = "المسار الثاني: الأيقونة" },
                new SiteSetting { Key = "Track3_Title", Value = "اللغات والمهارات الناعمة", Group = "Tracks", DisplayName = "المسار الثالث: العنوان" },
                new SiteSetting { Key = "Track3_Desc", Value = "اللغة الإنجليزية المهنية، مهارات التواصل، القيادة، والتحضير لاجتياز المقابلات الشخصية بنجاح.", Group = "Tracks", DisplayName = "المسار الثالث: الوصف" },
                new SiteSetting { Key = "Track3_Icon", Value = "fa-solid fa-language", Group = "Tracks", DisplayName = "المسار الثالث: الأيقونة" },

                // تواصل معنا
                new SiteSetting { Key = "Contact_HeroTitle", Value = "تواصل معنا", Group = "Contact", DisplayName = "عنوان صفحة التواصل" },
                new SiteSetting { Key = "Contact_HeroSubtitle", Value = "نحن هنا للإجابة على جميع استفساراتك ومساعدتك في اختيار المسار المناسب لك.", Group = "Contact", DisplayName = "وصف صفحة التواصل" },
                new SiteSetting { Key = "ContactPhone", Value = "+20 100 000 0000", Group = "Contact", DisplayName = "رقم الهاتف الأساسي" },
                new SiteSetting { Key = "ContactEmail", Value = "info@abstraction.com", Group = "Contact", DisplayName = "البريد الإلكتروني" },
                new SiteSetting { Key = "ContactAddress", Value = "القاهرة، جمهورية مصر العربية", Group = "Contact", DisplayName = "العنوان الجغرافي" },
                
                // السوشيال ميديا
                new SiteSetting { Key = "FacebookLink", Value = "https://facebook.com", Group = "Social", DisplayName = "رابط فيسبوك" },
                new SiteSetting { Key = "TwitterLink", Value = "https://twitter.com", Group = "Social", DisplayName = "رابط تويتر (X)" },
                new SiteSetting { Key = "LinkedInLink", Value = "https://linkedin.com", Group = "Social", DisplayName = "رابط لينكد إن" }
            };

            // زرع الإعدادات الناقصة فقط بدون مسح القديم
            foreach (var setting in defaultSettings)
            {
                if (!context.SiteSettings.Any(s => s.Key == setting.Key))
                {
                    context.SiteSettings.Add(setting);
                }
            }
            await context.SaveChangesAsync();
        }
    }
}   