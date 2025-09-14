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
                optionsBuilder.UseSqlServer("Server=DESKTOP-64M86CN\\SQLEXPRESS;Database=AppBanQuanAoThoiTrangNam;Trusted_Connection=True;TrustServerCertificate=True");
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
                    TenPTTT = "Banking"
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
                    IDKhachHang = Guid.Parse("992b39ef-127f-4349-9582-4336b5ecebbb"),
                    NgayTao = DateTime.Parse("2024-04-24T15:00:16.780"),
                }
            );
            // 👉 Seeding KhachHang (mặc định khách lẻ)
            modelBuilder.Entity<KhachHang>().HasData(
               new KhachHang
               {
                   IDKhachHang = Guid.Parse("e106c66d-f18d-4609-8a38-08e09d68e78c"),
                   Ten = "khachle",
                   Password = "$2a$10$UU8q3GWou.7Yvkglaq3vWOLX7CNG7GUMG/puoz1LI39VnUL/JS7Ba",
                   GioiTinh = 1,
                   NgaySinh = null,
                   Email = "khachle@gmail.com",
                   MaKhachHang = "kh001",
                   SDT = "0987654327",
                   DiemTich = 0,
                   TrangThai = 1
               },
               new KhachHang
               {
                   IDKhachHang = Guid.Parse("992b39ef-127f-4349-9582-4336b5ecebbb"),
                   Ten = "khachhangdemo",
                   Password = "$2a$10$UU8q3GWou.7Yvkglaq3vWOLX7CNG7GUMG/puoz1LI39VnUL/JS7Ba",
                   GioiTinh = null,
                   NgaySinh = null,
                   Email = "khachhang@gmail.com",
                   MaKhachHang = "kh002",
                   SDT = "0987654322",
                   DiemTich = 9999,
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
            // 👉 Seeding ChatLieu
            modelBuilder.Entity<ChatLieu>().HasData(
                new ChatLieu { ID = Guid.Parse("0282068c-edfd-4c76-a1b0-6f1698319eef"), Ten = "Cotton", TrangThai = 1 },
                new ChatLieu { ID = Guid.Parse("0d16634e-c334-40f7-a407-982447d694d0"), Ten = "Nhung", TrangThai = 1 },
                new ChatLieu { ID = Guid.Parse("7dda73f2-aae5-4659-8a86-f7f67c812dc9"), Ten = "Lụa", TrangThai = 1 },
                new ChatLieu { ID = Guid.Parse("8aeacf00-8e48-4446-bbe2-fe1623973d65"), Ten = "Polyester", TrangThai = 1 }
            );
            // 👉 Seeding MauSac
            modelBuilder.Entity<MauSac>().HasData(
                new MauSac { ID = Guid.Parse("5984fce1-b2f4-4b43-a2f0-30cb3ebefaec"), Ma = "#480cd4", Ten = "Tím", TrangThai = 1 },
                new MauSac { ID = Guid.Parse("4552f6dc-53f3-4966-a4bb-5a9328e972b8"), Ma = "#8d9b27", Ten = "Xanh lá", TrangThai = 1 },
                new MauSac { ID = Guid.Parse("a5919aa2-b789-423a-b962-6d0f51673a44"), Ma = "#3cb371", Ten = "Xanh", TrangThai = 1 }, 
                new MauSac { ID = Guid.Parse("754c5d9d-f44b-453f-9134-a951621f6aa9"), Ma = "#000000", Ten = "Đen", TrangThai = 1 }, 
                new MauSac { ID = Guid.Parse("ee6bc3a6-a09c-4c1c-ae32-c22578304978"), Ma = "#8B4513", Ten = "Nâu", TrangThai = 1 }, 
                new MauSac { ID = Guid.Parse("2e31b635-3164-4391-8007-cdee9e3b7d1a"), Ma = "#FFFF00", Ten = "Vàng", TrangThai = 1 }, 
                new MauSac { ID = Guid.Parse("f7ea0204-6a31-4891-b807-cff779915b6d"), Ma = "#f40b0b", Ten = "Đỏ", TrangThai = 1 }
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
            // 👉 Seeding LoaiSP
            modelBuilder.Entity<LoaiSP>().HasData(
            new LoaiSP
            {
                ID = Guid.Parse("a4c8590b-72b0-4c81-b210-02a7914dc6fb"),
                Ten = "Áo",
                TrangThai = 1
            },
            new LoaiSP
            {
                ID = Guid.Parse("c39e8d79-dab2-4ff7-b1c6-c6d70fc9a1f3"),
                Ten = "Áo thun",
                TrangThai = 1
            },
            new LoaiSP
            {
                ID = Guid.Parse("d8738663-caec-4a0b-88ff-211cda1aa5d5"),
                Ten = "Áo sơ mi",
                TrangThai = 1
            },
            new LoaiSP
            {
                ID = Guid.Parse("127a0d4b-136c-4de6-8bb1-2f3d9d1441d0"),
                Ten = "Áo khoác",
                TrangThai = 1
            },
            new LoaiSP
            {
                ID = Guid.Parse("c7f34203-1091-4683-a34c-dde0f456d5dc"),
                Ten = "Quần",
                TrangThai = 1
            },
            new LoaiSP
            {
                ID = Guid.Parse("08b41916-d2f6-4c25-b13f-74c6e1fd7721"),
                Ten = "Quần jean",
                TrangThai = 1
            },
            new LoaiSP
            {
                ID = Guid.Parse("126d769b-f14e-472f-94bc-2a7c61ed711e"),
                Ten = "Quần âu",
                TrangThai = 1
            },
            new LoaiSP
            {
                ID = Guid.Parse("a72099ad-7e50-4c3a-93ad-6740bff4ebc6"),
                Ten = "Quần short",
                TrangThai = 1
            },
            new LoaiSP
            {
                ID = Guid.Parse("33035929-4303-4ff7-8f4e-1118d4604f6e"),
                Ten = "Váy",
                TrangThai = 1
            }
        );
        }

    }
}
