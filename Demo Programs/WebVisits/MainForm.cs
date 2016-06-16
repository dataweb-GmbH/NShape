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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Dataweb.Xml;

using Dataweb.NShape;
using Dataweb.NShape.Advanced;
using Dataweb.NShape.GeneralShapes;
using Dataweb.NShape.Layouters;


namespace WebVisists {

	public partial class MainForm : Form {

		public MainForm() {
			InitializeComponent();
		}


		private void MainForm_Load(object sender, EventArgs e) {
			// Set User Acceess Rights
			RoleBasedSecurityManager sec = new RoleBasedSecurityManager();
			sec.CurrentRole = StandardRole.SuperUser;
			project.SecurityManager = sec;

			xmlStore.DirectoryName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"WebVisits");

			project.Name = "Webvisits Project";
			project.Create();
			// Add persistent libraries
			project.AddLibraryByName("Dataweb.nShape.GeneralShapes", false);
			project.AddLibraryByName("Dataweb.nShape.SoftwareArchitectureShapes", false);

			CreateLineStyles();

			// Delete default tools
			toolBoxAdapter.ToolSetController.Clear();
			toolBoxAdapter.ListView.ShowGroups = false;

			FillToolBox();
		}


		private void MainForm_Shown(object sender, EventArgs e) {
			if (MessageBox.Show(this, "Do you want to load a web statistics file now?", "Load web statistics file",
				MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
				loadWebStatisticsToolStripMenuItem_Click(this, null);
		}


		/// <summary>
		/// Load a statistics file and create shapes for the visualization:
		/// Shapes representing the web pages will be connected to each other with lines.
		/// </summary>
		private void loadWebStatisticsToolStripMenuItem_Click(object sender, EventArgs e) {
			string statsDir = Path.Combine("Demo Programs", Path.Combine("WebVisits", "Sample Web Statistics"));
			openFileDialog.Filter = "Web Statistics|*.xml|All files|*.*";
			openFileDialog.InitialDirectory = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(Application.StartupPath)), statsDir);
			openFileDialog.FileName = string.Empty;
			if (openFileDialog.ShowDialog() == DialogResult.OK) {
				// Create a new diagram
				ShapeType boxType = project.ShapeTypes["Ellipse"];
				ShapeType multiLineType = project.ShapeTypes["Polyline"];

				Dictionary<int, RectangleBase> boxes = new Dictionary<int, RectangleBase>();
				List<Polyline> lines = new List<Polyline>();
				//
				// Create shapes for the web pages and connect them with lines
				XmlScanner scanner = new XmlScanner(openFileDialog.FileName);
				scanner.ReadElement();
				scanner.ReadElement("WebVisits");
				scanner.ReadChild("Pages");
				if (scanner.ReadChild("Page"))
					do {
						scanner.ReadAttribute(); // id attribute
						// Create a shape representing this web page
						RectangleBase box = (RectangleBase)boxType.CreateInstance(pageTemplate);
						box.Width = 140;
						boxes.Add(scanner.IntValue, box);
						// Set the page name as the shape's text
						scanner.ReadAttribute();
						box.Text = scanner.StringValue;
					} while (scanner.ReadElement());
				scanner.ReadParent();
				if (scanner.ReadChild("Referral"))
					do {
						scanner.ReadAttribute(); // id1
						int id1 = scanner.IntValue;
						Shape shape1 = boxes[id1];
						scanner.ReadAttribute(); // id2
						int id2 = scanner.IntValue;
						Shape shape2 = boxes[id2];
						scanner.ReadAttribute(); // count
						int count = scanner.IntValue;
						
						// Create a line representing the referral info
						Polyline line = (Polyline)multiLineType.CreateInstance();
						line.EndCapStyle = project.Design.CapStyles.ClosedArrow;	// Set arrow tip
						line.LineStyle = GetLineStyle(count);						// Find a line style that visually represents the number of clicks
						// Connect the first line vertex to shape1
						line.Connect(ControlPointId.FirstVertex, shape1, ControlPointId.Reference);
						// Connect the last line vertex to shape2
						line.Connect(ControlPointId.LastVertex, shape2, ControlPointId.Reference);
						lines.Add(line);
					} while (scanner.ReadElement());
				scanner.ReadParent();
				scanner.Close();
				//
				// Insert all shapes into the diagram
				int cnt = 0;
				foreach (Diagram d in project.Repository.GetDiagrams())
					++cnt;
				// Create a new diagram for the read statistics
				Diagram diagram = new Diagram(string.Format("WebVisits Diagram {0}", cnt));
				diagram.Width = 1000;
				diagram.Height = 1000;
				diagram.BackgroundImageLayout = Dataweb.NShape.ImageLayoutMode.Fit;
				// Insert all web page shapes into the diagram
				foreach (RectangleBase b in boxes.Values)
					diagram.Shapes.Add(b, project.Repository.ObtainNewTopZOrder(diagram));
				// Insert all connector shapes (lines) into the diagram
				foreach (Polyline l in lines)
					diagram.Shapes.Add(l, project.Repository.ObtainNewBottomZOrder(diagram));
				// Empty the local shape buffers
				boxes.Clear();
				lines.Clear();
				//
				// Insert the diagram (including all shapes) into the repository (otherwise they won't be saved)
				project.Repository.InsertAll(diagram);
				//
				// Create a layouter in order to layout the shapes
				if (layouter == null)
					layouter = new RepulsionLayouter(project);
				// Adjust layouter parameters
				layouter.SpringRate = 14;
				layouter.Repulsion = 7;
				layouter.RepulsionRange = 400;
				layouter.Friction = 0;
				layouter.Mass = 50;
				// Set the shapes that sould be layouted
				layouter.AllShapes = diagram.Shapes;
				layouter.Shapes = diagram.Shapes;

				// Perform the layout
				layouter.Prepare();
				layouter.Execute(10);
				layouter.Fit(50, 50, diagram.Width - 100, diagram.Height - 100);

				// Display the result
				display.Diagram = diagram;
			}
		}


