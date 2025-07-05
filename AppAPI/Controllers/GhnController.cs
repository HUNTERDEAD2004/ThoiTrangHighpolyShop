using Microsoft.AspNetCore.Mvc;
using AppAPI.Services;

namespace AppAPI.Controllers
{
    [ApiController]
    [Route("api/ghn")]
    public class GhnController : ControllerBase
    {
        private readonly GHNService _ghnService;

        public GhnController(GHNService ghnService)
        {
            _ghnService = ghnService;
        }

        [HttpGet("provinces")]
        public async Task<IActionResult> GetProvinces()
        {
            var data = await _ghnService.GetAllProvinces();
            return Ok(data);
        }

        [HttpGet("districts/{provinceId}")]
        public async Task<IActionResult> GetDistricts(int provinceId)
        {
            var districts = await _ghnService.GetDistrictsByProvinceId(provinceId);
            return Ok(districts);
        }

        [HttpGet("province-id/{name}")]
        public async Task<IActionResult> GetProvinceId(string name)
        {
            var id = await _ghnService.GetProvinceIdByName(name);
            return id == null ? NotFound() : Ok(new { id });
        }

        [HttpGet("district-id")]
        public async Task<IActionResult> GetDistrictId(string provinceName, string districtName)
        {
            var provinceId = await _ghnService.GetProvinceIdByName(provinceName);
            if (provinceId == null) return NotFound(new { error = "Province not found" });

            var districtId = await _ghnService.GetDistrictIdByName(districtName, provinceId.Value);
            return districtId == null ? NotFound() : Ok(new { id = districtId });
        }

        [HttpGet("wards/{districtId}")]
        public async Task<IActionResult> GetWards(int districtId)
        {
            var wards = await _ghnService.GetWardsByDistrictId(districtId);
            return Ok(wards);
        }


        [HttpGet("ward-code")]
        public async Task<IActionResult> GetWardCode(string provinceName, string districtName, string wardName)
        {
            var provinceId = await _ghnService.GetProvinceIdByName(provinceName);
            if (provinceId == null) return NotFound();

            var districtId = await _ghnService.GetDistrictIdByName(districtName, provinceId.Value);
            if (districtId == null) return NotFound();

            var wardCode = await _ghnService.GetWardCode(wardName, districtId.Value);
            return wardCode == null ? NotFound() : Ok(new { wardCode });
        }

        [HttpPost("calculate-fee")]
        public async Task<IActionResult> CalculateFee([FromBody] CalculateFeeRequest req)
        {
            var provinceId = await _ghnService.GetProvinceIdByName(req.Province);
            if (provinceId == null) return BadRequest("Invalid province");

            var districtId = await _ghnService.GetDistrictIdByName(req.District, provinceId.Value);
            if (districtId == null) return BadRequest("Invalid district");

            var wardCode = await _ghnService.GetWardCode(req.Ward, districtId.Value);
            if (wardCode == null) return BadRequest("Invalid ward");

            var fee = await _ghnService.CalculateShippingFee(districtId.Value, wardCode, req.InsuranceValue, req.Coupon);
            return fee.Success ? Ok(fee) : BadRequest(fee.ErrorMessage);
        }
    }

    public class CalculateFeeRequest
    {
        public string Province { get; set; } = "";
        public string District { get; set; } = "";
        public string Ward { get; set; } = "";
        public int InsuranceValue { get; set; }
        public string? Coupon { get; set; }
    }
}
