using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AbstractionCenter.Services
{
    // واجهة الخدمة لسهولة الحقن (Dependency Injection)
    public interface IFileUploaderService
    {
        Task<string> UploadFileAsync(IFormFile file, string folderName);
    }

    // التنفيذ الفعلي لخدمة رفع الملفات
    public class FileUploaderService : IFileUploaderService
    {
        private readonly IWebHostEnvironment _env;

        public FileUploaderService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0)
                return null;

            // تحديد مسار المجلد داخل wwwroot
            string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", folderName);

            // إنشاء المجلد إذا لم يكن موجوداً
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // إنشاء اسم فريد للملف لمنع التكرار والتعارض
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // حفظ الملف في المسار
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // إرجاع المسار النسبي لحفظه في قاعدة البيانات
            return $"/uploads/{folderName}/{uniqueFileName}";
        }
    }
}