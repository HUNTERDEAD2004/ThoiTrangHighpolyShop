using AppAPI.IServices;
using AppData.IRepositories;
using AppData.Models;
using AppData.Repositories;
using AppData.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AppAPI.Services
{
    public class NhanVienService : INhanVienService
    {
        private readonly
        AssignmentDBContext _dbContext;

        public NhanVienService()
        {
            _dbContext = new AssignmentDBContext();
        }

        public bool Delete(Guid id)
        {

            try
            {
                var nv = _dbContext.NhanViens.FirstOrDefault(nv => nv.ID == id);
                if (nv != null)
                {
                    _dbContext.NhanViens.Remove(nv);
                    _dbContext.SaveChanges();
                    return true;
                }
                return false;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public List<NhanVien> GetAll()
        {
            try
            {
                return _dbContext.NhanViens
                        .Include(u => u.VaiTro)
                        .Where(u => u.VaiTro.Ten == "Nhân viên")
                        .OrderByDescending(u => u.TrangThai)
                        .ToList();
            }
            catch (Exception)
            {

                throw;
            }
        }

        /*public bool Update(Guid id, string ten, string email, string manhanvien, DateTime ngaysinh, int gioitinh, string password, string sdt, string diachi, int trangthai, Guid idvaitro)
        {
            try
            {
                var nv = _dbContext.NhanViens.FirstOrDefault(x => x.ID == id);
                if (nv != null)
                {
                    nv.Ten = ten;
                    nv.Email = email;
                    nv.MaNhanVien=manhanvien;
                    nv.NgaySinh = ngaysinh;
                    nv.GioiTinh = gioitinh;
                    nv.PassWord = password;
                    nv.SDT = sdt;
                    nv.DiaChi = diachi;
                    nv.TrangThai = trangthai;
                    nv.GioiTinh = gioitinh;
                    nv.IDVaiTro = idvaitro;
                    _dbContext.NhanViens.Update(nv);
                    _dbContext.SaveChanges();
                    return true;
                }
                return false;

            }
            catch (Exception)
            {

                throw;
            }
        }*/

        public bool Update(Guid id, string ten, string email, string manhanvien, DateTime ngaysinh, int gioitinh, string password, string sdt, string diachi, int trangthai, Guid idvaitro)
        {
            try
            {
                var nv = _dbContext.NhanViens.FirstOrDefault(x => x.ID == id);
                if (nv != null)
                {
                    // Cập nhật các thông tin khác
                    nv.Ten = ten;
                    nv.Email = email;
                    nv.MaNhanVien = manhanvien;
                    nv.NgaySinh = ngaysinh;
                    nv.GioiTinh = gioitinh;
                    nv.PassWord = password;
                    nv.SDT = sdt;
                    nv.DiaChi = diachi;
                    nv.TrangThai = trangthai;

                    // Giữ nguyên IDVaiTro mặc định, không thay đổi
                    nv.IDVaiTro = Guid.Parse("952C1A5D-74FF-4DAF-BA88-135C5440809C");

                    // Lưu thay đổi vào DB
                    _dbContext.NhanViens.Update(nv);
                    _dbContext.SaveChanges();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi cập nhật: " + ex.Message);
                throw;
            }
        }


        private string MaHoaMatKhau(string matKhau)
        {
            // Ở đây, bạn có thể sử dụng bất kỳ phương thức mã hóa mật khẩu nào phù hợp
            // Ví dụ: sử dụng thư viện BCrypt.Net để mã hóa mật khẩu
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(matKhau);
            return hashedPassword;

            // Đây chỉ là ví dụ đơn giản, không nên sử dụng trong môi trường thực tế
            //return matKhau;
        }
        //public async Task<NhanVien> Add(string ten, string email, string manhanvien, DateTime ngaysinh, int gioitinh, string password, string sdt, string diachi, int trangthai, Guid idvaitro)
        //{
        //    try
        //    {
        //        var check = await _dbContext.NhanViens.FirstOrDefaultAsync(x => x.Email.Trim().ToUpper() == email.Trim().ToUpper() || x.SDT.Trim().ToUpper() == sdt.Trim().ToUpper());
        //        if (check != null)
        //        {
        //            return null;
        //        }
        //        var vt = _dbContext.VaiTros.FirstOrDefault(x => x.Ten == "Nhân viên");
        //        var nv = new NhanVien();
        //        nv.ID = Guid.NewGuid();
        //        nv.Ten = ten;
        //        nv.Email = email;
        //        nv.MaNhanVien = manhanvien;
        //        nv.NgaySinh = ngaysinh;
        //        nv.GioiTinh = gioitinh;
        //        nv.PassWord = MaHoaMatKhau(password);
        //        nv.SDT = sdt;
        //        nv.DiaChi = diachi;
        //        nv.TrangThai = 1;
        //        nv.IDVaiTro = vt.ID;
        //        _dbContext.NhanViens.Add(nv);
        //        _dbContext.SaveChanges();
        //        return nv;
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }
        //}
        /*public async Task<NhanVien> Add(NhanVienViewModel model)
        {
            try
            {
                // Chuẩn hóa
                string normalizedEmail = model.Email?.Trim().ToLower();
                string normalizedSDT = model.SDT?.Trim();

                // Kiểm tra tồn tại
                var check = await _dbContext.NhanViens.FirstOrDefaultAsync(x =>
                    x.Email.ToLower().Trim() == normalizedEmail || x.SDT.Trim() == normalizedSDT);

                if (check != null)
                {
                    return null; // Email hoặc SĐT đã tồn tại
                }

                // Gán IDVaiTro cố định
                Guid nhanVienRoleId = Guid.Parse("952C1A5D-74FF-4DAF-BA88-135C5440809C");

                var nv = new NhanVien
                {
                    ID = Guid.NewGuid(),
                    Ten = model.Ten, 
                    Email = model.Email.Trim(),
                    MaNhanVien = model.MaNhanVien,
                    NgaySinh = model.NgaySinh ?? DateTime.Now,
                    GioiTinh = model.GioiTinh ?? 1,
                    PassWord = MaHoaMatKhau(model.Password),
                    SDT = model.SDT.Trim(),
                    DiaChi = model.DiaChi,
                    TrangThai = model.TrangThai ?? 1,
                    IDVaiTro = nhanVienRoleId
                };

                _dbContext.NhanViens.Add(nv);
                await _dbContext.SaveChangesAsync();

                return nv;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error thêm nhân viên: " + ex.Message);
                return null;
            }
        }*/

        private async Task<string> GenerateMaNhanVienAsync()
        {
            // Lấy tất cả mã nhân viên bắt đầu bằng AD
            var maNVList = await _dbContext.NhanViens
                .Where(nv => nv.MaNhanVien.StartsWith("AD"))
                .Select(nv => nv.MaNhanVien)
                .ToListAsync();

            int maxSoThuTu = 0;

            foreach (var ma in maNVList)
            {
                // Tách phần số sau 
                if (int.TryParse(ma.Substring(2), out int soThuTu))
                {
                    if (soThuTu > maxSoThuTu)
                        maxSoThuTu = soThuTu;
                }
            }

            // Tăng số thứ tự lên 1
            int nextNumber = maxSoThuTu + 1;
            return $"AD{nextNumber}"; // luôn có 2 chữ số (AD01, AD02,...)
        }


        public async Task<NhanVien> Add(NhanVienViewModel model)
        {
            try
            {
                // Chuẩn hóa
                string normalizedEmail = model.Email?.Trim().ToLower();
                string normalizedSDT = model.SDT?.Trim();

                // Kiểm tra tồn tại
                var check = await _dbContext.NhanViens.FirstOrDefaultAsync(x =>
                    x.Email.ToLower().Trim() == normalizedEmail || x.SDT.Trim() == normalizedSDT);

                if (check != null)
                {
                    return null; // Email hoặc SĐT đã tồn tại
                }

                // Gán IDVaiTro cố định
                Guid nhanVienRoleId = Guid.Parse("952C1A5D-74FF-4DAF-BA88-135C5440809C");

                // Tự động sinh mã nhân viên nếu chưa có
                string maNhanVien = string.IsNullOrWhiteSpace(model.MaNhanVien)
                    ? await GenerateMaNhanVienAsync()
                    : model.MaNhanVien.Trim();

                var nv = new NhanVien
                {
                    ID = Guid.NewGuid(),
                    Ten = model.Ten,
                    Email = model.Email.Trim(),
                    MaNhanVien = maNhanVien,
                    NgaySinh = model.NgaySinh ?? DateTime.Now,
                    GioiTinh = model.GioiTinh ?? 1,
                    PassWord = MaHoaMatKhau(model.Password),
                    SDT = model.SDT.Trim(),
                    DiaChi = model.DiaChi,
                    TrangThai = model.TrangThai ?? 1,
                    IDVaiTro = nhanVienRoleId
                };

                _dbContext.NhanViens.Add(nv);
                await _dbContext.SaveChangesAsync();

                return nv;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error thêm nhân viên: " + ex.Message);
                return null;
            }
        }


        public NhanVien GetById(Guid id)
        {
            try
            {
                var nv = _dbContext.NhanViens.FirstOrDefault(nv => nv.ID == id);
                return nv;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
