using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppData.Models
{
   public class LichSuHoaDon
    {
        public Guid ID { get; set; }

        public DateTime NgayLap { get; set; }

        public string? GhiChu { get; set; }

        public int TrangThai { get; set; } // 0 - chưa thanh toán, 1 - đã thanh toán, 2 - đã hủy

        public virtual IEnumerable<HoaDon>? HoaDons { get; set; }  
    }
}
