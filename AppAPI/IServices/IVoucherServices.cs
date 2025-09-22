using AppData.Models;
using AppData.ViewModels;

namespace AppAPI.IServices
{
    public interface IVoucherServices
    {
        public bool Add(VoucherView voucherview);
        public bool Update(Guid id, VoucherView voucherview);
        public bool Delete(Guid Id);
        public Voucher GetById(Guid Id);
        public List<Voucher> GetAll();
        public Voucher? GetVoucherByMa(string ma);
        public List<Voucher> GetAllVoucherByTien(decimal tongTien, Guid? userId = null);
        bool UpdateTrangThai(Guid id, int trangThaiMoi);
    }
}
