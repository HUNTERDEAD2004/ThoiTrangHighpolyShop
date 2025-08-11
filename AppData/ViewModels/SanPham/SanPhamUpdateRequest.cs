using AppData.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppData.ViewModels.SanPham
{
    public class SanPhamUpdateRequest
    {
        public Guid ID { get; set; }
        [Required]
        public string? Ten { get; set; }
        public string? AnhDaiDien { get; set; }
        public string? MoTa { get; set; }

        // CHUYỂN TỪ tên → ID
        [Required]
        public Guid IDChatLieu { get; set; }
        [Required]
        public Guid IDLoaiSP{ get; set; }
    }
}
