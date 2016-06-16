namespace NShapeViewer {
	partial class DisplayChildForm {
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
			this.statusStrip = new System.Windows.Forms.StatusStrip();
			this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
			this.toolStripStatusLabel6 = new System.Windows.Forms.ToolStripStatusLabel();
			this.shapeCntLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.toolStripStatusLabel7 = new System.Windows.Forms.ToolStripStatusLabel();
			this.sizeLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
			this.toolStripStatusLabel8 = new System.Windows.Forms.ToolStripStatusLabel();
			this.zoomLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.toolStripStatusLabel4 = new System.Windows.Forms.ToolStripStatusLabel();
			this.mousePosLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.toolStripStatusLabel5 = new System.Windows.Forms.ToolStripStatusLabel();
			this.topLeftLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.toolStripStatusLabel9 = new System.Windows.Forms.ToolStripStatusLabel();
			this.bottomRightLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.display = new Dataweb.NShape.WinFormsUI.Display();
			this.toolStripStatusLabel10 = new System.Windows.Forms.ToolStripStatusLabel();
			this.statusStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// statusStrip
			// 
			this.statusStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Visible;
			this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel2,
            this.toolStripStatusLabel6,
            this.shapeCntLabel,
            this.toolStripStatusLabel7,
            this.sizeLabel,
            this.toolStripStatusLabel10,
            this.toolStripStatusLabel1,
            this.toolStripStatusLabel8,
            this.zoomLabel,
            this.toolStripStatusLabel4,
            this.mousePosLabel,
            this.toolStripStatusLabel5,
            this.topLeftLabel,
            this.toolStripStatusLabel9,
            this.bottomRightLabel});
			this.statusStrip.Location = new System.Drawing.Point(0, 620);
			this.statusStrip.Name = "statusStrip";
			this.statusStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.ManagerRenderMode;
			this.statusStrip.Size = new System.Drawing.Size(862, 22);
			this.statusStrip.TabIndex = 1;
			this.statusStrip.Text = "statusStrip1";
			// 
			// toolStripStatusLabel2
			// 
			this.toolStripStatusLabel2.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
			this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
			this.toolStripStatusLabel2.Size = new System.Drawing.Size(58, 17);
			this.toolStripStatusLabel2.Text = "Diagram:";
			// 
			// toolStripStatusLabel6
			// 
			this.toolStripStatusLabel6.Name = "toolStripStatusLabel6";
			this.toolStripStatusLabel6.Size = new System.Drawing.Size(10, 17);
			this.toolStripStatusLabel6.Text = " ";
			// 
			// shapeCntLabel
			// 
			this.shapeCntLabel.ImageTransparentColor = System.Drawing.Color.White;
			this.shapeCntLabel.Name = "shapeCntLabel";
			this.shapeCntLabel.Size = new System.Drawing.Size(53, 17);
			this.shapeCntLabel.Text = "0 Shapes";
			this.shapeCntLabel.ToolTipText = "Number of shapes in the diagram";
			// 
			// toolStripStatusLabel7
			// 
			this.toolStripStatusLabel7.Name = "toolStripStatusLabel7";
			this.toolStripStatusLabel7.Size = new System.Drawing.Size(10, 17);
			this.toolStripStatusLabel7.Text = " ";
			// 
			// sizeLabel
			// 
			this.sizeLabel.Image = global::NShapeViewer.Properties.Resources.Size;
			this.sizeLabel.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.sizeLabel.Name = "sizeLabel";
			this.sizeLabel.Size = new System.Drawing.Size(91, 17);
			this.sizeLabel.Text = "Diagram Size";
			this.sizeLabel.ToolTipText = "Size of the diagram";
			// 
			// toolStripStatusLabel1
			// 
			this.toolStripStatusLabel1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
			this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
			this.toolStripStatusLabel1.Size = new System.Drawing.Size(51, 17);
			this.toolStripStatusLabel1.Text = "Display:";
			// 
			// toolStripStatusLabel8
			// 
			this.toolStripStatusLabel8.Name = "toolStripStatusLabel8";
			this.toolStripStatusLabel8.Size = new System.Drawing.Size(10, 17);
			this.toolStripStatusLabel8.Text = " ";
			// 
			// zoomLabel
			// 
			this.zoomLabel.Image = global::NShapeViewer.Properties.Resources.ZoomBtn;
			this.zoomLabel.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.zoomLabel.Name = "zoomLabel";
			this.zoomLabel.Size = new System.Drawing.Size(92, 17);
			this.zoomLabel.Text = "Zoom: 100 %";
			this.zoomLabel.ToolTipText = "Display zoom";
			// 
			// toolStripStatusLabel4
			// 
			this.toolStripStatusLabel4.Name = "toolStripStatusLabel4";
			this.toolStripStatusLabel4.Size = new System.Drawing.Size(10, 17);
			this.toolStripStatusLabel4.Text = " ";
			// 
			// mousePosLabel
			// 
			this.mousePosLabel.Image = global::NShapeViewer.Properties.Resources.Position;
			this.mousePosLabel.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.mousePosLabel.Name = "mousePosLabel";
			this.mousePosLabel.Size = new System.Drawing.Size(105, 17);
			this.mousePosLabel.Text = "Mouse Position";
			this.mousePosLabel.ToolTipText = "Position of the mouse cursor";
			// 
			// toolStripStatusLabel5
			// 
			this.toolStripStatusLabel5.Name = "toolStripStatusLabel5";
			this.toolStripStatusLabel5.Size = new System.Drawing.Size(10, 17);
			this.toolStripStatusLabel5.Text = " ";
			// 
			// topLeftLabel
			// 
			this.topLeftLabel.Image = global::NShapeViewer.Properties.Resources.TopLeft;
			this.topLeftLabel.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.topLeftLabel.Name = "topLeftLabel";
			this.topLeftLabel.Size = new System.Drawing.Size(67, 17);
			this.topLeftLabel.Text = "Top Left";
			this.topLeftLabel.ToolTipText = "Top left corner of the displayed area";
			// 
			// toolStripStatusLabel9
			// 
			this.toolStripStatusLabel9.Name = "toolStripStatusLabel9";
			this.toolStripStatusLabel9.Size = new System.Drawing.Size(10, 17);
			this.toolStripStatusLabel9.Text = " ";
			// 
			// bottomRightLabel
			// 
			this.bottomRightLabel.Image = global::NShapeViewer.Properties.Resources.BottomRight;
			this.bottomRightLabel.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.bottomRightLabel.Name = "bottomRightLabel";
			this.bottomRightLabel.Size = new System.Drawing.Size(94, 17);
			this.bottomRightLabel.Text = "Bottom Right";
			this.bottomRightLabel.ToolTipText = "Bottom right corner of the displayed area";
			// 
			// display
			// 
			this.display.AllowDrop = true;
			this.display.BackColor = System.Drawing.Color.WhiteSmoke;
			this.display.BackColorGradient = System.Drawing.Color.LightGray;
			this.display.BackgroundGradientAngle = 0;
			this.display.ConnectionPointShape = Dataweb.NShape.Controllers.ControlPointShape.Circle;
			this.display.ControlPointAlpha = ((byte)(255));
			this.display.DiagramSetController = null;
			this.display.Dock = System.Windows.Forms.DockStyle.Fill;
			this.display.GridAlpha = ((byte)(255));
			this.display.GridColor = System.Drawing.Color.White;
			this.display.GridSize = 20;
			this.display.GripSize = 3;
			this.display.HideDeniedMenuItems = false;
			this.display.HighQualityBackground = true;
			this.display.HighQualityRendering = true;
			this.display.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.display.Location = new System.Drawing.Point(0, 0);
			this.display.MinRotateRange = 30;
			this.display.Name = "display";
			this.display.PropertyController = null;
			this.display.RenderingQualityHighQuality = Dataweb.NShape.Advanced.RenderingQuality.HighQuality;
			this.display.RenderingQualityLowQuality = Dataweb.NShape.Advanced.RenderingQuality.DefaultQuality;
			this.display.ResizeGripShape = Dataweb.NShape.Controllers.ControlPointShape.Square;
			this.display.SelectionHilightColor = System.Drawing.Color.Firebrick;
			this.display.SelectionInactiveColor = System.Drawing.Color.Gray;
			this.display.SelectionInteriorColor = System.Drawing.Color.WhiteSmoke;
			this.display.SelectionNormalColor = System.Drawing.Color.DarkGreen;
			this.display.ShowDefaultContextMenu = false;
			this.display.IsGridVisible = false;
			this.display.ShowScrollBars = true;
			this.display.Size = new System.Drawing.Size(862, 620);
			this.display.SnapDistance = 5;
			this.display.SnapToGrid = true;
			this.display.TabIndex = 0;
			this.display.ToolPreviewBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(119)))), ((int)(((byte)(136)))), ((int)(((byte)(153)))));
			this.display.ToolPreviewColor = System.Drawing.Color.FromArgb(((int)(((byte)(96)))), ((int)(((byte)(70)))), ((int)(((byte)(130)))), ((int)(((byte)(180)))));
			this.display.ZoomLevel = 100;
			this.display.ZoomWithMouseWheel = true;
			this.display.DiagramChanged += new System.EventHandler(this.display_DiagramChanged);
			this.display.Scroll += new System.Windows.Forms.ScrollEventHandler(this.display_Scroll);
			this.display.SizeChanged += new System.EventHandler(this.display_SizeChanged);
			this.display.ZoomChanged += new System.EventHandler(this.display_ZoomChanged);
			this.display.DiagramChanging += new System.EventHandler(this.display_DiagramChanging);
			this.display.ShapeClick += new System.EventHandler<Dataweb.NShape.Controllers.DiagramPresenterShapeClickEventArgs>(this.display_ShapeClick);
			this.display.MouseMove += new System.Windows.Forms.MouseEventHandler(this.display_MouseMove);
			this.display.ShapeDoubleClick += new System.EventHandler<Dataweb.NShape.Controllers.DiagramPresenterShapeClickEventArgs>(this.display_ShapeDoubleClick);
			// 
			// toolStripStatusLabel10
			// 
			this.toolStripStatusLabel10.Name = "toolStripStatusLabel10";
			this.toolStripStatusLabel10.Size = new System.Drawing.Size(145, 17);
			this.toolStripStatusLabel10.Spring = true;
			// 
			// DisplayChildForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(862, 642);
			this.Controls.Add(this.display);
			this.Controls.Add(this.statusStrip);
			this.Name = "DisplayChildForm";
			this.Text = "DiagramDisplayForm";
			this.Shown += new System.EventHandler(this.DisplayChildForm_Shown);
			this.statusStrip.ResumeLayout(false);
			this.statusStrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Dataweb.NShape.WinFormsUI.Display display;
		private System.Windows.Forms.StatusStrip statusStrip;
		private System.Windows.Forms.ToolStripStatusLabel mousePosLabel;
		private System.Windows.Forms.ToolStripStatusLabel zoomLabel;
		private System.Windows.Forms.ToolStripStatusLabel shapeCntLabel;
		private System.Windows.Forms.ToolStripStatusLabel topLeftLabel;
		private System.Windows.Forms.ToolStripStatusLabel bottomRightLabel;
		private System.Windows.Forms.ToolStripStatusLabel sizeLabel;
		private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
		private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
		private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel6;
		private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel7;
		private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel4;
		private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel5;
		private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel8;
		private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel9;
		private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel10;
	}
}