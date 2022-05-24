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
				clone.DrawThumbnail(base.LargeIcon, IconMargin, IconTransparentColor);
				base.LargeIcon.MakeTransparent(IconTransparentColor);
			}
			using (Shape clone = Template.Shape.Clone()) {
				clone.DrawThumbnail(base.SmallIcon, IconMargin, IconTransparentColor);
				base.SmallIcon.MakeTransparent(IconTransparentColor);
			}
			ClearPreview();
			Title = Template.Title;
		}


		/// <override></override>
		protected TemplateTool(Template template, string category)
			: base(category) {
			if (template == null) throw new ArgumentNullException(nameof(template));
			_template = template;
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
		/// Gets the range of finding connection targets (typically: connection points and shapes).
		/// Defaults to the snap distance, adjustable in Display/IDiagramPresenter.
		/// </summary>
		protected virtual int GetFindGridLineRange(IDiagramPresenter diagramPresenter) {
			if (diagramPresenter == null) throw new ArgumentNullException(nameof(diagramPresenter));
			return diagramPresenter.SnapDistance;
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
			if (diagramPresenter == null) throw new ArgumentNullException(nameof(diagramPresenter));
			yield break;
		}


		/// <override></override>
		public override bool ProcessMouseEvent(IDiagramPresenter diagramPresenter, MouseEventArgsDg e) {
			if (diagramPresenter == null) throw new ArgumentNullException(nameof(diagramPresenter));
			bool result = false;

			MouseState newMouseState = MouseState.Create(diagramPresenter, e);
			diagramPresenter.SuspendUpdate();
			try {
				switch (e.EventType) {
					case MouseEventType.MouseMove:
						if (CurrentMouseState.Position != newMouseState.Position) {
							Invalidate(diagramPresenter);
							ProcessMouseMove(diagramPresenter, newMouseState);
						}
						break;
					case MouseEventType.MouseDown:
						// MouseDown starts drag-based actions
						// ToDo: Implement these features: Adding Segments to existing Lines, Move existing Lines and their ControlPoints
						if (e.Clicks > 1) result = ProcessDoubleClick(diagramPresenter, newMouseState);
						else result = ProcessMouseDown(diagramPresenter, newMouseState);
						break;

					case MouseEventType.MouseUp:
						// MouseUp finishes drag-actions. Click-based actions are handled by the MouseClick event
						// ToDo: Implement these features: Adding Segments to existing Lines, Move existing Lines and their ControlPoints
						result = ProcessMouseUp(diagramPresenter, newMouseState);
						break;

					default: throw new NShapeUnsupportedValueException(e.EventType);
				}
			} finally { 
				diagramPresenter.ResumeUpdate(); 
			}
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
			if (diagramPresenter == null) throw new ArgumentNullException(nameof(diagramPresenter));
			if (PreviewShape == null) {
				// Invalidate highlighted connection points
				InvalidateConnectionTargets(diagramPresenter, CurrentMouseState.X, CurrentMouseState.Y, ControlPointCapabilities.Connect,
						HighlightTargetsRange, CreateIsConnectionTargetFilter(diagramPresenter, Template.Shape, ControlPointId.FirstVertex));
			} else {
				diagramPresenter.InvalidateGrips(PreviewShape, ControlPointCapabilities.All);
				Point p = PreviewShape.GetControlPointPosition(ControlPointId.LastVertex);
				int range = Math.Max(GetFindTargetRange(diagramPresenter), HighlightTargetsRange);
				InvalidateConnectionTargets(diagramPresenter, p.X, p.Y, ControlPointCapabilities.Connect, range, 
						CreateIsConnectionTargetFilter(diagramPresenter, PreviewShape, ControlPointId.LastVertex));
			}
		}


		/// <override></override>
		public override void Draw(IDiagramPresenter diagramPresenter) {
			if (diagramPresenter == null) throw new ArgumentNullException(nameof(diagramPresenter));
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
				ControlPointId lastVertex = ControlPointId.LastVertex;
				if (Template.Shape.HasControlPointCapability(lastVertex, ControlPointCapabilities.Glue)) {
					if (PreviewShape == null)
						DrawConnectionTargets(diagramPresenter, Template.Shape, lastVertex, CurrentMouseState.Position, ControlPointCapabilities.Connect, 
							HighlightTargetsRange, 
							GetFindTargetRange(diagramPresenter), 
							CreateIsConnectionTargetFilter(diagramPresenter, Template.Shape, lastVertex));
					else {
						Point gluePtPos = PreviewShape.GetControlPointPosition(lastVertex);
						DrawConnectionTargets(diagramPresenter, PreviewShape, lastVertex, gluePtPos, ControlPointCapabilities.Connect, 
							HighlightTargetsRange, 
							GetFindTargetRange(diagramPresenter), 
							CreateIsConnectionTargetFilter(diagramPresenter, PreviewShape, lastVertex));
					}
				}
			}
		}


		// @@ Tool-Interface: New
		/// <summary>
		/// Returns true if the given shape (and control point) are regarded as a valid target for the current action.
		/// </summary>
		/// <param name="diagramPresenter">The current diagram presenter.</param>
		/// <param name="shapeAtCursor">The shape to test.</param>
		/// <param name="controlPointAtCursor">The id of the shape's control point to test.</param>
		protected virtual TargetFilterDelegate CreateIsConnectionTargetFilter(IDiagramPresenter diagramPresenter, Shape shapeAtCursor, ControlPointId controlPointAtCursor) {
			return (Shape shape, ControlPointId ctrlPointId) => {
				if (shape == shapeAtCursor)
					return false;
				// Skip invisible shapes
				if (!diagramPresenter.IsLayerVisible(shape.HomeLayer, shape.SupplementalLayers))
					return false;
				if (shapeAtCursor != null && shapeAtCursor.HasControlPointCapability(controlPointAtCursor, ControlPointCapabilities.Glue)) {
					return shape.HasControlPointCapability(ctrlPointId, ControlPointCapabilities.Connect)
						&& shapeAtCursor.CanConnect(controlPointAtCursor, shape, ctrlPointId);
				} else 
					return true;
			};
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


		// @@ Tool-Interface: New
		/// <summary>
		/// Called after the new linear shape was created from the template and before the new shape is inserted into the diagram and the repository.
		/// </summary>
		/// <param name="diagramPresenter">The current display presenter.</param>
		/// <param name="mouseState">The current mouse state.</param>
		/// <param name="discardLastVertex">
		/// Specifies if the last vertex of the preview shape (that 'sticks' to the mouse) should be discarded.
		/// By default, this is the case if the tool action was ended by pressing Esc or the right mouse button.
		/// </param>
		/// <returns>
		/// Returns true if the calling function should continue with the default implementation or 
		/// false if the overridden method has already handled creating the shape.
		/// </returns>
		protected virtual bool OnLinearShapeCreating(IDiagramPresenter diagramPresenter, MouseState mouseState, bool discardLastVertex) {
			return true;
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
			Shape currentShape = (_modifiedLinearShape as Shape) ?? PreviewShape;
			ControlPointCapabilities pointCapabilities;
			if (_pointAtCursor != ControlPointId.None && currentShape != null)
				pointCapabilities = ControlPointCapabilities.All;
			else
				pointCapabilities = ControlPointCapabilities.Connect | ControlPointCapabilities.Glue;
			Locator target = FindNearestTarget(diagramPresenter, mouseState.X, mouseState.Y, pointCapabilities, GetFindTargetRange(diagramPresenter), 
					CreateIsConnectionTargetFilter(diagramPresenter, currentShape, _pointAtCursor));
			shapeAtCursorInfo.Shape = target.Shape;
			shapeAtCursorInfo.ControlPointId = target.ControlPointId;

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
						DebugDiagnostics.Assert(PreviewShape != null);
						if (PreviewShape != null)
							PreviewShape.MoveControlPointTo(_pointAtCursor, p.X, p.Y, resizeModifier);
					} else {
						int snapDeltaX = 0, snapDeltaY = 0;
						if (diagramPresenter.SnapToGrid)
							FindNearestSnapPoint(diagramPresenter, mouseState.X, mouseState.Y, out snapDeltaX, out snapDeltaY);
						DebugDiagnostics.Assert(PreviewShape != null);
						if (PreviewShape != null)
							PreviewShape.MoveControlPointTo(_pointAtCursor, mouseState.X + snapDeltaX, mouseState.Y + snapDeltaY, resizeModifier);
					}
					Invalidate(ActionDiagramPresenter);
					break;

				default: throw new NShapeUnsupportedValueException(CurrentAction);
			}
			return result;
		}


		private bool ProcessMouseDown(IDiagramPresenter diagramPresenter, MouseState mouseState) {
			bool result = false;
			switch (CurrentAction) {
				case Action.None:
					if (mouseState.IsButtonDown(MouseButtonsDg.Left)) {
						if (diagramPresenter.SelectedShapes.Count > 0)
							diagramPresenter.UnselectAll();

						//ShapeAtCursorInfo targetShapeInfo = FindShapeAtCursor(diagramPresenter, mouseState.X, mouseState.Y, ControlPointCapabilities.Glue, diagramPresenter.ZoomedGripSize, false);
						ShapeAtCursorInfo targetShapeInfo = ShapeAtCursorInfo.Create(FindNearestTarget(diagramPresenter, mouseState.X, mouseState.Y, ControlPointCapabilities.Glue, 
								GetFindTargetRange(diagramPresenter), CreateIsConnectionTargetFilter(diagramPresenter, null, ControlPointId.None)));
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
							//ShapeAtCursorInfo shapeAtCursorInfo = FindShapeAtCursor(ActionDiagramPresenter, mouseState.X, mouseState.Y, ControlPointCapabilities.Connect, diagramPresenter.ZoomedGripSize, false);
							ShapeAtCursorInfo shapeAtCursorInfo = ShapeAtCursorInfo.Create(FindNearestTarget(diagramPresenter, mouseState.X, mouseState.Y, ControlPointCapabilities.Connect,
									GetFindTargetRange(diagramPresenter), CreateIsConnectionTargetFilter(diagramPresenter, PreviewShape, _pointAtCursor)));
							if (!shapeAtCursorInfo.IsEmpty && !shapeAtCursorInfo.IsCursorAtGluePoint)
								doFinishLine = true;
							else AddNewPoint(ActionDiagramPresenter, mouseState);
						}
						// Create line if necessary
						if (doFinishLine) {
							if (CurrentAction == Action.CreateLine)
								FinishLine(ActionDiagramPresenter, mouseState, false);
							else 
								FinishExtendLine(ActionDiagramPresenter, mouseState, false, (Shape)_modifiedLinearShape);

							while (IsToolActionPending)
								EndToolAction();
							OnToolExecuted(ExecutedEventArgs);
						}
					}
					result = true;
					break;

				default: throw new NShapeUnsupportedValueException(CurrentAction);
			}
			Invalidate(diagramPresenter);
			return result;
		}


		private bool ProcessMouseUp(IDiagramPresenter diagramPresenter, MouseState mouseState)
		{
			bool result = false;
			switch (CurrentAction) {
				case Action.None:
					if (mouseState.IsButtonDown(MouseButtonsDg.Right)) {
						Cancel();
						result = true;
					}
					break;

				case Action.CreateLine:
				case Action.ExtendLine:
					if (mouseState.IsButtonDown(MouseButtonsDg.Right)) {
						DebugDiagnostics.Assert(PreviewShape != null);
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
							else 
								FinishExtendLine(ActionDiagramPresenter, mouseState, true, (Shape)_modifiedLinearShape);
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
			bool result = false;
			if (IsToolActionPending) {
				DebugDiagnostics.Assert(PreviewShape != null);
				if (CurrentAction == Action.CreateLine)
					FinishLine(ActionDiagramPresenter, mouseState, true);
				else if (CurrentAction == Action.ExtendLine)
					FinishExtendLine(ActionDiagramPresenter, mouseState, true, (Shape)_modifiedLinearShape);

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
			Locator target = Locator.Empty;
			if (diagramPresenter.Project.SecurityManager.IsGranted(Permission.Connect, Template.Shape)) {
				target = FindNearestTarget(diagramPresenter, mouseState.X, mouseState.Y, ControlPointCapabilities.All, 
						GetFindTargetRange(diagramPresenter), CreateIsConnectionTargetFilter(diagramPresenter, Template.Shape, ControlPointId.FirstVertex));
			}
			int snapDeltaX = 0, snapDeltaY = 0;
			if (diagramPresenter.SnapToGrid) {
				if (target.IsEmpty || target.ControlPointId == ControlPointId.Reference)
					FindNearestSnapPoint(diagramPresenter, mouseState.X, mouseState.Y, out snapDeltaX, out snapDeltaY);
				else {
					Point p = target.Shape.GetControlPointPosition(target.ControlPointId);
					snapDeltaX = p.X - mouseState.X;
					snapDeltaY = p.Y - mouseState.Y;
				}
			}

			// set line's start coordinates
			Point start = Point.Empty;
			if (!target.IsEmpty) {
				if (target.ControlPointId == ControlPointId.Reference) {
					// ToDo: Get nearest point on line
					start = mouseState.Position;
					start.Offset(snapDeltaX, snapDeltaY);
				} else
					start = target.Shape.GetControlPointPosition(target.ControlPointId);
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
			if (target.Shape != null && target.Shape.HasControlPointCapability(target.ControlPointId, ControlPointCapabilities.Connect)) {
				if (CanActiveShapeConnectTo(PreviewShape, ControlPointId.FirstVertex, target.Shape, target.ControlPointId))
					PreviewShape.Connect(ControlPointId.FirstVertex, target.Shape, target.ControlPointId);
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
			DebugDiagnostics.Assert(PreviewLinearShape != null);
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
		/// Creates a new linear shape, copies all vertexes from the preview shape and inserts the new line 
		/// into the diagram by executing a command.
		/// </summary>
		private void FinishLine(IDiagramPresenter diagramPresenter, MouseState mouseState, bool ignorePointAtMouse) {
			if (diagramPresenter == null) throw new ArgumentNullException(nameof(diagramPresenter));
			DebugDiagnostics.Assert(PreviewShape != null);
			if (OnLinearShapeCreating(diagramPresenter, mouseState, ignorePointAtMouse)) {
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
						Locator target = FindNearestTarget(ActionDiagramPresenter, gluePtPos.X, gluePtPos.Y, ControlPointCapabilities.All, 
								GetFindTargetRange(diagramPresenter), CreateIsConnectionTargetFilter(diagramPresenter, newShape, ControlPointId.LastVertex));
						if (target.Shape != null && target.Shape.HasControlPointCapability(target.ControlPointId, ControlPointCapabilities.Glue) == false
								&& CanActiveShapeConnectTo(newShape, gluePointId, target.Shape, target.ControlPointId)) {
							if (aggregatedCommand == null) aggregatedCommand = new AggregatedCommand();
							aggregatedCommand.Add(new ConnectCommand(newShape, gluePointId, target.Shape, target.ControlPointId));
						}
					}
				}
				// execute command and insert it into the history
				if (aggregatedCommand != null)
					ActionDiagramPresenter.Project.ExecuteCommand(aggregatedCommand);
			}
		}


		/// <summary>
		/// Applies the changes (adding/inserting vertex points) from the preview shape to the line shape.
		/// </summary>
		private void FinishExtendLine(IDiagramPresenter diagramPresenter, MouseState mouseState, bool ignorePointAtMouse, Shape linearShapeToModify) {
			if (diagramPresenter == null) throw new ArgumentNullException(nameof(diagramPresenter));
			if (linearShapeToModify == null) throw new ArgumentNullException(nameof(linearShapeToModify));
			DebugDiagnostics.Assert(diagramPresenter == ActionDiagramPresenter);
			DebugDiagnostics.Assert(PreviewShape != null);
			DebugDiagnostics.Assert(_modifiedLinearShape is ILinearShape);

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
				ControlPointId nextOrigPtId = GetNextResizePointId((ILinearShape)linearShapeToModify, pointId, firstToLast);
				if (nextPointId != nextOrigPtId && nextPointId != endPointId) {
					// If the next point id of the preview does not equal the original shape's point id,
					// we have to create it...
					Point p = PreviewShape.GetControlPointPosition(nextPointId);
					ControlPointId beforePointId = (firstToLast) ? (ControlPointId)ControlPointId.LastVertex : pointId;
					if (aggregatedCommand == null) aggregatedCommand = new AggregatedCommand();
					aggregatedCommand.Add(new InsertVertexCommand(linearShapeToModify, beforePointId, p.X, p.Y));
				}
				pointId = nextPointId;
			} while (pointId != endPointId);
			// Set last point's position
			ControlPointId lastPtId = firstToLast ? ControlPointId.LastVertex : ControlPointId.FirstVertex;
			Point currPos = linearShapeToModify.GetControlPointPosition(lastPtId);
			Point newPos = PreviewShape.GetControlPointPosition(endPointId);
			DebugDiagnostics.Assert(aggregatedCommand != null);
			aggregatedCommand.Add(new MoveControlPointCommand(linearShapeToModify, lastPtId, newPos.X - currPos.X, newPos.Y - currPos.Y, ResizeModifiers.None));

			// Create connection for the last vertex
			Locator target = FindNearestTarget(diagramPresenter, newPos.X, newPos.Y, ControlPointCapabilities.All, 
					GetFindTargetRange(diagramPresenter), CreateIsConnectionTargetFilter(diagramPresenter, linearShapeToModify, lastPtId));
			if (target.Shape != null && target.ControlPointId != ControlPointId.None && 
					target.Shape.HasControlPointCapability(target.ControlPointId,ControlPointCapabilities.Glue) == false) {
				if (aggregatedCommand == null) aggregatedCommand = new AggregatedCommand();
				aggregatedCommand.Add(new ConnectCommand(linearShapeToModify, lastPtId, target.Shape, target.ControlPointId));
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
			if (diagramPresenter == null) throw new ArgumentNullException(nameof(diagramPresenter));
			bool result = false;

			// Return if action is not allowed
			if (!diagramPresenter.Project.SecurityManager.IsGranted(Permission.Insert, Template.Shape))
				return result;

			MouseState newMouseState = MouseState.Create(diagramPresenter, e);
			diagramPresenter.SuspendUpdate();
			try {
				switch (e.EventType) {
					case MouseEventType.MouseMove:
						result = OnProcessMouseMove(diagramPresenter, newMouseState);
						break;

					case MouseEventType.MouseUp:
						result = OnProcessMouseUp(diagramPresenter, newMouseState);
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



		/// <summary>
		/// Called when the preview shape(s) sould be moved.
		/// Override for implementing custom tool logic.
		/// </summary>
		/// <param name="diagramPresenter">The current diagram presenter.</param>
		/// <param name="mouseState">The current mouse state.</param>
		/// <returns>True if the default implementation should be executed, false if all work is done.</returns>
		protected virtual bool OnPreviewShapeMoving(IDiagramPresenter diagramPresenter, MouseState mouseState) {
			return true;
		}


		/// <summary>
		/// Called for creating the planar shape from the tool's template.
		/// </summary>
		/// <returns>True if the default implementation should be executed, false if all work is done.</returns>
		protected virtual bool OnPlanarShapeCreating(IDiagramPresenter diagramPresenter, MouseState mouseState) {
			return true;
		}


		/// <ToBeCompleted></ToBeCompleted>
		private bool OnProcessMouseUp(IDiagramPresenter diagramPresenter, MouseState mouseState) {
			if (diagramPresenter == null) throw new ArgumentNullException(nameof(diagramPresenter));
			Debug.Assert(diagramPresenter == ActionDiagramPresenter);
			bool result = false;
			if (IsToolActionPending && mouseState.IsButtonDown(MouseButtonsDg.Left)) {
				try {
					// Left mouse button was pressed: Create shape
					_executing = true;
					Invalidate(diagramPresenter);
					if (OnPlanarShapeCreating(diagramPresenter, mouseState)) {
						if (diagramPresenter.Diagram != null) {
							// We take the position from the preview shape, no need to re-calculate the snap distance to grid lines.
							int x = PreviewShape.X;
							int y = PreviewShape.Y;

							Shape newShape = Template.CreateShape();
							newShape.ZOrder = diagramPresenter.Project.Repository.ObtainNewTopZOrder(diagramPresenter.Diagram);
							newShape.MoveTo(x, y);

							diagramPresenter.InsertShape(newShape);
						}
					}
					result = true;
				} finally {
					_executing = false;
				}
				EndToolAction();
				OnToolExecuted(ExecutedEventArgs);
			} else if (mouseState.IsButtonDown(MouseButtonsDg.Right)) {
				// Right mouse button was pressed: Cancel Tool
				Cancel();
				result = true;
			}

			return result;
		}


		/// <ToBeCompleted></ToBeCompleted>
		private bool OnProcessMouseMove(IDiagramPresenter diagramPresenter, MouseState mouseState) {
			bool result = false;
			if (mouseState.Position != CurrentMouseState.Position) {
				// If no Preview exists, create a new one by starting a new ToolAction
				if (!IsToolActionPending)
					StartToolAction(diagramPresenter, (int)Action.Create, mouseState, false);

				Invalidate(ActionDiagramPresenter);
				if (OnPreviewShapeMoving(diagramPresenter, mouseState)) {
					// Move preview shape to Mouse Position
					PreviewShape.MoveTo(mouseState.X, mouseState.Y);
					// Snap to grid
					int snapDeltaX = 0, snapDeltaY = 0;
					if (diagramPresenter.SnapToGrid)
						FindNearestSnapPoint(diagramPresenter, PreviewShape, 0, 0, out snapDeltaX, out snapDeltaY);
					
					PreviewShape.MoveTo(mouseState.X + snapDeltaX, mouseState.Y + snapDeltaY);
				}
				Invalidate(ActionDiagramPresenter);
				result = true;
			}

			return result;
		}


		/// <override></override>
		public override bool ProcessKeyEvent(IDiagramPresenter diagramPresenter, KeyEventArgsDg e) {
			return base.ProcessKeyEvent(diagramPresenter, e);
		}


		/// <override></override>
		public override void EnterDisplay(IDiagramPresenter diagramPresenter) {
			if (!CurrentMouseState.IsEmpty && !_executing && !IsToolActionPending) {
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
			if (diagramPresenter == null) throw new ArgumentNullException(nameof(diagramPresenter));
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
			if (diagramPresenter == null) throw new ArgumentNullException(nameof(diagramPresenter));
			if (PreviewShape != null && diagramPresenter.SnapToGrid)
				diagramPresenter.InvalidateSnapIndicators(PreviewShape);
		}


		/// <override></override>
		public override IEnumerable<MenuItemDef> GetMenuItemDefs(IDiagramPresenter diagramPresenter) {
			if (diagramPresenter == null) throw new ArgumentNullException(nameof(diagramPresenter));
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
