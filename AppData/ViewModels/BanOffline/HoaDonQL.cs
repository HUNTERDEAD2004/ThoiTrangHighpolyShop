using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppData.ViewModels.BanOffline
{
    public class HoaDonQL
    {
        public Guid Id { get; set; }
        public string MaHD { get; set; }
        public DateTime? ThoiGian { get; set; }
        public string? KhachHang { get; set; }
        public string? SDTKH { get; set; }
        public string? SDTnhanhang { get; set; }
        public decimal TongTienHang { get; set; }
        public decimal? KhachDaTra { get; set; }

        public decimal? KhachCanTra { get; set; } // BỔ SUNG
        public decimal? TienThua => (KhachDaTra ?? 0) - (KhachCanTra ?? 0); // BỔ SUNG (nếu cần)

        public string PTTT { get; set; }
        public int TrangThai { get; set; }
        public int LoaiHD { get; set; }
    }
}
