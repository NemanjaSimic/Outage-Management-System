using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CECommon.TopologyConfiguration
{
	public class Config
	{
		private static Config instance;

		public static Config Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new Config();
				}
				return instance;
			}
		}
		private Config()
		{

		}
		public string ReadConfiguration(string path)
		{
			string retValue = "";

			try
			{
				using (TextReader sr = new StreamReader(path))
				{
					retValue = sr.ReadToEnd();
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}

			return retValue;
		}
	}
}
