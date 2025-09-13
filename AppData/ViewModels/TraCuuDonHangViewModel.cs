using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppData.ViewModels
{
    public class TraCuuDonHangViewModel
    {
        public string? MaHoaDon { get; set; }
        public string? TenNguoiNhan { get; set; }
        public string? SDT { get; set; }
        public string? DiaChi { get; set; }
        public int TrangThaiGiaoHang { get; set; }
        public decimal TongTien { get; set; }
        public DateTime? NgayNhanHang { get; set; }
        public string? GhiChu { get; set; }
    }
}
