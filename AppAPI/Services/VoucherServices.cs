using AppAPI.IServices;
using AppData.IRepositories;
using AppData.Models;
using AppData.Repositories;
using AppData.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AppAPI.Services
{
    public class VoucherServices : IVoucherServices
    {
        private readonly IAllRepository<Voucher> _voucherRepo;
        AssignmentDBContext context= new AssignmentDBContext();
        public VoucherServices()
        {
            _voucherRepo = new AllRepository<Voucher>(context,context.Vouchers);
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

            voucherview.Id=Guid.NewGuid();
            var voucher= new Voucher();
            voucher.ID=voucherview.Id;
            voucher.Ten=voucherview.Ten?.Trim();
            voucher.MaVoucher= normalizedCode;
            voucher.HinhThucGiamGia=voucherview.HinhThucGiamGia;
            voucher.GiaTriToiThieu = voucherview.GiaTriToiThieu;
            voucher.GiaTriToiDa = voucherview.GiaTriToiDa;
            voucher.GiaTri = voucherview.GiaTri;
            voucher.NgayApDung=voucherview.NgayApDung;
            voucher.NgayKetThuc=voucherview.NgayKetThuc;
            if (voucher.NgayApDung > voucher.NgayKetThuc)
            {
                return false;
            }
            voucher.SoLuong=voucherview.SoLuong;
            voucher.MoTa = voucherview.MoTa?.Trim();
            voucher.TrangThai=voucherview.TrangThai;
            voucher.IsPublic = voucherview.IsPublic;
            return _allRepository.Add(voucher);
        }

        public bool Delete(Guid Id)
        {
            var voucher = _voucherRepo.GetAll().FirstOrDefault(x => x.ID == Id);
            if (voucher != null)
            {
               
                return _voucherRepo.Delete(voucher);
            }
            else
            {
                return false;
            }
        }

        public List<Voucher> GetAll()
        {
            return _voucherRepo.GetAll();
        }

        public Voucher GetById(Guid Id)
        {
            return _voucherRepo.GetAll().FirstOrDefault(x => x.ID == Id);
        }

        public bool Update(Guid id,VoucherView voucherview)
        {
            var voucher = _allRepository.GetAll().FirstOrDefault(x => x.ID == id);
            if (voucher != null)
            {
                voucher.Ten = voucherview.Ten?.Trim();
                voucher.MaVoucher = voucherview.MaVoucher?.Trim();
                voucher.HinhThucGiamGia = voucherview.HinhThucGiamGia;
                voucher.GiaTriToiThieu = voucherview.GiaTriToiThieu;
                voucher.GiaTriToiDa = voucherview.GiaTriToiDa;
                voucher.GiaTri = voucherview.GiaTri;

                if (voucherview.NgayApDung > voucherview.NgayKetThuc)
                {
                    return false;
                }
                voucher.NgayApDung = voucherview.NgayApDung;
                voucher.NgayKetThuc = voucherview.NgayKetThuc;
                voucher.SoLuong = voucherview.SoLuong;
                voucher.MoTa = voucherview.MoTa?.Trim();
                voucher.TrangThai = voucherview.TrangThai;
                voucher.IsPublic = voucherview.IsPublic;

                return _allRepository.Update(voucher);
            }
            return false;
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

        public Voucher ApplyVoucher(string code, int totalAmount)
        {
            var voucher = _voucherRepo.GetAll().FirstOrDefault(v => v.Ten == code);
            if (voucher == null) return null;

            if (voucher.SoLuong > 0 &&
                totalAmount >= voucher.GiaTriToiThieu &&
                totalAmount <= voucher.GiaTriToiDa &&
                voucher.NgayApDung <= DateTime.Now &&
                voucher.NgayKetThuc >= DateTime.Now)
            {
                voucher.SoLuong--;
                _voucherRepo.Update(voucher);
                return voucher;
            }

            return null;
        }
    }
}
