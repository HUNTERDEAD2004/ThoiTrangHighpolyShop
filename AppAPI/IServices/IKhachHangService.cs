using AppData.Models;
using AppData.ViewModels;

namespace AppAPI.IServices
{
    public interface IKhachHangService
    {
        // Thêm khách hàng mới
        Task<KhachHang> Add(KhachHangViewModel kh, bool isFromAdmin = false);

        // Lấy danh sách khách hàng với thông tin địa chỉ
        Task<List<KhachHangViewModel>> GetAll();

        // Lấy khách hàng theo ID
        KhachHang GetById(Guid id);

        // Lấy khách hàng theo số điện thoại hoặc email
        KhachHang GetBySDT(string sdt);

        // Lấy khách hàng theo email (trả về ViewModel)
        KhachHangViewModel GetKhachHangByEmail(string email);

        // Tìm kiếm khách hàng theo tên và số điện thoại
        List<KhachHang> SearchKhachHang(string ten, string sdt);

        // Cập nhật thông tin khách hàng
        bool Update(KhachHangViewModel khv);

        // Xóa khách hàng
        bool Delete(Guid id);

        // Đổi mật khẩu quên mật khẩu
        Task<bool> ChangeForgotPassword(Guid id, string newPassword);

        // Lấy danh sách hóa đơn của khách hàng
        Task<List<HoaDon>> GetAllHDKH(Guid idkh);
    }
}