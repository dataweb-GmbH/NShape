/******************************************************************************
  Copyright 2009-2021 dataweb GmbH
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
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

using Dataweb.NShape.Advanced;
using Dataweb.NShape.Controllers;


namespace Dataweb.NShape.WinFormsUI {

	/// <summary>
	/// Connects any ListView to a NShape toolbox.
	/// </summary>
	public partial class ToolSetListViewPresenter : Component {
		
		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.WinFormsUI.ToolSetListViewPresenter" />.
		/// </summary>
		public ToolSetListViewPresenter() {
			InitializeImageLists();
			InitializeComponent();
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.WinFormsUI.ToolSetListViewPresenter" />.
		/// </summary>
		public ToolSetListViewPresenter(IContainer container) {
			container.Add(this);
			InitializeImageLists();
			InitializeComponent();
		}


		#region [Public] Properties

		/// <summary>
		/// Specifies the version of the assembly containing the component.
		/// </summary>
		[CategoryNShape()]
		public string ProductVersion {
			get { return this.GetType().Assembly.GetName().Version.ToString(); }
		}


		/// <summary>
		/// The controller of this presenter.
		/// </summary>
		[CategoryNShape()]
		public ToolSetController ToolSetController {
			get { return _toolSetController; }
			set {
				if (_toolSetController != null) {
					UnregisterToolBoxEventHandlers();
					if (_listView.ContextMenuStrip == presenterPrivateContextMenu)
						_listView.ContextMenuStrip = null;
				}
				_toolSetController = value;
				if (_toolSetController != null) {
					if (_listView != null && _listView.ContextMenuStrip == null)
						_listView.ContextMenuStrip = presenterPrivateContextMenu;
					RegisterToolBoxEventHandlers();
				}
			}
		}


		/// <summary>
		/// Specifies a ListView used as user interface for this presenter.
		/// </summary>
		[CategoryNShape()]
		public ListView ListView {
			get { return _listView; }
			set {
				if (_listView != null) {
					UnregisterListViewEventHandlers();
					if (_listView.ContextMenuStrip == presenterPrivateContextMenu)
						_listView.ContextMenuStrip = null;
				}
				_listView = value;
				if (_listView != null) {
					_listView.HeaderStyle = ColumnHeaderStyle.None;
					ColumnHeader header = new ColumnHeader();
					header.Name = headerName;
					header.Text = headerName;
					if (_listView.View == View.Details)
						header.Width = _listView.Width - SystemInformation.VerticalScrollBarWidth - (SystemInformation.Border3DSize.Width * 2) - _listView.Padding.Horizontal;
					_listView.Columns.Add(header);
					_listView.MultiSelect = false;
					_listView.FullRowSelect = true;
					_listView.ShowItemToolTips = true;
					_listView.ShowGroups = true;
					_listView.LabelEdit = false;
					_listView.HideSelection = false;
					_listView.SmallImageList = _smallImageList;
					_listView.LargeImageList = _largeImageList;
					if (_listView.ContextMenuStrip == null) 
						_listView.ContextMenuStrip = presenterPrivateContextMenu;

					RegisterListViewEventHandlers();
				}
			}
		}


		/// <summary>
		/// Specifies if MenuItemDefs that are not granted should appear as MenuItems in the dynamic context menu.
		/// </summary>
		[CategoryBehavior()]
		public bool HideDeniedMenuItems {
			get { return _hideMenuItemsIfNotGranted; }
			set { _hideMenuItemsIfNotGranted = value; }
		}


		/// <summary>
		/// If true, the standard context menu is created from MenuItemDefs. 
		/// If false, a user defined context menu is shown without creating additional menu items.
		/// </summary>
		[CategoryBehavior()]
		public bool ShowDefaultContextMenu {
			get { return _showDefaultContextMenu; }
			set { _showDefaultContextMenu = value; }
		}


		/// <summary>
		/// Dynamically built standard context menu. Will be used automatically if 
		/// the assigned listView has no ContextMenuStrip of its own.
		/// </summary>
		public ContextMenuStrip ContextMenuStrip {
			get { return presenterPrivateContextMenu; }
		}


		/// <summary>
		/// Gets the selected ListViewItem if the presenter's ListView.
		/// </summary>
		[Browsable(false)]
		public ListViewItem SelectedItem {
			get {
				ListViewItem result = null;
				if (_listView != null && _listView.SelectedItems.Count > 0)
					result = _listView.SelectedItems[0];
				return result;
			}
		}

		#endregion


		#region [Public] Methods

		/// <summary>
		/// Returns the ListViewItem associated with the given tool or null if no such item exists.
		/// </summary>
		public ListViewItem FindItem(Tool tool)
		{
			ListViewItem result = null;
			if (tool != null) {
				foreach (ListViewItem lvi in _listView.Items)
					if (lvi.Tag == tool) {
						result = lvi;
						break;
					}
			}
			return result;
		}


		/// <summary>
		/// Create and return a ListViewItem for the given tool.
		/// </summary>
		public ListViewItem CreateListViewItem(Tool tool)
		{
			if (tool == null) throw new ArgumentNullException("tool");
			ListViewItem item = new ListViewItem(tool.Title, tool.Name);
			item.ToolTipText = tool.ToolTipText;
			item.Tag = tool;

			int imgIdx = _smallImageList.Images.IndexOfKey(tool.Name);
			if (imgIdx < 0) {
				_smallImageList.Images.Add(tool.Name, tool.SmallIcon);
				_largeImageList.Images.Add(tool.Name, tool.LargeIcon);
				imgIdx = _smallImageList.Images.IndexOfKey(tool.Name);
			}
			item.ImageIndex = imgIdx;

			if (ListView.Groups.Count > 0)
				item.Group = FindGroup(tool.Category);
			
			return item;
		}

		#endregion

		#region [Private] Methods: (Un)Registering for events

		private void RegisterToolBoxEventHandlers() {
			_toolSetController.Cleared += toolBoxController_Cleared;
			_toolSetController.ToolAdded += toolBoxController_ToolAdded;
			_toolSetController.ToolChanged += toolBoxController_ToolChanged;
			_toolSetController.ToolRemoved += toolBoxController_ToolRemoved;
			_toolSetController.ToolSelected += toolBoxController_ToolSelected;
		}


		private void UnregisterToolBoxEventHandlers() {
			_toolSetController.ToolSelected -= toolBoxController_ToolSelected;
			_toolSetController.ToolRemoved -= toolBoxController_ToolRemoved;
			_toolSetController.ToolChanged -= toolBoxController_ToolChanged;
			_toolSetController.ToolAdded -= toolBoxController_ToolAdded;
			_toolSetController.Cleared -= toolBoxController_Cleared;
		}


		private void RegisterListViewEventHandlers() {
			_listView.MouseDown += listView_MouseDown;
			_listView.MouseUp += listView_MouseUp;
			_listView.MouseDoubleClick += listView_MouseDoubleClick;
			_listView.SizeChanged += listView_SizeChanged;
			_listView.SelectedIndexChanged += listView_SelectedIndexChanged;
			_listView.KeyDown += listView_KeyDown;
			if (_listView.ContextMenuStrip != null) {
				_listView.ContextMenuStrip.Opening += ContextMenuStrip_Opening;
				_listView.ContextMenuStrip.Closing += ContextMenuStrip_Closing;
			}
		}


		private void UnregisterListViewEventHandlers() {
			_listView.KeyDown -= listView_KeyDown;
			_listView.SelectedIndexChanged -= listView_SelectedIndexChanged;
			_listView.SizeChanged -= listView_SizeChanged;
			_listView.MouseDown -= listView_MouseDown;
			_listView.MouseUp -= listView_MouseUp;
			_listView.MouseDoubleClick -= listView_MouseDoubleClick;
			if (_listView.ContextMenuStrip != null) {
				_listView.ContextMenuStrip.Opening -= ContextMenuStrip_Opening;
				_listView.ContextMenuStrip.Closing -= ContextMenuStrip_Closing;
			}
		}

		#endregion


		#region [Private] Methods

		private void AssertListViewAvailable() {
			if (_listView == null) throw new NShapeException("Toolbox requires a ListView.");
		}


		private void InitializeImageLists() {
			_smallImageList = new ImageList();
			_smallImageList.ColorDepth = ColorDepth.Depth32Bit;
			_smallImageList.ImageSize = new Size(smallImageSize, smallImageSize);
			_smallImageList.TransparentColor = _transparentColor;

			_largeImageList = new ImageList();
			_largeImageList.ColorDepth = ColorDepth.Depth32Bit;
			_largeImageList.ImageSize = new Size(largeImageSize, largeImageSize);
			_largeImageList.TransparentColor = _transparentColor;
		}


		private void UpdateColumnWidth() {
			if (_listView == null || _listView.View != View.Details || _listView.Items.Count == 0)
				return;
			ColumnHeader header = _listView.Columns[headerName];
			if (header != null) {
				// There are special values for ListViewColumn.Width:
				// -1 = Width of the widest item
				// -2 = Fill remaining space
				// But we get stuck in an endless loop if we use them when adding items,
				// so we do the same manually...
				int hdrWidth = _listView.Width - _smallImageList.ImageSize.Width - _listView.Padding.Horizontal - 2;
				if (_listView.BorderStyle == BorderStyle.Fixed3D)
					hdrWidth -= (2 * SystemInformation.Border3DSize.Width);
				else if (_listView.BorderStyle == BorderStyle.FixedSingle)
					hdrWidth -= (2 * SystemInformation.BorderSize.Width);
				header.Width = hdrWidth;
			} else {
				Debug.Print("ColumnHeader '{0}' not found! This will result in an empty toolBox in details view.");
			}
		}


		private ListViewGroup FindGroup(string groupName)
		{
			ListViewGroup result = null;
			if (ListView != null && ListView.Groups.Count > 0) {
				foreach (ListViewGroup group in ListView.Groups) {
					if (group.Name == groupName) {
						result = group;
						break;
					}
				}
			}
			return result;
		}

		#endregion


		#region [Private] Event handler implementations

		private void toolBoxController_ToolSelected(object sender, ToolEventArgs e) {
			if (_listView != null) {
				ListViewItem item = FindItem(e.Tool);
				if (item != null && _listView.SelectedItems.IndexOf(item) < 0)
					item.Selected = true;
			}
		}


		private void toolBoxController_Cleared(object sender, EventArgs args) {
			if (_listView != null) {
				// This try/catch block is a workaround for a strange exception in WPF-Applications when trying to clear an empty tool box.
				try {
					_listView.Items.Clear();
				} catch (Exception exc) { 
					Debug.Fail(exc.Message); 
				}
				_smallImageList.Images.Clear();
				_largeImageList.Images.Clear();
			}
		}


		private void toolBoxController_ToolAdded(object sender, ToolEventArgs e) {
			// SaveChanges the list view: Move this to ToolSetListViewPresenter
			if (_listView == null) return;

			if (FindItem(e.Tool) != null)
				throw new NShapeException(string.Format("Tool {0} already exists.", e.Tool.Title));
			ListViewItem item = CreateListViewItem(e.Tool);
			// Put the tool into the right group
			if (!string.IsNullOrEmpty(e.Tool.Category)) {
				foreach (ListViewGroup group in _listView.Groups) {
					if (group.Name.Equals(e.Tool.Category, StringComparison.InvariantCultureIgnoreCase)) {
						item.Group = group;
						break;
					}
				}
				if (item.Group == null) {
					ListViewGroup group = new ListViewGroup(e.Tool.Category, e.Tool.Category);
					_listView.Groups.Add(group);
					item.Group = group;
				}
			}
			// Adjust the heading column in the list view
			if (_listView.Columns[headerName] != null) {
				using (Graphics gfx = Graphics.FromHwnd(_listView.Handle)) {
					SizeF txtSize = gfx.MeasureString(e.Tool.Title, _listView.Font);
					int currItemWidth = (int)Math.Round(txtSize.Width + 2 + _listView.SmallImageList.ImageSize.Width);
					if (currItemWidth > _largestItemWidth) {
						_largestItemWidth = currItemWidth;
						_listView.Columns[headerName].Width = _largestItemWidth;
					}
				}
			}
			// Add the item and select if default
			_listView.Items.Add(item);
		}


		private void toolBoxController_ToolChanged(object sender, ToolEventArgs e) {
			if (_listView != null && e.Tool is TemplateTool) {
				_listView.BeginUpdate();
				ListViewItem item = FindItem(e.Tool);
				if (item != null) {
					TemplateTool tool = (TemplateTool)e.Tool;
					item.Text = tool.Title;
					_largeImageList.Images[item.ImageIndex] = tool.LargeIcon;
					_smallImageList.Images[item.ImageIndex] = tool.SmallIcon;
				}
				_listView.EndUpdate();
			}
		}


		private void toolBoxController_ToolRemoved(object sender, ToolEventArgs e) {
			_listView.SuspendLayout();
			ListViewItem item = FindItem(e.Tool);
			_listView.Items.Remove(item);
			_listView.ResumeLayout();
		}


		private void listView_KeyDown(object sender, KeyEventArgs e) {
			if (_toolSetController.SelectedTool != null && e.KeyCode == Keys.Escape) {
				_toolSetController.SelectedTool.Cancel();
				if (SelectedItem != null && SelectedItem.Tag != _toolSetController.DefaultTool)
					SelectedItem.EnsureVisible();
			}
		}


		private void listView_SelectedIndexChanged(object sender, EventArgs e) {
			if (_listView.SelectedItems.Count > 0 && !_keepLastSelectedItem) {
				Tool tool = _listView.SelectedItems[0].Tag as Tool;
				if (tool != null) _toolSetController.SelectTool(tool, false);
			}
		}


		private void listView_SizeChanged(object sender, EventArgs e) {
			UpdateColumnWidth();
		}


		private void listView_MouseDoubleClick(object sender, MouseEventArgs e) {
			ListViewHitTestInfo hitTestInfo = ListView.HitTest(e.Location);
			if ((e.Button & MouseButtons.Left) == MouseButtons.Left) {
				if (hitTestInfo.Item != null) {
					Tool tool = hitTestInfo.Item.Tag as Tool;
					if (tool != null) _toolSetController.SelectTool(tool, true);
				}
			}
		}


		private void listView_MouseUp(object sender, MouseEventArgs e) {
			if (_keepLastSelectedItem && _lastSelectedItem != null) {
				_keepLastSelectedItem = false;
				_listView.SelectedIndices.Clear();
				_lastSelectedItem.Selected = true;
			}
		}


		private void listView_MouseDown(object sender, MouseEventArgs e) {
			ListViewHitTestInfo hitTestInfo = ListView.HitTest(e.Location);
			if (hitTestInfo.Item != null && !_listView.SelectedItems.Contains(hitTestInfo.Item)) {
				Tool tool = hitTestInfo.Item.Tag as Tool;
				if (tool != null) _toolSetController.SelectTool(tool, false);
			}
		}


		private void ContextMenuStrip_Opening(object sender, CancelEventArgs e) {
			if (_showDefaultContextMenu && _listView != null) {
				Debug.Assert(_listView.ContextMenuStrip != null);
				Point mousePos = _listView.PointToClient(Control.MousePosition);
				ListViewHitTestInfo hitTestInfo = ListView.HitTest(mousePos);
				Tool clickedTool = null;
				if (hitTestInfo.Item != null) clickedTool = hitTestInfo.Item.Tag as Tool;
				
				if (_toolSetController == null) throw new ArgumentNullException("ToolSetController");
				WinFormHelpers.BuildContextMenu(_listView.ContextMenuStrip, _toolSetController.GetMenuItemDefs(clickedTool), _toolSetController.Project, _hideMenuItemsIfNotGranted);
			}
			e.Cancel = _listView.ContextMenuStrip.Items.Count == 0;
		}


		private void ContextMenuStrip_Closing(object sender, ToolStripDropDownClosingEventArgs e) {
			if (sender == _listView.ContextMenuStrip)
				WinFormHelpers.CleanUpContextMenu(_listView.ContextMenuStrip);
			ToolSetController.SelectedTool = ToolSetController.DefaultTool;
			e.Cancel = false;
		}

		#endregion


		#region Fields

		private const string headerName = "Name";
		private const int templateDefaultSize = 20;
		private const int imageMargin = 2;
		private const int smallImageSize = 16;
		private const int largeImageSize = 32;

		private ToolSetController _toolSetController;
		private ListView _listView;
		private int _largestItemWidth = -1;

		// Settings
		private Color _transparentColor = Color.White;
		private bool _hideMenuItemsIfNotGranted = false;
		private bool _showDefaultContextMenu = true;
		
		// buffers for preventing listview to select listview items on right click
		private bool _keepLastSelectedItem = false;
		private ListViewItem _lastSelectedItem = null;

		// Small images for tool with same preview icon
		private ImageList _smallImageList;
		// Large images for tool with same preview icon
		private ImageList _largeImageList;

		#endregion
	}

}
