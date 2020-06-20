using OMS.Web.UI.Models.ViewModels;
using Outage.Common.PubSub.OutageDataContract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS.Web.Common.Mappers
{
    public class EquipmentMapper : IEquipmentMapper
    {
        //TODO:
        //private IOutageMapper _outageMapper;

        //public EquipmentMapper(IOutageMapper outageMapper)
        //{
        //    //_outageMapper = outageMapper;
        //}

        public EquipmentViewModel MapEquipment(EquipmentMessage equipment)
            => new EquipmentViewModel
            {
                Id = equipment.EquipmentId,
                Mrid = equipment.EquipmentMRID,
                ActiveOutages = new List<ActiveOutageViewModel>(),      //TODO: _outageMapper.MapActiveOutages(equipment.ActiveOutages),
                ArchivedOutages = new List<ArchivedOutageViewModel>(),  //TODO: _outageMapper.MapArchivedOutages(equipment.ArchivedOutages)
            };


        public IEnumerable<EquipmentViewModel> MapEquipments(IEnumerable<EquipmentMessage> equipments)
        => equipments.Select(e => MapEquipment(e)).ToList();
    }
}
