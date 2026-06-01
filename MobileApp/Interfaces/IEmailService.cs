using MobileApp.Models;

namespace MobileApp.Services.Interfaces;

public interface IEmailService
{
    Task SendOtpEmailAsync(string toEmail, string otp);
}
