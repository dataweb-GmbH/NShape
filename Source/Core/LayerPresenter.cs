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
using System.Drawing;

using Dataweb.NShape.Advanced;


namespace Dataweb.NShape.Controllers {

	/// <summary>
	/// Non-visual component implementing the functionality of a layer presenter.
	/// Provides methods and properties for connecting an ILayerView user interface widget with a LayerController.
	/// </summary>
	[ToolboxItem(true)]
	[ToolboxBitmap("LayerPresenter.bmp")]
	public class LayerPresenter : Component {

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Controllers.LayerPresenter" />.
		/// </summary>
		public LayerPresenter()
			: base() {
		}


		#region [Public] Events

		/// <ToBeCompleted></ToBeCompleted>
		public event EventHandler<LayersEventArgs> LayerSelectionChanged;

		#endregion


		#region [Public] Properties

		/// <summary>
		/// Specifies the version of the assembly containing the component.
		/// </summary>
		[CategoryNShape()]
		public string ProductVersion {
			get { return this.GetType().Assembly.GetName().Version.ToString(); }
		}


		/// <ToBeCompleted></ToBeCompleted>
		[CategoryNShape()]
		public LayerController LayerController {
			get { return _layerController; }
			set {
				if (_layerController != null) UnregisterLayerControllerEvents();
				_layerController = value;
				if (_layerController != null) RegisterLayerControllerEvents();
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		[CategoryNShape()]
		public IDiagramPresenter DiagramPresenter {
			get { return _diagramPresenter; }
			set {
				if (_diagramPresenter != null) {
					// Unregister events and clear layer view
					UnregisterDiagramPresenterEvents();
					if (_layerView != null) _layerView.Clear();
				}
				_diagramPresenter = value;
				if (_diagramPresenter != null) {
					RegisterDiagramPresenterEvents();
					TryFillLayerView();
				}
			}
		}


		/// <summary>
		/// Provides access to a <see cref="T:Dataweb.NShape.Project" />.
		/// </summary>
		[CategoryNShape()]
		public Project Project {
			get { return (_layerController == null) ? null : _layerController.Project; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		[CategoryNShape()]
		public ILayerView LayerView {
			get { return _layerView; }
			set {
				if (_layerView != null) {
					// Unregister events and clear view
					UnregisterLayerViewEvents();
					_layerView.Clear();
				}
				_layerView = value;
				if (_layerView != null) {
					// Register events
					RegisterLayerViewEvents();
					TryFillLayerView();
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


		/// <ToBeCompleted></ToBeCompleted>
		[Browsable(false)]
		public IReadOnlyCollection<Layer> SelectedLayers {
			get { return _selectedLayers; }
		}

		#endregion


		#region Methods (protected)

		/// <summary>
		/// Returns a collection of <see cref="T:Dataweb.NShape.Advanced.MenuItemDef" /> for constructing context menus etc.
		/// </summary>
		protected IEnumerable<MenuItemDef> GetMenuItemDefs() {
			if (_layerController == null || _diagramPresenter == null || _diagramPresenter.Diagram == null)
				yield break;

			bool separatorNeeded = false;
			foreach (MenuItemDef controllerAction in LayerController.GetMenuItemDefs(_diagramPresenter.Diagram, _selectedLayers)) {
				if (!separatorNeeded) separatorNeeded = true;
				yield return controllerAction;
			}

			string layersText = (_selectedLayers.Count > 1) ? Properties.Resources.Text_Layers : Properties.Resources.Text_Layer;

			if (separatorNeeded) yield return new SeparatorMenuItemDef();

			bool isFeasible;
			string description;
			char securityDomain = (_diagramPresenter.Diagram != null) ? _diagramPresenter.Diagram.SecurityDomainName : '-';

			isFeasible = _selectedLayers.Count == 1;
			if (_selectedLayers.Count == 0) description = string.Empty;
			else if (_selectedLayers.Count == 1) description = string.Format(Properties.Resources.MessageFmt_RenameLayer0, _selectedLayers[0].Title);
			else description = Properties.Resources.MessageTxt_TooManyLayersSelected;
			yield return new DelegateMenuItemDef(LayerController.MenuItemNameRenameLayer, 
				Properties.Resources.CaptionTxt_RenameLayer, Properties.Resources.RenameBtn, description, 
				isFeasible, Permission.Data, securityDomain, (a, p) => BeginRenameSelectedLayer());

			isFeasible = _selectedLayers.Count > 0;
			if (isFeasible)
				description = string.Format(Properties.Resources.MessageFmt_Activate01, _selectedLayers.Count, layersText);
			else description = Properties.Resources.MessageTxt_NoLayersSelected;
			yield return new DelegateMenuItemDef(LayerController.MenuItemNameActivateLayers,
				string.Format(Properties.Resources.CaptionFmt_Activate0, layersText), Properties.Resources.Enabled, description, 
				isFeasible, Permission.Data, securityDomain, (a, p) => ActivateSelectedLayers());

			isFeasible = _selectedLayers.Count > 0;
			description = isFeasible ? string.Format(Properties.Resources.MessageFmt_Deactivate01, _selectedLayers.Count, layersText) 
				: Properties.Resources.MessageTxt_NoLayersSelected;
			yield return new DelegateMenuItemDef(LayerController.MenuItemNameDeactivateLayers, 
				string.Format(Properties.Resources.CaptionFmt_Deactivate0, layersText), Properties.Resources.Disabled, description, 
				isFeasible, Permission.Data, securityDomain, (a, p) => DeactivateSelectedLayers()
			);

			yield return new SeparatorMenuItemDef();

			isFeasible = _selectedLayers.Count > 0;
			description = isFeasible ? string.Format(Properties.Resources.MessageFmt_Show01, _selectedLayers.Count, layersText) 
				: Properties.Resources.MessageTxt_NoLayersSelected;
			yield return new DelegateMenuItemDef(LayerController.MenuItemNameShowLayers, 
				string.Format(Properties.Resources.CaptionFmt_Show0, layersText), Properties.Resources.Visible, description,
				isFeasible, Permission.None, (a, p) => ShowSelectedLayers()
			);

			isFeasible = _selectedLayers.Count > 0;
			description = isFeasible ? string.Format(Properties.Resources.MessageFmt_Hide01, _selectedLayers.Count, layersText) 
				: Properties.Resources.MessageTxt_NoLayersSelected;
			yield return new DelegateMenuItemDef(LayerController.MenuItemNameHideLayers, 
				string.Format(Properties.Resources.CaptionFmt_Hide0, layersText), Properties.Resources.Invisible, description, 
				isFeasible, Permission.None, (a, p) => HideSelectedLayers()
			);
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected void OnSelectedLayersChanged(object sender, LayersEventArgs e) {
			if (LayerSelectionChanged != null) LayerSelectionChanged(sender, e);
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected void ShowSelectedLayers() {
			AssertLayerControllerIsSet();
			foreach (Layer selectedLayer in _selectedLayers)
				_diagramPresenter.SetLayerVisibility(selectedLayer.LayerId, true);
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected void HideSelectedLayers() {
			AssertLayerControllerIsSet();
			foreach (Layer selectedLayer in _selectedLayers)
				_diagramPresenter.SetLayerVisibility(selectedLayer.LayerId, true);
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected void ActivateSelectedLayers() {
			AssertLayerControllerIsSet();
			foreach (Layer selectedLayer in _selectedLayers)
				_diagramPresenter.SetLayerActive(selectedLayer.LayerId, true);
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected void DeactivateSelectedLayers() {
			AssertLayerControllerIsSet();
			foreach (Layer selectedLayer in _selectedLayers)
				_diagramPresenter.SetLayerActive(selectedLayer.LayerId, false);
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected void BeginRenameSelectedLayer() {
			if (_selectedLayers.Count == 0) throw new NShapeException("No layers selected.");
			_layerView.BeginEditLayerName(_selectedLayers[0]);
		}

		#endregion


		#region Methods (private)

		private void RegisterDiagramPresenterEvents() {
			_diagramPresenter.ActiveLayersChanged += diagramPresenter_ActiveLayersChanged;
			_diagramPresenter.LayerVisibilityChanged += diagramPresenter_LayerVisibilityChanged;
			_diagramPresenter.DiagramChanged += diagramPresenter_DiagramChanged;
		}


		private void UnregisterDiagramPresenterEvents() {
			_diagramPresenter.ActiveLayersChanged -= diagramPresenter_ActiveLayersChanged;
			_diagramPresenter.LayerVisibilityChanged -= diagramPresenter_LayerVisibilityChanged;
			_diagramPresenter.DiagramChanged -= diagramPresenter_DiagramChanged;
		}


		private void SetSelectedLayers(Layer layer) {
			_selectedLayers.Clear();
			_selectedLayers.Add(layer);
			OnSelectedLayersChanged(this, LayerHelper.GetLayersEventArgs(_selectedLayers));
		}


		private void SetSelectedLayers(IEnumerable<Layer> layers) {
			_selectedLayers.Clear();
			foreach (Layer l in layers) _selectedLayers.Add(l);
			OnSelectedLayersChanged(this, LayerHelper.GetLayersEventArgs(_selectedLayers));
		}


		private void SelectLayer(Layer layer) {
			_selectedLayers.Add(layer);
			OnSelectedLayersChanged(this, LayerHelper.GetLayersEventArgs(_selectedLayers));
		}


		private void UnselectLayer(Layer layer) {
			_selectedLayers.Remove(layer);
			OnSelectedLayersChanged(this, LayerHelper.GetLayersEventArgs(_selectedLayers));
		}


		private void UnselectAllLayers() {
			_selectedLayers.Clear();
			OnSelectedLayersChanged(this, LayerHelper.GetLayersEventArgs(_selectedLayers));
		}


		private void AssertLayerControllerIsSet() {
			if (_layerController == null) throw new Exception("Property 'LayerController' is not set.");
		}


		private void RegisterLayerControllerEvents() {
			_layerController.DiagramChanging += layerController_DiagramChanging;
			_layerController.DiagramChanged += layerController_DiagramChanged;
			_layerController.LayersAdded += layerController_LayerAdded;
			_layerController.LayersRemoved += layerController_LayerRemoved;
			_layerController.LayerModified += layerController_LayerModified;
			RegisterProjectEvents(_layerController.Project);
		}


		private void UnregisterLayerControllerEvents() {
			_layerController.DiagramChanging -= layerController_DiagramChanging;
			_layerController.DiagramChanged -= layerController_DiagramChanged;
			_layerController.LayersAdded -= layerController_LayerAdded;
			_layerController.LayersRemoved -= layerController_LayerRemoved;
			_layerController.LayerModified -= layerController_LayerModified;
			UnregisterProjectEvents(_layerController.Project);
		}


		private void RegisterProjectEvents(Project project) {
			if (project != null) {
				project.Closing += project_Closing;
				project.Closed += project_Closed;
			}
		}


		private void UnregisterProjectEvents(Project project) {
			if (project != null) {
				project.Closing -= project_Closing;
				project.Closed -= project_Closed;
			}
		}


		private void RegisterLayerViewEvents() {
			_layerView.LayerRenamed += layerView_LayerItemRenamed;
			_layerView.LayerLowerZoomThresholdChanged += layerView_LayerLowerZoomThresholdChanged;
			_layerView.LayerUpperZoomThresholdChanged += layerView_LayerUpperZoomThresholdChanged;
			_layerView.LayerViewMouseDown += layerView_MouseDown;
			_layerView.LayerViewMouseMove += layerView_MouseMove;
			_layerView.LayerViewMouseUp += layerView_MouseUp;
			_layerView.SelectedLayerChanged += _layerView_SelectedLayerChanged;
		}


		private void UnregisterLayerViewEvents() {
			_layerView.LayerRenamed -= layerView_LayerItemRenamed;
			_layerView.LayerLowerZoomThresholdChanged -= layerView_LayerLowerZoomThresholdChanged;
			_layerView.LayerUpperZoomThresholdChanged -= layerView_LayerUpperZoomThresholdChanged;
			_layerView.LayerViewMouseDown -= layerView_MouseDown;
			_layerView.LayerViewMouseMove -= layerView_MouseMove;
			_layerView.LayerViewMouseUp -= layerView_MouseUp;
			_layerView.SelectedLayerChanged -= _layerView_SelectedLayerChanged;
		}


		private void GetLayerState(Layer layer, out bool isActive, out bool isVisible) {
			if (layer == null) throw new ArgumentNullException(nameof(layer));
			if (_diagramPresenter == null) throw new ArgumentNullException(nameof(DiagramPresenter));
			isActive = _diagramPresenter.IsLayerActive(layer.LayerId);
			isVisible = _diagramPresenter.IsLayerVisible(layer.LayerId);
		}


		private void TryFillLayerView() {
			// Fill view with layer items if necessary
			if (_layerController != null && _diagramPresenter != null && _diagramPresenter.Diagram != null)
				AddLayerItemsToLayerView(_diagramPresenter.Diagram.Layers);
		}


		private void AddLayerItemsToLayerView(IEnumerable<Layer> layers) {
			_layerView.BeginUpdate();
			bool isActive, isVisible;
			foreach (Layer layer in layers) {
				GetLayerState(layer, out isActive, out isVisible);
				_layerView.AddLayer(layer, isActive, isVisible);
			}
			_layerView.EndUpdate();
		}


		private void AssertControllerIsSet() {
			if (LayerController == null) throw new ArgumentNullException(nameof(LayerController));
		}

		#endregion


		#region LayerController EventHandler implementations

		private void layerController_DiagramChanging(object sender, EventArgs e) {
			if (_layerView != null) _layerView.Clear();
		}


		private void layerController_DiagramChanged(object sender, EventArgs e) {
			TryFillLayerView();
		}


		private void layerController_LayerAdded(object sender, LayersEventArgs e) {
			// Create LayerView items for the new layers
			if (e.Layers != null && _layerView != null)
				AddLayerItemsToLayerView(e.Layers);
		}


		private void layerController_LayerModified(object sender, LayersEventArgs e) {
			if (e.Layers != null && _layerView != null) {
				bool isActive, isVisible;
				foreach (Layer layer in e.Layers) {
					GetLayerState(layer, out isActive, out isVisible);
					_layerView.RefreshLayer(layer, isActive, isVisible);
				}
			}
		}


		private void layerController_LayerRemoved(object sender, LayersEventArgs e) {
			foreach (Layer layer in e.Layers)
				_layerView.RemoveLayer(layer);
		}

		#endregion


		#region Project  EventHandler Implementations

		private void project_Closing(object sender, EventArgs e) {
			// Nothing to do
		}


		private void project_Closed(object sender, EventArgs e) {
			if (LayerView != null) LayerView.Clear();
		}

		#endregion


		#region IDiagramPresenter EventHandler implementations

		private void diagramPresenter_LayerVisibilityChanged(object sender, LayersEventArgs e) {
			_layerView.BeginUpdate();
			foreach (Layer layer in e.Layers)
				_layerView.RefreshLayer(layer, _diagramPresenter.IsLayerActive(layer.LayerId), _diagramPresenter.IsLayerVisible(layer.LayerId));
			_layerView.EndUpdate();
		}


		private void diagramPresenter_ActiveLayersChanged(object sender, LayersEventArgs e) {
			// When activating layers, we have to update all layers in the list because activating a home layer deactivates the other home layer(s).
			_layerView.BeginUpdate();
			foreach (Layer layer in _diagramPresenter.Diagram.Layers)
				_layerView.RefreshLayer(layer, _diagramPresenter.IsLayerActive(layer.LayerId), _diagramPresenter.IsLayerVisible(layer.LayerId));
			_layerView.EndUpdate();
		}


		private void diagramPresenter_DiagramChanged(object sender, EventArgs e) {
			_layerView.BeginUpdate();
			if (_layerView != null && DiagramPresenter.Diagram != null)
				AddLayerItemsToLayerView(_diagramPresenter.Diagram.Layers);
			_layerView.EndUpdate();
		}

		#endregion


		#region ILayerView EventHandler implementations

		private void _layerView_SelectedLayerChanged(object sender, LayersEventArgs e) {
			if (EnumerationHelper.IsEmpty(e.Layers))
				UnselectAllLayers();
			else
				SetSelectedLayers(e.Layers);
		}


		private void layerView_MouseDown(object sender, LayerMouseEventArgs e) {
			// Nothing to do here
		}


		private void layerView_MouseMove(object sender, LayerMouseEventArgs e) {
			// ToDo: MouseHover image highlighting
		}


		private void layerView_MouseUp(object sender, LayerMouseEventArgs e) {
			switch (e.Buttons) {
				case MouseButtonsDg.Left:
					switch (e.Item) {
						case LayerItem.Name:
							if (e.Layer != null && _selectedLayers.Contains(e.Layer)) 
								_layerView.BeginEditLayerName(e.Layer);
							break;
						case LayerItem.ActiveState:
							if (e.Layer != null && _selectedLayers.Count > 0)
								_diagramPresenter.SetLayerActive(_selectedLayers, !_diagramPresenter.IsLayerActive(e.Layer.LayerId));
							break;
						case LayerItem.Visibility:
							if (e.Layer != null && _selectedLayers.Count > 0)
								_diagramPresenter.SetLayerVisibility(_selectedLayers, !_diagramPresenter.IsLayerVisible(e.Layer.LayerId));
							break;
						case LayerItem.MinZoom:
							if (e.Layer != null && _selectedLayers.Contains(e.Layer)) 
								_layerView.BeginEditLayerMinZoomBound(e.Layer);
							break;
						case LayerItem.MaxZoom:
							if (e.Layer != null && _selectedLayers.Contains(e.Layer)) 
								_layerView.BeginEditLayerMaxZoomBound(e.Layer);
							break;
						case LayerItem.None:
							// nothing to do
							break;
						default: 
							throw new NShapeUnsupportedValueException(e.Item);
					}
					break;

					// Open context menu
				case MouseButtonsDg.Right:
					_layerView.OpenContextMenu(e.Position.X, e.Position.Y, GetMenuItemDefs(), LayerController.DiagramSetController.Project);
					break;

				default:
					// Ignore all other buttons as well as combinations of buttons
					break;
			}
		}


		private void layerView_LayerItemRenamed(object sender, LayerRenamedEventArgs e) {
			_layerController.RenameLayer(_diagramPresenter.Diagram, e.Layer, e.OldName, e.NewName);
		}


		private void layerView_LayerUpperZoomThresholdChanged(object sender, LayerZoomThresholdChangedEventArgs e) {
			_layerController.SetLayerZoomBounds(_diagramPresenter.Diagram, e.Layer, e.Layer.LowerZoomThreshold, e.NewZoomThreshold);
		}


		private void layerView_LayerLowerZoomThresholdChanged(object sender, LayerZoomThresholdChangedEventArgs e) {
			_layerController.SetLayerZoomBounds(_diagramPresenter.Diagram, e.Layer, e.NewZoomThreshold, e.Layer.UpperZoomThreshold);
		}

		#endregion


		#region Fields

		private LayerController _layerController;
		private IDiagramPresenter _diagramPresenter;
		private ILayerView _layerView;
		private ReadOnlyList<Layer> _selectedLayers = new ReadOnlyList<Layer>();
		private bool _hideMenuItemsIfNotGranted = false;

		private LayerEventArgs _layerEventArgs = new LayerEventArgs();
		private LayersEventArgs _layersEventArgs = new LayersEventArgs();

		#endregion
	}


	/// <summary>
	/// Interface for a layer presenter's user interface implementation.
	/// </summary>
	public interface ILayerView {

		#region Events

		/// <ToBeCompleted></ToBeCompleted>
		event EventHandler<LayersEventArgs> SelectedLayerChanged;

		/// <ToBeCompleted></ToBeCompleted>
		event EventHandler<LayerRenamedEventArgs> LayerRenamed;

		/// <ToBeCompleted></ToBeCompleted>
		event EventHandler<LayerZoomThresholdChangedEventArgs> LayerUpperZoomThresholdChanged;

		/// <ToBeCompleted></ToBeCompleted>
		event EventHandler<LayerZoomThresholdChangedEventArgs> LayerLowerZoomThresholdChanged;

		/// <ToBeCompleted></ToBeCompleted>
		event EventHandler<LayerMouseEventArgs> LayerViewMouseDown;

		/// <ToBeCompleted></ToBeCompleted>
		event EventHandler<LayerMouseEventArgs> LayerViewMouseMove;

		/// <ToBeCompleted></ToBeCompleted>
		event EventHandler<LayerMouseEventArgs> LayerViewMouseUp;

		#endregion


		#region Methods

		/// <summary>
		/// Clears the contents of the layer view.
		/// </summary>
		void Clear();

		/// <summary>
		/// Signals that the user interface is going to be updated.
		/// </summary>
		void BeginUpdate();

		/// <summary>
		/// Signals that the user interface was updated.
		/// </summary>
		void EndUpdate();

		/// <summary>
		/// Adds a new item to the user interface representing the given layer.
		/// </summary>
		void AddLayer(Layer layer, bool isActive, bool isVisible);

		/// <summary>
		/// Removes the item representing the given layer from the layerView's content.
		/// </summary>
		/// <param name="layer"></param>
		void RemoveLayer(Layer layer);

		/// <summary>
		/// Refreshes the contents of the item representing the given layer.
		/// </summary>
		void RefreshLayer(Layer layer, bool isActive, bool isVisible);

		/// <summary>
		/// Signals that the given layer is going to be renamed.
		/// </summary>
		void BeginEditLayerName(Layer layer);

		/// <summary>
		/// Signals that the lower zoom treshold of the given layer is going to be changed.
		/// </summary>
		void BeginEditLayerMinZoomBound(Layer layer);

		/// <summary>
		/// Signals that the upper zoom treshold of the given layer is going to be changed.
		/// </summary>
		void BeginEditLayerMaxZoomBound(Layer layer);

		/// <summary>
		/// Signals that the user interface should be redrawn.
		/// </summary>
		void Invalidate();

		/// <summary>
		/// Signals that a context menu buildt from the given contextMenuItemDefs should be opened at the given coordinates.
		/// </summary>
		void OpenContextMenu(int x, int y, IEnumerable<MenuItemDef> contextMenuItemDefs, Project project);

		#endregion
	
	}


	/// <summary>
	/// Specifies the type of user interface item.
	/// </summary>
	public enum LayerItem {
		/// <summary>Specifies the user iterface item displaying the layer's name.</summary>
		Name,
		/// <summary>Specifies the user iterface item displaying the layer's visibility state.</summary>
		Visibility,
		/// <summary>Specifies the user iterface item displaying the layer's active state.</summary>
		ActiveState,
		/// <summary>Specifies the user iterface item displaying the layer's lower zoom treshold.</summary>
		MinZoom,
		/// <summary>Specifies the user iterface item displaying the layer's upper zoom treshold.</summary>
		MaxZoom,
		/// <summary>No user iterface item.</summary>
		None
	}
	
	
	/// <summary>
	/// Mouse event args for layer view inplementations.
	/// </summary>
	public class LayerMouseEventArgs : MouseEventArgsDg {

		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("Use an overloaded version that takes selected layers.")]
		public LayerMouseEventArgs(Layer layer, LayerItem item, 
			MouseEventType eventType, MouseButtonsDg buttons, int clickCount, int wheelDelta, 
			Point position, KeysDg modifiers)
			: this(layer, item, EnumerationHelper.Enumerate(layer), eventType, buttons, clickCount, wheelDelta, position, modifiers) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("Use an overloaded version that takes selected layers.")]
		public LayerMouseEventArgs(Layer layer, LayerItem item, MouseEventArgsDg mouseEventArgs)
			: this(layer, item, EnumerationHelper.Enumerate(layer), mouseEventArgs.EventType, mouseEventArgs.Buttons, mouseEventArgs.Clicks, mouseEventArgs.WheelDelta, mouseEventArgs.Position, mouseEventArgs.Modifiers) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public LayerMouseEventArgs(Layer layer, LayerItem item, IEnumerable<Layer> selectedLayers, MouseEventArgsDg mouseEventArgs)
			: this(layer, item, selectedLayers, mouseEventArgs.EventType, mouseEventArgs.Buttons, mouseEventArgs.Clicks, mouseEventArgs.WheelDelta, mouseEventArgs.Position, mouseEventArgs.Modifiers) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public LayerMouseEventArgs(Layer layer, LayerItem item, IEnumerable<Layer> selectedlayers, 
			MouseEventType eventType, MouseButtonsDg buttons, int clickCount, int wheelDelta, Point position, KeysDg modifiers)
			: base(eventType, buttons, clickCount, wheelDelta, position, modifiers) {
			this._layer = layer;
			this._item = item;
			this._selectedLayers = new List<Layer>(selectedlayers);
		}


		/// <summary>
		/// Gets the layer for which the mouse event was raised.
		/// </summary>
		public Layer Layer {
			get { return _layer; }
			protected internal set { _layer = value; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		public LayerItem Item {
			get { return _item; }
			protected internal set { _item = value; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		public IEnumerable<Layer> SelectedLayers {
			get { return _selectedLayers; }
			protected internal set {
				_selectedLayers.Clear();
				_selectedLayers.AddRange(value);
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected internal LayerMouseEventArgs()
			: base() {
			_layer = null;
			_item = LayerItem.None;
			_selectedLayers = new List<Layer>();
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected internal void SetMouseEvent(MouseEventType eventType, MouseButtonsDg buttons, 
			int clickCount, int wheelDelta, Point position, KeysDg modifiers) {
			EventType = eventType;
			Buttons = buttons;
			Clicks = clickCount;
			Modifiers = modifiers;
			Position = position;
			WheelDelta = wheelDelta;
		}


		private Layer _layer;
		private List<Layer> _selectedLayers;
		private LayerItem _item;
	}

}
