using System.ComponentModel.DataAnnotations;

namespace AppData.Models
{
    public class SanPham
    {
        public Guid ID { get; set; }
        [StringLength(200, ErrorMessage = "Ten san pham khong duoc dai qua 40 tu.")]
        public string? Ten { get; set; }
        public string? Ma { get; set; }
        public string? MoTa { get; set; }
        public int TrangThai { get; set; }
        public Guid IDLoaiSP { get; set; }
        public Guid IDChatLieu { get; set; }
        public virtual LoaiSP? LoaiSP { get; set; }
        public virtual ChatLieu? ChatLieu { get; set; }
        public virtual IEnumerable<ChiTietSanPham>? ChiTietSanPhams { get; set; }
    }
}
