using System.Collections.Generic;

namespace CIM.Model
{
	/// <summary>
	/// CIMModelContext class holds maps with significant CIM model data
	/// in order to reduce memory usage.
	/// </summary>
	public class CIMModelContext
	{
		private byte nextNamespaceCode = 0;
		private Dictionary<string, byte> namespaceToCode = new Dictionary<string, byte>();

		private int nextCIMTypeCode = 0;
		private Dictionary<string, int> cimTypeToCode = new Dictionary<string, int>();

		private int nextAttributeCode = 0;
		private Dictionary<string, int> attributeToCode = new Dictionary<string, int>();
		private Dictionary<int, string> codeToAttribute = new Dictionary<int, string>();

		private int nextValueCode = 0;
		private Dictionary<string, int> valueToCode = new Dictionary<string, int>();
		private Dictionary<int, string> codeToValue = new Dictionary<int, string>();


		public CIMModelContext()
		{
			namespaceToCode.Add(string.Empty, nextNamespaceCode++);
			namespaceToCode.Add(CIMConstants.CIMNamespace, nextNamespaceCode++);

			valueToCode.Add(string.Empty, nextValueCode);
			codeToValue.Add(nextValueCode++, string.Empty);
		}


		#region public: Namespace methods
		/// <summary>
		/// Method returns the namespace with the given code.
		/// </summary>
		/// <param name="code">code of namespace</param>
		/// <returns>namespace string</returns>
		public string ReadNamespaceWithCode(byte code)
		{
			foreach (string namespaceName in namespaceToCode.Keys)
			{
				if (code == namespaceToCode[namespaceName])
				{
					return namespaceName;
				}
			}
			return string.Empty;
		}

		/// <summary>
		/// Method returns the code of given namespace or 0
		/// when given namespace isn't registered.
		/// </summary>
		/// <param name="namespaceString">namespace</param>
		/// <returns>code of namespace</returns>
		public byte ReadCodeOfNamespace(string namespaceString)
		{
			byte code = 0;
			if (namespaceString != null)
			{
				if (namespaceToCode.ContainsKey(namespaceString))
				{
					code = namespaceToCode[namespaceString];
				}
			}
			return code;
		}

		/// <summary>
		/// Method returns the code of given namespace.
		/// If such namespace is not registered, it will be added and it's code will be returned.
		/// </summary>
		/// <param name="namespaceString">namespace string</param>
		/// <returns>code of attribute</returns>
		public byte AddOrReadCodeForNamespace(string namespaceString)
		{
			byte code = 0;
			if (namespaceString != null)
			{
				if (namespaceToCode.ContainsKey(namespaceString))
				{
					code = namespaceToCode[namespaceString];
				}
				else
				{
					namespaceToCode.Add(namespaceString, nextNamespaceCode);
					code = nextNamespaceCode;
					nextNamespaceCode++;
				}
			}
			return code;
		}
		#endregion public: Namespace methods


		#region public: CIM type methods
		/// <summary>
		/// Method returns the CIM type string with the given code.
		/// </summary>
		/// <param name="code">code of CIMtype</param>
		/// <returns>CIM type string</returns>
		public string ReadCIMTypeWithCode(int code)
		{
			foreach (string cimType in cimTypeToCode.Keys)
			{
				if (code == cimTypeToCode[cimType])
				{
					return cimType;
				}
			}
			return CIMConstants.TypeNameIdentifiedObject;
		}

		/// <summary>
		/// Method returns the code of given CIM type or -1
		/// when given CIM type isn't registered.
		/// </summary>
		/// <param name="cimType">CIM type string</param>
		/// <returns>code of CIM type or -1 if such type is not registered</returns>
		public int ReadCodeOfCIMType(string cimType)
		{
			int code = -1;
			if (cimType != null)
			{
				if (cimTypeToCode.ContainsKey(cimType))
				{
					code = cimTypeToCode[cimType];
				}
			}
			return code;
		}

