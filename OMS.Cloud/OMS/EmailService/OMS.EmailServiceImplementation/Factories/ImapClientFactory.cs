using OMS.CallTrackingServiceImplementation.Dispatchers;
using OMS.CallTrackingServiceImplementation.EmailParsers;
using OMS.CallTrackingServiceImplementation.Imap;
using OMS.Common.Cloud.Names;
using OMS.Common.WcfClient.PubSub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS.EmailServiceImplementation.Factories
{
    public class ImapClientFactory
    {
        public ImapEmailClient CreateClient() =>
            new ImapEmailClient(
                new ImapEmailMapper(),
                new OutageEmailParser(),
                PublisherClient.CreateClient(), 
                new GraphHubDispatcher());
    }
}
