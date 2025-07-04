using AppData.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AppData.Configurations
{


    internal class KhachHangConfiguration : IEntityTypeConfiguration<KhachHang>
    {
        public void Configure(EntityTypeBuilder<KhachHang> builder)
        {
            builder.ToTable("KhachHang");

            builder.HasKey(x => x.IDKhachHang);

            builder.Property(x => x.MaKhachHang).HasColumnType("nvarchar(50)");
            builder.Property(x => x.Ten).HasColumnType("nvarchar(100)");
            builder.Property(x => x.Password).HasColumnType("varchar(MAX)");
            builder.Property(x => x.GioiTinh).HasColumnType("int");

            // 👇 Convert DateOnly? <=> DateTime?
            var dateOnlyConverter = new ValueConverter<DateOnly?, DateTime?>(
                v => v.HasValue ? v.Value.ToDateTime(TimeOnly.MinValue) : null,
                v => v.HasValue ? DateOnly.FromDateTime(v.Value) : null
            );

            builder.Property(x => x.NgaySinh)
                   .HasConversion(dateOnlyConverter)
                   .HasColumnType("date"); 

            builder.Property(x => x.Email).HasColumnType("varchar(250)");
            builder.Property(x => x.SDT).HasColumnType("varchar(10)");
            builder.Property(x => x.DiemTich).HasColumnType("int");
            builder.Property(x => x.TrangThai).HasColumnType("int");
        }
    }

}
