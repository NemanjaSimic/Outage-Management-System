namespace OMS.Web.Common
{
    /// <summary>
    /// Configuration section for service addresses
    /// </summary>
    public static class ServiceAddress
    {
        //public const string TopologyServiceAddress = nameof(TopologyServiceAddress);
        //public const string OutageServiceAddress = nameof(OutageServiceAddress);
        //public const string ScadaServiceAddress = nameof(ScadaServiceAddress);
        //public const string ScadaCommandServiceAddress = nameof(ScadaCommandServiceAddress);
        //public const string PubSubServiceAddress = nameof(PubSubServiceAddress);
        //public const string WebServiceAddress = nameof(WebServiceAddress);
    }

    /// <summary>
    /// Configuration section for hub endpoints
    /// </summary>
    public static class HubAddress
    {
        public const string GraphHubUrl = nameof(GraphHubUrl);
        public const string GraphHubName = nameof(GraphHubName);
        public const string ScadaHubUrl = nameof(ScadaHubUrl);
        public const string ScadaHubName = nameof(ScadaHubName);
        public const string OutageHubUrl = nameof(OutageHubUrl);
        public const string OutageHubName = nameof(OutageHubName);
    }
}
