using AppAPI.IServices;
using AppData.IRepositories;
using AppData.Models;
using AppData.Repositories;
using AppData.ViewModels;
using AppData.ViewModels.Mail;
using AppData.ViewModels.QLND;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;


namespace AppAPI.Services
{
    public class QuanLyNguoiDungService : IQuanLyNguoiDungService
    {
        private readonly AssignmentDBContext context;
        private readonly IAllRepository<NhanVien> reposNV;
        private readonly IAllRepository<KhachHang> reposKH;
        private readonly EmailService mailService;

        public QuanLyNguoiDungService(
            AssignmentDBContext _context,
            EmailService _mailService)
        {
            context = _context;
            reposNV = new AllRepository<NhanVien>(context, context.NhanViens);
            reposKH = new AllRepository<KhachHang>(context, context.KhachHangs);
            mailService = _mailService;
        }

        public async Task<bool> ForgetPassword(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    return false;
                }
                bool isEmployee = await CheckIfEmployee(email);
                if (isEmployee)
                {
                    string resetToken = GenerateToken();
                    await SaveUserData(email, resetToken, isEmployee);
                    string subject = "Reset Password";
                    string messageBody = "You have requested a password reset. Your reset token is: " + resetToken;
                    await SendEmail(email, subject, messageBody);

                    return true;
                }
                return false;
            }
            catch (Exception)
            {

                throw;

            }
        }
        public async Task<bool> ResetPassword(ResetPasswordRequest model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
                {
                    return false;
                }
                bool isEmployee = await CheckIfEmployee(model.Email);

                if (isEmployee)
                {
                    var nv = await context.NhanViens.FirstOrDefaultAsync(a => a.Email == model.Email);
                    if (nv != null)
                    {
                        nv.PassWord = MaHoaMatKhau(model.Password);
                        await context.SaveChangesAsync();
                        await SendEmail(nv.Email, "Đổi Mật Khẩu Thành Công", "Mật khẩu của bạn đã được đặt lại thành công.");

                        return true;
                    }
                }
                else
                {
                    var kh = await context.KhachHangs.FirstOrDefaultAsync(a => a.Email == model.Email);
                    if (kh != null)
                    {
                        kh.Password = MaHoaMatKhau(model.Password);

                        await context.SaveChangesAsync();
                        await SendEmail(kh.Email, "Đặt lại mật khẩu thành công", "Mật khẩu của bạn đã được đặt lại thành công.");
                        return true;
                    }
                }
                await SendEmail(model.Email, "Lỗi Đặt lại Mật khẩu", "Đã xảy ra lỗi khi đặt lại mật khẩu của bạn. Vui lòng thử lại sau.");
                return false;
            }
            catch (Exception) { throw; }
        }
        private string GenerateToken()
        {
            try
            {
                string token = Guid.NewGuid().ToString();
                return token;
            }
            catch (Exception) { throw; }
        }

        public async Task<bool> CheckIfEmployee(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    return false;
                }
                var nv = await context.NhanViens.FirstOrDefaultAsync(a => a.Email == email);
                var kh = await context.KhachHangs.FirstOrDefaultAsync(a => a.Email == email);
                return nv != null || kh != null;
            }
            catch (Exception)
            {

                throw;
            }
        }
        private async Task<bool> SaveUserData(string email, string data, bool isEmployee)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    return false;
                }
                if (isEmployee)
                {
                    var nv = await context.NhanViens.FirstOrDefaultAsync(a => a.Email == email);
                    if (nv != null)
                    {
                        nv.PassWord = MaHoaMatKhau(data);
                        await context.SaveChangesAsync();
                        return true;
                    }
                }
                else
                {
                    var kh = await context.KhachHangs.FirstOrDefaultAsync(a => a.Email == email);
                    if (kh != null)
                    {
                        kh.Password = MaHoaMatKhau(data);
                        await context.SaveChangesAsync();
                        return true;
                    }
                }
                return false;
            }
            catch (Exception)
            {

                throw;
            }
        }
        private async Task<bool> SendEmail(string email, string subject, string body)
        {
            try
            {
                var smtpClient = new SmtpClient("smtp.gmail.com", 587)
                {
                    UseDefaultCredentials = false,
                    EnableSsl = true,
                    Credentials = new NetworkCredential("tuanbaph34984@fpt.edu.vn", "dgjeixgeoxjysujz")
                };
                var mail = new MailMessage
                {
                    From = new MailAddress("tuanbaph34984@fpt.edu.vn"),
                    Subject = subject,
                    Body = body
                };
                mail.To.Add(email);
                await smtpClient.SendMailAsync(mail);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        public async Task<bool> ChangePassword(string email, string password, string newPassword)
        {
            try
            {
                var kh = await context.KhachHangs.FirstOrDefaultAsync(h => h.Email == email && h.Password == password);
                if (kh != null)
                {
                    kh.Password = MaHoaMatKhau(newPassword);
                    await context.SaveChangesAsync();
                    return true;
                }
                var nv = await context.NhanViens.FirstOrDefaultAsync(h => h.Email == email && h.PassWord == password);
                if (nv != null)
                {
                    nv.PassWord = MaHoaMatKhau(newPassword);
                    await context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception)
            {

                throw;

            }
        }


            public async Task<LoginViewModel> Login(string lg, string password)
            {
                try
                {
                    // Tìm nhân viên đầu tiên
                    var nv = await context.NhanViens.FirstOrDefaultAsync(a => a.Email == lg || a.SDT == lg);

                    if (nv != null)
                    {
                        if (KiemTraMatKhau(password, nv.PassWord))
                        {
                            if (nv.TrangThai == 1)
                            {
                                return new LoginViewModel
                                {
                                    Id = nv.ID,
                                    Email = nv.Email,
                                    Ten = nv.Ten,
                                    SDT = nv.SDT,
                                    DiaChi = nv.DiaChi,
                                    vaiTro = 0, // Nhân viên
                                    IsAccountLocked = false,
                                    Message = "Đăng nhập thành công"
                                };
                            }
                            else
                            {
                                return new LoginViewModel
                                {
                                    IsAccountLocked = true,
                                    Message = "Tài khoản nhân viên đã bị khóa.",
                                    vaiTro = 0
                                };
                            }
                        }
                        else
                        {
                            return new LoginViewModel
                            {
                                Message = "Mật khẩu không đúng.",
                                vaiTro = 0
                            };
                        }
                    }

                    // Nếu không phải nhân viên thì tìm khách hàng
                    var kh = await context.KhachHangs.FirstOrDefaultAsync(x => x.Email == lg || x.SDT == lg);

                    if (kh != null)
                    {
                        if (KiemTraMatKhau(password, kh.Password))
                        {

                        return new LoginViewModel
                        {
                            Id = kh.IDKhachHang,
                            Email = kh.Email,
                            Ten = kh.Ten,
                            SDT = kh.SDT,
                            //DiaChi = kh.DiaChi,
                            DiemTich = kh.DiemTich,
                            GioiTinh = kh.GioiTinh,
                            NgaySinh = kh.NgaySinh?.ToString("yyyy-MM-dd"),
                           vaiTro = 1, // Khách hàng
                            IsAccountLocked = false,
                            Message = "Đăng nhập thành công"
                        };
                        }
                        else
                        {
                            return new LoginViewModel
                            {
                                Message = "Mật khẩu không đúng.",
                                vaiTro = 1
                            };
                        }
                    }

                    // Không tìm thấy người dùng
                    return new LoginViewModel
                    {
                        Message = "Tài khoản không tồn tại."
                    };
                }
                catch (Exception ex)
                {
                    return new LoginViewModel
                    {
                        Message = $"Đã xảy ra lỗi hệ thống: {ex.Message}"
                    };
                }
            }

        private string MaHoaMatKhau(string matKhau)
        {
            // Ở đây, bạn có thể sử dụng bất kỳ phương thức mã hóa mật khẩu nào phù hợp
            // Ví dụ: sử dụng thư viện BCrypt.Net để mã hóa mật khẩu
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(matKhau);
            return hashedPassword;

            // Đây chỉ là ví dụ đơn giản, không nên sử dụng trong môi trường thực tế
            //return matKhau;
        }
        private bool KiemTraMatKhau(string nhap, string hashed)
        {
            // Ở đây, bạn phải sử dụng cùng một phương thức mã hóa mật khẩu
            // để kiểm tra mật khẩu nhập vào có khớp với mật khẩu đã lưu hay không
            // Ví dụ: sử dụng BCrypt.Net để kiểm tra
            return BCrypt.Net.BCrypt.Verify(nhap, hashed);

            // Đây chỉ là ví dụ đơn giản, không nên sử dụng trong môi trường thực tế
            //return nhap == hashed;
        }

        private async Task<string> GenerateNewMaKhachHang()
        {
            var lastMaKH = await context.KhachHangs
                .Where(x => x.MaKhachHang.StartsWith("KH"))
                .OrderByDescending(x => x.MaKhachHang)
                .Select(x => x.MaKhachHang)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (!string.IsNullOrEmpty(lastMaKH) && int.TryParse(lastMaKH.Substring(2), out int currentNumber))
            {
                nextNumber = currentNumber + 1;
            }

            return $"KH{nextNumber:D3}"; // VD: KH001
        }


        private string GenerateNumericCode(int length = 6)
        {
            var random = new Random();
            return string.Concat(Enumerable.Range(0, length).Select(_ => random.Next(0, 10)));
        }

        public async Task<KhachHang> RegisterKhachHang(KhachHangViewModel khachHang)
        {
            try
            {
                // Kiểm tra Email hoặc SDT đã tồn tại
                var existingKhachHang = await context.KhachHangs
                    .FirstOrDefaultAsync(kh => kh.Email == khachHang.Email || kh.SDT == khachHang.SDT);

                if (existingKhachHang != null)
                    return null; // Trùng email hoặc số điện thoại

                // Sinh mã khách hàng tự động
                string maKH = await GenerateNewMaKhachHang();

                // Parse ngày sinh nếu cần
                DateOnly? ngaySinh = null;
                if (!string.IsNullOrWhiteSpace(khachHang.NgaySinh))
                    if (DateOnly.TryParse(khachHang.NgaySinh, out var parsed))
                        ngaySinh = parsed;

                // Tạo khách hàng mới
                var kh = new KhachHang
                {
                    IDKhachHang = Guid.NewGuid(),
                    Ten = khachHang.Ten,
                    Email = khachHang.Email,
                    MaKhachHang = maKH,
                    GioiTinh = khachHang.GioiTinh,
                    NgaySinh = ngaySinh,
                    Password = MaHoaMatKhau(khachHang.Password),
                    SDT = khachHang.SDT,
                    DiemTich = 0,
                    TrangThai = 0 // ⚠️ Chưa xác thực
                };

                await context.KhachHangs.AddAsync(kh);

                // Tạo giỏ hàng mặc định
                var gioHang = new GioHang
                {
                    IDKhachHang = kh.IDKhachHang,
                    NgayTao = DateTime.Now,
                };
                await context.GioHangs.AddAsync(gioHang);

                // Tạo mã xác thực
                string token = GenerateNumericCode(); // mã 6 số

                var tokenEntity = new EmailVerificationToken
                {
                    IDKhachHang = kh.IDKhachHang,
                    Token = token,
                    ExpiryTime = DateTime.Now.AddMinutes(15)
                };
                await context.EmailVerificationTokens.AddAsync(tokenEntity);

                await context.SaveChangesAsync();

                await SendVerificationCodeEmail(kh.Email, kh.Ten, token);


                return kh;
            }
            catch (Exception)
            {
                throw;
            }
        }


        private async Task SendVerificationCodeEmail(string email, string ten, string token)
        {
            string subject = "🔐 Xác minh tài khoản - ThoiTrangHighpolyShop";
            string body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>Xác minh tài khoản</title>
</head>
<body style='margin:0;padding:0;font-family:Segoe UI, sans-serif;background:#f2f2f2;'>

    <div style='max-width:600px;margin:30px auto;background:#fff;border-radius:8px;box-shadow:0 4px 12px rgba(0,0,0,0.1);overflow:hidden;'>

        <!-- Header -->
        <div style='background:linear-gradient(135deg,#00c9ff,#92fe9d);padding:30px;text-align:center;color:#fff;'>
            <h1 style='margin:0;font-size:24px;'>🎉 Xác minh tài khoản của bạn</h1>
            <p style='margin:5px 0 0;'>Chào {ten}, cảm ơn bạn đã đăng ký!</p>
        </div>

        <!-- Body -->
        <div style='padding:30px;text-align:center;'>
            <p style='font-size:16px;color:#333;'>Để hoàn tất việc đăng ký tài khoản tại <strong>ThoiTrangHighpolyShop</strong>, vui lòng sử dụng mã xác thực bên dưới:</p>

            <div style='font-size:32px;font-weight:bold;margin:20px auto;padding:10px 20px;border-radius:8px;display:inline-block;background:linear-gradient(135deg,#f6d365,#fda085);color:#fff;letter-spacing:4px;'>
                {token}
            </div>

            <p style='font-size:14px;color:#888;'>Mã sẽ hết hạn sau <strong>15 phút</strong>. Vui lòng không chia sẻ mã này với người khác.</p>

            <a href='#' style='margin-top:20px;display:inline-block;padding:12px 24px;background:#00c9ff;color:white;text-decoration:none;border-radius:25px;font-weight:bold;box-shadow:0 4px 10px rgba(0, 201, 255, 0.4);transition:all 0.3s ease;'>
                🔑 Xác minh ngay
            </a>
        </div>

        <!-- Footer -->
        <div style='background:#2c3e50;color:white;padding:20px;text-align:center;font-size:12px;'>
            <p style='margin:0;'>📧 Đây là email tự động, vui lòng không phản hồi.</p>
            <p style='margin:5px 0 0;'>© 2025 ThoiTrangHighpolyShop. Mọi quyền được bảo lưu.</p>
        </div>

    </div>

</body>
</html>";

            await mailService.SendEmailAsync(email, subject, body);
          
        }



        public async Task<bool> ConfirmEmail(string email, string token)
        {
            try
            {
                var kh = await context.KhachHangs.FirstOrDefaultAsync(k => k.Email == email);
                if (kh == null) return false;

                var tokenEntity = await context.EmailVerificationTokens
                    .FirstOrDefaultAsync(t => t.IDKhachHang == kh.IDKhachHang && t.Token == token);

                if (tokenEntity == null || tokenEntity.ExpiryTime < DateTime.Now)
                    return false;

                // Cập nhật trạng thái đã xác thực
                kh.TrangThai = 1;
                context.EmailVerificationTokens.Remove(tokenEntity); // Xoá token sau khi dùng
                await context.SaveChangesAsync();
                
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<bool> VerifyEmail(Guid userId, string token)
        {
            var verifyToken = await context.EmailVerificationTokens
                .FirstOrDefaultAsync(x => x.IDKhachHang == userId && x.Token == token);

            if (verifyToken == null || verifyToken.ExpiryTime < DateTime.Now)
                return false;

            var kh = await context.KhachHangs.FindAsync(userId);
            if (kh == null) return false;

            kh.TrangThai = 1;
            context.EmailVerificationTokens.Remove(verifyToken);
            await context.SaveChangesAsync();
            return true;
        }



        public async Task<NhanVien> RegisterNhanVien(NhanVienViewModel nhanVien)
        {
            try
            {
                var kh = new NhanVien
                {
                    ID = Guid.NewGuid(),
                    Ten = nhanVien.Ten,
                    Email = nhanVien.Email,
                    PassWord = MaHoaMatKhau(nhanVien.Password),
                    IDVaiTro = nhanVien.IDVaiTro
                };
                context.NhanViens.Add(kh);
                await context.SaveChangesAsync();
                return kh;
            }
            catch (Exception)
            {

                throw;
            }         
        }
        //Tam
        public async Task<bool> ChangePassword(ChangePasswordRequest request)
        {
            try
            {
                var kh = await context.KhachHangs.FirstOrDefaultAsync(h => h.IDKhachHang == request.ID);
                if (kh != null)
                {
                    if (KiemTraMatKhau(request.OldPassword, kh.Password))
                    {
                        kh.Password = MaHoaMatKhau(request.NewPassword);
                        await context.SaveChangesAsync();
                        return true;
                    }
                }
                var nv = await context.NhanViens.FirstOrDefaultAsync(h => h.ID == request.ID);
                if (nv != null)
                {
                    if (KiemTraMatKhau(request.OldPassword, nv.PassWord))
                    {
                        nv.PassWord = MaHoaMatKhau(request.NewPassword);
                        await context.SaveChangesAsync();
                        return true;
                    }
                }
                return false;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<int> UseDiemTich(int diem, string id)
        {
            try
            {
                var khachHang = context.KhachHangs.First(x => x.IDKhachHang == new Guid(id));
                var quyDoiDiem = context.QuyDoiDiems.First(x => x.TrangThai > 0);

                if (quyDoiDiem == null)
                {
                    return 0;
                }
                else if (diem > khachHang.DiemTich)
                {
                    return 0;
                }
                else
                {
                    return diem * quyDoiDiem.TiLeTieuDiem;
                }
            }
            catch (Exception)
            {

                throw;

            }
        }


        public async Task<LoginViewModel> UpdateProfile(LoginViewModel loginViewModel)
        {
            try
            {
                // 1. Khách hàng
                var kh = await context.KhachHangs.FirstOrDefaultAsync(h => h.IDKhachHang == loginViewModel.Id);
                if (kh != null)
                {
                    kh.Ten = loginViewModel.Ten;
                    kh.SDT = loginViewModel.SDT;
                    kh.Email = loginViewModel.Email;
                    kh.GioiTinh = loginViewModel.GioiTinh;

                    if (!string.IsNullOrWhiteSpace(loginViewModel.NgaySinh) &&
                        DateOnly.TryParse(loginViewModel.NgaySinh, out var parsedNgaySinh))
                    {
                        kh.NgaySinh = parsedNgaySinh;
                    }
                    else
                    {
                        kh.NgaySinh = null;
                    }

                    await context.SaveChangesAsync();

                    return new LoginViewModel
                    {
                        Id = kh.IDKhachHang,
                        Email = kh.Email,
                        Ten = kh.Ten,
                        SDT = kh.SDT,
                        DiemTich = kh.DiemTich,
                        GioiTinh = kh.GioiTinh,
                        NgaySinh = kh.NgaySinh?.ToString("yyyy-MM-dd"), // nếu trả về dạng string
                        vaiTro = 1
                    };
                }

                // 2. Nhân viên
                var nv = await context.NhanViens.FirstOrDefaultAsync(h => h.ID == loginViewModel.Id);
                if (nv != null)
                {
                    nv.Ten = loginViewModel.Ten;
                    nv.SDT = loginViewModel.SDT;
                    nv.Email = loginViewModel.Email;
                    nv.DiaChi = loginViewModel.DiaChi;
                    nv.GioiTinh = loginViewModel.GioiTinh;

                    if (!string.IsNullOrWhiteSpace(loginViewModel.NgaySinh) &&
                        DateOnly.TryParse(loginViewModel.NgaySinh, out var parsedNgaySinhNV))
                    {
                        nv.NgaySinh = parsedNgaySinhNV;
                    }
                    else
                    {
                        nv.NgaySinh = null;
                    }

                    await context.SaveChangesAsync();

                    return new LoginViewModel
                    {
                        Id = nv.ID,
                        Email = nv.Email,
                        Ten = nv.Ten,
                        SDT = nv.SDT,
                        GioiTinh = nv.GioiTinh,
                        NgaySinh = nv.NgaySinh?.ToString("yyyy-MM-dd"), // nếu cần string
                        DiaChi = nv.DiaChi,
                        vaiTro = 0
                    };
                }

                return null;
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<bool> AddNhanhKH(KhachHang kh)
        {
            try
            {
                KhachHang KH = new KhachHang();
                GioHang gioHang = new GioHang()
                {
                    IDKhachHang = kh.IDKhachHang,
                    NgayTao = DateTime.Now,
                };
                await context.GioHangs.AddAsync(gioHang);
                await context.SaveChangesAsync();

                KH.IDKhachHang = Guid.NewGuid();
                KH.Ten = kh.Ten;
                KH.Email = kh.Email;
                KH.Password = MaHoaMatKhau(kh.Password);
                KH.SDT = kh.SDT;
                KH.DiemTich = 0;
                KH.TrangThai = 1;
               
                KH.DiemTich = 0;
                await context.KhachHangs.AddAsync(kh);
                await context.SaveChangesAsync();
               
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
