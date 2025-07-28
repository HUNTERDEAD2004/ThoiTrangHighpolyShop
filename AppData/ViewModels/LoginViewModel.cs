using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AppData.ViewModels
{
    public class LoginViewModel
    {

        [JsonPropertyName("IDKhachHang")]
        public Guid Id { get; set; }

            [EmailAddress]
            public string Email { get; set; }

            public string Ten { get; set; }

            public string SDT { get; set; }

            public string? DiaChi { get; set; }

            public int? DiemTich { get; set; } // Chỉ áp dụng cho Khách hàng

            public int? GioiTinh { get; set; }

       
        public string? NgaySinh { get; set; }

            /// <summary>
            /// 0 - Nhân viên, 1 - Khách hàng
            /// </summary>
            public int? vaiTro { get; set; }

            public bool IsAccountLocked { get; set; } // Tài khoản bị khóa?

            public string Message { get; set; } = string.Empty; // Thông báo hoặc trạng thái login
        

    }
}
