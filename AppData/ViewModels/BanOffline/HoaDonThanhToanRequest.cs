using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppData.ViewModels.BanOffline
{
    public class HoaDonThanhToanRequest
    {
        public Guid Id { get; set; }
        public Guid IdNhanVien { get; set; }
        public Guid IDPhuongThucThanhToan { get; set; }
        public DateTime NgayThanhToan { get; set; }
        public Guid IdVoucher { get; set; }
        //them tien ship va dia chi , ten va sdt nguoi nhan 
        public string tenNguoiNhan { get; set; } = string.Empty;
        public string sdtNguoiNhan { get; set; } = string.Empty;
        public decimal TienShip { get; set; }
        public string diaChi { get; set; } = string.Empty;
        public decimal TongTien { get; set; } // Khách phải trả
        public int DiemTichHD { get; set; }
        public int DiemSD { get; set; }
        public int TrangThai { get; set; }
        public string? GhiChu { get; set; }
    }
}
