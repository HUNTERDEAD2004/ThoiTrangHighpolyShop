using AppView.Models;
using AppView.Models.Momo;

namespace AppView.IServices
{
    public interface IMomoService
    {
        Task<MomoCreatePaymentResponseModel> CreatePaymentAsync(OrderInfoModel model);

        MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection);
    }
}
