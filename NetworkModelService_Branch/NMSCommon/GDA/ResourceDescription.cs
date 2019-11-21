using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace FTN.Common
{
	/// <summary>
	/// A class that describes generic model resource
	/// </summary>
	[Serializable]
	[DataContract]	
	public class ResourceDescription
	{
		private long id;
		private List<Property> properties = new List<Property>();

		public ResourceDescription()
		{
		}

		public ResourceDescription(long id)
		{
			this.id = id;
		}

		public ResourceDescription(long id, List<Property> properties)
		{
			this.id = id;
			foreach (Property property in properties)
			{
				this.AddProperty(property);
			}
		}

		public ResourceDescription(long id, List<ModelCode> propIds)
		{
			this.id = id;
			foreach (ModelCode propId in propIds)
			{
				this.properties.Add(new Property(propId));
			}
		}

		public ResourceDescription(ResourceDescription toCopy)
		{
			this.id = toCopy.id;
			foreach (Property property in toCopy.properties)
			{
				Property toAdd = new Property(property);
				this.AddProperty(toAdd);
			}
		}

		[DataMember]		
		public long Id
		{
			get { return id; }
			set { id = value; }
		}

		[DataMember]		
		public List<Property> Properties
		{
			get { return properties; }
			set { properties = value; }
		}

		/// <summary>
		/// Gets difference between second and first ResourceDescription.
		/// Difference == second - first
		/// </summary>
		/// <param name="resDesc1">First ResourceDescription.</param>
		/// <param name="resDesc2">Second ResourceDescription.</param>
		/// <param name="addNewProperties">Indicates whether to add new properties to difference.</param>
		/// <param name="ignoredPropertyIds">IDs of ignored properties.</param>
		/// <returns>ResourceDescription with different properties.</returns>
		public static ResourceDescription GetDifference(ResourceDescription resDesc1, ResourceDescription resDesc2, bool addNewProperties, HashSet<ModelCode> ignoredPropertyIds = null)
		{
			if (resDesc1.Id != resDesc2.Id)
			{
				throw new Exception(string.Format("Failed to generate difference between resource descriptions. IDs are different. First ID = {0}. Second ID = {1}", resDesc1.Id, resDesc2.Id));
			}
			else
			{
				ResourceDescription difference = new ResourceDescription(resDesc1.Id);

				foreach (Property secondProperty in resDesc2.Properties)
				{
					if (ignoredPropertyIds != null && ignoredPropertyIds.Contains(secondProperty.Id))
					{
						continue;
					}
					else
					{
						bool isNew = true;

						for (int i = 0; i < resDesc1.Properties.Count; i++)
						{
							if (resDesc1.Properties[i].Id == secondProperty.Id)
							{
								if (resDesc1.Properties[i] != secondProperty)
								{
									difference.AddProperty(secondProperty);
								}

								isNew = false;
							}
						}

						if (isNew == true && addNewProperties == true)
						{
							difference.AddProperty(secondProperty);
						}
					}
				}

				return difference;
			}
		}

		public void Update(ResourceDescription updateRD)
		{
			if (this.Id != updateRD.Id)
			{
				throw new Exception(string.Format("Failed to update resource description. IDs are different. Original ID = {0} . Update ID = {1}", this.Id, updateRD.Id));
			}
			else
			{
				foreach (Property updateProp in updateRD.Properties)
				{
					this.AddProperty(updateProp);
				}
			}
		}

		public void AddProperty(Property property)
		{
			bool isNew = true;

			for (int i = 0; i < this.Properties.Count; i++)
			{
				if (this.Properties[i].Id == property.Id)
				{
					this.Properties[i] = property;
					isNew = false;
				}
			}

			if (isNew == true)
			{
				this.properties.Add(property);
			}
		}

		public void AddProperty(ModelCode pid, PropertyValue value)
		{
			Property property = new Property(pid, value);
			this.AddProperty(property);
		}

		public void InsertProperty(int index, Property property)
		{
			while (index > properties.Count - 1)
			{
				properties.Add(null);
			}

			properties[index] = property;
		}

		public void ExportToXml(XmlTextWriter xmlWriter)
		{
			StringBuilder sb;

			xmlWriter.WriteStartElement("ResourceDescription");
			xmlWriter.WriteStartElement("id");
			xmlWriter.WriteStartAttribute("gid");
			xmlWriter.WriteValue(String.Format("0x{0:x16}", this.Id));
			xmlWriter.WriteStartAttribute("type");
			xmlWriter.WriteValue(((DMSType)ModelCodeHelper.ExtractTypeFromGlobalId(this.Id)).ToString());
			xmlWriter.WriteEndElement(); // id
			xmlWriter.WriteStartElement("Properties");
			for (int i = 0; i < this.Properties.Count; i++)
			{
				xmlWriter.WriteStartElement("Property");
				xmlWriter.WriteStartAttribute("id");
				xmlWriter.WriteValue(this.Properties[i].Id.ToString());
				xmlWriter.WriteStartAttribute("value");
				switch (Properties[i].Type)
				{
					case PropertyType.Float:
						xmlWriter.WriteValue(this.Properties[i].AsFloat());
						break;
					case PropertyType.Bool:
					case PropertyType.Byte:
					case PropertyType.Int32:
					case PropertyType.Int64:
					case PropertyType.TimeSpan:
					case PropertyType.DateTime:
						if (this.Properties[i].Id == ModelCode.IDOBJ_GID)
						{
							xmlWriter.WriteValue(String.Format("0x{0:x16}", this.Properties[i].AsLong()));
						}
						else
						{
							xmlWriter.WriteValue(this.Properties[i].AsLong());
						}

						break;
					case PropertyType.Enum:
						try
						{
							EnumDescs enumDescs = new EnumDescs();
							xmlWriter.WriteValue(enumDescs.GetStringFromEnum(this.Properties[i].Id, this.Properties[i].AsEnum()));
						}
						catch (Exception)
						{
							xmlWriter.WriteValue(this.Properties[i].AsEnum());
						}

						break;
					case PropertyType.Reference:
						xmlWriter.WriteValue(String.Format("0x{0:x16}", this.Properties[i].AsReference()));
						break;
					case PropertyType.String:
						if (this.Properties[i].PropertyValue.StringValue == null)
						{
							this.Properties[i].PropertyValue.StringValue = String.Empty;
						}
						xmlWriter.WriteValue(this.Properties[i].AsString());
						break;

					case PropertyType.Int64Vector:
					case PropertyType.ReferenceVector:
						if (this.Properties[i].AsLongs().Count > 0)
						{
							sb = new StringBuilder(100);
							for (int j = 0; j < this.Properties[i].AsLongs().Count; j++)
							{
								sb.Append(String.Format("0x{0:x16}", this.Properties[i].AsLongs()[j])).Append(", ");
							}

							xmlWriter.WriteValue(sb.ToString(0, sb.Length - 2));
						}
						else
						{
							xmlWriter.WriteValue("empty long/reference vector");
						}

						break;
					case PropertyType.TimeSpanVector:
						if (this.Properties[i].AsLongs().Count > 0)
						{
							sb = new StringBuilder(100);
							for (int j = 0; j < this.Properties[i].AsLongs().Count; j++)
							{
								sb.Append(String.Format("0x{0:x16}", this.Properties[i].AsTimeSpans()[j])).Append(", ");
							}

							xmlWriter.WriteValue(sb.ToString(0, sb.Length - 2));
						}
						else
						{
							xmlWriter.WriteValue("empty long/reference vector");
						}

						break;
					case PropertyType.Int32Vector:
						if (this.Properties[i].AsInts().Count > 0)
						{
							sb = new StringBuilder(100);
							for (int j = 0; j < this.Properties[i].AsInts().Count; j++)
							{
								sb.Append(String.Format("{0}", this.Properties[i].AsInts()[j])).Append(", ");
							}

							xmlWriter.WriteValue(sb.ToString(0, sb.Length - 2));
						}
						else
						{
							xmlWriter.WriteValue("empty int vector");
						}

						break;

					case PropertyType.DateTimeVector:
						if (this.Properties[i].AsDateTimes().Count > 0)
						{
							sb = new StringBuilder(100);
							for (int j = 0; j < this.Properties[i].AsDateTimes().Count; j++)
							{
								sb.Append(String.Format("{0}", this.Properties[i].AsDateTimes()[j])).Append(", ");
							}

							xmlWriter.WriteValue(sb.ToString(0, sb.Length - 2));
						}
						else
						{
							xmlWriter.WriteValue("empty DateTime vector");
						}

						break;

					case PropertyType.BoolVector:
						if (this.Properties[i].AsBools().Count > 0)
						{
							sb = new StringBuilder(100);
							for (int j = 0; j < this.Properties[i].AsBools().Count; j++)
							{
								sb.Append(String.Format("{0}", this.Properties[i].AsBools()[j])).Append(", ");
							}

							xmlWriter.WriteValue(sb.ToString(0, sb.Length - 2));
						}
						else
						{
							xmlWriter.WriteValue("empty int vector");
						}

						break;
					case PropertyType.FloatVector:
						if (this.Properties[i].AsFloats().Count > 0)
						{
							sb = new StringBuilder(100);
							for (int j = 0; j < this.Properties[i].AsFloats().Count; j++)
							{
								sb.Append(this.Properties[i].AsFloats()[j]).Append(", ");
							}

							xmlWriter.WriteValue(sb.ToString(0, sb.Length - 2));
						}
						else
						{
							xmlWriter.WriteValue("empty float vector");
						}

						break;
					case PropertyType.StringVector:
						if (this.Properties[i].AsStrings().Count > 0)
						{
							sb = new StringBuilder(100);
							for (int j = 0; j < this.Properties[i].AsStrings().Count; j++)
							{
								sb.Append(this.Properties[i].AsStrings()[j]).Append(", ");
							}

							xmlWriter.WriteValue(sb.ToString(0, sb.Length - 2));
						}
						else
						{
							xmlWriter.WriteValue("empty string vector");
						}

						break;
					case PropertyType.EnumVector:
						if (this.Properties[i].AsEnums().Count > 0)
						{
							sb = new StringBuilder(100);
							EnumDescs enumDescs = new EnumDescs();

							for (int j = 0; j < this.Properties[i].AsEnums().Count; j++)
							{
								try
								{
									sb.Append(String.Format("{0}", enumDescs.GetStringFromEnum(this.Properties[i].Id, this.Properties[i].AsEnums()[j]))).Append(", ");
								}
								catch (Exception)
								{
									sb.Append(String.Format("{0}", this.Properties[i].AsEnums()[j])).Append(", ");
								}
							}
							
							xmlWriter.WriteValue(sb.ToString(0, sb.Length - 2));
						}
						else
						{
							xmlWriter.WriteValue("empty enum vector");
						}

						break;
					
					default:
						throw new Exception("Failed to export Resource Description as XML. Invalid property type.");
				}

				xmlWriter.WriteEndElement(); // Property
			}

			xmlWriter.WriteEndElement(); // Properties
			xmlWriter.WriteEndElement(); // ResourceDescription
		}

		public bool ContainsProperty(ModelCode propertyID)
		{
			foreach (Property property in this.Properties)
			{
				if (property.Id == propertyID)
				{
					return true;
				}
			}

			return false;
		}

		public Property GetProperty(ModelCode propertyID)
		{
			foreach (Property p in this.Properties)
			{
				if (p.Id == propertyID)
				{
					return p;
				}
			}

			return null;
		}

		public Property GetProperty(long propertyID)
		{
			foreach (Property p in this.Properties)
			{
				if ((long)p.Id == propertyID)
				{
					return p;
				}
			}

			return null;
		}

		public bool UpdateProperty(Property updateProperty)
		{
			bool updated = false;

			for (int i = 0; i < this.Properties.Count; i++)
			{
				if (this.Properties[i].Id == updateProperty.Id)
				{
					this.Properties[i] = updateProperty;
					updated = true;
				}
			}

			return updated;
		}

		public bool RemoveProperty(ModelCode propertyId)
		{
			bool removed = false;

			for (int i = 0; i < this.Properties.Count; i++)
			{
				if (this.Properties[i].Id == propertyId)
				{
					this.properties.RemoveAt(i--);
					removed = true;
				}
			}

			return removed;
		}

		public bool RemoveProperties(HashSet<ModelCode> propertyIds)
		{
			bool removed = false;

			for (int i = 0; i < this.Properties.Count; i++)
			{
				if (propertyIds.Contains(this.Properties[i].Id))
				{
					this.properties.RemoveAt(i--);
					removed = true;
				}
			}

			return removed;
		}

		public class EqualityComparer : IEqualityComparer<ResourceDescription>
		{

			public bool Equals(ResourceDescription firstRD, ResourceDescription secondRD)
			{
				if((firstRD == null) || (secondRD == null))
				{
					throw new NullReferenceException();
				}

				if (ReferenceEquals(firstRD, secondRD) == true)
				{
					return true;
				}

				if (firstRD.id.Equals(secondRD.id) == false)
				{
					return false;
				}

				if ((firstRD.Properties.Count == 0) && (secondRD.Properties.Count == 0))
				{
					return true;
				}

				if (firstRD.Properties.Count.Equals(secondRD.Properties.Count) == false)
				{
					return false;
				}

				bool result = false;
				bool found = false;

				foreach (Property firstRDProperty in firstRD.Properties)
				{
					foreach (Property secondRDProperty in secondRD.Properties)
					{
						if (firstRDProperty.Id == secondRDProperty.Id)
						{
							if (firstRDProperty == secondRDProperty)
							{
								found = true;
								break;
							}
						}
					}

					if (found == false)
					{
						result = false;
						break;
					}
					else
					{
						found = false;
						result = true;
					}
				}

				return result;
			}

			public int GetHashCode(ResourceDescription rd)
			{
				int hash = rd.id.GetHashCode();

				foreach (Property p in rd.Properties)
				{
					if (p.Type.Equals(PropertyType.String) && p.PropertyValue.StringValue == null)
					{
						p.PropertyValue.StringValue = String.Empty;
					}
					hash = hash + p.Id.GetHashCode() + p.PropertyValue.GetHashCode();
				}
				return hash;
			}
		}
	}
}
