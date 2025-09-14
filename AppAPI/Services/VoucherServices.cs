using AppAPI.IServices;
using AppData.IRepositories;
using AppData.Models;
using AppData.Repositories;
using AppData.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace AppAPI.Services
{
    public class VoucherServices : IVoucherServices
    {
        private readonly IAllRepository<Voucher> _allRepository;
        AssignmentDBContext context = new AssignmentDBContext();
        public VoucherServices()
        {
            _allRepository = new AllRepository<Voucher>(context, context.Vouchers);
        }
        public bool Add(VoucherView voucherview)
        {
            // Reject duplicate voucher code (MaVoucher) case-insensitive
            var normalizedCode = voucherview.MaVoucher?.Trim();
            if (!string.IsNullOrWhiteSpace(normalizedCode))
            {
                var exists = context.Vouchers.Any(v => v.MaVoucher != null && v.MaVoucher.ToLower() == normalizedCode.ToLower());
                if (exists)
                {
                    return false;
                }
            }

            voucherview.Id = Guid.NewGuid();
            var voucher = new Voucher();
            voucher.ID = voucherview.Id;
            voucher.Ten = voucherview.Ten?.Trim();
            voucher.MaVoucher = normalizedCode;
            voucher.HinhThucGiamGia = voucherview.HinhThucGiamGia;
            voucher.GiaTriToiThieu = voucherview.GiaTriToiThieu;
            voucher.GiaTriToiDa = voucherview.GiaTriToiDa;
            voucher.GiaTri = voucherview.GiaTri;
            voucher.NgayApDung = voucherview.NgayApDung;
            voucher.NgayKetThuc = voucherview.NgayKetThuc;
            if (voucher.NgayApDung > voucher.NgayKetThuc)
            {
                return false;
            }
            voucher.SoLuong = voucherview.SoLuong;
            voucher.MoTa = voucherview.MoTa?.Trim();
            voucher.TrangThai = voucherview.TrangThai;
            voucher.IsPublic = voucherview.IsPublic;
            return _allRepository.Add(voucher);
        }

        public bool Delete(Guid Id)
        {
            var voucher = _allRepository.GetAll().FirstOrDefault(x => x.ID == Id);
            if (voucher != null)
            {

                return _allRepository.Delete(voucher);
            }
            else
            {
                return false;
            }
        }

        public List<Voucher> GetAll()
        {
            return _allRepository.GetAll();
        }

        public Voucher GetById(Guid Id)
        {
            return _allRepository.GetAll().FirstOrDefault(x => x.ID == Id);
        }

        public bool Update(Guid id, VoucherView voucherview)
        {
            try
            {
                var voucher = context.Vouchers.FirstOrDefault(x => x.ID == id);
                if (voucher == null) return false;

                // Map từ ViewModel sang Entity
                voucher.MaVoucher = voucherview.MaVoucher;
                voucher.Ten = voucherview.Ten;
                voucher.MoTa = voucherview.MoTa;
                voucher.GiaTri = voucherview.GiaTri;
                voucher.HinhThucGiamGia = voucherview.HinhThucGiamGia;
                voucher.SoLuong = voucherview.SoLuong;
                voucher.GiaTriToiThieu = voucherview.GiaTriToiThieu;
                voucher.GiaTriToiDa = voucherview.GiaTriToiDa;
                voucher.NgayApDung = voucherview.NgayApDung;
                voucher.NgayKetThuc = voucherview.NgayKetThuc;

                context.Vouchers.Update(voucher);
                context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public Voucher? GetVoucherByMa(string ma)
        {
            if (string.IsNullOrWhiteSpace(ma)) return null;
            return _allRepository.GetAll().FirstOrDefault(x => x.MaVoucher != null && x.MaVoucher.ToUpper() == ma.ToUpper());
        }
        public bool UpdateTrangThai(Guid id, int trangThaiMoi)
        {
            var voucher = context.Vouchers.FirstOrDefault(v => v.ID == id);
            if (voucher == null) return false;

            voucher.TrangThai = trangThaiMoi;
            context.Vouchers.Update(voucher);
            context.SaveChanges();
            return true;
        }

        //public List<Voucher> GetAllVoucherByTien(int tongTien) 
        //{
        //    return _allRepository.GetAll().Where(x=>x.NgayApDung<DateTime.Now && x.NgayKetThuc>DateTime.Now && x.SoTienCan<tongTien && x.TrangThai>0 && x.SoLuong>0).ToList();
        //}
    }
}
