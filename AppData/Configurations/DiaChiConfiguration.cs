using AppData.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppData.Configurations
{
    internal class DiaChiConfiguration : IEntityTypeConfiguration<DiaChi>
    {
        public void Configure(EntityTypeBuilder<DiaChi> builder)
        {
          
            builder.ToTable("DiaChi");         
            builder.HasKey(dc => dc.Id);

            // Quan hệ 1-N với KhachHang
            builder.HasOne(dc => dc.KhachHang)
                   .WithMany(kh => kh.DiaChi)
                   .HasForeignKey(dc => dc.IDKhachHang)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}