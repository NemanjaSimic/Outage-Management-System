using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Reflection;
using FTN.Common;

namespace FTN.Services.NetworkModelService.DataModel.Core
{
	public enum TypeOfReference : short
	{
		Reference = 1,
		Target = 2,
		Both = 3,
	}

	public class IdentifiedObject
	{
		/// <summary>
		/// Model Resources Description
		/// </summary>
		private static ModelResourcesDesc resourcesDescs = new ModelResourcesDesc();

		/// <summary>
		/// Global id of the identified object (SystemId - 4 nibls, DMSType - 4 nibls, FragmentId - 8 nibls)
		/// </summary>
		private long globalId;
		
		/// <summary>
		/// Name of identified object
		/// </summary>		
		private string name = string.Empty;

		/// <summary>
		/// Mrid (source) id of identified object
		/// </summary>		
		private string mrid = string.Empty;

		/// <summary>
		/// Description of identified object
		/// </summary>		
		private string aliasName = string.Empty;
		
		/// <summary>
		/// Initializes a new instance of the IdentifiedObject class.
		/// </summary>		
		/// <param name="globalId">Global id of the entity.</param>
		public IdentifiedObject(long globalId)
		{
			this.globalId = globalId;			
		}		

		/// <summary>
		/// Gets or sets global id of the entity (identified object).
		/// </summary>			
		public long GlobalId
		{
			get
			{
				return globalId;
			}

			set
			{
				globalId = value;
			}
		}

		/// <summary>
		/// Gets or sets name of the entity (identified object).
		/// </summary>			
		public string Name
		{
			get
			{				
				return name;
			}

			set
			{			
				name = value;
			}
		}

		/// <summary>
		/// Gets or sets mrid of the entity (identified object).
		/// </summary>			
		public string Mrid
		{
			get { return mrid; }
			set { mrid = value; }
		}

		/// <summary>
		/// Gets or sets description of the entity (identified object).
		/// </summary>			
		public string AliasName
		{
			get { return aliasName; }
			set { aliasName = value; }
		}		

		public static bool operator ==(IdentifiedObject x, IdentifiedObject y)
		{
			if(Object.ReferenceEquals(x, null) && Object.ReferenceEquals(y, null))
			{
				return true;
			}
			else if((Object.ReferenceEquals(x, null) && !Object.ReferenceEquals(y, null)) || (!Object.ReferenceEquals(x, null) && Object.ReferenceEquals(y, null)))
			{
				return false;
			}
			else
			{
				return x.Equals(y);
			}
		}

		public static bool operator !=(IdentifiedObject x, IdentifiedObject y)
		{
			return !(x == y);
		}

		public override bool Equals(object x)
		{
			if(Object.ReferenceEquals(x, null))
			{
				return false;
			}
			else
			{
				IdentifiedObject io = (IdentifiedObject)x;
				return ((io.GlobalId == this.GlobalId) && (io.name == this.name) && (io.mrid == this.mrid) &&
						(io.aliasName == this.aliasName));
			}
		}
		
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		#region IAccess implementation		

		public virtual bool HasProperty(ModelCode property)
		{
			switch(property)
			{
				case ModelCode.IDOBJ_GID:				
				case ModelCode.IDOBJ_NAME:
				case ModelCode.IDOBJ_ALIASNAME:
				case ModelCode.IDOBJ_MRID:
					return true;

				default:				
					return false;
			}
		}

		public virtual void GetProperty(Property property)
		{
			switch(property.Id)
			{
				case ModelCode.IDOBJ_GID:
					property.SetValue(globalId);
					break;

				case ModelCode.IDOBJ_NAME:
					property.SetValue(name);
					break;

				case ModelCode.IDOBJ_MRID:
					property.SetValue(mrid);
					break;

                case ModelCode.IDOBJ_ALIASNAME:
                    property.SetValue(aliasName);
                    break;
			
				default:
					string message = string.Format("Unknown property id = {0} for entity (GID = 0x{1:x16}).", property.Id.ToString(), this.GlobalId);
					CommonTrace.WriteTrace(CommonTrace.TraceError, message);
					throw new Exception(message);										
			}
		}

