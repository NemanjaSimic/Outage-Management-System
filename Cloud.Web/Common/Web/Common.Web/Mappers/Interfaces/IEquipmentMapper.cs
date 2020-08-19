using Common.OMS.OutageDatabaseModel;
using Common.PubSubContracts.DataContracts.OMS;
using Common.Web.Models.ViewModels;
using System.Collections.Generic;

namespace Common.Web.Mappers
{
    public interface IEquipmentMapper
    {
        EquipmentViewModel MapEquipment(Equipment equipment);
        EquipmentViewModel MapEquipment(EquipmentMessage equipment);
        IEnumerable<EquipmentViewModel> MapEquipments(IEnumerable<Equipment> equipment);
        IEnumerable<EquipmentViewModel> MapEquipments(IEnumerable<EquipmentMessage> equipment);
    }
}