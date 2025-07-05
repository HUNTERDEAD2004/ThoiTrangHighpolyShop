using AppData.ViewModels.QLND;

namespace AppAPI.IServices
{
    public interface IEmailService
    {      
            Task SendForgotPasswordConfirmation(ForgotPasswordRequest forgot);
            Task SendEmailVerificationAsync(string email, string token, string displayName);   
    }
}
