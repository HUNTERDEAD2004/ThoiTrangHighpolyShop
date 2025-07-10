using AppData.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppData.ViewModels.SanPham
{
    public class SanPhamRequest
    {
        public string Ten { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        public Guid IDChatLieu { get; set; }
        public List<Guid> IDMauSacs { get; set; } = new();
        public List<Guid> IDKichCos { get; set; } = new();
        public Guid IDLoaiSPCha { get; set; }
        public Guid? IDLoaiSPCon { get; set; } // nullable
    }

}
