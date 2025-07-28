using AppData.Models;
using AppData.ViewModels.DTO;
using Microsoft.AspNetCore.Mvc;

namespace AppAPI.IServices
{
    public interface IDiaChiService
    {
        Task<string?> AddDiaChiAsync(Guid khachHangId, DiaChiDTO dto);
        List<DiaChiDTO> GetDiaChis(Guid khachHangId);
        Task<string?> UpdateDiaChiAsync(Guid diaChiId, DiaChiDTO dto);
        Task<bool> DeleteDiaChiAsync(Guid diaChiId);
        Task<DiaChiDTO?> GetDefaultDiaChiAsync(Guid khachHangId);

        Task<IActionResult>SetDefaultDiaChiAsync(Guid khachHangId, Guid diaChiId);
        Task<DiaChiDTO?> GetDiaChiByIdAsync(Guid diaChiId);

    }
}
