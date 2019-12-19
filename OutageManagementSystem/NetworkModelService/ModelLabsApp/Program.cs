using Outage.Common;
using System;
using System.Windows.Forms;


namespace Outage.DataImporter.ModelLabsApp
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
        
		[STAThread]
		static void Main()
		{
			ILogger logger = LoggerWrapper.Instance;

			try
			{
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Application.Run(new ModelLabsAppForm());
			}
			catch (Exception e)
			{
				MessageBox.Show(string.Format("Application is going down!\n  {0}", e.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                logger.LogError($"Application is going down!\n  {e.Message}");
			}
			finally
			{
				Application.Exit();
			}
		}
	}
}
