using AuthServer.Infrastructure.MailKit;
using AuthServer.Mailing.Infrastructure;
using AuthServer.Mailing.Sender.Models;
using AuthServer.UserSystem.Models.Events;

namespace AuthServer.Mailing.Daemon.MailModelFactories
{
    public class AccountEmailChangeUndoEventMailModelFactory : IMailModelFactory<AccountEmailChangeUndoEvent>
    {
        private readonly string _templateFile;
        private readonly SmtpConfig _smtpConfig;

        public AccountEmailChangeUndoEventMailModelFactory(SmtpConfig smtpConfig, string templateFile)
        {
            this._smtpConfig = smtpConfig;
            this._templateFile = templateFile;
        }

        public MailModel Create(AccountEmailChangeUndoEvent model)
        {
            //Generate mailModel
            ViewModel viewModel = new ViewModel()
                                  .Add("AccountId", model.AccountId)
                                  .Add("Username", model.Username);
            MailModel mailModel = new MailModel()
                                  .AddFrom(_smtpConfig.Username)
                                  .AddTo(model.Email)
                                  .AddSubject("Account email changed back")
                                  .AddTemplateFile(_templateFile)
                                  .AddViewModel(viewModel);
            return mailModel;
        }
    }
}
