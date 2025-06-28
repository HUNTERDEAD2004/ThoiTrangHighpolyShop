using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppData.Models
{
    public class DiaChi
    {
        [Key]
        public Guid Id { get; set; }
        public Guid IDKhachHang { get; set; }
        public string? Xa { get; set; }
        public string? Quan { get; set; }
        public string? Huyen { get; set; }
        public bool IsDefault { get; set; } = false;
        public virtual KhachHang? KhachHang { get; set; }
    }
}
