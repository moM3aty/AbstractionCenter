using AbstractionCenter.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AbstractionCenter.Data
{
    public static class DbInitializer
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // 1. إنشاء الأدوار الأساسية
            string[] roleNames = { "Admin", "Instructor", "Student" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2. إنشاء حساب المدير العام الافتراضي
            var adminEmail = "admin@abstraction.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var newAdmin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "الإدارة العليا",
                    EmailConfirmed = true,
                    IsActive = true
                };
                var createPowerUser = await userManager.CreateAsync(newAdmin, "Admin@123");
                if (createPowerUser.Succeeded) await userManager.AddToRoleAsync(newAdmin, "Admin");
            }

            // 3. قائمة شاملة لكل إعدادات الموقع (تعكس كل ما تم استخدامه في الـ Views)
            var defaultSettings = new List<SiteSetting>
            {
                // --- الإعدادات العامة والروابط ---
                new SiteSetting { Key = "SiteName", Value = "مركز ابستراكشن", ValueEn = "Abstraction Center", Group = "General", DisplayName = "اسم الموقع" },
                new SiteSetting { Key = "FooterDesc", Value = "منصتك الأولى للتدريب وبناء القدرات المهنية وفق معايير عالمية.", ValueEn = "Your premier platform for training and professional capacity building according to global standards.", Group = "General", DisplayName = "وصف الفوتر" },
                
                // --- إعدادات الحسابات وكلمات المرور الافتراضية ---
                new SiteSetting { Key = "DefaultInstructorPass", Value = "Instructor@123", ValueEn = "Instructor@123", Group = "Security", DisplayName = "كلمة مرور المحاضرين الافتراضية" },
                new SiteSetting { Key = "DefaultStudentPass", Value = "Student@123", ValueEn = "Student@123", Group = "Security", DisplayName = "كلمة مرور الطلاب الافتراضية" },

                // --- روابط القائمة العلوية ---
                new SiteSetting { Key = "Nav_About", Value = "من نحن", ValueEn = "About Us", Group = "General", DisplayName = "رابط: من نحن" },
                new SiteSetting { Key = "Nav_Tracks", Value = "المسارات", ValueEn = "Tracks", Group = "General", DisplayName = "رابط: المسارات" },
                new SiteSetting { Key = "Nav_Staff", Value = "الخبراء", ValueEn = "Experts", Group = "General", DisplayName = "رابط: الخبراء" },
                new SiteSetting { Key = "Nav_Courses", Value = "البرامج", ValueEn = "Programs", Group = "General", DisplayName = "رابط: الدورات" },
                new SiteSetting { Key = "Nav_Verify", Value = "الشهادات", ValueEn = "Certificates", Group = "General", DisplayName = "رابط: الشهادات" },
                new SiteSetting { Key = "Nav_Contact", Value = "تواصل معنا", ValueEn = "Contact Us", Group = "General", DisplayName = "رابط: تواصل معنا" },
                
                // --- روابط التواصل الاجتماعي الشاملة ---
                new SiteSetting { Key = "Social_Facebook", Value = "https://facebook.com/abstraction", ValueEn = "https://facebook.com/abstraction", Group = "Social", DisplayName = "فيسبوك" },
                new SiteSetting { Key = "Social_Twitter", Value = "https://twitter.com/abstraction", ValueEn = "https://twitter.com/abstraction", Group = "Social", DisplayName = "تويتر (X)" },
                new SiteSetting { Key = "Social_Instagram", Value = "https://instagram.com/abstraction", ValueEn = "https://instagram.com/abstraction", Group = "Social", DisplayName = "انستجرام" },
                new SiteSetting { Key = "Social_LinkedIn", Value = "https://linkedin.com/company/abstraction", ValueEn = "https://linkedin.com/company/abstraction", Group = "Social", DisplayName = "لينكد إن" },
                new SiteSetting { Key = "Social_YouTube", Value = "https://youtube.com/@abstraction", ValueEn = "https://youtube.com/@abstraction", Group = "Social", DisplayName = "يوتيوب" },
                new SiteSetting { Key = "Social_TikTok", Value = "https://tiktok.com/@abstraction", ValueEn = "https://tiktok.com/@abstraction", Group = "Social", DisplayName = "تيك توك" },
                new SiteSetting { Key = "Social_WhatsApp", Value = "https://wa.me/201000000000", ValueEn = "https://wa.me/201000000000", Group = "Social", DisplayName = "واتساب" },

                // --- الصفحة الرئيسية: قسم الترحيب (Hero) ---
                new SiteSetting { Key = "HeroBadge", Value = "منصة الكفاءات العربية الأولى", ValueEn = "The Premier Capability Platform", Group = "Home", DisplayName = "الرئيسية: الشارة العلوي" },
                new SiteSetting { Key = "HeroTitle", Value = "نصنع قادة المستقبل في", ValueEn = "We Build Future Leaders at", Group = "Home", DisplayName = "الرئيسية: العنوان الكبير" },
                new SiteSetting { Key = "HeroSubtitle", Value = "رؤية طموحة لتمكين الشباب بالمهارات التقنية والإدارية الحديثة. نجمع بين أصالة القيم وحداثة التكنولوجيا.", ValueEn = "An ambitious vision to empower youth with modern technical and administrative skills. Combining values with technology.", Group = "Home", DisplayName = "الرئيسية: الوصف" },
                new SiteSetting { Key = "HeroImage", Value = "https://images.unsplash.com/photo-1522071820081-009f0129c71c?auto=format&fit=crop&w=1920&q=80", ValueEn = "https://images.unsplash.com/photo-1522071820081-009f0129c71c?auto=format&fit=crop&w=1920&q=80", Group = "Home", DisplayName = "الرئيسية: الصورة الرئيسية" },

                // --- الصفحة الرئيسية: قسم المميزات (Features) ---
                new SiteSetting { Key = "FeaturesTitle", Value = "لماذا نحن خيارك الأفضل؟", ValueEn = "Why Choose Us?", Group = "Home", DisplayName = "الرئيسية: عنوان المميزات" },
                new SiteSetting { Key = "FeaturesSubtitle", Value = "نقدم تجربة تعليمية فريدة صُممت لتلبي احتياجات سوق العمل بمعايير جودة عالمية.", ValueEn = "We offer a unique educational experience designed to meet market needs with quality.", Group = "Home", DisplayName = "الرئيسية: وصف المميزات" },
                new SiteSetting { Key = "F1_Title", Value = "أصالة وابتكار", ValueEn = "Innovation", Group = "Home", DisplayName = "الميزة 1: العنوان" },
                new SiteSetting { Key = "F1_Desc", Value = "محتوى تدريبي متطور يُقدم بأحدث أساليب التكنولوجيا والذكاء الاصطناعي.", ValueEn = "Advanced training delivered with the latest AI technology.", Group = "Home", DisplayName = "الميزة 1: الوصف" },
                new SiteSetting { Key = "F2_Title", Value = "نخبة الخبراء", ValueEn = "Elite Experts", Group = "Home", DisplayName = "الميزة 2: العنوان" },
                new SiteSetting { Key = "F2_Desc", Value = "تتلمذ على يد قامات مهنية تنقل لك خبراتها الحقيقية من سوق العمل.", ValueEn = "Learn from professionals who transfer real-world experience.", Group = "Home", DisplayName = "الميزة 2: الوصف" },
                new SiteSetting { Key = "F3_Title", Value = "شهادات موثقة", ValueEn = "Verified Certs", Group = "Home", DisplayName = "الميزة 3: العنوان" },
                new SiteSetting { Key = "F3_Desc", Value = "احصل على شهادات إتمام قابلة للتحقق الفوري تدعم سيرتك الذاتية.", ValueEn = "Earn verifiable completion certificates to boost your career.", Group = "Home", DisplayName = "الميزة 3: الوصف" },

                // --- الصفحة الرئيسية: قسم التحفيز (CTA) ---
                new SiteSetting { Key = "CTATitle", Value = "جاهز للانطلاق نحو القمة؟", ValueEn = "Ready to Reach the Top?", Group = "Home", DisplayName = "الرئيسية: عنوان التحفيز" },
                new SiteSetting { Key = "CTASubtitle", Value = "انضم إلى عائلتنا اليوم، وابدأ في بناء قدراتك لكي تكون الخيار الأول في سوق العمل.", ValueEn = "Join our family today and start building your capabilities to be the first choice.", Group = "Home", DisplayName = "الرئيسية: وصف التحفيز" },
                new SiteSetting { Key = "CTAImage", Value = "https://images.unsplash.com/photo-1519389950473-47ba0277781c?auto=format&fit=crop&w=800&q=80", ValueEn = "https://images.unsplash.com/photo-1519389950473-47ba0277781c?auto=format&fit=crop&w=800&q=80", Group = "Home", DisplayName = "الرئيسية: صورة التحفيز" },

                // --- صفحة من نحن (About) ---
                new SiteSetting { Key = "About_HeroTitle", Value = "قصتنا في أبستراكشن", ValueEn = "Our Story at Abstraction", Group = "About", DisplayName = "من نحن: العنوان" },
                new SiteSetting { Key = "About_HeroSubtitle", Value = "نحن صرح تعليمي وُلد من رحم الحاجة لبناء عقول شابة قادرة على قيادة المستقبل.", ValueEn = "An educational monument born to build young minds capable of leading.", Group = "About", DisplayName = "من نحن: الوصف" },
                new SiteSetting { Key = "About_Vision", Value = "أن نكون المنصة الأولى الموثوقة في سد الفجوة بين التعليم الأكاديمي وسوق العمل.", ValueEn = "To be the first trusted platform bridging the gap between education and market.", Group = "About", DisplayName = "من نحن: الرؤية" },
                new SiteSetting { Key = "About_Mission", Value = "تقديم برامج تدريبية وتأهيلية مُبتكرة مبنية على أسس التطبيق العملي والمشاريع.", ValueEn = "Providing innovative programs based on practical application and projects.", Group = "About", DisplayName = "من نحن: الرسالة" },

                // --- صفحة المسارات (Tracks) ---
                new SiteSetting { Key = "Tracks_HeroTitle", Value = "المسارات التدريبية", ValueEn = "Training Tracks", Group = "Tracks", DisplayName = "المسارات: العنوان" },
                new SiteSetting { Key = "Tracks_HeroSubtitle", Value = "نقدم مجموعة متنوعة من المسارات التي تغطي أهم متطلبات سوق العمل الحالي والمستقبلي.", ValueEn = "We offer a variety of tracks covering current and future market needs.", Group = "Tracks", DisplayName = "المسارات: الوصف" },
                new SiteSetting { Key = "Track1_Title", Value = "هندسة البرمجيات والتقنية", ValueEn = "Software & Tech", Group = "Tracks", DisplayName = "مسار 1: العنوان" },
                new SiteSetting { Key = "Track2_Title", Value = "الإدارة والأعمال", ValueEn = "Business & Admin", Group = "Tracks", DisplayName = "مسار 2: العنوان" },
                new SiteSetting { Key = "Track3_Title", Value = "اللغات والمهارات الناعمة", ValueEn = "Languages & Soft Skills", Group = "Tracks", DisplayName = "مسار 3: العنوان" },

                // --- صفحة فريق العمل (Staff) ---
                new SiteSetting { Key = "Staff_HeroTitle", Value = "نخبة من الخبراء", ValueEn = "Elite Experts", Group = "Staff", DisplayName = "الخبراء: العنوان" },
                new SiteSetting { Key = "Staff_HeroSubtitle", Value = "نفخر بنخبة من أفضل الخبراء الأكاديميين والمهنيين لضمان أعلى جودة تعليمية.", ValueEn = "We are proud of our academic and professional experts for high quality.", Group = "Staff", DisplayName = "الخبراء: الوصف" },

                // --- صفحة التحقق (Verify) ---
                new SiteSetting { Key = "Verify_HeroTitle", Value = "التحقق من صحة الشهادة", ValueEn = "Verify Certificate Authenticity", Group = "Verify", DisplayName = "التحقق: العنوان" },
                new SiteSetting { Key = "Verify_HeroSubtitle", Value = "أدخل رقم الشهادة الفريد للتحقق من موثوقيتها واعتمادها من المركز.", ValueEn = "Enter the unique certificate number to verify its authenticity.", Group = "Verify", DisplayName = "التحقق: الوصف" },

                // --- بيانات التواصل (Contact) ---
                new SiteSetting { Key = "Contact_HeroTitle", Value = "يسعدنا تواصلك", ValueEn = "We'd Love to Hear From You", Group = "Contact", DisplayName = "تواصل: العنوان" },
                new SiteSetting { Key = "Contact_HeroSubtitle", Value = "نحن هنا للإجابة على جميع استفساراتك ومساعدتك في اتخاذ الخطوة القادمة.", ValueEn = "We are here to answer your inquiries and help you take the next step.", Group = "Contact", DisplayName = "تواصل: الوصف" },
                new SiteSetting { Key = "ContactPhone", Value = "+20 100 000 0000", ValueEn = "+20 100 000 0000", Group = "Contact", DisplayName = "رقم الهاتف" },
                new SiteSetting { Key = "ContactEmail", Value = "info@abstraction.com", ValueEn = "info@abstraction.com", Group = "Contact", DisplayName = "البريد الإلكتروني" },
                new SiteSetting { Key = "ContactAddress", Value = "القاهرة، جمهورية مصر العربية", ValueEn = "Cairo, Egypt", Group = "Contact", DisplayName = "العنوان" }
            };

            // 4. حفظ الإعدادات في قاعدة البيانات
            foreach (var setting in defaultSettings)
            {
                var existingSetting = context.SiteSettings.FirstOrDefault(s => s.Key == setting.Key);
                if (existingSetting == null)
                {
                    context.SiteSettings.Add(setting);
                }
                else
                {
                    // تحديث الترجمة الإنجليزية إذا كانت مفقودة في السجل الموجود
                    if (string.IsNullOrEmpty(existingSetting.ValueEn))
                    {
                        existingSetting.ValueEn = setting.ValueEn;
                    }
                }
            }
            await context.SaveChangesAsync();
        }
    }
}