namespace Dataweb.NShape.WinFormsUI {

	partial class Display {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.components = new System.ComponentModel.Container();
			//this.scrollBarV = new System.Windows.Forms.VScrollBar();
			//this.scrollBarH = new System.Windows.Forms.HScrollBar();
			this.scrollBarV = new DisplayVScrollBar();
			this.scrollBarH = new DisplayHScrollBar();
			this.hScrollBarPanel = new System.Windows.Forms.Panel();
			this.displayContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.dummyItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.hScrollBarPanel.SuspendLayout();
			this.displayContextMenuStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// scrollBarV
			// 
			this.scrollBarV.Dock = System.Windows.Forms.DockStyle.Right;
			this.scrollBarV.Location = new System.Drawing.Point(616, 0);
			this.scrollBarV.Name = "scrollBarV";
			this.scrollBarV.Size = new System.Drawing.Size(17, 453);
			this.scrollBarV.TabIndex = 3;
			// 
			// scrollBarH
			// 
			this.scrollBarH.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
							| System.Windows.Forms.AnchorStyles.Right)));
			this.scrollBarH.Location = new System.Drawing.Point(0, 0);
			this.scrollBarH.Name = "scrollBarH";
			this.scrollBarH.Size = new System.Drawing.Size(616, 17);
			this.scrollBarH.TabIndex = 6;
			// 
			// hScrollBarPanel
			// 
			this.hScrollBarPanel.Controls.Add(this.scrollBarH);
			this.hScrollBarPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.hScrollBarPanel.Location = new System.Drawing.Point(0, 453);
			this.hScrollBarPanel.Name = "hScrollBarPanel";
			this.hScrollBarPanel.Size = new System.Drawing.Size(633, 17);
			this.hScrollBarPanel.TabIndex = 7;
			// 
			// displayContextMenuStrip
			// 
			this.displayContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.dummyItem});
			this.displayContextMenuStrip.Name = "contextMenuStrip1";
			this.displayContextMenuStrip.Size = new System.Drawing.Size(79, 26);
			this.displayContextMenuStrip.Closed += new System.Windows.Forms.ToolStripDropDownClosedEventHandler(this.displayContextMenuStrip_Closed);
			this.displayContextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.displayContextMenuStrip_Opening);
			// 
			// dummyItem
			// 
			this.dummyItem.Name = "dummyItem";
			this.dummyItem.Size = new System.Drawing.Size(78, 22);
			// 
			// toolTip
			// 
			this.toolTip.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
			// 
			// Display
			// 
			this.AllowDrop = true;
			//this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			//this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoScroll = true;
			this.Controls.Add(this.scrollBarV);
			this.Controls.Add(this.hScrollBarPanel);
			this.DoubleBuffered = true;
			this.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.Name = "Display";
			this.Size = new System.Drawing.Size(633, 470);
			this.hScrollBarPanel.ResumeLayout(false);
			this.displayContextMenuStrip.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.VScrollBar scrollBarV;
		private System.Windows.Forms.HScrollBar scrollBarH;
		private System.Windows.Forms.Panel hScrollBarPanel;
		private System.Windows.Forms.ContextMenuStrip displayContextMenuStrip;
		private System.Windows.Forms.ToolTip toolTip;
		private System.Windows.Forms.ToolStripMenuItem dummyItem;
	}
}
