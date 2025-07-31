using AppAPI.IServices;
using AppData.IRepositories;
using AppData.Models;
using AppData.Repositories;
using AppData.ViewModels;
using AppData.ViewModels.BanOffline;
using AppData.ViewModels.SanPham;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography.Xml;
namespace AppAPI.Services
{
    public class HoaDonService : IHoaDonService
    {
        private readonly IAllRepository<HoaDon> reposHoaDon;
        private readonly IAllRepository<ChiTietHoaDon> reposChiTietHoaDon;
        private readonly IAllRepository<ChiTietSanPham> repsCTSanPham;
        private readonly IAllRepository<Voucher> reposVoucher;
        private readonly IAllRepository<QuyDoiDiem> reposQuyDoiDiem;
        private readonly IAllRepository<LichSuTichDiem> reposLichSuTichDiem;
        private readonly IAllRepository<KhachHang> reposKhachHang;
        private readonly IAllRepository<SanPham> reposSanPham;
        private readonly IAllRepository<DanhGia> reposDanhGia;
        private readonly IAllRepository<DiaChi> reposDiaChi;                
        AssignmentDBContext context = new AssignmentDBContext();
        private readonly IGioHangServices _iGioHangServices;

        public HoaDonService(AssignmentDBContext context)
        {
            reposHoaDon = new AllRepository<HoaDon>(context, context.HoaDons);
            reposChiTietHoaDon = new AllRepository<ChiTietHoaDon>(context, context.ChiTietHoaDons);
            repsCTSanPham = new AllRepository<ChiTietSanPham>(context, context.ChiTietSanPhams);
            reposVoucher = new AllRepository<Voucher>(context, context.Vouchers);
            reposQuyDoiDiem = new AllRepository<QuyDoiDiem>(context, context.QuyDoiDiems);
            reposLichSuTichDiem = new AllRepository<LichSuTichDiem>(context, context.LichSuTichDiems);
            reposKhachHang = new AllRepository<KhachHang>(context, context.KhachHangs);
            reposSanPham = new AllRepository<SanPham>(context, context.SanPhams);        
            reposDanhGia = new AllRepository<DanhGia>(context, context.DanhGias);         
            reposDiaChi = new AllRepository<DiaChi>(context, context.DiaChis);
             context = new AssignmentDBContext();
            _iGioHangServices = new GioHangServices();
            this.context = context;               
            
        }

        public bool CheckHDHasLSGD(Guid idHoaDon)
        {
            var exist = reposLichSuTichDiem.GetAll().Any(c => c.IDHoaDon == idHoaDon);
            if (exist == true)
            {
                return true;
            }
            return false;
        }

        public int CheckVoucher(string ten, int tongtien)
        {
            var voucher = reposVoucher.GetAll().FirstOrDefault(p => p.Ten == ten);
            if (voucher != null)
            {
                if (tongtien >= voucher.GiaTriToiThieu && tongtien <= voucher.GiaTriToiDa
                    && DateTime.Compare(voucher.NgayApDung, DateTime.Now) <= 0
                    && DateTime.Compare(DateTime.Now, voucher.NgayKetThuc) <= 0
                    && voucher.SoLuong > 0)
                {
                    if (voucher.HinhThucGiamGia == 1)
                    {
                        tongtien -= voucher.GiaTri;
                        return tongtien;
                    }
                    else
                    {
                        tongtien = tongtien - (tongtien * voucher.GiaTri / 100);
                        return tongtien;
                    }
                }
                else
                {
                    return tongtien;
                }
            }
            else
            {
                return tongtien;
            }
        }


      


        private void HandleTichTieuDiem(HoaDonViewModel hoaDon, HoaDon newHoaDon, int subtotal, DonMuaSuccessViewModel result)
        {
            var quyDoi = reposQuyDoiDiem.GetAll().FirstOrDefault(p => p.TrangThai > 0);
            var khachHang = reposKhachHang.GetAll().FirstOrDefault(p => p.IDKhachHang == hoaDon.IDKhachHang);
            if (quyDoi == null || khachHang == null) return;

            int diemTich = quyDoi.TiLeTichDiem != 0 ? subtotal / quyDoi.TiLeTichDiem : 0;

            if (quyDoi.TrangThai == 1)
            {
                if (hoaDon.Diem.GetValueOrDefault() == 0)
                {
                    AddLichSuTichDiem(newHoaDon.ID, khachHang.MaKhachHang, diemTich, 1, quyDoi.ID);
                    result.DiemTich = diemTich;
                }
                else if (khachHang.DiemTich >= hoaDon.Diem)
                {
                    khachHang.DiemTich -= hoaDon.Diem.Value;
                    reposKhachHang.Update(khachHang);
                    AddLichSuTichDiem(newHoaDon.ID, khachHang.MaKhachHang, hoaDon.Diem.Value, 0, quyDoi.ID);
                }
            }
            else if (quyDoi.TrangThai == 2)
            {
                AddLichSuTichDiem(newHoaDon.ID, khachHang.MaKhachHang, diemTich, 1, quyDoi.ID);
                result.DiemTich = diemTich;

                if (khachHang.DiemTich >= hoaDon.Diem.GetValueOrDefault() && hoaDon.Diem != 0)
                {
                    khachHang.DiemTich -= hoaDon.Diem.Value;
                    context.KhachHangs.Update(khachHang);
                    AddLichSuTichDiem(newHoaDon.ID, khachHang.MaKhachHang, hoaDon.Diem.Value, 0, quyDoi.ID);
                }
            }
        }

        private void AddLichSuTichDiem(Guid idHoaDon, string maKhachHang, int diem, int trangThai, Guid idQuyDoi)
        {
            var lichSu = new LichSuTichDiem
            {
                ID = Guid.NewGuid(),
                MaKhachHang = maKhachHang,
                IDHoaDon = idHoaDon,
                Diem = diem,
                TrangThai = trangThai,
                IDQuyDoiDiem = idQuyDoi
            };
           context.LichSuTichDiems.Add(lichSu);
        }


