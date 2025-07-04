using AppAPI.IServices;
using AppData.Models;
using AppData.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AppAPI.Services
{
    public class KhachHangService : IKhachHangService
    {
        private readonly AssignmentDBContext _dbContext;
        public KhachHangService()
        {
            _dbContext = new AssignmentDBContext();
        }

        public async Task<KhachHang> Add(KhachHangViewModel kh)
        {
       
            KhachHang khang = new KhachHang()
            {
                IDKhachHang = Guid.NewGuid(),
                Ten = kh.Ten,
                MaKhachHang = kh.MaKhachHang,
                GioiTinh = kh.GioiTinh,
                NgaySinh = kh.NgaySinh,
                Email = kh.Email,
                Password = kh.Password,
                SDT = kh.SDT,
                DiemTich = kh.DiemTich,
                TrangThai = kh.TrangThai

            };
            await _dbContext.KhachHangs.AddAsync(khang);
            //await _dbContext.SaveChangesAsync();
            GioHang gh = new GioHang()
            {
                IDKhachHang = khang.IDKhachHang,
                NgayTao = DateTime.Now,
            };
            await _dbContext.GioHangs.AddAsync(gh);
            await _dbContext.SaveChangesAsync();
            return khang;
        }  
        public bool Delete(Guid id)
        {
            try
            {
                var kh = _dbContext.KhachHangs.FirstOrDefault(x => x.IDKhachHang == id);
                if (kh != null)
                {
                    _dbContext.KhachHangs.Remove(kh);
                    _dbContext.SaveChanges();
                    return true;
                }
                return false;
            }
            catch (Exception)
            {

                return false;
            }
        }

        public List<KhachHang> GetAll()
        {
            return _dbContext.KhachHangs.ToList();
        }
        //Nhinh thêm
        public async Task<List<HoaDon>> GetAllHDKH(Guid idkh)
        {
            return await (from hd in _dbContext.HoaDons.AsNoTracking()
                          join lstd in _dbContext.LichSuTichDiems.AsNoTracking() on hd.ID equals lstd.IDHoaDon into lstdGroup
                          from lstd in lstdGroup.DefaultIfEmpty()
                          join kh in _dbContext.KhachHangs.AsNoTracking() on lstd.MaKhachHang equals kh.MaKhachHang into khGroup
                          from kh in khGroup.DefaultIfEmpty()
                          where kh.IDKhachHang == idkh
                          select hd).ToListAsync();
        }
        //Nhinh-end
        public KhachHang GetById(Guid id)
        {
            return _dbContext.KhachHangs.FirstOrDefault(x => x.IDKhachHang == id);

        }
        //Nhinh thêm
        public KhachHang GetBySDT(string sdt)
        {
            return _dbContext.KhachHangs.FirstOrDefault(c=>c.SDT == sdt || c.Email == sdt);
        }

        public bool Update(KhachHang khachHang)
        {
            var kh = _dbContext.KhachHangs.FirstOrDefault(x => x.MaKhachHang == khachHang.MaKhachHang);
            if (kh != null)
            {
                kh.Ten = khachHang.Ten;
                kh.SDT = khachHang.SDT;
                kh.Email = khachHang.Email;
                kh.MaKhachHang = khachHang.MaKhachHang;
                kh.Password = khachHang.Password;
                kh.GioiTinh = khachHang.GioiTinh;
              
                kh.NgaySinh = khachHang.NgaySinh;
                kh.DiemTich = khachHang.DiemTich;
                kh.TrangThai = khachHang.TrangThai;
                _dbContext.KhachHangs.Update(kh);
                _dbContext.SaveChanges();
                return true;
            }
            return false;
        }

        //public bool Update(Guid id, string email, string password)
        //{
        //    var kh= _dbContext.KhachHangs.FirstOrDefault(a=>a.IDKhachHang == id);
        //    if (kh != null)
        //    {
        //        kh.Email = email;
        //        kh.Password = password;
        //        _dbContext.KhachHangs.Update(kh);   
        //        _dbContext.SaveChanges();
        //        return true;
        //    }
        //    return false;
        //}



        //public bool Update(KhachHang nv)
        //{
        //    try
        //    {
        //        var kh = _dbContext.KhachHangs.FirstOrDefault(x => x.IDKhachHang == nv.IDKhachHang);
        //        if (kh != null)
        //        {
        //            _dbContext.KhachHangs.Update(kh);
        //            _dbContext.SaveChanges();
        //            return true;
        //        }
        //        return false;
        //    }
        //    catch (Exception)
        //    {

        //        return false;
        //    }
        //}
    }
}
