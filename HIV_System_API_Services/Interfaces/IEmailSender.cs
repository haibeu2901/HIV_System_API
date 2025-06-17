using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Services.Interfaces
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string recipientEmail, string subject, string htmlContent);
    }
}
