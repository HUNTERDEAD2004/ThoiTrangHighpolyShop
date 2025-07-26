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
            try
            {
                var sanpham = await _context.SanPhams.FirstAsync(x => x.ID == request.ID);
                LoaiSP? loaiSPCon = _context.LoaiSPs.Where(x => x.IDLoaiSPCha != null).FirstOrDefault(x => x.Ten == request.TenLoaiSPCon);
                ChatLieu? chatLieu = _context.ChatLieus.FirstOrDefault(x => x.Ten == request.TenChatLieu);
                if (loaiSPCon == null)
                {
                    LoaiSP? loaiSPCha = _context.LoaiSPs.Where(x => x.IDLoaiSPCha == null).FirstOrDefault(x => x.Ten == request.TenLoaiSPCha);
                    if (loaiSPCha == null)
                    {
                        loaiSPCha = new LoaiSP() { ID = Guid.NewGuid(), Ten = request.TenLoaiSPCha, TrangThai = 1 };
                        _context.LoaiSPs.AddAsync(loaiSPCha);
                    }
                    loaiSPCon = new LoaiSP() { ID = Guid.NewGuid(), Ten = request.TenLoaiSPCon, IDLoaiSPCha = loaiSPCha.ID, TrangThai = 1 };
                    await _context.LoaiSPs.AddAsync(loaiSPCon);
                }
                if (chatLieu == null)
                {
                    chatLieu = new ChatLieu() { ID = Guid.NewGuid(), Ten = request.TenChatLieu, TrangThai = 1 };
                    await _context.AddAsync(chatLieu);
                }
                sanpham.Ten = request.Ten;
                sanpham.MoTa = request.MoTa;
                sanpham.AnhDaiDien = request.AnhDaiDien;
                sanpham.IDChatLieu = chatLieu.ID;
                sanpham.IDLoaiSP = loaiSPCon.ID;
                _context.SanPhams.Update(sanpham);
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
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
                var loaiSP = await _context.LoaiSPs.FirstAsync(x => x.ID == sanPham.IDLoaiSP);
                var response = new SanPhamUpdateRequest()
                {
                    ID = sanPham.ID,
                    Ten = sanPham.Ten,
                    MoTa = sanPham.MoTa,
                    TenChatLieu = _context.ChatLieus.First(x => x.ID == sanPham.IDChatLieu).Ten,
                    TenLoaiSPCha = _context.LoaiSPs.First(x => x.ID == loaiSP.IDLoaiSPCha).Ten,
                    TenLoaiSPCon = loaiSP.Ten
                };
                return response;
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
                // Lấy danh sách khuyến mãi áp dụng tại thời điểm hiện tại
                var khuyenMais = _context.KhuyenMais
                    .Where(x => x.NgayKetThuc > DateTime.Now && x.NgayApDung < DateTime.Now)
                    .ToList();

                // Truy vấn sản phẩm + loại sản phẩm con + cha + chi tiết + chất liệu
                var lstSanPham = (from sp in _context.SanPhams
                                  join loaiCon in _context.LoaiSPs on sp.IDLoaiSP equals loaiCon.ID
                                  join loaiCha in _context.LoaiSPs on loaiCon.IDLoaiSPCha equals loaiCha.ID into loaiChaJoin
                                  from loaiCha in loaiChaJoin.DefaultIfEmpty()
                                  join cl in _context.ChatLieus on sp.IDChatLieu equals cl.ID into chatLieuJoin
                                  from cl in chatLieuJoin.DefaultIfEmpty()
                                  select new
                                  {
                                      SanPham = sp,
                                      LoaiSPCon = loaiCon,
                                      LoaiSPCha = loaiCha,
                                      ChatLieu = cl,
                                      ChiTietSP = _context.ChiTietSanPhams
                                        .Where(x => x.IDSanPham == sp.ID)
                                        .OrderByDescending(x => x.NgayTao)
                                        .FirstOrDefault()
                                  }).ToList()
                   .Select(x => new SanPhamViewModelAdmin
                   {
                       ID = x.SanPham.ID,
                       Ten = x.SanPham.Ten,
                       Ma = x.SanPham.Ma,
                       TrangThai = x.SanPham.TrangThai,
                       LoaiSPCon = x.LoaiSPCon.Ten,
                       LoaiSPCha = x.LoaiSPCha != null ? x.LoaiSPCha.Ten : "Không có",
                       ChatLieu = x.ChatLieu != null ? x.ChatLieu.Ten : "Không xác định",
                       Anh = x.SanPham.AnhDaiDien,
                       GiaGoc = x.ChiTietSP != null ? x.ChiTietSP.GiaBan : 0,
                       SoLuong = x.ChiTietSP != null ? x.ChiTietSP.SoLuong : 0,
                       IDKhuyenMai = x.ChiTietSP?.IDKhuyenMai
                   }).ToList();

                // Tính giá bán sau khuyến mãi
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

                // Lấy tất cả biến thể của sản phẩm
                var chiTietList = await _context.ChiTietSanPhams
                    .Where(x => x.IDSanPham == id)
                    .ToListAsync();

                foreach (var chiTiet in chiTietList)
                {
                    // Nếu sản phẩm bị tắt → tắt toàn bộ chi tiết (kể cả mặc định)
                    // Nếu sản phẩm vẫn hoạt động → chỉ tắt các chi tiết không phải mặc định
                    if (trangThai != 1 || !chiTiet.IsDefault)
                    {
                        chiTiet.TrangThai = trangThai;
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
                // Kiểm tra loại sản phẩm
                LoaiSP? loaiSPCon = request.IDLoaiSPCon.HasValue
                    ? await _context.LoaiSPs.FirstOrDefaultAsync(x => x.ID == request.IDLoaiSPCon)
                    : await _context.LoaiSPs.FirstOrDefaultAsync(x => x.ID == request.IDLoaiSPCha);
                if (loaiSPCon == null)
                {
                    Console.WriteLine("Loại sản phẩm không tồn tại");
                    return false;
                }

                // Kiểm tra chất liệu
                var chatLieu = await _context.ChatLieus.FirstOrDefaultAsync(x => x.ID == request.IDChatLieu);
                if (chatLieu == null)
                {
                    Console.WriteLine("Chất liệu không tồn tại");
                    return false;
                }

                // Tạo mã sản phẩm
                var ma = _context.SanPhams.Any()
                    ? _context.SanPhams.Max(x => Convert.ToInt32(x.Ma.Substring(2)))
                    : 0;

                var sanPham = new SanPham
                {
                    ID = Guid.NewGuid(),
                    Ten = request.Ten,
                    Ma = "SP" + (ma + 1),
                    MoTa = request.MoTa,
                    AnhDaiDien = request.AnhDaiDien,
                    TrangThai = 0,
                    IDLoaiSP = loaiSPCon.ID,
                    IDChatLieu = chatLieu.ID
                };

                await _context.SanPhams.AddAsync(sanPham);

                // Tạo biến thể sản phẩm
                if (request.IDMauSacs.Count == 0 || request.IDKichCos.Count == 0)
                {
                    Console.WriteLine("Không có màu sắc hoặc kích cỡ để tạo biến thể");
                    return false;
                }

                foreach (var idMau in request.IDMauSacs.Distinct())
                {
                    foreach (var idSize in request.IDKichCos.Distinct())
                    {
                        var mau = await _context.MauSacs.FirstOrDefaultAsync(x => x.ID == idMau);
                        var size = await _context.KichCos.FirstOrDefaultAsync(x => x.ID == idSize);

                        if (mau == null || size == null)
                        {
                            Console.WriteLine($"Không tìm thấy màu {idMau} hoặc size {idSize}");
                            continue;
                        }

                        var chiTiet = new ChiTietSanPham
                        {
                            ID = Guid.NewGuid(),
                            IDSanPham = sanPham.ID,
                            IDMauSac = idMau,
                            IDKichCo = idSize,
                            SoLuong = 0,
                            GiaBan = 0,
                            NgayTao = DateTime.Now,
                            IsDefault = false,
                            TrangThai = 0,
                            MaSPChiTiet = RemoveUnicode($"{sanPham.Ma}{mau.Ten?.Trim().ToUpper()}{size.Ten?.Trim().ToUpper()}")
                        };

                        await _context.ChiTietSanPhams.AddAsync(chiTiet);
                    }
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
                var lstChiTietSanPham = await _context.ChiTietSanPhams
                    .Where(x => x.IDSanPham == request.IDSanPham)
                    .ToListAsync();

                var lst = new List<ChiTietSanPhamRequest>();
                var mauSacCanThemAnh = new List<MauSac>();

                foreach (var idMau in request.IDMauSacs)
                {
                    foreach (var idSize in request.IDKichCos)
                    {
                        var ctsp = await CreateChiTietSanPhamFromSanPham(idMau, idSize, request.IDSanPham);
                        if (ctsp != null)
                        {
                            lst.Add(ctsp);
                        }
                    }

                    // Kiểm tra màu chưa có ảnh
                    bool hasImage = await (from a in _context.Anhs
                                           join ctsp in _context.ChiTietSanPhams on a.IDSanPhamChiTiet equals ctsp.ID
                                           where ctsp.IDSanPham == request.IDSanPham && ctsp.IDMauSac == idMau
                                           select a).AnyAsync();

                    if (!hasImage)
                    {
                        var mau = await _context.MauSacs.FirstOrDefaultAsync(x => x.ID == idMau);
                        if (mau != null)
                        {
                            mauSacCanThemAnh.Add(mau);
                        }
                    }
                }

                var maSanPham = await _context.SanPhams
                    .Where(x => x.ID == request.IDSanPham)
                    .Select(x => x.Ma)
                    .FirstOrDefaultAsync();

                return new ChiTietSanPhamUpdateRequest
                {
                    IDSanPham = request.IDSanPham,
                    ChiTietSanPhams = lst
                        .GroupBy(x => new { x.IDMauSac, x.IDKichCo })
                        .Select(y => y.First())
                        .ToList(),
                    Location = 1,
                    MauSacs = mauSacCanThemAnh,
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
            try
            {
                var sanPham = await _context.SanPhams.FindAsync(idSanPham);
                if (sanPham == null) return new ChiTietSanPhamViewModelHome();

                var lstChiTietSanPham = await _context.ChiTietSanPhams
                    .Where(x => x.IDSanPham == idSanPham)
                    .ToListAsync();

                var mauSacIds = lstChiTietSanPham.Select(x => x.IDMauSac).Distinct().ToList();
                var kichCoIds = lstChiTietSanPham.Select(x => x.IDKichCo).Distinct().ToList();

                var mauSacs = await _context.MauSacs.Where(x => mauSacIds.Contains(x.ID)).ToListAsync();
                var kichCos = await _context.KichCos.Where(x => kichCoIds.Contains(x.ID)).ToListAsync();

                var anhList = await (from a in _context.Anhs
                                     join ctsp in _context.ChiTietSanPhams on a.IDSanPhamChiTiet equals ctsp.ID
                                     where ctsp.IDSanPham == idSanPham
                                     select a.DuongDan).ToListAsync();

                var chiTietSanPham = new ChiTietSanPhamViewModelHome
                {
                    IDSanPham = idSanPham,
                    Ten = sanPham.Ten,
                    MoTa = sanPham.MoTa,
                    Anhs = anhList.Select(x => new AnhRequest { DuongDan = x }).ToList(),
                    MauSacs = mauSacs.Select(x => new GiaTriViewModel { ID = x.ID, GiaTri = x.Ma }).ToList(),
                    KichCos = kichCos.OrderByDescending(x => x.Ten).Select(x => new GiaTriViewModel { ID = x.ID, GiaTri = x.Ten }).ToList(),
                    ChiTietSanPhams = new List<ChiTietSanPhamViewModel>()
                };

                var khuyenMais = await _context.KhuyenMais.Where(x => x.NgayKetThuc > DateTime.Now).ToListAsync();

                var idsXoaKM = new List<Guid>();

                foreach (var item in lstChiTietSanPham)
                {
                    var km = item.IDKhuyenMai != null ? khuyenMais.FirstOrDefault(x => x.ID == item.IDKhuyenMai) : null;

                    decimal giaBan = item.GiaBan;
                    int? trangThaiKM = null;
                    decimal? giaTriKM = null;

                    if (km != null)
                    {
                        giaBan = GetKhuyenMai(km.GiaTri, item.GiaBan, km.TrangThai);
                        trangThaiKM = km.TrangThai;
                        giaTriKM = km.GiaTri;
                    }
                    else if (item.IDKhuyenMai != null)
                    {
                        idsXoaKM.Add(item.ID);
                    }

                    chiTietSanPham.ChiTietSanPhams.Add(new ChiTietSanPhamViewModel
                    {
                        ID = item.ID,
                        MaCTSP = item.MaSPChiTiet,
                        Ten = sanPham.Ten,
                        SoLuong = item.SoLuong,
                        GiaBan = giaBan,
                        GiaGoc = item.GiaBan,
                        MauSac = item.IDMauSac.ToString(),
                        KichCo = item.IDKichCo.ToString(),
                        TrangThai = item.TrangThai,
                        TrangThaiKM = trangThaiKM,
                        GiaTriKM = giaTriKM
                    });
                }

                // Xoá khuyến mãi không hợp lệ (nếu có)
                if (idsXoaKM.Any())
                {
                    await DeleteKhuyenMaisAsync(idsXoaKM); // gọi 1 lần duy nhất
                }

                var danhGia = await (from sp in _context.SanPhams.Where(p => p.ID == idSanPham)
                                     join ctsp in _context.ChiTietSanPhams on sp.ID equals ctsp.IDSanPham
                                     join cthd in _context.ChiTietHoaDons on ctsp.ID equals cthd.IDCTSP
                                     join dg in _context.DanhGias.Where(p => p.TrangThai == 1) on cthd.ID equals dg.ID
                                     select dg.Sao).ToListAsync();

                danhGia = danhGia.Where(x => x.HasValue).ToList();

                if (danhGia.Any())
                {
                    chiTietSanPham.SoSao = (float)danhGia.Sum(x => x.Value) / danhGia.Count();
                    chiTietSanPham.sosaoPercent = Convert.ToInt32((chiTietSanPham.SoSao / 5f) * 100);
                    chiTietSanPham.SoDanhGia = danhGia.Count();
                }

                chiTietSanPham.LSTSPTuongTu = await (from sp in _context.SanPhams
                                                     where sp.IDLoaiSP == sanPham.IDLoaiSP && sp.ID != idSanPham
                                                     join ctsp in _context.ChiTietSanPhams on sp.ID equals ctsp.IDSanPham
                                                     where ctsp.TrangThai == 1
                                                     select new SanPhamTuongTuViewModel
                                                     {
                                                         IDSP = sp.ID,
                                                         TenSP = sp.Ten,
                                                         GiaSPTT = ctsp.GiaBan,
                                                         DuongDanSPTT = _context.Anhs.FirstOrDefault(x => x.IDSanPhamChiTiet == ctsp.ID).DuongDan,
                                                     }).Take(5).ToListAsync();

                return chiTietSanPham;
            }
            catch
            {
                return new ChiTietSanPhamViewModelHome();
            }
        }
        public Task<bool> UpdateChiTietSanPham(ChiTietSanPham chiTietSanPham)
        {
            throw new NotImplementedException();
        }
        public async Task<ChiTietSanPhamRequest?> CreateChiTietSanPhamFromSanPham(Guid idMauSac, Guid idKichCo, Guid? idSanPham = null)
        {
            try
            {
                // Lấy thông tin màu sắc đã có sẵn
                var mauSac = await _context.MauSacs.FirstOrDefaultAsync(x => x.ID == idMauSac);
                if (mauSac == null) return null;

                // Lấy thông tin kích cỡ, nếu chưa có thì thêm
                var kichCo = await _context.KichCos.FirstOrDefaultAsync(x => x.ID == idKichCo);
                if (kichCo == null) return null;

                var chiTiet = new ChiTietSanPhamRequest()
                {
                    IDChiTietSanPham = Guid.NewGuid(),
                    IDMauSac = mauSac.ID,
                    IDKichCo = kichCo.ID,
                    MaMau = mauSac.Ma,
                    TenMauSac = mauSac.Ten,
                    TenKichCo = kichCo.Ten,
                    GiaBan = 0,
                    SoLuong = 0
                };

                return chiTiet;
            }
            catch
            {
                return null;
            }
        }
        public async Task<bool> UpdateSoluongChiTietSanPham(Guid id, int soLuong)
        {
            try
            {
                var chiTietSanPham = _context.ChiTietSanPhams.First(x => x.ID == id);
                chiTietSanPham.SoLuong = soLuong;
                _context.ChiTietSanPhams.Update(chiTietSanPham);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<decimal> UpdateGiaBanChiTietSanPham(Guid id, decimal giaBan)
        {
            try
            {
                var chiTietSanPham = _context.ChiTietSanPhams.First(x => x.ID == id);
                chiTietSanPham.GiaBan = giaBan;
                _context.ChiTietSanPhams.Update(chiTietSanPham);
                await _context.SaveChangesAsync();
                if (chiTietSanPham.IDKhuyenMai != null)
                {
                    var khuyenMai = await _context.KhuyenMais.FirstAsync(x => x.ID == chiTietSanPham.IDKhuyenMai);
                    if (khuyenMai.NgayKetThuc > DateTime.Now && khuyenMai.TrangThai == 1)
                    {
                        giaBan = GetKhuyenMai(khuyenMai.GiaTri, giaBan, khuyenMai.TrangThai);
                    }
                }
                return giaBan;
            }
            catch
            {
                return -1;
            }
        }
        public async Task<bool> UpdateTrangThaiChiTietSanPham(Guid id, int trangThai)
        {
            try
            {
                var chiTiet = await _context.ChiTietSanPhams
                    .Include(x => x.SanPham)
                    .FirstOrDefaultAsync(x => x.ID == id);
                if (chiTiet == null) return false;

                // ❌ Không được bật nếu sản phẩm đang ngừng hoạt động
                if (trangThai == 1 && chiTiet.SanPham?.TrangThai != 1)
                {
                    return false;
                }

                // ❌ Không được tắt nếu đang là biến thể mặc định và đang hoạt động
                if (trangThai == 0 && chiTiet.IsDefault)
                {
                    return false;
                }

                // Cập nhật trạng thái
                chiTiet.TrangThai = trangThai;
                _context.ChiTietSanPhams.Update(chiTiet);

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
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
                                                   TrangThai = a.TrangThai
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
                if (chiTietSanPham.TrangThai != 1)
                {
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
                chiTietSanPham.TrangThai = 2;
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
                    .ToList();

                var result = (from ctsp in _context.ChiTietSanPhams
                              join ms in _context.MauSacs on ctsp.IDMauSac equals ms.ID
                              join a in _context.Anhs on ctsp.ID equals a.IDSanPhamChiTiet into anhGroup
                              from anh in anhGroup.DefaultIfEmpty() // LEFT JOIN
                              where ctsp.IDSanPham == idSanPham
                              select new UploadAnhViewModel
                              {
                                  IDChiTietSanPham = ctsp.ID,
                                  MaMau = ms.Ma,
                                  TenMau = ms.Ten,
                                  DuongDan = anh != null ? anh.DuongDan : ""
                              }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("LỖI: " + ex.Message); // Hoặc log ra file/log system nếu có
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
        public async Task<bool> UpdateImage(Anh anh)
        {
            try
            {
                var temp = await _context.Anhs.FirstOrDefaultAsync(x => x.ID == anh.ID);
                if (temp == null) return false;
                temp.DuongDan = anh.DuongDan;
                _context.Anhs.Update(temp);
                await _context.SaveChangesAsync();
                return true;

            }
            catch { return false; }
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
        public async Task<List<LoaiSP>> GetAllLoaiSPCha()
        {
            return await _context.LoaiSPs.Where(x => x.IDLoaiSPCha == null && x.TrangThai == 1).ToListAsync();
        }
        public async Task<List<LoaiSP>?> GetAllLoaiSPCon(Guid idLoaiSPCha)
        {
            return await _context.LoaiSPs
                .Where(x => x.IDLoaiSPCha == idLoaiSPCha && x.TrangThai == 1)
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

        public Task<List<ChiTietSanPham>> GetAllChiTietSanPham(Guid idSanPham)
        {
            throw new NotImplementedException();
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
            return await (from ctsp in _context.ChiTietSanPhams
                          join ms in _context.MauSacs on ctsp.IDMauSac equals ms.ID
                          join kc in _context.KichCos on ctsp.IDKichCo equals kc.ID
                          join sp in _context.SanPhams on ctsp.IDSanPham equals sp.ID
                          join km in _context.KhuyenMais.Where(c => c.NgayKetThuc > DateTime.Now && c.TrangThai != 2) on ctsp.IDKhuyenMai equals km.ID
                          into kmGroup
                          from km in kmGroup.DefaultIfEmpty()
                          where ctsp.IDSanPham == idsp && ctsp.TrangThai != 0
                          select new ChiTietCTSPBanHang()
                          {
                              Id = ctsp.ID,
                              Ten = sp.Ten,
                              ChiTiet = ms.Ten + " - " + kc.Ten,
                              idMauSac = ctsp.IDMauSac,
                              idKichCo = ctsp.IDKichCo,
                              SoLuong = ctsp.SoLuong,
                              Anh = sp.AnhDaiDien,
                              GiaGoc = ctsp.GiaBan,
                              GiaBan = km == null ? ctsp.GiaBan :
                    (km.TrangThai == 1 ? (int)(ctsp.GiaBan / 100 * (100 - km.GiaTri)) :
                    (km.GiaTri < ctsp.GiaBan ? (ctsp.GiaBan - (int)km.GiaTri) : 0)),
                          }).OrderByDescending(c => c.ChiTiet).ToListAsync();
        }

        public Guid GetIDsanPhamByIdCTSP(Guid idctsp)
        {
            var ctsp = _context.ChiTietSanPhams.FirstOrDefault(p => p.ID == idctsp);
            return ctsp.IDSanPham;
        }

        public async Task<List<HomeProductViewModel>> GetAllSanPhamTrangChu()
        {
            var result = await (from sp in _context.SanPhams.AsNoTracking().Where(c => c.TrangThai != 0)
                                join ctsp in _context.ChiTietSanPhams.Where(c => c.TrangThai == 1) on sp.ID equals ctsp.IDSanPham into ctspGroup
                                from ctsp in ctspGroup.DefaultIfEmpty()

                                join km in _context.KhuyenMais.Where(c => c.NgayKetThuc > DateTime.Now && c.TrangThai != 2) on ctsp.IDKhuyenMai equals km.ID into kmGroup
                                from km in kmGroup.DefaultIfEmpty()
                                select new HomeProductViewModel()
                                {
                                    Id = sp.ID,
                                    Ten = sp.Ten,
                                    IdCTSP = ctsp == null ? null : ctsp.ID,
                                    Anh = sp.AnhDaiDien,
                                    SLBan = (from hd in _context.HoaDons.AsNoTracking().Where(c => c.TrangThaiGiaoHang == 6 && c.LoaiHoaDon == 0)
                                             join cthd in _context.ChiTietHoaDons.AsNoTracking()
                                             on hd.ID equals cthd.IDHoaDon
                                             join ctsp in _context.ChiTietSanPhams.AsNoTracking()
                                             on cthd.IDCTSP equals ctsp.ID
                                             into ctspGroup
                                             from ctsp in ctspGroup.DefaultIfEmpty()
                                             where ctsp.IDSanPham == sp.ID
                                             select cthd).AsEnumerable().ToList().Sum(c => c.SoLuong),
                                    SoSao = (from cthd in _context.ChiTietHoaDons.AsNoTracking()
                                             join ctsp in _context.ChiTietSanPhams.AsNoTracking()
                                             on cthd.IDCTSP equals ctsp.ID
                                             into ctspGroup
                                             from ctsp in ctspGroup.DefaultIfEmpty()
                                             join dg in _context.DanhGias.AsNoTracking()
                                             on cthd.ID equals dg.ID
                                             where ctsp.IDSanPham == sp.ID
                                             select dg).AsEnumerable().ToList().Average(c => c.Sao),
                                    NgayTao = ctsp == null ? null : ctsp.NgayTao,
                                    GiaGoc = ctsp == null ? 0 : ctsp.GiaBan,
                                    GiaBan = km == null ? ctsp.GiaBan :
                                    (km.TrangThai == 1 ? (int)(ctsp.GiaBan / 100 * (100 - km.GiaTri)) :
                                    (km.GiaTri < ctsp.GiaBan ? (ctsp.GiaBan - (int)km.GiaTri) : 0)),
                                    KhuyenMai = (km == null ? null : km.GiaTri)
                                }).ToListAsync();
            return result;
        }
        #endregion
    }
}
