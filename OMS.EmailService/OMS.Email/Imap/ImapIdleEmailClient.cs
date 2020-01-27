<<<<<<< HEAD
﻿using ImapX;
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
=======
﻿namespace OMS.Email.Imap
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
>>>>>>> 2800298cec0dac58b6c9a650c22ac579428c4bc6

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
<<<<<<< HEAD
                    // notify clients about outage
                    _dispatcher.Dispatch(tracingModel.Gid);
                }

                // Todo: Send message to tracing service here 
=======
                    _dispatcher.Dispatch(tracingModel.Gid);
                }

                _publisher.Publish(
                    publication: new OutageEmailPublication(Topic.OUTAGE_EMAIL, new EmailToOutageMessage(tracingModel.Gid))
                    );
>>>>>>> 2800298cec0dac58b6c9a650c22ac579428c4bc6
            }
        }
    }
}
