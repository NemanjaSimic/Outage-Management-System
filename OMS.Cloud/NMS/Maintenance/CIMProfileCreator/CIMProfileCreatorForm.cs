using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using FTN.ESI.SIMES.CIM.Core;
using FTN.ESI.SIMES.CIM.Model;
using FTN.ESI.SIMES.CIM.Parser;

namespace FTN.ESI.SIMES.CIM.CIMProfileCreator
{
    public partial class CIMProfileCreatorForm : Form
    {
        private Profile profile = null;
        private FileStream fs;

        public CIMProfileCreatorForm()
        {
            InitializeComponent();

            RefreshControls();
        }


        private void RefreshControls()
        {
            bool isCIMProfileSelected = !string.IsNullOrWhiteSpace(textBoxCIMProfile.Text);
            buttonLoad.Enabled = isCIMProfileSelected;
            buttonGenerate.Enabled = (profile != null);
        }


		#region Opeartions
		private void ShowOpenCIMRDFSFileDialog()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Open CIM Profile File..";
            openFileDialog.Filter = "CIM-RDFS Files|*.rdfs;*.legacy-rdfs|All Files|*.*";
            openFileDialog.RestoreDirectory = true;

            DialogResult dialogResponse = openFileDialog.ShowDialog(this);
            if (dialogResponse == DialogResult.OK)
            {
                textBoxCIMProfile.Text = openFileDialog.FileName;
                toolTipService.SetToolTip(textBoxCIMProfile, openFileDialog.FileName);
            }
            RefreshControls();
        }

        private void LoadCIMRDFSFile()
        {
            ////LOAD RDFS AND MAKE A PROFILE
            try
            {
                profile = null;
                using (fs = File.Open(textBoxCIMProfile.Text, FileMode.Open))
                {
                    ProfileLoader rdfParser = new ProfileLoader();
                    profile = rdfParser.LoadProfileDocument(fs, textBoxCIMProfile.Text);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format("An error occurred.\n\n{0}", e.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            PrintProfile();
            RefreshControls();
        }

        //prints rdfs on tichTextBoxProfile
        private void PrintProfile()
        {
            richTextBoxProfile.Clear();
            if (profile != null)
            {
                richTextBoxProfile.Text = profile.ToString();
            }
        }

        //generates DLL from the loaded RDFS
		private void GenerateDLL()
		{
			try
			{
				ProfileCreator pc = new ProfileCreator();
				StringBuilder sb = pc.CreateProfile(NameSpaceTXT.Text, FileNameTXT.Text, ProductNameTXT.Text, VersionTXT.Text, profile);

				richTextBoxProfile.Clear();
				richTextBoxProfile.Text = sb.ToString();
			}
			catch (Exception e)
			{
				MessageBox.Show(string.Format("An error occurred.\n\n{0}", e.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
		#endregion Opeartions


		#region Event Handlers:
		private void buttonBrowse_Click(object sender, EventArgs e)
        {
            ShowOpenCIMRDFSFileDialog();
        }

        private void textBoxCIMProfile_DoubleClick(object sender, EventArgs e)
        {
            ShowOpenCIMRDFSFileDialog();
        }

        private void buttonLoad_Click(object sender, EventArgs e)
        {
            LoadCIMRDFSFile();
        }

        private void buttonExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonGenerate_Click(object sender, EventArgs e)
        {
            GenerateDLL();
        }
        #endregion Event Handlers
		
    }
}
