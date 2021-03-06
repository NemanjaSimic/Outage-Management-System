﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Outage.Common.PubSub.EmailDataContract
{
    [DataContract]
    public abstract class EmailServiceMessage: IPublishableMessage
    {
    }
    [DataContract]
    public class EmailToOutageMessage : EmailServiceMessage
    {

        public EmailToOutageMessage(long Gid)
        {
            this.Gid = Gid;
        }
        [DataMember]
        public long Gid { get; set; }
    }
}
