using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace dot_authentication.Services
{
    // This class is used by the application to send email for account confirmation and password reset.
    // For more details see https://go.microsoft.com/fwlink/?LinkID=532713
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string message)
        {//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
             return Execute("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", subject, message, email);
        }
         public Task Execute(string apiKey, string subject, string message, string email)
        {
            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress("admin@mrsandwich.co.in", "admin"),
                Subject = subject,
                PlainTextContent = message,
                HtmlContent = message
            };
            msg.AddTo(new EmailAddress(email));
            var response=client.SendEmailAsync(msg);
            Console.WriteLine(response);
            return client.SendEmailAsync(msg);
        }
    }
}
