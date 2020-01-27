<<<<<<< HEAD
﻿using OMS.Email.Dispatchers;
using OMS.Email.EmailParsers;
using OMS.Email.Imap;

namespace OMS.Email.Factories
{
=======
﻿namespace OMS.Email.Factories
{
    using OMS.Email.Imap;
    using OMS.Email.Dispatchers;
    using OMS.Email.EmailParsers;
    using Outage.Common.ServiceProxies.PubSub;
    using Outage.Common;

>>>>>>> 2800298cec0dac58b6c9a650c22ac579428c4bc6
    public class ImapIdleClientFactory
    {
        public ImapIdleEmailClient CreateClient() =>
            new ImapIdleEmailClient(
                new ImapEmailMapper(),
                new OutageEmailParser(),
<<<<<<< HEAD
                new GraphHubDispatcher()
                );
=======
                new PublisherProxy(EndpointNames.PublisherEndpoint),
                new GraphHubDispatcher());
>>>>>>> 2800298cec0dac58b6c9a650c22ac579428c4bc6
    }
}
