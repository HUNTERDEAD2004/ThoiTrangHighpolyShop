using AppAPI.IServices;
using AppData.Models;
using AppData.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;
using AppData.ViewModels.Mail;
using Microsoft.Extensions.Options;

namespace AppAPI.Services
{
    public class KhachHangService : IKhachHangService
    {
        private readonly AssignmentDBContext _dbContext;
        private readonly MailSettings _mailSettings;
        private readonly EmailService _mailService;

        public KhachHangService(IOptions<MailSettings> mailSettings, EmailService EmailService)
        {
            _dbContext = new AssignmentDBContext();
            _mailSettings = mailSettings.Value;
            _mailService = EmailService;
        }

        public async Task<KhachHang> Add(KhachHangViewModel kh, bool isFromAdmin = false)
        {
            var existing = await _dbContext.KhachHangs
                .FirstOrDefaultAsync(k => k.Email == kh.Email || k.SDT == kh.SDT);
            if (existing != null)
                return null;

            var khachId = Guid.NewGuid();

            DateOnly? ngaySinh = null;
            if (!string.IsNullOrWhiteSpace(kh.NgaySinh))
                if (DateOnly.TryParse(kh.NgaySinh, out var parsed))
                    ngaySinh = parsed;

            // Tự động tạo mã KH và password - không cho nhập từ bên ngoài
            string maKH = await GenerateNewMaKhachHang();
            string plainPassword = GenerateRandomPassword();
            string hashedPassword = MaHoaMatKhau(plainPassword);

            var khach = new KhachHang()
            {
                IDKhachHang = khachId,
                Ten = kh.Ten,
                MaKhachHang = maKH, // Tự động tạo
                GioiTinh = kh.GioiTinh,
                NgaySinh = ngaySinh,
                Email = kh.Email,
                Password = hashedPassword, // Tự động tạo và mã hóa
                SDT = kh.SDT,
                DiemTich = 0,
                TrangThai = isFromAdmin ? 1 : 0 // Admin tạo thì active luôn, user tự đăng ký thì chờ kích hoạt
            };

            await _dbContext.KhachHangs.AddAsync(khach);

            var gioHang = new GioHang()
            {
                IDKhachHang = khachId,
                NgayTao = DateTime.Now,
            };
            await _dbContext.GioHangs.AddAsync(gioHang);

            var diaChi = new DiaChi()
            {
                IDKhachHang = khachId,
                Xa = kh.Xa,
                Quan = kh.Quan,
                Huyen = kh.Huyen,
                Tinh = kh.Tinh,
                DiaChiChiTiet = kh.DiaChiChiTiet,
                IsDefault = true
            };
            await _dbContext.DiaChis.AddAsync(diaChi);

            await _dbContext.SaveChangesAsync();

            // Gửi email thông tin tài khoản
            await SendAccountInfoEmail(kh.Email, kh.Ten, maKH, plainPassword);

            return khach;
        }



