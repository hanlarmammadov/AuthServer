using AuthServer.Infrastructure.MailKit;
using AuthServer.Mailing.Infrastructure;
using AuthServer.Mailing.Sender.Models;
using AuthServer.UserSystem.Models.Events;

namespace AuthServer.Mailing.Daemon.MailModelFactories
{
    public class PasswordChangedEventModelFactory : IMailModelFactory<PasswordChangedEvent>
    {
        private readonly string _templateFile;
        private readonly SmtpConfig _smtpConfig;

        public PasswordChangedEventModelFactory(SmtpConfig smtpConfig, string templateFile)
        {
            this._smtpConfig = smtpConfig;
            this._templateFile = templateFile;
        }

        public MailModel Create(PasswordChangedEvent model)
        {
            //Generate mailModel
            ViewModel viewModel = new ViewModel()
                                  .Add("AccountId", model.AccountId)
                                  .Add("Username", model.Username);

            MailModel mailModel = new MailModel()
                                  .AddFrom(_smtpConfig.Username)
                                  .AddTo(model.Email)
                                  .AddSubject("Account password changed")
                                  .AddTemplateFile(_templateFile)
                                  .AddViewModel(viewModel);
            return mailModel;
        }
    }
}