		private void loadDiagramToolStripMenuItem_Click(object sender, EventArgs e) {
			using (OpenFileDialog openFileDlg = new OpenFileDialog()) {
				if (openFileDlg.ShowDialog(this) == DialogResult.OK) {
					// Close the current project
					project.Close();
					// Set project name (file name), directory and file extension
					xmlStore.DirectoryName = Path.GetDirectoryName(openFileDlg.FileName);
					xmlStore.FileExtension = Path.GetExtension(openFileDlg.FileName);
					project.Name = Path.GetFileNameWithoutExtension(openFileDlg.FileName);
					// Open (load) the diagram
					project.Open();
					// Display the first diagram
					foreach (Diagram d in project.Repository.GetDiagrams()) {
						display.Diagram = d;
						break;
					}
					// Fill the toolbox
					FillToolBox();
				}
			}
		}


		private void FillToolBox() {
			pageTemplate = null;
			Template labelTemplate = null;
			Template annotationTemplate = null;

			// Find existing templates for web-page, annotation and label
			foreach (Template template in project.Repository.GetTemplates()) {
				if (string.Compare(template.Name, templateNameAnnotation, StringComparison.InvariantCultureIgnoreCase) == 0) {
					annotationTemplate = template;
				} else if (string.Compare(template.Name, templateNameLabel, StringComparison.InvariantCultureIgnoreCase) == 0)
					labelTemplate = template;
				else if (string.Compare(template.Name, templateNameWebPage, StringComparison.InvariantCultureIgnoreCase) == 0)
					pageTemplate = template;
			}

			// Add desired tools:
			// We always want a selection tool (pointer tool)
			toolBoxAdapter.ToolSetController.AddTool(new SelectionTool(), true);
			// Add tools to the tool box for creating the shapes...
			if (pageTemplate == null) {
				// If there is no template for the shape representing the web page, create it.
				pageTemplate = new Template(templateNameWebPage, project.ShapeTypes["Ellipse"].CreateInstance());
				((RectangleBase)pageTemplate.Shape).FillStyle = project.Design.FillStyles["Green"];
				// Insert the template into the repository (otherwise it won't be saved)
				// Please note: Inserting a template will automatically create a tool in the toolbox
				project.Repository.InsertAll(pageTemplate);
			} else {
				// Add the existing template to the toolbox
				toolBoxAdapter.ToolSetController.CreateTemplateTool(pageTemplate);
			}
			//
			if (labelTemplate == null) {
				// If there is no template for the label shape, create it.
				labelTemplate = new Template(templateNameLabel, project.ShapeTypes["Label"].CreateInstance());
				((RectangleBase)labelTemplate.Shape).CharacterStyle = project.Design.CharacterStyles.Normal;
				// Insert the template into the repository (otherwise it won't be saved)
				// Please note: Inserting a template will automatically create a tool in the toolbox
				project.Repository.InsertAll(labelTemplate);
			} else {
				// Add the existing template to the toolbox
				toolBoxAdapter.ToolSetController.CreateTemplateTool(labelTemplate);
			}
			//
			if (annotationTemplate == null) {
				// If there is no template for the annotation shape, create it.
				annotationTemplate = new Template(templateNameAnnotation, project.ShapeTypes["Annotation"].CreateInstance());
				((RectangleBase)annotationTemplate.Shape).FillStyle = project.Design.FillStyles.Yellow;
				// Insert the template into the repository (otherwise it won't be saved)
				// Please note: Inserting a template will automatically create a tool in the toolbox
				project.Repository.InsertAll(annotationTemplate);
			} else {
				// Add the existing template to the toolbox
				toolBoxAdapter.ToolSetController.CreateTemplateTool(annotationTemplate);
			}
			
			// Activate the default tool (pointer tool by default).
			toolBoxAdapter.ToolSetController.SelectedTool = toolBoxAdapter.ToolSetController.DefaultTool;
		}