		public virtual void SetProperty(Property property)
		{
			switch(property.Id)
			{
				case ModelCode.IDOBJ_NAME:
					name = property.AsString();					
					break;

				case ModelCode.IDOBJ_ALIASNAME:
					aliasName = property.AsString();					
					break;

				case ModelCode.IDOBJ_MRID:					
					mrid = property.AsString();
					break;				

				default:					
					string message = string.Format("Unknown property id = {0} for entity (GID = 0x{1:x16}).", property.Id.ToString(), this.GlobalId);
					CommonTrace.WriteTrace(CommonTrace.TraceError, message);
					throw new Exception(message);					
			}
		}

		#endregion IAccess implementation

		#region IReference implementation	

		public virtual bool IsReferenced
		{
			get
			{			
				return false;
			}
		}
			
		public virtual void GetReferences(Dictionary<ModelCode, List<long>> references, TypeOfReference refType)
		{
			return;
		}

		public virtual void AddReference(ModelCode referenceId, long globalId)
		{
			string message = string.Format("Can not add reference {0} to entity (GID = 0x{1:x16}).", referenceId, this.GlobalId);
			CommonTrace.WriteTrace(CommonTrace.TraceError, message);
			throw new Exception(message);						
		}

		public virtual void RemoveReference(ModelCode referenceId, long globalId)
		{
			string message = string.Format("Can not remove reference {0} from entity (GID = 0x{1:x16}).", referenceId, this.GlobalId);
			CommonTrace.WriteTrace(CommonTrace.TraceError, message);
			throw new ModelException(message);		
		}

		#endregion IReference implementation

		#region utility methods

		public void GetReferences(Dictionary<ModelCode, List<long>> references)
		{
			GetReferences(references, TypeOfReference.Target | TypeOfReference.Reference);
		}

		public ResourceDescription GetAsResourceDescription(bool onlySettableAttributes)
		{
			ResourceDescription rd = new ResourceDescription(globalId);
			List<ModelCode> props = new List<ModelCode>();

			if (onlySettableAttributes == true)
			{
				props = resourcesDescs.GetAllSettablePropertyIdsForEntityId(globalId);
			}
			else
			{
				props = resourcesDescs.GetAllPropertyIdsForEntityId(globalId);
			}

			return rd;
		}

		public ResourceDescription GetAsResourceDescription(List<ModelCode> propIds)
		{
			ResourceDescription rd = new ResourceDescription(globalId);

			for (int i = 0; i < propIds.Count; i++)
			{
				rd.AddProperty(GetProperty(propIds[i]));
			}

			return rd;
		}

		public virtual Property GetProperty(ModelCode propId)
		{
			Property property = new Property(propId);
			GetProperty(property);
			return property;
		}

		public void GetDifferentProperties(IdentifiedObject compared, out List<Property> valuesInOriginal, out List<Property> valuesInCompared)
		{
			valuesInCompared = new List<Property>();
			valuesInOriginal = new List<Property>();

			ResourceDescription rd = this.GetAsResourceDescription(false);

			if (compared != null)
			{
				ResourceDescription rdCompared = compared.GetAsResourceDescription(false);

				for (int i = 0; i < rd.Properties.Count; i++)
				{
					if (rd.Properties[i] != rdCompared.Properties[i])
					{
						valuesInOriginal.Add(rd.Properties[i]);
						valuesInCompared.Add(rdCompared.Properties[i]);
					}
				}
			}
			else
			{
				for (int i = 0; i < rd.Properties.Count; i++)
				{
					valuesInOriginal.Add(rd.Properties[i]);
				}
			}
		}	

		#endregion utility methods
	}
}
