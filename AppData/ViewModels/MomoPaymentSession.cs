using AppData.ViewModels.BanOffline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppData.ViewModels
{
    public class MomoPaymentSession
    {
        public Guid HoaDonId { get; set; }
        public string OrderId { get; set; }
        public HoaDonThanhToanRequest RequestData { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}
