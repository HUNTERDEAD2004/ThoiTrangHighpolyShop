
using AppAPI.IServices;
using AppData.Models;
using AppData.ViewModels.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class DiaChiService : IDiaChiService
{
    private readonly AssignmentDBContext _dbContext;

    public DiaChiService(AssignmentDBContext dbContext)
    {
        _dbContext = dbContext;
    }



    public async Task<string?> AddDiaChiAsync(Guid khachHangId, DiaChiDTO dto)
    {
        var kh = await _dbContext.KhachHangs.FindAsync(khachHangId);
        if (kh == null) return null;

        if (dto.IsDefault)
        {
            var existing = _dbContext.DiaChis.Where(d => d.IDKhachHang == khachHangId && d.IsDefault);
            foreach (var dc in existing)
            {
                dc.IsDefault = false;
            }
        }

        var diaChi = new DiaChi
        {
            IDKhachHang = khachHangId,
            Tinh = dto.Tinh,
            Huyen = dto.Huyen,
            Xa = dto.Xa,
            DiaChiChiTiet = dto.DiaChiChiTiet, 
            IsDefault = dto.IsDefault
        };


        _dbContext.DiaChis.Add(diaChi);
        await _dbContext.SaveChangesAsync();

        return "Đã thêm địa chỉ";
    }

    public List<DiaChiDTO> GetDiaChis(Guid khachHangId)
    {
        return _dbContext.DiaChis
            .Where(x => x.IDKhachHang == khachHangId)
            .Select(x => new DiaChiDTO
            {
                IDDiaChi = x.Id,
                Tinh = x.Tinh,
                Huyen = x.Huyen,
                Xa = x.Xa,
                DiaChiChiTiet = x.DiaChiChiTiet,
                IsDefault = x.IsDefault
            }).ToList();
    }

    public async Task<string?> UpdateDiaChiAsync(Guid diaChiId, DiaChiDTO dto)
    {
        var dc = await _dbContext.DiaChis.FindAsync(diaChiId);
        if (dc == null) return null;

        dc.Tinh = dto.Tinh;
        dc.Huyen = dto.Huyen;
        dc.Xa = dto.Xa;
        dc.DiaChiChiTiet = dto.DiaChiChiTiet;
        dc.IsDefault = dto.IsDefault;

        if (dto.IsDefault)
        {
            var others = _dbContext.DiaChis
                .Where(x => x.IDKhachHang == dc.IDKhachHang && x.Id != diaChiId);

            foreach (var other in others)
            {
                other.IsDefault = false;
            }
        }

        await _dbContext.SaveChangesAsync();
        return "Đã cập nhật địa chỉ";
    }

    public async Task<bool> DeleteDiaChiAsync(Guid diaChiId)
    {
        var diaChi = await _dbContext.DiaChis.FindAsync(diaChiId);
        if (diaChi == null) return false;

        _dbContext.DiaChis.Remove(diaChi);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<DiaChiDTO?> GetDiaChiByIdAsync(Guid diaChiId)
    {
        var diaChi = await _dbContext.DiaChis.FindAsync(diaChiId);
        if (diaChi == null) return null;

        return new DiaChiDTO
        {
            IDDiaChi = diaChi.Id,
            Tinh = diaChi.Tinh,
            Huyen = diaChi.Huyen,
            Xa = diaChi.Xa,
            DiaChiChiTiet = diaChi.DiaChiChiTiet,
            IsDefault = diaChi.IsDefault
        };
    }


    public async Task<DiaChiDTO?> GetDefaultDiaChiAsync(Guid khachHangId)
    {
        var diaChi = await _dbContext.DiaChis
            .Where(x => x.IDKhachHang == khachHangId && x.IsDefault)
            .FirstOrDefaultAsync();

        if (diaChi == null) return null;

        return new DiaChiDTO
        {
            IDDiaChi = diaChi.Id,
            Tinh = diaChi.Tinh,
            Huyen = diaChi.Huyen,
            Xa = diaChi.Xa,
            DiaChiChiTiet = diaChi.DiaChiChiTiet,
            IsDefault = diaChi.IsDefault
        };
    }

    public async Task<IActionResult> SetDefaultDiaChiAsync(Guid khachHangId, Guid diaChiId)
    {
        var diaChi = await _dbContext.DiaChis
            .FirstOrDefaultAsync(x => x.IDKhachHang == khachHangId && x.Id == diaChiId);

        if (diaChi == null) return new NotFoundResult();

        // Bỏ mặc định các địa chỉ khác
        var others = _dbContext.DiaChis
            .Where(x => x.IDKhachHang == khachHangId && x.Id != diaChiId);

        foreach (var other in others)
        {
            other.IsDefault = false;
        }

        diaChi.IsDefault = true;
        await _dbContext.SaveChangesAsync();
        return new OkObjectResult("Đã cập nhật địa chỉ");
    }

}


