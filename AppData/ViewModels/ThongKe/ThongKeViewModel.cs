using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppData.ViewModels.ThongKe
{
	public class SanPhamThongKeViewModel
	{
		public string TenSP { get; set; }
		public int SoLuong { get; set; }
		public decimal DoanhThu { get; set; }
	}
	public class ThongKeViewModel
    {
        public int SoLuongThanhVien { get; set; }
        public int SoLuongDonHang { get; set; }//Don Hang Cho
        public int SoLuongSanPham { get; set; }
		public int SoDonHangMoi { get; set; }   // Đơn hàng mới hôm nay
		public List<SanPhamThongKeViewModel> ThongKeSanPham { get; set; } // Sản phẩm bán gần đây
		public List<ThongKeCotViewModel> BieuDoCot { get; set; }
        public List<ThongKeDuongViewModel> BieuDoDuong { get; set; }
        public List<ThongKeTronViewModel> BieuDoTron { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
    }
}
