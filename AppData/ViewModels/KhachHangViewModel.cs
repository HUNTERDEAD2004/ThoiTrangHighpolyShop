using AppData.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppData.ViewModels
{
    public class KhachHangViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]

       
        public string? Email { get; set; }

        public string? MaKhachHang { get; set; }

        public int? GioiTinh { get; set; } // 0: Nam, 1: Nữ

        public string? NgaySinh { get; set; }

        public string? DiaChi { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên")]
        public string? Ten { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [RegularExpression(@"^0[0-9]{9}$", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? SDT { get; set; }

       // public string? DiaChi { get; set; }

        public string? Tinh { get; set; }
        public string? Huyen { get; set; }
        public string? Quan { get; set; }
        public string? Xa { get; set; }
        public string? DiaChiChiTiet { get; set; }

       
        public string? Password { get; set; }
      
        public string? ConfirmPassword { get; set; }
        public int? DiemTich { get; set; }
        public int? TrangThai { get; set; }
    }
}
