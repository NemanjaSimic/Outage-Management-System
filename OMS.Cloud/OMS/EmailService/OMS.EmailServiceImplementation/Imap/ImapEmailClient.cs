using Common.PubSubContracts.DataContracts.EMAIL;
using ImapX;
using ImapX.Enums;
using OMS.EmailImplementation.Interfaces;
using OMS.EmailImplementation.Models;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using OMS.Common.PubSubContracts;
using System;
using System.Collections.Generic;
using System.Configuration;
using OMS.Common.Cloud.Names;

namespace OMS.EmailImplementation.Imap
{
    public class ImapEmailClient : IEmailClient
	{
		private readonly string baseLogString;

		private ICloudLogger logger;
		private ICloudLogger Logger
		{
			get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
		}

		protected readonly ImapClient client;

		protected readonly IImapEmailMapper mapper;
		protected readonly IEmailParser parser;
		protected readonly IPublisherContract publisher;

		protected readonly string address;
		protected readonly string password;
		protected readonly string server;
		protected readonly int port;

		public ImapEmailClient(IImapEmailMapper mapper, IEmailParser parser, IPublisherContract publisher)
		{
			this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
			string verboseMessage = $"{baseLogString} entering Ctor.";
			Logger.LogVerbose(verboseMessage);

			address = ConfigurationManager.AppSettings["emailAddress"];
			password = ConfigurationManager.AppSettings["emailPassword"];
			server = ConfigurationManager.AppSettings["emailServer"];
			port = Int32.Parse(ConfigurationManager.AppSettings["emailPort"]);

			this.mapper = mapper;
			this.parser = parser;
			this.publisher = publisher;

			client = new ImapClient(server, port, true);
		}

		public bool Connect()
		{
			if (!client.Connect())
			{
				return false;
			}

			if (!client.Login(address, password))
			{
				return false;
			}

			return true;
		}

		public IEnumerable<OutageMailMessage> GetUnreadMessages()
		{
			var outageMailMessages = new List<OutageMailMessage>();

			try
            {
				if (!client.IsConnected)
				{
					if(!client.Connect())
                    {
						Logger.LogError($"{baseLogString} GetUnreadMessages => client could not connect to the email server.");
						return outageMailMessages;
					}
				}

				Message[] messages = client.Folders["INBOX"].Search("UNSEEN", MessageFetchMode.Full);

				foreach (Message message in messages)
				{
					OutageMailMessage outageMessage = mapper.MapMail(message);

					outageMailMessages.Add(outageMessage);
					message.Seen = true;

					OutageTracingModel tracingModel = parser.Parse(outageMessage);

					publisher.Publish(new OutageEmailPublication(Topic.OUTAGE_EMAIL, new EmailToOutageMessage(tracingModel.Gid)), MicroserviceNames.OmsEmailService).Wait();
				}
			}
            catch (Exception e)
            {
				Logger.LogError($"{baseLogString} GetUnreadMessages => Message: {e.Message}", e);
			}
			
			return outageMailMessages;
		}
	}
}
