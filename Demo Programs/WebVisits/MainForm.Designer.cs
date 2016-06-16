namespace WebVisists {
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
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.loadWebStatisticsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.loadDiagramToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.saveDiagramToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.mainMenuStrip = new System.Windows.Forms.MenuStrip();
			this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.selectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.showLayoutWindowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
			this.display = new Dataweb.NShape.WinFormsUI.Display();
			this.diagramSetController = new Dataweb.NShape.Controllers.DiagramSetController();
			this.project = new Dataweb.NShape.Project(this.components);
			this.cachedRepository = new Dataweb.NShape.Advanced.CachedRepository();
			this.xmlStore = new Dataweb.NShape.XmlStore();
			this.toolbox = new System.Windows.Forms.ListView();
			this.toolBoxAdapter = new Dataweb.NShape.WinFormsUI.ToolSetListViewPresenter(this.components);
			this.toolSetController = new Dataweb.NShape.Controllers.ToolSetController();
			this.mainMenuStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadWebStatisticsToolStripMenuItem,
            this.loadDiagramToolStripMenuItem,
            this.saveDiagramToolStripMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
			this.fileToolStripMenuItem.Text = "File";
			// 
			// loadWebStatisticsToolStripMenuItem
			// 
			this.loadWebStatisticsToolStripMenuItem.Name = "loadWebStatisticsToolStripMenuItem";
			this.loadWebStatisticsToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
			this.loadWebStatisticsToolStripMenuItem.Text = "Load Web statistics...";
			this.loadWebStatisticsToolStripMenuItem.Click += new System.EventHandler(this.loadWebStatisticsToolStripMenuItem_Click);
			// 
			// loadDiagramToolStripMenuItem
			// 
			this.loadDiagramToolStripMenuItem.Name = "loadDiagramToolStripMenuItem";
			this.loadDiagramToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
			this.loadDiagramToolStripMenuItem.Text = "Load Diagram";
			this.loadDiagramToolStripMenuItem.Click += new System.EventHandler(this.loadDiagramToolStripMenuItem_Click);
			// 
			// saveDiagramToolStripMenuItem
			// 
			this.saveDiagramToolStripMenuItem.Name = "saveDiagramToolStripMenuItem";
			this.saveDiagramToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
			this.saveDiagramToolStripMenuItem.Text = "Save Diagram";
			this.saveDiagramToolStripMenuItem.Click += new System.EventHandler(this.saveDiagramToolStripMenuItem_Click);
			// 
			// mainMenuStrip
			// 
			this.mainMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem});
			this.mainMenuStrip.Location = new System.Drawing.Point(0, 0);
			this.mainMenuStrip.Name = "mainMenuStrip";
			this.mainMenuStrip.Size = new System.Drawing.Size(996, 24);
			this.mainMenuStrip.TabIndex = 3;
			// 
			// editToolStripMenuItem
			// 
			this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.selectAllToolStripMenuItem,
            this.showLayoutWindowToolStripMenuItem});
			this.editToolStripMenuItem.Name = "editToolStripMenuItem";
			this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
			this.editToolStripMenuItem.Text = "Edit";
			// 
			// selectAllToolStripMenuItem
			// 
			this.selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
			this.selectAllToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
			this.selectAllToolStripMenuItem.Text = "Select All";
			this.selectAllToolStripMenuItem.Click += new System.EventHandler(this.selectAllToolStripMenuItem_Click);
			// 
			// showLayoutWindowToolStripMenuItem
			// 
			this.showLayoutWindowToolStripMenuItem.Name = "showLayoutWindowToolStripMenuItem";
			this.showLayoutWindowToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
			this.showLayoutWindowToolStripMenuItem.Text = "Show Layout Window";
			this.showLayoutWindowToolStripMenuItem.Click += new System.EventHandler(this.showLayoutWindowToolStripMenuItem_Click);
			// 
			// openFileDialog
			// 
			this.openFileDialog.FileName = "Open Web Statistics File";
			// 
			// display
			// 
			this.display.AllowDrop = true;
			this.display.AutoScroll = true;
			this.display.BackColor = System.Drawing.SystemColors.Control;
			this.display.BackColorGradient = System.Drawing.SystemColors.ControlLightLight;
			this.display.DiagramSetController = this.diagramSetController;
			this.display.Dock = System.Windows.Forms.DockStyle.Fill;
			this.display.GridColor = System.Drawing.Color.Gainsboro;
			this.display.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.display.Location = new System.Drawing.Point(0, 0);
			this.display.Name = "display";
			this.display.PropertyController = null;
			this.display.RenderingQualityHighQuality = Dataweb.NShape.Advanced.RenderingQuality.MaximumQuality;
			this.display.SelectionHilightColor = System.Drawing.Color.Firebrick;
			this.display.SelectionInactiveColor = System.Drawing.Color.Gray;
			this.display.SelectionInteriorColor = System.Drawing.Color.WhiteSmoke;
			this.display.SelectionNormalColor = System.Drawing.Color.DarkGreen;
			this.display.Size = new System.Drawing.Size(996, 708);
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
			this.project.AutoGenerateTemplates = false;
			this.project.LibrarySearchPaths = ((System.Collections.Generic.IList<string>)(resources.GetObject("project.LibrarySearchPaths")));
			this.project.Name = null;
			this.project.Repository = this.cachedRepository;
			roleBasedSecurityManager1.CurrentRole = Dataweb.NShape.StandardRole.Administrator;
			roleBasedSecurityManager1.CurrentRoleName = "Administrator";
			this.project.SecurityManager = roleBasedSecurityManager1;
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
			this.xmlStore.FileExtension = ".nspj";
			this.xmlStore.ProjectName = "";
			// 
			// toolbox
			// 
			this.toolbox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.toolbox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.toolbox.FullRowSelect = true;
			this.toolbox.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.toolbox.HideSelection = false;
			this.toolbox.Location = new System.Drawing.Point(14, 600);
			this.toolbox.MultiSelect = false;
			this.toolbox.Name = "toolbox";
			this.toolbox.ShowItemToolTips = true;
			this.toolbox.Size = new System.Drawing.Size(145, 76);
			this.toolbox.TabIndex = 4;
			this.toolbox.UseCompatibleStateImageBehavior = false;
			this.toolbox.View = System.Windows.Forms.View.Details;
			// 
			// toolBoxAdapter
			// 
			this.toolBoxAdapter.HideDeniedMenuItems = false;
			this.toolBoxAdapter.ListView = this.toolbox;
			this.toolBoxAdapter.ShowDefaultContextMenu = true;
			this.toolBoxAdapter.ToolSetController = this.toolSetController;
			// 
			// toolSetController
			// 
			this.toolSetController.DiagramSetController = this.diagramSetController;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(996, 708);
			this.Controls.Add(this.toolbox);
			this.Controls.Add(this.mainMenuStrip);
			this.Controls.Add(this.display);
			this.Name = "MainForm";
			this.Text = "WebVisits";
			this.Load += new System.EventHandler(this.MainForm_Load);
			this.Shown += new System.EventHandler(this.MainForm_Shown);
			this.mainMenuStrip.ResumeLayout(false);
			this.mainMenuStrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Dataweb.NShape.WinFormsUI.Display display;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem loadWebStatisticsToolStripMenuItem;
		private System.Windows.Forms.MenuStrip mainMenuStrip;
		private System.Windows.Forms.OpenFileDialog openFileDialog;
		private System.Windows.Forms.ToolStripMenuItem saveDiagramToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem loadDiagramToolStripMenuItem;
		private Dataweb.NShape.WinFormsUI.ToolSetListViewPresenter toolBoxAdapter;
		private System.Windows.Forms.ListView toolbox;
		private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem selectAllToolStripMenuItem;
		private Dataweb.NShape.Controllers.DiagramSetController diagramSetController;
		private Dataweb.NShape.XmlStore xmlStore;
		private Dataweb.NShape.Controllers.ToolSetController toolSetController;
		private Dataweb.NShape.Advanced.CachedRepository cachedRepository;
		private Dataweb.NShape.Project project;
		private System.Windows.Forms.ToolStripMenuItem showLayoutWindowToolStripMenuItem;
	}
}

