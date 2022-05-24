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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Xml;

using Dataweb.NShape.Advanced;
using Dataweb.NShape.Commands;
using Dataweb.NShape.Controllers;
using Dataweb.NShape.WinFormsUI;


namespace Dataweb.NShape.Designer {

	public partial class DiagramDesignerMainForm : Form {

		[DllImport("Shell32.dll")]
		public extern static int SHGetSpecialFolderPath(IntPtr hwndOwner, StringBuilder lpszPath, int nFolder, int fCreate);


		public DiagramDesignerMainForm() {
			InitializeComponent();
			Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
			runtimeModeComboBox.SelectedIndex = 0;
			Visible = false;

			// Set texts for status bar tool tips
			SetToolTipTexts();

#if !DEBUG_UI
			historyTrackBar.Visible = false;
			toolStripContainer.TopToolStripPanel.Controls.Remove(debugToolStrip);
#endif
			// Find out the latest repository version
			_maxRepositoryVersion = 0;
			try {
				using (Project p = new Project()) {
					p.Name = "Test";
					p.Create();
					_maxRepositoryVersion = p.Repository.Version;
					p.Close();
				}
			} catch (Exception) { }
		}


		#region [Public] Properties

		public bool MouseWheelZoom {
			get { return _mouseWheelZoom; }
			set {
				_mouseWheelZoom = value;
				if (CurrentDisplay != null) CurrentDisplay.ZoomWithMouseWheel = value;
			}
		}


		public bool UseUniversalScroll {
			get { return _universalScroll; }
			set {
				_universalScroll = value;
				if (CurrentDisplay != null) CurrentDisplay.IsUniversalScrollEnabled = value;
			}
		}


		public MouseButtons PanButtons {
			get { return _panButtons; }
			set {
				_panButtons = value;
				if (CurrentDisplay != null) CurrentDisplay.PanMouseButton = value;
			}
		}


		public bool ShowScrollBars {
			get { return _showScrollBars; }
			set {
				_showScrollBars = value;
				if (CurrentDisplay != null) CurrentDisplay.ScrollBarsVisible = value;
			}
		}


		public bool ShowDiagramSheet {
			get { return _showSheet; }
			set {
				_showSheet = value;
				if (CurrentDisplay != null) CurrentDisplay.IsSheetVisible = value;
			}
		}


		public bool EnterClosesTextEditor {
			get { return _enterClosesTextEditor; }
			set {
				_enterClosesTextEditor = value;
				if (CurrentDisplay != null) CurrentDisplay.CloseCaptionEditorWithEnter = value;
			}
		}


		public bool ShowGrid {
			get { return _showGrid; }
			set {
				_showGrid = value;
				if (CurrentDisplay != null) CurrentDisplay.IsGridVisible = value;
			}
		}


		public bool SnapToGrid {
			get { return _snapToGrid; }
			set {
				_snapToGrid = value;
				if (CurrentDisplay != null) CurrentDisplay.SnapToGrid = value;
			}
		}


		public Color GridColor {
			get { return _gridColor; }
			set {
				_gridColor = value;
				if (CurrentDisplay != null) CurrentDisplay.GridColor = value;
			}
		}


		public bool ShowDefaultContextMenu {
			get { return _showDefaultContextMenu; }
			set {
				_showDefaultContextMenu = value;
				toolSetListViewPresenter.ShowDefaultContextMenu =
				modelTreePresenter.ShowDefaultContextMenu =
				layerEditorListView.ShowDefaultContextMenu = value;
				if (CurrentDisplay != null) CurrentDisplay.ShowDefaultContextMenu = value;
			}
		}


		public bool HideDeniedMenuItems {
			get { return _hideDeniedMenuItems; }
			set {
				_hideDeniedMenuItems = value;
				toolSetListViewPresenter.HideDeniedMenuItems =
				modelTreePresenter.HideDeniedMenuItems =
				layerEditorListView.HideDeniedMenuItems = value;
				if (CurrentDisplay != null) CurrentDisplay.HideDeniedMenuItems = value;
			}
		}


		public int GridSize {
			get { return _gridSize; }
			set {
				_gridSize = value;
				if (CurrentDisplay != null) CurrentDisplay.GridSize = value;
			}
		}


		public int SnapDistance {
			get { return _snapDistance; }
			set {
				_snapDistance = value;
				if (CurrentDisplay != null) CurrentDisplay.SnapDistance = value;
			}
		}


		public ControlPointShape ResizePointShape {
			get { return _resizePointShape; }
			set {
				_resizePointShape = value;
				if (CurrentDisplay != null) CurrentDisplay.ResizeGripShape = value;
			}
		}


		public ControlPointShape ConnectionPointShape {
			get { return _connectionPointShape; }
			set {
				_connectionPointShape = value;
				if (CurrentDisplay != null) CurrentDisplay.ConnectionPointShape = value;
			}
		}


		public int ControlPointSize {
			get { return _controlPointSize; }
			set {
				_controlPointSize = value;
				if (CurrentDisplay != null) CurrentDisplay.GripSize = value;
			}
		}


		public int ZoomHD {
			get { return _zoomHD; }
			set {
				if (_zoomHD != value) {
					_zoomHD = value;
					if (CurrentDisplay != null && CurrentDisplay.ZoomLevelHD != _zoomHD)
						CurrentDisplay.ZoomLevelHD = _zoomHD;
					UpdateZoomControl();
				}
			}
		}


		public bool HighQuality {
			get { return _highQuality; }
			set {
				_highQuality = value;
				if (CurrentDisplay != null) {
					CurrentDisplay.HighQualityBackground = value;
					CurrentDisplay.HighQualityRendering = value;
					CurrentDisplay.Refresh();
				}
			}
		}


		public Display CurrentDisplay {
			get { return _currentDisplay; }
			set {
				if (_currentDisplay != null) {
					UnregisterDisplayEvents(_currentDisplay);
					((IDiagramPresenter)_currentDisplay).CloseCaptionEditor(true);
				}
				_currentDisplay = value;
				if (_currentDisplay != null) {
					RegisterDisplayEvents(_currentDisplay);

					// Grip settings
					_currentDisplay.ConnectionPointShape = ConnectionPointShape;
					_currentDisplay.ResizeGripShape = ResizePointShape;
					_currentDisplay.GripSize = ControlPointSize;
					// Grid settings
					_currentDisplay.GridColor = GridColor;
					_currentDisplay.GridSize = GridSize;
					_currentDisplay.IsGridVisible = ShowGrid;
					// Display Settings
					_currentDisplay.ZoomWithMouseWheel = MouseWheelZoom;
					_currentDisplay.IsUniversalScrollEnabled = UseUniversalScroll;
					_currentDisplay.PanMouseButton = PanButtons;
					_currentDisplay.ScrollBarsVisible = ShowScrollBars;
					_currentDisplay.IsSheetVisible = ShowDiagramSheet;
					_currentDisplay.CloseCaptionEditorWithEnter = EnterClosesTextEditor;
					// Security Settings
					_currentDisplay.ShowDefaultContextMenu = ShowDefaultContextMenu;
					_currentDisplay.HideDeniedMenuItems = HideDeniedMenuItems;
					// Rendering settings
					_currentDisplay.HighQualityRendering = HighQuality;
					_currentDisplay.HighQualityBackground = HighQuality;
#if DEBUG_UI
					_currentDisplay.IsDebugInfoCellOccupationVisible = ShowCellOccupation;
					_currentDisplay.IsDebugInfoInvalidateVisible = ShowInvalidatedAreas;
#endif
					// Apply display's zoom to the UI
					ZoomHD = _currentDisplay.ZoomLevelHD;

					if (_currentDisplay.Diagram != null) {
						_currentDisplay.ActiveTool = toolSetController.SelectedTool;
						if (_currentDisplay.SelectedShapes.Count > 0)
							propertyController.SetObjects(0, _currentDisplay.SelectedShapes);
						else
							propertyController.SetObject(0, _currentDisplay.Diagram);
					}
					layerPresenter.DiagramPresenter = CurrentDisplay;

					if (_layoutControlForm != null) {
						if (_currentDisplay.Diagram != null)
							_layoutControlForm.Diagram = _currentDisplay.Diagram;
						else _layoutControlForm.Close();
					}

					display_ShapesSelected(_currentDisplay, EventArgs.Empty);
				}
			}
		}


#if DEBUG_UI

		public bool ShowCellOccupation {
			get { return _showCellOccupation; }
			set {
				_showCellOccupation = value;
				if (CurrentDisplay != null) CurrentDisplay.IsDebugInfoCellOccupationVisible = value;
			}
		}


		public bool ShowInvalidatedAreas {
			get { return _showInvalidatedAreas; }
			set {
				_showInvalidatedAreas = value;
				if (CurrentDisplay != null) CurrentDisplay.IsDebugInfoInvalidateVisible = value;
			}
		}

#endif

		#endregion


