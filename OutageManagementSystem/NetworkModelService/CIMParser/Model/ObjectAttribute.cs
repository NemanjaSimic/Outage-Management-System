using System.Collections.Generic;
using System.ComponentModel;
using CIM.Manager;

namespace CIM.Model
{
	/// <summary>
	/// ObjectAttribute class contains describing data for one attribute of CIM object.
	/// <para>
	/// <para>Two most imporatant properties which represents the data of an attribute are:</para>
	/// <para>FullName and Value.</para>
	/// </para>
	/// <para>@author: Stanislava Selena</para>
	/// </summary>
	public class ObjectAttribute
	{
		protected CIMModelContext modelContext;

		protected byte namespaceCode = 0;
		protected int attributeCode;
		protected int valueCode = 0;

		protected bool isReference;

		protected ProfileElement isInstanceOf = null;
		protected byte validationStatus = 0;
		protected List<string> validationMessages = null;


		public ObjectAttribute(CIMModelContext cimModelContext)
		{
			this.modelContext = cimModelContext;
			attributeCode = -1;
		}

		public ObjectAttribute(CIMModelContext cimModelContext, string attributeFullName)
		{
			this.modelContext = cimModelContext;
			attributeCode = -1;
			if (this.modelContext != null)
			{
				attributeCode = this.modelContext.AddOrReadCodeForAttribute(attributeFullName);
			}
		}


		#region Properties
		/// <summary>
		/// Gets and sets the fully qualified name of object attribute.
		/// </summary>
		public string FullName
		{
			get
			{
				if (modelContext != null)
				{
					return modelContext.ReadAttributeWithCode(attributeCode);
				}
				return string.Empty;
			}
			set
			{
				if (modelContext != null)
				{
					attributeCode = modelContext.AddOrReadCodeForAttribute(value);
				}
			}
		}

		/// <summary>
		/// Gets the attribute code.
		/// </summary>
		public int Code
		{
			get
			{
				return attributeCode;
			}
		}

		/// <summary>
		/// Gets and sets the namespace qualyfier of object attribute.
		/// </summary>
		public string NamespaceString
		{
			set
			{
				if (modelContext != null)
				{
					namespaceCode = modelContext.AddOrReadCodeForNamespace(value);
				}
			}
			get
			{
				if (modelContext != null)
				{
					return modelContext.ReadNamespaceWithCode(namespaceCode);
				}
				return CIMConstants.CIMNamespace;
			}
		}

		/// <summary>
		/// Gets the namespace qualyfier of CIM attribute with ":" at the end when namespace is not empty.
		/// </summary>
		[BrowsableAttribute(false)]
		public string NamespacePrintString
		{
			get
			{
				if (!string.IsNullOrEmpty(NamespaceString))
				{
					return string.Format("{0}{1}", NamespaceString, StringManipulationManager.SeparatorColon);
				}
				else
				{
					return NamespaceString;
				}
			}
		}

		/// <summary>
		/// Gets the short name of object attribute.
		/// </summary>
		public string ShortName
		{
			get
			{
				return StringManipulationManager.ExtractShortestName(FullName, StringManipulationManager.SeparatorDot);
			}
		}

		/// <summary>
		/// Gets and sets the value of object attribute in string format.
		/// </summary>
		public string Value
		{
			get
			{
				if (modelContext != null)
				{
					return modelContext.ReadValueWithCode(valueCode);
				}
				return string.Empty;
			}
			set
			{
				if (modelContext != null)
				{
					valueCode = modelContext.AddOrReadCodeForValue(value);
				}
			}

		}

		/// <summary>
		/// Get and sets the indicator of whether or not the value of this 
		/// attribute is reference to another CIM object.
		/// </summary>
		public bool IsReference
		{
			get
			{
				return isReference;
			}
			set
			{
				isReference = value;
			}
		}

