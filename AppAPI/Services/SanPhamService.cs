using System.Drawing;
using System.Linq;
using AppAPI.IServices;
using AppData.Models;
using AppData.ViewModels;
using AppData.ViewModels.BanOffline;
using AppData.ViewModels.SanPham;
using Microsoft.EntityFrameworkCore;

namespace AppAPI.Services
{
    public class SanPhamService : ISanPhamService
    {
        private readonly AssignmentDBContext _context;
        public SanPhamService()
        {
            _context = new AssignmentDBContext();
        }

        #region SanPham
        public async Task<bool> UpdateSanPham(SanPhamUpdateRequest request)
        {
            var existing = await _context.SanPhams.FirstOrDefaultAsync(x => x.ID == request.ID);
            if (existing == null) return false;

            existing.Ten = request.Ten?.Trim();
            existing.AnhDaiDien = request.AnhDaiDien;
            existing.MoTa = request.MoTa?.Trim();
            existing.IDChatLieu = request.IDChatLieu;
            existing.IDLoaiSP = request.IDLoaiSP; // Gán trực tiếp loại sản phẩm

            await _context.SaveChangesAsync();
            return true;
        }
        public Task<List<SanPhamViewModel>> TimKiemSanPham(SanPhamTimKiemNangCao sp)
        {
            throw new NotImplementedException();
        }
        public Task<List<SanPhamViewModel>> GetSanPhamByIdDanhMuc(Guid idloaisp)
        {
            throw new NotImplementedException();
        }
        public async Task<SanPhamUpdateRequest> GetSanPhamById(Guid id)
        {
            try
            {
                var sanPham = await _context.SanPhams.FirstAsync(x => x.ID == id);

                return new SanPhamUpdateRequest()
                {
                    ID = sanPham.ID,
                    Ten = sanPham.Ten,
                    MoTa = sanPham.MoTa,
                    AnhDaiDien = sanPham.AnhDaiDien,
                    IDChatLieu = sanPham.IDChatLieu,
                    IDLoaiSP = sanPham.IDLoaiSP // chỉ cần lấy loại hiện tại
                };
            }
            catch
            {
                return new SanPhamUpdateRequest();
            }
        }
        public List<SanPhamViewModelAdmin> GetAllSanPhamAdmin()
        {
            try
            {
                var now = DateTime.Now;

                var khuyenMais = _context.KhuyenMais
                    .Where(x => x.NgayKetThuc > now && x.NgayApDung < now)
                    .ToList();

                var lstSanPham = (from sp in _context.SanPhams
                                  join loai in _context.LoaiSPs on sp.IDLoaiSP equals loai.ID
                                  join cl in _context.ChatLieus on sp.IDChatLieu equals cl.ID into chatLieuJoin
                                  from cl in chatLieuJoin.DefaultIfEmpty()
                                  select new
                                  {
                                      SanPham = sp,
                                      LoaiSP = loai,
                                      ChatLieu = cl,
                                      ChiTietMoiNhat = _context.ChiTietSanPhams
                                          .Where(x => x.IDSanPham == sp.ID)
                                          .OrderByDescending(x => x.NgayTao)
                                          .FirstOrDefault(),
                                      TongSoLuong = _context.ChiTietSanPhams
                                          .Where(x => x.IDSanPham == sp.ID)
                                          .Sum(x => (int?)x.SoLuong) ?? 0
                                  }).ToList()
                    .Select(x => new SanPhamViewModelAdmin
                    {
                        ID = x.SanPham.ID,
                        Ten = x.SanPham.Ten,
                        Ma = x.SanPham.Ma,
                        TrangThai = x.SanPham.TrangThai,
                        LoaiSP = x.LoaiSP.Ten,
                        ChatLieu = x.ChatLieu != null ? x.ChatLieu.Ten : "Không xác định",
                        Anh = x.SanPham.AnhDaiDien,
                        GiaGoc = x.ChiTietMoiNhat?.GiaBan ?? 0,
                        SoLuong = x.TongSoLuong,
                        IDKhuyenMai = x.ChiTietMoiNhat?.IDKhuyenMai
                    }).ToList();

                foreach (var item in lstSanPham)
                {
                    var khuyenMai = khuyenMais.FirstOrDefault(x => x.ID == item.IDKhuyenMai);
                    item.GiaBan = khuyenMai != null
                        ? GetKhuyenMai(khuyenMai.GiaTri, item.GiaGoc, khuyenMai.TrangThai)
                        : item.GiaGoc;
                }

                return lstSanPham;
            }
            catch
            {
                return new List<SanPhamViewModelAdmin>();
            }
        }
        public async Task<List<SanPhamViewModel>> GetAllSanPham()
        {
            try
            {
                // Lấy khuyến mãi đang áp dụng
                var khuyenMais = _context.KhuyenMais
                    .Where(x => x.NgayApDung <= DateTime.Now && x.NgayKetThuc > DateTime.Now)
                    .ToList();

                // Truy vấn sản phẩm + loại sp + chi tiết (LEFT JOIN)
                var lstSanPham = await (from sp in _context.SanPhams
                                        join loaiSP in _context.LoaiSPs on sp.IDLoaiSP equals loaiSP.ID
                                        join ctsp in _context.ChiTietSanPhams on sp.ID equals ctsp.IDSanPham into ctspJoin
                                        from ctsp in ctspJoin.DefaultIfEmpty()
                                        where sp.TrangThai == 1
                                        select new SanPhamViewModel
                                        {
                                            ID = sp.ID,
                                            Ten = sp.Ten,
                                            TrangThai = sp.TrangThai,
                                            TrangThaiCTSP = ctsp != null ? ctsp.TrangThai : 0,
                                            LoaiSP = loaiSP.Ten,
                                            IdChiTietSanPham = ctsp != null ? ctsp.ID : Guid.Empty,
                                            Image = sp.AnhDaiDien,
                                            IDMauSac = ctsp != null ? ctsp.IDMauSac : null,
                                            IDKichCo = ctsp != null ? ctsp.IDKichCo : null,
                                            IDChatLieu = sp.IDChatLieu,
                                            GiaGoc = ctsp != null ? ctsp.GiaBan : 0,
                                            SoLuong = ctsp != null ? ctsp.SoLuong : 0,
                                            IDKhuyenMai = ctsp != null ? ctsp.IDKhuyenMai : null,
                                            NgayTao = ctsp != null ? ctsp.NgayTao : DateTime.MinValue,
                                            soSao = 0 // sẽ tính sau
                                        }).ToListAsync();

                // Tính điểm đánh giá và giá khuyến mãi sau khi lấy xong
                foreach (var item in lstSanPham)
                {
                    // Tính sao trung bình
                    var danhGias = (from cthd in _context.ChiTietHoaDons.AsNoTracking()
                                    join ctsp in _context.ChiTietSanPhams.AsNoTracking() on cthd.IDCTSP equals ctsp.ID
                                    join dg in _context.DanhGias.AsNoTracking() on cthd.ID equals dg.ID
                                    where ctsp.IDSanPham == item.ID
                                    select dg.Sao).ToList();

                    item.soSao = danhGias.Any() ? danhGias.Average() : 0;

                    // Tính giá sau khuyến mãi
                    if (item.IDKhuyenMai != null)
                    {
                        var khuyenMai = khuyenMais.FirstOrDefault(x => x.ID == item.IDKhuyenMai);
                        if (khuyenMai != null)
                        {
                            item.GiaBan = GetKhuyenMai(khuyenMai.GiaTri, item.GiaGoc.Value, khuyenMai.TrangThai);
                            item.TrangThaiKM = khuyenMai.TrangThai;
                            item.GiaTriKM = khuyenMai.GiaTri;
                        }
                        else
                        {
                            item.GiaBan = item.GiaGoc.Value;
                        }
                    }
                    else
                    {
                        item.GiaBan = item.GiaGoc.Value;
                    }
                }

                return lstSanPham;
            }
            catch
            {
                return new List<SanPhamViewModel>();
            }
        }
        public bool CheckTrungTenSP(SanPhamRequest lsp)
        {
            throw new NotImplementedException();
        }
        public async Task<bool> UpdateTrangThaiSanPham(Guid id, int trangThai)
        {
            try
            {
                var sanPham = await _context.SanPhams.FirstOrDefaultAsync(x => x.ID == id);
                if (sanPham == null) return false;

                sanPham.TrangThai = trangThai;

                var chiTietList = await _context.ChiTietSanPhams
                    .Where(x => x.IDSanPham == id)
                    .ToListAsync();

                foreach (var chiTiet in chiTietList)
                {
                    if (trangThai == 0)
                    {
                        chiTiet.TrangThai = 0;
                    }
                    else if (trangThai == 1)
                    {
                        // ✅ CHỈ BẬT những biến thể có tồn kho > 0 hoặc có giá bán > 0
                        if (chiTiet.SoLuong > 0 || chiTiet.GiaBan > 0)
                        {
                            chiTiet.TrangThai = 1;
                        }
                        else
                        {
                            chiTiet.TrangThai = 0; // không đủ điều kiện hoạt động
                        }
                    }
                }

                _context.SanPhams.Update(sanPham);
                _context.ChiTietSanPhams.UpdateRange(chiTietList);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<bool> AddSanPham(SanPhamRequest request)
        {
            try
            {
                var LoaiSP = await _context.LoaiSPs.FirstOrDefaultAsync(x => x.ID == request.IDLoaiSP);
                if (LoaiSP == null)
                {
                    Console.WriteLine("Loại sản phẩm không tồn tại.");
                    return false;
                }

                var chatLieu = await _context.ChatLieus.FirstOrDefaultAsync(x => x.ID == request.IDChatLieu);
                if (chatLieu == null)
                {
                    Console.WriteLine("Chất liệu không tồn tại.");
                    return false;
                }

                var maxMa = _context.SanPhams.Any()
                    ? _context.SanPhams.Max(x => Convert.ToInt32(x.Ma.Substring(2)))
                    : 0;
                var maSanPham = "SP" + (maxMa + 1);

                var sanPham = new SanPham
                {
                    ID = Guid.NewGuid(),
                    Ten = request.Ten?.Trim(),
                    Ma = maSanPham,
                    MoTa = request.MoTa?.Trim(),
                    AnhDaiDien = request.AnhDaiDien,
                    TrangThai = 0, // KHÔNG HOẠT ĐỘNG khi tạo
                    IDLoaiSP = request.IDLoaiSP,
                    IDChatLieu = chatLieu.ID
                };
                await _context.SanPhams.AddAsync(sanPham);

                if (!request.IDMauSacs.Any() || !request.IDKichCos.Any())
                {
                    Console.WriteLine("Không có màu sắc hoặc kích cỡ.");
                    return false;
                }

                bool hasDefaultSet = false;
                bool hasValidChiTiet = false;

                foreach (var idMau in request.IDMauSacs.Distinct())
                {
                    foreach (var idSize in request.IDKichCos.Distinct())
                    {
                        var mau = await _context.MauSacs.FirstOrDefaultAsync(x => x.ID == idMau);
                        var size = await _context.KichCos.FirstOrDefaultAsync(x => x.ID == idSize);

                        if (mau == null || size == null)
                        {
                            Console.WriteLine($"Không tìm thấy màu {idMau} hoặc size {idSize}.");
                            continue;
                        }

                        var maChiTiet = RemoveUnicode($"{maSanPham}{mau.Ten?.Trim().ToUpper()}{size.Ten?.Trim().ToUpper()}");

                        var chiTiet = new ChiTietSanPham
                        {
                            ID = Guid.NewGuid(),
                            IDSanPham = sanPham.ID,
                            IDMauSac = idMau,
                            IDKichCo = idSize,
                            SoLuong = 0,
                            GiaBan = 0,
                            NgayTao = DateTime.Now,
                            IsDefault = !hasDefaultSet,
                            TrangThai = 0, // luôn khởi tạo ở trạng thái không hoạt động
                            MaSPChiTiet = maChiTiet
                        };

                        await _context.ChiTietSanPhams.AddAsync(chiTiet);
                        hasDefaultSet = true;
                        hasValidChiTiet = true;
                    }
                }

                if (!hasValidChiTiet)
                {
                    Console.WriteLine("Không có chi tiết sản phẩm hợp lệ nào.");
                    return false;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi AddSanPham: " + ex.Message);
                return false;
            }
        }
        #endregion

        #region ChiTietSanPham
        public async Task<ChiTietSanPhamUpdateRequest> AddChiTietSanPham(ChiTietSanPhamAddRequest request)
        {
            try
            {
                var maSanPham = await _context.SanPhams
                    .Where(x => x.ID == request.IDSanPham)
                    .Select(x => x.Ma)
                    .FirstOrDefaultAsync();
                var lst = new List<ChiTietSanPhamRequest>();
                // Lặp từng màu và size để tạo chi tiết
                foreach (var idMau in request.IDMauSacs)
                {
                    foreach (var idSize in request.IDKichCos)
                    {
                        var mau = await _context.MauSacs.FirstOrDefaultAsync(x => x.ID == idMau);
                        var size = await _context.KichCos.FirstOrDefaultAsync(x => x.ID == idSize);

                        var maChiTiet = RemoveUnicode($"{maSanPham}{mau.Ten?.Trim().ToUpper()}{size.Ten?.Trim().ToUpper()}");

                        var chiTiet = new ChiTietSanPham
                        {
                            ID = Guid.NewGuid(),
                            IDSanPham = request.IDSanPham,
                            IDMauSac = idMau,
                            IDKichCo = idSize,
                            SoLuong = 0,
                            GiaBan = 0,
                            NgayTao = DateTime.Now,
                            IsDefault = false,
                            TrangThai = 0, // luôn khởi tạo ở trạng thái không hoạt động
                            MaSPChiTiet = maChiTiet
                        };
                        lst.Add(new ChiTietSanPhamRequest
                        {
                            IDChiTietSanPham = chiTiet.ID,
                            IDMauSac = chiTiet.IDMauSac,
                            IDKichCo = chiTiet.IDKichCo,
                            TenKichCo = size.Ten,
                            TenMauSac = mau.Ten,
                            MaMau = mau.Ma,
                            SoLuong = chiTiet.SoLuong,
                            GiaBan = chiTiet.GiaBan,
                            GiaGoc = chiTiet.GiaBan,
                            trangThai = chiTiet.TrangThai                          
                        });
                        await _context.ChiTietSanPhams.AddAsync(chiTiet);
                        _context.SaveChanges();
                    }
                }
                // Trả về kết quả
                return new ChiTietSanPhamUpdateRequest
                {
                    IDSanPham = request.IDSanPham,
                    ChiTietSanPhams = lst
                        .GroupBy(x => new { x.IDMauSac, x.IDKichCo })
                        .Select(y => y.First())
                        .ToList(),
                    Location = 1,
                    Ma = maSanPham
                };
            }
            catch
            {
                return new ChiTietSanPhamUpdateRequest();
            }
        }
        public async Task<ChiTietSanPhamViewModel?> GetChiTietSanPhamByID(Guid id)
        {
            var temp = await _context.ChiTietSanPhams
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ID == id);
            if (temp == null) return null;

            var sanPham = await _context.SanPhams
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ID == temp.IDSanPham);

            var mauSac = await _context.MauSacs
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ID == temp.IDMauSac);

            var kichCo = await _context.KichCos
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ID == temp.IDKichCo);

