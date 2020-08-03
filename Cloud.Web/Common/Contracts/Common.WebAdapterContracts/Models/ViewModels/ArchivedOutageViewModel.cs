using System;
using System.Collections.Generic;

namespace Common.Web.Models.ViewModels
{
    public class ArchivedOutageViewModel : OutageViewModel
    {
        public DateTime ArchivedAt { get; set; }

        public ArchivedOutageViewModel() => AffectedConsumers = new List<ConsumerViewModel>();
    }
}