        private async Task SendAccountInfoEmail(string email, string ten, string maKH, string password)
        {
            string subject = "🎉 Chào mừng bạn đến với ThoiTrangHighpolyShop!";
            string body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Thông tin tài khoản</title>
</head>
<body style='margin: 0; padding: 0; font-family: Arial, sans-serif; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); min-height: 100vh;'>
    <div style='max-width: 600px; margin: 0 auto; background: white; box-shadow: 0 10px 30px rgba(0,0,0,0.1);'>
        
        <!-- Header -->
        <div style='background: linear-gradient(135deg, #ff6b6b, #ee5a24); padding: 40px 30px; text-align: center; position: relative; overflow: hidden;'>
            <div style='position: absolute; top: -50px; right: -50px; width: 100px; height: 100px; background: rgba(255,255,255,0.1); border-radius: 50%; transform: rotate(45deg);'></div>
            <div style='position: absolute; bottom: -30px; left: -30px; width: 60px; height: 60px; background: rgba(255,255,255,0.1); border-radius: 50%;'></div>
            
            <!-- Logo Container -->
            <div style='margin-bottom: 20px;'>
                <img src='https://drive.google.com/file/d/1CY63QePGArcHRJyQP0Xz0sfutj5ZMwLS/view?usp=sharing' alt='ThoiTrangHighpolyShop Logo' style='max-width: 120px; height: auto; filter: drop-shadow(0 4px 8px rgba(0,0,0,0.3));' />
            </div>
            
            <h1 style='color: white; margin: 0; font-size: 28px; font-weight: bold; text-shadow: 2px 2px 4px rgba(0,0,0,0.3);'>
                ThoiTrangHighpolyShop
            </h1>
            <p style='color: rgba(255,255,255,0.9); margin: 10px 0 0 0; font-size: 16px;'>
                Thời trang đẳng cấp - Phong cách riêng biệt
            </p>
        </div>

        <!-- Welcome Message -->
        <div style='padding: 40px 30px 20px; text-align: center;'>
            <div style='background: linear-gradient(135deg, #4facfe, #00f2fe); width: 80px; height: 80px; border-radius: 50%; margin: 0 auto 20px; display: flex; align-items: center; justify-content: center; font-size: 35px;'>
                🎉
            </div>
            <h2 style='color: #2c3e50; margin: 0 0 10px 0; font-size: 24px;'>
                Chào mừng {ten}!
            </h2>
            <p style='color: #7f8c8d; margin: 0; font-size: 16px; line-height: 1.5;'>
                Tài khoản của bạn đã được tạo thành công.<br>
                Chúng tôi rất vui mừng được chào đón bạn!
            </p>
        </div>

        <!-- Account Info Card -->
        <div style='margin: 0 30px 30px; background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%); border-radius: 15px; padding: 2px;'>
            <div style='background: white; border-radius: 13px; padding: 30px;'>
                <h3 style='color: #2c3e50; margin: 0 0 20px 0; font-size: 20px; text-align: center;'>
                    📋 Thông tin tài khoản
                </h3>
                
                <div style='background: #f8f9fa; border-radius: 10px; padding: 20px; margin-bottom: 15px;'>
                    <div style='display: flex; align-items: center; margin-bottom: 15px;'>
                        <div style='background: #3498db; width: 40px; height: 40px; border-radius: 50%; display: flex; align-items: center; justify-content: center; margin-right: 15px; font-size: 18px;'>
                            🆔
                        </div>
                        <div>
                            <div style='font-size: 12px; color: #7f8c8d; margin-bottom: 2px;'>Mã khách hàng</div>
                            <div style='font-size: 16px; font-weight: bold; color: #2c3e50;'>{maKH}</div>
                        </div>
                    </div>
                    
                    <div style='display: flex; align-items: center; margin-bottom: 15px;'>
                        <div style='background: #e74c3c; width: 40px; height: 40px; border-radius: 50%; display: flex; align-items: center; justify-content: center; margin-right: 15px; font-size: 18px;'>
                            📧
                        </div>
                        <div>
                            <div style='font-size: 12px; color: #7f8c8d; margin-bottom: 2px;'>Email đăng nhập</div>
                            <div style='font-size: 16px; font-weight: bold; color: #2c3e50;'>{email}</div>
                        </div>
                    </div>
                    
                    <div style='display: flex; align-items: center;'>
                        <div style='background: #f39c12; width: 40px; height: 40px; border-radius: 50%; display: flex; align-items: center; justify-content: center; margin-right: 15px; font-size: 18px;'>
                            🔐
                        </div>
                        <div>
                            <div style='font-size: 12px; color: #7f8c8d; margin-bottom: 2px;'>Mật khẩu tạm thời</div>
                            <div style='font-size: 16px; font-weight: bold; color: #2c3e50; font-family: monospace; background: #fff3cd; padding: 5px 10px; border-radius: 5px; display: inline-block;'>{password}</div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Security Notice -->
        <div style='background: linear-gradient(135deg, #ffeaa7, #fdcb6e); margin: 0 30px 30px; border-radius: 10px; padding: 20px; border-left: 5px solid #e17055;'>
            <div style='display: flex; align-items: center;'>
                <div style='font-size: 24px; margin-right: 15px;'>🔒</div>
                <div>
                    <h4 style='color: #d63031; margin: 0 0 5px 0; font-size: 16px;'>Bảo mật quan trọng</h4>
                    <p style='color: #2d3436; margin: 0; font-size: 14px; line-height: 1.4;'>
                        Vui lòng <strong>đổi mật khẩu ngay</strong> sau khi đăng nhập lần đầu để đảm bảo tính bảo mật cho tài khoản của bạn.
                    </p>
                </div>
            </div>
        </div>

        <!-- CTA Button -->
        <div style='text-align: center; padding: 0 30px 40px;'>
            <a href='#' style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 15px 30px; border-radius: 25px; text-decoration: none; display: inline-block; font-weight: bold; font-size: 16px; box-shadow: 0 4px 15px rgba(102, 126, 234, 0.4); transition: all 0.3s ease;'>
                🚀 Bắt đầu mua sắm ngay
            </a>
        </div>

        <!-- Footer -->
        <div style='background: #2c3e50; color: white; padding: 30px; text-align: center;'>
            <h4 style='margin: 0 0 15px 0; font-size: 18px;'>💝 Ưu đãi đặc biệt cho thành viên mới</h4>
            <p style='margin: 0 0 20px 0; font-size: 14px; opacity: 0.8;'>
                Sử dụng mã <strong style='background: #e74c3c; padding: 3px 8px; border-radius: 3px;'>WELCOME20</strong> 
                để được giảm 20% cho đơn hàng đầu tiên!
            </p>
            
            <div style='border-top: 1px solid #34495e; padding-top: 20px; margin-top: 20px;'>
                <p style='margin: 0; font-size: 12px; opacity: 0.6;'>
                    <em>📧 Đây là email tự động, vui lòng không phản hồi.</em><br>
                    © 2025 ThoiTrangHighpolyShop. Mọi quyền được bảo lưu.
                </p>
            </div>
        </div>
    </div>
</body>
</html>";

            await _mailService.SendEmailAsync(email, subject, body);
        }


