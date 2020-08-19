using AuthServer.Infrastructure.MailKit;
using AuthServer.Mailing.Infrastructure;
using AuthServer.Mailing.Sender.Models;
using AuthServer.UserSystem.Models.Events;

namespace AuthServer.Mailing.Daemon.MailModelFactories
{
    public class AccountEmailChangedEventMailModelFactory : IMailModelFactory<AccountEmailChangedEvent>
    {
        private readonly string _templateFile;
        private readonly string _emailChangeUndoUrlBase;
        private readonly SmtpConfig _smtpConfig;

        public AccountEmailChangedEventMailModelFactory(SmtpConfig smtpConfig, string templateFile, string emailChangeUndoUrlBase)
        {
            this._smtpConfig = smtpConfig;
            this._templateFile = templateFile;
            this._emailChangeUndoUrlBase = emailChangeUndoUrlBase;
        }
         
        public MailModel Create(AccountEmailChangedEvent model)
        {
            //Generate mailModel
            ViewModel viewModel = new ViewModel()
                                  .Add("AccountId", model.AccountId)
                                  .Add("Username", model.Username)
                                  .Add("NewEmail", model.NewEmail)
                                  .Add("EmailChangeUndoUrl", $"{_emailChangeUndoUrlBase}/{model.EmailChangeRecordId}");
            MailModel mailModel = new MailModel()
                                  .AddFrom(_smtpConfig.Username)
                                  .AddTo(model.OldEmail)
                                  .AddSubject("Account email changed")
                                  .AddTemplateFile(_templateFile)
                                  .AddViewModel(viewModel);
            return mailModel;
        }
    }
}
