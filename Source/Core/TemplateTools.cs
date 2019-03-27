/******************************************************************************
  Copyright 2009-2017 dataweb GmbH
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

using Dataweb.NShape.Advanced;
using Dataweb.NShape.Commands;
using Dataweb.NShape.Controllers;


namespace Dataweb.NShape {


	/// <summary>
	/// Lets the user create a templated shape.
	/// </summary>
	public abstract class TemplateTool : Tool {

		/// <ToBeCompleted></ToBeCompleted>
		public Template Template {
			get { return _template; }
		}


		/// <override></override>
		public override void Dispose() {
			// Do not dispose the Template - it has to be disposed by the cache
			base.Dispose();
		}


		/// <override></override>
		public override void RefreshIcons() {
			using (Shape clone = Template.Shape.Clone()) {
				clone.DrawThumbnail(base.LargeIcon, margin, transparentColor);
				base.LargeIcon.MakeTransparent(transparentColor);
			}
			using (Shape clone = Template.Shape.Clone()) {
				clone.DrawThumbnail(base.SmallIcon, margin, transparentColor);
				base.SmallIcon.MakeTransparent(transparentColor);
			}
			ClearPreview();
			Title = Template.Title;
		}


		/// <override></override>
		protected TemplateTool(Template template, string category)
			: base(category) {
			if (template == null) throw new ArgumentNullException("template");
			this._template = template;
			Title = template.Title;
			ToolTipText = string.Format(Properties.Resources.CaptionFmt_TemplateTool_Tooltip0, Title);
			if (!string.IsNullOrEmpty(template.Shape.Type.Description))
				ToolTipText += Environment.NewLine + template.Shape.Type.Description;
			RefreshIcons();
		}


		/// <override></override>
		protected TemplateTool(Template template)
			: this(template, (template != null) ? template.Shape.Type.DefaultCategoryTitle : null) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected Shape PreviewShape {
			get { return _previewShape; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected virtual void CreatePreview(IDiagramPresenter diagramPresenter) {
			using (Shape s = Template.CreateShape())
				_previewShape = Template.Shape.Type.CreatePreviewInstance(s);
			_previewShape.DisplayService = diagramPresenter.DisplayService;
			_previewShape.Invalidate();
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected virtual void ClearPreview() {
			if (_previewShape != null)
				DeletePreviewShape(ref _previewShape);
		}


		#region Fields

		private Template _template;
		private Shape _previewShape;

		#endregion
	}


	/// <summary>
	/// Lets the user create a shape based on a point sequence.
	/// </summary>
	public class LinearShapeCreationTool : TemplateTool {

		/// <ToBeCompleted></ToBeCompleted>
		public LinearShapeCreationTool(Template template)
			: this(template, null) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public LinearShapeCreationTool(Template template, string category)
			: base(template, category) {
			if (!(template.Shape is ILinearShape))
				throw new NShapeException("The template's shape does not implement {0}.", typeof(ILinearShape).Name);
			ToolTipText += string.Format(Properties.Resources.CaptionFmt_LinearShapeCreationTool_Tooltip, Environment.NewLine);
		}


		#region IDisposable Interface

		/// <override></override>
		public override void Dispose() {
			base.Dispose();
		}

		#endregion


		/// <override></override>
		public override IEnumerable<MenuItemDef> GetMenuItemDefs(IDiagramPresenter diagramPresenter) {
			if (diagramPresenter == null) throw new ArgumentNullException("diagramPresenter");
			yield break;
		}


		/// <override></override>
		public override bool ProcessMouseEvent(IDiagramPresenter diagramPresenter, MouseEventArgsDg e) {
			if (diagramPresenter == null) throw new ArgumentNullException("diagramPresenter");
			bool result = false;

			MouseState newMouseState = MouseState.Empty;
			newMouseState.Buttons = e.Buttons;
			newMouseState.Modifiers = e.Modifiers;
			diagramPresenter.ControlToDiagram(e.Position, out newMouseState.Position);

			diagramPresenter.SuspendUpdate();
			try {
				switch (e.EventType) {
					case MouseEventType.MouseMove:
						if (CurrentMouseState.Position != newMouseState.Position)
							ProcessMouseMove(diagramPresenter, newMouseState);
						break;
					case MouseEventType.MouseDown:
						// MouseDown starts drag-based actions
						// ToDo: Implement these features: Adding Segments to existing Lines, Move existing Lines and their ControlPoints
						if (e.Clicks > 1) result = ProcessDoubleClick(diagramPresenter, newMouseState);
						else result = ProcessMouseClick(diagramPresenter, newMouseState);
						break;

					case MouseEventType.MouseUp:
						// MouseUp finishes drag-actions. Click-based actions are handled by the MouseClick event
						// ToDo: Implement these features: Adding Segments to existing Lines, Move existing Lines and their ControlPoints
						break;

					default: throw new NShapeUnsupportedValueException(e.EventType);
				}
			} finally { diagramPresenter.ResumeUpdate(); }
			base.ProcessMouseEvent(diagramPresenter, e);
			return result;
		}


		/// <override></override>
		public override bool ProcessKeyEvent(IDiagramPresenter diagramPresenter, KeyEventArgsDg e) {
			return base.ProcessKeyEvent(diagramPresenter, e);
		}


		/// <override></override>
		public override void EnterDisplay(IDiagramPresenter diagramPresenter) {
			Invalidate(diagramPresenter);
		}


		/// <override></override>
		public override void LeaveDisplay(IDiagramPresenter diagramPresenter) {
			Invalidate(diagramPresenter);
		}


		/// <override></override>
		public override void Invalidate(IDiagramPresenter diagramPresenter) {
			if (diagramPresenter == null) throw new ArgumentNullException("diagramPresenter");
			if (PreviewShape != null) {
				diagramPresenter.InvalidateGrips(PreviewShape, ControlPointCapabilities.All);
				Point p = PreviewShape.GetControlPointPosition(ControlPointId.LastVertex);
				InvalidateConnectionTargets(diagramPresenter, p.X, p.Y);
			} else InvalidateConnectionTargets(diagramPresenter, CurrentMouseState.X, CurrentMouseState.Y);
		}


		/// <override></override>
		public override void Draw(IDiagramPresenter diagramPresenter) {
			if (diagramPresenter == null) throw new ArgumentNullException("diagramPresenter");
			// Draw preview shape
			if (PreviewShape != null) {
				// Draw preview shape and its ControlPoints
				diagramPresenter.DrawShape(PreviewShape);
				diagramPresenter.ResetTransformation();
				try {
					foreach (ControlPointId pointId in PreviewShape.GetControlPointIds(ControlPointCapabilities.Glue | ControlPointCapabilities.Resize))
						diagramPresenter.DrawResizeGrip(IndicatorDrawMode.Normal, PreviewShape, pointId);
				} finally { diagramPresenter.RestoreTransformation(); }
			}

			// Highlight ConnectionPoints in range
			if (!CurrentMouseState.IsEmpty) {
				if (Template.Shape.HasControlPointCapability(ControlPointId.LastVertex, ControlPointCapabilities.Glue)) {
					if (PreviewShape == null)
						DrawConnectionTargets(diagramPresenter, Template.Shape, ControlPointId.LastVertex, CurrentMouseState.Position);
					else {
						Point gluePtPos = PreviewShape.GetControlPointPosition(ControlPointId.LastVertex);
						DrawConnectionTargets(diagramPresenter, PreviewShape, ControlPointId.LastVertex, gluePtPos);
					}
				}
			}
		}


		/// <override></override>
		protected override void StartToolAction(IDiagramPresenter diagramPresenter, int action, MouseState mouseState, bool wantAutoScroll) {
			Debug.Print("StartToolAction");
			base.StartToolAction(diagramPresenter, action, mouseState, wantAutoScroll);
		}


		/// <override></override>
		protected override void EndToolAction() {
			Debug.Print("EndToolAction");
			base.EndToolAction();
			ClearPreview();
			_modifiedLinearShape = null;
			_pointAtCursor = ControlPointId.None;
			_lastInsertedPointId = ControlPointId.None;
		}


		/// <override></override>
		protected override void CancelCore() {
			// Create the line until the last point that was created manually.
			// This feature only makes sense if an additional ControlPoint was created (other than the default points)
			ILinearShape templateShape = Template.Shape as ILinearShape;
			ILinearShape previewShape = PreviewShape as ILinearShape;
			if (IsToolActionPending && templateShape != null && previewShape != null
				&& previewShape.VertexCount > templateShape.VertexCount)
				FinishLine(ActionDiagramPresenter, CurrentMouseState, true);
		}


		static LinearShapeCreationTool() {
			_cursors = new Dictionary<ToolCursor, int>(6);
			_cursors.Add(ToolCursor.Default, CursorProvider.DefaultCursorID);
			_cursors.Add(ToolCursor.Pen, CursorProvider.RegisterCursor(Properties.Resources.PenCursor));
			_cursors.Add(ToolCursor.ExtendLine, CursorProvider.RegisterCursor(Properties.Resources.PenPlusCursor));
			_cursors.Add(ToolCursor.Connect, CursorProvider.RegisterCursor(Properties.Resources.HandCursor));
			_cursors.Add(ToolCursor.Disconnect, CursorProvider.RegisterCursor(Properties.Resources.HandCursor));
			_cursors.Add(ToolCursor.NotAllowed, CursorProvider.RegisterCursor(Properties.Resources.ActionDeniedCursor));
			// ToDo: Create better cursors for connecting/disconnecting
		}


		private bool ProcessMouseMove(IDiagramPresenter diagramPresenter, MouseState mouseState) {
			bool result = false;
			ShapeAtCursorInfo shapeAtCursorInfo = ShapeAtCursorInfo.Empty;
			if (_pointAtCursor != ControlPointId.None && ((_modifiedLinearShape as Shape) != null || PreviewShape != null))
				shapeAtCursorInfo = FindConnectionTarget(diagramPresenter, (_modifiedLinearShape as Shape) ?? PreviewShape, _pointAtCursor, mouseState.Position, false, true);
			else shapeAtCursorInfo = FindShapeAtCursor(diagramPresenter, mouseState.X, mouseState.Y, ControlPointCapabilities.Connect | ControlPointCapabilities.Glue, diagramPresenter.ZoomedGripSize, false);

			// set cursor depending on the object under the mouse cursor
			int currentCursorId = DetermineCursor(diagramPresenter, shapeAtCursorInfo.Shape, shapeAtCursorInfo.ControlPointId);
			if (CurrentAction == Action.None)
				diagramPresenter.SetCursor(currentCursorId);
			else ActionDiagramPresenter.SetCursor(currentCursorId);

			switch (CurrentAction) {
				case Action.None:
					Invalidate(diagramPresenter);
					break;

				case Action.CreateLine:
				case Action.ExtendLine:
					Invalidate(ActionDiagramPresenter);

					// Check for connectionpoints wihtin the snapArea
					ResizeModifiers resizeModifier = GetResizeModifier(mouseState);
					if (!shapeAtCursorInfo.IsEmpty) {
						Point p = Point.Empty;
						if (shapeAtCursorInfo.IsCursorAtGrip)
							p = shapeAtCursorInfo.Shape.GetControlPointPosition(shapeAtCursorInfo.ControlPointId);
						else p = mouseState.Position;
#if DEBUG_DIAGNOSTICS
						Assert(PreviewShape != null);
#endif
						if (PreviewShape != null)
							PreviewShape.MoveControlPointTo(_pointAtCursor, p.X, p.Y, resizeModifier);
					} else {
						int snapDeltaX = 0, snapDeltaY = 0;
						if (diagramPresenter.SnapToGrid)
							FindNearestSnapPoint(diagramPresenter, mouseState.X, mouseState.Y, out snapDeltaX, out snapDeltaY);
#if DEBUG_DIAGNOSTICS
						Assert(PreviewShape != null);
#endif
						if (PreviewShape != null)
							PreviewShape.MoveControlPointTo(_pointAtCursor, mouseState.X + snapDeltaX, mouseState.Y + snapDeltaY, resizeModifier);
					}
					Invalidate(ActionDiagramPresenter);
					break;

				default: throw new NShapeUnsupportedValueException(CurrentAction);
			}
			return result;
		}


		private bool ProcessMouseClick(IDiagramPresenter diagramPresenter, MouseState mouseState) {
			Debug.Print("ProcessMouseClick");
			bool result = false;
			switch (CurrentAction) {
				case Action.None:
					if (mouseState.IsButtonDown(MouseButtonsDg.Left)) {
						if (diagramPresenter.SelectedShapes.Count > 0)
							diagramPresenter.UnselectAll();

						ShapeAtCursorInfo targetShapeInfo = FindShapeAtCursor(diagramPresenter, mouseState.X, mouseState.Y, ControlPointCapabilities.Glue, diagramPresenter.ZoomedGripSize, false);
						if (IsExtendLineFeasible(CurrentAction, targetShapeInfo.Shape, targetShapeInfo.ControlPointId)) {
							if (diagramPresenter.Project.SecurityManager.IsGranted(Permission.Layout, targetShapeInfo.Shape)) {
								ExtendLine(diagramPresenter, mouseState, targetShapeInfo);
								result = true;
							}
						} else {
							// If no other ToolAction is in Progress (e.g. drawing a line or moving a point),
							// a normal MouseClick starts a new line in Point-By-Point mode
							if (diagramPresenter.Project.SecurityManager.IsGranted(Permission.Insert, Template.Shape)) {
								CreateLine(diagramPresenter, mouseState);
								result = true;
							}
						}
					} else if (mouseState.IsButtonDown(MouseButtonsDg.Right)) {
						Cancel();
						result = true;
					}
					break;

				case Action.CreateLine:
				case Action.ExtendLine:
					if (mouseState.IsButtonDown(MouseButtonsDg.Left)) {
						Invalidate(ActionDiagramPresenter);
						bool doFinishLine = false;
						// If the line has reached the MaxVertexCount limit, create it
						if (PreviewLinearShape.VertexCount >= PreviewLinearShape.MaxVertexCount)
							doFinishLine = true;
						else {
							// Check if it has to be connected to a shape or connection point
							ShapeAtCursorInfo shapeAtCursorInfo = base.FindShapeAtCursor(ActionDiagramPresenter, mouseState.X, mouseState.Y, ControlPointCapabilities.Connect, diagramPresenter.ZoomedGripSize, false);
							if (!shapeAtCursorInfo.IsEmpty && !shapeAtCursorInfo.IsCursorAtGluePoint)
								doFinishLine = true;
							else AddNewPoint(ActionDiagramPresenter, mouseState);
						}
						// Create line if necessary
						if (doFinishLine) {
							if (CurrentAction == Action.CreateLine)
								FinishLine(ActionDiagramPresenter, mouseState, false);
							else FinishExtendLine(ActionDiagramPresenter, mouseState, false);

							while (IsToolActionPending)
								EndToolAction();
							OnToolExecuted(ExecutedEventArgs);
						}
					} else if (mouseState.IsButtonDown(MouseButtonsDg.Right)) {
#if DEBUG_DIAGNOSTICS
						Assert(PreviewShape != null);
#endif
						// When creating a line, the new line has to have more than the minimum number of 
						// vertices because the last vertex (sticking to the mouse cursor) will not be created.
						if (CurrentAction == Action.CreateLine) {
							if (PreviewLinearShape.VertexCount <= PreviewLinearShape.MinVertexCount)
								Cancel();
							else FinishLine(ActionDiagramPresenter, mouseState, true);
						} else {
							// When extending a line, the new line has to have more than the minimum number of 
							// vertices and more than the original line because the last vertex will not be created.
							if (PreviewLinearShape.VertexCount <= PreviewLinearShape.MinVertexCount
								|| PreviewLinearShape.VertexCount - 1 == _modifiedLinearShape.VertexCount)
								Cancel();
							else FinishExtendLine(ActionDiagramPresenter, mouseState, true);
						}

						while (IsToolActionPending)
							EndToolAction();
						OnToolExecuted(ExecutedEventArgs);
					}
					result = true;
					break;

				default: throw new NShapeUnsupportedValueException(CurrentAction);
			}
			Invalidate(diagramPresenter);
			return result;
		}


		private bool ProcessDoubleClick(IDiagramPresenter diagramPresenter, MouseState mouseState) {
			Debug.Print("ProcessDoubleClick");
			bool result = false;
			if (IsToolActionPending) {
#if DEBUG_DIAGNOSTICS
				Assert(PreviewShape != null);
#endif
				if (CurrentAction == Action.CreateLine)
					FinishLine(ActionDiagramPresenter, mouseState, true);
				else if (CurrentAction == Action.ExtendLine)
					FinishExtendLine(ActionDiagramPresenter, mouseState, true);

				while (IsToolActionPending)
					EndToolAction();

				OnToolExecuted(ExecutedEventArgs);
				result = true;
			}
			return result;
		}


		private ILinearShape PreviewLinearShape {
			get { return (ILinearShape)PreviewShape; }
		}


		private Action CurrentAction {
			get {
				if (IsToolActionPending)
					return (Action)CurrentToolAction.Action;
				else return Action.None;
			}
		}


		/// <summary>
		/// Creates a new preview line shape
		/// </summary>
		private void CreateLine(IDiagramPresenter diagramPresenter, MouseState mouseState) {
			// Try to find a connection target
			ShapeAtCursorInfo targetShapeInfo = ShapeAtCursorInfo.Empty;
			if (diagramPresenter.Project.SecurityManager.IsGranted(Permission.Connect, Template.Shape))
				targetShapeInfo = FindConnectionTargetFromPosition(diagramPresenter, mouseState.X, mouseState.Y, false, true);

			int snapDeltaX = 0, snapDeltaY = 0;
			if (diagramPresenter.SnapToGrid) {
				if (targetShapeInfo.IsEmpty || targetShapeInfo.ControlPointId == ControlPointId.Reference)
					FindNearestSnapPoint(diagramPresenter, mouseState.X, mouseState.Y, out snapDeltaX, out snapDeltaY);
				else {
					Point p = targetShapeInfo.Shape.GetControlPointPosition(targetShapeInfo.ControlPointId);
					snapDeltaX = p.X - mouseState.X;
					snapDeltaY = p.Y - mouseState.Y;
				}
			}

			// set line's start coordinates
			Point start = Point.Empty;
			if (!targetShapeInfo.IsEmpty) {
				if (targetShapeInfo.ControlPointId == ControlPointId.Reference) {
					// ToDo: Get nearest point on line
					start = mouseState.Position;
					start.Offset(snapDeltaX, snapDeltaY);
				} else
					start = targetShapeInfo.Shape.GetControlPointPosition(targetShapeInfo.ControlPointId);
			} else {
				start = mouseState.Position;
				start.Offset(snapDeltaX, snapDeltaY);
			}
			// Start ToolAction
			StartToolAction(diagramPresenter, (int)Action.CreateLine, mouseState, true);

			// Create new preview shape
			CreatePreview(diagramPresenter);
			PreviewShape.MoveControlPointTo(ControlPointId.FirstVertex, start.X, start.Y, ResizeModifiers.None);
			PreviewShape.MoveControlPointTo(ControlPointId.LastVertex, mouseState.X, mouseState.Y, ResizeModifiers.None);
			// Connect to target shape if possible
			if (targetShapeInfo.IsCursorAtConnectionPoint) {
				if (CanActiveShapeConnectTo(PreviewShape, ControlPointId.FirstVertex, targetShapeInfo.Shape, targetShapeInfo.ControlPointId))
					PreviewShape.Connect(ControlPointId.FirstVertex, targetShapeInfo.Shape, targetShapeInfo.ControlPointId);
			}
			_lastInsertedPointId = ControlPointId.FirstVertex;
			_pointAtCursor = ControlPointId.LastVertex;
		}


		/// <summary>
		/// Creates a new preview line shape
		/// </summary>
		private void ExtendLine(IDiagramPresenter diagramPresenter, MouseState mouseState, ShapeAtCursorInfo targetShapeInfo) {
			// Try to find a connection target
			if (!targetShapeInfo.IsEmpty && targetShapeInfo.ControlPointId != ControlPointId.None) {
				// Start ToolAction
				StartToolAction(diagramPresenter, (int)Action.ExtendLine, mouseState, true);
				_modifiedLinearShape = (ILinearShape)targetShapeInfo.Shape;

				// create new preview shape
				CreatePreview(diagramPresenter);
				PreviewShape.CopyFrom(targetShapeInfo.Shape);	// Template will be copied but this is not really necessary as all styles will be overwritten with preview styles
				PreviewShape.MakePreview(diagramPresenter.Project.Design);

				_pointAtCursor = targetShapeInfo.ControlPointId;
				Point pointPos = targetShapeInfo.Shape.GetControlPointPosition(_pointAtCursor);
				if (_pointAtCursor == ControlPointId.FirstVertex) {
					ControlPointId insertId = PreviewLinearShape.GetNextVertexId(_pointAtCursor);
					_lastInsertedPointId = PreviewLinearShape.InsertVertex(insertId, pointPos.X, pointPos.Y);
				} else _lastInsertedPointId = PreviewLinearShape.InsertVertex(_pointAtCursor, pointPos.X, pointPos.Y);

				ResizeModifiers resizeModifier = GetResizeModifier(mouseState);
				PreviewShape.MoveControlPointTo(_pointAtCursor, mouseState.X, mouseState.Y, resizeModifier);
			}
		}


		/// <summary>
		/// Inserts a new point into the current preview line before the end point (that is sticking to the mouse cursor).
		/// </summary>
		private void AddNewPoint(IDiagramPresenter diagramPresenter, MouseState mouseState) {
#if DEBUG_DIAGNOSTICS
			Assert(PreviewLinearShape != null);
#endif
			if (PreviewLinearShape.VertexCount < PreviewLinearShape.MaxVertexCount) {
				ControlPointId existingPointId = ControlPointId.None;
				Point pointPos = PreviewShape.GetControlPointPosition(_pointAtCursor);
				foreach (ControlPointId ptId in PreviewShape.GetControlPointIds(ControlPointCapabilities.All)) {
					if (ptId == ControlPointId.Reference) continue;
					if (ptId == _pointAtCursor) continue;
					Point p = PreviewShape.GetControlPointPosition(ptId);
					if (p == pointPos && ptId != ControlPointId.Reference) {
						existingPointId = ptId;
						break;
					}
				}
				if (existingPointId == ControlPointId.None) {
					//lastInsertedPointId = PreviewLinearShape.InsertVertex(ControlPointId.LastVertex, pointPos.X, pointPos.Y);
					if (_pointAtCursor == ControlPointId.FirstVertex) {
						ControlPointId insertBeforeId = PreviewLinearShape.GetNextVertexId(_pointAtCursor);
						_lastInsertedPointId = PreviewLinearShape.InsertVertex(insertBeforeId, pointPos.X, pointPos.Y);
					} else
						_lastInsertedPointId = PreviewLinearShape.InsertVertex(_pointAtCursor, pointPos.X, pointPos.Y);
				}
			} else throw new InvalidOperationException(string.Format(Properties.Resources.MessageFmt_MaximumNumberOfVerticexReached0, PreviewLinearShape.MaxVertexCount));
		}


		/// <summary>
		/// Creates a new LinearShape and inserts it into the diagram of the CurrentDisplay by executing a Command.
		/// </summary>
		private void FinishLine(IDiagramPresenter diagramPresenter, MouseState mouseState, bool ignorePointAtMouse) {
#if DEBUG_DIAGNOSTICS
			Assert(PreviewShape != null);
#endif
			// Create a new shape from the template
			Shape newShape = Template.CreateShape();
			// Copy points from the PreviewShape to the new shape 
			// The current EndPoint of the preview (sticking to the mouse cursor) will be discarded
			foreach (ControlPointId pointId in PreviewShape.GetControlPointIds(ControlPointCapabilities.Resize)) {
				Point p = PreviewShape.GetControlPointPosition(pointId);
				switch (pointId) {
					case ControlPointId.Reference:
						// Skip ReferencePoint as it is not a physically existing vertex (identically to FirstVertex)
						continue;
					case ControlPointId.LastVertex:
						if (ignorePointAtMouse) continue;
						newShape.MoveControlPointTo(ControlPointId.LastVertex, p.X, p.Y, ResizeModifiers.None);
						break;
					case ControlPointId.FirstVertex:
						newShape.MoveControlPointTo(ControlPointId.FirstVertex, p.X, p.Y, ResizeModifiers.None);
						break;
					default:
						// Treat the last inserted Point as EndPoint
						if ((ignorePointAtMouse && pointId == _lastInsertedPointId) || ((ILinearShape)newShape).VertexCount == ((ILinearShape)newShape).MaxVertexCount)
							newShape.MoveControlPointTo(ControlPointId.LastVertex, p.X, p.Y, ResizeModifiers.None);
						else ((ILinearShape)newShape).InsertVertex(ControlPointId.LastVertex, p.X, p.Y);
						break;
				}
			}

			// Insert the new linear shape
			ActionDiagramPresenter.InsertShape(newShape);

			// Create an aggregated command which performs connecting the new shape to other shapes in one step
			AggregatedCommand aggregatedCommand = null;
			// Create connections
			foreach (ControlPointId gluePointId in newShape.GetControlPointIds(ControlPointCapabilities.Glue)) {
				ShapeConnectionInfo sci = PreviewShape.GetConnectionInfo(gluePointId, null);
				if (!sci.IsEmpty) {
					if (aggregatedCommand == null) aggregatedCommand = new AggregatedCommand();
					aggregatedCommand.Add(new ConnectCommand(newShape, gluePointId, sci.OtherShape, sci.OtherPointId));
				} else {
					// Create connection for the last vertex
					Point gluePtPos = newShape.GetControlPointPosition(gluePointId);
					ShapeAtCursorInfo targetInfo = FindConnectionTarget(ActionDiagramPresenter, newShape, ControlPointId.LastVertex, gluePtPos, false, true);
					if (!targetInfo.IsEmpty && !targetInfo.IsCursorAtGluePoint && targetInfo.ControlPointId != ControlPointId.None
						&& CanActiveShapeConnectTo(newShape, gluePointId, targetInfo.Shape, targetInfo.ControlPointId)) {
						if (aggregatedCommand == null) aggregatedCommand = new AggregatedCommand();
						aggregatedCommand.Add(new ConnectCommand(newShape, gluePointId, targetInfo.Shape, targetInfo.ControlPointId));
					}
				}
			}
			// execute command and insert it into the history
			if (aggregatedCommand != null)
				ActionDiagramPresenter.Project.ExecuteCommand(aggregatedCommand);
		}


		/// <summary>
		/// Creates a new LinearShape and inserts it into the diagram of the CurrentDisplay by executing a Command.
		/// </summary>
		private void FinishExtendLine(IDiagramPresenter diagramPresenter, MouseState mouseState, bool ignorePointAtMouse) {
#if DEBUG_DIAGNOSTICS
			Assert(PreviewShape != null);
			Assert(_modifiedLinearShape != null);
#endif
			Shape modifiedShape = (Shape)_modifiedLinearShape;

			// Copy points from the PreviewShape to the new shape 
			// Start at the opposite point of the point at mouse cursor and skip all existing points
			ControlPointId pointId, endPointId;
			bool firstToLast;
			if (_pointAtCursor == ControlPointId.FirstVertex) {
				pointId = ControlPointId.LastVertex;
				endPointId = ignorePointAtMouse ? _lastInsertedPointId : (ControlPointId)ControlPointId.FirstVertex;
				firstToLast = false;
			} else {
				pointId = ControlPointId.FirstVertex;
				endPointId = ignorePointAtMouse ? _lastInsertedPointId : (ControlPointId)ControlPointId.LastVertex;
				firstToLast = true;
			}

			// Create an aggregated command which performs connecting the new shape to other shapes in one step
			AggregatedCommand aggregatedCommand = null;

			// Process all point id's
			do {
				ControlPointId nextPointId = GetNextResizePointId(PreviewLinearShape, pointId, firstToLast);
				ControlPointId nextOrigPtId = GetNextResizePointId(_modifiedLinearShape, pointId, firstToLast);
				if (nextPointId != nextOrigPtId && nextPointId != endPointId) {
					// If the next point id of the preview does not equal the original shape's point id,
					// we have to create it...
					Point p = PreviewShape.GetControlPointPosition(nextPointId);
					ControlPointId beforePointId = (firstToLast) ? (ControlPointId)ControlPointId.LastVertex : pointId;
					if (aggregatedCommand == null) aggregatedCommand = new AggregatedCommand();
					aggregatedCommand.Add(new InsertVertexCommand(modifiedShape, beforePointId, p.X, p.Y));
				}
				pointId = nextPointId;
			} while (pointId != endPointId);
			// Set last point's position
			ControlPointId lastPtId = firstToLast ? ControlPointId.LastVertex : ControlPointId.FirstVertex;
			Point currPos = modifiedShape.GetControlPointPosition(lastPtId);
			Point newPos = PreviewShape.GetControlPointPosition(endPointId);
#if DEBUG_DIAGNOSTICS
			Assert(aggregatedCommand != null);
#endif
			aggregatedCommand.Add(new MoveControlPointCommand(modifiedShape, lastPtId, newPos.X - currPos.X, newPos.Y - currPos.Y, ResizeModifiers.None));

			// Create connection for the last vertex
			ShapeAtCursorInfo targetInfo = FindConnectionTarget(ActionDiagramPresenter, modifiedShape, lastPtId, newPos, false, true);
			if (!targetInfo.IsEmpty &&
				!targetInfo.IsCursorAtGluePoint &&
				targetInfo.ControlPointId != ControlPointId.None) {
				if (aggregatedCommand == null) aggregatedCommand = new AggregatedCommand();
				aggregatedCommand.Add(new ConnectCommand(modifiedShape, lastPtId, targetInfo.Shape, targetInfo.ControlPointId));
			}

			// Execute command and insert it into the history
			if (aggregatedCommand != null)
				ActionDiagramPresenter.Project.ExecuteCommand(aggregatedCommand);
		}


		private ControlPointId GetNextResizePointId(ILinearShape lineShape, ControlPointId currentPointId, bool firstToLast) {
			if (firstToLast) return lineShape.GetNextVertexId(currentPointId);
			else return lineShape.GetPreviousVertexId(currentPointId);
		}


		/// <summary>
		/// Set the cursor for the current action
		/// </summary>
		private int DetermineCursor(IDiagramPresenter diagramPresenter, Shape shape, ControlPointId pointId) {
			switch (CurrentAction) {
				case Action.None:
					if (IsExtendLineFeasible(CurrentAction, shape, pointId)) {
						if (diagramPresenter.Project.SecurityManager.IsGranted(Permission.Layout, shape))
							return _cursors[ToolCursor.ExtendLine];
						else return _cursors[ToolCursor.NotAllowed];
					} else if (diagramPresenter.Project.SecurityManager.IsGranted(Permission.Insert, Template.Shape)) {
						if (shape != null && pointId != ControlPointId.None
							&& IsConnectingFeasible(CurrentAction, shape, pointId))
							return _cursors[ToolCursor.Connect];
						else return _cursors[ToolCursor.Pen];
					} else return _cursors[ToolCursor.NotAllowed];

				case Action.CreateLine:
					if (diagramPresenter.Project.SecurityManager.IsGranted(Permission.Insert, Template.Shape)) {
						if (shape != null && pointId != ControlPointId.None
							&& IsConnectingFeasible(CurrentAction, shape, pointId))
							return _cursors[ToolCursor.Connect];
						else return _cursors[ToolCursor.Pen];
					} else return _cursors[ToolCursor.NotAllowed];

				case Action.ExtendLine:
					if (diagramPresenter.Project.SecurityManager.IsGranted(Permission.Layout, shape ?? (Shape)_modifiedLinearShape)) {
						if (IsConnectingFeasible(CurrentAction, shape, pointId))
							return _cursors[ToolCursor.Connect];
						else return _cursors[ToolCursor.ExtendLine];
					} else return _cursors[ToolCursor.NotAllowed];

				default: throw new NShapeUnsupportedValueException(CurrentAction);
			}
		}


		private bool IsExtendLineFeasible(Action action, Shape shape, ControlPointId pointId) {
			if (action != Action.None && action != Action.ExtendLine) return false;
			if ((shape as ILinearShape) == null) return false;
			if (pointId == ControlPointId.None) return false;
			if (shape.Type != Template.Shape.Type) return false;
			if (((ILinearShape)shape).VertexCount >= ((ILinearShape)shape).MaxVertexCount) return false;
			if (!shape.HasControlPointCapability(pointId, ControlPointCapabilities.Glue)) return false;
			if (shape.IsConnected(pointId, null) != ControlPointId.None) return false;
			return true;
		}


		private bool IsConnectingFeasible(Action action, Shape shape, ControlPointId pointId) {
			if (shape != null && pointId != ControlPointId.None
				&& Template.Shape.HasControlPointCapability(ControlPointId.LastVertex, ControlPointCapabilities.Glue))
				return (Template.Shape.CanConnect(ControlPointId.LastVertex, shape, pointId)
					&& shape.CanConnect(pointId, Template.Shape, ControlPointId.LastVertex));
			else return false;
		}


		private enum Action { None, ExtendLine, CreateLine }


		private enum ToolCursor {
			Default,
			NotAllowed,
			ExtendLine,
			Pen,
			Connect,
			Disconnect
		}


		#region Fields

		// Definition of the tool
		private static Dictionary<ToolCursor, int> _cursors;

		// Tool's state definition
		// stores the last inserted Point (and its coordinates), which will become the EndPoint when the CurrentTool is cancelled
		private ControlPointId _pointAtCursor;
		private ControlPointId _lastInsertedPointId;
		// Stores the currently modified ILinearShape. 
		// This could be a new shape created from the template but also an existing line that is extended with new points.
		private ILinearShape _modifiedLinearShape = null;

		#endregion
	}


	/// <summary>
	/// Lets the user place a new shape on the diagram.
	/// </summary>
	public class PlanarShapeCreationTool : TemplateTool {

		/// <ToBeCompleted></ToBeCompleted>
		public PlanarShapeCreationTool(Template template)
			: base(template) {
			Construct(template);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public PlanarShapeCreationTool(Template template, string category)
			: base(template, category) {
			Construct(template);
		}


		/// <override></override>
		public override bool ProcessMouseEvent(IDiagramPresenter diagramPresenter, MouseEventArgsDg e) {
			if (diagramPresenter == null) throw new ArgumentNullException("diagramPresenter");
			bool result = false;

			MouseState newMouseState = MouseState.Empty;
			newMouseState.Buttons = e.Buttons;
			newMouseState.Modifiers = e.Modifiers;

			// Return if action is not allowed
			if (!diagramPresenter.Project.SecurityManager.IsGranted(Permission.Insert, Template.Shape))
				return result;

			diagramPresenter.ControlToDiagram(e.Position, out newMouseState.Position);

			diagramPresenter.SuspendUpdate();
			try {
				switch (e.EventType) {
					case MouseEventType.MouseMove:
						if (newMouseState.Position != CurrentMouseState.Position) {
							// If no Preview exists, create a new one by starting a new ToolAction
							if (!IsToolActionPending)
								StartToolAction(diagramPresenter, (int)Action.Create, newMouseState, false);

							Invalidate(ActionDiagramPresenter);
							// Move preview shape to Mouse Position
							PreviewShape.MoveTo(newMouseState.X, newMouseState.Y);
							// Snap to grid
							if (diagramPresenter.SnapToGrid) {
								int snapDeltaX = 0, snapDeltaY = 0;
								FindNearestSnapPoint(diagramPresenter, PreviewShape, 0, 0, out snapDeltaX, out snapDeltaY);
								PreviewShape.MoveTo(newMouseState.X + snapDeltaX, newMouseState.Y + snapDeltaY);
							}
							Invalidate(ActionDiagramPresenter);
							result = true;
						}
						break;

					case MouseEventType.MouseUp:
						if (IsToolActionPending && newMouseState.IsButtonDown(MouseButtonsDg.Left)) {
							try {
								// Left mouse button was pressed: Create shape
								_executing = true;
								Invalidate(ActionDiagramPresenter);
								if (ActionDiagramPresenter.Diagram != null) {
									int x = PreviewShape.X;
									int y = PreviewShape.Y;

									Shape newShape = Template.CreateShape();
									newShape.ZOrder = ActionDiagramPresenter.Project.Repository.ObtainNewTopZOrder(ActionDiagramPresenter.Diagram);
									newShape.MoveTo(x, y);

									ActionDiagramPresenter.InsertShape(newShape);
									result = true;
								}
							} finally {
								_executing = false;
							}
							EndToolAction();
							OnToolExecuted(ExecutedEventArgs);
						} else if (newMouseState.IsButtonDown(MouseButtonsDg.Right)) {
							// Right mouse button was pressed: Cancel Tool
							Cancel();
							result = true;
						}
						break;

					case MouseEventType.MouseDown:
						// nothing to to yet
						// ToDo 3: Implement dragging a frame with the mouse and fit the shape into that frame when releasing the button
						break;

					default: throw new NShapeUnsupportedValueException(e.EventType);
				}
			} finally { diagramPresenter.ResumeUpdate(); }
			base.ProcessMouseEvent(diagramPresenter, e);
			return result;
		}


		/// <override></override>
		public override bool ProcessKeyEvent(IDiagramPresenter diagramPresenter, KeyEventArgsDg e) {
			return base.ProcessKeyEvent(diagramPresenter, e);
		}


		/// <override></override>
		public override void EnterDisplay(IDiagramPresenter diagramPresenter) {
			if (!CurrentMouseState.IsEmpty && !_executing) {
				if (diagramPresenter.Project.SecurityManager.IsGranted(Permission.Insert, Template.Shape))
					StartToolAction(diagramPresenter, (int)Action.Create, CurrentMouseState, false);
			}
		}


		/// <override></override>
		public override void LeaveDisplay(IDiagramPresenter diagramPresenter) {
			// Do not end tool action while inserting a shape (e.g. when showing a dialog 
			// on the DiagramPresenter's "ShapeInserted" event
			if (!_executing && IsToolActionPending)
				EndToolAction();
		}


		/// <override></override>
		public override void Draw(IDiagramPresenter diagramPresenter) {
			if (diagramPresenter == null) throw new ArgumentNullException("diagramPresenter");
			if (_drawPreview) {
				//if (DisplayContainsMousePos(ActionDisplay, CurrentMouseState.Position)) {
				diagramPresenter.DrawShape(PreviewShape);
				if (ActionDiagramPresenter.SnapToGrid)
					diagramPresenter.DrawSnapIndicators(PreviewShape);
				//}
			}
		}


		/// <override></override>
		public override void Invalidate(IDiagramPresenter diagramPresenter) {
			if (diagramPresenter == null) throw new ArgumentNullException("diagramPresenter");
			if (PreviewShape != null && diagramPresenter.SnapToGrid)
				diagramPresenter.InvalidateSnapIndicators(PreviewShape);
		}


		/// <override></override>
		public override IEnumerable<MenuItemDef> GetMenuItemDefs(IDiagramPresenter diagramPresenter) {
			if (diagramPresenter == null) throw new ArgumentNullException("diagramPresenter");
			yield break;
		}


		/// <override></override>
		protected override void CancelCore() {
			if (PreviewShape != null)
				ClearPreview();
		}


		/// <override></override>
		protected override void StartToolAction(IDiagramPresenter diagramPresenter, int action, MouseState mouseState, bool wantAutoScroll) {
			base.StartToolAction(diagramPresenter, action, mouseState, wantAutoScroll);
			CreatePreview(ActionDiagramPresenter);
			PreviewShape.MoveTo(mouseState.X, mouseState.Y);
			_drawPreview = true;
			diagramPresenter.SetCursor(CurrentCursorId);
		}


		/// <override></override>
		protected override void EndToolAction() {
			base.EndToolAction();
			_drawPreview = false;
			ClearPreview();
		}


		static PlanarShapeCreationTool() {
			_crossCursorId = CursorProvider.RegisterCursor(Properties.Resources.CrossCursor);
		}


		private void Construct(Template template) {
			if (!(template.Shape is IPlanarShape))
				throw new NShapeException("The template's shape does not implement {0}.", typeof(IPlanarShape).Name);
			_drawPreview = false;
		}


		private int CurrentCursorId {
			get { return _drawPreview ? _crossCursorId : CursorProvider.DefaultCursorID; }
		}


		private enum Action { None, Create }


		#region Fields

		// Definition of the tool
		private static int _crossCursorId;
		private bool _drawPreview;
		private bool _executing = false;

		#endregion
	}

}