using ImapX;
using OMS.Email.Interfaces;
using OMS.Email.Models;
using System;

namespace OMS.Email.Imap
{
    public class ImapIdleEmailClient : ImapEmailClient, IIdleEmailClient
    {
        private readonly IDispatcher _dispatcher;
        private readonly IEmailParser _parser;

        public ImapIdleEmailClient(IImapEmailMapper mapper, IEmailParser parser, IDispatcher dispatcher) : base(mapper)
        {
            _dispatcher = dispatcher;
            _parser = parser;
        }

        public bool StartIdling()
        {
            Folder folder = _client.Folders["INBOX"];
            return folder.StartIdling();
        }

        public void RegisterIdleHandler()
        {
            _client.OnNewMessagesArrived += OnMessageArrived;
        }

        private void OnMessageArrived(object sender, IdleEventArgs args)
        {
            foreach (var message in args.Messages)
            {
                OutageMailMessage outageMessage = _mapper.MapMail(message);
                Console.WriteLine(outageMessage);

                OutageTracingModel tracingModel = _parser.Parse(outageMessage);

                if (tracingModel.IsValidReport)
                {
                    // notify clients about outage
                    _dispatcher.Dispatch(tracingModel.Gid);
                }

                // Todo: Send message to tracing service here 
            }
        }
    }
}
