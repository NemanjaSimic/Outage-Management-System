namespace CECommon
{
	public class Node : TopologyElement
    {
		private TopologyElement parent;
		public TopologyElement Parent { get => parent; set => parent = value; }
		public Node(long gid) : base (gid)
		{

		}


	}
}
