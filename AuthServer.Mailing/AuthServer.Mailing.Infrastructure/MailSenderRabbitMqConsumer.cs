using AuthServer.Infrastructure.RabbitMq;
using AuthServer.Mailing.Sender.Managers.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System; 
using System.Threading.Tasks;
using TacitusLogger;

namespace AuthServer.Mailing.Infrastructure
{
    public class MailSenderRabbitMqConsumer<TModel> : RabbitMqConsumer<TModel> where TModel : class
    {
        private readonly IMailModelFactory<TModel> _mailModelFactory;
        private readonly IMailSender _mailSender;
        private readonly ILogger _logger;

        public MailSenderRabbitMqConsumer(IMailModelFactory<TModel> mailModelFactory,
                                          IMailSender mailSender,
                                          IConnection rabbitMqConn,
                                          RabbitMqQueueConfig queueConfig,
                                          ILogger logger)
            : base(rabbitMqConn, queueConfig, logger)
        {
            _mailModelFactory = mailModelFactory;
            _mailSender = mailSender;
            _logger = logger;
        }

        public override async Task ReceiveMessage(IModel model, TModel message, BasicDeliverEventArgs e, ILogger logger)
        { 
            try
            { 
                //Generate mailModel
                var mailModel = _mailModelFactory.Create(message);

                //Send confirmation email
                await _mailSender.SendEmail(mailModel);

                //Log this action
                _logger.LogInfo("MailSenderMassTransitConsumer.Consume",
                               $"Mail of type {model.ToString()} sent to mailbox: {mailModel.To}");
            }
            catch (Exception ex)
            {
                _logger.LogError("ConfirmMailModelConsumer.Consume", $"Some error occurred", new
                {
                    Model = model,
                    Exception = ex
                });
            }
        }
    }
}
