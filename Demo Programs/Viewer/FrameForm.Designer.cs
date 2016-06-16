namespace NShapeViewer {
	partial class FrameForm {
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrameForm));
			Dataweb.NShape.RoleBasedSecurityManager roleBasedSecurityManager1 = new Dataweb.NShape.RoleBasedSecurityManager();
			this.project = new Dataweb.NShape.Project(this.components);
			this.cachedRepository = new Dataweb.NShape.Advanced.CachedRepository();
			this.xmlStore = new Dataweb.NShape.XmlStore();
			this.modelController = new Dataweb.NShape.Controllers.ModelController();
			this.diagramSetController = new Dataweb.NShape.Controllers.DiagramSetController();
			this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
			this.menuStrip = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openProjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.closeProjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.diagramsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.windowsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.horizontalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.verticalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.cascadeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.aboutDiagramViewerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.modelTreeView = new System.Windows.Forms.TreeView();
			this.splitter1 = new System.Windows.Forms.Splitter();
			this.modelTreeViewPresenter = new Dataweb.NShape.WinFormsUI.ModelTreeViewPresenter();
			this.menuStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// project
			// 
			this.project.AutoLoadLibraries = true;
			this.project.LibrarySearchPaths = ((System.Collections.Generic.IList<string>)(resources.GetObject("project.LibrarySearchPaths")));
			this.project.Name = null;
			this.project.Repository = this.cachedRepository;
			roleBasedSecurityManager1.CurrentRole = Dataweb.NShape.StandardRole.Administrator;
			roleBasedSecurityManager1.CurrentRoleName = "Administrator";
			this.project.SecurityManager = roleBasedSecurityManager1;
			this.project.Opened += new System.EventHandler(this.project_Opened);
			this.project.Closing += new System.EventHandler(this.project_Closing);
			this.project.Closed += new System.EventHandler(this.project_Closed);
			// 
			// cachedRepository
			// 
			this.cachedRepository.ProjectName = null;
			this.cachedRepository.Store = this.xmlStore;
			this.cachedRepository.Version = 0;
			// 
			// xmlStore
			// 
			this.xmlStore.BackupGenerationMode = Dataweb.NShape.XmlStore.BackupFileGenerationMode.BakFile;
			this.xmlStore.DesignFileName = "";
			this.xmlStore.DirectoryName = "";
			this.xmlStore.FileExtension = ".xml";
			this.xmlStore.ProjectName = "";
			// 
			// modelController
			// 
			this.modelController.DiagramSetController = this.diagramSetController;
			// 
			// diagramSetController
			// 
			this.diagramSetController.ActiveTool = null;
			this.diagramSetController.Project = this.project;
			// 
			// openFileDialog
			// 
			this.openFileDialog.FileName = "openFileDialog1";
			this.openFileDialog.Filter = "NShape XML Repositories|*.nspj;*.xml|AllFiles|*.*";
			// 
			// menuStrip
			// 
			this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.diagramsToolStripMenuItem,
            this.windowsToolStripMenuItem,
            this.helpToolStripMenuItem});
			this.menuStrip.Location = new System.Drawing.Point(0, 0);
			this.menuStrip.MdiWindowListItem = this.windowsToolStripMenuItem;
			this.menuStrip.Name = "menuStrip";
			this.menuStrip.Size = new System.Drawing.Size(830, 24);
			this.menuStrip.TabIndex = 2;
			this.menuStrip.Text = "menuStrip1";
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openProjectToolStripMenuItem,
            this.closeProjectToolStripMenuItem,
            this.toolStripMenuItem1,
            this.exitToolStripMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
			this.fileToolStripMenuItem.Text = "File";
			// 
			// openProjectToolStripMenuItem
			// 
			this.openProjectToolStripMenuItem.Name = "openProjectToolStripMenuItem";
			this.openProjectToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
			this.openProjectToolStripMenuItem.Text = "Open Project...";
			this.openProjectToolStripMenuItem.Click += new System.EventHandler(this.openMenuItem_Click);
			// 
			// closeProjectToolStripMenuItem
			// 
			this.closeProjectToolStripMenuItem.Enabled = false;
			this.closeProjectToolStripMenuItem.Name = "closeProjectToolStripMenuItem";
			this.closeProjectToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
			this.closeProjectToolStripMenuItem.Text = "Close Project";
			this.closeProjectToolStripMenuItem.Click += new System.EventHandler(this.closeMenuItem_Click);
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new System.Drawing.Size(149, 6);
			// 
			// exitToolStripMenuItem
			// 
			this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			this.exitToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
			this.exitToolStripMenuItem.Text = "Exit";
			this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
			// 
			// diagramsToolStripMenuItem
			// 
			this.diagramsToolStripMenuItem.Name = "diagramsToolStripMenuItem";
			this.diagramsToolStripMenuItem.Size = new System.Drawing.Size(69, 20);
			this.diagramsToolStripMenuItem.Text = "Diagrams";
			// 
			// windowsToolStripMenuItem
			// 
			this.windowsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.horizontalToolStripMenuItem,
            this.verticalToolStripMenuItem,
            this.cascadeToolStripMenuItem});
			this.windowsToolStripMenuItem.Name = "windowsToolStripMenuItem";
			this.windowsToolStripMenuItem.Size = new System.Drawing.Size(68, 20);
			this.windowsToolStripMenuItem.Text = "Windows";
			// 
			// horizontalToolStripMenuItem
			// 
			this.horizontalToolStripMenuItem.Name = "horizontalToolStripMenuItem";
			this.horizontalToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
			this.horizontalToolStripMenuItem.Text = "Horizontal";
			this.horizontalToolStripMenuItem.Click += new System.EventHandler(this.horizontalToolStripMenuItem_Click);
			// 
			// verticalToolStripMenuItem
			// 
			this.verticalToolStripMenuItem.Name = "verticalToolStripMenuItem";
			this.verticalToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
			this.verticalToolStripMenuItem.Text = "Vertical";
			this.verticalToolStripMenuItem.Click += new System.EventHandler(this.verticalToolStripMenuItem_Click);
			// 
			// cascadeToolStripMenuItem
			// 
			this.cascadeToolStripMenuItem.Name = "cascadeToolStripMenuItem";
			this.cascadeToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
			this.cascadeToolStripMenuItem.Text = "Cascade";
			this.cascadeToolStripMenuItem.Click += new System.EventHandler(this.cascadeToolStripMenuItem_Click);
			// 
			// helpToolStripMenuItem
			// 
			this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutDiagramViewerToolStripMenuItem});
			this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
			this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
			this.helpToolStripMenuItem.Text = "Help";
			// 
			// aboutDiagramViewerToolStripMenuItem
			// 
			this.aboutDiagramViewerToolStripMenuItem.Name = "aboutDiagramViewerToolStripMenuItem";
			this.aboutDiagramViewerToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
			this.aboutDiagramViewerToolStripMenuItem.Text = "About NShape Viewer...";
			this.aboutDiagramViewerToolStripMenuItem.Click += new System.EventHandler(this.aboutDiagramViewerToolStripMenuItem_Click);
			// 
			// modelTreeView
			// 
			this.modelTreeView.Dock = System.Windows.Forms.DockStyle.Left;
			this.modelTreeView.FullRowSelect = true;
			this.modelTreeView.ImageIndex = 0;
			this.modelTreeView.Location = new System.Drawing.Point(0, 24);
			this.modelTreeView.Name = "modelTreeView";
			this.modelTreeView.SelectedImageIndex = 0;
			this.modelTreeView.Size = new System.Drawing.Size(151, 535);
			this.modelTreeView.TabIndex = 4;
			this.modelTreeView.Visible = false;
			// 
			// splitter1
			// 
			this.splitter1.Location = new System.Drawing.Point(151, 24);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = new System.Drawing.Size(3, 535);
			this.splitter1.TabIndex = 5;
			this.splitter1.TabStop = false;
			// 
			// modelTreeViewPresenter
			// 
			this.modelTreeViewPresenter.HideDeniedMenuItems = false;
			this.modelTreeViewPresenter.ModelTreeController = this.modelController;
			this.modelTreeViewPresenter.PropertyController = null;
			this.modelTreeViewPresenter.ShowDefaultContextMenu = true;
			this.modelTreeViewPresenter.TreeView = this.modelTreeView;
			// 
			// FrameForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(830, 559);
			this.Controls.Add(this.splitter1);
			this.Controls.Add(this.modelTreeView);
			this.Controls.Add(this.menuStrip);
			this.IsMdiContainer = true;
			this.MainMenuStrip = this.menuStrip;
			this.Name = "FrameForm";
			this.Text = "NShape Viewer";
			this.menuStrip.ResumeLayout(false);
			this.menuStrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Dataweb.NShape.Project project;
		private Dataweb.NShape.XmlStore xmlStore;
		private Dataweb.NShape.Controllers.ModelController modelController;
		private Dataweb.NShape.Controllers.DiagramSetController diagramSetController;
		private Dataweb.NShape.Advanced.CachedRepository cachedRepository;
		private System.Windows.Forms.OpenFileDialog openFileDialog;
		private System.Windows.Forms.MenuStrip menuStrip;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem openProjectToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem closeProjectToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem windowsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem horizontalToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem verticalToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem cascadeToolStripMenuItem;
		private System.Windows.Forms.TreeView modelTreeView;
		private System.Windows.Forms.Splitter splitter1;
		private Dataweb.NShape.WinFormsUI.ModelTreeViewPresenter modelTreeViewPresenter;
		private System.Windows.Forms.ToolStripMenuItem diagramsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem aboutDiagramViewerToolStripMenuItem;
	}
}