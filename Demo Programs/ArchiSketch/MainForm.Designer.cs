namespace ArchiSketch {
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
			Dataweb.NShape.RoleBasedSecurityManager roleBasedSecurityManager1 = new Dataweb.NShape.RoleBasedSecurityManager();
			this.mainMenuStrip = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.fileNewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.fileSaveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
			this.shapeTemplateTtoolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.stylesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.propertiesToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			this.diagramInsertToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.diagramDeleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.aboutArchiSketchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
			this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
			this.toolBoxStrip = new System.Windows.Forms.ToolStrip();
			this.toolBoxContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.addTemplateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.editTemplateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.deleteTemplateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.mainContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.propertiesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripContainer = new System.Windows.Forms.ToolStripContainer();
			this.display = new Dataweb.NShape.WinFormsUI.Display();
			this.diagramSetController = new Dataweb.NShape.Controllers.DiagramSetController();
			this.project = new Dataweb.NShape.Project(this.components);
			this.repository = new Dataweb.NShape.Advanced.CachedRepository();
			this.xmlStore = new Dataweb.NShape.XmlStore();
			this.toolStrip2 = new System.Windows.Forms.ToolStrip();
			this.diagramComboBox = new System.Windows.Forms.ToolStripComboBox();
			this.toolSetController = new Dataweb.NShape.Controllers.ToolSetController();
			this.mainMenuStrip.SuspendLayout();
			this.toolBoxContextMenuStrip.SuspendLayout();
			this.mainContextMenuStrip.SuspendLayout();
			this.toolStripContainer.ContentPanel.SuspendLayout();
			this.toolStripContainer.TopToolStripPanel.SuspendLayout();
			this.toolStripContainer.SuspendLayout();
			this.toolStrip2.SuspendLayout();
			this.SuspendLayout();
			// 
			// mainMenuStrip
			// 
			this.mainMenuStrip.Dock = System.Windows.Forms.DockStyle.None;
			this.mainMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.toolStripMenuItem1,
            this.helpToolStripMenuItem});
			this.mainMenuStrip.Location = new System.Drawing.Point(0, 0);
			this.mainMenuStrip.Name = "mainMenuStrip";
			this.mainMenuStrip.Size = new System.Drawing.Size(906, 24);
			this.mainMenuStrip.TabIndex = 3;
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileNewToolStripMenuItem,
            this.openToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.fileSaveAsToolStripMenuItem,
            this.exitToolStripMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
			this.fileToolStripMenuItem.Text = "File";
			// 
			// fileNewToolStripMenuItem
			// 
			this.fileNewToolStripMenuItem.Name = "fileNewToolStripMenuItem";
			this.fileNewToolStripMenuItem.Size = new System.Drawing.Size(121, 22);
			this.fileNewToolStripMenuItem.Text = "New";
			this.fileNewToolStripMenuItem.Click += new System.EventHandler(this.fileNewToolStripMenuItem_Click);
			// 
			// openToolStripMenuItem
			// 
			this.openToolStripMenuItem.Name = "openToolStripMenuItem";
			this.openToolStripMenuItem.Size = new System.Drawing.Size(121, 22);
			this.openToolStripMenuItem.Text = "Open...";
			this.openToolStripMenuItem.Click += new System.EventHandler(this.fileOpenToolStripMenuItem_Click);
			// 
			// saveToolStripMenuItem
			// 
			this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
			this.saveToolStripMenuItem.Size = new System.Drawing.Size(121, 22);
			this.saveToolStripMenuItem.Text = "Save";
			this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
			// 
			// fileSaveAsToolStripMenuItem
			// 
			this.fileSaveAsToolStripMenuItem.Name = "fileSaveAsToolStripMenuItem";
			this.fileSaveAsToolStripMenuItem.Size = new System.Drawing.Size(121, 22);
			this.fileSaveAsToolStripMenuItem.Text = "Save as...";
			this.fileSaveAsToolStripMenuItem.Click += new System.EventHandler(this.fileSaveAsToolStripMenuItem_Click);
			// 
			// exitToolStripMenuItem
			// 
			this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			this.exitToolStripMenuItem.Size = new System.Drawing.Size(121, 22);
			this.exitToolStripMenuItem.Text = "Exit";
			this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
			// 
			// editToolStripMenuItem
			// 
			this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyToolStripMenuItem,
            this.deleteToolStripMenuItem,
            this.toolStripMenuItem2,
            this.shapeTemplateTtoolStripMenuItem,
            this.stylesToolStripMenuItem,
            this.propertiesToolStripMenuItem1});
			this.editToolStripMenuItem.Name = "editToolStripMenuItem";
			this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
			this.editToolStripMenuItem.Text = "Edit";
			// 
			// copyToolStripMenuItem
			// 
			this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
			this.copyToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
			this.copyToolStripMenuItem.Text = "Copy";
			this.copyToolStripMenuItem.Click += new System.EventHandler(this.copyToolStripMenuItem_Click);
			// 
			// deleteToolStripMenuItem
			// 
			this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
			this.deleteToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
			this.deleteToolStripMenuItem.Text = "Delete";
			this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
			// 
			// toolStripMenuItem2
			// 
			this.toolStripMenuItem2.Name = "toolStripMenuItem2";
			this.toolStripMenuItem2.Size = new System.Drawing.Size(165, 6);
			// 
			// shapeTemplateTtoolStripMenuItem
			// 
			this.shapeTemplateTtoolStripMenuItem.Name = "shapeTemplateTtoolStripMenuItem";
			this.shapeTemplateTtoolStripMenuItem.Size = new System.Drawing.Size(168, 22);
			this.shapeTemplateTtoolStripMenuItem.Text = "Shape Template...";
			this.shapeTemplateTtoolStripMenuItem.Click += new System.EventHandler(this.shapeTemplateToolStripMenuItem_Click);
			// 
			// stylesToolStripMenuItem
			// 
			this.stylesToolStripMenuItem.Name = "stylesToolStripMenuItem";
			this.stylesToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
			this.stylesToolStripMenuItem.Text = "Styles...";
			this.stylesToolStripMenuItem.Click += new System.EventHandler(this.stylesToolStripMenuItem_Click);
			// 
			// propertiesToolStripMenuItem1
			// 
			this.propertiesToolStripMenuItem1.Name = "propertiesToolStripMenuItem1";
			this.propertiesToolStripMenuItem1.Size = new System.Drawing.Size(168, 22);
			this.propertiesToolStripMenuItem1.Text = "Properties...";
			this.propertiesToolStripMenuItem1.Click += new System.EventHandler(this.propertiesToolStripMenuItem_Click);
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.diagramInsertToolStripMenuItem,
            this.diagramDeleteToolStripMenuItem});
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new System.Drawing.Size(64, 20);
			this.toolStripMenuItem1.Text = "Diagram";
			// 
			// diagramInsertToolStripMenuItem
			// 
			this.diagramInsertToolStripMenuItem.Name = "diagramInsertToolStripMenuItem";
			this.diagramInsertToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
			this.diagramInsertToolStripMenuItem.Text = "Insert";
			this.diagramInsertToolStripMenuItem.Click += new System.EventHandler(this.diagramInsertToolStripMenuItem_Click);
			// 
			// diagramDeleteToolStripMenuItem
			// 
			this.diagramDeleteToolStripMenuItem.Name = "diagramDeleteToolStripMenuItem";
			this.diagramDeleteToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
			this.diagramDeleteToolStripMenuItem.Text = "Delete";
			this.diagramDeleteToolStripMenuItem.Click += new System.EventHandler(this.diagramDeleteToolStripMenuItem_Click);
			// 
			// helpToolStripMenuItem
			// 
			this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutArchiSketchToolStripMenuItem});
			this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
			this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
			this.helpToolStripMenuItem.Text = "Help";
			// 
			// aboutArchiSketchToolStripMenuItem
			// 
			this.aboutArchiSketchToolStripMenuItem.Name = "aboutArchiSketchToolStripMenuItem";
			this.aboutArchiSketchToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
			this.aboutArchiSketchToolStripMenuItem.Text = "About ArchiSketch...";
			this.aboutArchiSketchToolStripMenuItem.Click += new System.EventHandler(this.aboutArchiSketchToolStripMenuItem_Click);
			// 
			// saveFileDialog
			// 
			this.saveFileDialog.DefaultExt = "*.askp";
			this.saveFileDialog.Filter = "ArchiSketch Files (*.askp)|*.askp";
			this.saveFileDialog.Title = "Save ArchiSketch Project";
			// 
			// openFileDialog
			// 
			this.openFileDialog.DefaultExt = "*.askp";
			this.openFileDialog.FileName = "Open ArchiSketch Project";
			this.openFileDialog.Filter = "ArchiSketch Files (*.askp)|*.askp";
			// 
			// toolBoxStrip
			// 
			this.toolBoxStrip.ContextMenuStrip = this.toolBoxContextMenuStrip;
			this.toolBoxStrip.Dock = System.Windows.Forms.DockStyle.None;
			this.toolBoxStrip.Location = new System.Drawing.Point(138, 24);
			this.toolBoxStrip.Name = "toolBoxStrip";
			this.toolBoxStrip.Size = new System.Drawing.Size(111, 25);
			this.toolBoxStrip.TabIndex = 5;
			this.toolBoxStrip.Text = "mainToolStrip";
			// 
			// toolBoxContextMenuStrip
			// 
			this.toolBoxContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addTemplateToolStripMenuItem,
            this.editTemplateToolStripMenuItem,
            this.deleteTemplateToolStripMenuItem});
			this.toolBoxContextMenuStrip.Name = "toolBoxContextMenuStrip";
			this.toolBoxContextMenuStrip.Size = new System.Drawing.Size(161, 70);
			// 
			// addTemplateToolStripMenuItem
			// 
			this.addTemplateToolStripMenuItem.Name = "addTemplateToolStripMenuItem";
			this.addTemplateToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
			this.addTemplateToolStripMenuItem.Text = "Add Template";
			this.addTemplateToolStripMenuItem.Click += new System.EventHandler(this.addTemplateToolStripMenuItem_Click);
			// 
			// editTemplateToolStripMenuItem
			// 
			this.editTemplateToolStripMenuItem.Name = "editTemplateToolStripMenuItem";
			this.editTemplateToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
			this.editTemplateToolStripMenuItem.Text = "Edit Template";
			this.editTemplateToolStripMenuItem.Click += new System.EventHandler(this.editTemplateToolStripMenuItem_Click);
			// 
			// deleteTemplateToolStripMenuItem
			// 
			this.deleteTemplateToolStripMenuItem.Name = "deleteTemplateToolStripMenuItem";
			this.deleteTemplateToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
			this.deleteTemplateToolStripMenuItem.Text = "Delete Template";
			this.deleteTemplateToolStripMenuItem.Click += new System.EventHandler(this.deleteTemplateToolStripMenuItem_Click);
			// 
			// mainContextMenuStrip
			// 
			this.mainContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.propertiesToolStripMenuItem});
			this.mainContextMenuStrip.Name = "mainContextMenuStrip";
			this.mainContextMenuStrip.ShowItemToolTips = false;
			this.mainContextMenuStrip.Size = new System.Drawing.Size(172, 26);
			// 
			// propertiesToolStripMenuItem
			// 
			this.propertiesToolStripMenuItem.Name = "propertiesToolStripMenuItem";
			this.propertiesToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
			this.propertiesToolStripMenuItem.Text = "Shape Properties...";
			this.propertiesToolStripMenuItem.Click += new System.EventHandler(this.propertiesToolStripMenuItem_Click);
			// 
			// toolStripContainer
			// 
			this.toolStripContainer.BottomToolStripPanelVisible = false;
			// 
			// toolStripContainer.ContentPanel
			// 
			this.toolStripContainer.ContentPanel.AutoScroll = true;
			this.toolStripContainer.ContentPanel.Controls.Add(this.display);
			this.toolStripContainer.ContentPanel.Size = new System.Drawing.Size(906, 567);
			this.toolStripContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.toolStripContainer.LeftToolStripPanelVisible = false;
			this.toolStripContainer.Location = new System.Drawing.Point(0, 0);
			this.toolStripContainer.Name = "toolStripContainer";
			this.toolStripContainer.RightToolStripPanelVisible = false;
			this.toolStripContainer.Size = new System.Drawing.Size(906, 616);
			this.toolStripContainer.TabIndex = 7;
			this.toolStripContainer.Text = "toolStripContainer1";
			// 
			// toolStripContainer.TopToolStripPanel
			// 
			this.toolStripContainer.TopToolStripPanel.Controls.Add(this.mainMenuStrip);
			this.toolStripContainer.TopToolStripPanel.Controls.Add(this.toolStrip2);
			this.toolStripContainer.TopToolStripPanel.Controls.Add(this.toolBoxStrip);
			// 
			// display
			// 
			this.display.AllowDrop = true;
			this.display.AutoScroll = true;
			this.display.BackColorGradient = System.Drawing.SystemColors.Control;
			this.display.ContextMenuStrip = this.mainContextMenuStrip;
			this.display.DiagramSetController = this.diagramSetController;
			this.display.Dock = System.Windows.Forms.DockStyle.Fill;
			this.display.GridColor = System.Drawing.Color.Gainsboro;
			this.display.GridSize = 19;
			this.display.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.display.Location = new System.Drawing.Point(0, 0);
			this.display.Name = "display";
			this.display.PropertyController = null;
			this.display.SelectionHilightColor = System.Drawing.Color.Firebrick;
			this.display.SelectionInactiveColor = System.Drawing.Color.Gray;
			this.display.SelectionInteriorColor = System.Drawing.Color.WhiteSmoke;
			this.display.SelectionNormalColor = System.Drawing.Color.DarkGreen;
			this.display.Size = new System.Drawing.Size(906, 567);
			this.display.TabIndex = 0;
			this.display.ToolPreviewBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(119)))), ((int)(((byte)(136)))), ((int)(((byte)(153)))));
			this.display.ToolPreviewColor = System.Drawing.Color.FromArgb(((int)(((byte)(96)))), ((int)(((byte)(70)))), ((int)(((byte)(130)))), ((int)(((byte)(180)))));
			// 
			// diagramSetController
			// 
			this.diagramSetController.ActiveTool = null;
			this.diagramSetController.Project = this.project;
			// 
			// project
			// 
			this.project.LibrarySearchPaths = ((System.Collections.Generic.IList<string>)(resources.GetObject("project.LibrarySearchPaths")));
			this.project.Name = "ArchiSketch Project 1";
			this.project.Repository = this.repository;
			roleBasedSecurityManager1.CurrentRole = Dataweb.NShape.StandardRole.Administrator;
			roleBasedSecurityManager1.CurrentRoleName = "Administrator";
			this.project.SecurityManager = roleBasedSecurityManager1;
			// 
			// repository
			// 
			this.repository.ProjectName = "ArchiSketch Project 1";
			this.repository.Store = this.xmlStore;
			this.repository.Version = 0;
			// 
			// xmlStore
			// 
			this.xmlStore.DesignFileName = "";
			this.xmlStore.DirectoryName = "";
			this.xmlStore.FileExtension = ".xml";
			this.xmlStore.ProjectName = "ArchiSketch Project 1";
			// 
			// toolStrip2
			// 
			this.toolStrip2.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.diagramComboBox});
			this.toolStrip2.Location = new System.Drawing.Point(3, 24);
			this.toolStrip2.Name = "toolStrip2";
			this.toolStrip2.Size = new System.Drawing.Size(135, 25);
			this.toolStrip2.TabIndex = 6;
			// 
			// diagramComboBox
			// 
			this.diagramComboBox.Name = "diagramComboBox";
			this.diagramComboBox.Size = new System.Drawing.Size(121, 25);
			this.diagramComboBox.ToolTipText = "Select a diagram to display, rename the current diagram or create a new one.";
			this.diagramComboBox.SelectedIndexChanged += new System.EventHandler(this.diagramComboBox_SelectedIndexChanged);
			this.diagramComboBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.diagramComboBox_KeyDown);
			// 
			// toolSetController
			// 
			this.toolSetController.DiagramSetController = this.diagramSetController;
			this.toolSetController.ToolSelected += new System.EventHandler<Dataweb.NShape.Controllers.ToolEventArgs>(this.toolSetController_ToolSelected);
			this.toolSetController.ToolAdded += new System.EventHandler<Dataweb.NShape.Controllers.ToolEventArgs>(this.toolSetController_ToolAdded);
			this.toolSetController.ToolRemoved += new System.EventHandler<Dataweb.NShape.Controllers.ToolEventArgs>(this.toolSetController_ToolRemoved);
			this.toolSetController.ToolChanged += new System.EventHandler<Dataweb.NShape.Controllers.ToolEventArgs>(this.toolSetController_ToolChanged);
			this.toolSetController.TemplateEditorSelected += new System.EventHandler<Dataweb.NShape.Controllers.TemplateEditorEventArgs>(this.toolBox_ShowTemplateEditorDialog);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(906, 616);
			this.Controls.Add(this.toolStripContainer);
			this.MainMenuStrip = this.mainMenuStrip;
			this.Name = "MainForm";
			this.Text = "ArchiSketch 1.0";
			this.Load += new System.EventHandler(this.MainForm_Load);
			this.mainMenuStrip.ResumeLayout(false);
			this.mainMenuStrip.PerformLayout();
			this.toolBoxContextMenuStrip.ResumeLayout(false);
			this.mainContextMenuStrip.ResumeLayout(false);
			this.toolStripContainer.ContentPanel.ResumeLayout(false);
			this.toolStripContainer.TopToolStripPanel.ResumeLayout(false);
			this.toolStripContainer.TopToolStripPanel.PerformLayout();
			this.toolStripContainer.ResumeLayout(false);
			this.toolStripContainer.PerformLayout();
			this.toolStrip2.ResumeLayout(false);
			this.toolStrip2.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private Dataweb.NShape.Controllers.ToolSetController toolSetController;
		private System.Windows.Forms.MenuStrip mainMenuStrip;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
		private System.Windows.Forms.SaveFileDialog saveFileDialog;
		private System.Windows.Forms.OpenFileDialog openFileDialog;
		private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem diagramInsertToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem diagramDeleteToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem fileSaveAsToolStripMenuItem;
		private System.Windows.Forms.ToolStrip toolBoxStrip;
		private System.Windows.Forms.ToolStripMenuItem propertiesToolStripMenuItem;
		private System.Windows.Forms.ContextMenuStrip mainContextMenuStrip;
		private System.Windows.Forms.ToolStripMenuItem aboutArchiSketchToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
		private System.Windows.Forms.ToolStripMenuItem stylesToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem shapeTemplateTtoolStripMenuItem;
		private System.Windows.Forms.ContextMenuStrip toolBoxContextMenuStrip;
		private System.Windows.Forms.ToolStripMenuItem editTemplateToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem addTemplateToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem deleteTemplateToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem fileNewToolStripMenuItem;
		private Dataweb.NShape.Advanced.CachedRepository repository;
		private Dataweb.NShape.XmlStore xmlStore;
		private System.Windows.Forms.ToolStripContainer toolStripContainer;
		private System.Windows.Forms.ToolStrip toolStrip2;
		private System.Windows.Forms.ToolStripComboBox diagramComboBox;
		private Dataweb.NShape.WinFormsUI.Display display;
		private Dataweb.NShape.Controllers.DiagramSetController diagramSetController;
		private Dataweb.NShape.Project project;
		private System.Windows.Forms.ToolStripMenuItem propertiesToolStripMenuItem1;
	}
}

