namespace AppData.Models
{
    public class ChiTietGioHang
    {
        public Guid ID { get; set; }
        public int SoLuong { get; set; }
        public Guid IDCTSP { get; set; }
        public Guid IDKhachHang { get; set; }
        public virtual ChiTietSanPham? ChiTietSanPham { get; set; }
        public virtual GioHang? GioHang { get; set; }
    }
}
