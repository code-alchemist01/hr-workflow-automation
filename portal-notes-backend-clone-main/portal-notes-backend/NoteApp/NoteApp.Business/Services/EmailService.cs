// NoteApp.Business/Services/EmailService.cs

using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System.Threading.Tasks;

// Arayüzü (Interface) yeni metodumuzu içerecek şekilde güncelliyoruz.
public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string toEmail, string resetLink);
    Task SendPasswordResetCodeAsync(string toEmail, string code); // <-- YENİ METOT TANIMI
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // Bu metot, linkli şifre sıfırlama için burada kalabilir.
    public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
    {
        var mailSettings = _configuration.GetSection("MailSettings");
        var emailMessage = new MimeMessage();

        emailMessage.From.Add(new MailboxAddress("NoteApp Destek", mailSettings["FromEmail"]));
        emailMessage.To.Add(new MailboxAddress("", toEmail));
        emailMessage.Subject = "NoteApp Şifre Sıfırlama Talebi";

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = $"<h3>Şifre Sıfırlama</h3>" +
                       $"<p>Şifrenizi sıfırlamak için aşağıdaki linke tıklayınız. Bu link 1 saat geçerlidir.</p>" +
                       $"<a href='{resetLink}' style='padding: 10px 15px; background-color: #6C63FF; color: white; text-decoration: none; border-radius: 5px;'>Şifremi Sıfırla</a>"
        };
        emailMessage.Body = bodyBuilder.ToMessageBody();

        await SendEmailAsync(emailMessage);
    }

    // --- İSTEDİĞİN YENİ METOT ---
    // Bu metot, kullanıcıya 6 haneli doğrulama kodu gönderecek.
    public async Task SendPasswordResetCodeAsync(string toEmail, string code)
    {
        var mailSettings = _configuration.GetSection("MailSettings");
        var emailMessage = new MimeMessage();

        emailMessage.From.Add(new MailboxAddress("NoteApp Destek", mailSettings["FromEmail"]));
        emailMessage.To.Add(new MailboxAddress("", toEmail));
        emailMessage.Subject = "NoteApp Şifre Sıfırlama Kodunuz";

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = $"<div style='font-family: Arial, sans-serif; text-align: center; color: #333;'>" +
                       $"<h2>Şifre Sıfırlama İsteği</h2>" +
                       $"<p>Şifrenizi sıfırlamak için aşağıda verilen 6 haneli doğrulama kodunu kullanabilirsiniz.</p>" +
                       $"<p style='font-size: 28px; font-weight: bold; letter-spacing: 8px; background-color: #f2f2f2; padding: 15px; border-radius: 5px; display: inline-block;'>{code}</p>" +
                       $"<p style='font-size: 12px; color: #888;'>Eğer bu isteği siz yapmadıysanız, bu e-postayı görmezden geliniz. Bu kod 10 dakika geçerlidir.</p>" +
                       $"</div>"
        };
        emailMessage.Body = bodyBuilder.ToMessageBody();

        await SendEmailAsync(emailMessage);
    }

    // --- YARDIMCI METOT ---
    // E-posta gönderme mantığını tek bir yerde toplamak için özel (private) bir metot.
    // Bu, kod tekrarını önler ve yönetimi kolaylaştırır.
    private async Task SendEmailAsync(MimeMessage emailMessage)
    {
        var mailSettings = _configuration.GetSection("MailSettings");

        using var client = new SmtpClient();

        await client.ConnectAsync(mailSettings["Host"], int.Parse(mailSettings["Port"]!), SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(mailSettings["Username"], mailSettings["Password"]);
        await client.SendAsync(emailMessage);
        await client.DisconnectAsync(true);
    }
}