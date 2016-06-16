namespace Security_Demo {
	partial class SecuritySettingsEditor {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.label6 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.cboDomain = new System.Windows.Forms.ComboBox();
			this.cboUser = new System.Windows.Forms.ComboBox();
			this.permissionsPanel = new System.Windows.Forms.TableLayoutPanel();
			this.lblDomainPermissions = new System.Windows.Forms.Label();
			this.chkModifyPresent = new System.Windows.Forms.CheckBox();
			this.chkViewPresent = new System.Windows.Forms.CheckBox();
			this.chkModifyData = new System.Windows.Forms.CheckBox();
			this.chkViewData = new System.Windows.Forms.CheckBox();
			this.chkModifyLayout = new System.Windows.Forms.CheckBox();
			this.chkViewLayout = new System.Windows.Forms.CheckBox();
			this.chkModifyConnect = new System.Windows.Forms.CheckBox();
			this.chkViewConnect = new System.Windows.Forms.CheckBox();
			this.chkModifyDelete = new System.Windows.Forms.CheckBox();
			this.chkViewDelete = new System.Windows.Forms.CheckBox();
			this.chkModifyInsert = new System.Windows.Forms.CheckBox();
			this.chkViewInsert = new System.Windows.Forms.CheckBox();
			this.chkModifyTemplates = new System.Windows.Forms.CheckBox();
			this.chkViewTemplates = new System.Windows.Forms.CheckBox();
			this.chkModifySecurity = new System.Windows.Forms.CheckBox();
			this.chkViewSecurity = new System.Windows.Forms.CheckBox();
			this.chkModifyDesigns = new System.Windows.Forms.CheckBox();
			this.chkViewDesigns = new System.Windows.Forms.CheckBox();
			this.lblGeneralPermissions = new System.Windows.Forms.Label();
			this.lblPermissionDesigns = new System.Windows.Forms.Label();
			this.lblPermissionSecurity = new System.Windows.Forms.Label();
			this.lblPermissionTemplates = new System.Windows.Forms.Label();
			this.lblPermissionPresent = new System.Windows.Forms.Label();
			this.lblPermissionDelete = new System.Windows.Forms.Label();
			this.lblPermissionInsert = new System.Windows.Forms.Label();
			this.lblPermissionConnect = new System.Windows.Forms.Label();
			this.lblPermissionLayout = new System.Windows.Forms.Label();
			this.lblPermissionData = new System.Windows.Forms.Label();
			this.lblPermissions = new System.Windows.Forms.Label();
			this.lblViewAccess = new System.Windows.Forms.Label();
			this.lblModifyAccess = new System.Windows.Forms.Label();
			this.permissionsPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label6.Location = new System.Drawing.Point(12, 15);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(32, 13);
			this.label6.TabIndex = 4;
			this.label6.Text = "User:";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(12, 42);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(81, 13);
			this.label1.TabIndex = 3;
			this.label1.Text = "SecurityDomain";
			// 
			// cboDomain
			// 
			this.cboDomain.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboDomain.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.cboDomain.FormattingEnabled = true;
			this.cboDomain.Items.AddRange(new object[] {
            "A",
            "B",
            "C",
            "D",
            "E",
            "F",
            "G",
            "H",
            "I",
            "J",
            "K",
            "L",
            "M",
            "N",
            "O",
            "P",
            "Q",
            "R",
            "S",
            "T",
            "U",
            "V",
            "W",
            "X",
            "Y",
            "Z"});
			this.cboDomain.Location = new System.Drawing.Point(99, 39);
			this.cboDomain.Name = "cboDomain";
			this.cboDomain.Size = new System.Drawing.Size(45, 21);
			this.cboDomain.TabIndex = 2;
			this.cboDomain.SelectedIndexChanged += new System.EventHandler(this.cboUserOrDom_SelectedIndexChanged);
			// 
			// cboUser
			// 
			this.cboUser.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.cboUser.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboUser.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.cboUser.FormattingEnabled = true;
			this.cboUser.Location = new System.Drawing.Point(99, 12);
			this.cboUser.Name = "cboUser";
			this.cboUser.Size = new System.Drawing.Size(98, 21);
			this.cboUser.TabIndex = 1;
			this.cboUser.SelectedIndexChanged += new System.EventHandler(this.cboUserOrDom_SelectedIndexChanged);
			// 
			// permissionsPanel
			// 
			this.permissionsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.permissionsPanel.ColumnCount = 3;
			this.permissionsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.permissionsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.permissionsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.permissionsPanel.Controls.Add(this.lblDomainPermissions, 0, 7);
			this.permissionsPanel.Controls.Add(this.chkModifyPresent, 2, 13);
			this.permissionsPanel.Controls.Add(this.chkViewPresent, 1, 13);
			this.permissionsPanel.Controls.Add(this.chkModifyData, 2, 12);
			this.permissionsPanel.Controls.Add(this.chkViewData, 1, 12);
			this.permissionsPanel.Controls.Add(this.chkModifyLayout, 2, 11);
			this.permissionsPanel.Controls.Add(this.chkViewLayout, 1, 11);
			this.permissionsPanel.Controls.Add(this.chkModifyConnect, 2, 10);
			this.permissionsPanel.Controls.Add(this.chkViewConnect, 1, 10);
			this.permissionsPanel.Controls.Add(this.chkModifyDelete, 2, 9);
			this.permissionsPanel.Controls.Add(this.chkViewDelete, 1, 9);
			this.permissionsPanel.Controls.Add(this.chkModifyInsert, 2, 8);
			this.permissionsPanel.Controls.Add(this.chkViewInsert, 1, 8);
			this.permissionsPanel.Controls.Add(this.chkModifyTemplates, 2, 6);
			this.permissionsPanel.Controls.Add(this.chkViewTemplates, 1, 6);
			this.permissionsPanel.Controls.Add(this.chkModifySecurity, 2, 5);
			this.permissionsPanel.Controls.Add(this.chkViewSecurity, 1, 5);
			this.permissionsPanel.Controls.Add(this.chkModifyDesigns, 2, 4);
			this.permissionsPanel.Controls.Add(this.chkViewDesigns, 1, 4);
			this.permissionsPanel.Controls.Add(this.lblGeneralPermissions, 0, 3);
			this.permissionsPanel.Controls.Add(this.lblPermissionDesigns, 0, 4);
			this.permissionsPanel.Controls.Add(this.lblPermissionSecurity, 0, 5);
			this.permissionsPanel.Controls.Add(this.lblPermissionTemplates, 0, 6);
			this.permissionsPanel.Controls.Add(this.lblPermissionPresent, 0, 13);
			this.permissionsPanel.Controls.Add(this.lblPermissionDelete, 0, 9);
			this.permissionsPanel.Controls.Add(this.lblPermissionInsert, 0, 8);
			this.permissionsPanel.Controls.Add(this.lblPermissionConnect, 0, 10);
			this.permissionsPanel.Controls.Add(this.lblPermissionLayout, 0, 11);
			this.permissionsPanel.Controls.Add(this.lblPermissionData, 0, 12);
			this.permissionsPanel.Controls.Add(this.lblPermissions, 0, 0);
			this.permissionsPanel.Controls.Add(this.lblViewAccess, 1, 0);
			this.permissionsPanel.Controls.Add(this.lblModifyAccess, 2, 0);
			this.permissionsPanel.Location = new System.Drawing.Point(12, 66);
			this.permissionsPanel.Name = "permissionsPanel";
			this.permissionsPanel.RowCount = 15;
			this.permissionsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.permissionsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.permissionsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.permissionsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.permissionsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.permissionsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.permissionsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.permissionsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.permissionsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.permissionsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.permissionsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.permissionsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.permissionsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.permissionsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.permissionsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.permissionsPanel.Size = new System.Drawing.Size(257, 293);
			this.permissionsPanel.TabIndex = 0;
			// 
			// lblDomainPermissions
			// 
			this.lblDomainPermissions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.permissionsPanel.SetColumnSpan(this.lblDomainPermissions, 3);
			this.lblDomainPermissions.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblDomainPermissions.Location = new System.Drawing.Point(3, 100);
			this.lblDomainPermissions.Name = "lblDomainPermissions";
			this.lblDomainPermissions.Size = new System.Drawing.Size(251, 20);
			this.lblDomainPermissions.TabIndex = 39;
			this.lblDomainPermissions.Text = "Domain Permissions";
			this.lblDomainPermissions.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// chkModifyPresent
			// 
			this.chkModifyPresent.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
			this.chkModifyPresent.AutoSize = true;
			this.chkModifyPresent.Location = new System.Drawing.Point(224, 223);
			this.chkModifyPresent.Name = "chkModifyPresent";
			this.chkModifyPresent.Size = new System.Drawing.Size(15, 14);
			this.chkModifyPresent.TabIndex = 34;
			this.chkModifyPresent.UseVisualStyleBackColor = true;
			this.chkModifyPresent.CheckedChanged += new System.EventHandler(this.permissionCheckBox_CheckedChanged);
			// 
			// chkViewPresent
			// 
			this.chkViewPresent.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
			this.chkViewPresent.AutoSize = true;
			this.chkViewPresent.Location = new System.Drawing.Point(179, 223);
			this.chkViewPresent.Name = "chkViewPresent";
			this.chkViewPresent.Size = new System.Drawing.Size(15, 14);
			this.chkViewPresent.TabIndex = 33;
			this.chkViewPresent.UseVisualStyleBackColor = true;
			this.chkViewPresent.CheckedChanged += new System.EventHandler(this.permissionCheckBox_CheckedChanged);
			// 
			// chkModifyData
			// 
			this.chkModifyData.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
			this.chkModifyData.AutoSize = true;
			this.chkModifyData.Location = new System.Drawing.Point(224, 203);
			this.chkModifyData.Name = "chkModifyData";
			this.chkModifyData.Size = new System.Drawing.Size(15, 14);
			this.chkModifyData.TabIndex = 32;
			this.chkModifyData.UseVisualStyleBackColor = true;
			this.chkModifyData.CheckedChanged += new System.EventHandler(this.permissionCheckBox_CheckedChanged);
			// 
			// chkViewData
			// 
			this.chkViewData.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
			this.chkViewData.AutoSize = true;
			this.chkViewData.Location = new System.Drawing.Point(179, 203);
			this.chkViewData.Name = "chkViewData";
			this.chkViewData.Size = new System.Drawing.Size(15, 14);
			this.chkViewData.TabIndex = 31;
			this.chkViewData.UseVisualStyleBackColor = true;
			this.chkViewData.CheckedChanged += new System.EventHandler(this.permissionCheckBox_CheckedChanged);
			// 
			// chkModifyLayout
			// 
			this.chkModifyLayout.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
			this.chkModifyLayout.AutoSize = true;
			this.chkModifyLayout.Location = new System.Drawing.Point(224, 183);
			this.chkModifyLayout.Name = "chkModifyLayout";
			this.chkModifyLayout.Size = new System.Drawing.Size(15, 14);
			this.chkModifyLayout.TabIndex = 30;
			this.chkModifyLayout.UseVisualStyleBackColor = true;
			this.chkModifyLayout.CheckedChanged += new System.EventHandler(this.permissionCheckBox_CheckedChanged);
			// 
			// chkViewLayout
			// 
			this.chkViewLayout.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
			this.chkViewLayout.AutoSize = true;
			this.chkViewLayout.Location = new System.Drawing.Point(179, 183);
			this.chkViewLayout.Name = "chkViewLayout";
			this.chkViewLayout.Size = new System.Drawing.Size(15, 14);
			this.chkViewLayout.TabIndex = 29;
			this.chkViewLayout.UseVisualStyleBackColor = true;
			this.chkViewLayout.CheckedChanged += new System.EventHandler(this.permissionCheckBox_CheckedChanged);
			// 
			// chkModifyConnect
			// 
			this.chkModifyConnect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
			this.chkModifyConnect.AutoSize = true;
			this.chkModifyConnect.Location = new System.Drawing.Point(224, 163);
			this.chkModifyConnect.Name = "chkModifyConnect";
			this.chkModifyConnect.Size = new System.Drawing.Size(15, 14);
			this.chkModifyConnect.TabIndex = 28;
			this.chkModifyConnect.UseVisualStyleBackColor = true;
			this.chkModifyConnect.CheckedChanged += new System.EventHandler(this.permissionCheckBox_CheckedChanged);
			// 
			// chkViewConnect
			// 
			this.chkViewConnect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
			this.chkViewConnect.AutoSize = true;
			this.chkViewConnect.Location = new System.Drawing.Point(179, 163);
			this.chkViewConnect.Name = "chkViewConnect";
			this.chkViewConnect.Size = new System.Drawing.Size(15, 14);
			this.chkViewConnect.TabIndex = 27;
			this.chkViewConnect.UseVisualStyleBackColor = true;
			this.chkViewConnect.CheckedChanged += new System.EventHandler(this.permissionCheckBox_CheckedChanged);
			// 
			// chkModifyDelete
			// 
			this.chkModifyDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
			this.chkModifyDelete.AutoSize = true;
			this.chkModifyDelete.Location = new System.Drawing.Point(224, 143);
			this.chkModifyDelete.Name = "chkModifyDelete";
			this.chkModifyDelete.Size = new System.Drawing.Size(15, 14);
			this.chkModifyDelete.TabIndex = 26;
			this.chkModifyDelete.UseVisualStyleBackColor = true;
			this.chkModifyDelete.CheckedChanged += new System.EventHandler(this.permissionCheckBox_CheckedChanged);
			// 
			// chkViewDelete
			// 
			this.chkViewDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
			this.chkViewDelete.AutoSize = true;
			this.chkViewDelete.Location = new System.Drawing.Point(179, 143);
			this.chkViewDelete.Name = "chkViewDelete";
			this.chkViewDelete.Size = new System.Drawing.Size(15, 14);
			this.chkViewDelete.TabIndex = 25;
			this.chkViewDelete.UseVisualStyleBackColor = true;
			this.chkViewDelete.CheckedChanged += new System.EventHandler(this.permissionCheckBox_CheckedChanged);
			// 
			// chkModifyInsert
			// 
			this.chkModifyInsert.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
			this.chkModifyInsert.AutoSize = true;
			this.chkModifyInsert.Location = new System.Drawing.Point(224, 123);
			this.chkModifyInsert.Name = "chkModifyInsert";
			this.chkModifyInsert.Size = new System.Drawing.Size(15, 14);
			this.chkModifyInsert.TabIndex = 24;
			this.chkModifyInsert.UseVisualStyleBackColor = true;
			this.chkModifyInsert.CheckedChanged += new System.EventHandler(this.permissionCheckBox_CheckedChanged);
			// 
			// chkViewInsert
			// 
			this.chkViewInsert.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
			this.chkViewInsert.AutoSize = true;
			this.chkViewInsert.Location = new System.Drawing.Point(179, 123);
			this.chkViewInsert.Name = "chkViewInsert";
			this.chkViewInsert.Size = new System.Drawing.Size(15, 14);
			this.chkViewInsert.TabIndex = 23;
			this.chkViewInsert.UseVisualStyleBackColor = true;
			this.chkViewInsert.CheckedChanged += new System.EventHandler(this.permissionCheckBox_CheckedChanged);
			// 
			// chkModifyTemplates
			// 
			this.chkModifyTemplates.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
			this.chkModifyTemplates.AutoSize = true;
			this.chkModifyTemplates.Location = new System.Drawing.Point(224, 83);
			this.chkModifyTemplates.Name = "chkModifyTemplates";
			this.chkModifyTemplates.Size = new System.Drawing.Size(15, 14);
			this.chkModifyTemplates.TabIndex = 22;
			this.chkModifyTemplates.UseVisualStyleBackColor = true;
			this.chkModifyTemplates.CheckedChanged += new System.EventHandler(this.permissionCheckBox_CheckedChanged);
			// 
			// chkViewTemplates
			// 
			this.chkViewTemplates.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
			this.chkViewTemplates.AutoSize = true;
			this.chkViewTemplates.Location = new System.Drawing.Point(179, 83);
			this.chkViewTemplates.Name = "chkViewTemplates";
			this.chkViewTemplates.Size = new System.Drawing.Size(15, 14);
			this.chkViewTemplates.TabIndex = 21;
			this.chkViewTemplates.UseVisualStyleBackColor = true;
			this.chkViewTemplates.CheckedChanged += new System.EventHandler(this.permissionCheckBox_CheckedChanged);
			// 
			// chkModifySecurity
			// 
			this.chkModifySecurity.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
			this.chkModifySecurity.AutoSize = true;
			this.chkModifySecurity.Location = new System.Drawing.Point(224, 63);
			this.chkModifySecurity.Name = "chkModifySecurity";
			this.chkModifySecurity.Size = new System.Drawing.Size(15, 14);
			this.chkModifySecurity.TabIndex = 20;
			this.chkModifySecurity.UseVisualStyleBackColor = true;
			this.chkModifySecurity.CheckedChanged += new System.EventHandler(this.permissionCheckBox_CheckedChanged);
			// 
			// chkViewSecurity
			// 
			this.chkViewSecurity.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
			this.chkViewSecurity.AutoSize = true;
			this.chkViewSecurity.Location = new System.Drawing.Point(179, 63);
			this.chkViewSecurity.Name = "chkViewSecurity";
			this.chkViewSecurity.Size = new System.Drawing.Size(15, 14);
			this.chkViewSecurity.TabIndex = 19;
			this.chkViewSecurity.UseVisualStyleBackColor = true;
			this.chkViewSecurity.CheckedChanged += new System.EventHandler(this.permissionCheckBox_CheckedChanged);
			// 
			// chkModifyDesigns
			// 
			this.chkModifyDesigns.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
			this.chkModifyDesigns.AutoSize = true;
			this.chkModifyDesigns.Location = new System.Drawing.Point(224, 43);
			this.chkModifyDesigns.Name = "chkModifyDesigns";
			this.chkModifyDesigns.Size = new System.Drawing.Size(15, 14);
			this.chkModifyDesigns.TabIndex = 18;
			this.chkModifyDesigns.UseVisualStyleBackColor = true;
			this.chkModifyDesigns.CheckedChanged += new System.EventHandler(this.permissionCheckBox_CheckedChanged);
			// 
			// chkViewDesigns
			// 
			this.chkViewDesigns.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
			this.chkViewDesigns.AutoSize = true;
			this.chkViewDesigns.Location = new System.Drawing.Point(179, 43);
			this.chkViewDesigns.Name = "chkViewDesigns";
			this.chkViewDesigns.Size = new System.Drawing.Size(15, 14);
			this.chkViewDesigns.TabIndex = 17;
			this.chkViewDesigns.UseVisualStyleBackColor = true;
			this.chkViewDesigns.CheckedChanged += new System.EventHandler(this.permissionCheckBox_CheckedChanged);
			// 
			// lblGeneralPermissions
			// 
			this.lblGeneralPermissions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.permissionsPanel.SetColumnSpan(this.lblGeneralPermissions, 3);
			this.lblGeneralPermissions.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblGeneralPermissions.Location = new System.Drawing.Point(3, 20);
			this.lblGeneralPermissions.Name = "lblGeneralPermissions";
			this.lblGeneralPermissions.Size = new System.Drawing.Size(251, 20);
			this.lblGeneralPermissions.TabIndex = 14;
			this.lblGeneralPermissions.Text = "General Permissions";
			this.lblGeneralPermissions.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// lblPermissionDesigns
			// 
			this.lblPermissionDesigns.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.lblPermissionDesigns.AutoSize = true;
			this.lblPermissionDesigns.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblPermissionDesigns.Location = new System.Drawing.Point(3, 40);
			this.lblPermissionDesigns.Name = "lblPermissionDesigns";
			this.lblPermissionDesigns.Size = new System.Drawing.Size(45, 20);
			this.lblPermissionDesigns.TabIndex = 11;
			this.lblPermissionDesigns.Text = "Designs";
			this.lblPermissionDesigns.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// lblPermissionSecurity
			// 
			this.lblPermissionSecurity.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.lblPermissionSecurity.AutoSize = true;
			this.lblPermissionSecurity.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblPermissionSecurity.Location = new System.Drawing.Point(3, 60);
			this.lblPermissionSecurity.Name = "lblPermissionSecurity";
			this.lblPermissionSecurity.Size = new System.Drawing.Size(45, 20);
			this.lblPermissionSecurity.TabIndex = 3;
			this.lblPermissionSecurity.Text = "Security";
			this.lblPermissionSecurity.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// lblPermissionTemplates
			// 
			this.lblPermissionTemplates.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.lblPermissionTemplates.AutoSize = true;
			this.lblPermissionTemplates.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblPermissionTemplates.Location = new System.Drawing.Point(3, 80);
			this.lblPermissionTemplates.Name = "lblPermissionTemplates";
			this.lblPermissionTemplates.Size = new System.Drawing.Size(56, 20);
			this.lblPermissionTemplates.TabIndex = 10;
			this.lblPermissionTemplates.Text = "Templates";
			this.lblPermissionTemplates.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// lblPermissionPresent
			// 
			this.lblPermissionPresent.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.lblPermissionPresent.AutoSize = true;
			this.lblPermissionPresent.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblPermissionPresent.Location = new System.Drawing.Point(3, 220);
			this.lblPermissionPresent.Name = "lblPermissionPresent";
			this.lblPermissionPresent.Size = new System.Drawing.Size(43, 20);
			this.lblPermissionPresent.TabIndex = 5;
			this.lblPermissionPresent.Text = "Present";
			this.lblPermissionPresent.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// lblPermissionDelete
			// 
			this.lblPermissionDelete.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.lblPermissionDelete.AutoSize = true;
			this.lblPermissionDelete.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblPermissionDelete.Location = new System.Drawing.Point(3, 140);
			this.lblPermissionDelete.Name = "lblPermissionDelete";
			this.lblPermissionDelete.Size = new System.Drawing.Size(38, 20);
			this.lblPermissionDelete.TabIndex = 8;
			this.lblPermissionDelete.Text = "Delete";
			this.lblPermissionDelete.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// lblPermissionInsert
			// 
			this.lblPermissionInsert.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.lblPermissionInsert.AutoSize = true;
			this.lblPermissionInsert.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblPermissionInsert.Location = new System.Drawing.Point(3, 120);
			this.lblPermissionInsert.Name = "lblPermissionInsert";
			this.lblPermissionInsert.Size = new System.Drawing.Size(33, 20);
			this.lblPermissionInsert.TabIndex = 7;
			this.lblPermissionInsert.Text = "Insert";
			this.lblPermissionInsert.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// lblPermissionConnect
			// 
			this.lblPermissionConnect.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.lblPermissionConnect.AutoSize = true;
			this.lblPermissionConnect.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblPermissionConnect.Location = new System.Drawing.Point(3, 160);
			this.lblPermissionConnect.Name = "lblPermissionConnect";
			this.lblPermissionConnect.Size = new System.Drawing.Size(47, 20);
			this.lblPermissionConnect.TabIndex = 9;
			this.lblPermissionConnect.Text = "Connect";
			this.lblPermissionConnect.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// lblPermissionLayout
			// 
			this.lblPermissionLayout.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.lblPermissionLayout.AutoSize = true;
			this.lblPermissionLayout.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblPermissionLayout.Location = new System.Drawing.Point(3, 180);
			this.lblPermissionLayout.Name = "lblPermissionLayout";
			this.lblPermissionLayout.Size = new System.Drawing.Size(39, 20);
			this.lblPermissionLayout.TabIndex = 4;
			this.lblPermissionLayout.Text = "Layout";
			this.lblPermissionLayout.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// lblPermissionData
			// 
			this.lblPermissionData.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.lblPermissionData.AutoSize = true;
			this.lblPermissionData.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblPermissionData.Location = new System.Drawing.Point(3, 200);
			this.lblPermissionData.Name = "lblPermissionData";
			this.lblPermissionData.Size = new System.Drawing.Size(30, 20);
			this.lblPermissionData.TabIndex = 6;
			this.lblPermissionData.Text = "Data";
			this.lblPermissionData.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// lblPermissions
			// 
			this.lblPermissions.AutoSize = true;
			this.lblPermissions.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblPermissions.Location = new System.Drawing.Point(3, 0);
			this.lblPermissions.Name = "lblPermissions";
			this.lblPermissions.Size = new System.Drawing.Size(67, 13);
			this.lblPermissions.TabIndex = 40;
			this.lblPermissions.Text = "Permission";
			this.lblPermissions.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// lblViewAccess
			// 
			this.lblViewAccess.AutoSize = true;
			this.lblViewAccess.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblViewAccess.Location = new System.Drawing.Point(170, 0);
			this.lblViewAccess.Name = "lblViewAccess";
			this.lblViewAccess.Size = new System.Drawing.Size(34, 13);
			this.lblViewAccess.TabIndex = 41;
			this.lblViewAccess.Text = "View";
			this.lblViewAccess.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// lblModifyAccess
			// 
			this.lblModifyAccess.AutoSize = true;
			this.lblModifyAccess.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblModifyAccess.Location = new System.Drawing.Point(210, 0);
			this.lblModifyAccess.Name = "lblModifyAccess";
			this.lblModifyAccess.Size = new System.Drawing.Size(44, 13);
			this.lblModifyAccess.TabIndex = 42;
			this.lblModifyAccess.Text = "Modify";
			this.lblModifyAccess.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// SecuritySettingsEditor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(281, 371);
			this.Controls.Add(this.permissionsPanel);
			this.Controls.Add(this.cboUser);
			this.Controls.Add(this.cboDomain);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.label6);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SecuritySettingsEditor";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Edit Permissions";
			this.permissionsPanel.ResumeLayout(false);
			this.permissionsPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox cboDomain;
		private System.Windows.Forms.ComboBox cboUser;
		private System.Windows.Forms.TableLayoutPanel permissionsPanel;
		private System.Windows.Forms.Label lblDomainPermissions;
		private System.Windows.Forms.CheckBox chkModifyPresent;
		private System.Windows.Forms.CheckBox chkViewPresent;
		private System.Windows.Forms.CheckBox chkModifyData;
		private System.Windows.Forms.CheckBox chkViewData;
		private System.Windows.Forms.CheckBox chkModifyLayout;
		private System.Windows.Forms.CheckBox chkViewLayout;
		private System.Windows.Forms.CheckBox chkModifyConnect;
		private System.Windows.Forms.CheckBox chkViewConnect;
		private System.Windows.Forms.CheckBox chkModifyDelete;
		private System.Windows.Forms.CheckBox chkViewDelete;
		private System.Windows.Forms.CheckBox chkModifyInsert;
		private System.Windows.Forms.CheckBox chkViewInsert;
		private System.Windows.Forms.CheckBox chkModifyTemplates;
		private System.Windows.Forms.CheckBox chkViewTemplates;
		private System.Windows.Forms.CheckBox chkModifySecurity;
		private System.Windows.Forms.CheckBox chkViewSecurity;
		private System.Windows.Forms.CheckBox chkModifyDesigns;
		private System.Windows.Forms.CheckBox chkViewDesigns;
		private System.Windows.Forms.Label lblGeneralPermissions;
		private System.Windows.Forms.Label lblPermissionDesigns;
		private System.Windows.Forms.Label lblPermissionSecurity;
		private System.Windows.Forms.Label lblPermissionTemplates;
		private System.Windows.Forms.Label lblPermissionPresent;
		private System.Windows.Forms.Label lblPermissionDelete;
		private System.Windows.Forms.Label lblPermissionInsert;
		private System.Windows.Forms.Label lblPermissionConnect;
		private System.Windows.Forms.Label lblPermissionLayout;
		private System.Windows.Forms.Label lblPermissionData;
		private System.Windows.Forms.Label lblPermissions;
		private System.Windows.Forms.Label lblViewAccess;
		private System.Windows.Forms.Label lblModifyAccess;
	}
}