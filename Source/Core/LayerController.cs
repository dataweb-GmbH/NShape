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
using System.Drawing;
using System.Reflection;

using Dataweb.NShape.Advanced;
using Dataweb.NShape.Commands;


namespace Dataweb.NShape.Controllers {

	/// <ToBeCompleted></ToBeCompleted>
	[ToolboxItem(true)]
	[ToolboxBitmap(typeof(LayerController), "LayerController.bmp")]
	public class LayerController : Component {

		/// <ToBeCompleted></ToBeCompleted>
		public const String MenuItemNameAddLayer = "AddLayerAction";
		/// <ToBeCompleted></ToBeCompleted>
		public const String MenuItemNameAddMultipleLayers = "AddMultipleLayersAction";
		/// <ToBeCompleted></ToBeCompleted>
		public const String MenuItemNameDeleteLayers = "DeleteLayersAction";
		/// <ToBeCompleted></ToBeCompleted>
		public const String MenuItemNameRenameLayer = "RenameLayerAction";
		/// <ToBeCompleted></ToBeCompleted>
		public const String MenuItemNameActivateLayers = "ActivateLayersAction";
		/// <ToBeCompleted></ToBeCompleted>
		public const String MenuItemNameDeactivateLayers = "DeactivateLayersAction";
		/// <ToBeCompleted></ToBeCompleted>
		public const String MenuItemNameShowLayers = "ShowLayersAction";
		/// <ToBeCompleted></ToBeCompleted>
		public const String MenuItemNameHideLayers = "HideLayersAction";

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Controllers.LayerController" />.
		/// </summary>
		public LayerController() { }


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Controllers.LayerController" />.
		/// </summary>
		public LayerController(DiagramSetController diagramSetController)
			: this() {
			if (diagramSetController == null) throw new ArgumentNullException("diagramSetController");
			this.DiagramSetController = diagramSetController;
		}


		#region [Public] Events

		/// <summary>
		/// Occurs when the diagram of the LayerController has changed.
		/// </summary>
		public event EventHandler DiagramChanging;

		/// <summary>
		/// Raised when the diagram of the LayerController is going to change.
		/// </summary>
		public event EventHandler DiagramChanged;

		/// <summary>
		/// Occurs if layers has been added.
		/// </summary>
		public event EventHandler<LayersEventArgs> LayersAdded;

		/// <summary>
		/// Occurs if layers has been deleted.
		/// </summary>
		public event EventHandler<LayersEventArgs> LayersRemoved;

		/// <summary>
		/// Occurs if a layer has been modified.
		/// </summary>
		public event EventHandler<LayersEventArgs> LayerModified;

		#endregion


		#region [Public] Properties

		/// <summary>
		/// Specifies the version of the assembly containing the component.
		/// </summary>
		[CategoryNShape()]
		public string ProductVersion {
			get { return this.GetType().Assembly.GetName().Version.ToString(); }
		}


		/// <summary>
		/// Provides access to a DiagramSetController.
		/// </summary>
		[CategoryNShape()]
		public DiagramSetController DiagramSetController {
			get { return _diagramSetController; }
			set {
				if (_diagramSetController != null) UnregisterDiagramSetControllerEvents();
				_diagramSetController = value;
				if (_diagramSetController != null) RegisterDiagramSetControllerEvents();
			}
		}


		/// <summary>
		/// Provides access to a <see cref="T:Dataweb.NShape.Project" />.
		/// </summary>
		[CategoryNShape()]
		public Project Project {
			get {
				if (_diagramSetController == null) return null;
				else return _diagramSetController.Project;
			}
		}

		#endregion


		#region [Public] Methods

