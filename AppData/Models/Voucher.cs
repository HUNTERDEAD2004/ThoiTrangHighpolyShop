namespace AppData.Models
{
    public class Voucher
    {
        public Guid ID { get; set; }
        public string? Ten { get; set; }
        public string? MaVoucher { get; set; }
        public int HinhThucGiamGia { get; set; }//1 là giảm theo %, 0 là giảm thẳng giá tiền
        public decimal GiaTriToiThieu { get; set; } // Số tiền tối thiểu để áp dụng voucher 
        public decimal GiaTriToiDa { get; set; } // Số tiền tối đa được giảm khi áp dụng voucher`
        public int GiaTri { get; set; }
        public DateTime NgayApDung { get; set; }
        public DateTime NgayKetThuc { get; set; }
        public int SoLuong { get; set; }
        public string? MoTa { get; set; }
        public int TrangThai { get; set; } // 0 là chưa hoạt động, 1 là hoạt động, 2 là  hết hạn.< còn 2 trường hợp là áp dụng sơm và kết thúc sớm
                                           // là khi admin chọn chức năng này thì sẽ tự động chuyển trạng thái từ chưa hoạt động -> hoạt động và hoạt động -> hết hạn >
        public virtual IEnumerable<HoaDon>? HoaDons { get; set; }
        //Git
    }
}
