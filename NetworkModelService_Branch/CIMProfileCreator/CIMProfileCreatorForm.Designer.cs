namespace FTN.ESI.SIMES.CIM.CIMProfileCreator
{
    partial class CIMProfileCreatorForm
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
			this.toolTipService = new System.Windows.Forms.ToolTip(this.components);
			this.buttonBrowse = new System.Windows.Forms.Button();
			this.buttonGenerate = new System.Windows.Forms.Button();
			this.buttonLoad = new System.Windows.Forms.Button();
			this.buttonExit = new System.Windows.Forms.Button();
			this.VersionTXT = new System.Windows.Forms.TextBox();
			this.labelDLLVersion = new System.Windows.Forms.Label();
			this.labelProfileName = new System.Windows.Forms.Label();
			this.ProductNameTXT = new System.Windows.Forms.TextBox();
			this.FileNameTXT = new System.Windows.Forms.TextBox();
			this.NameSpaceTXT = new System.Windows.Forms.TextBox();
			this.labelProductName = new System.Windows.Forms.Label();
			this.labelNamespace = new System.Windows.Forms.Label();
			this.textBoxCIMProfile = new System.Windows.Forms.TextBox();
			this.labelCIMProfile = new System.Windows.Forms.Label();
			this.labelTitle = new System.Windows.Forms.Label();
			this.tableLayoutPanelMain = new System.Windows.Forms.TableLayoutPanel();
			this.richTextBoxProfile = new System.Windows.Forms.RichTextBox();
			this.tableLayoutPanelMain.SuspendLayout();
			this.SuspendLayout();
			// 
			// buttonBrowse
			// 
			this.buttonBrowse.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.buttonBrowse.Location = new System.Drawing.Point(515, 43);
			this.buttonBrowse.Margin = new System.Windows.Forms.Padding(10, 10, 15, 5);
			this.buttonBrowse.Name = "buttonBrowse";
			this.buttonBrowse.Size = new System.Drawing.Size(84, 23);
			this.buttonBrowse.TabIndex = 1;
			this.buttonBrowse.Text = "Browse..";
			this.toolTipService.SetToolTip(this.buttonBrowse, "browse for RDFS document..");
			this.buttonBrowse.UseVisualStyleBackColor = true;
			this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
			// 
			// buttonGenerate
			// 
			this.buttonGenerate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonGenerate.Location = new System.Drawing.Point(515, 193);
			this.buttonGenerate.Margin = new System.Windows.Forms.Padding(5, 3, 15, 15);
			this.buttonGenerate.Name = "buttonGenerate";
			this.buttonGenerate.Size = new System.Drawing.Size(84, 23);
			this.buttonGenerate.TabIndex = 3;
			this.buttonGenerate.Text = "Generate DLL";
			this.toolTipService.SetToolTip(this.buttonGenerate, "save report to file..");
			this.buttonGenerate.UseVisualStyleBackColor = true;
			this.buttonGenerate.Click += new System.EventHandler(this.buttonGenerate_Click);
			// 
			// buttonLoad
			// 
			this.buttonLoad.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonLoad.Location = new System.Drawing.Point(515, 74);
			this.buttonLoad.Margin = new System.Windows.Forms.Padding(5, 3, 15, 15);
			this.buttonLoad.Name = "buttonLoad";
			this.buttonLoad.Size = new System.Drawing.Size(84, 23);
			this.buttonLoad.TabIndex = 2;
			this.buttonLoad.Text = "Load profile";
			this.toolTipService.SetToolTip(this.buttonLoad, "load CIM profile..");
			this.buttonLoad.UseVisualStyleBackColor = true;
			this.buttonLoad.Click += new System.EventHandler(this.buttonLoad_Click);
			// 
			// buttonExit
			// 
			this.buttonExit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonExit.AutoSize = true;
			this.buttonExit.Location = new System.Drawing.Point(515, 234);
			this.buttonExit.Margin = new System.Windows.Forms.Padding(5, 3, 15, 15);
			this.buttonExit.Name = "buttonExit";
			this.buttonExit.Size = new System.Drawing.Size(84, 23);
			this.buttonExit.TabIndex = 5;
			this.buttonExit.Text = "Exit";
			this.toolTipService.SetToolTip(this.buttonExit, "exit application..");
			this.buttonExit.UseVisualStyleBackColor = true;
			this.buttonExit.Click += new System.EventHandler(this.buttonExit_Click);
			// 
			// VersionTXT
			// 
			this.VersionTXT.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.VersionTXT.Location = new System.Drawing.Point(102, 193);
			this.VersionTXT.Name = "VersionTXT";
			this.VersionTXT.Size = new System.Drawing.Size(400, 20);
			this.VersionTXT.TabIndex = 16;
			this.VersionTXT.Text = "1.0.0";
			// 
			// labelDLLVersion
			// 
			this.labelDLLVersion.AutoSize = true;
			this.labelDLLVersion.Location = new System.Drawing.Point(20, 193);
			this.labelDLLVersion.Margin = new System.Windows.Forms.Padding(20, 3, 3, 5);
			this.labelDLLVersion.Name = "labelDLLVersion";
			this.labelDLLVersion.Size = new System.Drawing.Size(45, 13);
			this.labelDLLVersion.TabIndex = 10;
			this.labelDLLVersion.Text = "Version:";
			this.labelDLLVersion.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// labelProfileName
			// 
			this.labelProfileName.AutoSize = true;
			this.labelProfileName.Location = new System.Drawing.Point(20, 141);
			this.labelProfileName.Margin = new System.Windows.Forms.Padding(20, 3, 3, 5);
			this.labelProfileName.Name = "labelProfileName";
			this.labelProfileName.Size = new System.Drawing.Size(55, 13);
			this.labelProfileName.TabIndex = 7;
			this.labelProfileName.Text = "File name:";
			this.labelProfileName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// ProductNameTXT
			// 
			this.ProductNameTXT.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.ProductNameTXT.Location = new System.Drawing.Point(102, 167);
			this.ProductNameTXT.Name = "ProductNameTXT";
			this.ProductNameTXT.Size = new System.Drawing.Size(400, 20);
			this.ProductNameTXT.TabIndex = 14;
			this.ProductNameTXT.Text = "Labs";
			// 
			// FileNameTXT
			// 
			this.FileNameTXT.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.FileNameTXT.Location = new System.Drawing.Point(102, 141);
			this.FileNameTXT.Name = "FileNameTXT";
			this.FileNameTXT.Size = new System.Drawing.Size(400, 20);
			this.FileNameTXT.TabIndex = 13;
			this.FileNameTXT.Text = "PowerTransformer";
			// 
			// NameSpaceTXT
			// 
			this.NameSpaceTXT.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.NameSpaceTXT.Location = new System.Drawing.Point(102, 115);
			this.NameSpaceTXT.Name = "NameSpaceTXT";
			this.NameSpaceTXT.Size = new System.Drawing.Size(400, 20);
			this.NameSpaceTXT.TabIndex = 12;
			this.NameSpaceTXT.Text = "FTN";
			// 
			// labelProductName
			// 
			this.labelProductName.AutoSize = true;
			this.labelProductName.Location = new System.Drawing.Point(20, 167);
			this.labelProductName.Margin = new System.Windows.Forms.Padding(20, 3, 3, 5);
			this.labelProductName.Name = "labelProductName";
			this.labelProductName.Size = new System.Drawing.Size(76, 13);
			this.labelProductName.TabIndex = 8;
			this.labelProductName.Text = "Product name:";
			this.labelProductName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// labelNamespace
			// 
			this.labelNamespace.AutoSize = true;
			this.labelNamespace.Location = new System.Drawing.Point(20, 115);
			this.labelNamespace.Margin = new System.Windows.Forms.Padding(20, 3, 3, 5);
			this.labelNamespace.Name = "labelNamespace";
			this.labelNamespace.Size = new System.Drawing.Size(67, 13);
			this.labelNamespace.TabIndex = 6;
			this.labelNamespace.Text = "Namespace:";
			this.labelNamespace.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// textBoxCIMProfile
			// 
			this.textBoxCIMProfile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxCIMProfile.BackColor = System.Drawing.SystemColors.Window;
			this.textBoxCIMProfile.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.textBoxCIMProfile.Location = new System.Drawing.Point(102, 43);
			this.textBoxCIMProfile.Margin = new System.Windows.Forms.Padding(3, 10, 3, 5);
			this.textBoxCIMProfile.Name = "textBoxCIMProfile";
			this.textBoxCIMProfile.ReadOnly = true;
			this.textBoxCIMProfile.Size = new System.Drawing.Size(400, 20);
			this.textBoxCIMProfile.TabIndex = 0;
			this.textBoxCIMProfile.WordWrap = false;
			this.textBoxCIMProfile.DoubleClick += new System.EventHandler(this.textBoxCIMProfile_DoubleClick);
			// 
			// labelCIMProfile
			// 
			this.labelCIMProfile.AutoSize = true;
			this.labelCIMProfile.Location = new System.Drawing.Point(20, 43);
			this.labelCIMProfile.Margin = new System.Windows.Forms.Padding(20, 10, 3, 5);
			this.labelCIMProfile.Name = "labelCIMProfile";
			this.labelCIMProfile.Size = new System.Drawing.Size(61, 13);
			this.labelCIMProfile.TabIndex = 2;
			this.labelCIMProfile.Text = "CIM Profile:";
			this.labelCIMProfile.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// labelTitle
			// 
			this.labelTitle.AutoSize = true;
			this.tableLayoutPanelMain.SetColumnSpan(this.labelTitle, 2);
			this.labelTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelTitle.Location = new System.Drawing.Point(5, 10);
			this.labelTitle.Margin = new System.Windows.Forms.Padding(5, 10, 3, 10);
			this.labelTitle.Name = "labelTitle";
			this.labelTitle.Size = new System.Drawing.Size(259, 13);
			this.labelTitle.TabIndex = 5;
			this.labelTitle.Text = "Select CIM Profile definition in RDFS format:";
			this.labelTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// tableLayoutPanelMain
			// 
			this.tableLayoutPanelMain.AllowDrop = true;
			this.tableLayoutPanelMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutPanelMain.ColumnCount = 3;
			this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanelMain.Controls.Add(this.richTextBoxProfile, 1, 7);
			this.tableLayoutPanelMain.Controls.Add(this.labelTitle, 0, 0);
			this.tableLayoutPanelMain.Controls.Add(this.labelCIMProfile, 0, 1);
			this.tableLayoutPanelMain.Controls.Add(this.textBoxCIMProfile, 1, 1);
			this.tableLayoutPanelMain.Controls.Add(this.labelNamespace, 0, 3);
			this.tableLayoutPanelMain.Controls.Add(this.NameSpaceTXT, 1, 3);
			this.tableLayoutPanelMain.Controls.Add(this.buttonBrowse, 2, 1);
			this.tableLayoutPanelMain.Controls.Add(this.labelProfileName, 0, 4);
			this.tableLayoutPanelMain.Controls.Add(this.FileNameTXT, 1, 4);
			this.tableLayoutPanelMain.Controls.Add(this.labelDLLVersion, 0, 6);
			this.tableLayoutPanelMain.Controls.Add(this.VersionTXT, 1, 6);
			this.tableLayoutPanelMain.Controls.Add(this.labelProductName, 0, 5);
			this.tableLayoutPanelMain.Controls.Add(this.ProductNameTXT, 1, 5);
			this.tableLayoutPanelMain.Controls.Add(this.buttonExit, 2, 7);
			this.tableLayoutPanelMain.Controls.Add(this.buttonGenerate, 2, 6);
			this.tableLayoutPanelMain.Controls.Add(this.buttonLoad, 2, 2);
			this.tableLayoutPanelMain.Location = new System.Drawing.Point(-2, 0);
			this.tableLayoutPanelMain.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanelMain.Name = "tableLayoutPanelMain";
			this.tableLayoutPanelMain.RowCount = 8;
			this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelMain.Size = new System.Drawing.Size(614, 636);
			this.tableLayoutPanelMain.TabIndex = 0;
			// 
			// richTextBoxProfile
			// 
			this.richTextBoxProfile.BackColor = System.Drawing.SystemColors.Window;
			this.richTextBoxProfile.Dock = System.Windows.Forms.DockStyle.Fill;
			this.richTextBoxProfile.Location = new System.Drawing.Point(102, 234);
			this.richTextBoxProfile.Name = "richTextBoxProfile";
			this.richTextBoxProfile.ReadOnly = true;
			this.tableLayoutPanelMain.SetRowSpan(this.richTextBoxProfile, 2);
			this.richTextBoxProfile.Size = new System.Drawing.Size(400, 399);
			this.richTextBoxProfile.TabIndex = 11;
			this.richTextBoxProfile.Text = "";
			this.richTextBoxProfile.WordWrap = false;
			// 
			// CIMProfileCreatorForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(612, 638);
			this.Controls.Add(this.tableLayoutPanelMain);
			this.MinimumSize = new System.Drawing.Size(525, 500);
			this.Name = "CIMProfileCreatorForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "CIM Profile Creator";
			this.tableLayoutPanelMain.ResumeLayout(false);
			this.tableLayoutPanelMain.PerformLayout();
			this.ResumeLayout(false);

        }

        #endregion

		private System.Windows.Forms.ToolTip toolTipService;
		private System.Windows.Forms.Button buttonBrowse;
		private System.Windows.Forms.Button buttonGenerate;
		private System.Windows.Forms.Button buttonLoad;
		private System.Windows.Forms.TextBox VersionTXT;
		private System.Windows.Forms.Label labelDLLVersion;
		private System.Windows.Forms.Label labelProfileName;
		private System.Windows.Forms.TextBox ProductNameTXT;
		private System.Windows.Forms.TextBox FileNameTXT;
		private System.Windows.Forms.TextBox NameSpaceTXT;
		private System.Windows.Forms.Label labelProductName;
		private System.Windows.Forms.Label labelNamespace;
		private System.Windows.Forms.TextBox textBoxCIMProfile;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanelMain;
		private System.Windows.Forms.RichTextBox richTextBoxProfile;
		private System.Windows.Forms.Label labelTitle;
		private System.Windows.Forms.Label labelCIMProfile;
		private System.Windows.Forms.Button buttonExit;
    }
}

