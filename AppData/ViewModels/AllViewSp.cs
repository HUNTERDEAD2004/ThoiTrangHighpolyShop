using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppData.ViewModels
{
    public  class AllViewSp
    {
        [Key]
        public Guid ID { get; set; }
        [StringLength(40, ErrorMessage = "Ten san pham khong duoc dai qua 40 tu.")]
        public string? MaSP { get; set; }
        public int SoLuongCTSP { get; set; }
        public string Ten { get; set; }
        [Required]
        public string MoTa { get; set; }
        public string? TenAnh { get; set; }  
        public int TrangThai { get; set; }
        public int TongSoSao { get; set; }
        public int TongDanhGia { get; set; }
        public Guid? IdKhuyenMai { get; set; }
        public int GiaBan { get; set; }
        public Guid IDLoaiSP { get; set; }
        public Guid? IDLoaiSPCha { get; set; }  
        public Guid IDChatLieu { get; set; }
    }
}
