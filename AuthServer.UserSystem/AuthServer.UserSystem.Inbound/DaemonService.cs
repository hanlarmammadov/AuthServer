using MassTransit;
using System;
using TacitusLogger;

namespace AuthServer.UserSystem.Inbound
{
    public class DaemonService
    {
        private readonly IBusControl _busControl;
        private readonly ILogger _logger;

        public DaemonService(IBusControl busControl, ILogger logger)
        {
            _busControl = busControl ?? throw new ArgumentNullException("busControl");
            _logger = logger ?? throw new ArgumentNullException("logger");
        }

        public bool Start()
        {
            _busControl.Start();
            _logger.Info("Daemon service started successfully").Log();
            return true;
        }
        public bool Stop()
        {
            _busControl.Stop();
            _logger.Info("Daemon service stopped successfully").Log();
            return true;
        }
    }
}