		private void saveDiagramToolStripMenuItem_Click(object sender, EventArgs e) {
			using (SaveFileDialog dlg = new SaveFileDialog()) {
				if (Directory.Exists(xmlStore.DirectoryName))
					dlg.InitialDirectory = xmlStore.DirectoryName;
				else dlg.FileName = Path.GetFileName(xmlStore.ProjectFilePath);

				if (dlg.ShowDialog() == DialogResult.OK) {
					// Update project name (file name) and directory
					xmlStore.DirectoryName = Path.GetDirectoryName(dlg.FileName);
					project.Name = Path.GetFileNameWithoutExtension(dlg.FileName);
					project.Repository.SaveChanges();
				}
			}
		}


		private void showLayoutWindowToolStripMenuItem_Click(object sender, EventArgs e) {
			// Show layout dialog
			Dataweb.NShape.WinFormsUI.LayoutDialog layoutWindow;
			if (layouter != null)
				layoutWindow = new Dataweb.NShape.WinFormsUI.LayoutDialog(layouter);
			else layoutWindow = new Dataweb.NShape.WinFormsUI.LayoutDialog();
			layoutWindow.Project = project;
			layoutWindow.Diagram = display.Diagram;
			layoutWindow.Show(this);
		}
		
		
		private void selectAllToolStripMenuItem_Click(object sender, EventArgs e) {
			display.SelectAll();
		}


		private void CreateLineStyles() {
			// Create custom line styles
			for (int j = 1; j <= 5; ++j) {
				for (int i = 1; i <= 10; ++i) {
					string name = string.Empty;
					IColorStyle colorStyle = null;
					DashType dashType = DashType.Solid;
					switch (j) {
						case 1:
							name = styleNameVeryPoor;
							colorStyle = project.Design.ColorStyles.Gray;
							dashType = DashType.Dot;
							break;
						case 2:
							name = styleNamePoor;
							colorStyle = project.Design.ColorStyles.Gray;
							dashType = DashType.Dash;
							break;
						case 3:
							name = styleNameNormal;
							colorStyle = project.Design.ColorStyles.Black;
							dashType = DashType.Solid;
							break;
						case 4:
							name = styleNameGood;
							colorStyle = project.Design.ColorStyles.Black;
							dashType = DashType.Solid;
							break;
						case 5:
							name = styleNameExcellent;
							colorStyle = project.Design.ColorStyles.Black;
							dashType = DashType.Solid;
							break;
						default: Debug.Fail(""); break;
					}
					LineStyle lineStyle = new LineStyle(string.Format("{0} {1}0%", name, i));
					lineStyle.LineWidth = Math.Max(1, j / 2);
					lineStyle.ColorStyle = colorStyle;
					lineStyle.DashType = dashType;
					
					// Add the new line style to the current design (make it usable in shapes)
					project.Design.AddStyle(lineStyle);
					// Add the new line style to the repository (otherwise it won't be saved)
					project.Repository.Insert(project.Design, lineStyle);
				}
			}
		}


		/// <summary>
		/// Find a line style that visually represents the given number (high number -> bold line)
		/// </summary>
		private ILineStyle GetLineStyle(int count) {
			int factor = 1 + (int)Math.Round(count / 100f);
			string name;
			switch (factor){
				case 1: name = styleNameVeryPoor; break;
				case 2: name = styleNamePoor; break;
				case 3: name = styleNameNormal; break;
				case 4: name = styleNameGood; break;
				default: name = styleNameExcellent; break;
			}
			if (factor < 10) {
				int rating = Math.Max(1, (int)Math.Round(count / (factor * 10f))) * 10;
				string styleName = string.Format("{0} {1}%", name, rating);
				return project.Design.LineStyles[styleName];
			} else return project.Design.LineStyles[styleNameExcellent + " 100%"];
		}


		private const string templateNameWebPage = "Web Page";
		private const string templateNameLabel = "Label";
		private const string templateNameAnnotation = "Annotation";

		private const string styleNameVeryPoor = "Very poor";
		private const string styleNamePoor = "Poor";
		private const string styleNameNormal = "Normal";
		private const string styleNameGood = "Good";
		private const string styleNameExcellent = "Excellent";

		private Template pageTemplate;
		private RepulsionLayouter layouter = null;
	}
}