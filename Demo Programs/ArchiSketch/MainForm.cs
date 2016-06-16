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
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

using Dataweb.NShape;
using Dataweb.NShape.Commands;
using Dataweb.NShape.Controllers;
using Dataweb.NShape.GeneralShapes;
using Dataweb.NShape.SoftwareArchitectureShapes;
using Dataweb.NShape.WinFormsUI;


namespace ArchiSketch {

	public partial class MainForm : Form {

		public MainForm() {
			InitializeComponent();
		}


		#region User Interface Events

		private void aboutArchiSketchToolStripMenuItem_Click(object sender, EventArgs e) {
			MessageBox.Show("Drawing software architectural sketches.\r\nBy Peter Pohmann - peter.pohmann@dataweb.de", "ArchiSketch 1.0");
		}


		private void deleteToolStripMenuItem_Click(object sender, EventArgs e) {
			display.DiagramSetController.DeleteShapes(display.Diagram, display.SelectedShapes, true);
		}


		private void stylesToolStripMenuItem_Click(object sender, EventArgs e) {
			DesignEditorDialog de = new DesignEditorDialog(project);
			de.ShowDialog(this);
		}


		private void shapeTemplateToolStripMenuItem_Click(object sender, EventArgs e) {
			EditToolBoxTemplate();
		}


		private void addTemplateToolStripMenuItem_Click(object sender, EventArgs e) {
			toolSetController.ShowTemplateEditor(false);
		}


		private void editTemplateToolStripMenuItem_Click(object sender, EventArgs e) {
			EditToolBoxTemplate();
		}


		private void deleteTemplateToolStripMenuItem_Click(object sender, EventArgs e) {
			if (toolSetController.SelectedTool is TemplateTool) {
				toolSetController.DeleteSelectedTemplate();
			}
		}


		private void propertiesToolStripMenuItem_Click(object sender, EventArgs e) {
			ShapePropertiesDialog d = new ShapePropertiesDialog();
			d.propertyController.Project = project;
			if (display.SelectedShapes.Count == 0)
				d.propertyController.SetObject(0, display.Diagram);
			else d.propertyController.SetObjects(0, display.SelectedShapes);
			d.Show(this);
		}


		private void toolBoxStrip_Click(object sender, EventArgs args) {
			ToolStripButton button = (ToolStripButton)sender;
			if (button.Checked)
				toolSetController.SelectTool((Tool)button.Tag, false);
			else 
				toolSetController.SelectTool(toolSetController.DefaultTool);
		}


		private void toolBoxStrip_DoubleClick(object sender, EventArgs args) {
			ToolStripButton button = (ToolStripButton)sender;
			if (button.Checked)
				toolSetController.SelectTool((Tool)button.Tag, true);
			else {
				UncheckAllOtherButtons(button);
				toolSetController.SelectTool(toolSetController.DefaultTool);
			}
		}


		private void MainForm_Load(object sender, EventArgs e) {
			// Turn off automatic loading of shape libraries. We want all diagrams to use the same shape libraries.
			project.AutoLoadLibraries = true;
			// Add shape library "GeneralShapes"
			project.AddLibrary(Assembly.GetAssembly(typeof(Circle)), false);
			// Add shape library "SoftwareArchitectureShapes"
			project.AddLibrary(Assembly.GetAssembly(typeof(CloudSymbol)), false);
			
			// Create a new project on start
			fileNewToolStripMenuItem_Click(this, EventArgs.Empty);
		}


		private void copyToolStripMenuItem_Click(object sender, EventArgs e) {
			display.CopyImageToClipboard(ImageFileFormat.EmfPlus, true);
		}


		private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
			Close();
		}


		private void saveToolStripMenuItem_Click(object sender, EventArgs e) {
			SaveCurrentProject();
		}


		private void diagramComboBox_KeyDown(object sender, KeyEventArgs e) {
			if (e.KeyCode == Keys.Enter) RenameCurrentDiagram(diagramComboBox.Text);
		}


		private void diagramComboBox_SelectedIndexChanged(object sender, EventArgs e) {
			if (diagramComboBox.SelectedIndex == diagramComboBox.Items.Count - 1)
				// Create and display a new Diagram
				DisplayNewDiagram();
			else
				// Display the selected diagram
				DisplayDiagram(diagramComboBox.Items[diagramComboBox.SelectedIndex].ToString());
		}


		private void diagramInsertToolStripMenuItem_Click(object sender, EventArgs e) {
			DisplayNewDiagram();
		}


		private void diagramDeleteToolStripMenuItem_Click(object sender, EventArgs e) {
			if (MessageBox.Show(string.Format("Delete diagram '{0}'?", display.Diagram.Name),
				"Delete Diagram", MessageBoxButtons.OKCancel) == DialogResult.OK) {
				project.Repository.DeleteAll(display.Diagram);
				UpdateDiagramCombo();
				DisplayDefaultDiagram();
			}
		}


