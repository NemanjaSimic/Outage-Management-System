using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace FTN.Common.GDA
{
	[DataContract]
	
	public class Association
	{
		private bool inverse;
		private ModelCode propertyId;
		private ModelCode type;

		public Association()
		{
			this.inverse = false;
			this.propertyId = 0;
			this.type = 0;
		}

		public Association(ModelCode property)
		{
			this.inverse = false;
			this.propertyId = property;
			this.type = 0;
		}

		public Association(ModelCode property, bool inverse)
		{
			this.inverse = inverse;
			this.propertyId = property;
			this.type = 0;
		}

		public Association(ModelCode property, ModelCode type)
		{
			this.inverse = false;
			this.propertyId = property;
			this.type = type;
		}

		public Association(ModelCode property, ModelCode type, bool inverse)
		{
			this.inverse = inverse;
			this.propertyId = property;
			this.type = type;
		}

		public Association(string property, string type)
		{
			if ((property != "") && (type != ""))
			{
				this.propertyId = (ModelCode)Enum.Parse(typeof(ModelCode), property);
				this.type = (ModelCode)Enum.Parse(typeof(ModelCode), type);
			}
			else if ((property != "") && (type == ""))
			{
				this.propertyId = (ModelCode)Enum.Parse(typeof(ModelCode), property);
			}
		}

		public Association(string property, string type, string inverse)
		{
			if (type != null && type != "")
			{
				this.type = (ModelCode)Enum.Parse(typeof(ModelCode), type);
			}

			if (property != null && property != "")
			{
				this.propertyId = (ModelCode)Enum.Parse(typeof(ModelCode), property);
			}

			if (inverse != null && inverse != "")
			{
				this.inverse = Boolean.Parse(inverse);
			}
			else if ((property == "") && (type == "") && (inverse == ""))
			{
				this.inverse = false;
				this.propertyId = 0;
				this.type = 0;
			}
		}

		[DataMember]
		public bool Inverse
		{
			get { return inverse; }
			set { inverse = value; }
		}

		[DataMember]
		public ModelCode PropertyId
		{
			get { return propertyId; }
			set { propertyId = value; }
		}

		[DataMember]
		public ModelCode Type
		{
			get { return type; }
			set { type = value; }
		}
	}
}
