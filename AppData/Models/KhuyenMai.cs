namespace AppData.Models
{
    public class KhuyenMai
    {
        public Guid ID { get; set; }
        public string? Ten { get; set; }
        public string? MaKhuyenMai { get; set; } 
        public int GiaTri { get; set; }
        public DateTime NgayApDung { get; set; }
        public DateTime NgayKetThuc { get; set; }
        public int KieuGiamGia { get; set; } // "Tiền mặt" hoặc "Phần trăm"
        public string? MoTa { get; set; }
        public int TrangThai { get; set; }// 0 là chưa hoạt động, 1 là hoạt động, 2 là  hết hạn.< còn 2 trường hợp là áp dụng sơm và kết thúc sớm
                                          // là khi admin chọn chức năng này thì sẽ tự động chuyển trạng thái từ chưa hoạt động -> hoạt động và hoạt động -> hết hạn >
        public virtual IEnumerable<ChiTietSanPham>? ChiTietSanPhams { get; set; }
        
    }
}
