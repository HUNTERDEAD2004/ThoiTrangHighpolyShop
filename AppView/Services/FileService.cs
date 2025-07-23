using AppData.ViewModels.SanPham;
using AppView.IServices;
using Microsoft.Extensions.Hosting;

namespace AppView.Services
{
    public class FileService : IFileService
    {
        public async Task<string> AddFile(IFormFile file, string wwwRootPath)
        {
            string fileName = Path.GetFileNameWithoutExtension(file.FileName);
            string extension = Path.GetExtension(file.FileName);
            fileName = fileName + DateTime.Now.ToString("yyyyMMddHHmmssfff") + extension;

            // Tạo thư mục nếu chưa có
            string folderPath = Path.Combine(wwwRootPath, "img", "product");

            // Lưu file
            string filePath = Path.Combine(folderPath, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Trả về đường dẫn tương đối dùng cho View
            return Path.Combine("/img/product/", fileName).Replace("\\", "/");
        }

        public bool DeleteFile(string filePath, string wwwRootPath)
        {
            try
            {
                string fullPath = Path.Combine(wwwRootPath, filePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

}
