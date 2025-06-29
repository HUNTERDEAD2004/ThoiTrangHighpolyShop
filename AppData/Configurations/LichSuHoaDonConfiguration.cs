using AppData.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppData.Configurations
{
    public class LichSuHoaDonConfiguration : IEntityTypeConfiguration<LichSuHoaDon>
    {
        public void Configure(EntityTypeBuilder<LichSuHoaDon> builder)
        {
            builder.ToTable("LichSuHoaDon");
            builder.HasKey(x => x.ID);
            builder.Property(x => x.NgayLap).HasColumnType("datetime").IsRequired();
            builder.Property(x => x.GhiChu).HasColumnType("nvarchar(255)").IsRequired();
            builder.Property(x => x.TrangThai).HasColumnType("int").IsRequired();
        }
    }

}
