using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppData.Models
{
   public class PhuongThucThanhToan
    {
        public Guid IDPTTT { get; set; }

        public string? TenPTTT { get; set; } // Tên phương thức thanh toán

        public virtual IEnumerable<HoaDon>? HoaDons { get; set; } // Liên kết với hóa đơn
    }
}
