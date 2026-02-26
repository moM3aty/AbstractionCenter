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

            // 3. إنشاء إعدادات الموقع الأساسية (تم اختصارها هنا للتركيز على البيانات التجريبية)
            if (!context.SiteSettings.Any())
            {
                var defaultSettings = new List<SiteSetting>
                {
                    new SiteSetting { Key = "SiteName", Value = "مركز أبستراكشن", ValueEn = "Abstraction Center", Group = "General", DisplayName = "اسم الموقع" },
                    new SiteSetting { Key = "FooterDesc", Value = "منصتك الأولى للتدريب وبناء القدرات المهنية وفق معايير عالمية.", ValueEn = "Your premier platform for training and professional capacity building.", Group = "General", DisplayName = "وصف الفوتر" },
                    new SiteSetting { Key = "ContactPhone", Value = "+20 100 000 0000", ValueEn = "+20 100 000 0000", Group = "Contact", DisplayName = "رقم الهاتف" },
                    new SiteSetting { Key = "ContactEmail", Value = "info@abstraction.com", ValueEn = "info@abstraction.com", Group = "Contact", DisplayName = "البريد الإلكتروني" }
                    // يمكنك إضافة باقي الإعدادات هنا كما كانت سابقاً
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