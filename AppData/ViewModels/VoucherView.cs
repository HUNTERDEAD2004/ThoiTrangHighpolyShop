using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppData.ViewModels
{
    public  class VoucherView
    {
        [Key]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Mời bạn nhập tên voucher")]
        [StringLength(40, ErrorMessage = "Tên không được quá 40 kí tự")]
        public string Ten { get; set; }

        [Required(ErrorMessage = "Mời bạn nhập mã voucher")]
        [StringLength(20, ErrorMessage = "Mã voucher không được quá 20 kí tự")]
        public string MaVoucher { get; set; }
        [Required(ErrorMessage = "Mời bạn chọn hình thức giảm giá")]
        [Range(0, 1, ErrorMessage = "Hình thức chỉ nhận 0 (tiền mặt) hoặc 1 (phần trăm)")]
        public int HinhThucGiamGia { get; set; }//0 là giảm theo %, 1 là giảm thẳng giá tiền
        [Required(ErrorMessage = "mời bạn nhập dữ liệu")]
        
        public int SoTienCan { get; set; }
        [Required(ErrorMessage = "Mời bạn nhập giá trị giảm")]
  

        public int GiaTri { get; set; }
        [Required(ErrorMessage = "Mời bạn nhập giá trị tối thiểu")]
        [Range(10000, double.MaxValue, ErrorMessage = "Giá trị tối thiểu phảitừ 10.000 trở lên")]

        public decimal GiaTriToiThieu { get; set; } // Số tiền tối thiểu để áp dụng voucher
        [Range(0, double.MaxValue, ErrorMessage = "Giảm tối đa phải >= 0")]
        public decimal GiaTriToiDa { get; set; } // Số tiền tối đa được giảm khi áp dụng voucher    
        [Required(ErrorMessage = "mời bạn nhập dữ liệu")]
        public DateTime NgayApDung { get; set; }
        [Required(ErrorMessage = "mời bạn nhập dữ liệu")]
        public DateTime NgayKetThuc { get; set; }
 
      
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int SoLuong { get; set; }
        
        public string? MoTa { get; set; }
        [Required(ErrorMessage = "mời bạn nhập dữ liệu")]
        public int TrangThai { get; set; }
        // Chế độ hiển thị: true = công khai, false = riêng tư
        public bool IsPublic { get; set; }
    }
}