		/// <summary>
		/// Gets the clean reference to CIMObject.
		/// <para>Only if Value property isn't empty and IsReference property is true, 
		/// this property gets the clean reference extracted from Value (without the "#" on begining).</para>
		/// <para>Otherwise, it returns null.</para>
		/// <remarks>The value of this property is ID of some CIMObject in CIMModel, 
		/// or may be an URI of enumeration value in coresponding CIM profile.</remarks>
		/// </summary>
		public string ValueOfReference
		{
			get
			{
				string valueOfReference = null;
				if (isReference && !string.IsNullOrEmpty(this.Value))
				{
					if (this.Value.StartsWith(StringManipulationManager.SeparatorSharp))
					{
						valueOfReference = StringManipulationManager.ExtractShortestName(Value, StringManipulationManager.SeparatorSharp);
					}
					else
					{
						valueOfReference = Value;
					}
				}
				return valueOfReference;
			}
		}

		/// <summary>
		/// Gets the indication of whether or not the value of this attribute is an empty string.
		/// </summary>
		public bool IsEmpty
		{
			get
			{
				return ((isReference && string.IsNullOrEmpty(ValueOfReference))
						|| (!isReference && string.IsNullOrEmpty(Value)));
			}
		}

		/// <summary>
		/// Gets the ProfileElement of which this ObjectAttribute is instance.
		/// <para>Can be null if validation to profile wasn't called or it this attribute is not supported by curren profile.</para>
		/// </summary>
		public ProfileElement IsInstanceOf
		{
			get
			{
				return isInstanceOf;
			}
			set
			{
				isInstanceOf = value;
			}
		}

		/// <summary>
		/// Gets and sets the validation status, where:
		/// <para> 0 - validation process was not jet performed</para>
		/// <para> 1 - object's attribute is fully valid</para>
		/// <para> 2 - object's attribute is not valid</para>
		/// </summary>
		public byte ValidationStatus
		{
			get
			{
				return validationStatus;
			}
			set
			{
				validationStatus = value;
			}
		}

		/// <summary>
		/// Gets the list of identifiers of validation messages attached to this attribute
		/// during validation process.
		/// </summary>
		public List<string> ValidationMessages
		{
			get
			{
				return validationMessages;
			}
		}
		#endregion Properties


		/// <summary>
		/// Method creates ObjectAttribute instance with following property values:
		/// <para>FullName = type + ".Member_Of_" + embeddedCategory</para>
		/// <para>IsReference = true</para>
		/// <para>Value = parentId</para>
		/// <para>This ObjectAttribute represents artificial attribute for representing
		/// info about parent inside of which some CIMObject is embedded.</para>
		/// </summary>
		/// <param name="cimModelContext">model context</param>
		/// <param name="type">CIM type of embedded child object</param>
		/// <param name="embeddedCategory">name of embedded category</param>
		/// <param name="parentId">parent object ID</param>
		/// <returns>ObjectAttribute instance or null if some argument is missing</returns>
		public static ObjectAttribute ConstructEmbeddedParentAttribute(CIMModelContext cimModelContext, string type, string embeddedCategory, string parentId)
		{
			ObjectAttribute specialAtt = null;
			if (!string.IsNullOrEmpty(type) && !string.IsNullOrEmpty(embeddedCategory) && !string.IsNullOrEmpty(parentId))
			{
				if (embeddedCategory.Contains(StringManipulationManager.SeparatorColon))
				{
					embeddedCategory = embeddedCategory.Substring(embeddedCategory.IndexOf(StringManipulationManager.SeparatorColon) + 1);
				}

				specialAtt = new ObjectAttribute(cimModelContext, string.Format("{0}.Member_Of_{1}", type, embeddedCategory));
				specialAtt.IsReference = true;
				specialAtt.Value = parentId;
			}
			return specialAtt;
		}


		/// <summary>
		/// Method adds identifier of validation message which is attached to this object attribute
		/// during validation process.
		/// </summary>
		/// <param name="validationMessageId"></param>
		public void AddValidationMessage(string validationMessageId)
		{
			if (!string.IsNullOrEmpty(validationMessageId))
			{
				validationStatus = 2;
				if (validationMessages == null)
				{
					validationMessages = new List<string>();
				}
				if (!validationMessages.Contains(validationMessageId))
				{
					validationMessages.Add(validationMessageId);
				}
			}
		}

		/// <summary>
		/// Method resets the validation informations for this object attribute.
		/// </summary>
		public void ClearValidationInformations()
		{
			IsInstanceOf = null;
			validationStatus = 0;
			if (validationMessages != null)
			{
				validationMessages.Clear();
				validationMessages = null;
			}
		}

		public override string ToString()
		{
			return FullName;
		}
	}
}
