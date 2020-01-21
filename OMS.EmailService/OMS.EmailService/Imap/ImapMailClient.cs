using ImapX;
using System;
using System.Configuration;

namespace OMS.EmailService.Imap
{
    public class ImapMailClient : IEmailClient
    {
        private readonly ImapClient _client;

        private readonly string _address;
        private readonly string _password;
        private readonly string _server;
        private readonly int _port;

        public ImapMailClient()
        {
            _address = ConfigurationManager.AppSettings["emailAddress"];
            _password = ConfigurationManager.AppSettings["emailPassword"];
            _server = ConfigurationManager.AppSettings["emailServer"];
            _port = Int32.Parse(ConfigurationManager.AppSettings["emailPort"]);

            _client = new ImapClient(_server, _port, true);
        }

        public bool Connect()
        {
            if (_client.Connect())
                if (_client.Login(_address, _password))
                    return true;

            return false;
        }
    }
}
