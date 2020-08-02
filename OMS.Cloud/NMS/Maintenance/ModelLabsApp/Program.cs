using OMS.Common.Cloud.Logger;
using System;
using System.Windows.Forms;


namespace Outage.DataImporter.ModelLabsApp
{
    static class Program
	{
		private static ICloudLogger logger;
		private static ICloudLogger Logger
		{
			get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>

		[STAThread]
		static void Main()
		{		
			try
			{
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Application.Run(new ModelLabsAppForm());
			}
			catch (Exception e)
			{
				MessageBox.Show(string.Format("Application is going down!\n  {0}", e.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				Logger.LogError($"Application is going down!\n  {e.Message}");
			}
			finally
			{
				Application.Exit();
			}
		}
	}
}
