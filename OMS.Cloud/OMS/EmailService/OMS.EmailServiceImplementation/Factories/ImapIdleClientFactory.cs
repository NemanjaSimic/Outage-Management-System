using OMS.CallTrackingServiceImplementation.Dispatchers;
using OMS.CallTrackingServiceImplementation.EmailParsers;
using OMS.CallTrackingServiceImplementation.Imap;
using OMS.Common.WcfClient.PubSub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS.EmailServiceImplementation.Factories
{
	public class ImapIdleClientFactory
	{
        public ImapIdleEmailClient CreateClient() =>
            new ImapIdleEmailClient(
                new ImapEmailMapper(),
                new OutageEmailParser(),
                PublisherClient.CreateClient(),
                new GraphHubDispatcher());
    }
}
