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
using System.Timers;

using Dataweb.NShape.Advanced;
using Dataweb.NShape.Commands;
using Dataweb.NShape.Controllers;


namespace Dataweb.NShape {

	/// <summary>
	/// Specifies the action performed when subsequently clicking on overlapping shapes.
	/// </summary>
	public enum OverlappingShapesAction {
		/// <summary>When clicking overlapping shapes, the selected shape will not change.</summary>
		None,
		/// <summary>When clicking overlapping shapes, the topmost shape is selected on each click.</summary>
		Topmost,
		/// <summary>When clicking overlapping shapes, the selection cycles through the overlapping shapes on each click.</summary>
		/// <remarks>This is the default setting.</remarks>
		Cycle
	}


	/// <summary>
	/// Lets the user size, move, rotate and select shapes.
	/// </summary>
	public class SelectionTool : Tool {

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.SelectionTool" />.
		/// </summary>
		public SelectionTool()
			: base("Standard") {
			Construct();
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.SelectionTool" />.
		/// </summary>
		public SelectionTool(string category)
			: base(category) {
			Construct();
		}


		/// <summary>
		/// Specifies how selection of overlapping shapes is handled.
		/// </summary>
		public OverlappingShapesAction OverlappingShapesAction {
			get { return _overlappingShapesSelectionMode; }
			set { _overlappingShapesSelectionMode = value; }
		}


		#region [Public] Tool Members

		/// <override></override>
		public override void RefreshIcons() {
			// nothing to do...
		}


		/// <override></override>
		public override bool ProcessMouseEvent(IDiagramPresenter diagramPresenter, MouseEventArgsDg e) {
			if (diagramPresenter == null) throw new ArgumentNullException(nameof(diagramPresenter));
			bool result = false;
			// get new mouse state
			MouseState newMouseState = MouseState.Create(diagramPresenter, e);
			try {
				diagramPresenter.SuspendUpdate();

				// Only process mouse action if the position of the mouse or a mouse button state changed
				//if (e.EventType != MouseEventType.MouseMove || newMouseState.Position != CurrentMouseState.Position) {
				// Process the mouse event
				switch (e.EventType) {
					case MouseEventType.MouseDown:
						// Start drag action such as drawing a SelectionFrame or moving selectedShapes/shape handles
						result = ProcessMouseDown(diagramPresenter, newMouseState);
						break;

					case MouseEventType.MouseMove:
						// Set cursors depending on HotSpots or draw moving/resizing preview
						result = ProcessMouseMove(diagramPresenter, newMouseState);
						break;

					case MouseEventType.MouseUp:
						// perform selection/moving/resizing
						result = ProcessMouseUp(diagramPresenter, newMouseState);
						break;

					default: throw new NShapeUnsupportedValueException(e.EventType);
				}
				//}
			} finally {
				diagramPresenter.ResumeUpdate();
			}
			base.ProcessMouseEvent(diagramPresenter, e);
			return result;
		}


		/// <override></override>
		public override bool ProcessKeyEvent(IDiagramPresenter diagramPresenter, KeyEventArgsDg e) {
			if (diagramPresenter == null) throw new ArgumentNullException(nameof(diagramPresenter));
			// if the keyPress was not handled by the base class, try to handle it here
			bool result = false;
			switch (e.EventType) {
				case KeyEventType.PreviewKeyDown:
				case KeyEventType.KeyPress:
					// do nothing
					break;
				case KeyEventType.KeyDown:
				case KeyEventType.KeyUp:
					if ((KeysDg)e.KeyCode == KeysDg.Delete) {
						// Update selected shape unter the mouse cursor because it was propably deleted
						if (!_selectedShapeAtCursorInfo.IsEmpty && !diagramPresenter.SelectedShapes.Contains(_selectedShapeAtCursorInfo.Shape)) {
							SetSelectedShapeAtCursor(diagramPresenter, CurrentMouseState.X, CurrentMouseState.Y, diagramPresenter.ZoomedGripSize, ControlPointCapabilities.All);
							Invalidate(diagramPresenter);
						}
					}

					// Update MouseState and Cursor when modifier keys are pressed or released
					KeysDg keyCode = (KeysDg)e.KeyCode;
					if ((keyCode & KeysDg.Shift) == KeysDg.Shift
						|| (keyCode & KeysDg.ShiftKey) == KeysDg.ShiftKey
						|| (keyCode & KeysDg.Control) == KeysDg.Control
						|| (keyCode & KeysDg.ControlKey) == KeysDg.ControlKey
						|| (keyCode & KeysDg.Alt) == KeysDg.Alt) {
						MouseState mouseState = CurrentMouseState;
						mouseState.Modifiers = (KeysDg)e.Modifiers;
						int cursorId = DetermineCursor(diagramPresenter, mouseState);
						diagramPresenter.SetCursor(cursorId);
					}
					break;
				default: throw new NShapeUnsupportedValueException(e.EventType);
			}
			if (base.ProcessKeyEvent(diagramPresenter, e)) result = true;
			return result;
		}


		/// <override></override>
		public override void EnterDisplay(IDiagramPresenter diagramPresenter) {
			// nothing to do
		}


		/// <override></override>
		public override void LeaveDisplay(IDiagramPresenter diagramPresenter) {
			// nothing to do
		}


		/// <override></override>
		public override IEnumerable<MenuItemDef> GetMenuItemDefs(IDiagramPresenter diagramPresenter) {
			if (diagramPresenter == null) throw new ArgumentNullException(nameof(diagramPresenter));
			int mouseX = CurrentMouseState.X;
			int mouseY = CurrentMouseState.Y;

			// Update the selected shape at cursor
			SetSelectedShapeAtCursor(diagramPresenter, mouseX, mouseY, diagramPresenter.ZoomedGripSize, ControlPointCapabilities.All);

			// Return the shape's actions
			if (diagramPresenter.SelectedShapes.Count == 1 && !_selectedShapeAtCursorInfo.IsEmpty) {
				bool separatorRequired = false;
				// ToDo: Create an aggregated command that creates a composite shape first and then a template from it
				if (_selectedShapeAtCursorInfo.Shape.Template != null) {
					// Deliver Template's actions
					foreach (MenuItemDef action in _selectedShapeAtCursorInfo.Shape.Template.GetMenuItemDefs()) {
						if (!separatorRequired) separatorRequired = true;
						yield return action;
					}
				}
				foreach (MenuItemDef action in _selectedShapeAtCursorInfo.Shape.GetMenuItemDefs(mouseX, mouseY, diagramPresenter.ZoomedGripSize)) {
					if (separatorRequired) yield return new SeparatorMenuItemDef();
					yield return action;
				}
				if (_selectedShapeAtCursorInfo.Shape.ModelObject != null) {
					if (separatorRequired) yield return new SeparatorMenuItemDef();
					foreach (MenuItemDef action in _selectedShapeAtCursorInfo.Shape.ModelObject.GetMenuItemDefs())
						yield return action;
				}
			} else {
				// ToDo: Find shape under the cursor and return its actions?
				// ToDo: Collect all actions provided by the diagram if no shape was right-clicked
			}
			// ToDo: Add tool-specific actions?
		}


		/// <override></override>
		public override void Draw(IDiagramPresenter diagramPresenter) {
			if (diagramPresenter == null) throw new ArgumentNullException(nameof(diagramPresenter));
			switch (CurrentAction) {
				case Action.Select:
					// nothing to do
					break;

				case Action.None:
				case Action.EditCaption:
					IDiagramPresenter presenter = (CurrentAction == Action.None) ? diagramPresenter : ActionDiagramPresenter;
					// MouseOver-Highlighting of the caption under the cursor 
					if (IsEditCaptionFeasible(presenter, CurrentMouseState, _selectedShapeAtCursorInfo)) {
						diagramPresenter.ResetTransformation();
						try {
							diagramPresenter.DrawCaptionBounds(IndicatorDrawMode.Highlighted, (ICaptionedShape)_selectedShapeAtCursorInfo.Shape, _selectedShapeAtCursorInfo.CaptionIndex);
						} finally { diagramPresenter.RestoreTransformation(); }
					}
					break;

				case Action.SelectWithFrame:
					diagramPresenter.ResetTransformation();
					try {
						diagramPresenter.DrawSelectionFrame(_frameRect);
					} finally { diagramPresenter.RestoreTransformation(); }
					break;

				case Action.MoveShape:
				case Action.MoveHandle:
					// Draw shape previews first
					diagramPresenter.DrawShapes(Previews.Values);

					// Then draw snap-lines and -points
					if (_selectedShapeAtCursorInfo != null && (_snapPtId > 0 || _snapDistanceX != 0 || _snapDistanceY != 0)) {
						Shape previewAtCursor = FindPreviewOfShape(_selectedShapeAtCursorInfo.Shape);
						diagramPresenter.DrawSnapIndicators(previewAtCursor);
					}
					// Finally, draw highlighted connection points and/or highlighted shape outlines
					if (diagramPresenter.SelectedShapes.Count == 1 && _selectedShapeAtCursorInfo.ControlPointId != ControlPointId.None) {
						Shape selectedPreview = FindPreviewOfShape(SelectedShapeAtCursor ?? diagramPresenter.SelectedShapes.TopMost);
						if (selectedPreview.HasControlPointCapability(_selectedShapeAtCursorInfo.ControlPointId, ControlPointCapabilities.Glue)) {
							// Find and highlight valid connection targets in range
							Point p = selectedPreview.GetControlPointPosition(SelectedControlPointIdAtCursor);
							DrawConnectionTargets(ActionDiagramPresenter, selectedPreview, SelectedControlPointIdAtCursor, p,
									ConnectionCapabilities, HighlightTargetsRange, GetFindTargetRange(diagramPresenter), CreateIsConnectionTargetFilter(diagramPresenter));
						}
					}
					break;

				case Action.PrepareRotate:
				case Action.Rotate:
					if (CurrentAction == Action.Rotate)
						diagramPresenter.DrawShapes(Previews.Values);
					diagramPresenter.ResetTransformation();
					try {
						if (PendingToolActionsCount == 1) {
							diagramPresenter.DrawAnglePreview(_rectBuffer.Location, CurrentMouseState.Position, _cursors[ToolCursor.Rotate], 0, 0);
						} else {
							// Get MouseState of the first click (on the rotation point)
							MouseState initMouseState = GetMouseStateOfPreviousAction();
							int startAngle, sweepAngle;
							CalcAngle(initMouseState, ActionStartMouseState, CurrentMouseState, out startAngle, out sweepAngle);

							// ToDo: Determine standard cursor size
							_rectBuffer.Location = _selectedShapeAtCursorInfo.Shape.GetControlPointPosition(_selectedShapeAtCursorInfo.ControlPointId);
							_rectBuffer.Width = _rectBuffer.Height = (int)Math.Ceiling(Geometry.DistancePointPoint(_rectBuffer.Location, CurrentMouseState.Position));

							diagramPresenter.DrawAnglePreview(_rectBuffer.Location, CurrentMouseState.Position,
								_cursors[ToolCursor.Rotate], startAngle, sweepAngle);
						}
					} finally { diagramPresenter.RestoreTransformation(); }
					break;

				default: throw new NShapeUnsupportedValueException(CurrentAction);
			}
		}


		/// <override></override>
		public override void Invalidate(IDiagramPresenter diagramPresenter) {
			if (diagramPresenter == null) throw new ArgumentNullException(nameof(diagramPresenter));
			switch (CurrentAction) {
				case Action.None:
				case Action.Select:
				case Action.EditCaption:
					if (!_selectedShapeAtCursorInfo.IsEmpty) {
						_selectedShapeAtCursorInfo.Shape.Invalidate();
						diagramPresenter.InvalidateGrips(_selectedShapeAtCursorInfo.Shape, ControlPointCapabilities.All);
					}
					break;

				case Action.SelectWithFrame:
					diagramPresenter.DisplayService.Invalidate(_frameRect);
					break;

				case Action.MoveHandle:
				case Action.MoveShape:
					if (Previews.Count > 0 && _selectedShapeAtCursorInfo.Shape != null) {
						InvalidateShapes(diagramPresenter, Previews.Values, ControlPointCapabilities.All);
						if (diagramPresenter.SnapToGrid) {
							Shape previewAtCursor = FindPreviewOfShape(_selectedShapeAtCursorInfo.Shape);
							diagramPresenter.InvalidateSnapIndicators(previewAtCursor);
						}
						if (CurrentAction == Action.MoveHandle && _selectedShapeAtCursorInfo.IsCursorAtGluePoint)
							InvalidateConnectionTargets(diagramPresenter, CurrentMouseState.X, CurrentMouseState.Y, 
									ConnectionCapabilities, HighlightTargetsRange, CreateIsConnectionTargetFilter(diagramPresenter));
					}
					break;

				case Action.PrepareRotate:
				case Action.Rotate:
					if (Previews.Count > 0)
						InvalidateShapes(diagramPresenter, Previews.Values);
					InvalidateAnglePreview(diagramPresenter);
					break;

				default: throw new NShapeUnsupportedValueException(typeof(MenuItemDef), CurrentAction);
			}
		}


		/// <summary>
		/// Returns the preview shape clone that is currently used to represent the preview of the current tool action for the given shape.
		/// Returns null if no such preview shape exists.
		/// </summary>
		public Shape FindPreviewOfShape(Shape shape) {
			if (shape == null) throw new ArgumentNullException(nameof(shape));
			DebugDiagnostics.Assert(_previewShapes.ContainsKey(shape), string.Format("No preview found for '{0}' shape.", shape.Type.Name));
			if (_previewShapes.TryGetValue(shape, out Shape result))
				return result;
			return null;
		}


		/// <summary>
		/// Returns the original shape the given preview shape clone was created from.
		/// </summary>
		public Shape FindShapeOfPreview(Shape previewShape) {
			if (previewShape == null) throw new ArgumentNullException(nameof(previewShape));
			DebugDiagnostics.Assert(_originalShapes.ContainsKey(previewShape), string.Format("No original shape found for '{0}' preview shape.", previewShape.Type.Name));
			return _originalShapes[previewShape];
		}


		/// <override></override>
		protected override void CancelCore() {
			_frameRect = Rectangle.Empty;
			_rectBuffer = Rectangle.Empty;

			//currentToolAction = ToolAction.None;
			_selectedShapeAtCursorInfo.Clear();
		}

		#endregion


		#region [Protected] Types

		/// <summary>
		/// Specifies the current action of the tool.
		/// </summary>
		protected enum Action {
			/// <summary>No action in progress</summary>
			None,
			/// <summary>A select action has been started.</summary>
			Select,
			/// <summary>An area select action has been started.</summary>
			SelectWithFrame,
			/// <summary>A edit action of an ICaptionedShape's caption has been started.</summary>
			EditCaption,
			/// <summary>A shape move action has been started.</summary>
			MoveShape,
			/// <summary>A shape handle move action (resize or connect) has been started.</summary>
			MoveHandle,
			/// <summary>A rotate action has been started but the mouse is too close to the rotation point.</summary>
			PrepareRotate,
			/// <summary>A rotate action has been started.</summary>
			Rotate
		}

		#endregion


		#region [Protected] Properties

		// @@ Tool-Interface: New
		/// <summary>
		/// The (original) shape that was selected and under the cursor when the tool action started.
		/// </summary>
		protected Shape SelectedShapeAtCursor {
			get { return _selectedShapeAtCursorInfo.Shape; }
		}


		// @@ Tool-Interface: New
		/// <summary>
		/// The control point of SelectedShapeAtCursor that was under the cursor when the tool action started.
		/// </summary>
		protected ControlPointId SelectedControlPointIdAtCursor {
			get { return _selectedShapeAtCursorInfo.ControlPointId; }
		}


		// @@ Tool-Interface: New
		/// <summary>
		/// The index of SelectedShapeAtCursor's caption that was under the cursor when the tool action started.
		/// </summary>
		protected int SelectedCaptionIndexAtCursor {
			get { return _selectedShapeAtCursorInfo.CaptionIndex; }
		}

		#endregion


		#region [Protected] Tool Members

		/// <override></override>
		protected override void StartToolAction(IDiagramPresenter diagramPresenter, int action, MouseState mouseState, bool wantAutoScroll) {
			if (diagramPresenter == null) throw new ArgumentNullException(nameof(diagramPresenter)); 
			base.StartToolAction(diagramPresenter, action, mouseState, wantAutoScroll);
			// Cancel deferred click action (if pending)
			CancelDeferredClickAction();
			// Empty selection frame
			_frameRect.Location = mouseState.Position;
			_frameRect.Size = Size.Empty;
		}


		/// <override></override>
		protected override void EndToolAction() {
			base.EndToolAction();
			//currentToolAction = ToolAction.None;
			if (!IsToolActionPending)
				ClearPreviews();
		}

		#endregion


		#region [Protected] MouseEvent processing implementation

		/// <summary>
		/// Process mouse down event
		/// </summary>
		private bool ProcessMouseDown(IDiagramPresenter diagramPresenter, MouseState mouseState) {
			if (diagramPresenter == null) throw new ArgumentNullException(nameof(diagramPresenter));
			bool result = false;

			// Check if the selected shape at cursor is still valid
			if (!_selectedShapeAtCursorInfo.IsEmpty
				&& (!diagramPresenter.SelectedShapes.Contains(_selectedShapeAtCursorInfo.Shape)
					|| _selectedShapeAtCursorInfo.Shape.HitTest(mouseState.X, mouseState.Y, ControlPointCapabilities.All, diagramPresenter.ZoomedGripSize) == ControlPointId.None)) {
				_selectedShapeAtCursorInfo.Clear();
			}

			// If no action is pending, try to start a new one...
			if (CurrentAction == Action.None) {
				// Get suitable action (depending on the currently selected shape under the mouse cursor)
				Action newAction = DetermineMouseDownAction(diagramPresenter, mouseState);
				if (newAction != Action.None) {
					//currentToolAction = newAction;
					bool wantAutoScroll;
					switch (newAction) {
						case Action.SelectWithFrame:
						case Action.MoveHandle:
						case Action.MoveShape:
							wantAutoScroll = true;
							result = true;
							break;
						default:
							wantAutoScroll = false;
							break;
					}
					StartToolAction(diagramPresenter, (int)newAction, mouseState, wantAutoScroll);

					// If the action requires preview shapes, create them now...
					switch (CurrentAction) {
						case Action.None:
						case Action.Select:
						case Action.SelectWithFrame:
						case Action.EditCaption:
							break;
						case Action.MoveHandle:
						case Action.MoveShape:
						case Action.PrepareRotate:
						case Action.Rotate:
							CreatePreviewShapes(diagramPresenter);
							result = true;
							break;
						default: throw new NShapeUnsupportedValueException(CurrentAction);
					}

					Invalidate(ActionDiagramPresenter);
				}
			} else {
				// ... otherwise cancel the action (if right mouse button was pressed)
				Action newAction = DetermineMouseDownAction(diagramPresenter, mouseState);
				if (newAction == Action.None) {
					Cancel();
					result = true;
				}
			}

			return result;
		}


		/// <summary>
		/// Process mouse move event
		/// </summary>
		private bool ProcessMouseMove(IDiagramPresenter diagramPresenter, MouseState mouseState) {
			if (diagramPresenter == null) throw new ArgumentNullException(nameof(diagramPresenter)); 
			bool result = true;

			if (!_selectedShapeAtCursorInfo.IsEmpty &&
				!diagramPresenter.SelectedShapes.Contains(_selectedShapeAtCursorInfo.Shape))
				_selectedShapeAtCursorInfo.Clear();

			Action newAction;
			switch (CurrentAction) {
				case Action.None:
					result = false;
					SetSelectedShapeAtCursor(diagramPresenter, mouseState.X, mouseState.Y, diagramPresenter.ZoomedGripSize, ControlPointCapabilities.All);
					Invalidate(diagramPresenter);
					break;

				case Action.Select:
				case Action.EditCaption:
					// Find unselected shape under the mouse cursor
					ShapeAtCursorInfo shapeAtCursorInfo = ShapeAtCursorInfo.Empty;
					newAction = CurrentAction;
					// As long as the current action is a point/click based action such as Select and EditCaption, we do *not* start
					// any new drag action until the mouse movement exceeds the minimum drag distance...
					if (IsDragActionFeasible(mouseState, MouseButtonsDg.Left)) {
						if (mouseState.IsButtonDown(MouseButtonsDg.Left)) {
							// Determine new drag action
							// Caution: Use previous mouse state because this was the point where the user clicked before dragging the mouse away - which is handled by this call!
							// We try to the the Action's start mouse state, the previous or - as fallback - the current mouse state.
							MouseState prevMouseState = !ActionStartMouseState.IsEmpty ? ActionStartMouseState : !CurrentMouseState.IsEmpty ? CurrentMouseState : mouseState;
							shapeAtCursorInfo = ShapeAtCursorInfo.Create(FindNearestTarget(diagramPresenter, prevMouseState.X, prevMouseState.Y, ControlPointCapabilities.None, 0, null));
							newAction = DetermineMouseMoveAction(diagramPresenter, prevMouseState, shapeAtCursorInfo);
						} else
							newAction = DetermineMouseMoveAction(diagramPresenter, mouseState, shapeAtCursorInfo);

						// If the action has changed, prepare and start the new action
						if (newAction != CurrentAction) {
							switch (newAction) {
								// Select --> SelectWithFrame
								case Action.SelectWithFrame:
									DebugDiagnostics.Assert(CurrentAction == Action.Select);
									StartToolAction(diagramPresenter, (int)newAction, ActionStartMouseState, true);
									_frameRect = PrepareFrameSelection(ActionDiagramPresenter, ActionStartMouseState);
									break;

								// Select --> (Select shape and) move shape
								case Action.MoveShape:
								case Action.MoveHandle:
								case Action.PrepareRotate:
									DebugDiagnostics.Assert(CurrentAction == Action.Select || CurrentAction == Action.EditCaption);
									if (_selectedShapeAtCursorInfo.IsEmpty) {
										// Select shape at cursor before start dragging it
										PerformSelection(ActionDiagramPresenter, ActionStartMouseState, shapeAtCursorInfo);
										SetSelectedShapeAtCursor(diagramPresenter, ActionStartMouseState.X, ActionStartMouseState.Y, 0, ControlPointCapabilities.None);
										DebugDiagnostics.Assert(!_selectedShapeAtCursorInfo.IsEmpty);
									}
									// Init moving shape
									DebugDiagnostics.Assert(!_selectedShapeAtCursorInfo.IsEmpty);
									CreatePreviewShapes(ActionDiagramPresenter);
									StartToolAction(diagramPresenter, (int)newAction, ActionStartMouseState, true);
									MoveShapesPreview(ActionDiagramPresenter, ActionStartMouseState);
									break;

								// Select --> (Select shape and) rotate shape / edit shape caption
								case Action.Rotate:
								case Action.EditCaption:
								case Action.None:
								case Action.Select:
									Debug.Fail("Unhandled change of CurrentAction.");
									break;
								default:
									Debug.Fail(string.Format("Unexpected {0} value: {1}", CurrentAction.GetType().Name, CurrentAction));
									break;
							}
							//currentToolAction = newAction;
						}
					}
					Invalidate(ActionDiagramPresenter);
					break;

				case Action.SelectWithFrame:
					_frameRect = PrepareFrameSelection(ActionDiagramPresenter, mouseState);
					break;

				case Action.MoveHandle:
					DebugDiagnostics.Assert(IsMoveHandleFeasible(ActionDiagramPresenter, mouseState, _selectedShapeAtCursorInfo));
					MoveHandlePreview(ActionDiagramPresenter, mouseState);
					break;

				case Action.MoveShape:
					MoveShapesPreview(diagramPresenter, mouseState);
					break;

				case Action.PrepareRotate:
				case Action.Rotate:
					DebugDiagnostics.Assert(IsRotatatingFeasible(ActionDiagramPresenter, mouseState, _selectedShapeAtCursorInfo));
					newAction = CurrentAction;
					// Find unselected shape under the mouse cursor
					newAction = DetermineMouseMoveAction(ActionDiagramPresenter, mouseState, _selectedShapeAtCursorInfo);

					// If the action has changed, prepare and start the new action
					if (newAction != CurrentAction) {
						switch (newAction) {
							// Rotate shape -> Prepare shape rotation
							case Action.PrepareRotate:
								DebugDiagnostics.Assert(CurrentAction == Action.Rotate);
								EndToolAction();
								ClearPreviews();
								break;

							// Prepare shape rotation -> Rotate shape
							case Action.Rotate:
								DebugDiagnostics.Assert(CurrentAction == Action.PrepareRotate);
								StartToolAction(ActionDiagramPresenter, (int)newAction, mouseState, false);
								CreatePreviewShapes(ActionDiagramPresenter);
								break;

							case Action.SelectWithFrame:
							case Action.MoveShape:
							case Action.EditCaption:
							case Action.MoveHandle:
							case Action.None:
							case Action.Select:
								Debug.Fail("Unhandled change of CurrentAction.");
								break;
							default:
								Debug.Fail(string.Format("Unexpected {0} value: {1}", CurrentAction.GetType().Name, CurrentAction));
								break;
						}
						//currentToolAction = newAction;
					}

					RotateShapesPreview(ActionDiagramPresenter, mouseState);
					break;

				default: throw new NShapeUnsupportedValueException(typeof(Action), CurrentAction);
			}

			int cursorId = DetermineCursor(diagramPresenter, mouseState);
			if (CurrentAction == Action.None) diagramPresenter.SetCursor(cursorId);
			else ActionDiagramPresenter.SetCursor(cursorId);

			return result;
		}


		/// <summary>
		/// Process mouse up event
		/// </summary>
		private bool ProcessMouseUp(IDiagramPresenter diagramPresenter, MouseState mouseState) {
			if (diagramPresenter == null) throw new ArgumentNullException(nameof(diagramPresenter)); 
			bool result = false;
			if (!_selectedShapeAtCursorInfo.IsEmpty &&
				!diagramPresenter.SelectedShapes.Contains(_selectedShapeAtCursorInfo.Shape))
				_selectedShapeAtCursorInfo.Clear();

			switch (CurrentAction) {
				case Action.None:
					// Handle double click
					if (mouseState.Clicks >= 2) {
						// Double click detected -> Cancel deferred single click action
						CancelDeferredClickAction();

						// If 'QuickRotate' is enabled, perform it now.
						if (diagramPresenter.Project.SecurityManager.IsGranted(Permission.Layout, diagramPresenter.SelectedShapes) && _enableQuickRotate) {
							if (!_selectedShapeAtCursorInfo.IsEmpty && _selectedShapeAtCursorInfo.IsCursorAtGrip
								&& _selectedShapeAtCursorInfo.Shape.HasControlPointCapability(_selectedShapeAtCursorInfo.ControlPointId, ControlPointCapabilities.Rotate)) {
								int angle = 900 * (mouseState.Clicks - 1);
								if (angle % 3600 != 0) {
									PerformQuickRotate(diagramPresenter, angle);
									result = true;
									OnToolExecuted(ExecutedEventArgs);
								}
							}
						}
					}
					break;

				case Action.Select:
					// Perform selection
					ShapeAtCursorInfo shapeAtCursorInfo = ShapeAtCursorInfo.Empty;
					// Check if a selected shape was clicked (needed for selecting parent/children of aggregated shapes)
					if (!_selectedShapeAtCursorInfo.IsEmpty && _selectedShapeAtCursorInfo.Shape.ContainsPoint(mouseState.X, mouseState.Y)) {
						// Find the shape to select
						Shape shape = ActionDiagramPresenter.Diagram.Shapes.FindShape(mouseState.X, mouseState.Y, ControlPointCapabilities.None, 0, _selectedShapeAtCursorInfo.Shape);
						if (shape != null) {
							shapeAtCursorInfo.Shape = shape;
							shapeAtCursorInfo.ControlPointId = shape.HitTest(mouseState.X, mouseState.Y, ControlPointCapabilities.All, 0);
							shapeAtCursorInfo.CaptionIndex = -1;
						}
					} else {
						// No shape was selected -> try to find a shape at the current mouse position
						shapeAtCursorInfo = ShapeAtCursorInfo.Create(FindNearestTarget(diagramPresenter, mouseState.X, mouseState.Y, ControlPointCapabilities.None, 0, null));
					}
					
					if (OverlappingShapesAction == OverlappingShapesAction.None || _selectedShapeAtCursorInfo.IsEmpty) {
						// Perform standard selection
						result = PerformSelection(ActionDiagramPresenter, mouseState, shapeAtCursorInfo);
						SetSelectedShapeAtCursor(ActionDiagramPresenter, mouseState.X, mouseState.Y, ActionDiagramPresenter.ZoomedGripSize, ControlPointCapabilities.All);
					} else {
						// Perform deferred shape selection (including overlapping shapes selection and aggregated shape sub-selection)
						// only if the mouse up event does not belong to a double/multi click.
						if (mouseState.Clicks <= 1) {
							// We use a lambda expression here because we need the IDiagramPresenter, the mouse state and the shapeAtCursorInfo.
							ElapsedEventHandler elapsedEventHandler = (o, e) => {
								CancelDeferredClickAction();
								PerformSelection(diagramPresenter, mouseState, shapeAtCursorInfo);
								SetSelectedShapeAtCursor(diagramPresenter, mouseState.X, mouseState.Y, diagramPresenter.ZoomedGripSize, ControlPointCapabilities.All);
							};
							StartDeferredClickAction(diagramPresenter, elapsedEventHandler);
							result = true;
						}
					}
					while (IsToolActionPending)
						EndToolAction();
					break;

				case Action.SelectWithFrame:
					// select all selectedShapes within the frame
					result = PerformFrameSelection(ActionDiagramPresenter, mouseState, _frameRect);
					while (IsToolActionPending)
						EndToolAction();
					break;

				case Action.EditCaption:
					// if the user clicked a caption, display the caption editor
					DebugDiagnostics.Assert(_selectedShapeAtCursorInfo.IsCursorAtCaption);
					// Update the selected shape at cursor
					SetSelectedShapeAtCursor(ActionDiagramPresenter, mouseState.X, mouseState.Y, ActionDiagramPresenter.ZoomedGripSize, ControlPointCapabilities.All);
					if (!_selectedShapeAtCursorInfo.IsEmpty && _selectedShapeAtCursorInfo.IsCursorAtCaption)
						ActionDiagramPresenter.OpenCaptionEditor((ICaptionedShape)_selectedShapeAtCursorInfo.Shape, _selectedShapeAtCursorInfo.CaptionIndex);
					EndToolAction();
					result = true;
					break;

				case Action.MoveHandle:
					//DebugDiagnostics.Assert(!_selectedShapeAtCursorInfo.IsEmpty);
					result = MoveHandle(ActionDiagramPresenter);
					while (IsToolActionPending)
						EndToolAction();
					break;

				case Action.MoveShape:
					//DebugDiagnostics.Assert(!_selectedShapeAtCursorInfo.IsEmpty);
					result = MoveShapes(ActionDiagramPresenter);
					while (IsToolActionPending)
						EndToolAction();
					break;

				case Action.PrepareRotate:
				case Action.Rotate:
					//DebugDiagnostics.Assert(!_selectedShapeAtCursorInfo.IsEmpty);
					if (CurrentAction == Action.Rotate)
						result = RotateShapes(ActionDiagramPresenter, mouseState);
					while (IsToolActionPending)
						EndToolAction();
					break;

				default: throw new NShapeUnsupportedValueException(CurrentAction);
			}

			SetSelectedShapeAtCursor(diagramPresenter, mouseState.X, mouseState.Y, diagramPresenter.ZoomedGripSize, ControlPointCapabilities.All);
			diagramPresenter.SetCursor(DetermineCursor(diagramPresenter, mouseState));

			if (!_deferredClickTimer.Enabled)
				OnToolExecuted(ExecutedEventArgs);
			return result;
		}

		#endregion


		#region [Protected] SelectionTool Methods


		// @@ Tool-Interface: New
		/// <summary>
		/// Returns true if the given shape (and control point) are regarded as a valid target for the current action.
		/// </summary>
		/// <param name="diagramPresenter">The current diagram presenter.</param>
		/// <returns></returns>
		protected virtual TargetFilterDelegate CreateIsConnectionTargetFilter(IDiagramPresenter diagramPresenter) {
			return (shape, cpId) => { return IsConnectionTarget(diagramPresenter, shape, cpId); };
		}


		private bool IsConnectionTarget(IDiagramPresenter diagramPresenter, Shape shape, ControlPointId ctrlPointId) {
			if (shape == SelectedShapeAtCursor)
				return false;
			// Skip invisible shapes
			if (ActionDiagramPresenter != null && !diagramPresenter.IsLayerVisible(shape.HomeLayer, shape.SupplementalLayers))
				return false;
			// We have to check connection abilties using the preview shape because the original shape may be still connected
			// while moving the mouse and therefore its CanConnect method would return 'false'.
			Previews.TryGetValue(SelectedShapeAtCursor, out Shape selectedShapePreview);
			if (selectedShapePreview != null && SelectedControlPointIdAtCursor != ControlPointId.None
				&& selectedShapePreview.HasControlPointCapability(SelectedControlPointIdAtCursor, ControlPointCapabilities.Glue)) {
				return shape.HasControlPointCapability(ctrlPointId, ConnectionCapabilities)
					&& selectedShapePreview.CanConnect(SelectedControlPointIdAtCursor, shape, ctrlPointId);
			} else
				return false;
		}


		// @@ Tool-Interface: New
		/// <summary>
		/// Gets the range of highlighting potential connection targets (typically: connection points).
		/// </summary>
		protected virtual int HighlightTargetsRange {
			get { return DefaultHighlightTargetsRange; }
		}


		// @@ Tool-Interface: New
		/// <summary>
		/// Gets the range of finding connection targets (typically: connection points and shapes).
		/// Defaults to the (zoomed) handle size, adjustable in Display/IDiagramPresenter.
		/// </summary>
		protected virtual int GetFindTargetRange(IDiagramPresenter diagramPresenter) {
			if (diagramPresenter == null) throw new ArgumentNullException(nameof(diagramPresenter));
			return diagramPresenter.ZoomedGripSize;
		}


		// @@ Tool-Interface: New
		/// <summary>
		/// Gets the range of finding grid lines.
		/// Defaults to the snap distance, adjustable in Display/IDiagramPresenter.
		/// </summary>
		protected virtual int GetFindGridLineRange(IDiagramPresenter diagramPresenter) {
			if (diagramPresenter == null) throw new ArgumentNullException(nameof(diagramPresenter));
			return diagramPresenter.SnapDistance;
		}


		// @@ Tool-Interface: New
		/// <summary>
		/// Called after the preview shapes were created.
		/// </summary>
		/// <param name="previewShapes">The dictionary of preview shapes: The key is the original shape, the value is the preview shape.</param>
		protected virtual void OnPreviewShapesCreated(IDictionary<Shape, Shape> previewShapes) {
			// Nothing to do
		}


		// @@ Tool-Interface: New
		/// <summary>
		/// Called when the preview shape(s) should be moved.
		/// Override for implementing custom tool logic.
		/// </summary>
		/// <param name="diagramPresenter">The current diagram presenter.</param>
		/// <param name="mouseState">The current mouse state.</param>
		/// <param name="shapeAtCursor">The (original and unmodified) shape that was under the cursor when the tool action started.</param>
		/// <param name="controlPointAtCursor">The control point (of the shapeAtCursor) that was under the cursor when the tool action started.</param>
		/// <returns>True if the default implementation should be executed, false if all work is done.</returns>
		protected virtual bool OnPreviewShapesMoving(IDiagramPresenter diagramPresenter, MouseState mouseState, Shape shapeAtCursor, ControlPointId controlPointAtCursor) {
			return true;
		}


		// @@ Tool-Interface: New
		/// <summary>
		/// Called when the original shape(s) should be moved.
		/// Override for implementing custom tool logic.
		/// </summary>
		/// <param name="diagramPresenter">The current diagram presenter.</param>
		/// <param name="mouseState">The current mouse state.</param>
		/// <param name="shapeAtCursor">The (original and unmodified) shape that was under the cursor when the tool action started.</param>
		/// <param name="controlPointAtCursor">The control point (of the shapeAtCursor) that was under the cursor when the tool action started.</param>
		/// <returns>True if the default implementation should be executed, false if all work is done.</returns>
		protected virtual bool OnShapesMoved(IDiagramPresenter diagramPresenter, MouseState mouseState, Shape shapeAtCursor, ControlPointId controlPointAtCursor) {
			return true;
		}


		// @@ Tool-Interface: New
		/// <summary>
		/// Called when the control point(s) of the preview shape(s) should be moved.
		/// Override for implementing custom tool logic.
		/// </summary>
		/// <param name="diagramPresenter">The current diagram presenter.</param>
		/// <param name="mouseState">The current mouse state.</param>
		/// <param name="shapeAtCursor">The (original and unmodified) shape that was under the cursor when the tool action started.</param>
		/// <param name="controlPointAtCursor">The control point (of the shapeAtCursor) that was under the cursor when the tool action started.</param>
		/// <returns>True if the default implementation should be executed, false if all work is done.</returns>
		protected virtual bool OnPreviewHandleMoving(IDiagramPresenter diagramPresenter, MouseState mouseState, Shape shapeAtCursor, ControlPointId controlPointAtCursor) {
			return true;
		}


		// @@ Tool-Interface: New
		/// <summary>
		/// Called when the control point(s) of the original shape(s) should be moved.
		/// Override for implementing custom tool logic.
		/// </summary>
		/// <param name="diagramPresenter">The current diagram presenter.</param>
		/// <param name="mouseState">The current mouse state.</param>
		/// <param name="shapeAtCursor">The (original and unmodified) shape that was under the cursor when the tool action started.</param>
		/// <param name="controlPointAtCursor">The control point (of the shapeAtCursor) that was under the cursor when the tool action started.</param>
		/// <returns>True if the default implementation should be executed, false if all work is done.</returns>
		protected virtual bool OnHandleMoved(IDiagramPresenter diagramPresenter, MouseState mouseState, Shape shapeAtCursor, ControlPointId controlPointAtCursor) {
			return true;
		}


		// @@ Tool-Interface: New
		/// <summary>
		/// Called when one or more shapes should be rotated.
		/// Override for implementing custom tool logic.
		/// </summary>
		/// <param name="diagramPresenter">The current diagram presenter.</param>
		/// <param name="mouseState">The current mouse state.</param>
		/// <param name="shapes">The (original and unmodified) shapes that should be rotated.</param>
		/// <param name="sweepAngle">The angle in tenths of degrees.</param>
		/// <returns>True if the default implementation should be executed, false if all work is done.</returns>
		protected virtual bool OnPreviewShapesRotating(IDiagramPresenter diagramPresenter, MouseState mouseState, IEnumerable<Shape> shapes, int sweepAngle) {
			return true;
		}


		// @@ Tool-Interface: New
		/// <summary>
		/// Called when one or more shapes should be rotated.
		/// Override for implementing custom tool logic.
		/// </summary>
		/// <param name="diagramPresenter">The current diagram presenter.</param>
		/// <param name="mouseState">The current mouse state.</param>
		/// <param name="shapes">The (original and unmodified) shapes that should be rotated.</param>
		/// <param name="sweepAngle">The angle in tenths of degrees.</param>
		/// <returns>True if the default implementation should be executed, false if all work is done.</returns>
		protected virtual bool OnShapesRotated(IDiagramPresenter diagramPresenter, MouseState mouseState, IEnumerable<Shape> shapes, int sweepAngle) {
			return true;
		}
		#endregion


		#region [Protected] Helper Methods

		/// <summary>
		/// Create Previews of all shapes selected in the CurrentDisplay.
		/// These previews are connected to all the shapes the original shapes are connected to.
		/// </summary>
		protected void CreatePreviewShapes(IDiagramPresenter diagramPresenter) {
			if (diagramPresenter == null) throw new ArgumentNullException(nameof(diagramPresenter));
			if (Previews.Count == 0 && diagramPresenter.SelectedShapes.Count > 0) {
				// first, clone all selected shapes...
				foreach (Shape shape in diagramPresenter.SelectedShapes) {
					if (!diagramPresenter.IsLayerVisible(shape.HomeLayer, shape.SupplementalLayers)) continue;
					AddPreview(shape, shape.Type.CreatePreviewInstance(shape), diagramPresenter.DisplayService);
				}
				// ...then restore connections between previews and connections between previews and non-selected shapes
				_targetShapeBuffer.Clear();
				foreach (Shape selectedShape in diagramPresenter.SelectedShapes.BottomUp) {
					if (!diagramPresenter.IsLayerVisible(selectedShape.HomeLayer, selectedShape.SupplementalLayers)) continue;
					// AttachGluePointToConnectionPoint the preview shape (and all it's cildren) to all the shapes the original shape was connected to
					// Additionally, create previews for all connected shapes and connect these to the appropriate target shapes
					ConnectPreviewOfShape(diagramPresenter, selectedShape);
				}
				_targetShapeBuffer.Clear();

				OnPreviewShapesCreated(Previews);
			}
		}


		/// <summary>Resets all preview shapes to their original states.</summary>
		protected void ResetPreviewShapes(IDiagramPresenter diagramPresenter) {
			// Dictionary "originalShapes" contains the previewShape as "Key" and the original shape as "Value"
			foreach (KeyValuePair<Shape, Shape> pair in _originalShapes) {
				// Copy all properties of the original shape to the preview shape 
				// (assigns the original model object to the preview shape!)
				IModelObject modelObj = pair.Value.ModelObject;
				pair.Key.CopyFrom(pair.Value);
				// If the original shape has a model object, clone and assign it.
				// Then, transform the copied shape into a preview shape
				pair.Key.ModelObject = null;
				pair.Key.MakePreview(diagramPresenter.Project.Design);
				if (modelObj != null) pair.Key.ModelObject = modelObj.Clone();
			}
		}

		#endregion


		#region [Private] Properties

		private Action CurrentAction {
			get {
				if (IsToolActionPending)
					return (Action)CurrentToolAction.Action;
				else return Action.None;
			}
		}

		#endregion


		#region [Private] Determine action depending on mouse state and event type

		/// <summary>
		/// Decide which tool action is suitable for the current mouse state.
		/// </summary>
		private Action DetermineMouseDownAction(IDiagramPresenter diagramPresenter, MouseState mouseState) {
			if (diagramPresenter == null) throw new ArgumentNullException(nameof(diagramPresenter));
			if (mouseState.IsButtonDown(MouseButtonsDg.Left)) {
				if (!_selectedShapeAtCursorInfo.IsEmpty
					&& IsEditCaptionFeasible(diagramPresenter, mouseState, _selectedShapeAtCursorInfo)) {
					// If the cursor is not over a caption of a selected shape when clicking left mouse button, 
					// we assume the user wants to select something
					// Same thing if no other action is granted.
					return Action.EditCaption;
				} else {
					// Moving shapes and handles is initiated as soon as the user starts drag action (move mouse 
					// while mouse button is pressed) 
					// If the user does not start a drag action, this will result in (un)selecting shapes.
					return Action.Select;
				}
			} else if (mouseState.IsButtonDown(MouseButtonsDg.Right)) {
				// Abort current action when clicking right mouse button
				return Action.None;
			} else {
				// Ignore other pressed mouse buttons
				return CurrentAction;
			}
		}


		/// <summary>
		/// Decide which tool action is suitable for the current mouse state.
		/// </summary>
		private Action DetermineMouseMoveAction(IDiagramPresenter diagramPresenter, MouseState mouseState, ShapeAtCursorInfo shapeAtCursorInfo) {
			if (diagramPresenter == null) throw new ArgumentNullException(nameof(diagramPresenter)); 
			switch (CurrentAction) {
				case Action.None:
				case Action.MoveHandle:
				case Action.MoveShape:
				case Action.SelectWithFrame:
					// Do not change the current action
					return CurrentAction;

				case Action.Select:
				case Action.EditCaption:
					if (mouseState.IsButtonDown(MouseButtonsDg.Left)) {
						// Check if cursor is over a control point and moving grips or rotating is feasible
						if (_selectedShapeAtCursorInfo.IsCursorAtGrip) {
							if (IsMoveHandleFeasible(diagramPresenter, mouseState, _selectedShapeAtCursorInfo))
								return Action.MoveHandle;
							else if (IsRotatatingFeasible(diagramPresenter, mouseState, _selectedShapeAtCursorInfo))
								return Action.PrepareRotate;
							else return Action.SelectWithFrame;
						} else {
							// If there is no shape under the cursor, start a SelectWithFrame action,
							// otherwise start a MoveShape action
							bool canMove = false;
							if (!_selectedShapeAtCursorInfo.IsEmpty) {
								// If there are selected shapes, check these shapes...
								canMove = IsMoveShapeFeasible(diagramPresenter, mouseState, _selectedShapeAtCursorInfo);
							} else {
								// ... otherwise check the shape under the cursor as it will be selected 
								// before starting a move action
								canMove = IsMoveShapeFeasible(diagramPresenter, mouseState, shapeAtCursorInfo);
							}
							if (canMove)
								return Action.MoveShape;
							else return Action.SelectWithFrame;
						}
					}
					return CurrentAction;

				case Action.PrepareRotate:
					if (mouseState.IsButtonDown(MouseButtonsDg.Left)) {
						// If the mouse has left the min rotate range, start 'real' rotating
						if (IsMinRotateRangeExceeded(diagramPresenter, mouseState))
							return Action.Rotate;
						else return CurrentAction;
					} else return CurrentAction;

				case Action.Rotate:
					if (mouseState.IsButtonDown(MouseButtonsDg.Left)) {
						// If the mouse has entered the min rotate range, start showing rotating hint
						if (!IsMinRotateRangeExceeded(diagramPresenter, mouseState))
							return Action.PrepareRotate;
						else return CurrentAction;
					} else return CurrentAction;

				default: throw new NShapeUnsupportedValueException(CurrentAction);
			}
		}


		private bool IsConnectingFeasible(ShapeAtCursorInfo manipulatedShapeInfo, ShapeAtCursorInfo targetShapeInfo) {
			if (manipulatedShapeInfo.IsEmpty || targetShapeInfo.IsEmpty) return false;
			if (manipulatedShapeInfo.IsCursorAtGluePoint)
				//return manipulatedShapeInfo.CanConnect(targetShapeInfo);
				return CanConnect(manipulatedShapeInfo.Shape, manipulatedShapeInfo.ControlPointId, targetShapeInfo.Shape, targetShapeInfo.ControlPointId);
			return false;
		}

		#endregion


		#region [Private] Action implementations

		#region Selecting Shapes

		// ToDo: Refactor, add protected virtual 'On...' methods for customizability
		// (Un)Select shape unter the mouse pointer
		private bool PerformSelection(IDiagramPresenter diagramPresenter, MouseState mouseState, ShapeAtCursorInfo shapeAtCursorInfo) {
			if (diagramPresenter == null) throw new ArgumentNullException(nameof(diagramPresenter)); 
			bool result = false;
			bool multiSelect = mouseState.IsKeyPressed(KeysDg.Control) || mouseState.IsKeyPressed(KeysDg.Shift);

			// When selecting shapes conteolpoints should be ignored as the user does not see them 
			// until a shape is selected
			const ControlPointCapabilities capabilities = ControlPointCapabilities.None;
			const int range = 0;

			// Determine the shape that has to be selected:
			Shape shapeToSelect = null;
			if (!_selectedShapeAtCursorInfo.IsEmpty) {
				// When in multiSelection mode, unselect the selected shape under the cursor
				if (multiSelect) shapeToSelect = _selectedShapeAtCursorInfo.Shape;
				else {
					// First, check if the selected shape under the cursor has children that can be selected
					shapeToSelect = _selectedShapeAtCursorInfo.Shape.Children.FindShape(mouseState.X, mouseState.Y, capabilities, range, null);
					// Second, check if the selected shape under the cursor has siblings that can be selected
					if (shapeToSelect == null && _selectedShapeAtCursorInfo.Shape.Parent != null) {
						shapeToSelect = _selectedShapeAtCursorInfo.Shape.Parent.Children.FindShape(mouseState.X, mouseState.Y, capabilities, range, _selectedShapeAtCursorInfo.Shape);
						// Discard found shape if it is the selected shape at cursor
						if (shapeToSelect == _selectedShapeAtCursorInfo.Shape) shapeToSelect = null;
						if (shapeToSelect == null) {
							foreach (Shape shape in _selectedShapeAtCursorInfo.Shape.Parent.Children.FindShapes(mouseState.X, mouseState.Y, capabilities, range)) {
								if (shape == _selectedShapeAtCursorInfo.Shape) continue;
								// Ignore layer visibility for child shapes
								shapeToSelect = shape;
								break;
							}
						}
					}
					// Third, check if there are non-selected shapes below the selected shape under the cursor
					// and select the spape according to the overlapping shapes selection mode
					switch (_overlappingShapesSelectionMode) {
						case OverlappingShapesAction.None:
							// Keep the selected shape
							shapeToSelect = _selectedShapeAtCursorInfo.Shape;
							break;
						case OverlappingShapesAction.Topmost:
							// Select the topmost shape 
							shapeToSelect = diagramPresenter.Diagram.Shapes.FindShape(mouseState.X, mouseState.Y, capabilities, range, null);
							break;
						case OverlappingShapesAction.Cycle:
							// Try to find the the next underlying shape by passing the selected shape as 'startShape' to FindShape(...)
							Shape startShape = _selectedShapeAtCursorInfo.Shape;
							while (startShape.Parent != null) startShape = startShape.Parent;	// get the selected shape's top-level parent
							if (shapeToSelect == null && diagramPresenter.Diagram.Shapes.Contains(startShape))
								shapeToSelect = diagramPresenter.Diagram.Shapes.FindShape(mouseState.X, mouseState.Y, capabilities, range, startShape);
							break;
						default:
							throw new NShapeUnsupportedValueException(typeof(OverlappingShapesAction), _overlappingShapesSelectionMode);
					}
				}
			}

			// If there was a shape to select related to the selected shape under the cursor
			// (a child or a sibling of the selected shape or a shape below it),
			// try to select the first non-selected shape under the cursor
			if (shapeToSelect == null && shapeAtCursorInfo.Shape != null
				&& shapeAtCursorInfo.Shape.ContainsPoint(mouseState.X, mouseState.Y))
				shapeToSelect = shapeAtCursorInfo.Shape;

			// If a new shape to select was found, perform selection
			if (shapeToSelect != null) {
				// (check if multiselection mode is enabled (Shift + Click or Ctrl + Click))
				if (multiSelect) {
					// if multiSelect is enabled, add/remove to/from selected selectedShapes...
					if (diagramPresenter.SelectedShapes.Contains(shapeToSelect)) {
						// if object is selected -> remove from selection
						diagramPresenter.UnselectShape(shapeToSelect);
						RemovePreviewOf(shapeToSelect);
						result = true;
					} else {
						// If object is not selected -> add to selection
						diagramPresenter.SelectShape(shapeToSelect, true);
						result = true;
					}
				} else {
					// ... otherwise deselect all selectedShapes but the clicked object
					ClearPreviews();
					// check if the clicked shape is a child of an already selected shape
					Shape childShape = null;
					if (diagramPresenter.SelectedShapes.Count == 1
						&& diagramPresenter.SelectedShapes.TopMost.Children != null
						&& diagramPresenter.SelectedShapes.TopMost.Children.Count > 0) {
						childShape = diagramPresenter.SelectedShapes.TopMost.Children.FindShape(mouseState.X, mouseState.Y, ControlPointCapabilities.None, 0, null);
					}
					if (childShape != null) diagramPresenter.SelectShape(childShape, false);
					else diagramPresenter.SelectShape(shapeToSelect, false);
					result = true;
				}

				// validate if the desired shape or its parent was selected
				if (shapeToSelect.Parent != null) {
					if (!diagramPresenter.SelectedShapes.Contains(shapeToSelect))
						if (diagramPresenter.SelectedShapes.Contains(shapeToSelect.Parent))
							shapeToSelect = shapeToSelect.Parent;
				}
			} else if (_selectedShapeAtCursorInfo.IsEmpty || _selectedShapeAtCursorInfo.Shape.HitTest(mouseState.X, mouseState.Y, ControlPointCapabilities.All, 0) == ControlPointId.None) {
				// if there was no other shape to select and none of the selected shapes is under the cursor,
				// clear selection
				if (!multiSelect) {
					if (diagramPresenter.SelectedShapes.Count > 0) {
						diagramPresenter.UnselectAll();
						ClearPreviews();
						result = true;
					}
				}
			} else {
				// if there was no other shape to select and a selected shape is under the cursor,
				// do nothing
			}
			return result;
		}


		// ToDo: Refactor, add protected virtual 'On...' methods for customizability
		/// <summary>
		/// Calculates and returns a new selection frame.
		/// </summary>
		private Rectangle PrepareFrameSelection(IDiagramPresenter diagramPresenter, MouseState mouseState) {
			if (diagramPresenter == null) throw new ArgumentNullException(nameof(diagramPresenter));

			Invalidate(diagramPresenter);
			//
			Rectangle result = Rectangle.Empty;
			result.X = Math.Min(ActionStartMouseState.X, mouseState.X);
			result.Y = Math.Min(ActionStartMouseState.Y, mouseState.Y);
			result.Width = Math.Max(ActionStartMouseState.X, mouseState.X) - result.X;
			result.Height = Math.Max(ActionStartMouseState.Y, mouseState.Y) - result.Y;
			//
			Invalidate(diagramPresenter);
			
			return result;
		}


		// ToDo: Refactor, add protected virtual 'On...' methods for customizability
		/// <summary>
		/// Select shapes inside the selection frame
		/// </summary>
		private bool PerformFrameSelection(IDiagramPresenter diagramPresenter, MouseState mouseState, Rectangle selectionFrame) {
			if (diagramPresenter == null) throw new ArgumentNullException(nameof(diagramPresenter));
			bool multiSelect = mouseState.IsKeyPressed(KeysDg.Control) || mouseState.IsKeyPressed(KeysDg.Shift);
			diagramPresenter.SelectShapes(selectionFrame, multiSelect);
			return true;
		}


		/// <summary>
		/// Starts a timer that executes the given event handler.
		/// Used for distinguishing double-clicks from subsequent clicks e.g. when selecting overlapping shapes.
		/// </summary>
		private void StartDeferredClickAction(IDiagramPresenter diagramPresenter, ElapsedEventHandler elapsedEventHandler) {
			// Ensure that existing Elapsed event handlers will be removed before adding a new one!
			CancelDeferredClickAction();
			Debug.Assert(_deferredClickHandler == null);
			_deferredClickHandler = elapsedEventHandler;

			// Start processing the click event 
			// (ensure timer elapses *after* the double click detection time!)
			if (_deferredClickTimer.Interval != DoubleClickTime + 1)
				_deferredClickTimer.Interval = DoubleClickTime + 1;
			if (_deferredClickTimer.SynchronizingObject != diagramPresenter)
				_deferredClickTimer.SynchronizingObject = diagramPresenter;
			_deferredClickTimer.Elapsed += _deferredClickHandler;
			_deferredClickTimer.AutoReset = false;
			_deferredClickTimer.Start();
		}


		/// <summary>
		/// Stops the timer for deferred actions.
		/// </summary>
		private void CancelDeferredClickAction() {
			_deferredClickTimer.Stop();
			if (_deferredClickHandler != null) {
				_deferredClickTimer.Elapsed -= _deferredClickHandler;
				_deferredClickHandler = null;
			}
		}

		#endregion


		#region Moving Shapes

		/// <summary>
		/// Prepare drawing preview of move action
		/// </summary>
		private void MoveShapesPreview(IDiagramPresenter diagramPresenter, MouseState mouseState) {
			DebugDiagnostics.Assert(diagramPresenter.SelectedShapes.Count > 0);
			DebugDiagnostics.Assert(!_selectedShapeAtCursorInfo.IsEmpty);

			// Reset all shapes to start values
			ResetPreviewShapes(diagramPresenter);

			if (OnPreviewShapesMoving(diagramPresenter, mouseState, SelectedShapeAtCursor, SelectedControlPointIdAtCursor)) {
				// Calculate the distance of the mouse movement
				int movedDistanceX = mouseState.X - ActionStartMouseState.X;
				int movedDistanceY = mouseState.Y - ActionStartMouseState.Y;
				// 
				_snapDistanceX = _snapDistanceY = 0;
				if (SelectedShapeAtCursor != null) {
					Point mouseOffset = CalculateMouseOffset(ActionStartMouseState.Position, SelectedShapeAtCursor, SelectedControlPointIdAtCursor);
					Point position = new Point(mouseState.X - mouseOffset.X, mouseState.Y - mouseOffset.Y);
					// Find target shape (and connection point)
					Locator target = FindNearestTarget(diagramPresenter, position.X, position.Y, ConnectionCapabilities,
							GetFindTargetRange(diagramPresenter), CreateIsConnectionTargetFilter(diagramPresenter));
					if (!target.IsEmpty) {
						Point snapDistance = CalculateDistanceToConnectionTarget(position, target.Shape, target.ControlPointId);
						_snapDistanceX = snapDistance.X;
						_snapDistanceY = snapDistance.Y;
					} else
						FindNearestSnapPoint(diagramPresenter, SelectedShapeAtCursor, SelectedControlPointIdAtCursor, movedDistanceX, movedDistanceY, out _snapDistanceX, out _snapDistanceY);
				}
				// Move (preview copies of) the selected shapes
				foreach (Shape originalShape in diagramPresenter.SelectedShapes) {
					// Get preview of the shape to move...
					Shape previewShape = FindPreviewOfShape(originalShape);
					// ...and move the preview shape to the new position
					previewShape.MoveTo(originalShape.X + movedDistanceX + _snapDistanceX, originalShape.Y + movedDistanceY + _snapDistanceY);
				}
			}
		}


		/// <summary>
		/// Applies the move action.
		/// </summary>
		private bool MoveShapes(IDiagramPresenter diagramPresenter) {
			bool result = false;
			DebugDiagnostics.Assert(!_selectedShapeAtCursorInfo.IsEmpty);
			if (ActionStartMouseState.Position != CurrentMouseState.Position) {
				if (OnShapesMoved(diagramPresenter, CurrentMouseState, _selectedShapeAtCursorInfo.Shape, _selectedShapeAtCursorInfo.ControlPointId)) {

					// calculate the movement
					int distanceX = CurrentMouseState.X - ActionStartMouseState.X;
					int distanceY = CurrentMouseState.Y - ActionStartMouseState.Y;

#if DEBUG_DIAGNOSTICS
					int dx = distanceX + _snapDistanceX;
					int dy = distanceY + _snapDistanceY;
					Debug.Assert(CurrentMouseState.Position == CurrentMouseState.Position);
					foreach (Shape selectedShape in diagramPresenter.SelectedShapes) {
						Shape previewShape = _previewShapes[selectedShape];
						Debug.Assert((previewShape.X == selectedShape.X && previewShape.Y == selectedShape.Y)
								|| (previewShape.X == selectedShape.X + dx && previewShape.Y == selectedShape.Y + dy),
								string.Format("Preview shape '{0}' was expected at position {1} but it is at position {2}.",
									previewShape.Type.Name, new Point(selectedShape.X, selectedShape.Y), new Point(previewShape.X, previewShape.Y)));
					}
#endif

					ICommand cmd = new MoveShapeByCommand(diagramPresenter.SelectedShapes, distanceX + _snapDistanceX, distanceY + _snapDistanceY);
					diagramPresenter.Project.ExecuteCommand(cmd);
				}
				result = true;
				_snapDistanceX = _snapDistanceY = 0;
				_snapPtId = ControlPointId.None;
			}
			return result;
		}

		#endregion


		#region Moving ControlPoints

		/// <summary>
		/// Prepare drawing preview of resize action 
		/// </summary>
		private void MoveHandlePreview(IDiagramPresenter diagramPresenter, MouseState mouseState) {
			if (diagramPresenter == null) throw new ArgumentNullException(nameof(diagramPresenter));

			// Create a filter delegate
			TargetFilterDelegate targetsFilter = CreateIsConnectionTargetFilter(diagramPresenter);

			// Invalidate previously highlighted connection points
			InvalidateConnectionTargets(diagramPresenter, CurrentMouseState.X, CurrentMouseState.Y, ConnectionCapabilities, HighlightTargetsRange, targetsFilter);
			// Reset all preview shapes to start values
			ResetPreviewShapes(diagramPresenter);

			if (OnPreviewHandleMoving(diagramPresenter, mouseState, _selectedShapeAtCursorInfo.Shape, _selectedShapeAtCursorInfo.ControlPointId)) {
				// Calculate the distance of the mouse movement
				int movedDistanceX = mouseState.X - ActionStartMouseState.X;
				int movedDistanceY = mouseState.Y - ActionStartMouseState.Y;
				Point mouseOffset = Point.Empty;
				// 
				_snapDistanceX = _snapDistanceY = 0;
				if (SelectedShapeAtCursor != null) {
					mouseOffset = CalculateMouseOffset(ActionStartMouseState.Position, SelectedShapeAtCursor, SelectedControlPointIdAtCursor);
					// Find target shape (and connection point)					
					_handleMoveTarget = FindNearestTarget(diagramPresenter, mouseState.X - mouseOffset.X, mouseState.Y - mouseOffset.Y, ControlPointCapabilities.All, 
							GetFindTargetRange(diagramPresenter), targetsFilter);
					if (!_handleMoveTarget.IsEmpty) {
						Point snapDistance = CalculateDistanceToConnectionTarget(mouseState.Position, _handleMoveTarget.Shape, _handleMoveTarget.ControlPointId);
						_snapDistanceX = snapDistance.X;
						_snapDistanceY = snapDistance.Y;
					} else
						FindNearestSnapPoint(diagramPresenter, SelectedShapeAtCursor, SelectedControlPointIdAtCursor, movedDistanceX, movedDistanceY, out _snapDistanceX, out _snapDistanceY);
				}
				//
				// Move selected shapes
				ResizeModifiers resizeModifier = GetResizeModifier(mouseState);
				Point originalPtPos = Point.Empty;
				foreach (Shape selectedShape in diagramPresenter.SelectedShapes) {
					Shape previewShape = FindPreviewOfShape(selectedShape);
					// Perform movement
					if (previewShape.HasControlPointCapability(_selectedShapeAtCursorInfo.ControlPointId, ControlPointCapabilities.Resize))
						previewShape.MoveControlPointBy(_selectedShapeAtCursorInfo.ControlPointId, movedDistanceX + _snapDistanceX, movedDistanceY + _snapDistanceY, resizeModifier);
				}

			}
			InvalidateConnectionTargets(diagramPresenter, mouseState.X, mouseState.Y, ConnectionCapabilities, HighlightTargetsRange, targetsFilter);
		}


		/// <summary>
		/// Apply the resize action
		/// </summary>
		private bool MoveHandle(IDiagramPresenter diagramPresenter) {
			bool result = false;
			//Invalidate(diagramPresenter);
			if (ActionStartMouseState.Position != CurrentMouseState.Position) {
				if (OnHandleMoved(diagramPresenter, CurrentMouseState, _selectedShapeAtCursorInfo.Shape, _selectedShapeAtCursorInfo.ControlPointId)) {

					int distanceX = CurrentMouseState.X - ActionStartMouseState.X;
					int distanceY = CurrentMouseState.Y - ActionStartMouseState.Y;

					// if the moved ControlPoint is a single GluePoint, snap to ConnectionPoints
					bool isGluePoint = false;
					if (diagramPresenter.SelectedShapes.Count == 1)
						isGluePoint = _selectedShapeAtCursorInfo.IsCursorAtGluePoint;

					ResizeModifiers resizeModifier = GetResizeModifier(CurrentMouseState);
					if (isGluePoint) {
						ICommand cmd = new MoveGluePointCommand(_selectedShapeAtCursorInfo.Shape, _selectedShapeAtCursorInfo.ControlPointId, _handleMoveTarget.Shape, _handleMoveTarget.ControlPointId, distanceX + _snapDistanceX, distanceY + _snapDistanceY, resizeModifier);
						diagramPresenter.Project.ExecuteCommand(cmd);
					} else {
						ICommand cmd = new MoveControlPointCommand(ActionDiagramPresenter.SelectedShapes, _selectedShapeAtCursorInfo.ControlPointId, distanceX + _snapDistanceX, distanceY + _snapDistanceY, resizeModifier);
						diagramPresenter.Project.ExecuteCommand(cmd);
					}
				}
				result = true;
			}
			_snapDistanceX = _snapDistanceY = 0;
			_snapPtId = ControlPointId.None;

			return result;
		}

		#endregion


		#region Rotating Shapes

		// ToDo: Refactor, add protected virtual 'On...' methods for customizability
		/// <summary>
		/// Prepare drawing preview of rotate action
		/// </summary>
		private void RotateShapesPreview(IDiagramPresenter diagramPresenter, MouseState mouseState) {
			InvalidateAnglePreview(ActionDiagramPresenter);
			if (PendingToolActionsCount >= 1
				&& ActionStartMouseState.Position != mouseState.Position) {
				if (IsMinRotateRangeExceeded(diagramPresenter, mouseState)) {
					// calculate new angle
					MouseState initMouseState = GetMouseStateOfPreviousAction();
					int startAngle, sweepAngle, prevSweepAngle;
					CalcAngle(initMouseState, ActionStartMouseState, CurrentMouseState, mouseState,
						out startAngle, out sweepAngle, out prevSweepAngle);

					// Reset all preview shapes to start values
					ResetPreviewShapes(diagramPresenter);

					if (OnPreviewShapesRotating(diagramPresenter, mouseState, diagramPresenter.SelectedShapes, sweepAngle)) {
						// ToDo: Implement rotation around a common rotation center
						foreach (Shape selectedShape in diagramPresenter.SelectedShapes) {
							Shape previewShape = FindPreviewOfShape(selectedShape);
							// Get ControlPointId of the first rotate control point
							ControlPointId rotatePtId = ControlPointId.None;
							foreach (ControlPointId id in previewShape.GetControlPointIds(ControlPointCapabilities.Rotate)) {
								rotatePtId = id;
								break;
							}
							if (rotatePtId == ControlPointId.None) throw new NShapeInternalException("{0} has no rotate control point.");
							Point rotationCenter = previewShape.GetControlPointPosition(rotatePtId);

							//// Restore original shape's angle
							//previewShape.Rotate(-prevSweepAngle, rotationCenter.X, rotationCenter.Y);

							// Perform rotation
							previewShape.Rotate(sweepAngle, rotationCenter.X, rotationCenter.Y);
						}
					}
				}
			}
			InvalidateAnglePreview(ActionDiagramPresenter);
		}


		/// <summary>
		/// Apply rotate action.
		/// </summary>
		private bool RotateShapes(IDiagramPresenter diagramPresenter, MouseState mouseState) {
			bool result = false;
			if (PendingToolActionsCount >= 1
				&& ActionStartMouseState.Position != CurrentMouseState.Position
				&& IsMinRotateRangeExceeded(ActionDiagramPresenter, CurrentMouseState)) {
				// Calculate rotation
				MouseState initMouseState = GetMouseStateOfPreviousAction();
				int startAngle, sweepAngle;
				CalcAngle(initMouseState, ActionStartMouseState, CurrentMouseState, out startAngle, out sweepAngle);

				if (OnShapesRotated(diagramPresenter, mouseState, diagramPresenter.SelectedShapes, sweepAngle)) {
					// Create and execute command
					ICommand cmd = new RotateShapesCommand(diagramPresenter.SelectedShapes, sweepAngle);
					diagramPresenter.Project.ExecuteCommand(cmd);
				}
				result = true;
			}
			return result;
		}


		private int CalcStartAngle(MouseState startMouseState, MouseState currentMouseState) {
			DebugDiagnostics.Assert(startMouseState != MouseState.Empty);
			DebugDiagnostics.Assert(currentMouseState != MouseState.Empty);
			float angleRad = Geometry.Angle(startMouseState.Position, currentMouseState.Position);
			return (3600 + Geometry.RadiansToTenthsOfDegree(angleRad)) % 3600;
		}


		private int CalcSweepAngle(MouseState initMouseState, MouseState prevMouseState, MouseState newMouseState) {
			DebugDiagnostics.Assert(initMouseState != MouseState.Empty);
			DebugDiagnostics.Assert(prevMouseState != MouseState.Empty);
			DebugDiagnostics.Assert(newMouseState != MouseState.Empty);
			float angleRad = Geometry.Angle(initMouseState.Position, prevMouseState.Position, newMouseState.Position);
			return (3600 + Geometry.RadiansToTenthsOfDegree(angleRad)) % 3600;
		}


		private int AlignAngle(int angle, MouseState mouseState) {
			int result = angle;
			if (mouseState.IsKeyPressed(KeysDg.Control) && mouseState.IsKeyPressed(KeysDg.Shift)) {
				// rotate by tenths of degrees
				// do nothing 
			} else if (mouseState.IsKeyPressed(KeysDg.Control)) {
				// rotate by full degrees
				result -= (result % 10);
			} else if (mouseState.IsKeyPressed(KeysDg.Shift)) {
				// rotate by 5 degrees
				result -= (result % 50);
			} else {
				// default:
				// rotate by 15 degrees
				result -= (result % 150);
			}
			return result;
		}


		private void CalcAngle(MouseState initMouseState, MouseState startMouseState, MouseState newMouseState, out int startAngle, out int sweepAngle) {
			startAngle = CalcStartAngle(initMouseState, ActionStartMouseState);
			int rawSweepAngle = CalcSweepAngle(initMouseState, ActionStartMouseState, newMouseState);
			sweepAngle = AlignAngle(rawSweepAngle, newMouseState);
		}


		private void CalcAngle(MouseState initMouseState, MouseState startMouseState, MouseState currentMouseState, MouseState newMouseState, out int startAngle, out int sweepAngle, out int prevSweepAngle) {
			CalcAngle(initMouseState, startMouseState, newMouseState, out startAngle, out sweepAngle);
			int rawPrevSweepAngle = CalcSweepAngle(initMouseState, ActionStartMouseState, currentMouseState);
			prevSweepAngle = AlignAngle(rawPrevSweepAngle, currentMouseState);
		}


		private bool IsMinRotateRangeExceeded(IDiagramPresenter diagramPresenter, MouseState mouseState) {
			if (diagramPresenter == null) throw new ArgumentNullException(nameof(diagramPresenter));
			if (mouseState == MouseState.Empty) throw new ArgumentException(nameof(mouseState));
			Point p;
			if (PendingToolActionsCount <= 1) p = ActionStartMouseState.Position;
			else {
				MouseState prevMouseState = GetMouseStateOfPreviousAction();
				p = prevMouseState.Position;
			}
			Debug.Assert(Geometry.IsValid(p));
			int dist = (int)Math.Round(Geometry.DistancePointPoint(p.X, p.Y, mouseState.X, mouseState.Y));
			diagramPresenter.DiagramToControl(dist, out dist);
			return (dist > diagramPresenter.MinRotateRange);
		}


		private MouseState GetMouseStateOfPreviousAction() {
			MouseState result = MouseState.Empty;
			bool firstItem = true;
			foreach (ActionDef actionDef in PendingToolActions) {
				if (!firstItem) {
					result = actionDef.MouseState;
					break;
				} else firstItem = false;
			}
			return result;
		}


		/// <summary>
		/// Specifies if a double click on the rotation handle will rotate the shape by 90°
		/// </summary>
		public bool EnableQuickRotate {
			get { return _enableQuickRotate; }
			set { _enableQuickRotate = value; }
		}


		private bool PerformQuickRotate(IDiagramPresenter diagramPresenter, int angle) {
			bool result = false;
			if (_enableQuickRotate) {
				ICommand cmd = new RotateShapesCommand(diagramPresenter.SelectedShapes, angle);
				diagramPresenter.Project.ExecuteCommand(cmd);
				InvalidateAnglePreview(diagramPresenter);
				result = true;
			}
			return result;
		}


		private void InvalidateAnglePreview(IDiagramPresenter diagramPresenter) {
			// invalidate previous angle preview
			diagramPresenter.InvalidateDiagram(
				_rectBuffer.X - _rectBuffer.Width - diagramPresenter.GripSize,
				_rectBuffer.Y - _rectBuffer.Height - diagramPresenter.GripSize,
				_rectBuffer.Width + _rectBuffer.Width + (2 * diagramPresenter.GripSize),
				_rectBuffer.Height + _rectBuffer.Height + (2 * diagramPresenter.GripSize));

			int requiredDistance;
			diagramPresenter.ControlToDiagram(diagramPresenter.MinRotateRange, out requiredDistance);
			int length = (int)Math.Round(Geometry.DistancePointPoint(ActionStartMouseState.X, ActionStartMouseState.Y, CurrentMouseState.X, CurrentMouseState.Y));

			// invalidate current angle preview / instruction preview
			_rectBuffer.Location = ActionStartMouseState.Position;
			if (length > requiredDistance)
				_rectBuffer.Width = _rectBuffer.Height = length;
			else
				_rectBuffer.Width = _rectBuffer.Height = requiredDistance;
			diagramPresenter.InvalidateDiagram(_rectBuffer.X - _rectBuffer.Width, _rectBuffer.Y - _rectBuffer.Height, _rectBuffer.Width + _rectBuffer.Width, _rectBuffer.Height + _rectBuffer.Height);
		}

		#endregion

		#endregion


		#region [Private] Preview management implementation

		/// <summary>
		/// The dictionary of preview shapes: The key is the original shape, the value is the preview shape.
		/// </summary>
		private IDictionary<Shape, Shape> Previews {
			get { return _previewShapes; }
		}


		private void AddPreview(Shape shape, Shape previewShape, IDisplayService displayService) {
			if (_originalShapes.ContainsKey(previewShape)) return;
			if (_previewShapes.ContainsKey(shape)) return;
			//// Set DisplayService for the preview shape
			//if (previewShape.DisplayService != displayService)
			//    previewShape.DisplayService = displayService;

			// Add shape and its preview to the appropriate dictionaries
			_previewShapes.Add(shape, previewShape);
			_originalShapes.Add(previewShape, shape);

			// Add shape's children and their previews to the appropriate dictionaries
			if (previewShape.Children.Count > 0) {
				IEnumerator<Shape> previewChildren = previewShape.Children.TopDown.GetEnumerator();
				IEnumerator<Shape> originalChildren = shape.Children.TopDown.GetEnumerator();

				previewChildren.Reset();
				originalChildren.Reset();
				bool processNext = false;
				if (previewChildren.MoveNext() && originalChildren.MoveNext())
					processNext = true;
				while (processNext) {
					AddPreview(originalChildren.Current, previewChildren.Current, displayService);
					processNext = (previewChildren.MoveNext() && originalChildren.MoveNext());
				}
			}

			// Set DisplayService for the preview shape
			if (previewShape.DisplayService != displayService)
				previewShape.DisplayService = displayService;
		}


		private void RemovePreviewOf(Shape originalShape) {
			if (_previewShapes.ContainsKey(originalShape)) {
				// Invalidate Preview Shape
				Shape previewShape = Previews[originalShape];
				previewShape.Invalidate();

				// remove previews of the shape and its children from the preview's dictionary
				_previewShapes.Remove(originalShape);
				if (originalShape.Children.Count > 0) {
					foreach (Shape childShape in originalShape.Children)
						_previewShapes.Remove(childShape);
				}
				// remove the shape and its children from the shape's dictionary
				_originalShapes.Remove(previewShape);
				if (previewShape.Children.Count > 0) {
					foreach (Shape childShape in previewShape.Children)
						_originalShapes.Remove(childShape);
				}
			}
		}


		private void RemovePreview(Shape previewShape) {
			Shape origShape = null;
			if (!_originalShapes.TryGetValue(previewShape, out origShape))
				throw new NShapeInternalException("This preview shape has no associated original shape in this tool.");
			else {
				// Invalidate Preview Shape
				previewShape.Invalidate();
				// Remove both, original- and preview shape from the appropriate dictionaries
				_previewShapes.Remove(origShape);
				_originalShapes.Remove(previewShape);
			}
		}


		private void ClearPreviews() {
			foreach (KeyValuePair<Shape, Shape> item in _previewShapes) {
				Shape previewShape = item.Value;
				DeletePreviewShape(ref previewShape);
			}
			_previewShapes.Clear();
			_originalShapes.Clear();
		}


		private bool IsConnectedToNonSelectedShape(IDiagramPresenter diagramPresenter, Shape shape) {
			foreach (ControlPointId gluePointId in shape.GetControlPointIds(ControlPointCapabilities.Glue)) {
				ShapeConnectionInfo sci = shape.GetConnectionInfo(gluePointId, null);
				if (!sci.IsEmpty
					&& !diagramPresenter.SelectedShapes.Contains(sci.OtherShape))
					return true;
			}
			return false;
		}


		/// <summary>
		/// Create previews of shapes connected to the given shape (and it's children) and connect them to the
		/// shape's preview (or the preview of it's child)
		/// </summary>
		private void ConnectPreviewOfShape(IDiagramPresenter diagramPresenter, Shape shape) {
			// process shape's children
			if (shape.Children != null && shape.Children.Count > 0) {
				foreach (Shape childShape in shape.Children)
					ConnectPreviewOfShape(diagramPresenter, childShape);
			}

			Shape preview = FindPreviewOfShape(shape);
			foreach (ShapeConnectionInfo connectionInfo in shape.GetConnectionInfos(ControlPointId.Any, null)) {
				if (!diagramPresenter.IsLayerVisible(connectionInfo.OtherShape.HomeLayer, connectionInfo.OtherShape.SupplementalLayers)) continue;
				if (diagramPresenter.SelectedShapes.Contains(connectionInfo.OtherShape)) {
					// Do not connect previews if BOTH of the connected shapes are part of the selection because 
					// this would restrict movement of the connector shapes and decreases performance (many 
					// unnecessary FollowConnectionPointWithGluePoint() calls)
					if (shape.HasControlPointCapability(connectionInfo.OwnPointId, ControlPointCapabilities.Glue)) {
						if (IsConnectedToNonSelectedShape(diagramPresenter, shape)) {
							Shape targetPreview = FindPreviewOfShape(connectionInfo.OtherShape);
							preview.Connect(connectionInfo.OwnPointId, targetPreview, connectionInfo.OtherPointId);
						}
					}
				} else {
					// Connect preview of shape to a non-selected shape if it is a single shape 
					// that has a glue point (e.g. a Label)
					if (preview.HasControlPointCapability(connectionInfo.OwnPointId, ControlPointCapabilities.Glue)) {
						// Only connect if the control point to be connected is not the control point to be moved
						if (shape == _selectedShapeAtCursorInfo.Shape && connectionInfo.OwnPointId != _selectedShapeAtCursorInfo.ControlPointId)
							preview.Connect(connectionInfo.OwnPointId, connectionInfo.OtherShape, connectionInfo.OtherPointId);
					} else
						// Create a preview of the shape that is connected to the preview (recursive call)
						CreateConnectedTargetPreviewShape(diagramPresenter, preview, connectionInfo);
				}
			}
		}


		/// <summary>
		/// Creates (or finds) a preview of the connection's PassiveShape and connects it to the current preview shape
		/// </summary>
		private void CreateConnectedTargetPreviewShape(IDiagramPresenter diagramPresenter, Shape previewShape, ShapeConnectionInfo connectionInfo) {
			// Check if any other selected shape is connected to the same non-selected shape
			Shape previewTargetShape;
			// If the current passiveShape is already connected to another shape of the current selection,
			// connect the current preview to the other preview's passiveShape
			if (!_targetShapeBuffer.TryGetValue(connectionInfo.OtherShape, out previewTargetShape)) {
				// If the current passiveShape is not connected to any other of the selected selectedShapes,
				// create a clone of the passiveShape and connect it to the corresponding preview
				// If the preview exists, abort connecting (in this case, the shape is a preview of a child shape)
				if (_previewShapes.ContainsKey(connectionInfo.OtherShape))
					return;
				else {
					previewTargetShape = connectionInfo.OtherShape.Type.CreatePreviewInstance(connectionInfo.OtherShape);
					AddPreview(connectionInfo.OtherShape, previewTargetShape, diagramPresenter.DisplayService);
				}
				// Add passive shape and it's clone to the passive shape dictionary
				_targetShapeBuffer.Add(connectionInfo.OtherShape, previewTargetShape);
			}
			// Connect the (new or existing) preview shapes
			// Skip connecting if the preview is already connected.
			DebugDiagnostics.Assert(previewTargetShape != null, "Error while creating connected preview shapes.");
			if (previewTargetShape.IsConnected(connectionInfo.OtherPointId, null) == ControlPointId.None) {
				previewTargetShape.Connect(connectionInfo.OtherPointId, previewShape, connectionInfo.OwnPointId);
				// check, if any shapes are connected to the connector (that is connected to the selected shape)
				foreach (ShapeConnectionInfo connectorCI in connectionInfo.OtherShape.GetConnectionInfos(ControlPointId.Any, null)) {
					if (!diagramPresenter.IsLayerVisible(connectionInfo.OtherShape.HomeLayer, connectorCI.OtherShape.SupplementalLayers)) continue;
					// skip if the connector is connected to the shape with more than one glue point
					if (connectorCI.OtherShape == FindShapeOfPreview(previewShape)) continue;
					if (connectorCI.OwnPointId != connectionInfo.OtherPointId) {
						// Check if the shape on the other end is selected.
						// If it is, connect to it's preview or skip connecting if the target preview does 
						// not exist yet (it will be connected when creating the targt's preview)
						if (diagramPresenter.SelectedShapes.Contains(connectorCI.OtherShape)) {
							if (_previewShapes.ContainsKey(connectorCI.OtherShape)) {
								Shape s = FindPreviewOfShape(connectorCI.OtherShape);
								if (s.IsConnected(connectorCI.OtherPointId, previewTargetShape) == ControlPointId.None)
									previewTargetShape.Connect(connectorCI.OwnPointId, s, connectorCI.OtherPointId);
							} else continue;
						} else if (connectorCI.OtherShape.HasControlPointCapability(connectorCI.OtherPointId, ControlPointCapabilities.Glue))
							// Connect connectors connected to the previewTargetShape
							CreateConnectedTargetPreviewShape(diagramPresenter, previewTargetShape, connectorCI);
						else if (connectorCI.OtherPointId == ControlPointId.Reference) {
							// Connect the other end of the previewTargetShape if the connection is a Point-To-Shape connection
							DebugDiagnostics.Assert(connectorCI.OtherShape.IsConnected(connectorCI.OtherPointId, previewTargetShape) == ControlPointId.None);
							if (previewTargetShape.IsConnected(connectorCI.OwnPointId, null) == ControlPointId.None)
								previewTargetShape.Connect(connectorCI.OwnPointId, connectorCI.OtherShape, connectorCI.OtherPointId);
						}
					}
				}
			}
		}


		// Experimental code
		//private void CreatePreviewsOfConnectedShapes(IDiagramPresenter diagramPresenter, Shape shape) {
		//   // process shape's children
		//   if (shape.Children != null && shape.Children.Count > 0) {
		//      foreach (Shape childShape in shape.Children)
		//         CreatePreviewsOfConnectedShapes(diagramPresenter, childShape);
		//   }

		//   Shape preview = FindPreviewOfShape(shape);
		//   foreach (ShapeConnectionInfo connectionInfo in shape.GetConnectionInfos(ControlPointId.Any, null)) {
		//      if (diagramPresenter.SelectedShapes.Contains(connectionInfo.OtherShape)) continue;
		//      // Create a preview of the shape that is connected to the preview (recursive call)
		//      DoCreatePreviewsOfConnectedShape(diagramPresenter, preview, connectionInfo);
		//   }
		//}


		///// <summary>
		///// Creates (or finds) a preview of the connection's PassiveShape and connects it to the current preview shape
		///// </summary>
		//private void DoCreatePreviewsOfConnectedShape(IDiagramPresenter diagramPresenter, Shape previewShape, ShapeConnectionInfo connectionInfo) {
		//   // Check if any other selected shape is connected to the same non-selected shape
		//   Shape previewTargetShape;
		//   // If the current passiveShape is already connected to another shape of the current selection,
		//   // connect the current preview to the other preview's passiveShape
		//   if (!targetShapeBuffer.TryGetValue(connectionInfo.OtherShape, out previewTargetShape)) {
		//      // If the current passiveShape is not connected to any other of the selected selectedShapes,
		//      // create a clone of the passiveShape and connect it to the corresponding preview
		//      // If the preview exists, abort connecting (in this case, the shape is a preview of a child shape)
		//      if (previewShapes.ContainsKey(connectionInfo.OtherShape)) 
		//         return;
		//      else {
		//         previewTargetShape = connectionInfo.OtherShape.Type.CreatePreviewInstance(connectionInfo.OtherShape);
		//         AddPreview(connectionInfo.OtherShape, previewTargetShape, diagramPresenter.DisplayService);
		//      }
		//      // Add passive shape and it's clone to the passive shape dictionary
		//      targetShapeBuffer.Add(connectionInfo.OtherShape, previewTargetShape);
		//   }
		//   // Connect the (new or existing) preview shapes
		//   // Skip connecting if the preview is already connected.
		//   Assert(previewTargetShape != null, "Error while creating connected preview shapes.");
		//   // check, if any shapes are connected to the connector (that is connected to the selected shape)
		//   foreach (ShapeConnectionInfo connectorCI in connectionInfo.OtherShape.GetConnectionInfos(ControlPointId.Any, null)) {
		//      if (connectorCI.OtherShape == FindShapeOfPreview(previewShape)) continue;
		//      if (connectorCI.OwnPointId == connectionInfo.OtherPointId) continue;
		//      DoCreatePreviewsOfConnectedShape(diagramPresenter, previewTargetShape, connectorCI);
		//   }
		//}


		///// <summary>
		///// Create previews of shapes connected to the given shape (and it's children) and connect them to the
		///// shape's preview (or the preview of it's child)
		///// </summary>
		//private void ConnectPreviewsToShape(IDiagramPresenter diagramPresenter, Shape shape) {
		//   // process shape's children
		//   if (shape.Children != null && shape.Children.Count > 0) {
		//      foreach (Shape childShape in shape.Children)
		//         ConnectPreviewsToShape(diagramPresenter, childShape);
		//   }

		//   Shape preview = FindPreviewOfShape(shape);
		//   foreach (ShapeConnectionInfo connectionInfo in shape.GetConnectionInfos(ControlPointId.Any, null)) {
		//      if (diagramPresenter.SelectedShapes.Contains(connectionInfo.OtherShape)) {
		//         // Do not connect previews if BOTH of the connected shapes are part of the selection because 
		//         // this would restrict movement of the connector shapes and decreases performance (many 
		//         // unnecessary FollowConnectionPointWithGluePoint() calls)
		//         if (shape.HasControlPointCapability(connectionInfo.OwnPointId, ControlPointCapabilities.Glue)) {
		//            if (IsConnectedToNonSelectedShape(diagramPresenter, shape)) {
		//               Shape targetPreview = FindPreviewOfShape(connectionInfo.OtherShape);
		//               preview.Connect(connectionInfo.OwnPointId, targetPreview, connectionInfo.OtherPointId);
		//            }
		//         }
		//      } else {
		//         // Connect preview of shape to a non-selected shape if it is a single shape 
		//         // that has a glue point (e.g. a Label)
		//         if (preview.HasControlPointCapability(connectionInfo.OwnPointId, ControlPointCapabilities.Glue)) {
		//            // Only connect if the control point to be connected is not the control point to be moved
		//            if (shape == SelectedShapeAtCursorInfo.Shape && connectionInfo.OwnPointId != SelectedShapeAtCursorInfo.ControlPointId) {
		//               if (preview.IsConnected(connectionInfo.OwnPointId, null) == ControlPointId.None)
		//                  preview.Connect(connectionInfo.OwnPointId, connectionInfo.OtherShape, connectionInfo.OtherPointId);
		//            }
		//         } else
		//            // Create a preview of the shape that is connected to the preview (recursive call)
		//            DoConnectPreviewsToShape(diagramPresenter, preview, connectionInfo);
		//      }
		//   }
		//}


		///// <summary>
		///// Creates (or finds) a preview of the connection's PassiveShape and connects it to the current preview shape
		///// </summary>
		//private void DoConnectPreviewsToShape(IDiagramPresenter diagramPresenter, Shape previewShape, ShapeConnectionInfo connectionInfo) {
		//   // Check if any other selected shape is connected to the same non-selected shape
		//   Shape previewTargetShape;
		//   // If the current passiveShape is already connected to another shape of the current selection,
		//   // connect the current preview to the other preview's passiveShape
		//   if (!targetShapeBuffer.TryGetValue(connectionInfo.OtherShape, out previewTargetShape)) return;

		//   if (previewTargetShape.IsConnected(connectionInfo.OtherPointId, null) == ControlPointId.None) {
		//      previewTargetShape.Connect(connectionInfo.OtherPointId, previewShape, connectionInfo.OwnPointId);
		//      // check, if any shapes are connected to the connector (that is connected to the selected shape)
		//      foreach (ShapeConnectionInfo connectorCI in connectionInfo.OtherShape.GetConnectionInfos(ControlPointId.Any, null)) {
		//         // skip if the connector is connected to the shape with more than one glue point
		//         if (connectorCI.OtherShape == FindShapeOfPreview(previewShape)) continue;
		//         if (connectorCI.OwnPointId != connectionInfo.OtherPointId) {
		//            // Check if the shape on the other end is selected.
		//            // If it is, connect to it's preview or skip connecting if the target preview does 
		//            // not exist yet (it will be connected when creating the targt's preview)
		//            if (diagramPresenter.SelectedShapes.Contains(connectorCI.OtherShape)) {
		//               if (previewShapes.ContainsKey(connectorCI.OtherShape)) {
		//                  Shape s = FindPreviewOfShape(connectorCI.OtherShape);
		//                  if (s.IsConnected(connectorCI.OtherPointId, previewTargetShape) == ControlPointId.None)
		//                     previewTargetShape.Connect(connectorCI.OwnPointId, s, connectorCI.OtherPointId);
		//               } else continue;
		//            } else if (connectorCI.OtherShape.HasControlPointCapability(connectorCI.OtherPointId, ControlPointCapabilities.Glue))
		//               // Connect connectors connected to the previewTargetShape
		//               DoConnectPreviewsToShape(diagramPresenter, previewTargetShape, connectorCI);
		//            else if (connectorCI.OtherPointId == ControlPointId.Reference) {
		//               // Connect the other end of the previewTargetShape if the connection is a Point-To-Shape connection
		//               Assert(connectorCI.OtherShape.IsConnected(connectorCI.OtherPointId, previewTargetShape) == ControlPointId.None);
		//               Assert(previewTargetShape.IsConnected(connectorCI.OwnPointId, null) == ControlPointId.None);
		//               previewTargetShape.Connect(connectorCI.OwnPointId, connectorCI.OtherShape, connectorCI.OtherPointId);
		//            }
		//         }
		//      }
		//   }
		//}

		#endregion


		#region [Private] Helper Methods

		// ToDo: Wenn unbedingt nötigt eine Funktion machen die heißt:
		//		int DetermineCursor(IDiagramPresenter diagramPresenter, MouseSTate mouseState, int Action, Shape shapeAtCursor, ControlPointId controlPointAtCursor)
		/// <summary>
		/// Determines the mouse cursor from the current action and the given mouse state.
		/// </summary>
		private int DetermineCursor(IDiagramPresenter diagramPresenter, MouseState mouseState) {
			switch (CurrentAction) {
				case Action.None:
					// If no action is pending, the folowing cursors are feasible:
					// - Default (no selected shape under cursor or action not granted)
					// - Move shape cursor
					// - Move grip cursor
					// - Rotate cursor
					// - Edit caption cursor
					if (!_selectedShapeAtCursorInfo.IsEmpty) {
						// Check if cursor is over a caption and editing caption is feasible
						if (IsEditCaptionFeasible(diagramPresenter, mouseState, _selectedShapeAtCursorInfo))
							//return cursors[ToolCursor.EditCaption];
							return _cursors[ToolCursor.MoveShape];
						// Check if cursor is over a control point and moving grips or rotating is feasible
						if (_selectedShapeAtCursorInfo.IsCursorAtGrip) {
							if (IsMoveHandleFeasible(diagramPresenter, mouseState, _selectedShapeAtCursorInfo))
								return _cursors[ToolCursor.MoveHandle];
							else if (IsRotatatingFeasible(diagramPresenter, mouseState, _selectedShapeAtCursorInfo))
								return _cursors[ToolCursor.Rotate];
							else 
								return _cursors[ToolCursor.Default];
						}
						// Check if cursor is inside the shape and move shape is feasible
						if (IsMoveShapeFeasible(diagramPresenter, mouseState, _selectedShapeAtCursorInfo))
							return _cursors[ToolCursor.MoveShape];
					}
					return _cursors[ToolCursor.Default];

				case Action.Select:
				case Action.SelectWithFrame:
					return _cursors[ToolCursor.Default];

				case Action.EditCaption:
					DebugDiagnostics.Assert(!_selectedShapeAtCursorInfo.IsEmpty);
					DebugDiagnostics.Assert(_selectedShapeAtCursorInfo.Shape is ICaptionedShape);
					// If the cursor is outside the caption, return default cursor
					int captionIndex = ((ICaptionedShape)_selectedShapeAtCursorInfo.Shape).FindCaptionFromPoint(mouseState.X, mouseState.Y);
					if (captionIndex == _selectedShapeAtCursorInfo.CaptionIndex)
						return _cursors[ToolCursor.EditCaption];
					else 
						return _cursors[ToolCursor.Default];

				case Action.MoveHandle:
					DebugDiagnostics.Assert(!_selectedShapeAtCursorInfo.IsEmpty);
					DebugDiagnostics.Assert(_selectedShapeAtCursorInfo.IsCursorAtGrip);
					if (_selectedShapeAtCursorInfo.IsCursorAtGluePoint) {
						Shape previewShape = FindPreviewOfShape(_selectedShapeAtCursorInfo.Shape);
						Point ptPos = previewShape.GetControlPointPosition(_selectedShapeAtCursorInfo.ControlPointId);
						Locator target = FindNearestTarget(diagramPresenter, ptPos.X, ptPos.Y, ControlPointCapabilities.All, 
								GetFindTargetRange(diagramPresenter), CreateIsConnectionTargetFilter(diagramPresenter));
						if (!target.IsEmpty && CanConnect(_selectedShapeAtCursorInfo.Shape, _selectedShapeAtCursorInfo.ControlPointId, target.Shape, target.ControlPointId))
							return _cursors[ToolCursor.Connect];
					}
					return _cursors[ToolCursor.MoveHandle];

				case Action.MoveShape:
					return _cursors[ToolCursor.MoveShape];

				case Action.PrepareRotate:
				case Action.Rotate:
					return _cursors[ToolCursor.Rotate];

				default: throw new NShapeUnsupportedValueException(CurrentAction);
			}
		}


		private void InvalidateShapes(IDiagramPresenter diagramPresenter, IEnumerable<Shape> shapes) {
			InvalidateShapes(diagramPresenter, shapes, ControlPointCapabilities.None);
		}


		private void InvalidateShapes(IDiagramPresenter diagramPresenter, IEnumerable<Shape> shapes, ControlPointCapabilities invalidateControlPoints) {
			foreach (Shape shape in shapes)
				DoInvalidateShape(diagramPresenter, shape, invalidateControlPoints);
		}


		private void SetSelectedShapeAtCursor(IDiagramPresenter diagramPresenter, int mouseX, int mouseY, int handleRadius, ControlPointCapabilities handleCapabilities) {
			// Find the shape under the cursor
			_selectedShapeAtCursorInfo.Clear();
			_selectedShapeAtCursorInfo.Shape = diagramPresenter.SelectedShapes.FindShape(mouseX, mouseY, handleCapabilities, handleRadius, null);

			// If there is a shape under the cursor, find the nearest control point and caption
			if (!_selectedShapeAtCursorInfo.IsEmpty) {
				ControlPointCapabilities capabilities;
				if (CurrentMouseState.IsKeyPressed(KeysDg.Control)) capabilities = ControlPointCapabilities.Resize /*| ControlPointCapabilities.Movable*/;
				else if (CurrentMouseState.IsKeyPressed(KeysDg.Shift)) capabilities = ControlPointCapabilities.Rotate;
				else capabilities = GripCapabilities;

				// Find control point at cursor that belongs to the selected shape at cursor
				_selectedShapeAtCursorInfo.ControlPointId = _selectedShapeAtCursorInfo.Shape.FindNearestControlPoint(mouseX, mouseY, diagramPresenter.ZoomedGripSize, capabilities);
				// Find caption at cursor (if the shape is a captioned shape)
				if (_selectedShapeAtCursorInfo.Shape is ICaptionedShape && ((ICaptionedShape)_selectedShapeAtCursorInfo.Shape).CaptionCount > 0)
					_selectedShapeAtCursorInfo.CaptionIndex = ((ICaptionedShape)_selectedShapeAtCursorInfo.Shape).FindCaptionFromPoint(mouseX, mouseY);
			}
		}


		private bool ShapeOrShapeRelativesContainsPoint(Shape shape, int x, int y, ControlPointCapabilities capabilities, int range) {
			if (shape.HitTest(x, y, capabilities, range) != ControlPointId.None)
				return true;
			else if (shape.Parent != null) {
				if (ShapeOrShapeRelativesContainsPoint(shape.Parent, x, y, capabilities, range))
					return true;
			}
			return false;
		}


		private void DoInvalidateShape(IDiagramPresenter diagramPresenter, Shape shape, ControlPointCapabilities controlPointsToInvalidate) {
			if (shape.Parent != null)
				DoInvalidateShape(diagramPresenter, shape.Parent, ControlPointCapabilities.None);
			else {
				shape.Invalidate();
				if (ControlPointCapabilities.All != ControlPointCapabilities.None)
					diagramPresenter.InvalidateGrips(shape, controlPointsToInvalidate);
			}
		}


		/// <summary>
		/// Checks whether a drag action can be performed.
		/// </summary>
		private bool IsDragActionFeasible(MouseState mouseState, MouseButtonsDg button) {
			int dx = 0, dy = 0;
			if (mouseState.IsButtonDown(button) && IsToolActionPending) {
				// Check the minimum drag distance before switching to a drag action
				dx = Math.Abs(mouseState.X - ActionStartMouseState.X);
				dy = Math.Abs(mouseState.Y - ActionStartMouseState.Y);
			}
			return (dx >= MinimumDragDistance.Width || dy >= MinimumDragDistance.Height);
		}


		/// <summary>
		/// Checks whether the selected shape(s) can be moved.
		/// </summary>
		private bool IsMoveShapeFeasible(IDiagramPresenter diagramPresenter, MouseState mouseState, ShapeAtCursorInfo shapeAtCursorInfo) {
			if (shapeAtCursorInfo.IsEmpty || mouseState.IsEmpty)
				return false;
			if (!diagramPresenter.Project.SecurityManager.IsGranted(Permission.Layout, shapeAtCursorInfo.Shape))
				return false;
			if (diagramPresenter.SelectedShapes.Count > 0 && !diagramPresenter.Project.SecurityManager.IsGranted(Permission.Layout, diagramPresenter.SelectedShapes))
				return false;
			if (!shapeAtCursorInfo.Shape.ContainsPoint(mouseState.X, mouseState.Y))
				return false;
			// 
			if (shapeAtCursorInfo.Shape.HasControlPointCapability(ControlPointId.Reference, ControlPointCapabilities.Glue))
				if (shapeAtCursorInfo.Shape.IsConnected(ControlPointId.Reference, null) != ControlPointId.None) return false;
			// Check if the shape is connected to other shapes with its glue points
			if (diagramPresenter.SelectedShapes.Count == 0) {
				// Check if the non-selected shape at cursor (which will be selected) owns glue points connected to other shapes
				foreach (ControlPointId id in shapeAtCursorInfo.Shape.GetControlPointIds(ControlPointCapabilities.Glue))
					if (shapeAtCursorInfo.Shape.IsConnected(id, null) != ControlPointId.None) return false;
			} else if (diagramPresenter.SelectedShapes.Contains(shapeAtCursorInfo.Shape)) {
				// LinearShapes that own connected gluePoints may not be moved.
				foreach (Shape shape in diagramPresenter.SelectedShapes) {
					if (shape is ILinearShape) {
						foreach (ControlPointId gluePointId in shape.GetControlPointIds(ControlPointCapabilities.Glue)) {
							ShapeConnectionInfo sci = shape.GetConnectionInfo(gluePointId, null);
							if (!sci.IsEmpty) {
								// Allow movement if the connected shapes are moved together
								if (!diagramPresenter.SelectedShapes.Contains(sci.OtherShape))
									return false;
							}
						}
					}
				}
			}
			return true;
		}


		/// <summary>
		/// Checks whether a handle of the selected shape can be moved.
		/// </summary>
		private bool IsMoveHandleFeasible(IDiagramPresenter diagramPresenter, MouseState mouseState, ShapeAtCursorInfo shapeAtCursorInfo) {
			if (shapeAtCursorInfo.IsEmpty)
				return false;
			// Collides with the resize modifiers
			//if (mouseState.IsKeyPressed(KeysDg.Shift)) return false;
			if (shapeAtCursorInfo.Shape.HasControlPointCapability(shapeAtCursorInfo.ControlPointId, ControlPointCapabilities.Glue)) {
				if (!diagramPresenter.Project.SecurityManager.IsGranted(Permission.Connect, diagramPresenter.SelectedShapes))
					return false;
			} else {
				if (!diagramPresenter.Project.SecurityManager.IsGranted(Permission.Layout, diagramPresenter.SelectedShapes))
					return false;
			}
			if (!shapeAtCursorInfo.Shape.HasControlPointCapability(shapeAtCursorInfo.ControlPointId, ControlPointCapabilities.Resize | ControlPointCapabilities.Glue /*| ControlPointCapabilities.Movable*/))
				return false;
			if (diagramPresenter.SelectedShapes.Count > 1) {
				// GluePoints may only be moved alone
				if (shapeAtCursorInfo.Shape.HasControlPointCapability(shapeAtCursorInfo.ControlPointId, ControlPointCapabilities.Glue))
					return false;
				// Check if all shapes that are going to be resizes are of the same type
				Shape lastShape = null;
				foreach (Shape shape in diagramPresenter.SelectedShapes) {
					if (lastShape != null && lastShape.Type != shape.Type)
						return false;
					lastShape = shape;
				}
			}
			return true;
		}


		/// <summary>
		/// Checks whether the selected shape(s) can be rotated.
		/// </summary>
		private bool IsRotatatingFeasible(IDiagramPresenter diagramPresenter, MouseState mouseState, ShapeAtCursorInfo shapeAtCursorInfo) {
			if (shapeAtCursorInfo.IsEmpty)
				return false;
			// Collides with the rotate modifiers
			//if (mouseState.IsKeyPressed(KeysDg.Shift)) return false;
			if (!diagramPresenter.Project.SecurityManager.IsGranted(Permission.Layout, diagramPresenter.SelectedShapes))
				return false;
			if (!shapeAtCursorInfo.Shape.HasControlPointCapability(shapeAtCursorInfo.ControlPointId, ControlPointCapabilities.Rotate))
				return false;
			if (diagramPresenter.SelectedShapes.Count > 1) {
				// check if all selected shapes have a rotate handle
				foreach (Shape selectedShape in diagramPresenter.SelectedShapes) {
					bool shapeHasRotateHandle = false;
					foreach (ControlPointId ptId in selectedShape.GetControlPointIds(ControlPointCapabilities.Rotate)) {
						shapeHasRotateHandle = true;
						break;
					}
					if (!shapeHasRotateHandle) return false;
				}
			}
			return true;
		}


		/// <summary>
		/// Checks whether a ahape caption can be edited.
		/// </summary>
		private bool IsEditCaptionFeasible(IDiagramPresenter diagramPresenter, MouseState mouseState, ShapeAtCursorInfo shapeAtCursorInfo) {
			if (shapeAtCursorInfo.IsEmpty || mouseState.IsEmpty)
				return false;
			if (!diagramPresenter.Project.SecurityManager.IsGranted(Permission.Data, shapeAtCursorInfo.Shape))
				return false;
			if (!shapeAtCursorInfo.IsCursorAtCaption)
				return false;
			else {
				// If there is another shape under the caption, prefer the "Select" action over the "EditCaption" action
				Shape s = diagramPresenter.Diagram.Shapes.FindShape(mouseState.X, mouseState.Y, ControlPointCapabilities.None, 0, shapeAtCursorInfo.Shape);
				if (s == null || s != shapeAtCursorInfo.Shape && s.ContainsPoint(mouseState.X, mouseState.Y))
					return false;
			}
			// Not necessary any more: Edit caption is triggered on MouseUp event and only of the mouse was not moved.
			//if (mouseState.IsKeyPressed(KeysDg.Control) || mouseState.IsKeyPressed(KeysDg.Shift))
			//    return false;
			return true;
		}

		#endregion


		#region [Private] Construction

		static SelectionTool() {
			_cursors = new Dictionary<ToolCursor, int>(8);
			// Register cursors
			_cursors.Add(ToolCursor.Default, CursorProvider.DefaultCursorID);
			_cursors.Add(ToolCursor.ActionDenied, CursorProvider.RegisterCursor(Properties.Resources.ActionDeniedCursor));
			_cursors.Add(ToolCursor.EditCaption, CursorProvider.RegisterCursor(Properties.Resources.EditTextCursor));
			_cursors.Add(ToolCursor.MoveShape, CursorProvider.RegisterCursor(Properties.Resources.MoveShapeCursor));
			_cursors.Add(ToolCursor.MoveHandle, CursorProvider.RegisterCursor(Properties.Resources.MovePointCursor));
			_cursors.Add(ToolCursor.Rotate, CursorProvider.RegisterCursor(Properties.Resources.RotateCursor));
			// ToDo: Create better Connect/Disconnect cursors
			_cursors.Add(ToolCursor.Connect, CursorProvider.RegisterCursor(Properties.Resources.HandCursor));
			_cursors.Add(ToolCursor.Disconnect, CursorProvider.RegisterCursor(Properties.Resources.HandCursor));
			_cursors.Add(ToolCursor.Pan, CursorProvider.RegisterCursor(Properties.Resources.HandOpenCursor));
			_cursors.Add(ToolCursor.PanActive, CursorProvider.RegisterCursor(Properties.Resources.HandGrabCursor));
		}


		private void Construct() {
			Title = Dataweb.NShape.Properties.Resources.CaptionText_SelectionTool_Title;
			ToolTipText = String.Format(Dataweb.NShape.Properties.Resources.CaptionFmt_SelectionTool_Description, Environment.NewLine);

			SmallIcon = Properties.Resources.PointerIconSmall;
			SmallIcon.MakeTransparent(Color.Fuchsia);

			LargeIcon = Properties.Resources.PointerIconLarge;
			LargeIcon.MakeTransparent(Color.Fuchsia);

			_frameRect = Rectangle.Empty;
		}

		#endregion


		#region [Private] Types

		private enum ToolCursor {
			Default,
			Rotate,
			MoveHandle,
			MoveShape,
			ActionDenied,
			EditCaption,
			Connect,
			Disconnect,
			Pan,
			PanActive
		}


		// connection handling stuff
		private struct ConnectionInfoBuffer : IEquatable<ConnectionInfoBuffer> {

			public static readonly ConnectionInfoBuffer Empty;

			public static bool operator ==(ConnectionInfoBuffer a, ConnectionInfoBuffer b) { return (a.connectionInfo == b.connectionInfo && a.shape == b.shape); }

			public static bool operator !=(ConnectionInfoBuffer a, ConnectionInfoBuffer b) { return !(a == b); }

			public Shape shape;

			public ShapeConnectionInfo connectionInfo;

			/// <override></override>
			public override bool Equals(object obj) { return obj is ConnectionInfoBuffer && this == (ConnectionInfoBuffer)obj; }

			/// <ToBeCompleted></ToBeCompleted>
			public bool Equals(ConnectionInfoBuffer other) {
				return other == this;
			}

			/// <override></override>
			public override int GetHashCode() {
				return HashCodeGenerator.CalculateHashCode(shape, connectionInfo);
			}

			static ConnectionInfoBuffer() {
				Empty.shape = null;
				Empty.connectionInfo = ShapeConnectionInfo.Empty;
			}
		}

		#endregion


		#region Fields

		private const ControlPointCapabilities ConnectionCapabilities = ControlPointCapabilities.Connect;
		private const ControlPointCapabilities GripCapabilities = ControlPointCapabilities.Resize | ControlPointCapabilities.Rotate /*| ControlPointCapabilities.Movable*/;

		// --- Cursor registration dictionary ---
		private static Dictionary<ToolCursor, int> _cursors;
		//
		private OverlappingShapesAction _overlappingShapesSelectionMode = OverlappingShapesAction.Cycle;
		private bool _enableQuickRotate = false;

		// --- State after the last ProcessMouseEvent ---
		// selected shape under the mouse cursor, being highlighted in the next drawing
		private ShapeAtCursorInfo _selectedShapeAtCursorInfo;
		private Locator _handleMoveTarget = Locator.Empty;
		// Rectangle that represents the transformed selection area in control coordinates
		private Rectangle _frameRect;
		// Stores the distance the SelectedShape was moved on X-axis for snapping the nearest gridpoint
		private int _snapDistanceX;
		// Stores the distance the SelectedShape was moved on Y-axis for snapping the nearest gridpoint
		private int _snapDistanceY;
		// Index of the controlPoint that snapped to grid/point/swimline
		private int _snapPtId;

		private ShapeAtCursorInfo _previouslySelectedShapeAtCursor = ShapeAtCursorInfo.Empty;
		private List<Shape> _previouslySelectedShapes = new List<Shape>();

		// For destincting single click / double click
		private Timer _deferredClickTimer = new Timer();
		private ElapsedEventHandler _deferredClickHandler = null;

		// -- Definition of current action
		// indicates the current action depending on the mouseButton State, selected selectedShapes and mouse movement
		//private ToolAction currentToolAction = ToolAction.None;
		// preview shapes (Key = original shape, Value = preview shape)
		private Dictionary<Shape, Shape> _previewShapes = new Dictionary<Shape, Shape>();
		// original shapes (Key = preview shape, Value = original shape)
		private Dictionary<Shape, Shape> _originalShapes = new Dictionary<Shape, Shape>();

		// Buffers
		// rectangle buffer 
		private Rectangle _rectBuffer;
		// used for buffering selectedShapes connected to the preview selectedShapes: key = passiveShape, values = targetShapes's clone
		private Dictionary<Shape, Shape> _targetShapeBuffer = new Dictionary<Shape, Shape>();
		// buffer used for storing connections that are temporarily disconnected for moving shapes
		private List<ConnectionInfoBuffer> _connectionsBuffer = new List<ConnectionInfoBuffer>();

		#endregion

	}


	/// <summary>
	/// Lets the user size, move, rotate and select shapes.
	/// </summary>
	[Obsolete("Use SelectionTool instead")]
	public class PointerTool : SelectionTool {

		/// <ToBeCompleted></ToBeCompleted>
		public PointerTool() : base() { }

		/// <ToBeCompleted></ToBeCompleted>
		public PointerTool(string category) : base(category) { }

	}

}
