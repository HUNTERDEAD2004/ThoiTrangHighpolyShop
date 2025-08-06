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
        public Guid IDMauSac { get; set; } // Màu đại diện
        public Guid? IDAnh { get; set; }   // Nếu cần xóa ảnh
        public List<Guid> DanhSachIDChiTietSP { get; set; } = new();
        public string MaMau { get; set; } = string.Empty;
        public string TenMau { get; set; } = string.Empty;
        public List<string> DuongDanAnh { get; set; } = new(); // Ảnh đã có
        public List<Guid> DanhSachIDAnh { get; set; } = new(); // ✅ Thêm dòng này
    }
}
