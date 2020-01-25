using Outage.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADACommanding
{
    public class SCADACommandingCache
    {
		private ILogger logger = LoggerWrapper.Instance;

		private Dictionary<long, long> ElementToMeasurementMapper { get; set; }
		#region Singleton
		private static object syncObj = new object();
		private static SCADACommandingCache instance;

		public static SCADACommandingCache Instance
		{
			get
			{
				lock (syncObj)
				{
					if (instance == null)
					{
						instance = new SCADACommandingCache();
					}
				}
				return instance;
			}
		}
		#endregion
		private SCADACommandingCache()
        {
			ElementToMeasurementMapper = new Dictionary<long, long>();
		}

		public void AddMeasurementToElement(long elementId, long measuerementId)
		{
			if (!ElementToMeasurementMapper.ContainsKey(elementId))
			{
				ElementToMeasurementMapper.Add(elementId, measuerementId);
			}
			else
			{
				logger.LogWarn($"Failed to map measurement with GID {measuerementId} to element with GID {elementId}. Element already exists in map.");
			}
		}

		public bool TryGetMeasurementOfElement(long elementId,out long measurementId)
		{
			bool success = false;
			measurementId = -1;
			if (ElementToMeasurementMapper.ContainsKey(elementId))
			{
				measurementId = ElementToMeasurementMapper[elementId];
				success = true;
			}
			return success;
		}
    }
}
