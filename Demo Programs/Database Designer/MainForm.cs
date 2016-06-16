/******************************************************************************
  Copyright 2009-2012 dataweb GmbH
  This file is part of the NShape framework.
  NShape is free software: you can redistribute it and/or modify it under the 
  terms of the GNU General Public License as published by the Free Software 
  Foundation, either version 3 of the License, or (at your option) any later 
  version.
  NShape is distributed in the hope that it will be useful, but WITHOUT ANY
  WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR 
  A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
  You should have received a copy of the GNU General Public License along with 
  NShape. If not, see <http://www.gnu.org/licenses/>.
******************************************************************************/

using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

using Dataweb.NShape;
using Dataweb.NShape.Advanced;
using Dataweb.NShape.GeneralShapes;
using Dataweb.NShape.SoftwareArchitectureShapes;
using Dataweb.NShape.WinFormsUI;


namespace Database_Designer {

	internal enum DockType { 
		None = 0,
		Left = 1, 
		Top = 2, 
		Right = 4, 
		Bottom = 8
	}


	public partial class DatabaseDesignerForm : Form {

		public DatabaseDesignerForm() {
			InitializeComponent();

			toolBoxWindow = new ToolBoxWindow();
			toolBoxWindow.Owner = this;
			toolBoxWindowDockType = DockType.Top ^ DockType.Right ^ DockType.Bottom;

			propertyWindow = new PropertyWindow();
			propertyWindow.Owner = this;
			propertyWindowDockType = DockType.Top ^ DockType.Right ^ DockType.Bottom;
		}

		#region Implementation

		private void MainForm_Load(object sender, EventArgs e) {
			// Connect NShape components
			propertyPresenter.PrimaryPropertyGrid = propertyWindow.PropertyGrid;
			toolSetPresenter.ListView = toolBoxWindow.ListView;
			// Enable loading needed Libraries
			project.AutoLoadLibraries = true;
			// Add library search path
			project.LibrarySearchPaths.Add(Application.StartupPath);
			// Add persistent libraries
			project.AddLibraryByName("Dataweb.NShape.GeneralShapes", false);
			project.AddLibraryByName("Dataweb.NShape.SoftwareArchitectureShapes", false);

			NewProject();
		}


