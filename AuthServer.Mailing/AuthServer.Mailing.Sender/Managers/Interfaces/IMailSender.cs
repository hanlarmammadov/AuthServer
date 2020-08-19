using AuthServer.Mailing.Sender.Models;
using System.Threading.Tasks;

namespace AuthServer.Mailing.Sender.Managers.Interfaces
{
    public interface IMailSender
    {
        Task SendEmail(MailModel mailModel);
    }
}
