namespace BasicTutorial {
	partial class Form1 {
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
			Dataweb.NShape.RoleBasedSecurityManager roleBasedSecurityManager2 = new Dataweb.NShape.RoleBasedSecurityManager();
			this.display1 = new Dataweb.NShape.WinFormsUI.Display();
			this.diagramSetController1 = new Dataweb.NShape.Controllers.DiagramSetController();
			this.project1 = new Dataweb.NShape.Project(this.components);
			this.cachedRepository1 = new Dataweb.NShape.Advanced.CachedRepository();
			this.xmlStore1 = new Dataweb.NShape.XmlStore();
			this.toolSetController1 = new Dataweb.NShape.Controllers.ToolSetController();
			this.toolSetListViewPresenter1 = new Dataweb.NShape.WinFormsUI.ToolSetListViewPresenter(this.components);
			this.toolListView = new System.Windows.Forms.ListView();
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.fileLoadStatisticsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.fileSaveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.menuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// display1
			// 
			this.display1.AllowDrop = true;
			this.display1.AutoScroll = true;
			this.display1.BackColorGradient = System.Drawing.SystemColors.Control;
			this.display1.DiagramSetController = this.diagramSetController1;
			this.display1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.display1.GridColor = System.Drawing.Color.Gainsboro;
			this.display1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.display1.Location = new System.Drawing.Point(0, 24);
			this.display1.Name = "display1";
			this.display1.PropertyController = null;
			this.display1.SelectionHilightColor = System.Drawing.Color.Firebrick;
			this.display1.SelectionInactiveColor = System.Drawing.Color.Gray;
			this.display1.SelectionInteriorColor = System.Drawing.Color.WhiteSmoke;
			this.display1.SelectionNormalColor = System.Drawing.Color.DarkGreen;
			this.display1.Size = new System.Drawing.Size(1008, 538);
			this.display1.TabIndex = 0;
			this.display1.ToolPreviewBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(119)))), ((int)(((byte)(136)))), ((int)(((byte)(153)))));
			this.display1.ToolPreviewColor = System.Drawing.Color.FromArgb(((int)(((byte)(96)))), ((int)(((byte)(70)))), ((int)(((byte)(130)))), ((int)(((byte)(180)))));
			// 
			// diagramSetController1
			// 
			this.diagramSetController1.ActiveTool = null;
			this.diagramSetController1.Project = this.project1;
			// 
			// project1
			// 
			this.project1.AutoGenerateTemplates = true;
			this.project1.LibrarySearchPaths = ((System.Collections.Generic.IList<string>)(resources.GetObject("project1.LibrarySearchPaths")));
			this.project1.Name = null;
			this.project1.Repository = this.cachedRepository1;
			roleBasedSecurityManager2.CurrentRole = Dataweb.NShape.StandardRole.Administrator;
			roleBasedSecurityManager2.CurrentRoleName = "Administrator";
			this.project1.SecurityManager = roleBasedSecurityManager2;
			// 
			// cachedRepository1
			// 
			this.cachedRepository1.ProjectName = null;
			this.cachedRepository1.Store = this.xmlStore1;
			this.cachedRepository1.Version = 0;
			// 
			// xmlStore1
			// 
			this.xmlStore1.DesignFileName = "";
			this.xmlStore1.DirectoryName = "";
			this.xmlStore1.FileExtension = ".xml";
			this.xmlStore1.ProjectName = "";
			// 
			// toolSetController1
			// 
			this.toolSetController1.DiagramSetController = this.diagramSetController1;
			// 
			// toolSetListViewPresenter1
			// 
			this.toolSetListViewPresenter1.HideDeniedMenuItems = false;
			this.toolSetListViewPresenter1.ListView = this.toolListView;
			this.toolSetListViewPresenter1.ShowDefaultContextMenu = true;
			this.toolSetListViewPresenter1.ToolSetController = this.toolSetController1;
			// 
			// toolListView
			// 
			this.toolListView.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.toolListView.FullRowSelect = true;
			this.toolListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.toolListView.HideSelection = false;
			this.toolListView.Location = new System.Drawing.Point(-1, 252);
			this.toolListView.MultiSelect = false;
			this.toolListView.Name = "toolListView";
			this.toolListView.ShowItemToolTips = true;
			this.toolListView.Size = new System.Drawing.Size(233, 310);
			this.toolListView.TabIndex = 1;
			this.toolListView.UseCompatibleStateImageBehavior = false;
			// 
			// menuStrip1
			// 
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(1008, 24);
			this.menuStrip1.TabIndex = 2;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileLoadStatisticsToolStripMenuItem,
            this.fileSaveToolStripMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
			this.fileToolStripMenuItem.Text = "File";
			// 
			// fileLoadStatisticsToolStripMenuItem
			// 
			this.fileLoadStatisticsToolStripMenuItem.Name = "fileLoadStatisticsToolStripMenuItem";
			this.fileLoadStatisticsToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
			this.fileLoadStatisticsToolStripMenuItem.Text = "Load statistics";
			// 
			// fileSaveToolStripMenuItem
			// 
			this.fileSaveToolStripMenuItem.Name = "fileSaveToolStripMenuItem";
			this.fileSaveToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
			this.fileSaveToolStripMenuItem.Text = "Save";
			this.fileSaveToolStripMenuItem.Click += new System.EventHandler(this.fileSaveToolStripMenuItem_Click);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1008, 562);
			this.Controls.Add(this.toolListView);
			this.Controls.Add(this.display1);
			this.Controls.Add(this.menuStrip1);
			this.MainMenuStrip = this.menuStrip1;
			this.Name = "Form1";
			this.Text = "Basic Tutorial - Editing";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Dataweb.NShape.WinFormsUI.Display display1;
		private Dataweb.NShape.Project project1;
		private Dataweb.NShape.Controllers.DiagramSetController diagramSetController1;
		private Dataweb.NShape.Advanced.CachedRepository cachedRepository1;
		private Dataweb.NShape.XmlStore xmlStore1;
		private Dataweb.NShape.Controllers.ToolSetController toolSetController1;
		private Dataweb.NShape.WinFormsUI.ToolSetListViewPresenter toolSetListViewPresenter1;
		private System.Windows.Forms.ListView toolListView;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem fileSaveToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem fileLoadStatisticsToolStripMenuItem;
	}
}

