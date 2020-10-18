using Common.PubSubContracts.DataContracts.EMAIL;
using ImapX;
using OMS.EmailImplementation.Interfaces;
using OMS.EmailImplementation.Models;
using OMS.Common.Cloud;
using OMS.Common.PubSubContracts;
using System;
using OMS.Common.Cloud.Names;
using OMS.Common.Cloud.Logger;

namespace OMS.EmailImplementation.Imap
{
    public class ImapIdleEmailClient : ImapEmailClient, IIdleEmailClient
    {
        private readonly string baseLogString;

        private ICloudLogger logger;
        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        public ImapIdleEmailClient(
             IImapEmailMapper mapper,
             IEmailParser parser,
             IPublisherContract publisher)
             : base(mapper, parser, publisher)
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
            string verboseMessage = $"{baseLogString} entering Ctor.";
            Logger.LogVerbose(verboseMessage);
        }

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
            try
            {
                foreach (var message in args.Messages)
                {
                    message.Seen = true;
                    OutageMailMessage outageMessage = mapper.MapMail(message);
                    Logger.LogInformation($"{baseLogString} OnMessageArrived => Message: {outageMessage}");

                    OutageTracingModel tracingModel = parser.Parse(outageMessage);

                    publisher.Publish(new OutageEmailPublication(Topic.OUTAGE_EMAIL, new EmailToOutageMessage(tracingModel.Gid)), MicroserviceNames.OmsEmailService).Wait();
                }
                
                ReregisterIdleHandler();
            }
            catch (Exception e)
            {
                Logger.LogError($"{baseLogString} OnMessageArrived => Exception: {e.Message}", e);
            }
        }
    }
}
