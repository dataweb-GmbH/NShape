namespace Database_Designer {
	partial class DatabaseDesignerForm {
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DatabaseDesignerForm));
			Dataweb.NShape.RoleBasedSecurityManager roleBasedSecurityManager1 = new Dataweb.NShape.RoleBasedSecurityManager();
			this.toolStripContainer = new System.Windows.Forms.ToolStripContainer();
			this.statusStrip1 = new System.Windows.Forms.StatusStrip();
			this.display = new Dataweb.NShape.WinFormsUI.Display();
			this.diagramSetController = new Dataweb.NShape.Controllers.DiagramSetController();
			this.project = new Dataweb.NShape.Project(this.components);
			this.cachedRepository = new Dataweb.NShape.Advanced.CachedRepository();
			this.xmlStore = new Dataweb.NShape.XmlStore();
			this.propertyController = new Dataweb.NShape.Controllers.PropertyController();
			this.toolStrip = new System.Windows.Forms.ToolStrip();
			this.clearButton = new System.Windows.Forms.ToolStripButton();
			this.openButton = new System.Windows.Forms.ToolStripButton();
			this.saveButton = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.addDiagramButton = new System.Windows.Forms.ToolStripButton();
			this.diagramsDropDownButton = new System.Windows.Forms.ToolStripDropDownButton();
			this.deleteDiagramButton = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
			this.cutButton = new System.Windows.Forms.ToolStripButton();
			this.copyButton = new System.Windows.Forms.ToolStripButton();
			this.pasteButton = new System.Windows.Forms.ToolStripButton();
			this.deleteButton = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.toolBoxButton = new System.Windows.Forms.ToolStripButton();
			this.propertyWindowButton = new System.Windows.Forms.ToolStripButton();
			this.diagramButton = new System.Windows.Forms.ToolStripButton();
			this.designButton = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.gridButton = new System.Windows.Forms.ToolStripButton();
			this.zoomInButton = new System.Windows.Forms.ToolStripButton();
			this.ZoomOutButton = new System.Windows.Forms.ToolStripButton();
			this.zoomLabel = new System.Windows.Forms.ToolStripLabel();
			this.toolSetPresenter = new Dataweb.NShape.WinFormsUI.ToolSetListViewPresenter(this.components);
			this.toolSetController = new Dataweb.NShape.Controllers.ToolSetController();
			this.propertyPresenter = new Dataweb.NShape.WinFormsUI.PropertyPresenter();
			this.toolStripContainer.BottomToolStripPanel.SuspendLayout();
			this.toolStripContainer.ContentPanel.SuspendLayout();
			this.toolStripContainer.TopToolStripPanel.SuspendLayout();
			this.toolStripContainer.SuspendLayout();
			this.toolStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// toolStripContainer
			// 
			// 
			// toolStripContainer.BottomToolStripPanel
			// 
			this.toolStripContainer.BottomToolStripPanel.Controls.Add(this.statusStrip1);
			// 
			// toolStripContainer.ContentPanel
			// 
			this.toolStripContainer.ContentPanel.Controls.Add(this.display);
			this.toolStripContainer.ContentPanel.Size = new System.Drawing.Size(884, 515);
			this.toolStripContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.toolStripContainer.Location = new System.Drawing.Point(0, 0);
			this.toolStripContainer.Name = "toolStripContainer";
			this.toolStripContainer.Size = new System.Drawing.Size(884, 562);
			this.toolStripContainer.TabIndex = 3;
			this.toolStripContainer.Text = "toolStripContainer";
			// 
			// toolStripContainer.TopToolStripPanel
			// 
			this.toolStripContainer.TopToolStripPanel.Controls.Add(this.toolStrip);
			// 
			// statusStrip1
			// 
			this.statusStrip1.Dock = System.Windows.Forms.DockStyle.None;
			this.statusStrip1.Location = new System.Drawing.Point(0, 0);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.ManagerRenderMode;
			this.statusStrip1.Size = new System.Drawing.Size(884, 22);
			this.statusStrip1.TabIndex = 5;
			this.statusStrip1.Text = "statusStrip1";
			// 
			// display
			// 
			this.display.AllowDrop = true;
			this.display.AutoScroll = true;
			this.display.BackColorGradient = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(226)))), ((int)(((byte)(226)))));
			this.display.BackgroundGradientAngle = 0;
			this.display.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
			this.display.DiagramSetController = this.diagramSetController;
			this.display.Dock = System.Windows.Forms.DockStyle.Fill;
			this.display.GridAlpha = ((byte)(128));
			this.display.GridColor = System.Drawing.Color.White;
			this.display.HideDeniedMenuItems = true;
			this.display.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.display.Location = new System.Drawing.Point(0, 0);
			this.display.Name = "display";
			this.display.PropertyController = this.propertyController;
			this.display.SelectionHilightColor = System.Drawing.Color.Firebrick;
			this.display.SelectionInactiveColor = System.Drawing.Color.Gray;
			this.display.SelectionInteriorColor = System.Drawing.Color.WhiteSmoke;
			this.display.SelectionNormalColor = System.Drawing.Color.DarkGreen;
			this.display.IsGridVisible = false;
			this.display.Size = new System.Drawing.Size(884, 515);
			this.display.SnapToGrid = false;
			this.display.TabIndex = 4;
			this.display.ToolPreviewBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(119)))), ((int)(((byte)(136)))), ((int)(((byte)(153)))));
			this.display.ToolPreviewColor = System.Drawing.Color.FromArgb(((int)(((byte)(96)))), ((int)(((byte)(70)))), ((int)(((byte)(130)))), ((int)(((byte)(180)))));
			this.display.ShapesSelected += new System.EventHandler(this.display_ShapesSelected);
			this.display.ZoomChanged += new System.EventHandler(this.display_ZoomChanged);
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
			this.xmlStore.DesignFileName = "";
			this.xmlStore.DirectoryName = "";
			this.xmlStore.FileExtension = ".xml";
			this.xmlStore.ProjectName = "";
			// 
			// propertyController
			// 
			this.propertyController.Project = this.project;
			this.propertyController.PropertyDisplayMode = Dataweb.NShape.Controllers.NonEditableDisplayMode.ReadOnly;
			// 
			// toolStrip
			// 
			this.toolStrip.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.clearButton,
            this.openButton,
            this.saveButton,
            this.toolStripSeparator1,
            this.addDiagramButton,
            this.diagramsDropDownButton,
            this.deleteDiagramButton,
            this.toolStripSeparator4,
            this.cutButton,
            this.copyButton,
            this.pasteButton,
            this.deleteButton,
            this.toolStripSeparator2,
            this.toolBoxButton,
            this.propertyWindowButton,
            this.diagramButton,
            this.designButton,
            this.toolStripSeparator3,
            this.gridButton,
            this.zoomInButton,
            this.ZoomOutButton,
            this.zoomLabel});
			this.toolStrip.Location = new System.Drawing.Point(3, 0);
			this.toolStrip.Name = "toolStrip";
			this.toolStrip.Size = new System.Drawing.Size(745, 25);
			this.toolStrip.TabIndex = 0;
			// 
			// clearButton
			// 
			this.clearButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.clearButton.Image = ((System.Drawing.Image)(resources.GetObject("clearButton.Image")));
			this.clearButton.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.clearButton.Name = "clearButton";
			this.clearButton.Size = new System.Drawing.Size(23, 22);
			this.clearButton.Text = "New Project";
			this.clearButton.Click += new System.EventHandler(this.clearButton_Click);
			// 
			// openButton
			// 
			this.openButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.openButton.Image = ((System.Drawing.Image)(resources.GetObject("openButton.Image")));
			this.openButton.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.openButton.Name = "openButton";
			this.openButton.Size = new System.Drawing.Size(23, 22);
			this.openButton.Text = "Open Project";
			this.openButton.Click += new System.EventHandler(this.openButton_Click);
			// 
			// saveButton
			// 
			this.saveButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.saveButton.Image = ((System.Drawing.Image)(resources.GetObject("saveButton.Image")));
			this.saveButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.saveButton.Name = "saveButton";
			this.saveButton.Size = new System.Drawing.Size(23, 22);
			this.saveButton.Text = "Save Project";
			this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
			// 
			// addDiagramButton
			// 
			this.addDiagramButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.addDiagramButton.Image = ((System.Drawing.Image)(resources.GetObject("addDiagramButton.Image")));
			this.addDiagramButton.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.addDiagramButton.Name = "addDiagramButton";
			this.addDiagramButton.Size = new System.Drawing.Size(23, 22);
			this.addDiagramButton.Text = "Add new Diagram";
			this.addDiagramButton.Click += new System.EventHandler(this.addDiagramButton_Click);
			// 
			// diagramsDropDownButton
			// 
			this.diagramsDropDownButton.Image = ((System.Drawing.Image)(resources.GetObject("diagramsDropDownButton.Image")));
			this.diagramsDropDownButton.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.diagramsDropDownButton.Name = "diagramsDropDownButton";
			this.diagramsDropDownButton.Size = new System.Drawing.Size(113, 22);
			this.diagramsDropDownButton.Text = "DiagramName";
			// 
			// deleteDiagramButton
			// 
			this.deleteDiagramButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.deleteDiagramButton.Image = ((System.Drawing.Image)(resources.GetObject("deleteDiagramButton.Image")));
			this.deleteDiagramButton.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.deleteDiagramButton.Name = "deleteDiagramButton";
			this.deleteDiagramButton.Size = new System.Drawing.Size(23, 22);
			this.deleteDiagramButton.Text = "Delete Diagram";
			this.deleteDiagramButton.Click += new System.EventHandler(this.deleteDiagramButton_Click);
			// 
			// toolStripSeparator4
			// 
			this.toolStripSeparator4.Name = "toolStripSeparator4";
			this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
			// 
			// cutButton
			// 
			this.cutButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.cutButton.Image = ((System.Drawing.Image)(resources.GetObject("cutButton.Image")));
			this.cutButton.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.cutButton.Name = "cutButton";
			this.cutButton.Size = new System.Drawing.Size(23, 22);
			this.cutButton.Text = "Cut";
			this.cutButton.Click += new System.EventHandler(this.cutButton_Click);
			// 
			// copyButton
			// 
			this.copyButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.copyButton.Image = ((System.Drawing.Image)(resources.GetObject("copyButton.Image")));
			this.copyButton.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.copyButton.Name = "copyButton";
			this.copyButton.Size = new System.Drawing.Size(23, 22);
			this.copyButton.Text = "Copy";
			this.copyButton.Click += new System.EventHandler(this.copyButton_Click);
			// 
			// pasteButton
			// 
			this.pasteButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.pasteButton.Image = ((System.Drawing.Image)(resources.GetObject("pasteButton.Image")));
			this.pasteButton.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.pasteButton.Name = "pasteButton";
			this.pasteButton.Size = new System.Drawing.Size(23, 22);
			this.pasteButton.Text = "Paste";
			this.pasteButton.Click += new System.EventHandler(this.pasteButton_Click);
			// 
			// deleteButton
			// 
			this.deleteButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.deleteButton.Image = ((System.Drawing.Image)(resources.GetObject("deleteButton.Image")));
			this.deleteButton.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.deleteButton.Name = "deleteButton";
			this.deleteButton.Size = new System.Drawing.Size(23, 22);
			this.deleteButton.Text = "Delete";
			this.deleteButton.Click += new System.EventHandler(this.deleteButton_Click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
			// 
			// toolBoxButton
			// 
			this.toolBoxButton.Checked = true;
			this.toolBoxButton.CheckState = System.Windows.Forms.CheckState.Checked;
			this.toolBoxButton.Image = ((System.Drawing.Image)(resources.GetObject("toolBoxButton.Image")));
			this.toolBoxButton.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.toolBoxButton.Name = "toolBoxButton";
			this.toolBoxButton.Size = new System.Drawing.Size(70, 22);
			this.toolBoxButton.Text = "Toolbox";
			this.toolBoxButton.Click += new System.EventHandler(this.toolBoxButton_Click);
			// 
			// propertyWindowButton
			// 
			this.propertyWindowButton.Image = ((System.Drawing.Image)(resources.GetObject("propertyWindowButton.Image")));
			this.propertyWindowButton.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.propertyWindowButton.Name = "propertyWindowButton";
			this.propertyWindowButton.Size = new System.Drawing.Size(80, 22);
			this.propertyWindowButton.Text = "Properties";
			this.propertyWindowButton.Click += new System.EventHandler(this.propertyWindowButton_Click);
			// 
			// diagramButton
			// 
			this.diagramButton.Image = ((System.Drawing.Image)(resources.GetObject("diagramButton.Image")));
			this.diagramButton.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.diagramButton.Name = "diagramButton";
			this.diagramButton.Size = new System.Drawing.Size(72, 22);
			this.diagramButton.Text = "Diagram";
			this.diagramButton.Click += new System.EventHandler(this.diagramButton_Click);
			// 
			// designButton
			// 
			this.designButton.Image = ((System.Drawing.Image)(resources.GetObject("designButton.Image")));
			this.designButton.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.designButton.Name = "designButton";
			this.designButton.Size = new System.Drawing.Size(63, 22);
			this.designButton.Text = "Design";
			this.designButton.Click += new System.EventHandler(this.designButton_Click);
			// 
			// toolStripSeparator3
			// 
			this.toolStripSeparator3.Name = "toolStripSeparator3";
			this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
			// 
			// gridButton
			// 
			this.gridButton.CheckOnClick = true;
			this.gridButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.gridButton.Image = ((System.Drawing.Image)(resources.GetObject("gridButton.Image")));
			this.gridButton.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.gridButton.Name = "gridButton";
			this.gridButton.Size = new System.Drawing.Size(23, 22);
			this.gridButton.Text = "Toggle Grid";
			this.gridButton.Click += new System.EventHandler(this.gridButton_Click);
			// 
			// zoomInButton
			// 
			this.zoomInButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.zoomInButton.Image = ((System.Drawing.Image)(resources.GetObject("zoomInButton.Image")));
			this.zoomInButton.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.zoomInButton.Name = "zoomInButton";
			this.zoomInButton.Size = new System.Drawing.Size(23, 22);
			this.zoomInButton.Text = "Zoom in";
			this.zoomInButton.Click += new System.EventHandler(this.zoomInButton_Click);
			// 
			// ZoomOutButton
			// 
			this.ZoomOutButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.ZoomOutButton.Image = ((System.Drawing.Image)(resources.GetObject("ZoomOutButton.Image")));
			this.ZoomOutButton.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.ZoomOutButton.Name = "ZoomOutButton";
			this.ZoomOutButton.Size = new System.Drawing.Size(23, 22);
			this.ZoomOutButton.Text = "Zoom out";
			this.ZoomOutButton.Click += new System.EventHandler(this.zoomOutButton_Click);
			// 
			// zoomLabel
			// 
			this.zoomLabel.Name = "zoomLabel";
			this.zoomLabel.Size = new System.Drawing.Size(35, 22);
			this.zoomLabel.Text = "100%";
			// 
			// toolSetPresenter
			// 
			this.toolSetPresenter.HideDeniedMenuItems = false;
			this.toolSetPresenter.ListView = null;
			this.toolSetPresenter.ShowDefaultContextMenu = true;
			this.toolSetPresenter.ToolSetController = this.toolSetController;
			// 
			// toolSetController
			// 
			this.toolSetController.DiagramSetController = this.diagramSetController;
			this.toolSetController.DesignEditorSelected += new System.EventHandler(this.toolSetController_DesignEditorSelected);
			this.toolSetController.TemplateEditorSelected += new System.EventHandler<Dataweb.NShape.Controllers.TemplateEditorEventArgs>(this.toolSetController_TemplateEditorSelected);
			// 
			// propertyPresenter
			// 
			this.propertyPresenter.PrimaryPropertyGrid = null;
			this.propertyPresenter.PropertyController = this.propertyController;
			this.propertyPresenter.SecondaryPropertyGrid = null;
			// 
			// DatabaseDesignerForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Info;
			this.ClientSize = new System.Drawing.Size(884, 562);
			this.Controls.Add(this.toolStripContainer);
			this.DoubleBuffered = true;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "DatabaseDesignerForm";
			this.Text = "DatabaseDesignerForm";
			this.Load += new System.EventHandler(this.MainForm_Load);
			this.Shown += new System.EventHandler(this.DatabaseDesignerForm_Shown);
			this.toolStripContainer.BottomToolStripPanel.ResumeLayout(false);
			this.toolStripContainer.BottomToolStripPanel.PerformLayout();
			this.toolStripContainer.ContentPanel.ResumeLayout(false);
			this.toolStripContainer.TopToolStripPanel.ResumeLayout(false);
			this.toolStripContainer.TopToolStripPanel.PerformLayout();
			this.toolStripContainer.ResumeLayout(false);
			this.toolStripContainer.PerformLayout();
			this.toolStrip.ResumeLayout(false);
			this.toolStrip.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private Dataweb.NShape.WinFormsUI.ToolSetListViewPresenter toolSetPresenter;
		private Dataweb.NShape.Project project;
		private System.Windows.Forms.ToolStripContainer toolStripContainer;
		private System.Windows.Forms.ToolStrip toolStrip;
		private System.Windows.Forms.ToolStripButton clearButton;
		private System.Windows.Forms.ToolStripButton openButton;
		private System.Windows.Forms.ToolStripButton saveButton;
		private System.Windows.Forms.ToolStripButton designButton;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripButton zoomInButton;
		private System.Windows.Forms.ToolStripButton ZoomOutButton;
		private System.Windows.Forms.ToolStripButton cutButton;
		private System.Windows.Forms.ToolStripButton copyButton;
		private System.Windows.Forms.ToolStripButton pasteButton;
		private System.Windows.Forms.ToolStripButton deleteButton;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
		private System.Windows.Forms.ToolStripLabel zoomLabel;
		private System.Windows.Forms.ToolStripButton gridButton;
		private System.Windows.Forms.ToolStripButton toolBoxButton;
		private System.Windows.Forms.ToolStripButton diagramButton;
		private System.Windows.Forms.ToolStripButton addDiagramButton;
		private Dataweb.NShape.WinFormsUI.Display display;
		private System.Windows.Forms.StatusStrip statusStrip1;
		private System.Windows.Forms.ToolStripButton propertyWindowButton;
		private System.Windows.Forms.ToolStripButton deleteDiagramButton;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
		private System.Windows.Forms.ToolStripDropDownButton diagramsDropDownButton;
		private Dataweb.NShape.WinFormsUI.PropertyPresenter propertyPresenter;
		private Dataweb.NShape.Controllers.DiagramSetController diagramSetController;
		private Dataweb.NShape.Controllers.ToolSetController toolSetController;
		private Dataweb.NShape.Controllers.PropertyController propertyController;
		private Dataweb.NShape.Advanced.CachedRepository cachedRepository;
		private Dataweb.NShape.XmlStore xmlStore;
	}
}

