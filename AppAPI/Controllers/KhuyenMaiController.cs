using AppAPI.IServices;
using AppAPI.Services;
using AppData.IRepositories;
using AppData.Models;
using AppData.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text.RegularExpressions;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AppAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KhuyenMaiController : ControllerBase
    {
        private readonly IKhuyenMaiServices _khuyenmai;
        private readonly AssignmentDBContext _dbcontext;

        public KhuyenMaiController()
        {
            _dbcontext = new AssignmentDBContext();
            _khuyenmai = new KhuyenMaiServices();
        }

        // GET: api/<KhuyenMaiController>
        [HttpGet]
        public List<KhuyenMai> Get()
        {
            return _khuyenmai.GetAll();
        }
        [Route("GetAllCTSPBySP")]
        [HttpGet]
        public async Task<List<AllViewCTSP>> GetAllCTSPBySp(Guid idSanPham)
        {
            if (!_dbcontext.ChiTietSanPhams.Any(c => c.IDSanPham == idSanPham)) throw new Exception($" khong tim thay san pham co id:{idSanPham}");
            var AllCTSP = await (from CTSP in _dbcontext.ChiTietSanPhams.AsNoTracking()
                                 join mausac in _dbcontext.MauSacs.AsNoTracking() on CTSP.IDMauSac equals mausac.ID
                                 join size in _dbcontext.KichCos.AsNoTracking() on CTSP.IDKichCo equals size.ID
                                 join sp in _dbcontext.SanPhams.AsNoTracking() on CTSP.IDSanPham equals sp.ID
                                 where CTSP.IDSanPham == idSanPham
                                 select new AllViewCTSP()
                                 {
                                     ID = CTSP.ID,
                                     MaCTSP = CTSP.MaSPChiTiet,
                                     TenSanPham = sp.Ten,
                                     TenAnh = (from spct in _dbcontext.ChiTietSanPhams.AsNoTracking()
                                               join sp in _dbcontext.SanPhams.AsNoTracking() on spct.IDSanPham equals sp.ID
                                               join mausac in _dbcontext.MauSacs.AsNoTracking() on spct.IDMauSac equals mausac.ID
                                               join a in _dbcontext.Anhs.AsNoTracking() on spct.ID equals a.IDSanPhamChiTiet
                                               where a.IDSanPhamChiTiet == spct.ID
                                               select a.DuongDan).FirstOrDefault() ?? "wwwroot\\img\\Default.png",
                                     IdKhuyenMai = (from km in _dbcontext.KhuyenMais where CTSP.IDKhuyenMai == km.ID select CTSP.IDKhuyenMai).FirstOrDefault(),
                                     TenMauSac = mausac.Ten,
                                     MaMauSac = mausac.Ma,
                                     TenKichCo = size.Ten,
                                     GiaGoc = (int)CTSP.GiaBan, // Explicit cast to fix CS0266  
                                     GiaKhuyenMai = (int)CTSP.GiaBan, // Explicit cast to fix CS0266  
                                     SoLuong = CTSP.SoLuong,
                                     NgayTao = CTSP.NgayTao,
                                     TrangThai = CTSP.TrangThai
                                 }).ToListAsync();
            return AllCTSP;
        }
        // TimCTSP Theo Id Khuyen Mai
        [Route("GetCTSPByIdKm")]
        [HttpGet]
        public async Task<List<AllViewCTSP>> GetAllCTSPByIdKhuyenMai(Guid idkm)
        {
            if (!_dbcontext.KhuyenMais.Any(c => c.ID == idkm)) throw new Exception($" khong tim thay san pham co id:{idkm}");
            var AllCTSP = await (from CTSP in _dbcontext.ChiTietSanPhams.AsNoTracking()
                                 join mausac in _dbcontext.MauSacs.AsNoTracking() on CTSP.IDMauSac equals mausac.ID
                                 join size in _dbcontext.KichCos.AsNoTracking() on CTSP.IDKichCo equals size.ID
                                 join sp in _dbcontext.SanPhams.AsNoTracking() on CTSP.IDSanPham equals sp.ID

                                 select new AllViewCTSP()
                                 {
                                     ID = CTSP.ID,
                                     TenSanPham = sp.Ten,
                                     IdKhuyenMai = (from km in _dbcontext.KhuyenMais where CTSP.IDKhuyenMai == km.ID select CTSP.IDKhuyenMai).FirstOrDefault(),
                                     TenMauSac = mausac.Ten,
                                     TenKichCo = size.Ten,
                                     GiaGoc = (int)CTSP.GiaBan,
                                     GiaKhuyenMai = (int)CTSP.GiaBan,
                                     SoLuong = CTSP.SoLuong,
                                     NgayTao = CTSP.NgayTao,
                                     TrangThai = CTSP.TrangThai
                                 }).Where(x => x.IdKhuyenMai == idkm).ToListAsync();
            return AllCTSP;
        }

        [Route("GetAllSP")]
        [HttpGet]

        public List<AllViewSp> GetAllSP()
        {

            var result = _dbcontext.SanPhams

                            .Join(_dbcontext.ChatLieus, sp => sp.IDChatLieu, cl => cl.ID, (sp, cl) => new { sp, cl })
                            .Join(_dbcontext.LoaiSPs, x => x.sp.IDLoaiSP, lsp => lsp.ID, (x, lsp) => new { x.sp, x.cl, lsp })
                            .Join(_dbcontext.ChiTietSanPhams, x => x.sp.ID, ctsp => ctsp.IDSanPham, (x, ctsp) => new { x.sp, x.cl, x.lsp, ctsp })
                            .GroupBy(x => x.sp.ID)
                            .Select(group => new AllViewSp
                            {
                                ID = group.Key,
                                SoLuongCTSP = group.Count(x => x.ctsp.ID != null),
                                Ten = group.First().sp.Ten,
                                MoTa = group.First().sp.MoTa,
                                TenAnh = (from a in _dbcontext.Anhs
                                          join ctsp in _dbcontext.ChiTietSanPhams on a.IDSanPhamChiTiet equals ctsp.ID
                                          where @group.Any(g => g.ctsp.ID == ctsp.ID) && ctsp.IDSanPham == @group.First().sp.ID
                                          select a.DuongDan
                                         ).FirstOrDefault(),
                                IdKhuyenMai = (from km in _dbcontext.KhuyenMais where @group.First().ctsp.IDKhuyenMai == km.ID select km.ID).FirstOrDefault(),

                                GiaBan = (int)group.First().ctsp.GiaBan,
                                IDLoaiSP = group.First().lsp.ID,
                                IDChatLieu = group.First().cl.ID,
                                TrangThai = group.First().sp.TrangThai,
                            }).ToList();
            return result;

        }
        [Route("GetAllSPByKhuyenMai")]
        [HttpGet]

        public List<AllViewSp> GetAllSPByKhuyenMai(Guid idkm)
        {
            if (!_dbcontext.KhuyenMais.Any(c => c.ID == idkm)) throw new Exception($" khong tim thay san pham co id:{idkm}");
            var result = _dbcontext.SanPhams
                            .Join(_dbcontext.ChatLieus, sp => sp.IDChatLieu, cl => cl.ID, (sp, cl) => new { sp, cl })
                            .Join(_dbcontext.LoaiSPs, x => x.sp.IDLoaiSP, lsp => lsp.ID, (x, lsp) => new { x.sp, x.cl, lsp })
                            .Join(_dbcontext.ChiTietSanPhams, x => x.sp.ID, ctsp => ctsp.IDSanPham, (x, ctsp) => new { x.sp, x.cl, x.lsp, ctsp })
                            .GroupBy(x => x.sp.ID)
                            .Select(group => new AllViewSp
                            {
                                ID = group.Key,
                                SoLuongCTSP = group.Count(x => x.ctsp.ID != null),
                                Ten = group.First().sp.Ten,
                                MoTa = group.First().sp.MoTa,
                                TenAnh = (from a in _dbcontext.Anhs
                                          join ctsp in _dbcontext.ChiTietSanPhams on a.IDSanPhamChiTiet equals ctsp.ID
                                          where @group.Any(g => g.ctsp.ID == ctsp.ID) && ctsp.IDSanPham == @group.First().sp.ID
                                          select a.DuongDan
                                         ).FirstOrDefault(),
                                IdKhuyenMai = (from km in _dbcontext.KhuyenMais where @group.First().ctsp.IDKhuyenMai == km.ID select km.ID).FirstOrDefault(),

                                GiaBan = (int)group.First().ctsp.GiaBan,
                                IDLoaiSP = group.First().lsp.ID,
                                IDChatLieu = group.First().cl.ID,
                                TrangThai = group.First().sp.TrangThai,
                            }).Where(x => x.IdKhuyenMai == idkm).ToList();
            return result;

        }
        [Route("GetAllSPByKmLoaiSPChatLieu")]
        [HttpGet]

        public async Task<List<AllViewSp>> GetAllSPByKm(Guid? idkm, Guid? idLoaiSP, Guid? idChatLieu)
        {
            if (!_dbcontext.KhuyenMais.Any(c => c.ID == idkm) && !_dbcontext.LoaiSPs.Any(c => c.ID == idLoaiSP) && !_dbcontext.ChatLieus.Any(y => y.ID == idChatLieu)) throw new Exception($" khong tim thay san pham co id:{idkm},{idLoaiSP},{idChatLieu}");
            var AllCTSP = (from SP in _dbcontext.SanPhams.AsNoTracking()
                           join loaisp in _dbcontext.LoaiSPs.AsNoTracking() on SP.IDLoaiSP equals loaisp.ID
                           join chatlieu in _dbcontext.ChatLieus.AsNoTracking() on SP.IDChatLieu equals chatlieu.ID
                           join CTSP in _dbcontext.ChiTietSanPhams.AsNoTracking() on SP.ID equals CTSP.IDSanPham
                           join anh in _dbcontext.Anhs.AsNoTracking() on CTSP.ID equals anh.IDSanPhamChiTiet
                           join km in _dbcontext.KhuyenMais.AsNoTracking() on CTSP.IDKhuyenMai equals km.ID
                           select new { SP, anh, loaisp, chatlieu, CTSP, km });
            // Tim Theo IdKhuyenMai
            if (!string.IsNullOrEmpty(idkm.ToString()))
            {
                AllCTSP = AllCTSP.AsNoTracking().Where(x => x.km.ID == idkm);
            }
            if (!string.IsNullOrEmpty(idLoaiSP.ToString()))
            {
                AllCTSP = AllCTSP.AsNoTracking().Where(x => x.loaisp.ID == idLoaiSP);

            }
            if (!string.IsNullOrEmpty(idChatLieu.ToString()))
            {
                AllCTSP = AllCTSP.AsNoTracking().Where(x => x.chatlieu.ID == idChatLieu);
            }
            var result = await AllCTSP.AsNoTracking().Select(c => new AllViewSp()
            {
                ID = c.SP.ID,
                Ten = c.SP.Ten,
                MoTa = c.SP.MoTa,
                TenAnh = c.anh.DuongDan,
                IdKhuyenMai = c.km.ID,

                GiaBan = (int)c.CTSP.GiaBan,
                IDLoaiSP = c.SP.IDLoaiSP,
                IDChatLieu = c.chatlieu.ID,
                TrangThai = c.CTSP.TrangThai

            }).ToListAsync();
            return result;
        }
        [Route("GetAllSPNoKM")]
        [HttpGet]

        public List<AllViewSp> GetAllSPNoKm(Guid id)
        {
            if (!_dbcontext.KhuyenMais.Any(c => c.ID == id)) throw new Exception($" khong tim thay san pham co id:{id}");
            var result = _dbcontext.SanPhams
                            .Join(_dbcontext.ChatLieus, sp => sp.IDChatLieu, cl => cl.ID, (sp, cl) => new { sp, cl })
                            .Join(_dbcontext.LoaiSPs, x => x.sp.IDLoaiSP, lsp => lsp.ID, (x, lsp) => new { x.sp, x.cl, lsp })
                            .Join(_dbcontext.ChiTietSanPhams, x => x.sp.ID, ctsp => ctsp.IDSanPham, (x, ctsp) => new { x.sp, x.cl, x.lsp, ctsp })
                            .GroupBy(x => x.sp.ID)
                            .Select(group => new AllViewSp
                            {
                                ID = group.Key,
                                SoLuongCTSP = group.Count(x => x.ctsp.ID != null),
                                Ten = group.First().sp.Ten,
                                MoTa = group.First().sp.MoTa,
                                TenAnh = (from a in _dbcontext.Anhs
                                          join ctsp in _dbcontext.ChiTietSanPhams on a.IDSanPhamChiTiet equals ctsp.ID
                                          where @group.Any(g => g.ctsp.ID == ctsp.ID) && ctsp.IDSanPham == @group.First().sp.ID
                                          select a.DuongDan
                                         ).FirstOrDefault(),
                                IdKhuyenMai = (from km in _dbcontext.KhuyenMais where @group.First().ctsp.IDKhuyenMai == km.ID select km.ID).FirstOrDefault(),

                                GiaBan = (int)group.First().ctsp.GiaBan,
                                IDLoaiSP = group.First().lsp.ID,
                                IDChatLieu = group.First().cl.ID,
                                TrangThai = group.First().sp.TrangThai,
                            }).Where(x => x.IdKhuyenMai != id).Where(x => x.TrangThai == 1).ToList();
            return result;
        }
        [Route("GetAllSPNoKMByLoaiSPChatLieu")]
        [HttpGet]

        public List<AllViewSp> TKGetAllSPNoKmByLoaiSPChatLieu(Guid id, Guid? idLoaiSP, Guid? idChatLieu)
        {
            if (!_dbcontext.LoaiSPs.Any(c => c.ID == idLoaiSP) && !_dbcontext.ChatLieus.Any(y => y.ID == idChatLieu)) throw new Exception($" khong tim thay san pham co id:{idLoaiSP},{idChatLieu}");
            //var AllCTSP = (from SP in _dbcontext.SanPhams.AsNoTracking()
            //               join anh in _dbcontext.Anhs.AsNoTracking() on SP.ID equals anh.IDSanPham
            //               join loaisp in _dbcontext.LoaiSPs.AsNoTracking() on SP.IDLoaiSP equals loaisp.ID
            //               join chatlieu in _dbcontext.ChatLieus.AsNoTracking() on SP.IDChatLieu equals chatlieu.ID
            //               join CTSP in _dbcontext.ChiTietSanPhams.AsNoTracking() on SP.ID equals CTSP.IDSanPham

            //               select new { SP, anh, loaisp, chatlieu, CTSP });
            //// Tim Theo LoaiSP,Chat Lieu

            //if (!string.IsNullOrEmpty(idLoaiSP.ToString()))
            //{
            //    AllCTSP = AllCTSP.AsNoTracking().Where(x => x.loaisp.ID == idLoaiSP);

            //}
            //if (!string.IsNullOrEmpty(idChatLieu.ToString()))
            //{
            //    AllCTSP = AllCTSP.AsNoTracking().Where(x => x.chatlieu.ID == idChatLieu);
            //}
            //var result = await AllCTSP.AsNoTracking().Select(c => new AllViewSp()
            //{
            //    ID = c.SP.ID,
            //    Ten = c.SP.Ten,
            //    MoTa = c.SP.MoTa,
            //    TenAnh = c.anh.DuongDan,
            //    IdKhuyenMai = (from km in _dbcontext.KhuyenMais where c.CTSP.IDKhuyenMai == km.ID select c.CTSP.IDKhuyenMai).FirstOrDefault(),
            //    TongSoSao = c.SP.TongSoSao,
            //    TongDanhGia = c.SP.TongDanhGia,
            //    GiaBan = c.CTSP.GiaBan,
            //    IDLoaiSP = c.SP.IDLoaiSP,
            //    IDLoaiSPCha = c.loaisp.IDLoaiSPCha,
            //    IDChatLieu = c.chatlieu.ID,
            //    TrangThai = c.CTSP.TrangThai

            //}).Where(x=>x.IdKhuyenMai!=id).ToListAsync();
            //return result;
            if (!_dbcontext.KhuyenMais.Any(c => c.ID == id)) throw new Exception($" khong tim thay san pham co id:{id}");
            var result = _dbcontext.SanPhams
                            .Join(_dbcontext.ChatLieus, sp => sp.IDChatLieu, cl => cl.ID, (sp, cl) => new { sp, cl })
                            .Join(_dbcontext.LoaiSPs, x => x.sp.IDLoaiSP, lsp => lsp.ID, (x, lsp) => new { x.sp, x.cl, lsp })
                            .Join(_dbcontext.ChiTietSanPhams, x => x.sp.ID, ctsp => ctsp.IDSanPham, (x, ctsp) => new { x.sp, x.cl, x.lsp, ctsp })
                            .GroupBy(x => x.sp.ID)
                            .Select(group => new AllViewSp
                            {
                                ID = group.Key,
                                SoLuongCTSP = group.Count(x => x.ctsp.ID != null),
                                Ten = group.First().sp.Ten,
                                MoTa = group.First().sp.MoTa,
                                TenAnh = (from a in _dbcontext.Anhs
                                          join ctsp in _dbcontext.ChiTietSanPhams on a.IDSanPhamChiTiet equals ctsp.ID
                                          where @group.Any(g => g.ctsp.ID == ctsp.ID) && ctsp.IDSanPham == @group.First().sp.ID
                                          select a.DuongDan
                                         ).FirstOrDefault(),
                                IdKhuyenMai = (from km in _dbcontext.KhuyenMais where @group.First().ctsp.IDKhuyenMai == km.ID select km.ID).FirstOrDefault(),

                                GiaBan = (int)group.First().ctsp.GiaBan,
                                IDLoaiSP = group.First().lsp.ID,
                                IDChatLieu = group.First().cl.ID,
                                TrangThai = group.First().sp.TrangThai,
                            }).Where(x => x.IdKhuyenMai != id).ToList();
            if (!string.IsNullOrEmpty(idLoaiSP.ToString()))
            {
                result = result.Where(x => x.IDLoaiSP == idLoaiSP).ToList();

            }
            if (!string.IsNullOrEmpty(idChatLieu.ToString()))
            {
                result = result.Where(x => x.IDChatLieu == idChatLieu).ToList();
            }
            return result;
        }

        // GET api/<KhuyenMaiController>/5
        [HttpGet("{id}")]
        public KhuyenMai Get(Guid id)
        {
            return _khuyenmai.GetById(id);
        }
        [Route("TimKiemTenKM")]
        [HttpGet]
        public List<KhuyenMai> GetByTen(string Ten)
        {
            return _khuyenmai.GetKMByName(Ten);
        }

        // POST api/<KhuyenMaiController>
        [HttpPost]
        public bool Post(KhuyenMaiView kmv)
        {
            return _khuyenmai.Add(kmv);
        }
        [Route("AddKmVoBT")]
        [HttpPut]
        // PUT api/<KhuyenMaiController>/5 
        public bool AddKMVoBienThe(List<Guid> bienThes, Guid IdKhuyenMai)
        {
            return _khuyenmai.AdKMVoBT(bienThes, IdKhuyenMai);
        }

        // Lam start
        [Route("XoaKmRaBT")]
        [HttpPut]
        // PUT api/<KhuyenMaiController>/5 
        public bool XoaKMRaBienThe(List<Guid> bienThes)
        {
            return _khuyenmai.XoaAllKMRaBT(bienThes);
        }
        //Lam end
        // PUT api/<KhuyenMaiController>/5

        [HttpPut("{id}")]
        public bool Put(KhuyenMaiView kmv)
        {
            var khuyenmai = _khuyenmai.GetById(kmv.ID);
            if (khuyenmai != null)
            {
                return _khuyenmai.Update(kmv);
            }
            else
            {
                return false;
            }
        }

        // DELETE api/<KhuyenMaiController>/5
        [HttpDelete("{id}")]
        public bool Delete(Guid id)
        {
            var khuyenmai = _khuyenmai.GetById(id);
            if (khuyenmai != null)
            {
                return _khuyenmai.Delete(khuyenmai.ID);
            }
            else
            {
                return false;
            }
        }
    }
}
