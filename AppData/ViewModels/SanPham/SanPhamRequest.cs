using AppData.Models;
using Microsoft.AspNetCore.Http;
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
        [Display(Name = "Tên sản phẩm")]
        public string Ten { get; set; } = string.Empty;
        [Display(Name = "Mô tả")]
        public string? MoTa { get; set; }
        [Display(Name = "Ảnh đại diện")]
        public string? AnhDaiDien { get; set; }
        public string MaSanPham { get; set; } = string.Empty;

        public Guid IDChatLieu { get; set; }
        public Guid IDLoaiSPCha { get; set; }
        public Guid? IDLoaiSPCon{ get; set; }

        public List<Guid> IDMauSacs { get; set; } = new();
        public List<Guid> IDKichCos { get; set; } = new();
    }


}
