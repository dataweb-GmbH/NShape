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
using System.Drawing.Imaging;

using Dataweb.NShape.Advanced;
using Dataweb.NShape.Controllers;


namespace Dataweb.NShape
{

	/// <summary>
	/// Specifies the outcome of a tool execution.
	/// </summary>
	/// <status>reviewed</status>
	public enum ToolResult {
		/// <summary>Tool was successfully executed</summary>
		Executed,
		/// <summary>Tool was canceled</summary>
		Canceled
	}


	/// <summary>
	/// Describes how a tool was executed.
	/// </summary>
	/// <status>reviewed</status>
	public class ToolExecutedEventArgs : EventArgs {

		/// <ToBeCompleted></ToBeCompleted>
		public ToolExecutedEventArgs(Tool tool, ToolResult result)
			: base() {
			if (tool == null) throw new ArgumentNullException(nameof(tool));
			_tool = tool;
			_result = result;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public Tool Tool {
			get { return _tool; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("Use ToolExecutedEventArgs.Result instead.")]
		[Browsable(false)]
		public ToolResult EventType {
			get { return Result; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		public ToolResult Result {
			get { return _result; }
		}


		private Tool _tool;
		private ToolResult _result;

	}


	/// <summary>
	/// Controls a user operation on a diagram.
	/// </summary>
	/// <status>reviewed</status>
	public abstract class Tool : IDisposable {

		#region IDisposable Members

		/// <summary>
		/// Releases all resources.
		/// </summary>
		public virtual void Dispose() {
			if (_smallIcon != null)
				_smallIcon.Dispose();
			_smallIcon = null;

			if (_largeIcon != null)
				_largeIcon.Dispose();
			_largeIcon = null;
		}

		#endregion


		/// <summary>
		/// A culture independent name, used as title if no title was specified.
		/// </summary>
		public string Name {
			get { return _name; }
		}


		/// <summary>
		/// A culture dependent title, used as label for toolbox items.
		/// </summary>
		public string Title {
			get { return _title; }
			set { _title = value; }
		}


		/// <summary>
		/// A culture dependent description, used as description for toolbox items.
		/// </summary>
		public virtual string Description {
			// TODO 2: Remove this implementation, when all derived classes have a better one.
			get { return _description; }
			set { _description = value; }
		}


		/// <summary>
		/// Specifies the toolbox category.
		/// </summary>
		public string Category {
			get { return _category; }
			set { _category = value; }
		}


		/// <summary>
		/// Specifies the tool tip text displayed for the toolbox item.
		/// </summary>
		public string ToolTipText {
			get { return Description; }
			set { Description = value; }
		}


		/// <summary>
		/// A small icon for this <see cref="T:Dataweb.NShape.Advanced.Tool" />.
		/// </summary>
		public Bitmap SmallIcon {
			get { return _smallIcon; }
			set { _smallIcon = value; }
		}


		/// <summary>
		/// A large icon for this <see cref="T:Dataweb.NShape.Advanced.Tool" />.
		/// </summary>
		public Bitmap LargeIcon {
			get { return _largeIcon; }
			set { _largeIcon = value; }
		}


		/// <summary>
		/// Specifies the minimum distance the mouse has to be moved before the tool is starting a drag action.
		/// </summary>
		public Size MinimumDragDistance {
			get { return _minDragDistance; }
			set { _minDragDistance = value; }
		}


		/// <summary>
		/// Specifies the interval (in ms) used for determine if two subsequent clicks are interpreted as double click.
		/// </summary>
		public int DoubleClickTime {
			get { return _doubleClickInterval; }
			set { _doubleClickInterval = value; }
		}


		/// <summary>
		/// Called by the <see cref="T:Dataweb.NShape.Advanced.Controllers.IDiagramPresenter" /> when the mouse has entered the control's area.
		/// </summary>
		public abstract void EnterDisplay(IDiagramPresenter diagramPresenter);


		/// <summary>
		/// Called by the <see cref="T:Dataweb.NShape.Advanced.Controllers.IDiagramPresenter" /> when the mouse has left the control's area.
		/// </summary>
		public abstract void LeaveDisplay(IDiagramPresenter diagramPresenter);


		/// <summary>
		/// Processes a mouse event.
		/// </summary>
		/// <param name="diagramPresenter">Diagram presenter where the event occurred.</param>
		/// <param name="e">Description of the mouse event.</param>
		/// <remarks>When overriding, the base classes method has to be called at the end.</remarks>
		/// <returns>True if the event was handled, false if the event was not handled.</returns>
		public virtual bool ProcessMouseEvent(IDiagramPresenter diagramPresenter, MouseEventArgsDg e) {
			if (diagramPresenter == null) throw new ArgumentNullException(nameof(diagramPresenter));
			_currentMouseState = MouseState.Create(diagramPresenter, e);
			diagramPresenter.Update();
			return false;
		}


		/// <summary>
		/// Processes a keyboard event.
		/// </summary>
		/// <param name="diagramPresenter">Diagram presenter where the event occurred.</param>
		/// <param name="e">Description of the keyboard event.</param>
		/// <returns>True if the event was handled, false if the event was not handled.</returns>
		public virtual bool ProcessKeyEvent(IDiagramPresenter diagramPresenter, KeyEventArgsDg e) {
			if (diagramPresenter == null) throw new ArgumentNullException(nameof(diagramPresenter));
			bool result = false;
			switch (e.EventType) {
				case KeyEventType.KeyDown:
					// Cancel tool
					if (e.KeyCode == (int)KeysDg.Escape) {
						Cancel();
						result = true;
					}
					break;

				case KeyEventType.KeyPress:
				case KeyEventType.PreviewKeyDown:
				case KeyEventType.KeyUp:
					// do nothing
					break;
				default: throw new NShapeUnsupportedValueException(e.EventType);
			}
			diagramPresenter.Update();
			return result;
		}


		/// <summary>
		/// Sets protected readonly-properties to invalid values and raises the ToolExecuted event.
		/// </summary>
		public void Cancel() {
			// End the tool's action
			while (IsToolActionPending)
				EndToolAction();

			// Reset the tool's state
			CancelCore();

			_currentMouseState = MouseState.Empty;

			OnToolExecuted(CancelledEventArgs);
		}


		/// <summary>
		/// Specifis if the tool wants the diagram presenter to scroll when reaching the presenter's bounds.
		/// </summary>
		public virtual bool WantsAutoScroll {
			get {
				if (_pendingActions.Count == 0) return false;
				else return _pendingActions.Peek().WantsAutoScroll;
			}
		}


		/// <summary>
		/// Returns a collection of <see cref="T:Dataweb.NShape.Advanced.MenuItemDef" /> for constructing context menus etc.
		/// </summary>
		public abstract IEnumerable<MenuItemDef> GetMenuItemDefs(IDiagramPresenter diagramPresenter);


		/// <summary>
		/// Request redrawing.
		/// </summary>
		public abstract void Invalidate(IDiagramPresenter diagramPresenter);


		/// <summary>
		/// Draws the tool's preview.
		/// </summary>
		/// <param name="diagramPresenter"></param>
		public abstract void Draw(IDiagramPresenter diagramPresenter);


		/// <summary>
		/// Redraw icons.
		/// </summary>
		public abstract void RefreshIcons();


		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("Use IsToolActionPending instead.")]
		public bool ToolActionPending {
			get { return IsToolActionPending; }
		}


		/// <summary>
		/// Indicates if the tool has pending actions.
		/// </summary>
		public bool IsToolActionPending {
			get { return _pendingActions.Count > 0; }
		}


		/// <summary>
		/// Occurs when the tool was executed or canceled.
		/// </summary>
		public event EventHandler<ToolExecutedEventArgs> ToolExecuted;


		#region [Protected] Construction and Destruction

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.Tool" />.
		/// </summary>
		protected Tool() {
			_smallIcon = new Bitmap(16, 16, PixelFormat.Format32bppPArgb);
			_largeIcon = new Bitmap(32, 32, PixelFormat.Format32bppPArgb);
			_name = "Tool_" + this.GetHashCode().ToString();
			ExecutedEventArgs = new ToolExecutedEventArgs(this, ToolResult.Executed);
			CancelledEventArgs = new ToolExecutedEventArgs(this, ToolResult.Canceled);
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.Tool" />.
		/// </summary>
		protected Tool(string category)
			: this() {
			if (!string.IsNullOrEmpty(category))
				_category = category;
		}


		/// <summary>
		/// Finalizer of <see cref="T:Dataweb.NShape.Advanced.Tool" />.
		/// </summary>
		~Tool() {
			Dispose();
		}

		#endregion


		#region [Protected] Properties

		/// <summary>
		/// Current state of the mouse (state after the last ProcessMouseEvent call).
		/// Position is in Diagram coordinates.
		/// </summary>
		protected MouseState CurrentMouseState {
			get { return _currentMouseState; }
		}


		/// <summary>
		/// The display used by the current (pending) action.
		/// </summary>
		protected IDiagramPresenter ActionDiagramPresenter {
			get {
				if (_pendingActions.Count == 0) throw new NShapeException("The action's current display was not set yet. Call StartToolAction method to set the action's current display.");
				else return _pendingActions.Peek().DiagramPresenter;
			}
		}


		/// <summary>
		/// Transformed start coordinates of the current (pending) action (diagram coordinates).
		/// Use SetActionStartPosition method to set this value and ClearActionStartPosition to clear it.
		/// </summary>
		protected MouseState ActionStartMouseState {
			get {
				if (_pendingActions.Count == 0) throw new NShapeInternalException("The action's start mouse state was not set yet. Call SetActionStartPosition method to set the start position.");
				else return _pendingActions.Peek().MouseState;
			}
		}


		/// <summary>
		/// Specifies the current tool action.
		/// </summary>
		protected ActionDef CurrentToolAction {
			get {
				if (_pendingActions.Count > 0)
					return _pendingActions.Peek();
				else return ActionDef.Empty;
			}
		}


		/// <summary>
		/// Indicates whether a tool action is pending.
		/// </summary>
		protected IEnumerable<ActionDef> PendingToolActions {
			get { return _pendingActions; }
		}


		/// <summary>
		/// Specifies the number of pending tool actions.
		/// </summary>
		protected int PendingToolActionsCount {
			get { return _pendingActions.Count; }
		}


		/// <summary>
		/// Buffer for <see cref="T:Dataweb.NShape.Advanced.ToolExecutedEventArgs" /> in order to minimize memory allocation overhead.
		/// </summary>
		protected ToolExecutedEventArgs ExecutedEventArgs;


		/// <summary>
		/// Buffer for <see cref="T:Dataweb.NShape.Advanced.ToolExecutedEventArgs" /> in order to minimize memory allocation overhead.
		/// </summary>
		protected ToolExecutedEventArgs CancelledEventArgs;

		#endregion


		#region [Protected] Methods (overridable)

		/// <summary>
		/// Sets the start coordinates for an action as well as the display to use for the action.
		/// </summary>
		protected virtual void StartToolAction(IDiagramPresenter diagramPresenter, int action, MouseState mouseState, bool wantAutoScroll) {
			if (diagramPresenter == null) throw new ArgumentNullException(nameof(diagramPresenter));
			if (mouseState == MouseState.Empty) throw new ArgumentException("mouseState");
			if (_pendingActions.Count > 0) {
				if (_pendingActions.Peek().DiagramPresenter != diagramPresenter)
					throw new NShapeException("There are actions pending for an other diagram presenter!");
			}
			ActionDef actionDef = ActionDef.Create(diagramPresenter, action, mouseState, wantAutoScroll);
			_pendingActions.Push(actionDef);
		}


		/// <summary>
		/// Ends a tool's action. Clears the start position for the action and the display used for the action.
		/// </summary>
		/// <remarks>When overriding this method, the call to the base implementation has to be called at last.</remarks>
		protected virtual void EndToolAction() {
			if (_pendingActions.Count <= 0) throw new InvalidOperationException("No tool actions pending.");
			IDiagramPresenter diagramPresenter = _pendingActions.Peek().DiagramPresenter;
			if (diagramPresenter != null) {
				Invalidate(diagramPresenter);
				diagramPresenter.Capture = false;
				diagramPresenter.SetCursor(CursorProvider.DefaultCursorID);
			}
			_pendingActions.Pop();
		}


		/// <summary>
		/// Perform all necessary actions to cancel the current tool actions.
		/// </summary>
		protected abstract void CancelCore();


		/// <summary>
		/// Called when a tool action has finished. Raises the ToolExecuted event.
		/// </summary>
		protected virtual void OnToolExecuted(ToolExecutedEventArgs eventArgs) {
			if (IsToolActionPending) throw new InvalidOperationException(string.Format("{0} tool actions pending.", _pendingActions.Count));
			if (ToolExecuted != null) ToolExecuted(this, eventArgs);
		}

		#endregion


		#region [Protected] Methods

		/// <summary>
		/// Indicates whether the given shape can connect to the given targetShape with the specified glue point.
		/// </summary>
		protected bool CanActiveShapeConnectTo(Shape shape, ControlPointId gluePointId, Shape targetShape) {
			if (shape == null) throw new ArgumentNullException(nameof(shape));
			if (targetShape == null) throw new ArgumentNullException(nameof(targetShape));
			return CanConnect(shape, gluePointId, targetShape, ControlPointId.Reference);
		}


		/// <summary>
		/// Indicates whether the given shape can connect to the given targetShape with the specified glue point.
		/// </summary>
		protected bool CanActiveShapeConnectTo(Shape shape, ControlPointId gluePointId, Shape targetShape, ControlPointId targetPointId) {
			if (shape == null) throw new ArgumentNullException(nameof(shape));
			if (targetShape == null) throw new ArgumentNullException(nameof(targetShape));
			return CanConnect(shape, gluePointId, targetShape, targetPointId);
		}


		/// <summary>
		/// Indicates whether the given linear shape can connect to the given targetShape with the specified glue point.
		/// This method also checks the unmoved glue point (you cannot connect two glue points to the same shape via Point-to-Shape) of an unconnected shape.
		/// </summary>
		protected bool CanActiveShapeConnectTo(IDiagramPresenter diagramPresenter, Shape shape, ControlPointId unmovedGluePoint, ControlPointId movedGluePoint, bool onlyUnselected) {
			if (shape is ILinearShape lineShape && lineShape.VertexCount == 2) {
				Point posA = shape.GetControlPointPosition(movedGluePoint);
				Point posB = shape.GetControlPointPosition(unmovedGluePoint);
				//
				TargetFilterDelegate filter = null;
				if (onlyUnselected)
					filter = (s, cp) => !diagramPresenter.SelectedShapes.Contains(s);
				//
				Locator targetShapeA = FindNearestTarget(diagramPresenter, posA.X, posA.Y, ControlPointCapabilities.Connect, diagramPresenter.ZoomedGripSize, filter);
				Locator targetShapeB = FindNearestTarget(diagramPresenter, posB.X, posB.Y, ControlPointCapabilities.Connect, diagramPresenter.ZoomedGripSize, filter);
				if (!targetShapeA.IsEmpty && targetShapeA.Shape == targetShapeB.Shape
					&& targetShapeA.ControlPointId == ControlPointId.Reference && targetShapeB.ControlPointId == ControlPointId.Reference)
					return false;
				else
					return CanConnect(shape, movedGluePoint, targetShapeA.Shape, targetShapeA.ControlPointId);
			}
			return true;
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected static bool CanConnect(Shape shape, ControlPointId controlPointId, Shape otherShape, ControlPointId otherControlPointId) {
			if (shape == null || controlPointId == ControlPointId.None || otherShape == null || otherControlPointId == ControlPointId.None)
				return false;
			bool shapeHasGluePoint = shape.HasControlPointCapability(controlPointId, ControlPointCapabilities.Glue);
			bool otherShapeHasGluePoint = otherShape.HasControlPointCapability(otherControlPointId, ControlPointCapabilities.Glue);
			// Check if 'otherShape' is the active shape and 'shape' is the passive shape
			if (otherShapeHasGluePoint && !shapeHasGluePoint)
				return CanConnect(otherShape, otherControlPointId, shape, controlPointId);
			// Check if active shape and passive shape can connect
			if (shapeHasGluePoint && !otherShapeHasGluePoint) {
				if (otherShape.HasControlPointCapability(otherControlPointId, ControlPointCapabilities.Connect) == false)
					return false;
				// Connecting both glue points to the same target shape via Point-to-Shape connection is not allowed
				foreach (ShapeConnectionInfo sci in shape.GetConnectionInfos(ControlPointId.Any, otherShape)) {
					if ((sci.OtherPointId == ControlPointId.Reference && sci.OwnPointId != controlPointId)
						|| (sci.OwnPointId == ControlPointId.Reference && otherShape.HasControlPointCapability(sci.OtherPointId, ControlPointCapabilities.Glue)))
						return false;
				}
				return shape.CanConnect(controlPointId, otherShape, otherControlPointId)
					&& otherShape.CanConnect(otherControlPointId, shape, controlPointId);
			}
			return false;
		}


		/// <summary>
		/// Disconnect, disposes and deletes the given preview <see cref="T:Dataweb.NShape.Advanced.Shape" />.
		/// </summary>
		/// <param name="shape"></param>
		protected void DeletePreviewShape(ref Shape shape) {
			if (shape != null) {
				// Disconnect all existing connections (disconnects model, too)
				foreach (ControlPointId pointId in shape.GetControlPointIds(ControlPointCapabilities.Connect | ControlPointCapabilities.Glue)) {
					ControlPointId otherPointId = shape.IsConnected(pointId, null);
					if (otherPointId != ControlPointId.None) shape.Disconnect(pointId);
				}
				// Delete model object
				if (shape.ModelObject != null)
					shape.ModelObject = null;
				// Delete shape
				shape.Invalidate();
				shape.Dispose();
				shape = null;
			}
		}


		/// <summary>
		/// Returns the resize modifiers that have to be applied.
		/// </summary>
		protected ResizeModifiers GetResizeModifier(MouseState mouseState) {
			ResizeModifiers result = ResizeModifiers.None;
			if (!mouseState.IsEmpty) {
				if ((mouseState.Modifiers & KeysDg.Shift) == KeysDg.Shift)
					result |= ResizeModifiers.MaintainAspect;
				if ((mouseState.Modifiers & KeysDg.Control) == KeysDg.Control)
					result |= ResizeModifiers.MirroredResize;
			}
			return result;
		}

		#endregion


		#region [Protected] Methods (Drawing and Invalidating)

		// For backwards compatibility only: Delete when deleting obsolete DrawConnectionTargets and InvalidateConnectionTargets functions
		private TargetFilterDelegate CreateHighlightConnectionPointsFilter(IDiagramPresenter diagramPresenter, IEnumerable<Shape> excludedShapes = null) {
			return (s, cp) => {
				if (excludedShapes != null && EnumerationHelper.Contains(excludedShapes, s))
					return false;
				return diagramPresenter.IsLayerVisible(s.HomeLayer, s.SupplementalLayers) && s.HasControlPointCapability(cp, ControlPointCapabilities.Connect);
			};
		}


		/// <summary>
		/// Invalidates all potential connection targets in range.
		/// </summary>
		[Obsolete("Use version with ControlPointCapabilities and TargetFilterDelegate parameters instead.")]
		protected void InvalidateConnectionTargets(IDiagramPresenter diagramPresenter, int positionX, int positionY) {
			InvalidateConnectionTargets(diagramPresenter, positionX, positionY, ControlPointCapabilities.Connect, 
					DefaultHighlightTargetsRange, CreateHighlightConnectionPointsFilter(diagramPresenter));
		}


		// @@ Tool-Interface: New overload (with TargetFilterDelegate)
		/// <summary>
		/// Invalidates all potential connection targets in range.
		/// </summary>
		protected void InvalidateConnectionTargets(IDiagramPresenter diagramPresenter, int positionX, int positionY, ControlPointCapabilities capabilities, int range, TargetFilterDelegate isConnectionTarget) {
			if (Geometry.IsValid(positionX, positionY)) {
				// Invalidate all potential connection targets in range
				Shape lastShape = null;
				foreach (Locator locator in FindConnectionTargets(diagramPresenter, positionX, positionY, capabilities, range, isConnectionTarget)) {
					// Skip shapes that were already processed...
					if (lastShape == locator.Shape)
						continue;
					locator.Shape.Invalidate();
					diagramPresenter.InvalidateGrips(locator.Shape, capabilities);
					lastShape = locator.Shape;
				}
			}
		}


		/// <summary>
		/// Draws all connection targets in range.
		/// </summary>
		[Obsolete("Use overloaded version with TargetFilterDelegate parameter instead.")]
		protected void DrawConnectionTargets(IDiagramPresenter diagramPresenter, Shape shape, int x, int y) {
			if (!Geometry.IsValid(x, y)) throw new ArgumentException("x, y");
			DrawConnectionTargets(diagramPresenter, shape, ControlPointId.None, new Point(x, y), ControlPointCapabilities.Connect, DefaultHighlightTargetsRange, diagramPresenter.ZoomedGripSize, 
				CreateHighlightConnectionPointsFilter(diagramPresenter));
		}


		/// <summary>
		/// Draws all connection targets in range.
		/// </summary>
		[Obsolete("Use overloaded version with TargetFilterDelegate parameter instead.")]
		protected void DrawConnectionTargets(IDiagramPresenter diagramPresenter, Shape shape, int x, int y, IEnumerable<Shape> excludedShapes) {
			DrawConnectionTargets(diagramPresenter, shape, ControlPointId.None, new Point(x, y), ControlPointCapabilities.Connect, DefaultHighlightTargetsRange, diagramPresenter.ZoomedGripSize,
				CreateHighlightConnectionPointsFilter(diagramPresenter, excludedShapes));
		}


		/// <summary>
		/// Draws all connection targets in range.
		/// </summary>
		[Obsolete("Use overloaded version with TargetFilterDelegate parameter instead.")]
		protected void DrawConnectionTargets(IDiagramPresenter diagramPresenter, Shape shape, ControlPointId gluePtId, Point newGluePtPos, IEnumerable<Shape> excludedShapes) {
			DrawConnectionTargets(diagramPresenter, shape, gluePtId, newGluePtPos, ControlPointCapabilities.Connect, DefaultHighlightTargetsRange, diagramPresenter.ZoomedGripSize,
				CreateHighlightConnectionPointsFilter(diagramPresenter, excludedShapes));
		}


		/// <summary>
		/// Draws all connection targets in range.
		/// </summary>
		[Obsolete("Use overloaded version with TargetFilterDelegate parameter instead.")]
		protected void DrawConnectionTargets(IDiagramPresenter diagramPresenter, Shape shape, ControlPointId gluePtId, Point newGluePtPos) {
			DrawConnectionTargets(diagramPresenter, shape, gluePtId, newGluePtPos, ControlPointCapabilities.Connect, DefaultHighlightTargetsRange, diagramPresenter.ZoomedGripSize,
				CreateHighlightConnectionPointsFilter(diagramPresenter));
		}


		// @@ Tool-Interface: New overload (with TargetFilterDelegate)
		/// <summary>
		/// Draws all connection targets in range.
		/// </summary>
		protected void DrawConnectionTargets(IDiagramPresenter diagramPresenter, Shape shapeAtCursor, ControlPointId gluePointAtCursor, Point newGluePtPos, 
				ControlPointCapabilities capabilities, int highlightRange, int connectRange, TargetFilterDelegate isConnectionTarget) 
		{
			if (diagramPresenter == null) throw new ArgumentNullException(nameof(diagramPresenter));
			if (shapeAtCursor == null) throw new ArgumentNullException(nameof(shapeAtCursor));
			if (gluePointAtCursor == ControlPointId.None || gluePointAtCursor == ControlPointId.Any)
			   throw new ArgumentException(string.Format("{0} is not a valid {1} for this operation.", gluePointAtCursor, typeof(ControlPointId).Name));
			if (!Geometry.IsValid(newGluePtPos)) throw new ArgumentException(nameof(newGluePtPos));
			if (!shapeAtCursor.HasControlPointCapability(gluePointAtCursor, ControlPointCapabilities.Glue))
			   throw new ArgumentException(string.Format("{0} is not a valid glue point.", gluePointAtCursor));
			if (!diagramPresenter.Project.SecurityManager.IsGranted(Permission.Connect, shapeAtCursor))
				return;

			// Highlight the concrete connection target:
			// If there is no connection point under the Cursor and the cursor is over a shape, draw the shape's outline highlighted
			// If there is a connection point under the cursor, highlight it later, along with all other potential targets
			Locator target = FindNearestTarget(diagramPresenter, newGluePtPos.X, newGluePtPos.Y, capabilities, connectRange, isConnectionTarget);
			if (!target.IsEmpty && target.ControlPointId == ControlPointId.Reference && target.Shape.ContainsPoint(newGluePtPos.X, newGluePtPos.Y))
				diagramPresenter.DrawShapeOutline(IndicatorDrawMode.Highlighted, target.Shape);

			// Draw all connection points of all shapes in range (except the excluded ones, see above)
			diagramPresenter.ResetTransformation();
			try {
				foreach (Locator potentialTarget in FindConnectionTargets(diagramPresenter, newGluePtPos.X, newGluePtPos.Y, capabilities, highlightRange, isConnectionTarget)) {
					// Skip empty targets (should not happen) and the shape at cursor
					if (potentialTarget.IsEmpty || potentialTarget.Shape == shapeAtCursor)
						continue;
					// Skip shapes that cannot connect
					if (!(shapeAtCursor.CanConnect(gluePointAtCursor, potentialTarget.Shape, potentialTarget.ControlPointId) 
							&& potentialTarget.Shape.CanConnect(potentialTarget.ControlPointId, shapeAtCursor, gluePointAtCursor))) 
						continue;
					IndicatorDrawMode drawMode;
					if (potentialTarget.Shape == target.Shape && potentialTarget.ControlPointId == target.ControlPointId)
						drawMode = IndicatorDrawMode.Highlighted;
					else
						drawMode = IndicatorDrawMode.Normal;
					diagramPresenter.DrawConnectionPoint(drawMode, potentialTarget.Shape, potentialTarget.ControlPointId);
				}
			} finally { diagramPresenter.RestoreTransformation(); }
		}

		#endregion


		#region [Protected] Static Methods: Finding shapes and points

		// @@ Tool-Interface: New
		/// <summary>
		/// Finds the nearest shape and control point of a shape of type TShape in range of the given position. 
		/// </summary>
		/// <param name="diagramPresenter">The current <see cref="T:Dataweb.NShape.Controllers.IDiagramPresenter" /></param>
		/// <param name="x">The X coordinate of the search position (in diagram coordinates).</param>
		/// <param name="y">The Y coordinate of the search position (in diagram coordinates).</param>
		/// <param name="capabilities">The capabilities of the control points to search for.</param>
		/// <param name="range">The range (radius) of the search (in diagram coordinates).</param>
		/// <param name="filter">A filter callback to refine the search by accepting or rejecting found shapes.</param>
		protected static Locator FindNearestTarget(IDiagramPresenter diagramPresenter, int x, int y, ControlPointCapabilities capabilities, int range, TargetFilterDelegate filter = null) {
			return FindNearestTarget(diagramPresenter, x, y, capabilities, false, range, filter);
		}


		// @@ Tool-Interface: New
		/// <summary>
		/// Finds the nearest shape and control point of a shape of type TShape in range of the given position. 
		/// </summary>
		/// <param name="diagramPresenter">The current <see cref="T:Dataweb.NShape.Controllers.IDiagramPresenter" /></param>
		/// <param name="x">The X coordinate of the search position (in diagram coordinates).</param>
		/// <param name="y">The Y coordinate of the search position (in diagram coordinates).</param>
		/// <param name="capabilities">The capabilities of the control points to search for.</param>
		/// <param name="searchChildShapes">Usually false. True if child shapes of aggregated and grouped shapes should be searched, too.</param>
		/// <param name="range">The range (radius) of the search (in diagram coordinates).</param>
		/// <param name="filter">A filter callback to refine the search by accepting or rejecting found shapes.</param>
		protected static Locator FindNearestTarget(IDiagramPresenter diagramPresenter, int x, int y, ControlPointCapabilities capabilities, bool searchChildShapes, int range, TargetFilterDelegate filter = null) {
			if (diagramPresenter == null) throw new ArgumentNullException(nameof(diagramPresenter));

			Locator result = Locator.Empty;
			if (diagramPresenter != null && diagramPresenter.Diagram != null) {
				Shape foundShape = null;
				ControlPointId foundPointId = ControlPointId.None;
				int foundZOrder = int.MinValue;
				//
				foreach (Shape s in diagramPresenter.Diagram.Shapes.FindShapes(x, y, capabilities, range)) {
					Shape shape;
					if (searchChildShapes && s is IShapeGroup)
						shape = s.Children.FindShape(x, y, capabilities, range, null) ?? s;
					else
						shape = s;

					if (shape != null) {
						// Skip shapes below the last matching shape
						if (shape.ZOrder < foundZOrder) continue;
						//
						// Find the nearest control point.
						// If no suitable point is in range, use HitTest (which also returns ControlPoinId.Reference)
						ControlPointId pointId = shape.FindNearestControlPoint(x, y, range, capabilities);
						if (pointId == ControlPointId.None) {
							// CAUTION: Make sure there are control point capabilities (at least Reference), otherwise ControlPointId.None will be returned!
							pointId = shape.HitTest(x, y, capabilities | ControlPointCapabilities.Reference, range);
						}
						// If a filter predicate was given, execute it
						if (filter != null && filter(shape, pointId) == false)
							continue;
						//
						foundShape = shape;
						foundZOrder = shape.ZOrder;
						foundPointId = pointId;
					}
				}
				result.Shape = foundShape;
				result.ControlPointId = foundPointId;
			}
			return result;
		}


		// @@ Tool-Interface: Changed to static
		/// <summary>
		/// Finds the nearest grid line from the given coordinates.
		/// </summary>
		/// <param name="diagramPresenter">The <see cref="T:Dataweb.NShape.Controllers.IDiagramPresenter" /></param>
		/// <param name="x">X coordinate</param>
		/// <param name="y">Y coordinate</param>
		/// <param name="snapDeltaX">Horizontal distance between x and the nearest snap point.</param>
		/// <param name="snapDeltaY">Vertical distance between y and the nearest snap point.</param>
		/// <returns>Distance to nearest snap point.</returns>
		/// <remarks>If snapping is disabled for the current diagramPresenter, this function does nothing.</remarks>
		protected static float FindNearestSnapPoint(IDiagramPresenter diagramPresenter, int x, int y, out int snapDeltaX, out int snapDeltaY) {
			if (diagramPresenter == null) throw new ArgumentNullException(nameof(diagramPresenter));

			float distance = float.MaxValue;
			snapDeltaX = snapDeltaY = 0;
			if (diagramPresenter.SnapToGrid) {
				Point distanceToGridLine = CalculateDistanceToGridLine(x, y, diagramPresenter.GridSize, diagramPresenter.SnapDistance);
				if (Geometry.IsValidCoordinate(distanceToGridLine.X))
					snapDeltaX = distanceToGridLine.X;
				if (Geometry.IsValidCoordinate(distanceToGridLine.Y))
					snapDeltaY = distanceToGridLine.Y;
				if (snapDeltaX != 0 || snapDeltaY != 0)
					distance = Geometry.DistancePointPoint(0, 0, snapDeltaX, snapDeltaY);
			}
			return distance;
		}


		// @@ Tool-Interface: New overload (Rectangle parameter)
		/// <summary>
		/// Finds the nearest grid line from the given rectangle.
		/// </summary>
		/// <param name="diagramPresenter">The <see cref="T:Dataweb.NShape.Controllers.IDiagramPresenter" /></param>
		/// <param name="bounds">The rectangle, typically the bounding rectangle of a shape.</param>
		/// <param name="snapDeltaX">Horizontal distance between x and the nearest snap point.</param>
		/// <param name="snapDeltaY">Vertical distance between y and the nearest snap point.</param>
		/// <returns>Distance to nearest snap point.</returns>
		/// <remarks>If snapping is disabled for the current diagramPresenter, this function does nothing.</remarks>
		protected static float FindNearestSnapPoint(IDiagramPresenter diagramPresenter, Rectangle bounds, out int snapDeltaX, out int snapDeltaY) {
			if (diagramPresenter == null) throw new ArgumentNullException(nameof(diagramPresenter));

			snapDeltaX = snapDeltaY = 0;
			int snapDistance = diagramPresenter.SnapDistance;
			float lowestDistanceX = float.MaxValue, lowestDistanceY = float.MaxValue;

			int boundsCenterX = (int)Math.Round(bounds.X + bounds.Width / 2f);
			int boundsCenterY = (int)Math.Round(bounds.Y + bounds.Height / 2f);

			//int dx, dy;
			// Calculate nearest grid line of bounding rectangle in X direction
			float currDistanceL = FindNearestSnapPoint(diagramPresenter, bounds.Left, 0, out int dxL, out _);
			if (currDistanceL < lowestDistanceX && currDistanceL >= 0 && currDistanceL <= snapDistance) {
				lowestDistanceX = currDistanceL;
				snapDeltaX = dxL;
			}
			float currDistanceR = FindNearestSnapPoint(diagramPresenter, bounds.Right, 0, out int dxR, out _);
			if (currDistanceR < lowestDistanceX && currDistanceR >= 0 && currDistanceR <= snapDistance) {
				lowestDistanceX = currDistanceR;
				snapDeltaX = dxR;
			}
			// Calculate nearest grid line of bounding rectangle in Y direction
			float currDistanceT = FindNearestSnapPoint(diagramPresenter, 0, bounds.Top, out _, out int dyT);
			if (currDistanceT < lowestDistanceY && currDistanceT >= 0 && currDistanceT <= snapDistance) {
				lowestDistanceY = currDistanceT;
				snapDeltaY = dyT;
			}
			float currDistanceB = FindNearestSnapPoint(diagramPresenter, 0, bounds.Bottom, out _, out int dyB);
			if (currDistanceB < lowestDistanceY && currDistanceB >= 0 && currDistanceB <= snapDistance) {
				lowestDistanceY = currDistanceB;
				snapDeltaY = dyB;
			}
			// Calculate nearest grid line of center point
			float currDistanceCx = FindNearestSnapPoint(diagramPresenter, boundsCenterX, 0, out int dxC, out _);
			if (currDistanceCx < lowestDistanceX && currDistanceCx >= 0 && currDistanceCx <= snapDistance) {
				lowestDistanceX = currDistanceCx;
				snapDeltaX = dxC;
			}
			float currDistanceCy = FindNearestSnapPoint(diagramPresenter, 0, boundsCenterY, out _, out int dyC);
			if (currDistanceCy < lowestDistanceY && currDistanceCy >= 0 && currDistanceCy <= snapDistance) {
				lowestDistanceY = currDistanceCy;
				snapDeltaY = dyC;
			}

			return Geometry.DistancePointPoint(0, 0, lowestDistanceX, lowestDistanceY);
		}


		// @@ Tool-Interface: New 
		/// <summary>
		/// Calculates the distance from the given position to the given shape's control point.
		/// Calculates the distance to the shape's connection foot if 'targetPoint' is ControlPointId.Reference.
		/// </summary>
		/// <returns>The distance in X and Y direction as a Point.</returns>
		protected static Point CalculateDistanceToConnectionTarget(Point position, Shape targetShape, ControlPointId targetPointId) {
			if (targetShape is null) throw new ArgumentNullException(nameof(targetShape));
			if (targetPointId == ControlPointId.Any) throw new ArgumentException();

			Point distance = Point.Empty;
			Point targetPos = Geometry.InvalidPoint;
			if (targetPointId == ControlPointId.Reference) {
				// Calculate snap distance to shape's connection foot (only if position is *not* on a planar shape)
				if (targetShape is ILinearShape || !targetShape.ContainsPoint(position.X, position.Y))
					targetPos = targetShape.CalculateConnectionFoot(position.X, position.Y);
			} else {
				// Calculate snap distance to 'real' control point
				targetPos = targetShape.GetControlPointPosition(targetPointId);
			}
			// Calculate snap distance
			if (Geometry.IsValid(targetPos))
				distance.Offset(targetPos.X - position.X, targetPos.Y - position.Y);
			return distance;
		}


		// @@ Tool-Interface: New 
		/// <summary>
		/// Calculates the distance from the given position to the nearest grid line.
		/// </summary>
		/// <returns>Returns the distance in X and Y direction as Point.</returns>
		protected static Point CalculateDistanceToGridLine(int fromX, int fromY, int gridSize, int range) {
			Point distanceToGridLine = Geometry.InvalidPoint;

			// calculate position of surrounding grid lines
			int left = fromX - (fromX % gridSize);
			int above = fromY - (fromY % gridSize);
			int right = fromX - (fromX % gridSize) + gridSize;
			int below = fromY - (fromY % gridSize) + gridSize;

			float currDistance;
			float lowestDistanceX = float.MaxValue, lowestDistanceY = float.MaxValue;

			// calculate distance from the given point to the surrounding grid lines
			// on X axis
			currDistance = right - fromX;
			if (currDistance <= range && currDistance > 0 && currDistance < lowestDistanceX) {
				lowestDistanceX = currDistance;
				distanceToGridLine.X = right - fromX;
			}
			currDistance = fromX - left;
			if (currDistance <= range && currDistance > 0 && currDistance < lowestDistanceX) {
				lowestDistanceX = currDistance;
				distanceToGridLine.X = left - fromX;
			}
			// on Y axis
			currDistance = fromY - above;
			if (currDistance <= range && currDistance > 0 && currDistance < lowestDistanceY) {
				lowestDistanceY = currDistance;
				distanceToGridLine.Y = above - fromY;
			}
			currDistance = below - fromY;
			if (currDistance <= range && currDistance > 0 && currDistance < lowestDistanceY) {
				lowestDistanceY = currDistance;
				distanceToGridLine.Y = below - fromY;
			}

			return distanceToGridLine;
		}


		// @@ Tool-Interface: New 
		/// <summary>
		/// Calculates the distance between of the mouse cursor and the exact position of the shape or the control point under the mouse cursor.
		/// All positions and distances in diagram coordinates.
		/// </summary>
		/// <param name="mousePos">The exact mouse position.</param>
		/// <param name="shapeAtMouse">The exact position of the shape under the mouse cursor.</param>
		/// <param name="controlPointAtMouse">The exact control point position of the control point under the mouse cursor.</param>
		/// <returns>Distance between mouse cursor and exact position of shape/control point.</returns>
		protected static Point CalculateMouseOffset(Point mousePos, Shape shapeAtMouse, ControlPointId controlPointAtMouse) {
			if (shapeAtMouse is null) throw new ArgumentNullException(nameof(shapeAtMouse));
			if (controlPointAtMouse == ControlPointId.Any) throw new ArgumentException();

			Point offset = Point.Empty;
			Point targetPos = Geometry.InvalidPoint;
			if (controlPointAtMouse == ControlPointId.Reference) {
				// Calculate snap distance to shape's connection foot 
				if (!shapeAtMouse.ContainsPoint(mousePos.X, mousePos.Y))
					targetPos = shapeAtMouse.CalculateConnectionFoot(mousePos.X, mousePos.Y);
			} else if (controlPointAtMouse != ControlPointId.None && controlPointAtMouse != ControlPointId.Any) {
				// Calculate snap distance to 'real' control point
				targetPos = shapeAtMouse.GetControlPointPosition(controlPointAtMouse);
			}
			// Calculate snap distance
			if (Geometry.IsValid(targetPos))
				offset.Offset(targetPos.X - mousePos.X, targetPos.Y - mousePos.Y);
			return offset;
		}

		#endregion


		#region [Protected] Methods: Finding shapes and points

#if DEBUG_COMPATIBILITY

		/// <summary>
		/// Find the topmost shape that is at the given point or has a control point with the given capabilities in range of the given point. 
		/// If parameter onlyUnselected is true, only shapes that are not selected will be returned.
		/// </summary>
		private ShapeAtCursorInfo __FindShapeAtCursor(IDiagramPresenter diagramPresenter, int x, int y, ControlPointCapabilities capabilities, int range, bool onlyUnselected) {
			// Find non-selected shape its connection point under cursor
			ShapeAtCursorInfo result = ShapeAtCursorInfo.Empty;
			int zOrder = int.MinValue;
			foreach (Shape shape in FindVisibleShapes(diagramPresenter, x, y, capabilities, range)) {
				// Skip selected shapes (if not wanted)
				if (onlyUnselected && diagramPresenter.SelectedShapes.Contains(shape)) continue;

				// No need to handle Parent shapes here as Children of CompositeShapes cannot be 
				// selected and grouped shapes keep their ZOrder

				// Skip shapes below the last matching shape
				if (shape.ZOrder < zOrder) continue;
				zOrder = shape.ZOrder;
				result.Shape = shape;
				result.ControlPointId = shape.HitTest(x, y, capabilities, range);
				if (result.Shape is ICaptionedShape)
					result.CaptionIndex = ((ICaptionedShape)shape).FindCaptionFromPoint(x, y);
			}
			return result;
		}


		/// <summary>
		/// Find the topmost shape that is not selected and has a valid ConnectionPoint (or ReferencePoint) 
		/// in range of the given point.
		/// </summary>
		private ShapeAtCursorInfo __DoFindConnectionTarget(IDiagramPresenter diagramPresenter, Shape shape, ControlPointId gluePointId, int x, int y, bool onlyUnselected, int range) {
			if (diagramPresenter == null) throw new ArgumentNullException(nameof(diagramPresenter));
			if (range < 0) throw new ArgumentOutOfRangeException(nameof(range));
			// Find (non-selected shape) its connection point under cursor
			ShapeAtCursorInfo result = ShapeAtCursorInfo.Empty;
			int resultZOrder = int.MinValue;
			if (diagramPresenter.Diagram != null) {
				foreach (Shape s in FindVisibleShapes(diagramPresenter, x, y, ControlPointCapabilities.Connect, range)) {
					if (s == shape) continue;
					if (!diagramPresenter.IsLayerVisible(s.HomeLayer, s.SupplementalLayers)) continue;
					// Skip shapes below the last matching shape
					if (s.ZOrder < resultZOrder) continue;
					// Skip shapes already connected to the found shape via point-to-shape connection
					if (shape != null) {
						if (!CanActiveShapeConnectTo(shape, gluePointId, s)) continue;
						if (!CanActiveShapeConnectTo(diagramPresenter, shape,
							(gluePointId == ControlPointId.FirstVertex) ? ControlPointId.LastVertex : ControlPointId.FirstVertex,
							gluePointId, onlyUnselected)) continue;
					}
					// Skip selected shapes (if not wanted)
					if (onlyUnselected && diagramPresenter.SelectedShapes.Contains(s)) continue;

					// In case a group is under the mouse, use an appropriate child shape of the group
					Shape target = null;
					if (s is IShapeGroup)
						target = s.Children.FindShape(x, y, ControlPointCapabilities.Connect, range, null);
					else target = s;

					// Perform a HitTest on the shape
					ControlPointId pointId = target.HitTest(x, y, ControlPointCapabilities.Connect, range);
					if (pointId != ControlPointId.None) {
						if (target.HasControlPointCapability(pointId, ControlPointCapabilities.Glue)) continue;
						result.Shape = target;
						result.ControlPointId = pointId;
						resultZOrder = target.ZOrder;
						// If the found connection point is a dedicated connection point, take it. If the found connection point is 
						// the reference point of a shape, continue searching for a dedicated connection point.
						if (result.ControlPointId != ControlPointId.Reference)
							break;
					}
				}
			}
			return result;
		}

#endif

		// @@ Tool-Interface: New
		/// <summary>
		/// Creates a filter delegate for all functions that accept a TargetFilterDelegate as parameter.
		/// </summary>
		protected delegate bool TargetFilterDelegate(Shape shape, ControlPointId controlPointId);


		// @@ Tool-Interface: New
		/// <summary>
		/// 
		/// </summary>
		/// <param name="diagramPresenter"></param>
		/// <param name="positionX"></param>
		/// <param name="positionY"></param>
		/// <param name="controlPointCapabilities"></param>
		/// <param name="range"></param>
		/// <param name="isConnectionTargetCallback"></param>
		/// <returns></returns>
		protected static IEnumerable<Locator> FindConnectionTargets(IDiagramPresenter diagramPresenter, int positionX, int positionY, ControlPointCapabilities controlPointCapabilities, int range, TargetFilterDelegate isConnectionTargetCallback) {
			if (diagramPresenter == null) throw new ArgumentNullException(nameof(diagramPresenter));
			if (isConnectionTargetCallback == null) throw new ArgumentNullException(nameof(isConnectionTargetCallback));
			if (diagramPresenter.Diagram == null)
				yield break;
			//
			foreach (Shape shape in diagramPresenter.Diagram.Shapes.FindShapes(positionX, positionY, controlPointCapabilities, range)) {
				foreach (ControlPointId controlPointId in shape.GetControlPointIds(controlPointCapabilities)) {
					if (isConnectionTargetCallback(shape, controlPointId))
						yield return new Locator(shape, controlPointId);
				}
			}
		}


		/// <summary>
		/// Find the topmost shape that is not selected and has a valid ConnectionPoint (or ReferencePoint) in range of the given point.
		/// </summary>
		[Obsolete("Use static method FindConnectionTargets instead.")]

		protected ShapeAtCursorInfo FindConnectionTarget(IDiagramPresenter diagramPresenter, Shape shape, ControlPointId gluePointId, Point newGluePointPos, bool onlyUnselected, bool snapToConnectionPoints) {
			if (diagramPresenter == null) throw new ArgumentNullException(nameof(diagramPresenter));
			if (shape == null) throw new ArgumentNullException(nameof(shape));
			ShapeAtCursorInfo result = ShapeAtCursorInfo.Empty;

			// Find (non-selected shape) its connection point under cursor
			TargetFilterDelegate filter = CreateFilterCallbackForFindConnectionTarget(diagramPresenter, shape, gluePointId, onlyUnselected);
			Locator target = FindNearestTarget(diagramPresenter, newGluePointPos.X, newGluePointPos.Y, ControlPointCapabilities.Connect, true, snapToConnectionPoints ? diagramPresenter.ZoomedGripSize : 0, filter);
			if (!target.IsEmpty) {
				result.Shape = target.Shape;
				result.ControlPointId = target.ControlPointId;
			}

#if DEBUG_COMPATIBILITY
			ShapeAtCursorInfo checkResult = __DoFindConnectionTarget(diagramPresenter, shape, gluePointId, newGluePointPos.X, newGluePointPos.Y, onlyUnselected, snapToConnectionPoints ? diagramPresenter.ZoomedGripSize : 0);
			if (result.Shape != checkResult.Shape || result.ControlPointId != checkResult.ControlPointId)
				Debug.Print("{0}.{1} is not the expected result {2}.{3}!", result.Shape, result.ControlPointId, checkResult.Shape, checkResult.ControlPointId);
#endif

			return result;
		}


		// Create a TargetFilterDelegate for 'FindConnectionTargets'.
		// Needed for backwards compatibility of the old, now obsolete, function 'FindConnectionTarget'.
		private TargetFilterDelegate CreateFilterCallbackForFindConnectionTarget(IDiagramPresenter diagramPresenter, Shape shape, ControlPointId gluePointId, bool onlyUnselected) {
			return (s, cp) => {
				if (s == shape)
					return false;
				if (onlyUnselected && diagramPresenter.SelectedShapes.Contains(s))
					return false;
				// Skip shapes already connected to the found shape via point-to-shape connection
				if (shape != null) {
					if (!CanActiveShapeConnectTo(shape, gluePointId, s))
						return false;
					if (!CanActiveShapeConnectTo(diagramPresenter, shape,
						(gluePointId == ControlPointId.FirstVertex) ? ControlPointId.LastVertex : ControlPointId.FirstVertex,
						gluePointId, onlyUnselected))
						return false;
					return true;
				}
				return false;
			};
		}


		/// <summary>
		/// Find the topmost shape that is not selected and has a valid ConnectionPoint (or ReferencePoint) in range of the given point.
		/// </summary>
		[Obsolete]

		protected ShapeAtCursorInfo FindConnectionTargetFromPosition(IDiagramPresenter diagramPresenter, int x, int y, bool onlyUnselected, bool snapToConnectionPoints) {
			ShapeAtCursorInfo result = ShapeAtCursorInfo.Empty;

			TargetFilterDelegate filter = null;
			if (onlyUnselected)
				filter = (s, cp) => { return diagramPresenter.SelectedShapes.Contains(s) == false; };

			Locator target = FindNearestTarget(diagramPresenter, x, y, ControlPointCapabilities.Connect, true, snapToConnectionPoints ? diagramPresenter.ZoomedGripSize : 0, filter);
			if (!target.IsEmpty) {
				result.Shape = target.Shape;
				result.ControlPointId = target.ControlPointId;
				if (result.Shape is ICaptionedShape captionedShape)
					result.CaptionIndex = captionedShape.FindCaptionFromPoint(x, y);
			}

#if DEBUG_COMPATIBILITY
			ShapeAtCursorInfo checkResult = __DoFindConnectionTarget(diagramPresenter, null, ControlPointId.None, x, y, onlyUnselected, snapToConnectionPoints ? diagramPresenter.ZoomedGripSize : 0);
			if (result.Shape != checkResult.Shape || result.ControlPointId != checkResult.ControlPointId)
				Debug.Print("{0}, Point {1} is not the expected result {2}, Point {3}!", result.Shape, result.ControlPointId, checkResult.Shape, checkResult.ControlPointId);
#endif

			return result;
		}


		/// <summary>
		/// Find the topmost shape that is at the given point or has a control point with the given capabilities in range of the given point. 
		/// If parameter onlyUnselected is true, only shapes that are not selected will be returned.
		/// </summary>
		[Obsolete("Use 'FindNearestTarget' instead.")]
		protected ShapeAtCursorInfo FindShapeAtCursor(IDiagramPresenter diagramPresenter, int x, int y, ControlPointCapabilities capabilities, int range, bool onlyUnselected) {
			// Find non-selected shape its connection point under cursor
			ShapeAtCursorInfo result = ShapeAtCursorInfo.Empty;

			TargetFilterDelegate filter = null;
			if (onlyUnselected)
				filter = (s, cp) => { return diagramPresenter.IsLayerVisible(s.HomeLayer, s.SupplementalLayers) && diagramPresenter.SelectedShapes.Contains(s) == false; };

			Locator target = FindNearestTarget(diagramPresenter, x, y, capabilities, range, filter);
			if (!target.IsEmpty) {
				result.Shape = target.Shape;
				result.ControlPointId = target.ControlPointId;
				if (result.Shape is ICaptionedShape captionedShape)
					result.CaptionIndex = captionedShape.FindCaptionFromPoint(x, y);
			}

#if DEBUG_COMPATIBILITY
			ShapeAtCursorInfo checkResult = __FindShapeAtCursor(diagramPresenter, x, y, capabilities, range, onlyUnselected);
			if (result.Shape != checkResult.Shape || result.ControlPointId != checkResult.ControlPointId)
				Debug.Print("{0}, Point {1} is not the expected result {2}, Point {3}!", result.Shape, result.ControlPointId, checkResult.Shape, checkResult.ControlPointId);
#endif

			return result;
		}

		#endregion


		#region [Protected] Obsolete Methods

		/// <summary>
		/// Indicates whether the given shape can connect to the given targetShape with the specified glue point.
		/// </summary>
		[Obsolete("Use CanActiveShapeConnectTo instead.")]
		protected bool CanConnectTo(Shape shape, ControlPointId gluePointId, Shape targetShape) {
			return CanActiveShapeConnectTo(shape, gluePointId, targetShape);
		}


		///<ToBeCompleted></ToBeCompleted>
		[Obsolete("Use CanActiveShapeConnectTo instead.")]
		protected bool CanConnectTo(IDiagramPresenter diagramPresenter, Shape shape, ControlPointId unmovedGluePoint, 
				ControlPointId movedControlPoint, bool onlyUnselected) {
			return CanActiveShapeConnectTo(diagramPresenter, shape, unmovedGluePoint, movedControlPoint, onlyUnselected);
		}


		/// <summary>
		/// Finds the nearest SnapPoint in range of the given shape's control point.
		/// </summary>
		/// <param name="diagramPresenter">The <see cref="T:Dataweb.NShape.Controllers.IDiagramPresenter" /></param>
		/// <param name="shape">The shape for which the nearest snap point is searched.</param>
		/// <param name="controlPointId">The control point of the shape.</param>
		/// <param name="pointOffsetX">Declares the distance, the shape is moved on X axis before finding snap point.</param>
		/// <param name="pointOffsetY">Declares the distance, the shape is moved on X axis before finding snap point.</param>
		/// <param name="snapDeltaX">Horizontal distance between ptX and the nearest snap point.</param>
		/// <param name="snapDeltaY">Vertical distance between ptY and the nearest snap point.</param>
		/// <returns>Distance to nearest snap point.</returns>
		protected float FindNearestSnapPoint(IDiagramPresenter diagramPresenter, Shape shape, ControlPointId controlPointId, 
				int pointOffsetX, int pointOffsetY, out int snapDeltaX, out int snapDeltaY) {
			if (diagramPresenter == null) throw new ArgumentNullException(nameof(diagramPresenter));
			if (shape == null) throw new ArgumentNullException(nameof(shape));
			if (controlPointId == ControlPointId.None)
				return FindNearestSnapPoint(diagramPresenter, shape, pointOffsetX, pointOffsetY, out snapDeltaX, out snapDeltaY);
			else {
				Point p = shape.GetControlPointPosition(controlPointId);
				return FindNearestSnapPoint(diagramPresenter, p.X + pointOffsetX, p.Y + pointOffsetY, out snapDeltaX, out snapDeltaY);
			}
		}


		/// <summary>
		/// Finds the nearest SnapPoint in range of the given shape.
		/// </summary>
		/// <param name="diagramPresenter">The <see cref="T:Dataweb.NShape.Controllers.IDiagramPresenter" /></param>
		/// <param name="shape">The shape for which the nearest snap point is searched.</param>
		/// <param name="shapeOffsetX">Declares the distance, the shape is moved on X axis before finding snap point.</param>
		/// <param name="shapeOffsetY">Declares the distance, the shape is moved on X axis before finding snap point.</param>
		/// <param name="snapDeltaX">Horizontal distance between ptX and the nearest snap point.</param>
		/// <param name="snapDeltaY">Vertical distance between ptY and the nearest snap point.</param>
		/// <returns>Distance to the calculated snap point.</returns>
		protected float FindNearestSnapPoint(IDiagramPresenter diagramPresenter, Shape shape, int shapeOffsetX, int shapeOffsetY, 
				out int snapDeltaX, out int snapDeltaY) {
			if (diagramPresenter == null) throw new ArgumentNullException(nameof(diagramPresenter));
			if (shape == null) throw new ArgumentNullException(nameof(shape));
			Rectangle shapeBounds = shape.GetBoundingRectangle(true);
			shapeBounds.Offset(shapeOffsetX, shapeOffsetY);
			return FindNearestSnapPoint(diagramPresenter, shapeBounds, out snapDeltaX, out snapDeltaY);
		}


		/// <summary>
		/// Finds the nearest SnapPoint in range of the given shape.
		/// </summary>
		/// <param name="diagramPresenter">The <see cref="T:Dataweb.NShape.Controllers.IDiagramPresenter" /></param>
		/// <param name="shape">The shape for which the nearest snap point is searched.</param>
		/// <param name="pointOffsetX">Declares the distance, the shape is moved on X axis before finding snap point.</param>
		/// <param name="pointOffsetY">Declares the distance, the shape is moved on X axis before finding snap point.</param>
		/// <param name="snapDeltaX">Horizontal distance between ptX and the nearest snap point.</param>
		/// <param name="snapDeltaY">Vertical distance between ptY and the nearest snap point.</param>
		/// <param name="controlPointCapability">Filter for control points taken into 
		/// account while calculating the snap distance.</param>
		/// <returns>Control point of the shape, the calculated distance refers to.</returns>
		protected ControlPointId FindNearestSnapPoint(IDiagramPresenter diagramPresenter, Shape shape, int pointOffsetX, int pointOffsetY,
				out int snapDeltaX, out int snapDeltaY, ControlPointCapabilities controlPointCapability) {
			if (diagramPresenter == null) throw new ArgumentNullException(nameof(diagramPresenter));
			if (shape == null) throw new ArgumentNullException(nameof(shape));

			snapDeltaX = snapDeltaY = 0;
			ControlPointId result = ControlPointId.None;
			int snapDistance = diagramPresenter.SnapDistance;
			float lowestDistance = float.MaxValue;
			foreach (ControlPointId ptId in shape.GetControlPointIds(controlPointCapability)) {
				int dx, dy;
				float currDistance = FindNearestSnapPoint(diagramPresenter, shape, ptId, pointOffsetX, pointOffsetY, out dx, out dy);
				if (currDistance < lowestDistance && currDistance >= 0 && currDistance <= snapDistance) {
					lowestDistance = currDistance;
					result = ptId;
					snapDeltaX = dx;
					snapDeltaY = dy;
				}
			}
			return result;
		}


		/// <summary>
		/// Finds the nearest ControlPoint in range of the given shape's ControlPoint. 
		/// If there is no ControlPoint in range, the snap distance to the nearest grid line will be calculated.
		/// </summary>
		/// <param name="diagramPresenter">The current <see cref="T:Dataweb.NShape.Controllers.IDiagramPresenter" /></param>
		/// <param name="shape">The shape for which the nearest shape control point is searched. Typically the shape that is manipulated by the tool.</param>
		/// <param name="controlPointId">The control point for which the nearest shape control point is searched. Typically the control point that is manipulated by the tool.</param>
		/// <param name="targetPointCapabilities">Capabilities of the control point to find.</param>
		/// <param name="pointOffsetX">Declares the distance, the shape is moved on X axis before finding snap point.</param>
		/// <param name="pointOffsetY">Declares the distance, the shape is moved on X axis before finding snap point.</param>
		/// <param name="snapDeltaX">Horizontal distance between ptX and the nearest snap point.</param>
		/// <param name="snapDeltaY">Vertical distance between ptY and the nearest snap point.</param>
		/// <param name="resultPointId">The Id of the returned shape's nearest ControlPoint.</param>
		/// <returns>The shape owning the found <see cref="T:Dataweb.NShape.Advanced.ControlPointId" />.</returns>
		[Obsolete("Use 'FindNearestTarget' instead.")]
		protected Shape FindNearestControlPoint(IDiagramPresenter diagramPresenter, Shape shape, ControlPointId controlPointId,
			ControlPointCapabilities targetPointCapabilities, int pointOffsetX, int pointOffsetY,
			out int snapDeltaX, out int snapDeltaY, out ControlPointId resultPointId) {
			return FindNearestControlPoint<Shape>(diagramPresenter, shape, controlPointId, targetPointCapabilities, pointOffsetX, pointOffsetY, out snapDeltaX, out snapDeltaY, out resultPointId);
		}


		/// <summary>
		/// Finds the nearest ControlPoint of a shape of type TShape in range of the given shape's ControlPoint. 
		/// If there is no ControlPoint in range, the snap distance to the nearest grid line will be calculated.
		/// </summary>
		/// <param name="diagramPresenter">The current <see cref="T:Dataweb.NShape.Controllers.IDiagramPresenter" /></param>
		/// <param name="shape">The shape for which the nearest shape control point is searched. Typically the shape that is manipulated by the tool.</param>
		/// <param name="controlPointId">The control point for which the nearest shape control point is searched. Typically the control point that is manipulated by the tool.</param>
		/// <param name="targetPointCapabilities">Capabilities of the control point to find.</param>
		/// <param name="offsetX">Declares the offset on X axis before finding control/snap point.</param>
		/// <param name="offsetY">Declares the offset on Y axis before finding control/snap point.</param>
		/// <param name="snapDeltaX">Horizontal distance between ptX and the nearest snap point.</param>
		/// <param name="snapDeltaY">Vertical distance between ptY and the nearest snap point.</param>
		/// <param name="resultPointId">The Id of the returned shape's nearest ControlPoint.</param>
		/// <returns>The shape owning the found <see cref="T:Dataweb.NShape.Advanced.ControlPointId" />.</returns>
		[Obsolete("Use 'FindNearestTarget' instead.")]
		protected TShape FindNearestControlPoint<TShape>(IDiagramPresenter diagramPresenter, Shape shape, ControlPointId controlPointId,
			ControlPointCapabilities targetPointCapabilities, int offsetX, int offsetY,
			out int snapDeltaX, out int snapDeltaY, out ControlPointId resultPointId) where TShape : Shape {
			if (diagramPresenter == null) throw new ArgumentNullException(nameof(diagramPresenter));
			if (shape == null) throw new ArgumentNullException(nameof(shape));

			TShape result = null;
			snapDeltaX = snapDeltaY = 0;
			resultPointId = ControlPointId.None;

			if (diagramPresenter.Diagram != null) {
				// Calculate new position of the ControlPoint
				Point ctrlPtPos = shape.GetControlPointPosition(controlPointId);
				ctrlPtPos.Offset(offsetX, offsetY);

				Locator foundTarget = FindNearestTarget(diagramPresenter, ctrlPtPos.X, ctrlPtPos.Y, targetPointCapabilities, diagramPresenter.SnapDistance, (s, cp) => s is TShape);
				if (!foundTarget.IsEmpty) {
					result = foundTarget.GetShape<TShape>();
					resultPointId = foundTarget.ControlPointId;
				}

				if (foundTarget.Shape != null && foundTarget.ControlPointId != ControlPointId.None) {
					Point d = CalculateDistanceToConnectionTarget(ctrlPtPos, foundTarget.GetShape<TShape>(), foundTarget.ControlPointId);
					snapDeltaX = d.X;
					snapDeltaY = d.Y;
				} else
					FindNearestSnapPoint(diagramPresenter, ctrlPtPos.X, ctrlPtPos.Y, out snapDeltaX, out snapDeltaY);
			}
			return result;
		}


		/// <summary>
		/// Indicates whether a grip was hit
		/// </summary>
		[Obsolete("Use Shape.FindNearestControlPoint and IDiagramPresenter.ZoomedGripSize instead.")]
		protected bool IsGripHit(IDiagramPresenter diagramPresenter, Shape shape, ControlPointId controlPointId, int x, int y) {
			if (shape == null) throw new ArgumentNullException(nameof(shape));
			Point p = shape.GetControlPointPosition(controlPointId);
			return IsGripHit(diagramPresenter, p.X, p.Y, x, y);
		}


		/// <summary>
		/// Indicates whether a grip was hit
		/// </summary>
		[Obsolete("Use Shape.FindNearestControlPoint and IDiagramPresenter.ZoomedGripSize instead.")]
		protected bool IsGripHit(IDiagramPresenter diagramPresenter, int controlPointX, int controlPointY, int x, int y) {
			if (diagramPresenter == null) throw new ArgumentNullException(nameof(diagramPresenter));
			return Geometry.DistancePointPoint(controlPointX, controlPointY, x, y) <= diagramPresenter.ZoomedGripSize;
		}

		#endregion


		#region [Protected] Types

		/// <summary>
		/// The current state of the mouse.
		/// </summary>
		protected struct MouseState : IEquatable<MouseState> {

			/// <ToBeCompleted></ToBeCompleted>
			public static bool operator ==(MouseState a, MouseState b) {
				return (a.Position == b.Position
					&& a.Modifiers == b.Modifiers
					&& a.Buttons == b.Buttons
					&& a.Ticks == b.Ticks);
			}

			/// <ToBeCompleted></ToBeCompleted>
			public static bool operator !=(MouseState a, MouseState b) {
				return !(a == b);
			}

			/// <ToBeCompleted></ToBeCompleted>
			public static MouseState Create(IDiagramPresenter diagramPresenter, MouseEventArgsDg mouseEventArgs) {
				if (diagramPresenter == null) throw new ArgumentNullException(nameof(diagramPresenter));
				MouseState result = Empty;
				result.Buttons = mouseEventArgs.Buttons;
				result.Modifiers = mouseEventArgs.Modifiers;
				result.Clicks = mouseEventArgs.Clicks;
				result.Ticks = DateTime.UtcNow.Ticks;
				diagramPresenter.ControlToDiagram(mouseEventArgs.Position, out result.Position);
				return result;
			}

			/// <ToBeCompleted></ToBeCompleted>
			public static MouseState Empty;

			/// <override></override>
			public override int GetHashCode() {
				return HashCodeGenerator.CalculateHashCode(Position, Buttons, Modifiers, Clicks, Ticks);
			}

			/// <override></override>
			public override bool Equals(object obj) {
				return (obj is MouseState && object.ReferenceEquals(this, obj));
			}

			/// <override></override>
			public bool Equals(MouseState other) {
				return other == this;
			}

			/// <ToBeCompleted></ToBeCompleted>
			public int X {
				get { return Position.X; }
			}

			/// <ToBeCompleted></ToBeCompleted>
			public int Y {
				get { return Position.Y; }
			}

			/// <ToBeCompleted></ToBeCompleted>
			public Point Position;

			/// <ToBeCompleted></ToBeCompleted>
			public KeysDg Modifiers;

			/// <ToBeCompleted></ToBeCompleted>
			public MouseButtonsDg Buttons;

			/// <ToBeCompleted></ToBeCompleted>
			public int Clicks;

			/// <ToBeCompleted></ToBeCompleted>
			public long Ticks;

			/// <summary>
			/// Specifies the pressed mouse buttons.
			/// For MouseUp events, it's the 'formerly pressed and now released' button.
			/// </summary>
			public bool IsButtonDown(MouseButtonsDg button) {
				return (Buttons & button) != 0;
			}

			/// <ToBeCompleted></ToBeCompleted>
			public bool IsKeyPressed(KeysDg modifier) {
				return (Modifiers & modifier) != 0;
			}

			/// <ToBeCompleted></ToBeCompleted>
			public bool IsEmpty {
				get { return this == Empty; }
			}


			static MouseState() {
				Empty.Position = Geometry.InvalidPoint;
				Empty.Modifiers = KeysDg.None;
				Empty.Buttons = 0;
				Empty.Clicks = 0;
				Empty.Ticks = DateTime.MinValue.Ticks;
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected struct ActionDef {

			/// <ToBeCompleted></ToBeCompleted>
			public static readonly ActionDef Empty;

			/// <ToBeCompleted></ToBeCompleted>
			public static ActionDef Create(IDiagramPresenter diagramPresenter, int action, MouseState mouseState, bool wantsAutoScroll) {
				ActionDef result = ActionDef.Empty;
				result.diagramPresenter = diagramPresenter;
				result.action = action;
				result.mouseState = mouseState;
				result.wantsAutoScroll = wantsAutoScroll;
				return result;
			}

			/// <ToBeCompleted></ToBeCompleted>
			public ActionDef(IDiagramPresenter diagramPresenter, int action, MouseState mouseState, bool wantsAutoScroll) {
				this.diagramPresenter = diagramPresenter;
				this.action = action;
				this.mouseState = mouseState;
				this.wantsAutoScroll = wantsAutoScroll;
			}

			/// <ToBeCompleted></ToBeCompleted>
			public IDiagramPresenter DiagramPresenter {
				get { return diagramPresenter; }
			}

			/// <ToBeCompleted></ToBeCompleted>
			public MouseState MouseState {
				get { return mouseState; }
			}

			/// <ToBeCompleted></ToBeCompleted>
			public int Action {
				get { return action; }
			}

			/// <ToBeCompleted></ToBeCompleted>
			public bool WantsAutoScroll {
				get { return wantsAutoScroll; }
			}

			static ActionDef() {
				Empty.diagramPresenter = null;
				Empty.action = int.MinValue;
				Empty.mouseState = MouseState.Empty;
				Empty.wantsAutoScroll = false;
			}

			private IDiagramPresenter diagramPresenter;
			private MouseState mouseState;
			private int action;
			private bool wantsAutoScroll;
		}


		/// <ToBeCompleted></ToBeCompleted>		
		protected struct ShapeAtCursorInfo : IEquatable<ShapeAtCursorInfo> {

			/// <ToBeCompleted></ToBeCompleted>
			public static bool operator ==(ShapeAtCursorInfo a, ShapeAtCursorInfo b) {
				return (a.Shape == b.Shape
					&& a.ControlPointId == b.ControlPointId
					&& a.CaptionIndex == b.CaptionIndex);
			}

			/// <ToBeCompleted></ToBeCompleted>
			public static bool operator !=(ShapeAtCursorInfo a, ShapeAtCursorInfo b) {
				return !(a == b);
			}

			/// <ToBeCompleted></ToBeCompleted>
			public static ShapeAtCursorInfo Create(Shape shape, ControlPointId controlPointId, int captionIndex = -1) {
				ShapeAtCursorInfo result = ShapeAtCursorInfo.Empty;
				result.Shape = shape;
				result.ControlPointId = controlPointId;
				result.CaptionIndex = captionIndex;
				return result;
			}
			/// <ToBeCompleted></ToBeCompleted>
			public static ShapeAtCursorInfo Create(Locator value) {
				return Create(value.Shape, value.ControlPointId);
			}

			/// <ToBeCompleted></ToBeCompleted>
			public static ShapeAtCursorInfo Empty;

			/// <override></override>
			public override int GetHashCode() {
				return HashCodeGenerator.CalculateHashCode(Shape, ControlPointId, CaptionIndex);
			}

			/// <override></override>
			public override bool Equals(object obj) {
				return (obj is ShapeAtCursorInfo && object.ReferenceEquals(this, obj));
			}

			/// <ToBeCompleted></ToBeCompleted>
			public bool Equals(ShapeAtCursorInfo other) {
				return other == this;
			}

			/// <ToBeCompleted></ToBeCompleted>
			public void Clear() {
				this.CaptionIndex = Empty.CaptionIndex;
				this.ControlPointId = Empty.ControlPointId;
				this.Shape = Empty.Shape;
			}

			/// <ToBeCompleted></ToBeCompleted>
			public Shape Shape;

			/// <ToBeCompleted></ToBeCompleted>
			public ControlPointId ControlPointId;

			/// <ToBeCompleted></ToBeCompleted>
			public int CaptionIndex;

			/// <ToBeCompleted></ToBeCompleted>
			public void Assign(Locator instance) {
				Shape = instance.Shape;
				ControlPointId = instance.ControlPointId;
				CaptionIndex = Empty.CaptionIndex;
			}

			/// <ToBeCompleted></ToBeCompleted>
			[Obsolete]
			public bool CanConnect(ShapeAtCursorInfo other) {
				return Tool.CanConnect(Shape, ControlPointId, other.Shape, other.ControlPointId);
			}

			/// <ToBeCompleted></ToBeCompleted>
			[Obsolete]
			public bool CanConnect(Shape otherShape, ControlPointId otherControlPointId) {
				return Tool.CanConnect(Shape, ControlPointId, otherShape, otherControlPointId);
			}

			/// <ToBeCompleted></ToBeCompleted>
			public bool IsCursorAtGrip {
				get {
					// ControlPointCapabilities of grips/handles of a shape that can be moved with the mouse
					const ControlPointCapabilities gripCapabilities = ControlPointCapabilities.Resize | ControlPointCapabilities.Rotate
							| ControlPointCapabilities.Movable | ControlPointCapabilities.Glue;
					return (Shape != null && ControlPointId != ControlPointId.None && ControlPointId != ControlPointId.Reference
						&& Shape.HasControlPointCapability(ControlPointId, gripCapabilities));
				}
			}

			/// <ToBeCompleted></ToBeCompleted>
			public bool IsCursorAtGluePoint {
				get {
					return (Shape != null && Shape.HasControlPointCapability(ControlPointId, ControlPointCapabilities.Glue));
				}
			}

			/// <ToBeCompleted></ToBeCompleted>
			public bool IsCursorAtConnectionPoint {
				get {
					return (Shape != null && Shape.HasControlPointCapability(ControlPointId, ControlPointCapabilities.Connect));
				}
			}

			/// <ToBeCompleted></ToBeCompleted>
			public bool IsCursorAtCaption {
				get { return (Shape is ICaptionedShape && CaptionIndex >= 0 && !IsCursorAtGrip); }
			}

			/// <ToBeCompleted></ToBeCompleted>
			public bool IsEmpty {
				get { return Shape == null; }
			}

			static ShapeAtCursorInfo() {
				Empty.Shape = null;
				Empty.ControlPointId = ControlPointId.None;
				Empty.CaptionIndex = -1;
			}
		}

		#endregion


		#region Fields

		/// <summary>The default minimum distance the mouse must move for a drag event.</summary>
		public static readonly Size DefaultMinimumDragDistance = new Size(4, 4);

		/// <summary>The margin around the tool's icon image.</summary>
		protected const int IconMargin = 1;
		/// <summary>The color that will appear as transparent in the tool's icon image.</summary>
		protected readonly Color IconTransparentColor = Color.LightGray;

		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("Use IconMargin instead.")]
		protected const int margin = IconMargin;
		
		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("Use IconTransparentColor instead.")]
		protected readonly Color transparentColor = Color.LightGray;

		// @@ Tool-Interface: Changed from private to internal. Make protected?
		// Default range for highlighting connection targets when moving a glue point
		internal const int DefaultHighlightTargetsRange = 50;

		// Unique name of the tool.
		private string _name;
		// Title that will be displayed in the tool box
		private string _title;
		// Category title of the tool, used for grouping tools in the tool box
		private string _category;
		// Hint that will be displayed when the mouse is hovering the tool
		private string _description;
		// small icon of the tool
		private Bitmap _smallIcon;
		// the large icon of the tool
		private Bitmap _largeIcon;
		// minimum distance the mouse has to move before a drag action is considered as drag action
		private Size _minDragDistance = DefaultMinimumDragDistance;
		// the interval which is used to determine whether two subsequent clicks are interpreted as double click.
		private int _doubleClickInterval = 500;
		//
		// --- Mouse state after last mouse event ---
		// State of the mouse after the last ProcessMouseEvents call
		private MouseState _currentMouseState = MouseState.Empty;

		// --- Definition of current action(s) ---
		// The stack contains 
		// - the display that is edited with this tool,
		// - transformed coordinates of the mouse position when an action has started (diagram coordinates)
		private Stack<ActionDef> _pendingActions = new Stack<ActionDef>(1);

		#endregion
	}

}
