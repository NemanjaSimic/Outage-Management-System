using Common.PubSubContracts.DataContracts.OMS;
using Common.Web.Models.ViewModels;
using System.Collections.Generic;

namespace Common.Web.Mappers
{
    public interface IEquipmentMapper
    {
        IEnumerable<EquipmentViewModel> MapEquipments(IEnumerable<EquipmentMessage> equipment);
        EquipmentViewModel MapEquipment(EquipmentMessage equipments);
    }
}