		/// <summary>
		/// Method returns the code of given CIM type.
		/// If such type is not registered, it will be added and it's code will be returned
		/// </summary>
		/// <param name="cimType">CIM type string</param>
		/// <returns>code of CIM type</returns>
		public int AddOrReadCodeForCIMType(string cimType)
		{
			int code = -1;
			if (cimType != null)
			{
				if (cimTypeToCode.ContainsKey(cimType))
				{
					code = cimTypeToCode[cimType];
				}
				else
				{
					cimTypeToCode.Add(cimType, nextCIMTypeCode);
					code = nextCIMTypeCode;
					nextCIMTypeCode++;
				}
			}
			return code;
		}
		#endregion public: CIM type methods


		#region public: Attribute methods
		/// <summary>
		/// Method returns the full name of attribute with the given code.
		/// </summary>
		/// <param name="code">code of attribute</param>
		/// <returns>full name of attribute</returns>
		public string ReadAttributeWithCode(int code)
		{
			if (codeToAttribute.ContainsKey(code))
			{
				return codeToAttribute[code];
			}
			return string.Empty;
		}

		/// <summary>
		/// Method returns the code of given attribute or -1
		/// when given attribute name isn't registered.
		/// </summary>
		/// <param name="attributeName">full name of attribute</param>
		/// <returns>code of attribute or -1 if such name is not registered</returns>
		public int ReadCodeOfAttribute(string attributeName)
		{
			int code = -1;
			if (attributeName != null)
			{
				if (attributeToCode.ContainsKey(attributeName))
				{
					code = attributeToCode[attributeName];
				}
			}
			return code;
		}

		/// <summary>
		/// Method returns the code of given attribute
		/// If such attribute name is not registered, it will be added and it's code will be returned
		/// </summary>
		/// <param name="attributeName">full name of attribute</param>
		/// <returns>code of attribute</returns>
		public int AddOrReadCodeForAttribute(string attributeName)
		{
			int code = -1;
			if (attributeName != null)
			{
				if (attributeToCode.ContainsKey(attributeName))
				{
					code = attributeToCode[attributeName];
				}
				else
				{
					attributeToCode.Add(attributeName, nextAttributeCode);
					codeToAttribute.Add(nextAttributeCode, attributeName);
					code = nextAttributeCode;
					nextAttributeCode++;
				}
			}
			return code;
		}
		#endregion public: Attribute methods


		#region public: Values methods
		/// <summary>
		/// Method returns the string representing CIM attribute value with the given code.
		/// </summary>
		/// <param name="code">code of CIM attribute value</param>
		/// <returns>string representing CIM attribute value</returns>
		public string ReadValueWithCode(int code)
		{
			if (codeToValue.ContainsKey(code))
			{
				return codeToValue[code];
			}
			return string.Empty;
		}

		/// <summary>
		/// Method returns the code of given CIM attribute value or null
		/// when given string isn't registered.
		/// </summary>
		/// <param name="valueString">string representing CIM attribute value</param>
		/// <returns>code of value or null</returns>
		public int ReadCodeOfValue(string valueString)
		{
			int code = -1;
			if (valueString != null)
			{
				if (valueToCode.ContainsKey(valueString))
				{
					code = valueToCode[valueString];
				}
			}
			return code;
		}

		/// <summary>
		/// Method returns the code of CIM attribute value.
		/// If such value is not registered, it will be added and it's code will be returned.
		/// </summary>
		/// <param name="valueString">string representing CIM attribute value</param>
		/// <returns>code of value</returns>
		public int AddOrReadCodeForValue(string valueString)
		{
			int code = -1;
			if (valueString != null)
			{
				if (valueToCode.ContainsKey(valueString))
				{
					code = valueToCode[valueString];
				}
				else
				{
					valueToCode.Add(valueString, nextValueCode);
					codeToValue.Add(nextValueCode, valueString);
					code = nextValueCode;
					nextValueCode++;
				}
			}
			return code;
		}
		#endregion public: Values methods

	}
}
