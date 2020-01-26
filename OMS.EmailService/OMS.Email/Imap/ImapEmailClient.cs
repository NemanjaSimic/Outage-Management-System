namespace OMS.Email.Imap
{
    using ImapX;
    using System;
    using ImapX.Enums;
    using OMS.Email.Models;
    using System.Configuration;
    using OMS.Email.Interfaces;
    using System.Collections.Generic;

    public class ImapEmailClient : IEmailClient
    {
        protected readonly ImapClient _client;
        protected readonly IImapEmailMapper _mapper;

        protected readonly string _address;
        protected readonly string _password;
        protected readonly string _server;
        protected readonly int _port;

        public ImapEmailClient(IImapEmailMapper mapper)
        {
            _address = ConfigurationManager.AppSettings["emailAddress"];
            _password = ConfigurationManager.AppSettings["emailPassword"];
            _server = ConfigurationManager.AppSettings["emailServer"];
            _port = Int32.Parse(ConfigurationManager.AppSettings["emailPort"]);

            _mapper = mapper;

            _client = new ImapClient(_server, _port, true);
        }

        public bool Connect()
        {
            if (_client.Connect())
                if (_client.Login(_address, _password))
                    return true;

            return false;
        }

        public IEnumerable<OutageMailMessage> GetUnreadMessages()
        {
            if (!_client.IsConnected)
                throw new NullReferenceException("ImapClient is a null value (not connected).");

            Message[] messages = _client.Folders["INBOX"].Search("UNSEEN", MessageFetchMode.Full);

            List<OutageMailMessage> outageMailMessages = new List<OutageMailMessage>();

            foreach (Message message in messages)
            {
                OutageMailMessage outageMessage = _mapper.MapMail(message);
                outageMailMessages.Add(outageMessage);
                message.Seen = true;
            }

            return outageMailMessages;
        }
    }
}
