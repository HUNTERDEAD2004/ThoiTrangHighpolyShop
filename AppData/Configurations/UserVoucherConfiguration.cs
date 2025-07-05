using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppData.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AppData.Configurations
{
    public class UserVoucherConfiguration : IEntityTypeConfiguration<UserVoucher>
    {
        public void Configure(EntityTypeBuilder<UserVoucher> builder)
        {
            builder.ToTable("UserVoucher");
            builder.HasKey(x => x.ID);

            // 👇 Convert DateOnly? <=> DateTime?
            var dateOnlyConverter = new ValueConverter<DateOnly?, DateTime?>(
                v => v.HasValue ? v.Value.ToDateTime(TimeOnly.MinValue) : null,
                v => v.HasValue ? DateOnly.FromDateTime(v.Value) : null
            );

            builder.Property(x => x.NgaySuDung).HasConversion(dateOnlyConverter).HasColumnType("Date");
            builder.Property(x => x.DaSuDung).HasColumnType("bit").IsRequired();
            builder.HasOne(x => x.Voucher).WithMany(x => x.UserVouchers).HasForeignKey(x => x.IDVoucher);
            builder.HasOne(x => x.KhachHang).WithMany(x => x.UserVouchers).HasForeignKey(x => x.IDKhachHang);
        }
    }
}