		private void toolSetController_ToolChanged(object sender, ToolEventArgs e) {
			ToolStripButton button = FindToolStripButton(e.Tool);
			button.Image = toolSetController.SelectedTool.SmallIcon;
		}


		private void toolSetController_ToolAdded(object sender, ToolEventArgs e) {
			UpdateToolBoxStrip();
		}


		private void toolSetController_ToolRemoved(object sender, ToolEventArgs e) {
			UpdateToolBoxStrip();
		}


		private void toolSetController_Changed(object sender, EventArgs e) {
			UpdateToolBoxStrip();
		}


		private void toolSetController_ToolSelected(object sender, ToolEventArgs e) {
			ToolStripButton button = FindToolStripButton(e.Tool);
			UncheckAllOtherButtons(button);
			if (button != null) button.Checked = true;
		}

		#endregion


		#region User Interface Events

		private void fileNewToolStripMenuItem_Click(object sender, EventArgs e) {
			project.Close();
			project.Name = newProjectName;
			OpenProject(true);
			DisplayNewDiagram();
			Text = GetFormTitle();
		}


		private void fileOpenToolStripMenuItem_Click(object sender, EventArgs e) {
			if (openFileDialog.ShowDialog(this) == DialogResult.OK) {
				// Check whether the repository is modified and ask the user whether to save it
				if (project.Repository.IsModified) {
					switch (MessageBox.Show("Do you want to save the current project?", "Close Project", MessageBoxButtons.YesNoCancel)) {
						case DialogResult.Yes:
							SaveCurrentProject();
							break;
						case DialogResult.Cancel:
							return;
						case DialogResult.No:
							break;
					}
				}
				// Close the current project
				project.Close();
				// Set the project name (file name)
				project.Name = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
				// Set the project location
				xmlStore.DirectoryName = Path.GetDirectoryName(openFileDialog.FileName);
				// Set the file extension
				xmlStore.FileExtension = Path.GetExtension(openFileDialog.FileName);
				
				// Now open the project
				OpenProject(false);
				Text = GetFormTitle();
			}
		}


		private void fileSaveAsToolStripMenuItem_Click(object sender, EventArgs e) {
			saveFileDialog.DefaultExt = "*.askp";
			saveFileDialog.Filter = "ArchiSketch project (*.askp)|*.askp|ArchiSketch template (*.askt)|*.askt|Windows meta file (*.emf)|*.emf";
			if (saveFileDialog.ShowDialog() == DialogResult.OK) {
				switch (Path.GetExtension(saveFileDialog.FileName)) {
					case ".askp":
						project.Name = Path.GetFileNameWithoutExtension(saveFileDialog.FileName);
						xmlStore.DirectoryName = Path.GetDirectoryName(saveFileDialog.FileName);
						xmlStore.FileExtension = Path.GetExtension(saveFileDialog.FileName);
						project.Repository.SaveChanges();
						Text = GetFormTitle();
						break;
					case ".askt":
						string fileName = xmlStore.DirectoryName;
						xmlStore.DirectoryName = Path.GetDirectoryName(saveFileDialog.FileName);
						project.Repository.SaveChanges();
						xmlStore.DirectoryName = fileName;
						break;
					case ".emf":
						Image image = display.Diagram.CreateImage(ImageFileFormat.Emf, null);
						image.Save(saveFileDialog.FileName);
						break;
					default:
						MessageBox.Show("Unknown file extension {0}.", "Cannot Save", MessageBoxButtons.OK, MessageBoxIcon.Warning);
						break;
				}
			}
		}

		#endregion


		#region Implementations

		private ToolStripButton FindToolStripButton(Tool tool) {
			ToolStripButton result = null;
			foreach (ToolStripItem tsi in toolBoxStrip.Items)
				if (tsi is ToolStripButton) {
					ToolStripButton tsb = (ToolStripButton)tsi;
					if (tsb.Tag == toolSetController.SelectedTool) {
						result = tsb;
						break;
					}
				}
			return result;
		}


		private void OpenProject(bool newProject) {
			try {
				if (newProject)
					project.Create();
				else 
					project.Open();
				UpdateToolBoxStrip();
				UpdateDiagramCombo();
				
				// Display first diagram (if a diagram exist)
				DisplayDefaultDiagram();
			} catch (Exception exc) {
				MessageBox.Show(exc.Message, "Cannot open project");
			}
		}


		private void UncheckAllOtherButtons(ToolStripButton button) {
			foreach (ToolStripItem tsi in toolBoxStrip.Items)
				if (tsi != button && tsi is ToolStripButton)
					((ToolStripButton)tsi).Checked = false;
		}