		/// <summary>
		/// Create tools for the shape toolbox and sort them into custom categories
		/// </summary>
		private void CreateTools() {
			toolSetPresenter.ToolSetController.Clear();
			toolSetPresenter.ToolSetController.AddTool(new SelectionTool(), true);

			string category = "Database Entities";
			DatabaseSymbol databaseShape = (DatabaseSymbol)project.ShapeTypes["Database"].CreateInstance();
			databaseShape.Width = 120;
			databaseShape.Height = 120;
			databaseShape.FillStyle = project.Design.FillStyles.Yellow;
			databaseShape.CharacterStyle = project.Design.CharacterStyles.Heading3;
			databaseShape.Text = "Database";
			CreateTemplateAndTool("Database", category, databaseShape);

			EntitySymbol tableShape = (EntitySymbol)project.ShapeTypes["Entity"].CreateInstance();
			tableShape.Width = 100;
			tableShape.Height = 160;
			tableShape.FillStyle = project.Design.FillStyles.Red;
			tableShape.CharacterStyle = project.Design.CharacterStyles.Heading3;
			tableShape.ParagraphStyle = project.Design.ParagraphStyles.Title;
			tableShape.ColumnCharacterStyle = project.Design.CharacterStyles.Caption;
			tableShape.ColumnParagraphStyle = project.Design.ParagraphStyles.Label;
			CreateTemplateAndTool("Entity", category, tableShape);

			RectangularLine line;
			ShapeType relationShapeType = project.ShapeTypes["RectangularLine"];
			line = (RectangularLine)relationShapeType.CreateInstance();
			line.LineStyle = project.Design.LineStyles.Thick;
			CreateTemplateAndTool("Relationship", category, line);

			line = (RectangularLine)relationShapeType.CreateInstance();
			line.LineStyle = project.Design.LineStyles.Thick;
			line.EndCapStyle = project.Design.CapStyles.ClosedArrow;
			CreateTemplateAndTool("1:n Relationship", category, line);

			line = (RectangularLine)relationShapeType.CreateInstance();
			line.LineStyle = project.Design.LineStyles.Thick;
			line.StartCapStyle = project.Design.CapStyles.ClosedArrow;
			line.EndCapStyle = project.Design.CapStyles.ClosedArrow;
			CreateTemplateAndTool("n:m Relationship", category, line);

			CloudSymbol cloudShape = (CloudSymbol)project.ShapeTypes["Cloud"].CreateInstance();
			cloudShape.FillStyle = project.Design.FillStyles.Blue;
			cloudShape.CharacterStyle = project.Design.CharacterStyles.Heading1;
			cloudShape.Width = 300;
			cloudShape.Height = 160;
			cloudShape.Text = "WAN / LAN";
			CreateTemplateAndTool("Cloud", category, cloudShape);

			category = "Description";
			Text text = (Text)project.ShapeTypes["Text"].CreateInstance();
			text.CharacterStyle = project.Design.CharacterStyles.Normal;
			text.Width = 100;
			CreateTemplateAndTool("Text", category, text);

			AnnotationSymbol annotationShape = (AnnotationSymbol)project.ShapeTypes["Annotation"].CreateInstance();
			annotationShape.FillStyle = project.Design.FillStyles.White;
			annotationShape.CharacterStyle = project.Design.CharacterStyles.Caption;
			annotationShape.ParagraphStyle = project.Design.ParagraphStyles.Text;
			annotationShape.Width = 120;
			annotationShape.Height = 120;
			CreateTemplateAndTool("Annotation", category, annotationShape);

			category = "Miscellaneous";
			RoundedBox roundedRectangle = (RoundedBox)project.ShapeTypes["RoundedBox"].CreateInstance();
			roundedRectangle.FillStyle = project.Design.FillStyles.Green;
			roundedRectangle.Width = 120;
			roundedRectangle.Height = 80;
			CreateTemplateAndTool("Box", category, roundedRectangle);

			Ellipse ellipse = (Ellipse)project.ShapeTypes["Ellipse"].CreateInstance();
			ellipse.FillStyle = project.Design.FillStyles.Yellow;
			ellipse.Width = 120;
			ellipse.Height = 80;
			CreateTemplateAndTool("Ellipse", category, ellipse);

			Picture picture = (Picture)project.ShapeTypes["Picture"].CreateInstance();
			picture.FillStyle = project.Design.FillStyles.Transparent;
			picture.Width = 120;
			picture.Height = 120;
			CreateTemplateAndTool("Picture", category, picture);

			ShapeType arcShapeType = project.ShapeTypes["CircularArc"];
			CircularArc arc;
			arc = (CircularArc)arcShapeType.CreateInstance();
			arc.LineStyle = project.Design.LineStyles.Thick;
			CreateTemplateAndTool("Arc", category, arc);

			arc = (CircularArc)arcShapeType.CreateInstance();
			arc.LineStyle = project.Design.LineStyles.Thick;
			arc.EndCapStyle = project.Design.CapStyles.ClosedArrow;
			CreateTemplateAndTool("Bowed Arrow", category, arc);

			arc = (CircularArc)arcShapeType.CreateInstance();
			arc.LineStyle = project.Design.LineStyles.Thick;
			arc.StartCapStyle = project.Design.CapStyles.ClosedArrow;
			arc.EndCapStyle = project.Design.CapStyles.ClosedArrow;
			CreateTemplateAndTool("Bowed Double Arrow", category, arc);
		}


		/// <summary>
		/// Creates a 'TemplateTool' for the shape toolbox and adds it to the specified toolbox category
		/// </summary>
		private void CreateTemplateAndTool(string name, string category, Shape shape) {
			Template template = new Template(name, shape);
			toolSetPresenter.ToolSetController.CreateTemplateTool(template, category);
			project.Repository.InsertAll(template);
		}


		private void AddNewDiagram() {
			// Count diagrams in order to get a decent default name for the new diagram
			int diagramCnt = 0;
			foreach (Diagram d in project.Repository.GetDiagrams())
				diagramCnt++;
			
			// Create the diagram and set default values
			Diagram diagram = new Diagram(string.Format("Diagram {0}", diagramCnt + 1));
			diagram.Width = 1600;
			diagram.Height = 1200;
			// Set background colors and a background image
			diagram.BackgroundGradientColor = Color.WhiteSmoke;
			if (diagramCnt % 2 == 0)
				diagram.BackgroundColor = Color.DarkRed;
			else diagram.BackgroundColor = Color.DarkBlue;
			diagram.BackgroundImage = new NamedImage(Database_Designer.Properties.Resources.NY028_3, "BackgroundImage");
			diagram.BackgroundImageLayout = ImageLayoutMode.Fit;

			// Insert the diagram into the repository (otherwise it won't be saved to file)
			project.Repository.InsertAll(diagram);
			// Display the diagram
			display.Diagram = diagram;

			// Add a menu item to the main menu for displaying this diagram 
			ToolStripMenuItem item = new ToolStripMenuItem();
			item.Text = diagram.Name;
			item.Tag = diagram;
			item.Click += new EventHandler(diagramDropDownItem_Click);
			
			diagramsDropDownButton.DropDown.Items.Add(item);
			diagramsDropDownButton.Text = diagram.Name;
		}


