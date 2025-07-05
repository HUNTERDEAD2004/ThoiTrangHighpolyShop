using AppData.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppData.ViewModels
{
   public class EmailVerificationToken
    {  
            public Guid ID { get; set; } = Guid.NewGuid();

            public Guid IDKhachHang { get; set; } 

            public string Token { get; set; }

            public DateTime ExpiryTime { get; set; }

            public virtual KhachHang KhachHang { get; set; }     

    }
}
