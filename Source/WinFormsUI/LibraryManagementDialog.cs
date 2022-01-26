/******************************************************************************
  Copyright 2009-2022 dataweb GmbH
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
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;


namespace Dataweb.NShape.WinFormsUI {
	
	/// <summary>
	/// Dialog used for loading NShape libraries into the current NShape project.
	/// </summary>
	[ToolboxItem(false)]
	public partial class LibraryManagementDialog : Form {

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.WinFormsUI.LibraryManagementDialog" />.
		/// </summary>
		/// <param name="project"></param>
		public LibraryManagementDialog(Project project) {
			InitializeComponent();
			if (project == null) throw new ArgumentNullException("project");
			this._project = project;
			Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath);
		}


		private void RefreshList() {
			// Collect currently used libraries
			_currentLibraries.Clear();
			foreach (Assembly assembly in _project.Libraries) {
				string assemblyPath = GetAssemblyFilePath(assembly);
				_currentLibraries.Add(assemblyPath, assembly);
				// Remove library from the "add" list
				if (_addedLibraries.ContainsKey(assemblyPath))
					_addedLibraries.Remove(assemblyPath);
			}
			// (Re-)Fill list view
			try {
				libraryListView.SuspendLayout();
				libraryListView.Items.Clear();
				foreach (KeyValuePair<string, Assembly> item in _currentLibraries) {
					// Skip assemblies in the "remove" list
					if (_removedLibraries.ContainsKey(item.Key)) continue;
					// Create and add list view item
					ListViewItem lvItem = CreateListViewItem(item.Key, item.Value);
					libraryListView.Items.Add(lvItem);
				}
				foreach (KeyValuePair<string, Assembly> item in _addedLibraries) {
					Debug.Assert(!_removedLibraries.ContainsKey(item.Key));
					// Create and add list view item
					ListViewItem lvItem = CreateListViewItem(item.Key, item.Value);
					libraryListView.Items.Add(lvItem);
				}
			} finally {
				foreach (ColumnHeader colHeader in libraryListView.Columns)
					colHeader.Width = -1;
				libraryListView.ResumeLayout();
			}
		}


		private ListViewItem CreateListViewItem(string assemblyPath, Assembly assembly) {
			if (string.IsNullOrEmpty(assemblyPath)) throw new ArgumentNullException("assemblyPath");
			if (assembly == null) throw new ArgumentNullException("assembly");
			// Get assembly path, name and version
			AssemblyName assemblyName = assembly.GetName();
			ListViewItem item = new ListViewItem(assemblyName.Name);
			item.SubItems.Add(assemblyName.Version.ToString());
			item.SubItems.Add(assemblyPath);
			return item;
		}


		private string GetAssemblyFilePath(Assembly assembly) {
			// Get assembly file path
			UriBuilder uriBuilder = new UriBuilder(assembly.CodeBase);
			return Uri.UnescapeDataString(uriBuilder.Path);
		}

		
		private void LibraryManagementDialog_Load(object sender, EventArgs e) {
			_project.LibraryLoaded += Project_LibraryLoaded;
			RefreshList();
		}


		private void LibraryManagementDialog_Shown(object sender, EventArgs e) {
			bool librariesLoaded = false;
			foreach (Assembly a in _project.Libraries) {
				librariesLoaded = true; 
				break;
			}
			if (!librariesLoaded) addLibraryButton_Click(this, null);
		}

	
		private void LibraryManagementDialog_FormClosed(object sender, FormClosedEventArgs e) {
			_project.LibraryLoaded -= Project_LibraryLoaded;
		}


		private void addLibraryButton_Click(object sender, EventArgs e) {
			openFileDialog.Filter = "Assembly Files|*.dll|All Files|*.*";
			openFileDialog.FileName = string.Empty;
			openFileDialog.Multiselect = true;
			if (string.IsNullOrEmpty(openFileDialog.InitialDirectory))
				openFileDialog.InitialDirectory = Application.StartupPath;
			
			if (openFileDialog.ShowDialog() == DialogResult.OK) {
				foreach (string fileName in openFileDialog.FileNames) {
					try {
						if (!_project.IsValidLibrary(fileName)) {
							MessageBox.Show(this, string.Format(InvalidLibraryMessage, fileName), "Invalid file type", MessageBoxButtons.OK, MessageBoxIcon.Error);
						} else {
							Assembly assembly = Assembly.LoadFile(fileName);
							string assemblyPath = GetAssemblyFilePath(assembly);
							// Remove library from the "remove" list
							if (_removedLibraries.ContainsKey(assemblyPath))
								_removedLibraries.Remove(assemblyPath);
							// If not currently in use, add library to the "Add" list 
							if (!_currentLibraries.ContainsKey(assemblyPath)
								&& !_addedLibraries.ContainsKey(assemblyPath))
								_addedLibraries.Add(assemblyPath, assembly);
						}
					} catch (Exception ex) {
						RefreshList();
						MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}
				RefreshList();
			}
		}


		private void removeLibraryButton_Click(object sender, EventArgs e) {
			bool removeConfirmed = false, removeLibrary = false;
			foreach (ListViewItem item in libraryListView.SelectedItems) {
				string assemblyPath = item.SubItems[2].Text;
				// If assembly is on the "add" list, remove it from there
				if (_addedLibraries.ContainsKey(assemblyPath))
					_addedLibraries.Remove(assemblyPath);
				// If assembly is currently in use, add it to the "remove" list
				else if (_currentLibraries.ContainsKey(assemblyPath)) {
					if (!removeConfirmed) {
						string msg = "When removing a library, the undo history will be cleared. You will not be able to undo changes made until now.";
						msg += Environment.NewLine + "Do you really want to remove loaded libraries?";
						DialogResult res = MessageBox.Show(this, msg, string.Empty, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
						removeLibrary = (res == DialogResult.Yes);
						removeConfirmed = true;
					}
					if (removeLibrary) _removedLibraries.Add(assemblyPath, _currentLibraries[assemblyPath]);
				}
			}
			RefreshList();
		}

	
		private void okButton_Click(object sender, EventArgs e) {
			_project.LibraryLoaded -= Project_LibraryLoaded;

			// Repaint windows under the file dialog before starting with adding libraries
			if (Owner != null) Owner.Refresh();
			Application.DoEvents();

			// Remove obsolete Libraries
			foreach (KeyValuePair<string, Assembly> item in _removedLibraries) {
				try {
					_project.RemoveLibrary(item.Value);
				} catch (Exception exc) {
					MessageBox.Show(this, exc.Message, "Cannot remove Library", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}
			}
			// Add new libraries
			foreach (KeyValuePair<string, Assembly> item in _addedLibraries) {
				try {
					_project.AddLibraryByFilePath(item.Key, true);
				} catch (Exception exc) {
					MessageBox.Show(this, exc.Message, "Cannot add Library", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}
			}
			DialogResult = DialogResult.OK;
			if (!Modal) Close();
		}


		private void cancelButton_Click(object sender, EventArgs e) {
			DialogResult = DialogResult.Cancel;
			if (!Modal) Close();
		}


		private void Project_LibraryLoaded(object sender, LibraryLoadedEventArgs e) {
			RefreshList();
		}


		private void openFileDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e) {
			string reason;
			e.Cancel = !_project.IsValidLibrary(openFileDialog.FileName, out reason);
			string msg = string.Format(InvalidLibraryMessage, openFileDialog.FileName, reason);
			if (e.Cancel) 
				MessageBox.Show(this, msg, "Not an NShape library", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}


		private Project _project;
		private SortedList<string, Assembly> _addedLibraries = new SortedList<string, Assembly>();
		private SortedList<string, Assembly> _currentLibraries = new SortedList<string,Assembly>();
		private SortedList<string, Assembly> _removedLibraries = new SortedList<string, Assembly>();

		private const string InvalidLibraryMessage = "'{0}' is not a valid NShape library: {1}";
	}
}