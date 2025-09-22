using System;
using System.ComponentModel.DataAnnotations;

namespace AppData.ViewModels
{
    public class NhanVienViewModel
    {
        [Required(ErrorMessage = "Tên không được để trống")]
        public string Ten { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [RegularExpression(@"^(0|\+84)[0-9]{9}$", ErrorMessage = "Số điện thoại không hợp lệ.")]
        public string SDT { get; set; }

        [MaxLength(100, ErrorMessage = "Địa chỉ tối đa 100 ký tự")]
        public string? DiaChi { get; set; }

        public string? MaNhanVien { get; set; }

        /// <summary>
        /// Ngày sinh hiển thị dạng string để format dễ hơn
        /// </summary>
        public string? NgaySinh { get; set; }

        public int? GioiTinh { get; set; }

        public int? TrangThai { get; set; }

        public Guid IDVaiTro { get; set; }
    }
}
