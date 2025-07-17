using HIV_System_API_Services.Interfaces;
using Microsoft.Extensions.Configuration;
using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HIV_System_API_Services.Implements
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _config;

        public EmailSender(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config), "Cấu hình không được để trống.");
        }

        public async Task SendEmailAsync(string recipientEmail, string subject, string htmlContent)
        {
            if (string.IsNullOrWhiteSpace(recipientEmail))
                throw new ArgumentException("Địa chỉ email người nhận không được để trống.", nameof(recipientEmail));

            // Validate email format
            string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (!Regex.IsMatch(recipientEmail, emailPattern))
                throw new ArgumentException("Địa chỉ email người nhận không hợp lệ.", nameof(recipientEmail));

            if (string.IsNullOrWhiteSpace(subject))
                throw new ArgumentException("Tiêu đề email không được để trống.", nameof(subject));

            if (string.IsNullOrWhiteSpace(htmlContent))
                throw new ArgumentException("Nội dung HTML của email không được để trống.", nameof(htmlContent));

            // Validate SMTP settings
            var smtpSettings = _config.GetSection("SmtpSettings");
            if (smtpSettings == null)
                throw new InvalidOperationException("Cấu hình SMTP không được tìm thấy.");

            var senderName = smtpSettings["SenderName"];
            var senderEmail = smtpSettings["SenderEmail"];
            var server = smtpSettings["Server"];
            var portString = smtpSettings["Port"];
            var username = smtpSettings["Username"];
            var password = smtpSettings["Password"];

            if (string.IsNullOrWhiteSpace(senderName))
                throw new InvalidOperationException("Tên người gửi SMTP không được để trống.");

            if (string.IsNullOrWhiteSpace(senderEmail) || !Regex.IsMatch(senderEmail, emailPattern))
                throw new InvalidOperationException("Địa chỉ email người gửi SMTP không hợp lệ.");

            if (string.IsNullOrWhiteSpace(server))
                throw new InvalidOperationException("Máy chủ SMTP không được để trống.");

            if (string.IsNullOrWhiteSpace(portString) || !int.TryParse(portString, out int port) || port <= 0)
                throw new InvalidOperationException("Cổng SMTP không hợp lệ.");

            if (string.IsNullOrWhiteSpace(username))
                throw new InvalidOperationException("Tên người dùng SMTP không được để trống.");

            if (string.IsNullOrWhiteSpace(password))
                throw new InvalidOperationException("Mật khẩu SMTP không được để trống.");

            try
            {
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress(senderName, senderEmail));
                email.To.Add(MailboxAddress.Parse(recipientEmail));
                email.Subject = subject;

                email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                {
                    Text = htmlContent
                };

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(server, port, MailKit.Security.SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(username, password);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
            }
            catch (SmtpCommandException ex)
            {
                throw new InvalidOperationException($"Lỗi SMTP khi gửi email tới {recipientEmail}: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Không thể gửi email tới {recipientEmail}.", ex);
            }
        }
    }
}