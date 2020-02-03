using Outage.Common.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Outage.Common.PubSub.CalculationEngineDataContract
{
   
    //[Serializable]
    [DataContract]
    public class CalculationEnginePublication : Publication
    {
        public CalculationEnginePublication(Topic topic, CalculationEngineMessage message) : base(topic, message)
        {

        }
    }
}
