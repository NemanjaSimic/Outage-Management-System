<<<<<<< HEAD
﻿using OMS.Email.Imap;

namespace OMS.Email.Factories
{
    public class ImapClientFactory
    {
        public ImapEmailClient CreateClient() => 
            new ImapEmailClient(new ImapEmailMapper());
=======
﻿namespace OMS.Email.Factories
{
    using OMS.Email.Dispatchers;
    using OMS.Email.EmailParsers;
    using OMS.Email.Imap;
    using Outage.Common;
    using Outage.Common.ServiceProxies.PubSub;

    public class ImapClientFactory
    {
        public ImapEmailClient CreateClient() =>
            new ImapEmailClient(
                new ImapEmailMapper(),
                new OutageEmailParser(),
                new PublisherProxy(EndpointNames.PublisherEndpoint),
                new GraphHubDispatcher());
>>>>>>> 2800298cec0dac58b6c9a650c22ac579428c4bc6
    }
}
