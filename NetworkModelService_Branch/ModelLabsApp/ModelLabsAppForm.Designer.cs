namespace ModelLabsApp
{
	partial class ModelLabsAppForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.tableLayoutPanelMain = new System.Windows.Forms.TableLayoutPanel();
			this.labelCIMXMLFile = new System.Windows.Forms.Label();
			this.textBoxCIMFile = new System.Windows.Forms.TextBox();
			this.buttonBrowseLocation = new System.Windows.Forms.Button();
			this.richTextBoxReport = new System.Windows.Forms.RichTextBox();
			this.labelProfile = new System.Windows.Forms.Label();
			this.comboBoxProfile = new System.Windows.Forms.ComboBox();
			this.buttonConvertCIM = new System.Windows.Forms.Button();
			this.labelReport = new System.Windows.Forms.Label();
			this.buttonExit = new System.Windows.Forms.Button();
			this.buttonApplyDelta = new System.Windows.Forms.Button();
			this.toolTipControl = new System.Windows.Forms.ToolTip(this.components);
			this.tableLayoutPanelMain.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanelMain
			// 
			this.tableLayoutPanelMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutPanelMain.ColumnCount = 3;
			this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanelMain.Controls.Add(this.labelCIMXMLFile, 0, 0);
			this.tableLayoutPanelMain.Controls.Add(this.textBoxCIMFile, 1, 0);
			this.tableLayoutPanelMain.Controls.Add(this.buttonBrowseLocation, 2, 0);
			this.tableLayoutPanelMain.Controls.Add(this.richTextBoxReport, 1, 4);
			this.tableLayoutPanelMain.Controls.Add(this.labelProfile, 0, 1);
			this.tableLayoutPanelMain.Controls.Add(this.comboBoxProfile, 1, 1);
			this.tableLayoutPanelMain.Controls.Add(this.buttonConvertCIM, 2, 2);
			this.tableLayoutPanelMain.Controls.Add(this.labelReport, 0, 4);
			this.tableLayoutPanelMain.Controls.Add(this.buttonExit, 2, 5);
			this.tableLayoutPanelMain.Controls.Add(this.buttonApplyDelta, 2, 3);
			this.tableLayoutPanelMain.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanelMain.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanelMain.Name = "tableLayoutPanelMain";
			this.tableLayoutPanelMain.RowCount = 6;
			this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelMain.Size = new System.Drawing.Size(672, 524);
			this.tableLayoutPanelMain.TabIndex = 0;
			// 
			// labelCIMXMLFile
			// 
			this.labelCIMXMLFile.AutoSize = true;
			this.labelCIMXMLFile.Location = new System.Drawing.Point(5, 13);
			this.labelCIMXMLFile.Margin = new System.Windows.Forms.Padding(5, 13, 3, 3);
			this.labelCIMXMLFile.Name = "labelCIMXMLFile";
			this.labelCIMXMLFile.Size = new System.Drawing.Size(75, 13);
			this.labelCIMXMLFile.TabIndex = 1;
			this.labelCIMXMLFile.Text = "CIM/XML file :";
			// 
			// textBoxCIMFile
			// 
			this.textBoxCIMFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxCIMFile.BackColor = System.Drawing.SystemColors.Window;
			this.textBoxCIMFile.Location = new System.Drawing.Point(86, 10);
			this.textBoxCIMFile.Margin = new System.Windows.Forms.Padding(3, 10, 3, 3);
			this.textBoxCIMFile.Name = "textBoxCIMFile";
			this.textBoxCIMFile.ReadOnly = true;
			this.textBoxCIMFile.Size = new System.Drawing.Size(492, 20);
			this.textBoxCIMFile.TabIndex = 1;
			this.textBoxCIMFile.DoubleClick += new System.EventHandler(this.textBoxCIMFileOnDoubleClick);
			// 
			// buttonBrowseLocation
			// 
			this.buttonBrowseLocation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonBrowseLocation.Location = new System.Drawing.Point(584, 10);
			this.buttonBrowseLocation.Margin = new System.Windows.Forms.Padding(3, 10, 10, 10);
			this.buttonBrowseLocation.Name = "buttonBrowseLocation";
			this.buttonBrowseLocation.Size = new System.Drawing.Size(78, 23);
			this.buttonBrowseLocation.TabIndex = 2;
			this.buttonBrowseLocation.Text = "Browse..";
			this.buttonBrowseLocation.UseVisualStyleBackColor = true;
			this.buttonBrowseLocation.Click += new System.EventHandler(this.buttonBrowseLocationOnClick);
			// 
			// richTextBoxReport
			// 
			this.richTextBoxReport.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.richTextBoxReport.BackColor = System.Drawing.SystemColors.Window;
			this.richTextBoxReport.Location = new System.Drawing.Point(86, 140);
			this.richTextBoxReport.Margin = new System.Windows.Forms.Padding(3, 3, 3, 10);
			this.richTextBoxReport.Name = "richTextBoxReport";
			this.richTextBoxReport.ReadOnly = true;
			this.richTextBoxReport.Size = new System.Drawing.Size(492, 338);
			this.richTextBoxReport.TabIndex = 5;
			this.richTextBoxReport.Text = "";
			// 
			// labelProfile
			// 
			this.labelProfile.AutoSize = true;
			this.labelProfile.Location = new System.Drawing.Point(5, 56);
			this.labelProfile.Margin = new System.Windows.Forms.Padding(5, 13, 3, 5);
			this.labelProfile.Name = "labelProfile";
			this.labelProfile.Size = new System.Drawing.Size(64, 13);
			this.labelProfile.TabIndex = 13;
			this.labelProfile.Text = "CIM Profile :";
			// 
			// comboBoxProfile
			// 
			this.comboBoxProfile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.comboBoxProfile.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxProfile.ForeColor = System.Drawing.SystemColors.WindowText;
			this.comboBoxProfile.FormattingEnabled = true;
			this.comboBoxProfile.Location = new System.Drawing.Point(86, 53);
			this.comboBoxProfile.Margin = new System.Windows.Forms.Padding(3, 10, 3, 5);
			this.comboBoxProfile.Name = "comboBoxProfile";
			this.comboBoxProfile.Size = new System.Drawing.Size(492, 21);
			this.comboBoxProfile.TabIndex = 3;
			// 
			// buttonConvertCIM
			// 
			this.buttonConvertCIM.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonConvertCIM.Location = new System.Drawing.Point(584, 82);
			this.buttonConvertCIM.Margin = new System.Windows.Forms.Padding(3, 3, 10, 3);
			this.buttonConvertCIM.Name = "buttonConvertCIM";
			this.buttonConvertCIM.Size = new System.Drawing.Size(78, 23);
			this.buttonConvertCIM.TabIndex = 4;
			this.buttonConvertCIM.Text = "Convert";
			this.toolTipControl.SetToolTip(this.buttonConvertCIM, "convert CIM to NMS delta..");
			this.buttonConvertCIM.UseVisualStyleBackColor = true;
			this.buttonConvertCIM.Click += new System.EventHandler(this.buttonConvertCIMOnClick);
			// 
			// labelReport
			// 
			this.labelReport.AutoSize = true;
			this.labelReport.Location = new System.Drawing.Point(5, 150);
			this.labelReport.Margin = new System.Windows.Forms.Padding(5, 13, 3, 5);
			this.labelReport.Name = "labelReport";
			this.labelReport.Size = new System.Drawing.Size(45, 13);
			this.labelReport.TabIndex = 14;
			this.labelReport.Text = "Report :";
			// 
			// buttonExit
			// 
			this.buttonExit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonExit.Location = new System.Drawing.Point(584, 491);
			this.buttonExit.Margin = new System.Windows.Forms.Padding(3, 3, 10, 10);
			this.buttonExit.Name = "buttonExit";
			this.buttonExit.Size = new System.Drawing.Size(78, 23);
			this.buttonExit.TabIndex = 6;
			this.buttonExit.Text = "Exit";
			this.toolTipControl.SetToolTip(this.buttonExit, "exit application..");
			this.buttonExit.UseVisualStyleBackColor = true;
			this.buttonExit.Click += new System.EventHandler(this.buttonExitOnClick);
			// 
			// buttonApplyDelta
			// 
			this.buttonApplyDelta.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonApplyDelta.Location = new System.Drawing.Point(584, 111);
			this.buttonApplyDelta.Margin = new System.Windows.Forms.Padding(3, 3, 10, 3);
			this.buttonApplyDelta.Name = "buttonApplyDelta";
			this.buttonApplyDelta.Size = new System.Drawing.Size(78, 23);
			this.buttonApplyDelta.TabIndex = 4;
			this.buttonApplyDelta.Text = "Apply Delta";
			this.toolTipControl.SetToolTip(this.buttonApplyDelta, "apply delta to NMS..");
			this.buttonApplyDelta.UseVisualStyleBackColor = true;
			this.buttonApplyDelta.Click += new System.EventHandler(this.buttonApplyDeltaOnClick);
			// 
			// ModelLabsAppForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(672, 524);
			this.Controls.Add(this.tableLayoutPanelMain);
			this.Name = "ModelLabsAppForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Model Labs App";
			this.tableLayoutPanelMain.ResumeLayout(false);
			this.tableLayoutPanelMain.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanelMain;
		private System.Windows.Forms.Label labelCIMXMLFile;
		private System.Windows.Forms.TextBox textBoxCIMFile;
		private System.Windows.Forms.Button buttonBrowseLocation;
		private System.Windows.Forms.Button buttonConvertCIM;
		private System.Windows.Forms.ToolTip toolTipControl;
        private System.Windows.Forms.RichTextBox richTextBoxReport;
		private System.Windows.Forms.Label labelProfile;
		private System.Windows.Forms.ComboBox comboBoxProfile;
		private System.Windows.Forms.Label labelReport;
		private System.Windows.Forms.Button buttonExit;
		private System.Windows.Forms.Button buttonApplyDelta;
	}
}

