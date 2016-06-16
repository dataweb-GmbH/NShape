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
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;

using Dataweb.NShape;
using Dataweb.NShape.Advanced;
using Dataweb.NShape.Controllers;
using Dataweb.NShape.WinFormsUI;


namespace NShape_WPF_Demo {

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {


		public MainWindow() {
			InitializeComponent();
			Title = appTitle;

			// Enable XP-Style for WinForms controls
			System.Windows.Forms.Application.EnableVisualStyles();

			// Initialize WinForms ListView
			winFormsListView.View = System.Windows.Forms.View.Details;
			winFormsListView.ShowGroups = true;
			winFormsListView.ShowItemToolTips = true;
			winFormsListView.MultiSelect = false;
			winFormsListView.Width = (int)Math.Round(toolBoxHost.ActualWidth);
			winFormsListView.Height = (int)Math.Round(toolBoxHost.ActualHeight);
			toolBoxHost.Child = winFormsListView;
			// Initialize WinForms PropertyGrid
			propertyGridHost.Child = propertyGrid;

			String appStartPath = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

			// Setup NShape components
			project.AutoLoadLibraries = true;
			project.LibrarySearchPaths.Add(appStartPath);
			project.Repository = repository;
			repository.Store = xmlStore;

			// Connecting NShape components
			diagramSetController.Project = project;
			//
			propertyController.Project = project;
			propertyPresenter.PropertyController = propertyController;
			propertyPresenter.PrimaryPropertyGrid = propertyGrid;
			//
			toolSetPresenter.ListView = winFormsListView;
			toolSetPresenter.ToolSetController = toolSetController;
			toolSetController.DiagramSetController = diagramSetController;
			toolSetController.AddTool(new SelectionTool(), true);

			project.Opened += new EventHandler(project_Opened);
			project.Closed += new EventHandler(project_Closed);

			ActivateMenuItems();
		}


		private void ActivateMenuItems() {
			openProjectMenuItem.IsEnabled = !project.IsOpen;
			saveProjectMenuItem.IsEnabled =
			saveProjectAsMenuItem.IsEnabled =
			closeProjectMenuItem.IsEnabled = project.IsOpen;
		}


		private void CreateDiagramTab(Diagram diagram) {
			// Create tab items and add them to the tab control
			TabItem tabItem = new TabItem();
			tabItem.Header = diagram.Title;
			tabControl.Items.Add(tabItem);

			// Create a new grid to host other controls
			Grid grid = new Grid();
			// Create WindowsFormsHost and add it to grid
			System.Windows.Forms.Integration.WindowsFormsHost displayHost = new System.Windows.Forms.Integration.WindowsFormsHost();
			grid.Children.Add(displayHost);

			// Create display and assign it to the WinFormsHost
			//
			Display display = new Display();
			// Connect with other NSHape components
			display.DiagramSetController = diagramSetController;
			display.PropertyController = propertyController;
			display.Diagram = diagram;
			// Setup display properties
			display.BackColor = System.Drawing.Color.FromKnownColor(System.Drawing.KnownColor.ControlDark);
			display.BackColorGradient = System.Drawing.Color.FromKnownColor(System.Drawing.KnownColor.ControlLight);
			display.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			displayHost.Child = display;

			// Set the grid as the content of a tab item
			tabItem.Content = grid;
		}


		// Returns false, if the save should be retried with another project name.
		private bool SaveProject() {
			bool result = false;
			Mouse.OverrideCursor = Cursors.Wait;
			try {
				// Save changes
				project.Repository.SaveChanges();
				result = true;
			} catch (IOException exc) {
				MessageBox.Show(this, exc.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			} catch (Exception exc) {
				MessageBox.Show(this, exc.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			} finally {
				Mouse.OverrideCursor = null;
			}
			return result;
		}


		// Returns false, if the save should be retried with another project name.
		private bool SaveProjectAs() {
			bool result = false;
			if (((CachedRepository)project.Repository).Store is XmlStore) {
				XmlStore xmlStore = (XmlStore)((CachedRepository)project.Repository).Store;

				// Select a file name
				SaveFileDialog saveFileDialog = new SaveFileDialog();
				saveFileDialog.CreatePrompt = false;		// Do not ask wether to create the file
				saveFileDialog.CheckFileExists = false;		// Do not check wether the file does NOT exist
				saveFileDialog.CheckPathExists = true;		// Ask wether to overwrite existing file
				saveFileDialog.Filter = "NShape XML Repositories|*.nspj;*.xml|All Files|*.*";
				if (Directory.Exists(xmlStore.DirectoryName))
					saveFileDialog.InitialDirectory = xmlStore.DirectoryName;
				else
					saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
				saveFileDialog.FileName = Path.GetFileName(xmlStore.ProjectFilePath);

				// Try to save repository to file...
				if (saveFileDialog.ShowDialog().GetValueOrDefault(false)) {
					// Set the selected project name (file name) and the location (directory)
					xmlStore.DirectoryName = Path.GetDirectoryName(saveFileDialog.FileName);
					project.Name = Path.GetFileNameWithoutExtension(saveFileDialog.FileName);
					Title = string.Format("{0} - {1}", appTitle, project.Name);
					// Delete file if it exists, because the user was prompted wether to overwrite it before (SaveFileDialog.CheckPathExists).
					if (project.Repository.Exists()) project.Repository.Erase();
					saveProjectMenuItem.IsEnabled = true;
					// Save the project (with error handling and wait cursor)
					result = SaveProject();
				}
			} else MessageBox.Show(this, "'Save as...' is not supported for database repositories.", "Not supported", MessageBoxButton.OK, MessageBoxImage.Error);
			return result;
		}


		private bool CloseProject() {
			bool result = true;
			if (project.Repository.IsModified) {
				string msg = string.Format("Do you want to save the current project '{0}' before closing it?", project.Name);
				MessageBoxResult dlgResult = MessageBox.Show(this, msg, "Save changes", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
				switch (dlgResult) {
					case MessageBoxResult.Yes:
						if (project.Repository.Exists())
							SaveProject();
						else SaveProjectAs();
						break;
					case MessageBoxResult.No:
						// do nothing
						break;
					case MessageBoxResult.Cancel:
						result = false;
						break;
				}
			}

			if (result) {
				// Close the existing project
				project.Close();
				// Clear all displays and diagramControllers
				tabControl.Items.Clear();
			}

			return result;
		}


		#region [Private] NShape Component Event Handlers

		private void project_Closed(object sender, EventArgs e) {
			tabControl.Items.Clear();
			ActivateMenuItems();
		}


		private void project_Opened(object sender, EventArgs e) {
			foreach (Diagram diagram in repository.GetDiagrams())
				CreateDiagramTab(diagram);
			if (tabControl.Items.Count > 0)
				tabControl.SelectedIndex = 0;
			ActivateMenuItems();
		}

		#endregion


		#region [Private] Menu Item Event Handler

		private void openProjectMenuItem_Click(object sender, RoutedEventArgs e) {
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "NShape Project Files|*.nspj;*.xml|All Files|*.*";
			openFileDialog.Multiselect = false;
			if (openFileDialog.ShowDialog(this).GetValueOrDefault()) {
				string directory = System.IO.Path.GetDirectoryName(openFileDialog.FileName);
				string fileName = System.IO.Path.GetFileNameWithoutExtension(openFileDialog.FileName);
				string fileExtension = System.IO.Path.GetExtension(openFileDialog.FileName);
				xmlStore.DirectoryName = directory;
				xmlStore.FileExtension = fileExtension;

				project.Name = fileName;
				project.Open();
			}
		}


		private void saveProjectMenuItem_Click(object sender, RoutedEventArgs e) {
			SaveProject();
		}


		private void saveProjectAsMenuItem_Click(object sender, RoutedEventArgs e) {
			SaveProjectAs();
		}


		private void closeProjectMenuItem_Click(object sender, RoutedEventArgs e) {
			//if (project.Repository
			if (project.IsOpen) project.Close();
		}


		private void quitMenuItem_Click(object sender, RoutedEventArgs e) {
			if (project.IsOpen) project.Close();
			Close();
		}


		private void aboutMenuItem_Click(object sender, RoutedEventArgs e) {
			MessageBox.Show(this, string.Format("NShape WPF Demo, based on NShape version {0}", project.ProductVersion), "About...", MessageBoxButton.OK, MessageBoxImage.Information);
		}

		#endregion


		#region Fields

		// NShape components for viewing diagrams
		private Project project = new Project();
		private CachedRepository repository = new CachedRepository();
		private XmlStore xmlStore = new XmlStore();
		private DiagramSetController diagramSetController = new DiagramSetController();
		// NShape components for editing diagrams
		private PropertyController propertyController = new PropertyController();
		private PropertyPresenter propertyPresenter = new PropertyPresenter();
		private ToolSetController toolSetController = new ToolSetController();
		private ToolSetListViewPresenter toolSetPresenter = new ToolSetListViewPresenter();
		// WinForms controls 
		private System.Windows.Forms.PropertyGrid propertyGrid = new System.Windows.Forms.PropertyGrid();
		private System.Windows.Forms.ListView winFormsListView = new System.Windows.Forms.ListView();

		const string appTitle = "NShape WPF Diagram Designer";

		#endregion
	}

}