		private void CheckFrameworkVersion() {
			System.Reflection.Assembly exeAssembly = this.GetType().Assembly;
			System.Reflection.Assembly coreAssembly = typeof(Project).Assembly;
			System.Reflection.Assembly uiAssembly = typeof(Display).Assembly;

			if (exeAssembly == null || coreAssembly == null || uiAssembly == null) {
				throw new Exception("Failed to retrive component's assemblies.");
			} else {
				// Check installed .NET framework version
				Version coreAssemblyVersion = new Version(coreAssembly.ImageRuntimeVersion.Replace("v", string.Empty));
				Version uiAssemblyVersion = new Version(uiAssembly.ImageRuntimeVersion.Replace("v", string.Empty));
				Version exeAssemblyVersion = new Version(exeAssembly.ImageRuntimeVersion.Replace("v", string.Empty));
				if (Environment.Version < coreAssemblyVersion
					|| Environment.Version < uiAssemblyVersion
					|| Environment.Version < exeAssemblyVersion) {
					string msg = string.Empty;
					msg += string.Format("The installed .NET framework version does not meet the requirements:{0}", Environment.NewLine);
					msg += string.Format(".NET Framework {0} is installed, version {1} is required.", Environment.Version, coreAssembly.ImageRuntimeVersion);
					throw new NShapeException(msg);
				}

				System.Reflection.AssemblyName designerAssemblyName = this.GetType().Assembly.GetName();
				System.Reflection.AssemblyName coreAssemblyName = typeof(Project).Assembly.GetName();
				System.Reflection.AssemblyName uiAssemblyName = typeof(Display).Assembly.GetName();
				// Check nShape framework library versions
				if (coreAssemblyName.Version != uiAssemblyName.Version) {
					string msg = string.Empty;
					msg += "The versions of the loaded nShape framework libraries do not match:" + Environment.NewLine;
					msg += string.Format("{0}: Version {1}{2}", coreAssemblyName.Name, coreAssemblyName.Version, Environment.NewLine);
					msg += string.Format("{0}: Version {1}{2}", uiAssemblyName.Name, uiAssemblyName.Version, Environment.NewLine);
					throw new NShapeException(msg);
				}
				// Check program against used nShape framework library versions
				if (coreAssemblyName.Version != designerAssemblyName.Version
					|| uiAssemblyName.Version != designerAssemblyName.Version) {
					string msg = string.Empty;
					msg += "The version of this program does not match the versions of the loaded nShape framework libraries:" + Environment.NewLine;
					msg += string.Format("{0}: Version {1}{2}", designerAssemblyName.Name, designerAssemblyName.Version, Environment.NewLine);
					msg += string.Format("{0}: Version {1}{2}", coreAssemblyName.Name, coreAssemblyName.Version, Environment.NewLine);
					msg += string.Format("{0}: Version {1}{2}", uiAssemblyName.Name, uiAssemblyName.Version, Environment.NewLine);
					MessageBox.Show(this, msg, "Assembly Version Mismatch", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}
			}
		}


		private void SetToolTipTexts() {
			statusStrip.ShowItemToolTips = true;

			statusLabelMessage.ToolTipText = string.Empty;

			toolStripStatusLabelDiagram.ToolTipText =
			statusLabelShapeCount.ToolTipText = "Number of shapes in the current diagram";

			toolStripStatusLabelSelection.ToolTipText = "Position and size of the current selection";
			statusLabelMousePosition.ToolTipText = "Mouse position in diagram coordinates";
			statusLabelSelectionSize.ToolTipText = "The size of the current selection in diagram coordinates";

			toolStripStatusLabelDisplayArea.ToolTipText = "The currently displayed area in diagram coordinates";
			statusLabelTopLeft.ToolTipText = "The top left corner of currently displayed area in diagram coordinates";
			statusLabelBottomRight.ToolTipText = "The bottom right corner of currently displayed area in diagram coordinates";
		}


		#region [Private] Methods: ConfigFile, Project and Store

		private XmlReader OpenCfgReader(string filePath) {
			XmlReaderSettings settings = new XmlReaderSettings();
			settings.CloseInput = true;
			return XmlReader.Create(filePath, settings);
		}


		private XmlWriter OpenCfgWriter(string filePath) {
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.CloseOutput = true;
			settings.Indent = true;
			return XmlWriter.Create(filePath, settings);
		}


		private void CreateConfigFile(string filePath) {
			// Create config file and basic nodes
			XmlWriter cfgWriter = OpenCfgWriter(filePath);
			cfgWriter.WriteStartDocument();
			cfgWriter.WriteStartElement(NodeNameSettings);
			// Setting "ProjectDirectory"
			cfgWriter.WriteStartElement(NodeNameProjectDirectory);
			cfgWriter.WriteEndElement();
			// Setting "LoadToolbarSettings"
			cfgWriter.WriteStartElement(NodeNameLoadToolbarSettings);
			cfgWriter.WriteEndElement();
			// Setting "Recent Projects"
			cfgWriter.WriteStartElement(NodeNameProjects);
			cfgWriter.WriteEndElement();
			// Setting "Window Settings"
			cfgWriter.WriteStartElement(NodeNameWindowSettings);
			cfgWriter.WriteEndElement();
			cfgWriter.Close();
		}


		private XmlNode CreateConfigNode(XmlDocument xmlDoc, XmlNode parent, string nodeName) {
			XmlNode result = xmlDoc.CreateNode(XmlNodeType.Element, nodeName, xmlDoc.NamespaceURI);
			if (parent != null)
				parent.AppendChild(result);
			else xmlDoc.AppendChild(result);
			return result;
		}


		private XmlDocument ReadConfigFile() {
			XmlDocument result = new XmlDocument();
			string filePath = Path.Combine(_configFolder, ConfigFileName);
			if (File.Exists(filePath)) {
				XmlReader cfgReader = OpenCfgReader(filePath);

				result.Load(cfgReader);
				cfgReader.Close();
			}
			return result;
		}


		private void ReadConfig(XmlDocument xmlDoc) {
			// Read recently used directory for XML repositories
			XmlNode projectDirNode = xmlDoc.SelectSingleNode(string.Format(QueryNodeAttr, NodeNameProjectDirectory, AttrNamePath));
			if (projectDirNode != null) _xmlStoreDirectory = projectDirNode.Attributes[AttrNamePath].Value;

			// Read recently used directory for XML repositories
			XmlNode loadToolbarsNode = xmlDoc.SelectSingleNode(string.Format(QueryNodeAttr, NodeNameLoadToolbarSettings, AttrNameValue));
			if (loadToolbarsNode != null) _loadToolStripLayoutOnStartup = bool.Parse(loadToolbarsNode.Attributes[AttrNameValue].Value);

			// Read recently opened projects
			foreach (XmlNode xmlNode in xmlDoc.SelectNodes(string.Format(QueryNode, NodeNameProject))) {
				try {
					RepositoryInfo repositoryInfo = RepositoryInfo.Empty;
					repositoryInfo.projectName = xmlNode.Attributes[AttrNameName].Value;
					repositoryInfo.typeName = xmlNode.Attributes[AttrNameRepositoryType].Value;
					repositoryInfo.computerName = xmlNode.Attributes[AttrNameServerName].Value;
					repositoryInfo.location = xmlNode.Attributes[AttrNameDataSource].Value;
					if (repositoryInfo != RepositoryInfo.Empty && !_recentProjects.Contains(repositoryInfo)) {
						if (_recentProjects.Count == recentProjectsItemCount)
							_recentProjects.RemoveAt(0);
						_recentProjects.Add(repositoryInfo);
					}
				} catch (Exception exc) {
					Debug.Fail(exc.Message);
				}
			}
		}


		private void ReadWindowConfig(XmlDocument xmlDoc) {
			// Load ToolStrip Positions only if a config was saved before
			if (_loadToolStripLayoutOnStartup) ToolStripManager.LoadSettings(this, GetToolStripSettingsName());
			else _loadToolStripLayoutOnStartup = true;  // Load next time

			// Read window settings
			XmlNode wndSettingsNode = xmlDoc.SelectSingleNode(string.Format(QueryNode, NodeNameWindowSettings));
			if (wndSettingsNode != null && wndSettingsNode.Attributes.Count > 0) {
				try {
					int val;
					if (int.TryParse(wndSettingsNode.Attributes[AttrNamePositionX].Value, out val)) Left = val;
					if (int.TryParse(wndSettingsNode.Attributes[AttrNamePositionY].Value, out val)) Top = val;
					if (int.TryParse(wndSettingsNode.Attributes[AttrNameWidth].Value, out val)) Width = val;
					if (int.TryParse(wndSettingsNode.Attributes[AttrNameHeight].Value, out val)) Height = val;
					WindowState = (FormWindowState)int.Parse(wndSettingsNode.Attributes[AttrNameState].Value);
				} catch (Exception exc) {
					Debug.Fail(exc.Message);
				}
			}
		}


		private void SaveConfigFile() {
			// Save ToolStrip Positions
			try {
				ToolStripManager.SaveSettings(this, GetToolStripSettingsName());

				// Create a new config file if it does not exist
				string filePath = Path.Combine(_configFolder, ConfigFileName);
				if (!File.Exists(filePath)) 
					CreateConfigFile(filePath);

				XmlDocument xmlDoc = new XmlDocument();
				using (XmlReader cfgReader = OpenCfgReader(filePath)) {
					xmlDoc.Load(cfgReader);
					cfgReader.Close();
				}

				XmlNode settingsNode = xmlDoc.SelectSingleNode(string.Format(QueryNode, NodeNameSettings));
				Debug.Assert(settingsNode != null);

				// Find the "Project Directory" node
				XmlNode projectDirectoryNode = xmlDoc.SelectSingleNode(string.Format(QueryNode, NodeNameProjectDirectory));
				if (projectDirectoryNode == null)
					projectDirectoryNode = CreateConfigNode(xmlDoc, settingsNode, NodeNameProjectDirectory);
				// Save last project directory
				projectDirectoryNode.RemoveAll();
				projectDirectoryNode.Attributes.Append(xmlDoc.CreateAttribute(AttrNamePath)).Value = _xmlStoreDirectory;

				// Find the "Load Toolbar Settings" node
				XmlNode loadToolbarSettingsNode = xmlDoc.SelectSingleNode(string.Format(QueryNode, NodeNameLoadToolbarSettings));
				if (loadToolbarSettingsNode == null)
					loadToolbarSettingsNode = CreateConfigNode(xmlDoc, settingsNode, NodeNameLoadToolbarSettings);
				// Save last project directory
				loadToolbarSettingsNode.RemoveAll();
				loadToolbarSettingsNode.Attributes.Append(xmlDoc.CreateAttribute(AttrNameValue)).Value = _loadToolStripLayoutOnStartup.ToString();

				// Find the "Projects" node
				XmlNode repositoriesNode = xmlDoc.SelectSingleNode(string.Format(QueryNode, NodeNameProjects));
				Debug.Assert(repositoriesNode != null);
				// Save all recent projects
				repositoriesNode.RemoveAll();
				foreach (RepositoryInfo projectInfo in _recentProjects) {
					XmlNode newNode = xmlDoc.CreateNode(XmlNodeType.Element, NodeNameProject, xmlDoc.NamespaceURI);
					newNode.Attributes.Append(xmlDoc.CreateAttribute(AttrNameName)).Value = projectInfo.projectName;
					newNode.Attributes.Append(xmlDoc.CreateAttribute(AttrNameRepositoryType)).Value = projectInfo.typeName;
					newNode.Attributes.Append(xmlDoc.CreateAttribute(AttrNameServerName)).Value = projectInfo.computerName;
					newNode.Attributes.Append(xmlDoc.CreateAttribute(AttrNameDataSource)).Value = projectInfo.location;
					repositoriesNode.AppendChild(newNode);
				}

				// Find "WindowSettings" node
				XmlNode wndSettingsNode = xmlDoc.SelectSingleNode(string.Format(QueryNode, NodeNameWindowSettings));
				Debug.Assert(wndSettingsNode != null);
				if (wndSettingsNode.Attributes.Count == 0) {
					wndSettingsNode.Attributes.Append(xmlDoc.CreateAttribute(AttrNamePositionX));
					wndSettingsNode.Attributes.Append(xmlDoc.CreateAttribute(AttrNamePositionY));
					wndSettingsNode.Attributes.Append(xmlDoc.CreateAttribute(AttrNameWidth));
					wndSettingsNode.Attributes.Append(xmlDoc.CreateAttribute(AttrNameHeight));
					wndSettingsNode.Attributes.Append(xmlDoc.CreateAttribute(AttrNameState));
				}
				switch (WindowState) {
					case FormWindowState.Maximized:
					case FormWindowState.Minimized:
						wndSettingsNode.Attributes[AttrNameState].Value = ((int)FormWindowState.Maximized).ToString();
						break;
					case FormWindowState.Normal:
						wndSettingsNode.Attributes[AttrNamePositionX].Value = Left.ToString();
						wndSettingsNode.Attributes[AttrNamePositionY].Value = Top.ToString();
						wndSettingsNode.Attributes[AttrNameWidth].Value = Width.ToString();
						wndSettingsNode.Attributes[AttrNameHeight].Value = Height.ToString();
						wndSettingsNode.Attributes[AttrNameState].Value = ((int)FormWindowState.Normal).ToString();
						break;
				}

				// Save to file
				using (XmlWriter cfgWriter = OpenCfgWriter(filePath)) {
					xmlDoc.Save(cfgWriter);
					cfgWriter.Close();
				}
			} catch (System.Configuration.ConfigurationException exc) {
				// This kind of exception is thrown when multiple instances of the NShape designer are 
				// closed with the "Close Group" command and all instances try to save their config. 
				// As Merging the changes of each instance is too much overhead here, the first instance 
				// that opened the config files may write its changes, all other changes are discarded.
				Debug.Print(exc.Message);
			} catch (IOException exc) {
				// See comment above (this exception deals with the XML config file)
				Debug.Print(exc.Message);
			}
		}


		private string GetToolStripSettingsName() {
			return string.Format("{0} {1}", this.Name, ProductVersion);
		}


		private void MaintainRecentProjects() {
			// Check existence of all recently opened (XML) projects 
			List<int> missingProjects = null;
			for (int i = _recentProjects.Count - 1; i >= 0; --i) {
				if (_recentProjects[i].typeName == RepositoryInfo.SqlServerStoreTypeName)
					continue;
				else if (_recentProjects[i].typeName == RepositoryInfo.XmlStoreTypeName) {
					if (!File.Exists(_recentProjects[i].location)) {
						// Add all indexes of missing projects, decide later what to to
						if (missingProjects == null) missingProjects = new List<int>();
						missingProjects.Insert(0, i);
					}
				}
			}
			// If projects are missing, let the user decide what to do
			if (missingProjects != null && missingProjects.Count > 0) {
				for (int i = missingProjects.Count - 1; i >= 0; --i)
					_recentProjects.RemoveAt(missingProjects[i]);
				SaveConfigFile();
			}
		}


		private void ReplaceStore(string projectName, Store store) {
			UnregisterRepositoryEvents();
			project.Name = projectName;
			((CachedRepository)project.Repository).Store = store;
			if (store is XmlStore) {
				XmlStore xmlStore = (XmlStore)store;
				xmlStore.BackupFileExtension = XmlStore.DefaultBackupFileExtension;
				xmlStore.BackupGenerationMode = XmlStore.BackupFileGenerationMode.BakFile;
			}
			RegisterRepositoryEvents();
		}


		private void PrependRecentProjectsMenuItem(RepositoryInfo projectInfo) {
			ToolStripItem item = new ToolStripMenuItem(projectInfo.projectName);
			item.Tag = projectInfo;
			item.ToolTipText = string.Format("Project: {0}{3}Location: {1}{3}Repository Type: {2}", projectInfo.projectName, projectInfo.location, projectInfo.typeName, Environment.NewLine);
			item.Click += openRecentProjectMenuItem_Click;
			recentProjectsMenuItem.DropDownItems.Insert(0, item);
			if (!recentProjectsMenuItem.Visible) recentProjectsMenuItem.Visible = true;
		}


		private void CreateRecentProjectsMenuItems() {
			ClearRecentProjectsMenu();
			recentProjectsMenuItem.Visible = (_recentProjects.Count > 0);
			foreach (RepositoryInfo recentProject in _recentProjects)
				PrependRecentProjectsMenuItem(recentProject);
		}


		private RepositoryInfo GetReposistoryInfo(Project project) {
			RepositoryInfo projectInfo = RepositoryInfo.Empty;
			projectInfo.projectName = project.Name;
			Store store = ((CachedRepository)project.Repository).Store;
			if (store is XmlStore) {
				projectInfo.typeName = RepositoryInfo.XmlStoreTypeName;
				string filePath = ((XmlStore)store).ProjectFilePath;
				projectInfo.location = filePath;
				projectInfo.computerName = Environment.MachineName;
			} else if (store is SqlStore) {
				projectInfo.typeName = RepositoryInfo.SqlServerStoreTypeName;
				projectInfo.location = ((SqlStore)store).DatabaseName;
				projectInfo.computerName = ((SqlStore)store).ServerName;
			} else Debug.Fail("Unexpected repository type");
			return projectInfo;
		}


		private bool AddToRecentProjects(Project project) {
			return AddToRecentProjects(GetReposistoryInfo(project));
		}


		private bool AddToRecentProjects(RepositoryInfo projectInfo) {
			// Check if the project already exists in the recent projects list
			foreach (RepositoryInfo recentProject in _recentProjects)
				if (recentProject == projectInfo) return false;
			// If it does not, add it and create a new menu item
			if (_recentProjects.Count == recentProjectsItemCount)
				_recentProjects.RemoveAt(0);
			_recentProjects.Add(projectInfo);
			PrependRecentProjectsMenuItem(projectInfo);
			SaveConfigFile();
			return true;
		}


		private bool RemoveFromRecentProjects(Project project) {
			return RemoveFromRecentProjects(GetReposistoryInfo(project));
		}


		private bool RemoveFromRecentProjects(RepositoryInfo projectInfo) {
			return _recentProjects.Remove(projectInfo);
		}


		private void UpdateRecentProjectsMenu() {
			ClearRecentProjectsMenu();
			foreach (RepositoryInfo pi in _recentProjects)
				PrependRecentProjectsMenuItem(pi);
		}


		private void ClearRecentProjectsMenu() {
			for (int i = recentProjectsMenuItem.DropDownItems.Count - 1; i >= 0; --i) {
				recentProjectsMenuItem.DropDownItems[i].Click -= openRecentProjectMenuItem_Click;
				recentProjectsMenuItem.DropDownItems[i].Dispose();
			}
			recentProjectsMenuItem.DropDownItems.Clear();
		}


		private void CreateProject(string projectName, Store store, bool askUserLoadLibraries) {
			ReplaceStore(projectName, store);
			project.Create();
			_projectSaved = false;
			DisplayDiagrams(true);
			if (askUserLoadLibraries)
				CheckLibrariesLoaded();
			// Adjust menu items
			saveMenuItem.Enabled = true;
			saveAsMenuItem.Enabled = true;
			upgradeVersionMenuItem.Enabled = false;
			if (store is XmlStore || store == null) {
				useEmbeddedImagesToolStripMenuItem.Enabled = true;
				if (store != null)
					useEmbeddedImagesToolStripMenuItem.Checked = (((XmlStore)store).ImageLocation == XmlStore.ImageFileLocation.Embedded);
			} else useEmbeddedImagesToolStripMenuItem.Enabled = false;
		}


		private void CheckLibrariesLoaded() {
			bool librariesLoaded = false;
			foreach (System.Reflection.Assembly a in project.Libraries) {
				librariesLoaded = true;
				break;
			}
			if (!librariesLoaded) {
				if (MessageBox.Show(this, "Do you want to load shape libraries now?", "Load shape libraries",
					MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes) {
					using (LibraryManagementDialog dlg = new LibraryManagementDialog(project))
						dlg.ShowDialog(this);
				}
			}
		}


		private void OpenProject(string projectName, Store repository) {
			Cursor = Cursors.WaitCursor;
			Application.DoEvents();
			string errorMessage = null;
			try {
				ReplaceStore(projectName, repository);
				project.Open();
				DisplayDiagrams(false);

				_projectSaved = true;
				// Move project on top of the recent projects list 
				RepositoryInfo repositoryInfo = GetReposistoryInfo(project);
				RemoveFromRecentProjects(repositoryInfo);
				AddToRecentProjects(repositoryInfo);
				UpdateRecentProjectsMenu();
			} catch (LoadLibraryException exc) {
				errorMessage = string.Format("Loading library '{0}' failed", exc.AssemblyName);
				errorMessage += (exc.InnerException != null) ? string.Format(": {0}{0}{1}", Environment.NewLine, exc.InnerException.Message) : ".";
			} catch (Exception exc) {
				errorMessage = exc.Message;
			} finally {
				if (errorMessage != null) {
					MessageBox.Show(this, errorMessage, "Error while opening Repository.", MessageBoxButtons.OK, MessageBoxIcon.Error);
					project.Close();
				}
				Cursor = Cursors.Default;
			}
		}


		private bool SaveProject() {
			bool result = false;
			if (!_projectSaved || !project.Repository.Exists())
				result = SaveProjectAs();
			else result = DoSaveProject();
			return result;
		}


		private bool SaveProjectAs() {
			bool result = false;
			CachedRepository cachedRepository = (CachedRepository)project.Repository;
			bool isNewstore = false;

			// If there is no store, create an XmlStore
			if (cachedRepository.Store == null) {
				// Get default storage location
				if (!Directory.Exists(_xmlStoreDirectory)) {
					StringBuilder path = new StringBuilder(512);
					const int COMMON_DOCUMENTS = 0x002e;
					if (SHGetSpecialFolderPath(IntPtr.Zero, path, COMMON_DOCUMENTS, 0) != 0)
						_xmlStoreDirectory = Path.Combine(Path.Combine(path.ToString(), "NShape"), "Demo Projects");
					else
						_xmlStoreDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
				}

				XmlStore xmlStore = new XmlStore() {
					DirectoryName = _xmlStoreDirectory,
					FileExtension = ProjectFileExtension,
					LazyLoading = true
				};
				ReplaceStore(cachedRepository.ProjectName, xmlStore);
				isNewstore = true;
			}

			// Save store as...
			if (cachedRepository.Store is XmlStore) {
				XmlStore xmlStore = (XmlStore)cachedRepository.Store;

				// Select a file name
				saveFileDialog.CreatePrompt = false;        // Do not ask whether to create the file
				saveFileDialog.CheckFileExists = false;     // Do not check whether the file does NOT exist
				saveFileDialog.CheckPathExists = true;      // Ask whether to overwrite existing file
				saveFileDialog.DefaultExt = ProjectFileExtension;
				saveFileDialog.AddExtension = true;
				saveFileDialog.Filter = FileFilterXmlRepository;
				if (Directory.Exists(xmlStore.DirectoryName))
					saveFileDialog.InitialDirectory = xmlStore.DirectoryName;
				else
					saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
				saveFileDialog.FileName = Path.GetFileName(xmlStore.ProjectFilePath);

				// Try to save repository to file...
				if (saveFileDialog.ShowDialog() == DialogResult.OK) {
					if (xmlStore.ProjectFilePath != saveFileDialog.FileName) {
						project.Name = Path.GetFileNameWithoutExtension(saveFileDialog.FileName);
						xmlStore.DirectoryName = Path.GetDirectoryName(saveFileDialog.FileName);
						string ext = Path.GetExtension(saveFileDialog.FileName);
						if (!string.IsNullOrEmpty(ext) && ext != xmlStore.FileExtension)
							xmlStore.FileExtension = ext;

						Text = string.Format("{0} - {1}", AppTitle, project.Name);
					}
					// Delete file if it exists, because the user was prompted whether to overwrite it before (SaveFileDialog.CheckPathExists).
					if (project.Repository.Exists())
						project.Repository.Erase();
					// If the XmlStore was freshly created, call create in order to create the store media and (internally) open the store
					if (isNewstore)
						xmlStore.Create(cachedRepository);
					xmlStore.ImageLocation = XmlImageFileLocation;

					saveMenuItem.Enabled = true;
					result = DoSaveProject();
				}
			} else if (cachedRepository.Store is AdoNetStore) {
				// Save repository to database because the database and the project name are 
				// selected before creating the project when using AdoNet stores
				saveMenuItem.Enabled = true;
				result = DoSaveProject();
			} else {
				if (cachedRepository.Store == null)
					MessageBox.Show(this, "There is no store component attached to the repository.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				else {
					string msg = string.Format("Unsupported store type: '{0}'.", cachedRepository.Store.GetType().Name);
					MessageBox.Show(this, msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
			return result;
		}


		private bool DoSaveProject() {
			bool result = false;
			this.Cursor = Cursors.WaitCursor;
			Application.DoEvents();
			try {
				if (cachedRepository.Store is XmlStore) {
					// Ensure that all diagrams are loaded completely for XmlStores with LazyLoading enabled
					foreach (Diagram d in project.Repository.GetDiagrams())
						project.Repository.GetDiagramShapes(d);
				}

				// Close open caption editor before saving
				if (CurrentDisplay != null)
					((IDiagramPresenter)CurrentDisplay).CloseCaptionEditor(true);

				// Save changes to file / database
				project.Repository.SaveChanges();
				_projectSaved = true;

				// Add project to "Recent Projects" list
				RepositoryInfo projectInfo = GetReposistoryInfo(project);
				RemoveFromRecentProjects(projectInfo);
				AddToRecentProjects(projectInfo);
				UpdateRecentProjectsMenu();
				result = true;
			} catch (IOException exc) {
				MessageBox.Show(this, exc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			} catch (Exception exc) {
				MessageBox.Show(this, exc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			} finally {
				this.Cursor = Cursors.Default;
			}
			return result;
		}


		private bool CloseProject() {
			bool result = true;
			// We check not only the Repository's IsModified property but also the projectIsModified field
			// in order to suppress the "Save changes?" question when closing a new project with loaded 
			// libraries (which modify the repository)
			if (_projectIsModified && project.Repository.IsModified) {
				string msg = string.Format("Do you want to save the current project '{0}' before closing it?", project.Name);
				DialogResult dlgResult = MessageBox.Show(this, msg, "Save changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
				switch (dlgResult) {
					case DialogResult.Yes:
						if (project.Repository.Exists())
							DoSaveProject();
						else SaveProjectAs();
						break;
					case DialogResult.No:
						// do nothing
						break;
					case DialogResult.Cancel:
						result = false;
						break;
				}
			}

			if (result) {
				project.Close();
				_projectSaved = false;
				// Clear all displays and diagramControllers
				for (int i = displayTabControl.TabPages.Count - 1; i >= 0; --i) {
					DisplayTabPage displayTabPage = displayTabControl.TabPages[i] as DisplayTabPage;
					displayTabControl.TabPages.RemoveAt(i);
					if (displayTabPage != null) {
						UnregisterDisplayEvents(displayTabPage.Display);
						displayTabPage.Dispose();
					}
				}
			}

			return result;
		}


		private XmlStore.ImageFileLocation XmlImageFileLocation {
			get {
				return useEmbeddedImagesToolStripMenuItem.Checked ? XmlStore.ImageFileLocation.Embedded : XmlStore.ImageFileLocation.Directory;
			}
		}


		private void ImportRepositoryFromClipboard() {
			try {
				const StringComparison comparison = StringComparison.InvariantCultureIgnoreCase;
				string xmlString = Clipboard.GetText();
				// We insist on UTF-8 encoding because of the conversion into a byte array which must use a defined encoding
				if (!xmlString.StartsWith("<?xml", comparison) || xmlString.IndexOf("encoding=\"utf-8\"?>", comparison) < 0)
					MessageBox.Show(this, "Clipboard does not contain valid UTF-8 encoded XML text.", "Invalid Data", MessageBoxButtons.OK, MessageBoxIcon.Error);
				else if (xmlString.IndexOf("<dataweb_nshape version=", comparison) < 0)
					MessageBox.Show(this, "Clipboard does not contain valid XML repository data.", "Invalid Data", MessageBoxButtons.OK, MessageBoxIcon.Error);
				else {
					if (project.IsOpen) {
						DialogResult res = MessageBox.Show("Importing XML from Clipboard requires the project to be closed. Do you want to close the current project now?",
							"Close Project?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
						if (res == DialogResult.No)
							return;
						if (!CloseProject())
							return;
					}

					// Delete current repository
					project.Repository = null;
					project.Name = NewProjectName;

					using (MemoryStream memStream = new MemoryStream(Encoding.UTF8.GetBytes(xmlString)))
						project.ReadXml(memStream);
					// Assign the new Repository to the main form's cachedRepository component
					cachedRepository = (CachedRepository)project.Repository;

					// Display the result
					DisplayDiagrams(false);
				}
			} catch (Exception exc) {
				MessageBox.Show(this, exc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}


		private void ExportRepositoryToClipboard() {
			try {
				if (project.Repository is CachedRepository && project.Repository.IsOpen) {
					string xmlString = project.GetXml();
					Clipboard.SetText(xmlString);
				}
			} catch (Exception exc) {
				MessageBox.Show(this, exc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		#endregion


		#region [Private] Methods: Update Controls states and visibility

		private void DisplayMouseCoordinates(int x, int y) {
			statusLabelMousePosition.Text = string.Format(PointFormatStr, x, y);
		}


		private void UpdateStatusInfo() {
			Point mousePos = Control.MousePosition;
			Size selectionSize = Size.Empty;
			Rectangle bounds = Rectangle.Empty;
			int shapeCnt = 0;
			if (CurrentDisplay != null && CurrentDisplay.Diagram != null) {
				CurrentDisplay.ControlToDiagram(CurrentDisplay.DrawBounds, out bounds);
				//diagramSize = currentDisplay.Diagram.Size;
				shapeCnt = _currentDisplay.Diagram.Shapes.Count;

				if (CurrentDisplay.SelectedShapes.Count > 0)
					selectionSize = CurrentDisplay.SelectedShapes.GetBoundingRectangle(true).Size;
			}
			statusLabelTopLeft.Text = string.Format(PointFormatStr, bounds.Left, bounds.Top);
			statusLabelBottomRight.Text = string.Format(PointFormatStr, bounds.Right, bounds.Bottom);
			statusLabelMousePosition.Text = string.Format(PointFormatStr, mousePos.X, mousePos.X);
			statusLabelSelectionSize.Text = string.Format(SizeFormatStr, selectionSize.Width, selectionSize.Height);
			statusLabelShapeCount.Text = string.Format(ShapeCntFormatStr, shapeCnt, shapeCnt == 1 ? string.Empty : "s");
		}


		private void UpdateAllMenuItems() {
			upgradeVersionMenuItem.Enabled = false;
			if (!project.IsOpen)
				upgradeVersionMenuItem.ToolTipText = "No project opened.";
			else if (!project.Repository.CanModifyVersion)
				upgradeVersionMenuItem.ToolTipText = "The current repository does not support upgrading storage version.";
			else if (project.Repository.Version >= _maxRepositoryVersion)
				upgradeVersionMenuItem.ToolTipText = "The repository storage version is up to date.";
			else {
				upgradeVersionMenuItem.Enabled = true;
				upgradeVersionMenuItem.ToolTipText = string.Format("Upgrade repository storage version from {0} to {1}.",
					project.Repository.Version, _maxRepositoryVersion);
			}

			if (cachedRepository.Store is XmlStore || cachedRepository.Store == null) {
				useEmbeddedImagesToolStripMenuItem.Enabled = true;
				if (cachedRepository.Store != null)
					useEmbeddedImagesToolStripMenuItem.Checked = (((XmlStore)cachedRepository.Store).ImageLocation == XmlStore.ImageFileLocation.Embedded);
				useEmbeddedImagesToolStripMenuItem.ToolTipText = "Embed images into the storage file instead of saving them to a seperate image directory.";
			} else {
				useEmbeddedImagesToolStripMenuItem.Enabled = false;
				useEmbeddedImagesToolStripMenuItem.Checked = false;
				useEmbeddedImagesToolStripMenuItem.ToolTipText = "The current repository does not support embedding images.";
			}

			UpdateEditMenuItems();
			UpdateUndoMenuItems();
		}


		private void UpdateUndoMenuItems() {
			// Undo / Redo
			undoToolStripSplitButton.Enabled =
			undoMenuItem.Enabled = diagramSetController.Project.History.UndoCommandCount > 0;
			redoToolStripSplitButton.Enabled =
			redoMenuItem.Enabled = diagramSetController.Project.History.RedoCommandCount > 0;
		}


		private void UpdateEditMenuItems() {
			bool shapesOnly, shapesAndModels;
			if (CurrentDisplay != null && CurrentDisplay.Diagram != null) {
				// Cut
				shapesOnly = shapesAndModels = CurrentDisplay.DiagramSetController.CanCut(CurrentDisplay.Diagram, CurrentDisplay.SelectedShapes);
				cutShapeButton.Enabled = shapesOnly;
				cutShapeOnlyMenuItem.Enabled = shapesOnly;
				cutShapeAndModelMenuItem.Enabled = shapesAndModels;
				// Copy
				shapesOnly =
				shapesAndModels = CurrentDisplay.DiagramSetController.CanCopy(CurrentDisplay.SelectedShapes);
				copyShapeButton.Enabled = shapesOnly;
				copyAsImageToolStripMenuItem.Enabled = true;
				copyShapeOnlyMenuItem.Enabled = shapesOnly;
				copyShapeAndModelMenuItem.Enabled = shapesAndModels;
				// Paste
				pasteButton.Enabled =
				pasteMenuItem.Enabled = CurrentDisplay.DiagramSetController.CanPaste(CurrentDisplay.Diagram);
				// Delete
				shapesOnly =
				shapesAndModels = CurrentDisplay.DiagramSetController.CanDeleteShapes(CurrentDisplay.Diagram, CurrentDisplay.SelectedShapes);
				deleteShapeButton.Enabled = shapesOnly;
				deleteShapeOnlyMenuItem.Enabled = shapesOnly;
				deleteShapeAndModelMenuItem.Enabled = shapesAndModels;
				// ToForeGround / ToBackground
				toForegroundMenuItem.Enabled =
				toBackgroundMenuItem.Enabled = CurrentDisplay.DiagramSetController.CanLiftShapes(CurrentDisplay.Diagram, CurrentDisplay.SelectedShapes);
				// Selection
				selectAllToolStripMenuItem.Enabled = (CurrentDisplay.Diagram.Shapes.Count > 0
													&& CurrentDisplay.SelectedShapes.Count < CurrentDisplay.Diagram.Shapes.Count);
				unselectAllToolStripMenuItem.Enabled = (CurrentDisplay.SelectedShapes.Count > 0);
				selectAllOfTypeToolStripMenuItem.Enabled = (CurrentDisplay.SelectedShapes.Count == 1);
				selectAllOfTemplateToolStripMenuItem.Enabled = (CurrentDisplay.SelectedShapes.Count == 1 && CurrentDisplay.SelectedShapes.TopMost.Template != null);
			} else {
				// Cut
				cutShapeButton.Enabled =
				cutShapeOnlyMenuItem.Enabled =
				cutShapeAndModelMenuItem.Enabled = false;
				// Copy
				shapesOnly =
				shapesAndModels =
				copyShapeButton.Enabled =
				copyAsImageToolStripMenuItem.Enabled =
				copyShapeOnlyMenuItem.Enabled =
				copyShapeAndModelMenuItem.Enabled = false;
				// Paste
				pasteButton.Enabled =
				pasteMenuItem.Enabled = false;
				// Delete
				shapesOnly =
				shapesAndModels =
				deleteShapeButton.Enabled =
				deleteShapeOnlyMenuItem.Enabled =
				deleteShapeAndModelMenuItem.Enabled = false;
				// ToForeGround / ToBackground
				toForegroundMenuItem.Enabled =
				toBackgroundMenuItem.Enabled = false;
				// Selection
				selectAllToolStripMenuItem.Enabled =
				unselectAllToolStripMenuItem.Enabled =
				selectAllOfTypeToolStripMenuItem.Enabled =
				selectAllOfTemplateToolStripMenuItem.Enabled = false;
			}
		}


		private void UpdateZoomControl() {
			int cursorPos = -1;
			if (zoomToolStripComboBox.Focused)
				cursorPos = zoomToolStripComboBox.SelectionStart;

			string txt = string.Format(PercentFormatStr, _currentDisplay.ZoomFactor * 100);
			if (txt != zoomToolStripComboBox.Text)
				zoomToolStripComboBox.Text = txt;

			if (zoomToolStripComboBox.Focused)
				zoomToolStripComboBox.SelectionStart = cursorPos;

			UpdateStatusInfo();
		}


		private void UpdateModelComponentsVisibility(bool visible) {
			if (modelTreeView.Visible != visible) modelTreeView.Visible = visible;
			if (visible) {
				if (!propertyWindowTabControl.TabPages.Contains(propertyWindowModelTab))
					propertyWindowTabControl.TabPages.Insert(1, propertyWindowModelTab);
			} else {
				if (propertyWindowTabControl.TabPages.Contains(propertyWindowModelTab))
					propertyWindowTabControl.TabPages.Remove(propertyWindowModelTab);
			}
		}

		#endregion


		#region [Private] Methods: Manage Display Controls

		/// <summary>
		/// Creates a ownerDisplay for each diagram in the project and a default one if there isn't any.
		/// </summary>
		private void DisplayDiagrams(bool isNewProject) {
			// Display all diagrams of the project
			bool diagramAdded = false;
			foreach (Diagram diagram in project.Repository.GetDiagrams()) {
				DisplayTabPage displayTabPage = CreateDiagramTabPage(diagram, !diagramAdded);
				displayTabControl.TabPages.Add(displayTabPage);
				if (!diagramAdded) diagramAdded = true;
			}
			// If the project has no diagram, create the a new one.
			if (!diagramAdded && isNewProject) {
				Diagram diagram = new Diagram(string.Format(DefaultDiagramNameFmt, displayTabControl.TabPages.Count + 1));
				project.Repository.InsertAll(diagram);
			}
			if (displayTabControl.TabCount > 0) {
				displayTabControl.SelectedIndex = 0;
				// Call selectedIndexChanged event handler immediately because otherwise it would be called 
				// when this method is completed (but we need a CurrentDisplay now)
				displaysTabControl_SelectedIndexChanged(displayTabControl, EventArgs.Empty);
				showDiagramSettingsToolStripMenuItem_Click(this, EventArgs.Empty);
			}
			UpdateAllMenuItems();
		}


		private Display CreateDiagramDisplay(Diagram diagram) {
			// Create a new ownerDisplay
			Display display = new Display();
			display.Name = string.Format("Display{0}", displayTabControl.TabCount + 1);
			display.BackColor = Color.DarkGray;
			display.HighQualityRendering = HighQuality;
			display.HighQualityBackground = HighQuality;
			display.IsGridVisible = ShowGrid;
			display.ZoomWithMouseWheel = MouseWheelZoom;
			display.IsUniversalScrollEnabled = UseUniversalScroll;
			display.PanMouseButton = PanButtons;
			display.ScrollBarsVisible = ShowScrollBars;
			display.IsSheetVisible = ShowDiagramSheet;
			display.CloseCaptionEditorWithEnter = EnterClosesTextEditor;
			display.GridSize = GridSize;
			display.SnapToGrid = SnapToGrid;
			display.SnapDistance = SnapDistance;
			display.GripSize = ControlPointSize;
			display.ResizeGripShape = ResizePointShape;
			display.ConnectionPointShape = ConnectionPointShape;
			display.ZoomLevelHD = ZoomHD;
#if DEBUG_UI
			display.IsDebugInfoCellOccupationVisible = ShowCellOccupation;
			display.IsDebugInfoInvalidateVisible = ShowInvalidatedAreas;
#endif
			display.Dock = DockStyle.Fill;
			//
			// Assign DiagramSetController and diagram
			display.PropertyController = propertyController;
			display.DiagramSetController = diagramSetController;
			display.Diagram = diagram;
			display.ActiveTool = toolSetController.SelectedTool;
			display.UserMessage += display_UserMessage;
			//
			return display;
		}


		private DisplayTabPage CreateDiagramTabPage(Diagram diagram, bool createDisplay) {
			DisplayTabPage tabPage = new DisplayTabPage(diagram.Title);
			tabPage.Tag = diagram;
			if (createDisplay)
				tabPage.Display = CreateDiagramDisplay(diagram);
			return tabPage;
		}


		private void RemoveDisplayOfDiagram(Diagram diagram) {
			DisplayTabPage tabPage = FindDisplayTabPage(diagram);
			if (tabPage != null) {
				displayTabControl.TabPages.Remove(tabPage);
				tabPage.Dispose();
			}
			UpdateAllMenuItems();
		}


		private DisplayTabPage FindDisplayTabPage(Diagram diagram) {
			foreach (TabPage tabPage in displayTabControl.TabPages) {
				if (tabPage is DisplayTabPage && tabPage.Tag == diagram)
					return (DisplayTabPage)tabPage;
			}
			return null;
		}

		#endregion


		#region [Private] Methods: Register event handlers

		private void RegisterRepositoryEvents() {
			if (project.Repository != null) {
				// Diagram events
				project.Repository.DiagramInserted += repository_DiagramInserted;
				project.Repository.DiagramUpdated += repository_DiagramUpdated;
				project.Repository.DiagramDeleted += repository_DiagramDeleted;
				// ModelObject events
				project.Repository.ModelObjectsInserted += repository_ModelObjectsInsertedOrDeleted;
				project.Repository.ModelObjectsDeleted += repository_ModelObjectsInsertedOrDeleted;

				// Event handlers used for detecting changes
				project.Repository.ConnectionDeleted += Repository_ConnectionModified;
				project.Repository.ConnectionInserted += Repository_ConnectionModified;
				project.Repository.DesignDeleted += Repository_DesignModified;
				project.Repository.DesignInserted += Repository_DesignModified;
				project.Repository.DesignUpdated += Repository_DesignModified;
				project.Repository.ModelDeleted += Repository_ModelModified;
				project.Repository.ModelInserted += Repository_ModelModified;
				project.Repository.ModelUpdated += Repository_ModelModified;
				project.Repository.ModelMappingsDeleted += Repository_ModelMappingsModified;
				project.Repository.ModelMappingsInserted += Repository_ModelMappingsModified;
				project.Repository.ModelMappingsUpdated += Repository_ModelMappingsModified;
				project.Repository.ModelObjectsUpdated += Repository_ModelObjectsModified;
				project.Repository.ProjectUpdated += Repository_ProjectModified;
				project.Repository.ShapesDeleted += Repository_ShapesModified;
				project.Repository.ShapesInserted += Repository_ShapesModified;
				project.Repository.ShapesUpdated += Repository_ShapesModified;
				project.Repository.StyleDeleted += Repository_StyleModified;
				project.Repository.StyleInserted += Repository_StyleModified;
				project.Repository.StyleUpdated += Repository_StyleModified;
				project.Repository.TemplateDeleted += Repository_TemplateModified;
				project.Repository.TemplateInserted += Repository_TemplateModified;
				project.Repository.TemplateShapeReplaced += Repository_TemplateModified;
				project.Repository.TemplateUpdated += Repository_TemplateModified;
			}
		}


		private void UnregisterRepositoryEvents() {
			if (project.Repository != null) {
				// Diagram events
				project.Repository.DiagramInserted -= repository_DiagramInserted;
				project.Repository.DiagramUpdated -= repository_DiagramUpdated;
				project.Repository.DiagramDeleted -= repository_DiagramDeleted;

				// ModelObject events
				project.Repository.ModelObjectsInserted -= repository_ModelObjectsInsertedOrDeleted;
				project.Repository.ModelObjectsDeleted -= repository_ModelObjectsInsertedOrDeleted;

				// Event handlers used for detecting changes
				project.Repository.ConnectionDeleted -= Repository_ConnectionModified;
				project.Repository.ConnectionInserted -= Repository_ConnectionModified;
				project.Repository.DesignDeleted -= Repository_DesignModified;
				project.Repository.DesignInserted -= Repository_DesignModified;
				project.Repository.DesignUpdated -= Repository_DesignModified;
				project.Repository.ModelDeleted -= Repository_ModelModified;
				project.Repository.ModelInserted -= Repository_ModelModified;
				project.Repository.ModelUpdated -= Repository_ModelModified;
				project.Repository.ModelMappingsDeleted -= Repository_ModelMappingsModified;
				project.Repository.ModelMappingsInserted -= Repository_ModelMappingsModified;
				project.Repository.ModelMappingsUpdated -= Repository_ModelMappingsModified;
				project.Repository.ModelObjectsUpdated -= Repository_ModelObjectsModified;
				project.Repository.ProjectUpdated -= Repository_ProjectModified;
				project.Repository.ShapesDeleted -= Repository_ShapesModified;
				project.Repository.ShapesInserted -= Repository_ShapesModified;
				project.Repository.ShapesUpdated -= Repository_ShapesModified;
				project.Repository.StyleDeleted -= Repository_StyleModified;
				project.Repository.StyleInserted -= Repository_StyleModified;
				project.Repository.StyleUpdated -= Repository_StyleModified;
				project.Repository.TemplateDeleted -= Repository_TemplateModified;
				project.Repository.TemplateInserted -= Repository_TemplateModified;
				project.Repository.TemplateShapeReplaced -= Repository_TemplateModified;
				project.Repository.TemplateUpdated -= Repository_TemplateModified;
			}
		}


		private void RegisterDisplayEvents(Display display) {
			if (display != null) {
				display.Scroll += display_Scroll;
				display.Resize += display_Resize;
				display.MouseMove += display_MouseMove;
				display.ShapesSelected += display_ShapesSelected;
				display.ShapesInserted += display_ShapesInserted;
				display.ShapesRemoved += display_ShapesRemoved;
				display.ZoomChanged += display_ZoomChanged;
			}
		}


		private void UnregisterDisplayEvents(Display display) {
			if (display != null) {
				display.Scroll -= display_Scroll;
				display.Resize -= display_Resize;
				display.MouseMove -= display_MouseMove;
				display.ShapesSelected -= display_ShapesSelected;
				display.ShapesInserted -= display_ShapesInserted;
				display.ShapesRemoved -= display_ShapesRemoved;
				display.ZoomChanged -= display_ZoomChanged;
			}
		}


		private void RegisterPropertyControllerEvents() {
			propertyController.ObjectsSet += propertyController_ObjectsSet;
		}


		private void UnregisterPropertyControllerEvents() {
			propertyController.ObjectsSet -= propertyController_ObjectsSet;
		}

		#endregion


		#region [Private] Event Handler implementations - Project, History and Repository

		private void project_LibraryLoaded(object sender, LibraryLoadedEventArgs e) {
			// nothing to do here...
		}


		private void project_Opened(object sender, EventArgs e) {
			RegisterRepositoryEvents();
			RegisterPropertyControllerEvents();
			// Set main form title
			Text = string.Format("{0} - {1}", AppTitle, project.Name);
			// Hide/Show ModelTreeView
			UpdateModelComponentsVisibility(false);
			foreach (IModelObject m in project.Repository.GetModelObjects(null)) {
				UpdateModelComponentsVisibility(true);
				break;
			}
		}


		private void project_Closed(object sender, EventArgs e) {
			if (_layoutControlForm != null) _layoutControlForm.Close();

			UnregisterPropertyControllerEvents();
			UnregisterRepositoryEvents();

			historyTrackBar.Maximum = project.History.UndoCommandCount + project.History.RedoCommandCount;
			Text = AppTitle;
			statusLabelMessage.Text =
			statusLabelMousePosition.Text = string.Empty;
		}


		private void history_CommandAdded(object sender, CommandEventArgs e) {
			UpdateUndoMenuItems();
			if (CurrentDisplay != null) {
				if (project.History.UndoCommandCount + project.History.RedoCommandCount != historyTrackBar.Maximum)
					historyTrackBar.Maximum = project.History.UndoCommandCount + project.History.RedoCommandCount;
				_currHistoryPos = 0;
				historyTrackBar.Value = 0;
			}
		}


		private void history_CommandsExecuted(object sender, CommandsEventArgs e) {
			UpdateUndoMenuItems();
			try {
				historyTrackBar.ValueChanged -= historyTrackBar_ValueChanged;
				if (e.Reverted) {
					if (_currHistoryPos != historyTrackBar.Value) {
						_currHistoryPos += e.Commands.Count;
						historyTrackBar.Value += e.Commands.Count;
					}
				} else {
					if (_currHistoryPos > 0) _currHistoryPos -= e.Commands.Count;
					if (historyTrackBar.Value > 0) historyTrackBar.Value -= e.Commands.Count;
				}
			} finally {
				historyTrackBar.ValueChanged += historyTrackBar_ValueChanged;
			}
		}


		private void repository_DiagramDeleted(object sender, RepositoryDiagramEventArgs e) {
			RemoveDisplayOfDiagram(e.Diagram);
			_projectIsModified = true;
		}


		private void repository_DiagramUpdated(object sender, RepositoryDiagramEventArgs e) {
			DisplayTabPage tabPage = FindDisplayTabPage(e.Diagram);
			if (tabPage != null) tabPage.Text = e.Diagram.Title;
			UpdateStatusInfo();
			_projectIsModified = true;
		}


		private void repository_DiagramInserted(object sender, RepositoryDiagramEventArgs e) {
			DisplayTabPage tabPage = FindDisplayTabPage(e.Diagram);
			if (tabPage == null) {
				bool createDisplay = displayTabControl.TabCount == 0;
				displayTabControl.TabPages.Add(CreateDiagramTabPage(e.Diagram, createDisplay));
			}
			UpdateAllMenuItems();
			_projectIsModified = true;
		}


		private void repository_ModelObjectsInsertedOrDeleted(object sender, RepositoryModelObjectsEventArgs e) {
			bool modelExists = false;
			foreach (IModelObject modelObject in project.Repository.GetModelObjects(null)) {
				modelExists = true;
				break;
			}
			UpdateModelComponentsVisibility(modelExists);
			_projectIsModified = true;
		}


		private void Repository_TemplateModified(object sender, RepositoryTemplateEventArgs e) {
			_projectIsModified = true;
		}


		private void Repository_StyleModified(object sender, RepositoryStyleEventArgs e) {
			_projectIsModified = true;
		}


		private void Repository_ShapesModified(object sender, RepositoryShapesEventArgs e) {
			_projectIsModified = true;
		}


		private void Repository_ProjectModified(object sender, RepositoryProjectEventArgs e) {
			_projectIsModified = true;
		}


		private void Repository_ModelObjectsModified(object sender, RepositoryModelObjectsEventArgs e) {
			_projectIsModified = true;
		}


		private void Repository_ModelMappingsModified(object sender, RepositoryTemplateEventArgs e) {
			_projectIsModified = true;
		}


		private void Repository_ModelModified(object sender, RepositoryModelEventArgs e) {
			_projectIsModified = true;
		}


		private void Repository_DesignModified(object sender, RepositoryDesignEventArgs e) {
			_projectIsModified = true;
		}


		private void Repository_ConnectionModified(object sender, RepositoryShapeConnectionEventArgs e) {
			_projectIsModified = true;
		}

		#endregion


		#region [Private] Event Handler implementations - Display and Diagrams


		private void diagramSetController_SelectModelObjectsRequested(object sender, ModelObjectsEventArgs e) {
			// ToDo: Find the first display that contains the selected model object's shapes.
			// Problem: 
			// As there is only one propertyController for all displays and the displays unselect all selected
			// shapes before selecting the shapes of the model objects, all selected objects of the property controller 
			// are unselected when processing the next display. We have to find a solution for this issue.
		}


		private void display_Resize(object sender, EventArgs e) {
			UpdateStatusInfo();
		}


		private void display_Scroll(object sender, ScrollEventArgs e) {
			UpdateStatusInfo();
		}


		private void display_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e) {
			int x = e.X, y = e.Y;
			if (CurrentDisplay != null)
				CurrentDisplay.ControlToDiagram(e.X, e.Y, out x, out y);
			DisplayMouseCoordinates(x, y);
		}


		private void display_DragOver(object sender, System.Windows.Forms.DragEventArgs e) {
			Point p = Point.Empty;
			p.Offset(e.X, e.Y);
			if (CurrentDisplay != null)
				CurrentDisplay.ScreenToDiagram(p, out p);
			DisplayMouseCoordinates(p.X, p.Y);
		}


		private void display_ShapesSelected(object sender, EventArgs e) {
			if (sender.Equals(CurrentDisplay)) {
				int cnt = CurrentDisplay.SelectedShapes.Count;
				if (cnt > 0)
					statusLabelMessage.Text = string.Format("{0} shape{1} selected.", cnt, cnt > 1 ? "s" : string.Empty);
				else
					statusLabelMessage.Text = string.Empty;
				UpdateAllMenuItems();
				UpdateStatusInfo();
				//
				if (_layoutControlForm != null)
					_layoutControlForm.SelectedShapes = CurrentDisplay.SelectedShapes;
			}
		}


		private void display_ShapesRemoved(object sender, DiagramPresenterShapesEventArgs e) {
			UpdateStatusInfo();
		}


		private void display_ShapesInserted(object sender, DiagramPresenterShapesEventArgs e) {
			UpdateStatusInfo();
		}


		private void display_ZoomChanged(object sender, EventArgs e) {
			UpdateZoomControl();
		}


		private void display_UserMessage(object sender, UserMessageEventArgs e) {
			MessageBoxIcon icon = MessageBoxIcon.Information;
			MessageBox.Show(this, e.MessageText, "Information", MessageBoxButtons.OK, icon);
		}

		#endregion


		#region [Private] Event Handler implementations - ModelTree

		private void modelTree_ModelObjectSelected(object sender, ModelObjectSelectedEventArgs eventArgs) {
			this.propertyWindowTabControl.SelectedTab = this.propertyWindowModelTab;
		}

		#endregion


		#region [Private] Event Handler implementations - ToolBox

		private void toolBoxAdapter_ShowTemplateEditorDialog(object sender, TemplateEditorEventArgs e) {
			_templateEditorDialog = new TemplateEditorDialog(e.Project, e.Template);
			_templateEditorDialog.Show(this);
		}


		private void toolBoxAdapter_ShowLibraryManagerDialog(object sender, EventArgs e) {
			LibraryManagementDialog dlg = new LibraryManagementDialog(project);
			dlg.Show(this);
		}


		private void toolBoxAdapter_ToolSelected(object sender, EventArgs e) {
			// ToDo:
			// If the ownerDisplay and the ToolBox are not connected directly, one can handle this event and 
			// assign the SelectedTool as the DIsplay's CurrentTool
		}


		private void toolBoxAdapter_ShowDesignEditor(object sender, System.EventArgs e) {
			DesignEditorDialog dlg = new DesignEditorDialog(project);
			dlg.Show(this);
		}

		#endregion


		#region [Private] Event Handler implementations - Misc

		private void DiagramDesignerMainForm_Load(object sender, EventArgs e) {
			try {
				// Read config file
				_configFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), _nShapeConfigDirectory);
				if (!Directory.Exists(_configFolder)) Directory.CreateDirectory(_configFolder);
				// Init history controls
				historyTrackBar.Minimum = 0;
				historyTrackBar.Maximum = 0;
				historyTrackBar.Value = 0;
				historyTrackBar.TickStyle = System.Windows.Forms.TickStyle.BottomRight;
				historyTrackBar.TickFrequency = 1;
				_currHistoryPos = historyTrackBar.Value;
				project.History.CommandAdded += history_CommandAdded;
				project.History.CommandsExecuted += history_CommandsExecuted;
				diagramSetController.SelectModelObjectsRequested += diagramSetController_SelectModelObjectsRequested;

				// Deactivate "View Help" menu item if help file cannot be found
				viewHelpToolStripMenuItem.Enabled = false;
				DirectoryInfo helpDir = GetHelpDir();
				if (!helpDir.Exists)
					viewHelpToolStripMenuItem.ToolTipText = String.Format("Help file directory '{0}' does not exist (or is not accessible).", helpDir.FullName);
				else {
					FileInfo helpFile = GetHelpFile();
					if (helpFile != null && helpFile.Exists)
						viewHelpToolStripMenuItem.Enabled = true;
					else
						viewHelpToolStripMenuItem.ToolTipText = "Help file does not exist (or is not accessible).";
				}

				// Add library load support
				project.AutoLoadLibraries = true;
				project.LibrarySearchPaths.Add(Application.StartupPath);

				XmlDocument config = ReadConfigFile();
				ReadConfig(config);

				MaintainRecentProjects();
				CreateRecentProjectsMenuItems();

				// Get command line parameters and check if a repository should be loaded on startup
				RepositoryInfo repositoryInfo = RepositoryInfo.Empty;
				string[] commandLineArgs = Environment.GetCommandLineArgs();
				if (commandLineArgs != null) {
					int cnt = commandLineArgs.Length;
					foreach (string arg in commandLineArgs) {
						Debug.Print(arg);
						string path = Path.GetFullPath(arg);
						if (string.IsNullOrEmpty(path)) continue;
						else {
							if (path == Path.GetFullPath(Application.ExecutablePath))
								continue;
							// Check if the file is an xml document
							if (File.Exists(path)) {
								TextReader reader = null;
								try {
									reader = File.OpenText(path);
									if (reader.ReadLine().Contains("xml")) {
										repositoryInfo.computerName = Environment.MachineName;
										repositoryInfo.location = path;
										repositoryInfo.projectName = Path.GetFileNameWithoutExtension(path);
										repositoryInfo.typeName = RepositoryInfo.XmlStoreTypeName;
									}
								} finally {
									if (reader != null) {
										reader.Close();
										reader.Dispose();
										reader = null;
									}
								}
							}
						}
					}
				}

				XmlStore store;
				if (repositoryInfo != RepositoryInfo.Empty) {
					store = new XmlStore() {
						DirectoryName = Path.GetDirectoryName(repositoryInfo.location),
						FileExtension = Path.GetExtension(repositoryInfo.location),
						LazyLoading = true
					};
					OpenProject(repositoryInfo.projectName, store);
				} else {
					NewXmlRepositoryProject(false);
					project.AddLibraryByName("Dataweb.NShape.GeneralShapes", true);
#if DEBUG
					// Shape libraries
					//project.AddLibraryByName("Dataweb.NShape.SoftwareArchitectureShapes");
					//project.AddLibraryByName("Dataweb.NShape.FlowChartShapes");
					//project.AddLibraryByName("Dataweb.NShape.ElectricalShapes");
					// ModelObjectTypes libraries
					//project.AddLibraryByFilePath("Dataweb.NShape.GeneralModelObjects.dll");
#endif
					// Mark project as "Not modified" in order to suppress the "Do you want to save changes" question
					_projectIsModified = false;
				}

				UpdateAllMenuItems();
				UpdateStatusInfo();

				// Setting the form's WindowState will show the form immediately, so we have to perform this step
				// after all initialization is done.
				ReadWindowConfig(config);
			} catch (Exception ex) {
				MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}


		private void DiagramDesignerMainForm_Shown(object sender, System.EventArgs e) {
			CheckFrameworkVersion();
			CheckLibrariesLoaded();
		}


		private void DiagramDesignerMainForm_FormClosing(object sender, FormClosingEventArgs e) {
			if (!CloseProject()) e.Cancel = true;
			else SaveConfigFile();
		}


		private void displaysTabControl_SelectedIndexChanged(object sender, EventArgs e) {
			try {
				Cursor = Cursors.WaitCursor;
				DisplayTabPage tab = displayTabControl.SelectedTab as DisplayTabPage;
				if (tab != null) {
					// Create a display and load diagram if not done yet
					if (tab.Display == null) {
						Diagram diagram = (Diagram)tab.Tag;
						tab.Display = CreateDiagramDisplay(diagram);
					}
					CurrentDisplay = tab.Display;
					UpdateAllMenuItems();
				}
			} finally {
				Cursor = Cursors.Default;
			}
		}


		private void LayoutControlForm_LayoutChanged(object sender, EventArgs e) {
			// Why? Makes preview switch between original and new State.
			// currentDisplay.SaveChanges();
		}


		private void LayoutControlForm_FormClosed(object sender, FormClosedEventArgs e) {
			_layoutControlForm = null;
		}


		private void propertyController_ObjectsSet(object sender, PropertyControllerEventArgs e) {
			if (e.PageIndex == 0) {
				string typeName = GetCommonTypeName(e.Objects);
				propertyWindowTabControl.TabPages[e.PageIndex].Text = string.Format("{0}{1}", string.IsNullOrEmpty(typeName) ? "Properties" : typeName, (e.Objects.Count > 1) ? "s" : string.Empty);
			}
		}


		private string GetCommonTypeName(IEnumerable<object> objects) {
			Type type = null;
			foreach (object o in objects) {
				Type objType = null;
				if (o is Shape)
					objType = typeof(Shape);
				else if (o is IModelObject)
					objType = typeof(IModelObject);
				else objType = (o != null) ? o.GetType() : null;

				if (type != null) {
					if (type != objType)
						type = typeof(object);
				} else type = objType;
			}
			if (type == typeof(IModelObject))
				return "Model";
			else return (type != null) ? type.Name : string.Empty;
		}


		private void detailsToolStripMenuItem_Click(object sender, EventArgs e) {
			toolboxListView.View = View.Details;
		}


		private void largeIconsToolStripMenuItem_Click(object sender, EventArgs e) {
			toolboxListView.View = View.LargeIcon;
		}


		private void listToolStripMenuItem_Click(object sender, EventArgs e) {
			toolboxListView.View = View.List;
		}


		private void tilesToolStripMenuItem_Click(object sender, EventArgs e) {
			toolboxListView.View = View.Tile;
		}


		private void smallIconsToolStripMenuItem_Click(object sender, EventArgs e) {
			toolboxListView.View = View.SmallIcon;
		}

		#endregion


		#region [Private] Event Handler implementations - Toolbar

		private void refreshButton_Click(object sender, EventArgs e) {
			if (CurrentDisplay != null)
				CurrentDisplay.Refresh();
		}


		private void forwardButton_Click(object sender, EventArgs e) {
			if (displayTabControl.SelectedIndex < displayTabControl.TabPages.Count - 1)
				++displayTabControl.SelectedIndex;
		}


		private void backButton_Click(object sender, EventArgs e) {
			if (displayTabControl.SelectedIndex > 0)
				displayTabControl.SelectedIndex--;
		}


		private void undoToolStripSplitButton_DropDownOpening(object sender, EventArgs e) {
			undoToolStripSplitButton.DropDownItems.Clear();
			if (CurrentDisplay != null) {
				int nr = 0;
				foreach (string cmdDesc in project.History.GetUndoCommandDescriptions(historyDropDownItemCount)) {
					System.Windows.Forms.ToolStripItem item = new System.Windows.Forms.ToolStripMenuItem();
					item.Text = string.Format("{0}: {1}", ++nr, cmdDesc);
					item.Click += undoItem_Click;
					undoToolStripSplitButton.DropDownItems.Add(item);
				}
			}
		}


		private void redoToolStripSplitButton_DropDownOpening(object sender, EventArgs e) {
			redoToolStripSplitButton.DropDownItems.Clear();
			if (CurrentDisplay != null) {
				int nr = 0;
				foreach (string cmdDesc in project.History.GetRedoCommandDescriptions(historyDropDownItemCount)) {
					System.Windows.Forms.ToolStripItem item = new System.Windows.Forms.ToolStripMenuItem();
					item.Text = string.Format("{0}: {1}", ++nr, cmdDesc);
					item.Click += redoItem_Click;
					redoToolStripSplitButton.DropDownItems.Add(item);
				}
			}
		}


		private void zoomToolStripComboBox_SelectedIndexChanged(object sender, EventArgs e) {
			string zoomText = zoomToolStripComboBox.Text.Replace('%', ' ').Trim();
			float zoom;
			if (float.TryParse(zoomText, NumberStyles.Float, CultureInfo.InvariantCulture, out zoom) || float.TryParse(zoomText, NumberStyles.Float, CultureInfo.CurrentCulture, out zoom)) {
				ZoomHD = (int)Math.Round(zoom * Display.ZoomLevelHDFactor);
			}
		}


		private void ZoomToolStripComboBox_TextChanged(object sender, EventArgs e) {
			int cursorPos = -1;
			if (zoomToolStripComboBox.Focused)
				cursorPos = zoomToolStripComboBox.SelectionStart;

			string txt = null;
			if (zoomToolStripComboBox.Text.Contains("%"))
				txt = zoomToolStripComboBox.Text.Replace("%", string.Empty).Trim();
			else
				txt = zoomToolStripComboBox.Text.Trim();
			// Parse text and set zoom level
			float zoomInPercent;
			if (float.TryParse(txt, NumberStyles.Float, CultureInfo.InvariantCulture, out zoomInPercent)
					|| float.TryParse(txt, NumberStyles.Float, CultureInfo.CurrentCulture, out zoomInPercent))
				if (zoomInPercent > 0)
					ZoomHD = (int)Math.Round(zoomInPercent * Display.ZoomLevelHDFactor);

			if (zoomToolStripComboBox.Focused)
				zoomToolStripComboBox.SelectionStart = cursorPos;
		}


		private void runtimeModeButton_SelectedIndexChanged(object sender, EventArgs e) {
			((RoleBasedSecurityManager)project.SecurityManager).CurrentRoleName = runtimeModeComboBox.Text;
		}

		#endregion


		#region [Private] Event Handler implementations - Menu item "File"

		private void newXMLRepositoryToolStripMenuItem_Click(object sender, EventArgs e) {
			NewXmlRepositoryProject(true);
		}


		private void NewXmlRepositoryProject(bool askUserLoadLibraries) {
			if (CloseProject()) {
				//if (!Directory.Exists(xmlStoreDirectory)) {
				//    StringBuilder path = new StringBuilder(512);
				//    const int COMMON_DOCUMENTS = 0x002e;
				//    if (SHGetSpecialFolderPath(IntPtr.Zero, path, COMMON_DOCUMENTS, 0) != 0)
				//        xmlStoreDirectory = Path.Combine(Path.Combine(path.ToString(), "NShape"), "Demo Projects");
				//    else xmlStoreDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
				//}
				//XmlStore store = new XmlStore(xmlStoreDirectory, ProjectFileExtension);

				// Do not create a new XmlStore yet. This will be done when saving for the first time.
				CreateProject(NewProjectName, null, askUserLoadLibraries);
			}
		}


		private void newSQLServerRepositoryToolStripMenuItem_Click(object sender, EventArgs e) {
			string projectName;
			AdoNetStore store = GetAdoNetStore(out projectName, OpenAdoNetRepositoryDialog.Mode.CreateProject);
			if (store != null) CreateProject(projectName, store, true);
		}


		private AdoNetStore GetAdoNetStore() {
			string projectName;
			return GetAdoNetStore(out projectName, OpenAdoNetRepositoryDialog.Mode.CreateSchema);
		}


		private AdoNetStore GetAdoNetStore(out string projectName, OpenAdoNetRepositoryDialog.Mode mode) {
			projectName = string.Empty;
			AdoNetStore result = null;
			try {
				Cursor = Cursors.WaitCursor;
				Application.DoEvents();
				using (OpenAdoNetRepositoryDialog dlg = new OpenAdoNetRepositoryDialog(this, DefaultServerName, DefaultDatabaseName, mode)) {
					if (dlg.ShowDialog() == DialogResult.OK && CloseProject()) {
						if (dlg.ProviderName == SqlServerProviderName) {
							result = new SqlStore(dlg.ServerName, dlg.DatabaseName);
							projectName = dlg.ProjectName;
						} else MessageBox.Show(this, "Unsupported database repository.", "Unsupported repository", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}
			} finally { Cursor = Cursors.Default; }
			return result;
		}


		private void openXMLRepositoryToolStripMenuItem_Click(object sender, EventArgs e) {
			openFileDialog.Filter = FileFilterXmlRepository;
			if (Directory.Exists(_xmlStoreDirectory))
				openFileDialog.InitialDirectory = _xmlStoreDirectory;
			if (openFileDialog.ShowDialog() == DialogResult.OK && CloseProject()) {
				_xmlStoreDirectory = Path.GetDirectoryName(openFileDialog.FileName);
				XmlStore store = new XmlStore() {
					DirectoryName = _xmlStoreDirectory,
					FileExtension = Path.GetExtension(openFileDialog.FileName),
					LazyLoading = true
				};
				OpenProject(Path.GetFileNameWithoutExtension(openFileDialog.FileName), store);
			}
		}


		private void openSQLServerRepositoryToolStripMenuItem_Click(object sender, EventArgs e) {
			string projectName;
			AdoNetStore store = GetAdoNetStore(out projectName, OpenAdoNetRepositoryDialog.Mode.OpenProject);
			if (store != null) OpenProject(projectName, store);
		}


		private void openRecentProjectMenuItem_Click(object sender, EventArgs e) {
			Debug.Assert(sender is ToolStripItem && ((ToolStripItem)sender).Tag is RepositoryInfo);
			RepositoryInfo repositoryInfo = (RepositoryInfo)((ToolStripItem)sender).Tag;
			if (CloseProject()) {
				Store store = RepositoryInfo.CreateStore(repositoryInfo);
				if (store != null) OpenProject(repositoryInfo.projectName, store);
				else MessageBox.Show(this, string.Format("{0} repositories are not supported by this version.", repositoryInfo.typeName), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}


		private void saveAsToolStripMenuItem_Click(object sender, EventArgs e) {
			SaveProjectAs();
		}


		private void saveToolStripMenuItem_Click(object sender, EventArgs e) {
			SaveProject();
		}


		private void upgradeVersionMenuItem_Click(object sender, EventArgs e) {
			try {
				string msg = string.Format("Project '{0}' will be upgraded from file version {1} to {2}. "
					+ "{3}After upgrading, the project will be saved automatically.{3}Do you want to continue upgrading and saving the project?",
					project.Name, project.Repository.Version, _maxRepositoryVersion, Environment.NewLine);
				DialogResult res = MessageBox.Show(msg, "Upgrade and Save Project?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
				if (res == DialogResult.Yes) {
					// Upgrade repository version
					project.UpgradeVersion();
				}
			} catch (Exception exc) {
				MessageBox.Show(this, exc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}


		private void useEmbeddedImagesToolStripMenuItem_CheckedChanged(object sender, EventArgs e) {
			XmlStore xmlStore = cachedRepository.Store as XmlStore;
			if (xmlStore != null) {
				if (xmlStore.ImageLocation != XmlImageFileLocation)
					xmlStore.ImageLocation = XmlImageFileLocation;
			}
		}


		private void closeProjectToolStripMenuItem_Click(object sender, EventArgs e) {
			CloseProject();
		}


		private void ManageShapeAndModelLibrariesMenuItem_Click(object sender, EventArgs e) {
			using (LibraryManagementDialog dlg = new LibraryManagementDialog(project)) {
				dlg.StartPosition = FormStartPosition.CenterParent;
				dlg.ShowDialog(this);
			}
		}


		private void importRepositoryMenuItem_Click(object sender, System.EventArgs e) {
			ImportRepositoryFromClipboard();
		}


		private void exportRepositoryMenuItem_Click(object sender, System.EventArgs e) {
			ExportRepositoryToClipboard();
		}


		private void exportDiagramAsMenuItem_Click(object sender, EventArgs e) {
			using (ExportDiagramDialog dlg = new ExportDiagramDialog(CurrentDisplay))
				dlg.ShowDialog(this);
		}


		private void emfPlusFileToolStripMenuItem_Click(object sender, EventArgs e) {
			ExportMetaFile(ImageFileFormat.EmfPlus);
		}


		private void emfOnlyFileToolStripMenuItem_Click(object sender, EventArgs e) {
			ExportMetaFile(ImageFileFormat.Emf);
		}


		private void pngFileToolStripMenuItem_Click(object sender, EventArgs e) {
			ExportBitmapFile(ImageFileFormat.Png);
		}


		private void jpgFileToolStripMenuItem_Click(object sender, EventArgs e) {
			ExportBitmapFile(ImageFileFormat.Jpeg);
		}


		private void bmpFileToolStripMenuItem_Click(object sender, EventArgs e) {
			ExportBitmapFile(ImageFileFormat.Bmp);
		}


		private void quitToolStripMenuItem_Click(object sender, EventArgs e) {
			Application.Exit();
		}


		private Image GetImageFromDiagram(ImageFileFormat imageFormat) {
			Image result = null;
			Color backColor = Color.Transparent;
			if (CurrentDisplay.SelectedShapes.Count > 0)
				result = CurrentDisplay.Diagram.CreateImage(imageFormat, CurrentDisplay.SelectedShapes.BottomUp, CurrentDisplay.GetVisibleLayerIds(), CurrentDisplay.GridSize, false, backColor);
			else
				result = CurrentDisplay.Diagram.CreateImage(imageFormat, null, CurrentDisplay.GetVisibleLayerIds(), 0, true, backColor);
			return result;
		}


		[DllImport("gdi32.dll")]
		static extern IntPtr CopyEnhMetaFile(IntPtr hemfSrc, string fileName);

		[DllImport("gdi32.dll")]
		static extern bool DeleteEnhMetaFile(IntPtr hemf);

		private void ExportMetaFile(ImageFileFormat imageFormat) {
			string fileFilter = null;
			switch (imageFormat) {
				case ImageFileFormat.Emf:
				case ImageFileFormat.EmfPlus:
					fileFilter = "Enhanced Meta Files|*.emf|All Files|*.*"; break;
				default: throw new NShapeUnsupportedValueException(imageFormat);
			}
			saveFileDialog.Filter = fileFilter;
			if (saveFileDialog.ShowDialog() == DialogResult.OK) {
				string fileName = saveFileDialog.FileName;
				if (File.Exists(fileName)) File.Delete(fileName);

				using (Metafile image = GetImageFromDiagram(imageFormat) as Metafile) {
					if (image != null) {
						IntPtr hEmf = image.GetHenhmetafile();
						if (hEmf != IntPtr.Zero) {
							IntPtr resHEnh = CopyEnhMetaFile(hEmf, fileName);
							if (resHEnh != IntPtr.Zero)
								DeleteEnhMetaFile(resHEnh);
							DeleteEnhMetaFile(hEmf);
						}
					} else Debug.Fail("GetImageFromDiagram did not return a metafile image!");
				}
			}
		}


		private void ExportBitmapFile(ImageFileFormat imageFileFormat) {
			string fileFilter = null;
			ImageFormat imageFormat = null;
			switch (imageFileFormat) {
				case ImageFileFormat.Bmp:
					fileFilter = "Bitmap Picture Files|*.bmp|All Files|*.*";
					imageFormat = ImageFormat.Bmp;
					break;
				case ImageFileFormat.Gif:
					fileFilter = "Graphics Interchange Format Files|*.gif|All Files|*.*";
					imageFormat = ImageFormat.Gif;
					break;
				case ImageFileFormat.Jpeg:
					fileFilter = "Joint Photographic Experts Group (JPEG) Files|*.jpeg;*.jpg|All Files|*.*";
					imageFormat = ImageFormat.Jpeg;
					break;
				case ImageFileFormat.Png:
					fileFilter = "Portable Network Graphics Files|*.png|All Files|*.*";
					imageFormat = ImageFormat.Png;
					break;
				case ImageFileFormat.Tiff:
					fileFilter = "Tagged Image File Format Files|*.tiff;*.tif|All Files|*.*";
					imageFormat = ImageFormat.Tiff;
					break;
				default: throw new NShapeUnsupportedValueException(imageFileFormat);
			}
			saveFileDialog.Filter = fileFilter;
			if (saveFileDialog.ShowDialog() == DialogResult.OK) {
				string fileName = saveFileDialog.FileName;
				if (File.Exists(fileName)) File.Delete(fileName);

				using (Image image = GetImageFromDiagram(imageFileFormat)) {
					if (image != null) {
						ImageCodecInfo codecInfo = null;
						ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();
						foreach (ImageCodecInfo ici in encoders) {
							if (ici.FormatID.Equals(imageFormat.Guid)) {
								codecInfo = ici;
								break;
							}
						}
						EncoderParameters encoderParams = new EncoderParameters(3);
						encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.RenderMethod, (long)EncoderValue.RenderProgressive);
						// JPG specific encoder parameter
						encoderParams.Param[1] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)95);
						// TIFF specific encoder parameter
						encoderParams.Param[2] = new EncoderParameter(System.Drawing.Imaging.Encoder.Compression, (long)EncoderValue.CompressionLZW);

						image.Save(fileName, codecInfo, encoderParams);
					}
				}
			}
		}

		#endregion


		#region [Private] Event Handler implementations - Menu item "Edit"

		private void newDiagramToolStripMenuItem_Click(object sender, EventArgs e) {
			Diagram diagram = new Diagram(string.Format("Diagram {0}", displayTabControl.TabPages.Count + 1));
			diagram.Title = diagram.Name;
			ICommand cmd = new CreateDiagramCommand(diagram);
			project.ExecuteCommand(cmd);
			displayTabControl.SelectedTab = FindDisplayTabPage(diagram);
			showDiagramSettingsToolStripMenuItem_Click(this, EventArgs.Empty);
		}


		private void deleteDiagramToolStripMenuItem_Click(object sender, EventArgs e) {
			Diagram diagram = CurrentDisplay.Diagram;
			ICommand cmd = new DeleteDiagramCommand(diagram);
			project.ExecuteCommand(cmd);

			// Try to remove Display (in case the Repository-Event was not handled)
			RemoveDisplayOfDiagram(diagram);
		}


		private void copyShapeOnlyItem_Click(object sender, EventArgs e) {
			CurrentDisplay.Copy(false);
			UpdateAllMenuItems();
		}


		private void copyShapeAndModelItem_Click(object sender, EventArgs e) {
			CurrentDisplay.Copy(true);
			UpdateAllMenuItems();
		}


		private void copyAsImageToolStripMenuItem_Click(object sender, EventArgs e) {
			if (CurrentDisplay != null && CurrentDisplay.Diagram != null) {
				// Copy image as PNG and EMFPlusDual formats to clipboard
				CurrentDisplay.CopyImagesToClipboard(CurrentDisplay.GetVisibleLayerIds(), false);
			}
		}


		private void cutShapeOnlyItem_Click(object sender, EventArgs e) {
			CurrentDisplay.Cut(false);
			UpdateAllMenuItems();
		}


		private void cutShapeAndModelItem_Click(object sender, EventArgs e) {
			CurrentDisplay.Cut(true);
			UpdateAllMenuItems();
		}


		private void pasteMenuItem_Click(object sender, EventArgs e) {
			CurrentDisplay.Paste(CurrentDisplay.GridSize, CurrentDisplay.GridSize);
			UpdateAllMenuItems();
		}


		private void deleteShapeAndModelItem_Click(object sender, EventArgs e) {
			CurrentDisplay.DeleteShapes(true);
			UpdateAllMenuItems();
		}


		private void deleteShapeOnlyItem_Click(object sender, EventArgs e) {
			CurrentDisplay.DeleteShapes(false);
			UpdateAllMenuItems();
		}


		private void selectAllToolStripMenuItem_Click(object sender, EventArgs e) {
			if (CurrentDisplay != null && CurrentDisplay.Diagram != null)
				CurrentDisplay.SelectAll();
		}


		private void selectAllShapesOfTheSameTypeToolStripMenuItem_Click(object sender, EventArgs e) {
			if (CurrentDisplay != null && CurrentDisplay.Diagram != null && CurrentDisplay.SelectedShapes.Count > 0) {
				Shape refShape = CurrentDisplay.SelectedShapes.TopMost;
				foreach (Shape s in CurrentDisplay.Diagram.Shapes) {
					if (s == refShape) continue;
					if (s.Type == refShape.Type)
						CurrentDisplay.SelectShape(s, true);
				}
			}
		}


		private void selectAllShapesOfTheSameTemplateToolStripMenuItem_Click(object sender, EventArgs e) {
			if (CurrentDisplay != null && CurrentDisplay.Diagram != null && CurrentDisplay.SelectedShapes.Count > 0) {
				Shape refShape = CurrentDisplay.SelectedShapes.TopMost;
				foreach (Shape s in CurrentDisplay.Diagram.Shapes) {
					if (s == refShape) continue;
					if (s.Template == refShape.Template)
						CurrentDisplay.SelectShape(s, true);
				}
			}
		}


		private void unselectAllToolStripMenuItem_Click(object sender, EventArgs e) {
			if (CurrentDisplay != null && CurrentDisplay.Diagram != null)
				CurrentDisplay.UnselectAll();
		}


		private void toForegroundMenuItem_Click(object sender, System.EventArgs e) {
			CurrentDisplay.DiagramSetController.LiftShapes(CurrentDisplay.Diagram, CurrentDisplay.SelectedShapes, ZOrderDestination.ToTop);
			UpdateAllMenuItems();
		}


		private void toBackgroundMenuItem_Click(object sender, System.EventArgs e) {
			CurrentDisplay.DiagramSetController.LiftShapes(CurrentDisplay.Diagram, CurrentDisplay.SelectedShapes, ZOrderDestination.ToBottom);
			UpdateAllMenuItems();
		}


		private void historyTrackBar_ValueChanged(object sender, EventArgs e) {
			int d = _currHistoryPos - historyTrackBar.Value;
			bool commandExecuted = false;
			try {
				//project.History.CommandExecuted -= history_CommandExecuted;
				project.History.CommandsExecuted -= history_CommandsExecuted;

				if (d != 0) {
					if (d < 0) project.History.Undo(d * (-1));
					else if (d > 0) project.History.Redo(d);
					commandExecuted = true;
				}
			} catch (NShapeSecurityException exc) {
				MessageBox.Show(this, exc.Message, "Command execution failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
				commandExecuted = false;
			} catch (Exception exc) {
				Debug.Fail(exc.Message);
				throw;
			} finally {
				//project.History.CommandExecuted += history_CommandExecuted;
				project.History.CommandsExecuted += history_CommandsExecuted;
			}

			if (commandExecuted)
				_currHistoryPos = historyTrackBar.Value;
			else if (historyTrackBar.Value != _currHistoryPos)
				historyTrackBar.Value = _currHistoryPos;
			UpdateAllMenuItems();
		}


		private void undoButton_Click(object sender, EventArgs e) {
			if (historyTrackBar.Value < historyTrackBar.Maximum)
				historyTrackBar.Value += 1;
			if (sender is ToolStripItem) ((ToolStripItem)sender).Invalidate();
		}


		private void redoButton_Click(object sender, EventArgs e) {
			if (historyTrackBar.Value > historyTrackBar.Minimum)
				historyTrackBar.Value -= 1;
			if (sender is ToolStripItem) ((ToolStripItem)sender).Invalidate();
		}


		private void undoItem_Click(object sender, EventArgs e) {
			System.Windows.Forms.ToolStripSplitButton button = (System.Windows.Forms.ToolStripSplitButton)((System.Windows.Forms.ToolStripItem)sender).OwnerItem;
			// Undo was executed from the main menu (DropDownList)
			if (button != null) {
				int idx = button.DropDownItems.IndexOf((System.Windows.Forms.ToolStripMenuItem)sender);
				historyTrackBar.Value += idx + 1;
			} else
				// Undo was executed from context menu
				historyTrackBar.Value += 1;
		}


		private void redoItem_Click(object sender, EventArgs e) {
			System.Windows.Forms.ToolStripSplitButton button = (System.Windows.Forms.ToolStripSplitButton)((System.Windows.Forms.ToolStripItem)sender).OwnerItem;
			// Redo was executed from the main menu (DropDownList)
			if (button != null) {
				int idx = button.DropDownItems.IndexOf((System.Windows.Forms.ToolStripMenuItem)sender);
				historyTrackBar.Value -= idx + 1;
			} else
				// Redo was executed from context menu
				historyTrackBar.Value -= 1;
		}

		#endregion


		#region [Private] Event Handler implementations - Menu Item "View"

		private void showGridToolStripMenuItem_Click(object sender, EventArgs e) {
			bool isChecked = false;
			if (sender is System.Windows.Forms.ToolStripMenuItem)
				isChecked = ((System.Windows.Forms.ToolStripMenuItem)sender).Checked;
			else if (sender is System.Windows.Forms.ToolStripButton)
				isChecked = ((System.Windows.Forms.ToolStripButton)sender).Checked;
			ShowGrid = isChecked;
			showGridMenuItem.Checked = isChecked;
			showGridToolbarButton.Checked = isChecked;
		}


		private void debugDrawOccupationToolbarButton_Click(object sender, EventArgs e) {
#if DEBUG_UI
			ShowCellOccupation = debugDrawOccupationToolbarButton.Checked;
#endif
		}


		private void debugDrawInvalidatedAreaToolbarButton_Click(object sender, EventArgs e) {
#if DEBUG_UI
			ShowInvalidatedAreas = debugDrawInvalidatedAreaToolbarButton.Checked;
#endif
		}


		private void showDisplaySettingsItem_Click(object sender, EventArgs e) {
			using (DisplaySettingsForm dlg = new DisplaySettingsForm(this)) {
				dlg.ShowGrid = ShowGrid;
				dlg.MouseWheelZoom = MouseWheelZoom;
				dlg.ShowScrollBars = ShowScrollBars;
				dlg.ShowDiagramSheet = ShowDiagramSheet;
				dlg.EnterClosesTextEditor = EnterClosesTextEditor;
				dlg.SnapToGrid = SnapToGrid;
				dlg.GridColor = GridColor;
				dlg.GridSize = GridSize;
				dlg.SnapDistance = SnapDistance;
				dlg.ResizePointShape = ResizePointShape;
				dlg.ConnectionPointShape = ConnectionPointShape;
				dlg.ControlPointSize = ControlPointSize;
				dlg.ShowDefaultContextMenu = ShowDefaultContextMenu;
				dlg.HideDeniedMenuItems = HideDeniedMenuItems;

				if (dlg.ShowDialog(this) == DialogResult.OK) {
					MouseWheelZoom = dlg.MouseWheelZoom;
					ShowScrollBars = dlg.ShowScrollBars;
					ShowDiagramSheet = dlg.ShowDiagramSheet;
					EnterClosesTextEditor = dlg.EnterClosesTextEditor;
					ShowGrid = dlg.ShowGrid;
					GridColor = dlg.GridColor;
					showGridMenuItem.Checked = ShowGrid;
					showGridToolbarButton.Checked = ShowGrid;
					SnapToGrid = dlg.SnapToGrid;
					GridSize = dlg.GridSize;
					SnapDistance = dlg.SnapDistance;
					ResizePointShape = dlg.ResizePointShape;
					ConnectionPointShape = dlg.ConnectionPointShape;
					ControlPointSize = dlg.ControlPointSize;
					ShowDefaultContextMenu = dlg.ShowDefaultContextMenu;
					HideDeniedMenuItems = dlg.HideDeniedMenuItems;
				}
			}
		}


		private void showDiagramSettingsToolStripMenuItem_Click(object sender, EventArgs e) {
			propertyController.SetObject(0, CurrentDisplay.Diagram);
		}


		private void editDesignsAndStylesToolStripMenuItem_Click(object sender, EventArgs e) {
			DesignEditorDialog dlg = new DesignEditorDialog(project);
			dlg.Show(this);
		}


		private void viewShowLayoutControlToolStripMenuItem_Click(object sender, EventArgs e) {
			if (_layoutControlForm == null) {
				LayoutDialog lcf = new LayoutDialog();
				lcf.Project = CurrentDisplay.Project;
				lcf.Diagram = CurrentDisplay.Diagram;
				lcf.SelectedShapes = CurrentDisplay.SelectedShapes;
				lcf.FormClosed += LayoutControlForm_FormClosed;
				lcf.LayoutChanged += LayoutControlForm_LayoutChanged;
				lcf.Show(this);
				_layoutControlForm = lcf;
			} else {
				_layoutControlForm.Activate();
			}
		}


		private void highQualityToolStripMenuItem_Click(object sender, EventArgs e) {
			HighQuality = !HighQuality;
			highQualityRenderingMenuItem.Checked = HighQuality;
		}


		private void resetToolbarsToolStripMenuItem_Click(object sender, EventArgs e) {
			DialogResult res;
			string areYouSureQuestion = "Do you really want to reset the layout of all toolbars to default?";
			res = MessageBox.Show(this, areYouSureQuestion, "Reset Toolbar Layout", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
			if (res == DialogResult.Yes) {
				// Store setting "Do no load layout at next start"
				_loadToolStripLayoutOnStartup = false;
				// Ask for restart
				string msgTxt = "Resetting the toolbar layout requires restarting the application. \nDo want to restart now?";
				MessageBox.Show(this, msgTxt, "Restart Required", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
				if (res == System.Windows.Forms.DialogResult.Yes) {
					if (CloseProject()) {
						try {
							const string quotedPathFmt = "\"{0}\"";
							ProcessStartInfo startInfo = new ProcessStartInfo(string.Format(quotedPathFmt, Application.ExecutablePath));
							if (cachedRepository.Store is XmlStore) startInfo.Arguments = string.Format(quotedPathFmt, ((XmlStore)cachedRepository.Store).ProjectFilePath);

							// Save settings, start the application and end this instance
							SaveConfigFile();
							Application.Exit();

							Process.Start(startInfo);
						} catch (Exception exc) {
							MessageBox.Show(exc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
						}
					}
				} else MessageBox.Show(this, "The toolbar layout will be reset when restarting the application.", "Reset Toolbar Layout", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}

		#endregion


		#region [Private] Event Handler implementations - Menu item "Tools"

		private void adoNetDatabaseGeneratorToolStripMenuItem_Click(object sender, EventArgs e) {
			if (project.Repository == null)
				MessageBox.Show(this, "No repository set.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			else if (!(project.Repository is CachedRepository))
				MessageBox.Show(this, string.Format("Repositories of type '{0}' are not supported by the database generator.", project.Repository.GetType().Name), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			else {
				if (project.IsOpen) {
					string msgStr = "You are about to create a new database schema for a NShape database repository." + Environment.NewLine + Environment.NewLine;
					msgStr += "If you proceed, the current project will be closed and you will be asked for a database server and for choosing a set of NShape libraries." + Environment.NewLine;
					msgStr += "You can not save projects in the database using other than the selected libraries." + Environment.NewLine + Environment.NewLine;
					msgStr += "Do you want to proceed?";
					DialogResult result = MessageBox.Show(this, msgStr, "Create ADO.NET database schema", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
					if (result != DialogResult.Yes) return;
					if (!CloseProject()) return;
				}
				AdoNetStore store = GetAdoNetStore();
				if (store != null) {
					CachedRepository cachedReporitory = (CachedRepository)project.Repository;
					cachedReporitory.Store = store;
					project.RemoveAllLibraries();
					using (LibraryManagementDialog dlg = new LibraryManagementDialog(project))
						dlg.ShowDialog(this);
					project.RegisterEntityTypes();

					Cursor = Cursors.WaitCursor;
					Application.DoEvents();
					try {
						store.DropDbSchema();
						store.CreateDbCommands(cachedReporitory);
						store.CreateDbSchema(cachedReporitory);
						project.Close();
						MessageBox.Show(this, "Database schema created successfully.", "Schema created", MessageBoxButtons.OK, MessageBoxIcon.Information);
					} catch (Exception exc) {
						MessageBox.Show(this, "An error occured while creating database schema:" + Environment.NewLine + exc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					} finally {
						Cursor = Cursors.Default;
					}
				}
			}
		}


		private void testDataGeneratorToolStripMenuItem_Click(object sender, EventArgs e) {
			try {
				if (project.ShapeTypes["RoundedBox"] == null || project.ShapeTypes["Polyline"] == null)
					MessageBox.Show(this, "Shape library 'GeneralShapes' is required for creating test data.", "Library missing", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				else {
					using (TestDataGeneratorDialog dlg = new TestDataGeneratorDialog(project)) {
						DialogResult res = dlg.ShowDialog(this);
						if (res == DialogResult.OK) {
							Refresh();
							try {
								Cursor = Cursors.WaitCursor;

								// Create test diagram
								TestDataGenerator.CreateDiagram(project,
									dlg.DiagramName,
									dlg.ShapeSize,
									dlg.ShapesPerRow,
									dlg.ShapesPerColumn,
									dlg.ConnectShapes,
									dlg.CreateShapesWithModelObjects,
									dlg.CreateModelMappings,
									dlg.AddShapesToLayers);

								displayTabControl.SelectedTab = displayTabControl.TabPages[displayTabControl.TabCount - 1];
							} catch (Exception exc) {
								MessageBox.Show(this, exc.Message, "Error while creating test data", MessageBoxButtons.OK, MessageBoxIcon.Error);
							} finally {
								Cursor = Cursors.Default;
							}
						}
					}
				}
			} catch (Exception exc) {
				MessageBox.Show(this, exc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}


		private void nShapeEventMonitorToolStripMenuItem_Click(object sender, EventArgs e) {
			if (_eventMoniorForm == null) {
				_eventMoniorForm = new EventMonitorForm();
				_eventMoniorForm.FormClosed += eventMoniorForm_FormClosed;
				try {
					// Display
					_eventMoniorForm.AddEventSource(CurrentDisplay);
					// Project + History
					_eventMoniorForm.AddEventSource(project);
					_eventMoniorForm.AddEventSource(project.History);
					// Repository
					_eventMoniorForm.AddEventSource(cachedRepository);
					// DiagramSetController
					_eventMoniorForm.AddEventSource(diagramSetController);
					// ToolSetController + ToolSetPresenter + Tools
					_eventMoniorForm.AddEventSource(toolSetController);
					_eventMoniorForm.AddEventSource(toolSetListViewPresenter);
					foreach (Tool tool in toolSetController.Tools)
						_eventMoniorForm.AddEventSource(tool);
					// PropertyController + PropertyPresenter
					_eventMoniorForm.AddEventSource(propertyController);
					_eventMoniorForm.AddEventSource(propertyPresenter);

					_eventMoniorForm.AddEventSource(layerController);
					_eventMoniorForm.AddEventSource(layerPresenter);

					if (modelTreeController != null)
						_eventMoniorForm.AddEventSource(modelTreeController);
					if (modelTreePresenter != null)
						_eventMoniorForm.AddEventSource(modelTreePresenter);

					_eventMoniorForm.Show();
				} catch (Exception exc) {
					MessageBox.Show(this, exc.Message, "Error while opening EventMonitor", MessageBoxButtons.OK, MessageBoxIcon.Error);
					_eventMoniorForm.Close();
				}
			} else _eventMoniorForm.Close();
			nShapeEventMonitorToolStripMenuItem.Checked = (_eventMoniorForm != null);
		}


		private void eventMoniorForm_FormClosed(object sender, FormClosedEventArgs e) {
			_eventMoniorForm.FormClosed -= eventMoniorForm_FormClosed;
			_eventMoniorForm.Dispose();
			_eventMoniorForm = null;
			nShapeEventMonitorToolStripMenuItem.Checked = (_eventMoniorForm != null);
		}

		#endregion


		#region [Private] Event Handler implementations - Menu item "Help"

		private void aboutToolStripMenuItem_Click(object sender, EventArgs e) {
			using (AboutBox dlg = new AboutBox())
				dlg.ShowDialog(this);
		}


		private void viewHelpToolStripMenuItem_Click(object sender, EventArgs e) {
			FileInfo helpFile = GetHelpFile();
			if (helpFile != null)
				Process.Start(new ProcessStartInfo(helpFile.FullName));
		}


		private DirectoryInfo GetHelpDir() {
			DirectoryInfo programDir = new DirectoryInfo(Application.StartupPath);
			return new DirectoryInfo(Path.Combine(programDir.Parent.FullName, "Documentation"));
		}


		private FileInfo GetHelpFile() {
			FileInfo helpFile = null;
			DirectoryInfo helpDir = GetHelpDir();
			if (helpDir.Exists) {
				foreach (FileInfo fileInfo in helpDir.GetFiles("NShape*.chm")) {
					if (helpFile == null || fileInfo.CreationTimeUtc > helpFile.CreationTimeUtc)
						helpFile = fileInfo;
				}
			}
			return helpFile;
		}

		#endregion


		#region [Private] Types

		private struct RepositoryInfo : IEquatable<RepositoryInfo> {

			public static readonly RepositoryInfo Empty;

			public const string XmlStoreTypeName = "XML";

			public const string SqlServerStoreTypeName = "SQL Server";

			public static bool operator ==(RepositoryInfo a, RepositoryInfo b) {
				return (a.location == b.location
					&& a.projectName == b.projectName
					&& a.computerName == b.computerName
					&& a.typeName == b.typeName);
			}

			public static bool operator !=(RepositoryInfo a, RepositoryInfo b) { return !(a == b); }

			public static Store CreateStore(RepositoryInfo repositoryInfo) {
				Store store = null;
				if (repositoryInfo.typeName == RepositoryInfo.XmlStoreTypeName) {
					store = new XmlStore() {
						DirectoryName = Path.GetDirectoryName(repositoryInfo.location),
						FileExtension = Path.GetExtension(repositoryInfo.location),
						LazyLoading = true
					};
				} else if (repositoryInfo.typeName == RepositoryInfo.SqlServerStoreTypeName) {
					store = new SqlStore();
					((SqlStore)store).DatabaseName = repositoryInfo.location;
					((SqlStore)store).ServerName = repositoryInfo.computerName;
				} else {
					Debug.Fail(string.Format("Unsupported {0} value '{1}'", typeof(RepositoryInfo).Name, repositoryInfo));
				}
				return store;
			}

			public override bool Equals(object obj) {
				return (obj is RepositoryInfo && ((RepositoryInfo)obj) == this);
			}

			public bool Equals(RepositoryInfo other) {
				return other == this;
			}

			public override int GetHashCode() {
				return HashCodeGenerator.CalculateHashCode(location, projectName, computerName, typeName);
			}

			public RepositoryInfo(string projectName, string typeName, string serverName, string dataSource) {
				this.projectName = projectName;
				this.typeName = typeName;
				this.computerName = serverName;
				this.location = dataSource;
			}

			public string projectName;

			public string typeName;

			/// <summary>
			/// Contains the server name (in case of an AdoRepository) or the computer name (in case of a XmlStore).
			/// </summary>
			public string computerName;

			/// <summary>
			/// Contains the database' data source (in case of an AdoRepository) or the path to an XML file (in case of a XmlStore).
			/// </summary>
			public string location;

			static RepositoryInfo() {
				Empty.location = string.Empty;
				Empty.projectName = string.Empty;
				Empty.computerName = string.Empty;
				Empty.typeName = string.Empty;
			}
		}

		#endregion


		#region [Private] Constants

		private const string AppTitle = "NShape Designer";
		private const string SqlServerProviderName = "SQL Server";
		private const string DefaultDatabaseName = "NShape";
		private const string DefaultServerName = ".\\SQLEXPRESS";
		private const string ConfigFileName = "Config.cfg";
		private const string ProjectFileExtension = XmlStore.DefaultProjectFileExtension;
		private const string FileFilterXmlRepository = "NShape Designer Files|*.nspj|XML Repository Files|*.nspj;*.xml|All Files|*.*";

		private const string NewProjectName = "New NShape Project";
		private const string DefaultDiagramNameFmt = "Diagram {0}";

		private const string QueryNode = "//{0}";
		private const string QueryNodeAttr = "//{0}[@{1}]";

		private const string NodeNameSettings = "Settings";
		private const string NodeNameLoadToolbarSettings = "LoadToolBarSettings";
		private const string NodeNameProjectDirectory = "ProjectDirectory";
		private const string NodeNameProjects = "Projects";
		private const string NodeNameProject = "Project";
		private const string NodeNameWindowSettings = "WindowSettings";

		private const string AttrNameValue = "Value";
		private const string AttrNamePath = "Path";
		private const string AttrNameName = "Name";
		private const string AttrNameRepositoryType = "RepositoryType";
		private const string AttrNameServerName = "ServerName";
		private const string AttrNameDataSource = "DataSource";
		private const string AttrNameState = "State";
		private const string AttrNamePositionX = "PositionX";
		private const string AttrNamePositionY = "PositionY";
		private const string AttrNameWidth = "Width";
		private const string AttrNameHeight = "Height";
		private const int RecentProjectsLimit = 15;

		private const string ShapeCntFormatStr = "{0} Shape{1}";
		private const string PercentFormatStr = "{0:F1} %";
		private const string PointFormatStr = "{0}, {1}";
		private const string SizeFormatStr = "{0} x {1}";

		#endregion


		#region [Private] Fields

		private const int historyDropDownItemCount = 20;
		private const int recentProjectsItemCount = 25;

		private EventMonitorForm _eventMoniorForm;
		private LayoutDialog _layoutControlForm;
		private TemplateEditorDialog _templateEditorDialog;

		private string _nShapeConfigDirectory = Path.Combine(Path.Combine("dataweb", "NShape"), Application.ProductName);
		private string _xmlStoreDirectory;
		private bool _projectSaved = false;
		private int _maxRepositoryVersion;

		private int _currHistoryPos;
		private Display _currentDisplay;

		// display config
		private bool _mouseWheelZoom = true;
		private bool _showScrollBars = true;
		private bool _universalScroll = true;
		private MouseButtons _panButtons = MouseButtons.Middle | MouseButtons.Right;
		private bool _showSheet = true;
		private bool _enterClosesTextEditor = true;
		private bool _showGrid = true;
		private bool _snapToGrid = true;
		private int _gridSize = 20;
		private Color _gridColor = Color.Gainsboro;
		private int _snapDistance = 5;
		private ControlPointShape _resizePointShape = ControlPointShape.Square;
		private ControlPointShape _connectionPointShape = ControlPointShape.Circle;
		private int _controlPointSize = 3;
		private bool _showDefaultContextMenu = true;
		private bool _hideDeniedMenuItems = false;
#if DEBUG
		private bool _showCellOccupation = false;
		private bool _showInvalidatedAreas = false;
#endif

		private string _configFolder;
		private int _zoomHD = 100 * Display.ZoomLevelHDFactor;
		private bool _highQuality = true;

		private List<RepositoryInfo> _recentProjects = new List<RepositoryInfo>(recentProjectsItemCount);
		private bool _loadToolStripLayoutOnStartup = true;

		// Specifies whether the project is regarded as changed from the user's perspective 
		// (loading libraries in an empty repository created at startup will set the repository's IsModified state to "True")
		private bool _projectIsModified = false;

#if TdbRepository
		private const string fileFilterAllRepositories = "NShape Repository Files|*.xml;*.tdbd|XML Repository Files|*.xml|TurboDB Repository Databases|*.tdbd|All Files|*.*";
		private const string fileFilterTurboDBRepository = "TurboDB Repository Databases|*.tdbd|All Files|*.*";
#endif
		#endregion

	}


	public class DisplayTabPage : TabPage {

		public DisplayTabPage()
			: base() {
		}


		public DisplayTabPage(string text)
			: base(text) {
		}


		protected override void Dispose(bool disposing) {
			if (disposing)
				Display = null; // Clears and disposes the display
			base.Dispose(disposing);
		}


		public Display Display {
			get { return _tabDisplay; }
			set {
				if (_tabDisplay != null && _tabDisplay != value) {
					Controls.Remove(_tabDisplay);
					_tabDisplay.Clear();
					_tabDisplay.Dispose();
					_tabDisplay = null;
				}
				_tabDisplay = value;
				if (_tabDisplay != null)
					Controls.Add(_tabDisplay);
			}
		}


		private Display _tabDisplay;
	}


}