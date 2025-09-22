using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppData.ViewModels
{
    public class PaymentResultViewModel
    {
        public bool Success { get; set; }
        public string OrderId { get; set; }
        public string Amount { get; set; }
        public string Message { get; set; }
    }
}
