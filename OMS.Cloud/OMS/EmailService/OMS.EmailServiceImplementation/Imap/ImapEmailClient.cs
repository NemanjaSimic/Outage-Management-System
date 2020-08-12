using Common.PubSubContracts.DataContracts.EMAIL;
using ImapX;
using ImapX.Enums;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using OMS.CallTrackingServiceImplementation.Interfaces;
using OMS.CallTrackingServiceImplementation.Models;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using OMS.Common.PubSubContracts;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS.CallTrackingServiceImplementation.Imap
{
	public class ImapEmailClient : IEmailClient
	{
		private ICloudLogger logger;
		private ICloudLogger Logger
		{
			get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
		}

		protected readonly ImapClient client;

		protected readonly IImapEmailMapper mapper;
		protected readonly IEmailParser parser;
		protected readonly IPublisherContract publisher;
		protected readonly IDispatcher dispatcher;

		protected readonly string address;
		protected readonly string password;
		protected readonly string server;
		protected readonly int port;

		public ImapEmailClient(IImapEmailMapper mapper, IEmailParser parser, IPublisherContract publisher, IDispatcher dispatcher)
		{
			address = ConfigurationManager.AppSettings["emailAddress"];
			password = ConfigurationManager.AppSettings["emailPassword"];
			server = ConfigurationManager.AppSettings["emailServer"];
			port = Int32.Parse(ConfigurationManager.AppSettings["emailPort"]);

			this.mapper = mapper;
			this.parser = parser;
			this.publisher = publisher;
			this.dispatcher = dispatcher;

			client = new ImapClient();
		}

		public bool Connect()
		{
			if (client.Connect())
			{
				if (client.Login(address, password))
				{
					return true;
				}
			}

			return false;
		}

		public IEnumerable<OutageMailMessage> GetUnreadMessages()
		{
			if (!client.IsConnected)
			{
				throw new NullReferenceException("ImapClient is a null value (not connected).");
			}

			Message[] messages = client.Folders["INBOX"].Search("UNSEEN", MessageFetchMode.Full);

			List<OutageMailMessage> outageMailMessages = new List<OutageMailMessage>();

			foreach (Message message in messages)
			{
				OutageMailMessage outageMessage = mapper.MapMail(message);

				outageMailMessages.Add(outageMessage);
				message.Seen = true;

				OutageTracingModel tracingModel = parser.Parse(outageMessage);

				if (tracingModel.IsValidReport)
				{
					dispatcher.Dispatch(tracingModel.Gid);
				}

				try
				{
					publisher.Publish(new OutageEmailPublication(Topic.OUTAGE_EMAIL, new EmailToOutageMessage(tracingModel.Gid)), "EmailService"); //TODO: SErvice defines
 				}
				catch (Exception)
				{
					Logger.LogError("[ImapEmailClient::GetUnreadMessages] Sending to PubSub Engine failed.");
					//Console.WriteLine("[ImapEmailClient::GetUnreadMessages] Sending to PubSub Engine failed.");
				}
			}
			return outageMailMessages;
		}
	}
}
