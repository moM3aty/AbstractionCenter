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

            // 3. إنشاء إعدادات الموقع الأساسية (مضاف إليها إعدادات الشهادة)
            if (!context.SiteSettings.Any())
            {
                var defaultSettings = new List<SiteSetting>
                {
                    // --- إعدادات عامة وهيدر وفوتر ---
                    new SiteSetting { Key = "SiteName", Value = "مركز أبستراكشن", ValueEn = "Abstraction Center", Group = "General", DisplayName = "اسم الموقع" },
                    new SiteSetting { Key = "FooterDesc", Value = "منصتك الأولى للتدريب وبناء القدرات المهنية وفق معايير عالمية.", ValueEn = "Your premier platform for training and professional capacity building.", Group = "General", DisplayName = "وصف الفوتر" },
                    new SiteSetting { Key = "CopyrightText", Value = "تم التطوير بواسطة GMTWEB", ValueEn = "Developed by GMTWEB", Group = "General", DisplayName = "حقوق النشر" },
                    
                    // --- روابط القائمة العلوية ---
                    new SiteSetting { Key = "Nav_About", Value = "من نحن", ValueEn = "About Us", Group = "General", DisplayName = "رابط من نحن" },
                    new SiteSetting { Key = "Nav_Courses", Value = "البرامج المتاحة", ValueEn = "Available Programs", Group = "General", DisplayName = "رابط البرامج" },
                    new SiteSetting { Key = "Nav_Tracks", Value = "مساراتنا", ValueEn = "Our Tracks", Group = "General", DisplayName = "رابط المسارات" },
                    new SiteSetting { Key = "Nav_Staff", Value = "الخبراء", ValueEn = "Experts", Group = "General", DisplayName = "رابط الخبراء" },
                    new SiteSetting { Key = "Nav_Verify", Value = "التحقق من الشهادة", ValueEn = "Verify Certificate", Group = "General", DisplayName = "رابط التحقق" },
                    new SiteSetting { Key = "Nav_Contact", Value = "تواصل معنا", ValueEn = "Contact", Group = "General", DisplayName = "رابط التواصل" },

                    // --- بيانات التواصل والسوشيال ميديا ---
                    new SiteSetting { Key = "ContactPhone", Value = "+20 100 000 0000", ValueEn = "+20 100 000 0000", Group = "Contact", DisplayName = "رقم الهاتف" },
                    new SiteSetting { Key = "ContactEmail", Value = "info@abstraction.com", ValueEn = "info@abstraction.com", Group = "Contact", DisplayName = "البريد الإلكتروني" },
                    new SiteSetting { Key = "ContactAddress", Value = "القاهرة، جمهورية مصر العربية", ValueEn = "Cairo, Egypt", Group = "Contact", DisplayName = "العنوان" },
                    new SiteSetting { Key = "Social_Facebook", Value = "https://facebook.com", ValueEn = "https://facebook.com", Group = "Contact", DisplayName = "فيسبوك" },
                    new SiteSetting { Key = "Social_Twitter", Value = "https://twitter.com", ValueEn = "https://twitter.com", Group = "Contact", DisplayName = "تويتر / X" },
                    new SiteSetting { Key = "Social_Instagram", Value = "https://instagram.com", ValueEn = "https://instagram.com", Group = "Contact", DisplayName = "انستجرام" },
                    new SiteSetting { Key = "Social_LinkedIn", Value = "https://linkedin.com", ValueEn = "https://linkedin.com", Group = "Contact", DisplayName = "لينكد إن" },
                    new SiteSetting { Key = "Social_YouTube", Value = "https://youtube.com", ValueEn = "https://youtube.com", Group = "Contact", DisplayName = "يوتيوب" },
                    new SiteSetting { Key = "Social_WhatsApp", Value = "https://wa.me/201000000000", ValueEn = "https://wa.me/201000000000", Group = "Contact", DisplayName = "واتساب" },
                    
                    // --- ترويسة صفحة التواصل ---
                    new SiteSetting { Key = "Contact_HeroTitle", Value = "يسعدنا تواصلك", ValueEn = "We'd Love to Hear From You", Group = "Contact", DisplayName = "عنوان صفحة التواصل" },
                    new SiteSetting { Key = "Contact_HeroSubtitle", Value = "نحن هنا للإجابة على جميع استفساراتك ومساعدتك في اتخاذ الخطوة القادمة.", ValueEn = "We are here to answer all your inquiries and help you take the next step.", Group = "Contact", DisplayName = "وصف صفحة التواصل" },

                    // --- الصفحة الرئيسية (Home Page) ---
                    new SiteSetting { Key = "HeroBadge", Value = "منصة الكفاءات العربية الأولى", ValueEn = "The Premier Capability Platform", Group = "Home", DisplayName = "وسام البانر" },
                    new SiteSetting { Key = "HeroTitle", Value = "نصنع قادة المستقبل في", ValueEn = "We Build Future Leaders at", Group = "Home", DisplayName = "عنوان البانر" },
                    new SiteSetting { Key = "HeroSubtitle", Value = "رؤية طموحة لتمكين الشباب بالمهارات التقنية والإدارية الحديثة.", ValueEn = "An ambitious vision to empower youth with modern technical and administrative skills.", Group = "Home", DisplayName = "وصف البانر" },
                    new SiteSetting { Key = "HeroImage", Value = "https://images.unsplash.com/photo-1522071820081-009f0129c71c?auto=format&fit=crop&w=1920&q=80", ValueEn = "https://images.unsplash.com/photo-1522071820081-009f0129c71c?auto=format&fit=crop&w=1920&q=80", Group = "Home", DisplayName = "صورة البانر" },
                    new SiteSetting { Key = "FeaturesTitle", Value = "لماذا نحن خيارك الأفضل؟", ValueEn = "Why Choose Us?", Group = "Home", DisplayName = "عنوان المميزات" },
                    new SiteSetting { Key = "FeaturesSubtitle", Value = "نقدم تجربة تعليمية فريدة صُممت لتلبي احتياجات سوق العمل.", ValueEn = "We offer a unique educational experience designed to meet market needs.", Group = "Home", DisplayName = "وصف المميزات" },
                    new SiteSetting { Key = "F1_Title", Value = "أصالة وابتكار", ValueEn = "Originality & Innovation", Group = "Home", DisplayName = "ميزة 1 - عنوان" },
                    new SiteSetting { Key = "F1_Desc", Value = "محتوى تدريبي متطور يُقدم بأحدث أساليب التكنولوجيا.", ValueEn = "Advanced training content delivered with the latest technology.", Group = "Home", DisplayName = "ميزة 1 - وصف" },
                    new SiteSetting { Key = "F2_Title", Value = "نخبة الخبراء", ValueEn = "Elite Experts", Group = "Home", DisplayName = "ميزة 2 - عنوان" },
                    new SiteSetting { Key = "F2_Desc", Value = "تتلمذ على يد قامات مهنية تنقل لك خبراتها الحقيقية.", ValueEn = "Learn from professional figures who transfer their real market experiences to you.", Group = "Home", DisplayName = "ميزة 2 - وصف" },
                    new SiteSetting { Key = "F3_Title", Value = "شهادات موثقة", ValueEn = "Certified Credentials", Group = "Home", DisplayName = "ميزة 3 - عنوان" },
                    new SiteSetting { Key = "F3_Desc", Value = "احصل على شهادات إتمام قابلة للتحقق الفوري تدعم سيرتك الذاتية.", ValueEn = "Earn verifiable completion certificates to boost your resume.", Group = "Home", DisplayName = "ميزة 3 - وصف" },
                    new SiteSetting { Key = "CTATitle", Value = "جاهز للانطلاق نحو القمة؟", ValueEn = "Ready to Reach the Top?", Group = "Home", DisplayName = "عنوان الدعوة" },
                    new SiteSetting { Key = "CTASubtitle", Value = "لا تضيع المزيد من الوقت. انضم إلى عائلتنا اليوم.", ValueEn = "Don't waste any more time. Join our family today.", Group = "Home", DisplayName = "وصف الدعوة" },
                    new SiteSetting { Key = "CTAImage", Value = "https://images.unsplash.com/photo-1519389950473-47ba0277781c?auto=format&fit=crop&w=800&q=80", ValueEn = "https://images.unsplash.com/photo-1519389950473-47ba0277781c?auto=format&fit=crop&w=800&q=80", Group = "Home", DisplayName = "صورة الدعوة" },

                    // --- صفحة من نحن (About Us) ---
                    new SiteSetting { Key = "About_HeroImage", Value = "https://images.unsplash.com/photo-1541339907198-e08756dedf3f?auto=format&fit=crop&w=1920&q=80", ValueEn = "https://images.unsplash.com/photo-1541339907198-e08756dedf3f?auto=format&fit=crop&w=1920&q=80", Group = "About", DisplayName = "صورة بانر من نحن" },
                    new SiteSetting { Key = "About_HeroTitle", Value = "قصتنا في أبستراكشن", ValueEn = "Our Story at Abstraction", Group = "About", DisplayName = "عنوان بانر من نحن" },
                    new SiteSetting { Key = "About_HeroSubtitle", Value = "نحن صرح تعليمي وُلد من رحم الحاجة لبناء عقول شابة قادرة على قيادة المستقبل.", ValueEn = "We are an educational institution born from the need to build young minds capable of leading the future.", Group = "About", DisplayName = "وصف بانر من نحن" },
                    new SiteSetting { Key = "About_TeamImage", Value = "https://images.unsplash.com/photo-1531482615713-2afd69097998?auto=format&fit=crop&w=800&q=80", ValueEn = "https://images.unsplash.com/photo-1531482615713-2afd69097998?auto=format&fit=crop&w=800&q=80", Group = "About", DisplayName = "صورة الفريق" },
                    new SiteSetting { Key = "About_Vision", Value = "أن نكون المنصة الأولى الموثوقة في سد الفجوة بين التعليم الأكاديمي واحتياجات سوق العمل الحقيقية.", ValueEn = "To be the first trusted platform in bridging the gap between academic education and real market needs.", Group = "About", DisplayName = "الرؤية" },
                    new SiteSetting { Key = "About_Mission", Value = "تقديم برامج تدريبية وتأهيلية مُبتكرة مبنية على أسس التطبيق العملي والمشاريع الحقيقية.", ValueEn = "Providing innovative training programs based on practical application and real-world projects.", Group = "About", DisplayName = "الرسالة" },
                    new SiteSetting { Key = "About_ValuesTitle", Value = "قيمنا المؤسسية", ValueEn = "Our Core Values", Group = "About", DisplayName = "عنوان القيم" },
                    new SiteSetting { Key = "About_Value1_Title", Value = "الأمانة والنزاهة", ValueEn = "Integrity & Honesty", Group = "About", DisplayName = "قيمة 1 - عنوان" },
                    new SiteSetting { Key = "About_Value1_Desc", Value = "نلتزم بتقديم محتوى علمي دقيق بشفافية تامة ومصداقية مطلقة.", ValueEn = "We are committed to providing accurate scientific content with complete transparency.", Group = "About", DisplayName = "قيمة 1 - وصف" },
                    new SiteSetting { Key = "About_Value2_Title", Value = "التطور المستمر", ValueEn = "Continuous Improvement", Group = "About", DisplayName = "قيمة 2 - عنوان" },
                    new SiteSetting { Key = "About_Value2_Desc", Value = "تحديث دائم لمساراتنا لمواكبة تسارع التكنولوجيا ومتطلبات العصر.", ValueEn = "Constantly updating our tracks to keep pace with accelerating technology.", Group = "About", DisplayName = "قيمة 2 - وصف" },
                    new SiteSetting { Key = "About_Value3_Title", Value = "المجتمع الواحد", ValueEn = "One Community", Group = "About", DisplayName = "قيمة 3 - عنوان" },
                    new SiteSetting { Key = "About_Value3_Desc", Value = "نبني شبكة علاقات قوية وتفاعلية بين الطلاب والمدربين وسوق العمل.", ValueEn = "We build a strong and interactive network between students, trainers, and the job market.", Group = "About", DisplayName = "قيمة 3 - وصف" },

                    // --- صفحة المسارات (Tracks) ---
                    new SiteSetting { Key = "Tracks_HeroTitle", Value = "المسارات التدريبية", ValueEn = "Training Tracks", Group = "Tracks", DisplayName = "عنوان صفحة المسارات" },
                    new SiteSetting { Key = "Tracks_HeroSubtitle", Value = "نقدم مجموعة متنوعة من المسارات التي تغطي أهم متطلبات سوق العمل.", ValueEn = "We offer a variety of tracks covering the most important requirements of the job market.", Group = "Tracks", DisplayName = "وصف صفحة المسارات" },
                    new SiteSetting { Key = "Track1_Title", Value = "مسار البرمجة والتطوير", ValueEn = "Programming & Development", Group = "Tracks", DisplayName = "مسار 1 - العنوان" },
                    new SiteSetting { Key = "Track1_Desc", Value = "تعلم لغات البرمجة الحديثة وتطوير تطبيقات الويب والهواتف الذكية.", ValueEn = "Learn modern programming languages and web/mobile app development.", Group = "Tracks", DisplayName = "مسار 1 - الوصف" },
                    new SiteSetting { Key = "Track1_Icon", Value = "fa-solid fa-laptop-code", ValueEn = "fa-solid fa-laptop-code", Group = "Tracks", DisplayName = "مسار 1 - الأيقونة" },
                    new SiteSetting { Key = "Track2_Title", Value = "مسار الذكاء الاصطناعي", ValueEn = "Artificial Intelligence", Group = "Tracks", DisplayName = "مسار 2 - العنوان" },
                    new SiteSetting { Key = "Track2_Desc", Value = "اكتشف عالم البيانات وتعلم بناء نماذج التعلم الآلي والشبكات العصبية.", ValueEn = "Discover the data world and learn to build machine learning models.", Group = "Tracks", DisplayName = "مسار 2 - الوصف" },
                    new SiteSetting { Key = "Track2_Icon", Value = "fa-solid fa-brain", ValueEn = "fa-solid fa-brain", Group = "Tracks", DisplayName = "مسار 2 - الأيقونة" },
                    new SiteSetting { Key = "Track3_Title", Value = "مسار التصميم الجرافيكي", ValueEn = "Graphic Design", Group = "Tracks", DisplayName = "مسار 3 - العنوان" },
                    new SiteSetting { Key = "Track3_Desc", Value = "احترف برامج التصميم وصناعة الهويات البصرية والواجهات.", ValueEn = "Master design software and create visual identities and interfaces.", Group = "Tracks", DisplayName = "مسار 3 - الوصف" },
                    new SiteSetting { Key = "Track3_Icon", Value = "fa-solid fa-paintbrush", ValueEn = "fa-solid fa-paintbrush", Group = "Tracks", DisplayName = "مسار 3 - الأيقونة" },

                    // --- صفحة التحقق من الشهادة ---
                    new SiteSetting { Key = "Verify_HeroTitle", Value = "التحقق من صحة الشهادة", ValueEn = "Verify Certificate Authenticity", Group = "Verify", DisplayName = "عنوان التحقق" },
                    new SiteSetting { Key = "Verify_HeroSubtitle", Value = "أدخل رقم الشهادة الفريد (Serial Number) للتحقق من موثوقيتها واعتمادها.", ValueEn = "Enter the unique Serial Number to verify its authenticity and approval.", Group = "Verify", DisplayName = "وصف التحقق" },

                    // --- تعليمات الدفع ---
                    new SiteSetting { Key = "PaymentInstructions", Value = "يرجى تحويل رسوم الدورة إلى الحساب البنكي التالي: \n بنك كذا - حساب رقم: 00000000 \n وإرسال صورة الإيصال عبر الواتساب.", ValueEn = "Please transfer the course fees to the following bank account: \n Bank XYZ - Account No: 00000000 \n and send the receipt image via WhatsApp.", Group = "Payment", DisplayName = "تعليمات الدفع" },

                    // --- إعدادات نصوص الشهادة المعتمدة (الجديدة) ---
                    new SiteSetting { Key = "Cert_Title", Value = "شهادة إتمام", ValueEn = "Certificate of Completion", Group = "Certificate", DisplayName = "العنوان الرئيسي" },
                    new SiteSetting { Key = "Cert_Subtitle", Value = "Certificate of Completion", ValueEn = "شهادة إتمام", Group = "Certificate", DisplayName = "العنوان الفرعي" },
                    new SiteSetting { Key = "Cert_Intro", Value = "يشهد مركز أبستراكشن للتدريب والاستشارات بأن الفاضل/ـة:", ValueEn = "Abstraction Center for Training & Consulting certifies that:", Group = "Certificate", DisplayName = "النص الافتتاحي" },
                    new SiteSetting { Key = "Cert_Statement1", Value = "قد اجتاز/ت بنجاح كافة متطلبات المسار التدريبي التخصصي المكثف بعنوان:", ValueEn = "Has successfully completed all requirements of the intensive specialized training track titled:", Group = "Certificate", DisplayName = "نص اجتياز المتطلبات" },
                    new SiteSetting { Key = "Cert_Statement2", Value = "بمعدل ساعات تدريبية معتمدة، مع إتمام كافة المشاريع العملية والتقييمات المقررة بنجاح،", ValueEn = "with accredited training hours, successfully completing all practical projects and scheduled assessments,", Group = "Certificate", DisplayName = "نص الساعات والمشاريع" },
                    new SiteSetting { Key = "Cert_DateText", Value = "وذلك في تاريخ", ValueEn = "on the date of", Group = "Certificate", DisplayName = "نص التاريخ" },
                    new SiteSetting { Key = "Cert_AdminName", Value = "إدارة الأكاديمية", ValueEn = "Academy Administration", Group = "Certificate", DisplayName = "اسم جهة الاعتماد" },
                    new SiteSetting { Key = "Cert_AdminTitle", Value = "مركز أبستراكشن للتدريب", ValueEn = "Abstraction Training Center", Group = "Certificate", DisplayName = "صفة جهة الاعتماد" },
                    new SiteSetting { Key = "Cert_ScanText", Value = "امسح للتحقق من الموثوقية", ValueEn = "Scan to Verify", Group = "Certificate", DisplayName = "نص الـ QR Code" }
                };
                context.SiteSettings.AddRange(defaultSettings);
                await context.SaveChangesAsync();
            }

            // =================================================================
            // 4. ضخ البيانات التجريبية (Dummy Data) إذا كانت قاعدة البيانات فارغة
            // =================================================================
            if (!context.Courses.Any())
            {
                // أ. إنشاء حساب محاضر تجريبي
                var instructorUser = new ApplicationUser
                {
                    UserName = "instructor@test.com",
                    Email = "instructor@test.com",
                    FullName = "د. أحمد خليل",
                    Specialization = "خبير هندسة البرمجيات",
                    ProfilePicture = "https://ui-avatars.com/api/?name=Ahmed&background=0b1120&color=d4af37",
                    EmailConfirmed = true,
                    IsActive = true
                };
                if (await userManager.FindByEmailAsync(instructorUser.Email) == null)
                {
                    await userManager.CreateAsync(instructorUser, "Inst@123");
                    await userManager.AddToRoleAsync(instructorUser, "Instructor");
                }
                var instId = (await userManager.FindByEmailAsync(instructorUser.Email)).Id;

                // ب. إنشاء حساب طالب تجريبي
                var studentUser = new ApplicationUser
                {
                    UserName = "student@test.com",
                    Email = "student@test.com",
                    FullName = "محمد إبراهيم السيد",
                    Specialization = "طالب جامعي",
                    ProfilePicture = "https://ui-avatars.com/api/?name=Mohamed&background=f1f5f9&color=0b1120",
                    EmailConfirmed = true,
                    IsActive = true
                };
                if (await userManager.FindByEmailAsync(studentUser.Email) == null)
                {
                    await userManager.CreateAsync(studentUser, "Student@123");
                    await userManager.AddToRoleAsync(studentUser, "Student");
                }
                var studId = (await userManager.FindByEmailAsync(studentUser.Email)).Id;

                // ج. إنشاء دورة تدريبية (Course)
                var course = new Course
                {
                    Title = "دبلومة هندسة البرمجيات الشاملة",
                    Description = "تغطي هذه الدبلومة كافة المفاهيم الحديثة في تطوير الويب والبرمجيات باستخدام أحدث التقنيات وأفضل الممارسات في سوق العمل.",
                    ImageUrl = "https://images.unsplash.com/photo-1498050108023-c5249f4df085?auto=format&fit=crop&w=800&q=80",
                    CreatedAt = DateTime.Now.AddDays(-30)
                };
                context.Courses.Add(course);
                await context.SaveChangesAsync();

                // د. إنشاء دفعة مسندة للمحاضر (Batch)
                var batch = new Batch
                {
                    CourseId = course.Id,
                    InstructorId = instId,
                    BatchName = "دفعة رواد التقنية - 2026",
                    StartDate = DateTime.Now.AddDays(-15),
                    Status = BatchStatus.InProgress
                };
                context.Batches.Add(batch);
                await context.SaveChangesAsync();

                // هـ. تسجيل الطالب في الدفعة
                var enrollment = new StudentBatch
                {
                    BatchId = batch.Id,
                    StudentId = studId,
                    Status = StudentAcademicStatus.Studying,
                    EnrollmentDate = DateTime.Now.AddDays(-14)
                };
                context.StudentBatches.Add(enrollment);
                await context.SaveChangesAsync();

                // و. إنشاء المنهج (الوحدات والمحتوى)
                var lesson1 = new Lesson { BatchId = batch.Id, Title = "الوحدة الأولى: أساسيات البرمجة", Order = 1 };
                var lesson2 = new Lesson { BatchId = batch.Id, Title = "الوحدة الثانية: تطوير الويب (Frontend)", Order = 2 };
                context.Lessons.AddRange(lesson1, lesson2);
                await context.SaveChangesAsync();

                context.LessonContents.AddRange(
                    new LessonContent { LessonId = lesson1.Id, Title = "مقدمة تعريفية بالمسار", Type = ContentType.Video, VideoUrl = "https://www.youtube.com/embed/dQw4w9WgXcQ", Order = 1 },
                    new LessonContent { LessonId = lesson1.Id, Title = "ملف العرض التقديمي (PDF)", Type = ContentType.Pdf, FilePath = "https://www.w3.org/WAI/ER/tests/xhtml/testfiles/resources/pdf/dummy.pdf", Order = 2 },
                    new LessonContent { LessonId = lesson2.Id, Title = "بناء واجهات المستخدم", Type = ContentType.Video, VideoUrl = "https://www.youtube.com/embed/tgbNymZ7vqY", Order = 1 },
                    new LessonContent { LessonId = lesson2.Id, Title = "التكليف الأول: بناء صفحة هبوط", Type = ContentType.Assignment, Description = "قم ببناء صفحة هبوط باستخدام HTML و CSS وارفع الملف مضغوطاً هنا.", Order = 2 }
                );
                await context.SaveChangesAsync();

                // ز. إنشاء اختبار نهائي
                var finalExam = new FinalExam { CourseId = course.Id, PassingScorePercentage = 70 };
                context.FinalExams.Add(finalExam);
                await context.SaveChangesAsync();

                batch.FinalExamId = finalExam.Id;
                await context.SaveChangesAsync();

                context.ExamQuestions.AddRange(
                    new ExamQuestion { FinalExamId = finalExam.Id, QuestionText = "أي من اللغات التالية تُستخدم لتنسيق صفحات الويب؟", Option1 = "HTML", Option2 = "CSS", Option3 = "Python", Option4 = "Java", CorrectOption = 2 },
                    new ExamQuestion { FinalExamId = finalExam.Id, QuestionText = "ما هو الرمز المستخدم لتعريف المتغيرات في لغة C#؟", Option1 = "var", Option2 = "let", Option3 = "def", Option4 = "dim", CorrectOption = 1 }
                );
                await context.SaveChangesAsync();

                // ح. إنشاء طلب توظيف محاضر (للأدمن)
                context.InstructorApplications.Add(new InstructorApplication
                {
                    FullName = "م. سارة محمود",
                    Email = "sara@test.com",
                    PhoneNumber = "01012345678",
                    Specialization = "إدارة الأعمال والتسويق",
                    Status = RequestStatus.Pending,
                    AppliedAt = DateTime.Now.AddDays(-2)
                });

                // ط. إنشاء طلب تسجيل لطالب جديد (للأدمن)
                context.RegistrationRequests.Add(new RegistrationRequest
                {
                    StudentId = studId,
                    BatchId = batch.Id,
                    FullName = "علي حسن يوسف",
                    Specialization = "محاسب",
                    Level = "مبتدئ",
                    WhatsAppNumber = "01198765432",
                    Status = RequestStatus.Pending,
                    RequestDate = DateTime.Now.AddDays(-1)
                });

                // ي. إصدار شهادة تجريبية معتمدة للطالب (تظهر في السجل وللطالب)
                context.Certificates.Add(new Certificate
                {
                    StudentId = studId,
                    BatchId = batch.Id,
                    IsApproved = true,
                    IssueDate = DateTime.Now,
                    UniqueSerialNumber = "TEST-CERT-2026"
                });

                await context.SaveChangesAsync();
            }
        }
    }
}