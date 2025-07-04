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

        #region LoaiSP

        public async Task<bool> DeleteLoaiSP(Guid id)
        {
            var lsp = await _context.LoaiSPs.FindAsync(id);
            if (lsp == null) throw new Exception($"Không tìm thấy Loại sản phẩm: {id}");
            // Check LoaiSP đag đc sử dụng k
            if (_context.SanPhams.Any(c => c.IDLoaiSP == id)) return false;
            _context.LoaiSPs.Remove(lsp);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<LoaiSP>> GetAllLoaiSP()
        {
            return await _context.LoaiSPs.AsNoTracking().OrderByDescending(x => x.TrangThai).ToListAsync();
        }

        public async Task<LoaiSP> GetLoaiSPById(Guid id)
        {
            return await _context.LoaiSPs.FindAsync(id);
        }

        public async Task<LoaiSP> SaveLoaiSP(LoaiSPRequest lsp)
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
                Lsp.IDLoaiSPCha = lsp.IDLoaiSPCha;
                Lsp.TrangThai = 1;
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
                    IDLoaiSPCha = lsp.IDLoaiSPCha,
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

        public async Task<LoaiSP> AddSpCha(Guid idLoaiSPCha, string ten, int trangthai)
        {
            var tenChuanHoa = ChuanHoaTen(ten);
            var check = _context.LoaiSPs.AsEnumerable().FirstOrDefault(x => ChuanHoaTen(x.Ten) == tenChuanHoa && x.IDLoaiSPCha != idLoaiSPCha);

            if (check != null)
            {
                return null;
            }

            LoaiSP kc = new LoaiSP()
            {
                ID = Guid.NewGuid(),
                IDLoaiSPCha = idLoaiSPCha,
                Ten = ten.Trim(),
                TrangThai = trangthai
            };

            _context.LoaiSPs.Add(kc);
            await _context.SaveChangesAsync();
            return kc;
        }

        #endregion
    }
}
