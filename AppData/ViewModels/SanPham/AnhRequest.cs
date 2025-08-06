using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppData.ViewModels.SanPham
{
    public class AnhRequest
    {
        public Guid IDSanPhamChiTiet { get; set; }
        public string DuongDan { get; set; } = string.Empty;
        public string? MaMau { get; set; }
    }
}
