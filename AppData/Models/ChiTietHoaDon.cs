namespace AppData.Models
{
    public class ChiTietHoaDon
    {
        public Guid ID { get; set; }
        public decimal DonGia { get; set; }
        public int SoLuong { get; set; }
        public int TrangThai { get; set; }
        public Guid IDCTSP { get; set; }
        public Guid IDHoaDon { get; set; }
        public virtual HoaDon? HoaDon { get; set; }
        public virtual ChiTietSanPham? ChiTietSanPham { get; set; }
    }
}
