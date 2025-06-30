//using System.Net.Mail;
//using System.Net;
//using AppData.ViewModels.QLND;
//using AppAPI.IServices;

//namespace AppAPI.Services
//{
//    public class EmailService : IEmailService
//    {
//        public async Task SendForgotPasswordConfirmation(ForgotPasswordRequest forgot)
//        {
//            // Create an instance of SmtpClient
//            var smtpClient = new SmtpClient("smtp.gmail.com", 587);
//            smtpClient.UseDefaultCredentials = false;
//            smtpClient.EnableSsl = true;
//            smtpClient.Credentials = new NetworkCredential("nhuph20156@gmail.com", "Nhucong2003.");

//            // Create an instance of MailMessage
//            var mailMessage = new MailMessage();
//            mailMessage.From = new MailAddress("nhuph20156@gmail.com");
//            mailMessage.To.Add(new MailAddress(forgot.Email));
//            mailMessage.Subject = "Reset Password";
//            mailMessage.Body = "Please click the link below to reset your password:<br><a href='https://localhost:7095/api/ResetPassword?email=" + forgot.Email + "&token=" + forgot.Token + "'>Reset Password</a>";

//            // Send the email
//            await smtpClient.SendMailAsync(mailMessage);
//            mailMessage.Dispose();
//        }
//    }
//}
using System.Net;
using System.Net.Mail;
using AppData.ViewModels.QLND;
using AppAPI.IServices;
using Microsoft.Extensions.Configuration;

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

        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var mailSettings = _configuration.GetSection("MailSettings");

            var smtpClient = new SmtpClient(mailSettings["Server"], int.Parse(mailSettings["Port"]))
            {
                UseDefaultCredentials = false,
                EnableSsl = true,
                Credentials = new NetworkCredential(mailSettings["UserName"], mailSettings["Password"])
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(mailSettings["SenderEmail"], mailSettings["SenderName"]),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(new MailAddress(toEmail));
            await smtpClient.SendMailAsync(mailMessage);
            mailMessage.Dispose();
        }
    }
}

