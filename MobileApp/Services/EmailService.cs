// Services/EmailService.cs
using MailKit.Net.Smtp;
using MimeKit;
using MobileApp.Services.Interfaces;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendOtpEmailAsync(string toEmail, string otp)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(
            _config["EmailSettings:SenderName"]!,
            _config["EmailSettings:SenderEmail"]!
        ));
        email.To.Add(MailboxAddress.Parse(toEmail));
        email.Subject = "Mã xác nhận đặt lại mật khẩu";
        email.Body = new TextPart("html")
        {
            Text = $@"
                <div style='font-family: Arial; max-width: 400px; margin: auto;'>
                    <h2>Đặt lại mật khẩu</h2>
                    <p>Mã OTP của bạn là:</p>
                    <h1 style='letter-spacing: 8px; color: #1A1A1A;'>{otp}</h1>
                    <p style='color: #999;'>Mã có hiệu lực trong 10 phút.</p>
                </div>"
        };

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(
            _config["EmailSettings:SmtpHost"]!,
            int.Parse(_config["EmailSettings:SmtpPort"]!),
            MailKit.Security.SecureSocketOptions.StartTls
        );
        await smtp.AuthenticateAsync(
            _config["EmailSettings:SenderEmail"]!,
            _config["EmailSettings:AppPassword"]!
        );
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
}