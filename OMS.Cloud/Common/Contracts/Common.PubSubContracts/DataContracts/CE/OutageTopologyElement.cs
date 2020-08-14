using OMS.Common.PubSubContracts.Interfaces;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Common.PubSubContracts.DataContracts.CE
{
    [DataContract(IsReference = true)]
    public class OutageTopologyElement : IOutageTopologyElement
    {
        #region Fields
        private long id;
        private long firstEnd;
        private List<long> secondEnd;
        private string dmsType;
        private bool isRemote;
        private ushort distanceFromSource;
        private bool isActive;
        private bool noReclosing;
        private bool isOpen;
        #endregion

        #region Properties
        [DataMember]
        public long Id { get { return id; } set { id = value; } }
        [DataMember]
        public long FirstEnd { get { return firstEnd; } set { firstEnd = value; } }
        [DataMember]
        public List<long> SecondEnd { get { return secondEnd; } set { secondEnd = value; } }
        [DataMember]
        public string DmsType { get { return dmsType; } set { dmsType = value; } }
        [DataMember]
        public bool IsRemote { get { return isRemote; } set { isRemote = value; } }
        [DataMember]
        public bool IsActive { get { return isActive; } set { isActive = value; } }
        [DataMember]
        public ushort DistanceFromSource { get { return distanceFromSource; } set { distanceFromSource = value; } }
        [DataMember]
        public bool NoReclosing { get { return noReclosing; } set { noReclosing = value; } }
        [DataMember]
        public bool IsOpen { get { return isOpen; } set { isOpen = value; } }
        #endregion

        public OutageTopologyElement(long gid)
        {
            this.Id = gid;
            this.SecondEnd = new List<long>();
        }
    }
}
