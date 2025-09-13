using AppData.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AppAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserVoucherController : ControllerBase
    {
        private readonly AssignmentDBContext _context;

        public UserVoucherController()
        {
            _context = new AssignmentDBContext();
        }

        public class AssignRequest
        {
            public Guid VoucherId { get; set; }
            public List<Guid> CustomerIds { get; set; } = new List<Guid>();
        }

        // Lấy danh sách ID khách hàng đã được gán một voucher
        [HttpGet("assigned-ids")]
        public IActionResult GetAssignedCustomerIds([FromQuery] Guid voucherId)
        {
            if (voucherId == Guid.Empty)
            {
                return BadRequest("voucherId không hợp lệ");
            }

            var ids = _context.UserVouchers
                .Where(uv => uv.IDVoucher == voucherId)
                .Select(uv => uv.IDKhachHang)
                .ToList();

            return Ok(ids);
        }

        [HttpPost("assign")]
        public IActionResult AssignVoucherToCustomers([FromBody] AssignRequest request)
        {
            if (request.VoucherId == Guid.Empty || request.CustomerIds == null || request.CustomerIds.Count == 0)
            {
                return BadRequest("Thiếu thông tin cần thiết");
            }

            var voucher = _context.Vouchers.FirstOrDefault(v => v.ID == request.VoucherId);
            if (voucher == null)
            {
                return NotFound("Voucher không tồn tại");
            }

            var existing = _context.UserVouchers
                .Where(uv => uv.IDVoucher == request.VoucherId && request.CustomerIds.Contains(uv.IDKhachHang))
                .Select(uv => uv.IDKhachHang)
                .ToHashSet();

            var newMappings = request.CustomerIds
                .Where(id => !existing.Contains(id))
                .Select(id => new UserVoucher
                {
                    ID = Guid.NewGuid(),
                    IDKhachHang = id,
                    IDVoucher = request.VoucherId,
                    DaSuDung = false
                }).ToList();

            if (newMappings.Count == 0)
            {
                return Ok(0);
            }

            _context.UserVouchers.AddRange(newMappings);
            _context.SaveChanges();
            return Ok(newMappings.Count);
        }
    }
}




