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
                // Lấy hóa đơn để biết IDKhachHang
                var hoaDon = await _context.HoaDons
                    .FirstOrDefaultAsync(h => h.ID == request.IdHoaDon);
                if (hoaDon == null) return false;

                // Tìm chi tiết hóa đơn đã tồn tại
                var hdct = await _context.ChiTietHoaDons
                    .FirstOrDefaultAsync(c => c.IDHoaDon == request.IdHoaDon
                                           && c.IDCTSP == request.IdChiTietSanPham);

                // Check tồn kho
                var ctsp = await _context.ChiTietSanPhams.FindAsync(request.IdChiTietSanPham);
                if (ctsp == null || ctsp.SoLuong < request.SoLuong)
                    return false;

                if (hdct == null)
                {
                    // Tạo mới ChiTietHoaDon
                    var newHDCT = new ChiTietHoaDon
                    {
                        ID = Guid.NewGuid(),
                        IDHoaDon = request.IdHoaDon,
                        IDCTSP = request.IdChiTietSanPham,
                        DonGia = request.DonGia,
                        SoLuong = request.SoLuong,
                        TrangThai = 0,
                    };

                    // Tạo mới DanhGia, gắn FK về HDCT + lấy IDKhachHang từ HoaDon
                    var danhgia = new DanhGia
                    {
                        ID = Guid.NewGuid(),
                        IDChiTietHoaDon = newHDCT.ID,
                        IDKhachHang = hoaDon.IDKhachHang, // Lấy từ hóa đơn
                        TrangThai = 0
                    };

                    await _context.AddRangeAsync(newHDCT, danhgia);
                }
                else
                {
                    // Nếu đã có thì chỉ cộng số lượng
                    hdct.SoLuong += request.SoLuong;
                    _context.ChiTietHoaDons.Update(hdct);
                }

                // Trừ kho
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
                var exist = await _context.ChiTietHoaDons.FindAsync(id);
                if (exist == null) throw new Exception($"Không tìm thấy CTHD: {id}");

                // Tăng lại số lượng cho sản phẩm
                var ctsp = await _context.ChiTietSanPhams.FindAsync(exist.IDCTSP);
                if (ctsp != null)
                {
                    ctsp.SoLuong += exist.SoLuong;
                    _context.ChiTietSanPhams.Update(ctsp);
                }

                // Xoá đánh giá (nếu có)
                var danhgia = await _context.DanhGias
                    .FirstOrDefaultAsync(c => c.IDChiTietHoaDon == id);

                if (danhgia != null)
                {
                    _context.DanhGias.Remove(danhgia);
                }

                // Xoá chi tiết hoá đơn
                _context.ChiTietHoaDons.Remove(exist);

                // Chỉ SaveChanges 1 lần
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                // Có thể log ex.Message để debug
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
