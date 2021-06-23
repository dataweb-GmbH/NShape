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
using System.ComponentModel;
using System.Drawing;

using Dataweb.NShape.Advanced;


namespace Dataweb.NShape.ElectricalShapes {


	public class AutoSwitchSymbol : ElectricalSquareBase {

		/// <override></override>
		public override Shape Clone() {
			Shape result = new AutoSwitchSymbol(Type, this.Template);
			result.CopyFrom(this);
			return result;
		}


		/// <override></override>
		protected override bool CalculatePath() {
			if (base.CalculatePath()) {
				CalcShapeRectangles(out smallRectBuffer, out largeRectBuffer);

				Path.Reset();
				Path.StartFigure();
				Path.AddRectangle(smallRectBuffer);
				Path.AddRectangle(largeRectBuffer);
				Path.CloseFigure();
				return true;
			}
			return false;
		}


		protected override Rectangle CalculateBoundingRectangle(bool tight) {
			if (tight) {
				CalcShapeRectangles(out smallRectBuffer, out largeRectBuffer);
				largeRectBuffer.Offset(X, Y);
				smallRectBuffer.Offset(X, Y);

				float angle = Geometry.TenthsOfDegreeToDegrees(Angle);
				Point tl = Geometry.RotatePoint(X, Y, angle, largeRectBuffer.Left, largeRectBuffer.Top);
				Point tr = Geometry.RotatePoint(X, Y, angle, largeRectBuffer.Right, largeRectBuffer.Top);
				Point bl1 = Geometry.RotatePoint(X, Y, angle, largeRectBuffer.Left, largeRectBuffer.Bottom);
				Point br1 = Geometry.RotatePoint(X, Y, angle, largeRectBuffer.Right, largeRectBuffer.Bottom);
				Point bl2 = Geometry.RotatePoint(X, Y, angle, smallRectBuffer.Left, smallRectBuffer.Bottom);
				Point br2 = Geometry.RotatePoint(X, Y, angle, smallRectBuffer.Right, smallRectBuffer.Bottom);

				Rectangle result = Rectangle.Empty;
				Geometry.CalcBoundingRectangle(tl, tr, bl1, br1, out result);
				result = Geometry.UniteWithRectangle(bl2, result);
				result = Geometry.UniteWithRectangle(br2, result);

				ShapeUtils.InflateBoundingRectangle(ref result, LineStyle);
				return result;
			} else return base.CalculateBoundingRectangle(tight);
		}


