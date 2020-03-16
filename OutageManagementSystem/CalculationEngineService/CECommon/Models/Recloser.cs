using CECommon.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CECommon.Models
{
    public class Recloser : TopologyElement
    {
        private readonly int maxNumberOfTries = 3;
        public Recloser(ITopologyElement element) : base(element.Id)
        {
            Id = element.Id;
            Description = element.Description;
            Mrid = element.Mrid;
            Name = element.Name;
            NominalVoltage = element.NominalVoltage;
            FirstEnd = element.FirstEnd;
            SecondEnd = element.SecondEnd;
            DmsType = element.DmsType;
            Measurements = element.Measurements;
            IsRemote = element.IsRemote;
            IsActive = element.IsActive;
            NoReclosing = element.NoReclosing;
            NumberOfTry = 0;
        }
        public int NumberOfTry { get; set; }
        public bool IsReachedMaximumOfTries()
        {
            return (NumberOfTry >= maxNumberOfTries);
        }


    }
}