		private void AddLoadedDiagram(Diagram diagram) {
			display.Diagram = diagram;

			// Add a menu item to the main menu for displaying this diagram 
			ToolStripMenuItem item = new ToolStripMenuItem();
			item.Text = diagram.Name;
			item.Tag = diagram;
			item.Click += new EventHandler(diagramDropDownItem_Click);

			diagramsDropDownButton.DropDown.Items.Add(item);
			diagramsDropDownButton.Text = diagram.Name;
		}


		private void ClearLoadedDiagrams() {
			// clear all menu items used for selecting the diagram
			for (int i = diagramsDropDownButton.DropDown.Items.Count - 1; i >= 0; --i) {
				diagramsDropDownButton.DropDown.Items[i].Click -= diagramDropDownItem_Click;
				diagramsDropDownButton.DropDown.Items.RemoveAt(i);
			}
		}


		private void DeleteDiagram(Diagram diagram) {
			// Delete menu item 
			for (int i = diagramsDropDownButton.DropDown.Items.Count - 1; i >= 0; --i) {
				if (diagramsDropDownButton.DropDown.Items[i].Tag == diagram) {
					diagramsDropDownButton.DropDown.Items[i].Click -= new EventHandler(diagramDropDownItem_Click);
					diagramsDropDownButton.DropDown.Items.RemoveAt(i);

					// Delete the diagram including its contents
					project.Repository.DeleteAll(diagram);
					diagram = null;
				}
			}

			// Switch to the previous diagram
			if (display.Diagram == null && diagramsDropDownButton.DropDown.Items.Count > 0)
				display.Diagram = diagramsDropDownButton.DropDown.Items[0].Tag as Diagram;
			else {
				display.Refresh();
				diagramsDropDownButton.Text = "<No Diagrams>";
			}
		}


		private void NewProject(){
			// If a project is already open, close it first.
			if (project.IsOpen) {
				ClearLoadedDiagrams();
				project.Close();
			}

			// Clear project location and file extension and set the default project name
			// This will force the user to select a new location and name for the project when saving.
			xmlStore.DirectoryName = xmlStore.FileExtension = string.Empty;
			project.Name = defaultProjectName;
			project.Create();

			// (Re-)fill the shape toolbox
			CreateTools();

			// Add a new diagram
			AddNewDiagram();
			SetButtonStates();
		}


		private void LoadRepository() {
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.Filter = fileFilter;
			if (dlg.ShowDialog() == DialogResult.OK) {
				if (!string.IsNullOrEmpty(dlg.FileName) && File.Exists(dlg.FileName)) {
					// Close the current project and clear the UI
					project.Close();
					ClearLoadedDiagrams();

					// Set project name, location and file extension
					xmlStore.FileExtension = Path.GetExtension(dlg.FileName);
					xmlStore.DirectoryName = Path.GetDirectoryName(dlg.FileName);
					project.Name = Path.GetFileNameWithoutExtension(dlg.FileName);
					// Load the project from file
					project.Open();

					// As we want a custom toolbox, we have to clear the tools created when loading the 
					// project ans re-fill the toolbox with tools for the existing templates:
					toolSetController.Clear();
					toolSetController.AddTool(new SelectionTool(), true);
					foreach (Template template in project.Repository.GetTemplates()) {
						// Set the default category for all shapes not handled here
						string category = null;
						if (template.Shape is DatabaseSymbol
							|| template.Shape is EntitySymbol
							|| template.Shape is Polyline
							|| template.Shape is CloudSymbol) {
							category = "Database Entities";
						} else if (template.Shape is Text
							|| template.Shape is AnnotationSymbol) {
							category = "Description";
						} else category = "Miscellaneous";

						// Create a tool for the loaded template
						toolSetController.CreateTemplateTool(template, category);
					}

					// And add menu entries for selecting the loaded diagrams
					foreach (Diagram diagram in project.Repository.GetDiagrams())
						AddLoadedDiagram(diagram);
				} else MessageBox.Show(string.Format("File '{0}' does not exist.", dlg.FileName));
			}
		}


