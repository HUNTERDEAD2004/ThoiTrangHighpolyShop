using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppData.ViewModels.BanOffline
{
    public class ChiTietCTSPBanHang
    {
        public Guid Id { get; set; }
        public string Ten { get; set; }
        public string ChiTiet { get; set; }
        public string Anh { get; set; }
        public decimal GiaGoc { get; set; }
        public decimal GiaBan { get; set; }
        public int SoLuong { get; set; }
        public Guid idMauSac { get; set; }
        public Guid idKichCo { get; set; }
    }
}
