using System.Collections.Generic;
using CIM.Model;

namespace CIM.ModelCompare
{
	class CIMEntityDifference
	{
		private string rdfId;
		private string mRID;
		////              attName                   old               new
		private SortedList<string, KeyValuePair<List<ObjectAttribute>, List<ObjectAttribute>>> attributes = new SortedList<string, KeyValuePair<List<ObjectAttribute>, List<ObjectAttribute>>>();

		public SortedList<string, KeyValuePair<List<ObjectAttribute>, List<ObjectAttribute>>> Attributes
		{
			get
			{
				return attributes;
			}
		}

		public string RdfId
		{
			get
			{ 
				return rdfId; 
			}
			set
			{
				rdfId = value;
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

		public void AddAttribute(ObjectAttribute oldAtt, ObjectAttribute newAtt)
		{
			//ako ima samo old
			if(newAtt == null)
			{
				if(oldAtt != null)
				{
					if(!attributes.ContainsKey(oldAtt.FullName))
					{
						////create new
						KeyValuePair<List<ObjectAttribute>, List<ObjectAttribute>> atts = new KeyValuePair<List<ObjectAttribute>, List<ObjectAttribute>>(new List<ObjectAttribute>(), new List<ObjectAttribute>());
						attributes.Add(oldAtt.FullName,atts);
					}
					KeyValuePair<List<ObjectAttribute>, List<ObjectAttribute>> attrs = attributes[oldAtt.FullName];
						attrs.Key.Add(oldAtt);
				}
			}
			else
			{
				//ako ima samo new
				if(oldAtt == null)
				{
					if(!attributes.ContainsKey(newAtt.FullName))
					{
						////create new
						KeyValuePair<List<ObjectAttribute>, List<ObjectAttribute>> atts = new KeyValuePair<List<ObjectAttribute>, List<ObjectAttribute>>(new List<ObjectAttribute>(),new List<ObjectAttribute>());
						attributes.Add(newAtt.FullName, atts);
					}
						KeyValuePair<List<ObjectAttribute>, List<ObjectAttribute>> attrs = attributes[newAtt.FullName];
						attrs.Value.Add(newAtt);
					
				}
				else
				{
					//ako postoje oba i imaju isti naziv
					if(string.Compare(oldAtt.FullName, newAtt.FullName) == 0)
					{
						if(attributes.ContainsKey(oldAtt.FullName))
						{
							////create new
							KeyValuePair<List<ObjectAttribute>, List<ObjectAttribute>> atts = new KeyValuePair<List<ObjectAttribute>, List<ObjectAttribute>>(new List<ObjectAttribute>(), new List<ObjectAttribute>());
							attributes.Add(oldAtt.FullName, atts);
						}
							KeyValuePair<List<ObjectAttribute>, List<ObjectAttribute>> attrs = attributes[oldAtt.FullName];
							attrs.Key.Add(oldAtt);
							attrs.Value.Add(newAtt);
					}
				}
			}
		}		

	}
}
