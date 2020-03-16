using Outage.Common;
using Outage.Common.GDA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Outage.DataModel
{
	public class EnergyConsumer : ConductingEquipment
	{
		#region Fields
		private string firstName;
		private string lastName;
		#endregion

		public EnergyConsumer(long globalId) : base(globalId)
		{
		}

		protected EnergyConsumer(EnergyConsumer ec) : base(ec)
		{
			FirstName = ec.FirstName;
			LastName = ec.LastName;
		}

		#region Properties
		public string FirstName
		{
			get { return firstName; }
			set { firstName = value; } 
		}
		public string LastName 
		{
			get { return lastName; }
			set { lastName = value; } 
		}
		#endregion

		public override bool Equals(object obj)
		{
			if (base.Equals(obj)) //in case we add new props
			{
				EnergyConsumer x = (EnergyConsumer)obj;
				return (x.FirstName == this.FirstName && x.LastName == this.LastName);
			}
			else
			{
				return false;
			}
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		#region IAccess implementation

		public override bool HasProperty(ModelCode property)
		{
			switch (property) //in case we add new props
			{
				//case ModelCode.EQUIPMENT_AGGREGATE:
				//case ModelCode.EQUIPMENT_NORMALLYINSERVICE:

				//	return true;
				case ModelCode.ENERGYCONSUMER_LASTNAME:
					return true;
				case ModelCode.ENERGYCONSUMER_FIRSTNAME:
					return true;
				default:
					return base.HasProperty(property);
			}
		}

		public override void GetProperty(Property property)
		{
			switch (property.Id) //in case we add new props
			{
				//case ModelCode.EQUIPMENT_AGGREGATE:
				//	property.SetValue(aggregate);
				//	break;

				//case ModelCode.EQUIPMENT_NORMALLYINSERVICE:
				//	property.SetValue(normallyInService);
				//	break;			
				case ModelCode.ENERGYCONSUMER_LASTNAME:
					property.SetValue(lastName);
					break;
				case ModelCode.ENERGYCONSUMER_FIRSTNAME:
					property.SetValue(firstName);
					break;
				default:
					base.GetProperty(property);
					break;
			}
		}

		public override void SetProperty(Property property)
		{
			switch (property.Id) //in case we add new props
			{
				//case ModelCode.EQUIPMENT_AGGREGATE:					
				//	aggregate = property.AsBool();
				//	break;

				//case ModelCode.EQUIPMENT_NORMALLYINSERVICE:
				//	normallyInService = property.AsBool();
				//	break;
				case ModelCode.ENERGYCONSUMER_LASTNAME:
					lastName = property.AsString();
					break;
				case ModelCode.ENERGYCONSUMER_FIRSTNAME:
					firstName = property.AsString();
					break;
				default:
					base.SetProperty(property);
					break;
			}
		}

		#endregion IAccess implementation

		#region IClonable
		public override IdentifiedObject Clone()
		{
			return new EnergyConsumer(this);
		}
		#endregion
	}
}
