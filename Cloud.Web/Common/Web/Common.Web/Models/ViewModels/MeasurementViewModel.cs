using OMS.Common.Cloud;
using System;

namespace Common.Web.Models.ViewModels
{
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
