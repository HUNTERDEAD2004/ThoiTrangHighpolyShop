using System.Globalization;
using System.Text;
using AppAPI.IServices;
using AppData.Models;
using AppData.ViewModels.SanPham;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppAPI.Services
{
    public class LoaiSPService : ILoaiSPService
    {
        private readonly AssignmentDBContext _context;
        public LoaiSPService()
        {
            _context = new AssignmentDBContext();
        }

        private string ChuanHoaTen(string ten)
        {
            if (string.IsNullOrWhiteSpace(ten)) return string.Empty;
            var normalized = ten.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in normalized)
            {
                var cat = CharUnicodeInfo.GetUnicodeCategory(c);
                if (cat != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }
            return sb.ToString().Normalize(NormalizationForm.FormC).ToUpper().Trim();
        }

        #region CRUD LoaiSP

        public async Task<bool> DeleteLoaiSP(Guid id)
        {
            var loaiSP = await _context.LoaiSPs.FindAsync(id);
            if (loaiSP == null)
                throw new Exception($"Không tìm thấy loại sản phẩm với ID: {id}");

            var isUsed = await _context.SanPhams.AnyAsync(sp => sp.IDLoaiSP == id);
            if (isUsed) return false;

            _context.LoaiSPs.Remove(loaiSP);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<LoaiSP>> GetAllLoaiSP()
        {
            return await _context.LoaiSPs.AsNoTracking().OrderByDescending(x => x.TrangThai).ToListAsync();
        }

        public async Task<LoaiSP?> GetLoaiSPById(Guid id)
        {
            return await _context.LoaiSPs.FindAsync(id);
        }

        public async Task<LoaiSP?> SaveLoaiSP(LoaiSPRequest lsp)
        {

            var tenChuanHoa = ChuanHoaTen(lsp.Ten);

            var existingLoaiSP = _context.LoaiSPs
                .AsEnumerable()
                .FirstOrDefault(x => ChuanHoaTen(x.Ten) == tenChuanHoa && x.ID != lsp.ID);

            if (existingLoaiSP != null)
            {
                return null;
            }

            var Lsp = await _context.LoaiSPs.FindAsync(lsp.ID);
            if (Lsp != null)
            {
                Lsp.Ten = lsp.Ten.Trim();
                Lsp.TrangThai = lsp.TrangThai;
                _context.LoaiSPs.Update(Lsp);
                await _context.SaveChangesAsync();
                return Lsp;
            }
            else
            {
                var loaiSP = new LoaiSP()
                {
                    ID = Guid.NewGuid(),
                    Ten = lsp.Ten.Trim(),
                    TrangThai = 1,
                };
                await _context.LoaiSPs.AddAsync(loaiSP);
                await _context.SaveChangesAsync();
                return loaiSP;
            }
        }

        public bool CheckTrungLoaiSP(LoaiSPRequest lsp)
        {
            var tenChuanHoa = ChuanHoaTen(lsp.Ten);
            var existing = _context.LoaiSPs.AsEnumerable().FirstOrDefault(x => ChuanHoaTen(x.Ten) == tenChuanHoa && x.ID != lsp.ID);
            return existing == null;
        }

        public async Task<LoaiSP> AddLoaiSP( string ten, int trangThai)
        {
            var tenChuanHoa = ChuanHoaTen(ten);

            // Lấy toàn bộ để xử lý logic normalize
            var all = await _context.LoaiSPs.ToListAsync();
            if (all.Any(x => ChuanHoaTen(x.Ten) == tenChuanHoa))
                return null;

            var loaiSP = new LoaiSP
            {
                ID = Guid.NewGuid(),
                Ten = ten.Trim(),
                TrangThai = trangThai
            };

            _context.LoaiSPs.Add(loaiSP);
            await _context.SaveChangesAsync();
            return loaiSP;
        }

        #endregion
    }
}
