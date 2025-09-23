
using System.Net;
using System.Net.Mail;
using AppData.ViewModels.QLND;
using AppAPI.IServices;
using Microsoft.Extensions.Configuration;
using System.Net.Mime;

namespace AppAPI.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendForgotPasswordConfirmation(ForgotPasswordRequest forgot)
        {
            string subject = "Reset Password";
            string body = $"Please click the link below to reset your password:<br>" +
                          $"<a href='https://localhost:7095/api/ResetPassword?email={forgot.Email}&token={forgot.Token}'>Reset Password</a>";

            await SendEmailAsync(forgot.Email, subject, body);
        }

        public async Task SendEmailVerificationAsync(string email, string token, string displayName)
        {
            string subject = "Xác minh tài khoản";
            string url = $"https://localhost:7095/api/ConfirmEmail?email={email}&token={token}";
            string body = $"Xin chào {displayName},<br><br>Vui lòng nhấn vào liên kết bên dưới để xác minh tài khoản của bạn:<br>" +
                          $"<a href='{url}'>Xác minh tài khoản</a><br><br>Link sẽ hết hạn sau 15 phút.";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendEmailAsync(string toEmail, string subject, string bodyHtml)
        {
            var mailSettings = _configuration.GetSection("MailSettings");

            using var smtpClient = new SmtpClient(mailSettings["Server"], int.Parse(mailSettings["Port"]))
            {
                UseDefaultCredentials = false,
                EnableSsl = true,
                Credentials = new NetworkCredential(mailSettings["UserName"], mailSettings["Password"])
            };

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(mailSettings["SenderEmail"], mailSettings["SenderName"]),
                Subject = subject,
                IsBodyHtml = true
            };

            mailMessage.To.Add(new MailAddress(toEmail));

            // Đường dẫn logo - kiểm tra nhiều đường dẫn có thể
            string[] possibleLogoPaths = {
                    Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "logo_HightPoly.png"),
                    Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "logo_HighPoly.png"), // Chữ P viết hoa
                    Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "logo_HightPoly.png"),
                    Path.Combine(Directory.GetCurrentDirectory(), "Assets", "img", "logo_HightPoly.png")
                };

            string? logoPath = possibleLogoPaths.FirstOrDefault(File.Exists);

            if (!string.IsNullOrEmpty(logoPath))
            {
                try
                {
                    // Đọc file logo vào byte array
                    byte[] logoBytes = await File.ReadAllBytesAsync(logoPath);

                    // Tạo AlternateView HTML
                    var htmlView = AlternateView.CreateAlternateViewFromString(bodyHtml, null, MediaTypeNames.Text.Html);

                    // Tạo LinkedResource từ byte array
                    using var logoStream = new MemoryStream(logoBytes);
                    var logo = new LinkedResource(logoStream, "image/png") // Fix: use string "image/png"
                    {
                        ContentId = "logoHighPoly",
                        TransferEncoding = TransferEncoding.Base64
                    };

                    htmlView.LinkedResources.Add(logo);
                    mailMessage.AlternateViews.Add(htmlView);

                    Console.WriteLine($"Logo found and attached: {logoPath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error attaching logo: {ex.Message}");
                    // Nếu có lỗi với logo, gửi email không có logo
                    mailMessage.Body = bodyHtml.Replace("cid:logoHighPoly", ""); // Loại bỏ logo reference
                }
            }
            else
            {
                Console.WriteLine("Logo not found in any of the expected paths:");
                foreach (var path in possibleLogoPaths)
                {
                    Console.WriteLine($"- {path}");
                }

                // Gửi email không có logo, thay thế bằng text
                string bodyWithoutLogo = bodyHtml.Replace(
                    "<img src=\"cid:logoHighPoly\" alt=\"Logo ThoiTrangHighpolyShop\" style=\"width:120px; height:auto;\" />",
                    "<div style=\"width:120px; height:60px; background:#fff; color:#333; display:flex; align-items:center; justify-content:center; border-radius:10px; font-weight:bold; margin:0 auto;\">LOGO</div>"
                );
                mailMessage.Body = bodyWithoutLogo;
            }

            await smtpClient.SendMailAsync(mailMessage);
        }

        // Phương thức alternative với Base64 embedded
        public async Task SendEmailWithEmbeddedLogoAsync(string toEmail, string subject, string bodyHtml)
        {
            var mailSettings = _configuration.GetSection("MailSettings");

            using var smtpClient = new SmtpClient(mailSettings["Server"], int.Parse(mailSettings["Port"]))
            {
                UseDefaultCredentials = false,
                EnableSsl = true,
                Credentials = new NetworkCredential(mailSettings["UserName"], mailSettings["Password"])
            };

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(mailSettings["SenderEmail"], mailSettings["SenderName"]),
                Subject = subject,
                IsBodyHtml = true
            };

            mailMessage.To.Add(new MailAddress(toEmail));

            // Tìm logo file
            string[] possibleLogoPaths = {
                    Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "logo_HightPoly.png"),
                    Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "logo_HighPoly.png"),
                    Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "logo_HightPoly.png")
                };

            string? logoPath = possibleLogoPaths.FirstOrDefault(File.Exists);

            if (!string.IsNullOrEmpty(logoPath))
            {
                try
                {
                    // Đọc logo và chuyển thành Base64
                    byte[] logoBytes = await File.ReadAllBytesAsync(logoPath);
                    string base64Logo = Convert.ToBase64String(logoBytes);
                    string mimeType = Path.GetExtension(logoPath).ToLower() switch
                    {
                        ".png" => "image/png",
                        ".jpg" or ".jpeg" => "image/jpeg",
                        ".gif" => "image/gif",
                        _ => "image/png"
                    };

                    // Thay thế cid: bằng data URL
                    string bodyWithEmbeddedLogo = bodyHtml.Replace(
                        "cid:logoHighPoly",
                        $"data:{mimeType};base64,{base64Logo}"
                    );

                    mailMessage.Body = bodyWithEmbeddedLogo;
                    Console.WriteLine($"Logo embedded as Base64: {logoPath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error embedding logo: {ex.Message}");
                    // Fallback: text logo
                    string bodyWithTextLogo = bodyHtml.Replace(
                        "<img src=\"cid:logoHighPoly\" alt=\"Logo ThoiTrangHighpolyShop\" style=\"width:120px; height:auto;\" />",
                        "<div style=\"width:120px; height:60px; background:linear-gradient(135deg,#667eea,#764ba2); color:white; display:flex; align-items:center; justify-content:center; border-radius:10px; font-weight:bold; margin:0 auto; font-size:12px;\">HIGHPOLY</div>"
                    );
                    mailMessage.Body = bodyWithTextLogo;
                }
            }
            else
            {
                // Không tìm thấy logo - sử dụng text replacement
                string bodyWithTextLogo = bodyHtml.Replace(
                    "<img src=\"cid:logoHighPoly\" alt=\"Logo ThoiTrangHighpolyShop\" style=\"width:120px; height:auto;\" />",
                    "<div style=\"width:120px; height:60px; background:linear-gradient(135deg,#667eea,#764ba2); color:white; display:flex; align-items:center; justify-content:center; border-radius:10px; font-weight:bold; margin:0 auto; font-size:12px; text-shadow:1px 1px 2px rgba(0,0,0,0.3);\">🏪 HIGHPOLY</div>"
                );
                mailMessage.Body = bodyWithTextLogo;
                Console.WriteLine("Logo not found, using text replacement");
            }

            await smtpClient.SendMailAsync(mailMessage);
        }

        // Debug method để kiểm tra đường dẫn file
        public void CheckLogoPath()
        {
            string[] possiblePaths = {
                    Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "logo_HightPoly.png"),
                    Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "logo_HighPoly.png"),
                    Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "logo_HightPoly.png"),
                    Path.Combine(Directory.GetCurrentDirectory(), "Assets", "img", "logo_HightPoly.png")
                };

            Console.WriteLine("=== CHECKING LOGO PATHS ===");
            Console.WriteLine($"Current Directory: {Directory.GetCurrentDirectory()}");

            foreach (var path in possiblePaths)
            {
                bool exists = File.Exists(path);
                Console.WriteLine($"{(exists ? "✓" : "✗")} {path}");
            }

            // Liệt kê tất cả files trong thư mục img
            string imgFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img");
            if (Directory.Exists(imgFolder))
            {
                Console.WriteLine($"\n=== FILES IN {imgFolder} ===");
                var files = Directory.GetFiles(imgFolder, "*.png");
                foreach (var file in files)
                {
                    Console.WriteLine($"Found: {Path.GetFileName(file)}");
                }
            }
        }
    }
}

