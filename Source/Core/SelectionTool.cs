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
			MouseState newMouseState = MouseState.Empty;
			newMouseState.Buttons = e.Buttons;
			newMouseState.Modifiers = e.Modifiers;
			newMouseState.Clicks = e.Clicks;
			newMouseState.Ticks = DateTime.UtcNow.Ticks;
			diagramPresenter.ControlToDiagram(e.Position, out newMouseState.Position);

			diagramPresenter.SuspendUpdate();
			try {
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
			} finally { diagramPresenter.ResumeUpdate(); }
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

					// Update Cursor when modifier keys are pressed or released
					if (((KeysDg)e.KeyCode & KeysDg.Shift) == KeysDg.Shift
						|| ((KeysDg)e.KeyCode & KeysDg.ShiftKey) == KeysDg.ShiftKey
						|| ((KeysDg)e.KeyCode & KeysDg.Control) == KeysDg.Control
						|| ((KeysDg)e.KeyCode & KeysDg.ControlKey) == KeysDg.ControlKey
						|| ((KeysDg)e.KeyCode & KeysDg.Alt) == KeysDg.Alt) {
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
					if (_selectedShapeAtCursorInfo != null && (_snapPtId > 0 || _snapDeltaX != 0 || _snapDeltaY != 0)) {
						Shape previewAtCursor = FindPreviewOfShape(_selectedShapeAtCursorInfo.Shape);
						diagramPresenter.DrawSnapIndicators(previewAtCursor);
					}
					// Finally, draw highlighted connection points and/or highlighted shape outlines
					if (diagramPresenter.SelectedShapes.Count == 1 && _selectedShapeAtCursorInfo.ControlPointId != ControlPointId.None) {
						Shape preview = FindPreviewOfShape(diagramPresenter.SelectedShapes.TopMost);
						if (preview.HasControlPointCapability(_selectedShapeAtCursorInfo.ControlPointId, ControlPointCapabilities.Glue)) {
							// Find and highlight valid connection targets in range
							Point p = preview.GetControlPointPosition(_selectedShapeAtCursorInfo.ControlPointId);
							DrawConnectionTargets(ActionDiagramPresenter, _selectedShapeAtCursorInfo.Shape, _selectedShapeAtCursorInfo.ControlPointId, p, ActionDiagramPresenter.SelectedShapes);
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
							MouseState initMouseState = GetPreviousMouseState();
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
#if DEBUG_DIAGNOSTICS
					Assert(!_selectedShapeAtCursorInfo.IsEmpty);
#endif
					if (Previews.Count > 0 && _selectedShapeAtCursorInfo.Shape != null) {
						InvalidateShapes(diagramPresenter, Previews.Values, false);
						if (diagramPresenter.SnapToGrid) {
							Shape previewAtCursor = FindPreviewOfShape(_selectedShapeAtCursorInfo.Shape);
							diagramPresenter.InvalidateSnapIndicators(previewAtCursor);
						}
						if (CurrentAction == Action.MoveHandle && _selectedShapeAtCursorInfo.IsCursorAtGluePoint)
							InvalidateConnectionTargets(diagramPresenter, CurrentMouseState.X, CurrentMouseState.Y);
					}
					break;

				case Action.PrepareRotate:
				case Action.Rotate:
					if (Previews.Count > 0)
						InvalidateShapes(diagramPresenter, Previews.Values, false);
					InvalidateAnglePreview(diagramPresenter);
					break;

				default: throw new NShapeUnsupportedValueException(typeof(MenuItemDef), CurrentAction);
			}
		}


		/// <summary>
		/// Returns the preview shape clone that is currently used to represent the preview of the current tool action for the given shape.
		/// </summary>
		public Shape FindPreviewOfShape(Shape shape) {
			if (shape == null) throw new ArgumentNullException(nameof(shape));
#if DEBUG_DIAGNOSTICS
			Assert(_previewShapes.ContainsKey(shape), string.Format("No preview found for '{0}' shape.", shape.Type.Name));
#endif
			return _previewShapes[shape];
		}


		/// <summary>
		/// Returns the original shape the given preview shape clone was created from.
		/// </summary>
		public Shape FindShapeOfPreview(Shape previewShape) {
			if (previewShape == null) throw new ArgumentNullException(nameof(previewShape));
#if DEBUG_DIAGNOSTICS
			Assert(_originalShapes.ContainsKey(previewShape), string.Format("No original shape found for '{0}' preview shape.", previewShape.Type.Name));
#endif
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


		#region [Protected] Tool Members

		/// <override></override>
		protected override void StartToolAction(IDiagramPresenter diagramPresenter, int action, MouseState mouseState, bool wantAutoScroll) {
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


		#region [Private] Properties

		private Action CurrentAction {
			get {
				if (IsToolActionPending)
					return (Action)CurrentToolAction.Action;
				else return Action.None;
			}
		}


		//private ShapeAtCursorInfo SelectedShapeAtCursorInfo {
		//    get { return selectedShapeAtCursorInfo; }
		//}

		#endregion


		#region [Private] MouseEvent processing implementation

		private bool ProcessMouseDown(IDiagramPresenter diagramPresenter, MouseState mouseState) {
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


		private bool ProcessMouseMove(IDiagramPresenter diagramPresenter, MouseState mouseState) {
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
					if (mouseState.IsButtonDown(MouseButtonsDg.Left)) {
						if (IsDragActionFeasible(mouseState, MouseButtonsDg.Left)) {
							// Determine new drag action
							shapeAtCursorInfo = FindShapeAtCursor(ActionDiagramPresenter, ActionStartMouseState.X, ActionStartMouseState.Y, ControlPointCapabilities.None, 0, true);
							newAction = DetermineMouseMoveAction(ActionDiagramPresenter, ActionStartMouseState, shapeAtCursorInfo);
						}
					} else newAction = DetermineMouseMoveAction(ActionDiagramPresenter, ActionStartMouseState, shapeAtCursorInfo);

					// If the action has changed, prepare and start the new action
					if (newAction != CurrentAction) {
						switch (newAction) {
							// Select --> SelectWithFrame
							case Action.SelectWithFrame:
#if DEBUG_DIAGNOSTICS
								Assert(CurrentAction == Action.Select);
#endif
								StartToolAction(diagramPresenter, (int)newAction, ActionStartMouseState, true);
								PrepareSelectionFrame(ActionDiagramPresenter, ActionStartMouseState);
								break;

							// Select --> (Select shape and) move shape
							case Action.MoveShape:
							case Action.MoveHandle:
							case Action.PrepareRotate:
#if DEBUG_DIAGNOSTICS
								Assert(CurrentAction == Action.Select || CurrentAction == Action.EditCaption);
#endif
								if (_selectedShapeAtCursorInfo.IsEmpty) {
									// Select shape at cursor before start dragging it
									PerformSelection(ActionDiagramPresenter, ActionStartMouseState, shapeAtCursorInfo);
									SetSelectedShapeAtCursor(diagramPresenter, ActionStartMouseState.X, ActionStartMouseState.Y, 0, ControlPointCapabilities.None);
#if DEBUG_DIAGNOSTICS
									Assert(!_selectedShapeAtCursorInfo.IsEmpty);
#endif
								}
								// Init moving shape
#if DEBUG_DIAGNOSTICS
								Assert(!_selectedShapeAtCursorInfo.IsEmpty);
#endif
								CreatePreviewShapes(ActionDiagramPresenter);
								StartToolAction(diagramPresenter, (int)newAction, ActionStartMouseState, true);
								PrepareMoveShapePreview(ActionDiagramPresenter, ActionStartMouseState);
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
					Invalidate(ActionDiagramPresenter);
					break;

				case Action.SelectWithFrame:
					PrepareSelectionFrame(ActionDiagramPresenter, mouseState);
					break;

				case Action.MoveHandle:
#if DEBUG_DIAGNOSTICS
					Assert(IsMoveHandleFeasible(ActionDiagramPresenter, mouseState, _selectedShapeAtCursorInfo));
#endif
					PrepareMoveHandlePreview(ActionDiagramPresenter, mouseState);
					break;

				case Action.MoveShape:
					PrepareMoveShapePreview(diagramPresenter, mouseState);
					break;

				case Action.PrepareRotate:
				case Action.Rotate:
#if DEBUG_DIAGNOSTICS
					Assert(IsRotatatingFeasible(ActionDiagramPresenter, mouseState, _selectedShapeAtCursorInfo));
#endif
					newAction = CurrentAction;
					// Find unselected shape under the mouse cursor
					newAction = DetermineMouseMoveAction(ActionDiagramPresenter, mouseState, _selectedShapeAtCursorInfo);

					// If the action has changed, prepare and start the new action
					if (newAction != CurrentAction) {
						switch (newAction) {
							// Rotate shape -> Prepare shape rotation
							case Action.PrepareRotate:
#if DEBUG_DIAGNOSTICS
								Assert(CurrentAction == Action.Rotate);
#endif
								EndToolAction();
								ClearPreviews();
								break;

							// Prepare shape rotation -> Rotate shape
							case Action.Rotate:
#if DEBUG_DIAGNOSTICS
								Assert(CurrentAction == Action.PrepareRotate);
#endif
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

					PrepareRotatePreview(ActionDiagramPresenter, mouseState);
					break;

				default: throw new NShapeUnsupportedValueException(typeof(Action), CurrentAction);
			}

			int cursorId = DetermineCursor(diagramPresenter, mouseState);
			if (CurrentAction == Action.None) diagramPresenter.SetCursor(cursorId);
			else ActionDiagramPresenter.SetCursor(cursorId);

			return result;
		}


		private bool ProcessMouseUp(IDiagramPresenter diagramPresenter, MouseState mouseState) {
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
							shapeAtCursorInfo.ControlPointId = shape.HitTest(mouseState.X, mouseState.Y, ControlPointCapabilities.None, 0);
							shapeAtCursorInfo.CaptionIndex = -1;
						}
					} else
						// No shape was selected -> try to find a shape at the current mouse position
						shapeAtCursorInfo = FindShapeAtCursor(diagramPresenter, mouseState.X, mouseState.Y, ControlPointCapabilities.None, ActionDiagramPresenter.ZoomedGripSize, false);

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
							InitDeferredClickAction(diagramPresenter, elapsedEventHandler);
							result = true;
						}
					}
					EndToolAction();
					break;

				case Action.SelectWithFrame:
					// select all selectedShapes within the frame
					result = PerformFrameSelection(ActionDiagramPresenter, mouseState);
					while (IsToolActionPending)
						EndToolAction();
					break;

				case Action.EditCaption:
					// if the user clicked a caption, display the caption editor
#if DEBUG_DIAGNOSTICS
					Assert(_selectedShapeAtCursorInfo.IsCursorAtCaption);
#endif
					// Update the selected shape at cursor
					SetSelectedShapeAtCursor(ActionDiagramPresenter, mouseState.X, mouseState.Y, ActionDiagramPresenter.ZoomedGripSize, ControlPointCapabilities.All);
					if (!_selectedShapeAtCursorInfo.IsEmpty && _selectedShapeAtCursorInfo.IsCursorAtCaption)
						ActionDiagramPresenter.OpenCaptionEditor((ICaptionedShape)_selectedShapeAtCursorInfo.Shape, _selectedShapeAtCursorInfo.CaptionIndex);
					EndToolAction();
					result = true;
					break;

				case Action.MoveHandle:
#if DEBUG_DIAGNOSTICS
					Assert(!_selectedShapeAtCursorInfo.IsEmpty);
#endif
					result = PerformMoveHandle(ActionDiagramPresenter);
					while (IsToolActionPending)
						EndToolAction();
					break;

				case Action.MoveShape:
#if DEBUG_DIAGNOSTICS
					Assert(!_selectedShapeAtCursorInfo.IsEmpty);
#endif
					result = PerformMoveShape(ActionDiagramPresenter);
					while (IsToolActionPending)
						EndToolAction();
					break;

				case Action.PrepareRotate:
				case Action.Rotate:
#if DEBUG_DIAGNOSTICS
					Assert(!_selectedShapeAtCursorInfo.IsEmpty);
#endif
					if (CurrentAction == Action.Rotate)
						result = PerformRotate(ActionDiagramPresenter);
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


		#region [Private] Determine action depending on mouse state and event type

		/// <summary>
		/// Decide which tool action is suitable for the current mouse state.
		/// </summary>
		private Action DetermineMouseDownAction(IDiagramPresenter diagramPresenter, MouseState mouseState) {
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
				return manipulatedShapeInfo.CanConnect(targetShapeInfo);
			return false;
		}

		#endregion


		#region [Private] Action implementations

		#region Selecting Shapes

		// (Un)Select shape unter the mouse pointer
		private bool PerformSelection(IDiagramPresenter diagramPresenter, MouseState mouseState, ShapeAtCursorInfo shapeAtCursorInfo) {
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


		// Calculate new selection frame
		private void PrepareSelectionFrame(IDiagramPresenter diagramPresenter, MouseState mouseState) {
			_previewMouseState = mouseState;

			Invalidate(ActionDiagramPresenter);

			_frameRect.X = Math.Min(ActionStartMouseState.X, mouseState.X);
			_frameRect.Y = Math.Min(ActionStartMouseState.Y, mouseState.Y);
			_frameRect.Width = Math.Max(ActionStartMouseState.X, mouseState.X) - _frameRect.X;
			_frameRect.Height = Math.Max(ActionStartMouseState.Y, mouseState.Y) - _frameRect.Y;

			Invalidate(ActionDiagramPresenter);
		}


		// Select shapes inside the selection frame
		private bool PerformFrameSelection(IDiagramPresenter diagramPresenter, MouseState mouseState) {
			bool multiSelect = mouseState.IsKeyPressed(KeysDg.Control) || mouseState.IsKeyPressed(KeysDg.Shift);
			diagramPresenter.SelectShapes(_frameRect, multiSelect);
			return true;
		}


		private void InitDeferredClickAction(IDiagramPresenter diagramPresenter, ElapsedEventHandler elapsedEventHandler) {
			// Ensure that existing Elapsed event handlers will be removed before adding a new one!
			CancelDeferredClickAction();
			Debug.Assert(_deferredClickHandler == null);
			_deferredClickHandler = elapsedEventHandler;

			// Start processing the click event
			if (_deferredClickTimer.Interval != DoubleClickTime)
				_deferredClickTimer.Interval = DoubleClickTime;
			if (_deferredClickTimer.SynchronizingObject != diagramPresenter)
				_deferredClickTimer.SynchronizingObject = diagramPresenter;
			_deferredClickTimer.Elapsed += _deferredClickHandler;
			_deferredClickTimer.AutoReset = true;
			_deferredClickTimer.Start();
		}


		private void CancelDeferredClickAction() {
			_deferredClickTimer.Stop();
			if (_deferredClickHandler != null) {
				_deferredClickTimer.Elapsed -= _deferredClickHandler;
				_deferredClickHandler = null;
			}
		}

		#endregion


		#region Connecting / Disconnecting GluePoints

		private bool ShapeHasGluePoint(Shape shape) {
			foreach (ControlPointId id in shape.GetControlPointIds(ControlPointCapabilities.Glue))
				return true;
			return false;
		}


		private void DisconnectGluePoints(IDiagramPresenter diagramPresenter) {
			foreach (Shape selectedShape in diagramPresenter.SelectedShapes) {
				foreach (ControlPointId ptId in selectedShape.GetControlPointIds(ControlPointCapabilities.Connect | ControlPointCapabilities.Glue)) {
					// disconnect GluePoints if they are moved together with their targets
					bool skip = false;
					foreach (ShapeConnectionInfo ci in selectedShape.GetConnectionInfos(ptId, null)) {
						if (ci.OwnPointId != ptId) throw new NShapeInternalException("Fatal error: Unexpected ShapeConnectionInfo was returned.");
						if (diagramPresenter.SelectedShapes.Contains(ci.OtherShape)) {
							skip = false;
							break;
						}
					}
					if (skip) continue;

					// otherwise, compare positions of the GluePoint with it's targetPoint and disconnect if they are not equal
					if (selectedShape.HasControlPointCapability(ptId, ControlPointCapabilities.Glue)) {
						Shape previewShape = FindPreviewOfShape(selectedShape);
						if (selectedShape.GetControlPointPosition(ptId) != previewShape.GetControlPointPosition(ptId)) {
							bool isConnected = false;
							foreach (ShapeConnectionInfo sci in selectedShape.GetConnectionInfos(ptId, null)) {
								if (sci.OwnPointId == ptId) {
									isConnected = true;
									break;
								} else throw new NShapeInternalException("Fatal error: Unexpected ShapeConnectionInfo was returned.");
							}
							if (isConnected) {
								ICommand cmd = new DisconnectCommand(selectedShape, ptId);
								diagramPresenter.Project.ExecuteCommand(cmd);
							}
						}
					}
				}
			}
		}


		private void ConnectGluePoints(IDiagramPresenter diagramPresenter) {
			foreach (Shape selectedShape in diagramPresenter.SelectedShapes) {
				// find selectedShapes that own GluePoints
				foreach (ControlPointId gluePointId in selectedShape.GetControlPointIds(ControlPointCapabilities.Glue)) {
					Point gluePointPos = Point.Empty;
					gluePointPos = selectedShape.GetControlPointPosition(gluePointId);

					// find selectedShapes to connect to
					foreach (Shape shape in FindVisibleShapes(diagramPresenter, gluePointPos.X, gluePointPos.Y, ControlPointCapabilities.Connect, diagramPresenter.GripSize)) {
						if (diagramPresenter.SelectedShapes.Contains(shape)) {
							// restore connections that were disconnected before
							ControlPointId targetPointId = shape.FindNearestControlPoint(gluePointPos.X, gluePointPos.Y, 0, ControlPointCapabilities.Connect);
							if (targetPointId != ControlPointId.None)
								selectedShape.Connect(gluePointId, shape, targetPointId);
						} else {
							ShapeAtCursorInfo shapeInfo = FindConnectionTarget(diagramPresenter, selectedShape, gluePointId, gluePointPos, true, true);
							if (shapeInfo.ControlPointId != ControlPointId.None) {
								ICommand cmd = new ConnectCommand(selectedShape, gluePointId, shapeInfo.Shape, shapeInfo.ControlPointId);
								diagramPresenter.Project.ExecuteCommand(cmd);
							}
							//else if (shape.ContainsPoint(gluePointPos.X, gluePointPos.Y)) {
							//   ICommand cmd = new ConnectCommand(selectedShape, gluePointId, shape, ControlPointId.Reference);
							//   display.Project.ExecuteCommand(cmd);
							//}
						}
					}
				}
			}
		}

		#endregion


		#region Moving Shapes

		// prepare drawing preview of move action
		private void PrepareMoveShapePreview(IDiagramPresenter diagramPresenter, MouseState mouseState) {
			_previewMouseState = mouseState;

#if DEBUG_DIAGNOSTICS
				Assert(diagramPresenter.SelectedShapes.Count > 0);
				Assert(!_selectedShapeAtCursorInfo.IsEmpty);
#endif
			// calculate the movement
			int distanceX = mouseState.X - ActionStartMouseState.X;
			int distanceY = mouseState.Y - ActionStartMouseState.Y;
			// calculate "Snap to Grid" offset
			_snapDeltaX = _snapDeltaY = 0;
			if (diagramPresenter.SnapToGrid && !_selectedShapeAtCursorInfo.IsEmpty) {
				FindNearestSnapPoint(diagramPresenter, _selectedShapeAtCursorInfo.Shape, distanceX, distanceY, out _snapDeltaX, out _snapDeltaY);
				distanceX += _snapDeltaX;
				distanceY += _snapDeltaY;
			}

			// Reset all shapes to start values
			ResetPreviewShapes(diagramPresenter);

			// Move (preview copies of) the selected shapes
			Rectangle shapeBounds = Rectangle.Empty;
			foreach (Shape originalShape in diagramPresenter.SelectedShapes) {
				// Get preview of the shape to move...
				Shape previewShape = FindPreviewOfShape(originalShape);
				// ...and move the preview shape to the new position
				previewShape.MoveTo(originalShape.X + distanceX, originalShape.Y + distanceY);
			}
		}


		// Apply the move action
		private bool PerformMoveShape(IDiagramPresenter diagramPresenter) {
			bool result = false;
			if (_selectedShapeAtCursorInfo.IsEmpty) {
				// This should never happen...
				Debug.Assert(!_selectedShapeAtCursorInfo.IsEmpty);
			}

			if (ActionStartMouseState.Position != CurrentMouseState.Position) {
				// calculate the movement
				int distanceX = CurrentMouseState.X - ActionStartMouseState.X;
				int distanceY = CurrentMouseState.Y - ActionStartMouseState.Y;
				//snapDeltaX = snapDeltaY = 0;
				//if (diagramPresenter.SnapToGrid)
				//   FindNearestSnapPoint(diagramPresenter, SelectedShapeAtCursorInfo.Shape, distanceX, distanceY, out snapDeltaX, out snapDeltaY, ControlPointCapabilities.All);

#if DEBUG_DIAGNOSTICS
				int dx = distanceX + _snapDeltaX;
				int dy = distanceY + _snapDeltaY;
				Debug.Assert(CurrentMouseState.Position == CurrentMouseState.Position);
				foreach (Shape selectedShape in diagramPresenter.SelectedShapes) {
					Shape previewShape = _previewShapes[selectedShape];
					Debug.Assert((previewShape.X == selectedShape.X && previewShape.Y == selectedShape.Y)
							|| (previewShape.X == selectedShape.X + dx && previewShape.Y == selectedShape.Y + dy), 
							string.Format("Preview shape '{0}' was expected at position {1} but it is at position {2}.", 
								previewShape.Type.Name, new Point(selectedShape.X, selectedShape.Y), new Point(previewShape.X, previewShape.Y)));
				}
#endif

				ICommand cmd = new MoveShapeByCommand(diagramPresenter.SelectedShapes, distanceX + _snapDeltaX, distanceY + _snapDeltaY);
				diagramPresenter.Project.ExecuteCommand(cmd);

				_snapDeltaX = _snapDeltaY = 0;
				_snapPtId = ControlPointId.None;
				result = true;
			}
			return result;
		}

		#endregion


		#region Moving ControlPoints

		// prepare drawing preview of resize action 
		private void PrepareMoveHandlePreview(IDiagramPresenter diagramPresenter, MouseState mouseState) {
			_previewMouseState = mouseState;

			InvalidateConnectionTargets(diagramPresenter, CurrentMouseState.X, CurrentMouseState.Y);

			int distanceX = mouseState.X - ActionStartMouseState.X;
			int distanceY = mouseState.Y - ActionStartMouseState.Y;

			// calculate "Snap to Grid/ControlPoint" offset
			_snapDeltaX = _snapDeltaY = 0;
			if (_selectedShapeAtCursorInfo.IsCursorAtGluePoint) {
				ControlPointId targetPtId;
				Shape targetShape = FindNearestControlPoint(diagramPresenter, _selectedShapeAtCursorInfo.Shape, _selectedShapeAtCursorInfo.ControlPointId, ControlPointCapabilities.Connect, distanceX, distanceY, out _snapDeltaX, out _snapDeltaY, out targetPtId);
			} else
				FindNearestSnapPoint(diagramPresenter, _selectedShapeAtCursorInfo.Shape, _selectedShapeAtCursorInfo.ControlPointId, distanceX, distanceY, out _snapDeltaX, out _snapDeltaY);
			distanceX += _snapDeltaX;
			distanceY += _snapDeltaY;

			// Reset all preview shapes to start values
			ResetPreviewShapes(diagramPresenter);

			// Move selected shapes
			ResizeModifiers resizeModifier = GetResizeModifier(mouseState);
			Point originalPtPos = Point.Empty;
			foreach (Shape selectedShape in diagramPresenter.SelectedShapes) {
				Shape previewShape = FindPreviewOfShape(selectedShape);
				// Perform movement
				if (previewShape.HasControlPointCapability(_selectedShapeAtCursorInfo.ControlPointId, ControlPointCapabilities.Resize))
					previewShape.MoveControlPointBy(_selectedShapeAtCursorInfo.ControlPointId, distanceX, distanceY, resizeModifier);
			}

			InvalidateConnectionTargets(diagramPresenter, mouseState.X, mouseState.Y);
		}


		// apply the resize action
		private bool PerformMoveHandle(IDiagramPresenter diagramPresenter) {
			bool result = false;
			Invalidate(diagramPresenter);

			int distanceX = CurrentMouseState.X - ActionStartMouseState.X;
			int distanceY = CurrentMouseState.Y - ActionStartMouseState.Y;

			// if the moved ControlPoint is a single GluePoint, snap to ConnectionPoints
			bool isGluePoint = false;
			if (diagramPresenter.SelectedShapes.Count == 1)
				isGluePoint = _selectedShapeAtCursorInfo.IsCursorAtGluePoint;
			// Snap to Grid or ControlPoint
			bool calcSnapDistance = true;
			ShapeAtCursorInfo targetShapeInfo = ShapeAtCursorInfo.Empty;
			if (isGluePoint) {
				Point currentPtPos = _selectedShapeAtCursorInfo.Shape.GetControlPointPosition(_selectedShapeAtCursorInfo.ControlPointId);
				Point newPtPos = Point.Empty;
				newPtPos.Offset(currentPtPos.X + distanceX, currentPtPos.Y + distanceY);
				targetShapeInfo = FindConnectionTarget(ActionDiagramPresenter, _selectedShapeAtCursorInfo.Shape, _selectedShapeAtCursorInfo.ControlPointId, newPtPos, true, true);
				if (!targetShapeInfo.IsEmpty && _selectedShapeAtCursorInfo.CanConnect(targetShapeInfo)) {
					// If there is a target shape to connect to, get the position of the target connection point 
					// and move the gluepoint exactly to this position
					calcSnapDistance = false;
					if (targetShapeInfo.ControlPointId != ControlPointId.Reference) {
						Point pt = targetShapeInfo.Shape.GetControlPointPosition(targetShapeInfo.ControlPointId);
						distanceX = pt.X - currentPtPos.X;
						distanceY = pt.Y - currentPtPos.Y;
					} else {
						// If the target point is the reference point, use the previously calculated snap distance
						// ToDo: We need a solution for calculating the nearest point on the target shape's outline
						distanceX += _snapDeltaX;
						distanceY += _snapDeltaY;
					}
				}
			}
			if (calcSnapDistance && !_selectedShapeAtCursorInfo.IsEmpty) {
				FindNearestSnapPoint(diagramPresenter, _selectedShapeAtCursorInfo.Shape, _selectedShapeAtCursorInfo.ControlPointId, distanceX, distanceY, out _snapDeltaX, out _snapDeltaY);
				distanceX += _snapDeltaX;
				distanceY += _snapDeltaY;
			}

			ResizeModifiers resizeModifier = GetResizeModifier(CurrentMouseState);
			if (isGluePoint) {
				ICommand cmd = new MoveGluePointCommand(_selectedShapeAtCursorInfo.Shape, _selectedShapeAtCursorInfo.ControlPointId, targetShapeInfo.Shape, targetShapeInfo.ControlPointId, distanceX, distanceY, resizeModifier);
				diagramPresenter.Project.ExecuteCommand(cmd);
			} else {
				ICommand cmd = new MoveControlPointCommand(ActionDiagramPresenter.SelectedShapes, _selectedShapeAtCursorInfo.ControlPointId, distanceX, distanceY, resizeModifier);
				diagramPresenter.Project.ExecuteCommand(cmd);
			}

			_snapDeltaX = _snapDeltaY = 0;
			_snapPtId = ControlPointId.None;
			result = true;

			return result;
		}

		#endregion


		#region Rotating Shapes

		private int CalcStartAngle(MouseState startMouseState, MouseState currentMouseState) {
#if DEBUG_DIAGNOSTICS
			Assert(startMouseState != MouseState.Empty);
			Assert(currentMouseState != MouseState.Empty);
#endif
			float angleRad = Geometry.Angle(startMouseState.Position, currentMouseState.Position);
			return (3600 + Geometry.RadiansToTenthsOfDegree(angleRad)) % 3600;
		}


		private int CalcSweepAngle(MouseState initMouseState, MouseState prevMouseState, MouseState newMouseState) {
#if DEBUG_DIAGNOSTICS
			Assert(initMouseState != MouseState.Empty);
			Assert(prevMouseState != MouseState.Empty);
			Assert(newMouseState != MouseState.Empty);
#endif
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
				MouseState prevMouseState = GetPreviousMouseState();
				p = prevMouseState.Position;
			}
			Debug.Assert(Geometry.IsValid(p));
			int dist = (int)Math.Round(Geometry.DistancePointPoint(p.X, p.Y, mouseState.X, mouseState.Y));
			diagramPresenter.DiagramToControl(dist, out dist);
			return (dist > diagramPresenter.MinRotateRange);
		}


		private MouseState GetPreviousMouseState() {
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


		// prepare drawing preview of rotate action
		private void PrepareRotatePreview(IDiagramPresenter diagramPresenter, MouseState mouseState) {
			_previewMouseState = mouseState;
			InvalidateAnglePreview(ActionDiagramPresenter);
			if (PendingToolActionsCount >= 1
				&& ActionStartMouseState.Position != mouseState.Position) {
				if (IsMinRotateRangeExceeded(diagramPresenter, mouseState)) {
					// calculate new angle
					MouseState initMouseState = GetPreviousMouseState();
					int startAngle, sweepAngle, prevSweepAngle;
					CalcAngle(initMouseState, ActionStartMouseState, CurrentMouseState, mouseState,
						out startAngle, out sweepAngle, out prevSweepAngle);

					// Reset all preview shapes to start values
					ResetPreviewShapes(diagramPresenter);

					// ToDo: Implement rotation around a common rotation center
					Point rotationCenter = Point.Empty;
					foreach (Shape selectedShape in diagramPresenter.SelectedShapes) {
						Shape previewShape = FindPreviewOfShape(selectedShape);
						// Get ControlPointId of the first rotate control point
						ControlPointId rotatePtId = ControlPointId.None;
						foreach (ControlPointId id in previewShape.GetControlPointIds(ControlPointCapabilities.Rotate)) {
							rotatePtId = id;
							break;
						}
						if (rotatePtId == ControlPointId.None) throw new NShapeInternalException("{0} has no rotate control point.");
						rotationCenter = previewShape.GetControlPointPosition(rotatePtId);

						//// Restore original shape's angle
						//previewShape.Rotate(-prevSweepAngle, rotationCenter.X, rotationCenter.Y);

						// Perform rotation
						previewShape.Rotate(sweepAngle, rotationCenter.X, rotationCenter.Y);
					}
				}
			}
			InvalidateAnglePreview(ActionDiagramPresenter);
		}


		// apply rotate action
		private bool PerformRotate(IDiagramPresenter diagramPresenter) {
			bool result = false;
			if (PendingToolActionsCount >= 1
				&& ActionStartMouseState.Position != CurrentMouseState.Position
				&& IsMinRotateRangeExceeded(ActionDiagramPresenter, CurrentMouseState)) {
				// Calculate rotation
				MouseState initMouseState = GetPreviousMouseState();
				int startAngle, sweepAngle;
				CalcAngle(initMouseState, ActionStartMouseState, CurrentMouseState, out startAngle, out sweepAngle);
				// Create and execute command
				ICommand cmd = new RotateShapesCommand(diagramPresenter.SelectedShapes, sweepAngle);
				diagramPresenter.Project.ExecuteCommand(cmd);
				result = true;
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
#if DEBUG_DIAGNOSTICS
			Assert(previewTargetShape != null, "Error while creating connected preview shapes.");
#endif
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
#if DEBUG_DIAGNOSTICS
							Assert(connectorCI.OtherShape.IsConnected(connectorCI.OtherPointId, previewTargetShape) == ControlPointId.None);
#endif
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

		private void SetSelectedShapeAtCursor(IDiagramPresenter diagramPresenter, int mouseX, int mouseY, int handleRadius, ControlPointCapabilities handleCapabilities) {
			// Find the shape under the cursor
			_selectedShapeAtCursorInfo.Clear();
			_selectedShapeAtCursorInfo.Shape = diagramPresenter.SelectedShapes.FindShape(mouseX, mouseY, handleCapabilities, handleRadius, null);

			// If there is a shape under the cursor, find the nearest control point and caption
			if (!_selectedShapeAtCursorInfo.IsEmpty) {
				ControlPointCapabilities capabilities;
				if (CurrentMouseState.IsKeyPressed(KeysDg.Control)) capabilities = ControlPointCapabilities.Resize /*| ControlPointCapabilities.Movable*/;
				else if (CurrentMouseState.IsKeyPressed(KeysDg.Shift)) capabilities = ControlPointCapabilities.Rotate;
				else capabilities = _gripCapabilities;

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
							else return _cursors[ToolCursor.Default];
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
#if DEBUG_DIAGNOSTICS
					Assert(!_selectedShapeAtCursorInfo.IsEmpty);
					Assert(_selectedShapeAtCursorInfo.Shape is ICaptionedShape);
#endif
					// If the cursor is outside the caption, return default cursor
					int captionIndex = ((ICaptionedShape)_selectedShapeAtCursorInfo.Shape).FindCaptionFromPoint(mouseState.X, mouseState.Y);
					if (captionIndex == _selectedShapeAtCursorInfo.CaptionIndex)
						return _cursors[ToolCursor.EditCaption];
					else return _cursors[ToolCursor.Default];

				case Action.MoveHandle:
#if DEBUG_DIAGNOSTICS
					Assert(!_selectedShapeAtCursorInfo.IsEmpty);
					Assert(_selectedShapeAtCursorInfo.IsCursorAtGrip);
#endif
					if (_selectedShapeAtCursorInfo.IsCursorAtGluePoint) {
						Shape previewShape = FindPreviewOfShape(_selectedShapeAtCursorInfo.Shape);
						Point ptPos = previewShape.GetControlPointPosition(_selectedShapeAtCursorInfo.ControlPointId);
						ShapeAtCursorInfo shapeAtCursorInfo = FindConnectionTarget(
							diagramPresenter,
							_selectedShapeAtCursorInfo.Shape,
							_selectedShapeAtCursorInfo.ControlPointId,
							ptPos,
							true,
							false);
						if (!shapeAtCursorInfo.IsEmpty && _selectedShapeAtCursorInfo.CanConnect(shapeAtCursorInfo))
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


		/// <summary>
		/// Create Previews of all shapes selected in the CurrentDisplay.
		/// These previews are connected to all the shapes the original shapes are connected to.
		/// </summary>
		private void CreatePreviewShapes(IDiagramPresenter diagramPresenter) {
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
			}
		}


		private void ResetPreviewShapes(IDiagramPresenter diagramPresenter) {
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


		private void InvalidateShapes(IDiagramPresenter diagramPresenter, IEnumerable<Shape> shapes, bool invalidateGrips) {
			foreach (Shape shape in shapes)
				DoInvalidateShape(diagramPresenter, shape, invalidateGrips);
		}


		private void DoInvalidateShape(IDiagramPresenter diagramPresenter, Shape shape, bool invalidateGrips) {
			if (shape.Parent != null)
				DoInvalidateShape(diagramPresenter, shape.Parent, false);
			else {
				shape.Invalidate();
				if (invalidateGrips)
					diagramPresenter.InvalidateGrips(shape, ControlPointCapabilities.All);
			}
		}


		private bool IsDragActionFeasible(MouseState mouseState, MouseButtonsDg button) {
			int dx = 0, dy = 0;
			if (mouseState.IsButtonDown(button) && IsToolActionPending) {
				// Check the minimum drag distance before switching to a drag action
				dx = Math.Abs(mouseState.X - ActionStartMouseState.X);
				dy = Math.Abs(mouseState.Y - ActionStartMouseState.Y);
			}
			return (dx >= MinimumDragDistance.Width || dy >= MinimumDragDistance.Height);
		}


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
				// ToDo: If there are *many* shapes selected (e.g. 10000), this check will be extremly slow...
				if (diagramPresenter.SelectedShapes.Count < 10000) {
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
			}
			return true;
		}


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

			public static bool operator ==(ConnectionInfoBuffer x, ConnectionInfoBuffer y) { return (x.connectionInfo == y.connectionInfo && x.shape == y.shape); }

			public static bool operator !=(ConnectionInfoBuffer x, ConnectionInfoBuffer y) { return !(x == y); }

			public Shape shape;

			public ShapeConnectionInfo connectionInfo;

			/// <override></override>
			public override bool Equals(object obj) { return obj is ConnectionInfoBuffer && this == (ConnectionInfoBuffer)obj; }

			/// <ToBeCompleted></ToBeCompleted>
			public bool Equals(ConnectionInfoBuffer other) {
				return other == this;
			}

			/// <override></override>
			public override int GetHashCode() { return base.GetHashCode(); }

			static ConnectionInfoBuffer() {
				Empty.shape = null;
				Empty.connectionInfo = ShapeConnectionInfo.Empty;
			}
		}

		#endregion


		#region Fields

		// --- Description of the tool ---
		private static Dictionary<ToolCursor, int> _cursors;
		//
		private OverlappingShapesAction _overlappingShapesSelectionMode = OverlappingShapesAction.Cycle;
		private bool _enableQuickRotate = false;
		private ControlPointCapabilities _gripCapabilities = ControlPointCapabilities.Resize | ControlPointCapabilities.Rotate /*| ControlPointCapabilities.Movable*/;

		// --- State after the last ProcessMouseEvent ---
		// selected shape under the mouse cursor, being highlighted in the next drawing
		private ShapeAtCursorInfo _selectedShapeAtCursorInfo;
		// Rectangle that represents the transformed selection area in control coordinates
		private Rectangle _frameRect;
		// Stores the distance the SelectedShape was moved on X-axis for snapping the nearest gridpoint
		private int _snapDeltaX;
		// Stores the distance the SelectedShape was moved on Y-axis for snapping the nearest gridpoint
		private int _snapDeltaY;
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
		// 
		// Specifies whether the currently calculated preview has been drawn. 
		// As long as the calculated preview is not drawn yet, the new preview will not be calculated.
		private MouseState _previewMouseState = MouseState.Empty;

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