using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AppData.Models;
using AppData.ViewModels;

public class EmailVerificationTokenConfiguration : IEntityTypeConfiguration<EmailVerificationToken>
{
    public void Configure(EntityTypeBuilder<EmailVerificationToken> builder)
    {
        builder.ToTable("EmailVerificationTokens");

        builder.HasKey(e => e.ID);

        builder.Property(e => e.Token)
               .IsRequired();

        builder.Property(e => e.ExpiryTime)
               .IsRequired();

        
        builder.HasOne(e => e.KhachHang)
               .WithMany() 
               .HasForeignKey(e => e.IDKhachHang)
               .OnDelete(DeleteBehavior.Cascade); 
    }
}
