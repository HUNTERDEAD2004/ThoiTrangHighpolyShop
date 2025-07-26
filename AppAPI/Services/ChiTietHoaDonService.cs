using System.Diagnostics;
using AppAPI.IServices;
using AppData.IRepositories;
using AppData.Models;
using AppData.Repositories;
using AppData.ViewModels.BanOffline;
using Microsoft.EntityFrameworkCore;

namespace AppAPI.Services
{
    public class ChiTietHoaDonService : IChiTietHoaDonService
    {
        private readonly AssignmentDBContext _context;
        public ChiTietHoaDonService()
        {
            _context = new AssignmentDBContext();
        }

        public async Task<bool> SaveCTHoaDon(HoaDonChiTietRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var hdctList = await _context.ChiTietHoaDons
                .Where(c => c.IDHoaDon == request.IdHoaDon && c.IDCTSP == request.IdChiTietSanPham)
                .ToListAsync();

                var hdct = hdctList.FirstOrDefault();

                var ctsp = await _context.ChiTietSanPhams.FindAsync(request.IdChiTietSanPham);
                if (ctsp == null || ctsp.SoLuong < request.SoLuong)
                    return false;

                if (hdct == null)
                {
                    var danhgia = new DanhGia
                    {
                        ID = (Guid)request.Id,
                        IDKhachHang = Guid.Parse("e106c66d-f18d-4609-8a38-08e09d68e78c"),
                        TrangThai = 0
                    };

                    var newHDCT = new ChiTietHoaDon
                    {
                        ID = danhgia.ID,
                        IDHoaDon = request.IdHoaDon,
                        IDCTSP = request.IdChiTietSanPham,
                        DonGia = request.DonGia,
                        SoLuong = request.SoLuong,
                        TrangThai = 0,
                    };

                    await _context.AddRangeAsync(danhgia, newHDCT);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    hdct.SoLuong += request.SoLuong;
                    _context.ChiTietHoaDons.Update(hdct);
                }

                ctsp.SoLuong -= request.SoLuong;
                _context.ChiTietSanPhams.Update(ctsp);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<bool> DeleteCTHoaDon(Guid id)
        {
            try
            {
                var exist = _context.ChiTietHoaDons.Find(id);
                if (exist == null) throw new Exception($"Không tìm thấy CTHD: {id}");
                //Tăng lại số lượng cho sp
                var ctsp = await _context.ChiTietSanPhams.FindAsync(exist.IDCTSP);
                ctsp.SoLuong += exist.SoLuong;
                _context.ChiTietSanPhams.Update(ctsp);
                await _context.SaveChangesAsync();
                //Xóa đánh giá 
                var danhgia = await _context.DanhGias.Where(c => c.ID == id).FirstOrDefaultAsync();
                _context.DanhGias.Remove(danhgia);
                await _context.SaveChangesAsync();
                _context.ChiTietHoaDons.Remove(exist);
                await _context.SaveChangesAsync();
                return true;
            }catch(Exception ex)
            {
                return false;
            }
        }

        public List<ChiTietHoaDon> GetAllCTHoaDon()
        {
            return _context.ChiTietHoaDons.ToList();
        }

        public async Task<List<HoaDonChiTietViewModel>> GetHDCTByIdHD(Guid idhd)
        {
            List<HoaDonChiTietViewModel> lsthdct = await (from cthd in _context.ChiTietHoaDons
                                                          join ctsp in _context.ChiTietSanPhams on cthd.IDCTSP equals ctsp.ID
                                                          join ms in _context.MauSacs on ctsp.IDMauSac equals ms.ID
                                                          join kc in _context.KichCos on ctsp.IDKichCo equals kc.ID
                                                          join sp in _context.SanPhams on ctsp.IDSanPham equals sp.ID
                                                          join km in _context.KhuyenMais.Where(c => c.NgayKetThuc > DateTime.Now && c.TrangThai != 2) on ctsp.IDKhuyenMai equals km.ID
                                                          into kmGroup
                                                          from km in kmGroup.DefaultIfEmpty()
                                                          where cthd.IDHoaDon == idhd
                                                          select new HoaDonChiTietViewModel()
                                                          {
                                                              Id = cthd.ID,
                                                              IdHoaDon = cthd.IDHoaDon,
                                                              IdSP = sp.ID,
                                                              Ten = sp.Ten,
                                                              PhanLoai = ms.Ten + " - " + kc.Ten,
                                                              SoLuong = cthd.SoLuong,
                                                              GiaGoc = ctsp.GiaBan,
                                                              GiaKM = km == null ? ctsp.GiaBan :
                                                            (km.TrangThai == 1 ? (int)(ctsp.GiaBan / 100 * (100 - km.GiaTri)) :
                                                            (km.GiaTri < ctsp.GiaBan ? (ctsp.GiaBan - (int)km.GiaTri) : 0)),                                                             
                                                          }).ToListAsync();
            return lsthdct;
        }

        public async  Task<bool> UpdateSL(Guid id, int sl)
        {
            try
            {
                var cthd = _context.ChiTietHoaDons.Find(id);
                var ctsp = _context.ChiTietSanPhams.Find(cthd.IDCTSP);

                var chenhlech = cthd.SoLuong - sl;
                if (chenhlech < 0 && chenhlech * (-1) > ctsp.SoLuong) return false;
                ctsp.SoLuong += chenhlech;
                _context.ChiTietSanPhams.Update(ctsp);
                await _context.SaveChangesAsync();
                cthd.SoLuong = sl;

                _context.ChiTietHoaDons.Update(cthd);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
