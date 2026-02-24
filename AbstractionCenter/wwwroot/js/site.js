// wwwroot/js/site.js
// السكريبت الأساسي للتحكم في تفاعلات الواجهة الأمامية

document.addEventListener("DOMContentLoaded", function () {

    // 1. تأثير الـ Navbar عند النزول بالصفحة
    const navbar = document.querySelector('.navbar');
    if (navbar) {
        window.addEventListener('scroll', () => {
            if (window.scrollY > 50) {
                navbar.style.background = 'rgba(255, 255, 255, 0.95)';
                navbar.style.boxShadow = '0 4px 15px rgba(0,0,0,0.1)';
            } else {
                navbar.style.background = 'rgba(255, 255, 255, 0.7)';
                navbar.style.boxShadow = '0 4px 6px -1px rgba(0, 0, 0, 0.1)';
            }
        });
    }

    // 2. تفعيل التبويبات (Tabs) في صفحة تفاصيل الدورة (CourseDetails)
    const tabs = document.querySelectorAll('.course-nav button');
    if (tabs.length > 0) {
        tabs.forEach(tab => {
            tab.addEventListener('click', function () {
                // إزالة التفعيل عن كل التبويبات
                tabs.forEach(t => t.classList.remove('active'));
                // تفعيل التبويب المضغوط
                this.classList.add('active');

                // هنا يمكن إضافة كود لإظهار وإخفاء المحتوى بناءً على التبويب
                // (مثل تبديل عرض قسم "المحتوى" وقسم "الواجبات")
            });
        });
    }

});