		protected internal AutoSwitchSymbol(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal AutoSwitchSymbol(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		private void CalcShapeRectangles(out Rectangle smallRectangle, out Rectangle largeRectangle) {
			smallRectangle = largeRectangle = Rectangle.Empty;

			int left = (int)Math.Round(-Size / 2f);
			int top = (int)Math.Round(-Size / 2f);
			int bottom = top + Size;

			smallRectangle.Width = (Size / 6) * 3;
			smallRectangle.Height = Size / 6;
			smallRectangle.X = left + ((Size - smallRectangle.Width) / 2);
			smallRectangle.Y = bottom - smallRectangle.Height;

			largeRectangle.Width = Size - (smallRectangle.Width / 4);
			largeRectangle.Height = Size - smallRectangle.Height;
			largeRectangle.X = left + ((Size - largeRectangle.Width) / 2);
			largeRectangle.Y = top;
		}


		private Rectangle smallRectBuffer;
		private Rectangle largeRectBuffer;
	}


	public class SwitchSymbol : ElectricalSquareBase {

		/// <override></override>
		public override Shape Clone() {
			Shape result = new SwitchSymbol(Type, this.Template);
			result.CopyFrom(this);
			return result;
		}


		/// <override></override>
		protected override bool CalculatePath() {
			if (base.CalculatePath()) {
				Path.Reset();
				int left = (int)Math.Round(-Size / 2f);
				int top = (int)Math.Round(-Size / 2f);

				_shapeRect.X = left;
				_shapeRect.Y = top;
				_shapeRect.Width = Size;
				_shapeRect.Height = Size;

				Path.StartFigure();
				Path.AddRectangle(_shapeRect);
				Path.CloseFigure();
				return true;
			}
			return false;
		}


		protected internal SwitchSymbol(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal SwitchSymbol(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		#region Fields
		Rectangle _shapeRect;
		#endregion
	}


	public class TransformerSymbol : ElectricalRectangleBase {

		[Browsable(false)]
		public override IFillStyle FillStyle {
			get { return base.FillStyle; }
			set { base.FillStyle = value; }
		}


		/// <override></override>
		public override Shape Clone() {
			Shape result = new TransformerSymbol(Type, this.Template);
			result.CopyFrom(this);
			return result;
		}


		/// <override></override>
		public override bool HasControlPointCapability(ControlPointId controlPointId, ControlPointCapabilities controlPointCapability) {
			switch (controlPointId) {
				case ControlPointIds.TopCenterControlPoint:
				case ControlPointIds.BottomCenterControlPoint:
					return ((controlPointCapability & ControlPointCapabilities.Resize) != 0
						|| ((controlPointCapability & ControlPointCapabilities.Connect) != 0 && IsConnectionPointEnabled(controlPointId)));
				case ControlPointIds.TopLeftControlPoint:
				case ControlPointIds.TopRightControlPoint:
				case ControlPointIds.MiddleLeftControlPoint:
				case ControlPointIds.MiddleRightControlPoint:
				case ControlPointIds.BottomLeftControlPoint:
				case ControlPointIds.BottomRightControlPoint:
					return ((controlPointCapability & ControlPointCapabilities.Resize) != 0);
				case ControlPointIds.MiddleCenterControlPoint:
				case ControlPointId.Reference:
					return ((controlPointCapability & ControlPointCapabilities.Reference) != 0
						|| (controlPointCapability & ControlPointCapabilities.Rotate) != 0);
				default:
					return base.HasControlPointCapability(controlPointId, controlPointCapability);
			}
		}


		protected override bool ContainsPointCore(int x, int y) {
			Rectangle upperRing, lowerRing;
			CalcRingBounds(out upperRing, out lowerRing);
			upperRing.Offset(X, Y);
			lowerRing.Offset(X, Y);
			float radius = upperRing.Width / 2f;
			PointF upperRingCenter = Geometry.RotatePoint(X, Y, Geometry.TenthsOfDegreeToDegrees(Angle), upperRing.X + radius, upperRing.Y + radius);
			PointF lowerRingCenter = Geometry.RotatePoint(X, Y, Geometry.TenthsOfDegreeToDegrees(Angle), lowerRing.X + radius, lowerRing.Y + radius);
			return Geometry.CircleContainsPoint(upperRingCenter.X, upperRingCenter.Y, radius, x, y, 0)
				|| Geometry.CircleContainsPoint(lowerRingCenter.X, lowerRingCenter.Y, radius, x, y, 0);
		}


		public override void Draw(Graphics graphics) {
			Pen pen = ToolCache.GetPen(LineStyle, null, null);
			DrawOutline(graphics, pen);
		}


		protected override Rectangle CalculateBoundingRectangle(bool tight) {
			if (tight) {
				CalcRingBounds(out _upperRingBounds, out _lowerRingBounds);
				float angle = Geometry.TenthsOfDegreeToDegrees(Angle);
				float ringRadius = _upperRingBounds.Width / 2f;

				PointF upperCenter = PointF.Empty;
				upperCenter.X = X + (_upperRingBounds.Left + _upperRingBounds.Width / 2f);
				upperCenter.Y = Y + (_upperRingBounds.Top + _upperRingBounds.Height / 2f);
				PointF lowerCenter = PointF.Empty;
				lowerCenter.X = X + (_lowerRingBounds.Left + _lowerRingBounds.Width / 2f);
				lowerCenter.Y = Y + (_lowerRingBounds.Top + _lowerRingBounds.Height / 2f);

				upperCenter = Geometry.RotatePoint(X, Y, angle, upperCenter);
				lowerCenter = Geometry.RotatePoint(X, Y, angle, lowerCenter);

				Rectangle result = Rectangle.Empty;
				result.X = (int)Math.Round(Math.Min(upperCenter.X - ringRadius, lowerCenter.X - ringRadius));
				result.Y = (int)Math.Round(Math.Min(upperCenter.Y - ringRadius, lowerCenter.Y - ringRadius));
				result.Width = (int)Math.Round(Math.Max(upperCenter.X + ringRadius, lowerCenter.X + ringRadius)) - result.X;
				result.Height = (int)Math.Round(Math.Max(upperCenter.Y + ringRadius, lowerCenter.Y + ringRadius)) - result.Y;

				result = Geometry.UniteWithRectangle(Geometry.RotatePoint(X, Y, angle, X, Y - (Height / 2)), result);
				result = Geometry.UniteWithRectangle(Geometry.RotatePoint(X, Y, angle, X, Y - (Height / 2) + Height), result);

				ShapeUtils.InflateBoundingRectangle(ref result, LineStyle);
				return result;
			} else return base.CalculateBoundingRectangle(tight);
		}


		protected internal TransformerSymbol(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal TransformerSymbol(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		/// <override></override>
		protected override void InitializeToDefault(IStyleSet styleSet) {
			base.InitializeToDefault(styleSet);
			FillStyle = styleSet.FillStyles.Transparent;
			Width = 40;
			Height = 70;
		}


		/// <override></override>
		protected override bool CalculatePath() {
			Path.Reset();

			int top = -Height / 2;
			int bottom = -(Height / 2) + Height;
			CalcRingBounds(out _upperRingBounds, out _lowerRingBounds);

			// Add the lines between connection point and circles only if 
			// necessary. Otherwise, DrawPath() throws an OutOfMemoryException 
			// on Windows XP/Vista when drawing with a pen thicker than 1 pixel...
			Path.StartFigure();
			if (top < _upperRingBounds.Top)
				Path.AddLine(0, top, 0, _upperRingBounds.Top);
			Path.AddEllipse(_upperRingBounds);
			Path.AddEllipse(_lowerRingBounds);
			if (bottom > _lowerRingBounds.Bottom)
				Path.AddLine(0, _lowerRingBounds.Bottom, 0, bottom);
			Path.CloseFigure();

			return true;
		}


		private void CalcRingBounds(out Rectangle upperRingBounds, out Rectangle lowerRingBounds) {
			upperRingBounds = lowerRingBounds = Rectangle.Empty;

			float ringDiameter = Math.Min(5 * (Height / 8f), Width);
			float d = Math.Min(Height / 8f, Width / 4f);
			int left = (int)Math.Round(-ringDiameter / 2f);

			upperRingBounds.X = left;
			upperRingBounds.Y = (int)Math.Round(-ringDiameter + d);
			upperRingBounds.Width = (int)Math.Round(ringDiameter);
			upperRingBounds.Height = (int)Math.Round(ringDiameter);

			lowerRingBounds.X = left;
			lowerRingBounds.Y = (int)Math.Round(-d);
			lowerRingBounds.Width = (int)Math.Round(ringDiameter);
			lowerRingBounds.Height = (int)Math.Round(ringDiameter);
		}


		#region Fields

		private Rectangle _upperRingBounds;
		private Rectangle _lowerRingBounds;

		#endregion
	}


	public class EarthSymbol : ElectricalRectangleBase {

		/// <override></override>
		protected override void InitializeToDefault(IStyleSet styleSet) {
			base.InitializeToDefault(styleSet);
			Width = 40;
			Height = 40;
		}


		/// <override></override>
		public override Shape Clone() {
			Shape result = new EarthSymbol(Type, this.Template);
			result.CopyFrom(this);
			return result;
		}


		/// <override></override>
		public override bool HasControlPointCapability(ControlPointId controlPointId, ControlPointCapabilities controlPointCapability) {
			switch (controlPointId) {
				case ControlPointIds.TopCenterControlPoint:
					return ((controlPointCapability & ControlPointCapabilities.Resize) != 0
							|| ((controlPointCapability & ControlPointCapabilities.Connect) != 0
								&& IsConnectionPointEnabled(controlPointId)));
				case ControlPointIds.TopLeftControlPoint:
				case ControlPointIds.TopRightControlPoint:
				case ControlPointIds.MiddleLeftControlPoint:
				case ControlPointIds.MiddleRightControlPoint:
				case ControlPointIds.BottomLeftControlPoint:
				case ControlPointIds.BottomCenterControlPoint:
				case ControlPointIds.BottomRightControlPoint:
					return ((controlPointCapability & ControlPointCapabilities.Resize) != 0);
				case ControlPointIds.MiddleCenterControlPoint:
					return ((controlPointCapability & ControlPointCapabilities.Reference) != 0
							|| (controlPointCapability & ControlPointCapabilities.Rotate) != 0);
				default:
					return base.HasControlPointCapability(controlPointId, controlPointCapability);
				//return false;
			}
		}


		protected override Rectangle CalculateBoundingRectangle(bool tight) {
			if (tight) {
				CalcShapePoints(ref _shapeBuffer);

				Matrix.Reset();
				Matrix.Translate(X, Y);
				Matrix.RotateAt(Geometry.TenthsOfDegreeToDegrees(Angle), Center, System.Drawing.Drawing2D.MatrixOrder.Append);
				Matrix.TransformPoints(_shapeBuffer);

				Rectangle result;
				Geometry.CalcBoundingRectangle(_shapeBuffer, out result);
				ShapeUtils.InflateBoundingRectangle(ref result, LineStyle);
				return result;
			} else return base.CalculateBoundingRectangle(tight);
		}


		protected internal EarthSymbol(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal EarthSymbol(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		/// <override></override>
		protected override bool CalculatePath() {
			if (base.CalculatePath()) {
				CalcShapePoints(ref _shapeBuffer);

				Path.Reset();
				Path.StartFigure();
				Path.AddLines(_shapeBuffer);
				Path.CloseFigure();

				return true;
			}
			return false;
		}


		private void CalcShapePoints(ref Point[] points) {
			int left = (int)Math.Round(-Width / 2f);
			int top = (int)Math.Round(-Height / 2f);
			int right = left + Width;
			int bottom = top + Height;

			int lineWidth = LineStyle.LineWidth;
			int lineX = -lineWidth;
			int largeAntennaTop = -lineWidth;
			int largeAntennaBottom = lineWidth;
			int mediumAntennaTop = bottom - (int)Math.Round(Height / 4f) - lineWidth;
			int mediumAntennaBottom = Math.Min(bottom, bottom - (int)Math.Round(Height / 4f) + lineWidth);
			int smallAntennaTop = bottom - lineWidth - lineWidth;

			if (points == null) points = new Point[25];

			// downward line from top to large 'antenna', left side
			points[0].X = lineX;
			points[0].Y = top;
			points[1].X = lineX;
			points[1].Y = largeAntennaTop;

			// large 'antenna', left side
			points[2].X = left;
			points[2].Y = largeAntennaTop;
			points[3].X = left;
			points[3].Y = largeAntennaBottom;
			points[4].X = lineX;
			points[4].Y = largeAntennaBottom;

			// downward line from large 'antenna' to medium 'antenna', left side
			points[5].X = lineX;
			points[5].Y = mediumAntennaTop;

			// medium 'antenna', left side
			int antennaLeft = left + (int)Math.Round(Width / 6f);
			points[6].X = antennaLeft;
			points[6].Y = mediumAntennaTop;
			points[7].X = antennaLeft;
			points[7].Y = mediumAntennaBottom;
			points[8].X = lineX;
			points[8].Y = mediumAntennaBottom;

			// downward line from medium 'antenna' to small 'antenna', left side				
			points[9].X = lineX;
			points[9].Y = smallAntennaTop;

			// small 'antenna', complete
			antennaLeft = left + (int)Math.Round(Width / 3f);
			int antennaRight = right - (int)Math.Round(Width / 3f);
			points[10].X = antennaLeft;
			points[10].Y = smallAntennaTop;
			points[11].X = antennaLeft;
			points[11].Y = bottom;
			points[12].X = antennaRight;
			points[12].Y = bottom;
			lineX = lineWidth;
			points[13].X = antennaRight;
			points[13].Y = smallAntennaTop;
			points[14].X = lineX;
			points[14].Y = smallAntennaTop;

			// upward line from small 'antenna' to medium 'antenna', right side
			points[15].X = lineX;
			points[15].Y = mediumAntennaBottom;

			// medium 'antenna', right side
			antennaRight = right - (int)Math.Round(Width / 6f);
			points[16].X = antennaRight;
			points[16].Y = mediumAntennaBottom;
			points[17].X = antennaRight;
			points[17].Y = mediumAntennaTop;
			points[18].X = lineX;
			points[18].Y = mediumAntennaTop;

			// upward line from medium 'antenna' to large 'antenna', right side
			points[19].X = lineX;
			points[19].Y = largeAntennaBottom;

			// large 'antenna', right side
			points[20].X = right;
			points[20].Y = largeAntennaBottom;
			points[21].X = right;
			points[21].Y = largeAntennaTop;
			points[22].X = lineX;
			points[22].Y = largeAntennaTop;

			// upward line from large 'antenna' to top, right side
			points[23].X = lineX;
			points[23].Y = top;
			points[24].X = -lineWidth;
			points[24].Y = top;
		}


		#region Fields

		private Point[] _shapeBuffer = null;

		#endregion
	}


	public class DisconnectingPoint : ElectricalRectangleBase {

		/// <override></override>
		public override Shape Clone() {
			Shape result = new DisconnectingPoint(Type, this.Template);
			result.CopyFrom(this);
			return result;
		}


		/// <override></override>
		public override bool HasControlPointCapability(ControlPointId controlPointId, ControlPointCapabilities controlPointCapability) {
			switch (controlPointId) {
				case ControlPointIds.MiddleLeftControlPoint:
				case ControlPointIds.MiddleRightControlPoint:
					return ((controlPointCapability & ControlPointCapabilities.Resize) != 0
							|| ((controlPointCapability & ControlPointCapabilities.Connect) != 0
								&& IsConnectionPointEnabled(controlPointId)));
				case ControlPointIds.TopLeftControlPoint:
				case ControlPointIds.TopCenterControlPoint:
				case ControlPointIds.TopRightControlPoint:
				case ControlPointIds.BottomLeftControlPoint:
				case ControlPointIds.BottomCenterControlPoint:
				case ControlPointIds.BottomRightControlPoint:
					return ((controlPointCapability & ControlPointCapabilities.Resize) != 0);
				case ControlPointIds.MiddleCenterControlPoint:
					return ((controlPointCapability & ControlPointCapabilities.Reference) != 0
							|| (controlPointCapability & ControlPointCapabilities.Rotate) != 0);
				default:
					return base.HasControlPointCapability(controlPointId, controlPointCapability);
				//return false;
			}
		}


		protected internal DisconnectingPoint(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal DisconnectingPoint(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		/// <override></override>
		protected override bool CalculatePath() {
			if (base.CalculatePath()) {
				Path.Reset();
				int left = (int)Math.Round(-Width / 2f);
				int top = (int)Math.Round(-Height / 2f);
				int right = left + Width;
				int bottom = top + Height;

				int offsetX = (int)Math.Round(Width / 6f);
				int offsetY = (int)Math.Round(Height / 3f);

				Path.StartFigure();
				Path.AddLine(left, top, left, bottom);
				Path.CloseFigure();
				Path.StartFigure();
				Path.AddLine(left, top, left, bottom);
				Path.CloseFigure();
				Path.StartFigure();
				Path.AddLine(left + offsetX, top + offsetY, left + offsetX, bottom - offsetY);
				Path.CloseFigure();
				Path.StartFigure();
				Path.AddLine(left + offsetX + offsetX, 0, right - offsetX - offsetX, 0);
				Path.CloseFigure();
				Path.StartFigure();
				Path.AddLine(right - offsetX, top + offsetY, right - offsetX, bottom - offsetY);
				Path.CloseFigure();
				Path.StartFigure();
				Path.AddLine(right, top, right, bottom);
				Path.CloseFigure();
				return true;
			}
			return false;
		}


		#region Fields
		private Rectangle _shapeBuffer = Rectangle.Empty;
		#endregion
	}

}
