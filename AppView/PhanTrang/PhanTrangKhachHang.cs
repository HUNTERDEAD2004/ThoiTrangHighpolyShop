using AppData.ViewModels;

namespace AppView.PhanTrang
{
    public class PhanTrangKhachHang
    {
        public IEnumerable<KhachHangViewModel> listkh { get; set; }= new List<KhachHangViewModel>();
        public PagingInfo PagingInfo { get; set; } = new PagingInfo();
    } 
}
