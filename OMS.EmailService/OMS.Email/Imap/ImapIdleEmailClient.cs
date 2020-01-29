namespace OMS.Email.Imap
{
    using ImapX;
    using OMS.Email.Interfaces;
    using OMS.Email.Models;
    using Outage.Common;
    using Outage.Common.PubSub.EmailDataContract;
    using Outage.Common.ServiceContracts.PubSub;
    using System;

    public class ImapIdleEmailClient : ImapEmailClient, IIdleEmailClient
    {
        public ImapIdleEmailClient(
             IImapEmailMapper mapper,
             IEmailParser parser,
             IPublisher publisher,
             IDispatcher dispatcher)
             : base(mapper, parser, publisher, dispatcher)
        { }

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
                message.Seen = true;
                OutageMailMessage outageMessage = _mapper.MapMail(message);
                Console.WriteLine(outageMessage);

                OutageTracingModel tracingModel = _parser.Parse(outageMessage);

                if (tracingModel.IsValidReport && _dispatcher.IsConnected)
                {
                    _dispatcher.Dispatch(tracingModel.Gid);
                }

                try
                {
                    _publisher.Publish(
                        publication: new OutageEmailPublication(Topic.OUTAGE_EMAIL, new EmailToOutageMessage(tracingModel.Gid))
                        );
                }
                catch (Exception)
                {
                    Console.WriteLine("Sending to PubSub Engine failed.");
                }
            }
        }
    }
}
