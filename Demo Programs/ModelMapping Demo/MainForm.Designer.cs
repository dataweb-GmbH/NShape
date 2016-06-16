namespace ModelMapping_Demo {
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
			Dataweb.NShape.RoleBasedSecurityManager roleBasedSecurityManager4 = new Dataweb.NShape.RoleBasedSecurityManager();
			this.display1 = new Dataweb.NShape.WinFormsUI.Display();
			this.diagramSetController = new Dataweb.NShape.Controllers.DiagramSetController();
			this.project = new Dataweb.NShape.Project(this.components);
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.intTrackBar = new System.Windows.Forms.TrackBar();
			this.label2 = new System.Windows.Forms.Label();
			this.floatTrackBar = new System.Windows.Forms.TrackBar();
			this.label1 = new System.Windows.Forms.Label();
			this.checkBox = new System.Windows.Forms.CheckBox();
			this.label3 = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.intTrackBar)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.floatTrackBar)).BeginInit();
			this.SuspendLayout();
			// 
			// display1
			// 
			this.display1.AllowDrop = true;
			this.display1.BackColorGradient = System.Drawing.SystemColors.Control;
			this.display1.DiagramSetController = this.diagramSetController;
			this.display1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.display1.GridColor = System.Drawing.Color.Gainsboro;
			this.display1.GridSize = 19;
			this.display1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.display1.Location = new System.Drawing.Point(0, 0);
			this.display1.Name = "display1";
			this.display1.PropertyController = null;
			this.display1.SelectionHilightColor = System.Drawing.Color.Firebrick;
			this.display1.SelectionInactiveColor = System.Drawing.Color.Gray;
			this.display1.SelectionInteriorColor = System.Drawing.Color.WhiteSmoke;
			this.display1.SelectionNormalColor = System.Drawing.Color.DarkGreen;
			this.display1.Size = new System.Drawing.Size(416, 521);
			this.display1.TabIndex = 0;
			this.display1.ToolPreviewBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(119)))), ((int)(((byte)(136)))), ((int)(((byte)(153)))));
			this.display1.ToolPreviewColor = System.Drawing.Color.FromArgb(((int)(((byte)(96)))), ((int)(((byte)(70)))), ((int)(((byte)(130)))), ((int)(((byte)(180)))));
			// 
			// diagramSetController
			// 
			this.diagramSetController.ActiveTool = null;
			this.diagramSetController.Project = this.project;
			// 
			// project
			// 
			this.project.LibrarySearchPaths = ((System.Collections.Generic.IList<string>)(resources.GetObject("project.LibrarySearchPaths")));
			this.project.Name = null;
			this.project.Repository = null;
			roleBasedSecurityManager4.CurrentRole = Dataweb.NShape.StandardRole.Administrator;
			roleBasedSecurityManager4.CurrentRoleName = "Administrator";
			this.project.SecurityManager = roleBasedSecurityManager4;
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = new System.Drawing.Point(0, 0);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.label3);
			this.splitContainer1.Panel1.Controls.Add(this.intTrackBar);
			this.splitContainer1.Panel1.Controls.Add(this.label2);
			this.splitContainer1.Panel1.Controls.Add(this.floatTrackBar);
			this.splitContainer1.Panel1.Controls.Add(this.label1);
			this.splitContainer1.Panel1.Controls.Add(this.checkBox);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.display1);
			this.splitContainer1.Size = new System.Drawing.Size(629, 521);
			this.splitContainer1.SplitterDistance = 209;
			this.splitContainer1.TabIndex = 1;
			// 
			// intTrackBar
			// 
			this.intTrackBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.intTrackBar.AutoSize = false;
			this.intTrackBar.Location = new System.Drawing.Point(12, 124);
			this.intTrackBar.Maximum = 360;
			this.intTrackBar.Name = "intTrackBar";
			this.intTrackBar.Size = new System.Drawing.Size(194, 31);
			this.intTrackBar.TabIndex = 5;
			this.intTrackBar.TickFrequency = 30;
			this.intTrackBar.Scroll += new System.EventHandler(this.intTrackBar_Scroll);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 202);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(60, 13);
			this.label2.TabIndex = 4;
			this.label2.Text = "Float Value";
			// 
			// floatTrackBar
			// 
			this.floatTrackBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.floatTrackBar.AutoSize = false;
			this.floatTrackBar.Location = new System.Drawing.Point(12, 218);
			this.floatTrackBar.Maximum = 100;
			this.floatTrackBar.Name = "floatTrackBar";
			this.floatTrackBar.Size = new System.Drawing.Size(194, 31);
			this.floatTrackBar.TabIndex = 3;
			this.floatTrackBar.TickFrequency = 10;
			this.floatTrackBar.Scroll += new System.EventHandler(this.floatTrackBar_Scroll);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 108);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(70, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "Integer Value";
			// 
			// checkBox
			// 
			this.checkBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.checkBox.Location = new System.Drawing.Point(15, 45);
			this.checkBox.Name = "checkBox";
			this.checkBox.Size = new System.Drawing.Size(191, 24);
			this.checkBox.TabIndex = 1;
			this.checkBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.checkBox.UseVisualStyleBackColor = true;
			this.checkBox.CheckedChanged += new System.EventHandler(this.checkBox_CheckedChanged);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(12, 29);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(76, 13);
			this.label3.TabIndex = 6;
			this.label3.Text = "Boolean Value";
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(629, 521);
			this.Controls.Add(this.splitContainer1);
			this.Name = "Form1";
			this.Text = "Model Mapping Demo";
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel1.PerformLayout();
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.intTrackBar)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.floatTrackBar)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Dataweb.NShape.WinFormsUI.Display display1;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TrackBar floatTrackBar;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.CheckBox checkBox;
		private Dataweb.NShape.Controllers.DiagramSetController diagramSetController;
		private Dataweb.NShape.Project project;
		private System.Windows.Forms.TrackBar intTrackBar;
		private System.Windows.Forms.Label label3;
	}
}