		private void SaveRepository() {
			using (SaveFileDialog dlg = new SaveFileDialog()) {
				dlg.CreatePrompt = false;
				dlg.Filter = fileFilter;
				if (!string.IsNullOrEmpty(xmlStore.DirectoryName))
					dlg.InitialDirectory = xmlStore.DirectoryName;
				dlg.FileName = project.Name + (string.IsNullOrEmpty(xmlStore.FileExtension) ? ".nspj" : xmlStore.FileExtension);
				if (dlg.ShowDialog(this) == DialogResult.OK) {
					// Set selected project name (fila name), location and file extension
					project.Name = Path.GetFileNameWithoutExtension(dlg.FileName);
					xmlStore.DirectoryName = Path.GetDirectoryName(dlg.FileName);
					xmlStore.FileExtension = Path.GetExtension(dlg.FileName);
				}
			}
			// Save the project
			if (!string.IsNullOrEmpty(project.Name) && !string.IsNullOrEmpty(xmlStore.DirectoryName))
				project.Repository.SaveChanges();
		}


		private void SetButtonStates() {
			if (display.Diagram != null) {
				cutButton.Enabled = display.DiagramSetController.CanCut(display.Diagram, display.SelectedShapes);
				copyButton.Enabled = display.DiagramSetController.CanCopy(display.SelectedShapes);
				pasteButton.Enabled = display.DiagramSetController.CanPaste(display.Diagram);
				deleteButton.Enabled = display.DiagramSetController.CanDeleteShapes(display.Diagram, display.SelectedShapes);
			}
		}

		#endregion


		#region ToolWindow Docking Imlpementation

		private Rectangle ContentBounds {
			get {
				contentBounds.X = Bounds.X + SystemInformation.Border3DSize.Width;
				contentBounds.Y = Bounds.Y
					+ SystemInformation.CaptionHeight
					+ SystemInformation.Border3DSize.Height
					+ SystemInformation.Border3DSize.Height
					+ SystemInformation.FixedFrameBorderSize.Height
					+ SystemInformation.FixedFrameBorderSize.Height;
				if (toolStripContainer.TopToolStripPanelVisible)
					contentBounds.Y += toolStripContainer.TopToolStripPanel.Height;
				contentBounds.Width = display.DrawBounds.Width;
				contentBounds.Height = display.DrawBounds.Height;
				return contentBounds;
			}
		}


		protected override void OnMove(EventArgs e) {
			base.OnMove(e);
			if (toolBoxWindow.Visible)
				DockToolWindow();
			if (propertyWindow.Visible)
				DockPropertyWindow();
		}


		protected override void OnResize(EventArgs e) {
			base.OnResize(e);
			if (toolBoxWindow != null && toolBoxWindow.Visible)
				DockToolWindow();
			if (toolBoxWindow != null && propertyWindow.Visible)
				DockPropertyWindow();
		}


		private void DockToolWindow() {
			Rectangle contentBounds = Rectangle.Empty;
			contentBounds = ContentBounds;

			try {
				toolBoxWindow.SuspendLayout();
				int toolBoxBottom;
				if (propertyWindow.Visible)
					toolBoxBottom = contentBounds.Top + (contentBounds.Height / 2);
				else
					toolBoxBottom = contentBounds.Bottom;

				if ((toolBoxWindowDockType & DockType.Left) > 0)
					toolBoxWindow.Left = contentBounds.Left;

				if ((toolBoxWindowDockType & DockType.Top) > 0)
					toolBoxWindow.Top = contentBounds.Top + margin;

				if ((toolBoxWindowDockType & DockType.Right) > 0)
					toolBoxWindow.Left = contentBounds.Right - toolBoxWindow.Width - margin;

				if ((toolBoxWindowDockType & DockType.Bottom) > 0)
					toolBoxWindow.Height = toolBoxBottom - toolBoxWindow.Top - margin;
			} finally {
				toolBoxWindow.ResumeLayout();
			}
		}


		private void DockPropertyWindow() {
			Rectangle contentBounds = Rectangle.Empty;
			contentBounds = ContentBounds;

			try {
				propertyWindow.SuspendLayout();
				int propertyWindowTop;
				if (toolBoxWindow.Visible)
					propertyWindowTop = contentBounds.Bottom - (contentBounds.Height / 2);
				else
					propertyWindowTop = contentBounds.Top;

				if ((propertyWindowDockType & DockType.Left) > 0)
					propertyWindow.Left = contentBounds.Left;
				if ((propertyWindowDockType & DockType.Top) > 0)
					propertyWindow.Top = propertyWindowTop + 10;
				if ((propertyWindowDockType & DockType.Right) > 0)
					propertyWindow.Left = contentBounds.Right - propertyWindow.Width - margin;
				if ((propertyWindowDockType & DockType.Bottom) > 0)
					propertyWindow.Height = contentBounds.Bottom - propertyWindow.Top - margin;
			} finally {
				propertyWindow.ResumeLayout();
			}
		}


