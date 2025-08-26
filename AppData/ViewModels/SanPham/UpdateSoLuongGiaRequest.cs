using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppData.ViewModels.SanPham
{
    public class UpdateSoLuongGiaRequest
    {
        public List<Guid> Ids { get; set; }
        public decimal? GiaGoc { get; set; }
        public int? SoLuong { get; set; }
    }
}
