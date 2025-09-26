using AppData.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace AppData.Models
{
    public class AssignmentDBContext : DbContext
    {
        public AssignmentDBContext()
        {
        }
        public AssignmentDBContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<ChiTietGioHang> ChiTietGioHangs { get; set; }
        public DbSet<ChiTietSanPham> ChiTietSanPhams { get; set; }
        public DbSet<MauSac> MauSacs { get; set; }
        public DbSet<KichCo> KichCos { get; set; }
        public DbSet<ChatLieu> ChatLieus { get; set; }
        public DbSet<ChiTietHoaDon> ChiTietHoaDons { get; set; }
        public DbSet<GioHang> GioHangs { get; set; }
        public DbSet<HoaDon> HoaDons { get; set; }
        public DbSet<KhuyenMai> KhuyenMais { get; set; }
        public DbSet<LichSuTichDiem> LichSuTichDiems { get; set; }
        public DbSet<LoaiSP> LoaiSPs { get; set; }
        public DbSet<KhachHang> KhachHangs { get; set; }
        public DbSet<NhanVien> NhanViens { get; set; }
        public DbSet<DanhGia> DanhGias { get; set; }
        public DbSet<QuyDoiDiem> QuyDoiDiems { get; set; }
        public DbSet<SanPham> SanPhams { get; set; }
        public DbSet<VaiTro> VaiTros { get; set; }
        public DbSet<Voucher> Vouchers { get; set; }
        public DbSet<Anh> Anhs { get; set; }
        public DbSet<LichSuHoaDon> LichSuHoaDons { get; set; }
        public DbSet<UserVoucher> UserVouchers { get; set; }
        public DbSet<PhuongThucThanhToan> PhuongThucThanhToans { get; set; }

        public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; }

        public DbSet<DiaChi> DiaChis { get; set; }
     
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=MSI;Database=AppBanQuanAoThoiTrangNam;Trusted_Connection=True;TrustServerCertificate=True");
            }
        }
        //"Server=DESKTOP-NQ6KMAG\SQLEXPRESS;Database=AppBanQuanAoThoiTrangNam;Trusted_Connection=True;TrustServerCertificate=True"
        //@"Data Source=DESKTOP-3K22IAU;Initial Catalog=AppBanQuanAoThoiTrangNam;Integrated Security=True"
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            // 👉 Seeding PhuongThucThanhToan
            modelBuilder.Entity<PhuongThucThanhToan>().HasData(
                new PhuongThucThanhToan
                {
                    IDPTTT = Guid.Parse("9b6289bf-5e83-419a-94d5-926abb264961"),
                    TenPTTT = "Cash"
                },
                new PhuongThucThanhToan
                {
                    IDPTTT = Guid.Parse("f29cd85d-0251-4b50-8867-6a88891417f6"),
                    TenPTTT = "MOMO Banking"
                },
                new PhuongThucThanhToan
                {
                    IDPTTT = Guid.Parse("DD56B0D5-721D-4CD3-A20A-CEB190755E26"),
                    TenPTTT = "COD"
                },
                new PhuongThucThanhToan
                {
                    IDPTTT = Guid.Parse("761881CC-2324-4760-9628-6ED287A59AC7"),
                    TenPTTT = "VNPAY"
                });
            // 👉 Seeding Gio Hang
            modelBuilder.Entity<GioHang>().HasData(
               new GioHang
               {
                   IDKhachHang = Guid.Parse("e106c66d-f18d-4609-8a38-08e09d68e78c"),
                   NgayTao = DateTime.Parse("2024-04-24T15:00:16.780"),
               },
                new GioHang
                {
                    IDKhachHang = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    NgayTao = DateTime.Parse("2024-04-24T15:00:16.780"),
                }
            );
            // 👉 Seeding KhachHang (mặc định khách lẻ)
            modelBuilder.Entity<KhachHang>().HasData(
               new KhachHang
               {
                   //$2a$10$UU8q3GWou.7Yvkglaq3vWOLX7CNG7GUMG/puoz1LI39VnUL/JS7Ba
                   IDKhachHang = Guid.Parse("e106c66d-f18d-4609-8a38-08e09d68e78c"),
                   Ten = "Khách Lẻ",
                   Password = "",
                   GioiTinh = 1,
                   NgaySinh = null,
                   Email = "khachle@gmail.com",
                   MaKhachHang = "kh001",
                   SDT = "",
                   DiemTich = 0,
                   TrangThai = 1
               },
               new KhachHang
               {
                   IDKhachHang = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                   Ten = "Khách vãng lai",
                   Password = "",
                   GioiTinh = null,
                   NgaySinh = null,
                   Email = "",
                   MaKhachHang = "kh002",
                   SDT = "",
                   DiemTich = 0,
                   TrangThai = 1
               }
            );
            // 👉 Seeding NhanVien test
            modelBuilder.Entity<NhanVien>().HasData(
               new NhanVien
               {
                   ID = Guid.Parse("4127568f-30d8-447f-83b6-45bc740051ca"),
                   Ten = "nhanvien",
                   Email = "nhanvien@gmail.com",
                   SDT = "0987654322",
                   DiaChi = "Quan10-HoChiMinh-VietNam",
                   GioiTinh = 1,
                   TrangThai = 1,
                   IDVaiTro = Guid.Parse("952c1a5d-74ff-4daf-ba88-135c5440809c"),
                   PassWord = "$2a$10$UU8q3GWou.7Yvkglaq3vWOLX7CNG7GUMG/puoz1LI39VnUL/JS7Ba"
               }
            );
            // 👉 Seeding KichCo
            modelBuilder.Entity<KichCo>().HasData(
                new KichCo { ID = Guid.Parse("5c33b977-5f2f-4abf-9f88-0934fbe6bbd5"), Ten = "S", TrangThai = 1 },
                new KichCo { ID = Guid.Parse("65a264a7-1da7-4937-af45-0683563c98f3"), Ten = "M", TrangThai = 1 },
                new KichCo { ID = Guid.Parse("3c6dbcc1-78d9-4afe-b7bd-6ca99e321119"), Ten = "L", TrangThai = 1 },
                new KichCo { ID = Guid.Parse("add670bd-a69a-4200-9267-d54cf2171795"), Ten = "XL", TrangThai = 1 },
                new KichCo { ID = Guid.Parse("5e863e9f-8c80-4ea6-9903-01633155e470"), Ten = "2XL", TrangThai = 1 },
                new KichCo { ID = Guid.Parse("ca656d78-80a1-4409-933e-cc3ee5faf0d8"), Ten = "3XL", TrangThai = 1 }
            );
        }

    }
}
