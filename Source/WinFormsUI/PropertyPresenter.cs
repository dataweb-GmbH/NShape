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
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

using Dataweb.NShape.Advanced;
using Dataweb.NShape.Controllers;


namespace Dataweb.NShape.WinFormsUI {

	/// <summary>
	/// Uses one or two <see cref="T:System.Windows.Forms.PropertyGrid"/> controls to edit properties of shapes, diagrams and model objects.
	/// </summary>
	[ToolboxItem(true)]
	[ToolboxBitmap(typeof(PropertyPresenter), "PropertyPresenter.bmp")]
	public class PropertyPresenter : Component {

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.WinFormsUI.PropertyPresenter" />.
		/// </summary>
		public PropertyPresenter() {
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.WinFormsUI.PropertyPresenter" />.
		/// </summary>
		public PropertyPresenter(IPropertyController propertyController)
			: this() {
			if (propertyController == null) throw new ArgumentNullException("propertyController");
			this.PropertyController = propertyController;
		}


		/// <summary>
		/// Specifies the version of the assembly containing the component.
		/// </summary>
		[CategoryNShape()]
		public string ProductVersion {
			get { return this.GetType().Assembly.GetName().Version.ToString(); }
		}


		/// <summary>
		/// Specifies the controller for this presenter.
		/// </summary>
		[CategoryNShape()]
		public IPropertyController PropertyController {
			get { return _propertyController; }
			set {
				UnregisterPropertyControllerEvents();
				_propertyController = value;
				RegisterPropertyControllerEvents();
			}
		}


		/// <summary>
		/// Specifies the number of property grids/pages available
		/// </summary>
		[Browsable(false)]
		public int PageCount {
			get {
				int result = 0;
				if (_primaryPropertyGrid != null) ++result;
				if (_secondaryPropertyGrid != null) ++result;
				return result;
			}
		}


		/// <summary>
		/// Specifies a PropertyGrid for editing primary objects.
		/// </summary>
		public PropertyGrid PrimaryPropertyGrid {
			get { return _primaryPropertyGrid; }
			set {
				UnregisterPropertyGridEvents(_primaryPropertyGrid);
				_primaryPropertyGrid = value;
				RegisterPropertyGridEvents(_primaryPropertyGrid);
			}
		}


		/// <summary>
		/// Specifies a PropertyGrid for editing secondary objects.
		/// </summary>
		public PropertyGrid SecondaryPropertyGrid {
			get { return _secondaryPropertyGrid; }
			set {
				UnregisterPropertyGridEvents(_secondaryPropertyGrid);
				_secondaryPropertyGrid = value;
				RegisterPropertyGridEvents(_secondaryPropertyGrid);
			}
		}


		#region [Private] Methods

		private void GetPropertyGrid(int pageIndex, out PropertyGrid propertyGrid) {
			propertyGrid = null;
			switch (pageIndex) {
				case 0:
					propertyGrid = _primaryPropertyGrid;
					break;
				case 1:
					propertyGrid = _secondaryPropertyGrid;
					break;
				default: throw new ArgumentOutOfRangeException("pageIndex");
			}
		}


		private void AssertControllerExists() {
			if (PropertyController == null) throw new InvalidOperationException("Property PropertyController is not set.");
		}


		private PropertyInfo GetPropertyInfo(PropertyGrid propertyGrid, GridItem item) {
			if (propertyGrid == null) throw new ArgumentNullException("propertyGrid");
			if (item == null) throw new ArgumentNullException("item");
			PropertyInfo result = null;
			int cnt = propertyGrid.SelectedObjects.Length;
			Type selectedObjectsType = propertyGrid.SelectedObject.GetType();
			// handle the case that the edited property is an expendable property, e.g. of type 'Font'. 
			// In this case, the property that has to be changed is not the edited item itself but it's parent item.
			if (item.Parent.PropertyDescriptor != null) {
				// the current selectedItem is a ChildItem of the edited object's property
				result = selectedObjectsType.GetProperty(item.Parent.PropertyDescriptor.Name);
			} else if (item.PropertyDescriptor != null)
				result = selectedObjectsType.GetProperty(item.PropertyDescriptor.Name);
			return result;
		}

		#endregion


		#region [Private] Methods: (Un)Registering for events

		private void RegisterPropertyControllerEvents() {
			if (_propertyController != null) {
				_propertyController.ObjectsModified += propertyController_RefreshObjects;
				_propertyController.SelectedObjectsChanged += propertyController_SelectedObjectsChanged;
				_propertyController.ProjectClosing += propertyController_ProjectClosing;
			}
		}


		private void UnregisterPropertyControllerEvents() {
			if (_propertyController != null) {
				TypeDescriptionProviderDg.PropertyController = null;
				_propertyController.ObjectsModified -= propertyController_RefreshObjects;
				_propertyController.SelectedObjectsChanged -= propertyController_SelectedObjectsChanged;
				_propertyController.ProjectClosing -= propertyController_ProjectClosing;
			}
		}


		private void RegisterPropertyGridEvents(PropertyGrid propertyGrid) {
			if (propertyGrid != null) {
				propertyGrid.PropertyValueChanged += propertyGrid_PropertyValueChanged;
				propertyGrid.SelectedGridItemChanged += propertyGrid_SelectedGridItemChanged;
				propertyGrid.KeyDown += propertyGrid_KeyDown;
			}
		}

		private void UnregisterPropertyGridEvents(PropertyGrid propertyGrid) {
			if (propertyGrid != null) {
				//propertyGrid.Enter -= propertyGrid_Enter;
				//propertyGrid.Leave -= propertyGrid_Leave;
				propertyGrid.PropertyValueChanged -= propertyGrid_PropertyValueChanged;
				propertyGrid.SelectedGridItemChanged -= propertyGrid_SelectedGridItemChanged;
				propertyGrid.KeyDown -= propertyGrid_KeyDown;
			}
		}

		#endregion


		#region [Private] Methods: Event handler implementations

		private void propertyGrid_Leave(object sender, EventArgs e) {
			TypeDescriptionProviderDg.PropertyController = null;
		}


		private void propertyGrid_Enter(object sender, EventArgs e) {
			TypeDescriptionProviderDg.PropertyController = _propertyController;
		}


		private void propertyController_SelectedObjectsChanged(object sender, PropertyControllerEventArgs e) {
			AssertControllerExists();

			_propertyController.CancelSetProperty();
			if (_propertyController.Project != null && _propertyController.Project.IsOpen)
				StyleUITypeEditor.Project = _propertyController.Project;

			PropertyGrid grid = null;
			GetPropertyGrid(e.PageIndex, out grid);
			if (grid != null) {
				TypeDescriptionProviderDg.PropertyController = _propertyController;
				if (e.Objects.Count > 0)
					grid.SelectedObjects = e.GetObjectArray();
				else if (grid.SelectedObject != null)
					grid.SelectedObject = null;
				grid.Visible = true;
			}
		}


		private void propertyController_RefreshObjects(object sender, PropertyControllerEventArgs e) {
			AssertControllerExists();

			StyleUITypeEditor.Project = _propertyController.Project;
			PropertyGrid grid = null;
			GetPropertyGrid(e.PageIndex, out grid);
			if (grid != null) {
				grid.SuspendLayout();
				grid.Refresh();
				grid.ResumeLayout();
			}
		}


		private void propertyController_ProjectClosing(object sender, EventArgs e) {
			AssertControllerExists();
			_propertyController.CancelSetProperty();
			if (_primaryPropertyGrid != null && _primaryPropertyGrid.SelectedObject != null)
				_primaryPropertyGrid.SelectedObject = null;
			if (_secondaryPropertyGrid != null && _secondaryPropertyGrid.SelectedObject != null)
				_secondaryPropertyGrid.SelectedObject = null;
		}


		private void propertyGrid_KeyDown(object sender, KeyEventArgs e) {
			if (e.KeyCode == Keys.Escape && PropertyController != null)
				_propertyController.CancelSetProperty();
		}


		private void propertyGrid_PropertyValueChanged(object sender, PropertyValueChangedEventArgs e) {
			AssertControllerExists();
			_propertyController.CommitSetProperty();
			if (sender is PropertyGrid) ((PropertyGrid)sender).Refresh();
		}


		private void propertyGrid_SelectedGridItemChanged(object sender, SelectedGridItemChangedEventArgs e) {
			if (_propertyController != null
				&& e.NewSelection != null
				&& e.OldSelection != null) {
				if (e.OldSelection.Equals(e.NewSelection)) return;
				else {
					_propertyController.CancelSetProperty();
					TypeDescriptionProviderDg.PropertyController = _propertyController;
				}
			}
		}

		#endregion


		#region Fields

		private PropertyGrid _primaryPropertyGrid = null;
		private PropertyGrid _secondaryPropertyGrid = null;
		private IPropertyController _propertyController = null;

		#endregion

	}
}
