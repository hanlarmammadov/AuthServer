using AuthServer.Infrastructure.MailKit;
using AuthServer.Mailing.Infrastructure;
using AuthServer.Mailing.Sender.Models;
using AuthServer.UserSystem.Models;

namespace AuthServer.Mailing.Daemon.MailModelFactories
{
    public class ConfirmMailModelMailModelFactory : IMailModelFactory<ConfirmMailModel>
    {
        private readonly string _templateFile;
        private readonly SmtpConfig _smtpConfig;

        public ConfirmMailModelMailModelFactory(SmtpConfig smtpConfig, string templateFile)
        {
            this._smtpConfig = smtpConfig;
            this._templateFile = templateFile;
        }

        public MailModel Create(ConfirmMailModel model)
        {
            //Generate mailModel
            ViewModel viewModel = new ViewModel()
                                  .Add("Username", model.Username)
                                  .Add("Url", model.Url);
            MailModel mailModel = new MailModel()
                                  .AddFrom(_smtpConfig.Username)
                                  .AddTo(model.Email)
                                  .AddSubject("Email confirmation")
                                  .AddTemplateFile(_templateFile)
                                  .AddViewModel(viewModel);

            return mailModel;
        }
    }
}
