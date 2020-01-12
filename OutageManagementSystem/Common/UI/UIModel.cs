using Outage.Common;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Outage.Common.UI
{
	[Serializable]
	[DataContract]
	public class UIModel
	{
		private ILogger logger = LoggerWrapper.Instance;

		[DataMember]
		public long FirstNode { get; set; }
        [DataMember]
        public Dictionary<long, UINode> Nodes { get; set; }
        [DataMember]
        public Dictionary<long, HashSet<long>> Relations { get; set; }

        public UIModel()
        {
            Nodes = new Dictionary<long, UINode>();
            Relations = new Dictionary<long, HashSet<long>>();
        }

		public void AddRelation(long source, long destination)
		{
			if (Relations.ContainsKey(source))
			{
				try
				{
					Relations[source].Add(destination);
				}
				catch (Exception)
				{
					string message = $"Failed to make relation. Relaton {source} - {destination} already exists.";
					logger.LogDebug(message);
				}
			}
			else
			{
				Relations.Add(source, new HashSet<long>() { destination });
			}
		}
		public void AddNode(UINode newNode)
		{
			if (!Nodes.ContainsKey(newNode.Gid))
			{
				Nodes.Add(newNode.Gid, newNode);
			}
		}
		public HashSet<long> GetRelatedElements(long sourceGid)
		{
			if (Relations.ContainsKey(sourceGid))
			{
				return Relations[sourceGid];
			}

			return null;
		}
	}
}
