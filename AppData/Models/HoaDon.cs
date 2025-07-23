namespace AppData.Models
{
    public class HoaDon
    {
        public Guid ID { get; set; }
        public string? MaHoaDon { get; set; }
        public Guid? IDNhanVien { get; set; }
        public Guid? IDVoucher { get; set; }
        public Guid IDKhachHang { get; set; } 
        public Guid IDLichSuHD { get; set; }
        public Guid IDPhuongThucTT { get; set; }
        public DateTime NgayTao { get; set; }
        public DateTime? NgayThanhToan { get; set; }
        public DateTime? NgayNhanHang { get; set; }
        public string? TenNguoiNhan { get; set; }
        public string? SDT { get; set; }
        public string? Email { get; set; }
        public string? DiaChi { get; set; }
        public decimal TienShip { get; set; }
        public decimal? TongTien { get; set; }
        public int LoaiHoaDon { get; set; } // Off-1 // On-0
        public string? GhiChu { get; set; } 
        public int TrangThaiGiaoHang { get; set; }
        //Các trạng thái của đơn hàng
        /*
         * 1-đơn nháp
         * 2-Chờ xác nhận
         * 3-Đang giao hàng //ko đc hủy
         * 6-thành công //nhận hàng thành công// đc đánh giá // đc hủy nếu ngày thanh toán < 3
         --Hoàn hàng
         * 9-chờ xác nhận hoàn hàng
         * 4-đang hoàn hàng
         * 5-Hoàn hàng thành công
         --Hủy khi chưa giao
         * 7-Đơn hủy
         * 8-Chờ xác nhận hủy
         */
        public virtual IEnumerable<LichSuTichDiem>? LichSuTichDiems { get; set; }
        public virtual NhanVien? NhanVien { get; set; }
        public virtual Voucher? Voucher { get; set; }

        public virtual LichSuHoaDon? LichSuHoaDons { get; set; }

        public virtual KhachHang? KhachHang { get; set; }

        public virtual PhuongThucThanhToan? PhuongThucThanhToans { get; set; }
        public virtual IEnumerable<ChiTietHoaDon>? ChiTietHoaDons { get; set; }
    }
}
