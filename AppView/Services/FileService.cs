using AppData.ViewModels.SanPham;
using AppView.IServices;
using Microsoft.Extensions.Hosting;

namespace AppView.Services
{
    public class FileService : IFileService
    {
        public async Task<string> AddFile(IFormFile file, string wwwRootPath)
        {
            if (file == null || file.Length == 0) return string.Empty;
			var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
			string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
			if (!allowedExtensions.Contains(extension))
			{
				return string.Empty; // hoặc throw exception nếu muốn
			}
			// Tạo tên file duy nhất
			string fileName = Path.GetFileNameWithoutExtension(file.FileName);
            //string extension = Path.GetExtension(file.FileName);
            string uniqueFileName = $"{fileName}_{DateTime.Now:yyyyMMddHHmmssfff}{extension}";

            // Tạo thư mục nếu chưa có
            string folderPath = Path.Combine(wwwRootPath, "img", "product");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Đường dẫn tuyệt đối
            string fullPath = Path.Combine(folderPath, uniqueFileName);

            // Lưu file
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Trả về đường dẫn tương đối cho view/controller
            return $"/img/product/{uniqueFileName}".Replace("\\", "/");
        }

        public bool DeleteFile(string filePath, string wwwRootPath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) return false;

            try
            {
                // Xử lý path để tránh lỗi sai đường dẫn
                string relativePath = filePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString());
                string fullPath = Path.Combine(wwwRootPath, relativePath);

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                // Log nếu cần
                Console.WriteLine($"Lỗi khi xóa file: {ex.Message}");
                return false;
            }
        }
    }
}
