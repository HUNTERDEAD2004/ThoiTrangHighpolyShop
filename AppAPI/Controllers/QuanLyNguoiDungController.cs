using AppAPI.IServices;
using AppAPI.Services;
using AppData.Models;
using AppData.ViewModels;
using AppData.ViewModels.QLND;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AppAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuanLyNguoiDungController : ControllerBase
    {
        private IQuanLyNguoiDungService service;
        private readonly IConfiguration _configuration;
        private readonly EmailService mailService;
     

        public QuanLyNguoiDungController(
            IQuanLyNguoiDungService service,
            IConfiguration configuration,
            EmailService mailService)
        {
            this.service = service;
            _configuration = configuration;
            this.mailService = mailService;
        }


        [HttpGet("DangNhap")]
        public async Task<IActionResult> Login(string lg, string password)
        {
            var login = await service.Login(lg, password);

            if (login.Message == "Đăng nhập thành công")
            {
                // ✅ Gán session ở đây
                HttpContext.Session.SetString("LoginInfor", JsonConvert.SerializeObject(login));

                return Ok(login);
            }

            if (login.IsAccountLocked)
            {
                return Unauthorized(new { error = login.Message });
            }

            return BadRequest(new { error = login.Message });
        }







        [HttpPost("XacThucEmail")]
        public async Task<IActionResult> XacThucEmail([FromBody] XacThucEmailViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest("Thiếu email hoặc mã xác minh");

            var result = await service.ConfirmEmail(model.Email, model.Token);

            if (!result)
                return BadRequest("Mã xác thực không hợp lệ hoặc đã hết hạn.");

            return Ok("Tài khoản đã được xác thực thành công.");
        }



        [HttpPost("DangKyKhachHang")]
        public async Task<IActionResult> DangKyKhachHang(KhachHangViewModel khachHang)
        {
            try
            {
                var kh = await service.RegisterKhachHang(khachHang);

                if (kh == null)
                {
                    return BadRequest("Email hoặc số điện thoại đã được sử dụng.");
                }

                return Ok("Đăng ký thành công! Vui lòng kiểm tra email để xác minh tài khoản.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Đã xảy ra lỗi hệ thống: " + ex.Message);
            }
        }


        [HttpPut("DoiMatKhau")]
        public async Task<IActionResult> DoiMatKhau(string email, string oldPassword, string newPassword)
        {
            var dmk = await service.ChangePassword(email, oldPassword, newPassword);
            if (!dmk)
            {
                return BadRequest("Đổi mật khẩu  thành công");

            }
            else
            {
                return Ok("Đổi mật khẩu khong  thành công");
            }
        }
        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword(string request)
        {
            if (!IsValidEmailAddress(request))
            {
                return BadRequest("địa chỉ email không hợp lệ");
            }

            var result = await service.ForgetPassword(request);

            if (result)
            {
                return Ok("Success");
            }
            else
            {
                return BadRequest("Loi khi gui mail");
            }
        }
        private bool IsValidEmailAddress(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return false;
            }

            var emailAddressAttribute = new EmailAddressAttribute();

            return emailAddressAttribute.IsValid(email);
        }
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
        {
            if (!IsValidEmailAddress(request.Email))
            {
                return BadRequest("địa chỉ email không hợp lệ");
            }

            if (!IsValidPassword(request.Password))
            {
                return BadRequest("Mật khẩu không hợp lệ");
            }

            if (request.Password != request.ConfirmPassword)
            {
                return BadRequest("mật khẩu không khớp");
            }

            var result = await service.ResetPassword(request);

            if (result)
            {
                return Ok("Đặt lại mật khẩu thành công");
            }
            else
            {
                return BadRequest("Không thể đặt lại mật khẩu");
            }
        }
        private bool IsValidPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return false;
            }

            // Password must have at least 8 characters
            if (password.Length < 8)
            {
                return false;
            }

            var hasUpperCase = false;
            var hasLowerCase = false;
            var hasDigit = false;
            var hasSpecialChar = false;

            // Check each character in the password
            foreach (var c in password)
            {
                if (char.IsUpper(c))
                {
                    hasUpperCase = true;
                }
                else if (char.IsLower(c))
                {
                    hasLowerCase = true;
                }
                else if (char.IsDigit(c))
                {
                    hasDigit = true;
                }
                else
                {
                    hasSpecialChar = true;
                }
            }

            return hasUpperCase && hasLowerCase && hasDigit && hasSpecialChar;
        }
        //Tam
        [HttpPut("ChangePassword")]
        public async Task<IActionResult> DoiMatKhau(ChangePasswordRequest request)
        {
            var dmk = await service.ChangePassword(request);
            if (!dmk)
            {
                return BadRequest("Đổi mật khẩu khong  thành công");
            }
            else
            {
                return Ok("Đổi mật khẩu  thành công");
            }
        }
        [HttpPut("UpdateProfile1")]
        public async Task<IActionResult> UpdateProfile(LoginViewModel request)
        {
            LoginViewModel dmk = await service.UpdateProfile(request);
            if (dmk == null)
            {
                return BadRequest("Đổi thông tin người dùng không  thành công");
            }
            else
            {
                return Ok(dmk);
            }
        }
        [HttpGet("UseDiemTich")]
        public async Task<int> UseDiemTich(int diem, string id)
        {
            return await service.UseDiemTich(diem, id);
        }
        //End
        //Nhinh
        [HttpPost("AddNhanhKH")]
        public async Task<bool> AddNhanhKH(KhachHang kh)
        {
            return await service.AddNhanhKH(kh);
        }
    }
}
