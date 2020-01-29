using CECommon.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CECommon.Model
{
    public class BaseVoltage : IGraphElement
    {
        public long Id { get; set; }
        public float Value { get; set; }
    }
}
