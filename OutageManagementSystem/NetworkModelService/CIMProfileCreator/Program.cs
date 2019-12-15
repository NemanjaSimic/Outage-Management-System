using System;
using System.Windows.Forms;

namespace FTN.ESI.SIMES.CIM.CIMProfileCreator
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
                Application.Run(new CIMProfileCreatorForm());
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format("An error occurred.\nApplication is going down.\n\n{0}", e.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Application.Exit();
            }
        }
    }
}
