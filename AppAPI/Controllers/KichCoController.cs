using AppAPI.IServices;
using AppAPI.Services;
using AppData.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AppAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KichCoController : ControllerBase
    {
        private readonly IQlThuocTinhService service;
        private readonly AssignmentDBContext _dbContext;
        public KichCoController()
        {
            service = new QlThuocTinhService();
            _dbContext = new AssignmentDBContext();
        }
        #region KichCo
        [HttpGet("GetAllKichCo")]
        public async Task<IActionResult> GetAllKichCo()
        {
            var rn = await service.GetAllKichCo();
            return Ok(rn);
        }
        [Route("TimKiemKichCo")]
        [HttpGet]
        public async Task<IActionResult> GetAllKichCo(string? name)
        {
            var tr = _dbContext.KichCos.Where(v => v.Ten.Contains(name)).ToList();
            return Ok(tr);
        }
        [Route("GetKichCoById")]
        [HttpGet]
        public async Task<IActionResult> GetKichCoById(Guid id)
        {
            var tr = await service.GetKickCoById(id);
            if (tr == null) return BadRequest();
            return Ok(tr);
        }
		[HttpPost("ThemKichCo")]
		public async Task<IActionResult> Add(string ten, int trangthai)
		{
			if (string.IsNullOrWhiteSpace(ten))
				return BadRequest(new { success = false, message = "Tên kích cỡ không được để trống!" });

			// Kiểm tra trùng tên
			var exist = _dbContext.KichCos.FirstOrDefault(k => k.Ten.ToLower() == ten.ToLower());
			if (exist != null)
				return BadRequest(new { success = false, message = "Kích cỡ này đã tồn tại!" });

			var nv = await service.AddKichCo(ten, trangthai);
			if (nv == null)
				return BadRequest(new { success = false, message = "Thêm thất bại!" });

			// Trả về luôn đối tượng KichCo mới
			return Ok(nv);
		}

		// PUT api/<NhanVienController>/5
		[HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, string ten, int trangthai)
        {
            var bv = await service.UpdateKichCo(id, ten, trangthai);
            if (bv == null)
            {
                return BadRequest(); // Trả về BadRequest nếu tên trùng
            }

            return Ok(bv);

        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteKichCo(Guid id)
        {
            var loaiSP = await service.DeleteKichCo(id);
            return Ok(loaiSP);
        }
        #endregion
    }
}