        //Bán hàng tại quầy
        public bool CreateHoaDonOffline(Guid idnhanvien)
        {
            try
            {
                HoaDon hoaDon1 = new HoaDon();
                hoaDon1.ID = Guid.NewGuid();
                hoaDon1.IDNhanVien = idnhanvien;
                hoaDon1.NgayTao = DateTime.Now;
                hoaDon1.TrangThaiGiaoHang = 1;
                hoaDon1.LoaiHoaDon = 1;
                hoaDon1.MaHoaDon = "HD" + (hoaDon1.ID).ToString().Substring(0, 8).ToUpper();
                if (reposHoaDon.Add(hoaDon1))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public DonMuaSuccessViewModel CreateHoaDon(List<ChiTietHoaDonViewModel> chiTietHoaDons, HoaDonViewModel hoaDon)
        {
            var result = new DonMuaSuccessViewModel();

            if (hoaDon == null || chiTietHoaDons == null || !chiTietHoaDons.Any())
                return ErrorResult("Dữ liệu đầu vào không hợp lệ", -100);

            using var transaction = context.Database.BeginTransaction();
            try
            {
                var lichSu = CreateLichSuHoaDon(hoaDon.TrangThai);
                string diaChi = ResolveDiaChi(hoaDon);
                var hoaDonEntity = BuildHoaDonEntity(hoaDon, lichSu.ID, diaChi);
                ApplyVoucherIfAvailable(hoaDon, hoaDonEntity, result);

                context.HoaDons.Add(hoaDonEntity);
                context.SaveChanges();

                int subtotal = 0;
                var gioHangList = ProcessChiTietHoaDon(chiTietHoaDons, hoaDonEntity.ID, ref subtotal, transaction, result,hoaDon);
                result.GioHangs = gioHangList;

                if (hoaDon.IDKhachHang != Guid.Empty)
                {
                    HandleTichTieuDiem(hoaDon, hoaDonEntity, subtotal, result);
                    result.Login = true;
                }

                if (hoaDon.TrangThai && hoaDon.IDKhachHang != Guid.Empty)
                {
                    _iGioHangServices.DeleteCart(hoaDon.IDKhachHang);
                }


                context.SaveChanges();
                transaction.Commit();

                result.ID = hoaDonEntity.ID.ToString();
                result.Ten = hoaDon.Ten;
                result.Email = hoaDon.Email;
                result.SDT = hoaDon.SDT;
                result.TongTien = hoaDon.TongTien;
                result.GhiChu = hoaDon.GhiChu ?? "";
                result.DiaChi = diaChi;
                result.PhuongThucThanhToan = hoaDon.PhuongThucThanhToan;
                result.DiemSuDung = hoaDon.Diem ?? 0;

                return result;
            }
            catch (Exception ex)
            {
                
                transaction.Rollback();
                return ErrorResult("Lỗi hệ thống khi tạo đơn hàng", -999);
            }
        }

        private DonMuaSuccessViewModel ErrorResult(string message, int code)
        {
            return new DonMuaSuccessViewModel
            {
                ErrorMessage = message,
                ErrorCode = code
            };
        }

        private LichSuHoaDon CreateLichSuHoaDon(bool trangThai)
        {
            var lichSu = new LichSuHoaDon
            {
                ID = Guid.NewGuid(),
                NgayLap = DateTime.Now,
                GhiChu = "Tạo đơn hàng",
                TrangThai = trangThai ? 1 : 0
            };
            context.LichSuHoaDons.Add(lichSu);
            context.SaveChanges();
            return lichSu;
        }

        private string ResolveDiaChi(HoaDonViewModel hoaDon)
        {
            if (hoaDon.IDDiaChi.HasValue)
            {
                var diaChiObj = context.DiaChis.FirstOrDefault(d => d.Id == hoaDon.IDDiaChi);
                return diaChiObj?.ToString() ?? hoaDon.DiaChi ?? "";
            }
            return hoaDon.DiaChi ?? "";
        }

        private HoaDon BuildHoaDonEntity(HoaDonViewModel vm, Guid lichSuId, string diaChi)
        {
            return new HoaDon
            {
                ID = Guid.NewGuid(),
               // IDNhanVien = vm.IDNhanVien ?? Guid.Empty,
                IDKhachHang = vm.IDKhachHang  ,
                IDPhuongThucTT = vm.IDPhuongThucTT,
                IDLichSuHD = lichSuId,
                NgayTao = DateTime.Now,
                NgayThanhToan = DateTime.Now,
                NgayNhanHang = vm.NgayNhanHang,
                MaHoaDon = "HD" + Guid.NewGuid().ToString("N")[..8].ToUpper(),
                TenNguoiNhan = vm.Ten,
                Email = vm.Email,
                SDT = vm.SDT,
                DiaChi = diaChi,
                TienShip = vm.TienShip,
                TongTien = vm.TongTien,
                GhiChu = vm.GhiChu ?? "",
                TrangThaiGiaoHang = 2,
                LoaiHoaDon = 0
            };
        }

        private void ApplyVoucherIfAvailable(HoaDonViewModel vm, HoaDon hoaDonEntity, DonMuaSuccessViewModel result)
        {
            if (!string.IsNullOrWhiteSpace(vm.TenVoucher))
            {
                var voucher = reposVoucher.GetAll().FirstOrDefault(v => v.Ten == vm.TenVoucher);
                if (voucher != null)
                {
                    hoaDonEntity.IDVoucher = voucher.ID;
                    voucher.SoLuong--;
                    reposVoucher.Update(voucher);
                    result.MaVoucher = voucher.Ten;
                }
            }
        }

        private List<GioHangRequest> ProcessChiTietHoaDon(List<ChiTietHoaDonViewModel> chiTietList,
     Guid hoaDonId, ref int subtotal,
     IDbContextTransaction transaction,
     DonMuaSuccessViewModel result,
     HoaDonViewModel hoaDon)
        {
            var gioHangList = new List<GioHangRequest>();
            var danhGiaList = new List<DanhGia>();
            var chiTietListToAdd = new List<ChiTietHoaDon>();

            foreach (var item in chiTietList)
            {
                var ctsp = context.ChiTietSanPhams.FirstOrDefault(p => p.ID == item.IDChiTietSanPham);
                if (ctsp == null)
                    throw new Exception($"Không tìm thấy sản phẩm ID {item.IDChiTietSanPham}");

                if (ctsp.SoLuong < item.SoLuong)
                {
                    transaction.Rollback();
                    result.ErrorCode = -2;
                    result.ErrorMessage = $"Sản phẩm {item.IDChiTietSanPham} không đủ hàng.";
                    return new List<GioHangRequest>();
                }

                var chiTietId = Guid.NewGuid();

                danhGiaList.Add(new DanhGia
                {
                    ID = chiTietId,
                    TrangThai = 0,
                    IDKhachHang = hoaDon.IDKhachHang,
                });

                chiTietListToAdd.Add(new ChiTietHoaDon
                {
                    ID = chiTietId,
                    IDHoaDon = hoaDonId,
                    IDCTSP = item.IDChiTietSanPham,
                    SoLuong = item.SoLuong,
                    DonGia = item.DonGia,
                    TrangThai = 1
                });

                subtotal += item.SoLuong * item.DonGia;

                var sp = reposSanPham.GetAll().FirstOrDefault(p => p.ID == ctsp.IDSanPham);
                gioHangList.Add(new GioHangRequest
                {
                    Ten = sp?.Ten,
                    DonGia = item.DonGia,
                    SoLuong = item.SoLuong,
                    KichCo = context.KichCos.FirstOrDefault(k => k.ID == ctsp.IDKichCo)?.Ten,
                    MauSac = context.MauSacs.FirstOrDefault(m => m.ID == ctsp.IDMauSac)?.Ten,
                    Anh = sp?.AnhDaiDien
                });
            }

            
            context.DanhGias.AddRange(danhGiaList);
            context.ChiTietHoaDons.AddRange(chiTietListToAdd);
            context.SaveChanges();

            return gioHangList;
        }



        public bool DeleteHoaDon(Guid id)
        {
            try
            {
                HoaDon hoaDon = reposHoaDon.GetAll().FirstOrDefault(p => p.ID == id);
                var lsthdct = reposChiTietHoaDon.GetAll().Where(c => c.IDHoaDon == hoaDon.ID).ToList();

                var deletedg = context.DanhGias.Where(c => lsthdct.Select(x => x.ID).Contains(c.ID)).ToList();
                foreach (var item in lsthdct)
                {
                    var ctsp = repsCTSanPham.GetAll().FirstOrDefault(c => c.ID == item.IDCTSP);
                    ctsp.SoLuong += item.SoLuong;
                    repsCTSanPham.Update(ctsp);
                }
                //Xóa chiTietHD
                context.ChiTietHoaDons.RemoveRange(lsthdct);
                context.SaveChanges();
                //Xóa đánh giá
                context.DanhGias.RemoveRange(deletedg);
                context.SaveChanges();
                //Xóa hóa đơn
                context.HoaDons.Remove(hoaDon);
                context.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        }

        public bool XacNhanDonHang(Guid idHoaDon)
        {
            var hoaDon = context.HoaDons.Include(h => h.ChiTietHoaDons).FirstOrDefault(h => h.ID == idHoaDon);
            if (hoaDon == null) return false;

            foreach (var chiTiet in hoaDon.ChiTietHoaDons)
            {
                var ctsp = context.ChiTietSanPhams.FirstOrDefault(c => c.ID == chiTiet.IDCTSP);
                if (ctsp == null || ctsp.SoLuong < chiTiet.SoLuong)
                    return false;

                ctsp.SoLuong -= chiTiet.SoLuong;
                context.Update(ctsp);
            }

            hoaDon.TrangThaiGiaoHang = 0; 
            context.Update(hoaDon);
            context.SaveChanges();
            return true;
        }


        public List<ChiTietHoaDon> GetAllChiTietHoaDon(Guid idHoaDon)
        {
            return reposChiTietHoaDon.GetAll().Where(x => x.IDHoaDon == idHoaDon).ToList();
        }

        public List<HoaDon> GetAllHDCho()
        {
            return context.HoaDons.Where(c => c.TrangThaiGiaoHang == 1 || c.TrangThaiGiaoHang == 0).OrderByDescending(c => c.TrangThaiGiaoHang).ToList();
        }
        //Nhinh
        public List<HoaDonQL> GetAllHDQly()
        {
            var result = (from hd in context.HoaDons
                          join lstd in context.LichSuTichDiems on hd.ID equals lstd.IDHoaDon into lstdGroup
                          from lstd in lstdGroup.DefaultIfEmpty()
                          join kh in context.KhachHangs on lstd.MaKhachHang equals kh.MaKhachHang into khGroup
                          from kh in khGroup.DefaultIfEmpty()
                          where hd.TrangThaiGiaoHang != 1 && hd.TrangThaiGiaoHang != 0
                          select new HoaDonQL()
                          {
                              Id = hd.ID,
                              MaHD = hd.MaHoaDon,
                              KhachHang = kh != null ? kh.Ten : "Khách lẻ",
                              SDTKH = kh != null ? kh.SDT : null,
                              SDTnhanhang = hd.SDT != null ? hd.SDT : "null",
                              // PTTT = hd.PhuongThucThanhToan,
                              ThoiGian = hd.NgayTao,
                              //                              GiamGia = (from vc in context.Vouchers
                              //                                         where vc.ID == hd.IDVoucher
                              //                                         select vc.TrangThai == 0 ? vc.GiaTri : context.ChiTietHoaDons.Where(c => c.IDHoaDon == hd.ID).ToList().AsEnumerable().Sum(c => c.DonGia * c.SoLuong) / 100 * vc.GiaTri)
                              //.FirstOrDefault(),
                              //KhachDaTra = (hd.TrangThaiGiaoHang == 6 || hd.PhuongThucThanhToan == "VNPay" && hd.TrangThaiGiaoHang != 7) == true ? hd.TongTien : 0,
                              // TongTienHang = context.ChiTietHoaDons.Where(c => c.IDHoaDon == hd.ID).ToList().AsQueryable().Sum(c => c.DonGia * c.SoLuong),
                              LoaiHD = hd.LoaiHoaDon,
                              TrangThai = hd.TrangThaiGiaoHang,
                          }).Distinct().ToList();

            return result;
        }
        public List<HoaDon> GetAllHoaDon()
        {
            return reposHoaDon.GetAll();
        }
        public ChiTietHoaDonQL GetCTHDByID(Guid idhd)
        {
            try
            {
                var lsthdct = (from cthd in context.ChiTietHoaDons
                               join ctsp in context.ChiTietSanPhams on cthd.IDCTSP equals ctsp.ID
                               join ms in context.MauSacs on ctsp.IDMauSac equals ms.ID
                               join kc in context.KichCos on ctsp.IDKichCo equals kc.ID
                               join sp in context.SanPhams on ctsp.IDSanPham equals sp.ID
                               join km in context.KhuyenMais on ctsp.IDKhuyenMai equals km.ID into kmGroup
                               from km in kmGroup.DefaultIfEmpty()
                               where cthd.IDHoaDon == idhd
                               select new HoaDonChiTietViewModel
                               {
                                   Id = cthd.ID,
                                   IdHoaDon = cthd.IDHoaDon,
                                   IdSP = sp.ID,
                                   Ten = sp.Ten,
                                   MaCTSP = ctsp.MaSPChiTiet,
                                   PhanLoai = ms.Ten + " - " + kc.Ten,
                                   SoLuong = cthd.SoLuong,
                                   GiaGoc = ctsp.GiaBan,
                                   GiaLuu = cthd.DonGia == null ? 0 : cthd.DonGia,
                                   GiaKM = km == null ? ctsp.GiaBan :
                    (km.TrangThai == 1 ? (int)(ctsp.GiaBan / 100 * (100 - km.GiaTri)) :
                    (km.GiaTri < ctsp.GiaBan ? (ctsp.GiaBan - (int)km.GiaTri) : 0)),
                               }).ToList();

                var result = (from hd in context.HoaDons
                              join nv in context.NhanViens on hd.IDNhanVien equals nv.ID
                              into nvGroup
                              from nv in nvGroup.DefaultIfEmpty()
                              join lstd in context.LichSuTichDiems on hd.ID equals lstd.IDHoaDon into lstdGroup
                              from lstd in lstdGroup.DefaultIfEmpty()
                              join kh in context.KhachHangs on lstd.MaKhachHang equals kh.MaKhachHang into khGroup
                              from kh in khGroup.DefaultIfEmpty()
                              where hd.ID == idhd
                              select new ChiTietHoaDonQL
                              {
                                  Id = hd.ID,
                                  MaHD = hd.MaHoaDon,
                                  NgayTao = hd.NgayTao,
                                  NgayThanhToan = hd.NgayThanhToan != null ? hd.NgayThanhToan : null,
                                  NgayNhanHang = hd.NgayNhanHang != null ? hd.NgayNhanHang : null,
                                  //  PTTT = hd.PhuongThucThanhToan,
                                  NhanVien = nv != null ? nv.Ten : null,
                                  LoaiHD = hd.LoaiHoaDon,
                                  KhachHang = kh == null ? "Khách lẻ" : kh.Ten,
                                  NguoiNhan = hd.TenNguoiNhan != null ? hd.TenNguoiNhan : null,
                                  DiaChi = hd.DiaChi != null ? hd.DiaChi : null,
                                  SĐT = hd.SDT != null ? hd.SDT : null,
                                  Email = hd.Email != null ? hd.Email : null,
                                  //  TienShip = hd.TienShip != null ? hd.TienShip : null,
                                  TrangThai = hd.TrangThaiGiaoHang,
                                  //ThueVAT = hd.ThueVAT,
                                  // KhachCanTra = hd.TongTien,
                                  // TienKhachTra = (hd.TrangThaiGiaoHang == 6 || hd.PhuongThucThanhToan == "VNPay" && hd.TrangThaiGiaoHang != 7) ? hd.TongTien : 0,
                                  GhiChu = hd.GhiChu,
                                  TruTieuDiem = (from lstd in context.LichSuTichDiems
                                                 join qdd in context.QuyDoiDiems on lstd.IDQuyDoiDiem equals qdd.ID
                                                 where lstd.IDHoaDon == hd.ID && lstd.TrangThai == 0
                                                 select lstd.Diem * qdd.TiLeTieuDiem).FirstOrDefault(),
                                  voucher = (from vc in context.Vouchers
                                             where vc.ID == hd.IDVoucher
                                             select new Voucher
                                             {
                                                 ID = vc.ID,
                                                 Ten = vc.Ten,
                                                 GiaTri = vc.GiaTri,
                                                 TrangThai = vc.TrangThai,
                                                 HinhThucGiamGia = vc.HinhThucGiamGia,
                                             }).FirstOrDefault(),
                                  listsp = lsthdct,
                                  lstlstd = (from lstd in context.LichSuTichDiems
                                             where lstd.IDHoaDon == hd.ID
                                             select lstd).OrderBy(c => c.TrangThai).ToList()
                              }).FirstOrDefault();

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public HoaDonViewModelBanHang GetHDBanHang(Guid id)
        {
            List<HoaDonChiTietViewModel> lsthdct = (from cthd in context.ChiTietHoaDons
                                                    join ctsp in context.ChiTietSanPhams on cthd.IDCTSP equals ctsp.ID
                                                    join ms in context.MauSacs on ctsp.IDMauSac equals ms.ID
                                                    join kc in context.KichCos on ctsp.IDKichCo equals kc.ID
                                                    join sp in context.SanPhams on ctsp.IDSanPham equals sp.ID
                                                    join km in context.KhuyenMais.Where(c => c.NgayKetThuc > DateTime.Now && c.TrangThai != 2) on ctsp.IDKhuyenMai equals km.ID
                                                    into kmGroup
                                                    from km in kmGroup.DefaultIfEmpty()
                                                    where cthd.IDHoaDon == id
                                                    select new HoaDonChiTietViewModel()
                                                    {
                                                        Id = cthd.ID,
                                                        IdHoaDon = cthd.IDHoaDon,
                                                        IdSP = sp.ID,
                                                        Ten = sp.Ten,
                                                        MaCTSP = ctsp.MaSPChiTiet,
                                                        PhanLoai = ms.Ten + " - " + kc.Ten,
                                                        SoLuong = cthd.SoLuong,
                                                        GiaGoc = (int)ctsp.GiaBan,
                                                        GiaKM = km == null ? (int)ctsp.GiaBan :
                                                        (km.TrangThai == 1 ? (int)(ctsp.GiaBan / 100 * (100 - km.GiaTri)) :
                                                        (km.GiaTri < ctsp.GiaBan ? ((int)ctsp.GiaBan - (int)km.GiaTri) : 0)),

                                                    }).AsEnumerable().Reverse().ToList();
            var result = (from hd in reposHoaDon.GetAll()
                          join lstd in reposLichSuTichDiem.GetAll() on hd.ID equals lstd.IDHoaDon into lstdGroup
                          from lstd in lstdGroup.DefaultIfEmpty()
                          join kh in reposKhachHang.GetAll() on lstd?.MaKhachHang equals kh?.MaKhachHang into khGroup
                          from kh in khGroup.DefaultIfEmpty()
                          where hd.ID == id
                          select new HoaDonViewModelBanHang()
                          {
                              Id = hd.ID,
                              MaHD = hd.MaHoaDon,
                              IdKhachHang = kh?.IDKhachHang,
                              TenKhachHang = kh?.Ten,
                              lstHDCT = lsthdct,
                              GhiChu = hd.GhiChu == null ? "" : hd.GhiChu,
                          }).FirstOrDefault();
            return result;
        }

        public HoaDon GetHoaDonById(Guid idhd)
        {
            return reposHoaDon.GetAll().FirstOrDefault(c => c.ID == idhd);
        }

        public LichSuTichDiem GetLichSuGiaoDichByIdHD(Guid idHoaDon)
        {
            return reposLichSuTichDiem.GetAll().FirstOrDefault(c => c.IDHoaDon == idHoaDon);
        }

        public bool HuyHD(Guid idhd, Guid idnv)
        {
            try
            {
                var hd = context.HoaDons.Where(c => c.ID == idhd).FirstOrDefault();
                //Update hd
                hd.IDNhanVien = idnv;
                hd.TrangThaiGiaoHang = 7;
                //hd.TongTien = 0;
                context.HoaDons.Update(hd);
                context.SaveChanges();

                // Cộng lại số lượng hàng
                var lsthdct = context.ChiTietHoaDons.Where(c => c.IDHoaDon == idhd).ToList();
                foreach (var hdct in lsthdct)
                {
                    var ctsp = context.ChiTietSanPhams.FirstOrDefault(c => c.ID == hdct.IDCTSP);
                    ctsp.SoLuong += hdct.SoLuong;
                    context.ChiTietSanPhams.Update(ctsp);
                    context.SaveChanges();
                }

                // Cộng lại số lượng voucher nếu áp dụng
                if (hd.IDVoucher != null)
                {
                    var vc = context.Vouchers.FirstOrDefault(c => c.ID == hd.IDVoucher);
                    vc.SoLuong += 1;
                    context.Vouchers.Update(vc);
                    context.SaveChanges();
                }
                // Cộng lại tiêu điểm cho khách hàng
                var lstlstd = context.LichSuTichDiems.Where(c => c.IDHoaDon == idhd).ToList();
                if (lstlstd.Count != 0)
                {
                    var tieud = lstlstd.Where(c => c.TrangThai == 0).FirstOrDefault();
                    var tichd = lstlstd.Where(c => c.TrangThai == 1).FirstOrDefault();
                    if (lstlstd.Count == 1)
                    {
                        if (tieud != null)
                        {
                            //Cộng điểm kh
                            var kh = context.KhachHangs.Where(c => c.MaKhachHang == tieud.MaKhachHang).FirstOrDefault();
                            kh.DiemTich += tieud.Diem;
                            context.KhachHangs.Update(kh);
                            context.SaveChanges();
                            //Thêm 1 lịch sử trả lại điểm
                            LichSuTichDiem diemtra = new LichSuTichDiem()
                            {
                                ID = new Guid(),
                                IDHoaDon = hd.ID,
                                MaKhachHang = kh.MaKhachHang,
                                Diem = tieud.Diem,
                                TrangThai = 2,
                                IDQuyDoiDiem = tieud.IDQuyDoiDiem,
                            };
                            context.LichSuTichDiems.Add(diemtra);
                            context.SaveChanges();
                            //tieud.Diem = 0;
                            //context.LichSuTichDiems.Update(tieud);
                            //context.SaveChanges();
                        }
                        else
                        {
                            tichd.Diem = 0;
                            context.LichSuTichDiems.Update(tichd);
                            context.SaveChanges();
                        }
                    }
                    if (lstlstd.Count == 2)
                    {
                        //Cộng điểm khách hàng
                        var kh = context.KhachHangs.Where(c => c.MaKhachHang == tieud.MaKhachHang).FirstOrDefault();
                        kh.DiemTich += tieud.Diem;
                        context.KhachHangs.Update(kh);
                        context.SaveChanges();

                        //Thêm 1 lịch sử trả lại điểm
                        LichSuTichDiem diemtra = new LichSuTichDiem()
                        {
                            ID = new Guid(),
                            IDHoaDon = hd.ID,
                            MaKhachHang = kh.MaKhachHang,
                            Diem = tieud.Diem,
                            TrangThai = 2,
                            IDQuyDoiDiem = tieud.IDQuyDoiDiem,
                        };
                        context.LichSuTichDiems.Add(diemtra);
                        context.SaveChanges();
                        //Xóa tích sửa tiêu = 0
                        //context.LichSuTichDiems.Remove(tieud);
                        //context.SaveChanges();


                        //tichd.Diem = 0;
                        //tichd.TrangThai = 2;
                        context.LichSuTichDiems.Remove(tichd);
                        context.SaveChanges();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        //Copy
        public async Task<bool> CopyHD(Guid idhd, Guid idnv)
        {
            try
            {
                var hd = context.HoaDons.Where(c => c.ID == idhd).FirstOrDefault();
                //hóa đơn chi tiết
                var lsthdct = context.ChiTietHoaDons.Where(c => c.IDHoaDon == idhd).ToList();
                // lịch sử tích điểm
                var lstlstd = context.LichSuTichDiems.Where(c => c.IDHoaDon == idhd).ToList();

                // Tạo hóa đơn mới có sản phẩm y hệt hóa đơn sao chép
                HoaDon hoaDon = new HoaDon();
                hoaDon.ID = Guid.NewGuid();
                hoaDon.NgayTao = DateTime.Now;
                hoaDon.TenNguoiNhan = hd.TenNguoiNhan;
                hoaDon.SDT = hd.SDT;
                hoaDon.DiaChi = hd.DiaChi;
                hoaDon.Email = hd.DiaChi;
                hoaDon.GhiChu = "Copy " + hd.MaHoaDon;
                hoaDon.IDNhanVien = idnv;
                hoaDon.TrangThaiGiaoHang = 0;
                hoaDon.LoaiHoaDon = hd.LoaiHoaDon;
                hoaDon.MaHoaDon = "HD" + (hoaDon.ID).ToString().Substring(0, 8).ToUpper();
                context.HoaDons.Add(hoaDon);
                context.SaveChanges();

                // Tạo chi tiết hóa đơn mới
                foreach (var item in lsthdct)
                {
                    var ctsp = context.ChiTietSanPhams.FirstOrDefault(c => c.ID == item.IDCTSP);
                    if (ctsp.SoLuong > item.SoLuong)
                    {
                        var danhgia = new DanhGia()
                        {
                            ID = new Guid(),
                            TrangThai = 0,
                        };
                        await context.DanhGias.AddAsync(danhgia);
                        await context.SaveChangesAsync();

                        ChiTietHoaDon ct = new ChiTietHoaDon()
                        {
                            ID = danhgia.ID,
                            SoLuong = item.SoLuong,
                            TrangThai = 0,
                            IDCTSP = item.IDCTSP,
                            IDHoaDon = hoaDon.ID
                        };
                        await context.ChiTietHoaDons.AddAsync(ct);
                        await context.SaveChangesAsync();

                        // Trừ số lượng
                        ctsp.SoLuong -= item.SoLuong;
                        context.ChiTietSanPhams.Update(ctsp);
                        await context.SaveChangesAsync();
                    }
                }
                // Nếu có khách hàng -> Tạo lịch sử ms
                var qqd = context.QuyDoiDiems.FirstOrDefault(c => c.TrangThai != 0);
                if (lstlstd.Count != 0)
                {
                    LichSuTichDiem lstd = new LichSuTichDiem()
                    {
                        ID = new Guid(),
                        Diem = 0,
                        TrangThai = 1,
                        MaKhachHang = lstlstd.FirstOrDefault(c => c.IDHoaDon == hd.ID).MaKhachHang,
                        IDHoaDon = hoaDon.ID,
                        IDQuyDoiDiem = qqd.ID,
                    };
                    await context.LichSuTichDiems.AddAsync(lstd);
                    await context.SaveChangesAsync();
                }
                ;
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public List<HoaDon> LichSuGiaoDich(string makhachhang)
        {
            var idhoadon = reposLichSuTichDiem.GetAll().Where(p => p.MaKhachHang == makhachhang).ToList();
            List<HoaDon> lichSuGiaoDich = new List<HoaDon>();
            foreach (var item in idhoadon)
            {
                lichSuGiaoDich.Add(reposHoaDon.GetAll().FirstOrDefault(p => p.ID == item.IDHoaDon));
            }
            return lichSuGiaoDich;
        }

        public List<HoaDon> TimKiemVaLocHoaDon(string ten, int? loc)
        {
            List<HoaDon> timkiem = reposHoaDon.GetAll().Where(p => p.TenNguoiNhan.ToLower().Contains(ten.ToLower())).ToList();
            if (loc == 0)
            {
                List<HoaDon> locTangDan = timkiem.OrderBy(p => p.NgayTao).ToList();
                return locTangDan;
            }
            else if (loc == 1)
            {
                List<HoaDon> locGiamDan = timkiem.OrderByDescending(p => p.NgayTao).ToList();
                return locGiamDan;
            }
            return timkiem;
        }

        public bool UpdateGhiChuHD(Guid idhd, Guid idnv, string ghichu)
        {
            try
            {
                var hd = reposHoaDon.GetAll().FirstOrDefault(c => c.ID == idhd);
                if (ghichu == "null")
                {
                    hd.GhiChu = null;
                    hd.IDNhanVien = idnv;
                }
                else
                {
                    hd.GhiChu = ghichu;
                }
                reposHoaDon.Update(hd);
                return true;

            }
            catch (Exception ex)
            {
                return false;
            }
        }
        // Check khách hàng đã dùng voucher này chưa
        public bool CheckCusUseVoucher(Guid idkh, Guid idvoucher)
        {
            var hdkh = (from hd in context.HoaDons
                        join lstd in context.LichSuTichDiems on hd.ID equals lstd.IDHoaDon
                        join kh in context.KhachHangs on lstd.MaKhachHang equals kh.MaKhachHang
                        where kh.IDKhachHang == idkh
                        select hd).ToList();
            if (hdkh == null)
            {
                return false;
            }
            return (hdkh.Any(c => c.IDVoucher == idvoucher) ? true : false);
        }
        public bool UpdateHoaDon(HoaDonThanhToanRequest hoaDon)
        {
            var update = reposHoaDon.GetAll().FirstOrDefault(p => p.ID == hoaDon.Id);

            //Lưu tiền vào hóa đơn chi tiết
            var lsthdct = context.ChiTietHoaDons.Where(c => c.IDHoaDon == hoaDon.Id).ToList();
            //Xóa hóa đơn chi tiết có số lượng = 0
            var delete = lsthdct.Where(c => c.SoLuong == 0).ToList();
            context.ChiTietHoaDons.RemoveRange(delete);
            context.SaveChanges();

            lsthdct = context.ChiTietHoaDons.Where(c => c.IDHoaDon == hoaDon.Id).ToList();
            foreach (var item in lsthdct)
            {
                var result = (from ctsp in context.ChiTietSanPhams
                              join km in context.KhuyenMais
                                  .Where(c => c.NgayKetThuc > DateTime.Now && c.TrangThai != 2)
                                  on ctsp.IDKhuyenMai equals km.ID into kmGroup
                              from km in kmGroup.DefaultIfEmpty()
                              where ctsp.ID == item.IDCTSP
                              select km != null ? (km.TrangThai == 0 ? (km.GiaTri < ctsp.GiaBan ? (ctsp.GiaBan - km.GiaTri) : 0) : (ctsp.GiaBan * (100 - km.GiaTri) / 100)) : ctsp.GiaBan)
                             .FirstOrDefault();
                item.DonGia = result;
                context.ChiTietHoaDons.Update(item);
                context.SaveChanges();
            }
            //Update LSTD tích (ban đầu chỉ có duy nhất 1
            var lstd = context.LichSuTichDiems.FirstOrDefault(c => c.IDHoaDon == hoaDon.Id);
            if (lstd != null)
            {
                //Lấy id quy đổi điểm dùng hiện tại
                var qdiem = context.QuyDoiDiems.FirstOrDefault(c => c.TrangThai != 0);
                if (qdiem.TrangThai == 1) // Chỉ tích hoặc tiêu
                {
                    if (hoaDon.DiemTichHD >= hoaDon.DiemSD) // Trường hợp tích = tiêu = 0
                    {
                        lstd.Diem = hoaDon.DiemTichHD;
                        context.LichSuTichDiems.Update(lstd);
                        context.SaveChanges();
                    }
                    else if (hoaDon.DiemSD > hoaDon.DiemTichHD) // Trường hợp tiêu nhiều hơn tích
                    {
                        lstd.Diem = hoaDon.DiemSD;
                        lstd.TrangThai = 0;
                        context.LichSuTichDiems.Update(lstd);
                        context.SaveChanges();
                    }
                }
                else if (qdiem.TrangThai == 2) // Vừa tích vừa tiêu
                {
                    if (hoaDon.DiemTichHD != 0)
                    {
                        lstd.Diem = hoaDon.DiemTichHD;
                        context.LichSuTichDiems.Update(lstd);
                        context.SaveChanges();
                    }
                    if (hoaDon.DiemSD != 0)
                    {
                        // Tạo lstieudiem
                        LichSuTichDiem lstieu = new LichSuTichDiem()
                        {
                            ID = new Guid(),
                            TrangThai = 0,
                            IDHoaDon = hoaDon.Id,
                            MaKhachHang = lstd.MaKhachHang,
                            Diem = hoaDon.DiemSD,
                            IDQuyDoiDiem = qdiem.ID,
                        };
                        context.LichSuTichDiems.Add(lstieu);
                        context.SaveChanges();
                    }
                }
                // Thêm điểm cho Khách hàng và trừ
                var kh = reposKhachHang.GetAll().FirstOrDefault(c => c.MaKhachHang == lstd.MaKhachHang);
                kh.DiemTich += hoaDon.DiemTichHD;
                kh.DiemTich -= hoaDon.DiemSD;
                reposKhachHang.Update(kh);
            }
            // Trừ số lượng voucher nếu có
            var vc = context.Vouchers.Find(hoaDon.IdVoucher);
            if (vc != null)
            {
                vc.SoLuong -= 1;
                context.Vouchers.Update(vc);
                context.SaveChanges();
            }
            // UpdateHD
            update.IDNhanVien = hoaDon.IdNhanVien;
            update.NgayThanhToan = hoaDon.NgayThanhToan;
            update.TrangThaiGiaoHang = hoaDon.TrangThai;
            update.TongTien = hoaDon.TongTien;
            // update.PhuongThucThanhToan = hoaDon.PTTT;
            update.IDVoucher = hoaDon.IdVoucher == Guid.Empty ? null : hoaDon.IdVoucher;
            return reposHoaDon.Update(update);
        }

        public bool UpdateTrangThaiGiaoHang(Guid idHoaDon, int trangThai, Guid? idNhanVien)
        {
            var update = reposHoaDon.GetAll().FirstOrDefault(p => p.ID == idHoaDon);
            List<ChiTietHoaDon> chitiethoadon = reposChiTietHoaDon.GetAll().Where(p => p.IDHoaDon == idHoaDon).ToList();
            if (update != null)
            {
                if (trangThai == 10)
                {
                    foreach (var item in chitiethoadon)
                    {
                        var ctSanPham = repsCTSanPham.GetAll().FirstOrDefault(p => p.ID == item.IDCTSP);
                        ctSanPham.SoLuong -= item.SoLuong;
                        repsCTSanPham.Update(ctSanPham);
                    }
                }
                if (trangThai == 5)
                {
                    foreach (var item in chitiethoadon)
                    {
                        var CTsanPham = repsCTSanPham.GetAll().FirstOrDefault(p => p.ID == item.IDCTSP);
                        CTsanPham.SoLuong += item.SoLuong;
                        repsCTSanPham.Update(CTsanPham);
                    }
                }
                if (trangThai == 6)
                {
                    var lstlstd = context.LichSuTichDiems.Where(c => c.IDHoaDon == idHoaDon).ToList();
                    if (lstlstd.Count != 0)
                    {
                        var td = lstlstd.Where(c => c.TrangThai == 1).FirstOrDefault();
                        if (td != null)
                        {
                            var kh = context.KhachHangs.Where(c => c.MaKhachHang == td.MaKhachHang).FirstOrDefault();
                            kh.DiemTich += td.Diem;
                            context.KhachHangs.Update(kh);
                            context.SaveChanges();
                        }
                    }
                    update.NgayThanhToan = update.NgayThanhToan == null ? DateTime.Now : update.NgayThanhToan;
                    update.NgayNhanHang = update.NgayNhanHang == null ? DateTime.Now : update.NgayNhanHang;
                }
                update.TrangThaiGiaoHang = trangThai;
                // update.IDNhanVien = idNhanVien;
                reposHoaDon.Update(update);
                return true;
            }
            else
            {
                return false;
            }
        }
        //Giao thành công
        public bool ThanhCong(Guid idhd, Guid idnv) // Chỉ cho đơn online
        {
            try
            {
                var hd = context.HoaDons.FirstOrDefault(c => c.ID == idhd);
                hd.TrangThaiGiaoHang = 6;
                hd.IDNhanVien = idnv;
                hd.NgayNhanHang = DateTime.Now;
                hd.NgayThanhToan = DateTime.Now;
                context.HoaDons.Update(hd);
                context.SaveChanges();
                //Cộng tích điểm cho khách
                var lstlstd = context.LichSuTichDiems.Where(c => c.IDHoaDon == idhd).ToList();
                if (lstlstd.Count != 0)
                {
                    var td = lstlstd.Where(c => c.TrangThai == 1).FirstOrDefault();
                    if (td != null)
                    {
                        //Trừ điểm kh
                        var kh = context.KhachHangs.Where(c => c.MaKhachHang == td.MaKhachHang).FirstOrDefault();
                        kh.DiemTich += td.Diem;
                        context.KhachHangs.Update(kh);
                        context.SaveChanges();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        // Hoàn hàng
        public bool HoanHang(Guid idhd, Guid idnv)
        {
            try
            {
                var hd = context.HoaDons.FirstOrDefault(c => c.ID == idhd);
                hd.TrangThaiGiaoHang = 4;
                hd.IDNhanVien = idnv;

                //Trừ đi điểm tích của khách hàng nếu có
                var lstlstd = context.LichSuTichDiems.Where(c => c.IDHoaDon == idhd).ToList();
                if (lstlstd.Count != 0)
                {
                    var td = lstlstd.Where(c => c.TrangThai == 1).FirstOrDefault();
                    if (td != null)
                    {
                        //Trừ điểm kh
                        var kh = context.KhachHangs.Where(c => c.MaKhachHang == td.MaKhachHang).FirstOrDefault();
                        kh.DiemTich -= td.Diem;
                        context.KhachHangs.Update(kh);
                        context.SaveChanges();
                        // Tạo 1 lịch sử trừ điểm 
                        LichSuTichDiem diemtru = new LichSuTichDiem()
                        {
                            ID = new Guid(),
                            IDHoaDon = hd.ID,
                            MaKhachHang = kh.MaKhachHang,
                            Diem = td.Diem,
                            TrangThai = 3,
                            IDQuyDoiDiem = td.IDQuyDoiDiem,
                        };
                        context.LichSuTichDiems.Add(diemtru);
                        context.SaveChanges();
                        //Cập nhật tích điểm
                        //td.Diem = 0;
                        //context.LichSuTichDiems.Update(td);
                        //context.SaveChanges();
                    }
                }
                context.HoaDons.Update(hd);
                context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        // Hoàn hàng thành công
        public bool HoanHangThanhCong(Guid idhd, Guid idnv)
        {
            try
            {
                var hd = context.HoaDons.FirstOrDefault(c => c.ID == idhd);
                hd.TrangThaiGiaoHang = 5;
                hd.IDNhanVien = idnv;
                hd.TongTien = 0;
                context.HoaDons.Update(hd);
                context.SaveChanges();

                // Cộng lại số lượng hàng
                var lsthdct = context.ChiTietHoaDons.Where(c => c.IDHoaDon == idhd).ToList();
                foreach (var hdct in lsthdct)
                {
                    var ctsp = context.ChiTietSanPhams.FirstOrDefault(c => c.ID == hdct.IDCTSP);
                    ctsp.SoLuong += hdct.SoLuong;
                    context.ChiTietSanPhams.Update(ctsp);
                    context.SaveChanges();
                }

                // Cộng lại số lượng voucher nếu áp dụng
                if (hd.IDVoucher != null)
                {
                    var vc = context.Vouchers.FirstOrDefault(c => c.ID == hd.IDVoucher);
                    vc.SoLuong += 1;
                    context.Vouchers.Update(vc);
                    context.SaveChanges();
                }
                // Cộng lại điểm khách hàng dùng cho hóa đơn
                var lstlstd = context.LichSuTichDiems.Where(c => c.IDHoaDon == idhd).ToList();
                if (lstlstd.Count != 0)
                {
                    var tieud = lstlstd.Where(c => c.TrangThai == 0).FirstOrDefault();
                    if (tieud != null)
                    {
                        //Cộng điểm kh
                        var kh = context.KhachHangs.Where(c => c.MaKhachHang == tieud.MaKhachHang).FirstOrDefault();
                        kh.DiemTich += tieud.Diem;
                        context.KhachHangs.Update(kh);
                        context.SaveChanges();
                        //Thêm 1 lịch sử trả lại điểm cho đơn hoàn thành công
                        LichSuTichDiem diemtra = new LichSuTichDiem()
                        {
                            ID = new Guid(),
                            IDHoaDon = hd.ID,
                            MaKhachHang = kh.MaKhachHang,
                            Diem = tieud.Diem,
                            TrangThai = 4,
                            IDQuyDoiDiem = tieud.IDQuyDoiDiem,
                        };
                        context.LichSuTichDiems.Add(diemtra);
                        context.SaveChanges();
                   
                    }

                }
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public List<HoaDon> LichSuGiaoDich(Guid idNguoiDung)
        {
            throw new NotImplementedException();
        }
    }
}
