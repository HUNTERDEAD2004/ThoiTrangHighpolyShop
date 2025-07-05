using AppAPI.IServices;
using AppAPI.Services;
using AppData.Models;
using AppData.ViewModels;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AppAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NhanVienController : ControllerBase
    {
        private readonly INhanVienService _nhanVienService;
        private readonly AssignmentDBContext _dbContext;
        public NhanVienController()
        {
            _nhanVienService = new NhanVienService();
            _dbContext = new AssignmentDBContext();
        }
        // GET: api/<NhanVienController>
        [HttpGet("GetAll")]
        public List<NhanVien> GetAllNhanVien()
        {
            return _nhanVienService.GetAll();
        }
        [Route("TimKiemNhanVien")]
        [HttpGet]
        public List<NhanVien> GetAllNhanVien(string? name)
        {
            return _dbContext.NhanViens.Where(v => v.Ten.Contains(name) || v.SDT.Contains(name)).ToList();


        }

        // GET api/<NhanVienController>/5
        [Route("GetById")]
        [HttpGet]
        public NhanVien? GetById(Guid id)
        {
            return _nhanVienService.GetById(id);
        }


        // POST api/<NhanVienController>
        //[HttpPost("DangKyNhanVien")]
        //public async Task<IActionResult> Add(string ten, string email, string manhanvien, DateTime ngaysinh, int gioitinh, string password, string sdt, string diachi, int trangthai, Guid idvaitro)
        //{
        //    var tr = await _nhanVienService.Add(ten, email,manhanvien,ngaysinh,gioitinh, password, sdt, diachi, trangthai, idvaitro);
        //    if (tr == null)
        //    {
        //        return BadRequest();
        //    }
        //    return Ok(tr);
        //}





        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] NhanVienViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _nhanVienService.Add(model);

            if (result == null)
                return BadRequest("Email hoặc SĐT đã tồn tại.");

            return Ok(result); 
        }






        // PUT api/<NhanVienController>/5
        [HttpPut("{id}")]
        public IActionResult Put(Guid id, [FromBody] NhanVien nv)
        {
            var existing = _nhanVienService.GetById(id);
            if (existing == null) return NotFound();

            // Cập nhật dữ liệu từ nv truyền vào
            existing.Ten = nv.Ten;
            existing.Email = nv.Email;
            existing.MaNhanVien = nv.MaNhanVien;
            existing.NgaySinh = nv.NgaySinh;
            existing.GioiTinh = nv.GioiTinh;
            existing.PassWord = nv.PassWord;
            existing.SDT = nv.SDT;
            existing.DiaChi = nv.DiaChi;
            existing.TrangThai = nv.TrangThai;

            // Gắn cứng IDVaiTro mặc định
            existing.IDVaiTro = Guid.Parse("952C1A5D-74FF-4DAF-BA88-135C5440809C");

            _dbContext.NhanViens.Update(existing);
            _dbContext.SaveChanges();

            return Ok(true);
        }


        // DELETE api/<NhanVienController>/5
        [HttpDelete("{id}")]
        public bool Delete(Guid id)
        {
            if (_nhanVienService.Delete(id))
            {
                return true;
            }
            return false;
        }

    }
}
