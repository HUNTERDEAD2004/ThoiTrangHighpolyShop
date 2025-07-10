using AppData.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppData.ViewModels.SanPham
{
    public class ChiTietSanPhamUpdateRequest
    {
        public Guid IDSanPham { get; set; }
        public string? Ma { get; set; } // Mã sản phẩm
        public List<ChiTietSanPhamRequest> ChiTietSanPhams { get; set; } = new();
        public string? TrangThai { get; set; }
        public int? Location { get; set; } // Có thể dùng cho việc redirect hay thông báo vị trí UI

        // Thông tin hiển thị cho frontend (tùy chọn)
        public List<MauSac>? MauSacs { get; set; } // OK nếu dùng để hiển thị ra tên màu sắc
    }

}