		private void SaveCurrentProject() {
			bool doIt = false;
			if (project.Name.Equals(newProjectName, StringComparison.InvariantCultureIgnoreCase)) {
				if (saveFileDialog.ShowDialog(this) == DialogResult.OK) {
					// Update the project name (file name) 
					project.Name = Path.GetFileNameWithoutExtension(saveFileDialog.FileName);
					// Update the project location 
					xmlStore.DirectoryName = Path.GetDirectoryName(saveFileDialog.FileName);
					// Update the file extension
					xmlStore.FileExtension = Path.GetExtension(saveFileDialog.FileName);
					
					// If a project with this name exists, delete it
					if (repository.Exists()) repository.Erase();
					doIt = true;
				}
			} else doIt = true;
			// Save the project
			if (doIt) project.Repository.SaveChanges();
		}


		private string GetFormTitle() {
			return project.Name + " - ArchiSketch 1.0";
		}


		private void RenameCurrentDiagram(string newName) {
			display.Diagram.Name = newName;
			project.Repository.Update(display.Diagram);
			UpdateDiagramCombo();
		}


		private void UpdateToolBoxStrip() {
			toolBoxStrip.SuspendLayout();
			try {
				// delete buttons from left to first separator
				while (toolBoxStrip.Items.Count > 0)
					toolBoxStrip.Items.RemoveAt(0);
				//
				// Create buttons for the tools
				int index = 0;
				foreach (Tool t in toolSetController.Tools) {
					ToolStripButton b = new ToolStripButton(null, t.SmallIcon);
					b.Tag = t;
					b.CheckOnClick = true;
					b.Click += toolBoxStrip_Click;
					b.DoubleClick += toolBoxStrip_DoubleClick;
					b.ToolTipText = t.ToolTipText;
					b.DoubleClickEnabled = true;
					toolBoxStrip.Items.Insert(index, b);
					++index;
				}
			} finally {
				toolBoxStrip.ResumeLayout();
			}
		}
		
		
		private void UpdateDiagramCombo() {
			diagramComboBox.Text = string.Empty;
			diagramComboBox.Items.Clear();
			foreach (Diagram d in project.Repository.GetDiagrams())
			   diagramComboBox.Items.Add(d.Name);
			diagramComboBox.Items.Add("<New diagram...>");
		}


		private void DisplayNewDiagram() {
			// Find a unique name for the new Diagram
			int N = 0;
			string newName;
			bool found;
			do {
				++N;
				newName = string.Format("Diagram {0}", N);
				// Search name in list of diagrams
				found = false;
				foreach (Diagram d in project.Repository.GetDiagrams())
					if (d.Name.Equals(newName, StringComparison.InvariantCultureIgnoreCase)) {
						found = true;
						break;
					}
			} while (found);
			
			Diagram diagram = new Diagram(newName);
			CreateDiagramCommand cmd = new CreateDiagramCommand(diagram);
			project.ExecuteCommand(cmd);

			UpdateDiagramCombo();
			diagramComboBox.Text = newName;
		}


		private void DisplayDiagram(string diagramName) {
			Diagram diagram = GetDiagram(diagramName);
			if (diagram != null) {
				display.Diagram = diagram;
				// Refresh Diagram combo
				int index = 0;
				foreach (Diagram d in project.Repository.GetDiagrams()) {
					if (d.Name == display.Diagram.Name) {
						diagramComboBox.SelectedIndex = index;
						break;
					}
					++index;
				}
			}
		}


		private Diagram GetDiagram(string diagramName) {
			Diagram result = null;
			// GetDiagram(diagramName) throws an exception if the name does not exist in the repository,
			// so we iterate through all existing diagrams
			foreach (Diagram d in project.Repository.GetDiagrams())
			   if (d.Name == diagramName) {
			      result = d;
			      break;
			   }
			return result;
		}


		private void DisplayDefaultDiagram() {
			IEnumerator<Diagram> enumerator = project.Repository.GetDiagrams().GetEnumerator();
			if (enumerator.MoveNext()) DisplayDiagram(enumerator.Current.Name);
			else DisplayDiagram(null);
		}


		private void EditToolBoxTemplate() {
			if (toolSetController.SelectedTool is TemplateTool)
				toolSetController.ShowTemplateEditor(true);
			else MessageBox.Show("Select a template insertion tool and repeat the command.", "Editing a Template");
		}

		#endregion


		#region NShape ToolBox event handlers for showing dialogs

		private void toolBox_ShowTemplateEditorDialog(object sender, TemplateEditorEventArgs e) {
			TemplateEditorDialog dlg = new TemplateEditorDialog(e.Project, e.Template);
			dlg.Show();
		}


		private void toolBox_ShowLibraryManagerDialog(object sender, EventArgs e) {
			LibraryManagementDialog dlg = new LibraryManagementDialog(project);
		}


		private void toolBox_ShowDesignEditorDialog(object sender, EventArgs e) {
			DesignEditorDialog dlg = new DesignEditorDialog(project);
		}

		#endregion


		private const string newProjectName = "<New Project>";
	}
}