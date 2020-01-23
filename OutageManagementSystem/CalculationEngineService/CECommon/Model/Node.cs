namespace CECommon
{
	public class Node : TopologyElement
    {
		private long parent;
		public long Parent { get => parent; set => parent = value; }
		public Node(long gid) : base (gid)
		{

		}


	}
}
