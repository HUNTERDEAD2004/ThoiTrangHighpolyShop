//using AppAPI.IServices;
//using AppData.Models;
//using Microsoft.AspNetCore.Mvc;

//// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

//namespace AppAPI.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class ChiTietKhuyenMaiController : ControllerBase
//    {
//        private readonly IChiTietKhuyenMaiServices _services;
//        public ChiTietKhuyenMaiController(IChiTietKhuyenMaiServices services)
//        {
//            this._services = services;
//        }
//        // GET: api/<ChiTietKhuyenMaiController>
//        [HttpGet]
//        public List<ChiTietKhuyenMai> Get()
//        {
//            return _services.GetAll();
//        }

//        // GET api/<ChiTietKhuyenMaiController>/5
//        [HttpGet("{id}")]
//        public ChiTietKhuyenMai Get(Guid id)
//        {
//            return _services.GetById(id);
//        }

//        // POST api/<ChiTietKhuyenMaiController>
//        [HttpPost]
//        public bool Post(int TrangThai, Guid IdBienThe, Guid IdKhuyenMai)
//        {
//            return _services.Add(TrangThai, IdBienThe, IdKhuyenMai);
//        }

//        // PUT api/<ChiTietKhuyenMaiController>/5
//        [HttpPut("{id}")]
//        public bool Put(Guid id, int TrangThai, Guid IdBienThe, Guid IdKhuyenMai)
//        {
//            var trangThai = _services.GetById(id);
//            if(trangThai != null)
//            {
//                return _services.Update(trangThai.ID, TrangThai, IdBienThe, IdKhuyenMai);
//            }
//            else
//            {
//                return false;
//            }
//        }

//        // DELETE api/<ChiTietKhuyenMaiController>/5
//        [HttpDelete("{id}")]
//        public bool Delete(Guid id)
//        {
//            var trangThai = _services.GetById(id);
//            if (trangThai != null)
//            {
//                return _services.Delete(trangThai.ID);
//            }
//            else
//            {
//                return false;
//            }
//        }
//    }
//}
