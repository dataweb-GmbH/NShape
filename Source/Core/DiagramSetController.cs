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

using Dataweb.NShape.Advanced;
using Dataweb.NShape.Commands;


namespace Dataweb.NShape.Controllers {

	/// <summary>
	/// Controls the behavior of a set of diagrams.
	/// </summary>
	[ToolboxItem(true)]
	[ToolboxBitmap(typeof(DiagramSetController), "DiagramSetController.bmp")]
	public class DiagramSetController : Component {

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Controllers.DiagramSetController" />.
		/// </summary>
		public DiagramSetController() {
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Controllers.DiagramSetController" />.
		/// </summary>
		public DiagramSetController(Project project)
			: this() {
			if (project == null) throw new ArgumentNullException(nameof(project));
			Project = project;
		}


		#region [Public] Events

		/// <ToBeCompleted></ToBeCompleted>
		public event EventHandler ProjectChanging;

		/// <ToBeCompleted></ToBeCompleted>
		public event EventHandler ProjectChanged;

		/// <ToBeCompleted></ToBeCompleted>
		public event EventHandler ToolChanged;

		/// <ToBeCompleted></ToBeCompleted>
		public event EventHandler<ModelObjectsEventArgs> SelectModelObjectsRequested;

		/// <ToBeCompleted></ToBeCompleted>
		public event EventHandler<DiagramEventArgs> DiagramAdded;

		/// <ToBeCompleted></ToBeCompleted>
		public event EventHandler<DiagramEventArgs> DiagramRemoved;

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
		/// Provides access to a <see cref="T:Dataweb.NShape.Project" />.
		/// </summary>
		[CategoryNShape()]
		public Project Project {
			get { return _project; }
			set {
				if (ProjectChanging != null) ProjectChanging(this, EventArgs.Empty);
				if (_project != null) UnregisterProjectEvents();
				_project = value;
				if (_project != null) RegisterProjectEvents();
				if (ProjectChanged != null) ProjectChanged(this, EventArgs.Empty);
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		[Browsable(false)]
		public Tool ActiveTool {
			get { return _tool; }
			set {
				_tool = value;
				if (ToolChanged != null) ToolChanged(this, EventArgs.Empty);
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		[Browsable(false)]
		public IEnumerable<Diagram> Diagrams {
			get {
				foreach (DiagramController diagramController in _diagramControllers)
					yield return diagramController.Diagram;
			}
		}

		#endregion


		#region [Public] Methods

		/// <ToBeCompleted></ToBeCompleted>
		public Diagram CreateDiagram(string name) {
			if (name == null) throw new ArgumentNullException(nameof(name));
			AssertProjectIsOpen();
			// Create new diagram
			Diagram diagram = new Diagram(name);
			diagram.Width = 1000;
			diagram.Height = 1000;
			ICommand cmd = new CreateDiagramCommand(diagram);
			_project.ExecuteCommand(cmd);
			return diagram;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void CloseDiagram(string name) {
			if (name == null) throw new ArgumentNullException(nameof(name));
			AssertProjectIsOpen();
			int idx = IndexOf(name);
			if (idx >= 0) {
				DiagramEventArgs eventArgs = GetDiagramEventArgs(_diagramControllers[idx].Diagram);
				_diagramControllers.RemoveAt(idx);
				if (DiagramRemoved != null) DiagramRemoved(this, eventArgs);
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void CloseDiagram(Diagram diagram) {
			if (diagram == null) throw new ArgumentNullException(nameof(diagram));
			AssertProjectIsOpen();
			int idx = DiagramControllerIndexOf(diagram);
			if (idx >= 0) {
				DiagramController controller = _diagramControllers[idx];
				_diagramControllers.RemoveAt(idx);

				DiagramEventArgs eventArgs = GetDiagramEventArgs(controller.Diagram);
				if (DiagramRemoved != null) DiagramRemoved(this, eventArgs);
				controller.Diagram = null;
				controller = null;
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void DeleteDiagram(string name) {
			if (name == null) throw new ArgumentNullException(nameof(name));
			AssertProjectIsOpen();
			int idx = IndexOf(name);
			if (idx >= 0) {
				DiagramController controller = _diagramControllers[idx];
				ICommand cmd = new DeleteDiagramCommand(controller.Diagram);
				_project.ExecuteCommand(cmd);
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void DeleteDiagram(Diagram diagram) {
			if (diagram == null) throw new ArgumentNullException(nameof(diagram));
			AssertProjectIsOpen();
			int idx = DiagramControllerIndexOf(diagram);
			if (idx >= 0) {
				DiagramController controller = _diagramControllers[idx];
				ICommand cmd = new DeleteDiagramCommand(controller.Diagram);
				_project.ExecuteCommand(cmd);
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void SelectModelObjects(IEnumerable<IModelObject> modelObjects) {
			if (SelectModelObjectsRequested != null) 
				SelectModelObjectsRequested(this, GetModelObjectsEventArgs(modelObjects));
		}


		/// <summary>
		/// Inserts the given shape in the given diagram and adds it to the given layers.
		/// </summary>
		/// <remarks>
		/// The ZOrder of the shape will not be changed! 
		/// If the shape does not have a defined ZOrder, set it after inserting the shape by calling diagram.Shapes.SetZorder().
		/// </remarks>
		[Obsolete("Use an overloaded version taking home layer and supplemental layers instead.")]
		public void InsertShape(Diagram diagram, Shape shape, LayerIds activeLayers, bool withModelObjects) {
			if (diagram == null) throw new ArgumentNullException(nameof(diagram));
			if (shape == null) throw new ArgumentNullException(nameof(shape));
			InsertShapes(diagram, SingleInstanceEnumerator<Shape>.Create(shape), Layer.NoLayerId, activeLayers, withModelObjects);
		}


		/// <summary>
		/// Inserts the given shapes in the given diagram and adds them to the given layers.
		/// </summary>
		/// <remarks>
		/// The ZOrder of the shapes will not be changed! 
		/// If the shapes do not have a defined ZOrder (e.g. after creation), set it after inserting by calling diagram.Shapes.SetZorder().
		/// </remarks>
		public void InsertShapes(Diagram diagram, IEnumerable<Shape> shapes, int activeHomeLayer, LayerIds activeLayers, bool withModelObjects) {
			if (diagram == null) throw new ArgumentNullException(nameof(diagram));
			if (shapes == null) throw new ArgumentNullException(nameof(shapes));
			ICommand cmd = new CreateShapesCommand(diagram, activeHomeLayer, activeLayers, shapes, withModelObjects, true);
			Project.ExecuteCommand(cmd);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void DeleteShapes(Diagram diagram, Shape shape, bool withModelObjects) {
			if (diagram == null) throw new ArgumentNullException(nameof(diagram));
			if (shape == null) throw new ArgumentNullException(nameof(shape));
			DeleteShapes(diagram, SingleInstanceEnumerator<Shape>.Create(shape), withModelObjects);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void DeleteShapes(Diagram diagram, IEnumerable<Shape> shapes, bool withModelObjects) {
			if (diagram == null) throw new ArgumentNullException(nameof(diagram));
			if (shapes == null) throw new ArgumentNullException(nameof(shapes));
			if (withModelObjects) {
				foreach (Shape s in shapes) {
					if (!diagram.Shapes.Contains(s))
						throw new NShapeException(Dataweb.NShape.Properties.Resources.MessageTxt_OneOfTheGivenShapesIsNotPartOfTheGivenDiagram);
					if (s.ModelObject != null && s.ModelObject.ShapeCount > 1) {
						string messageText = string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_01CanNotBeDeletedWhileMoreThanOneShapesReferToIt,
															s.ModelObject.Type.Name, s.ModelObject.Name);
						throw new NShapeException(messageText);
					}
				}
			}
			ICommand cmd = new DeleteShapesCommand(diagram, shapes, withModelObjects);
			Project.ExecuteCommand(cmd);
		}


		/// <summary>
		/// Creates and executes an (undoable) command that moves the given shapes by the specified delta.
		/// </summary>
		public void MoveShapes(Diagram diagram, IEnumerable<Shape> shapes, int deltaX, int deltaY) {
			if (diagram == null) throw new ArgumentNullException(nameof(diagram));
			if (shapes == null) throw new ArgumentNullException(nameof(shapes));
			ICommand cmd = new MoveShapeByCommand(shapes, deltaX, deltaY);
			Project.ExecuteCommand(cmd);
		}


		/// <summary>
		/// Copies the specified shapes of the specified diagram.
		/// </summary>
		/// <param name="diagram">The diagram that contains the shapes to be copied.</param>
		/// <param name="shapes">The shapes to be copied.</param>
		/// <param name="withModelObjects">Specifies whether assigned model objects should be copied, too</param>
		public void Copy(Diagram diagram, IEnumerable<Shape> shapes, bool withModelObjects) {
			Copy(diagram, shapes, withModelObjects, Geometry.InvalidPoint);
		}


		/// <summary>
		/// Copies the specified shapes of the specified diagram.
		/// </summary>
		/// <param name="diagram">The diagram that contains the shapes to be copied.</param>
		/// <param name="shapes">The shapes to be copied.</param>
		/// <param name="withModelObjects">Specifies whether assigned model objects should be copied, too</param>
		/// <param name="position">Mouse cursor position in diagram coordinates.</param>
		public void Copy(Diagram diagram, IEnumerable<Shape> shapes, bool withModelObjects, Point position) {
			if (diagram == null) throw new ArgumentNullException(nameof(diagram));
			if (shapes == null) throw new ArgumentNullException(nameof(shapes));

			_editBuffer.Clear();
			_editBuffer.action = EditAction.Copy;
			_editBuffer.withModelObjects = withModelObjects;
			_editBuffer.initialMousePos = position;
			_editBuffer.shapes.AddRange(shapes);
			
			// We have to copy the shapes immediately because shapes (and/or model objects) may 
			// be deleted after they are copied to 'clipboard'.
			// Copy shapes:
			// Use the ShapeCollection's Clone method in order to maintain connections 
			// between shapes inside the collection
			_editBuffer.shapes = _editBuffer.shapes.Clone(withModelObjects);
		}


		/// <summary>
		/// Cuts the specified shapes out of the specified diagram.
		/// </summary>
		/// <param name="diagram">The diagram that contains the shapes to be cut out.</param>
		/// <param name="shapes">The shapes to be cut out.</param>
		/// <param name="withModelObjects">Specifies whether assigned model objects should be cut, too</param>
		public void Cut(Diagram diagram, IEnumerable<Shape> shapes, bool withModelObjects) {
			Cut(diagram, shapes, withModelObjects, Geometry.InvalidPoint);
		}


		/// <summary>
		/// Cuts the specified shapes out of the specified diagram.
		/// </summary>
		/// <param name="diagram">The diagram that contains the shapes to be cut out.</param>
		/// <param name="shapes">The shapes to be cut out.</param>
		/// <param name="withModelObjects">Specifies whether assigned model objects should be cut, too</param>
		/// <param name="position">Mouse cursor position in diagram coordinates.</param>
		public void Cut(Diagram diagram, IEnumerable<Shape> shapes, bool withModelObjects, Point position) {
			if (diagram == null) throw new ArgumentNullException(nameof(diagram));
			if (shapes == null) throw new ArgumentNullException(nameof(shapes));
			if (_project == null) throw new InvalidOperationException("Property Project not set!");
			Dictionary<Shape, IModelObject> cutModelObjects = null;

			_editBuffer.Clear();
			_editBuffer.action = EditAction.Cut;
			_editBuffer.withModelObjects = withModelObjects;
			_editBuffer.initialMousePos = position;
			_editBuffer.shapes.AddRange(shapes);
			// Store model objects and shape connections
			foreach (Shape s in _editBuffer.shapes) {
				// Store model objects before they will be deleted
				if (withModelObjects && s.ModelObject != null) {
					if (cutModelObjects == null) cutModelObjects = new Dictionary<Shape, IModelObject>();
					cutModelObjects.Add(s, s.ModelObject);
				}
				// Store all connections of *active* shapes to other shapes that were also cut
				foreach (ShapeConnectionInfo ci in s.GetConnectionInfos(ControlPointId.Any, null)) {
					// Skip shapes that are not the active shapes
					if (s.HasControlPointCapability(ci.OwnPointId, ControlPointCapabilities.Glue) == false)
						continue;
					// Skip connections to shapes that were not cut
					if (!_editBuffer.shapes.Contains(ci.OtherShape))
						continue;
					// Store connection of active shape
					ShapeConnection conn = ShapeConnection.Empty;
					conn.ConnectorShape = s;
					conn.GluePointId = ci.OwnPointId;
					conn.TargetShape = ci.OtherShape;
					conn.TargetPointId = ci.OtherPointId;
					_editBuffer.connections.Add(conn);
				}
			}

			ICommand cmd = new DeleteShapesCommand(diagram, _editBuffer.shapes, withModelObjects);
			_project.ExecuteCommand(cmd);

			// Restore deleted model objects so they are available for pasting (one or more times)
			if (withModelObjects && cutModelObjects != null) {
				foreach (KeyValuePair<Shape, IModelObject> item in cutModelObjects)
					item.Key.ModelObject = item.Value;
			}
		}


		/// <summary>
		/// Inserts the previously copied/cut shapes into the specified diagram.
		/// </summary>
		/// <param name="diagram">The diagram where to insert the previously cut/copied shapes.</param>
		/// <param name="activeLayers">The layers which the inserted shapes will be assigned to.</param>
		[Obsolete("Use an overloaded version taking home layer and supplemental layers instead.")]
		public void Paste(Diagram diagram, LayerIds activeLayers) {
			Paste(diagram, Layer.NoLayerId, activeLayers);
		}


		/// <summary>
		/// Inserts the previously copied/cut shapes into the specified diagram.
		/// </summary>
		/// <param name="diagram">The diagram where to insert the previously cut/copied shapes.</param>
		/// <param name="homeLayer">The home layer for the inserted shapes.</param>
		/// <param name="supplementalLayers">The layers which the inserted shapes will be assigned to.</param>
		public void Paste(Diagram diagram, int homeLayer, LayerIds supplementalLayers) {
			Paste(diagram, homeLayer, supplementalLayers, 20, 20);
		}


		/// <summary>
		/// Inserts the previously copied/cut shapes into the specified diagram.
		/// </summary>
		/// <param name="diagram">The diagram where to insert the previously cut/copied shapes.</param>
		/// <param name="activeLayers">The layers which the inserted shapes will be assigned to.</param>
		/// <param name="position">Specifies the mouse cursor position in diagram coordinates where the shapes should be inserted.</param>
		/// <remarks>Inserting at the specified position will only work if the shapes were copied/cut with a valid position.</remarks>
		[Obsolete("Use an overloaded version taking home layer and supplemental layers instead.")]
		public void Paste(Diagram diagram, LayerIds activeLayers, Point position) {
			Paste(diagram, Layer.NoLayerId, activeLayers, position);
		}


		/// <summary>
		/// Inserts the previously copied/cut shapes into the specified diagram.
		/// </summary>
		/// <param name="diagram">The diagram where to insert the previously cut/copied shapes.</param>
		/// <param name="homeLayer">The home layer for the inserted shapes.</param>
		/// <param name="supplementalLayers">The layers which the inserted shapes will be assigned to.</param>
		/// <param name="position">Specifies the mouse cursor position in diagram coordinates where the shapes should be inserted.</param>
		/// <remarks>Inserting at the specified position will only work if the shapes were copied/cut with a valid position.</remarks>
		public void Paste(Diagram diagram, int homeLayer, LayerIds supplementalLayers, Point position) {
			if (!_editBuffer.IsEmpty) {
				int dx = 40, dy = 40;
				if (Geometry.IsValid(position)) {
					Rectangle rect = _editBuffer.shapes.GetBoundingRectangle(true);
					dx = position.X - (rect.X + (rect.Width / 2));
					dy = position.Y - (rect.Y + (rect.Height / 2));
				}
				Paste(diagram, homeLayer, supplementalLayers, dx, dy);
				_editBuffer.initialMousePos = position;
			}
		}


		/// <summary>
		/// Inserts the previously copied/cut shapes into the specified diagram.
		/// </summary>
		/// <param name="diagram">The diagram where to insert the previously cut/copied shapes.</param>
		/// <param name="activeLayers">The layers which the inserted shapes will be assigned to.</param>
		/// <param name="offsetX">Specifies the offset on the X axis in diagram coordinates that will be applied to the inserted shape's position.</param>
		/// <param name="offsetY">Specifies the offset on the Y axis in diagram coordinates that will be applied to the inserted shape's position.</param>
		[Obsolete("Use an overloaded version taking home layer and supplemental layers instead.")]
		public void Paste(Diagram diagram, LayerIds activeLayers, int offsetX, int offsetY) {
			Paste(diagram, Layer.NoLayerId, activeLayers, offsetX, offsetX);
		}


		/// <summary>
		/// Inserts the previously copied/cut shapes into the specified diagram.
		/// </summary>
		/// <param name="diagram">The diagram where to insert the previously cut/copied shapes.</param>
		/// <param name="homeLayer">The home layer for the inserted shapes.</param>
		/// <param name="supplementalLayers">The layers which the inserted shapes will be assigned to.</param>
		/// <param name="offsetX">Specifies the offset on the X axis in diagram coordinates that will be applied to the inserted shape's position.</param>
		/// <param name="offsetY">Specifies the offset on the Y axis in diagram coordinates that will be applied to the inserted shape's position.</param>
		public void Paste(Diagram diagram, int homeLayer, LayerIds supplementalLayers, int offsetX, int offsetY) {
			if (diagram == null) throw new ArgumentNullException(nameof(diagram));
			if (!_editBuffer.IsEmpty) {
				++_editBuffer.pasteCount;

				// Check if there are connections to restore
				if (_editBuffer.connections.Count > 0) {
					// Restore connections of cut shapes at first paste action.
					foreach (ShapeConnection connInfo in _editBuffer.connections) {
						connInfo.ConnectorShape.Connect(
							connInfo.GluePointId,
							connInfo.TargetShape,
							connInfo.TargetPointId);
					}
					// After the paste action, the (then connected) shapes are cloned 
					// with their connections, so we can empty the buffer here.
					_editBuffer.connections.Clear();
				}
				// Create command
				ICommand cmd = new CreateShapesCommand(
					diagram,
					homeLayer,
					supplementalLayers,
					_editBuffer.shapes,
					_editBuffer.withModelObjects,
					(_editBuffer.action == EditAction.Cut),
					offsetX,
					offsetY);
				// Execute InsertCommand and select inserted shapes
				Project.ExecuteCommand(cmd);

				// Clone shapes for another paste operation
				// We have to copy the shapes immediately because shapes (and/or model objects) may 
				// be deleted after they are copied to 'clipboard'.
				_editBuffer.shapes = _editBuffer.shapes.Clone(_editBuffer.withModelObjects);
				if (_editBuffer.action == EditAction.Cut) _editBuffer.action = EditAction.Copy;
			}
		}


		/// <summary>
		/// Aggregates the given shapes to a group.
		/// </summary>
		[Obsolete("Use an overloaded version taking home layer and supplemental layers instead.")]
		public void GroupShapes(Diagram diagram, IEnumerable<Shape> shapes, LayerIds activeLayers) {
			GroupShapes(diagram, shapes, Layer.NoLayerId, activeLayers);
		}


		/// <summary>
		/// Aggregates the given shapes to a group.
		/// </summary>
		public void GroupShapes(Diagram diagram, IEnumerable<Shape> shapes, int homeLayer, LayerIds supplementalLayers) {
			if (diagram == null) throw new ArgumentNullException(nameof(diagram));
			if (shapes == null) throw new ArgumentNullException(nameof(shapes));
			int cnt = 0;
			foreach (Shape s in shapes)
				if (++cnt > 1) break;
			if (cnt > 1) {
				ShapeType groupShapeType = Project.ShapeTypes["ShapeGroup"];
				Debug.Assert(groupShapeType != null);

				Shape groupShape = groupShapeType.CreateInstance();
				ICommand cmd = new GroupShapesCommand(diagram, homeLayer, supplementalLayers, groupShape, shapes);
				Project.ExecuteCommand(cmd);
			}
		}


		/// <summary>
		/// Aggregate the given shapes to a composite shape based on the bottom shape.
		/// </summary>
		[Obsolete("Use an overloaded version taking home layer and supplemental layers instead.")]
		public void AggregateCompositeShape(Diagram diagram, Shape compositeShape, IEnumerable<Shape> shapes, LayerIds layers) {
			AggregateCompositeShape(diagram, compositeShape, shapes, Layer.NoLayerId, layers);
		}


		/// <summary>
		/// Aggregate the given shapes to a composite shape based on the bottom shape.
		/// </summary>
		public void AggregateCompositeShape(Diagram diagram, Shape compositeShape, IEnumerable<Shape> shapes, int homeLayer, LayerIds supplementalLayers) {
			if (compositeShape == null) throw new ArgumentNullException(nameof(compositeShape));
			if (shapes == null) throw new ArgumentNullException(nameof(shapes));
			// Add shapes to buffer (TopDown)
			_shapeBuffer.Clear();
			foreach (Shape shape in shapes) {
				if (shape == compositeShape) continue;
				_shapeBuffer.Add(shape);
			}
			ICommand cmd = new AggregateCompositeShapeCommand(diagram, homeLayer, supplementalLayers, compositeShape, _shapeBuffer);
			Project.ExecuteCommand(cmd);
			_shapeBuffer.Clear();
		}


		/// <summary>
		/// Ungroups the given shape group
		/// </summary>
		public void UngroupShapes(Diagram diagram, Shape groupShape) {
			if (diagram == null) throw new ArgumentNullException(nameof(diagram));
			if (groupShape == null) throw new ArgumentNullException(nameof(groupShape));
			if (!(groupShape is IShapeGroup))
				throw new ArgumentException(string.Format(Properties.Resources.MessageFmt_GroupShapeDoesNotImplpementInterface0, typeof(IShapeGroup).Name));
			// Add grouped shapes to shape buffer for selecting them later
			_shapeBuffer.Clear();
			_shapeBuffer.AddRange(groupShape.Children);

			ICommand cmd = new UngroupShapesCommand(diagram, groupShape);
			Project.ExecuteCommand(cmd);

			_shapeBuffer.Clear();
		}


		/// <summary>
		/// Splits the given composite shape into independent shapes.
		/// </summary>
		public void SplitCompositeShape(Diagram diagram, Shape compositeShape) {
			if (diagram == null) throw new ArgumentNullException(nameof(diagram));
			if (compositeShape == null) throw new ArgumentNullException(nameof(compositeShape));
			if (compositeShape == null) throw new ArgumentNullException(nameof(compositeShape));
			Debug.Assert(!(compositeShape is IShapeGroup));
			// Add grouped shapes to shape buffer for selecting them later
			_shapeBuffer.Clear();
			_shapeBuffer.AddRange(compositeShape.Children);
			_shapeBuffer.Add(compositeShape);

			ICommand cmd = new SplitCompositeShapeCommand(diagram, compositeShape.HomeLayer, compositeShape.SupplementalLayers, compositeShape);
			Project.ExecuteCommand(cmd);

			_shapeBuffer.Clear();
		}


		/// <summary>
		/// Adds the given shapes to the given layers.
		/// </summary>
		[Obsolete("Use an overloaded version taking home layer and supplemental layers instead.")]
		public void AddShapesToLayers(Diagram diagram, IEnumerable<Shape> shapes, LayerIds layers) {
			AddShapesToLayers(diagram, shapes, Layer.NoLayerId, layers);
		}


		/// <summary>
		/// Adds the given shapes to the given layers.
		/// </summary>
		public void AddShapesToLayers(Diagram diagram, IEnumerable<Shape> shapes, int homeLayer, LayerIds supplementalLayers) {
			if (diagram == null) throw new ArgumentNullException(nameof(diagram));
			if (shapes == null) throw new ArgumentNullException(nameof(shapes));
			ICommand cmd = new AddShapesToLayersCommand(diagram, shapes, homeLayer, supplementalLayers);
			Project.ExecuteCommand(cmd);
		}


		/// <summary>
		/// Assigns the given shapes to the given layers. If the shape was assigned to layers, these will be replaced.
		/// </summary>
		[Obsolete("Use an overloaded version taking home layer and supplemental layers instead.")]
		public void AssignShapesToLayers(Diagram diagram, IEnumerable<Shape> shapes, LayerIds layers) {
			AssignShapesToLayers(diagram, shapes, Layer.NoLayerId, layers);
		}


		/// <summary>
		/// Assigns the given shapes to the given layers. If the shape was assigned to layers, these will be replaced.
		/// </summary>
		public void AssignShapesToLayers(Diagram diagram, IEnumerable<Shape> shapes, int homeLayer, LayerIds supplementalLayers) {
			if (diagram == null) throw new ArgumentNullException(nameof(diagram));
			if (shapes == null) throw new ArgumentNullException(nameof(shapes));

			ICommand cmd = new AssignShapesToLayersCommand(diagram, shapes, homeLayer, supplementalLayers);
			Project.ExecuteCommand(cmd);
		}


		/// <summary>
		/// Removes the given shapes from all layers.
		/// </summary>
		public void RemoveShapesFromLayers(Diagram diagram, IEnumerable<Shape> shapes) {
			RemoveShapesFromLayers(diagram, shapes, LayerHelper.GetAllLayerIds(diagram.Layers));
		}


		/// <summary>
		/// Removes the given shapes from the given layers.
		/// </summary>
		[Obsolete("Use an overloaded version taking home layer and supplemental layers instead.")]
		public void RemoveShapesFromLayers(Diagram diagram, IEnumerable<Shape> shapes, LayerIds layers) {
			RemoveShapesFromLayers(diagram, shapes, LayerHelper.GetAllLayerIds(layers));
		}


		/// <summary>
		/// Removes the given shapes from the given layers.
		/// </summary>
		public void RemoveShapesFromLayers(Diagram diagram, IEnumerable<Shape> shapes, IEnumerable<int> layerIds) {
			if (diagram == null) throw new ArgumentNullException(nameof(diagram));
			if (shapes == null) throw new ArgumentNullException(nameof(shapes));

			ICommand cmd = new RemoveShapesFromLayersCommand(diagram, shapes);
			Project.ExecuteCommand(cmd);
		}


		/// <summary>
		/// Lists one shape on top or to bottom
		/// </summary>
		public void LiftShape(Diagram diagram, Shape shape, ZOrderDestination liftMode) {
			if (diagram == null) throw new ArgumentNullException(nameof(diagram));
			if (shape == null) throw new ArgumentNullException(nameof(shape));
			ICommand cmd = null;
			cmd = new LiftShapeCommand(diagram, shape, liftMode);
			Project.ExecuteCommand(cmd);
		}


		/// <summary>
		/// Lifts a collection of shapes on top or to bottom.
		/// </summary>
		public void LiftShapes(Diagram diagram, IEnumerable<Shape> shapes, ZOrderDestination liftMode) {
			if (diagram == null) throw new ArgumentNullException(nameof(diagram));
			if (shapes == null) throw new ArgumentNullException(nameof(shapes));
			ICommand cmd = new LiftShapeCommand(diagram, shapes, liftMode);
			Project.ExecuteCommand(cmd);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public bool CanInsertShapes(Diagram diagram, IEnumerable<Shape> shapes, out string reason) {
			if (diagram == null) throw new ArgumentNullException(nameof(diagram));
			if (shapes == null) throw new ArgumentNullException(nameof(shapes));
			reason = null;
			if (!Project.SecurityManager.IsGranted(Permission.Insert, shapes)) {
				reason = string.Format(Properties.Resources.MessageFmt_Permission0NotGranted, Permission.Insert);
				return false;
			} else if (diagram.Shapes.ContainsAny(shapes)) {
				reason = Properties.Resources.MessageTxt_DiagramAlreadyContainsAtLeastOneOfTheShapesToBeInserted;
				return false;
			} else return true;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public bool CanInsertShapes(Diagram diagram, IEnumerable<Shape> shapes) {
			string reason;
			return CanInsertShapes(diagram, shapes, out reason);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public bool CanDeleteShapes(Diagram diagram, IEnumerable<Shape> shapes) {
			if (diagram == null) throw new ArgumentNullException(nameof(diagram));
			if (shapes == null) throw new ArgumentNullException(nameof(shapes));
			return Project.SecurityManager.IsGranted(Permission.Delete, shapes)
				&& diagram.Shapes.ContainsAll(shapes);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public bool CanMoveShapes(Diagram diagram, IEnumerable<Shape> shapes) {
			if (!Project.SecurityManager.IsGranted(Permission.Layout, shapes))
				return false;
			else {
				foreach (Shape s in shapes) {
					// Check if the non-selected shape at cursor (which will be selected) owns glue points connected to other shapes
					foreach (ControlPointId id in s.GetControlPointIds(ControlPointCapabilities.Glue))
						if (s.IsConnected(id, null) != ControlPointId.None) 
							return false;
				}
				return true;
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		public bool CanCut(Diagram diagram, IEnumerable<Shape> shapes) {
			if (diagram == null) throw new ArgumentNullException(nameof(diagram));
			if (shapes == null) throw new ArgumentNullException(nameof(shapes));
			return CanCopy(shapes) && CanDeleteShapes(diagram, shapes);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public bool CanCopy(IEnumerable<Shape> shapes) {
			if (shapes == null) throw new ArgumentNullException(nameof(shapes));
			// Check if shapes is not an empty collection
			foreach (Shape s in shapes) return true;
			return false;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public bool CanPaste(Diagram diagram) {
			string reason;
			return CanPaste(diagram, out reason);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public bool CanPaste(Diagram diagram, out string reason) {
			if (diagram == null) throw new ArgumentNullException(nameof(diagram));
			reason = null;
			if (_editBuffer.IsEmpty) {
				reason = Properties.Resources.MessageTxt_NoShapesCutOrCopied;
				return false;
			} else {
				if (_editBuffer.action != EditAction.Copy)
					if (!CanInsertShapes(diagram, _editBuffer.shapes))
						return false;
				if (!Project.SecurityManager.IsGranted(Permission.Insert, _editBuffer.shapes)) {
					reason = string.Format(Properties.Resources.MessageFmt_Permission0NotGranted, Permission.Insert);
					return false;
				} else return true;
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		public bool CanGroupShapes(IEnumerable<Shape> shapes) {
			if (shapes == null) throw new ArgumentNullException(nameof(shapes));
			int cnt= 0;
			foreach (Shape shape in shapes) {
				++cnt;
				if (cnt >= 2) return Project.SecurityManager.IsGranted(Permission.Delete, shapes);
			}
			return false;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public bool CanUngroupShape(Diagram diagram, IEnumerable<Shape> shapes, out string reason) {
			if (diagram == null) throw new ArgumentNullException(nameof(diagram));
			if (shapes == null) throw new ArgumentNullException(nameof(shapes));
			foreach (Shape shape in shapes) {
				if (shape is IShapeGroup && shape.Parent == null)
					return CanInsertShapes(diagram, shape.Children, out reason);
				else {
					reason = Properties.Resources.MessageTxt_ShapeIsNotAGroupShape;
					return false;
				}
			}
			reason = Properties.Resources.MessageFmt_NoShapesToUngroup;
			return false;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public bool CanUngroupShape(Diagram diagram, IEnumerable<Shape> shapes) {
			string reason;
			return CanUngroupShape(diagram, shapes, out reason);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public bool CanAggregateShapes(Diagram diagram, IReadOnlyShapeCollection shapes) {
			string reason;
			return CanAggregateShapes(diagram, shapes, out reason);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public bool CanAggregateShapes(Diagram diagram, IReadOnlyShapeCollection shapes, out string reason) {
			if (diagram == null) throw new ArgumentNullException(nameof(diagram));
			if (shapes == null) throw new ArgumentNullException(nameof(shapes));
			reason = null;
			Shape compositeShape = shapes.Bottom;
			if (shapes.Count <= 1)
				reason = Properties.Resources.MessageFmt_NoShapesSelected;
			else if (shapes.Count <= 1)
				reason = Properties.Resources.MessageFmt_NotEnoughShapesSelected;
			else if (!CanDeleteShapes(diagram, shapes))
				reason = string.Format(Properties.Resources.MessageFmt_Permission0NotGranted, Permission.Delete);
			else if (!Project.SecurityManager.IsGranted(Permission.Insert, compositeShape))
				reason = string.Format(Properties.Resources.MessageFmt_Permission0NotGranted, Permission.Insert);
			else if (compositeShape is IShapeGroup)
				reason = Properties.Resources.MessageTxt_GroupsCannotBeAggregated;
			return string.IsNullOrEmpty(reason);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public bool CanSplitShapeAggregation(Diagram diagram, IReadOnlyShapeCollection shapes) {
			string reason;
			return CanSplitShapeAggregation(diagram, shapes, out reason);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public bool CanSplitShapeAggregation(Diagram diagram, IReadOnlyShapeCollection shapes, out string reason) {
			if (diagram == null) throw new ArgumentNullException(nameof(diagram));
			if (shapes == null) throw new ArgumentNullException(nameof(shapes));
			reason = null;
			Shape compositeShape = shapes.TopMost;
			if (shapes.Count == 0)
				reason = Properties.Resources.MessageFmt_NoShapesSelected;
			else if (shapes.Count > 1)
				reason = Properties.Resources.MessageTxt_TooManyShapesSelected;
			else if (compositeShape is IShapeGroup)
				reason = Properties.Resources.MessageTxt_GroupsCannotBeDisaggregated;
			else if (compositeShape.Children.Count == 0)
				reason = Properties.Resources.MessageTxt_ShapeIsNotACompositeShape;
			else if (!CanInsertShapes(diagram, compositeShape.Children))
				reason = string.Format(Properties.Resources.MessageFmt_Permission0NotGranted, Permission.Insert);
			else if (!Project.SecurityManager.IsGranted(Permission.Delete, compositeShape))
				reason = string.Format(Properties.Resources.MessageFmt_Permission0NotGranted, Permission.Delete);
			return string.IsNullOrEmpty(reason);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public bool CanLiftShapes(Diagram diagram, IEnumerable<Shape> shapes) {
			if (diagram == null) throw new ArgumentNullException(nameof(diagram));
			if (shapes == null) throw new ArgumentNullException(nameof(shapes));
			return Project.SecurityManager.IsGranted(Permission.Layout, shapes)
				&& diagram.Shapes.ContainsAll(shapes);
		}
		
		#endregion


		#region [Internal] Types

		internal enum EditAction { None, Copy, Cut }


		internal class EditBuffer {

			public EditBuffer() {
				action = EditAction.None;
				initialMousePos = Geometry.InvalidPoint;
				pasteCount = 0;
				shapes = new ShapeCollection();
				connections = new List<ShapeConnection>();
			}

			public bool IsEmpty {
				get { return shapes.Count == 0; }
			}
			
			public void Clear() {
				initialMousePos = Geometry.InvalidPoint;
				action = EditAction.None;
				pasteCount = 0;
				shapes.Clear();
				connections.Clear();
			}

			public Point initialMousePos;

			public EditAction action;

			public int pasteCount;

			public bool withModelObjects;

			// ShapeCollection in "IList" mode without spatial index
			public ShapeCollection shapes;

			public List<ShapeConnection> connections;
		}

		#endregion


		#region [Internal] Properties

		internal IReadOnlyCollection<DiagramController> DiagramControllers {
			get { return _diagramControllers; }
		}

		#endregion


		#region [Public/Internal] Methods

		// ToDo: Make these methods protected internal as soon as the WinFormsUI.Display class 
		// is split into DiagramPresenter and Display:IDiagramView
		/// <ToBeCompleted></ToBeCompleted>
		public DiagramController OpenDiagram(Diagram diagram) {
			if (diagram == null) throw new ArgumentNullException(nameof(diagram));
			return DoAddDiagramController(diagram);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public DiagramController OpenDiagram(string name) {
			if (name == null) throw new ArgumentNullException(nameof(name));
			AssertProjectIsOpen();
			// Try to find diagram with given name
			Diagram diagram = null;
			foreach (Diagram d in _project.Repository.GetDiagrams()) {
				if (string.Compare(d.Name, name, StringComparison.InvariantCultureIgnoreCase) == 0) {
					diagram = d;
					break;
				}
			}
			// If a suitable diagram was found, create a diagramController for it
			if (diagram == null) return null;
			else return DoAddDiagramController(diagram);
		}

		#endregion


		#region [Private] Methods: Registering event handlers

		private void RegisterProjectEvents() {
			_project.Opened += project_ProjectOpen;
			_project.Closed += project_ProjectClosed;
			if (_project.IsOpen) RegisterRepositoryEvents();
		}


		private void UnregisterProjectEvents(){
			_project.Opened -= project_ProjectOpen;
			_project.Closed -= project_ProjectClosed;
		}


		private void RegisterRepositoryEvents() {
			_project.Repository.DiagramInserted += Repository_DiagramInserted;
			_project.Repository.DiagramDeleted += Repository_DiagramDeleted;
			
			_project.Repository.DesignUpdated += Repository_DesignUpdated;

			_project.Repository.TemplateShapeReplaced += Repository_TemplateShapeReplaced;

			_project.Repository.ShapesInserted += Repository_ShapesInserted;
			_project.Repository.ShapesDeleted += Repository_ShapesDeleted;
		}


		private void UnregisterRepositoryEvents() {
			_project.Repository.DiagramInserted -= Repository_DiagramInserted;
			_project.Repository.DiagramDeleted -= Repository_DiagramDeleted;
			
			_project.Repository.DesignUpdated -= Repository_DesignUpdated;
			
			_project.Repository.TemplateShapeReplaced -= Repository_TemplateShapeReplaced;

			_project.Repository.ShapesInserted -= Repository_ShapesInserted;
			_project.Repository.ShapesDeleted -= Repository_ShapesDeleted;
		}

		#endregion


		#region [Private] Methods: Event handler implementations

		private void project_ProjectClosed(object sender, EventArgs e) {
			UnregisterRepositoryEvents();
		}

		
		private void project_ProjectOpen(object sender, EventArgs e) {
			Debug.Assert(_project.Repository != null);
			RegisterRepositoryEvents();
		}

	
		private void Repository_DesignUpdated(object sender, RepositoryDesignEventArgs e) {
			// nothing to do
		}


		private void Repository_DiagramDeleted(object sender, RepositoryDiagramEventArgs e) {
			CloseDiagram(e.Diagram);
		}


		private void Repository_DiagramInserted(object sender, RepositoryDiagramEventArgs e) {
			// nothing to do
		}


		private void Repository_TemplateShapeReplaced(object sender, RepositoryTemplateShapeReplacedEventArgs e) {
			// Nothing to do here... Should be done by the ReplaceShapeCommand

			//foreach (Diagram diagram in Diagrams) {
			//   foreach (Shape oldShape in diagram.Shapes) {
			//      if (oldShape.Template == e.Template) {
			//         Shape newShape = e.Template.CreateShape();
			//         newShape.CopyFrom(oldShape);
			//         diagram.Shapes.Replace(oldShape, newShape);
			//      }
			//   }
			//}
		}


		private void Repository_ShapesDeleted(object sender, RepositoryShapesEventArgs e) {
			// Check if the deleted shapes still exists in its diagram and remove them in this case
			foreach (Shape s in e.Shapes) {
				if (s.Diagram != null) {
					Diagram d = s.Diagram;
					d.Shapes.Remove(s);
				}
			}
		}


		private void Repository_ShapesInserted(object sender, RepositoryShapesEventArgs e) {
			// Insert shapes that are not yet part of its diagram
			foreach (Shape shape in e.Shapes) {
				Diagram d = e.GetDiagram(shape);
				if (d != null && !d.Shapes.Contains(shape))
					d.Shapes.Add(shape);
			}
		}

		#endregion


		#region [Private] Methods

		private void AssertProjectIsOpen() {
			if (_project == null) throw new NShapePropertyNotSetException(this, "Project");
			if (!_project.IsOpen) throw new NShapeException(Properties.Resources.MessageTxt_ProjectIsNotOpen);
		}


		private int IndexOf(string name) {
			for (int i = _diagramControllers.Count - 1; i >= 0; --i) {
				if (string.Compare(_diagramControllers[i].Diagram.Name, name, StringComparison.InvariantCultureIgnoreCase) == 0)
					return i;
			}
			return -1;
		}
		
		
		private int DiagramControllerIndexOf(Diagram diagram) {
			for (int i = _diagramControllers.Count - 1; i >= 0; --i) {
				if (_diagramControllers[i].Diagram == diagram)
					return i;
			}
			return -1;
		}


		private DiagramController DoAddDiagramController(Diagram diagram) {
			//if (DiagramControllerIndexOf(diagram) >= 0) throw new ArgumentException("The diagram was already opened.");
			int controllerIdx = DiagramControllerIndexOf(diagram);
			if (controllerIdx < 0) {
				DiagramController controller = new DiagramController(this, diagram);
				_diagramControllers.Add(controller);
				if (DiagramAdded != null) DiagramAdded(this, GetDiagramEventArgs(controller.Diagram));
				return controller;
			} else return _diagramControllers[controllerIdx];
		}


		private DiagramEventArgs GetDiagramEventArgs(Diagram diagram) {
			_diagramEventArgs.Diagram = diagram;
			return _diagramEventArgs;
		}


		//private DiagramShapesEventArgs GetShapeEventArgs(Shape shape, Diagram diagram) {
		//    if (shape == null) throw new ArgumentNullException(nameof(shape));
		//    diagramShapeEventArgs.SetDiagramShapes(shape, diagram);
		//    return diagramShapeEventArgs;
		//}


		//private DiagramShapesEventArgs GetShapeEventArgs(IEnumerable<Shape> shapes, Diagram diagram) {
		//    if (shapes == null) throw new ArgumentNullException(nameof(shapes));
		//    diagramShapeEventArgs.SetDiagramShapes(shapes, diagram);
		//    return diagramShapeEventArgs;
		//}


		private ModelObjectsEventArgs GetModelObjectsEventArgs(IEnumerable<IModelObject> modelObjects) {
			_modelObjectEventArgs.SetModelObjects(modelObjects);
			return _modelObjectEventArgs;
		}

		#endregion


		#region Fields

		private Project _project = null;
		private Tool _tool;
		private ReadOnlyList<DiagramController> _diagramControllers = new ReadOnlyList<DiagramController>();

		// Cut'n'Paste buffers
		private EditBuffer _editBuffer = new EditBuffer();		// Buffer for Copy/Cut/Paste-Actions
		private Rectangle _copyCutBounds = Rectangle.Empty;
		private Point _copyCutMousePos = Point.Empty;
		// Other buffers
		private List<Shape> _shapeBuffer = new List<Shape>();
		private List<IModelObject> _modelBuffer = new List<IModelObject>();
		// EventArgs buffers
		private DiagramEventArgs _diagramEventArgs = new DiagramEventArgs();
		private ModelObjectsEventArgs _modelObjectEventArgs = new ModelObjectsEventArgs();

		#endregion
	}


	#region Enums

	/// <summary>
	/// Specifies the draw mode of selection indicators and grips.
	/// </summary>
	public enum IndicatorDrawMode {
		/// <summary>Selection indicators and grips will be drawn normally.</summary>
		Normal,
		/// <summary>Selection indicators and grips will be drawn in an highlighted way, typically in a signal color.</summary>
		Highlighted,
		/// <summary>Selection indicators and grips will be drawn in a less noticeable way, typically in gray or transparent.</summary>
		Deactivated
	};


	/// <summary>
	/// This is the NShape representation of the <see cref="T:System.Windows.Forms.MouseButtons" /> enumeration.
	/// </summary>
	[Flags]
	public enum MouseButtonsDg {
		/// <summary>No mouse button was pressed.</summary>
		None = 0,
		/// <summary>The left mouse button was pressed.</summary>
		Left = 0x100000,
		/// <summary>The right mouse button was pressed.</summary>
		Right = 0x200000,
		/// <summary>The middle mouse button was pressed.</summary>
		Middle = 0x400000,
		/// <summary>The first XButton was pressed.</summary>
		ExtraButton1 = 0x800000,
		/// <summary>The second XButton was pressed.</summary>
		ExtraButton2 = 0x1000000,
	}


	/// <summary>
	/// Specifies the kind of mouse event.
	/// </summary>
	public enum MouseEventType { 
		/// <summary>A mouse button was pressed.</summary>
		MouseDown,
		/// <summary>The mouse was moved.</summary>
		MouseMove,
		/// <summary>A mouse button was released.</summary>
		MouseUp 
	};


	/// <summary>
	/// Specifies the kind of key event.
	/// </summary>
	public enum KeyEventType { 
		/// <summary>A key was pressed down.</summary>
		KeyDown, 
		/// <summary>A key was pressed.</summary>
		KeyPress, 
		/// <summary>A key was released.</summary>
		KeyUp, 
		/// <summary>A key is going to be pressed down.</summary>
		PreviewKeyDown 
	}


	/// <summary>
	/// Specifies key codes and modifiers. This is the NShape representation of the <see cref="T:System.Windows.Forms.Keys" /> enumeration.
	/// </summary>
	[Flags]
	public enum KeysDg {
		/// <summary>The A key.</summary>
		A = 0x41,
		/// <summary>The add key.</summary>
		Add = 0x6b,
		/// <summary>The ALT modifier key.</summary>
		Alt = 0x40000,
		/// <summary>The application key (Microsoft Natural Keyboard).</summary>
		Apps = 0x5d,
		/// <summary>The ATTN key.</summary>
		Attn = 0xf6,
		/// <summary>The B key.</summary>
		B = 0x42,
		/// <summary>The BACKSPACE key.</summary>
		Back = 8,
		/// <summary>The browser back key (Windows 2000 or later).</summary>
		BrowserBack = 0xa6,
		/// <summary>The browser favorites key (Windows 2000 or later).</summary>
		BrowserFavorites = 0xab,
		/// <summary>The browser forward key (Windows 2000 or later).</summary>
		BrowserForward = 0xa7,
		/// <summary>The browser home key (Windows 2000 or later).</summary>
		BrowserHome = 0xac,
		/// <summary>The browser refresh key (Windows 2000 or later).</summary>
		BrowserRefresh = 0xa8,
		/// <summary>The browser search key (Windows 2000 or later).</summary>
		BrowserSearch = 170,
		/// <summary>The browser stop key (Windows 2000 or later).</summary>
		BrowserStop = 0xa9,
		/// <summary>The C key.</summary>
		C = 0x43,
		/// <summary>The CANCEL key.</summary>
		Cancel = 3,
		/// <summary>The CAPS LOCK key.</summary>
		Capital = 20,
		/// <summary>The CAPS LOCK key.</summary>
		CapsLock = 20,
		/// <summary>The CLEAR key.</summary>
		Clear = 12,
		/// <summary>The CTRL modifier key.</summary>
		Control = 0x20000,
		/// <summary>The CTRL key.</summary>
		ControlKey = 0x11,
		/// <summary>The CRSEL key.</summary>
		Crsel = 0xf7,
		/// <summary>The D key.</summary>
		D = 0x44,
		/// <summary>The 0 key.</summary>
		D0 = 0x30,
		/// <summary>The 1 key.</summary>
		D1 = 0x31,
		/// <summary>The 2 key.</summary>
		D2 = 50,
		/// <summary>The 3 key.</summary>
		D3 = 0x33,
		/// <summary>The 4 key.</summary>
		D4 = 0x34,
		/// <summary>The 5 key.</summary>
		D5 = 0x35,
		/// <summary>The 6 key.</summary>
		D6 = 0x36,
		/// <summary>The 7 key.</summary>
		D7 = 0x37,
		/// <summary>The 8 key.</summary>
		D8 = 0x38,
		/// <summary>The 9 key.</summary>
		D9 = 0x39,
		/// <summary>The decimal key.</summary>
		Decimal = 110,
		/// <summary>The DEL key.</summary>
		Delete = 0x2e,
		/// <summary>The divide key.</summary>
		Divide = 0x6f,
		/// <summary>The DOWN ARROW key.</summary>
		Down = 40,
		/// <summary>The E key.</summary>
		E = 0x45,
		/// <summary>The END key.</summary>
		End = 0x23,
		/// <summary>The ENTER key.</summary>
		Enter = 13,
		/// <summary>The ERASE EOF key.</summary>
		EraseEof = 0xf9,
		/// <summary>The ESC key.</summary>
		Escape = 0x1b,
		/// <summary>The EXECUTE key.</summary>
		Execute = 0x2b,
		/// <summary>The EXSEL key.</summary>
		Exsel = 0xf8,
		/// <summary>The F key.</summary>
		F = 70,
		/// <summary>The F1 key.</summary>
		F1 = 0x70,
		/// <summary>The F10 key.</summary>
		F10 = 0x79,
		/// <summary>The F11 key.</summary>
		F11 = 0x7a,
		/// <summary>The F12 key.</summary>
		F12 = 0x7b,
		/// <summary>The F13 key.</summary>
		F13 = 0x7c,
		/// <summary>The F14 key.</summary>
		F14 = 0x7d,
		/// <summary>The F15 key.</summary>
		F15 = 0x7e,
		/// <summary>The F16 key.</summary>
		F16 = 0x7f,
		/// <summary>The F17 key.</summary>
		F17 = 0x80,
		/// <summary>The F18 key.</summary>
		F18 = 0x81,
		/// <summary>The F19 key.</summary>
		F19 = 130,
		/// <summary>The F2 key.</summary>
		F2 = 0x71,
		/// <summary>The F20 key.</summary>
		F20 = 0x83,
		/// <summary>The F21 key.</summary>
		F21 = 0x84,
		/// <summary>The F22 key.</summary>
		F22 = 0x85,
		/// <summary>The F23 key.</summary>
		F23 = 0x86,
		/// <summary>The F24 key.</summary>
		F24 = 0x87,
		/// <summary>The F3 key.</summary>
		F3 = 0x72,
		/// <summary>The F4 key.</summary>
		F4 = 0x73,
		/// <summary>The F5 key.</summary>
		F5 = 0x74,
		/// <summary>The F6 key.</summary>
		F6 = 0x75,
		/// <summary>The F7 key.</summary>
		F7 = 0x76,
		/// <summary>The F8 key.</summary>
		F8 = 0x77,
		/// <summary>The F9 key.</summary>
		F9 = 120,
		/// <summary>The IME final mode key.</summary>
		FinalMode = 0x18,
		/// <summary>The G key.</summary>
		G = 0x47,
		/// <summary>The H key.</summary>
		H = 0x48,
		/// <summary>The IME Hanguel mode key. (maintained for compatibility; use HangulMode) </summary>
		HanguelMode = 0x15,
		/// <summary>The IME Hangul mode key.</summary>
		HangulMode = 0x15,
		/// <summary>The IME Hanja mode key.</summary>
		HanjaMode = 0x19,
		/// <summary>The HELP key.</summary>
		Help = 0x2f,
		/// <summary>The HOME key.</summary>
		Home = 0x24,
		/// <summary>The I key.</summary>
		I = 0x49,
		/// <summary>The IME accept key, replaces <see cref="F:System.Windows.Forms.Keys.IMEAccept"></see>.</summary>
		IMEAccept = 30,
		/// <summary>The IME accept key. Obsolete, use <see cref="F:System.Windows.Forms.Keys.IMEAccept"></see> instead.</summary>
		IMEAceept = 30,
		/// <summary>The IME convert key.</summary>
		IMEConvert = 0x1c,
		/// <summary>The IME mode change key.</summary>
		IMEModeChange = 0x1f,
		/// <summary>The IME nonconvert key.</summary>
		IMENonconvert = 0x1d,
		/// <summary>The INS key.</summary>
		Insert = 0x2d,
		/// <summary>The J key.</summary>
		J = 0x4a,
		/// <summary>The IME Junja mode key.</summary>
		JunjaMode = 0x17,
		/// <summary>The K key.</summary>
		K = 0x4b,
		/// <summary>The IME Kana mode key.</summary>
		KanaMode = 0x15,
		/// <summary>The IME Kanji mode key.</summary>
		KanjiMode = 0x19,
		/// <summary>The bitmask to extract a key code from a key value.</summary>
		KeyCode = 0xffff,
		/// <summary>The L key.</summary>
		L = 0x4c,
		/// <summary>The start application one key (Windows 2000 or later).</summary>
		LaunchApplication1 = 0xb6,
		/// <summary>The start application two key (Windows 2000 or later).</summary>
		LaunchApplication2 = 0xb7,
		/// <summary>The launch mail key (Windows 2000 or later).</summary>
		LaunchMail = 180,
		/// <summary>The left mouse button.</summary>
		LButton = 1,
		/// <summary>The left CTRL key.</summary>
		LControlKey = 0xa2,
		/// <summary>The LEFT ARROW key.</summary>
		Left = 0x25,
		/// <summary>The LINEFEED key.</summary>
		LineFeed = 10,
		/// <summary>The left ALT key.</summary>
		LMenu = 0xa4,
		/// <summary>The left SHIFT key.</summary>
		LShiftKey = 160,
		/// <summary>The left Windows logo key (Microsoft Natural Keyboard).</summary>
		LWin = 0x5b,
		/// <summary>The M key.</summary>
		M = 0x4d,
		/// <summary>The middle mouse button (three-button mouse).</summary>
		MButton = 4,
		/// <summary>The media next track key (Windows 2000 or later).</summary>
		MediaNextTrack = 0xb0,
		/// <summary>The media play pause key (Windows 2000 or later).</summary>
		MediaPlayPause = 0xb3,
		/// <summary>The media previous track key (Windows 2000 or later).</summary>
		MediaPreviousTrack = 0xb1,
		/// <summary>The media Stop key (Windows 2000 or later).</summary>
		MediaStop = 0xb2,
		/// <summary>The ALT key.</summary>
		Menu = 0x12,
		/// <summary>The bitmask to extract modifiers from a key value.</summary>
		Modifiers = -65536,
		/// <summary>The multiply key.</summary>
		Multiply = 0x6a,
		/// <summary>The N key.</summary>
		N = 0x4e,
		/// <summary>The PAGE DOWN key.</summary>
		Next = 0x22,
		/// <summary>A constant reserved for future use.</summary>
		NoName = 0xfc,
		/// <summary>No key pressed.</summary>
		None = 0,
		/// <summary>The NUM LOCK key.</summary>
		NumLock = 0x90,
		/// <summary>The 0 key on the numeric keypad.</summary>
		NumPad0 = 0x60,
		/// <summary>The 1 key on the numeric keypad.</summary>
		NumPad1 = 0x61,
		/// <summary>The 2 key on the numeric keypad.</summary>
		NumPad2 = 0x62,
		/// <summary>The 3 key on the numeric keypad.</summary>
		NumPad3 = 0x63,
		/// <summary>The 4 key on the numeric keypad.</summary>
		NumPad4 = 100,
		/// <summary>The 5 key on the numeric keypad.</summary>
		NumPad5 = 0x65,
		/// <summary>The 6 key on the numeric keypad.</summary>
		NumPad6 = 0x66,
		/// <summary>The 7 key on the numeric keypad.</summary>
		NumPad7 = 0x67,
		/// <summary>The 8 key on the numeric keypad.</summary>
		NumPad8 = 0x68,
		/// <summary>The 9 key on the numeric keypad.</summary>
		NumPad9 = 0x69,
		/// <summary>The O key.</summary>
		O = 0x4f,
		/// <summary>The OEM 1 key.</summary>
		Oem1 = 0xba,
		/// <summary>The OEM 102 key.</summary>
		Oem102 = 0xe2,
		/// <summary>The OEM 2 key.</summary>
		Oem2 = 0xbf,
		/// <summary>The OEM 3 key.</summary>
		Oem3 = 0xc0,
		/// <summary>The OEM 4 key.</summary>
		Oem4 = 0xdb,
		/// <summary>The OEM 5 key.</summary>
		Oem5 = 220,
		/// <summary>The OEM 6 key.</summary>
		Oem6 = 0xdd,
		/// <summary>The OEM 7 key.</summary>
		Oem7 = 0xde,
		/// <summary>The OEM 8 key.</summary>
		Oem8 = 0xdf,
		/// <summary>The OEM angle bracket or backslash key on the RT 102 key keyboard (Windows 2000 or later).</summary>
		OemBackslash = 0xe2,
		/// <summary>The CLEAR key.</summary>
		OemClear = 0xfe,
		/// <summary>The OEM close bracket key on a US standard keyboard (Windows 2000 or later).</summary>
		OemCloseBrackets = 0xdd,
		/// <summary>The OEM comma key on any country/region keyboard (Windows 2000 or later).</summary>
		Oemcomma = 0xbc,
		/// <summary>The OEM minus key on any country/region keyboard (Windows 2000 or later).</summary>
		OemMinus = 0xbd,
		/// <summary>The OEM open bracket key on a US standard keyboard (Windows 2000 or later).</summary>
		OemOpenBrackets = 0xdb,
		/// <summary>The OEM period key on any country/region keyboard (Windows 2000 or later).</summary>
		OemPeriod = 190,
		/// <summary>The OEM pipe key on a US standard keyboard (Windows 2000 or later).</summary>
		OemPipe = 220,
		/// <summary>The OEM plus key on any country/region keyboard (Windows 2000 or later).</summary>
		Oemplus = 0xbb,
		/// <summary>The OEM question mark key on a US standard keyboard (Windows 2000 or later).</summary>
		OemQuestion = 0xbf,
		/// <summary>The OEM singled/double quote key on a US standard keyboard (Windows 2000 or later).</summary>
		OemQuotes = 0xde,
		/// <summary>The OEM Semicolon key on a US standard keyboard (Windows 2000 or later).</summary>
		OemSemicolon = 0xba,
		/// <summary>The OEM tilde key on a US standard keyboard (Windows 2000 or later).</summary>
		Oemtilde = 0xc0,
		/// <summary>The P key.</summary>
		P = 80,
		/// <summary>The PA1 key.</summary>
		Pa1 = 0xfd,
		/// <summary>Used to pass Unicode characters as if they were keystrokes. The Packet key value is the low word of a 32-bit virtual-key value used for non-keyboard input methods.</summary>
		Packet = 0xe7,
		/// <summary>The PAGE DOWN key.</summary>
		PageDown = 0x22,
		/// <summary>The PAGE UP key.</summary>
		PageUp = 0x21,
		/// <summary>The PAUSE key.</summary>
		Pause = 0x13,
		/// <summary>The PLAY key.</summary>
		Play = 250,
		/// <summary>The PRINT key.</summary>
		Print = 0x2a,
		/// <summary>The PRINT SCREEN key.</summary>
		PrintScreen = 0x2c,
		/// <summary>The PAGE UP key.</summary>
		Prior = 0x21,
		/// <summary>The PROCESS KEY key.</summary>
		ProcessKey = 0xe5,
		/// <summary>The Q key.</summary>
		Q = 0x51,
		/// <summary>The R key.</summary>
		R = 0x52,
		/// <summary>The right mouse button.</summary>
		RButton = 2,
		/// <summary>The right CTRL key.</summary>
		RControlKey = 0xa3,
		/// <summary>The RETURN key.</summary>
		Return = 13,
		/// <summary>The RIGHT ARROW key.</summary>
		Right = 0x27,
		/// <summary>The right ALT key.</summary>
		RMenu = 0xa5,
		/// <summary>The right SHIFT key.</summary>
		RShiftKey = 0xa1,
		/// <summary>The right Windows logo key (Microsoft Natural Keyboard).</summary>
		RWin = 0x5c,
		/// <summary>The S key.</summary>
		S = 0x53,
		/// <summary>The SCROLL LOCK key.</summary>
		Scroll = 0x91,
		/// <summary>The SELECT key.</summary>
		Select = 0x29,
		/// <summary>The select media key (Windows 2000 or later).</summary>
		SelectMedia = 0xb5,
		/// <summary>The separator key.</summary>
		Separator = 0x6c,
		/// <summary>The SHIFT modifier key.</summary>
		Shift = 0x10000,
		/// <summary>The SHIFT key.</summary>
		ShiftKey = 0x10,
		/// <summary>The computer sleep key.</summary>
		Sleep = 0x5f,
		/// <summary>The PRINT SCREEN key.</summary>
		Snapshot = 0x2c,
		/// <summary>The SPACEBAR key.</summary>
		Space = 0x20,
		/// <summary>The subtract key.</summary>
		Subtract = 0x6d,
		/// <summary>The T key.</summary>
		T = 0x54,
		/// <summary>The TAB key.</summary>
		Tab = 9,
		/// <summary>The U key.</summary>
		U = 0x55,
		/// <summary>The UP ARROW key.</summary>
		Up = 0x26,
		/// <summary>The V key.</summary>
		V = 0x56,
		/// <summary>The volume down key (Windows 2000 or later).</summary>
		VolumeDown = 0xae,
		/// <summary>The volume mute key (Windows 2000 or later).</summary>
		VolumeMute = 0xad,
		/// <summary>The volume up key (Windows 2000 or later).</summary>
		VolumeUp = 0xaf,
		/// <summary>The W key.</summary>
		W = 0x57,
		/// <summary>The X key.</summary>
		X = 0x58,
		/// <summary>The first x mouse button (five-button mouse).</summary>
		XButton1 = 5,
		/// <summary>The second x mouse button (five-button mouse).</summary>
		XButton2 = 6,
		/// <summary>The Y key.</summary>
		Y = 0x59,
		/// <summary>The Z key.</summary>
		Z = 90,
		/// <summary>The ZOOM key.</summary>
		Zoom = 0xfb
	}

	#endregion


	#region EventArgs

	/// <summary>Provides data for mouse events.</summary>
	public class MouseEventArgsDg : EventArgs {

		/// <summary>
		/// Initializing a new instance of <see cref="T:Dataweb.NShape.Controllers.MouseEventArgsDg" />.
		/// </summary>
		public MouseEventArgsDg(MouseEventType eventType, MouseButtonsDg buttons, int clicks, int delta, Point location, KeysDg modifiers) {
			_buttons = buttons;
			_clicks = clicks;
			_wheelDelta = delta;
			_eventType = eventType;
			_position = location;
			_modifiers = modifiers;
		}


		/// <summary>
		/// Contains the type of MouseEvent that was raised.
		/// </summary>
		public MouseEventType EventType {
			get { return _eventType; }
			protected set { _eventType = value; }
		}


		/// <summary>
		/// Contains a combination of all MouseButtons that were pressed.
		/// </summary>
		public MouseButtonsDg Buttons {
			get { return _buttons; }
			protected set { _buttons = value; }
		}


		/// <summary>
		/// Contains the number of clicks.
		/// </summary>
		public int Clicks {
			get { return _clicks; }
			protected set { _clicks = value; }
		}


		/// <summary>
		/// Contains a (signed) count of the number of detents the mouse wheel was rotated.
		/// A detent is one notch of the mouse wheel.
		/// </summary>
		public int WheelDelta {
			get { return _wheelDelta; }
			protected set { _wheelDelta = value; }
		}


		/// <summary>
		/// Contains the position (in diagram coordinates) of the mouse cursor at the time the event was raised.
		/// </summary>
		public Point Position {
			get { return _position; }
			protected set { _position = value; }
		}


		/// <summary>
		/// Contains the modifiers in case any modifier keys were pressed
		/// </summary>
		public KeysDg Modifiers {
			get { return _modifiers; }
			protected set { _modifiers = value; }
		}


		/// <summary>
		/// Initializes an new empty instance of <see cref="T:Dataweb.NShape.Controllers.MouseEventArgsDg" />.
		/// </summary>
		protected internal MouseEventArgsDg() {
			_buttons = MouseButtonsDg.None;
			_clicks = 0;
			_wheelDelta = 0;
			_eventType = MouseEventType.MouseMove;
			_position = Point.Empty;
		}


		#region Fields

		/// <ToBeCompleted></ToBeCompleted>
		private MouseEventType _eventType;
		/// <ToBeCompleted></ToBeCompleted>
		private MouseButtonsDg _buttons;
		/// <ToBeCompleted></ToBeCompleted>
		private Point _position;
		/// <ToBeCompleted></ToBeCompleted>
		private int _wheelDelta;
		/// <ToBeCompleted></ToBeCompleted>
		private int _clicks;
		/// <ToBeCompleted></ToBeCompleted>
		private KeysDg _modifiers;
		
		#endregion
	}


	/// <summary>Provides data for keyboard events.</summary>
	public class KeyEventArgsDg : EventArgs {

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Controllers.KeyEventArgsDg" />.
		/// </summary>
		public KeyEventArgsDg(KeyEventType eventType, int keyData, char keyChar, bool handled, bool suppressKeyPress) {
			this._eventType = eventType;
			this._handled = handled;
			this._keyChar = keyChar;
			this._keyData = keyData;
			this._suppressKeyPress = suppressKeyPress;
		}


		/// <summary>Specifies the kind of key event.</summary>
		public KeyEventType EventType {
			get { return _eventType; }
			protected set { _eventType = value; }
		}


		/// <summary>Gets the character corresponding to the key pressed.</summary>
		public char KeyChar {
			get { return _keyChar; }
			protected set { _keyChar = value; }
		}


		/// <summary>Gets the key data for a keyboard event.</summary>
		public int KeyData {
			get { return _keyData; }
			protected set { _keyData = value; }
		}


		/// <summary>Gets or sets a value indicating whether the event was handled.</summary>
		public bool Handled {
			get { return _handled; }
			set { _handled = value; }
		}


		/// <summary>Gets or sets a value indicating whether the key event should be passed on to the underlying control.</summary>
		public bool SuppressKeyPress {
			get { return _suppressKeyPress; }
			set { _suppressKeyPress = value; }
		}


		/// <summary>Gets a value indicating whether the CTRL key was pressed.</summary>
		public bool Control {
			get { return (_keyData & KeyCodeControl) == KeyCodeControl; }
		}


		/// <summary>Gets or sets a value indicating whether the key event should be passed on to the underlying control</summary>
		public bool Shift {
			get { return (_keyData & KeyCodeShift) == KeyCodeShift; }
		}


		/// <summary>Gets a value indicating whether the ALT key was pressed.</summary>
		public bool Alt {
			get { return (_keyData & KeyCodeAlt) == KeyCodeAlt; }
		}


		/// <summary>Gets the keyboard code for a keyboard event.</summary>
		public int KeyCode {
			get { return _keyData & MaxKeyCode; }
		}


		/// <summary>Specifies the currently pressed modifier keys.</summary>
		public int Modifiers {
			get { return (_keyData & ~MaxKeyCode); }
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected internal KeyEventArgsDg() {
			this._eventType = KeyEventType.PreviewKeyDown;
			this._handled = false;
			this._keyChar = '\0';
			this._keyData = 0;
			this._suppressKeyPress = false;
		}


		/// <summary>
		/// The bitmask to extract a key code from a key value.
		/// </summary>
		protected const int MaxKeyCode = ushort.MaxValue;

		
		/// <summary>
		/// The SHIFT modifier key.
		/// </summary>
		protected const int KeyCodeShift = ushort.MaxValue;

		
		/// <summary>
		/// The CTRL modifier key.
		/// </summary>
		protected const int KeyCodeControl = 131072;

		
		/// <summary>
		/// The ALT modifier key.
		/// </summary>
		protected const int KeyCodeAlt = 262144;


		/// <ToBeCompleted></ToBeCompleted>
		private KeyEventType _eventType;
		/// <ToBeCompleted></ToBeCompleted>
		private char _keyChar;
		/// <ToBeCompleted></ToBeCompleted>
		private int _keyData;
		/// <ToBeCompleted></ToBeCompleted>
		private bool _handled;
		/// <ToBeCompleted></ToBeCompleted>
		private bool _suppressKeyPress;
	}


	/// <ToBeCompleted></ToBeCompleted>
	public class DiagramEventArgs : EventArgs {

		/// <ToBeCompleted></ToBeCompleted>
		public DiagramEventArgs(Diagram diagram) {
			if (diagram == null) throw new ArgumentNullException(nameof(diagram));
			this.diagram = diagram;
		}

		/// <ToBeCompleted></ToBeCompleted>
		public Diagram Diagram {
			get { return diagram; }
			internal set { diagram = value; }
		}

		internal DiagramEventArgs() { }

		private Diagram diagram;

	}


	/// <ToBeCompleted></ToBeCompleted>
	public class ShapesEventArgs : EventArgs {

		/// <ToBeCompleted></ToBeCompleted>
		public ShapesEventArgs(IEnumerable<Shape> shapes) {
			if (shapes == null) throw new ArgumentNullException(nameof(shapes));
			this.shapes.AddRange(shapes);
		}

		/// <ToBeCompleted></ToBeCompleted>
		public IReadOnlyCollection<Shape> Shapes {
			get { return shapes; }
		}

		internal ShapesEventArgs() {
			this.shapes.Clear();
		}

		internal void SetShapes(IEnumerable<Shape> shapes) {
			this.shapes.Clear();
			this.shapes.AddRange(shapes);
		}

		internal void SetShape(Shape shape) {
			this.shapes.Clear();
			this.shapes.Add(shape);
		}

		private ReadOnlyList<Shape> shapes = new ReadOnlyList<Shape>();
	}


	/// <ToBeCompleted></ToBeCompleted>
	public class ShapeEventArgs : EventArgs {

		/// <ToBeCompleted></ToBeCompleted>
		public ShapeEventArgs(Shape shape) {
			if (shape == null) throw new ArgumentNullException(nameof(shape));
			this.shape = shape;
		}

		/// <ToBeCompleted></ToBeCompleted>
		public Shape Shape {
			get { return shape; }
		}

		internal ShapeEventArgs() {
		}

		internal void SetShape(Shape shape) {
			this.shape = shape;
		}

		private Shape shape = null;
	}


	/// <ToBeCompleted></ToBeCompleted>
	public class ModelObjectsEventArgs : EventArgs {

		/// <ToBeCompleted></ToBeCompleted>
		public ModelObjectsEventArgs(IEnumerable<IModelObject> modelObjects) {
			if (modelObjects == null) throw new ArgumentNullException(nameof(modelObjects));
			this.modelObjects.AddRange(modelObjects);
		}

		/// <ToBeCompleted></ToBeCompleted>
		public IReadOnlyCollection<IModelObject> ModelObjects {
			get { return modelObjects; }
		}

		internal ModelObjectsEventArgs() {
			this.modelObjects.Clear();
		}

		internal void SetModelObjects(IEnumerable<IModelObject> modelObjects) {
			this.modelObjects.Clear();
			this.modelObjects.AddRange(modelObjects);
		}

		internal void SetModelObject(IModelObject modelObject) {
			this.modelObjects.Clear();
			this.modelObjects.Add(modelObject);
		}

		private ReadOnlyList<IModelObject> modelObjects = new ReadOnlyList<IModelObject>();
	}


	/// <ToBeCompleted></ToBeCompleted>
	public class DiagramShapesEventArgs : ShapesEventArgs {

		/// <ToBeCompleted></ToBeCompleted>
		public DiagramShapesEventArgs(IEnumerable<Shape> shapes, Diagram diagram)
			: base(shapes) {
			if (diagram == null) throw new ArgumentNullException(nameof(diagram));
			this.diagram = diagram;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public Diagram Diagram {
			get { return diagram; }
		}


		internal DiagramShapesEventArgs()
			: base() {
		}


		internal void SetDiagram(Diagram diagram) {
			this.diagram = diagram;
		}


		internal void SetDiagramShapes(IEnumerable<Shape> shapes, Diagram diagram) {
			SetShapes(shapes);
			SetDiagram(diagram);
		}


		internal void SetDiagramShapes(Shape shape, Diagram diagram) {
			SetShape(shape);
			SetDiagram(diagram);
		}


		private Diagram diagram;
	}

	#endregion


	#region Exceptions

	/// <ToBeCompleted></ToBeCompleted>
	public class DiagramControllerNotFoundException : NShapeException {

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Controllers.DiagramControllerNotFoundException" />.
		/// </summary>
		public DiagramControllerNotFoundException(Diagram diagram)
			: base("No {0} found for {1} '{2}'", typeof(DiagramController).Name, typeof(Diagram).Name, (diagram != null) ? diagram.Name : string.Empty) {
		}

	}

	#endregion

}
