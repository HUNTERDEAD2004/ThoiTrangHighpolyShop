using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppData.ViewModels.SanPham
{
    public class DonMuaSuccessViewModel
    {
        public string ID { get; set; }
        public string Ten { get; set; }
        public string Email { get; set; }
        public string SDT { get; set; }
        public string DiaChi { get; set; }
        public string? MaHoaDon { get; set; }   
        public string PhuongThucThanhToan { get; set; }
        public Guid? IDPhuongThucTT { get; set; } // ➕ nếu frontend cần

        public decimal TongTien { get; set; } // ✅ sửa từ int → decimal
        public int DiemTich { get; set; }
        public int DiemSuDung { get; set; }

        public Guid IDKhachHang { get; set; }
        public string MaVoucher { get; set; }
        public bool Login { get; set; }
        public string GhiChu { get; set; }
        public Guid? IDDiaChi { get; set; }
        public int ErrorCode { get; set; } = 0;

      
        public string ErrorMessage { get; set; } = "";
        public List<GioHangRequest> GioHangs { get; set; }
    }

}