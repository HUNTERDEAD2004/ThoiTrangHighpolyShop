using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppData.ViewModels
{
    public class NhanVienViewModel
    {
        public string Ten { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string SDT { get; set; }
        public string? DiaChi { get; set; }
        public string? MaNhanVien { get; set; }
        public DateTime? NgaySinh { get; set; }
        public int? GioiTinh { get; set; } // 1: Nam, 0: Nữ
        public int? TrangThai { get; set; } // 0: Không hoạt động, 1: Hoạt động
        public Guid IDVaiTro { get; set; }
    }
}
