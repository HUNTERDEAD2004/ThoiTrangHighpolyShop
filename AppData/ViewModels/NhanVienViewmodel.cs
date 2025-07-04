using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppData.ViewModels
{
    public class NhanVienViewModel
    {
   
        [Required]
        //[RegularExpression(@"^[\p{L} \.'\-]{2,50}$", ErrorMessage = "Tên không hợp lệ.")]
        public string Ten { get; set; }

        [Required]
        //[RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d@$!%*?&]{6,20}$", ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự, bao gồm cả chữ và số.")]
        public string Password { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [RegularExpression(@"^(0|\+84)[0-9]{9}$", ErrorMessage = "Số điện thoại không hợp lệ.")]
        public string SDT { get; set; }

        [MaxLength(100)]
        public string? DiaChi { get; set; }

        //[RegularExpression(@"^[A-Z0-9]{3,20}$", ErrorMessage = "Mã nhân viên phải viết hoa và có từ 3-20 ký tự.")]
        public string? MaNhanVien { get; set; }

        public DateOnly? NgaySinh { get; set; }

        
        public int? GioiTinh { get; set; }

        public int? TrangThai { get; set; }

       
        public Guid IDVaiTro { get; set; }
    }

}

