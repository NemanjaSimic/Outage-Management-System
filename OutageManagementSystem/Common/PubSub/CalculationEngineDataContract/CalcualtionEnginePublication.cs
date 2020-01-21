using Outage.Common.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Outage.Common.PubSub.CalculationEngineDataContract
{
   
    [Serializable]
    [DataContract]
    public class CalcualtionEnginePublication : Publication
    {
        public CalcualtionEnginePublication(Topic topic, CalculationEngineMessage message) : base(topic, message)
        {

        }
    }
}
