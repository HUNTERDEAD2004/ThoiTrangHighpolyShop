using AppData.Models;
using AppData.ViewModels;
using AppData.ViewModels.BanOffline;
using AppData.ViewModels.SanPham;
using Microsoft.EntityFrameworkCore;

namespace AppAPI.IServices
{
    public interface ISanPhamService
    {
        #region SanPham
        List<SanPhamViewModelAdmin> GetAllSanPhamAdmin();
        Task<List<SanPhamViewModel>> GetAllSanPham();
        Task<List<SanPhamViewModel>> TimKiemSanPham(SanPhamTimKiemNangCao sp);
        Task<SanPhamUpdateRequest> GetSanPhamById(Guid id);
        Task<List<SanPhamViewModel>> GetSanPhamByIdDanhMuc(Guid idloaisp);
        Task<bool> AddSanPham(SanPhamRequest request);
        Task<bool> UpdateSanPham(SanPhamUpdateRequest request);
        Task<bool> UpdateTrangThaiSanPham(Guid id, int trangThai);
        bool CheckTrungTenSP(SanPhamRequest lsp);
        public Guid GetIDsanPhamByIdCTSP(Guid idctsp);
        #endregion

        #region LoaiSanPham
        Task<List<LoaiSP>> GetAllLoaiSP();
        Task<LoaiSP> GetLoaiSPById(Guid id);
        Task<LoaiSP> SaveLoaiSP(LoaiSPRequest lsp);
        Task<bool> DeleteLoaiSP(Guid id);
        bool CheckTrungLoaiSP(LoaiSPRequest lsp);
        #endregion

        #region ChiTietSanPham
        Task<ChiTietSanPhamUpdateRequest> AddChiTietSanPham(ChiTietSanPhamAddRequest request);
        Task<ChiTietSanPhamViewModel?> GetChiTietSanPhamByID(Guid id);
        Task<ChiTietSanPhamViewModelHome> GetAllChiTietSanPhamHome(Guid idSanPham);
        Task<List<ChiTietSanPhamViewModel>> GetAllChiTietSanPham();
        Task<List<ChiTietSanPhamViewModelAdmin>> GetAllChiTietSanPhamAdmin(Guid idSanPham);
        Task<bool> DeleteChiTietSanPham(Guid id);
        Task<bool> UpdateSoluongChiTietSanPham(Guid id, int soLuong);
        Task<decimal> UpdateGiaGocChiTietSanPham(Guid id, decimal giaGoc);
        Task<bool> UpdateTrangThaiChiTietSanPham(Guid id);
        Task<bool> UpdateChiTietSanPham(ChiTietSanPham chiTietSanPham);
        Task<bool> UndoChiTietSanPham(Guid id);
        List<UploadAnhViewModel> GetAllAnhSanPhamChiTiet(Guid idSanPham);
        Task<bool> AddImage(List<AnhRequest> requests);
        Task<bool> DeleteImage(Guid id);
        Task<bool> UpdateMacDinhChiTietSanPham(Guid idChiTietSP);
        #endregion

        #region other
        Task<List<MauSac>> GetAllMauSac();
        Task<List<KichCo>> GetAllKichCo();
        Task<List<ChatLieu>> GetAllChatLieu();
        #endregion

        //Nhinh thêm
        #region SanPhamBanHang
        Task<List<HomeProductViewModel>> GetAllSanPhamTrangChu();
        Task<List<SanPhamBanHang>> GetAllSanPhamTaiQuay();
        Task<ChiTietSanPhamBanHang> GetChiTietSPBHById(Guid idsp); // Sản phẩm và list màu, list size
        Task<List<ChiTietCTSPBanHang>> GetChiTietCTSPBanHang(Guid idsp); // Chitet sp 
        #endregion
    }
}
