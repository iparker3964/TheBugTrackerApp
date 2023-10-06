using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheBugTrackerApp.Models;

namespace TheBugTrackerApp.Services
{
    public class BTEmailService : IEmailSender
    {
        private readonly MailSettings _mailSettings;
        public BTEmailService(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }
        public async Task SendEmailAsync(string emailTo, string subject, string htmlMessage)
        {
            MimeMessage email = new MimeMessage();

            email.Sender = (MailboxAddress.Parse(_mailSettings.Mail));
            email.To.Add(MailboxAddress.Parse(emailTo));
            email.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlMessage
            };

            email.Body = bodyBuilder.ToMessageBody();

            try
            {
                using var smtp = new SmtpClient();
                smtp.Connect(_mailSettings.Host,_mailSettings.Port,SecureSocketOptions.StartTls);
                smtp.Authenticate(_mailSettings.Mail,_mailSettings.Password);
                
                await smtp.SendAsync(email);
                smtp.Disconnect(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"****ERROR**** Error sending email --> {ex.Message}");
                throw;
            }
        }
    }
}
