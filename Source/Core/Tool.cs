/******************************************************************************
  Copyright 2009-2016 dataweb GmbH
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
using System.Timers;

using Dataweb.NShape.Advanced;
using Dataweb.NShape.Commands;
using Dataweb.NShape.Controllers;


namespace Dataweb.NShape {

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
		public ToolExecutedEventArgs(Tool tool, ToolResult eventType)
			: base() {
			if (tool == null) throw new ArgumentNullException("tool");
			this.tool = tool;
			this.eventType = eventType;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public Tool Tool {
			get { return tool; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		public ToolResult EventType {
			get { return eventType; }
		}


		private Tool tool;
		private ToolResult eventType;

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
			if (smallIcon != null)
				smallIcon.Dispose();
			smallIcon = null;

			if (largeIcon != null)
				largeIcon.Dispose();
			largeIcon = null;
		}

		#endregion


		/// <summary>
		/// A culture independent name, used as title if no title was specified.
		/// </summary>
		public string Name {
			get { return name; }
		}


		/// <summary>
		/// A culture dependent title, used as label for toolbox items.
		/// </summary>
		public string Title {
			get { return title; }
			set { title = value; }
		}


		/// <summary>
		/// A culture dependent description, used as description for toolbox items.
		/// </summary>
		public virtual string Description {
			// TODO 2: Remove this implementation, when all derived classes have a better one.
			get { return description; }
			set { description = value; }
		}


		/// <summary>
		/// Specifies the toolbox category.
		/// </summary>
		public string Category {
			get { return category; }
			set { category = value; }
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
			get { return smallIcon; }
			set { smallIcon = value; }
		}


		/// <summary>
		/// A large icon for this <see cref="T:Dataweb.NShape.Advanced.Tool" />.
		/// </summary>
		public Bitmap LargeIcon {
			get { return largeIcon; }
			set { largeIcon = value; }
		}


		/// <summary>
		/// Specifies the minimum distance the mouse has to be moved before the tool is starting a drag action.
		/// </summary>
		public Size MinimumDragDistance {
			get { return minDragDistance; }
			set { minDragDistance = value; }
		}


		/// <summary>
		/// Specifies the interval (in ms) used for determine if two subsequent clicks are interpreted as double click.
		/// </summary>
		public int DoubleClickTime {
			get { return doubleClickInterval; }
			set { doubleClickInterval = value; }
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
			if (diagramPresenter == null) throw new ArgumentNullException("display");
			currentMouseState.Buttons = e.Buttons;
			currentMouseState.Modifiers = e.Modifiers;
			currentMouseState.Clicks = e.Clicks;
			currentMouseState.Ticks = DateTime.UtcNow.Ticks;
			diagramPresenter.ControlToDiagram(e.Position, out currentMouseState.Position);
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
			if (diagramPresenter == null) throw new ArgumentNullException("diagramPresenter");
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

			currentMouseState = MouseState.Empty;

			OnToolExecuted(CancelledEventArgs);
		}


		/// <summary>
		/// Specifis if the tool wants the diagram presenter to scroll when reaching the presenter's bounds.
		/// </summary>
		public virtual bool WantsAutoScroll {
			get {
				if (pendingActions.Count == 0) return false;
				else return pendingActions.Peek().WantsAutoScroll;
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
			get { return pendingActions.Count > 0; }
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
			smallIcon = new Bitmap(16, 16);
			largeIcon = new Bitmap(32, 32);
			name = "Tool_" + this.GetHashCode().ToString();
			ExecutedEventArgs = new ToolExecutedEventArgs(this, ToolResult.Executed);
			CancelledEventArgs = new ToolExecutedEventArgs(this, ToolResult.Canceled);
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.Tool" />.
		/// </summary>
		protected Tool(string category)
			: this() {
			if (!string.IsNullOrEmpty(category))
				this.category = category;
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
			get { return currentMouseState; }
		}


		/// <summary>
		/// The display used by the current (pending) action.
		/// </summary>
		protected IDiagramPresenter ActionDiagramPresenter {
			get {
				if (pendingActions.Count == 0) throw new NShapeException("The action's current display was not set yet. Call StartToolAction method to set the action's current display.");
				else return pendingActions.Peek().DiagramPresenter;
			}
		}


		/// <summary>
		/// Transformed start coordinates of the current (pending) action (diagram coordinates).
		/// Use SetActionStartPosition method to set this value and ClearActionStartPosition to clear it.
		/// </summary>
		protected MouseState ActionStartMouseState {
			get {
				if (pendingActions.Count == 0) throw new NShapeInternalException("The action's start mouse state was not set yet. Call SetActionStartPosition method to set the start position.");
				else return pendingActions.Peek().MouseState;
			}
		}


		/// <summary>
		/// Specifies the current tool action.
		/// </summary>
		protected ActionDef CurrentToolAction {
			get {
				if (pendingActions.Count > 0)
					return pendingActions.Peek();
				else return ActionDef.Empty;
			}
		}


		/// <summary>
		/// Indicates whether a tool action is pending.
		/// </summary>
		protected IEnumerable<ActionDef> PendingToolActions {
			get { return pendingActions; }
		}


		/// <summary>
		/// Specifies the number of pending tool actions.
		/// </summary>
		protected int PendingToolActionsCount {
			get { return pendingActions.Count; }
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
			if (diagramPresenter == null) throw new ArgumentNullException("diagramPresenter");
			if (mouseState == MouseState.Empty) throw new ArgumentException("mouseState");
			if (pendingActions.Count > 0) {
				if (pendingActions.Peek().DiagramPresenter != diagramPresenter)
					throw new NShapeException("There are actions pending for an other diagram presenter!");
			}
			ActionDef actionDef = ActionDef.Create(diagramPresenter, action, mouseState, wantAutoScroll);
			pendingActions.Push(actionDef);
		}


		/// <summary>
		/// Ends a tool's action. Clears the start position for the action and the display used for the action.
		/// </summary>
		/// <remarks>When overriding this method, the call to the base implementation has to be called at last.</remarks>
		protected virtual void EndToolAction() {
			if (pendingActions.Count <= 0) throw new InvalidOperationException("No tool actions pending.");
			IDiagramPresenter diagramPresenter = pendingActions.Peek().DiagramPresenter;
			if (diagramPresenter != null) {
				Invalidate(diagramPresenter);
				diagramPresenter.Capture = false;
				diagramPresenter.SetCursor(CursorProvider.DefaultCursorID);
			}
			pendingActions.Pop();
		}


		/// <summary>
		/// Perform all necessary actions to cancel the current tool actions.
		/// </summary>
		protected abstract void CancelCore();


		/// <summary>
		/// Called when a tool action has finished. Raises the ToolExecuted event.
		/// </summary>
		protected virtual void OnToolExecuted(ToolExecutedEventArgs eventArgs) {
			if (IsToolActionPending) throw new InvalidOperationException(string.Format("{0} tool actions pending.", pendingActions.Count));
			if (ToolExecuted != null) ToolExecuted(this, eventArgs);
		}

		#endregion


		#region [Protected] Methods

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
				// Delete model obejct
				if (shape.ModelObject != null)
					shape.ModelObject = null;
				// Delete shape
				shape.Invalidate();
				shape.Dispose();
				shape = null;
			}
		}


		/// <summary>
		/// Indicates whether the given shape can connect to the given targetShape with the specified glue point.
		/// </summary>
		[Obsolete("Use CanActiveShapeConnectTo instead.")]
		protected bool CanConnectTo(Shape shape, ControlPointId gluePointId, Shape targetShape) {
			return CanActiveShapeConnectTo(shape, gluePointId, targetShape);
		}


		/// <summary>
		/// Indicates whether the given shape can connect to the given targetShape with the specified glue point.
		/// </summary>
		protected bool CanActiveShapeConnectTo(Shape shape, ControlPointId gluePointId, Shape targetShape) {
			if (shape == null) throw new ArgumentNullException("shape");
			if (targetShape == null) throw new ArgumentNullException("targetShape");
			// Connecting both glue points to the same target shape via Point-to-Shape connection is not allowed
			ControlPointId connectedPoint = ControlPointId.None;
			foreach (ShapeConnectionInfo sci in shape.GetConnectionInfos(ControlPointId.Any, targetShape)) {
				if ((sci.OtherPointId == ControlPointId.Reference && sci.OwnPointId != gluePointId)
					|| (sci.OwnPointId == ControlPointId.Reference && targetShape.HasControlPointCapability(sci.OtherPointId, ControlPointCapabilities.Glue)))
					return false;
			}
			return (shape.CanConnect(gluePointId, targetShape, ControlPointId.Reference)
				&& targetShape.CanConnect(ControlPointId.Reference, shape, gluePointId));
		}


		/// <summary>
		/// Indicates whether the given shape can connect to the given targetShape with the specified glue point.
		/// </summary>
		protected bool CanActiveShapeConnectTo(Shape shape, ControlPointId gluePointId, Shape targetShape, ControlPointId targetPointId) {
			if (shape == null) throw new ArgumentNullException("shape");
			if (targetShape == null) throw new ArgumentNullException("targetShape");
			// Connecting both glue points to the same target shape via Point-to-Shape connection is not allowed
			ControlPointId connectedPoint = ControlPointId.None;
			foreach (ShapeConnectionInfo sci in shape.GetConnectionInfos(ControlPointId.Any, targetShape)) {
				if ((sci.OtherPointId == ControlPointId.Reference && sci.OwnPointId != gluePointId)
					|| (sci.OwnPointId == ControlPointId.Reference && targetShape.HasControlPointCapability(sci.OtherPointId, ControlPointCapabilities.Glue)))
					return false;
			}
			return (shape.CanConnect(gluePointId, targetShape, targetPointId)
				&& targetShape.CanConnect(targetPointId, shape, gluePointId));
		}


		///<ToBeCompleted></ToBeCompleted>
		[Obsolete("Use CanActiveShapeConnectTo instead.")]
		protected bool CanConnectTo(IDiagramPresenter diagramPresenter, Shape shape, ControlPointId unmovedGluePoint, ControlPointId movedControlPoint, bool onlyUnselected) {
			return CanActiveShapeConnectTo(diagramPresenter, shape, unmovedGluePoint, movedControlPoint, onlyUnselected);
		}


		/// <summary>
		/// Indicates whether the given linear shape can connect to the given targetShape with the specified glue point.
		/// This method also checks the unmoved glue point (you cannot connect two glue points to the same shape via Point-to-Shape).
		/// </summary>
		protected bool CanActiveShapeConnectTo(IDiagramPresenter diagramPresenter, Shape shape, ControlPointId unmovedGluePoint, ControlPointId movedGluePoint, bool onlyUnselected) {
			if (shape is ILinearShape && ((ILinearShape)shape).VertexCount == 2) {
				Point posA = shape.GetControlPointPosition(unmovedGluePoint);
				Point posB = shape.GetControlPointPosition(movedGluePoint);
				ShapeAtCursorInfo shapeInfoA = FindShapeAtCursor(diagramPresenter, posA.X, posA.Y, ControlPointCapabilities.All, diagramPresenter.ZoomedGripSize, onlyUnselected);
				ShapeAtCursorInfo shapeInfoB = FindShapeAtCursor(diagramPresenter, posB.X, posB.Y, ControlPointCapabilities.All, diagramPresenter.ZoomedGripSize, onlyUnselected);
				if (!shapeInfoA.IsEmpty
					&& shapeInfoA.Shape == shapeInfoB.Shape
					&& (shapeInfoA.ControlPointId == ControlPointId.Reference
						|| shapeInfoB.ControlPointId == ControlPointId.Reference))
					return false;
			}
			return true;
		}


		/// <summary>
		/// Indicates whether a grip was hit
		/// </summary>
		protected bool IsGripHit(IDiagramPresenter diagramPresenter, Shape shape, ControlPointId controlPointId, int x, int y) {
			if (shape == null) throw new ArgumentNullException("shape");
			Point p = shape.GetControlPointPosition(controlPointId);
			return IsGripHit(diagramPresenter, p.X, p.Y, x, y);
		}


		/// <summary>
		/// Indicates whether a grip was hit
		/// </summary>
		protected bool IsGripHit(IDiagramPresenter diagramPresenter, int controlPointX, int controlPointY, int x, int y) {
			if (diagramPresenter == null) throw new ArgumentNullException("display");
			return Geometry.DistancePointPoint(controlPointX, controlPointY, x, y) <= diagramPresenter.ZoomedGripSize;
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


#if DEBUG_DIAGNOSTICS
		internal void Assert(bool condition) {
			Assert(condition, null);
		}


		internal void Assert(bool condition, string message) {
			if (condition == false) {
				if (string.IsNullOrEmpty(message)) throw new NShapeInternalException("Assertion Failure.");
				else throw new NShapeInternalException(string.Format("Assertion Failure: {0}", message));
			}
		}
#endif

		#endregion


		#region [Protected] Methods (Drawing and Invalidating)

		/// <summary>
		/// Invalidates all connection targets in range.
		/// </summary>
		protected void InvalidateConnectionTargets(IDiagramPresenter diagramPresenter, int currentPosX, int currentPosY) {
			// invalidate selectedShapes in last range
			diagramPresenter.InvalidateGrips(shapesInRange, ControlPointCapabilities.Connect);

			if (Geometry.IsValid(currentPosX, currentPosY)) {
				ShapeAtCursorInfo shapeAtCursor = FindConnectionTargetFromPosition(diagramPresenter, currentPosX, currentPosY, false, true);
				if (!shapeAtCursor.IsEmpty) shapeAtCursor.Shape.Invalidate();

				// invalidate selectedShapes in current range
				shapesInRange.Clear();
				shapesInRange.AddRange(FindVisibleShapes(diagramPresenter, currentPosX, currentPosY, ControlPointCapabilities.Connect, pointHighlightRange));
				if (shapesInRange.Count > 0)
					diagramPresenter.InvalidateGrips(shapesInRange, ControlPointCapabilities.Connect);
			}
		}


		/// <summary>
		/// Draws all connection targets in range.
		/// </summary>
		protected void DrawConnectionTargets(IDiagramPresenter diagramPresenter, Shape shape, int x, int y) {
			if (!Geometry.IsValid(x, y)) throw new ArgumentException("x, y");
			Point p = Point.Empty;
			p.Offset(x, y);
			DrawConnectionTargets(diagramPresenter, shape, ControlPointId.None, p, EmptyEnumerator<Shape>.Empty);
		}


		/// <summary>
		/// Draws all connection targets in range.
		/// </summary>
		protected void DrawConnectionTargets(IDiagramPresenter diagramPresenter, Shape shape, int x, int y, IEnumerable<Shape> excludedShapes) {
			Point p = Point.Empty;
			p.Offset(x, y);
			DrawConnectionTargets(diagramPresenter, shape, ControlPointId.None, p, excludedShapes);
		}


		/// <summary>
		/// Draws all connection targets in range.
		/// </summary>
		protected void DrawConnectionTargets(IDiagramPresenter diagramPresenter, Shape shape, ControlPointId gluePtId, Point newGluePtPos) {
			DrawConnectionTargets(diagramPresenter, shape, gluePtId, newGluePtPos, EmptyEnumerator<Shape>.Empty);
		}


		/// <summary>
		/// Draws all connection targets in range.
		/// </summary>
		protected void DrawConnectionTargets(IDiagramPresenter diagramPresenter, Shape shape, ControlPointId gluePtId, Point newGluePtPos, IEnumerable<Shape> excludedShapes) {
			if (diagramPresenter == null) throw new ArgumentNullException("diagramPresenter");
			if (!Geometry.IsValid(newGluePtPos)) throw new ArgumentException("newGluePtPos");
			if (shape == null) throw new ArgumentNullException("shape");
			//if (gluePtId == ControlPointId.None || gluePtId == ControlPointId.Any)
			//   throw new ArgumentException(string.Format("{0} is not a valid {1} for this operation.", gluePtId, typeof(ControlPointId).Name));
			//if (!shape.HasControlPointCapability(gluePtId, ControlPointCapabilities.Glue))
			//   throw new ArgumentException(string.Format("{0} is not a valid glue point.", gluePtId));
			if (!diagramPresenter.Project.SecurityManager.IsGranted(Permission.Connect, shape))
				return;

			// Find connection target shape at the given position
			ShapeAtCursorInfo shapeAtCursor = ShapeAtCursorInfo.Empty;
			if (shape != null && gluePtId != ControlPointId.None)
				shapeAtCursor = FindConnectionTarget(diagramPresenter, shape, gluePtId, newGluePtPos, false, false);
			else shapeAtCursor = FindConnectionTargetFromPosition(diagramPresenter, newGluePtPos.X, newGluePtPos.Y, false, false);

			// Add shapes in range to the shapebuffer and then remove all excluded shapes
			shapeBuffer.Clear();
			shapeBuffer.AddRange(shapesInRange);
			foreach (Shape excludedShape in excludedShapes) {
				shapeBuffer.Remove(excludedShape);
				if (excludedShape == shapeAtCursor.Shape)
					shapeAtCursor.Clear();
			}

			// If there is no ControlPoint under the Cursor and the cursor is over a shape, draw the shape's outline
			if (!shapeAtCursor.IsEmpty && shapeAtCursor.ControlPointId == ControlPointId.Reference
				&& shapeAtCursor.Shape.ContainsPoint(newGluePtPos.X, newGluePtPos.Y)) {
				diagramPresenter.DrawShapeOutline(IndicatorDrawMode.Highlighted, shapeAtCursor.Shape);
			}

			// Draw all connectionPoints of all shapes in range (except the excluded ones, see above)
			diagramPresenter.ResetTransformation();
			try {
				for (int i = shapeBuffer.Count - 1; i >= 0; --i) {
					Shape s = shapeBuffer[i];
					foreach (int ptId in s.GetControlPointIds(ControlPointCapabilities.Connect)) {
						if (!(shape.CanConnect(gluePtId, s, ptId) && s.CanConnect(ptId, shape, gluePtId))) 
							continue;
						IndicatorDrawMode drawMode = IndicatorDrawMode.Normal;
						if (s == shapeAtCursor.Shape && ptId == shapeAtCursor.ControlPointId)
							drawMode = IndicatorDrawMode.Highlighted;
						diagramPresenter.DrawConnectionPoint(drawMode, s, ptId);
					}
				}
			} finally { diagramPresenter.RestoreTransformation(); }
		}

		#endregion


		#region [Protected] Methods (Finding shapes and points)

		/// <summary>
		/// Finds the nearest snap point for a point.
		/// </summary>
		/// <param name="diagramPresenter">The <see cref="T:Dataweb.NShape.Controllers.IDiagramPresenter" /></param>
		/// <param name="x">X coordinate</param>
		/// <param name="y">Y coordinate</param>
		/// <param name="snapDeltaX">Horizontal distance between x and the nearest snap point.</param>
		/// <param name="snapDeltaY">Vertical distance between y and the nearest snap point.</param>
		/// <returns>Distance to nearest snap point.</returns>
		/// <remarks>If snapping is disabled for the current ownerDisplay, this function does nothing.</remarks>
		protected float FindNearestSnapPoint(IDiagramPresenter diagramPresenter, int x, int y, out int snapDeltaX, out int snapDeltaY) {
			if (diagramPresenter == null) throw new ArgumentNullException("diagramPresenter");

			float distance = float.MaxValue;
			snapDeltaX = snapDeltaY = 0;
			if (diagramPresenter.SnapToGrid) {
				// calculate position of surrounding grid lines
				int gridSize = diagramPresenter.GridSize;
				int left = x - (x % gridSize);
				int above = y - (y % gridSize);
				int right = x - (x % gridSize) + gridSize;
				int below = y - (y % gridSize) + gridSize;
				float currDistance = 0;
				int snapDistance = diagramPresenter.SnapDistance;

				// calculate distance from the given point to the surrounding grid lines
				currDistance = y - above;
				if (currDistance <= snapDistance && currDistance >= 0 && currDistance < distance) {
					distance = currDistance;
					snapDeltaY = above - y;
				}
				currDistance = right - x;
				if (currDistance <= snapDistance && currDistance >= 0 && currDistance < distance) {
					distance = currDistance;
					snapDeltaX = right - x;
				}
				currDistance = below - y;
				if (currDistance <= snapDistance && currDistance >= 0 && currDistance < distance) {
					distance = currDistance;
					snapDeltaY = below - y;
				}
				currDistance = x - left;
				if (currDistance <= snapDistance && currDistance >= 0 && currDistance < distance) {
					distance = currDistance;
					snapDeltaX = left - x;
				}

				// calculate approximate distance from the given point to the surrounding grid points
				currDistance = Geometry.DistancePointPoint(x, y, left, above);
				if (currDistance <= snapDistance && currDistance >= 0 && currDistance < distance) {
					distance = currDistance;
					snapDeltaX = left - x;
					snapDeltaY = above - y;
				}
				currDistance = Geometry.DistancePointPoint(x, y, right, above);
				if (currDistance <= snapDistance && currDistance >= 0 && currDistance < distance) {
					distance = currDistance;
					snapDeltaX = right - x;
					snapDeltaY = above - y;
				}
				currDistance = Geometry.DistancePointPoint(x, y, left, below);
				if (currDistance <= snapDistance && currDistance >= 0 && currDistance < distance) {
					distance = currDistance;
					snapDeltaX = left - x;
					snapDeltaY = below - y;
				}
				currDistance = Geometry.DistancePointPoint(x, y, right, below);
				if (currDistance <= snapDistance && currDistance >= 0 && currDistance < distance) {
					distance = currDistance;
					snapDeltaX = right - x;
					snapDeltaY = below - y;
				}
			}
			return distance;
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
			if (diagramPresenter == null) throw new ArgumentNullException("diagramPresenter");
			if (shape == null) throw new ArgumentNullException("shape");

			snapDeltaX = snapDeltaY = 0;
			Point p = shape.GetControlPointPosition(controlPointId);
			return FindNearestSnapPoint(diagramPresenter, p.X + pointOffsetX, p.Y + pointOffsetY, out snapDeltaX, out snapDeltaY);
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
			if (diagramPresenter == null) throw new ArgumentNullException("diagramPresenter");
			if (shape == null) throw new ArgumentNullException("shape");

			snapDeltaX = snapDeltaY = 0;
			int snapDistance = diagramPresenter.SnapDistance;
			float lowestDistance = float.MaxValue;

			Rectangle shapeBounds = shape.GetBoundingRectangle(true);
			shapeBounds.Offset(shapeOffsetX, shapeOffsetY);
			int boundsCenterX = (int)Math.Round(shapeBounds.X + shapeBounds.Width / 2f);
			int boundsCenterY = (int)Math.Round(shapeBounds.Y + shapeBounds.Width / 2f);

			int dx, dy;
			float currDistance;
			// Calculate snap distance of center point
			currDistance = FindNearestSnapPoint(diagramPresenter, boundsCenterX, boundsCenterY, out dx, out dy);
			if (currDistance < lowestDistance && currDistance >= 0 && currDistance <= snapDistance) {
				lowestDistance = currDistance;
				snapDeltaX = dx;
				snapDeltaY = dy;
			}

			// Calculate snap distance of bounding rectangle
			currDistance = FindNearestSnapPoint(diagramPresenter, shapeBounds.Left, shapeBounds.Top, out dx, out dy);
			if (currDistance < lowestDistance && currDistance >= 0 && currDistance <= snapDistance) {
				lowestDistance = currDistance;
				snapDeltaX = dx;
				snapDeltaY = dy;
			}
			currDistance = FindNearestSnapPoint(diagramPresenter, shapeBounds.Right, shapeBounds.Top, out dx, out dy);
			if (currDistance < lowestDistance && currDistance >= 0 && currDistance <= snapDistance) {
				lowestDistance = currDistance;
				snapDeltaX = dx;
				snapDeltaY = dy;
			}
			currDistance = FindNearestSnapPoint(diagramPresenter, shapeBounds.Left, shapeBounds.Bottom, out dx, out dy);
			if (currDistance < lowestDistance && currDistance >= 0 && currDistance <= snapDistance) {
				lowestDistance = currDistance;
				snapDeltaX = dx;
				snapDeltaY = dy;
			}
			currDistance = FindNearestSnapPoint(diagramPresenter, shapeBounds.Right, shapeBounds.Bottom, out dx, out dy);
			if (currDistance < lowestDistance && currDistance >= 0 && currDistance <= snapDistance) {
				lowestDistance = currDistance;
				snapDeltaX = dx;
				snapDeltaY = dy;
			}
			return lowestDistance;
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
			if (diagramPresenter == null) throw new ArgumentNullException("diagramPresenter");
			if (shape == null) throw new ArgumentNullException("shape");

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
		/// <param name="diagramPresenter">The <see cref="T:Dataweb.NShape.Controllers.IDiagramPresenter" /></param>
		/// <param name="shape">The given shape.</param>
		/// <param name="controlPointId">the given shape's ControlPoint</param>
		/// <param name="targetPointCapabilities">Capabilities of the control point to find.</param>
		/// <param name="pointOffsetX">Declares the distance, the shape is moved on X axis before finding snap point.</param>
		/// <param name="pointOffsetY">Declares the distance, the shape is moved on X axis before finding snap point.</param>
		/// <param name="snapDeltaX">Horizontal distance between ptX and the nearest snap point.</param>
		/// <param name="snapDeltaY">Vertical distance between ptY and the nearest snap point.</param>
		/// <param name="resultPointId">The Id of the returned shape's nearest ControlPoint.</param>
		/// <returns>The shape owning the found <see cref="T:Dataweb.NShape.Advanced.ControlPointId" />.</returns>
		protected Shape FindNearestControlPoint(IDiagramPresenter diagramPresenter, Shape shape, ControlPointId controlPointId,
			ControlPointCapabilities targetPointCapabilities, int pointOffsetX, int pointOffsetY,
			out int snapDeltaX, out int snapDeltaY, out ControlPointId resultPointId) {
			if (diagramPresenter == null) throw new ArgumentNullException("diagramPresenter");
			if (shape == null) throw new ArgumentNullException("shape");

			Shape result = null;
			snapDeltaX = snapDeltaY = 0;
			resultPointId = ControlPointId.None;

			if (diagramPresenter.Diagram != null) {
				// calculate new position of the ControlPoint
				Point ctrlPtPos = shape.GetControlPointPosition(controlPointId);
				ctrlPtPos.Offset(pointOffsetX, pointOffsetY);

				int snapDistance = diagramPresenter.SnapDistance;
				int resultZOrder = int.MinValue;
				IEnumerable<Shape> foundShapes = FindVisibleShapes(diagramPresenter, ctrlPtPos.X, ctrlPtPos.Y, ControlPointCapabilities.Connect, snapDistance);
				foreach (Shape foundShape in foundShapes) {
					if (foundShape == shape) continue;
					//
					// Find the nearest control point
					float distance, lowestDistance = float.MaxValue;
					ControlPointId foundPtId = foundShape.FindNearestControlPoint(
							ctrlPtPos.X, ctrlPtPos.Y, snapDistance, targetPointCapabilities);
					//
					// Skip shapes without matching control points or below the last matching shape
					if (foundPtId == ControlPointId.None) continue;
					if (foundShape.ZOrder < resultZOrder) continue;
					//
					// If a valid control point was found, check whether it matches the criteria
					if (foundPtId != ControlPointId.Reference) {
						// If the shape itself is hit, do not calculate the snap distance because snapping 
						// to "real" control point has a higher priority.
						// Set TargetPointId and result shape in order to skip snapping to gridlines
						Point targetPtPos = foundShape.GetControlPointPosition(foundPtId);
						distance = Geometry.DistancePointPoint(ctrlPtPos.X, ctrlPtPos.Y, targetPtPos.X, targetPtPos.Y);
						if (distance <= snapDistance && distance < lowestDistance) {
							lowestDistance = distance;
							snapDeltaX = targetPtPos.X - ctrlPtPos.X;
							snapDeltaY = targetPtPos.Y - ctrlPtPos.Y;
						} else continue;
					}
					resultZOrder = foundShape.ZOrder;
					resultPointId = foundPtId;
					result = foundShape;
				}
				// calcualte distance to nearest grid point if there is no suitable control point in range
				if (resultPointId == ControlPointId.None)
					FindNearestSnapPoint(diagramPresenter, ctrlPtPos.X, ctrlPtPos.Y, out snapDeltaX, out snapDeltaY);
				else if (result != null && resultPointId == ControlPointId.Reference) {
					// ToDo: Calculate snap distance if the current mouse position outside the shape's outline
					if (result.ContainsPoint(ctrlPtPos.X, ctrlPtPos.Y)) {

					}
				}
			}
			return result;
		}


		///// <summary>
		///// Find the topmost shape that is not selected and has a valid ConnectionPoint (or ReferencePoint) in range of the given point.
		///// </summary>
		//protected ShapeAtCursorInfo FindConnectionTargetFromPosition(IDiagramPresenter diagramPresenter, int x, int y, bool onlyUnselected) {
		//   return FindConnectionTargetFromPosition(diagramPresenter, x, y, onlyUnselected, true);
		//}


		/// <summary>
		/// Find the topmost shape that is not selected and has a valid ConnectionPoint (or ReferencePoint) in range of the given point.
		/// </summary>
		protected ShapeAtCursorInfo FindConnectionTargetFromPosition(IDiagramPresenter diagramPresenter, int x, int y, bool onlyUnselected, bool snapToConnectionPoints) {
			return DoFindConnectionTarget(diagramPresenter, null, ControlPointId.None, x, y, onlyUnselected, snapToConnectionPoints ? diagramPresenter.ZoomedGripSize : 0);
		}


		/// <summary>
		/// Find the topmost shape that is not selected and has a valid ConnectionPoint (or ReferencePoint) in range of the given point.
		/// </summary>
		protected ShapeAtCursorInfo FindConnectionTarget(IDiagramPresenter diagramPresenter, Shape shape, ControlPointId gluePointId, Point newGluePointPos, bool onlyUnselected, bool snapToConnectionPoints) {
			if (diagramPresenter == null) throw new ArgumentNullException("diagramPresenter");
			if (shape == null) throw new ArgumentNullException("shape");
			// Find (non-selected shape) its connection point under cursor
			ShapeAtCursorInfo result = ShapeAtCursorInfo.Empty;
			if (diagramPresenter.Diagram != null)
				result = DoFindConnectionTarget(diagramPresenter, shape, gluePointId, newGluePointPos.X, newGluePointPos.Y, onlyUnselected, snapToConnectionPoints ? diagramPresenter.ZoomedGripSize : 0);
			return result;
		}


		/// <summary>
		/// Find the topmost shape that is at the given point or has a control point with the given capabilities in range of the given point. 
		/// If parameter onlyUnselected is true, only shapes that are not selected will be returned.
		/// </summary>
		protected ShapeAtCursorInfo FindShapeAtCursor(IDiagramPresenter diagramPresenter, int x, int y, ControlPointCapabilities capabilities, int range, bool onlyUnselected) {
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
		/// Finds all shapes that meet the given requirements and are *not* invisible due to layer restrictions
		/// </summary>
		protected IEnumerable<Shape> FindVisibleShapes(IDiagramPresenter diagramPresenter, int x, int y, ControlPointCapabilities pointCapabilities, int distance) {
			if (diagramPresenter.Diagram == null) yield break;
			foreach (Shape s in diagramPresenter.Diagram.Shapes.FindShapes(x, y, pointCapabilities, distance)) {
				if (s.Layers == LayerIds.None || diagramPresenter.IsLayerVisible(s.Layers)) 
					yield return s;
			}
		}


		/// <summary>
		/// Find the topmost shape that is not selected and has a valid ConnectionPoint (or ReferencePoint) 
		/// in range of the given point.
		/// </summary>
		private ShapeAtCursorInfo DoFindConnectionTarget(IDiagramPresenter diagramPresenter, Shape shape, ControlPointId gluePointId, int x, int y, bool onlyUnselected, int range) {
			if (diagramPresenter == null) throw new ArgumentNullException("diagramPresenter");
			if (range < 0) throw new ArgumentOutOfRangeException("range");
			// Find (non-selected shape) its connection point under cursor
			ShapeAtCursorInfo result = ShapeAtCursorInfo.Empty;
			int resultZOrder = int.MinValue;
			if (diagramPresenter.Diagram != null) {
				foreach (Shape s in FindVisibleShapes(diagramPresenter, x, y, ControlPointCapabilities.Connect, range)) {
					if (s == shape) continue;
					if (s.Layers != LayerIds.None && !diagramPresenter.IsLayerVisible(s.Layers)) continue;
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
			public static MouseState Empty;

			/// <override></override>
			public override int GetHashCode() {
				// Overflow is fine, just wrap
				unchecked {
					// We use prime numbers 17 and 23, could also be other prime numbers
					int result = 17;
					result = result * 23 + Position.GetHashCode();
					result = result * 23 + Buttons.GetHashCode();
					result = result * 23 + Modifiers.GetHashCode();
					result = result * 23 + Ticks.GetHashCode();
					return result;
				}
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

			/// <ToBeCompleted></ToBeCompleted>
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
			public static ShapeAtCursorInfo Empty;

			/// <override></override>
			public override int GetHashCode() {
				// Overflow is fine, just wrap
				unchecked {
					// We use prime numbers 17 and 23, could also be other prime numbers
					int result = 17;
					if (Shape != null) result = result * 23 + Shape.GetHashCode();
					result = result * 23 + ControlPointId.GetHashCode();
					result = result * 23 + CaptionIndex.GetHashCode();
					return result;
				}
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
			public bool CanConnect(ShapeAtCursorInfo other) {
				if (IsEmpty || other == ShapeAtCursorInfo.Empty
					|| !((IsCursorAtConnectionPoint && other.IsCursorAtGluePoint) || (other.IsCursorAtConnectionPoint && IsCursorAtGluePoint)))
					return false;
				else
					return CanConnect(other.Shape, other.ControlPointId);
			}

			/// <ToBeCompleted></ToBeCompleted>
			public bool CanConnect(Shape otherShape, ControlPointId otherControlPointId) {
				return (Shape.CanConnect(ControlPointId, otherShape, otherControlPointId)
					&& otherShape.CanConnect(otherControlPointId, Shape, ControlPointId));
			}

			/// <ToBeCompleted></ToBeCompleted>
			public bool IsCursorAtGrip {
				get {
					return (Shape != null
					&& ControlPointId != ControlPointId.None
					&& ControlPointId != ControlPointId.Reference);
				}
			}

			/// <ToBeCompleted></ToBeCompleted>
			public bool IsCursorAtGluePoint {
				get {
					return (Shape != null
						&& Shape.HasControlPointCapability(ControlPointId, ControlPointCapabilities.Glue));
				}
			}

			/// <ToBeCompleted></ToBeCompleted>
			public bool IsCursorAtConnectionPoint {
				get {
					return (Shape != null
						&& Shape.HasControlPointCapability(ControlPointId, ControlPointCapabilities.Connect));
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

		#endregion


		#region Fields

		/// <ToBeCompleted></ToBeCompleted>
		protected int margin = 1;
		/// <ToBeCompleted></ToBeCompleted>
		protected Color transparentColor = Color.LightGray;

		// --- Description of the tool ---
		// Unique name of the tool.
		private string name;
		// Title that will be displayed in the tool box
		private string title;
		// Category title of the tool, used for grouping tools in the tool box
		private string category;
		// Hint that will be displayed when the mouse is hovering the tool
		private string description;
		// small icon of the tool
		private Bitmap smallIcon;
		// the large icon of the tool
		private Bitmap largeIcon;
		// minimum distance the mouse has to move before a drag action is considered as drag action
		private Size minDragDistance = new Size(4, 4);
		// the interval which is used to determine whether two subsequent clicks are interpreted as double click.
		private int doubleClickInterval = 500;
		//
		// margin and background colors of the toolbox icons "LargeIcon" and "SmallIcon"
		// highlighting connection targets in range
		private int pointHighlightRange = 50;
		//
		// --- Mouse state after last mouse event ---
		// State of the mouse after the last ProcessMouseEvents call
		private MouseState currentMouseState = MouseState.Empty;
		// Shapes whose connection points will be highlighted in the next drawing
		private List<Shape> shapesInRange = new List<Shape>();

		// --- Definition of current action(s) ---
		// The stack contains 
		// - the display that is edited with this tool,
		// - transformed coordinates of the mouse position when an action has started (diagram coordinates)
		private Stack<ActionDef> pendingActions = new Stack<ActionDef>(1);
		// 
		// Work buffer for shapes
		private List<Shape> shapeBuffer = new List<Shape>();

		#endregion
	}

}