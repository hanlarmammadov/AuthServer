using AuthServer.Mailing.Sender.Models;

namespace AuthServer.Mailing.Infrastructure
{
    public interface IMailModelFactory<TModel>
    {
        MailModel Create(TModel model);
    }
}
