using AppData.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppData.ViewModels.SanPham
{
    public class ChiTietSanPhamAddRequest
    {
        public Guid IDSanPham { get; set; }
        public List<Guid> IDMauSacs { get; set; } = new();
        public List<Guid> IDKichCos { get; set; } = new();
    }
}
