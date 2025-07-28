using AppAPI.IServices;
using AppData.IRepositories;
using AppData.Models;
using AppData.Repositories;
using AppData.ViewModels;
using AppData.ViewModels.SanPham;
using Microsoft.EntityFrameworkCore;

namespace AppAPI.Services
{
    public class GioHangServices : IGioHangServices
    {
        private readonly IAllRepository<GioHang> repos;
        AssignmentDBContext context = new AssignmentDBContext();
        private readonly ISanPhamService _iSanPhamService;
        public GioHangServices()
        {
            repos = new AllRepository<GioHang>(context, context.GioHangs);
            _iSanPhamService = new SanPhamService();
        }
        public bool Add(Guid IdKhachHang, DateTime ngaytao)
        {
            GioHang gioHang = new GioHang();
            gioHang.IDKhachHang = IdKhachHang;
            gioHang.NgayTao = ngaytao;
            return repos.Add(gioHang);
        }

        public bool Delete(Guid Id)
        {
            var gioHang = repos.GetAll().FirstOrDefault(x => x.IDKhachHang == Id);
            if (gioHang != null)
            {
                return repos.Delete(gioHang);
            }
            else
            {
                return false;
            }
        }

        public List<GioHang> GetAll()
        {
            return repos.GetAll().ToList();
        }

        public GioHang GetById(Guid Id)
        {
            return repos.GetAll().FirstOrDefault(x => x.IDKhachHang == Id);
        }

        public bool Update(Guid Id, DateTime ngaytao)
        {
            var gioHang = repos.GetAll().FirstOrDefault(x => x.IDKhachHang == Id);
            if (gioHang != null)
            {
                gioHang.NgayTao = ngaytao;
                return repos.Update(gioHang);
            }
            else
            {
                return false;
            }
        }
        public GioHangViewModel GetCart(List<GioHangRequest> request)
        {
            var response = new GioHangViewModel();
            long tongTien = 0;
            ChiTietSanPhamViewModel chiTietSanPham;
            foreach (var item in request)
            {
                chiTietSanPham = _iSanPhamService.GetChiTietSanPhamByID(item.IDChiTietSanPham);
                item.DonGia = (int?)chiTietSanPham.GiaBan; // Explicitly cast 'decimal' to 'int?'
                item.Ten = chiTietSanPham.Ten;
                item.MauSac = chiTietSanPham.MauSac;
                item.KichCo = chiTietSanPham.KichCo;
                item.Anh = chiTietSanPham.Anh;
                item.HetHang = chiTietSanPham.SoLuong < item.SoLuong ? false : chiTietSanPham.TrangThai < 1 ? false : true;
                tongTien += item.DonGia.Value * item.SoLuong;
            }
            response.GioHangs = request;
            response.TongTien = tongTien;
            return response;
        }
        public GioHangViewModel GetCartLogin(string idNguoiDung)
        {
            var lstChiTietGioHang = context.ChiTietGioHangs.Where(x => x.IDKhachHang == new Guid(idNguoiDung)).ToList();
            ChiTietSanPhamViewModel chiTietSanPham;
            List<GioHangRequest> lst = new List<GioHangRequest>();
            var response = new GioHangViewModel();
            long tongTien = 0;
            foreach (var item in lstChiTietGioHang)
            {
                chiTietSanPham = _iSanPhamService.GetChiTietSanPhamByID(item.IDCTSP);
                lst.Add(new GioHangRequest()
                {
                    IDChiTietSanPham = chiTietSanPham.ID,
                    SoLuong = item.SoLuong,
                    DonGia = (int?)chiTietSanPham.GiaBan, // Explicit cast from decimal to int?
                    Ten = chiTietSanPham.Ten,
                    MauSac = chiTietSanPham.MauSac,
                    KichCo = chiTietSanPham.KichCo,
                    Anh = chiTietSanPham.Anh,
                    HetHang = chiTietSanPham.SoLuong < item.SoLuong ? false : true
                });
                tongTien += (long)((int?)chiTietSanPham.GiaBan * item.SoLuong); // Cast to int? then to long
            }
            response.GioHangs = lst;
            response.TongTien = tongTien;
            return response;
        }
        public async Task<bool> AddCart(ChiTietGioHang chiTietGioHang)
        {
            try
            {
                
                var existingItem = await context.ChiTietGioHangs.FirstOrDefaultAsync(x =>
                    x.IDKhachHang == chiTietGioHang.IDKhachHang &&
                    x.IDCTSP == chiTietGioHang.IDCTSP);

                if (existingItem != null)
                {
                   
                    existingItem.SoLuong += chiTietGioHang.SoLuong;
                    
                }
                else
                {
                    
                    context.ChiTietGioHangs.Add(chiTietGioHang);
                }

                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception e)
            {
                
                Console.WriteLine(e.ToString()); 
                                               
                return false;
            }
        }
        public async Task<bool> DeleteCart(Guid idNguoiDung)
        {
            try
            {
                var lstChiTietGioHang = context.ChiTietGioHangs.Where(x => x.IDKhachHang == idNguoiDung).ToList();
                context.ChiTietGioHangs.RemoveRange(lstChiTietGioHang);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public async Task<bool> DeleteCartbyIDCTSP(Guid idctsp, Guid idNguoiDung)
        {
            try
            {
                var lstChiTietGioHang = context.ChiTietGioHangs.Where(x => x.IDKhachHang == idNguoiDung && x.IDCTSP == idctsp);
                context.ChiTietGioHangs.RemoveRange(lstChiTietGioHang);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public async Task<bool> UpdateCart(Guid idctsp, int soluong, Guid idNguoiDung)
        {
            try
            {
                ChiTietGioHang lstChiTietGioHang = context.ChiTietGioHangs.First(x => x.IDKhachHang == idNguoiDung && x.IDCTSP == idctsp);
                lstChiTietGioHang.SoLuong = soluong;
                context.ChiTietGioHangs.Update(lstChiTietGioHang);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}
