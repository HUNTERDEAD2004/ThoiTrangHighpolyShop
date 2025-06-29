using AppData.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppData.ViewModels.BanOffline;

namespace AppData.Configurations
{
    public class PhuongThucThanhToanConfiguration : IEntityTypeConfiguration<PhuongThucThanhToan>
    {
        public void Configure(EntityTypeBuilder<PhuongThucThanhToan> builder)
        {
            builder.ToTable("PhuongThucThanhToan");

            builder.HasKey(x => x.IDPTTT);

            builder.Property(x => x.TenPTTT)
                .HasColumnType("nvarchar(100)")
                .IsRequired();
<<<<<<< HEAD
=======

            // Thiết lập quan hệ 1-1 với HoaDon
            builder.HasOne(x => x.HoaDons).WithOne(x => x.PhuongThucThanhToans).HasForeignKey<HoaDon>();
>>>>>>> 4b7de0706a676773222572c2c8415916b0f7e645
        }
    }

}
