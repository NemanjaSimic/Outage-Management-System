using OMS.Web.UI.Models.ViewModels;
using Outage.Common.PubSub.OutageDataContract;
using System.Collections.Generic;

namespace OMS.Web.Common.Mappers
{
    public interface IEquipmentMapper
    {
        IEnumerable<EquipmentViewModel> MapEquipments(IEnumerable<EquipmentMessage> equipment);
        EquipmentViewModel MapEquipment(EquipmentMessage equipments);
    }
}