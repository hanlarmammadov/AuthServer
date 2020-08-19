using AuthServer.Infrastructure.MailKit;
using AuthServer.Mailing.Infrastructure;
using AuthServer.Mailing.Sender.Models;
using AuthServer.UserSystem.Models; 

namespace AuthServer.Mailing.Daemon.MailModelFactories
{
    public class DirectPassRecoveryMailModelFactory : IMailModelFactory<DirectPassRecoveryMailModel>
    {
        private readonly string _templateFile;
        private readonly SmtpConfig _smtpConfig;

        public DirectPassRecoveryMailModelFactory(SmtpConfig smtpConfig, string templateFile)
        {
            this._smtpConfig = smtpConfig;
            this._templateFile = templateFile;
        }

        public MailModel Create(DirectPassRecoveryMailModel model)
        {
            //Generate mailModel
            ViewModel viewModel = new ViewModel()
                                 .Add("Username", model.Username)
                                 .Add("Password", model.PasswordPlain);
            var mailModel = new MailModel()
                                  .AddFrom(_smtpConfig.Username)
                                  .AddTo(model.Email)
                                  .AddSubject("Password recovery")
                                  .AddTemplateFile(_templateFile)
                                  .AddViewModel(viewModel);

            return mailModel;
        }
    }
}