		/// <summary>
		/// Adds a new layer to the given diagram.
		/// </summary>
		public void AddLayer(Diagram diagram) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			AssertDiagramSetControllerIsSet();
			string newLayerName = GetNewLayerName(diagram);
			AddLayer(diagram, newLayerName);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void AddLayer(Diagram diagram, string layerName) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			if (layerName == null) throw new ArgumentNullException("layerName");
			AssertDiagramSetControllerIsSet();
			if (diagram.Layers.FindLayer(layerName) != null) 
				throw new NShapeException("Layer name '{0}' already exists.", layerName);
			Command cmd = new AddLayerCommand(diagram, layerName);
			Project.ExecuteCommand(cmd);
			if (LayersAdded != null) LayersAdded(this, LayerHelper.GetLayersEventArgs(diagram.Layers[layerName]));
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void RemoveLayers(Diagram diagram, IEnumerable<Layer> layers) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			if (layers == null) throw new ArgumentNullException("layers");
			AssertDiagramSetControllerIsSet();
			Command cmd = new RemoveLayerCommand(diagram, layers);
			Project.ExecuteCommand(cmd);
			if (LayersRemoved != null) LayersRemoved(this, LayerHelper.GetLayersEventArgs(layers));
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void RemoveLayer(Diagram diagram, string layerName) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			if (string.IsNullOrEmpty(layerName)) throw new ArgumentNullException("layerName");
			AssertDiagramSetControllerIsSet();
			Layer layer = diagram.Layers.FindLayer(layerName);
			if (layer == null) throw new NShapeException("Layer '{0}' does not exist.", layerName);
			Command cmd = new RemoveLayerCommand(diagram, layer);
			Project.ExecuteCommand(cmd);
			if (LayersRemoved != null) LayersRemoved(this, LayerHelper.GetLayersEventArgs(layer));
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void RenameLayer(Diagram diagram, Layer layer, string oldName, string newName) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			if (layer == null) throw new ArgumentNullException("layer");
			if (oldName == null) throw new ArgumentNullException("oldName");
			if (newName == null) throw new ArgumentNullException("newName");
			AssertDiagramSetControllerIsSet();
			ICommand cmd = new EditLayerCommand(diagram, layer, EditLayerCommand.ChangedProperty.Name, oldName, newName);
			Project.ExecuteCommand(cmd);
			if (LayerModified != null) LayerModified(this, LayerHelper.GetLayersEventArgs(layer));
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void SetLayerZoomBounds(Diagram diagram, Layer layer, int lowerZoomBounds, int upperZoomBounds) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			if (layer == null) throw new ArgumentNullException("layer");
			AssertDiagramSetControllerIsSet();
			ICommand cmdMinZoom = null;
			ICommand cmdMaxZoom = null;
			if (layer.LowerZoomThreshold != lowerZoomBounds)
				cmdMinZoom = new EditLayerCommand(diagram, layer, 
									EditLayerCommand.ChangedProperty.LowerZoomThreshold, 
									layer.LowerZoomThreshold, lowerZoomBounds);
			if (layer.UpperZoomThreshold != upperZoomBounds)
				cmdMaxZoom = new EditLayerCommand(diagram, layer, 
									EditLayerCommand.ChangedProperty.UpperZoomThreshold, 
									layer.UpperZoomThreshold, upperZoomBounds);
			
			ICommand cmd;
			if (cmdMinZoom != null && cmdMaxZoom != null) {
			    cmd = new AggregatedCommand();
			    ((AggregatedCommand)cmd).Add(cmdMinZoom);
			    ((AggregatedCommand)cmd).Add(cmdMaxZoom);
			} else if (cmdMinZoom != null && cmdMaxZoom == null)
			    cmd = cmdMinZoom;
			else if (cmdMaxZoom != null && cmdMinZoom == null)
			    cmd = cmdMaxZoom;
			else cmd = null;
			
			if (cmd != null) {
			    Project.ExecuteCommand(cmd);
			    if (LayerModified != null) LayerModified(this, LayerHelper.GetLayersEventArgs(layer));
			}
		}


		/// <summary>
		/// Returns a collection of <see cref="T:Dataweb.NShape.Advanced.MenuItemDef" /> for constructing context menus etc.
		/// </summary>
		public IEnumerable<MenuItemDef> GetMenuItemDefs(Diagram diagram, IReadOnlyCollection<Layer> selectedLayers) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			if (selectedLayers == null) throw new ArgumentNullException("selectedLayers");
			bool isFeasible;
			string description;

			isFeasible = true;
			description = Properties.Resources.MessageTxt_AddANewLayerToTheDiagram;
			yield return new DelegateMenuItemDef(MenuItemNameAddLayer, Properties.Resources.CaptionTxt_AddLayer,
				Properties.Resources.NewLayer, description, isFeasible, Permission.Data, diagram.SecurityDomainName,
				(a, p) => AddLayer(diagram));

			isFeasible = true;
			description = Properties.Resources.MessageTxt_AddMultipleNewLayersToTheDiagram;
			yield return new DelegateMenuItemDef(MenuItemNameAddMultipleLayers, Properties.Resources.CaptionTxt_AddMultipleLayers, 
				Properties.Resources.NewLayer, description, isFeasible, Permission.Data, diagram.SecurityDomainName,
				(a, p) => { for (int i = 0; i < 10; ++i) AddLayer(diagram);});

