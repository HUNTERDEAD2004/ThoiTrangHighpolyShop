using AppData.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppData.ViewModels.BanOffline
{
    public class SanPhamBanHang
    {
        public Guid Id { get; set; }
        public string MaSP { get; set; }
        public string Anh { get; set; }
        public string Ten { get; set; }
        public Guid IdLsp { get; set; }
        public decimal? GiaBan { get; set; }
        public decimal? GiaGoc { get; set; }

    }
}
