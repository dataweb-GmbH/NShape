/******************************************************************************
  Copyright 2009-2019 dataweb GmbH
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
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

using Dataweb.NShape.Advanced;
using Dataweb.NShape.Commands;
using Dataweb.NShape.Controllers;


namespace Dataweb.NShape.WinFormsUI {

	/// <summary>
	/// A component used for displaying diagrams.
	/// </summary>
	[Designer(typeof(DisplayDesigner))]
	public partial class Display : UserControl, IDiagramPresenter, IDisplayService {

		/// <ToBeCompleted></ToBeCompleted>
		public const string MenuItemNameSelectShapesGroupItem = "SelectShapesGroupItem";
		/// <ToBeCompleted></ToBeCompleted>
		public const string MenuItemNameShowShapeInfo = "ShowShapeInfoAction";
		/// <ToBeCompleted></ToBeCompleted>
		public const string MenuItemNameSelectAll = "SelectAllAction";
		/// <ToBeCompleted></ToBeCompleted>
		public const string MenuItemNameUnselectAll = "UnselectAllAction";
		/// <ToBeCompleted></ToBeCompleted>
		public const string MenuItemNameSelectByType = "SelectByTypeAction";
		/// <ToBeCompleted></ToBeCompleted>
		public const string MenuItemNameSelectByTemplate = "SelectByTemplateAction";
		/// <ToBeCompleted></ToBeCompleted>
		public const string MenuItemNameBringToFront = "BringToFrontAction";
		/// <ToBeCompleted></ToBeCompleted>
		public const string MenuItemNameSendToBack = "SendToBackAction";
		/// <ToBeCompleted></ToBeCompleted>
		public const string MenuItemNameAssignToLayers = "AssignToLayersAction";
		/// <ToBeCompleted></ToBeCompleted>
		public const string MenuItemNameAddToLayers = "AddToLayersAction";
		/// <ToBeCompleted></ToBeCompleted>
		public const string MenuItemNameRemoveFromLayers = "RemoveFromLayersAction";
		/// <ToBeCompleted></ToBeCompleted>
		public const string MenuItemNameGroupShapes = "GroupShapesAction";
		/// <ToBeCompleted></ToBeCompleted>
		public const string MenuItemNameUngroupShapes = "UngroupShapesAction";
		/// <ToBeCompleted></ToBeCompleted>
		public const string MenuItemNameAggregateShapes = "AggregateShapesAction";
		/// <ToBeCompleted></ToBeCompleted>
		public const string MenuItemNameDisaggregateShapes = "DisaggregateShapesAction";
		/// <ToBeCompleted></ToBeCompleted>
		public const string MenuItemNameCutGroupItem = "CutGroupItem";
		/// <ToBeCompleted></ToBeCompleted>
		public const string MenuItemNameCut = "CutAction";
		/// <ToBeCompleted></ToBeCompleted>
		public const string MenuItemNameCutShapesOnly = "CutShapesOnlyAction";
		/// <ToBeCompleted></ToBeCompleted>
		public const string MenuItemNameCutWithModels = "CutWithModelsAction";
		/// <ToBeCompleted></ToBeCompleted>
		public const string MenuItemNameCopyAsImage = "CopyAsImageAction";
		/// <ToBeCompleted></ToBeCompleted>
		public const string MenuItemNameCopyGroupItem = "CopyGroupItem";
		/// <ToBeCompleted></ToBeCompleted>
		public const string MenuItemNameCopy = "CopyAction";
		/// <ToBeCompleted></ToBeCompleted>
		public const string MenuItemNameCopyShapesOnly = "CopyShapesOnlyAction";
		/// <ToBeCompleted></ToBeCompleted>
		public const string MenuItemNameCopyWithModels = "CopyWithModelsAction";
		/// <ToBeCompleted></ToBeCompleted>
		public const string MenuItemNamePaste = "PasteAction";
		/// <ToBeCompleted></ToBeCompleted>
		public const string MenuItemNameDeleteGroupItem = "DeleteGroupItem";
		/// <ToBeCompleted></ToBeCompleted>
		public const string MenuItemNameDelete = "DeleteAction";
		/// <ToBeCompleted></ToBeCompleted>
		public const string MenuItemNameDeleteShapesOnly = "DeleteShapesOnlyAction";
		/// <ToBeCompleted></ToBeCompleted>
		public const string MenuItemNameDeleteWithModels = "DeleteWithModelsAction";
		/// <ToBeCompleted></ToBeCompleted>
		public const string MenuItemNameShowProperties = "ShowPropertiesAction";
		/// <ToBeCompleted></ToBeCompleted>
		public const string MenuItemNameUndo = "UndoAction";
		/// <ToBeCompleted></ToBeCompleted>
		public const string MenuItemNameRedo = "RedoAction";


		/// <summary>
		/// Constructor
		/// </summary>
		public Display() {
			this.SetStyle(
				// enable double buffered painting, transparent background and resize redrawing
				ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer | ControlStyles.UserPaint
				| ControlStyles.SupportsTransparentBackColor
				| ControlStyles.ResizeRedraw
				// enable focusable container control
				| ControlStyles.ContainerControl | ControlStyles.Selectable
				, true);
			this.UpdateStyles();
			this.TabStop = true;
			_infoGraphics = Graphics.FromHwnd(this.Handle);

			// Initialize components
			InitializeComponent();
			AllowDrop = true;
			AutoScroll = false;

			// Set event handlers
#if DEBUG_DIAGNOSTICS
			scrollBarV.ValueChanged += ScrollBar_ValueChanged;
			scrollBarH.ValueChanged += ScrollBar_ValueChanged;
#endif
			scrollBarH.Scroll += ScrollBar_Scroll;
			scrollBarV.Scroll += ScrollBar_Scroll;
			scrollBarH.KeyDown += scrollBar_KeyDown;
			scrollBarV.KeyDown += scrollBar_KeyDown;
			scrollBarH.KeyPress += scrollBar_KeyPress;
			scrollBarV.KeyPress += scrollBar_KeyPress;
			scrollBarH.KeyUp += scrollBar_KeyUp;
			scrollBarV.KeyUp += scrollBar_KeyUp;
			scrollBarH.MouseEnter += ScrollBars_MouseEnter;
			scrollBarV.MouseEnter += ScrollBars_MouseEnter;
			scrollBarH.VisibleChanged += ScrollBar_VisibleChanged;
			scrollBarV.VisibleChanged += ScrollBar_VisibleChanged;
			scrollBarH.GotFocus += scrollBar_GotFocus;
			scrollBarV.GotFocus += scrollBar_GotFocus;
			HScrollBarVisible = VScrollBarVisible = false;
			hScrollBarPanel.BackColor = BackColor;

			// Calculate grip shapes
			CalcControlPointShape(_resizePointPath, _resizePointShape, _handleRadius);
			CalcControlPointShape(_rotatePointPath, ControlPointShape.RotateArrow, _handleRadius);
			CalcControlPointShape(_connectionPointPath, _connectionPointShape, _handleRadius);
			//
			_previewTextFormatter.FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.MeasureTrailingSpaces;
			_previewTextFormatter.Trimming = StringTrimming.EllipsisCharacter;

			//
			_gridSpace = MmToPixels(5);
			_gridSize.Width = _gridSpace;
			_gridSize.Height = _gridSpace;
			_invalidateDelta = _handleRadius;

			// used for fixed refresh rate rendering
			_autoScrollTimer.Enabled = false;
			_autoScrollTimer.Interval = 10;
			_autoScrollTimer.Tick += autoScrollTimer_Tick;
		}


		/// <summary>
		/// Finalizer
		/// </summary>
		~Display() {
			Dispose();
		}


		#region IDisplayService Members (explicit implementation)

		/// <override></override>
		void IDisplayService.Invalidate(int x, int y, int width, int height) {
			DoInvalidateDiagram(x, y, width, height);
		}


		/// <override></override>
		void IDisplayService.Invalidate(Rectangle rectangle) {
			DoInvalidateDiagram(rectangle);
		}


		/// <override></override>
		void IDisplayService.NotifyBoundsChanged() {
			// Nothing to do. Will be removed soon...
		}


		/// <override></override>
		Graphics IDisplayService.InfoGraphics {
			get { return _infoGraphics; }
		}


		/// <override></override>
		IFillStyle IDisplayService.HintBackgroundStyle {
			get {
				if (_hintBackgroundStyle == null) {
					_hintBackgroundStyle = new FillStyle("Hint Background Style");
					_hintBackgroundStyle.BaseColorStyle = new ColorStyle("Hint Background Color", SelectionInteriorColor);
					_hintBackgroundStyle.AdditionalColorStyle = _hintBackgroundStyle.BaseColorStyle;
					_hintBackgroundStyle.FillMode = FillMode.Solid;
				}
				return _hintBackgroundStyle;
			}
		}


		/// <override></override>
		ILineStyle IDisplayService.HintForegroundStyle {
			get {
				if (_hintForegroundStyle == null) {
					_hintForegroundStyle = new LineStyle("Hint Foreground Line Style");
					_hintForegroundStyle.ColorStyle = new ColorStyle("Hint Foreground Color", ToolPreviewColor);
					_hintForegroundStyle.DashCap = DashCap.Round;
					_hintForegroundStyle.LineJoin = LineJoin.Round;
					_hintForegroundStyle.LineWidth = 1;
				}
				return _hintForegroundStyle;
			}
		}

		#endregion


		#region IDiagramPresenter Members (explicit implementation)

		/// <override></override>
		[Browsable(false)]
		IDisplayService IDiagramPresenter.DisplayService {
			get { return this; }
		}


		///// <override></override>
		//void IDiagramPresenter.NotifyShapesInserted(IEnumerable<Shape> shapes) {
		//   OnShapesInserted(new DiagramPresenterShapesEventArgs(shapes));
		//}


		///// <override></override>
		//void IDiagramPresenter.NotifyShapesRemoved(IEnumerable<Shape> shapes) {
		//   OnShapesRemoved(new DiagramPresenterShapesEventArgs(shapes));
		//}


		/// <override></override>
		int IDiagramPresenter.ZoomedGripSize {
			get { return Math.Max(1, (int)Math.Round(_handleRadius / _zoomfactor)); }
		}


		/// <override></override>
		void IDiagramPresenter.InvalidateDiagram(int x, int y, int width, int height) {
			DoInvalidateDiagram(x, y, width, height);
		}


		/// <override></override>
		void IDiagramPresenter.InvalidateDiagram(Rectangle rect) {
			DoInvalidateDiagram(rect);
		}


		/// <override></override>
		void IDiagramPresenter.InvalidateGrips(Shape shape, ControlPointCapabilities controlPointCapability) {
			DoInvalidateGrips(shape, controlPointCapability);
		}


		/// <override></override>
		void IDiagramPresenter.InvalidateGrips(IEnumerable<Shape> shapes, ControlPointCapabilities controlPointCapability) {
			if (shapes == null) throw new ArgumentNullException("shapes");
			Point p = Point.Empty;
			int transformedHandleRadius;
			ControlToDiagram(_handleRadius, out transformedHandleRadius);
			++transformedHandleRadius;
			Rectangle r = Rectangle.Empty;
			foreach (Shape shape in shapes)
				r = Geometry.UniteRectangles(shape.GetBoundingRectangle(false), r);
			r.Inflate(transformedHandleRadius, transformedHandleRadius);
			DoInvalidateDiagram(r);
		}


		/// <override></override>
		void IDiagramPresenter.InvalidateSnapIndicators(Shape shape) {
			if (shape == null) throw new ArgumentNullException("shape");
			int transformedPointRadius, transformedGridSize;
			ControlToDiagram(GripSize, out transformedPointRadius);
			transformedGridSize = (int)Math.Round(GridSize * (ZoomLevel / 100f));

			Rectangle bounds = shape.GetBoundingRectangle(false);
			Point p = Point.Empty;
			foreach (ControlPointId id in shape.GetControlPointIds(ControlPointCapabilities.All)) {
				p = shape.GetControlPointPosition(id);
				bounds = Geometry.UniteRectangles(p.X, p.Y, p.X, p.Y, bounds);
			}
			bounds.Inflate(transformedPointRadius, transformedPointRadius);
			DoInvalidateDiagram(bounds);
		}


		/// <override></override>
		void IDiagramPresenter.SuspendUpdate() {
			DoSuspendUpdate();
		}


		/// <override></override>
		void IDiagramPresenter.ResumeUpdate() {
			DoResumeUpdate();
		}


		/// <override></override>
		void IDiagramPresenter.ResetTransformation() {
			DoResetTransformation();
		}


		/// <override></override>
		void IDiagramPresenter.RestoreTransformation() {
			DoRestoreTransformation();
		}


		/// <override></override>
		void IDiagramPresenter.Update() {
			//Console.WriteLine("[{0}]\t Update called", DateTime.Now.ToString("HH:mm:ss.ffff"));
			Update();
		}


		/// <override></override>
		void IDiagramPresenter.DrawShape(Shape shape) {
			if (shape == null) throw new ArgumentNullException("shape");
			if (_currentGraphics == null) throw new InvalidOperationException(ErrMessageOnlyAllowedWhilePaining);
			if (!_graphicsIsTransformed) throw new InvalidOperationException(ErrMessageRestoreTransformationHasNotBeenCalled);
			shape.Draw(_currentGraphics);
		}


		/// <override></override>
		void IDiagramPresenter.DrawShapes(IEnumerable<Shape> shapes) {
			if (shapes == null) throw new ArgumentNullException("shapes");
			if (_currentGraphics == null) throw new InvalidOperationException(ErrMessageOnlyAllowedWhilePaining);
			if (!_graphicsIsTransformed) throw new InvalidOperationException(ErrMessageRestoreTransformationHasNotBeenCalled);
			foreach (Shape shape in shapes)
				shape.Draw(_currentGraphics);
		}


		/// <override></override>
		void IDiagramPresenter.DrawResizeGrip(IndicatorDrawMode drawMode, Shape shape, ControlPointId pointId) {
			if (shape == null) throw new ArgumentNullException("shape");
			Point p = shape.GetControlPointPosition(pointId);
			DrawResizeGripCore(p.X, p.Y, drawMode);
		}


		/// <override></override>
		void IDiagramPresenter.DrawRotateGrip(IndicatorDrawMode drawMode, Shape shape, ControlPointId pointId) {
			if (shape == null) throw new ArgumentNullException("shape");
			Point p = shape.GetControlPointPosition(pointId);
			DrawRotateGripCore(p.X, p.Y, drawMode);
		}


		/// <override></override>
		void IDiagramPresenter.DrawConnectionPoint(IndicatorDrawMode drawMode, Shape shape, ControlPointId pointId) {
			if (shape == null) throw new ArgumentNullException("shape");
			Debug.Assert(shape.HasControlPointCapability(pointId, ControlPointCapabilities.Connect | ControlPointCapabilities.Glue));
			// Get control point popsition
			Point p = shape.GetControlPointPosition(pointId);
			// Get connection of control point
			ShapeConnectionInfo connection = ShapeConnectionInfo.Empty;
			if (shape.HasControlPointCapability(pointId, ControlPointCapabilities.Glue))
				connection = shape.GetConnectionInfo(pointId, null);
			// Draw point
			DrawConnectionPointCore(p.X, p.Y, drawMode,
				shape.HasControlPointCapability(pointId, ControlPointCapabilities.Resize | ControlPointCapabilities.Movable),
				connection);
		}


		/// <override></override>
		void IDiagramPresenter.DrawCaptionBounds(IndicatorDrawMode drawMode, ICaptionedShape shape, int captionIndex) {
			DoDrawCaptionBounds(drawMode, shape, captionIndex);
		}


		/// <override></override>
		void IDiagramPresenter.DrawShapeOutline(IndicatorDrawMode drawMode, Shape shape) {
			if (shape == null) throw new ArgumentNullException("shape");
			if (_currentGraphics == null) throw new InvalidOperationException(ErrMessageOnlyAllowedWhilePaining);
			if (!_graphicsIsTransformed) throw new NShapeException(ErrMessageRestoreTransformationHasNotBeenCalled);
			DoDrawShapeOutline(drawMode, shape);
		}


		/// <override></override>
		void IDiagramPresenter.DrawSnapIndicators(Shape shape) {
			if (shape == null) throw new ArgumentNullException("shape");
			if (_currentGraphics == null) throw new InvalidOperationException(ErrMessageOnlyAllowedWhilePaining);
			int left = int.MaxValue;
			int top = int.MaxValue;
			int right = int.MinValue;
			int bottom = int.MinValue;
			int snapIndicatorRadius = _handleRadius;

			bool graphicsWasTransformed = _graphicsIsTransformed;
			if (_graphicsIsTransformed) DoResetTransformation();
			try {
				Rectangle shapeBounds = shape.GetBoundingRectangle(true);
				int zoomedGridSize;
				ControlToDiagram(GridSize, out zoomedGridSize);

				bool drawLeft = (shapeBounds.Left % GridSize == 0);
				bool drawTop = (shapeBounds.Top % GridSize == 0);
				bool drawRight = (shapeBounds.Right % GridSize == 0);
				bool drawBottom = (shapeBounds.Bottom % GridSize == 0);

				// transform shape bounds to control coordinates
				DiagramToControl(shapeBounds, out shapeBounds);

				// draw outlines
				if (drawLeft) _currentGraphics.DrawLine(_outerSnapPen, shapeBounds.Left, shapeBounds.Top - 1, shapeBounds.Left, shapeBounds.Bottom + 1);
				if (drawRight) _currentGraphics.DrawLine(_outerSnapPen, shapeBounds.Right, shapeBounds.Top - 1, shapeBounds.Right, shapeBounds.Bottom + 1);
				if (drawTop) _currentGraphics.DrawLine(_outerSnapPen, shapeBounds.Left - 1, shapeBounds.Top, shapeBounds.Right + 1, shapeBounds.Top);
				if (drawBottom) _currentGraphics.DrawLine(_outerSnapPen, shapeBounds.Left - 1, shapeBounds.Bottom, shapeBounds.Right + 1, shapeBounds.Bottom);
				// fill interior
				if (drawLeft) _currentGraphics.DrawLine(_innerSnapPen, shapeBounds.Left, shapeBounds.Top, shapeBounds.Left, shapeBounds.Bottom);
				if (drawRight) _currentGraphics.DrawLine(_innerSnapPen, shapeBounds.Right, shapeBounds.Top, shapeBounds.Right, shapeBounds.Bottom);
				if (drawTop) _currentGraphics.DrawLine(_innerSnapPen, shapeBounds.Left, shapeBounds.Top, shapeBounds.Right, shapeBounds.Top);
				if (drawBottom) _currentGraphics.DrawLine(_innerSnapPen, shapeBounds.Left, shapeBounds.Bottom, shapeBounds.Right, shapeBounds.Bottom);

				foreach (ControlPointId id in shape.GetControlPointIds(ControlPointCapabilities.All)) {
					Point p = Point.Empty;
					p = shape.GetControlPointPosition(id);

					// check if the point is on a gridline
					bool hGridLineContainsPoint = p.X % (GridSize * _zoomfactor) == 0;
					bool vGridLineContainsPoint = p.Y % (GridSize * _zoomfactor) == 0;
					// collect coordinates for bounding box
					if (p.X < left) left = p.X;
					if (p.X > right) right = p.X;
					if (p.Y < top) top = p.Y;
					if (p.Y > bottom) bottom = p.Y;

					if (hGridLineContainsPoint || vGridLineContainsPoint) {
						DiagramToControl(p, out p);
						_currentGraphics.FillEllipse(HandleInteriorBrush, p.X - snapIndicatorRadius, p.Y - snapIndicatorRadius, snapIndicatorRadius * 2, snapIndicatorRadius * 2);
						_currentGraphics.DrawEllipse(_innerSnapPen, p.X - snapIndicatorRadius, p.Y - snapIndicatorRadius, snapIndicatorRadius * 2, snapIndicatorRadius * 2);
					}
				}
			} finally {
				if (graphicsWasTransformed) DoRestoreTransformation();
			}
		}


		/// <override></override>
		void IDiagramPresenter.DrawSelectionFrame(Rectangle frameRect) {
			if (_currentGraphics == null) throw new InvalidOperationException(ErrMessageOnlyAllowedWhilePaining);
			if (_graphicsIsTransformed) throw new NShapeException(ErrMessageResetTransformHasNotBeenCalled);
			DiagramToControl(frameRect, out _rectBuffer);
			if (HighQualityRendering) {
				_currentGraphics.FillRectangle(ToolPreviewBackBrush, _rectBuffer);
				_currentGraphics.DrawRectangle(ToolPreviewPen, _rectBuffer);
			} else {
				ControlPaint.DrawLockedFrame(_currentGraphics, _rectBuffer, false);
				//ControlPaint.DrawFocusRectangle(graphics, rectBuffer, Color.White, Color.Black);
			}
		}


		/// <override></override>
		void IDiagramPresenter.DrawAnglePreview(Point center, Point mousePos, int cursorId, int startAngle, int sweepAngle) {
			if (_currentGraphics == null) throw new InvalidOperationException(ErrMessageOnlyAllowedWhilePaining);
			if (_graphicsIsTransformed) throw new NShapeException(ErrMessageResetTransformHasNotBeenCalled);
			// Get cursor size
			Size cursorSize = _registeredCursors[cursorId].Size;
			// transform diagram coordinates to control coordinates
			int radius = (int)Math.Round(Geometry.DistancePointPoint(center, mousePos));
			DiagramToControl(radius, out radius);
			DiagramToControl(center, out center);
			DiagramToControl(mousePos, out mousePos);
			// Check if the cursor has the minimum distance from the rotation point
			if (radius > _minRotateDistance) {
				// Calculate angle and angle info text
				float startAngleDeg = Geometry.TenthsOfDegreeToDegrees(startAngle);
				float sweepAngleDeg = Geometry.TenthsOfDegreeToDegrees(sweepAngle <= 1800 ? sweepAngle : (sweepAngle - 3600));

				string anglePrefix;
				if (sweepAngleDeg == 0) anglePrefix = string.Empty;
				else if (sweepAngleDeg < 0) anglePrefix = "-";
				else anglePrefix = "+";
				string angleInfoText = null;
				if (SelectedShapes.Count == 1 && SelectedShapes.TopMost is IPlanarShape) {
					float shapeAngleDeg = Geometry.TenthsOfDegreeToDegrees(((IPlanarShape)SelectedShapes.TopMost).Angle);
					angleInfoText = string.Format("{0}° ({1}° {2} {3}°)", (360 + shapeAngleDeg + sweepAngleDeg) % 360, shapeAngleDeg, anglePrefix, Math.Abs(sweepAngleDeg));
				} else
					angleInfoText = string.Format("{0}{1}°", anglePrefix, Math.Abs(sweepAngleDeg));

				// Calculate size of the text's layout rectangle
				Rectangle layoutRect = Rectangle.Empty;
				layoutRect.Size = TextMeasurer.MeasureText(_currentGraphics, angleInfoText, Font, Size.Empty, _previewTextFormatter);
				layoutRect.Width = Math.Min((int)Math.Round(radius * 1.5), layoutRect.Width);
				// Calculate the circumcircle of the LayoutRectangle and the distance between mouse and rotation center...
				float circumCircleRadius = Geometry.DistancePointPoint(-cursorSize.Width / 2f, -cursorSize.Height / 2f, layoutRect.Width, layoutRect.Height) / 2f;
				float mouseDistance = Math.Max(Geometry.DistancePointPoint(center, mousePos), 0.0001f);
				float interpolationFactor = circumCircleRadius / mouseDistance;
				// ... then transform the layoutRectangle towards the mouse cursor
				PointF textCenter = Geometry.VectorLinearInterpolation((PointF)mousePos, (PointF)center, interpolationFactor);
				layoutRect.X = (int)Math.Round(textCenter.X - (layoutRect.Width / 2f));
				layoutRect.Y = (int)Math.Round(textCenter.Y - (layoutRect.Height / 2f));

				// Draw angle pie
				int pieSize = radius + radius;
				if (HighQualityRendering) {
					_currentGraphics.DrawEllipse(ToolPreviewPen, center.X - radius, center.Y - radius, pieSize, pieSize);
					_currentGraphics.FillPie(ToolPreviewBackBrush, center.X - radius, center.Y - radius, pieSize, pieSize, startAngleDeg, sweepAngleDeg);
					_currentGraphics.DrawPie(ToolPreviewPen, center.X - radius, center.Y - radius, pieSize, pieSize, startAngleDeg, sweepAngleDeg);
				} else {
					_currentGraphics.DrawPie(Pens.Black, center.X - radius, center.Y - radius, pieSize, pieSize, startAngleDeg, sweepAngleDeg);
					_currentGraphics.DrawPie(Pens.Black, center.X - radius, center.Y - radius, pieSize, pieSize, startAngleDeg, sweepAngleDeg);
				}
				_currentGraphics.DrawString(angleInfoText, Font, Brushes.Black, layoutRect, _previewTextFormatter);
			} else {
				// If cursor is nearer to the rotation point that the required distance,
				// draw rotation instuction preview
				if (HighQualityRendering) {
					float diameter = _minRotateDistance + _minRotateDistance;
					// draw angle preview circle
					_currentGraphics.DrawEllipse(ToolPreviewPen, center.X - _minRotateDistance, center.Y - _minRotateDistance, diameter, diameter);
					_currentGraphics.FillPie(ToolPreviewBackBrush, center.X - _minRotateDistance, center.Y - _minRotateDistance, diameter, diameter, 0, 45f);
					_currentGraphics.DrawPie(ToolPreviewPen, center.X - _minRotateDistance, center.Y - _minRotateDistance, diameter, diameter, 0, 45f);

					// Draw rotation direction arrow line
					int bowInsetX, bowInsetY;
					bowInsetX = bowInsetY = _minRotateDistance / 4;
					_currentGraphics.DrawArc(ToolPreviewPen, center.X - _minRotateDistance + bowInsetX, center.Y - _minRotateDistance + bowInsetY, diameter - (2 * bowInsetX), diameter - (2 * bowInsetY), 0, 22.5f);
					// Calculate Arrow Tip
					int arrowTipX = 0; int arrowTipY = 0;
					arrowTipX = center.X + _minRotateDistance - bowInsetX;
					arrowTipY = center.Y;
					Geometry.RotatePoint(center.X, center.Y, 45f, ref arrowTipX, ref arrowTipY);
					_arrowShape[0].X = arrowTipX;
					_arrowShape[0].Y = arrowTipY;
					//
					arrowTipX = center.X + _minRotateDistance - bowInsetX - GripSize - GripSize;
					arrowTipY = center.Y;
					Geometry.RotatePoint(center.X, center.Y, 22.5f, ref arrowTipX, ref arrowTipY);
					_arrowShape[1].X = arrowTipX;
					_arrowShape[1].Y = arrowTipY;
					//
					arrowTipX = center.X + _minRotateDistance - bowInsetX + GripSize + GripSize;
					arrowTipY = center.Y;
					Geometry.RotatePoint(center.X, center.Y, 22.5f, ref arrowTipX, ref arrowTipY);
					_arrowShape[2].X = arrowTipX;
					_arrowShape[2].Y = arrowTipY;
					// Draw arrow tip
					_currentGraphics.FillPolygon(ToolPreviewBackBrush, _arrowShape);
					_currentGraphics.DrawPolygon(ToolPreviewPen, _arrowShape);
				} else _currentGraphics.DrawPie(Pens.Black, center.X - _minRotateDistance, center.Y - _minRotateDistance, 2 * _minRotateDistance, 2 * _minRotateDistance, 0, 45f);
			}
		}


		/// <override></override>
		void IDiagramPresenter.DrawLine(Point a, Point b) {
			if (_currentGraphics == null) throw new InvalidOperationException(ErrMessageOnlyAllowedWhilePaining);
			if (_graphicsIsTransformed) throw new NShapeException(ErrMessageResetTransformHasNotBeenCalled);
			DiagramToControl(a, out a);
			DiagramToControl(b, out b);
			_currentGraphics.DrawLine(_outerSnapPen, a, b);
			_currentGraphics.DrawLine(_innerSnapPen, a, b);
		}


		/// <override></override>
		void IDiagramPresenter.OpenCaptionEditor(ICaptionedShape shape, int x, int y) {
			DoOpenCaptionEditor(shape, x, y);
		}


		/// <override></override>
		void IDiagramPresenter.OpenCaptionEditor(ICaptionedShape shape, int labelIndex) {
			DoOpenCaptionEditor(shape, labelIndex, string.Empty);
		}


		/// <override></override>
		void IDiagramPresenter.OpenCaptionEditor(ICaptionedShape shape, int labelIndex, string newText) {
			DoOpenCaptionEditor(shape, labelIndex, newText);
		}


		/// <override></override>
		void IDiagramPresenter.CloseCaptionEditor(bool applyChanges) {
			DoCloseCaptionEditor(applyChanges);
		}


		/// <override></override>
		void IDiagramPresenter.SetCursor(int cursorId) {
			// If cursor was not loaded yet, load it now
			if (!_registeredCursors.ContainsKey(cursorId))
				LoadRegisteredCursor(cursorId);
			if (_registeredCursors[cursorId] != Cursor)
				Cursor = _registeredCursors[cursorId] ?? Cursors.Default;
		}

		#endregion


		#region [Public] IDiagramPresenter Events

		/// <override></override>
		public event EventHandler ShapesSelected;

		/// <override></override>
		public event EventHandler<DiagramPresenterShapeClickEventArgs> ShapeClick;

		/// <override></override>
		public event EventHandler<DiagramPresenterShapeClickEventArgs> ShapeDoubleClick;

		/// <override></override>
		public event EventHandler<DiagramPresenterShapesEventArgs> ShapesInserted;

		/// <override></override>
		public event EventHandler<DiagramPresenterShapesEventArgs> ShapesRemoved;

		/// <override></override>
		public event EventHandler<DiagramPresenterShapeEventArgs> ShapeMoved;

		/// <override></override>
		public event EventHandler<DiagramPresenterShapeEventArgs> ShapeResized;

		/// <override></override>
		public event EventHandler<DiagramPresenterShapeEventArgs> ShapeRotated;

		/// <override></override>
		public event EventHandler<LayersEventArgs> LayerVisibilityChanged;

		/// <override></override>
		public event EventHandler<LayersEventArgs> ActiveLayersChanged;

		/// <override></override>
		public event EventHandler ZoomChanged;

		/// <override></override>
		public event EventHandler DiagramChanging;

		/// <override></override>
		public event EventHandler DiagramChanged;

		/// <override></override>
		public event EventHandler<UserMessageEventArgs> UserMessage;

		#endregion


		#region [Public] IDiagramPresenter Properties

		/// <override></override>
		[CategoryNShape]
		public DiagramSetController DiagramSetController {
			get { return _diagramSetController; }
			set {
				if (_diagramSetController != null) {
					UnregisterDiagramSetControllerEvents();
					_privateTool = _diagramSetController.ActiveTool;
				}
				_diagramSetController = value;
				if (_diagramSetController != null) {
					RegisterDiagramSetControllerEvents();
					if (_privateTool != null)
						_diagramSetController.ActiveTool = _privateTool;
				}
			}
		}


		/// <override></override>
		[ReadOnly(true)]
		[Browsable(false)]
		[CategoryNShape]
		public Diagram Diagram {
			get { return _diagramController == null ? null : _diagramController.Diagram; }
			set {
				if (_diagramSetController == null) throw new ArgumentNullException("DiagramSetController");
				OnDiagramChanging(EventArgs.Empty);
				if (Diagram != null)
					DiagramController = null;	// Close diagram and unregister events
				if (value != null)
					DiagramController = _diagramSetController.OpenDiagram(value);	// Register events
				OnDiagramChanged(EventArgs.Empty);
			}
		}


		/// <summary>
		/// Provides access to a <see cref="T:Dataweb.NShape.Project" />.
		/// </summary>
		[Browsable(false)]
		public Project Project {
			get { return (_diagramSetController == null) ? null : _diagramSetController.Project; }
		}


		/// <override></override>
		[Browsable(false)]
		public IShapeCollection SelectedShapes {
			get { return _selectedShapes; }
		}


		/// <override></override>
		[Browsable(false)]
		public ICollection<int> ActiveLayerIds {
			get { return _activeLayerIds; }
		}


		/// <override></override>
		[Obsolete("Use ActiveSupplementalLayers instead.")]
		[Browsable(false)]
		public LayerIds ActiveLayers {
			get { return ActiveSupplementalLayers; }
		}


		/// <override></override>
		[Browsable(false)]
		public LayerIds ActiveSupplementalLayers {
			get { return _activeCombinableLayers; }
		}


		/// <override></override>
		[Browsable(false)]
		public int ActiveHomeLayer {
			get { return _activeNonCombinableLayer; }
		}


		/// <override></override>
		[Obsolete("Use HiddenLayerIds instead.")]
		[Browsable(false)]
		public LayerIds HiddenLayers {
			get { return _hiddenSharedLayers; }
		}


		/// <override></override>
		[Browsable(false)]
		public ICollection<int> HiddenLayerIds {
			get { return _hiddenLayerIds; }
		}

		#endregion


		#region [Public] Properties

		/// <summary>
		/// Specifies the version of the assembly containing the component.
		/// </summary>
		[CategoryNShape()]
		[Browsable(true)]
		public new string ProductVersion {
			get { return base.ProductVersion; }
		}


		/// <summary>
		/// The PropertyController for editing shapes, model objects and diagrams.
		/// </summary>
		[CategoryNShape()]
		public PropertyController PropertyController {
			get { return _propertyController; }
			set { _propertyController = value; }
		}


		/// <summary>
		/// The currently active tool.
		/// </summary>
		[ReadOnly(true)]
		[Browsable(false)]
		[CategoryNShape()]
		public Tool ActiveTool {
			get { return (_diagramSetController == null) ? _privateTool : _diagramSetController.ActiveTool; }
			set {
				if (_diagramSetController != null) _diagramSetController.ActiveTool = value;
				else _privateTool = value;
				if (ActiveTool.MinimumDragDistance != SystemInformation.DoubleClickSize)
					ActiveTool.MinimumDragDistance = SystemInformation.DoubleClickSize;
				if (ActiveTool.DoubleClickTime != SystemInformation.DoubleClickTime)
					ActiveTool.DoubleClickTime = SystemInformation.DoubleClickTime;
			}
		}


		/// <summary>
		/// The currently active tool.
		/// </summary>
		[Obsolete("Use ActiveTool instead")]
		[ReadOnly(true)]
		[Browsable(false)]
		public Tool CurrentTool {
			get { return ActiveTool; }
			set { ActiveTool = value; }
		}


		/// <summary>
		/// Bounds of the display's drawing area (client area minus scroll bars) in control coordinates
		/// </summary>
		[Browsable(false)]
		public Rectangle DrawBounds {
			get {
				if (!Geometry.IsValid(_drawBounds)) {
					_drawBounds.X = _drawBounds.Y = 0;
					if (VScrollBarVisible) _drawBounds.Width = Width - scrollBarV.Width;
					else _drawBounds.Width = Width;
					if (HScrollBarVisible) _drawBounds.Height = Height - scrollBarH.Height;
					else _drawBounds.Height = Height;
				}
				return _drawBounds;
			}
		}


		/// <override></override>
		public override ContextMenuStrip ContextMenuStrip {
			get {
				ContextMenuStrip result = base.ContextMenuStrip;
				if (result == null && ShowDefaultContextMenu)
					result = displayContextMenuStrip;
				return result;
			}
			set {
				base.ContextMenuStrip = value;
			}
		}

		#endregion


		#region [Public] Properties: Behavior

		/// <summary>
		/// Specifies if MenuItemDefs that are not granted should appear as MenuItems in the dynamic context menu.
		/// </summary>
		[CategoryBehavior()]
		[DefaultValue(Display.DefaultValueHideMenuItemsIfNotGranted)]
		public bool HideDeniedMenuItems {
			get { return _hideMenuItemsIfNotGranted; }
			set { _hideMenuItemsIfNotGranted = value; }
		}


		/// <summary>
		/// Enables or disables direct zooming with mouse wheel.
		/// True means zooming when rotating the mouse wheel,
		/// False means zooming when pressing Ctrl and rotating the mouse wheel.
		/// </summary>
		[CategoryBehavior()]
		[DefaultValue(Display.DefaultValueZoomWithMouseWheel)]
		public bool ZoomWithMouseWheel {
			get { return _zoomWithMouseWheel; }
			set { _zoomWithMouseWheel = value; }
		}


		/// <summary>
		/// Shows or hides scroll bars.
		/// </summary>
		[CategoryBehavior()]
		[DefaultValue(Display.DefaultValueShowScrollBars)]
		public bool ShowScrollBars {
			get { return _showScrollBars; }
			set { _showScrollBars = value; }
		}


		/// <summary>
		/// Enables or disables snapping of shapes and control points to grid lines.
		/// </summary>
		[CategoryBehavior()]
		[DefaultValue(Display.DefaultValueSnapToGrid)]
		public bool SnapToGrid {
			get { return ((Control.ModifierKeys & (Keys.Control | Keys.ControlKey)) == 0) ? _snapToGrid : !_snapToGrid; }
			set { _snapToGrid = value; }
		}


		/// <summary>
		/// Specifies the distance for snapping shapes and control points to grid lines.
		/// </summary>
		[CategoryBehavior()]
		[DefaultValue(Display.DefaultValueSnapDistance)]
		public int SnapDistance {
			get { return _snapDistance; }
			set { _snapDistance = value; }
		}


		/// <summary>
		/// If true, the standard context menu is created from MenuItemDefs. 
		/// If false, a user defined context menu is shown without creating additional menu items.
		/// </summary>
		[CategoryBehavior()]
		[DefaultValue(Display.DefaultValueShowDefaultContextMenu)]
		public bool ShowDefaultContextMenu {
			get { return _showDefaultContextMenu; }
			set { _showDefaultContextMenu = value; }
		}


		/// <summary>
		/// Specifies the minimum distance of the mouse cursor from the shape's rotate point while rotating.
		/// </summary>
		[CategoryBehavior()]
		[DefaultValue(Display.DefaultValueMinRotateDistance)]
		public int MinRotateRange {
			get { return _minRotateDistance; }
			set { _minRotateDistance = value; }
		}

		#endregion


		#region [Public] Properties: Appearance

		/// <summary>
		/// Zoom in percentage.
		/// </summary>
		[CategoryAppearance()]
		[DefaultValue(Display.DefaultValueZoomLevel)]
		public int ZoomLevel {
			get { return _zoomLevel; }
			set {
				if (value < 0) throw new ArgumentException(Dataweb.NShape.Properties.Resources.MessageTxt_ValueHasToBeGreaterThanZero);
				if (_zoomLevel != value) {
					_zoomLevel = value;
					_zoomfactor = Math.Max(value / 100f, 0.001f);

					UnselectShapesOfInvisibleLayers();
					DoCloseCaptionEditor(true);

					ResetBounds();
					UpdateScrollBars();
					Invalidate();

					OnZoomChanged(EventArgs.Empty);
				}
			}
		}


		/// <summary>
		/// Specifies the distance between the grid lines.
		/// </summary>
		[CategoryAppearance()]
		[DefaultValue(Display.DefaultValueGridSize)]
		public int GridSize {
			get { return this._gridSpace; }
			set {
				if (value <= 0) throw new ArgumentException(Dataweb.NShape.Properties.Resources.MessageTxt_ValueHasToBeGreaterThanZero);
				_gridSpace = value;
				_gridSize.Width = _gridSpace;
				_gridSize.Height = _gridSpace;
			}
		}


		/// <summary>
		/// The radius of a control point grip from the center to the outer handle bound.
		/// </summary>
		[CategoryAppearance()]
		[DefaultValue(Display.DefaultValueGripSize)]
		public int GripSize {
			get { return _handleRadius; }
			set {
				if (value <= 0) throw new ArgumentOutOfRangeException("value");
				else {
					_handleRadius = value;
					_invalidateDelta = _handleRadius;

					CalcControlPointShape(_rotatePointPath, ControlPointShape.RotateArrow, _handleRadius);
					CalcControlPointShape(_resizePointPath, _resizePointShape, _handleRadius);
					CalcControlPointShape(_connectionPointPath, _connectionPointShape, _handleRadius);
					Invalidate();
				}
			}
		}


		/// <summary>Will soon be removed. Use IsGridVisible instead.</summary>
		[Obsolete("Use IsGridVisible instead")]
		[Browsable(false)]
		[ReadOnly(true)]
		public bool ShowGrid {
		    get { return IsGridVisible; }
		    set { IsGridVisible = value;}
		}

	
		/// <summary>
		/// Specifies whether grid lines should be visible.
		/// </summary>
		[CategoryAppearance()]
		[DefaultValue(Display.DefaultValueShowGrid)]
		public bool IsGridVisible {
			get { return _gridVisible; }
			set {
				_gridVisible = value;
				Invalidate(this._drawBounds);
			}
		}


		/// <summary>Specifies whether the diagram sheet (including its shadow) is visible or not.</summary>
		[CategoryAppearance()]
		[DefaultValue(true)]
		public bool IsSheetVisible {
			get { return _drawDiagramSheet; }
			set {
				_drawDiagramSheet = value;
				Invalidate();
			}
		}


#if DEBUG_UI

		/// <summary>Will soon be removed. Use IsDebugInfoInvalidateVisible instead.</summary>
		[Obsolete("Use IsDebugInfoInvalidateVisible instead.")]
		[Browsable(false)]
		[ReadOnly(true)]
		public bool ShowInvalidatedAreas {
			get { return IsDebugInfoInvalidateVisible; }
			set { IsDebugInfoInvalidateVisible = value; }
		}


		/// <summary>
		/// Specifies whether grid lines should be visible.
		/// </summary>
		[CategoryAppearance()]
		[Browsable(false)]
		[ReadOnly(true)]
		public bool IsDebugInfoInvalidateVisible {
			get { return _isInvalidateAreasVisible; }
			set {
				_isInvalidateAreasVisible = value;
				if (_isInvalidateAreasVisible) {
					if (_clipRectBrush1 == null) _clipRectBrush1 = new SolidBrush(_invalidatedAreaColor1);
					if (_clipRectBrush2 == null) _clipRectBrush2 = new SolidBrush(_invalidatedAreaColor2);
					if (_invalidatedAreaPen1 == null) _invalidatedAreaPen1 = new Pen(_invalidatedAreaColor1);
					if (_invalidatedAreaPen2 == null) _invalidatedAreaPen2 = new Pen(_invalidatedAreaColor2);
					_invalidatedAreas = new List<Rectangle>();
				} else {
					_clipRectBrush = null;
					_invalidatedAreaPen = null;
					GdiHelpers.DisposeObject(ref _clipRectBrush1);
					GdiHelpers.DisposeObject(ref _clipRectBrush2);
					GdiHelpers.DisposeObject(ref _invalidatedAreaPen1);
					GdiHelpers.DisposeObject(ref _invalidatedAreaPen2);
					if (_invalidatedAreas != null) {
						_invalidatedAreas.Clear();
						_invalidatedAreas = null;
					}
				}
				Invalidate();
			}
		}


		/// <summary>Will soon be removed. Use IsDebugInfoCellOccupationVisible instead.</summary>
		[Obsolete("Use IsDebugInfoCellOccupationVisible instead.")]
		[Browsable(false)]
		[ReadOnly(true)]
		public bool ShowCellOccupation {
			get { return IsDebugInfoCellOccupationVisible; }
			set { IsDebugInfoCellOccupationVisible = value; }
		}


		/// <summary>
		/// Specifies whether grid lines should be visible.
		/// </summary>
		[CategoryAppearance()]
		[Browsable(false)]
		[ReadOnly(true)]
		public bool IsDebugInfoCellOccupationVisible {
			get { return _isCellOccupationVisible; }
			set {
				_isCellOccupationVisible = value;
				Invalidate();
			}
		}

#endif


		/// <summary>
		/// Specifies whether high quality rendering settings should be allpied.
		/// </summary>
		[CategoryAppearance()]
		[DefaultValue(Display.DefaultValueHighQualityRendering)]
		public bool HighQualityRendering {
			get { return _highQualityRendering; }
			set {
				_highQualityRendering = value;
				_currentRenderingQuality = _highQualityRendering ? _renderingQualityHigh : _renderingQualityLow;
				DisposeObject(ref _controlBrush);
				if (_infoGraphics != null) GdiHelpers.ApplyGraphicsSettings(_infoGraphics, _currentRenderingQuality);
				Invalidate();
			}
		}


		/// <summary>
		/// Specifies whether the control's background should bew rendered in high quality.
		/// </summary>
		[CategoryAppearance()]
		[DefaultValue(Display.DefaultValueHighQualityBackground)]
		public bool HighQualityBackground {
			get { return _highQualityBackground; }
			set {
				_highQualityBackground = value;
				if (Diagram != null)
					Diagram.HighQualityRendering = value;
				DisposeObject(ref _controlBrush);
				if (_infoGraphics != null) GdiHelpers.ApplyGraphicsSettings(_infoGraphics, _currentRenderingQuality);
				Invalidate();
			}
		}


		/// <summary>
		/// Specifies the shape of grips used for resizing shapes.
		/// </summary>
		[CategoryAppearance()]
		[DefaultValue(Display.DefaultValueResizePointShape)]
		public ControlPointShape ResizeGripShape {
			get { return _resizePointShape; }
			set {
				_resizePointShape = value;
				CalcControlPointShape(_resizePointPath, _resizePointShape, _handleRadius);
				Invalidate();
			}
		}


		/// <summary>
		/// Specifies the shape of connection points provided by a shape.
		/// </summary>
		[CategoryAppearance()]
		[DefaultValue(Display.DefaultValueConnectionPointShape)]
		public ControlPointShape ConnectionPointShape {
			get { return _connectionPointShape; }
			set {
				_connectionPointShape = value;
				CalcControlPointShape(_connectionPointPath, _connectionPointShape, _handleRadius);
				Invalidate();
			}
		}


		/// <summary>
		/// Specifies the angle of the background color gradient.
		/// </summary>
		[CategoryAppearance()]
		[DefaultValue(Display.DefaultValueBackgroundGradientAngle)]
		public int BackgroundGradientAngle {
			get { return _controlBrushGradientAngle; }
			set {
				int x = Math.Abs(value) / 360;
				_controlBrushGradientAngle = (((x * 360) + value) % 360);
				_controlBrushGradientSin = Math.Sin(Geometry.DegreesToRadians(_controlBrushGradientAngle));
				_controlBrushGradientCos = Math.Cos(Geometry.DegreesToRadians(_controlBrushGradientAngle));
				DisposeObject(ref _controlBrush);
				Invalidate();
			}
		}


		/// <summary>
		/// Specifies the control's background color. This defines also the base color of the color gradient.
		/// </summary>
		[CategoryAppearance()]
		//[DefaultValue(Display.DefaultValueBackColor)]
		public override Color BackColor {
			get { return base.BackColor; }
			set {
				base.BackColor = value;
				hScrollBarPanel.BackColor = value;
				DisposeObject(ref _controlBrush);
				Invalidate();
			}
		}


		/// <summary>
		/// Specifies the gradient color of the background color gradient.
		/// </summary>
		[CategoryAppearance()]
		//[DefaultValue(Display.DefaultValueBackColorGradient)]
		public Color BackColorGradient {
			get { return _gradientBackColor; }
			set {
				_gradientBackColor = value;
				DisposeObject(ref _controlBrush);
				Invalidate();
			}
		}


		/// <summary>
		/// Specifies the transparency of the grid lines.
		/// </summary>
		[CategoryAppearance()]
		[DefaultValue(Display.DefaultValueGridAlpha)]
		public byte GridAlpha {
			get { return _gridAlpha; }
			set {
				_gridAlpha = value;
				DisposeObject(ref _gridPen);
				Invalidate();
			}
		}


		/// <summary>
		/// Specifies the transparenc of control point grips.
		/// </summary>
		[CategoryAppearance()]
		[DefaultValue(Display.DefaultValueSelectionAlpha)]
		public byte ControlPointAlpha {
			get { return _selectionAlpha; }
			set {
				_selectionAlpha = value;

				DisposeObject(ref _handleInteriorBrush);
				DisposeObject(ref _outlineNormalPen);
				DisposeObject(ref _outlineHilightPen);
				DisposeObject(ref _outlineInactivePen);
				Invalidate();
			}
		}


		/// <summary>
		/// Specifies the color of the grid lines.
		/// </summary>
		[CategoryAppearance()]
		public Color GridColor {
			get { return _gridColor; }
			set {
				_gridColor = value;
				DisposeObject(ref _gridPen);
				Invalidate();
			}
		}


		/// <summary>
		/// Specifies the inner color of the selection indicator.
		/// </summary>
		[CategoryAppearance()]
		public Color SelectionInteriorColor {
			get { return _selectionInteriorColor; }
			set {
				if (_hintBackgroundStyle != null) {
					ToolCache.NotifyStyleChanged(_hintBackgroundStyle);
					_hintBackgroundStyle = null;
				}
				_selectionInteriorColor = value;

				DisposeObject(ref _outlineInteriorPen);
				DisposeObject(ref _handleInteriorBrush);
				Invalidate();
			}
		}


		/// <summary>
		/// Specifies the color of the selection indicator.
		/// </summary>
		[CategoryAppearance()]
		public Color SelectionNormalColor {
			get { return _selectionNormalColor; }
			set {
				_selectionNormalColor = value;
				DisposeObject(ref _outlineNormalPen);
				Invalidate();
			}
		}


		/// <summary>
		/// Specifies the color of a highlighted selection.
		/// </summary>
		[CategoryAppearance()]
		public Color SelectionHilightColor {
			get { return _selectionHilightColor; }
			set {
				_selectionHilightColor = value;
				DisposeObject(ref _outlineNormalPen);
				Invalidate();
			}
		}


		/// <summary>
		/// Specifies the color of an inactive/deactivated selection.
		/// </summary>
		[CategoryAppearance()]
		public Color SelectionInactiveColor {
			get { return _selectionInactiveColor; }
			set {
				_selectionInactiveColor = value;
				DisposeObject(ref _outlineNormalPen);
				Invalidate();
			}
		}


		/// <summary>
		/// Specifies the color of tool preview hints.
		/// </summary>
		[CategoryAppearance()]
		public Color ToolPreviewColor {
			get { return _toolPreviewColor; }
			set {
				if (_hintForegroundStyle != null) {
					ToolCache.NotifyStyleChanged(_hintForegroundStyle);
					_hintForegroundStyle = null;
				}
				_toolPreviewColor = value;
				DisposeObject(ref _toolPreviewPen);
				Invalidate();
			}
		}


		/// <summary>
		/// Specifies the color the background color of tool preview hints.
		/// </summary>
		[CategoryAppearance()]
		public Color ToolPreviewBackColor {
			get { return _toolPreviewBackColor; }
			set {
				_toolPreviewBackColor = value;
				DisposeObject(ref _toolPreviewBackBrush);
				Invalidate();
			}
		}


		/// <summary>
		/// Specifies the rendering quality in high quality mode
		/// </summary>
		[CategoryAppearance()]
		[DefaultValue(Display.DefaultValueRenderingQualityHigh)]
		public RenderingQuality RenderingQualityHighQuality {
			get { return _renderingQualityHigh; }
			set {
				_renderingQualityHigh = value;
				if (_highQualityRendering) {
					_currentRenderingQuality = _renderingQualityHigh;
					if (_infoGraphics != null) GdiHelpers.ApplyGraphicsSettings(_infoGraphics, _currentRenderingQuality);
				}
			}
		}


		/// <summary>
		/// Specifies the rendering quality in low quality mode
		/// </summary>
		[CategoryAppearance()]
		[DefaultValue(Display.DefaultValueRenderingQualityLow)]
		public RenderingQuality RenderingQualityLowQuality {
			get { return _renderingQualityLow; }
			set {
				_renderingQualityLow = value;
				if (!_highQualityRendering) {
					_currentRenderingQuality = _renderingQualityLow;
					if (_infoGraphics != null) GdiHelpers.ApplyGraphicsSettings(_infoGraphics, _currentRenderingQuality);
				}
			}
		}

		#endregion


		#region [Public] Methods: Coordinate transformation

		/// <summary>
		/// Transformes diagram coordinates to control coordinates
		/// </summary>
		public void DiagramToControl(int dX, int dY, out int cX, out int cY) {
			cX = _diagramPosX + (int)Math.Round((dX - _scrollPosX) * _zoomfactor);
			cY = _diagramPosY + (int)Math.Round((dY - _scrollPosY) * _zoomfactor);
		}


		/// <summary>
		/// Transformes diagram coordinates to control coordinates
		/// </summary>
		public void DiagramToControl(Point dPt, out Point cPt) {
			cPt = Point.Empty;
			cPt.Offset(
				_diagramPosX + (int)Math.Round((dPt.X - _scrollPosX) * _zoomfactor),
				_diagramPosY + (int)Math.Round((dPt.Y - _scrollPosY) * _zoomfactor)
				);
		}


		/// <summary>
		/// Transformes diagram coordinates to control coordinates
		/// </summary>
		public void DiagramToControl(Rectangle dRect, out Rectangle cRect) {
			cRect = Rectangle.Empty;
			cRect.Offset(
				_diagramPosX + (int)Math.Round((dRect.X - _scrollPosX) * _zoomfactor),
				_diagramPosY + (int)Math.Round((dRect.Y - _scrollPosY) * _zoomfactor)
				);
			cRect.Width = (int)Math.Round(dRect.Width * _zoomfactor);
			cRect.Height = (int)Math.Round(dRect.Height * _zoomfactor);
		}


		/// <summary>
		/// Transformes diagram coordinates to control coordinates
		/// </summary>
		public void DiagramToControl(int dDistance, out int cDistance) {
			cDistance = (int)Math.Round(dDistance * _zoomfactor);
		}


		/// <summary>
		/// Transformes diagram coordinates to control coordinates
		/// </summary>
		public void DiagramToControl(Size dSize, out Size cSize) {
			cSize = Size.Empty;
			cSize.Width = (int)Math.Round(dSize.Width * _zoomfactor);
			cSize.Height = (int)Math.Round(dSize.Height * _zoomfactor);
		}


		/// <summary>
		/// Transformes control coordinates to diagram coordinates
		/// </summary>
		public void ControlToDiagram(int cX, int cY, out int dX, out int dY) {
			dX = (int)Math.Round((cX - _diagramPosX) / _zoomfactor) + _scrollPosX;
			dY = (int)Math.Round((cY - _diagramPosY) / _zoomfactor) + _scrollPosY;
		}


		/// <summary>
		/// Transformes control coordinates to diagram coordinates
		/// </summary>
		public void ControlToDiagram(Point cPt, out Point dPt) {
			dPt = Point.Empty;
			dPt.X = (int)Math.Round((cPt.X - _diagramPosX) / _zoomfactor) + _scrollPosX;
			dPt.Y = (int)Math.Round((cPt.Y - _diagramPosY) / _zoomfactor) + _scrollPosY;
		}


		/// <summary>
		/// Transformes control coordinates to diagram coordinates
		/// </summary>
		public void ControlToDiagram(Rectangle cRect, out Rectangle dRect) {
			dRect = Rectangle.Empty;
			dRect.X = (int)Math.Round((cRect.X - _diagramPosX) / _zoomfactor) + _scrollPosX;
			dRect.Y = (int)Math.Round((cRect.Y - _diagramPosY) / _zoomfactor) + _scrollPosY;
			dRect.Width = (int)Math.Round((cRect.Width / _zoomfactor));
			dRect.Height = (int)Math.Round((cRect.Height / _zoomfactor));
		}


		/// <summary>
		/// Transformes control coordinates to diagram coordinates
		/// </summary>
		public void ControlToDiagram(Size cSize, out Size dSize) {
			dSize = Size.Empty;
			dSize.Width = (int)Math.Round((cSize.Width / _zoomfactor));
			dSize.Height = (int)Math.Round((cSize.Height / _zoomfactor));
		}


		/// <summary>
		/// Transformes control coordinates to diagram coordinates
		/// </summary>
		public void ControlToDiagram(int cDistance, out int dDistance) {
			dDistance = (int)Math.Round((cDistance / _zoomfactor));
		}


		/// <summary>
		/// Transformes screen coordinates to diagram coordinates
		/// </summary>
		public void ScreenToDiagram(Point sPt, out Point dPt) {
			ControlToDiagram(PointToClient(sPt), out dPt);
		}


		/// <summary>
		/// Transformes screen coordinates to diagram coordinates
		/// </summary>
		public void ScreenToDiagram(Rectangle sRect, out Rectangle dRect) {
			ControlToDiagram(RectangleToClient(sRect), out dRect);
		}

		#endregion


		#region [Public] Methods: (Un)Selecting shapes

		/// <summary>
		/// Clears the current selection.
		/// </summary>
		public void UnselectAll() {
			if (_selectedShapes.Count > 0) {
				ClearSelection();
				PerformSelectionNotifications();
			}
		}


		/// <summary>
		/// Removes the given Shape from the current selection.
		/// </summary>
		public void UnselectShape(Shape shape) {
			if (shape == null) throw new ArgumentNullException("shape");
			if (_selectedShapes.Count > 0) {
				DoUnselectShape(shape);
				PerformSelectionNotifications();
			}
		}


		/// <summary>
		/// Removes the given Shape from the current selection.
		/// </summary>
		private void UnselectShapes(IEnumerable<Shape> shapes) {
			if (shapes == null) throw new ArgumentNullException("shapes");
			if (_selectedShapes.Count > 0) {
				DoSuspendUpdate();
				foreach (Shape s in shapes) {
					if (_selectedShapes.Contains(s))
						DoUnselectShape(s);
				}
				DoResumeUpdate();
				PerformSelectionNotifications();
			}
		}


		/// <summary>
		/// Selects the given shape. Current selection will be cleared.
		/// </summary>
		public void SelectShape(Shape shape) {
			SelectShape(shape, false);
		}


		/// <summary>
		/// Selects the given shape.
		/// </summary>
		/// <param name="shape">Shape to be selected.</param>
		/// <param name="addToSelection">If true, the given shape will be added to the current selection, otherwise the current selection will be cleared before selecting this shape.</param>
		public void SelectShape(Shape shape, bool addToSelection) {
			if (shape == null) throw new ArgumentNullException("shape");
			DoSelectShape(shape, addToSelection);
			PerformSelectionNotifications();
		}


		/// <summary>
		/// Selects the given shape.
		/// </summary>
		/// <param name="shapes">Shape to be selected.</param>
		/// <param name="addToSelection">If true, the given shape will be added to the current selection, otherwise the current selection will be cleared before selecting this shape.</param>
		public void SelectShapes(IEnumerable<Shape> shapes, bool addToSelection) {
			if (shapes == null) throw new ArgumentNullException("shapes");
			if (!addToSelection)
				ClearSelection();
			if (SelectedShapes.ContainsAll(shapes))
				SelectedShapes.RemoveRange(shapes);
			else {
				foreach (Shape shape in shapes) {
					Debug.Assert(Diagram.Shapes.Contains(shape));
					DoSelectShape(shape, true);
				}
			}
			PerformSelectionNotifications();
		}


		/// <summary>
		/// Selects all shapes within the given area.
		/// </summary>
		/// <param name="area">All shapes in the given rectangle will be selected.</param>
		/// <param name="addToSelection">If true, the given shape will be added to the current selection, otherwise the current selection will be cleared before selecting this shape.</param>
		public void SelectShapes(Rectangle area, bool addToSelection) {
			if (Diagram != null) {
				// ensure rectangle width and height are positive
				if (area.Size.Width < 0) {
					area.Width = Math.Abs(area.Width);
					area.X = area.X - area.Width;
				}
				if (area.Size.Height < 0) {
					area.Height = Math.Abs(area.Height);
					area.Y = area.Y - area.Height;
				}
				SelectShapes(Diagram.Shapes.FindShapes(area.X, area.Y, area.Width, area.Height, true), addToSelection);
			}
		}


		/// <summary>
		/// Selects all shapes of the given shape type.
		/// </summary>
		public void SelectShapes(ShapeType shapeType, bool addToSelection) {
			if (shapeType == null) throw new ArgumentNullException("shapeType");
			if (Diagram != null) {
				// Find all shapes of the same ShapeType
				_shapeBuffer.Clear();
				foreach (Shape shape in Diagram.Shapes) {
					if (shape.Type == shapeType)
						_shapeBuffer.Add(shape);
				}
				SelectShapes(_shapeBuffer, addToSelection);
			}
		}


		/// <summary>
		/// Selects all shapes based on the given template.
		/// </summary>
		public void SelectShapes(Template template, bool addToSelection) {
			if (template == null) throw new ArgumentNullException("template");
			if (Diagram != null) {
				// Find all shapes of the same ShapeType
				_shapeBuffer.Clear();
				foreach (Shape shape in Diagram.Shapes) {
					if (shape.Template == template)
						_shapeBuffer.Add(shape);
				}
				SelectShapes(_shapeBuffer, addToSelection);
			}
		}


		/// <summary>
		/// Selects all shapes of the diagram.
		/// </summary>
		public void SelectAll() {
			DoSuspendUpdate();
			_shapesConnectedToSelectedShapes.Clear();
			_selectedShapes.Clear();

			_selectedShapes.AddRange(Diagram.Shapes);
			foreach (Shape shape in _selectedShapes.BottomUp)
				DoInvalidateShape(shape);
			DoResumeUpdate();
			PerformSelectionNotifications();
		}

		#endregion


		#region [Public] Methods

		/// <summary>
		/// Fetches the indicated diagram from the repository and displays it.
		/// </summary>
		//[Obsolete("Use OpenDiagram method instead.")]
		public bool LoadDiagram(string diagramName) {
			return OpenDiagram(diagramName);
		}


		/// <summary>
		/// Fetches the indicated diagram from the repository and displays it.
		/// </summary>
		public bool OpenDiagram(string diagramName) {
			if (diagramName == null) throw new ArgumentNullException("diagramName");
			if (DiagramSetController == null) throw new NShapeException("Property DiagramSetController is not set!");
			bool result = false;
			// Clear current selectedShapes and models
			if (Project.Repository == null)
				throw new NShapeException("Repository is not set to an instance of IRepository.");
			if (!Project.Repository.IsOpen)
				throw new NShapeException("Repository is not open.");

			Diagram d = Project.Repository.GetDiagram(diagramName);
			if (d != null) {
				// Use property setter because it updates the shape's display service 
				// and loads all diagram shapes from repository
				Diagram = d;
				result = true;
			}

			UpdateScrollBars();
			Refresh();
			return result;
		}


		/// <summary>
		/// Creates the indicated diagram, inserts it into the repository and displays it.
		/// </summary>
		public bool CreateDiagram(string diagramName) {
			if (diagramName == null) throw new ArgumentNullException("diagramName");
			if (DiagramSetController == null) throw new NShapeException("Property DiagramSetController is not set!");
			bool result = false;
			// Clear current selectedShapes and models
			if (Project.Repository == null)
				throw new NShapeException("Repository is not set to an instance of IRepository.");
			if (!Project.Repository.IsOpen)
				throw new NShapeException("Repository is not open.");

			// Clear current content of the display
			Clear();

			Diagram d = new Diagram(diagramName);
			Project.Repository.Insert(d);
			Diagram = d;
			result = true;

			UpdateScrollBars();
			Refresh();
			return result;
		}


		/// <summary>
		/// Clears diagram and all buffers.
		/// </summary>
		public void Clear() {
			//ClearSelection();
			if (Diagram != null) Diagram = null;
			_selectedShapes.Clear();
			_shapeBuffer.Clear();
			_editBuffer.Clear();
			Invalidate();
		}


		/// <summary>
		/// Returns a collection of <see cref="T:Dataweb.NShape.Advanced.MenuItemDef" /> for constructing context menus etc.
		/// </summary>
		public IEnumerable<MenuItemDef> GetMenuItemDefs() {
			if (Diagram != null) {
				Point mousePos = Point.Empty;
				ScreenToDiagram(Control.MousePosition, out mousePos);
				// Fins the nearest (topmost) shape under the cursor (selected shapes are preferred)
				Shape shapeUnderCursor = null;
				if (_selectedShapes.Count > 0)
					shapeUnderCursor = _selectedShapes.FindShape(mousePos.X, mousePos.Y, ControlPointCapabilities.None, 0, null);
				if (shapeUnderCursor == null) 
					shapeUnderCursor = Diagram.Shapes.FindShape(mousePos.X, mousePos.Y, ControlPointCapabilities.None, 0, null);
				
				bool modelObjectsAssigned = ModelObjectsAssigned(_selectedShapes);

				#region Context menu structure of the display specific context menu
				// Select...
				// Bring to front
				// Send to bottom
				// --------------------------
				// Add Shapes to active Layers
				// Assign Shapes to active Layers
				// Remove Shapes from all Layers
				// --------------------------
				// Group shapes
				// Ungroup shapes
				// Aggregate composite shape
				// Split composite shape
				// --------------------------
				// Cut
				// Copy
				// Paste
				// Delete
				// --------------------------
				// Diagram Properties
				// --------------------------
				// Undo
				// Redo
				//
				#endregion

				// Create a action group
				yield return new GroupMenuItemDef(MenuItemNameSelectShapesGroupItem, 
					Properties.Resources.CaptionTxt_GroupSelect, null, 
					Properties.Resources.TooltipTxt_GroupSelect, true,
					new MenuItemDef[] {
						CreateSelectAllMenuItemDef(),
						CreateSelectByTemplateMenuItemDef(shapeUnderCursor),
						CreateSelectByTypeMenuItemDef(shapeUnderCursor),
						CreateUnselectAllMenuItemDef()
					}, -1);
				yield return new SeparatorMenuItemDef();
#if DEBUG_UI
				yield return CreateShowShapeInfoMenuItemDef(Diagram, _selectedShapes);
				yield return new SeparatorMenuItemDef();
#endif
				yield return CreateBringToFrontMenuItemDef(Diagram, _selectedShapes);
				yield return CreateSendToBackMenuItemDef(Diagram, _selectedShapes);
				yield return new SeparatorMenuItemDef();
				yield return CreateAddShapesToLayersMenuItemDef(Diagram, _selectedShapes, ActiveHomeLayer, ActiveSupplementalLayers, false);
				yield return CreateAddShapesToLayersMenuItemDef(Diagram, _selectedShapes, ActiveHomeLayer, ActiveSupplementalLayers, true);
				yield return CreateRemoveShapesFromLayersMenuItemDef(Diagram, _selectedShapes);
				yield return new SeparatorMenuItemDef();
				yield return CreateGroupShapesMenuItemDef(Diagram, _selectedShapes, ActiveHomeLayer, ActiveSupplementalLayers);
				yield return CreateUngroupMenuItemDef(Diagram, _selectedShapes);
				yield return CreateAggregateMenuItemDef(Diagram, _selectedShapes, ActiveHomeLayer, ActiveSupplementalLayers, mousePos);
				yield return CreateUnaggregateMenuItemDef(Diagram, _selectedShapes);
				yield return new SeparatorMenuItemDef();
				yield return CreateCutMenuItemDef(Diagram, _selectedShapes, modelObjectsAssigned, mousePos);
				yield return CreateCopyImageMenuItemDef(Diagram, _selectedShapes);
				yield return CreateCopyMenuItemDef(Diagram, _selectedShapes, modelObjectsAssigned, mousePos);
				yield return CreatePasteMenuItemDef(Diagram, _selectedShapes, mousePos);
				yield return CreateDeleteMenuItemDef(Diagram, _selectedShapes, modelObjectsAssigned);
				if (_propertyController != null) {
					yield return new SeparatorMenuItemDef();
					yield return CreatePropertiesMenuItemDef(Diagram, _selectedShapes, mousePos);
				}
				yield return new SeparatorMenuItemDef();
				yield return CreateUndoMenuItemDef();
				yield return CreateRedoMenuItemDef();
			}
		}


		/// <summary>
		/// Inserts the given shape in the displayed diagram.
		/// </summary>
		public void InsertShape(Shape shape) {
			InsertShapes(SingleInstanceEnumerator<Shape>.Create(shape));
		}


		/// <summary>
		/// Inserts the given shapes in the displayed diagram.
		/// </summary>
		public void InsertShapes(IEnumerable<Shape> shapes) {
			if (Diagram != null) {
				DiagramSetController.InsertShapes(Diagram, shapes, ActiveHomeLayer, ActiveSupplementalLayers, true);
				OnShapesInserted(GetShapesEventArgs(shapes));
				SelectShapes(shapes, false);
			}
		}


		/// <summary>
		/// Deletes the selected shapes.
		/// </summary>
		public void DeleteShapes() {
			DeleteShapes(SelectedShapes, true);
		}


		/// <summary>
		/// Deletes the selected shapes.
		/// </summary>
		public void DeleteShapes(bool withModelObjects) {
			DeleteShapes(SelectedShapes, withModelObjects);
		}


		/// <summary>
		/// Deletes the given shape in the displayed diagram.
		/// </summary>
		public void DeleteShape(Shape shape) {
			DeleteShapes(SingleInstanceEnumerator<Shape>.Create(shape), true);
		}


		/// <summary>
		/// Deletes the given shape in the displayed diagram.
		/// </summary>
		public void DeleteShape(Shape shape, bool withModelObjects) {
			DeleteShapes(SingleInstanceEnumerator<Shape>.Create(shape), withModelObjects);
		}


		/// <summary>
		/// Deletes the given shapes in the displayed diagram.
		/// </summary>
		public void DeleteShapes(IEnumerable<Shape> shapes) {
			DeleteShapes(shapes, true);
		}


		/// <summary>
		/// Deletes the given shapes in the displayed diagram.
		/// </summary>
		public void DeleteShapes(IEnumerable<Shape> shapes, bool withModelObejcts) {
			if (Diagram != null) {
				DiagramSetController.DeleteShapes(Diagram, shapes, withModelObejcts);
				OnShapesRemoved(GetShapesEventArgs(shapes));
				UnselectShapes(shapes);
			}
		}


		/// <summary>
		/// Cuts the selected shapes.
		/// </summary>
		public void Cut() {
			Cut(true, PointToClient(Control.MousePosition));
		}


		/// <summary>
		/// Cut the selected shapes with or without model objects.
		/// </summary>
		public void Cut(bool withModelObjects) {
			if (Diagram != null && _selectedShapes.Count > 0)
				Cut(withModelObjects, Geometry.InvalidPoint);
		}


		/// <summary>
		/// Cut the selected shapes with or without model objects.
		/// </summary>
		/// <param name="withModelObjects">Specifies whether the shape should be copied with its model objects (if assigned).</param>
		/// <param name="mousePosDiagram">Current Mouse position in diagram coordinates.</param>
		public void Cut(bool withModelObjects, Point mousePosDiagram) {
			if (Diagram != null && _selectedShapes.Count > 0)
				PerformCut(Diagram, _selectedShapes, withModelObjects, mousePosDiagram);
		}


		/// <summary>
		/// Copies the selected shapes.
		/// </summary>
		public void Copy() {
			Copy(true, PointToClient(Control.MousePosition));
		}


		/// <summary>
		/// Copy the selected shapes with or without model objects.
		/// </summary>
		public void Copy(bool withModelObjects) {
			Copy(withModelObjects, Geometry.InvalidPoint);
		}


		/// <summary>
		/// Copy the selected shapes with or without model objects.
		/// </summary>
		/// <param name="withModelObjects">Specifies whether the shape should be copied with its model objects (if assigned).</param>
		/// <param name="mousePosDiagram">Current Mouse position in diagram coordinates.</param>
		public void Copy(bool withModelObjects, Point mousePosDiagram) {
			if (Diagram != null && SelectedShapes.Count > 0)
				PerformCopy(Diagram, SelectedShapes, withModelObjects, mousePosDiagram);
		}


		/// <summary>
		/// Pastes the previously copied or cut shapes into the displayed diagram.
		/// </summary>
		public void Paste() {
			Paste(PointToClient(Control.MousePosition));
		}


		/// <summary>
		/// Paste the copied or cut shapes.
		/// </summary>
		/// <param name="mousePosDiagram">Current mouse position in diagram coordinates.</param>
		public void Paste(Point mousePosDiagram) {
			if (Diagram != null && _diagramSetController.CanPaste(Diagram))
				PerformPaste(Diagram, ActiveHomeLayer, ActiveSupplementalLayers, mousePosDiagram);
		}


		/// <summary>
		/// Paste the copied or cut shapes.
		/// </summary>
		/// <param name="offsetX">Horizontal offset in diagram coordinates relative to the original shape's X coordinate.</param>
		/// <param name="offsetY">Vertical offset in diagram coordinates relative to the original shape's Y coordinate.</param>
		public void Paste(int offsetX, int offsetY) {
			if (Diagram != null && _diagramSetController.CanPaste(Diagram))
				PerformPaste(Diagram, ActiveHomeLayer, ActiveSupplementalLayers, offsetX, offsetY);
		}


		/// <summary>
		/// Delete the selected shapes with or without model objects.
		/// </summary>
		public void Delete(bool withModelObjects) {
			if (Diagram != null && SelectedShapes.Count > 0) {
				PerformDelete(Diagram, _selectedShapes, withModelObjects);
			}
		}


		/// <summary>
		/// Ensures that the given coordinates are inside the displayed area by scrolling the display to the given position.
		/// </summary>
		public void EnsureVisible(int x, int y) {
			EnsureVisible(x, y, 0);
		}


		/// <summary>
		/// Ensures that the given coordinates are inside the displayed area by scrolling the display to the given position.
		/// </summary>
		public void EnsureVisible(int x, int y, int margin) {
			int ctrlX, ctrlY, ctrlMargin;
			DiagramToControl(x, y, out ctrlX, out ctrlY);
			DiagramToControl(margin, out ctrlMargin);
			if (DrawBoundsContainsPoint(ctrlX, ctrlY, ctrlMargin)) {
				Rectangle viewArea = Rectangle.Empty;
				ControlToDiagram(DrawBounds, out viewArea);
				viewArea.Inflate(-margin, -margin);

				// scroll horizontally
				int deltaX = 0, deltaY = 0;
				if (HScrollBarVisible) {
					if (x < viewArea.Left)
						// Scroll left
						deltaX = x - viewArea.Left;
					else if (viewArea.Right < x)
						// Scroll right
						deltaX = x - viewArea.Right;
				}
				if (VScrollBarVisible) {
					if (y < viewArea.Top)
						// Scroll left
						deltaY = y - viewArea.Top;
					else if (viewArea.Bottom < y)
						// Scroll right
						deltaY = y - viewArea.Bottom;
				}
				ScrollTo(_scrollPosX + deltaX, _scrollPosY + deltaY);
			}
		}


		/// <summary>
		/// Ensures that the given shape is inside the displayed area.
		/// </summary>
		/// <param name="shape">The shape that should be completely visible.</param>
		public void EnsureVisible(Shape shape) {
			if (shape == null) throw new ArgumentNullException("shape");
			EnsureVisible(shape.GetBoundingRectangle(false));
		}

			
		/// <summary>
		/// Ensures that the given shape is inside the displayed area.
		/// </summary>
		/// <param name="shape">The shape that should be completely visible.</param>
		/// <param name="margin">Specifies the margin (in diagram coordinates) around the area that should be visible.</param>
		public void EnsureVisible(Shape shape, int margin) {
			if (shape == null) throw new ArgumentNullException("shape");
			EnsureVisible(shape.GetBoundingRectangle(false), margin);
		}


		/// <summary>
		/// Ensures that the given area is inside the displayed area.
		/// </summary>
		/// <param name="viewAreaDiagram">The area (in diagram coordinates) that should be visible.</param>
		public void EnsureVisible(Rectangle viewAreaDiagram) {
			EnsureVisible(viewAreaDiagram, 0);
		}


		/// <summary>
		/// Ensures that the given area is inside the displayed area.
		/// </summary>
        /// <param name="viewAreaDiagram">The area (in diagram coordinates) that should be visible.</param>
		/// <param name="margin">Specifies the margin (in diagram coordinates) around the area that should be visible.</param>
		public void EnsureVisible(Rectangle viewAreaDiagram, int margin) {
			// Check if the diagram has to be zoomed
			Rectangle areaBoundsDiag = Rectangle.Empty;
			ControlToDiagram(DrawBounds, out areaBoundsDiag);
			areaBoundsDiag.Inflate(-margin, -margin);
            // Check if Zoom has to be adjusted
			if (areaBoundsDiag.Width < viewAreaDiagram.Width || areaBoundsDiag.Height < viewAreaDiagram.Height) {
				float scale = Geometry.CalcScaleFactor(viewAreaDiagram.Width, viewAreaDiagram.Height, areaBoundsDiag.Width, areaBoundsDiag.Height);
				ZoomLevel = Math.Max(1, (int)Math.Floor(ZoomLevel * scale));
			}

			// Recalculate viewArea in case a new zoom level was applied
			ControlToDiagram(DrawBounds, out areaBoundsDiag);
			areaBoundsDiag.Inflate(-margin, -margin);
            if (viewAreaDiagram.Left < areaBoundsDiag.Left
				|| viewAreaDiagram.Top < areaBoundsDiag.Top
				|| viewAreaDiagram.Right > areaBoundsDiag.Right
				|| viewAreaDiagram.Bottom > areaBoundsDiag.Bottom) {

				// scroll horizontally
				int deltaX = 0, deltaY = 0;
				if (HScrollBarVisible) {
					if (viewAreaDiagram.Left < areaBoundsDiag.Left)
						// Scroll left
						deltaX = viewAreaDiagram.Left - areaBoundsDiag.Left;
					else if (areaBoundsDiag.Right < viewAreaDiagram.Right)
						// Scroll right
						deltaX = viewAreaDiagram.Right - areaBoundsDiag.Right;
				}
				if (VScrollBarVisible) {
					if (viewAreaDiagram.Top < areaBoundsDiag.Top)
						// Scroll up
						deltaY = viewAreaDiagram.Top - areaBoundsDiag.Top;
					else if (areaBoundsDiag.Bottom < viewAreaDiagram.Bottom)
						// Scroll down
						deltaY = viewAreaDiagram.Bottom - areaBoundsDiag.Bottom;
				}
				ScrollTo(_scrollPosX + deltaX, _scrollPosY + deltaY);
			}
		}


		/// <summary>
		/// Returns all layer ids that are currently visible.
		/// In contrast to the HiddenLayers property, this method respects the zoom threshold settings of the layers.
		/// </summary>
		public IEnumerable<int> GetVisibleLayerIds() {
			// Init result with Layers.All in order to ensure that all shapes are visible when there are no layers...
			if (_diagramController.Diagram != null && _diagramController.Diagram.Layers.Count > 0) {
				foreach (Layer layer in _diagramController.Diagram.Layers) {
					if (_hiddenLayerIds.Contains(layer.LayerId)) continue;
					if (layer.LowerZoomThreshold <= _zoomLevel && layer.UpperZoomThreshold >= _zoomLevel)
						yield return layer.LayerId;
				}
			}
		}


		/// <summary>
		/// Shows or hides the given layer(s).
		/// </summary>
		[Obsolete("Use an overload taking layer(s) or layer ids instead.")]
		public void SetLayerVisibility(LayerIds layers, bool visible) {
			if (layers == LayerIds.All)
				SetLayerVisibility(Diagram.Layers, visible);
			else 
				SetLayerVisibility(LayerHelper.GetAllLayerIds(layers), visible);
		}


		/// <summary>
		/// Shows or hides the given layer(s).
		/// </summary>
		public void SetLayerVisibility(int layerId, bool visible) {
			SetLayerVisibility(EnumerationHelper.Enumerate(Diagram.Layers[layerId]), visible);
		}


		/// <summary>
		/// Shows or hides the given layer(s).
		/// </summary>
		public void SetLayerVisibility(IEnumerable<int> layerIds, bool visible) {
			if (layerIds == null) throw new ArgumentNullException("layerIds");
			SetLayerVisibility(Diagram.Layers.FindLayers(layerIds), visible);
		}


		/// <summary>
		/// Shows or hides the given layer(s).
		/// </summary>
		public void SetLayerVisibility(IEnumerable<Layer> layers, bool visible) {
			if (layers == null) throw new ArgumentNullException("layers");
			// Hide or show layers
			// Shared layers
			if (visible) _hiddenSharedLayers ^= (_hiddenSharedLayers & Layer.ConvertToLayerIds(layers));
			else _hiddenSharedLayers |= Layer.ConvertToLayerIds(layers);
			// All layer ids
			foreach (Layer layer in layers) {
				if (visible)
					_hiddenLayerIds.Remove(layer.LayerId);
				else {
					if (!_hiddenLayerIds.Contains(layer.LayerId))
						_hiddenLayerIds.Add(layer.LayerId);
				}
			}

			UnselectShapesOfInvisibleLayers();
			// Update presenter
			Invalidate();
			// Perform notification only if a diagram is displayed.
			if (Diagram != null) 
				OnLayerVisibilityChanged(LayerHelper.GetLayersEventArgs(layers));
		}


		/// <summary>
		/// Sets the given layers as active layers.
		/// </summary>
		[Obsolete("Use an overload taking layer(s) or layer ids instead.")]
		public void SetLayerActive(LayerIds layers, bool active) {
			if (layers == LayerIds.All)
				SetLayerActive(Diagram.Layers, active);
			else 
				SetLayerActive(LayerHelper.GetAllLayerIds(layers), active);
		}


		/// <summary>
		/// Sets the given layers as active layers.
		/// </summary>
		public void SetLayerActive(int layerId, bool active) {
			SetLayerActive(EnumerationHelper.Enumerate(layerId), active);
		}


		/// <summary>
		/// Sets the given layers as active layers.
		/// </summary>
		/// <remarks>
		/// If the layerIds contain more than one home layer id, the last home layer in the sequence will be activated.
		/// </remarks>
		public void SetLayerActive(IEnumerable<int> layerIds, bool active) {
			if (layerIds == null) throw new ArgumentNullException("layerIds");
			SetLayerActive(Diagram.Layers.FindLayers(layerIds), active);
		}


		/// <summary>
		/// Sets the given layers as active layers.
		/// </summary>
		/// <remarks>
		/// If the layerIds contain more than one home layer id, the last home layer in the sequence will be activated.
		/// </remarks>
		public void SetLayerActive(IEnumerable<Layer> layers, bool active) {
			if (layers == null) throw new ArgumentNullException("layers");

			// Split the given layers into combinable and uncombinable layers
			int uncombinableLayer = Layer.NoLayerId;
			LayerIds combinableLayers = LayerIds.None;
			foreach (Layer layer in layers) {
				if (Layer.IsCombinable(layer.LayerId))
					combinableLayers |= Layer.ConvertToLayerIds(layer.LayerId);
				else
					// Overwrite the uncombinable layer in order to take the last one in sequence.
					uncombinableLayer = layer.LayerId;
			}
			// Activate/Deactivate the given layers
			if (active) {
				_activeCombinableLayers |= combinableLayers;
				_activeNonCombinableLayer = uncombinableLayer;
			} else {
				_activeCombinableLayers ^= (_activeCombinableLayers & combinableLayers);
				if (uncombinableLayer == _activeNonCombinableLayer)
					_activeNonCombinableLayer = Layer.NoLayerId;
			}
			_activeLayerIds.Clear();
			_activeLayerIds.Add(LayerHelper.GetAllLayerIds(_activeCombinableLayers));
			if (_activeNonCombinableLayer != Layer.NoLayerId)
				_activeLayerIds.Add(_activeNonCombinableLayer);
			
			// Update presenter
			Invalidate();
			// Perform notification only if a diagram is displayed.
			if (Diagram != null)
				OnActiveLayersChanged(LayerHelper.GetLayersEventArgs(layers));
		}


		/// <summary>
		/// Tests whether any of the given layers is visible.
		/// </summary>		
		public bool IsLayerVisible(LayerIds layerId) {
			if (_hiddenSharedLayers == LayerIds.None || layerId == LayerIds.None) return true;
			else return !((_hiddenSharedLayers & layerId) != 0);
		}


		/// <summary>
		/// Tests whether any of the given layers is visible.
		/// </summary>
		public bool IsLayerVisible(int layerId) {
			return !_hiddenLayerIds.Contains(layerId);
		}


		/// <summary>
		/// Tests whether any of the given layers is visible.
		/// </summary>
		public bool IsLayerVisible(IEnumerable<int> layerIds) {
			return !_hiddenLayerIds.ContainsAll(layerIds);
		}


		/// <summary>
		/// Tests whether any of the given layers is visible.
		/// </summary>
		public bool IsLayerVisible(int homeLayer, LayerIds supplementalLayers) {
			if (homeLayer == Layer.NoLayerId && supplementalLayers == LayerIds.None)
				return true;
			else {
				bool result = false;
				if (homeLayer != Layer.NoLayerId && IsLayerVisible(homeLayer))
					result = true;
				if (supplementalLayers != LayerIds.None && IsLayerVisible(supplementalLayers))
					result = true;
				return result;
			}
		}


		/// <summary>
		/// Tests whether all of the given layers are active.
		/// </summary>
		[Obsolete("Use an overload taking layer(s) or layer ids instead.")]
		public bool IsLayerActive(LayerIds layerId) {
			foreach (int id in LayerHelper.GetAllLayerIds(layerId))
				if (_activeLayerIds.Contains(id))
					return true;
			return false;
		}


		/// <summary>
		/// Tests whether all of the given layers are active.
		/// </summary>
		public bool IsLayerActive(int layerId) {
			return _activeLayerIds.Contains(layerId);
		}


		/// <summary>
		/// Tests whether all of the given layers are active.
		/// </summary>
		public bool IsLayerActive(IEnumerable<int> layerIds) {
			return _activeLayerIds.ContainsAll(layerIds);
		}


		/// <summary>
		/// Copies the selected shapes (or the whole diagram if no shapes are selected) as image of the specified format to the clipboard.
		/// </summary>
		/// <param name="fileFormat">Format of the image that will be copied to the clipboard.</param>
		/// <param name="clearClipboard">Specifies if the clipboard should be emptied before copying the image.</param>
		public void CopyImageToClipboard(ImageFileFormat fileFormat, bool clearClipboard) {
			CopyImageToClipboard(fileFormat, null, clearClipboard);
		}


		/// <summary>
		/// Copies the selected shapes (or the whole diagram if no shapes are selected) as image of the specified format to the clipboard.
		/// </summary>
		/// <param name="fileFormat">Format of the image that will be copied to the clipboard.</param>
		/// <param name="visibleLayers">Specifies the visible layers. Shapes that are assigned to at least one of these layers will be rendered.</param>
		/// <param name="clearClipboard">Specifies if the clipboard should be emptied before copying the image.</param>
		public void CopyImageToClipboard(ImageFileFormat fileFormat, IEnumerable<int> visibleLayers, bool clearClipboard) {
			if (Diagram != null) {
				// Copy image to clipboard
				IEnumerable<Shape> shapes = (SelectedShapes.Count > 0) ? SelectedShapes : null;
				int margin = (SelectedShapes.Count > 0) ? 10 : 0;
				switch (fileFormat) {
					case ImageFileFormat.Bmp:
					case ImageFileFormat.Gif:
					case ImageFileFormat.Jpeg:
					case ImageFileFormat.Png:
					case ImageFileFormat.Tiff:
						// Copy diagram/selected shapes to clipboard as PNG bitmap image file
						Bitmap bmpImage = (Bitmap)Diagram.CreateImage(fileFormat, shapes, visibleLayers, margin, (shapes == null), System.Drawing.Color.Empty);
						// Clipboard.SetData(DataFormats.Bitmap, bmpImage);
						// In .NET 4.x, Clipboard.SetData() will clear the clipboard, so we have to use the Windows API 
						// functions in order to ensure we can put EMF and BMP to the clipboard at the same time.
						ClipboardHelper.PutBitmapOnClipboard(Handle, bmpImage, clearClipboard);
						Debug.Assert(Clipboard.ContainsData(DataFormats.Bitmap));
						break;
					case ImageFileFormat.Emf:
					case ImageFileFormat.EmfPlus:
						// Copy diagram/selected shapes to clipboard as EMF vector graphic file
						Metafile emfImage = (Metafile)Diagram.CreateImage(ImageFileFormat.EmfPlus, shapes, visibleLayers, margin, (shapes == null), System.Drawing.Color.Empty);
						ClipboardHelper.PutEnhMetafileOnClipboard(Handle, emfImage, clearClipboard);
						Debug.Assert(Clipboard.ContainsData(DataFormats.EnhancedMetafile));
						break;
					case ImageFileFormat.Svg:
						throw new NotImplementedException();
					default:
						throw new NShapeUnsupportedValueException(fileFormat);
				}
			}
		}


		/// <summary>
		/// Copies the selected shapes (or the whole diagram if no shapes are selected) as EMF and as bitmap image to the clipboard.
		/// </summary>
		/// <param name="clearClipboard">Specifies if the clipboard should be emptied before copying the image.</param>
		public void CopyImagesToClipboard(bool clearClipboard) {
			CopyImagesToClipboard(null, clearClipboard);
		}


		/// <summary>
		/// Copies the selected shapes (or the whole diagram if no shapes are selected) as EMF and as bitmap image to the clipboard.
		/// </summary>
		/// <param name="visibleLayers">Specifies the visible layers. Shapes that are assigned to at least one of these layers will be rendered.</param>
		/// <param name="clearClipboard">Specifies if the clipboard should be emptied before copying the image.</param>
		public void CopyImagesToClipboard(IEnumerable<int> visibleLayers, bool clearClipboard) {
			if (Diagram != null) {
				if (ClipboardHelper.OpenClipboard(Handle)) {
					try {
						// Clear clipboard if necessary
						if (clearClipboard)
							ClipboardHelper.EmptyClipboard();
						// Copy images (EMF and Bitmap) to clipboard
						IEnumerable<Shape> shapes = (SelectedShapes.Count > 0) ? SelectedShapes : null;
						int margin = (SelectedShapes.Count > 0) ? 10 : 0;
						//
						// Copy diagram/selected shapes to clipboard as EMF vector graphic file
						Metafile emfImage = (Metafile)Diagram.CreateImage(ImageFileFormat.EmfPlus, shapes, visibleLayers, margin, (shapes == null), System.Drawing.Color.Empty);
						ClipboardHelper.AddEnhMetafileToClipboard(emfImage);
						//
						// Copy diagram/selected shapes to clipboard as PNG bitmap image file
						Bitmap bmpImage = (Bitmap)Diagram.CreateImage(ImageFileFormat.Bmp, shapes, visibleLayers, margin, (shapes == null), System.Drawing.Color.Empty);
						ClipboardHelper.AddBitmapToClipboard(bmpImage);
					} finally {
						ClipboardHelper.CloseClipboard();
					}
				}
				Debug.Assert(Clipboard.ContainsData(DataFormats.EnhancedMetafile));
				Debug.Assert(Clipboard.ContainsData(DataFormats.Bitmap));
			}
		}

		#endregion


		/// <summary>
		/// This DiagramPresenter's controller.
		/// </summary>
		[Browsable(false)]
		protected DiagramController DiagramController {
			get { return _diagramController; }
			set {
				Debug.Assert(_diagramSetController != null);
				if (_diagramController != null) {
					UnregisterDiagramControllerEvents();
					if (_diagramController.Diagram != null) {
						_diagramSetController.CloseDiagram(_diagramController.Diagram);
						if (_diagramController.Diagram != null) {
							UnregisterDiagramEvents();
							_diagramController.Diagram = null;
						}
						_diagramController = null;
						Clear();
					}
				}
				_diagramController = value;
				if (_diagramController != null) {
					RegisterDiagramControllerEvents();
					if (_diagramController.Diagram != null) {
						RegisterDiagramEvents();
						DisplayDiagram();
					}
				}
			}
		}


		#region [Protected] Methods: On[Event] event processing

		/// <ToBeCompleted></ToBeCompleted>
		protected virtual void OnShapesSelected(EventArgs eventArgs) {
			if (ShapesSelected != null && !SelectingChangedShapes) 
				ShapesSelected(this, eventArgs);
		}

		/// <ToBeCompleted></ToBeCompleted>
		protected virtual void OnShapeClick(DiagramPresenterShapeClickEventArgs eventArgs) {
			if (ShapeClick != null) ShapeClick(this, eventArgs);
		}

		/// <ToBeCompleted></ToBeCompleted>
		protected virtual void OnShapeDoubleClick(DiagramPresenterShapeClickEventArgs eventArgs) {
			if (ShapeDoubleClick != null) ShapeDoubleClick(this, eventArgs);
		}

		/// <ToBeCompleted></ToBeCompleted>
		protected internal virtual void OnShapesInserted(DiagramPresenterShapesEventArgs eventArgs) {
			if (ShapesInserted != null) ShapesInserted(this, eventArgs);
		}

		/// <ToBeCompleted></ToBeCompleted>
		protected internal virtual void OnShapesRemoved(DiagramPresenterShapesEventArgs eventArgs) {
			if (ShapesRemoved != null) ShapesRemoved(this, eventArgs);
		}

		/// <ToBeCompleted></ToBeCompleted>
		protected virtual void OnLayerVisibilityChanged(LayersEventArgs eventArgs) {
			if (LayerVisibilityChanged != null) LayerVisibilityChanged(this, eventArgs);
		}

		/// <ToBeCompleted></ToBeCompleted>
		protected virtual void OnActiveLayersChanged(LayersEventArgs eventArgs) {
			if (ActiveLayersChanged != null) ActiveLayersChanged(this, eventArgs);
		}

		/// <ToBeCompleted></ToBeCompleted>
		protected virtual void OnZoomChanged(EventArgs eventArgs) {
			if (ZoomChanged != null) ZoomChanged(this, eventArgs);
		}

		/// <ToBeCompleted></ToBeCompleted>
		protected virtual void OnDiagramChanging(EventArgs eventArgs) {
			if (DiagramChanging != null) DiagramChanging(this, eventArgs);
			ResetBounds();
			_hiddenSharedLayers = LayerIds.None;
			_hiddenLayerIds.Clear();
			_activeLayerIds.Clear();
		}

		/// <ToBeCompleted></ToBeCompleted>
		protected virtual void OnDiagramChanged(EventArgs eventArgs) {
			UpdateScrollBars();
			if (DiagramChanged != null) DiagramChanged(this, eventArgs);
		}

		/// <ToBeCompleted></ToBeCompleted>
		protected virtual void OnUserMessage(UserMessageEventArgs eventArgs) {
			if (UserMessage == null) {
				string msgFormatStr = "{0}{1}{1}In order to show this message in the correct context, handle the {2}.UserMessage event.";
				throw new WarningException(string.Format(msgFormatStr, eventArgs.MessageText, Environment.NewLine, GetType().FullName));
			} else 
				UserMessage(this, eventArgs);
		}

		#endregion


		#region [Protected] Methods: On[Event] event processing overrides

		/// <override></override>
		protected override void OnGotFocus(EventArgs e) {
			base.OnGotFocus(e);
		}

		/// <override></override>
		protected override void OnLostFocus(EventArgs e) {
			base.OnLostFocus(e);
		}

		/// <override></override>
		protected override void OnMouseWheel(MouseEventArgs e) {
			base.OnMouseWheel(e);
			// ToDo: Redirect MouseWheel movement to the current tool?
			//if (CurrentTool != null)
			//   CurrentTool.ProcessMouseEvent(this, WinFormHelpers.GetMouseEventArgs(MouseEventType.MouseWheel, e));

			if (Diagram != null) {
				if (_zoomWithMouseWheel || (Control.ModifierKeys & Keys.Control) != 0) {
					// Store position of the mouse cursor (in diagram coordinates)
					Point currMousePos = Point.Empty;
					ControlToDiagram(e.Location, out currMousePos);

					if (e.Delta != 0) {
						// Calculate current zoom stepping according to the current zoom factor.
						// This kind of accelleration feels for the user as a more 'constant' zoom behavior
						int currZoomStep;
						if (ZoomLevel < 30) currZoomStep = 1;
						else if (ZoomLevel < 60) currZoomStep = 2;
						else if (_zoomLevel < 100) currZoomStep = 5;
						else if (_zoomLevel < 300) currZoomStep = 10;
						else currZoomStep = 50;
						// Calculate zoom direction
						currZoomStep *= (int)(e.Delta / Math.Abs(e.Delta));
						// Set zoom level (and ensure the value is within a reasonable range)
						ZoomLevel = Math.Min(Math.Max(0, (ZoomLevel + currZoomStep) - (ZoomLevel % currZoomStep)), 4000);
					}

					// Restore mouse cursors's position by scrolling
					Point newMousePos = Point.Empty;
					ControlToDiagram(e.Location, out newMousePos);
					ScrollBy(currMousePos.X - newMousePos.X, currMousePos.Y - newMousePos.Y);
				} else {
					int value = -(e.Delta / 6);
					if ((Control.ModifierKeys & Keys.Shift) != 0)
						ScrollBy(value, 0);
					else ScrollBy(0, value);
				}
			}
		}

		/// <override></override>
		protected override void OnMouseClick(MouseEventArgs e) {
			base.OnMouseClick(e);
			ProcessClickEvent(e, false);
		}

		/// <override></override>
		protected override void OnMouseDown(MouseEventArgs e) {
			base.OnMouseDown(e);
			_mouseEventWasHandled = false;
			if (_inplaceTextbox != null) DoCloseCaptionEditor(true);
			if (ScrollBarContainsPoint(e.Location))
				_mouseEventWasHandled = true;
			else {
				if (ActiveTool != null) {
					try {
						if (ActiveTool.ProcessMouseEvent(this, WinFormHelpers.GetMouseEventArgs(this, MouseEventType.MouseDown, e, DrawBounds)))
							_mouseEventWasHandled = true;
					} catch (Exception exc) {
						ActiveTool.Cancel();
						Debug.Print(GetFailMessage(exc));
						throw;
					}
				}
			}
		}

		/// <override></override>
		protected override void OnMouseDoubleClick(MouseEventArgs e) {
			base.OnMouseDoubleClick(e);
			ProcessClickEvent(e, true);
		}

		/// <override></override>
		protected override void OnMouseEnter(EventArgs e) {
			base.OnMouseEnter(e);
			if (ActiveTool != null) {
				try {
					ActiveTool.EnterDisplay(this);
				} catch (Exception exc) {
					ActiveTool.Cancel();
					Debug.Print(GetFailMessage(exc));
					throw;
				}
			}
		}

		/// <override></override>
		protected override void OnMouseLeave(EventArgs e) {
			base.OnMouseLeave(e);
			if (ActiveTool != null) {
				try {
					ActiveTool.LeaveDisplay(this);
				} catch (Exception exc) {
					ActiveTool.Cancel();
					Debug.Print(GetFailMessage(exc));
					throw;
				}
			}
		}

		/// <override></override>
		protected override void OnMouseMove(MouseEventArgs e) {
			//Console.WriteLine("[{0}]\t OnMouseMove (Entering)", DateTime.Now.ToString("HH:mm:ss.ffff"));
			base.OnMouseMove(e);
			if (_universalScrollEnabled)
				PerformUniversalScroll(e.Location);
			else {
				if (ActiveTool != null && !ScrollBarContainsPoint(e.Location)) {
					try {
						//Console.WriteLine("[{0}]\t Tool.ProcessMouseEvent calling", DateTime.Now.ToString("HH:mm:ss.ffff"));
						if (ActiveTool.ProcessMouseEvent(this, WinFormHelpers.GetMouseEventArgs(this, MouseEventType.MouseMove, e, DrawBounds)))
							_mouseEventWasHandled = true;
						//Console.WriteLine("[{0}]\t Tool.ProcessMouseEvent finished", DateTime.Now.ToString("HH:mm:ss.ffff"));

						if (ActiveTool.WantsAutoScroll && DrawBoundsContainsPoint(e.X, e.Y, autoScrollMargin)) {
							int x, y;
							ControlToDiagram(e.X, e.Y, out x, out y);
							int margin;
							ControlToDiagram(autoScrollMargin, out margin);
							EnsureVisible(x, y, margin);
							if (!_autoScrollTimer.Enabled) _autoScrollTimer.Enabled = true;
						} else if (_autoScrollTimer.Enabled)
							_autoScrollTimer.Enabled = false;
					} catch (Exception exc) {
						ActiveTool.Cancel();
						Debug.Print(GetFailMessage(exc));
						throw;
					}
				}
			}
			_lastMousePos = e.Location;
			//Console.WriteLine("[{0}]\t OnMouseMove (Leaving)", DateTime.Now.ToString("HH:mm:ss.ffff"));
		}

		/// <override></override>
		protected override void OnMouseUp(MouseEventArgs e) {
			base.OnMouseUp(e);
			if (!this.Focused)
				this.Focus();
			if (ActiveTool != null && !ScrollBarContainsPoint(e.Location)) {
				try {
					if (ActiveTool.ProcessMouseEvent(this, WinFormHelpers.GetMouseEventArgs(this, MouseEventType.MouseUp, e, DrawBounds)))
						_mouseEventWasHandled = true;
				} catch (Exception exc) {
					ActiveTool.Cancel();
					Debug.Print(GetFailMessage(exc));
					throw;
				}
			}

			// If the mouse event was not handled otherwise, handle it here:
			if (!_mouseEventWasHandled) {
				// Use switch statement here because we do not want to handle mouse button combinations
				switch (e.Button) {

					// Display diagram properties
					case MouseButtons.Left:
						ShowDiagramProperties(e.Location);
						break;

					// Start universal scroll
					case MouseButtons.Middle:
						if (_universalScrollEnabled) EndUniversalScroll();
						else StartUniversalScroll(e.Location);
						break;

					// Show context menu
					case MouseButtons.Right:
						if (ActiveTool != null) {
							if (Diagram != null) {
								Point mousePos = Point.Empty;
								ControlToDiagram(e.Location, out mousePos);
								// if there is no selected shape under the cursor
								if (SelectedShapes.FindShape(mousePos.X, mousePos.Y, ControlPointCapabilities.None, 0, null) == null) {
									// Check if there is a non-selected shape under the cursor 
									// and select it in this case
									Shape shape = Diagram.Shapes.FindShape(mousePos.X, mousePos.Y, ControlPointCapabilities.None, 0, null);
									if (shape != null) SelectShape(shape);
									else UnselectAll();
								}
							}
							// Display context menu
							if (ContextMenuStrip != null) {
								if (ContextMenuStrip.Visible) ContextMenuStrip.Close();
								ContextMenuStrip.Show(PointToScreen(e.Location));
							}
						}
						break;
				}
			}
			_mouseEventWasHandled = false;
		}

		/// <override></override>
		protected override void OnMouseCaptureChanged(EventArgs e) {
			base.OnMouseCaptureChanged(e);
		}

		/// <override></override>
		protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e) {
			base.OnPreviewKeyDown(e);
			if (ActiveTool != null) {
				try {
					ActiveTool.ProcessKeyEvent(this, WinFormHelpers.GetKeyEventArgs(e));
				} catch (Exception exc) {
					Debug.Print(GetFailMessage(exc));
					ActiveTool.Cancel();
					throw;
				}
			}
		}

		/// <override></override>
		protected override void OnKeyDown(KeyEventArgs e) {
			base.OnKeyDown(e);
			if (!e.Handled && ActiveTool != null) {
				try {
					e.Handled = ActiveTool.ProcessKeyEvent(this, WinFormHelpers.GetKeyEventArgs(KeyEventType.KeyDown, e));
				} catch (Exception exc) {
					Debug.Print(exc.Message);
					ActiveTool.Cancel();
					throw;
				}

				if (!e.Handled) {
					try {
						switch (e.KeyCode) {
							case Keys.F2:
								if (Diagram != null
									&& SelectedShapes.Count == 1
									&& Project.SecurityManager.IsGranted(Permission.Data, SelectedShapes.TopMost)
									&& SelectedShapes.TopMost is ICaptionedShape) {
									ICaptionedShape captionedShape = (ICaptionedShape)SelectedShapes.TopMost;
									if (captionedShape.CaptionCount > 0) {
										DoOpenCaptionEditor(captionedShape, 0, string.Empty);
										e.Handled = true;
									}
								}
								break;

							case Keys.Left:
							case Keys.Right:
							case Keys.Up:
							case Keys.Down:
								if (SelectedShapes.Count == 0) {
									int newHValue = scrollBarH.Value;
									if (HScrollBarVisible) {
										int deltaH = (int)Math.Round(Math.Max((int)(e.Shift ? scrollBarH.LargeChange : scrollBarH.SmallChange) / 2f, 1));
										if (e.KeyCode == Keys.Left)
											newHValue -= deltaH;
										else if (e.KeyCode == Keys.Right)
											newHValue += deltaH;
									}
									int newVValue = scrollBarV.Value;
									if (VScrollBarVisible) {
										int deltaV = (int)Math.Round(Math.Max((int)(e.Shift ? scrollBarV.LargeChange : scrollBarV.SmallChange) / 2f, 1));
										if (e.KeyCode == Keys.Up)
											newVValue -= deltaV;
										else if (e.KeyCode == Keys.Down)
											newVValue += deltaV;
									}
									ScrollTo(newHValue, newVValue);
									e.Handled = true;
								} else {
									int deltaX = 0, deltaY = 0;
									switch (e.KeyCode) {
										case Keys.Left:
										case Keys.Right:
											deltaX = e.Shift ? GridSize : 1;
											if (e.KeyCode == Keys.Left) 
												deltaX = -deltaX;
											break;
										case Keys.Up:
										case Keys.Down:
											deltaY = e.Shift ? GridSize : 1;
											if (e.KeyCode == Keys.Up)
												deltaY = -deltaY;
											break;
									}
									if (DiagramSetController.CanMoveShapes(Diagram, SelectedShapes)) {
										DiagramSetController.MoveShapes(Diagram, SelectedShapes, deltaX, deltaY);
										e.Handled = true;
									}
								}
								break;

							// Delete / Cut
							case Keys.Delete:
								if (Diagram != null && _inplaceTextbox == null && SelectedShapes.Count > 0) {
									if (e.Modifiers == Keys.Shift) {
										// Cut
										if (DiagramController.Owner.CanCut(Diagram, SelectedShapes)) {
											if (ActiveTool != null && ActiveTool.IsToolActionPending)
												ActiveTool.Cancel();

											PerformCut(Diagram, SelectedShapes, true, Geometry.InvalidPoint);
											e.Handled = true;
										}
									} else {
										// Delete
										if (DiagramController.Owner.CanDeleteShapes(Diagram, SelectedShapes)) {
											if (ActiveTool != null && ActiveTool.IsToolActionPending)
												ActiveTool.Cancel();
											PerformDelete(Diagram, SelectedShapes, true);
											// Route event to the current tool in order to refresh the tool - otherwise it will 
											// not notice that the deleted shape was deleted...
											if (ActiveTool != null)
												ActiveTool.ProcessKeyEvent(this, WinFormHelpers.GetKeyEventArgs(KeyEventType.KeyDown, e));
											e.Handled = true;
										}
									}
								}
								break;

							// Select All
							case Keys.A:
								if ((e.Modifiers & Keys.Control) == Keys.Control) {
									if (Diagram != null && SelectedShapes.Count != Diagram.Shapes.Count) {
										SelectAll();
										e.Handled = true;
									}
								}
								break;

							// Copy / Paste
							case Keys.Insert:
								if (e.Modifiers == Keys.Control) {
									// Copy
									if (Diagram != null
										&& SelectedShapes.Count > 0
										&& DiagramController.Owner.CanCopy(SelectedShapes)) {
											PerformCopy(Diagram, SelectedShapes, true, Geometry.InvalidPoint);
											e.Handled = true;
									}
								} else if (e.Modifiers == Keys.Shift) {
									if (Diagram != null
										&& ActiveTool != null
										&& DiagramController.Owner.CanPaste(Diagram)) {
											PerformPaste(Diagram, ActiveHomeLayer, ActiveSupplementalLayers, Geometry.InvalidPoint);
											e.Handled = true;
									}
								}
								break;

							// Copy
							case Keys.C:
								if ((e.Modifiers & Keys.Control) == Keys.Control) {
									if (Diagram != null
										&& SelectedShapes.Count > 0
										&& DiagramController.Owner.CanCopy(SelectedShapes)) {
											PerformCopy(Diagram, SelectedShapes, true, Geometry.InvalidPoint);
											e.Handled = true;
									}
								}
								break;

							// Paste
							case Keys.V:
								if ((e.Modifiers & Keys.Control) == Keys.Control) {
									if (Diagram != null
										&& ActiveTool != null
										&& DiagramController.Owner.CanPaste(Diagram)) {
											PerformPaste(Diagram, ActiveHomeLayer, ActiveSupplementalLayers, Geometry.InvalidPoint);
											e.Handled = true;
									}
								}
								break;

							// Cut
							case Keys.X:
								if ((e.Modifiers & Keys.Control) == Keys.Control
									&& Diagram != null
									&& ActiveTool != null
									&& SelectedShapes.Count > 0
									&& DiagramController.Owner.CanCut(Diagram, SelectedShapes)) {
										PerformCut(Diagram, SelectedShapes, true, Geometry.InvalidPoint);
										e.Handled = true;
								}
								break;

							// Undo/Redo
							case Keys.Z:
								if ((e.Modifiers & Keys.Control) != 0
									&& Diagram != null
									&& ActiveTool != null) {
									if ((e.Modifiers & Keys.Shift) != 0) {
										if (DiagramController.Owner.Project.History.RedoCommandCount > 0) {
											PerformRedo();
											e.Handled = true;
										}
									} else {
										if (DiagramController.Owner.Project.History.UndoCommandCount > 0) {
											PerformUndo();
											e.Handled = true;
										}
									}
								}
								break;

							case Keys.ShiftKey:
							case Keys.ControlKey:
							case Keys.LControlKey:
							case Keys.RControlKey:
								// Nothing to do here
								break;

							default:
								// Do nothing
								break;
						}
					} catch (Exception exc) {
						OnUserMessage(new UserMessageEventArgs(exc.Message));
					}
				}
			}
		}

		/// <override></override>
		protected override void OnKeyUp(KeyEventArgs e) {
			base.OnKeyUp(e);
			if (!e.Handled && ActiveTool != null) {
				try {
					e.Handled = ActiveTool.ProcessKeyEvent(this, WinFormHelpers.GetKeyEventArgs(KeyEventType.KeyUp, e));
					if (!e.Handled) {
						switch (e.KeyCode) {
							case Keys.ShiftKey:
							case Keys.ControlKey:
							case Keys.LControlKey:
							case Keys.RControlKey:
								// Nothing to do here
								break;

							default:
								// Do nothing
								break;
						}
					}
				} catch (Exception exc) {
					Debug.Print(exc.Message);
					ActiveTool.Cancel();
				}
			}
		}

		/// <override></override>
		protected override void OnKeyPress(KeyPressEventArgs e) {
			base.OnKeyPress(e);
			if (!e.Handled && ActiveTool != null) {
				KeyEventArgsDg eventArgs = WinFormHelpers.GetKeyEventArgs(e);
				try {
					e.Handled = ActiveTool.ProcessKeyEvent(this, eventArgs);
				} catch (Exception exc) {
					Debug.Print(exc.Message);
					ActiveTool.Cancel();
				}

				// Show caption editor
				if (!e.Handled
					&& _selectedShapes.Count == 1
					&& _selectedShapes.TopMost is ICaptionedShape
					&& !char.IsControl(eventArgs.KeyChar)
					&& Project.SecurityManager.IsGranted(Permission.Present, SelectedShapes.TopMost)) {
					string pressedKey = eventArgs.KeyChar.ToString();
					ICaptionedShape labeledShape = (ICaptionedShape)_selectedShapes.TopMost;
					if (labeledShape.CaptionCount > 0)
						DoOpenCaptionEditor(labeledShape, 0, pressedKey);
				}
			}
		}

		/// <override></override>
		protected override void OnDragEnter(DragEventArgs drgevent) {
			base.OnDragEnter(drgevent);
		}

		/// <override></override>
		protected override void OnDragOver(DragEventArgs drgevent) {
			if (drgevent.Data.GetDataPresent(typeof(ModelObjectDragInfo)) && Diagram != null) {
				Point mousePosCtrl = PointToClient(MousePosition);
				Point mousePosDiagram;
				ControlToDiagram(mousePosCtrl, out mousePosDiagram);

				Shape shape = Diagram.Shapes.FindShape(mousePosDiagram.X, mousePosDiagram.Y, ControlPointCapabilities.None, 0, null);
				if (shape != null && shape.ModelObject == null)
					drgevent.Effect = DragDropEffects.Move;
				else drgevent.Effect = DragDropEffects.None;
			} else base.OnDragOver(drgevent);
		}

		/// <override></override>
		protected override void OnDragDrop(DragEventArgs drgevent) {
			if (drgevent.Data.GetDataPresent(typeof(ModelObjectDragInfo)) && Diagram != null) {
				Point mousePosDiagram;
				ControlToDiagram(PointToClient(MousePosition), out mousePosDiagram);

				Shape shape = Diagram.Shapes.FindShape(mousePosDiagram.X, mousePosDiagram.Y, ControlPointCapabilities.None, 0, null);
				if (shape != null && shape.ModelObject == null) {
					ModelObjectDragInfo dragInfo = (ModelObjectDragInfo)drgevent.Data.GetData(typeof(ModelObjectDragInfo));
					ICommand cmd = new AssignModelObjectCommand(shape, dragInfo.ModelObject);
					Project.ExecuteCommand(cmd);
				}
			} else base.OnDragDrop(drgevent);
		}

		/// <override></override>
		protected override void OnDragLeave(EventArgs e) {
			base.OnDragLeave(e);
		}

		/// <override></override>
		protected override void OnContextMenuStripChanged(EventArgs e) {
			if (ContextMenuStrip != null && ContextMenuStrip != displayContextMenuStrip) {
				if (displayContextMenuStrip != null) {
					displayContextMenuStrip.Opening -= displayContextMenuStrip_Opening;
					displayContextMenuStrip.Closed -= displayContextMenuStrip_Closed;
				}
				displayContextMenuStrip = ContextMenuStrip;
				if (displayContextMenuStrip != null) {
					displayContextMenuStrip.Opening += displayContextMenuStrip_Opening;
					displayContextMenuStrip.Closed += displayContextMenuStrip_Closed;
				}
			}
			base.OnContextMenuStripChanged(e);
		}

		/// <override></override>
		protected override void OnScroll(ScrollEventArgs se) {
			// Do not call base.OnScroll() here!
			// Base implementation will be called in SetScrollPos()
			//
			bool isHScroll = (se.ScrollOrientation == ScrollOrientation.HorizontalScroll);
			if ((isHScroll && !_scrollBarHVisible) || (!isHScroll && !_scrollBarVVisible))
				return;

			// Handle scroll messages sent directly to the control, e.g. by touch pads of notebooks.
			switch (se.Type) {
				case ScrollEventType.First:
					ScrollTo(se.ScrollOrientation, isHScroll ? scrollBarH.Minimum : scrollBarV.Minimum);
					break;
				case ScrollEventType.Last:
					ScrollTo(se.ScrollOrientation, isHScroll ? scrollBarH.Maximum : scrollBarV.Maximum);
					break;
				case ScrollEventType.LargeDecrement:
				case ScrollEventType.LargeIncrement:
					// Scroll by 1/4 of LargeChange because the LargeChange (== Display width/height) value 
					// would be too big for smooth scrolling
					int largeValue = (isHScroll ? scrollBarH.LargeChange : scrollBarV.LargeChange) / 4;
					ScrollBy(se.ScrollOrientation, (se.Type == ScrollEventType.SmallIncrement) ? largeValue : -largeValue);
					break;
				case ScrollEventType.SmallDecrement:
				case ScrollEventType.SmallIncrement:
					int smallValue = isHScroll ? scrollBarH.SmallChange : scrollBarV.SmallChange;
					ScrollBy(se.ScrollOrientation, (se.Type == ScrollEventType.SmallIncrement) ? smallValue : -smallValue);
					break;

				case ScrollEventType.EndScroll:
				case ScrollEventType.ThumbPosition:
				case ScrollEventType.ThumbTrack:
					// Nothing to do here because we cannot determine the correct scroll position anyway
					break;
				default:
					Debug.Fail(string.Format("Unhandled {0}.Type value: {1}", typeof(ScrollEventArgs).Name, se.Type));
					break;
			}
			// Avoid scrolling by the inherited AutoScroll mechanism
			AutoScrollPosition = Point.Empty;
		}

		/// <override></override>
		protected override void OnResize(EventArgs e) {
			base.OnResize(e);
			ResetBounds();
			UpdateScrollBars();
			Invalidate();
		}

		/// <override></override>
		protected override void OnLayout(LayoutEventArgs e) {
			base.OnLayout(e);
		}

		/// <override></override>
		protected override void OnInvalidated(InvalidateEventArgs e) {
			base.OnInvalidated(e);
		}

		/// <override></override>
		protected override void OnPaintBackground(PaintEventArgs e) {
			if (BackgroundImage != null) base.OnPaintBackground(e);
			else {
				try {
					CurrentGraphics = e.Graphics;
					CurrentGraphics.Clear(Color.Transparent);
					if (Parent != null && BackColor.A < 255 || BackColorGradient.A < 255) {
						// If the display has a transparent background, force the display's parent to redraw
						// by calling InvokePaintBackground and InvokePaint for the parent control.
						Rectangle rect = Rectangle.Empty;
						rect.Offset(Left, Top);
						rect.Width = Width;
						rect.Height = Height;
						e.Graphics.TranslateTransform(-rect.X, -rect.Y);
						try {
							using (PaintEventArgs pea = new PaintEventArgs(e.Graphics, rect)) {
								pea.Graphics.SetClip(rect);
								InvokePaintBackground(Parent, pea);
								InvokePaint(Parent, pea);
							}
						} finally {
							e.Graphics.TranslateTransform(rect.X, rect.Y);
						}
					}
					DrawControl(e.ClipRectangle);
				} finally {
					CurrentGraphics = null;
				}
			}
		}

		/// <override></override>
		protected override void OnPaint(PaintEventArgs e) {
			try {
#if DEBUG_UI
				//Console.WriteLine("[{0}]\t OnPaint called", DateTime.Now.ToString("HH:mm:ss.ffff"));
				_stopWatch.Reset();
				_stopWatch.Start();
#endif
				CurrentGraphics = e.Graphics;

				// =====   DRAW DIAGRAM   =====
				if (Diagram != null) {
					Diagram.IsFastRedrawEnabled = (ActiveTool != null && ActiveTool.IsToolActionPending);
					DrawDiagram(e.ClipRectangle);
				}

				// =====   DRAW UNIVERSAL SCROLL INDICATOR   =====
				if (_universalScrollEnabled)
					_universalScrollCursor.Draw(_currentGraphics, _universalScrollFixPointBounds);

				// =====   DRAW DEBUG INFO   =====
#if DEBUG_UI
				_stopWatch.Stop();
				string debugInfoTxt = string.Empty;
				debugInfoTxt += string.Format("{1} ms{0}", Environment.NewLine, _stopWatch.ElapsedMilliseconds);
				_currentGraphics.DrawString(debugInfoTxt, Font, Brushes.Red, Point.Empty);

				//float drawAreaCenterX = (DrawBounds.X + DrawBounds.Width) / 2f;
				//float drawAreaCenterY = (DrawBounds.Y + DrawBounds.Height) / 2f;
				//float zoomedDiagramWidth = (DiagramBounds.Width * zoomfactor) + (2 * scrollAreaMargin);
				//float zoomedDiagramHeight = (DiagramBounds.Height * zoomfactor) + (2 * scrollAreaMargin);

				//Rectangle r;
				//r = DiagramBounds;
				//DiagramToControl(r, out r);
				//currentGraphics.DrawRectangle(Pens.Red, r.X, r.Y, r.Width, r.Height);
				//r = ScrollAreaBounds;
				//DiagramToControl(r, out r);
				//currentGraphics.DrawRectangle(Pens.Red, r.X, r.Y, r.Width, r.Height);
#endif
			} finally {
				CurrentGraphics = null;
			}
		}

		/// <override></override>
		protected override void NotifyInvalidate(Rectangle invalidatedArea) {
			base.NotifyInvalidate(invalidatedArea);
		}

		#endregion


		#region [Protected] Methods

		/// <summary>
		/// Draws a resize grip at the given position.
		/// </summary>
		protected virtual void DrawResizeGripCore(int x, int y, IndicatorDrawMode drawMode) {
			if (_currentGraphics == null) throw new InvalidOperationException(ErrMessageOnlyAllowedWhilePaining);
			if (_graphicsIsTransformed) throw new NShapeException(ErrMessageResetTransformHasNotBeenCalled);
			if (HighQualityRendering) {
				Pen handlePen = null;
				Brush handleBrush = null;
				switch (drawMode) {
					case IndicatorDrawMode.Normal:
						handlePen = HandleNormalPen;
						handleBrush = HandleInteriorBrush;
						break;
					case IndicatorDrawMode.Deactivated:
						handlePen = HandleInactivePen;
						handleBrush = Brushes.Transparent;
						break;
					case IndicatorDrawMode.Highlighted:
						handlePen = HandleHilightPen;
						handleBrush = HandleInteriorBrush;
						break;
					default: throw new NShapeUnsupportedValueException(typeof(IndicatorDrawMode), drawMode);
				}
				DoDrawControlPointPath(_resizePointPath, x, y, handlePen, handleBrush);
			} else {
				DiagramToControl(x, y, out x, out y);
				_rectBuffer.X = x - _handleRadius;
				_rectBuffer.Y = y - _handleRadius;
				_rectBuffer.Width = _rectBuffer.Height = _handleRadius + _handleRadius;
				ControlPaint.DrawContainerGrabHandle(_currentGraphics, _rectBuffer);
			}
		}


		/// <summary>
		/// Draws a resize grip at the given position.
		/// </summary>
		protected virtual void DrawRotateGripCore(int x, int y, IndicatorDrawMode drawMode) {
			if (_currentGraphics == null) throw new InvalidOperationException(ErrMessageOnlyAllowedWhilePaining);
			if (_graphicsIsTransformed) throw new NShapeException(ErrMessageResetTransformHasNotBeenCalled);
			if (HighQualityRendering) {
				Pen handlePen = null;
				Brush handleBrush = null;
				switch (drawMode) {
					case IndicatorDrawMode.Normal:
						handlePen = HandleNormalPen;
						handleBrush = HandleInteriorBrush;
						break;
					case IndicatorDrawMode.Deactivated:
						handlePen = HandleInactivePen;
						handleBrush = Brushes.Transparent;
						break;
					case IndicatorDrawMode.Highlighted:
						handlePen = HandleHilightPen;
						handleBrush = HandleInteriorBrush;
						break;
					default: throw new NShapeUnsupportedValueException(typeof(IndicatorDrawMode), drawMode);
				}
				DoDrawControlPointPath(_rotatePointPath, x, y, handlePen, handleBrush);
			} else {
				DiagramToControl(x, y, out x, out y);
				_rectBuffer.X = x - _handleRadius;
				_rectBuffer.Y = y - _handleRadius;
				_rectBuffer.Width =
				_rectBuffer.Height = _handleRadius + _handleRadius;
				ControlPaint.DrawGrabHandle(_currentGraphics, _rectBuffer, false, (drawMode == IndicatorDrawMode.Deactivated));
			}
		}


		/// <summary>
		/// Draws a connection point at the given position. 
		/// If the connection point is also a resize grip, the resize grip will be drawn, too.
		/// If the connection point is a is point-to-shape connected glue point, the outline of the connected shape will be highlighted.
		/// </summary>
		protected virtual void DrawConnectionPointCore(int x, int y, IndicatorDrawMode drawMode, bool isResizeGrip) {
			DrawConnectionPointCore(x, y, drawMode, isResizeGrip, ShapeConnectionInfo.Empty);
		}


		/// <summary>
		/// Draws a connection point at the given position. 
		/// If the connection point is also a resize grip, the resize grip will be drawn, too.
		/// If the connection point is a is point-to-shape connected glue point, the outline of the connected shape will be highlighted.
		/// </summary>
		protected virtual void DrawConnectionPointCore(int x, int y, IndicatorDrawMode drawMode, bool isResizeGrip, ShapeConnectionInfo connection) {
			if (_currentGraphics == null) throw new InvalidOperationException(ErrMessageOnlyAllowedWhilePaining);
			if (_graphicsIsTransformed) throw new NShapeException(ErrMessageResetTransformHasNotBeenCalled);
			if (HighQualityRendering) {
				int hdlRad;
				Pen handlePen = null;
				Brush handleBrush = null;
				switch (drawMode) {
					case IndicatorDrawMode.Normal:
						handlePen = HandleInactivePen;
						handleBrush = HandleInteriorBrush;
						hdlRad = _handleRadius;
						// If the control point is a glue point, highlight the connected connection points
						if (!connection.IsEmpty) {
							if (connection.OtherPointId == ControlPointId.Reference) {
								// If the glue point is attached to a shape instead of a connection point, highlight the connected shape's outline.
								DoRestoreTransformation();
								DoDrawShapeOutline(IndicatorDrawMode.Highlighted, connection.OtherShape);
								DoResetTransformation();
							}
							handlePen = HandleHilightPen;
						}
						break;
					case IndicatorDrawMode.Deactivated:
						handlePen = HandleInactivePen;
						handleBrush = Brushes.Transparent;
						hdlRad = _handleRadius;
						break;
					case IndicatorDrawMode.Highlighted:
						handlePen = HandleHilightPen;
						handleBrush = HandleInteriorBrush;
						hdlRad = _handleRadius + 1;
						break;
					default: throw new NShapeUnsupportedValueException(typeof(IndicatorDrawMode), drawMode);
				}
				if (isResizeGrip) {
					// Determine the grip that has to be drawn first
					if (_resizePointShape > _connectionPointShape) {
						DrawResizeGripCore(x, y, IndicatorDrawMode.Normal);
						DoDrawControlPointPath(_connectionPointPath, x, y, handlePen, handleBrush);
					} else {
						DoDrawControlPointPath(_connectionPointPath, x, y, handlePen, handleBrush);
						DrawResizeGripCore(x, y, IndicatorDrawMode.Normal);
					}

				} else DoDrawControlPointPath(_connectionPointPath, x, y, handlePen, handleBrush);
			} else {
				DiagramToControl(x, y, out x, out y);
				_rectBuffer.X = x - _handleRadius;
				_rectBuffer.Y = y - _handleRadius;
				_rectBuffer.Width =
				_rectBuffer.Height = _handleRadius + _handleRadius;
				ControlPaint.DrawGrabHandle(_currentGraphics, _rectBuffer, true, true);
			}
		}

		#endregion


		#region [Protected] Method overrides

		/// <override></override>
		protected override bool IsInputKey(Keys keyData) {
			// Get raw key data (pressed key without modifier keys)
			Keys rawKeyData = keyData & ~Keys.Modifiers;
			// Define cursor keys input keys for the display control. 
			// This is necessary because the display's scrollbars will receive the focus when 
			// pressing the arrow keys (e.g. for moving shapes with keyboard) which
			// will break scrolling and zooming behavior as the scrollbar handle arrow keys and mouse
			// wheel themselfs (for scrolling) and they will not loose the focus until another control 
			// (that is not part of the display control) was focused.
			switch (rawKeyData) {
			    case Keys.Down:
			    case Keys.Left:
			    case Keys.Right:
			    case Keys.Up:
			        return true;
				// Handling the modifier keys as InputKeys is not necessary but useful for debugging...
				//case Keys.ShiftKey:
				//case Keys.LShiftKey:
				//case Keys.RShiftKey:
				//case Keys.ControlKey:
				//case Keys.LControlKey:
				//case Keys.RControlKey:
				//case Keys.Alt:
				//    return true;
			    default:
			        return base.IsInputKey(keyData);
			}
		}

		#endregion


		private bool MultiSelect {
			get {
				if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift) return true;
				else if ((Control.ModifierKeys & Keys.ShiftKey) == Keys.ShiftKey) return true;
				else if ((Control.ModifierKeys & Keys.Control) == Keys.Control) return true;
				else if ((Control.ModifierKeys & Keys.ControlKey) == Keys.ControlKey) return true;
				else return false;
			}
		}


		#region [Private] Properties: Pens, Brushes, Bounds, etc

		private Graphics CurrentGraphics {
			get { return _currentGraphics; }
			set {
				if (_currentGraphics != null) {
					if (value != null) 
						throw new InvalidOperationException(string.Format(Dataweb.NShape.WinFormsUI.Properties.Resources.MessageFmt_CurrentGraphicsIsNotNull0, Environment.NewLine));
					_graphicsIsTransformed = false;
				}
				_currentGraphics = value;
				if (_currentGraphics != null) {
					GdiHelpers.ApplyGraphicsSettings(_currentGraphics, _currentRenderingQuality);
					_graphicsIsTransformed = false;
				}
			}
		}


		/// <summary>
		/// The bounds of the diagram 'sheet'.
		/// </summary>
		private Rectangle DiagramBounds {
			get {
				if (Diagram == null) return Rectangle.Empty;
				else {
					Rectangle r = Rectangle.Empty;
					r.Size = Diagram.Size;
					return r;
				}
			}
		}


		/// <summary>
		/// The bounds of the scrollable area: The bounds of the diagram including all its shapes (also shapes beside the diagram 'sheet') and the scroll margin.
		/// </summary>
		private Rectangle ScrollAreaBounds {
			get {
				if (Diagram == null) return Rectangle.Empty;
				if (!Geometry.IsValid(_scrollAreaBounds)) {
					Rectangle shapeBounds = Diagram.Shapes.GetBoundingRectangle(false);
					_scrollAreaBounds = Geometry.UniteRectangles(0, 0, Diagram.Width, Diagram.Height, Geometry.IsValid(shapeBounds) ? shapeBounds : Rectangle.Empty);
					_scrollAreaBounds.Inflate(scrollAreaMargin, scrollAreaMargin);
				}
				return _scrollAreaBounds;
			}
		}


		private bool HScrollBarVisible {
			get { return _scrollBarHVisible; }
			set {
				_scrollBarHVisible = value;
				scrollBarH.Visible = _scrollBarHVisible;
				hScrollBarPanel.Visible = _scrollBarHVisible;
				if (!_scrollBarHVisible) SetScrollPosX(0);
			}
		}


		private bool VScrollBarVisible {
			get { return _scrollBarVVisible; }
			set {
				_scrollBarVVisible = value;
				scrollBarV.Visible = _scrollBarVVisible;
				if (!_scrollBarVVisible) SetScrollPosY(0);
			}
		}


		private Pen GridPen {
			get {
				if (_gridPen == null)
					//CreatePen(gridColor, gridAlpha, 1, new float[2] { 2, 2 }, false, ref gridPen);
					CreatePen(_gridColor, _gridAlpha, 1, null, false, ref _gridPen);
				return _gridPen;
			}
		}


		private Pen OutlineInteriorPen {
			get {
				if (_outlineInteriorPen == null) CreatePen(_selectionInteriorColor, ref _outlineInteriorPen);
				return _outlineInteriorPen;
			}
		}


		private Pen OutlineNormalPen {
			get {
				if (_outlineNormalPen == null) CreatePen(_selectionNormalColor, _selectionAlpha, _handleRadius, ref _outlineNormalPen);
				return _outlineNormalPen;
			}
		}


		private Pen OutlineHilightPen {
			get {
				if (_outlineHilightPen == null) CreatePen(_selectionHilightColor, _selectionAlpha, _handleRadius, ref _outlineHilightPen);
				return _outlineHilightPen;
			}
		}


		private Pen OutlineInactivePen {
			get {
				if (_outlineInactivePen == null) CreatePen(_selectionInactiveColor, _selectionAlpha, _handleRadius, ref _outlineInactivePen);
				return _outlineInactivePen;
			}
		}


		private Pen HandleNormalPen {
			get {
				if (_handleNormalPen == null) CreatePen(_selectionNormalColor, ref _handleNormalPen);
				return _handleNormalPen;
			}
		}


		private Pen HandleHilightPen {
			get {
				if (_handleHilightPen == null) CreatePen(_selectionHilightColor, ref _handleHilightPen);
				return _handleHilightPen;
			}
		}


		private Pen HandleInactivePen {
			get {
				if (_handleInactivePen == null) CreatePen(_selectionInactiveColor, ref _handleInactivePen);
				return _handleInactivePen;
			}
		}


		private Brush HandleInteriorBrush {
			get {
				if (_handleInteriorBrush == null) CreateBrush(_selectionInteriorColor, _selectionAlpha, ref _handleInteriorBrush);
				return _handleInteriorBrush;
			}
		}


		private Brush InplaceTextboxBackBrush {
			get {
				if (_inplaceTextboxBackBrush == null)
					CreateBrush(SelectionInteriorColor, inlaceTextBoxBackAlpha, ref  _inplaceTextboxBackBrush);
				return _inplaceTextboxBackBrush;
			}
		}


		private Pen ToolPreviewPen {
			get {
				if (_toolPreviewPen == null) CreatePen(_toolPreviewColor, _toolPreviewColorAlpha, ref _toolPreviewPen);
				return _toolPreviewPen;
			}
		}


		private Brush ToolPreviewBackBrush {
			get {
				if (_toolPreviewBackBrush == null) CreateBrush(_toolPreviewBackColor, _toolPreviewBackColorAlpha, ref _toolPreviewBackBrush);
				return _toolPreviewBackBrush;
			}
		}

		#endregion


		#region [Private] Methods: Invalidating

		/// <override></override>
		private void DoSuspendUpdate() {
			if (_suspendUpdateCounter == 0)
				Debug.Assert(_invalidatedAreaBuffer == Rectangle.Empty);
			++_suspendUpdateCounter;
		}


		/// <override></override>
		private void DoResumeUpdate() {
			if (_suspendUpdateCounter <= 0) throw new InvalidOperationException("Missing subsequent call of method SuspendUpdate.");
			--_suspendUpdateCounter;
			if (_suspendUpdateCounter == 0) {
				if (_boundsChanged) {
					ResetBounds();
					UpdateScrollBars();
					Invalidate();
				} else {
					if (!_invalidatedAreaBuffer.IsEmpty && Geometry.IsValid(_invalidatedAreaBuffer))
						base.Invalidate(_invalidatedAreaBuffer, true);
				}
				_invalidatedAreaBuffer = Rectangle.Empty;
			}
		}


		/// <summary>
		/// Invalidates the shape (or its parent(s)) along with all ControlPoints
		/// </summary>
		/// <param name="shape"></param>
		private void DoInvalidateShape(Shape shape) {
			DoSuspendUpdate();
			if (shape.Parent != null)
				// parents invalidate their children themselves
				DoInvalidateShape(shape.Parent);
			else {
				shape.Invalidate();
				foreach (ControlPointId gluePtId in shape.GetControlPointIds(ControlPointCapabilities.Glue)) {
					if (shape.IsConnected(gluePtId, null) == ControlPointId.Reference)
						DoInvalidateShape(shape.GetConnectionInfo(gluePtId, null).OtherShape);
				}
				DoInvalidateGrips(shape, ControlPointCapabilities.All);
			}
			DoResumeUpdate();
		}


		private void DoInvalidateGrips(Shape shape, ControlPointCapabilities controlPointCapability) {
			if (shape == null) throw new ArgumentNullException("shape");
			Point p = Point.Empty;
			int transformedHandleRadius;
			ControlToDiagram(_handleRadius, out transformedHandleRadius);
			++transformedHandleRadius;
			Rectangle r = Rectangle.Empty;
			foreach (ControlPointId id in shape.GetControlPointIds(controlPointCapability)) {
				p = shape.GetControlPointPosition(id);
				if (r.IsEmpty) {
					r.X = p.X;
					r.Y = p.Y;
					r.Width = r.Height = 1;
				} else r = Geometry.UniteRectangles(p.X, p.Y, p.X, p.Y, r);
			}
			r.Inflate(transformedHandleRadius, transformedHandleRadius);
			DoInvalidateDiagram(r);

			// This consumes twice the time of the solution above:
			//int transformedHandleSize = transformedHandleRadius+transformedHandleRadius;
			//foreach (int pointId in shape.GetControlPointIds(controlPointCapabilities)) {
			//   p = shape.GetControlPointPosition(pointId);
			//   Invalidate(p.X - transformedHandleRadius, p.Y - transformedHandleRadius, transformedHandleSize, transformedHandleSize);
			//}
		}


		private void DoInvalidateDiagram(int x, int y, int width, int height) {
			_rectBuffer.X = x;
			_rectBuffer.Y = y;
			_rectBuffer.Width = width;
			_rectBuffer.Height = height;
			DoInvalidateDiagram(_rectBuffer);
		}


		private void DoInvalidateDiagram(Rectangle rect) {
			if (!Geometry.IsValid(rect)) throw new ArgumentException("rect");
			DiagramToControl(rect, out _rectBuffer);
			_rectBuffer.Inflate(_invalidateDelta + 1, _invalidateDelta + 1);

#if DEBUG_UI
			if (_isInvalidateAreasVisible) {
				if (_invalidatedAreas == null) _invalidatedAreas = new List<Rectangle>();
				_invalidatedAreas.Add(rect);
			}
#endif

			// traditional rendering
			if (_suspendUpdateCounter > 0) _invalidatedAreaBuffer = Geometry.UniteRectangles(_invalidatedAreaBuffer, _rectBuffer);
			else base.Invalidate(_rectBuffer);
		}

		#endregion


		#region [Private] Methods: Calculating and applying transformation

		private void CalcDiagramPosition() {
			float zoomedDiagramWidth = (ScrollAreaBounds.Width - (2 * scrollAreaMargin)) * _zoomfactor;
			float zoomedDiagramHeight = (ScrollAreaBounds.Height - (2 * scrollAreaMargin)) * _zoomfactor;
			float zoomedDiagramOffsetX = ((ScrollAreaBounds.X + scrollAreaMargin) * _zoomfactor);
			float zoomedDiagramOffsetY = ((ScrollAreaBounds.Y + scrollAreaMargin) * _zoomfactor);
			float drawAreaCenterX = (DrawBounds.X + DrawBounds.Width) / 2f;
			float drawAreaCenterY = (DrawBounds.Y + DrawBounds.Height) / 2f;
			_diagramPosX = (int)Math.Round(-zoomedDiagramOffsetX + (drawAreaCenterX - (zoomedDiagramWidth / 2)));
			_diagramPosY = (int)Math.Round(-zoomedDiagramOffsetY + (drawAreaCenterY - (zoomedDiagramHeight / 2)));
		}


		private void DoRestoreTransformation() {
			if (_currentGraphics == null) throw new InvalidOperationException(ErrMessageOnlyAllowedWhilePaining);
			if (_graphicsIsTransformed) throw new InvalidOperationException("Graphics context is already transformed.");
			// transform graphics object
			_currentGraphics.ScaleTransform(_zoomfactor, _zoomfactor, MatrixOrder.Prepend);
			_currentGraphics.TranslateTransform(_diagramPosX, _diagramPosY, MatrixOrder.Append);
			_currentGraphics.TranslateTransform(-_scrollPosX * _zoomfactor, -_scrollPosY * _zoomfactor, MatrixOrder.Append);
			_graphicsIsTransformed = true;
		}


		private void DoResetTransformation() {
			if (_currentGraphics == null) throw new InvalidOperationException(ErrMessageOnlyAllowedWhilePaining);
			if (!_graphicsIsTransformed) throw new InvalidOperationException("Graphics context is not transformed.");
			Debug.Assert(_graphicsIsTransformed);
			_currentGraphics.ResetTransform();
			_graphicsIsTransformed = false;
		}

		#endregion


		#region [Private] Methods: Drawing

		/// <summary>
		/// Draws the bounds of all captions of the shape
		/// </summary>
		private void DoDrawCaptionBounds(IndicatorDrawMode drawMode, ICaptionedShape shape) {
			if (shape == null) throw new ArgumentNullException("shape");
			if (_graphicsIsTransformed) throw new NShapeException(ErrMessageResetTransformHasNotBeenCalled);
			if (Project.SecurityManager.IsGranted(Permission.Data, (Shape)shape)) {
				for (int i = shape.CaptionCount - 1; i >= 0; --i)
					DoDrawCaptionBounds(drawMode, shape, i);
			}
		}


		private void DoDrawCaptionBounds(IndicatorDrawMode drawMode, ICaptionedShape shape, int captionIndex) {
			if (shape == null) throw new ArgumentNullException("shape");
			if (_currentGraphics == null) throw new InvalidOperationException(ErrMessageOnlyAllowedWhilePaining);
			if (_graphicsIsTransformed) throw new NShapeException(ErrMessageResetTransformHasNotBeenCalled);
			// Skip shapes that are not visible
			if (!IsLayerVisible(((Shape)shape).HomeLayer, ((Shape)shape).SupplementalLayers))
				return;
			if (_inplaceTextbox == null || _inplaceShape != shape || _inplaceCaptionIndex != captionIndex) {
				// Draw caption rectangle (placeholder bounds are calculated for empty captions
				shape.GetCaptionTextBounds(captionIndex, out _pointBuffer[0], out _pointBuffer[1], out _pointBuffer[2], out _pointBuffer[3]);
				DiagramToControl(_pointBuffer[0], out _pointBuffer[0]);
				DiagramToControl(_pointBuffer[1], out _pointBuffer[1]);
				DiagramToControl(_pointBuffer[2], out _pointBuffer[2]);
				DiagramToControl(_pointBuffer[3], out _pointBuffer[3]);
				Pen pen = null;
				switch (drawMode) {
					case IndicatorDrawMode.Deactivated:
						pen = HandleInactivePen;
						break;
					case IndicatorDrawMode.Normal:
						pen = HandleNormalPen;
						break;
					case IndicatorDrawMode.Highlighted:
						pen = HandleHilightPen;
						break;
					default: throw new NShapeUnsupportedValueException(drawMode);
				}
				_currentGraphics.DrawPolygon(pen, _pointBuffer);
			}
		}


		private void DoDrawControlPoints(Shape shape) {
			DoDrawControlPoints(shape, ControlPointCapabilities.Resize | ControlPointCapabilities.Connect | ControlPointCapabilities.Glue);
		}


		private void DoDrawControlPoints(Shape shape, ControlPointCapabilities capabilities) {
			DoDrawControlPoints(shape, capabilities, IndicatorDrawMode.Normal);
		}


		private void DoDrawControlPoints(Shape shape, ControlPointCapabilities capabilities, IndicatorDrawMode drawMode) {
			if (shape == null) throw new ArgumentNullException("shape");
			if (_graphicsIsTransformed) throw new NShapeException(ErrMessageResetTransformHasNotBeenCalled);
			Point p = Point.Empty;
			if (!IsLayerVisible(((Shape)shape).HomeLayer, ((Shape)shape).SupplementalLayers))
				return;
			// First, draw Resize- and ConnectionPoints
			foreach (ControlPointId id in shape.GetControlPointIds(capabilities)) {
				if (id == ControlPointId.Reference) continue;

				// Get point position and transform the coordinates
				p = shape.GetControlPointPosition(id);
				bool isResizeGrip = shape.HasControlPointCapability(id, ControlPointCapabilities.Resize);
				if (shape.HasControlPointCapability(id, ControlPointCapabilities.Glue)) {
					ShapeConnectionInfo connectionInfo = shape.GetConnectionInfo(id, null);
					DrawConnectionPointCore(p.X, p.Y, drawMode, isResizeGrip, connectionInfo);
				} else if (shape.HasControlPointCapability(id, ControlPointCapabilities.Connect)) {
					if (shape.HasControlPointCapability(id, ControlPointCapabilities.Reference)) continue;
					DrawConnectionPointCore(p.X, p.Y, drawMode, isResizeGrip);
				} else if (isResizeGrip)
					DrawResizeGripCore(p.X, p.Y, drawMode);
			}
			// Draw the roation point on top of all other points
			if ((capabilities & ControlPointCapabilities.Rotate) != 0) {
				foreach (ControlPointId id in shape.GetControlPointIds(ControlPointCapabilities.Rotate)) {
					p = shape.GetControlPointPosition(id);
					DrawRotateGripCore(p.X, p.Y, drawMode);
				}
			}
		}


		private void DoDrawShapeOutline(IndicatorDrawMode drawMode, Shape shape) {
			Debug.Assert(shape != null);
			Debug.Assert(_currentGraphics != null);
			if (!IsLayerVisible(((Shape)shape).HomeLayer, ((Shape)shape).SupplementalLayers))
				return;

			Pen backgroundPen = null;
			Pen foregroundPen = null;
			if (shape.Parent != null) DoDrawParentOutline(shape.Parent);
			switch (drawMode) {
				case IndicatorDrawMode.Deactivated:
					backgroundPen = OutlineInactivePen;
					foregroundPen = OutlineInteriorPen;
					break;
				case IndicatorDrawMode.Normal:
					backgroundPen = OutlineNormalPen;
					foregroundPen = OutlineInteriorPen;
					break;
				case IndicatorDrawMode.Highlighted:
					backgroundPen = OutlineHilightPen;
					foregroundPen = OutlineInteriorPen;
					break;
				default: throw new NShapeUnsupportedValueException(typeof(IndicatorDrawMode), drawMode);
			}
			// scale lineWidth 
			backgroundPen.Width = GripSize / _zoomfactor;
			foregroundPen.Width = 1 / _zoomfactor;

			shape.DrawOutline(_currentGraphics, backgroundPen);
			shape.DrawOutline(_currentGraphics, foregroundPen);
		}


		private void DoDrawParentOutline(Shape parentShape) {
			Debug.Assert(_currentGraphics != null);
			if (!IsLayerVisible(((Shape)parentShape).HomeLayer, ((Shape)parentShape).SupplementalLayers))
				return;
			if (parentShape.Parent != null) DoDrawParentOutline(parentShape.Parent);
			parentShape.DrawOutline(_currentGraphics, OutlineInactivePen);
		}


		/// <summary>
		/// Translates and draws the given ControlPoint path at the given position (diagram coordinates).
		/// </summary>
		private void DoDrawControlPointPath(GraphicsPath path, int x, int y, Pen pen, Brush brush) {
			if (_currentGraphics == null) throw new InvalidOperationException(ErrMessageOnlyAllowedWhilePaining);
			// transform the given 
			DiagramToControl(x, y, out x, out y);

			// transform ControlPoint Shape
			_pointMatrix.Reset();
			_pointMatrix.Translate(x, y);
			path.Transform(_pointMatrix);
			// draw ConnectionPoint shape
			_currentGraphics.FillPath(brush, path);
			_currentGraphics.DrawPath(pen, path);
			// undo ControlPoint transformation
			_pointMatrix.Reset();
			_pointMatrix.Translate(-x, -y);
			path.Transform(_pointMatrix);
		}


		private void UpdateScrollBars() {
			try {
				//SuspendLayout();
				if (_showScrollBars && Diagram != null) {
					// Update diagram offset and draw area
					Rectangle drawBoundsDgrmCoords;
					CalcDiagramPosition();
					ControlToDiagram(DrawBounds, out drawBoundsDgrmCoords);

					// Show/hide vertical scroll bar
					if (ScrollAreaBounds.Height < drawBoundsDgrmCoords.Height) {
						if (VScrollBarVisible) VScrollBarVisible = false;
					} else if (!VScrollBarVisible && _showScrollBars) VScrollBarVisible = true;
					// Show/hide horizontal scroll bar
					if (ScrollAreaBounds.Width < drawBoundsDgrmCoords.Width) {
						if (HScrollBarVisible) {
							HScrollBarVisible = false;
							//Debug.Assert(hScrollBarPanel.Visible == false && scrollBarH.Visible == false);
						}
					} else if (!HScrollBarVisible && _showScrollBars) {
						HScrollBarVisible = true;
						//Debug.Assert(hScrollBarPanel.Visible == true && scrollBarH.Visible == true);
					}
					// Set scrollbar's width/height
					scrollBarV.Height = DrawBounds.Height;
					scrollBarH.Width = DrawBounds.Width;

					// Update diagram offset and draw area
					CalcDiagramPosition();
					ControlToDiagram(DrawBounds, out drawBoundsDgrmCoords);
					if (HScrollBarVisible || VScrollBarVisible) {
						int zoomedDiagramPosX = (int)Math.Round(_diagramPosX / _zoomfactor);
						int zoomedDiagramPosY = (int)Math.Round(_diagramPosY / _zoomfactor);
						int smallChange = 1;	// Math.Max(1, GridSize / 2);

						// Set up vertical scrollbar
						if (VScrollBarVisible) {
							scrollBarV.SmallChange = smallChange;
							scrollBarV.LargeChange = Math.Max(1, drawBoundsDgrmCoords.Height);
							scrollBarV.Minimum = ScrollAreaBounds.Y + zoomedDiagramPosY;
							scrollBarV.Maximum = ScrollAreaBounds.Height + scrollBarV.Minimum;
						}

						// Set horizontal scrollBar position, size  and limits
						if (HScrollBarVisible) {
							scrollBarH.SmallChange = smallChange;
							scrollBarH.LargeChange = Math.Max(1, drawBoundsDgrmCoords.Width);
							scrollBarH.Minimum = ScrollAreaBounds.X + zoomedDiagramPosX;
							scrollBarH.Maximum = ScrollAreaBounds.Width + scrollBarH.Minimum;
						}

						// Maintain scroll position when zooming out
						ScrollTo(scrollBarH.Value, scrollBarV.Value);
					}
				} else {
					if (VScrollBarVisible) VScrollBarVisible = false;
					if (HScrollBarVisible) HScrollBarVisible = false;
				}
				_boundsChanged = false;
			} finally {
				//ResumeLayout(); 
			}
		}


		private void UpdateControlBrush() {
			if (_controlBrush == null) {
				if (_highQualityBackground) {
					_rectBuffer.X = 0;
					_rectBuffer.Y = 0;
					_rectBuffer.Width = 1000;
					_rectBuffer.Height = 1000;
					if (_gradientBackColor == Color.Empty) {
						// create a gradient brush based on the given BackColor (1/3 lighter in the upperLeft, 1/3 darker in the lowerRight)
						int lR = BackColor.R + ((BackColor.R / 3)); if (lR > 255) lR = 255;
						int lG = BackColor.G + ((BackColor.G / 3)); if (lG > 255) lG = 255;
						int lB = BackColor.B + ((BackColor.B / 3)); if (lB > 255) lB = 255;
						int dR = BackColor.R - ((BackColor.R / 3)); if (lR < 0) lR = 0;
						int dG = BackColor.G - ((BackColor.G / 3)); if (lG < 0) lG = 0;
						int dB = BackColor.B - ((BackColor.B / 3)); if (lB < 0) lB = 0;
						_controlBrush = new LinearGradientBrush(_rectBuffer, Color.FromArgb(lR, lG, lB), Color.FromArgb(dR, dG, dB), _controlBrushGradientAngle);
					} else _controlBrush = new LinearGradientBrush(_rectBuffer, BackColorGradient, BackColor, _controlBrushGradientAngle);
				} else _controlBrush = new SolidBrush(BackColor);
				_controlBrushSize = Size.Empty;
			}

			// apply transformation
			if (_controlBrush is LinearGradientBrush && this.Size != _controlBrushSize) {
				double rectWidth = Math.Abs((1000 * _controlBrushGradientCos) - (1000 * _controlBrushGradientSin));		// (width * cos) - (Height * sin)
				double rectHeight = Math.Abs((1000 * _controlBrushGradientSin) + (1000 * _controlBrushGradientCos));		// (width * sin) + (height * cos)
				double gradLen = (rectWidth + rectHeight) / 2;
				float scaleX = (float)(Width / gradLen);
				float scaleY = (float)(Height / gradLen);

				((LinearGradientBrush)_controlBrush).ResetTransform();
				((LinearGradientBrush)_controlBrush).ScaleTransform(scaleX, scaleY);
				((LinearGradientBrush)_controlBrush).RotateTransform(_controlBrushGradientAngle);
				_controlBrushSize = this.Size;
			}
		}


		/// <summary>
		/// Draw the display control including the diagram's shadow (if a diagram is displayed)
		/// </summary>
		private void DrawControl(Rectangle clipRectangle) {
			UpdateControlBrush();
			clipRectangle.Inflate(2, 2);
			if (Diagram == null || !_drawDiagramSheet) {
				// No diagram is shown... just fill the control with its background color/gradient
				//graphics.FillRectangle(controlBrush, clipRectangle.X, clipRectangle.Y, clipRectangle.Width, clipRectangle.Height);
				_currentGraphics.FillRectangle(_controlBrush, ClientRectangle);
			} else {
				Rectangle ctrlDiagramBounds = Rectangle.Empty;
				DiagramToControl(DiagramBounds, out ctrlDiagramBounds);

				// =====   DRAW CONTROL BACKGROUND   =====
				if (Diagram != null && !Diagram.BackgroundColor.IsEmpty && Diagram.BackgroundColor.A == 255 && Diagram.BackgroundGradientColor.A == 255) {
					// Above the diagram
					if (clipRectangle.Top < ctrlDiagramBounds.Top)
						_currentGraphics.FillRectangle(_controlBrush,
							clipRectangle.Left, clipRectangle.Top,
							clipRectangle.Width, ctrlDiagramBounds.Top - clipRectangle.Top);
					// Left of the diagram
					if (clipRectangle.Left < ctrlDiagramBounds.Left)
						_currentGraphics.FillRectangle(_controlBrush,
							clipRectangle.Left, Math.Max(clipRectangle.Top, ctrlDiagramBounds.Top - 1),
							ctrlDiagramBounds.Left - clipRectangle.Left, Math.Min(clipRectangle.Height, ctrlDiagramBounds.Height + 2));
					// Right of the diagram
					if (clipRectangle.Right > ctrlDiagramBounds.Right)
						_currentGraphics.FillRectangle(_controlBrush,
							ctrlDiagramBounds.Right, Math.Max(clipRectangle.Top, ctrlDiagramBounds.Top - 1),
							clipRectangle.Right - ctrlDiagramBounds.Right, Math.Min(clipRectangle.Height, ctrlDiagramBounds.Height + 2));
					// Below the diagram
					if (clipRectangle.Bottom > ctrlDiagramBounds.Bottom)
						_currentGraphics.FillRectangle(_controlBrush,
							clipRectangle.Left, ctrlDiagramBounds.Bottom,
							clipRectangle.Width, clipRectangle.Bottom - ctrlDiagramBounds.Bottom);
				} else _currentGraphics.FillRectangle(_controlBrush, clipRectangle);

				// =====   DRAW DIAGRAM SHADOW   =====
				if (clipRectangle.Right >= ctrlDiagramBounds.Right || clipRectangle.Bottom >= ctrlDiagramBounds.Bottom) {
					if (Diagram.BackgroundColor.A == 0 && Diagram.BackgroundGradientColor.A == 0 && NamedImage.IsNullOrEmpty(Diagram.BackgroundImage)) {
						// Transparent diagrams have no shadow... so there's nothing to do.
					} else {
						// Change alpha value of brush according to the diagram's alpha:
						// This solves the problem that transparent 
						if (_diagramShadowBrush is SolidBrush) {
							// Shadow alpha is half of the average value of the background color's alpha:
							int alpha = Math.Max(1, (Diagram.BackgroundColor.A + Diagram.BackgroundGradientColor.A) / 4);
							if (alpha != ((SolidBrush)_diagramShadowBrush).Color.A) {
								Color shadowColor = Color.FromArgb(alpha, ((SolidBrush)_diagramShadowBrush).Color);
								DisposeObject(ref _diagramShadowBrush);
								_diagramShadowBrush = new SolidBrush(shadowColor);
							}
						}
						// Now draw the shadow
						int zoomedShadowSize = (int)Math.Round(shadowSize * _zoomfactor);
						// This feature is currently deactivated because it can be confusing for the
						// user when the shadow shines through the diagram
						//if (Diagram.BackgroundColor.A < 255 || Diagram.BackgroundGradientColor.A < 255)
						//    currentGraphics.FillRectangle(diagramShadowBrush, diagramBounds.X + zoomedShadowSize, diagramBounds.Y + zoomedShadowSize, diagramBounds.Width, diagramBounds.Height);
						//else 
						{
							_currentGraphics.FillRectangle(_diagramShadowBrush,
								ctrlDiagramBounds.Right, ctrlDiagramBounds.Y + zoomedShadowSize,
								zoomedShadowSize, ctrlDiagramBounds.Height);
							_currentGraphics.FillRectangle(_diagramShadowBrush,
								ctrlDiagramBounds.X + zoomedShadowSize, ctrlDiagramBounds.Bottom,
								ctrlDiagramBounds.Width - zoomedShadowSize, zoomedShadowSize);
						}
					}
				}
			}
		}


		/// <summary>
		/// Draw the diagram, selection indicators and other stuff
		/// </summary>
		private void DrawDiagram(Rectangle clipRectangle) {
			Debug.Assert(Diagram != null);

			// Clipping transformation
			// transform clipping area from control to Diagram coordinates
			clipRectangle.Inflate(_invalidateDelta + 1, _invalidateDelta + 1);
			ControlToDiagram(clipRectangle, out _clipRectBuffer);

			Rectangle diagramBounds = Rectangle.Empty;
			DiagramToControl(DiagramBounds, out diagramBounds);

			if (_drawDiagramSheet) {
				// Draw Background
				DoRestoreTransformation();
				Diagram.DrawBackground(_currentGraphics, _clipRectBuffer);
				DoResetTransformation();
				// Draw grid
				if (_gridVisible)
					DrawGrid(ref clipRectangle, ref diagramBounds);
				// Draw diagram border
				_currentGraphics.DrawRectangle(Pens.Black, diagramBounds);
			}

			// Get visible layers
			HashCollection<int> visibleHomeLayers = new HashCollection<int>(GetVisibleLayerIds());
			LayerIds visibleSharedLayers = Layer.ConvertToLayerIds(visibleHomeLayers);

			// Draw shapes and their outlines (selection indicator)
			DoRestoreTransformation();
			// Draw Shapes
			Diagram.DrawShapes(_currentGraphics, visibleSharedLayers, visibleHomeLayers, _clipRectBuffer);
			// Draw selection indicator(s)
			foreach (Shape shape in _selectedShapes.BottomUp) {
				if (shape.DisplayService != this) {
					Debug.Fail("Invalid display service");
					continue;
				}
				if (Diagram.IsShapeVisible(shape, visibleSharedLayers, visibleHomeLayers)) {
					Rectangle shapeBounds = shape.GetBoundingRectangle(false);
					if (Geometry.RectangleIntersectsWithRectangle(shapeBounds, _clipRectBuffer)) {
						// ToDo:  (not sure if we want these features)
						// * Should DrawShapeOutline(...) draw LinearShapes with LineCaps? (It doesn't at the moment)
						// * If the selected shape implements ILinearShape, try to get its LineCaps
						// * Find a way how to obtain the CustomLineCaps from the ToolCache without knowing if a line has CapStyles properties...

						// Draw Shape's Outline
						DoDrawShapeOutline(IndicatorDrawMode.Normal, shape);
					} //else Debug.Print("{0} does not intersect with clipping rectangle {1}", shape.Type.Name, clipRectBuffer);
				}
			}

			// Determine the type of handles to draw
			ControlPointCapabilities capabilities = ControlPointCapabilities.None;
			if (_selectedShapes.Count == 1) capabilities = ControlPointCapabilities.All; //ControlPointCapabilities.Rotate | ControlPointCapabilities.Glue | ControlPointCapabilities.Resize | ControlPointCapabilities.Movable | ControlPointCapabilities.Connect;
			else if (_selectedShapes.Count > 1) {
				// Determine the common control points
				capabilities = ControlPointCapabilities.None;
				if (_selectedShapes.TopMost is IPlanarShape) {
					capabilities = ControlPointCapabilities.Rotate | ControlPointCapabilities.Resize;
					// If all selected shapes are of the same type, highlight all handles.
					// Otherwise only the rotate handle
					ShapeType shapeType = _selectedShapes.TopMost.Type;
					foreach (Shape s in _selectedShapes) {
						// Clear all capabilities and break is a linear shape was found
						if (s is ILinearShape) {
							capabilities = ControlPointCapabilities.None;
							break;
						}
						// Remove the resize capability if shape type does not match
						if (s.Type != shapeType && (capabilities & ControlPointCapabilities.Resize) != 0)
							capabilities ^= ControlPointCapabilities.Resize;
					}
				}
			}

			// Now draw Handles, caption bounds, etc
			if (_selectedShapes.Count > 0) {
			    DoResetTransformation();
			    foreach (Shape shape in SelectedShapes.BottomUp) {
			        if (shape.DisplayService != this) {
						Debug.Fail("Invalid display service!");
			            continue;
			        }
					if (Diagram.IsShapeVisible(shape, visibleSharedLayers, visibleHomeLayers)) {
						Rectangle shapeBounds = shape.GetBoundingRectangle(false);
						if (Geometry.RectangleIntersectsWithRectangle(shapeBounds, _clipRectBuffer)) {
							// Draw ControlPoints
							DoDrawControlPoints(shape, capabilities);
							// Draw CaptionTextBounds / InPlaceTextBox
							if (_inplaceTextbox != null) {
								_currentGraphics.FillRectangle(InplaceTextboxBackBrush, _inplaceTextbox.Bounds);
								_currentGraphics.DrawRectangle(HandleInactivePen, _inplaceTextbox.Bounds);
							}
							if (shape is ICaptionedShape)
								DoDrawCaptionBounds(IndicatorDrawMode.Deactivated, (ICaptionedShape)shape);
						} //else Debug.Print("{0} does not intersect with clipping rectangle {1}", shape.Type.Name, clipRectBuffer);
					}

				}
				// If the selected shape is a point-to-shape connected shape, highlight the partner shape's outline
				foreach (KeyValuePair<Shape, ShapeConnectionInfo> connectedShapePair in _shapesConnectedToSelectedShapes) {
					Rectangle r = connectedShapePair.Key.GetBoundingRectangle(false);
					if (Geometry.RectangleIntersectsWithRectangle(r, _clipRectBuffer))
						((IDiagramPresenter)this).DrawConnectionPoint(IndicatorDrawMode.Normal, connectedShapePair.Value.OtherShape, connectedShapePair.Value.OtherPointId);
				}
				DoRestoreTransformation();
			}

			// Draw tool preview
			if (ActiveTool != null) {
				try {
					ActiveTool.Draw(this);
				} catch (Exception exc) {
					Debug.Print(exc.Message);
					ActiveTool.Cancel();
				}
			}

			// Draw debug infos
#if DEBUG_UI
			#region Fill occupied cells and draw cell borders

			if (_isCellOccupationVisible) {
				Rectangle bounds = Geometry.UniteRectangles(0, 0, _diagramController.Diagram.Width, _diagramController.Diagram.Height,
					(_diagramController.Diagram.Shapes.Count > 0) ? _diagramController.Diagram.Shapes.GetBoundingRectangle(false) : Rectangle.Empty);

				int startX = ((bounds.X >= 0) ? bounds.X : (bounds.X / Diagram.CellSize) - 1) * Diagram.CellSize;
				int startY = ((bounds.Y >= 0) ? bounds.Y : (bounds.Y / Diagram.CellSize) - 1) * Diagram.CellSize;
				int endX = (bounds.Width + startX) - (bounds.Width % Diagram.CellSize) + ((bounds.X >= 0) ? Diagram.CellSize : 2 * Diagram.CellSize);
				int endY = (bounds.Height + startY) - (bounds.Height % Diagram.CellSize) + ((bounds.Y >= 0) ? Diagram.CellSize : 2 * Diagram.CellSize);

				((ShapeCollection)_diagramController.Diagram.Shapes).DrawOccupiedCells(_currentGraphics, startX, startY, endX, endY);
				// Draw cell borders
				for (int iX = startX; iX <= endX; iX += Diagram.CellSize)
					_currentGraphics.DrawLine(Pens.Green, iX, startY, iX, endY);
				for (int iY = startY; iY <= endY; iY += Diagram.CellSize)
					_currentGraphics.DrawLine(Pens.Green, startX, iY, endX, iY);
			}

			#endregion

			#region Visualize invalidated Rectangles
			if (_isInvalidateAreasVisible) {
				_clipRectBrush = (_clipRectBrush == _clipRectBrush1) ? _clipRectBrush2 : _clipRectBrush1;
				_invalidatedAreaPen = (_invalidatedAreaPen == _invalidatedAreaPen1) ? _invalidatedAreaPen2 : _invalidatedAreaPen1;

				if (_clipRectBuffer.Width < Bounds.Width && _clipRectBuffer.Height < Bounds.Height) {
					Debug.Print("ClipRectangle = {0}, ClipRectBuffer = {1}", clipRectangle, _clipRectBuffer);
					_currentGraphics.FillRectangle(_clipRectBrush, _clipRectBuffer);
				}

				if (_invalidatedAreas != null) {
					foreach (Rectangle area in _invalidatedAreas)
						_currentGraphics.DrawRectangle(_invalidatedAreaPen, area);
					_invalidatedAreas.Clear();
				}
			}
			#endregion

			#region Visualize AutoScroll Bounds
			//Rectangle autoScrollArea = DrawBounds;
			//autoScrollArea.Inflate(-autoScrollMargin, -autoScrollMargin);
			//ControlToDiagram(autoScrollArea, out autoScrollArea);
			//currentGraphics.DrawRectangle(Pens.Blue, autoScrollArea);
			//currentGraphics.DrawString(string.Format("AutoScroll Area: {0}", autoScrollArea), Font, Brushes.Blue, autoScrollArea.Location);

			//Point p = PointToClient(Control.MousePosition);
			//if (AutoScrollAreaContainsPoint(p.X, p.Y)) {
			//   ControlToDiagram(p, out p);
			//   GdiHelpers.DrawPoint(currentGraphics, Pens.Red, p.X, p.Y, 3);
			//}
			#endregion

			// Count invalidation
			//++paintCounter;
#endif
			DoResetTransformation();
		}


		private void DrawGrid(ref Rectangle clipRectangle, ref Rectangle diagramBounds) {
			// We have to use floats for calculation or otherwise the rounding error sums up and 
			// causes the grid to wander around when zooming...
			float zoomedGridSpace = _gridSpace * _zoomfactor;

			// Reset smoothing mode to "None" for drawing the grid lines - there's no need for smoothing 
			// horizontal / vertical 1-pixel lines anyway...
			// This improves performance dramatically: ~3 ms vs ~9 ms for drawing an empty diagram sheet.
			SmoothingMode origSmoothingMode = _currentGraphics.SmoothingMode;
			_currentGraphics.SmoothingMode = SmoothingMode.None;

			// Adjust grid spacing when zooming
			float magnificationFactor = ((10 * _gridSpace) / (int)Math.Ceiling(zoomedGridSpace * 10));
			if (magnificationFactor >= 2) {
				zoomedGridSpace = (_gridSpace * magnificationFactor) * _zoomfactor;
			} else if (zoomedGridSpace > _gridSpace * 2)
				zoomedGridSpace = (_gridSpace / 2f) * _zoomfactor;

			if (zoomedGridSpace > 1) {
				// Calculate grid start/end positions for the given diagram rectangle
				float startX = diagramBounds.Left - (DiagramBounds.X * _zoomfactor);
				float startY = diagramBounds.Top - (DiagramBounds.Y * _zoomfactor);
				float endX = diagramBounds.Right;
				float endY = diagramBounds.Bottom;

				// Line grid
				// (Use integer based drawing functions because they are faster)
				// Draw vertical lines
				int top = Math.Max(clipRectangle.Top, diagramBounds.Top - (int)Math.Round(DiagramBounds.Y * _zoomfactor));
				int bottom = Math.Min(clipRectangle.Bottom, diagramBounds.Bottom);
				int clipLeft = clipRectangle.Left; int clipRight = clipRectangle.Right;
				for (float i = startX; i <= endX; i += zoomedGridSpace) {
					if (i < clipLeft || i > clipRight) continue;
					int p = (int)Math.Round(i);
					_currentGraphics.DrawLine(GridPen, p, top, p, bottom);
				}
				// Draw horizontal lines
				int left = Math.Max(clipRectangle.Left, diagramBounds.Left - (int)Math.Round(DiagramBounds.X * _zoomfactor));
				int right = Math.Min(clipRectangle.Right, diagramBounds.Right);
				int clipTop = clipRectangle.Top; int clipBottom = clipRectangle.Bottom;
				for (float i = startY; i <= endY; i += zoomedGridSpace) {
					if (i < clipTop || i > clipBottom) continue;
					int p = (int)Math.Round(i);
					_currentGraphics.DrawLine(GridPen, left, p, right, p);
				}

				// Cross grid (very slow!)
				// ToDo: Use a pen with a dash pattern
				//for (int x = startX; x <= endX; x += zoomedGridSpace) {
				//   for (int y = startY; y <= endY; mouseY += zoomedGridSpace) {
				//      graphics.DrawLine(gridPen, x - 1, y, x + 1, y);
				//      graphics.DrawLine(gridPen, x, y - 1, x, y + 1);
				//   }
				//}


				// Point grid
				//Rectangle r = Rectangle.Empty;
				//r.X = startX;
				//r.Y = startY;
				//r.Width = endX;
				//r.Height = endY;
				//Size s = Size.Empty;
				//s.Width = zoomedGridSpace;
				//s.Height = zoomedGridSpace;
				//ControlPaint.DrawGrid(graphics, r, s, Color.Transparent);
			}

			// Restore smoothing mode
			_currentGraphics.SmoothingMode = origSmoothingMode;
		}

		#endregion


		#region [Private] Methods: Creating drawing resources

		private void CalcControlPointShape(GraphicsPath path, ControlPointShape pointShape, int halfSize) {
			path.Reset();
			switch (pointShape) {
				case ControlPointShape.Circle:
					path.StartFigure();
					path.AddEllipse(-halfSize, -halfSize, halfSize + halfSize, halfSize + halfSize);
					path.CloseFigure();
					path.FillMode = System.Drawing.Drawing2D.FillMode.Alternate;
					break;
				case ControlPointShape.Diamond:
					path.StartFigure();
					path.AddLine(0, -halfSize, halfSize, 0);
					path.AddLine(halfSize, 0, 0, halfSize);
					path.AddLine(0, halfSize, -halfSize, 0);
					path.AddLine(-halfSize, 0, 0, -halfSize);
					path.CloseFigure();
					path.FillMode = System.Drawing.Drawing2D.FillMode.Alternate;
					break;
				case ControlPointShape.Hexagon:
					float sixthSize = (halfSize + halfSize) / 6f;
					path.StartFigure();
					path.AddLine(-sixthSize, -halfSize, sixthSize, -halfSize);
					path.AddLine(sixthSize, -halfSize, halfSize, 0);
					path.AddLine(halfSize, 0, sixthSize, halfSize);
					path.AddLine(sixthSize, halfSize, -sixthSize, halfSize);
					path.AddLine(-sixthSize, halfSize, -halfSize, 0);
					path.AddLine(-halfSize, 0, -sixthSize, -halfSize);
					path.CloseFigure();
					path.FillMode = System.Drawing.Drawing2D.FillMode.Alternate;
					break;
				case ControlPointShape.RotateArrow:
					PointF p = Geometry.IntersectCircleWithLine(0f, 0f, halfSize, 0, 0, -halfSize, -halfSize, true);
					Debug.Assert(Geometry.IsValid(p));
					float quaterSize = halfSize / 2f;
					_rectBuffer.X = _rectBuffer.Y = -halfSize;
					_rectBuffer.Width = _rectBuffer.Height = halfSize + halfSize;

					path.StartFigure();
					// arrow line
					path.AddArc(_rectBuffer, -90, 315);
					path.AddLine(p.X, p.Y, -halfSize, -halfSize);
					path.AddLine(-halfSize, -halfSize, 0, -halfSize);
					path.CloseFigure();

					// closed arrow tip
					//path.StartFigure();
					//path.AddLine(0, -halfSize, 0, 0);
					//path.AddLine(0, 0, p.Value.X + 1, p.Value.Y + 1);
					//path.AddLine(p.Value.X + 1, p.Value.Y + 1, 0, 0);
					//path.AddLine(0, 0, 0, -halfSize);
					//path.CloseFigure();

					// open arrow tip
					path.StartFigure();
					path.AddLine(0, -halfSize, 0, 0);
					path.AddLine(0, 0, -quaterSize, -quaterSize);
					path.AddLine(-quaterSize, -quaterSize, 0, 0);
					path.AddLine(0, 0, 0, -halfSize);
					path.CloseFigure();

					path.CloseAllFigures();
					path.FillMode = System.Drawing.Drawing2D.FillMode.Winding;
					break;
				case ControlPointShape.Square:
					_rectBuffer.X = _rectBuffer.Y = -halfSize;
					_rectBuffer.Width = _rectBuffer.Height = halfSize + halfSize;
					path.StartFigure();
					path.AddRectangle(_rectBuffer);
					path.CloseFigure();
					path.FillMode = System.Drawing.Drawing2D.FillMode.Alternate;
					break;
				default: throw new NShapeUnsupportedValueException(typeof(ControlPointShape), pointShape);
			}
		}


		private void CreatePen(Color color, ref Pen pen) {
			CreatePen(color, 255, 1, null, true, ref pen);
		}


		private void CreatePen(Color color, byte alpha, ref Pen pen) {
			CreatePen(color, alpha, 1, null, true, ref pen);
		}


		private void CreatePen(Color color, float lineWidth, ref Pen pen) {
			CreatePen(color, 255, lineWidth, null, true, ref pen);
		}


		private void CreatePen(Color color, byte alpha, float lineWidth, ref Pen pen) {
			CreatePen(color, 255, lineWidth, null, true, ref pen);
		}


		private void CreatePen(Color color, byte alpha, float lineWidth, float[] dashPattern, bool highQuality, ref Pen pen) {
			DisposeObject(ref pen);
			pen = new Pen(alpha != 255 ? Color.FromArgb(alpha, color) : color, lineWidth);
			if (dashPattern != null) {
				pen.DashPattern = dashPattern;
				pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Custom;
			} else
				pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
			pen.LineJoin = highQuality ? LineJoin.Round : LineJoin.Miter;
			pen.StartCap = highQuality ? LineCap.Round : LineCap.Flat;
			pen.EndCap = highQuality ? LineCap.Round : LineCap.Flat;
		}


		private void CreateBrush(Color color, ref Brush brush) {
			CreateBrush(color, 255, ref brush);
		}


		private void CreateBrush(Color color, byte alpha, ref Brush brush) {
			DisposeObject(ref brush);
			brush = new SolidBrush(Color.FromArgb(alpha, color));
		}


		private void CreateBrush(Color gradientStartColor, Color gradientEndColor, Rectangle brushBounds, float gradientAngle, ref Brush brush) {
			DisposeObject(ref brush);
			brush = new LinearGradientBrush(brushBounds, gradientStartColor, gradientEndColor, gradientAngle, true);
		}

		#endregion


		#region [Private] Methods: Caption editor implementation

		private void DoOpenCaptionEditor(ICaptionedShape shape, int x, int y) {
			if (shape == null) throw new ArgumentNullException("shape");
			_inplaceShape = shape;
			_inplaceCaptionIndex = shape.FindCaptionFromPoint(x, y);
			if (_inplaceCaptionIndex >= 0)
				DoOpenCaptionEditor(shape, _inplaceCaptionIndex, string.Empty);
		}


		private void DoOpenCaptionEditor(ICaptionedShape captionedShape, int captionIndex, string newText) {
			if (captionedShape == null) throw new ArgumentNullException("captionedShape");
			if (captionIndex < 0) throw new ArgumentOutOfRangeException("labelIndex");
			_inplaceShape = captionedShape;
			_inplaceCaptionIndex = captionIndex;

			// Store (and hide) caption's current text
			string currentText = captionedShape.GetCaptionText(_inplaceCaptionIndex);
			captionedShape.HideCaptionText(_inplaceCaptionIndex);

			// Create and show inplace text editor
			_inplaceTextbox = new InPlaceTextBox(this, _inplaceShape, _inplaceCaptionIndex, currentText, newText);
			_inplaceTextbox.KeyDown += inPlaceTextBox_KeyDown;
			_inplaceTextbox.Leave += inPlaceTextBox_Leave;
			_inplaceTextbox.ShortcutsEnabled = true;

			// Show caption editor
			this.Controls.Add(_inplaceTextbox);
			_inplaceTextbox.Focus();
			_inplaceTextbox.Invalidate();
		}


		/// <summary>
		/// Closes the caption editor.
		/// </summary>
		private void DoCloseCaptionEditor(bool saveChanges) {
			if (_inplaceTextbox != null) {
				// Hide text editor
				_inplaceTextbox.Hide();

				// End editing
				if (saveChanges) {
					if (_inplaceTextbox.Text != _inplaceTextbox.OriginalText) {
						ICommand cmd = new SetCaptionTextCommand(_inplaceShape, _inplaceCaptionIndex, _inplaceTextbox.OriginalText, _inplaceTextbox.Text);
						Project.ExecuteCommand(cmd);
					} else 
						_inplaceShape.SetCaptionText(_inplaceCaptionIndex, _inplaceTextbox.Text);
				} else 
					_inplaceShape.SetCaptionText(_inplaceCaptionIndex, _inplaceTextbox.OriginalText);
				// Show hidden caption text
				_inplaceShape.ShowCaptionText(_inplaceCaptionIndex);

				// Clean up
				_inplaceTextbox.KeyDown -= inPlaceTextBox_KeyDown;
				_inplaceTextbox.Leave -= inPlaceTextBox_Leave;
				DisposeObject(ref _inplaceTextbox);
				_inplaceShape = null;
				_inplaceCaptionIndex = -1;
			}
		}

		#endregion


		#region [Private] Methods: Shape selection implementation

		/// <summary>
		/// Invalidate all selected Shapes and their ControlPoints before clearing the selection
		/// </summary>
		private void ClearSelection() {
			DoSuspendUpdate();
			foreach (Shape shape in _selectedShapes)
				DoInvalidateShape(shape);
			_selectedShapes.Clear();

			foreach (KeyValuePair<Shape, ShapeConnectionInfo> pair in _shapesConnectedToSelectedShapes) {
				pair.Key.Invalidate();
				DoInvalidateGrips(pair.Key, ControlPointCapabilities.Connect);
			}
			_shapesConnectedToSelectedShapes.Clear();
			DoResumeUpdate();
		}


		private void UnselectShapesOfInvisibleLayers() {
			DoSuspendUpdate();
			bool shapesRemoved = false;
			foreach (Shape selectedShape in _selectedShapes.TopDown) {
				if (selectedShape.SupplementalLayers != LayerIds.None && !IsLayerVisible(selectedShape.HomeLayer, selectedShape.SupplementalLayers)) {
					DoUnselectShape(selectedShape);
					shapesRemoved = true;
				}
				if (_hiddenLayerIds.Contains(selectedShape.HomeLayer)) {
					DoUnselectShape(selectedShape);
					shapesRemoved = true;
				}
			}
			DoResumeUpdate();
			if (shapesRemoved) PerformSelectionNotifications();
		}


		/// <summary>
		/// Removes the shape from the list of selected shapes and invalidates the shape and its ControlPoints
		/// </summary>
		/// <param name="shape"></param>
		private void DoUnselectShape(Shape shape) {
			if (shape.Parent != null) {
				foreach (Shape s in shape.Parent.Children)
					RemoveShapeFromSelection(s);
			} else RemoveShapeFromSelection(shape);
		}


		private void RemoveShapeFromSelection(Shape shape) {
			DoSuspendUpdate();
			_selectedShapes.Remove(shape);
			foreach (ShapeConnectionInfo ci in shape.GetConnectionInfos(ControlPointId.Any, null))
				_shapesConnectedToSelectedShapes.Remove(ci.OtherShape);
			DoInvalidateShape(shape);
			DoResumeUpdate();
		}


		/// <summary>
		/// Checks if the shape has a parent and handles ShapeAggregation selection in case.
		/// </summary>
		/// <param name="shape">The shape that has to be selected</param>
		/// <param name="addToSelection">Specifies whether the given shape is added to the current selection or the currenty selected shapes will be unseleted.</param>
		private void DoSelectShape(Shape shape, bool addToSelection) {
			if (shape == null) throw new ArgumentNullException("shape");
			Debug.Assert(Diagram != null && (Diagram.Shapes.Contains(shape) || shape.Parent != null));
			if (!IsLayerVisible(shape.HomeLayer, shape.SupplementalLayers))
				return;
			// Check if the selected shape is a child shape. 
			// Sub-selection of a CompositeShapes' children is not allowed
			if (shape.Parent != null) {
				if (!(shape.Parent is IShapeGroup))
					DoSelectShape(shape.Parent, addToSelection);
				else {
					if (!addToSelection
						&& _selectedShapes.Count == 1
						&& (_selectedShapes.Contains(shape.Parent) || _selectedShapes.TopMost.Parent == shape.Parent)) {
						ClearSelection();
						AddShapeToSelection(shape);
					} else {
						if (!addToSelection)
							ClearSelection();
						if (!_selectedShapes.Contains(shape.Parent))
							AddShapeToSelection(shape.Parent);
					}
				}
			} else {
				// standard selection
				if (!addToSelection)
					ClearSelection();
				AddShapeToSelection(shape);
			}
		}


		private void SelectShapeAggregation(ShapeAggregation aggregation, bool addToSelection) {
			_shapeBuffer.Clear();
			_shapeBuffer.AddRange(aggregation);
			bool allSelected = true;
			int cnt = _shapeBuffer.Count;
			foreach (Shape s in _shapeBuffer) {
				if (!_selectedShapes.Contains(s)) {
					allSelected = false;
					break;
				}
			}
			DoSuspendUpdate();
			// if all Shapes of the aggregation are selected, select the shape itself
			if (allSelected) {
				// If the shape should be added to the selection, remove the aggregation's shapes
				// from the selection and add the selected shape
				if (addToSelection) {
					foreach (Shape s in aggregation)
						RemoveShapeFromSelection((Shape)s);
				} else
					ClearSelection();
				AddShapesToSelection(aggregation);
			} else {
				if (!addToSelection)
					ClearSelection();
				AddShapesToSelection(aggregation);
			}
			DoResumeUpdate();
		}


		/// <summary>
		/// Adds the shapes to the list of selected shapes and invalidates the shapes and their controlPoints
		/// </summary>
		private void AddShapesToSelection(IEnumerable<Shape> shapes) {
			foreach (Shape shape in shapes)
				AddShapeToSelection(shape);
		}


		/// <summary>
		/// Adds the shapes to the list of selected shapes and invalidates the shape and its controlPoints
		/// </summary>
		private void AddShapeToSelection(Shape shape) {
			// When selecting with frame, it can easily happen that shapes are inside 
			// the selection frame that are already selected... so skip them here
			if (!_selectedShapes.Contains(shape)) {
				Debug.Assert(Diagram != null && (Diagram.Shapes.Contains(shape) || shape.Parent != null));
				_selectedShapes.Add(shape);
				foreach (ShapeConnectionInfo ci in shape.GetConnectionInfos(ControlPointId.Any, null)) {
					//if (selectedShapes.Contains(ci.OtherShape)) continue;
					if (ci.OtherPointId != ControlPointId.Reference) continue;
					if (_shapesConnectedToSelectedShapes.ContainsKey(ci.OtherShape))
						continue;
					_shapesConnectedToSelectedShapes.Add(ci.OtherShape, ShapeConnectionInfo.Create(ci.OtherPointId, shape, ci.OwnPointId));
				}
				DoInvalidateShape(shape);
			}
		}


		/// <summary>
		/// Enables/disables all ownerDisplay context menu items that are not suitable depending on the selected shapes. 
		/// Raises the ShapesSelected event.
		/// </summary>
		private void PerformSelectionNotifications() {
			if (_propertyController != null) {
				if (_selectedShapes.Count > 0) {
					_propertyController.SetObjects(1, GetModelObjects(_selectedShapes));
					_propertyController.SetObjects(0, _selectedShapes);
				} else {
					// Usage of lastMousePos is ok here because it will only be updated in OnMouseMove
					// while selection always is a result of OnMouseDown or OnMouseUp.
					ShowDiagramProperties(_lastMousePos);
				}
			}
			OnShapesSelected(EventArgs.Empty);
		}


		private IEnumerable<IModelObject> GetModelObjects(IEnumerable<Shape> shapes) {
			foreach (Shape shape in shapes) {
				if (shape.ModelObject == null) continue;
				else yield return shape.ModelObject;
			}
		}

		#endregion


		#region [Private] Methods: Converting between Pixels and mm

		private SizeF PixelsToMm(Size size) {
			return PixelsToMm(size.Width, size.Height);
		}


		private SizeF PixelsToMm(float width, float height) {
			SizeF result = SizeF.Empty;
			result.Width = width / (float)(_infoGraphics.DpiX * mmToInchFactor);
			result.Height = height / (float)(_infoGraphics.DpiY * mmToInchFactor);
			return result;
		}


		private float PixelsToMm(int length) {
			int dpi = (int)Math.Round((_infoGraphics.DpiX + _infoGraphics.DpiY) / 2);
			return length / (float)(_infoGraphics.DpiX * mmToInchFactor);
		}


		private Size MmToPixels(SizeF size) {
			return MmToPixels(size.Width, size.Height);
		}


		private Size MmToPixels(float width, float height) {
			Size result = Size.Empty;
			result.Width = (int)Math.Round((_infoGraphics.DpiX * mmToInchFactor) * width);
			result.Height = (int)Math.Round((_infoGraphics.DpiY * mmToInchFactor) * height);
			return result;
		}


		private int MmToPixels(float length) {
			int dpi = (int)Math.Round((_infoGraphics.DpiX + _infoGraphics.DpiY) / 2);
			return (int)Math.Round((_infoGraphics.DpiX * mmToInchFactor) * length);
		}

		#endregion


		#region [Private] Methods: Scrolling implementation

		private bool ScrollBarContainsPoint(Point p) {
			return ScrollBarContainsPoint(p.X, p.Y);
		}


		private bool ScrollBarContainsPoint(int x, int y) {
			if (HScrollBarVisible && Geometry.RectangleContainsPoint(hScrollBarPanel.Bounds, x, y))
				return true;
			if (VScrollBarVisible && Geometry.RectangleContainsPoint(scrollBarV.Bounds, x, y))
				return true;
			return false;
		}


		private void ResetBounds() {
			_drawBounds =
			_scrollAreaBounds = Geometry.InvalidRectangle;
		}


		private void ScrollBy(int deltaX, int deltaY) {
			if (deltaX != 0 || deltaY != 0) ScrollTo(_scrollPosX + deltaX, _scrollPosY + deltaY);
		}


		private void ScrollBy(ScrollOrientation orientation, int delta) {
			if (orientation == ScrollOrientation.HorizontalScroll)
				ScrollTo(_scrollPosX + delta, _scrollPosY);
			else ScrollTo(_scrollPosX, _scrollPosY + delta);
		}


		private void ScrollTo(ScrollOrientation orientation, int value) {
			if (orientation == ScrollOrientation.HorizontalScroll)
				ScrollTo(value, _scrollPosY);
			else ScrollTo(_scrollPosX, value);
		}


		private void ScrollTo(int x, int y) {
			if (HScrollBarVisible) {
				if (x < scrollBarH.Minimum)
					x = scrollBarH.Minimum;
				else if (x > scrollBarH.Maximum - scrollBarH.LargeChange)
					x = scrollBarH.Maximum - scrollBarH.LargeChange;
			} else x = 0;

			if (VScrollBarVisible) {
				if (y < scrollBarV.Minimum)
					y = scrollBarV.Minimum;
				else if (y > scrollBarV.Maximum - scrollBarV.LargeChange)
					y = scrollBarV.Maximum - scrollBarV.LargeChange;
			} else y = 0;

			if (x != _scrollPosX || y != _scrollPosY) {
				// ToDo: Scroll InPlaceTextBox with along the rest of the display content
				DoCloseCaptionEditor(true);

				SetScrollPosX(x);
				SetScrollPosY(y);

				Invalidate();
			}
		}


		private void SetScrollPosX(int newValue) {
			if (HScrollBarVisible) {
				if (newValue < scrollBarH.Minimum) newValue = scrollBarH.Minimum;
				else if (newValue > scrollBarH.Maximum) newValue = scrollBarH.Maximum;
			} else {
				if (scrollBarH.Minimum != 0) scrollBarH.Minimum = 0;
				if (scrollBarH.Maximum != ScrollAreaBounds.Width) scrollBarH.Maximum = ScrollAreaBounds.Width;
			}
			ScrollEventArgs e = new ScrollEventArgs(ScrollEventType.ThumbPosition, scrollBarH.Value, newValue, ScrollOrientation.HorizontalScroll);

			scrollBarH.Value = newValue;
			_scrollPosX = newValue;

			base.OnScroll(e);
		}


		private void SetScrollPosY(int newValue) {
			if (VScrollBarVisible) {
				if (newValue < scrollBarV.Minimum) newValue = scrollBarV.Minimum;
				else if (newValue > scrollBarV.Maximum) newValue = scrollBarV.Maximum;
			} else {
				if (scrollBarV.Minimum != 0) scrollBarV.Minimum = 0;
				if (scrollBarV.Maximum != ScrollAreaBounds.Height) scrollBarV.Maximum = ScrollAreaBounds.Height;
			}
			ScrollEventArgs e = new ScrollEventArgs(ScrollEventType.ThumbPosition, scrollBarV.Value, newValue, ScrollOrientation.VerticalScroll);

			scrollBarV.Value = newValue;
			_scrollPosY = newValue;

			base.OnScroll(e);
		}

		#endregion


		#region [Private] Methods: UniversalScroll implementation

		/// <summary>
		/// Returns truw if the given point (control coordinates) is inside the auto scroll area
		/// </summary>
		private bool DrawBoundsContainsPoint(int x, int y, int margin) {
			return (x <= DrawBounds.Left + margin
				|| y <= DrawBounds.Top + margin
				|| x > DrawBounds.Width - margin
				|| y > DrawBounds.Height - margin);
		}


		private void StartUniversalScroll(Point startPos) {
			_universalScrollEnabled = true;
			_universalScrollStartPos = startPos;
			_universalScrollFixPointBounds = Rectangle.Empty;
			_universalScrollFixPointBounds.Size = _universalScrollCursor.Size;
			_universalScrollFixPointBounds.Offset(
			_universalScrollStartPos.X - (_universalScrollFixPointBounds.Width / 2),
			_universalScrollStartPos.Y - (_universalScrollFixPointBounds.Height / 2));
			Invalidate(_universalScrollFixPointBounds);
		}


		private void PerformUniversalScroll(Point currentPos) {
			if (_universalScrollEnabled) {
				Cursor = GetUniversalScrollCursor(currentPos);
				if (!Geometry.RectangleContainsPoint(_universalScrollFixPointBounds, currentPos)) {
					Invalidate(_universalScrollFixPointBounds);
					int slowDownFactor = 8; //(int)Math.Round((100f / autoScrollTimer.Interval) * 2);
					int minimumX = _universalScrollCursor.Size.Width / 2;
					int minimumY = _universalScrollCursor.Size.Height / 2;
					// Calculate distance to move 
					int deltaX = (currentPos.X - _universalScrollStartPos.X);
					deltaX = (Math.Abs(deltaX) < minimumX) ? 0 : (int)Math.Round(deltaX / (_zoomfactor * slowDownFactor));
					int deltaY = (currentPos.Y - _universalScrollStartPos.Y);
					deltaY = (Math.Abs(deltaY) < minimumY) ? 0 : (int)Math.Round(deltaY / (_zoomfactor * slowDownFactor));
					// Perform scrolling
					ScrollBy(deltaX, deltaY);
					if (!_autoScrollTimer.Enabled) _autoScrollTimer.Enabled = true;
				} else _autoScrollTimer.Enabled = false;
			}
		}


		private Cursor GetUniversalScrollCursor(Point currentPos) {
			Cursor result;
			if (Geometry.RectangleContainsPoint(_universalScrollFixPointBounds, currentPos))
				result = Cursors.NoMove2D;
			else {
				float angle = (360 + Geometry.RadiansToDegrees(Geometry.Angle(_universalScrollStartPos, currentPos))) % 360;
				if ((angle > 337.5f && angle <= 360) || (angle >= 0 && angle <= 22.5f))
					result = Cursors.PanEast;
				else if (angle > 22.5f && angle <= 67.5f)
					result = Cursors.PanSE;
				else if (angle > 67.5f && angle <= 112.5f)
					result = Cursors.PanSouth;
				else if (angle > 112.5f && angle <= 157.5f)
					result = Cursors.PanSW;
				else if (angle > 157.5f && angle <= 202.5f)
					result = Cursors.PanWest;
				else if (angle > 202.5f && angle <= 247.5f)
					result = Cursors.PanNW;
				else if (angle > 247.5f && angle <= 292.5f)
					result = Cursors.PanNorth;
				else if (angle > 292.5f && angle <= 337.5f)
					result = Cursors.PanNE;
				else result = Cursors.NoMove2D;
			}
			return result;
		}


		private void EndUniversalScroll() {
			Invalidate(_universalScrollFixPointBounds);
			_autoScrollTimer.Enabled = false;
			_universalScrollEnabled = false;
			_universalScrollStartPos = Geometry.InvalidPoint;
			_universalScrollFixPointBounds = Geometry.InvalidRectangle;
			Cursor = Cursors.Default;
		}

		#endregion


		#region [Private] Methods

		private void DisplayDiagram() {
			if (Diagram != null) {
				Diagram.DisplayService = this;
				if (Diagram is Diagram)
					Diagram.HighQualityRendering = HighQualityRendering;
			}
			UpdateScrollBars();
			Invalidate();
		}


		private void ShowDiagramProperties(Point mousePosition) {
			if (_propertyController != null) {
				Point mousePos = Point.Empty;
				ControlToDiagram(mousePosition, out mousePos);
				object objectToSelect = null;
				if (Geometry.RectangleContainsPoint(DiagramBounds, mousePos))
					objectToSelect = Diagram;
				_propertyController.SetObject(1, null);
				_propertyController.SetObject(0, objectToSelect);
			}
		}


		private bool SelectingChangedShapes {
			get { return _collectingChangesCounter > 0; }
		}


		/// <summary>
		/// Starts collecting shapes that were changed by executing a DiagramController action.
		/// These shapes will be selected after the action was executed.
		/// </summary>
		private void StartSelectingChangedShapes() {
			Debug.Assert(_collectingChangesCounter >= 0);
			if (_collectingChangesCounter == 0) {
				SuspendLayout();
				UnselectAll();
			}
			++_collectingChangesCounter;
		}


		/// <summary>
		/// Ends collecting shapes that were changed by executing a DiagramController action.
		/// These shapes will be selected after the action was executed.
		/// </summary>
		private void EndSelectingChangedShapes() {
			Debug.Assert(_collectingChangesCounter > 0);
			--_collectingChangesCounter;
			if (_collectingChangesCounter == 0) {
				ResumeLayout();
				PerformSelectionNotifications();
			}
		}


		private void ProcessClickEvent(MouseEventArgs eventArgs, bool isDoubleClickEvent) {
			// Check if a shape has been clicked
			if (!ScrollBarContainsPoint(eventArgs.Location) && Diagram != null) {
				int mouseX, mouseY;
				ControlToDiagram(eventArgs.X, eventArgs.Y, out mouseX, out mouseY);
				// If a selected shape was clicked, the event will be raised for the selected shape (even if it is behind other shapes)...
				Shape clickedShape = null;
				foreach (Shape s in _selectedShapes.FindShapes(mouseX, mouseY, ControlPointCapabilities.None, 0)) {
					clickedShape = s;
					break;
				}
				// ... otherwise, the event will be raised for the topmost shape containing the clicked coordinates
				if (clickedShape == null) {
					foreach (Shape s in Diagram.Shapes.FindShapes(mouseX, mouseY, ControlPointCapabilities.None, 0)) {
						clickedShape = s;
						break;
					}
				}
				// Raise event (if a clicked shape was found)
				if (clickedShape != null) {
					if (isDoubleClickEvent)
						OnShapeDoubleClick(new DiagramPresenterShapeClickEventArgs(clickedShape, WinFormHelpers.GetMouseEventArgs(this, MouseEventType.MouseUp, eventArgs)));
					else OnShapeClick(new DiagramPresenterShapeClickEventArgs(clickedShape, WinFormHelpers.GetMouseEventArgs(this, MouseEventType.MouseUp, eventArgs)));
				}
			}
		}


		private void DisposeObject<T>(ref T disposableObject) where T : IDisposable {
			if (disposableObject != null) disposableObject.Dispose();
			disposableObject = default(T);
		}


		private Cursor GetDefaultCursor() {
			if (_universalScrollEnabled) return Cursors.NoMove2D;
			else return Cursors.Default;
		}


		private Cursor LoadCursorFromResource(byte[] resource, int cursorId) {
			Cursor result = null;
			if (resource != null) {
				MemoryStream stream = new MemoryStream(resource, 0, resource.Length, false);
				try {
					result = new Cursor(stream);
					result.Tag = cursorId;
				} finally {
					stream.Close();
					stream.Dispose();
				}
			}
			return result;
		}


		private void LoadRegisteredCursor(int cursorId) {
			Debug.Assert(!_registeredCursors.ContainsKey(cursorId));
			Cursor cursor = LoadCursorFromResource(CursorProvider.GetResource(cursorId), cursorId);
			_registeredCursors.Add(cursorId, cursor ?? Cursors.Default);
		}


		/// <summary>
		/// Replaces all the shapes with clones and clears their DisplayServices
		/// </summary>
		private void CreateNewClones(ShapeCollection shapes, bool withModelObjects) {
			CreateNewClones(shapes, withModelObjects, 0, 0);
		}


		/// <summary>
		/// Replaces all the shapes with clones and clears their DisplayServices
		/// </summary>
		private void CreateNewClones(ShapeCollection shapes, bool withModelObjects, int offsetX, int offsetY) {
			// clone from last to first shape in order to maintain the ZOrder
			foreach (Shape shape in shapes.BottomUp) {
				Shape clone = shape.Clone();
				if (withModelObjects) clone.ModelObject = shape.ModelObject.Clone();
				clone.MoveControlPointBy(ControlPointId.Reference, offsetX, offsetY, ResizeModifiers.None);
				shapes.Replace(shape, clone);
			}
		}


		private string GetFailMessage(Exception exc) {
			if (exc != null)
				return string.Format("{0}{1}Stack Trace:{1}{2}", exc.Message, Environment.NewLine, exc.StackTrace);
			else return string.Empty;
		}

		#endregion


		#region [Private] Methods: Creating MenuItemDefs

		private string GetShapeDisplayName(Shape shape) {
			if (shape == null) throw new ArgumentNullException("shape");
			return (shape.Template != null) ? string.Format("'{0}'", shape.Template.Title) : shape.Type.Name;
		}


		private MenuItemDef CreateSelectAllMenuItemDef() {
			bool isFeasible = _selectedShapes.Count != Diagram.Shapes.Count;
			string description = isFeasible ? Properties.Resources.TooltipTxt_SelectAllShapesOfTheDiagram
				: Properties.Resources.TooltipTxt_AllShapesOfTheDiagramAreSelected;

			return new DelegateMenuItemDef(MenuItemNameSelectAll, Properties.Resources.CaptionTxt_SelectAll, null, description, isFeasible, Permission.None,
				(action, project) => SelectAll());
		}


		private MenuItemDef CreateUnselectAllMenuItemDef() {
			bool isFeasible = _selectedShapes.Count > 0;
			string description = isFeasible ? Properties.Resources.TooltipTxt_UnselectAllSelectedShapes : Properties.Resources.TooltipTxt_NoShapesSelected;

			return new DelegateMenuItemDef(MenuItemNameUnselectAll, Properties.Resources.CaptionTxt_UnselectAll, null, description, isFeasible, Permission.None,
				(action, project) => UnselectAll());
		}


		private MenuItemDef CreateSelectByTypeMenuItemDef(Shape shapeUnderCursor) {
			bool isFeasible = shapeUnderCursor != null;
			string title, description;
			if (isFeasible) {
				title = string.Format(Properties.Resources.CaptionFmt_SelectAllShapesOfType0, shapeUnderCursor.Type.Name);
				description = string.Format(Properties.Resources.TooltipFmt_SelectAllShapesOfType0InTheDiagram, shapeUnderCursor.Type.Name);
			} else {
				title = Properties.Resources.CaptionTxt_SelectAllShapesOfAType;
				description = Properties.Resources.TooltipTxt_NoShapeUnderTheMouseCursor;
			}
			return new DelegateMenuItemDef(MenuItemNameSelectByType, title, null, description, isFeasible, Permission.None,
				(action, project) => SelectShapes(shapeUnderCursor.Type, MultiSelect));
		}


		private MenuItemDef CreateSelectByTemplateMenuItemDef(Shape shapeUnderCursor) {
			bool isFeasible;
			string description;
			string title;
			if (shapeUnderCursor == null) {
				isFeasible = false;
				title = Properties.Resources.CaptionTxt_SelectAllShapesBasedOnATemplate;
				description = Properties.Resources.TooltipTxt_NoShapeUnderTheMouseCursor;
			} else if (shapeUnderCursor.Template == null) {
				isFeasible = false;
				title = Properties.Resources.CaptionTxt_SelectAllShapesBasedOnATemplate;
				description = Properties.Resources.TooltipTxt_TheShapeUnderTheCursorIsNotBasedOnAnyTemplate;
			} else {
				isFeasible = true;
				string templateTitle = string.IsNullOrEmpty(shapeUnderCursor.Template.Title) ?
					shapeUnderCursor.Template.Name : shapeUnderCursor.Template.Title;
				title = string.Format(Properties.Resources.CaptionFmt_SelectAllShapesBasedOnTemplate0, templateTitle);
				description = string.Format(Properties.Resources.TooltipFmt_SelectAllShapesOfTheDiagramBasedOnTemplate0, templateTitle);
			}
			return new DelegateMenuItemDef(MenuItemNameSelectByTemplate, title, null, description, isFeasible, Permission.None,
				(action, project) => SelectShapes(shapeUnderCursor.Template, MultiSelect));
		}


		private MenuItemDef CreateShowShapeInfoMenuItemDef(Diagram diagram, IShapeCollection shapes) {
			bool isFeasible = shapes.Count == 1;
			string description;
			if (isFeasible)
				description = string.Format(Properties.Resources.TooltipFmt_ShowInformationFor01, shapes.TopMost.Type.Name, shapes.TopMost.Template);
			else description = shapes.Count > 0 ? Properties.Resources.TooltipFmt_MoreThan1ShapeSelected : Properties.Resources.TooltipTxt_NoShapesSelected;

			return new DelegateMenuItemDef(MenuItemNameShowShapeInfo, Properties.Resources.CaptionTxt_ShapeInfo, 
				Properties.Resources.Information, description, isFeasible, Permission.None, shapes, 
				(a, p) => {
					using (ShapeInfoDialog dlg = new ShapeInfoDialog(Project, shapes.TopMost))
						dlg.ShowDialog();
				}
			);
		}


		private MenuItemDef CreateBringToFrontMenuItemDef(Diagram diagram, IShapeCollection shapes) {
			bool isFeasible = _diagramSetController.CanLiftShapes(diagram, shapes);
			string description;
			if (isFeasible) {
				if (shapes.Count == 1) 
					description = string.Format(Properties.Resources.TooltipFmt_Bring0ToFront, GetShapeDisplayName(shapes.TopMost));
				else description = string.Format(Properties.Resources.TooltipFmt_Bring0ShapesToFront, shapes.Count);
			} else description = Properties.Resources.TooltipTxt_NoShapesSelected;

			return new DelegateMenuItemDef(MenuItemNameBringToFront, Properties.Resources.CaptionTxt_BringToFront, 
				Properties.Resources.ToForeground, description, isFeasible, Permission.Layout, shapes, 
				(a, p) => _diagramSetController.LiftShapes(diagram, shapes, ZOrderDestination.ToTop));
		}


		private MenuItemDef CreateSendToBackMenuItemDef(Diagram diagram, IShapeCollection shapes) {
			bool isFeasible = _diagramSetController.CanLiftShapes(diagram, shapes);
			string description;
			if (isFeasible) {
				if (shapes.Count == 1)
					description = string.Format(Properties.Resources.TooltipFmt_Send0ToBackground, GetShapeDisplayName(shapes.TopMost));
				else description = string.Format(Properties.Resources.TooltipFmt_Send0ShapesToBackground, shapes.Count);
			} else description = Properties.Resources.TooltipTxt_NoShapesSelected;

			return new DelegateMenuItemDef(MenuItemNameSendToBack,
				Properties.Resources.CaptionTxt_SendToBack, Properties.Resources.ToBackground,
				description, isFeasible, Permission.Layout, shapes,
				(a, p) => _diagramSetController.LiftShapes(diagram, shapes, ZOrderDestination.ToBottom));
		}


		private MenuItemDef CreateAddShapesToLayersMenuItemDef(Diagram diagram, IShapeCollection shapes, int homeLayer, LayerIds supplementalLayers, bool replaceLayers) {
			bool isFeasible = (shapes.Count > 0 && homeLayer != Layer.NoLayerId && supplementalLayers != LayerIds.None);
			string description;
			if (isFeasible) {
				// Build layer description text
				int layerCnt = 0;
				string layerNames = string.Empty;
				foreach (Layer layer in EnumerationHelper.Enumerate(Diagram.Layers[homeLayer], Diagram.Layers.FindLayers(supplementalLayers))) {
					++layerCnt;
					if (layerCnt <= 3) layerNames += string.Format("{0}{1}{2}{1}", (layerCnt > 1) ? ", " : string.Empty, "'", layer.Name);
				}
				if (layerCnt > 3) 
					layerNames = string.Format(Properties.Resources.TooltipFmt_NumberOfLayers, layerCnt);
				else if (layerCnt > 1)
					layerNames = string.Format(Properties.Resources.TooltipFmt_LayersByName, layerNames);
				else 
					layerNames = string.Format(Properties.Resources.TooltipFmt_LayerByName, layerNames);

				// Build menu item description text
				if (shapes.Count == 1) {
					if (replaceLayers)
						description = string.Format(Properties.Resources.TooltipFmt_AssignShape0ToLayer1, GetShapeDisplayName(shapes.TopMost), layerNames);
					else description = string.Format(Properties.Resources.TooltipFmt_AddShape0ToLayer1, GetShapeDisplayName(shapes.TopMost), layerNames);
				} else {
					if (replaceLayers)
						description = string.Format(Properties.Resources.TooltipFmt_Assign0ShapesToLayer1, shapes.Count, layerNames);
					else description = string.Format(Properties.Resources.TooltipFmt_Add0ShapesToLayer1, shapes.Count, layerNames);
				}
			} else {
				if (shapes.Count == 0) description = Properties.Resources.TooltipTxt_NoShapesSelected;
				else description = Properties.Resources.TooltipTxt_NoLayersActive;
			}
			string name = replaceLayers ? MenuItemNameAssignToLayers : MenuItemNameAddToLayers;
			string label = replaceLayers ? Properties.Resources.CaptionTxt_AssignShapesToActiveLayers : Properties.Resources.CaptionTxt_AddShapesToActiveLayers;
			Bitmap image = replaceLayers ? Properties.Resources.AssignToLayer : Properties.Resources.AddToLayer;
			DelegateMenuItemDef.ActionExecuteDelegate execDelegate;
			if (replaceLayers)
				execDelegate = (a, p) => _diagramSetController.AssignShapesToLayers(diagram, shapes, homeLayer, supplementalLayers);
			else
				execDelegate = (a, p) => _diagramSetController.AddShapesToLayers(diagram, shapes, homeLayer, supplementalLayers);

			return new DelegateMenuItemDef(name, label, image, description, isFeasible, Permission.Layout, shapes, execDelegate);
		}


		private MenuItemDef CreateRemoveShapesFromLayersMenuItemDef(Diagram diagram, IShapeCollection shapes) {
			bool isFeasible = false;
			foreach (Shape s in shapes) {
				if (s.SupplementalLayers != LayerIds.None) {
					isFeasible = true;
					break;
				}
			}
			string description;
			if (isFeasible) {
				if (shapes.Count == 1)
					description = string.Format(Properties.Resources.TooltipFmt_RemoveShape0FromAllLayers, GetShapeDisplayName(shapes.TopMost));
				else description = string.Format(Properties.Resources.TooltipFmt_Remove0ShapesFromAllLayers, shapes.Count);
			} else {
				if (shapes.Count <= 0) description = Properties.Resources.TooltipTxt_NoShapesSelected;
				else description = Properties.Resources.TooltipFmt_ShapesNotAssignedToLayers;
			}

			return new DelegateMenuItemDef(MenuItemNameRemoveFromLayers, Properties.Resources.CaptionTxt_RemoveShapesFromAllLayers, 
				Properties.Resources.RemoveFromAllLayers, description, isFeasible, Permission.Layout, shapes, 
				(a, p) => _diagramSetController.RemoveShapesFromLayers(diagram, shapes));
		}


		private MenuItemDef CreateGroupShapesMenuItemDef(Diagram diagram, IShapeCollection shapes, int activeHomeLayer, LayerIds activeLayers) {
			bool isFeasible = _diagramSetController.CanGroupShapes(shapes);
			string description = isFeasible ? string.Format(Properties.Resources.TooltipFmt_Group0Shapes, shapes.Count) : Properties.Resources.TooltipTxt_NotEnoughShapesSelected;

			return new DelegateMenuItemDef(MenuItemNameGroupShapes, Properties.Resources.CaptionTxt_GroupShapes, 
				Properties.Resources.GroupBtn, description, isFeasible, Permission.Insert, shapes,
				(a, p) => PerformGroupShapes(diagram, shapes, activeHomeLayer, activeLayers));
		}


		private MenuItemDef CreateUngroupMenuItemDef(Diagram diagram, IShapeCollection shapes) {
			bool isFeasible = _diagramSetController.CanUngroupShape(diagram, shapes);
			string description;
			if (isFeasible)
				description = string.Format(Properties.Resources.TooltipFmt_Ungroup0Shapes, shapes.TopMost.Children.Count);
			else {
				if (shapes.TopMost is IShapeGroup && shapes.TopMost.Parent is IShapeGroup)
					description = Properties.Resources.TooltipTxt_TheSelectedGroupIsMemberOfAnotherGroup;
				else description = Properties.Resources.TooltipTxt_NoGroupSelected;
			}

			return new DelegateMenuItemDef(MenuItemNameUngroupShapes, Properties.Resources.CaptionTxt_UngroupShapes, 
				Properties.Resources.UngroupBtn, description, isFeasible, Permission.Insert, shapes,
				(a, p) => PerformUngroupShapes(diagram, shapes.TopMost));
		}


		private MenuItemDef CreateAggregateMenuItemDef(Diagram diagram, IShapeCollection shapes, int activeHomeLayer, LayerIds activeLayers, Point position) {
			string reason;
			bool isFeasible = _diagramSetController.CanAggregateShapes(diagram, shapes, out reason);
			string description = isFeasible ? string.Format(Properties.Resources.TooltipFmt_Aggregate0ShapesAsCompositeShape, shapes.Count) : reason;
			// Get host for aggregated child shapes
			Shape compositeShape = shapes.Bottom;
			if (compositeShape is ILinearShape) {
				isFeasible = false;
				description = Properties.Resources.TooltipTxt_LinearShapesMayNotBeTheBaseForACompositeShape;
			}
			return new DelegateMenuItemDef(MenuItemNameAggregateShapes, Properties.Resources.CaptionTxt_AggregateShapes, 
				Properties.Resources.AggregateShapeBtn, description, isFeasible, Permission.Delete, shapes,
				(a, p) => PerformAggregateCompositeShape(diagram, compositeShape, shapes, activeHomeLayer, activeLayers));
		}


		private MenuItemDef CreateUnaggregateMenuItemDef(Diagram diagram, IShapeCollection shapes) {
			string reason = null;
			bool isFeasible = _diagramSetController.CanSplitShapeAggregation(diagram, shapes, out reason);
			string description = isFeasible ? Properties.Resources.TooltipTxt_DisaggregateTheSelectedShapeAggregation : reason;
			if (!isFeasible) {
				if (shapes.Count <= 0)
					description = Properties.Resources.TooltipTxt_NoShapesSelected;
				else if (shapes.Count == 1)
					description = Properties.Resources.TooltipTxt_SelectedShapeIsNotACompositeShape;
				else description = Properties.Resources.TooltipTxt_TooManyShapesSelected;
			}
			return new DelegateMenuItemDef(MenuItemNameDisaggregateShapes, Properties.Resources.CaptionTxt_DisaggregateShapes, 
				Properties.Resources.SplitShapeAggregationBtn, description, isFeasible, Permission.Insert, shapes,
				(a, p) => PerformSplitCompositeShape(diagram, shapes.Bottom));
		}


		private MenuItemDef CreateCutMenuItemDef(Diagram diagram, IShapeCollection shapes, bool modelObjectsAssigned, Point position) {
			string title = Properties.Resources.CaptionTxt_Cut;
			Bitmap icon = Properties.Resources.CutBtn; ;
			Permission permission = Permission.Delete; ;
			string description;
			bool isFeasible = false;
			if (!IsActionGranted(permission, shapes))
				description = string.Format(Properties.Resources.TooltipFmt_Permission0IsNotGranted, permission);
			else if (!_diagramSetController.CanCut(diagram, shapes))
				description = Properties.Resources.TooltipTxt_NoShapesSelected;
			else {
				isFeasible = true;
				description = (shapes.Count == 1) ? string.Format(Properties.Resources.TooltipFmt_CutShape0, GetShapeDisplayName(shapes.TopMost)) 
					: string.Format(Properties.Resources.TooltipFmt_Cut0Shapes, shapes.Count);
			}

			if (!modelObjectsAssigned)
				return new DelegateMenuItemDef(MenuItemNameCut, title, icon, description, isFeasible, permission, shapes,
					(a, p) => PerformCut(diagram, shapes, false, position));
			else
				return new GroupMenuItemDef(MenuItemNameCutGroupItem, title, icon, description, isFeasible,
					new MenuItemDef[] {
						// Cut shapes only
						new DelegateMenuItemDef(MenuItemNameCutShapesOnly, title, icon, description, isFeasible, permission, shapes, 
							(a, p) => PerformCut(diagram, shapes, false, position)),
						// Cut shapes with models
						new DelegateMenuItemDef(MenuItemNameCutWithModels, string.Format("{0} {1}", title, Properties.Resources.TooltipTxt_WithModelsPostfix), 
							icon, string.Format("{0} {1}", description, Properties.Resources.TooltipTxt_WithModelsPostfix), isFeasible, permission, shapes, 
							(a, p) => PerformCut(diagram, shapes, true, position))
					}, 1);
		}


		private MenuItemDef CreateCopyImageMenuItemDef(Diagram diagram, IShapeCollection selectedShapes) {
			string title = Properties.Resources.CaptionTxt_CopyAsImage;
			Bitmap icon = Properties.Resources.CopyAsImage;
			Permission permission = Permission.None;
			bool isFeasible = true;
			string description = (selectedShapes.Count > 0) ? Properties.Resources.TooltipTxt_CopySelectionAsPNGAndEMFImageToClipboard
				: Properties.Resources.TooltipTxt_CopyDiagramAsPNGAndEMFImageToClipboard;

			return new DelegateMenuItemDef(MenuItemNameCopyAsImage, title, icon, description, isFeasible, permission,
				(a, p) => {
					IEnumerable<Shape> shapes = (selectedShapes.Count > 0) ? selectedShapes : null;
					IEnumerable<int> visibleLayers = GetVisibleLayerIds();
					int margin = (selectedShapes.Count > 0) ? 10 : 0;
					CopyImagesToClipboard(visibleLayers, true);
					if (!Clipboard.ContainsData(DataFormats.EnhancedMetafile)) { Debug.Print("Clipboard does NOT contain EMF image!"); }
					if (!Clipboard.ContainsData(DataFormats.Bitmap)) { Debug.Print("Clipboard does NOT contain PNG image!"); }
				}
			);
		}


		private MenuItemDef CreateCopyMenuItemDef(Diagram diagram, IShapeCollection shapes, bool modelObjectsAssigned, Point position) {
			string title = Properties.Resources.CaptionTxt_Copy;
			Bitmap icon = Properties.Resources.CopyBtn;
			Permission permission = Permission.None;
			bool isFeasible = _diagramSetController.CanCopy(shapes);
			string description = null;
			if (isFeasible) {
				description = (shapes.Count == 1) ? string.Format(Properties.Resources.TooltipFmt_CopyShape0, GetShapeDisplayName(shapes.TopMost))
					: string.Format(Properties.Resources.TooltipFmt_Copy0Shapes, shapes.Count);
			} else description = Properties.Resources.TooltipTxt_NoShapesSelected;

			if (!modelObjectsAssigned)
				return new DelegateMenuItemDef(MenuItemNameCopy, title, icon, description, isFeasible, permission, shapes,
					(a, p) => PerformCopy(diagram, shapes, false, position));
			else
				return new GroupMenuItemDef(MenuItemNameCopyGroupItem, title, icon, description, isFeasible,
					new MenuItemDef[] {
						// Cut shapes only
						new DelegateMenuItemDef(MenuItemNameCopyShapesOnly, title, icon, description, isFeasible, permission, shapes, 
							(a, p) => PerformCopy(diagram, shapes, false, position)),
						// Cut shapes with models
						new DelegateMenuItemDef(MenuItemNameCopyWithModels, string.Format("{0} {1}", title, Properties.Resources.TooltipTxt_WithModelsPostfix), 
							icon,  string.Format("{0} {1}", description, Properties.Resources.TooltipTxt_WithModelsPostfix), isFeasible, permission, shapes, 
							(a, p) => PerformCopy(diagram, shapes, true, position)) 
					}, 1);
		}


		private MenuItemDef CreatePasteMenuItemDef(Diagram diagram, IShapeCollection shapes, Point position) {
			//string description;
			//bool isFeasible = diagramSetController.CanPaste(diagram, out description);
			//if (isFeasible)
			//    description = string.Format("Paste {0} shape{1}", shapes.Count, shapes.Count > 1 ? "s" : "");

			bool isFeasible = _diagramSetController.CanPaste(diagram);
			string description = null;
			if (isFeasible)
				description = (shapes.Count == 1) ? string.Format(Properties.Resources.TooltipFmt_PasteShape0, GetShapeDisplayName(shapes.TopMost))
					: string.Format(Properties.Resources.TooltipFmt_Paste0Shapes, shapes.Count);
			else description = Properties.Resources.TooltipTxt_NoShapesCutCopiedYet;

			return new DelegateMenuItemDef(MenuItemNamePaste, Properties.Resources.CaptionTxt_Paste, Properties.Resources.PasteBtn, description,
				isFeasible, Permission.Insert, diagram.SecurityDomainName, (a, p) => Paste(position));
		}


		private MenuItemDef CreateDeleteMenuItemDef(Diagram diagram, IShapeCollection shapes, bool modelObjectsAssigned) {
			string title = Properties.Resources.CaptionTxt_Delete;
			Bitmap icon = Properties.Resources.DeleteBtn;
			Permission permission = Permission.Delete;
			string description;
			bool isFeasible = false;
			if (!IsActionGranted(permission, shapes))
				description = string.Format(Properties.Resources.TooltipFmt_Permission0IsNotGranted, permission);
			else if (!_diagramSetController.CanDeleteShapes(diagram, shapes))
				description = Properties.Resources.TooltipTxt_NoShapesSelected;
			else {
				isFeasible = true;
				description = (shapes.Count == 1) ? string.Format(Properties.Resources.TooltipFmt_DeleteShape0, GetShapeDisplayName(shapes.TopMost)) 
					: string.Format(Properties.Resources.TooltipFmt_Delete0Shapes, shapes.Count);
			}

			if (!modelObjectsAssigned)
				return new DelegateMenuItemDef(MenuItemNameDelete, title, icon, description, isFeasible, permission, shapes,
					(a, p) => PerformDelete(diagram, shapes, false));
			else {
				bool otherShapesAssignedToModels = OtherShapesAssignedToModels(shapes);
				string deleteWithModelsDesc;
				if (otherShapesAssignedToModels)
					deleteWithModelsDesc = Properties.Resources.TooltipTxt_ThereAreShapesAssignedToTheModelObjects;
				else deleteWithModelsDesc = string.Format("{0} {1}", description, Properties.Resources.TooltipTxt_WithModelsPostfix);

				return new GroupMenuItemDef(MenuItemNameDeleteGroupItem, title, icon, description, isFeasible,
					new MenuItemDef[] {
						// Cut shapes only
						new DelegateMenuItemDef(MenuItemNameDeleteShapesOnly, title, icon, description, isFeasible, permission, shapes, 
							(a, p) => PerformDelete(diagram, shapes, false)),
						new DelegateMenuItemDef(MenuItemNameDeleteWithModels, string.Format("{0} {1}", title, Properties.Resources.TooltipTxt_WithModelsPostfix), 
							icon,  deleteWithModelsDesc, !otherShapesAssignedToModels, permission, shapes, 
							(a, p) => PerformDelete(diagram, shapes, true))
					}, 1);
			}
		}


		private MenuItemDef CreatePropertiesMenuItemDef(Diagram diagram, IShapeCollection shapes, Point position) {
			bool isFeasible = (Diagram != null && PropertyController != null);
			string description;
			object obj = null;
			if (!isFeasible) description = Properties.Resources.TooltipTxt_PropertiesAreNotAvailable;
			else {
				Shape s = shapes.FindShape(position.X, position.Y, ControlPointCapabilities.None, 0, null);
				if (s != null) {
					description = (shapes.Count == 1) ? string.Format(Properties.Resources.TooltipFmt_ShowPropertiesOfShape0, GetShapeDisplayName(s))
						: description = string.Format(Properties.Resources.TooltipFmt_ShowPropertiesOf0Shapes, shapes.Count);
					obj = s;
				} else {
					description = string.Format(Properties.Resources.TooltipFmt_ShowPropertiesOfDiagram0, Diagram.Title);
					obj = diagram;
				}
			}

			return new DelegateMenuItemDef(MenuItemNameShowProperties, Properties.Resources.CaptionTxt_Properties, Properties.Resources.DiagramPropertiesBtn3,
				description, isFeasible, Permission.Data, shapes,
				(a, p) => PropertyController.SetObjects(0, shapes));
		}


		private MenuItemDef CreateUndoMenuItemDef() {
			bool isFeasible = Project.History.UndoCommandCount > 0;
			string description = isFeasible ? Project.History.GetUndoCommandDescription() : Properties.Resources.TooltipTxt_NoUndoCommandsLeft;

			return new DelegateMenuItemDef(MenuItemNameUndo, Properties.Resources.CaptionTxt_Undo, Properties.Resources.UndoBtn, description, isFeasible,
				Permission.None, (a, p) => PerformUndo());
		}


		private MenuItemDef CreateRedoMenuItemDef() {
			bool isFeasible = Project.History.RedoCommandCount > 0;
			string description = isFeasible ? Project.History.GetRedoCommandDescription() : Properties.Resources.TooltipTxt_NoRedoCommandsLeft;

			return new DelegateMenuItemDef(MenuItemNameRedo, Properties.Resources.CaptionTxt_Redo, Properties.Resources.RedoBtn, description, isFeasible,
				Permission.None, (a, p) => PerformRedo());
		}


		private bool IsActionGranted(Permission permission, IEnumerable<Shape> shapes) {
			if (shapes == null) throw new ArgumentNullException("shapes");
			return Project.SecurityManager.IsGranted(permission, shapes);
		}

		#endregion


		#region [Private] Methods: MenuItemDef implementations

		private void PerformGroupShapes(Diagram diagram, IShapeCollection shapes, int activeHomeLayer, LayerIds activeLayers) {
			Debug.Assert(shapes.Count > 0);

			// Buffer the currently selected shapes because the collection will be emptied by calling StartSelectingChangedShapes
			_shapeBuffer.Clear();
			_shapeBuffer.AddRange(shapes);

			_diagramSetController.GroupShapes(diagram, _shapeBuffer, activeHomeLayer, activeLayers);

			OnShapesRemoved(GetShapesEventArgs(shapes));
			OnShapesInserted(GetShapesEventArgs(_shapeBuffer[0].Parent));
			SelectShape(_shapeBuffer[0].Parent, false);
		}


		private void PerformUngroupShapes(Diagram diagram, Shape shape) {
			Debug.Assert(shape.Children.Count > 0);
			_shapeBuffer.Clear();
			_shapeBuffer.AddRange(shape.Children);

			_diagramSetController.UngroupShapes(diagram, shape);

			OnShapesRemoved(GetShapesEventArgs(shape));
			OnShapesInserted(GetShapesEventArgs(_shapeBuffer));
			SelectShapes(_shapeBuffer, false);
		}


		private void PerformAggregateCompositeShape(Diagram diagram, Shape compositeShape, IShapeCollection shapes, int homeLayer, LayerIds supplementalLayers) {
			// Buffer the currently selected shapes because the collection will be emptied by calling StartSelectingChangedShapes
			_shapeBuffer.Clear();
			_shapeBuffer.AddRange(shapes);

			_diagramSetController.AggregateCompositeShape(diagram, compositeShape, _shapeBuffer, homeLayer, supplementalLayers);

			OnShapesRemoved(GetShapesEventArgs(compositeShape.Children));
			SelectShape(compositeShape, false);
		}


		private void PerformSplitCompositeShape(Diagram diagram, Shape shape) {
			_shapeBuffer.Clear();
			_shapeBuffer.AddRange(shape.Children);

			// Set DiagramPresenter in "Listen for repository changes" mode
			_diagramSetController.SplitCompositeShape(diagram, shape);

			OnShapesInserted(GetShapesEventArgs(_shapeBuffer));
			SelectShape(shape);
			SelectShapes(_shapeBuffer, true);
		}


		private void PerformCut(Diagram diagram, IEnumerable<Shape> shapes, bool withModelObjects, Point position) {
			DiagramPresenterShapesEventArgs e = GetShapesEventArgs(shapes);
			UnselectShapes(shapes);

			if (Geometry.IsValid(position))
				_diagramSetController.Cut(diagram, e.Shapes, withModelObjects, position);
			else _diagramSetController.Cut(diagram, e.Shapes, withModelObjects);

			OnShapesRemoved(e);
		}


		private void PerformCopy(Diagram diagram, IEnumerable<Shape> shapes, bool withModelObjects, Point position) {
			if (Geometry.IsValid(position))
				_diagramSetController.Copy(diagram, shapes, withModelObjects, position);
			else _diagramSetController.Copy(diagram, shapes, withModelObjects);
		}


		private void PerformPaste(Diagram diagram, int homeLayer, LayerIds supplementalLayers, Point position) {
			try {
				StartSelectingChangedShapes();
				if (!Geometry.IsValid(position))
					_diagramSetController.Paste(diagram, homeLayer, supplementalLayers, GridSize, GridSize);
				else 
					_diagramSetController.Paste(diagram, homeLayer, supplementalLayers, position);
			} finally {
				OnShapesInserted(GetShapesEventArgs(_selectedShapes));
				EndSelectingChangedShapes();
			}
		}


		private void PerformPaste(Diagram diagram, int homeLayer, LayerIds supplementalLayers, int offsetX, int offsetY) {
			try {
				StartSelectingChangedShapes();
				if (!Geometry.IsValid(offsetX, offsetY))
					_diagramSetController.Paste(diagram, homeLayer, supplementalLayers, GridSize, GridSize);
				else 
					_diagramSetController.Paste(diagram, homeLayer, supplementalLayers, offsetX, offsetY);
			} finally {
				OnShapesInserted(GetShapesEventArgs(_selectedShapes));
				EndSelectingChangedShapes();
			}
		}


		private void PerformDelete(Diagram diagram, IEnumerable<Shape> shapes, bool withModelObjects) {
			DiagramPresenterShapesEventArgs e = GetShapesEventArgs(shapes);
			
			UnselectShapes(e.Shapes);
			_diagramSetController.DeleteShapes(diagram, e.Shapes, withModelObjects);

			OnShapesRemoved(e);
		}


		private void PerformUndo() {
			// Set DiagramPresenter in "Listen for repository changes" mode
			try {
				StartSelectingChangedShapes();
				Project.History.Undo();
			} finally { EndSelectingChangedShapes(); }
		}


		private void PerformRedo() {
			// Set DiagramPresenter in "Listen for repository changes" mode
			try {
				StartSelectingChangedShapes();
				Project.History.Redo();
			} finally { EndSelectingChangedShapes(); }
		}


		private bool ModelObjectsAssigned(IEnumerable<Shape> shapes) {
			foreach (Shape s in shapes)
				if (s.ModelObject != null) return true;
			return false;
		}


		private bool OtherShapesAssignedToModels(IEnumerable<Shape> shapes) {
			foreach (Shape shape in shapes) {
				if (shape.ModelObject != null)
					if (OtherShapeAssignedToModel(shape.ModelObject, shape))
						return true;
			}
			return false;
		}


		private bool OtherShapeAssignedToModel(IModelObject modelObject, Shape shape) {
#if DEBUG_DIAGNOSTICS
			Debug.Assert(modelObject.ShapeCount > 0);
			if (modelObject.ShapeCount == 1) {
				foreach (Shape assignedShape in modelObject.Shapes) 
					Debug.Assert(assignedShape == shape);
			}
#endif
			return modelObject.ShapeCount > 1;
			//foreach (Shape assignedShape in modelObject.Shapes) {
			//   if (assignedShape != shape) return true;
			//   foreach (IModelObject childModelObject in Project.Repository.GetModelObjects(modelObject))
			//      if (OtherShapeAssignedToModel(childModelObject, shape)) return true;
			//}
			//return false;
		}

		#endregion


		#region [Private] Methods: (Un)Registering events

		private void RegisterProjectEvents() {
			if (!_projectIsRegistered) {
				Debug.Assert(Project != null);
				Project.Opened += Project_ProjectOpen;
				Project.Closing += Project_ProjectClosing;
				Project.Closed += Project_ProjectClosed;
				_projectIsRegistered = true;
				if (Project.IsOpen) RegisterRepositoryEvents();
			}
		}


		private void UnregisterProjectEvents() {
			if (_projectIsRegistered) {
				Debug.Assert(Project != null);
				Project.Opened -= Project_ProjectOpen;
				Project.Closing -= Project_ProjectClosing;
				Project.Closed -= Project_ProjectClosed;
				_projectIsRegistered = false;
				if (Project.Repository != null) UnregisterRepositoryEvents();
			}
		}


		private void RegisterRepositoryEvents() {
			if (!_repositoryIsRegistered) {
				Debug.Assert(Project.Repository != null);
				Project.Repository.TemplateShapeReplaced += Repository_TemplateShapeReplaced;
				Project.Repository.DiagramUpdated += Repository_DiagramUpdated;
				Project.Repository.ShapesInserted += Repository_ShapesInserted;
				Project.Repository.ShapesUpdated += Repository_ShapesUpdated;
				Project.Repository.ShapesDeleted += Repository_ShapesDeleted;
				Project.Repository.ConnectionInserted += Repository_ShapeConnectionInsertedOrDeleted;
				Project.Repository.ConnectionDeleted += Repository_ShapeConnectionInsertedOrDeleted;
				// handle this when creating connected lines
				_repositoryIsRegistered = true;
			}
		}


		private void UnregisterRepositoryEvents() {
			if (_repositoryIsRegistered) {
				Debug.Assert(Project.Repository != null);
				Project.Repository.TemplateShapeReplaced -= Repository_TemplateShapeReplaced;
				Project.Repository.DiagramUpdated -= Repository_DiagramUpdated;
				Project.Repository.ShapesInserted -= Repository_ShapesInserted;
				Project.Repository.ShapesUpdated -= Repository_ShapesUpdated;
				Project.Repository.ShapesDeleted -= Repository_ShapesDeleted;
				Project.Repository.ConnectionInserted -= Repository_ShapeConnectionInsertedOrDeleted;
				Project.Repository.ConnectionDeleted -= Repository_ShapeConnectionInsertedOrDeleted;
				_repositoryIsRegistered = false;
			}
		}


		private void RegisterDiagramEvents() {
			Diagram.Resized += diagram_Resized;
			Diagram.ShapeMoved += diagram_ShapeMoved;
			Diagram.ShapeResized += diagram_ShapeResized;
			Diagram.ShapeRotated += diagram_ShapeRotated;
		}


		private void UnregisterDiagramEvents() {
			Diagram.ShapeMoved -= diagram_ShapeMoved;
			Diagram.ShapeResized -= diagram_ShapeResized;
			Diagram.ShapeRotated -= diagram_ShapeRotated;
			Diagram.Resized -= diagram_Resized;
		}


		private void RegisterDiagramControllerEvents() {
			_diagramController.DiagramChanged += Controller_DiagramChanged;
			_diagramController.DiagramChanging += Controller_DiagramChanging;
		}


		private void UnregisterDiagramControllerEvents() {
			_diagramController.DiagramChanged -= Controller_DiagramChanged;
			_diagramController.DiagramChanging -= Controller_DiagramChanging;
		}


		private void RegisterDiagramSetControllerEvents() {
			_diagramSetController.ProjectChanging += diagramSetController_ProjectChanging;
			_diagramSetController.ProjectChanged += diagramSetController_ProjectChanged;
			_diagramSetController.SelectModelObjectsRequested += diagramSetController_SelectModelObjectRequested;
			if (_diagramSetController.Project != null) RegisterProjectEvents();
		}


		private void UnregisterDiagramSetControllerEvents() {
			if (_diagramSetController.Project != null) UnregisterProjectEvents();
			_diagramSetController.SelectModelObjectsRequested -= diagramSetController_SelectModelObjectRequested;
			_diagramSetController.ProjectChanging -= diagramSetController_ProjectChanging;
			_diagramSetController.ProjectChanged -= diagramSetController_ProjectChanged;
		}

		#endregion


		#region [Private] Methods: EventHandler implementations

		private DiagramPresenterShapesEventArgs GetShapesEventArgs(Shape shape) {
			_shapesEventArgs.SetShapes(shape);
			return _shapesEventArgs;
		}


		private DiagramPresenterShapesEventArgs GetShapesEventArgs(IEnumerable<Shape> shapes) {
			_shapesEventArgs.SetShapes(shapes);
			return _shapesEventArgs;
		}


		private DiagramPresenterShapeEventArgs GetShapeEventArgs(Shape shape) {
			_shapeEventArgs.SetShape(shape);
			return _shapeEventArgs;
		}


		private void diagramSetController_SelectModelObjectRequested(object sender, ModelObjectsEventArgs e) {
			if (Diagram != null) {
				UnselectAll();
				foreach (IModelObject modelObject in e.ModelObjects) {
					if (modelObject.ShapeCount == 0) continue;
					foreach (Shape s in modelObject.Shapes)
						if (Diagram.Shapes.Contains(s)) SelectShape(s, true);
				}
				int margin;
				ControlToDiagram(autoScrollMargin, out margin);
				if (_selectedShapes.Count == 1)
					EnsureVisible(_selectedShapes.TopMost, margin);
				else 
					EnsureVisible(_selectedShapes.GetBoundingRectangle(false), margin);
			}
		}


		private void diagramSetController_ProjectChanged(object sender, EventArgs e) {
			if (_diagramSetController != null && _diagramSetController.Project != null)
				RegisterProjectEvents();
		}


		private void diagramSetController_ProjectChanging(object sender, EventArgs e) {
			if (_diagramSetController != null && _diagramSetController.Project != null)
				UnregisterProjectEvents();
		}


		private void diagram_Resized(object sender, EventArgs e) {
			if (_suspendUpdateCounter > 0)
				_boundsChanged = true;
			else {
				// Store current display position
				int origPosX, origPosY;
				ControlToDiagram(DrawBounds.X, DrawBounds.Y, out origPosX, out origPosY);

				ResetBounds();
				UpdateScrollBars();

				int newPosX, newPosY;
				ControlToDiagram(DrawBounds.X, DrawBounds.Y, out newPosX, out newPosY);

				// Scroll to original position
				ScrollBy(-(newPosX - origPosX), -(newPosY - origPosY));
				Invalidate();
			}
		}


		private void diagram_ShapeMoved(object sender, ShapeEventArgs e) {
			if (ShapeMoved != null) ShapeMoved(this, GetShapeEventArgs(e.Shape));
		}


		private void diagram_ShapeResized(object sender, ShapeEventArgs e) {
			if (ShapeResized != null) ShapeResized(this, GetShapeEventArgs(e.Shape));
		}


		private void diagram_ShapeRotated(object sender, ShapeEventArgs e) {
			if (ShapeRotated != null) ShapeRotated(this, GetShapeEventArgs(e.Shape));
		}

		
		private void inPlaceTextBox_Leave(object sender, EventArgs e) {
			DoCloseCaptionEditor(true);
		}


		private void inPlaceTextBox_KeyDown(object sender, KeyEventArgs e) {
			if (e.Modifiers == Keys.None && e.KeyCode == Keys.Escape) {
				DoCloseCaptionEditor(false);
				e.Handled = true;
			}
				//else if (e.Modifiers == Keys.None && e.KeyCode == Keys.Enter) {
			else if (e.KeyCode == Keys.F2) {
				DoCloseCaptionEditor(true);
				e.Handled = true;
			}
		}


		private void Project_ProjectClosing(object sender, EventArgs e) {
			Clear();
		}


		private void Project_ProjectClosed(object sender, EventArgs e) {
			UnregisterRepositoryEvents();
		}


		private void Project_ProjectOpen(object sender, EventArgs e) {
			RegisterRepositoryEvents();
		}


		private void Repository_DiagramUpdated(object sender, RepositoryDiagramEventArgs e) {
			if (Diagram != null && e.Diagram == Diagram) {
				UpdateScrollBars();
				Invalidate();
			}
		}


		private void Repository_ShapesUpdated(object sender, RepositoryShapesEventArgs e) {
			if (Diagram != null && SelectingChangedShapes) {
				DoSuspendUpdate();
				foreach (Shape s in e.Shapes) {
					if (s.Diagram == Diagram)
						SelectShape(s, true);
				}
				DoResumeUpdate();
			}
		}


		private void Repository_ShapesInserted(object sender, RepositoryShapesEventArgs e) {
			if (Diagram != null && SelectingChangedShapes) {
				DoSuspendUpdate();
				foreach (Shape s in e.Shapes) {
					if (s.Diagram == Diagram)
						SelectShape(s, true);
				}
				DoResumeUpdate();
			}
		}


		private void Repository_ShapesDeleted(object sender, RepositoryShapesEventArgs e) {
			UnselectShapes(e.Shapes);
		}


		private void Repository_ShapeConnectionInsertedOrDeleted(object sender, RepositoryShapeConnectionEventArgs e) {
			if (Diagram != null && Diagram.Shapes.Contains(e.ConnectorShape) && Diagram.Shapes.Contains(e.TargetShape)) {
				if (_selectedShapes.Contains(e.ConnectorShape)) {
					e.ConnectorShape.Invalidate();
					e.TargetShape.Invalidate();
				}
			}
		}


		private void Repository_TemplateShapeReplaced(object sender, RepositoryTemplateShapeReplacedEventArgs e) {
			SuspendLayout();
			foreach (Shape selectedShape in _selectedShapes) {
				if (selectedShape.Template == e.Template) {
					Rectangle bounds = selectedShape.GetBoundingRectangle(true);
					foreach (Shape s in Diagram.Shapes.FindShapes(bounds.X, bounds.Y, bounds.Width, bounds.Height, false)) {
						if (s == selectedShape)
							_selectedShapes.Remove(selectedShape);
						else if (s.Template == e.Template)
							_selectedShapes.Replace(selectedShape, s);
					}
				}
			}
			ResumeLayout();
		}


		private void Repository_SavedChanges(object sender, EventArgs e) {
			// Nothing to do here
		}


		private void scrollBar_GotFocus(object sender, EventArgs e) {
			// If the scrollBar gets focussed, Focus the display itself.
			// This is necessary because in WinForms, the CanFocus method can 
			// neither be set nor overwritten.
			//this.Select();
		}


		private void scrollBar_KeyUp(object sender, KeyEventArgs e) {
			OnKeyUp(e);
		}


		private void scrollBar_KeyPress(object sender, KeyPressEventArgs e) {
			OnKeyPress(e);
		}


		private void scrollBar_KeyDown(object sender, KeyEventArgs e) {
			OnKeyDown(e);
		}


		private void ScrollBars_MouseEnter(object sender, EventArgs e) {
			if (Cursor != Cursors.Default) Cursor = Cursors.Default;
		}


		private void ScrollBar_ValueChanged(object sender, EventArgs e) {
			ScrollBar scrollBar = sender as ScrollBar;
			if (scrollBar != null) {
				if (!scrollBar.Visible && scrollBar.Value != 0) {
					//Debug.Fail("Invisible scrollbar was scrolled!");
					Debug.Print("Scrollbar {0}'s  value changed to {1}", scrollBar.Name, scrollBar.Value);
				}
			}
		}


		private void ScrollBar_Scroll(object sender, ScrollEventArgs e) {
			//OnScroll(e);
			switch (e.ScrollOrientation) {
				case ScrollOrientation.HorizontalScroll:
					ScrollTo(e.NewValue, _scrollPosY);
					break;
				case ScrollOrientation.VerticalScroll:
					ScrollTo(_scrollPosX, e.NewValue);
					break;
				default:
					Debug.Fail("Unexpected ScrollOrientation value!");
					break;
			}
		}


		private void ScrollBar_VisibleChanged(object sender, EventArgs e) {
			ScrollBar scrollBar = sender as ScrollBar;
			if (scrollBar != null)
				Invalidate(RectangleToClient(scrollBar.RectangleToScreen(scrollBar.Bounds)));
			ResetBounds();
			//CalcDiagramPosition();
			//// ToDo: Set ScrollBar's Min and Max properties
		}


		private void autoScrollTimer_Tick(object sender, EventArgs e) {
			Point p = PointToClient(Control.MousePosition);
			OnMouseMove(new MouseEventArgs(Control.MouseButtons, 0, p.X, p.Y, 0));
		}


		private void ToolTip_Popup(object sender, PopupEventArgs e) {
			Point p = PointToClient(Control.MousePosition);
			toolTip.ToolTipTitle = string.Empty;
			if (Diagram != null) {
				Shape shape = Diagram.Shapes.FindShape(p.X, p.Y, ControlPointCapabilities.None, _handleRadius, null);
				if (shape != null) {
					if (shape.ModelObject != null)
						toolTip.ToolTipTitle = shape.ModelObject.Name + " (" + shape.ModelObject.GetType() + ")";
				}
			}
		}


		private void Controller_DiagramChanging(object sender, EventArgs e) {
			OnDiagramChanging(e);
		}


		private void Controller_DiagramChanged(object sender, EventArgs e) {
			OnDiagramChanged(e);
		}


		private void displayContextMenuStrip_Opening(object sender, CancelEventArgs e) {
			if (_showDefaultContextMenu && Project != null) {
				// Remove DummyItem
				if (ContextMenuStrip.Items.Contains(dummyItem))
					ContextMenuStrip.Items.Remove(dummyItem);
				// Collect all actions provided by the current tool
				if (ActiveTool != null)
					WinFormHelpers.BuildContextMenu(ContextMenuStrip, ActiveTool.GetMenuItemDefs(this), Project, _hideMenuItemsIfNotGranted);
				// Collect all actions provided by the display itself
				WinFormHelpers.BuildContextMenu(ContextMenuStrip, GetMenuItemDefs(), Project, _hideMenuItemsIfNotGranted);

				if (ContextMenuStrip.Items.Count == 0) e.Cancel = true;
			}
		}


		private void displayContextMenuStrip_Closed(object sender, ToolStripDropDownClosedEventArgs e) {
			if (_showDefaultContextMenu) {
				WinFormHelpers.CleanUpContextMenu(ContextMenuStrip);
				// Add dummy item because context menus without items will not show
				if (ContextMenuStrip.Items.Count == 0)
					ContextMenuStrip.Items.Add(dummyItem);
			}
		}

		#endregion


		#region [Private] Types

		private enum EditAction { None, Copy, Cut, CopyWithModels, CutWithModels }


		private class EditBuffer {
			public EditBuffer() {
				action = EditAction.None;
				initialMousePos = null;
				pasteCount = 0;
				shapes = new ShapeCollection(5);
			}
			public void Clear() {
				initialMousePos = null;
				action = EditAction.None;
				pasteCount = 0;
				shapes.Clear();
			}
			public Point? initialMousePos;
			public EditAction action;
			public int pasteCount;
			public ShapeCollection shapes;
		}


		private class DisplayInvalidatedEventArgs : InvalidateEventArgs {

			public DisplayInvalidatedEventArgs()
				: base(Rectangle.Empty) {
			}


			public DisplayInvalidatedEventArgs(Rectangle invalidatedRect)
				: base(Rectangle.Empty) {
				r = invalidatedRect;
			}


			public new Rectangle InvalidRect {
				get { return r; }
				internal set { r = value; }
			}


			private Rectangle r = Rectangle.Empty;
		}


		private class DisplayShapesEventArgs : DiagramPresenterShapesEventArgs {

			public DisplayShapesEventArgs(Shape shape)
				: base(shape) {
			}


			public DisplayShapesEventArgs(IEnumerable<Shape> shapes)
				: base(shapes) {
			}


			internal DisplayShapesEventArgs()
				: base() {
			}


			internal ReadOnlyList<Shape> ShapesList {
				get { return (ReadOnlyList<Shape>)Shapes; }
			}


			internal void SetShapes(IEnumerable<Shape> shapes) {
				ShapesList.Clear();
				ShapesList.AddRange(shapes);
			}


			internal void SetShapes(Shape shape) {
				ShapesList.Clear();
				ShapesList.Add(shape);
			}

		}


		private class DisplayShapeEventArgs : DiagramPresenterShapeEventArgs {

			public DisplayShapeEventArgs(Shape shape)
				: base(shape) {
			}


			internal DisplayShapeEventArgs()
				: base() {
			}


			internal void SetShape(Shape shape) {
				Shape = shape;
			}

		}

		#endregion


		#region Property Default Values

		// Behavior
		private const bool DefaultValueZoomWithMouseWheel = false;
		private const bool DefaultValueShowScrollBars = true;
		private const bool DefaultValueHideMenuItemsIfNotGranted = false;
		private const bool DefaultValueShowDefaultContextMenu = true;
		private const bool DefaultValueSnapToGrid = true;
		private const int DefaultValueSnapDistance = 5;
		private const int DefaultValueMinRotateDistance = 30;

		// Appearance
		private const int DefaultValueGripSize = 3;
		private const int DefaultValueGridSize = 20;
		private const bool DefaultValueShowGrid = true;
		private const int DefaultValueZoomLevel = 100;
		private const bool DefaultValueHighQualityBackground = true;
		private const bool DefaultValueHighQualityRendering = true;
		private const ControlPointShape DefaultValueResizePointShape = ControlPointShape.Square;
		private const ControlPointShape DefaultValueConnectionPointShape = ControlPointShape.Circle;

		private const int DefaultValueBackgroundGradientAngle = 45;
		private const byte DefaultValueGridAlpha = 255;
		private const byte DefaultValueSelectionAlpha = 255;
		private const byte defaultPreviewColorAlpha = 96;
		private const byte defaultPreviewBackColorAlpha = 64;

		private const RenderingQuality defaultRenderingQuality = RenderingQuality.HighQuality;
		private const RenderingQuality DefaultValueRenderingQualityHigh = RenderingQuality.HighQuality;
		private const RenderingQuality DefaultValueRenderingQualityLow = RenderingQuality.DefaultQuality;

		private static readonly Color DefaultValueBackColor = Color.FromKnownColor(KnownColor.Control);
		private static readonly Color DefaultValueBackColorGradient = Color.FromKnownColor(KnownColor.Control);
		private static readonly Color DefaultValueGridColor = Color.Gainsboro;
		private static readonly Color DefaultColorSelectionNormal = Color.DarkGreen;
		private static readonly Color DefaultColorSelectionHilight = Color.Firebrick;
		private static readonly Color DefaultColorSelectionInactive = Color.Gray;
		private static readonly Color DefaultColorSelectionInterior = Color.WhiteSmoke;
		private static readonly Color DefaultColorToolPreview = Color.FromArgb(defaultPreviewColorAlpha, Color.SteelBlue);
		private static readonly Color defaultToolPreviewBackColor = Color.FromArgb(defaultPreviewBackColorAlpha, Color.LightSlateGray);

		#endregion


		#region Constants

		private const int shadowSize = 20;
		private const int scrollAreaMargin = 40;	// The distance between the diagram 'sheet' and the end of the scrollable area
		private const int autoScrollMargin = 40;
		private const byte inlaceTextBoxBackAlpha = 128;

		private const double mmToInchFactor = 0.039370078740157477;

		// String constants for exception messages
		private const string ErrMessageOnlyAllowedWhilePaining = "Calling this method is only allowed while painting.";
		private const string ErrMessageResetTransformHasNotBeenCalled = "ResetTransformation has to be called before calling this method.";
		private const string ErrMessageRestoreTransformationHasNotBeenCalled = "RestoreTransformation has to be called before calling this method.";

		#endregion


		#region Fields

		private static Dictionary<int, Cursor> _registeredCursors = new Dictionary<int, Cursor>(10);

		private bool _projectIsRegistered = false;
		private bool _repositoryIsRegistered = false;

		// Contains all active layers (new shapes will be assigned to these layers)
		private HashCollection<int> _activeLayerIds = new HashCollection<int>();
		private LayerIds _activeCombinableLayers = LayerIds.None;
		private int _activeNonCombinableLayer = Layer.NoLayerId;
		// Contains all hidden layers
		private LayerIds _hiddenSharedLayers = LayerIds.None;
		private HashCollection<int> _hiddenLayerIds = new HashCollection<int>();

		// Fields for mouse related display behavior (click handling, scroll and zoom)
		private int _minRotateDistance = DefaultValueMinRotateDistance;
		private bool _mouseEventWasHandled = false;
		private bool _zoomWithMouseWheel = false;
		private ScrollEventArgs _scrollEventArgsH = new ScrollEventArgs(ScrollEventType.ThumbTrack, 0);
		private ScrollEventArgs _scrollEventArgsV = new ScrollEventArgs(ScrollEventType.ThumbTrack, 0);
		private Point _universalScrollStartPos = Geometry.InvalidPoint;
		private Rectangle _universalScrollFixPointBounds = Geometry.InvalidRectangle;
		private Cursor _universalScrollCursor = Cursors.NoMove2D;
		private bool _universalScrollEnabled = false;
		private Timer _autoScrollTimer = new Timer();

		private bool _showScrollBars = DefaultValueShowScrollBars;
		private bool _hideMenuItemsIfNotGranted = DefaultValueHideMenuItemsIfNotGranted;
		private int _handleRadius = DefaultValueGripSize;
		private bool _snapToGrid = DefaultValueSnapToGrid;
		private int _snapDistance = DefaultValueSnapDistance;
		private int _gridSpace = DefaultValueGridSize;
		private bool _gridVisible = DefaultValueShowGrid;
		private Size _gridSize = Size.Empty;
		private bool _scrollBarHVisible = true;
		private bool _scrollBarVVisible = true;
#if DEBUG_UI
		private bool _isInvalidateAreasVisible = false;
		private bool _isCellOccupationVisible = false;
#endif
		private bool _showDefaultContextMenu = DefaultValueShowDefaultContextMenu;
		private ControlPointShape _resizePointShape = DefaultValueResizePointShape;
		private ControlPointShape _connectionPointShape = DefaultValueConnectionPointShape;

		// Graphics and Graphics Settings
		private Graphics _infoGraphics;
		private Matrix _pointMatrix = new Matrix();
		private Matrix _matrix = new Matrix();
		private Graphics _currentGraphics;
		private Rectangle _invalidationBuffer = Rectangle.Empty;
		private bool _graphicsIsTransformed = false;
		private int _suspendUpdateCounter = 0;
		private int _collectingChangesCounter = 0;
		private Rectangle _invalidatedAreaBuffer = Rectangle.Empty;
		private DisplayInvalidatedEventArgs _invalidatedEventArgs = new DisplayInvalidatedEventArgs();
		private bool _boundsChanged = false;
		private FillStyle _hintBackgroundStyle = null;
		private LineStyle _hintForegroundStyle = null;

		private bool _highQualityBackground = DefaultValueHighQualityBackground;
		private bool _highQualityRendering = DefaultValueHighQualityRendering;
		private RenderingQuality _renderingQualityHigh = DefaultValueRenderingQualityHigh;
		private RenderingQuality _renderingQualityLow = DefaultValueRenderingQualityLow;
		private RenderingQuality _currentRenderingQuality = defaultRenderingQuality;

		// Colors
		private byte _gridAlpha = DefaultValueGridAlpha;
		private byte _toolPreviewColorAlpha = defaultPreviewColorAlpha;
		private byte _toolPreviewBackColorAlpha = defaultPreviewBackColorAlpha;
		private byte _selectionAlpha = DefaultValueSelectionAlpha;
		private Color _backColor = DefaultValueBackColor;
		private Color _gradientBackColor = DefaultValueBackColorGradient;
		private Color _gridColor = DefaultValueGridColor;
		private Color _selectionNormalColor = DefaultColorSelectionNormal;
		private Color _selectionHilightColor = DefaultColorSelectionHilight;
		private Color _selectionInactiveColor = DefaultColorSelectionInactive;
		private Color _selectionInteriorColor = DefaultColorSelectionInterior;
		private Color _toolPreviewColor = DefaultColorToolPreview;
		private Color _toolPreviewBackColor = defaultToolPreviewBackColor;

		// Pens
		private Pen _gridPen;						// pen for drawing the grid
		private Pen _outlineInteriorPen;			// pen for the interior of thick outlines
		private Pen _outlineNormalPen;				// pen for drawing thick shape outlines (normal)
		private Pen _outlineHilightPen;				// pen for drawing thick shape outlines (highlighted)
		private Pen _outlineInactivePen;			// pen for drawing thick shape outlines (inactive)
		private Pen _handleNormalPen;				// pen for drawing shape handles (normal)
		private Pen _handleHilightPen;				// pen for drawing connection point indicators
		private Pen _handleInactivePen;				// pen for drawing inactive handles
		private Pen _toolPreviewPen;				// Pen for drawing tool preview infos (rotation preview, selection frame, etc)
		private Pen _outerSnapPen = new Pen(Color.FromArgb(196, Color.WhiteSmoke), 2);
		private Pen _innerSnapPen = new Pen(Color.FromArgb(196, Color.SteelBlue), 1);

		// Brushes
		private Brush _controlBrush = null;				// brush for painting the ownerDisplay control's background
		private Brush _handleInteriorBrush = null;		// brush for filling shape handles
		private Brush _toolPreviewBackBrush = null;		// brush for filling tool preview info (rotation preview, selection frame, etc)
		private Brush _inplaceTextboxBackBrush = null;	// Brush for filling the background of the inplaceTextBox.
		private Brush _diagramShadowBrush = new SolidBrush(Color.FromArgb(128, Color.Gray)); // Brush for a shadow underneath the diagram

		// Other drawing stuff
		private int _controlBrushGradientAngle = DefaultValueBackgroundGradientAngle;
		private StringFormat _previewTextFormatter = new StringFormat();
		private double _controlBrushGradientSin = Math.Sin(Geometry.DegreesToRadians(45f));
		private double _controlBrushGradientCos = Math.Cos(Geometry.DegreesToRadians(45f));
		private Size _controlBrushSize = Size.Empty;
		private GraphicsPath _rotatePointPath = new GraphicsPath();
		private GraphicsPath _connectionPointPath = new GraphicsPath();
		private GraphicsPath _resizePointPath = new GraphicsPath();
		private Point[] _arrowShape = new Point[3];

		private bool _drawDiagramSheet = true;	// Specifies whether to draw the diagram sheet
		private int _diagramPosX;		// Position of the left side of the Diagram on the control
		private int _diagramPosY;		// Position of the upper side of the Diagram on the control
		private int _zoomLevel = DefaultValueZoomLevel;	// Zoom level in percentage
		private float _zoomfactor = 1f;// zoomFactor for transforming Diagram coordinates to control coordinates (range: >0 to x, 100% == 1)
		private int _scrollPosX = 0;	// horizontal position of the scrolled Diagram (== horizontal scrollbar value)
		private int _scrollPosY = 0;	// vertical position of the scrolled Diagram (== vertical scrollbar value)
		private int _invalidateDelta;	// handle radius or selection outline lineWidth (amount of pixels the invalidated area has to be increased)

		// Components
		private PropertyController _propertyController;
		private DiagramSetController _diagramSetController;
		private DiagramController _diagramController;
		private Tool _privateTool = null;

		// -- In-Place Editing --
		// text box currently used for in-place text editing
		private InPlaceTextBox _inplaceTextbox;
		// shape currently edited
		private ICaptionedShape _inplaceShape;
		// index of caption within shape
		private int _inplaceCaptionIndex;

		// Lists and Collections
		private ShapeCollection _selectedShapes = new ShapeCollection();
		private Dictionary<Shape, ShapeConnectionInfo> _shapesConnectedToSelectedShapes = new Dictionary<Shape, ShapeConnectionInfo>();
		private EditBuffer _editBuffer = new EditBuffer();	// Buffer for Copy/Cut/Paste-Actions
		private Rectangle _copyCutBounds = Rectangle.Empty;
		private Point _copyCutMousePos = Point.Empty;
		private List<Shape> _shapeBuffer = new List<Shape>();
		private List<IModelObject> _modelBuffer = new List<IModelObject>();

		// Buffers
		private Rectangle _rectBuffer;					// buffer for rectangles
		private Point[] _pointBuffer = new Point[4];		// point array buffer
		private Rectangle _clipRectBuffer;				// buffer for clipRectangle transformation
		private Rectangle _drawBounds;					// drawing area of the display (ClientRectangle - scrollbars)
		private Rectangle _scrollAreaBounds;				// Scrollable area (Diagram sheet incl. off-sheet shapes and margin)
		//private GraphicsPath selectionPath = new GraphicsPath();	// Path used for highlighting all selected selectedShapes

		// Temporary Buffer for last Mouse position (for MouseCursor sensitive context menu actions, e.g. Paste)
		private Point _lastMousePos;

		DisplayShapesEventArgs _shapesEventArgs = new DisplayShapesEventArgs();
		DisplayShapeEventArgs _shapeEventArgs = new DisplayShapeEventArgs();

#if DEBUG_UI
		// Debugging stuff
		private Stopwatch _stopWatch = new Stopwatch();
		//private long paintCounter;
		private Color _invalidatedAreaColor1 = Color.FromArgb(32, Color.Red);
		private Color _invalidatedAreaColor2 = Color.FromArgb(32, Color.Green);
		private Brush _clipRectBrush;
		private Brush _clipRectBrush1;
		private Brush _clipRectBrush2;
		private Pen _invalidatedAreaPen = null;
		private Pen _invalidatedAreaPen1 = null;
		private Pen _invalidatedAreaPen2 = null;
		private List<Rectangle> _invalidatedAreas;
#endif
		#endregion
	}


	internal class DisplayVScrollBar : VScrollBar {
		
		public DisplayVScrollBar()
			: base() {
		}

		public new bool CanFocus {
			get { return false; }
		}

		protected override void OnGotFocus(EventArgs e) {
			base.OnGotFocus(e);
		}

	}


	internal class DisplayHScrollBar : HScrollBar {

		public DisplayHScrollBar()
			: base() {
		}

		public new bool CanFocus {
			get { return false; }
		}

		protected override void OnGotFocus(EventArgs e) {
			base.OnGotFocus(e);
		}

	}

}