			isFeasible = selectedLayers.Count > 0;
			description = isFeasible ? string.Format(Properties.Resources.MessageFmt_Delete0Layers, selectedLayers.Count) : Properties.Resources.MessageTxt_NoLayersSelected;
			yield return new DelegateMenuItemDef(MenuItemNameDeleteLayers,
				(selectedLayers.Count > 1) ? Properties.Resources.CaptionTxt_DeleteLayer : Properties.Resources.CaptionTxt_DeleteLayers,
				Properties.Resources.DeleteBtn, description, isFeasible, Permission.Data, diagram.SecurityDomainName,
				(a, p) => this.RemoveLayers(diagram, selectedLayers)
			);
		}

		#endregion


		#region [Private] Methods

		private void RegisterDiagramSetControllerEvents() {
			_diagramSetController.ProjectChanged += diagramSetController_ProjectChanged;
			_diagramSetController.ProjectChanging += diagramSetController_ProjectChanging;
			if (_diagramSetController.Project != null) RegisterProjectEvents();
		}


		private void UnregisterDiagramSetControllerEvents() {
			if (_diagramSetController.Project != null) UnregisterProjectEvents();
			_diagramSetController.ProjectChanging -= diagramSetController_ProjectChanging;
			_diagramSetController.ProjectChanged -= diagramSetController_ProjectChanged;
		}


		private void RegisterProjectEvents() {
			AssertProjectIsSet();
			Project.Opened += project_ProjectOpen;
			Project.Closing += project_ProjectClosing;
			Project.Closed += project_ProjectClosed;
			if (Project.IsOpen) project_ProjectOpen(this, null);
		}

		
		private void UnregisterProjectEvents() {
			AssertProjectIsSet();
			if (Project.Repository != null)
				UnregisterRepositoryEvents();
			Project.Opened -= project_ProjectOpen;
			Project.Closing -= project_ProjectClosing;
			Project.Closed -= project_ProjectClosed;
		}


		private void RegisterRepositoryEvents() {
			AssertRepositoryIsSet();
			if (!_repositoryEventsRegistered) {
				Project.Repository.DiagramUpdated += Repository_DiagramUpdated;
				_repositoryEventsRegistered = true;
			}
		}


		private void UnregisterRepositoryEvents() {
			AssertRepositoryIsSet();
			if (_repositoryEventsRegistered) {
				Project.Repository.DiagramUpdated -= Repository_DiagramUpdated;
				_repositoryEventsRegistered = false;
			}
		}
		

		private void AssertProjectIsSet() {
			if (Project == null) throw new NShapeException("{0}'s property 'Project' is not set.", typeof(DiagramSetController).FullName);
		}


		private void AssertRepositoryIsSet() {
			AssertProjectIsSet();
			if (Project.Repository == null) throw new NShapeException("Project's 'Repository' property is not set.");
		}

		
		private void AssertDiagramSetControllerIsSet() {
			if (_diagramSetController == null) throw new NShapeException("Property 'DiagramController' is not set.");
		}


		private string GetNewLayerName(Diagram diagram) {
			int nextLayerId = diagram.Layers.GetNextAvailableLayerId();
			return string.Format("Layer {0:D2}", nextLayerId);
		}

		#endregion


		#region [Private] Methods: EventHandler implementations

		private void diagramSetController_ProjectChanging(object sender, EventArgs e) {
			if (_diagramSetController.Project != null) UnregisterProjectEvents();
		}


		private void diagramSetController_ProjectChanged(object sender, EventArgs e) {
			if (_diagramSetController.Project != null) RegisterProjectEvents();
		}


		private void project_ProjectOpen(object sender, EventArgs e) {
			AssertRepositoryIsSet();
			RegisterRepositoryEvents();
		}


		private void project_ProjectClosing(object sender, EventArgs e) {
			// nothing to do...
		}


		private void project_ProjectClosed(object sender, EventArgs e) {
			AssertRepositoryIsSet();
			UnregisterRepositoryEvents();
		}


		private void Repository_DiagramUpdated(object sender, RepositoryDiagramEventArgs e) {
			// This is only a simple workaround.
			// ToDo: Create a new event for this purpose!
			if (DiagramChanging != null && DiagramChanged != null) {
				DiagramChanging(this, EventArgs.Empty);
				DiagramChanged(this, EventArgs.Empty);
			}
		}


		private void diagrammController_ActiveLayersChanged(object sender, LayersEventArgs e) {
			if (LayerModified != null) LayerModified(this, e);
		}


		private void diagramController_LayerVisibilityChanged(object sender, LayersEventArgs e) {
			if (LayerModified != null) LayerModified(this, e);
		}


		private void diagramController_DiagramChanging(object sender, EventArgs e) {
			if (DiagramChanging != null) DiagramChanging(this, e);
		}


		private void diagramController_DiagramChanged(object sender, EventArgs e) {
			if (DiagramChanged != null) DiagramChanged(this, e);
		}

		#endregion
	

		#region Fields
		
		/// <ToBeCompleted></ToBeCompleted>
		public const int MaxLayerCount = 31;

		private DiagramSetController _diagramSetController = null;
		private bool _repositoryEventsRegistered = false;

		private LayersEventArgs _layersEventArgs = new LayersEventArgs();
		private LayerEventArgs _layerEventArgs = new LayerEventArgs();
		private LayerRenamedEventArgs _layerRenamedEventArgs = new LayerRenamedEventArgs();

		#endregion
	}

}
