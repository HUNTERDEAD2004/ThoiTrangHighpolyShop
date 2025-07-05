using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppData.Models
{
    public class UserVoucher
    {
        public Guid ID { get; set; }
        public Guid IDKhachHang { get; set; }
        public Guid IDVoucher { get; set; }
        public bool DaSuDung { get; set; }
        public DateOnly? NgaySuDung { get; set; }
        public virtual KhachHang? KhachHang { get; set; }
        public virtual Voucher? Voucher { get; set; }
    }
}