            var anh = await _context.Anhs
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IDSanPhamChiTiet == temp.ID);

            if (sanPham == null || mauSac == null || kichCo == null)
                return null;

            var chiTietSanPham = new ChiTietSanPhamViewModel
            {
                ID = temp.ID,
                Ten = sanPham.Ten,
                SoLuong = temp.SoLuong,
                GiaGoc = temp.GiaBan,
                TrangThai = sanPham.TrangThai == 0 ? 0 : temp.TrangThai,
                Anh = anh?.DuongDan,
                MauSac = mauSac.Ten,
                KichCo = kichCo.Ten
            };

            var khuyenMai = await _context.KhuyenMais
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ID == temp.IDKhuyenMai && x.NgayKetThuc > DateTime.Now);

            chiTietSanPham.GiaBan = khuyenMai != null
                ? GetKhuyenMai(khuyenMai.GiaTri, chiTietSanPham.GiaGoc, khuyenMai.TrangThai)
                : chiTietSanPham.GiaGoc;

            return chiTietSanPham;
        }
        public async Task<ChiTietSanPhamViewModelHome> GetAllChiTietSanPhamHome(Guid idSanPham)
        {
            var result = new ChiTietSanPhamViewModelHome
            {
                IDSanPham = idSanPham,
                MauSacs = new List<GiaTriViewModel>(),
                KichCos = new List<GiaTriViewModel>(),
                Anhs = new List<AnhRequest>(),
                ChiTietSanPhams = new List<ChiTietSanPhamViewModel>(),
                LSTSPTuongTu = new List<SanPhamTuongTuViewModel>()
            };

            var sanPham = await _context.SanPhams
                .Include(sp => sp.ChiTietSanPhams)
                .FirstOrDefaultAsync(sp => sp.ID == idSanPham);

            if (sanPham == null) return result;

            result.Ten = sanPham.Ten;
            result.MoTa = sanPham.MoTa;

            var lstChiTietSanPham = sanPham.ChiTietSanPhams?.ToList() ?? new List<ChiTietSanPham>();

            // Màu sắc
            var mauSacIds = lstChiTietSanPham.Select(x => x.IDMauSac).Distinct().ToList();
            result.MauSacs = await _context.MauSacs
                .Where(x => mauSacIds.Contains(x.ID))
                .Select(x => new GiaTriViewModel { ID = x.ID, GiaTri = x.Ma, TenMau = x.Ten })
                .ToListAsync();

            // Kích cỡ
            var kichCoIds = lstChiTietSanPham.Select(x => x.IDKichCo).Distinct().ToList();
            result.KichCos = await _context.KichCos
                .Where(x => kichCoIds.Contains(x.ID))
                .OrderByDescending(x => x.Ten)
                .Select(x => new GiaTriViewModel { ID = x.ID, GiaTri = x.Ten })
                .ToListAsync();

            // Mapping màu
            var ctspToMau = lstChiTietSanPham.ToDictionary(ct => ct.ID, ct => ct.IDMauSac);
            var mauMaDict = result.MauSacs.ToDictionary(ms => ms.ID, ms => ms.GiaTri);

            // Lấy ảnh từ DB trực tiếp
            var anhList = await _context.Anhs
                .Where(a => lstChiTietSanPham.Select(ct => ct.ID).Contains(a.IDSanPhamChiTiet))
                .Where(a => !string.IsNullOrWhiteSpace(a.DuongDan))
                .Select(a => new AnhRequest
                {
                    IDAnh = a.ID,
                    IDSanPhamChiTiet = a.IDSanPhamChiTiet,
                    DuongDan = a.DuongDan
                })
                .ToListAsync();

            result.Anhs = anhList;

            // Chi tiết sản phẩm
            foreach (var item in lstChiTietSanPham)
            {
                var firstAnh = anhList.FirstOrDefault(a => a.IDSanPhamChiTiet == item.ID)?.DuongDan;

                result.ChiTietSanPhams.Add(new ChiTietSanPhamViewModel
                {
                    ID = item.ID,
                    MaCTSP = item.MaSPChiTiet,
                    Ten = sanPham.Ten,
                    SoLuong = item.SoLuong,
                    GiaBan = item.GiaBan,
                    GiaGoc = item.GiaBan,
                    MauSac = item.IDMauSac.ToString(),
                    KichCo = item.IDKichCo.ToString(),
                    TrangThai = item.TrangThai,
                    Anh = firstAnh
                });
            }

            // Đánh giá
            var danhGia = await (from ctsp in _context.ChiTietSanPhams
                                 join cthd in _context.ChiTietHoaDons on ctsp.ID equals cthd.IDCTSP
                                 join dg in _context.DanhGias.Where(p => p.TrangThai == 1) on cthd.ID equals dg.ID
                                 where ctsp.IDSanPham == idSanPham
                                 select dg.Sao)
                                 .Where(x => x.HasValue)
                                 .ToListAsync();

            if (danhGia.Any())
            {
                result.SoSao = (float)danhGia.Sum(x => x.Value) / danhGia.Count();
                result.sosaoPercent = Convert.ToInt32((result.SoSao / 5f) * 100);
                result.SoDanhGia = danhGia.Count();
            }

            // Sản phẩm tương tự
            result.LSTSPTuongTu = await (from sp in _context.SanPhams
                                         where sp.IDLoaiSP == sanPham.IDLoaiSP && sp.ID != idSanPham
                                         join ctsp in _context.ChiTietSanPhams on sp.ID equals ctsp.IDSanPham
                                         where ctsp.TrangThai == 1
                                         group new { sp, ctsp } by sp.ID into g
                                         select new SanPhamTuongTuViewModel
                                         {
                                             IDSP = g.Key,
                                             TenSP = g.First().sp.Ten,
                                             GiaSPTT = g.First().ctsp.GiaBan,
                                             DuongDanSPTT = _context.Anhs
                                                 .Where(a => a.IDSanPhamChiTiet == g.First().ctsp.ID)
                                                 .Select(a => a.DuongDan)
                                                 .FirstOrDefault()
                                         })
                                         .Take(5)
                                         .ToListAsync();

            return result;
        }
        public Task<bool> UpdateChiTietSanPham(ChiTietSanPham chiTietSanPham)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> UpdateSoluongChiTietSanPham(Guid id, int soLuong)
        {
            try
            {
                var chiTietSanPham = await _context.ChiTietSanPhams.FirstAsync(x => x.ID == id);

                chiTietSanPham.SoLuong = soLuong;

                // Nếu cả giá bán <= 0 thì ngừng hoạt động
                if (soLuong <= 0)
                {
                    chiTietSanPham.TrangThai = 0;
                }
                else
                {
                    chiTietSanPham.TrangThai = 1;
                }

                _context.ChiTietSanPhams.Update(chiTietSanPham);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<decimal> UpdateGiaGocChiTietSanPham(Guid id, decimal giaGoc)
        {
            try
            {
                var chiTietSanPham = await _context.ChiTietSanPhams.FirstOrDefaultAsync(x => x.ID == id);
                if (chiTietSanPham == null) return -1;

                // Cập nhật giá gốc (được lưu vào trường GiaBan)
                chiTietSanPham.GiaBan = giaGoc;

                // Cập nhật trạng thái: nếu giá <= 0 thì Ngùng hoạt động, ngược lại thì Hoạt động
                chiTietSanPham.TrangThai = (giaGoc <= 0) ? 0 : 1;

                _context.ChiTietSanPhams.Update(chiTietSanPham);
                await _context.SaveChangesAsync();

                // Tính giá thực tế để hiển thị (nếu có khuyến mãi còn hiệu lực)
                decimal giaThucTe = giaGoc;

                if (chiTietSanPham.IDKhuyenMai.HasValue)
                {
                    var km = await _context.KhuyenMais.FirstOrDefaultAsync(x =>
                        x.ID == chiTietSanPham.IDKhuyenMai &&
                        x.TrangThai == 1 &&
                        x.NgayKetThuc > DateTime.Now);

                    if (km != null)
                    {
                        giaThucTe = GetKhuyenMai(km.GiaTri, giaGoc, km.KieuGiamGia);
                    }
                }

                return giaThucTe;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi cập nhật giá gốc: " + ex.Message);
                return -1;
            }
        }
        public async Task<bool> UpdateTrangThaiChiTietSanPham(Guid id)
        {
            try
            {
                var chiTietSanPham = await _context.ChiTietSanPhams.FindAsync(id);

                if (chiTietSanPham == null)
                {
                    Console.WriteLine("Không tìm thấy chi tiết sản phẩm.");
                    return false;
                }

                bool isHiding = chiTietSanPham.TrangThai == 1;
                chiTietSanPham.TrangThai = isHiding ? 0 : 1;

                if (isHiding && chiTietSanPham.IsDefault)
                {
                    chiTietSanPham.IsDefault = false;

                    var bienTheKhac = await _context.ChiTietSanPhams
                        .Where(x => x.IDSanPham == chiTietSanPham.IDSanPham
                                 && x.ID != chiTietSanPham.ID
                                 && x.TrangThai == 1
                                 && x.GiaBan > 0
                                 && x.SoLuong > 0)
                        .OrderBy(x => x.GiaBan)
                        .FirstOrDefaultAsync();

                    if (bienTheKhac != null)
                    {
                        bienTheKhac.IsDefault = true;
                        _context.ChiTietSanPhams.Update(bienTheKhac);
                    }
                    else
                    {
                        Console.WriteLine("Không còn biến thể hợp lệ để gán mặc định.");
                    }
                }

                if (!isHiding)
                {
                    var daCoMacDinh = await _context.ChiTietSanPhams
                        .AnyAsync(x => x.IDSanPham == chiTietSanPham.IDSanPham && x.IsDefault && x.TrangThai == 1);

                    if (!daCoMacDinh && chiTietSanPham.GiaBan > 0 && chiTietSanPham.SoLuong > 0)
                    {
                        chiTietSanPham.IsDefault = true;
                    }
                }

                _context.ChiTietSanPhams.Update(chiTietSanPham);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi cập nhật trạng thái chi tiết sản phẩm: " + ex.Message);
                return false;
            }
        }
        public async Task<List<ChiTietSanPhamViewModelAdmin>> GetAllChiTietSanPhamAdmin(Guid idSanPham)
        {
            try
            {
                // Lấy danh sách khuyến mãi còn hiệu lực
                var khuyenMais = _context.KhuyenMais
                    .Where(x => x.NgayKetThuc > DateTime.Now && x.NgayApDung <= DateTime.Now)
                    .ToList();

                // Lấy chi tiết sản phẩm
                var lstChiTietSanPham = await (from a in _context.ChiTietSanPhams.Where(x => x.IDSanPham == idSanPham)
                                               join b in _context.MauSacs on a.IDMauSac equals b.ID
                                               join c in _context.KichCos on a.IDKichCo equals c.ID
                                               select new ChiTietSanPhamViewModelAdmin
                                               {
                                                   ID = a.ID,
                                                   Ma = a.MaSPChiTiet,
                                                   TenMauSac = b.Ten,
                                                   MaMauSac = b.Ma,
                                                   TenKichCo = c.Ten,
                                                   SoLuong = a.SoLuong,
                                                   GiaGoc = a.GiaBan,
                                                   IDKhuyenMai = a.IDKhuyenMai,
                                                   TrangThai = a.TrangThai,
                                                   MacDinh = a.IsDefault,
                                               }).ToListAsync();

                // Gom các ID cần xóa khuyến mãi
                var idsToRemove = new List<Guid>();

                foreach (var item in lstChiTietSanPham)
                {
                    item.GiaBan = item.GiaGoc;

                    if (item.IDKhuyenMai != null)
                    {
                        var km = khuyenMais.FirstOrDefault(x => x.ID == item.IDKhuyenMai);

                        if (km != null)
                        {
                            item.GiaBan = GetKhuyenMai(km.GiaTri, item.GiaGoc, km.TrangThai);
                            item.GiaTriKhuyenMai = km.TrangThai == 1
                                ? $"-{km.GiaTri}%"
                                : $"-{km.GiaTri}";
                        }
                        else
                        {
                            idsToRemove.Add(item.ID); // gom ID lại để xử lý sau
                        }
                    }
                }

                // Xử lý cập nhật DB nếu có ID cần xoá
                if (idsToRemove.Any())
                {
                    DeleteKhuyenMaisAsync(idsToRemove); // gọi hàm xử lý 1 lượt
                }

                return lstChiTietSanPham.OrderBy(x => x.Ma).ToList();
            }
            catch
            {
                return new List<ChiTietSanPhamViewModelAdmin>();
            }
        }
        public async Task<List<ChiTietSanPhamViewModel>> GetAllChiTietSanPham()
        {
            try
            {
                return await (from sp in _context.SanPhams.AsNoTracking()
                              join ctsp in _context.ChiTietSanPhams.AsNoTracking()
                              on sp.ID equals ctsp.IDSanPham
                              select new ChiTietSanPhamViewModel()
                              {
                                  ID = ctsp.ID,
                                  Ten = sp.Ten,
                                  GiaBan = ctsp.GiaBan,
                                  GiaGoc = ctsp.GiaBan,
                                  TrangThai = ctsp.TrangThai,
                                  SoLuong = ctsp.SoLuong,
                              }).ToListAsync();
            }
            catch
            {
                return new List<ChiTietSanPhamViewModel>();
            }
        }
        public async Task<bool> DeleteChiTietSanPham(Guid id)
        {
            try
            {
                var chiTietSanPham = await _context.ChiTietSanPhams.FindAsync(id);
                if (chiTietSanPham == null) return false;

                if (chiTietSanPham.TrangThai == 1)
                {
                    bool isDefault = chiTietSanPham.IsDefault;

                    // Nếu đang là mặc định, kiểm tra còn biến thể nào khác hợp lệ không
                    if (isDefault)
                    {
                        var bienTheKhac = await _context.ChiTietSanPhams
                            .Where(x => x.IDSanPham == chiTietSanPham.IDSanPham
                                     && x.ID != chiTietSanPham.ID
                                     && x.TrangThai == 1
                                     && x.GiaBan > 0
                                     && x.SoLuong > 0)
                            .ToListAsync();

                        if (bienTheKhac.Count == 0)
                        {
                            Console.WriteLine("Không thể xóa vì không còn biến thể hợp lệ để gán mặc định.");
                            return false;
                        }

                        // Nếu có biến thể khác → gỡ mặc định & gán mới
                        chiTietSanPham.IsDefault = false;

                        var newMacDinh = bienTheKhac.OrderBy(x => x.GiaBan).First();
                        newMacDinh.IsDefault = true;
                        _context.ChiTietSanPhams.Update(newMacDinh);
                    }

                    chiTietSanPham.TrangThai = 0;
                    _context.ChiTietSanPhams.Update(chiTietSanPham);
                    await _context.SaveChangesAsync();
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
        public async Task<bool> UndoChiTietSanPham(Guid id)
        {
            try
            {
                var chiTietSanPham = await _context.ChiTietSanPhams.FindAsync(id);
                if (chiTietSanPham == null) return false;

                // Đã đang hoạt động => không cần undo
                if (chiTietSanPham.TrangThai == 1) return false;

                // Giá và số lượng phải hợp lệ
                if (chiTietSanPham.GiaBan <= 0 || chiTietSanPham.SoLuong <= 0)
                {
                    Console.WriteLine("Không thể bật lại vì giá hoặc số lượng không hợp lệ.");
                    return false;
                }

                // Kiểm tra sản phẩm cha
                var sanPham = await _context.SanPhams.FindAsync(chiTietSanPham.IDSanPham);
                if (sanPham == null || sanPham.TrangThai != 1)
                {
                    Console.WriteLine("Không thể bật lại vì sản phẩm cha đang bị ẩn hoặc không tồn tại.");
                    return false;
                }

                // OK, bật lại
                chiTietSanPham.TrangThai = 1;
                _context.ChiTietSanPhams.Update(chiTietSanPham);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public List<UploadAnhViewModel> GetAllAnhSanPhamChiTiet(Guid idSanPham)
        {
            try
            {
                var chiTietSPs = _context.ChiTietSanPhams
                            .Where(x => x.IDSanPham == idSanPham)
                            .Include(x => x.MauSac)
                            .Include(x => x.Anhs)
                            .AsEnumerable(); // chuyển sang xử lý in-memory

                var result = chiTietSPs
                     .Where(x => x.MauSac != null) // tránh null
                     .GroupBy(ctsp => ctsp.IDMauSac)
                     .Select(group => new UploadAnhViewModel
                     {
                         IDMauSac = group.Key,
                         TenMau = group.First().MauSac?.Ten ?? "Không rõ",
                         MaMau = group.First().MauSac?.Ma ?? "#000000",
                         DanhSachIDChiTietSP = group.Select(x => x.ID).ToList(),

                         DuongDanAnh = group
                             .SelectMany(x => x.Anhs ?? new List<Anh>())
                             .Where(a => !string.IsNullOrWhiteSpace(a.DuongDan))
                             //.OrderBy(a => a.ThuTu) // nếu cần sắp xếp
                             .Select(a => a.DuongDan)
                             .Distinct()
                             .ToList(),

                         DanhSachIDAnh = group
                             .SelectMany(x => x.Anhs ?? new List<Anh>())
                             .Where(a => !string.IsNullOrWhiteSpace(a.DuongDan))
                             .Select(a => a.ID)
                             .Distinct()
                             .ToList(),
                         IDAnh = group
                             .SelectMany(x => x.Anhs ?? new List<Anh>())
                             .Where(a => !string.IsNullOrWhiteSpace(a.DuongDan))
                             .Select(a => a.ID)
                             .FirstOrDefault()
                     })
                     .ToList();

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetAllAnhSanPhamChiTiet] Lỗi: {ex.Message}\n{ex.StackTrace}");
                return new List<UploadAnhViewModel>();
            }
        }
        public async Task<bool> AddImage(List<AnhRequest> requests)
        {
            try
            {
                if (requests == null || !requests.Any())
                    return false;
                var listAnh = requests.Select(x => new Anh
                {
                    ID = Guid.NewGuid(),
                    IDSanPhamChiTiet = x.IDSanPhamChiTiet,
                    DuongDan = x.DuongDan,
                    TrangThai = 1
                }).ToList();

                _context.Anhs.AddRange(listAnh);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AddImage] Lỗi khi lưu ảnh: {ex.Message}");
                return false;
            }
        }
        public async Task<bool> DeleteImage(Guid id)
        {
            var temp = await _context.Anhs
             .FirstOrDefaultAsync(x => x.ID == id);

            if (temp == null) return false;

            // Kiểm tra ChiTietSanPham liên kết có IDMauSac hay không
            bool hasMauSac = temp.ChiTietSanPham?.IDMauSac != null;

            if (hasMauSac)
            {
                temp.DuongDan = null;
                _context.Anhs.Update(temp);
            }
            else
            {
                _context.Anhs.Remove(temp);
            }

            await _context.SaveChangesAsync();
            return true;

        }
        public async Task<bool> UpdateMacDinhChiTietSanPham(Guid idChiTietSP)
        {
            // B1: Lấy chi tiết sản phẩm cần cập nhật và đảm bảo đang hoạt động
            var chiTiet = await _context.ChiTietSanPhams
                .FirstOrDefaultAsync(x => x.ID == idChiTietSP && x.TrangThai == 1);

            if (chiTiet == null)
            {
                Console.WriteLine("Chi tiết sản phẩm không tồn tại hoặc không hoạt động.");
                return false;
            }

            var idSanPham = chiTiet.IDSanPham;

            // B2: Lấy tất cả chi tiết sản phẩm đang hoạt động của sản phẩm này
            var danhSachChiTiet = await _context.ChiTietSanPhams
                .Where(x => x.IDSanPham == idSanPham && x.TrangThai == 1)
                .ToListAsync();

            if (!danhSachChiTiet.Any())
            {
                Console.WriteLine("Không có chi tiết sản phẩm hoạt động.");
                return false;
            }

            // B3: Đặt tất cả về false trước
            foreach (var item in danhSachChiTiet)
            {
                item.IsDefault = false;
            }

            // B4: Gán mặc định cho ID được chọn
            var chiTietMoi = danhSachChiTiet.FirstOrDefault(x => x.ID == idChiTietSP);
            if (chiTietMoi != null)
            {
                chiTietMoi.IsDefault = true;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region LoaiSP
        public Task<LoaiSP> SaveLoaiSP(LoaiSPRequest lsp)
        {
            throw new NotImplementedException();
        }
        public Task<LoaiSP> GetLoaiSPById(Guid id)
        {
            throw new NotImplementedException();
        }
        public bool CheckTrungLoaiSP(LoaiSPRequest lsp)
        {
            throw new NotImplementedException();
        }
        public async Task<bool> DeleteLoaiSP(Guid id)
        {
            try
            {
                var loaiSP = await _context.LoaiSPs.FindAsync(id);
                loaiSP.TrangThai = 0;
                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<List<LoaiSP>> GetAllLoaiSP()
        {
            return await _context.LoaiSPs
                .Where(x => x.TrangThai == 1)
                .ToListAsync();
        }
        #endregion

        #region Other
        public string RemoveUnicode(string text)
        {
            string[] arr1 = new string[] { "á", "à", "ả", "ã", "ạ", "â", "ấ", "ầ", "ẩ", "ẫ", "ậ", "ă", "ắ", "ằ", "ẳ", "ẵ", "ặ", "đ", "é", "è", "ẻ", "ẽ", "ẹ", "ê", "ế", "ề", "ể", "ễ", "ệ", "í", "ì", "ỉ", "ĩ", "ị", "ó", "ò", "ỏ", "õ", "ọ", "ô", "ố", "ồ", "ổ", "ỗ", "ộ", "ơ", "ớ", "ờ", "ở", "ỡ", "ợ", "ú", "ù", "ủ", "ũ", "ụ", "ư", "ứ", "ừ", "ử", "ữ", "ự", "ý", "ỳ", "ỷ", "ỹ", "ỵ", };
            string[] arr2 = new string[] { "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "d", "e", "e", "e", "e", "e", "e", "e", "e", "e", "e", "e", "i", "i", "i", "i", "i", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "u", "u", "u", "u", "u", "u", "u", "u", "u", "u", "u", "y", "y", "y", "y", "y", };
            for (int i = 0; i < arr1.Length; i++)
            {
                text = text.Replace(arr1[i].ToUpper(), arr2[i].ToUpper());
            }
            text = text.Replace(" ", "");
            return text;
        }

        public async Task<List<MauSac>> GetAllMauSac()
        {
            return await _context.MauSacs.Where(x => x.TrangThai == 1).ToListAsync();
        }

        public async Task<List<KichCo>> GetAllKichCo()
        {
            return await _context.KichCos.Where(x => x.TrangThai == 1).ToListAsync();
        }

        public async Task<List<ChatLieu>> GetAllChatLieu()
        {
            return await _context.ChatLieus.Where(x => x.TrangThai == 1).ToListAsync();
        }

        public decimal GetKhuyenMai(decimal giaTri, decimal giaSP, int kieugiamgia)
        {
            var tienKhuyenMai = giaSP;
            //var khuyenMai = _context.KhuyenMais.First(x => x.ID == idKhuyenMai);
            if (kieugiamgia == 0)
            {
                tienKhuyenMai -= giaTri;
            }
            else if (kieugiamgia == 1)
            {
                tienKhuyenMai -= (giaTri * giaSP) / 100;
            }
            return tienKhuyenMai < 0 ? 0 : tienKhuyenMai;
        }

        public async Task DeleteKhuyenMaisAsync(List<Guid> idsChiTietSP)
        {
            var items = await _context.ChiTietSanPhams
                .Where(x => idsChiTietSP.Contains(x.ID) && x.IDKhuyenMai != null)
                .ToListAsync();

            foreach (var item in items)
            {
                item.IDKhuyenMai = null;
            }

            _context.ChiTietSanPhams.UpdateRange(items);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteKhuyenMaiAsync(Guid id)
        {
            var item = await _context.ChiTietSanPhams.FirstOrDefaultAsync(x => x.ID == id);
            if (item != null)
            {
                item.IDKhuyenMai = null;
                _context.ChiTietSanPhams.Update(item);
                await _context.SaveChangesAsync();
            }
        }
        #endregion

        //Nhinh thêm
        #region SanPhamBanHang
        public async Task<List<SanPhamBanHang>> GetAllSanPhamTaiQuay()
        {
            var result = await (from sp in _context.SanPhams.AsNoTracking().Where(c => c.TrangThai != 0)
                                join ctsp in _context.ChiTietSanPhams.Where(c => c.TrangThai == 1) on sp.ID equals ctsp.IDSanPham into ctspGroup
                                from ctsp in ctspGroup.DefaultIfEmpty()

                                join km in _context.KhuyenMais.Where(c => c.NgayKetThuc > DateTime.Now && c.TrangThai != 2) on ctsp.IDKhuyenMai equals km.ID into kmGroup
                                from km in kmGroup.DefaultIfEmpty()
                                select new SanPhamBanHang()
                                {
                                    Id = sp.ID,
                                    Ten = sp.Ten,
                                    MaSP = sp.Ma,
                                    Anh = sp.AnhDaiDien,
                                    GiaGoc = ctsp == null ? null : ctsp.GiaBan,
                                    GiaBan = km == null ? ctsp.GiaBan :
                                    (km.TrangThai == 1 ? (int)(ctsp.GiaBan / 100 * (100 - km.GiaTri)) :
                                    (km.GiaTri < ctsp.GiaBan ? (ctsp.GiaBan - (int)km.GiaTri) : 0)),
                                    IdLsp = sp.IDLoaiSP,
                                }).OrderBy(c => c.MaSP).Distinct().ToListAsync();

            return result;
        }
        public async Task<ChiTietSanPhamBanHang> GetChiTietSPBHById(Guid idsp)
        {
            var lstMS = (from ctsp in _context.ChiTietSanPhams.AsNoTracking()
                         join ms in _context.MauSacs.AsNoTracking() on ctsp.IDMauSac equals ms.ID
                         where ctsp.IDSanPham == idsp && ctsp.TrangThai != 0
                         select new MauSac
                         {
                             ID = ms.ID,
                             Ma = ms.Ma,
                             Ten = ms.Ten,
                         }).Distinct().ToList();

            var lstKC = (from ctsp in _context.ChiTietSanPhams
                         join kc in _context.KichCos on ctsp.IDKichCo equals kc.ID
                         where ctsp.IDSanPham == idsp && ctsp.TrangThai != 0
                         select new KichCo
                         {
                             ID = kc.ID,
                             Ten = kc.Ten,
                         }).Distinct().ToList();

            var result = await (from sp in _context.SanPhams.AsNoTracking()
                                where sp.ID == idsp && sp.TrangThai != 0
                                select new ChiTietSanPhamBanHang()
                                {
                                    Id = sp.ID,
                                    Ten = sp.Ten,
                                    lstMau = lstMS,
                                    lstKC = lstKC,
                                }).FirstOrDefaultAsync();
            return result;
        }
        public async Task<List<ChiTietCTSPBanHang>> GetChiTietCTSPBanHang(Guid idsp)
        {
            // Lấy ảnh theo từng màu sắc
            var anhTheoMau = await (from a in _context.Anhs
                                    join ct in _context.ChiTietSanPhams on a.IDSanPhamChiTiet equals ct.ID
                                    where a.TrangThai == 1 && ct.IDSanPham == idsp
                                    group new { a, ct } by ct.IDMauSac into g
                                    select new
                                    {
                                        IDMauSac = g.Key,
                                        DuongDan = g.OrderBy(x => x.a.ID).Select(x => x.a.DuongDan).FirstOrDefault()
                                    }).ToListAsync();

            // Load tất cả chi tiết sản phẩm (chưa map ảnh)
            var ctspList = await (from ctsp in _context.ChiTietSanPhams
                                  join ms in _context.MauSacs on ctsp.IDMauSac equals ms.ID
                                  join kc in _context.KichCos on ctsp.IDKichCo equals kc.ID
                                  join sp in _context.SanPhams on ctsp.IDSanPham equals sp.ID
                                  join km in _context.KhuyenMais
                                      .Where(c => c.NgayKetThuc > DateTime.Now && c.TrangThai != 2)
                                      on ctsp.IDKhuyenMai equals km.ID into kmGroup
                                  from km in kmGroup.DefaultIfEmpty()
                                  where ctsp.IDSanPham == idsp && ctsp.TrangThai != 0
                                  select new
                                  {
                                      ctsp,
                                      ms,
                                      kc,
                                      sp,
                                      km
                                  }).ToListAsync();

            // Duyệt và map thủ công
            var result = ctspList.Select(x =>
            {
                var duongDan = anhTheoMau.FirstOrDefault(a => a.IDMauSac == x.ctsp.IDMauSac)?.DuongDan ?? "";
                var giaBan = x.km == null
                             ? x.ctsp.GiaBan
                             : (x.km.TrangThai == 1
                                 ? (int)(x.ctsp.GiaBan / 100 * (100 - x.km.GiaTri))
                                 : (x.km.GiaTri < x.ctsp.GiaBan ? x.ctsp.GiaBan - (int)x.km.GiaTri : 0));

                return new ChiTietCTSPBanHang
                {
                    Id = x.ctsp.ID,
                    Ten = x.sp.Ten,
                    ChiTiet = x.ms.Ten + " - " + x.kc.Ten,
                    idMauSac = x.ctsp.IDMauSac,
                    idKichCo = x.ctsp.IDKichCo,
                    SoLuong = x.ctsp.SoLuong,
                    GiaGoc = x.ctsp.GiaBan,
                    GiaBan = giaBan,
                    Anh = duongDan
                };
            }).OrderByDescending(x => x.ChiTiet).ToList();

            return result;
        }
        public Guid GetIDsanPhamByIdCTSP(Guid idctsp)
        {
            var ctsp = _context.ChiTietSanPhams.FirstOrDefault(p => p.ID == idctsp);
            return ctsp.IDSanPham;
        }
        public async Task<List<HomeProductViewModel>> GetAllSanPhamTrangChu()
        {
            // Lấy dữ liệu gốc: sản phẩm + chi tiết + khuyến mãi
            var dataRaw = await (from sp in _context.SanPhams.AsNoTracking()
                                 where sp.TrangThai != 0
                                 join ctsp in _context.ChiTietSanPhams
                                     .Where(c => c.TrangThai == 1)
                                     on sp.ID equals ctsp.IDSanPham into ctspGroup
                                 from ctsp in ctspGroup.DefaultIfEmpty()

                                 join km in _context.KhuyenMais
                                     .Where(c => c.NgayKetThuc > DateTime.Now && c.TrangThai != 2)
                                     on ctsp.IDKhuyenMai equals km.ID into kmGroup
                                 from km in kmGroup.DefaultIfEmpty()

                                 select new
                                 {
                                     sp,
                                     ctsp,
                                     km
                                 }).ToListAsync();

            // Lấy sẵn toàn bộ đánh giá để tính SoSao
            var danhGiaLookup = await (from cthd in _context.ChiTietHoaDons
                                       join dg in _context.DanhGias on cthd.ID equals dg.ID
                                       join ctsp in _context.ChiTietSanPhams on cthd.IDCTSP equals ctsp.ID
                                       group dg by ctsp.IDSanPham into g
                                       select new
                                       {
                                           IDSanPham = g.Key,
                                           SoSaoTB = (double?)g.Average(x => x.Sao)
                                       }).ToDictionaryAsync(x => x.IDSanPham, x => x.SoSaoTB);

            // Lấy sẵn tổng số lượng bán
            var soLuongBanLookup = await (from h in _context.HoaDons
                                          where h.TrangThaiGiaoHang == 6 && h.LoaiHoaDon == 0
                                          join cthd in _context.ChiTietHoaDons on h.ID equals cthd.IDHoaDon
                                          join ctsp in _context.ChiTietSanPhams on cthd.IDCTSP equals ctsp.ID
                                          group cthd by ctsp.IDSanPham into g
                                          select new
                                          {
                                              IDSanPham = g.Key,
                                              SoLuongBan = g.Sum(x => x.SoLuong)
                                          }).ToDictionaryAsync(x => x.IDSanPham, x => x.SoLuongBan);

            // Mapping sang ViewModel
            var result = dataRaw.Select(item =>
            {
                var soSao = danhGiaLookup.ContainsKey(item.sp.ID) ? danhGiaLookup[item.sp.ID] : 0;
                var slBan = soLuongBanLookup.ContainsKey(item.sp.ID) ? soLuongBanLookup[item.sp.ID] : 0;

                // Tính giá bán sau khuyến mãi
                decimal? giaBan = null;
                if (item.ctsp != null)
                {
                    giaBan = item.ctsp.GiaBan;
                    if (item.km != null)
                    {
                        if (item.km.TrangThai == 1) // Giảm %
                            giaBan = item.ctsp.GiaBan / 100 * (100 - item.km.GiaTri);
                        else // Giảm số tiền trực tiếp
                            giaBan = (item.km.GiaTri < item.ctsp.GiaBan)
                                ? (item.ctsp.GiaBan - item.km.GiaTri)
                                : 0;
                    }
                }

                return new HomeProductViewModel
                {
                    Id = item.sp.ID,
                    Ten = item.sp.Ten,
                    IdCTSP = item.ctsp?.ID,
                    Anh = item.sp.AnhDaiDien,
                    SLBan = slBan,
                    SoSao = soSao,
                    NgayTao = item.ctsp?.NgayTao,
                    GiaGoc = item.ctsp?.GiaBan ?? 0,
                    GiaBan = giaBan,
                    KhuyenMai = item.km?.GiaTri,
                    SoLuongSP = item.ctsp?.SoLuong ?? 0
                };
            }).ToList();

            return result;
        }
        #endregion
    }
}
