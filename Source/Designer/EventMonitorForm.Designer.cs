namespace Dataweb.NShape.Designer {
	
	partial class EventMonitorForm {
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EventMonitorForm));
			this.eventSourcesListBox = new System.Windows.Forms.CheckedListBox();
			this.componentsContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.checkAllMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.uncheckAllMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.splitter1 = new System.Windows.Forms.Splitter();
			this.componentsPanel = new System.Windows.Forms.Panel();
			this.eventPanel = new System.Windows.Forms.Panel();
			this.eventListBox = new System.Windows.Forms.ListBox();
			this.alwaysOnTopCheckBox = new System.Windows.Forms.CheckBox();
			this.componentsContextMenuStrip.SuspendLayout();
			this.componentsPanel.SuspendLayout();
			this.eventPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// eventSourcesListBox
			// 
			this.eventSourcesListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.eventSourcesListBox.CheckOnClick = true;
			this.eventSourcesListBox.ContextMenuStrip = this.componentsContextMenuStrip;
			this.eventSourcesListBox.FormattingEnabled = true;
			this.eventSourcesListBox.IntegralHeight = false;
			this.eventSourcesListBox.Location = new System.Drawing.Point(0, 26);
			this.eventSourcesListBox.Name = "eventSourcesListBox";
			this.eventSourcesListBox.Size = new System.Drawing.Size(200, 394);
			this.eventSourcesListBox.TabIndex = 2;
			this.eventSourcesListBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.componentsListBox_ItemCheck);
			// 
			// componentsContextMenuStrip
			// 
			this.componentsContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.checkAllMenuItem,
            this.uncheckAllMenuItem});
			this.componentsContextMenuStrip.Name = "componentsContextMenuStrip";
			this.componentsContextMenuStrip.Size = new System.Drawing.Size(128, 48);
			// 
			// checkAllMenuItem
			// 
			this.checkAllMenuItem.Name = "checkAllMenuItem";
			this.checkAllMenuItem.Size = new System.Drawing.Size(127, 22);
			this.checkAllMenuItem.Text = "Enable all";
			this.checkAllMenuItem.Click += new System.EventHandler(this.checkAllMenuItem_Click);
			// 
			// uncheckAllMenuItem
			// 
			this.uncheckAllMenuItem.Name = "uncheckAllMenuItem";
			this.uncheckAllMenuItem.Size = new System.Drawing.Size(127, 22);
			this.uncheckAllMenuItem.Text = "Disable all";
			this.uncheckAllMenuItem.Click += new System.EventHandler(this.uncheckAllMenuItem_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(6, 4);
			this.label1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(66, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "Components";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(9, 4);
			this.label2.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(40, 13);
			this.label2.TabIndex = 1;
			this.label2.Text = "Events";
			// 
			// splitter1
			// 
			this.splitter1.Location = new System.Drawing.Point(200, 0);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = new System.Drawing.Size(3, 420);
			this.splitter1.TabIndex = 3;
			this.splitter1.TabStop = false;
			// 
			// componentsPanel
			// 
			this.componentsPanel.Controls.Add(this.eventSourcesListBox);
			this.componentsPanel.Controls.Add(this.label1);
			this.componentsPanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.componentsPanel.Location = new System.Drawing.Point(0, 0);
			this.componentsPanel.Name = "componentsPanel";
			this.componentsPanel.Size = new System.Drawing.Size(200, 420);
			this.componentsPanel.TabIndex = 4;
			// 
			// eventPanel
			// 
			this.eventPanel.Controls.Add(this.eventListBox);
			this.eventPanel.Controls.Add(this.alwaysOnTopCheckBox);
			this.eventPanel.Controls.Add(this.label2);
			this.eventPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.eventPanel.Location = new System.Drawing.Point(203, 0);
			this.eventPanel.Name = "eventPanel";
			this.eventPanel.Size = new System.Drawing.Size(351, 420);
			this.eventPanel.TabIndex = 5;
			// 
			// eventListBox
			// 
			this.eventListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.eventListBox.FormattingEnabled = true;
			this.eventListBox.IntegralHeight = false;
			this.eventListBox.Location = new System.Drawing.Point(0, 26);
			this.eventListBox.Name = "eventListBox";
			this.eventListBox.Size = new System.Drawing.Size(351, 394);
			this.eventListBox.TabIndex = 2;
			// 
			// alwaysOnTopCheckBox
			// 
			this.alwaysOnTopCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.alwaysOnTopCheckBox.AutoSize = true;
			this.alwaysOnTopCheckBox.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.alwaysOnTopCheckBox.Location = new System.Drawing.Point(252, 3);
			this.alwaysOnTopCheckBox.Name = "alwaysOnTopCheckBox";
			this.alwaysOnTopCheckBox.Size = new System.Drawing.Size(96, 17);
			this.alwaysOnTopCheckBox.TabIndex = 3;
			this.alwaysOnTopCheckBox.Text = "Always on Top";
			this.alwaysOnTopCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.alwaysOnTopCheckBox.UseVisualStyleBackColor = true;
			this.alwaysOnTopCheckBox.CheckedChanged += new System.EventHandler(this.alwaysOnTopCheckBox_CheckedChanged);
			// 
			// EventMonitorForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(554, 420);
			this.Controls.Add(this.eventPanel);
			this.Controls.Add(this.splitter1);
			this.Controls.Add(this.componentsPanel);
			this.DoubleBuffered = true;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "EventMonitorForm";
			this.Text = "NShape Event Monitor";
			this.componentsContextMenuStrip.ResumeLayout(false);
			this.componentsPanel.ResumeLayout(false);
			this.componentsPanel.PerformLayout();
			this.eventPanel.ResumeLayout(false);
			this.eventPanel.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.CheckedListBox eventSourcesListBox;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Splitter splitter1;
		private System.Windows.Forms.Panel componentsPanel;
		private System.Windows.Forms.Panel eventPanel;
		private System.Windows.Forms.ContextMenuStrip componentsContextMenuStrip;
		private System.Windows.Forms.ToolStripMenuItem checkAllMenuItem;
		private System.Windows.Forms.ToolStripMenuItem uncheckAllMenuItem;
		private System.Windows.Forms.ListBox eventListBox;
		private System.Windows.Forms.CheckBox alwaysOnTopCheckBox;
	}
}