using CECommon.Interfaces;

namespace CECommon.Providers
{
    public class Provider
    {
        #region Singleton
        private static Provider instance;
        private static object syncObj = new object();
        public static Provider Instance
        {
            get
            {
                lock (syncObj)
                {
                    if (instance == null)
                    {
                        instance = new Provider();
                    }
                }
                return instance;
            }
        }
        #endregion
        private Provider(){ }
        public ITopologyProvider TopologyProvider { get; set; } 
        public IModelProvider ModelProvider { get; set; }
        public ITopologyConverterProvider TopologyConverterProvider { get; set; }
        public ISCADAResultHandler SCADAResultHandler { get; set; }
        public IMeasurementProvider MeasurementProvider { get; set; }
    }
}
