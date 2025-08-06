using AppAPI.IServices;
using AppAPI.Services;
using AppData.Models;
using AppData.ViewModels.SanPham;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppAPI.Controllers
{
    [Route("api/SanPham")]
    [ApiController]
    public class SanPhamController : ControllerBase
    {
        private readonly ISanPhamService _sanPhamServices;
        public SanPhamController(ISanPhamService sanPhamService)
        {
            this._sanPhamServices = sanPhamService;
        }
        #region SanPham
        [HttpGet("getAll")]
        public async Task<IActionResult> GetAllSanPham()
        {
            var listSP = await _sanPhamServices.GetAllSanPham();
            return Ok(listSP);
        }
        [HttpGet("GetAllSanPhamAdmin")]
        public List<SanPhamViewModelAdmin> GetAllSanPhamAdmin()
        {
            var listSP = _sanPhamServices.GetAllSanPhamAdmin();
            return listSP;
        }
        [HttpGet("GetSanPhamById")]
        public async Task<IActionResult> GetSanPhamById(Guid id)
        {
            var sanPham = await _sanPhamServices.GetSanPhamById(id);
            if (sanPham == null) return NotFound();
            return Ok(sanPham);
        }
        [HttpPut("UpdateSanPham")]
        public async Task<bool> UpdateSanPham(SanPhamUpdateRequest request)
        {
            return await _sanPhamServices.UpdateSanPham(request);
        }
        [HttpGet("getByIdLsp/{idLsp}")]
        public async Task<IActionResult> GetSanPhamByIdDanhMuc(Guid idLsp)
        {
            var sanPham = await _sanPhamServices.GetSanPhamByIdDanhMuc(idLsp);
            if (sanPham == null) return NotFound();
            return Ok(sanPham);
        }
        [HttpGet("checkTrungTen")]
        public async Task<IActionResult> CheckTrung(SanPhamRequest request)
        {
            var listSP = _sanPhamServices.CheckTrungTenSP(request);
            return Ok(listSP);
        }
        [HttpPost("timKiemNC")]
        public async Task<IActionResult> TimKiemSanPham(SanPhamTimKiemNangCao sp)
        {
            var listSP = await _sanPhamServices.TimKiemSanPham(sp);
            return Ok(listSP);
        }
        [HttpPost("AddSanPham")]
        public async Task<IActionResult> CreateSanPham(SanPhamRequest request)
        {
            if (request == null) return BadRequest(false); // Trả về false nếu request null

            var result = await _sanPhamServices.AddSanPham(request);

            if (result) return Ok(true);  // Thành công
            else return BadRequest(false); // Lỗi logic bên trong service
        }
        [HttpDelete("UpdateTrangThaiSanPham")]
        public async Task<IActionResult> UpdateTrangThaiSanPham(Guid id, int trangThai)
        {
            await _sanPhamServices.UpdateTrangThaiSanPham(id, trangThai);
            return Ok();
        }
        [HttpGet("GetAllAnhSanPhamChiTiet")]
        public List<UploadAnhViewModel> GetAllAnhSanPhamChiTiet(Guid idSanPham)
        {
            return _sanPhamServices.GetAllAnhSanPhamChiTiet(idSanPham);
        }
        #endregion

        #region ChiTietSanPham
        [HttpGet("GetChiTietSanPhamByID")]
        public async Task<IActionResult> GetChiTietSanPhamByID(Guid id)
        {
            var result = await _sanPhamServices.GetChiTietSanPhamByID(id);
            if (result == null)
                return NotFound(new { success = false, message = "Không tìm thấy chi tiết sản phẩm" });

            return Ok(result);
        }
        [HttpGet("GetAllChiTietSanPhamHome")]
        public async Task<IActionResult> GetAllChiTietSanPhamHome(string idSanPham)
        {
            var lstChiTietSanPham = await _sanPhamServices.GetAllChiTietSanPhamHome(new Guid(idSanPham));
            return Ok(lstChiTietSanPham);
        }
        [HttpGet("GetAllChiTietSanPhamAdmin")]
        public async Task<IActionResult> GetAllChiTietSanPham(Guid idSanPham)
        {
            var lstChiTietSanPham = await _sanPhamServices.GetAllChiTietSanPhamAdmin(idSanPham);
            return Ok(lstChiTietSanPham);
        }
        [HttpPost("AddChiTietSanPham")]
        public async Task<IActionResult> AddChiTietSanPham(ChiTietSanPhamAddRequest request)
        {
            if (request == null) return BadRequest();
            var response = await _sanPhamServices.AddChiTietSanPham(request);
            return Ok(response);
        }
        [HttpPut("UpdateSoluongChiTietSanPham")]
        public async Task<bool> UpdateSoluongChiTietSanPham(ChiTietSanPhamRequest request)
        {
            var response = await _sanPhamServices.UpdateSoluongChiTietSanPham(request.IDChiTietSanPham, request.SoLuong);
            return response;
        }
        [HttpPut("UpdateMacDinhChiTietSanPham/{id}")]
        public async Task<IActionResult> UpdateMacDinhChiTietSanPham(Guid id)
        {
            if (id == Guid.Empty) return BadRequest("ID không hợp lệ");

            var result = await _sanPhamServices.UpdateMacDinhChiTietSanPham(id);
            if (!result) return BadRequest("Cập nhật thất bại");

            return Ok("Cập nhật thành công");
        }
        [HttpPut("UpdateGiaGocChiTietSanPham")]
        public async Task<ActionResult<decimal>> UpdateGiaGocChiTietSanPham([FromBody] ChiTietSanPhamRequest request)
        {
            if (request.GiaGoc == null || request.IDChiTietSanPham == Guid.Empty)
                return BadRequest("Thiếu thông tin");

            var result = await _sanPhamServices.UpdateGiaGocChiTietSanPham(request.IDChiTietSanPham, request.GiaGoc);

            if (result < 0)
                return NotFound(); // hoặc return BadRequest nếu muốn

            return Ok(result); // Trả về decimal trực tiếp
        }
        [HttpGet("UpdateTrangThaiChiTietSanPham")]
        public bool UpdateTrangThaiChiTietSanPham(string id)
        {
            var response = _sanPhamServices.UpdateTrangThaiChiTietSanPham(new Guid(id)).Result;
            return response;
        }
        [HttpGet("GetAllChiTietSanPham")]
        public async Task<IActionResult> GetAllChiTietSanPham()
        {
            var sanPham = await _sanPhamServices.GetAllChiTietSanPham();
            return Ok(sanPham);
        }
        [HttpGet("GetIDsanPhamByIdCTSP")]
        public Guid GetIDsanPhamByIdCTSP(Guid idctsp)
        {
            return _sanPhamServices.GetIDsanPhamByIdCTSP(idctsp);
        }
        [HttpDelete("DeleteChiTietSanPham")]
        public async Task<bool> DeleteChiTietSanPham(Guid id)
        {
            return await  _sanPhamServices.DeleteChiTietSanPham(id);
        }
        [HttpGet("UndoChiTietSanPham")]
        public async Task<bool> UndoChiTietSanPham(Guid id)
        {
            return await _sanPhamServices.UndoChiTietSanPham(id);
        }
        [HttpPost("AddImage")]
        public async Task<bool> AddImage(List<AnhRequest> requests)
        {
            return await _sanPhamServices.AddImage(requests);
        }
        [HttpDelete("DeleteImage")]
        public async Task<bool> DeleteImage(string id)
        {
            return await _sanPhamServices.DeleteImage(new Guid(id));
        }
        #endregion

        #region LoaiSP
        [HttpGet("GetAllLoaiSPCha")]
        public async Task<IActionResult> GetAllLoaiSPCha()
        {
            var listLsp = await _sanPhamServices.GetAllLoaiSPCha();
            return Ok(listLsp);
        }
        [HttpGet("GetAllLoaiSPCon")]
        public async Task<IActionResult> GetAllLoaiSPCon(Guid idLoaiSPCha)
        {
            var listLsp = await _sanPhamServices.GetAllLoaiSPCon(idLoaiSPCha);

            if (listLsp == null || !listLsp.Any())
                return NotFound();

            return Ok(listLsp);
        }
        #endregion

        #region Other
        [HttpGet("GetAllMauSac")]
        public async Task<IActionResult> GetAllMauSac()
        {
            var lstMauSac = await _sanPhamServices.GetAllMauSac();
            return Ok(lstMauSac);
        }
        [HttpGet("GetAllKichCo")]
        public async Task<IActionResult> GetAllKichCo()
        {
            var lstKichCo = await _sanPhamServices.GetAllKichCo();
            return Ok(lstKichCo);
        }
        [HttpGet("GetAllChatLieu")]
        public async Task<IActionResult> GetAllChatLieu()
        {
            var lstChatLieu = await _sanPhamServices.GetAllChatLieu();
            return Ok(lstChatLieu);
        }
        #endregion

        #region SanPhamBanHang
        [HttpGet("getAllSPBanHang")]
        public async Task<IActionResult> GetAllSanPhamBanHang()
        {
            var listSP = await _sanPhamServices.GetAllSanPhamTaiQuay();
            return Ok(listSP);
        }
        [HttpGet("getAllSPTrangChu")]
        public async Task<IActionResult> GetAllSanPhamTrangChu()
        {
            var listSP = await _sanPhamServices.GetAllSanPhamTrangChu();
            return Ok(listSP);
        }
        [HttpGet("getChiTietSPBHById/{idsp}")]
        public async Task<IActionResult> GetChiTietSPBHById(Guid idsp)
        {
            var listSP = await _sanPhamServices.GetChiTietSPBHById(idsp);
            return Ok(listSP);
        }
        [HttpGet("getChiTietCTSPBHById/{idsp}")]
        public async Task<IActionResult> GetChiTietCTSPBHById(Guid idsp)
        {
            var listCTSP = await _sanPhamServices.GetChiTietCTSPBanHang(idsp);
            return Ok(listCTSP);
        }
        #endregion

    }
}
