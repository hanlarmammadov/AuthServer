using AuthServer.Infrastructure.MailKit;
using AuthServer.Mailing.Infrastructure;
using AuthServer.Mailing.Sender.Models;
using AuthServer.UserSystem.Models;

namespace AuthServer.Mailing.Daemon.MailModelFactories
{
    public class ResetLinkPasswordRecoveryMailModelFactory : IMailModelFactory<ResetLinkPasswordRecoveryMailModel>
    {
        private readonly string _templateFile;
        private readonly SmtpConfig _smtpConfig;

        public ResetLinkPasswordRecoveryMailModelFactory(SmtpConfig smtpConfig, string templateFile)
        {
            this._smtpConfig = smtpConfig;
            this._templateFile = templateFile;
        }

        public MailModel Create(ResetLinkPasswordRecoveryMailModel model)
        {
            //Generate mailModel
            ViewModel viewModel = new ViewModel()
                                  .Add("Username", model.Username)
                                  .Add("Url", model.Url);
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
