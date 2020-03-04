namespace OMS.Web.UI.Models.ViewModels
{
    using Outage.Common;
    using System;

    public class MeasurementViewModel : IEquatable<MeasurementViewModel>
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public float Value { get; set; }
        public AlarmType AlarmType { get; set; } 

        public bool Equals(MeasurementViewModel other)
            => Id == other.Id
            && Type == other.Type
            && Value == other.Value
            && AlarmType == other.AlarmType;
    }
}
