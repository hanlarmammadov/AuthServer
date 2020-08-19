using Microsoft.Extensions.Configuration;
using System;

namespace AuthServer.Infrastructure.MailKit
{
    public class SmtpConfig
    {
        private readonly string _host;
        private readonly int _port;
        private readonly bool _ssl;
        private readonly string _username;
        private readonly string _password;

        public SmtpConfig(string host, int port, bool ssl, string username, string password)
        {
            _host = host;
            _port = port;
            _ssl = ssl;
            _username = username;
            _password = password;
        }
        public SmtpConfig(IConfigurationSection configs)
        {
            if (configs == null)
                throw new ArgumentNullException("configs");

            _host = configs["host"];
            _port = configs.GetValue<int>("port");
            _ssl = configs.GetValue<bool>("ssl");
            _username = configs["username"];
            _password = configs["password"];
        }

        public string Host => _host;
        public int Port => _port;
        public bool Ssl => _ssl;
        public string Username => _username;
        public string Password => _password;
    }
}
