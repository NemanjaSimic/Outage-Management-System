using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;

namespace FTN.Services.NetworkModelService.TestClientUI
{
	public class Config
	{

        private string resultDirecotry = string.Empty;

        public string ResultDirecotry
        {
            get { return resultDirecotry; }
        }

        private Config()
		{
            resultDirecotry = ConfigurationManager.AppSettings["ResultDirecotry"];

            if (!Directory.Exists(resultDirecotry))
            {
                Directory.CreateDirectory(resultDirecotry);
            }
		}

        #region Static members

        private static Config instance = null;

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

        #endregion Static members
	}
}
