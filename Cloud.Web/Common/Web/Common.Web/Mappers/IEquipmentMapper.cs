using Common.Web.UI.Models.ViewModels;
using Outage.Common.PubSub.OutageDataContract;
using System.Collections.Generic;

namespace Common.Web.Mappers
{
    public interface IEquipmentMapper
    {
        IEnumerable<EquipmentViewModel> MapEquipments(IEnumerable<EquipmentMessage> equipment);
        EquipmentViewModel MapEquipment(EquipmentMessage equipments);
    }
}