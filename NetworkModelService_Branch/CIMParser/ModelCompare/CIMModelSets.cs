using System.Collections.Generic;

namespace CIM.ModelCompare
{
	public class CIMModelSets
	{
		private string modelSourcePath = string.Empty;
		private SortedList<string, CIMEntity> modelMRID = new SortedList<string, CIMEntity>();
		private SortedList<string, CIMEntity> modelRDFID = new SortedList<string, CIMEntity>();
		private SortedList<string, CIMEntity> modelRDFIDFull = new SortedList<string, CIMEntity>();
		private Dictionary<string, string> namespaces = new Dictionary<string, string>();

		public Dictionary<string, string> Namespaces
		{
			get
			{
				return namespaces;
			}
			set
			{
				namespaces = value;
			}
		}

		private string header = string.Empty;


		public string SourcePath
		{
			get
			{
				return modelSourcePath;
			}
			set
			{
				modelSourcePath = value;
			}
		}

		public SortedList<string, CIMEntity> ModelMRID
		{
			get
			{
				return modelMRID;
			}
		}

		public SortedList<string, CIMEntity> ModelRDFID
		{
			get
			{
				return modelRDFID;
			}
		}

		public SortedList<string, CIMEntity> ModelRDFIDFull
		{
			get
			{
				return modelRDFIDFull;
			}
		}

		public string Header
		{
			get
			{
				return header;
			}
			set
			{
				header = value;
			}
		}


		public void AddToModelMRID(CIMEntity entity)
		{
			if (entity != null)
			{
				if (!modelMRID.ContainsKey(entity.MRID))
				{
					modelMRID.Add(entity.MRID, entity);
				}
			}
		}

		public void AddToModelRDFID(CIMEntity entity)
		{
			if (entity != null)
			{
				if (!modelRDFID.ContainsKey(entity.RdfID))
				{
					modelRDFID.Add(entity.RdfID, entity);
				}
			}
		}

		public void AddToModelRDFIDFull(CIMEntity entity)
		{
			if(entity != null)
			{
				
				if(!modelRDFIDFull.ContainsKey(entity.RdfID))
				{
					modelRDFIDFull.Add(entity.RdfID, entity);
				}
			}
		}

		public void AddRDFNamespace(string prefixAndNamespace, string uri)
		{
			if(!string.IsNullOrEmpty(prefixAndNamespace) && !string.IsNullOrEmpty(uri))
			{
				if(!Namespaces.ContainsKey(prefixAndNamespace))
				{
					Namespaces.Add(prefixAndNamespace, uri);
				}
				else
				{
					Namespaces[prefixAndNamespace] = uri;
				}
			}
		}

	}
}
