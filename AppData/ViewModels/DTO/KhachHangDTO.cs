using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppData.ViewModels.DTO
{
    public class KhachHangDTO
    {
        public Guid IDKhachHang { get; set; }
        public string? Ten { get; set; }
        public string? Email { get; set; }
        public string? SDT { get; set; }
        public int? GioiTinh { get; set; }
        public string? NgaySinh { get; set; } 
        public int? DiemTich { get; set; }
        public string? DiaChi { get; set; }
    }

}
