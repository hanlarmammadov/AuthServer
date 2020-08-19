using AuthServer.Mailing.Sender.Managers.Interfaces;
using AuthServer.Mailing.Sender.Models;
using FluentEmail.Razor;
using FluentEmail.Smtp;
using System; 
using System.Net.Mail; 
using System.Threading.Tasks;
using TacitusLogger;

namespace AuthServer.Mailing.Sender.Managers
{
    public class SmtpMailSender : IMailSender
    {
        private readonly SmtpSender _smtpSender;
        private readonly object _smtpSettings;
        private readonly ILogger _logger;

        public SmtpMailSender(SmtpClient smtpClient, ILogger logger)
        {
            _smtpSender = new SmtpSender(smtpClient);

            _smtpSettings = new
            {
                Host = smtpClient.Host,
                Port = smtpClient.Port,
                Ssl = smtpClient.EnableSsl,
                Timeout = smtpClient.Timeout
            };

            _logger = logger;
        }

        public async Task SendEmail(MailModel mailModel)
        {
            try
            {
                var email = new FluentEmail.Core.Email(mailModel.From)
                                             .To(mailModel.To)
                                             .Subject(mailModel.Subject);
                email.Renderer = new RazorRenderer();

                if (mailModel.TemplateFile != null)
                    email.UsingTemplateFromFile<ViewModel>(mailModel.TemplateFile, mailModel.ViewModel);
                else
                    email.UsingTemplate<ViewModel>(mailModel.Template, mailModel.ViewModel);

                await _smtpSender.SendAsync(email);
            }
            catch (SmtpFailedRecipientException ex)
            {
                _logger.LogError("SmtpMailSender.SendEmail", "SmtpFailedRecipientException occurred while sending mail", new
                {
                    MailModel = mailModel,
                    SmtpSettings = _smtpSettings,
                    Exception = ex
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("SmtpMailSender.SendEmail", "Exception occurred while sending mail", new
                {
                    MailModel = mailModel,
                    SmtpSettings = _smtpSettings,
                    Exception = ex
                });

                throw;
            }
        }


    }
}
