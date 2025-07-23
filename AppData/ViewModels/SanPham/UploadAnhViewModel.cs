using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AppData.ViewModels.SanPham
{
    public class UploadAnhViewModel
    {
        public Guid IDAnh { get; set; }  // <- Bắt buộc phải có để xoá ảnh
        public Guid IDChiTietSanPham { get; set; }
        public string MaMau { get; set; } = string.Empty;
        public string TenMau { get; set; } = string.Empty;
        [NotMapped]
        public IFormFile? Image { get; set; }
        public string DuongDan { get; set; } = string.Empty;
    }
}
