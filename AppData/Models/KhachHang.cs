using AppData.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppData.Models
{
    public class KhachHang
    {
        public Guid IDKhachHang { get; set; }
        public string? MaKhachHang { get; set; }
        [Required]
        public string? Ten { get; set; }
        [Required]
        public string? Password { get; set; }
        public int? GioiTinh { get; set; }

        [JsonConverter(typeof(DateOnlyJsonConverterNewtonsoft))]
        public DateOnly? NgaySinh { get; set; }
        [EmailAddress]
        public string? Email { get; set; }
        public string? SDT { get; set; }
        public int? DiemTich { get; set; }
        public int? TrangThai { get; set; }
        public virtual GioHang? GioHang { get; set; }
        public virtual IEnumerable<HoaDon>? HoaDons { get; set; }
        public virtual IEnumerable<DiaChi>? DiaChi { get; set; }
       public virtual IEnumerable<DanhGia>? DanhGias { get; set; }
      public virtual ICollection<EmailVerificationToken> EmailVerificationTokens { get; set; }
    }
}
