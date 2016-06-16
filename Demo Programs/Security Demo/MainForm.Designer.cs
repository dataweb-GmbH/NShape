namespace Security_Demo {
	partial class MainForm {
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			Dataweb.NShape.RoleBasedSecurityManager roleBasedSecurityManager3 = new Dataweb.NShape.RoleBasedSecurityManager();
			this.diagramSetController = new Dataweb.NShape.Controllers.DiagramSetController();
			this.project = new Dataweb.NShape.Project(this.components);
			this.cachedRepository = new Dataweb.NShape.Advanced.CachedRepository();
			this.xmlStore = new Dataweb.NShape.XmlStore();
			this.propertyController = new Dataweb.NShape.Controllers.PropertyController();
			this.toolSetController = new Dataweb.NShape.Controllers.ToolSetController();
			this.listView1 = new System.Windows.Forms.ListView();
			this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
			this.splitContainer2 = new System.Windows.Forms.SplitContainer();
			this.display1 = new Dataweb.NShape.WinFormsUI.Display();
			this.splitContainer3 = new System.Windows.Forms.SplitContainer();
			this.toolboxGroupBox = new System.Windows.Forms.GroupBox();
			this.GeneralPermissionsPanel = new System.Windows.Forms.TableLayoutPanel();
			this.lblCurrentGenPerm = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.propertiesGroupBox = new System.Windows.Forms.GroupBox();
			this.ObjectPermissionsPanel = new System.Windows.Forms.TableLayoutPanel();
			this.label4 = new System.Windows.Forms.Label();
			this.lblCurrentSecObj = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.lblCurrentDomain = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.lblCurrentDomPerm = new System.Windows.Forms.Label();
			this.securitySettingsPanel = new System.Windows.Forms.Panel();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.cboUser = new System.Windows.Forms.ComboBox();
			this.ItemVisibilityGroupBox = new System.Windows.Forms.GroupBox();
			this.chkHideProperties = new System.Windows.Forms.CheckBox();
			this.chkHideMenuItems = new System.Windows.Forms.CheckBox();
			this.editPermissionsButton = new System.Windows.Forms.Button();
			this.PermissionsGroupBox = new System.Windows.Forms.GroupBox();
			this.domainPermissionInfoPanel = new System.Windows.Forms.TableLayoutPanel();
			this.label1 = new System.Windows.Forms.Label();
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
			this.lblModifyAccess = new System.Windows.Forms.Label();
			this.lblViewAccess = new System.Windows.Forms.Label();
			this.cboDomain = new System.Windows.Forms.ComboBox();
			this.panel1 = new System.Windows.Forms.Panel();
			this.splitter1 = new System.Windows.Forms.Splitter();
			this.toolSetListViewPresenter = new Dataweb.NShape.WinFormsUI.ToolSetListViewPresenter(this.components);
			this.propertyPresenter = new Dataweb.NShape.WinFormsUI.PropertyPresenter();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
			this.splitContainer2.Panel1.SuspendLayout();
			this.splitContainer2.Panel2.SuspendLayout();
			this.splitContainer2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
			this.splitContainer3.Panel1.SuspendLayout();
			this.splitContainer3.Panel2.SuspendLayout();
			this.splitContainer3.SuspendLayout();
			this.toolboxGroupBox.SuspendLayout();
			this.GeneralPermissionsPanel.SuspendLayout();
			this.propertiesGroupBox.SuspendLayout();
			this.ObjectPermissionsPanel.SuspendLayout();
			this.securitySettingsPanel.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.ItemVisibilityGroupBox.SuspendLayout();
			this.PermissionsGroupBox.SuspendLayout();
			this.domainPermissionInfoPanel.SuspendLayout();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// diagramSetController
			// 
			this.diagramSetController.ActiveTool = null;
			this.diagramSetController.Project = this.project;
			// 
			// project
			// 
			this.project.AutoLoadLibraries = true;
			this.project.LibrarySearchPaths = ((System.Collections.Generic.IList<string>)(resources.GetObject("project.LibrarySearchPaths")));
			this.project.Name = null;
			this.project.Repository = this.cachedRepository;
			roleBasedSecurityManager3.CurrentRole = Dataweb.NShape.StandardRole.Administrator;
			roleBasedSecurityManager3.CurrentRoleName = "Administrator";
			this.project.SecurityManager = roleBasedSecurityManager3;
			// 
			// cachedRepository
			// 
			this.cachedRepository.ProjectName = null;
			this.cachedRepository.Store = this.xmlStore;
			this.cachedRepository.Version = 0;
			// 
			// xmlStore
			// 
			this.xmlStore.DesignFileName = "";
			this.xmlStore.DirectoryName = "";
			this.xmlStore.FileExtension = ".xml";
			this.xmlStore.ProjectName = "";
			// 
			// propertyController
			// 
			this.propertyController.Project = this.project;
			this.propertyController.PropertyDisplayMode = Dataweb.NShape.Controllers.NonEditableDisplayMode.ReadOnly;
			this.propertyController.ObjectsSet += new System.EventHandler<Dataweb.NShape.Controllers.PropertyControllerEventArgs>(this.propertyController1_ObjectsSet);
			// 
			// toolSetController
			// 
			this.toolSetController.DiagramSetController = this.diagramSetController;
			this.toolSetController.DesignEditorSelected += new System.EventHandler(this.toolSetController1_DesignEditorSelected);
			this.toolSetController.LibraryManagerSelected += new System.EventHandler(this.toolSetController1_LibraryManagerSelected);
			this.toolSetController.TemplateEditorSelected += new System.EventHandler<Dataweb.NShape.Controllers.TemplateEditorEventArgs>(this.toolSetController1_TemplateEditorSelected);
			// 
			// listView1
			// 
			this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.listView1.FullRowSelect = true;
			this.listView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.listView1.HideSelection = false;
			this.listView1.Location = new System.Drawing.Point(3, 57);
			this.listView1.MultiSelect = false;
			this.listView1.Name = "listView1";
			this.listView1.ShowItemToolTips = true;
			this.listView1.Size = new System.Drawing.Size(301, 170);
			this.listView1.TabIndex = 0;
			this.listView1.UseCompatibleStateImageBehavior = false;
			// 
			// propertyGrid1
			// 
			this.propertyGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.propertyGrid1.Location = new System.Drawing.Point(3, 74);
			this.propertyGrid1.Name = "propertyGrid1";
			this.propertyGrid1.Size = new System.Drawing.Size(301, 360);
			this.propertyGrid1.TabIndex = 0;
			// 
			// splitContainer2
			// 
			this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer2.Location = new System.Drawing.Point(203, 0);
			this.splitContainer2.Name = "splitContainer2";
			// 
			// splitContainer2.Panel1
			// 
			this.splitContainer2.Panel1.Controls.Add(this.display1);
			// 
			// splitContainer2.Panel2
			// 
			this.splitContainer2.Panel2.Controls.Add(this.splitContainer3);
			this.splitContainer2.Size = new System.Drawing.Size(1061, 682);
			this.splitContainer2.SplitterDistance = 748;
			this.splitContainer2.TabIndex = 0;
			// 
			// display1
			// 
			this.display1.AllowDrop = true;
			this.display1.BackColorGradient = System.Drawing.SystemColors.Control;
			this.display1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.display1.DiagramSetController = this.diagramSetController;
			this.display1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.display1.GridColor = System.Drawing.Color.Gainsboro;
			this.display1.GridSize = 19;
			this.display1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.display1.Location = new System.Drawing.Point(0, 0);
			this.display1.Name = "display1";
			this.display1.PropertyController = this.propertyController;
			this.display1.SelectionHilightColor = System.Drawing.Color.Firebrick;
			this.display1.SelectionInactiveColor = System.Drawing.Color.Gray;
			this.display1.SelectionInteriorColor = System.Drawing.Color.WhiteSmoke;
			this.display1.SelectionNormalColor = System.Drawing.Color.DarkGreen;
			this.display1.Size = new System.Drawing.Size(748, 682);
			this.display1.TabIndex = 0;
			this.display1.ToolPreviewBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(119)))), ((int)(((byte)(136)))), ((int)(((byte)(153)))));
			this.display1.ToolPreviewColor = System.Drawing.Color.FromArgb(((int)(((byte)(96)))), ((int)(((byte)(70)))), ((int)(((byte)(130)))), ((int)(((byte)(180)))));
			// 
			// splitContainer3
			// 
			this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer3.Location = new System.Drawing.Point(0, 0);
			this.splitContainer3.Name = "splitContainer3";
			this.splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer3.Panel1
			// 
			this.splitContainer3.Panel1.Controls.Add(this.toolboxGroupBox);
			// 
			// splitContainer3.Panel2
			// 
			this.splitContainer3.Panel2.Controls.Add(this.propertiesGroupBox);
			this.splitContainer3.Size = new System.Drawing.Size(309, 682);
			this.splitContainer3.SplitterDistance = 238;
			this.splitContainer3.TabIndex = 0;
			// 
			// toolboxGroupBox
			// 
			this.toolboxGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.toolboxGroupBox.Controls.Add(this.GeneralPermissionsPanel);
			this.toolboxGroupBox.Controls.Add(this.listView1);
			this.toolboxGroupBox.Location = new System.Drawing.Point(0, 3);
			this.toolboxGroupBox.Name = "toolboxGroupBox";
			this.toolboxGroupBox.Size = new System.Drawing.Size(306, 233);
			this.toolboxGroupBox.TabIndex = 2;
			this.toolboxGroupBox.TabStop = false;
			this.toolboxGroupBox.Text = "Toolbox";
			// 
			// GeneralPermissionsPanel
			// 
			this.GeneralPermissionsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.GeneralPermissionsPanel.ColumnCount = 1;
			this.GeneralPermissionsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.GeneralPermissionsPanel.Controls.Add(this.lblCurrentGenPerm, 0, 1);
			this.GeneralPermissionsPanel.Controls.Add(this.label5, 0, 0);
			this.GeneralPermissionsPanel.Location = new System.Drawing.Point(3, 18);
			this.GeneralPermissionsPanel.Name = "GeneralPermissionsPanel";
			this.GeneralPermissionsPanel.RowCount = 3;
			this.GeneralPermissionsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.GeneralPermissionsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.GeneralPermissionsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.GeneralPermissionsPanel.Size = new System.Drawing.Size(301, 27);
			this.GeneralPermissionsPanel.TabIndex = 1;
			// 
			// lblCurrentGenPerm
			// 
			this.lblCurrentGenPerm.AutoSize = true;
			this.lblCurrentGenPerm.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblCurrentGenPerm.Location = new System.Drawing.Point(3, 13);
			this.lblCurrentGenPerm.Name = "lblCurrentGenPerm";
			this.lblCurrentGenPerm.Size = new System.Drawing.Size(144, 13);
			this.lblCurrentGenPerm.TabIndex = 6;
			this.lblCurrentGenPerm.Text = "Security, Templates, Designs";
			// 
			// label5
			// 
			this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label5.AutoSize = true;
			this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label5.Location = new System.Drawing.Point(3, 0);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(125, 13);
			this.label5.TabIndex = 5;
			this.label5.Text = "General Permissions:";
			// 
			// propertiesGroupBox
			// 
			this.propertiesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.propertiesGroupBox.Controls.Add(this.propertyGrid1);
			this.propertiesGroupBox.Controls.Add(this.ObjectPermissionsPanel);
			this.propertiesGroupBox.Location = new System.Drawing.Point(0, 0);
			this.propertiesGroupBox.Name = "propertiesGroupBox";
			this.propertiesGroupBox.Size = new System.Drawing.Size(306, 440);
			this.propertiesGroupBox.TabIndex = 2;
			this.propertiesGroupBox.TabStop = false;
			this.propertiesGroupBox.Text = "Property Editor";
			// 
			// ObjectPermissionsPanel
			// 
			this.ObjectPermissionsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ObjectPermissionsPanel.ColumnCount = 2;
			this.ObjectPermissionsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.ObjectPermissionsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.ObjectPermissionsPanel.Controls.Add(this.label4, 0, 0);
			this.ObjectPermissionsPanel.Controls.Add(this.lblCurrentSecObj, 1, 0);
			this.ObjectPermissionsPanel.Controls.Add(this.label2, 0, 1);
			this.ObjectPermissionsPanel.Controls.Add(this.lblCurrentDomain, 1, 1);
			this.ObjectPermissionsPanel.Controls.Add(this.label3, 0, 2);
			this.ObjectPermissionsPanel.Controls.Add(this.lblCurrentDomPerm, 1, 2);
			this.ObjectPermissionsPanel.Location = new System.Drawing.Point(3, 19);
			this.ObjectPermissionsPanel.Name = "ObjectPermissionsPanel";
			this.ObjectPermissionsPanel.RowCount = 3;
			this.ObjectPermissionsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.ObjectPermissionsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.ObjectPermissionsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.ObjectPermissionsPanel.Size = new System.Drawing.Size(301, 60);
			this.ObjectPermissionsPanel.TabIndex = 1;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label4.Location = new System.Drawing.Point(3, 0);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(48, 13);
			this.label4.TabIndex = 3;
			this.label4.Text = "Object:";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// lblCurrentSecObj
			// 
			this.lblCurrentSecObj.AutoSize = true;
			this.lblCurrentSecObj.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblCurrentSecObj.Location = new System.Drawing.Point(86, 0);
			this.lblCurrentSecObj.Name = "lblCurrentSecObj";
			this.lblCurrentSecObj.Size = new System.Drawing.Size(126, 13);
			this.lblCurrentSecObj.TabIndex = 6;
			this.lblCurrentSecObj.Text = "Dataweb.NShape.Shape";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.Location = new System.Drawing.Point(3, 13);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(53, 13);
			this.label2.TabIndex = 1;
			this.label2.Text = "Domain:";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// lblCurrentDomain
			// 
			this.lblCurrentDomain.AutoSize = true;
			this.lblCurrentDomain.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblCurrentDomain.Location = new System.Drawing.Point(86, 13);
			this.lblCurrentDomain.Name = "lblCurrentDomain";
			this.lblCurrentDomain.Size = new System.Drawing.Size(14, 13);
			this.lblCurrentDomain.TabIndex = 7;
			this.lblCurrentDomain.Text = "A";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label3.Location = new System.Drawing.Point(3, 26);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(77, 13);
			this.label3.TabIndex = 2;
			this.label3.Text = "Permissions:";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// lblCurrentDomPerm
			// 
			this.lblCurrentDomPerm.AutoSize = true;
			this.lblCurrentDomPerm.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblCurrentDomPerm.Location = new System.Drawing.Point(86, 26);
			this.lblCurrentDomPerm.Name = "lblCurrentDomPerm";
			this.lblCurrentDomPerm.Size = new System.Drawing.Size(189, 26);
			this.lblCurrentDomPerm.TabIndex = 8;
			this.lblCurrentDomPerm.Text = "Insert, Delete, Connect, Layout, Data, Present";
			// 
			// securitySettingsPanel
			// 
			this.securitySettingsPanel.Controls.Add(this.groupBox1);
			this.securitySettingsPanel.Controls.Add(this.ItemVisibilityGroupBox);
			this.securitySettingsPanel.Controls.Add(this.editPermissionsButton);
			this.securitySettingsPanel.Controls.Add(this.PermissionsGroupBox);
			this.securitySettingsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.securitySettingsPanel.Location = new System.Drawing.Point(0, 0);
			this.securitySettingsPanel.Name = "securitySettingsPanel";
			this.securitySettingsPanel.Size = new System.Drawing.Size(200, 682);
			this.securitySettingsPanel.TabIndex = 43;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.cboUser);
			this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.groupBox1.Location = new System.Drawing.Point(3, 140);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(197, 50);
			this.groupBox1.TabIndex = 6;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "User Role";
			// 
			// cboUser
			// 
			this.cboUser.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.cboUser.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboUser.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.cboUser.FormattingEnabled = true;
			this.cboUser.Location = new System.Drawing.Point(9, 19);
			this.cboUser.Name = "cboUser";
			this.cboUser.Size = new System.Drawing.Size(185, 21);
			this.cboUser.TabIndex = 1;
			this.cboUser.SelectedIndexChanged += new System.EventHandler(this.userOrDomainCheckBox_SelectedIndexChanged);
			// 
			// ItemVisibilityGroupBox
			// 
			this.ItemVisibilityGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ItemVisibilityGroupBox.Controls.Add(this.chkHideProperties);
			this.ItemVisibilityGroupBox.Controls.Add(this.chkHideMenuItems);
			this.ItemVisibilityGroupBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ItemVisibilityGroupBox.Location = new System.Drawing.Point(3, 12);
			this.ItemVisibilityGroupBox.Name = "ItemVisibilityGroupBox";
			this.ItemVisibilityGroupBox.Size = new System.Drawing.Size(197, 66);
			this.ItemVisibilityGroupBox.TabIndex = 2;
			this.ItemVisibilityGroupBox.TabStop = false;
			this.ItemVisibilityGroupBox.Text = "Item Visibility Settings";
			// 
			// chkHideProperties
			// 
			this.chkHideProperties.AutoSize = true;
			this.chkHideProperties.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.chkHideProperties.Location = new System.Drawing.Point(9, 42);
			this.chkHideProperties.Name = "chkHideProperties";
			this.chkHideProperties.Size = new System.Drawing.Size(168, 17);
			this.chkHideProperties.TabIndex = 3;
			this.chkHideProperties.Text = "Hide Properties If Not Granted";
			this.chkHideProperties.UseVisualStyleBackColor = true;
			this.chkHideProperties.CheckedChanged += new System.EventHandler(this.chkHideProperties_CheckedChanged);
			// 
			// chkHideMenuItems
			// 
			this.chkHideMenuItems.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.chkHideMenuItems.AutoSize = true;
			this.chkHideMenuItems.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.chkHideMenuItems.Location = new System.Drawing.Point(9, 19);
			this.chkHideMenuItems.Name = "chkHideMenuItems";
			this.chkHideMenuItems.Size = new System.Drawing.Size(176, 17);
			this.chkHideMenuItems.TabIndex = 2;
			this.chkHideMenuItems.Text = "Hide Menu Items If Not Granted";
			this.chkHideMenuItems.UseVisualStyleBackColor = true;
			this.chkHideMenuItems.CheckedChanged += new System.EventHandler(this.chkHideMenuItems_CheckedChanged);
			// 
			// editPermissionsButton
			// 
			this.editPermissionsButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.editPermissionsButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.editPermissionsButton.Location = new System.Drawing.Point(12, 97);
			this.editPermissionsButton.Name = "editPermissionsButton";
			this.editPermissionsButton.Size = new System.Drawing.Size(185, 23);
			this.editPermissionsButton.TabIndex = 5;
			this.editPermissionsButton.Text = "Edit Permissions...";
			this.editPermissionsButton.UseVisualStyleBackColor = true;
			this.editPermissionsButton.Click += new System.EventHandler(this.editPermissionsButton_Click);
			// 
			// PermissionsGroupBox
			// 
			this.PermissionsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.PermissionsGroupBox.Controls.Add(this.domainPermissionInfoPanel);
			this.PermissionsGroupBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.PermissionsGroupBox.Location = new System.Drawing.Point(3, 215);
			this.PermissionsGroupBox.Name = "PermissionsGroupBox";
			this.PermissionsGroupBox.Size = new System.Drawing.Size(197, 464);
			this.PermissionsGroupBox.TabIndex = 1;
			this.PermissionsGroupBox.TabStop = false;
			this.PermissionsGroupBox.Text = "Permissions";
			// 
			// domainPermissionInfoPanel
			// 
			this.domainPermissionInfoPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.domainPermissionInfoPanel.ColumnCount = 3;
			this.domainPermissionInfoPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.domainPermissionInfoPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.domainPermissionInfoPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.domainPermissionInfoPanel.Controls.Add(this.label1, 0, 6);
			this.domainPermissionInfoPanel.Controls.Add(this.lblDomainPermissions, 0, 8);
			this.domainPermissionInfoPanel.Controls.Add(this.chkModifyPresent, 2, 14);
			this.domainPermissionInfoPanel.Controls.Add(this.chkViewPresent, 1, 14);
			this.domainPermissionInfoPanel.Controls.Add(this.chkModifyData, 2, 13);
			this.domainPermissionInfoPanel.Controls.Add(this.chkViewData, 1, 13);
			this.domainPermissionInfoPanel.Controls.Add(this.chkModifyLayout, 2, 12);
			this.domainPermissionInfoPanel.Controls.Add(this.chkViewLayout, 1, 12);
			this.domainPermissionInfoPanel.Controls.Add(this.chkModifyConnect, 2, 11);
			this.domainPermissionInfoPanel.Controls.Add(this.chkViewConnect, 1, 11);
			this.domainPermissionInfoPanel.Controls.Add(this.chkModifyDelete, 2, 10);
			this.domainPermissionInfoPanel.Controls.Add(this.chkViewDelete, 1, 10);
			this.domainPermissionInfoPanel.Controls.Add(this.chkModifyInsert, 2, 9);
			this.domainPermissionInfoPanel.Controls.Add(this.chkViewInsert, 1, 9);
			this.domainPermissionInfoPanel.Controls.Add(this.chkModifyTemplates, 2, 4);
			this.domainPermissionInfoPanel.Controls.Add(this.chkViewTemplates, 1, 4);
			this.domainPermissionInfoPanel.Controls.Add(this.chkModifySecurity, 2, 3);
			this.domainPermissionInfoPanel.Controls.Add(this.chkViewSecurity, 1, 3);
			this.domainPermissionInfoPanel.Controls.Add(this.chkModifyDesigns, 2, 2);
			this.domainPermissionInfoPanel.Controls.Add(this.chkViewDesigns, 1, 2);
			this.domainPermissionInfoPanel.Controls.Add(this.lblGeneralPermissions, 0, 1);
			this.domainPermissionInfoPanel.Controls.Add(this.lblPermissionDesigns, 0, 2);
			this.domainPermissionInfoPanel.Controls.Add(this.lblPermissionSecurity, 0, 3);
			this.domainPermissionInfoPanel.Controls.Add(this.lblPermissionTemplates, 0, 4);
			this.domainPermissionInfoPanel.Controls.Add(this.lblPermissionPresent, 0, 14);
			this.domainPermissionInfoPanel.Controls.Add(this.lblPermissionDelete, 0, 10);
			this.domainPermissionInfoPanel.Controls.Add(this.lblPermissionInsert, 0, 9);
			this.domainPermissionInfoPanel.Controls.Add(this.lblPermissionConnect, 0, 11);
			this.domainPermissionInfoPanel.Controls.Add(this.lblPermissionLayout, 0, 12);
			this.domainPermissionInfoPanel.Controls.Add(this.lblPermissionData, 0, 13);
			this.domainPermissionInfoPanel.Controls.Add(this.lblPermissions, 0, 0);
			this.domainPermissionInfoPanel.Controls.Add(this.lblModifyAccess, 2, 0);
			this.domainPermissionInfoPanel.Controls.Add(this.lblViewAccess, 1, 0);
			this.domainPermissionInfoPanel.Controls.Add(this.cboDomain, 2, 6);
			this.domainPermissionInfoPanel.Location = new System.Drawing.Point(9, 46);
			this.domainPermissionInfoPanel.Name = "domainPermissionInfoPanel";
			this.domainPermissionInfoPanel.RowCount = 16;
			this.domainPermissionInfoPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.domainPermissionInfoPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.domainPermissionInfoPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.domainPermissionInfoPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.domainPermissionInfoPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.domainPermissionInfoPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.domainPermissionInfoPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.domainPermissionInfoPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.domainPermissionInfoPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.domainPermissionInfoPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.domainPermissionInfoPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.domainPermissionInfoPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.domainPermissionInfoPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.domainPermissionInfoPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.domainPermissionInfoPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.domainPermissionInfoPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.domainPermissionInfoPanel.Size = new System.Drawing.Size(185, 385);
			this.domainPermissionInfoPanel.TabIndex = 0;
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.label1.AutoSize = true;
			this.domainPermissionInfoPanel.SetColumnSpan(this.label1, 2);
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(3, 120);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(84, 27);
			this.label1.TabIndex = 3;
			this.label1.Text = "SecurityDomain:";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// lblDomainPermissions
			// 
			this.lblDomainPermissions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.domainPermissionInfoPanel.SetColumnSpan(this.lblDomainPermissions, 3);
			this.lblDomainPermissions.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblDomainPermissions.Location = new System.Drawing.Point(3, 167);
			this.lblDomainPermissions.Name = "lblDomainPermissions";
			this.lblDomainPermissions.Size = new System.Drawing.Size(179, 20);
			this.lblDomainPermissions.TabIndex = 39;
			this.lblDomainPermissions.Text = "Domain Permissions";
			this.lblDomainPermissions.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// chkModifyPresent
			// 
			this.chkModifyPresent.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
			this.chkModifyPresent.AutoCheck = false;
			this.chkModifyPresent.AutoSize = true;
			this.chkModifyPresent.Location = new System.Drawing.Point(152, 290);
			this.chkModifyPresent.Name = "chkModifyPresent";
			this.chkModifyPresent.Size = new System.Drawing.Size(15, 14);
			this.chkModifyPresent.TabIndex = 34;
			this.chkModifyPresent.UseVisualStyleBackColor = true;
			// 
			// chkViewPresent
			// 
			this.chkViewPresent.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
			this.chkViewPresent.AutoCheck = false;
			this.chkViewPresent.AutoSize = true;
			this.chkViewPresent.Location = new System.Drawing.Point(106, 290);
			this.chkViewPresent.Name = "chkViewPresent";
			this.chkViewPresent.Size = new System.Drawing.Size(15, 14);
			this.chkViewPresent.TabIndex = 33;
			this.chkViewPresent.UseVisualStyleBackColor = true;
			// 
			// chkModifyData
			// 
			this.chkModifyData.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
			this.chkModifyData.AutoCheck = false;
			this.chkModifyData.AutoSize = true;
			this.chkModifyData.Location = new System.Drawing.Point(152, 270);
			this.chkModifyData.Name = "chkModifyData";
			this.chkModifyData.Size = new System.Drawing.Size(15, 14);
			this.chkModifyData.TabIndex = 32;
			this.chkModifyData.UseVisualStyleBackColor = true;
			// 
			// chkViewData
			// 
			this.chkViewData.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
			this.chkViewData.AutoCheck = false;
			this.chkViewData.AutoSize = true;
			this.chkViewData.Location = new System.Drawing.Point(106, 270);
			this.chkViewData.Name = "chkViewData";
			this.chkViewData.Size = new System.Drawing.Size(15, 14);
			this.chkViewData.TabIndex = 31;
			this.chkViewData.UseVisualStyleBackColor = true;
			// 
			// chkModifyLayout
			// 
			this.chkModifyLayout.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
			this.chkModifyLayout.AutoCheck = false;
			this.chkModifyLayout.AutoSize = true;
			this.chkModifyLayout.Location = new System.Drawing.Point(152, 250);
			this.chkModifyLayout.Name = "chkModifyLayout";
			this.chkModifyLayout.Size = new System.Drawing.Size(15, 14);
			this.chkModifyLayout.TabIndex = 30;
			this.chkModifyLayout.UseVisualStyleBackColor = true;
			// 
			// chkViewLayout
			// 
			this.chkViewLayout.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
			this.chkViewLayout.AutoCheck = false;
			this.chkViewLayout.AutoSize = true;
			this.chkViewLayout.Location = new System.Drawing.Point(106, 250);
			this.chkViewLayout.Name = "chkViewLayout";
			this.chkViewLayout.Size = new System.Drawing.Size(15, 14);
			this.chkViewLayout.TabIndex = 29;
			this.chkViewLayout.UseVisualStyleBackColor = true;
			// 
			// chkModifyConnect
			// 
			this.chkModifyConnect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
			this.chkModifyConnect.AutoCheck = false;
			this.chkModifyConnect.AutoSize = true;
			this.chkModifyConnect.Location = new System.Drawing.Point(152, 230);
			this.chkModifyConnect.Name = "chkModifyConnect";
			this.chkModifyConnect.Size = new System.Drawing.Size(15, 14);
			this.chkModifyConnect.TabIndex = 28;
			this.chkModifyConnect.UseVisualStyleBackColor = true;
			// 
			// chkViewConnect
			// 
			this.chkViewConnect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
			this.chkViewConnect.AutoCheck = false;
			this.chkViewConnect.AutoSize = true;
			this.chkViewConnect.Location = new System.Drawing.Point(106, 230);
			this.chkViewConnect.Name = "chkViewConnect";
			this.chkViewConnect.Size = new System.Drawing.Size(15, 14);
			this.chkViewConnect.TabIndex = 27;
			this.chkViewConnect.UseVisualStyleBackColor = true;
			// 
			// chkModifyDelete
			// 
			this.chkModifyDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
			this.chkModifyDelete.AutoCheck = false;
			this.chkModifyDelete.AutoSize = true;
			this.chkModifyDelete.Location = new System.Drawing.Point(152, 210);
			this.chkModifyDelete.Name = "chkModifyDelete";
			this.chkModifyDelete.Size = new System.Drawing.Size(15, 14);
			this.chkModifyDelete.TabIndex = 26;
			this.chkModifyDelete.UseVisualStyleBackColor = true;
			// 
			// chkViewDelete
			// 
			this.chkViewDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
			this.chkViewDelete.AutoCheck = false;
			this.chkViewDelete.AutoSize = true;
			this.chkViewDelete.Location = new System.Drawing.Point(106, 210);
			this.chkViewDelete.Name = "chkViewDelete";
			this.chkViewDelete.Size = new System.Drawing.Size(15, 14);
			this.chkViewDelete.TabIndex = 25;
			this.chkViewDelete.UseVisualStyleBackColor = true;
			// 
			// chkModifyInsert
			// 
			this.chkModifyInsert.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
			this.chkModifyInsert.AutoCheck = false;
			this.chkModifyInsert.AutoSize = true;
			this.chkModifyInsert.Location = new System.Drawing.Point(152, 190);
			this.chkModifyInsert.Name = "chkModifyInsert";
			this.chkModifyInsert.Size = new System.Drawing.Size(15, 14);
			this.chkModifyInsert.TabIndex = 24;
			this.chkModifyInsert.UseVisualStyleBackColor = true;
			// 
			// chkViewInsert
			// 
			this.chkViewInsert.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
			this.chkViewInsert.AutoCheck = false;
			this.chkViewInsert.AutoSize = true;
			this.chkViewInsert.Location = new System.Drawing.Point(106, 190);
			this.chkViewInsert.Name = "chkViewInsert";
			this.chkViewInsert.Size = new System.Drawing.Size(15, 14);
			this.chkViewInsert.TabIndex = 23;
			this.chkViewInsert.UseVisualStyleBackColor = true;
			// 
			// chkModifyTemplates
			// 
			this.chkModifyTemplates.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
			this.chkModifyTemplates.AutoCheck = false;
			this.chkModifyTemplates.AutoSize = true;
			this.chkModifyTemplates.Location = new System.Drawing.Point(152, 83);
			this.chkModifyTemplates.Name = "chkModifyTemplates";
			this.chkModifyTemplates.Size = new System.Drawing.Size(15, 14);
			this.chkModifyTemplates.TabIndex = 22;
			this.chkModifyTemplates.UseVisualStyleBackColor = true;
			// 
			// chkViewTemplates
			// 
			this.chkViewTemplates.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
			this.chkViewTemplates.AutoCheck = false;
			this.chkViewTemplates.AutoSize = true;
			this.chkViewTemplates.Location = new System.Drawing.Point(106, 83);
			this.chkViewTemplates.Name = "chkViewTemplates";
			this.chkViewTemplates.Size = new System.Drawing.Size(15, 14);
			this.chkViewTemplates.TabIndex = 21;
			this.chkViewTemplates.UseVisualStyleBackColor = true;
			// 
			// chkModifySecurity
			// 
			this.chkModifySecurity.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
			this.chkModifySecurity.AutoCheck = false;
			this.chkModifySecurity.AutoSize = true;
			this.chkModifySecurity.Location = new System.Drawing.Point(152, 63);
			this.chkModifySecurity.Name = "chkModifySecurity";
			this.chkModifySecurity.Size = new System.Drawing.Size(15, 14);
			this.chkModifySecurity.TabIndex = 20;
			this.chkModifySecurity.UseVisualStyleBackColor = true;
			// 
			// chkViewSecurity
			// 
			this.chkViewSecurity.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
			this.chkViewSecurity.AutoCheck = false;
			this.chkViewSecurity.AutoSize = true;
			this.chkViewSecurity.Location = new System.Drawing.Point(106, 63);
			this.chkViewSecurity.Name = "chkViewSecurity";
			this.chkViewSecurity.Size = new System.Drawing.Size(15, 14);
			this.chkViewSecurity.TabIndex = 19;
			this.chkViewSecurity.UseVisualStyleBackColor = true;
			// 
			// chkModifyDesigns
			// 
			this.chkModifyDesigns.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
			this.chkModifyDesigns.AutoCheck = false;
			this.chkModifyDesigns.AutoSize = true;
			this.chkModifyDesigns.Location = new System.Drawing.Point(152, 43);
			this.chkModifyDesigns.Name = "chkModifyDesigns";
			this.chkModifyDesigns.Size = new System.Drawing.Size(15, 14);
			this.chkModifyDesigns.TabIndex = 18;
			this.chkModifyDesigns.UseVisualStyleBackColor = true;
			// 
			// chkViewDesigns
			// 
			this.chkViewDesigns.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
			this.chkViewDesigns.AutoCheck = false;
			this.chkViewDesigns.AutoSize = true;
			this.chkViewDesigns.Location = new System.Drawing.Point(106, 43);
			this.chkViewDesigns.Name = "chkViewDesigns";
			this.chkViewDesigns.Size = new System.Drawing.Size(15, 14);
			this.chkViewDesigns.TabIndex = 17;
			this.chkViewDesigns.UseVisualStyleBackColor = true;
			// 
			// lblGeneralPermissions
			// 
			this.lblGeneralPermissions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.domainPermissionInfoPanel.SetColumnSpan(this.lblGeneralPermissions, 3);
			this.lblGeneralPermissions.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblGeneralPermissions.Location = new System.Drawing.Point(3, 20);
			this.lblGeneralPermissions.Name = "lblGeneralPermissions";
			this.lblGeneralPermissions.Size = new System.Drawing.Size(179, 20);
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
			this.lblPermissionPresent.Location = new System.Drawing.Point(3, 287);
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
			this.lblPermissionDelete.Location = new System.Drawing.Point(3, 207);
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
			this.lblPermissionInsert.Location = new System.Drawing.Point(3, 187);
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
			this.lblPermissionConnect.Location = new System.Drawing.Point(3, 227);
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
			this.lblPermissionLayout.Location = new System.Drawing.Point(3, 247);
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
			this.lblPermissionData.Location = new System.Drawing.Point(3, 267);
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
			// lblModifyAccess
			// 
			this.lblModifyAccess.AutoSize = true;
			this.lblModifyAccess.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblModifyAccess.Location = new System.Drawing.Point(137, 0);
			this.lblModifyAccess.Name = "lblModifyAccess";
			this.lblModifyAccess.Size = new System.Drawing.Size(44, 13);
			this.lblModifyAccess.TabIndex = 42;
			this.lblModifyAccess.Text = "Modify";
			this.lblModifyAccess.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// lblViewAccess
			// 
			this.lblViewAccess.AutoSize = true;
			this.lblViewAccess.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblViewAccess.Location = new System.Drawing.Point(97, 0);
			this.lblViewAccess.Name = "lblViewAccess";
			this.lblViewAccess.Size = new System.Drawing.Size(34, 13);
			this.lblViewAccess.TabIndex = 41;
			this.lblViewAccess.Text = "View";
			this.lblViewAccess.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// cboDomain
			// 
			this.cboDomain.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
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
			this.cboDomain.Location = new System.Drawing.Point(137, 123);
			this.cboDomain.Name = "cboDomain";
			this.cboDomain.Size = new System.Drawing.Size(45, 21);
			this.cboDomain.TabIndex = 43;
			this.cboDomain.SelectedIndexChanged += new System.EventHandler(this.userOrDomainCheckBox_SelectedIndexChanged);
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.securitySettingsPanel);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(200, 682);
			this.panel1.TabIndex = 2;
			// 
			// splitter1
			// 
			this.splitter1.Location = new System.Drawing.Point(200, 0);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = new System.Drawing.Size(3, 682);
			this.splitter1.TabIndex = 3;
			this.splitter1.TabStop = false;
			// 
			// toolSetListViewPresenter
			// 
			this.toolSetListViewPresenter.HideDeniedMenuItems = false;
			this.toolSetListViewPresenter.ListView = this.listView1;
			this.toolSetListViewPresenter.ShowDefaultContextMenu = true;
			this.toolSetListViewPresenter.ToolSetController = this.toolSetController;
			// 
			// propertyPresenter
			// 
			this.propertyPresenter.PrimaryPropertyGrid = this.propertyGrid1;
			this.propertyPresenter.PropertyController = this.propertyController;
			this.propertyPresenter.SecondaryPropertyGrid = null;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1264, 682);
			this.Controls.Add(this.splitContainer2);
			this.Controls.Add(this.splitter1);
			this.Controls.Add(this.panel1);
			this.Name = "MainForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Security Demo";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.splitContainer2.Panel1.ResumeLayout(false);
			this.splitContainer2.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
			this.splitContainer2.ResumeLayout(false);
			this.splitContainer3.Panel1.ResumeLayout(false);
			this.splitContainer3.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
			this.splitContainer3.ResumeLayout(false);
			this.toolboxGroupBox.ResumeLayout(false);
			this.GeneralPermissionsPanel.ResumeLayout(false);
			this.GeneralPermissionsPanel.PerformLayout();
			this.propertiesGroupBox.ResumeLayout(false);
			this.ObjectPermissionsPanel.ResumeLayout(false);
			this.ObjectPermissionsPanel.PerformLayout();
			this.securitySettingsPanel.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.ItemVisibilityGroupBox.ResumeLayout(false);
			this.ItemVisibilityGroupBox.PerformLayout();
			this.PermissionsGroupBox.ResumeLayout(false);
			this.domainPermissionInfoPanel.ResumeLayout(false);
			this.domainPermissionInfoPanel.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private Dataweb.NShape.WinFormsUI.Display display1;
		private Dataweb.NShape.Controllers.DiagramSetController diagramSetController;
		private Dataweb.NShape.Project project;
		private Dataweb.NShape.Advanced.CachedRepository cachedRepository;
		private Dataweb.NShape.XmlStore xmlStore;
		private Dataweb.NShape.Controllers.ToolSetController toolSetController;
		private Dataweb.NShape.WinFormsUI.ToolSetListViewPresenter toolSetListViewPresenter;
		private System.Windows.Forms.ListView listView1;
		private Dataweb.NShape.Controllers.PropertyController propertyController;
		private Dataweb.NShape.WinFormsUI.PropertyPresenter propertyPresenter;
		private System.Windows.Forms.PropertyGrid propertyGrid1;
		private System.Windows.Forms.SplitContainer splitContainer2;
		private System.Windows.Forms.SplitContainer splitContainer3;
		private System.Windows.Forms.Panel securitySettingsPanel;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Splitter splitter1;
		private System.Windows.Forms.CheckBox chkHideMenuItems;
		private System.Windows.Forms.GroupBox ItemVisibilityGroupBox;
		private System.Windows.Forms.CheckBox chkHideProperties;
		private System.Windows.Forms.TableLayoutPanel GeneralPermissionsPanel;
		private System.Windows.Forms.Label lblCurrentGenPerm;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.TableLayoutPanel ObjectPermissionsPanel;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label lblCurrentSecObj;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label lblCurrentDomain;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label lblCurrentDomPerm;
		private System.Windows.Forms.GroupBox toolboxGroupBox;
		private System.Windows.Forms.GroupBox propertiesGroupBox;
		private System.Windows.Forms.GroupBox PermissionsGroupBox;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox cboUser;
		private System.Windows.Forms.TableLayoutPanel domainPermissionInfoPanel;
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
		private System.Windows.Forms.Button editPermissionsButton;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.ComboBox cboDomain;
	}
}

