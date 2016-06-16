namespace ArchiSketch {
	partial class ShapePropertiesDialog {
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
			this.shapePropertyGrid = new System.Windows.Forms.PropertyGrid();
			this.propertyPresenter = new Dataweb.NShape.WinFormsUI.PropertyPresenter();
			this.propertyController = new Dataweb.NShape.Controllers.PropertyController();
			this.SuspendLayout();
			// 
			// shapePropertyGrid
			// 
			this.shapePropertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.shapePropertyGrid.Location = new System.Drawing.Point(0, 0);
			this.shapePropertyGrid.Name = "shapePropertyGrid";
			this.shapePropertyGrid.Size = new System.Drawing.Size(341, 504);
			this.shapePropertyGrid.TabIndex = 0;
			// 
			// propertyPresenter
			// 
			this.propertyPresenter.PrimaryPropertyGrid = this.shapePropertyGrid;
			this.propertyPresenter.PropertyController = this.propertyController;
			this.propertyPresenter.SecondaryPropertyGrid = null;
			// 
			// propertyController
			// 
			this.propertyController.Project = null;
			this.propertyController.PropertyDisplayMode = Dataweb.NShape.Controllers.NonEditableDisplayMode.ReadOnly;
			// 
			// ShapePropertiesDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(341, 504);
			this.Controls.Add(this.shapePropertyGrid);
			this.Name = "ShapePropertiesDialog";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Shape Properties";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ShapePropertiesDialog_FormClosed);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.PropertyGrid shapePropertyGrid;
		private Dataweb.NShape.WinFormsUI.PropertyPresenter propertyPresenter;
		internal Dataweb.NShape.Controllers.PropertyController propertyController;
	}
}