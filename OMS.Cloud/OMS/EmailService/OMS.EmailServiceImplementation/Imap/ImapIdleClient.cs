using Common.PubSubContracts.DataContracts.EMAIL;
using ImapX;
using OMS.CallTrackingServiceImplementation.Interfaces;
using OMS.CallTrackingServiceImplementation.Models;
using OMS.Common.Cloud;
using OMS.Common.PubSubContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS.CallTrackingServiceImplementation.Imap
{
    public class ImapIdleEmailClient : ImapEmailClient, IIdleEmailClient
    {
        public ImapIdleEmailClient(
             IImapEmailMapper mapper,
             IEmailParser parser,
             IPublisherContract publisher,
             IDispatcher dispatcher)
             : base(mapper, parser, publisher, dispatcher)
        { }

        public bool StartIdling()
        {
            Folder folder = client.Folders.Inbox;
            folder.StopIdling();
            return folder.StartIdling();
        }

        public void RegisterIdleHandler()
        {
            client.Folders.Inbox.OnNewMessagesArrived += OnMessageArrived;
        }

        public void UnregisterIdleHandler()
        {
            client.Folders.Inbox.OnNewMessagesArrived -= OnMessageArrived;
        }

        public void ReregisterIdleHandler()
        {
            UnregisterIdleHandler();
            RegisterIdleHandler();
            StartIdling();
        }

        private void OnMessageArrived(object sender, IdleEventArgs args)
        {
            foreach (var message in args.Messages)
            {
                message.Seen = true;
                OutageMailMessage outageMessage = mapper.MapMail(message);
                Console.WriteLine(outageMessage);

                OutageTracingModel tracingModel = parser.Parse(outageMessage);

                if (tracingModel.IsValidReport)
                {
                    dispatcher.Dispatch(tracingModel.Gid);
                }

                try
                {
                    publisher.Publish(
                        publication: new OutageEmailPublication(Topic.OUTAGE_EMAIL, new EmailToOutageMessage(tracingModel.Gid))
                        );
                }
                catch (Exception)
                {
                    Console.WriteLine("[ImapIdleEmailClient::OnMessageArrived] Sending to PubSub Engine failed.");
                }
            }

            ReregisterIdleHandler();
        }
    }
}