		private void DatabaseDesignerForm_Shown(object sender, EventArgs e) {
			toolBoxWindow.Show(this);
			DockToolWindow();
			Activate();
		}

		#endregion


		#region Event Handler Implementations (UI)

		private void display_ShapesSelected(object sender, EventArgs e) {
			// Update UI buttons
			SetButtonStates();
		}


		private void toolBoxButton_Click(object sender, EventArgs e) {
			if (toolBoxButton.Checked) {
				toolBoxWindow.Hide();
			}
			else {
				toolBoxWindow.Show();
				this.Focus();
			}
			DockPropertyWindow();
			DockToolWindow();
			toolBoxButton.Checked = toolBoxWindow.Visible;
		}


		private void propertyWindowButton_Click(object sender, EventArgs e) {
			if (propertyWindowButton.Checked) {
				propertyWindow.Hide();
			}
			else {
				propertyWindow.Show();
				this.Focus();
			}
			DockToolWindow();
			DockPropertyWindow();
			propertyWindowButton.Checked = propertyWindow.Visible;
		}


		private void designButton_Click(object sender, EventArgs e) {
			toolSetController.ShowDesignEditor();
		}


		private void diagramButton_Click(object sender, EventArgs e) {
			if (!propertyWindow.Visible) propertyWindowButton_Click(this, null);
			propertyController.SetObject(0, display.Diagram);
		}


		private void gridButton_Click(object sender, EventArgs e) {
			display.IsGridVisible = gridButton.Checked;
		}

		
		private void zoomInButton_Click(object sender, EventArgs e) {
			if (display.ZoomLevel < 10000)
				display.ZoomLevel += zoomStep;
		}


		private void zoomOutButton_Click(object sender, EventArgs e) {
			if (display.ZoomLevel > zoomStep)
				display.ZoomLevel -= zoomStep;
		}

		
		private void cutButton_Click(object sender, EventArgs e) {
			display.Cut(false);
		}


		private void copyButton_Click(object sender, EventArgs e) {
			display.Copy(false);
		}


		private void pasteButton_Click(object sender, EventArgs e) {
			display.Paste(display.GridSize, display.GridSize);
		}


		private void deleteButton_Click(object sender, EventArgs e) {
			display.Delete(false);
		}

		
		private void deleteDiagramButton_Click(object sender, EventArgs e) {
			DeleteDiagram(display.Diagram);
		}
		
		
		private void diagramDropDownItem_Click(object sender, EventArgs e) {
			display.UnselectAll();
			display.Diagram = ((ToolStripMenuItem)sender).Tag as Diagram;
			diagramsDropDownButton.Text = display.Diagram.Name;
		}


		private void addDiagramButton_Click(object sender, EventArgs e) {
			AddNewDiagram();
		}


		private void clearButton_Click(object sender, EventArgs e) {
			NewProject();
		}

		
		private void saveButton_Click(object sender, EventArgs e) {
			SaveRepository();
		}


		private void openButton_Click(object sender, EventArgs e) {
			LoadRepository();
		}

		#endregion


		#region NShape component's event handler implementations

		private void display_ZoomChanged(object sender, EventArgs e) {
			zoomLabel.Text = string.Format("{0} %", display.ZoomLevel);
		}


		private void toolSetController_TemplateEditorSelected(object sender, Dataweb.NShape.Controllers.TemplateEditorEventArgs e) {
			TemplateEditorDialog dlg = new TemplateEditorDialog(e.Project, e.Template);
			dlg.Show(this);
		}


		private void toolSetController_DesignEditorSelected(object sender, EventArgs e) {
			DesignEditorDialog dlg = new DesignEditorDialog(project);
			dlg.Show(this);
		}

		#endregion


		#region Fields

		private ToolBoxWindow toolBoxWindow;
		private PropertyWindow propertyWindow;
		private Rectangle contentBounds = Rectangle.Empty;
		private const int margin = 10;
		private const int zoomStep = 5;
		private const string fileFilter = "NShape XML Repository Files|*.nspj;*.xml|All Files|*.*";
		private const string defaultProjectName = "Project 1";

		private DockType toolBoxWindowDockType = 0;
		private DockType propertyWindowDockType = 0;

		#endregion
	}

}
