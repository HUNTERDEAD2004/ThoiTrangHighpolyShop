using AppAPI.IServices;
using AppAPI.Services;
using AppData.IRepositories;
using AppData.Models;
using AppData.Repositories;
using AppData.ViewModels;
using AppData.ViewModels.SanPham;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security;

namespace AppAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KhachHangController : ControllerBase
    {
        private readonly IKhachHangService _khachHangService;
        private readonly AssignmentDBContext _dbcontext;

        public KhachHangController(IKhachHangService khachHangService, AssignmentDBContext dbcontext)
        {
            _khachHangService = khachHangService;
            _dbcontext = dbcontext;
        }

        // GET: api/KhachHang
        [HttpGet]
        public async Task<List<KhachHang>> GetAll()
        {
            return await _dbcontext.KhachHangs.ToListAsync();
        }

        // GET: api/KhachHang/get-view-all
        [HttpGet("get-view-all")]
        public async Task<List<KhachHangViewModel>> GetAllWithDiaChi()
        {
            return await _khachHangService.GetAll();
        }

        // GET: api/KhachHang/TimKiemKH
        [HttpGet("TimKiemKH")]
        public List<KhachHang> GetAllKhachHang(string? Ten, string? SDT)
        {
            return _khachHangService.SearchKhachHang(Ten, SDT);
        }

        // GET: api/KhachHang/GetById
        [HttpGet("GetById")]
        public KhachHang GetById(Guid id)
        {
            return _khachHangService.GetById(id);
        }

        // GET: api/KhachHang/GetKhachHangByEmail
        [HttpGet("GetKhachHangByEmail")]
        public KhachHangViewModel GetKhachHangByEmail(string email)
        {
            return _khachHangService.GetKhachHangByEmail(email);
        }

        // GET: api/KhachHang/ChangeForgotPassword
        [HttpGet("ChangeForgotPassword")]
        public async Task<bool> ChangeForgotPassword(string id, string password)
        {
            if (Guid.TryParse(id, out var guidId))
            {
                return await _khachHangService.ChangeForgotPassword(guidId, password);
            }
            return false;
        }

        // GET: api/KhachHang/getBySDT
        [HttpGet("getBySDT")]
        public KhachHang GetBySDT(string sdt)
        {
            return _khachHangService.GetBySDT(sdt);
        }

        // GET: api/KhachHang/getAllHDKH
        [HttpGet("getAllHDKH")]
        public async Task<List<HoaDon>> GetAllHDKH(Guid idkh)
        {
            return await _khachHangService.GetAllHDKH(idkh);
        }

        // POST: api/KhachHang
        [HttpPost]
        public async Task<IActionResult> Post(KhachHangViewModel khachHang, bool isAdmin = false)
        {
            try
            {
                var kh = await _khachHangService.Add(khachHang, isAdmin);
                if (kh == null)
                {
                    return BadRequest("Email hoặc số điện thoại đã tồn tại");
                }

                return Ok(new
                {
                    Message = "Đăng ký thành công",
                    MaKhachHang = kh.MaKhachHang,
                    Email = kh.Email
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi: {ex.Message}");
            }
        }

        // PUT: api/KhachHang/PutKhView
        [HttpPut("PutKhView")]
        public IActionResult PutKhView(KhachHangViewModel khv)
        {
            try
            {
                var result = _khachHangService.Update(khv);
                if (!result)
                    return BadRequest("Cập nhật thất bại - Không tìm thấy khách hàng");

                return Ok("Cập nhật thành công");
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi: {ex.Message}");
            }
        }

        // DELETE: api/KhachHang/{id}
        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            try
            {
                var result = _khachHangService.Delete(id);
                if (!result)
                    return BadRequest("Xóa thất bại - Không tìm thấy khách hàng");

                return Ok("Xóa thành công");
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi: {ex.Message}");
            }
        }
    }
}