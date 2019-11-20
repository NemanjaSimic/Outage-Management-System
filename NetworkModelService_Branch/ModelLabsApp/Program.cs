using System;
using System.Windows.Forms;

namespace ModelLabsApp
{
	static class Program
	{
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
			}
			finally
			{
				Application.Exit();
			}
		}
	}
}
