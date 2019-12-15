
namespace CIM.ModelCompare
{
	public class CIMEntity
	{
		private string rdfID;	
		private string mRID;

		private string source;
		private string hash;
		private int startLine;
		private int startColumn;
		private int endLine;
		private int endColumn;


		public CIMEntity()
		{
		}


		public string RdfID
		{
			get
			{
				return rdfID;
			}
			set
			{
				rdfID = value;
			}
		}

		public string MRID
		{
			get
			{
				return mRID;
			}
			set
			{
				mRID = value;
			}
		}

		public string Source
		{
			get
			{
				return source;
			}
			set
			{
				source = value;
			}
		}

		public string Hash
		{
			get
			{
				return hash;
			}
			set
			{
				hash = value;
			}
		}

		public int StartLine
		{
			get
			{
				return startLine;
			}
			set
			{
				startLine = value;
			}
		}

		public int StartColumn
		{
			get
			{
				return startColumn;
			}
			set
			{
				startColumn = value;
			}
		}

		public int EndLine
		{
			get
			{
				return endLine;
			}
			set
			{
				endLine = value;
			}
		}

		public int EndColumn
		{
			get
			{
				return endColumn;
			}
			set
			{
				endColumn = value;
			}
		}


		
	}
}
