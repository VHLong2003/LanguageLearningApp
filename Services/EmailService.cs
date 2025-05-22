using MailKit.Net.Smtp;
using MimeKit;
using System.Threading.Tasks;

namespace LanguageLearningApp.Services
{
    public class EmailService
    {
        private readonly string _smtpServer = "smtp.gmail.com";
        private readonly int _smtpPort = 587;
        private readonly string _smtpUser = "oshioxi.daotoan@gmail.com";  // Sửa thành email thật
        private readonly string _smtpPass = "baiebosfydjiyjmv";     // Sửa thành mật khẩu app

        public async Task<bool> SendOtpAsync(string toEmail, string otpCode)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("LanguageLearningApp", _smtpUser));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = "Xác nhận đăng ký tài khoản - OTP";

            message.Body = new TextPart("plain")
            {
                Text = $"Mã xác nhận của bạn là: {otpCode}\nMã có hiệu lực trong 10 phút."
            };

            try
            {
                using var client = new SmtpClient();
                await client.ConnectAsync(_smtpServer, _smtpPort, false);
                await client.AuthenticateAsync(_smtpUser, _smtpPass);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
