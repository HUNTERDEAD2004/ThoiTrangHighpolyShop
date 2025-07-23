using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppData.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppData.Configurations
{
    internal class ChiTietSanPhamConfiguration : IEntityTypeConfiguration<ChiTietSanPham>
    {
        public void Configure(EntityTypeBuilder<ChiTietSanPham> builder)
        {
            builder.ToTable("ChiTietSanPham");
            builder.HasKey(x => x.ID);
            builder.Property(x => x.MaSPChiTiet).HasColumnType("nvarchar(100)");
            builder.Property(x => x.SoLuong).HasColumnType("int");
            builder.Property(x => x.GiaBan).HasColumnType("decimal(18,2)");
            builder.Property(x => x.NgayTao).HasColumnType("datetime");
            builder.Property(x => x.IsDefault).HasColumnType("bit").HasDefaultValue(false); ;
            builder.Property(x => x.TrangThai).HasColumnType("int");
            builder.HasOne(x => x.MauSac).WithMany(x => x.ChiTietSanPhams).HasForeignKey(x => x.IDMauSac);
            builder.HasOne(x => x.KichCo).WithMany(x => x.ChiTietSanPhams).HasForeignKey(x => x.IDKichCo);
            builder.HasOne(x => x.SanPham).WithMany(x => x.ChiTietSanPhams).HasForeignKey(x => x.IDSanPham);
            builder.HasOne(x => x.KhuyenMai).WithMany(x => x.ChiTietSanPhams).HasForeignKey(x => x.IDKhuyenMai);
        }
    }
}
