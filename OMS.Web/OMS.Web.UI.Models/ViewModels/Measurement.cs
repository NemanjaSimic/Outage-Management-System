namespace OMS.Web.UI.Models.ViewModels
{
    using System;

    public class Measurement : IEquatable<Measurement>
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public float Value { get; set; }

        public bool Equals(Measurement other)
            => Id == other.Id
            && Type == other.Type
            && Value == other.Value;
    }
}
