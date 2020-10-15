using Kuvio.Kernel.Core.Services.Email;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kuvio.Kernel.Core.Services.Email
{
    public interface IEmailService
    {
        Task Execute(string subject, string content);
        Task Execute(string toEmailAddress, string subject, string content);
        Task Execute(List<string> toEmailAddresses, string subject, string content);
        Task Execute(string fromEmailAddress, string toEmailAddress, string subject, string content);
        Task Execute(string fromEmailAddress, List<string> toEmailAddresses, string subject, string content);
        Task Execute(List<string> toEmailAddress, string subject, string content, List<EmailAttachment> attachments);
        Task Execute(string fromEmailAddress, List<string> toEmailAddresses, string subject, string content, List<EmailAttachment> attachments);
    }
}