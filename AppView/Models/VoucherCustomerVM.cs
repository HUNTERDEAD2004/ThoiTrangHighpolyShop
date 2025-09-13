using AppData.ViewModels;
using System.Collections.Generic;

namespace AppView.Models
{
    public class VoucherCustomerVM
    {
        public VoucherView VoucherForm { get; set; } = new VoucherView();
        public IEnumerable<KhachHangViewModel> Customers { get; set; } = new List<KhachHangViewModel>();
        public List<Guid> SelectedCustomerIds { get; set; } = new List<Guid>();
    }
}


