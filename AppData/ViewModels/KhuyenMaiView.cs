using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AppData.ViewModels
{
    public class KhuyenMaiView : IValidatableObject
    {
        [Key]
        public Guid ID { get; set; }

        [Required(ErrorMessage = "Mời bạn nhập tên khuyến mãi")]
        [StringLength(40, ErrorMessage = "Tên không được quá 40 ký tự")]
        public string Ten { get; set; }

        [Required(ErrorMessage = "Mời bạn nhập mã voucher")]
        [StringLength(50, ErrorMessage = " MaKhuyenMai không được quá 50 ký tự")]
        public string MaKhuyenMai { get; set; }

        [Required(ErrorMessage = "Mời bạn nhập giá trị")]
        [Range(1, 1000000000, ErrorMessage = "Giá trị phải lớn hơn 0")]
        public int GiaTri { get; set; }

        [Required(ErrorMessage = "Mời bạn nhập ngày áp dụng")]
        [DataType(DataType.DateTime)]
        public DateTime NgayApDung { get; set; }

        [Required(ErrorMessage = "Mời bạn nhập ngày kết thúc")]
        [DataType(DataType.DateTime)]
        public DateTime NgayKetThuc { get; set; }

        [StringLength(200, ErrorMessage = "Mô tả không được quá 200 ký tự")]
        public string? MoTa { get; set; }

        [Range(0, 2, ErrorMessage = "Trạng thái không hợp lệ")]
        public int TrangThai { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (NgayKetThuc < NgayApDung)
            {
                yield return new ValidationResult("Ngày kết thúc phải lớn hơn hoặc bằng ngày áp dụng", new[] { nameof(NgayKetThuc) });
            }

            if (TrangThai == 1 && (GiaTri <= 10 || GiaTri > 50))
            {
                yield return new ValidationResult("Nếu giảm theo phần trăm, giá trị phải từ 1 đến 100", new[] { nameof(GiaTri) });
            }
        }
    }
}
