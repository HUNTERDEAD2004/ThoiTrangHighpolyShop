using AppAPI.IServices;
using AppData.ViewModels.DTO;
using Microsoft.AspNetCore.Mvc;

namespace AppAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DiaChiController : ControllerBase
    {
        private readonly IDiaChiService _diaChiService;

        public DiaChiController(IDiaChiService diaChiService)
        {
            _diaChiService = diaChiService;
        }

        [HttpPost("khachhang/{id}/diachi")]
        public async Task<IActionResult> AddDiaChi(Guid id, [FromBody] DiaChiDTO dto)
        {
            var result = await _diaChiService.AddDiaChiAsync(id, dto);
            return result == null ? NotFound("Không tìm thấy khách hàng") : Ok(result);
        }

        [HttpGet("khachhang/{id}/diachi")]
        public IActionResult GetDiaChis(Guid id)
        {
            var list = _diaChiService.GetDiaChis(id);
            return Ok(list);
        }

        [HttpPut("diachi/{diaChiId}")]
        public async Task<IActionResult> UpdateDiaChi(Guid diaChiId, [FromBody] DiaChiDTO dto)
        {
            var result = await _diaChiService.UpdateDiaChiAsync(diaChiId, dto);
            return result == null ? NotFound("Không tìm thấy địa chỉ") : Ok(result);
        }

        [HttpDelete("diachi/{diaChiId}")]
        public async Task<IActionResult> DeleteDiaChi(Guid diaChiId)
        {
            var result = await _diaChiService.DeleteDiaChiAsync(diaChiId);
            return result == null ? NotFound("Không tìm thấy địa chỉ") : Ok(result);
        }

        [HttpGet("khachhang/{id}/diachi-default")]
        public async Task<IActionResult> GetDefaultDiaChi(Guid id)
        {
            var diaChi = await _diaChiService.GetDefaultDiaChiAsync(id);
            return diaChi == null ? NotFound("Không có địa chỉ mặc định") : Ok(diaChi);
        }

        [HttpPost("{khachHangId}/datmacdinh/{diaChiId}")]
        public async Task<IActionResult> SetDefault(Guid khachHangId, Guid diaChiId)
        {
            var result = await _diaChiService.SetDefaultDiaChiAsync(khachHangId, diaChiId);
            if (result == null) return NotFound();
            return Ok(result);
        }
        [HttpGet("diachi/{diaChiId}")]
        public async Task<IActionResult> GetDiaChiById(Guid diaChiId)
        {
            var diaChi = await _diaChiService.GetDiaChiByIdAsync(diaChiId);
            return diaChi == null ? NotFound("Không tìm thấy địa chỉ") : Ok(diaChi);
        }

    }
}