        private string GenerateRandomPassword(int length = 8)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private async Task<string> GenerateNewMaKhachHang()
        {
            var last = await _dbContext.KhachHangs
                .Where(x => x.MaKhachHang.StartsWith("KH"))
                .OrderByDescending(x => x.MaKhachHang)
                .Select(x => x.MaKhachHang)
                .FirstOrDefaultAsync();

            int next = 1;
            if (!string.IsNullOrEmpty(last) && int.TryParse(last.Substring(2), out int current))
                next = current + 1;

            return $"KH{next:D3}";
        }

        private string MaHoaMatKhau(string input)
        {
            return BCrypt.Net.BCrypt.HashPassword(input);
        }

        public bool Delete(Guid id)
        {
            try
            {
                var kh = _dbContext.KhachHangs.FirstOrDefault(x => x.IDKhachHang == id);
                if (kh != null)
                {
                    _dbContext.KhachHangs.Remove(kh);
                    _dbContext.SaveChanges();
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<List<KhachHangViewModel>> GetAll()
        {
            var model = await (from kh in _dbContext.KhachHangs
                               join dc in _dbContext.DiaChis on kh.IDKhachHang equals dc.IDKhachHang
                               where dc.IsDefault == true
                               orderby kh.MaKhachHang descending 
                               select new KhachHangViewModel
                               {
                                   Id = kh.IDKhachHang,
                                   MaKhachHang = kh.MaKhachHang,
                                   Ten = kh.Ten,
                                   Email = kh.Email,
                                   SDT = kh.SDT,
                                   NgaySinh = kh.NgaySinh.HasValue ? kh.NgaySinh.Value.ToString("yyyy-MM-dd") : null,
                                   GioiTinh = kh.GioiTinh,
                                   TrangThai = kh.TrangThai,
                                   DiemTich = kh.DiemTich,
                                   Xa = dc.Xa,
                                   Quan = dc.Quan,
                                   Huyen = dc.Huyen,
                                   Tinh = dc.Tinh,
                                   DiaChiChiTiet = dc.DiaChiChiTiet,
                                   DiaChi = $"{dc.DiaChiChiTiet}, {dc.Xa}, {dc.Quan}, {dc.Huyen}, {dc.Tinh}"
                               }).ToListAsync();

            return model;
        }

        public async Task<List<HoaDon>> GetAllHDKH(Guid idkh)
        {
            return await (from hd in _dbContext.HoaDons.AsNoTracking()
                          join lstd in _dbContext.LichSuTichDiems.AsNoTracking() on hd.ID equals lstd.IDHoaDon into lstdGroup
                          from lstd in lstdGroup.DefaultIfEmpty()
                          join kh in _dbContext.KhachHangs.AsNoTracking() on lstd.MaKhachHang equals kh.MaKhachHang into khGroup
                          from kh in khGroup.DefaultIfEmpty()
                          where kh.IDKhachHang == idkh
                          select hd).ToListAsync();
        }

        public KhachHang GetById(Guid id)
        {
            return _dbContext.KhachHangs.FirstOrDefault(x => x.IDKhachHang == id);
        }

        public KhachHang GetBySDT(string sdt)
        {
            return _dbContext.KhachHangs.FirstOrDefault(c => c.SDT == sdt || c.Email == sdt);
        }

        public bool Update(KhachHangViewModel khachHang)
        {
            var kh = _dbContext.KhachHangs.FirstOrDefault(x => x.IDKhachHang == khachHang.Id);
            if (kh != null)
            {
                kh.Ten = khachHang.Ten;
                kh.SDT = khachHang.SDT;
                kh.Email = khachHang.Email;
                kh.GioiTinh = khachHang.GioiTinh;

                // Parse string -> DateOnly?
                if (!string.IsNullOrWhiteSpace(khachHang.NgaySinh) && DateOnly.TryParse(khachHang.NgaySinh, out var parsedDate))
                {
                    kh.NgaySinh = parsedDate;
                }
                else
                {
                    kh.NgaySinh = null;
                }

                kh.DiemTich = khachHang.DiemTich;
                kh.TrangThai = khachHang.TrangThai;

            

                _dbContext.KhachHangs.Update(kh);
                _dbContext.SaveChanges();
                return true;
            }
            return false;
        }

        // Thêm method đổi mật khẩu
        public async Task<bool> ChangeForgotPassword(Guid id, string newPassword)
        {
            try
            {
                var kh = await _dbContext.KhachHangs.FirstOrDefaultAsync(x => x.IDKhachHang == id);
                if (kh != null)
                {
                    kh.Password = MaHoaMatKhau(newPassword);
                    _dbContext.KhachHangs.Update(kh);
                    await _dbContext.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Thêm method lấy khách hàng theo email
        public KhachHangViewModel GetKhachHangByEmail(string email)
        {
            var kh = _dbContext.KhachHangs.FirstOrDefault(x => x.Email == email);
            if (kh != null)
            {
                return new KhachHangViewModel
                {
                    Id = kh.IDKhachHang,
                    Email = kh.Email,
                    Ten = kh.Ten,
                    MaKhachHang = kh.MaKhachHang
                };
            }
            return null;
        }

        // Thêm method tìm kiếm khách hàng
        public List<KhachHang> SearchKhachHang(string ten, string sdt)
        {
            var query = _dbContext.KhachHangs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(ten))
            {
                query = query.Where(x => x.Ten.Contains(ten));
            }

            if (!string.IsNullOrWhiteSpace(sdt))
            {
                query = query.Where(x => x.SDT.Contains(sdt));
            }

            return query.ToList();
        }
    }
}