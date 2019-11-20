using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using FTN.Common;
using FTN.ESI.SIMES.CIM.CIMAdapter;
using FTN.ESI.SIMES.CIM.CIMAdapter.Manager;
using FTN.ServiceContracts;

namespace ModelLabsApp
{
	public partial class ModelLabsAppForm : Form
	{
		private CIMAdapter adapter = new CIMAdapter();
		private Delta nmsDelta = null;
		
        public ModelLabsAppForm()
		{
			InitializeComponent();

			InitGUIElements();
		}

		private void InitGUIElements()
		{
			buttonConvertCIM.Enabled = false;
			buttonApplyDelta.Enabled = false;

			comboBoxProfile.DataSource = Enum.GetValues(typeof(SupportedProfiles));
			comboBoxProfile.SelectedItem = SupportedProfiles.PowerTransformer;
			//comboBoxProfile.Enabled = false; //// other profiles are not supported
		}

		private void ShowOpenCIMXMLFileDialog()
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Title = "Open CIM Document File..";
			openFileDialog.Filter = "CIM-XML Files|*.xml;*.txt;*.rdf|All Files|*.*";
			openFileDialog.RestoreDirectory = true;

			DialogResult dialogResponse = openFileDialog.ShowDialog(this);
			if (dialogResponse == DialogResult.OK)
			{
				textBoxCIMFile.Text = openFileDialog.FileName;
				toolTipControl.SetToolTip(textBoxCIMFile, openFileDialog.FileName);
				buttonConvertCIM.Enabled = true;
				richTextBoxReport.Clear();
			}
			else
			{
				buttonConvertCIM.Enabled = false;
			}
		}

		private void ConvertCIMXMLToDMSNetworkModelDelta()
		{
			////SEND CIM/XML to ADAPTER
			try
			{
                if (textBoxCIMFile.Text == string.Empty)
                {
                    MessageBox.Show("Must enter CIM/XML file.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

				string log;
				nmsDelta = null;
				using (FileStream fs = File.Open(textBoxCIMFile.Text, FileMode.Open))
				{
					nmsDelta = adapter.CreateDelta(fs, (SupportedProfiles)(comboBoxProfile.SelectedItem), out log);
					richTextBoxReport.Text = log;
				}
				if (nmsDelta != null)
				{
					//// export delta to file
					using (XmlTextWriter xmlWriter = new XmlTextWriter(".\\deltaExport.xml", Encoding.UTF8))
					{
						xmlWriter.Formatting = Formatting.Indented;
						nmsDelta.ExportToXml(xmlWriter);
						xmlWriter.Flush();
					}
				}
			}
			catch (Exception e)
			{
				MessageBox.Show(string.Format("An error occurred.\n\n{0}", e.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

			buttonApplyDelta.Enabled = (nmsDelta != null);
            textBoxCIMFile.Text = string.Empty;
		}

		private void ApplyDMSNetworkModelDelta()
		{
			//// APPLY Delta
            if (nmsDelta != null)
            {
                try
                {
                    string log = adapter.ApplyUpdates(nmsDelta);
                    richTextBoxReport.AppendText(log);
                    nmsDelta = null;
                    buttonApplyDelta.Enabled = (nmsDelta != null);
                }
                catch (Exception e)
                {
                    MessageBox.Show(string.Format("An error occurred.\n\n{0}", e.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("No data is imported into delta object.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
		}

		
		private void buttonBrowseLocationOnClick(object sender, EventArgs e)
		{
			ShowOpenCIMXMLFileDialog();
		}

		private void textBoxCIMFileOnDoubleClick(object sender, EventArgs e)
		{
			ShowOpenCIMXMLFileDialog();
		}

		private void buttonConvertCIMOnClick(object sender, EventArgs e)
		{
			ConvertCIMXMLToDMSNetworkModelDelta();
		}

		private void buttonApplyDeltaOnClick(object sender, EventArgs e)
		{
			ApplyDMSNetworkModelDelta();
		}

		private void buttonExitOnClick(object sender, EventArgs e)
		{
			Close();
		}
	}
}
