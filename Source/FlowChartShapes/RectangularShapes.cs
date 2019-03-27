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
using System.Drawing;
using System.Drawing.Drawing2D;

using Dataweb.NShape.Advanced;
using System.Diagnostics;


namespace Dataweb.NShape.FlowChartShapes {

	public class TerminatorSymbol : FlowChartRectangleBase {

		/// <override></override>
		public override Shape Clone() {
			Shape result = new TerminatorSymbol(Type, this.Template);
			result.CopyFrom(this);
			return result;
		}


		/// <override></override>
		protected override Rectangle CalculateBoundingRectangle(bool tight) {
			if (tight) {
				Rectangle result = Rectangle.Empty; ;
				float angle = Geometry.TenthsOfDegreeToDegrees(Angle);
				if (angle == 0 || angle == 180) {
					result.X = X - Width / 2;
					result.Y = Y - Height / 2;
					result.Width = Width;
					result.Height = Height;
				} else if (angle == 90 || angle == 270) {
					result.X = X - Height / 2;
					result.Y = Y - Width / 2;
					result.Width = Height;
					result.Height = Width;
				} else {
					float arcRadius = ArcDiameter / 2f;
					PointF topLeftCenter, bottomRightCenter;
					if (ArcDiameter == Width) {
						topLeftCenter = Geometry.RotatePoint(X, Y, angle, X, Y - (Height / 2f) + arcRadius);
						bottomRightCenter = Geometry.RotatePoint(X, Y, angle, X, Y + (Height / 2f) - arcRadius);
					} else {
						topLeftCenter = Geometry.RotatePoint(X, Y, angle, X - (Width / 2f) + arcRadius, Y);
						bottomRightCenter = Geometry.RotatePoint(X, Y, angle, X + (Width / 2f) - arcRadius, Y);
					}
					result.X = (int)Math.Round(Math.Min(topLeftCenter.X - arcRadius, bottomRightCenter.X - arcRadius));
					result.Y = (int)Math.Round(Math.Min(topLeftCenter.Y - arcRadius, bottomRightCenter.Y - arcRadius));
					result.Width = (int)Math.Round(Math.Max(topLeftCenter.X + arcRadius, bottomRightCenter.X + arcRadius)) - result.X;
					result.Height = (int)Math.Round(Math.Max(topLeftCenter.Y + arcRadius, bottomRightCenter.Y + arcRadius)) - result.Y;
				}
				return result;
			} else return base.CalculateBoundingRectangle(tight);
		}


		/// <override></override>
		protected internal TerminatorSymbol(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		/// <override></override>
		protected internal TerminatorSymbol(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		/// <override></override>
		protected override void CalcCaptionBounds(int index, out Rectangle captionBounds) {
			if (index != 0) throw new ArgumentOutOfRangeException("index");
			int left = (int)Math.Round(-Width / 2f);
			int top = (int)Math.Round(-Height / 2f);
			captionBounds = Rectangle.Empty;
			captionBounds.X = left + (ArcDiameter / 4);
			captionBounds.Y = top;
			captionBounds.Width = Width - (ArcDiameter / 2);
			captionBounds.Height = Height;
		}


		/// <override></override>
		protected override bool CalculatePath() {
			if (base.CalculatePath()) {
				int left = (int)Math.Round(-Width / 2f);
				int top = (int)Math.Round(-Height / 2f);
				int right = left + Width;
				int bottom = top + Height;

				int startAngle = (ArcDiameter == Width) ? 180 : 90;
				const int sweepAngle = 180;

				Path.Reset();
				Path.StartFigure();
				float arcRadius = ArcDiameter / 2f;
				if (ArcDiameter == Height) {
					Path.AddArc(left, top, ArcDiameter, ArcDiameter, startAngle, sweepAngle);
					if (Width > ArcDiameter)
						Path.AddLine(left + arcRadius, top, right - arcRadius, top);
					Path.AddArc(right - ArcDiameter, top, ArcDiameter, ArcDiameter, 180 + startAngle, sweepAngle);
					if (Width > ArcDiameter)
						Path.AddLine(right - arcRadius, bottom, left + arcRadius, bottom);
				} else {
					Path.AddArc(left, top, ArcDiameter, ArcDiameter, startAngle, sweepAngle);
					if (Height > ArcDiameter)
						Path.AddLine(right, top + arcRadius, right, bottom - arcRadius);
					Path.AddArc(left, bottom - ArcDiameter, ArcDiameter, ArcDiameter, 180 + startAngle, sweepAngle);
					if (Height > ArcDiameter)
						Path.AddLine(left, bottom - arcRadius, left, top + arcRadius);
				}
				Path.CloseFigure();
				return true;
			} else return false;
		}


		private int ArcDiameter {
			get { return (Width < Height) ? Width : Height; }
		}

	}


	public class ProcessSymbol : FlowChartRectangleBase {

		/// <summary>
		/// Provides constants for the control point id's of the shape.
		/// </summary>
		new public class ControlPointIds {
			/// <summary>ControlPointId of the top left control point.</summary>
			public const int TopLeftControlPoint = 1;
			/// <summary>ControlPointId of the top center control point.</summary>
			public const int TopCenterControlPoint = 2;
			/// <summary>ControlPointId of the top right control point.</summary>
			public const int TopRightControlPoint = 3;
			/// <summary>ControlPointId of the middle left control point.</summary>
			public const int MiddleLeftControlPoint = 4;
			/// <summary>ControlPointId of the middle right control point.</summary>
			public const int MiddleRightControlPoint = 5;
			/// <summary>ControlPointId of the bottom left control point.</summary>
			public const int BottomLeftControlPoint = 6;
			/// <summary>ControlPointId of the bottom center control point.</summary>
			public const int BottomCenterControlPoint = 7;
			/// <summary>ControlPointId of the bottom right control point.</summary>
			public const int BottomRightControlPoint = 8;
			/// <summary>ControlPointId of the center control point.</summary>
			public const int MiddleCenterControlPoint = 9;
			/// <summary>ControlPointId of the top left connection point.</summary>
			public const int TopLeftConnectionPoint = 10;
			/// <summary>ControlPointId of the top right connection point.</summary>
			public const int TopRightConnectionPoint = 11;
			/// <summary>ControlPointId of the bottom left connection point.</summary>
			public const int BottomLeftConnectionPoint = 12;
			/// <summary>ControlPointId of the bottom right connection point.</summary>
			public const int BottomRightConnectionPoint = 13;
		}


		/// <override></override>
		public override Shape Clone() {
			Shape result = new ProcessSymbol(Type, this.Template);
			result.CopyFrom(this);
			return result;
		}


		/// <override></override>
		protected override int ControlPointCount {
			get { return 13; }
		}


		/// <override></override>
		public override bool HasControlPointCapability(ControlPointId controlPointId, ControlPointCapabilities controlPointCapability) {
			// Customize control point capabilities
			switch (controlPointId) {
				case ControlPointIds.TopLeftConnectionPoint:
				case ControlPointIds.TopRightConnectionPoint:
				case ControlPointIds.BottomLeftConnectionPoint:
				case ControlPointIds.BottomRightConnectionPoint:
					// Add connection point functionality to the additional points
					return ((controlPointCapability & ControlPointCapabilities.Connect) != 0 && IsConnectionPointEnabled(controlPointId));
				default:
					return base.HasControlPointCapability(controlPointId, controlPointCapability);
			}
		}


		/// <override></override>
		protected override void CalcControlPoints() {
			base.CalcControlPoints();

			int left = (int)Math.Round(-Width / 2f);
			int top = (int)Math.Round(-Height / 2f);
			int right = left + Width;
			int bottom = top + Height;

			ControlPoints[9].X = left + (int)Math.Round(Width / 4f);
			ControlPoints[9].Y = top;
			ControlPoints[10].X = right - (int)Math.Round(Width / 4f);
			ControlPoints[10].Y = top;
			ControlPoints[11].X = left + (int)Math.Round(Width / 4f);
			ControlPoints[11].Y = bottom;
			ControlPoints[12].X = right - (int)Math.Round(Width / 4f);
			ControlPoints[12].Y = bottom;
		}


		/// <override></override>
		protected override bool CalculatePath() {
			if (base.CalculatePath()) {
				int left = (int)Math.Round(-Width / 2f);
				int top = (int)Math.Round(-Height / 2f);

				shapeRect.X = left;
				shapeRect.Y = top;
				shapeRect.Width = Width;
				shapeRect.Height = Height;

				Path.Reset();
				Path.StartFigure();
				Path.AddRectangle(shapeRect);
				Path.CloseFigure();
				return true;
			} else return false;
		}


		/// <override></override>
		protected internal ProcessSymbol(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		/// <override></override>
		protected internal ProcessSymbol(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		Rectangle shapeRect;
	}


	public class PredefinedProcessSymbol : ProcessSymbol {

		/// <override></override>
		public override Shape Clone() {
			Shape result = new PredefinedProcessSymbol(Type, this.Template);
			result.CopyFrom(this);
			return result;
		}


		/// <override></override>
		protected internal PredefinedProcessSymbol(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		/// <override></override>
		protected internal PredefinedProcessSymbol(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		/// <override></override>
		protected override void CalcCaptionBounds(int index, out Rectangle captionBounds) {
			if (index != 0) throw new ArgumentOutOfRangeException("index");
			base.CalcCaptionBounds(index, out captionBounds);
			captionBounds.X = (int)Math.Round(-(Width / 2f) + (Width / 8f));
			captionBounds.Width = Width - (Width / 4);
		}


		/// <override></override>
		protected override bool CalculatePath() {
			if (base.CalculatePath()) {
				Path.Reset();

				int left = (int)Math.Round(-Width / 2f);
				int top = (int)Math.Round(-Height / 2f);
				int right = left + Width;
				int sideBarWidth = 10;
				if (Width / 8 < 10)
					sideBarWidth = Width / 8;
				Rectangle rect = Rectangle.Empty;

				Path.StartFigure();
				rect.X = left; rect.Y = top; rect.Width = Width; rect.Height = Height;
				Path.AddRectangle(rect);

				rect.Width = sideBarWidth;
				Path.AddRectangle(rect);

				rect.X = right - sideBarWidth;
				Path.AddRectangle(rect);
				Path.CloseFigure();
				Path.FillMode = System.Drawing.Drawing2D.FillMode.Winding;
				return true;
			} else return false;
		}
	}


	public class InputOutputSymbol : FlowChartRectangleBase {

		/// <override></override>
		public override Shape Clone() {
			Shape result = new InputOutputSymbol(Type, this.Template);
			result.CopyFrom(this);
			return result;
		}


		/// <override></override>
		protected override Rectangle CalculateBoundingRectangle(bool tight) {
			if (tight) {
				int left = X + (int)Math.Round(-Width / 2f);
				int top = Y + (int)Math.Round(-Height / 2f);
				int right = left + Width;
				int bottom = top + Height;
				int offset = GetParallelOffset();
				float angle = Geometry.TenthsOfDegreeToDegrees(Angle);

				Point tl = Geometry.RotatePoint(X, Y, angle, left + offset, top);
				Point tr = Geometry.RotatePoint(X, Y, angle, right, top);
				Point bl = Geometry.RotatePoint(X, Y, angle, left, bottom);
				Point br = Geometry.RotatePoint(X, Y, angle, right - offset, bottom);

				Rectangle result;
				Geometry.CalcBoundingRectangle(tl, tr, bl, br, out result);
				ShapeUtils.InflateBoundingRectangle(ref result, LineStyle);

				return result;
			} else return base.CalculateBoundingRectangle(tight);
		}


		/// <override></override>
		protected internal InputOutputSymbol(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		/// <override></override>
		protected internal InputOutputSymbol(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		/// <override></override>
		protected override void CalcControlPoints() {
			int offset = GetParallelOffset();

			int left = (int)Math.Round(-Width / 2f);
			int top = (int)Math.Round(-Height / 2f);
			int right = left + Width;
			int bottom = top + Height;

			// top row (left to right)
			ControlPoints[0].X = left + offset;
			ControlPoints[0].Y = top;
			ControlPoints[1].X = 0;
			ControlPoints[1].Y = top;
			ControlPoints[2].X = right;
			ControlPoints[2].Y = top;

			// middle row (left to right)
			ControlPoints[3].X = left + (offset / 2);
			ControlPoints[3].Y = 0;
			ControlPoints[4].X = right - (offset / 2);
			ControlPoints[4].Y = 0;

			// bottom row (left to right)
			ControlPoints[5].X = left;
			ControlPoints[5].Y = bottom;
			ControlPoints[6].X = 0;
			ControlPoints[6].Y = bottom;
			ControlPoints[7].X = right - offset;
			ControlPoints[7].Y = bottom;

			// rotate handle
			ControlPoints[8].X = 0;
			ControlPoints[8].Y = 0;
		}


		/// <override></override>
		protected override void CalcCaptionBounds(int index, out Rectangle captionBounds) {
			if (index != 0) throw new ArgumentOutOfRangeException("index");
			base.CalcCaptionBounds(index, out captionBounds);
		}


		/// <override></override>
		protected override bool CalculatePath() {
			if (base.CalculatePath()) {
				Path.Reset();
				int left = (int)Math.Round(-Width / 2f);
				int top = (int)Math.Round(-Height / 2f);
				int right = left + Width;
				int bottom = top + Height;
				int offset = Height / 4;
				if (offset > Width)
					offset = Width / 4;

				// top row (left to right)
				shapePoints[0].X = left + offset; ;
				shapePoints[0].Y = top;
				shapePoints[1].X = right;
				shapePoints[1].Y = top;

				// bottom row (right to left)
				shapePoints[2].X = right - offset;
				shapePoints[2].Y = bottom;
				shapePoints[3].X = left;
				shapePoints[3].Y = bottom;

				Path.StartFigure();
				Path.AddPolygon(shapePoints);
				Path.CloseFigure();
				return true;
			} else return false;
		}


		private int GetParallelOffset() {
			return Math.Min(Height / 4, Width / 4);
		}


		#region Fields
		Point[] shapePoints = new Point[4];
		#endregion
	}


	public class DocumentSymbol : FlowChartRectangleBase {

		/// <override></override>
		public override Shape Clone() {
			Shape result = new DocumentSymbol(Type, this.Template);
			result.CopyFrom(this);
			return result;
		}


		/// <override></override>
		public override bool HasControlPointCapability(ControlPointId controlPointId, ControlPointCapabilities controlPointCapability) {
			// Customize control point capabilities:
			// Remove connection capability from the bottom center control point
			if (controlPointId == ControlPointIds.BottomCenterControlPoint && (controlPointCapability & ControlPointCapabilities.Connect) != 0)
				return false;
			else return base.HasControlPointCapability(controlPointId, controlPointCapability);
		}


		/// <override></override>
		protected internal DocumentSymbol(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		/// <override></override>
		protected internal DocumentSymbol(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		/// <override></override>
		protected override void CalcCaptionBounds(int index, out Rectangle captionBounds) {
			if (index != 0) throw new ArgumentOutOfRangeException("index");
			base.CalcCaptionBounds(index, out captionBounds);
			captionBounds.Height -= (2 * CalcTearOffHeight());
		}


		/// <override></override>
		protected override bool CalculatePath() {
			if (base.CalculatePath()) {
				int left = (int)Math.Round(-Width / 2f);
				int top = (int)Math.Round(-Height / 2f);
				int right = left + Width;
				int bottom = top + Height;
				int tearOffHeight = CalcTearOffHeight();

				Path.Reset();
				Path.StartFigure();
				Path.AddLine(left, top, left, bottom - tearOffHeight);
				Path.AddBezier(left, bottom - tearOffHeight, 0, bottom + (2 * tearOffHeight), 0, bottom - (4 * tearOffHeight), right, bottom - tearOffHeight);
				Path.AddLine(right, bottom - tearOffHeight, right, top);
				Path.AddLine(right, top, left, top);
				Path.CloseFigure();
				return true;
			} else return false;
		}


		private int CalcTearOffHeight() { return (int)Math.Round(Math.Min(20, Height / 8f)); }

	}


	public class OffpageConnectorSymbol : FlowChartRectangleBase {


		/// <summary>
		/// Provides constants for the control point id's of the shape.
		/// </summary>
		new public class ControlPointIds {
			/// <summary>ControlPointId of the top left control point.</summary>
			public const int TopLeftControlPoint = 1;
			/// <summary>ControlPointId of the top center control point.</summary>
			public const int TopCenterControlPoint = 2;
			/// <summary>ControlPointId of the middle left control point.</summary>
			public const int MiddleLeftControlPoint = 4;
			/// <summary>ControlPointId of the middle right control point.</summary>
			public const int MiddleRightControlPoint = 5;
			/// <summary>ControlPointId of the bottom left control point.</summary>
			public const int BottomLeftControlPoint = 6;
			/// <summary>ControlPointId of the bottom center control point.</summary>
			public const int BottomCenterControlPoint = 7;
			/// <summary>ControlPointId of the bottom right control point.</summary>
			public const int MiddleCenterControlPoint = 9;
		}


		/// <override></override>
		public override Shape Clone() {
			Shape result = new OffpageConnectorSymbol(Type, this.Template);
			result.CopyFrom(this);
			return result;
		}


		protected override Rectangle CalculateBoundingRectangle(bool tight) {
			if (tight) {
				CalcShapePoints(ref shapePoints);

				Matrix.Reset();
				Matrix.Translate(X, Y);
				Matrix.RotateAt(Geometry.TenthsOfDegreeToDegrees(Angle), Center, MatrixOrder.Append);
				Matrix.TransformPoints(shapePoints);

				Rectangle result;
				Geometry.CalcBoundingRectangle(shapePoints, out result);
				ShapeUtils.InflateBoundingRectangle(ref result, LineStyle);
				return result;
			} else return base.CalculateBoundingRectangle(tight);
		}


		/// <override></override>
		public override bool HasControlPointCapability(ControlPointId controlPointId, ControlPointCapabilities controlPointCapability) {
			// Customize control point capabilities
			switch (controlPointId) {
				case FlowChartRectangleBase.ControlPointIds.TopRightControlPoint:
				case FlowChartRectangleBase.ControlPointIds.BottomRightControlPoint:
					// These control points are not supported by this shape
					throw new ArgumentException("controlPointId");
				default: 
					return base.HasControlPointCapability(controlPointId, controlPointCapability);
			}
		}


		/// <override></override>
		protected override int ControlPointCount {
			// Remove unwanted control points
			get { return 7; }
		}


		protected internal OffpageConnectorSymbol(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal OffpageConnectorSymbol(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		/// <override></override>
		protected override void CalcControlPoints() {
			int left = (int)Math.Round(-Width / 2f);
			int top = (int)Math.Round(-Height / 2f);
			int right = left + Width;
			int bottom = top + Height;

			// top row
			ControlPoints[0].X = left;
			ControlPoints[0].Y = top;
			ControlPoints[1].X = 0;
			ControlPoints[1].Y = top;

			// middle row
			ControlPoints[2].X = left;
			ControlPoints[2].Y = 0;
			ControlPoints[3].X = right;
			ControlPoints[3].Y = 0;

			// bottom row
			ControlPoints[4].X = left;
			ControlPoints[4].Y = bottom;
			ControlPoints[5].X = 0;
			ControlPoints[5].Y = bottom;

			// Rotate point
			ControlPoints[6].X = 0;
			ControlPoints[6].Y = 0;
		}


		protected override ControlPointId GetControlPointId(int index) {
			switch (index) {
				case 0: return ControlPointIds.TopLeftControlPoint;
				case 1: return ControlPointIds.TopCenterControlPoint;
				case 2: return ControlPointIds.MiddleLeftControlPoint;
				case 3: return ControlPointIds.MiddleRightControlPoint;
				case 4: return ControlPointIds.BottomLeftControlPoint;
				case 5: return ControlPointIds.BottomCenterControlPoint;
				case 6: return ControlPointIds.MiddleCenterControlPoint;
				default: return ControlPointId.None;
			}
		}


		protected override int GetControlPointIndex(ControlPointId id) {
			switch (id) {
				case ControlPointIds.TopLeftControlPoint: return 0;
				case ControlPointIds.TopCenterControlPoint: return 1;
				case ControlPointIds.MiddleLeftControlPoint: return 2;
				case ControlPointIds.MiddleRightControlPoint: return 3;
				case ControlPointIds.BottomLeftControlPoint: return 4;
				case ControlPointIds.BottomCenterControlPoint: return 5;
				case ControlPointIds.MiddleCenterControlPoint: return 6;
				default: return -1;
			}
		}


		/// <override></override>
		protected override void CalcCaptionBounds(int index, out Rectangle captionBounds) {
			if (index != 0) throw new ArgumentOutOfRangeException("index");
			base.CalcCaptionBounds(index, out captionBounds);
			int top = (int)Math.Round(-Height / 2f);
			captionBounds.Y = top + (int)Math.Round(Height / 4f);
			captionBounds.Height = (int)Math.Round(Height / 2f);
			captionBounds.Width = (int)Math.Round(Width - Width / 4f);
		}


		/// <override></override>
		protected override bool CalculatePath() {
			if (base.CalculatePath()) {
				CalcShapePoints(ref shapePoints);

				Path.Reset();
				Path.StartFigure();
				Path.AddPolygon(shapePoints);
				Path.CloseFigure();
				return true;
			} else return false;
		}


		private void CalcShapePoints(ref Point[] points) {
			int left = (int)Math.Round(-Width / 2f);
			int top = (int)Math.Round(-Height / 2f);
			int right = left + Width;
			int bottom = top + Height;

			shapePoints[0].X = left;
			shapePoints[0].Y = top;
			shapePoints[1].X = 0;
			shapePoints[1].Y = top;
			shapePoints[2].X = right;
			shapePoints[2].Y = 0;
			shapePoints[3].X = 0;
			shapePoints[3].Y = bottom;
			shapePoints[4].X = left;
			shapePoints[4].Y = bottom;
		}


		// Fields
		Point[] shapePoints = new Point[5];
	}


	public class OnlineStorageSymbol : FlowChartRectangleBase {

		/// <override></override>
		public override Shape Clone() {
			Shape result = new OnlineStorageSymbol(Type, this.Template);
			result.CopyFrom(this);
			return result;
		}


		/// <override></override>
		public override bool HasControlPointCapability(ControlPointId controlPointId, ControlPointCapabilities controlPointCapability) {
			//if (controlPointId == FlowChartRectangleBase.MiddleRightControlPoint)
			//    // Remove the 'Connect' cappability from the middle right control point
			//    return (controlPointCapability & ControlPointCapabilities.Resize) != 0;
			//else
				return base.HasControlPointCapability(controlPointId, controlPointCapability);
		}


		/// <override></override>
		protected internal OnlineStorageSymbol(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		/// <override></override>
		protected internal OnlineStorageSymbol(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		/// <override></override>
		protected override Rectangle CalculateBoundingRectangle(bool tight) {
			if (tight) {
				int arcRadius = GetArcRadius();
				if (arcRadius > 0) {
					int left = X + (int)Math.Round(-Width / 2f);
					int top = Y + (int)Math.Round(-Height / 2f);
					int right = left + Width;
					int bottom = top + Height;

					float angle = Geometry.TenthsOfDegreeToDegrees(Angle);
					Point tl = Geometry.RotatePoint(X, Y, angle, left + arcRadius, top);
					Point tr = Geometry.RotatePoint(X, Y, angle, right, top);
					Point bl = Geometry.RotatePoint(X, Y, angle, left + arcRadius, bottom);
					Point br = Geometry.RotatePoint(X, Y, angle, right, bottom);
					Point arcCenter = Geometry.RotatePoint(X, Y, angle, left + arcRadius, Y);

					// Calculate intersection with ellipse
					Rectangle arcBounds;
					Geometry.CalcBoundingRectangleEllipse(arcCenter.X, arcCenter.Y, arcRadius + arcRadius, Height, angle, out arcBounds);

					Rectangle result = Rectangle.Empty;
					Geometry.CalcBoundingRectangle(tl, tr, bl, br, out result);
					result = Geometry.UniteRectangles(arcBounds, result);
					ShapeUtils.InflateBoundingRectangle(ref result, LineStyle);
					return result;
				} else return base.CalculateBoundingRectangle(tight);
			} else return base.CalculateBoundingRectangle(tight);
		}


		/// <override></override>
		protected override void CalcCaptionBounds(int index, out Rectangle captionBounds) {
			if (index != 0) throw new ArgumentOutOfRangeException("index");
			base.CalcCaptionBounds(index, out captionBounds);
			int left = (int)Math.Round(-Width / 2f);
			int top = (int)Math.Round(-Height / 2f);
			int arcRadius = GetArcRadius();
			captionBounds.X = left + arcRadius;
			captionBounds.Y = top;
			captionBounds.Width = Width - arcRadius - arcRadius;
			captionBounds.Height = Height;
		}


		/// <override></override>
		protected override bool CalculatePath() {
			if (base.CalculatePath()) {
				int left = (int)Math.Round(-Width / 2f);
				int top = (int)Math.Round(-Height / 2f);
				int right = left + Width;
				int bottom = top + Height;
				int arcRadius = GetArcRadius();

				Path.StartFigure();
				Path.AddArc(left, top, arcRadius + arcRadius, Height, 90, 180);
				Path.AddLine(left + arcRadius, top, right, top);
				Path.AddArc(right - arcRadius, top, arcRadius + arcRadius, Height, -90, -180);
				Path.AddLine(right, bottom, left + arcRadius, bottom);
				Path.CloseFigure();
				return true;
			}
			return false;
		}


		private int GetArcRadius() {
			return (Width < Height) ? Width / 4 : Height / 4;
		}

	}


	public class DrumStorageSymbol : FlowChartRectangleBase {

		/// <override></override>
		public override Shape Clone() {
			Shape result = new DrumStorageSymbol(Type, this.Template);
			result.CopyFrom(this);
			return result;
		}


		/// <override></override>
		protected override Rectangle CalculateBoundingRectangle(bool tight) {
			if (tight) {
				int arcRadius = GetArcRadius();
				if (arcRadius > 0) {
					int left = X + (int)Math.Round(-Width / 2f);
					int top = Y + (int)Math.Round(-Height / 2f);
					int right = left + Width;
					int bottom = top + Height;
					float angle = Geometry.TenthsOfDegreeToDegrees(Angle);

					Point leftCenter = Geometry.RotatePoint(X, Y, angle, left + arcRadius, Y);
					Point rightCenter = Geometry.RotatePoint(X, Y, angle, right - arcRadius, Y);

					Rectangle leftSideBounds;
					Geometry.CalcBoundingRectangleEllipse(leftCenter.X, leftCenter.Y, 2 * arcRadius, Height, angle, out leftSideBounds);
					ShapeUtils.InflateBoundingRectangle(ref leftSideBounds, LineStyle);

					Rectangle rightSideBounds;
					Geometry.CalcBoundingRectangleEllipse(rightCenter.X, rightCenter.Y, 2 * arcRadius, Height, angle, out rightSideBounds);
					ShapeUtils.InflateBoundingRectangle(ref rightSideBounds, LineStyle);

					return Geometry.UniteRectangles(leftSideBounds, rightSideBounds);
				} else return base.CalculateBoundingRectangle(tight);
			} else return base.CalculateBoundingRectangle(tight);
		}


		protected internal DrumStorageSymbol(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal DrumStorageSymbol(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		/// <override></override>
		protected override void CalcCaptionBounds(int index, out Rectangle captionBounds) {
			if (index != 0) throw new ArgumentOutOfRangeException("index");
			int left = (int)Math.Round(-Width / 2f);
			int top = (int)Math.Round(-Height / 2f);
			int arcRadius = GetArcRadius();
			captionBounds = Rectangle.Empty;
			captionBounds.X = left + arcRadius;
			captionBounds.Y = top;
			captionBounds.Width = Width - (2 * arcRadius);
			captionBounds.Height = Height;
		}


		/// <override></override>
		protected override bool CalculatePath() {
			if (base.CalculatePath()) {
				Path.Reset();
				int left = (int)Math.Round(-Width / 2f);
				int top = (int)Math.Round(-Height / 2f);
				int right = left + Width;
				int bottom = top + Height;
				int arcRadius = GetArcRadius();

				Path.StartFigure();
				Path.AddArc(right - (2 * arcRadius), top, arcRadius + arcRadius, Height, -90, -180);
				Path.AddLine(right - (2 * arcRadius), bottom, left + arcRadius, bottom);
				Path.AddArc(left, top, arcRadius + arcRadius, Height, 90, 180);
				Path.AddLine(left + arcRadius, top, right - arcRadius - arcRadius, top);
				Path.AddArc(right - (2 * arcRadius), top, arcRadius + arcRadius, Height, -90, 180);
				Path.AddArc(right - (2 * arcRadius), top, arcRadius + arcRadius, Height, 90, 180);
				Path.CloseFigure();
				Path.FillMode = System.Drawing.Drawing2D.FillMode.Winding;
				return true;
			} else return false;
		}


		private int GetArcRadius() {
			return (Width < Height) ? Width / 4 : Height / 4;
		}
	}


	public class DiskStorageSymbol : FlowChartRectangleBase {

		/// <override></override>
		public override Shape Clone() {
			Shape result = new DiskStorageSymbol(Type, this.Template);
			result.CopyFrom(this);
			return result;
		}


		/// <override></override>
		protected override Rectangle CalculateBoundingRectangle(bool tight) {
			if (tight) {
				int arcRadius = GetArcRadius();
				if (arcRadius > 0) {
					int left = X + (int)Math.Round(-Width / 2f);
					int top = Y + (int)Math.Round(-Height / 2f);
					int right = left + Width;
					int bottom = top + Height;
					float angle = Geometry.TenthsOfDegreeToDegrees(Angle);
					int halfLineWidth = (int)Math.Round(LineStyle.LineWidth / 2f, MidpointRounding.AwayFromZero);

					Point topCenter = Geometry.RotatePoint(X, Y, angle, X, top + arcRadius);
					Point bottomCenter = Geometry.RotatePoint(X, Y, angle, X, bottom - arcRadius);
					Rectangle topBounds;
					Geometry.CalcBoundingRectangleEllipse(topCenter.X, topCenter.Y, Width, 2 * arcRadius, angle, out topBounds);
					ShapeUtils.InflateBoundingRectangle(ref topBounds, LineStyle);
					Rectangle bottomBounds;
					Geometry.CalcBoundingRectangleEllipse(bottomCenter.X, bottomCenter.Y, Width, 2 * arcRadius, angle, out bottomBounds);
					ShapeUtils.InflateBoundingRectangle(ref bottomBounds, LineStyle);

					return Geometry.UniteRectangles(topBounds, bottomBounds);
				} else return base.CalculateBoundingRectangle(tight);
			} else return base.CalculateBoundingRectangle(tight);
		}


		protected internal DiskStorageSymbol(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal DiskStorageSymbol(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		/// <override></override>
		protected override void InitializeToDefault(IStyleSet styleSet) {
			base.InitializeToDefault(styleSet);
			Height = 40;
			Width = 40;
		}


		/// <override></override>
		protected override void CalcCaptionBounds(int index, out Rectangle captionBounds) {
			if (index != 0) throw new ArgumentOutOfRangeException("index");
			base.CalcCaptionBounds(index, out captionBounds);
			int left = (int)Math.Round(-Width / 2f);
			int top = (int)Math.Round(-Height / 2f);
			int arcRadius = GetArcRadius();
			captionBounds.X = left + arcRadius;
			captionBounds.Y = top;
			captionBounds.Width = Width - arcRadius - arcRadius;
			captionBounds.Height = Height;
		}


		/// <override></override>
		protected override bool CalculatePath() {
			if (base.CalculatePath()) {
				Path.Reset();
				int left = (int)Math.Round(-Width / 2f);
				int top = (int)Math.Round(-Height / 2f);
				int right = left + Width;
				int bottom = top + Height;
				int arcRadius = GetArcRadius();
				int arcSpacing = arcRadius;
				if (Height / arcRadius < 6)
					arcSpacing = Height / 6;

				Path.StartFigure();
				// Disk #1
				Path.AddArc(left, top, Width, arcRadius + arcRadius, 180, -180);
				Path.AddArc(left, top, Width, arcRadius + arcRadius, 0, 180);
				// Disk #2
				Path.AddArc(left, top + arcSpacing, Width, arcRadius + arcRadius, 180, -180);
				Path.AddArc(left, top + arcSpacing, Width, arcRadius + arcRadius, 0, 180);
				// Disk #3
				Path.AddArc(left, top + arcSpacing + arcSpacing, Width, arcRadius + arcRadius, 180, -180);
				Path.AddArc(left, top + arcSpacing + arcSpacing, Width, arcRadius + arcRadius, 0, 180);
				// Outline
				Path.AddLine(left, top + arcRadius, left, bottom - arcRadius);
				Path.AddArc(left, bottom - arcRadius - arcRadius, Width, arcRadius + arcRadius, 180, -180);
				Path.AddLine(right, bottom - arcRadius, right, top + arcRadius);
				Path.AddArc(left, top, Width, arcRadius + arcRadius, 0, -180);
				Path.CloseFigure();
				Path.FillMode = System.Drawing.Drawing2D.FillMode.Winding;
				return true;
			} else return false;
		}


		private int GetArcRadius() {
			return (int)Math.Round(Math.Min(Height / 4f, Width / 4f));
		}
	}


	public class PreparationSymbol : FlowChartRectangleBase {

		/// <override></override>
		public override Shape Clone() {
			Shape result = new PreparationSymbol(Type, this.Template);
			result.CopyFrom(this);
			return result;
		}


		protected override Rectangle CalculateBoundingRectangle(bool tight) {
			if (tight) {
				CalcShapePoints(ref shapePoints);

				Matrix.Reset();
				Matrix.Translate(X, Y);
				Matrix.RotateAt(Geometry.TenthsOfDegreeToDegrees(Angle), Center, MatrixOrder.Append);
				Matrix.TransformPoints(shapePoints);

				Rectangle result;
				Geometry.CalcBoundingRectangle(shapePoints, out result);
				ShapeUtils.InflateBoundingRectangle(ref result, LineStyle);
				return result;
			} else return base.CalculateBoundingRectangle(tight);
		}


		protected internal PreparationSymbol(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal PreparationSymbol(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		private int CalcEdgeInset() { return Height / 6; }


		/// <override></override>
		protected override void CalcControlPoints() {
			int left = (int)Math.Round(-Width / 2f);
			int top = (int)Math.Round(-Height / 2f);
			int right = left + Width;
			int bottom = top + Height;
			int edgeInset = CalcEdgeInset();

			// top row
			ControlPoints[0].X = left + edgeInset;
			ControlPoints[0].Y = top;
			ControlPoints[1].X = 0;
			ControlPoints[1].Y = top;
			ControlPoints[2].X = right - edgeInset;
			ControlPoints[2].Y = top;

			// middle row
			ControlPoints[3].X = left;
			ControlPoints[3].Y = 0;
			ControlPoints[4].X = right;
			ControlPoints[4].Y = 0;

			// bottom row
			ControlPoints[5].X = left + edgeInset;
			ControlPoints[5].Y = bottom;
			ControlPoints[6].X = 0;
			ControlPoints[6].Y = bottom;
			ControlPoints[7].X = right - edgeInset;
			ControlPoints[7].Y = bottom;

			ControlPoints[8].X = 0;
			ControlPoints[8].Y = 0;
		}


		/// <override></override>
		protected override bool CalculatePath() {
			if (base.CalculatePath()) {
				CalcShapePoints(ref shapePoints);

				Path.Reset();
				Path.StartFigure();
				Path.AddPolygon(shapePoints);
				return true;
			} else return false;
		}


		private void CalcShapePoints(ref Point[] points) {
			int left = (int)Math.Round(-Width / 2f);
			int top = (int)Math.Round(-Height / 2f);
			int right = left + Width;
			int bottom = top + Height;
			int edgeInset = CalcEdgeInset();

			points[0].X = left + edgeInset;
			points[0].Y = top;
			points[1].X = right - edgeInset;
			points[1].Y = top;
			points[2].X = right;
			points[2].Y = 0;
			points[3].X = right - edgeInset;
			points[3].Y = bottom;
			points[4].X = left + edgeInset;
			points[4].Y = bottom;
			points[5].X = left;
			points[5].Y = 0;
		}


		#region Fields
		private Point[] shapePoints = new Point[6];
		#endregion
	}


	public class ManualInputSymbol : FlowChartRectangleBase {

		/// <override></override>
		public override Shape Clone() {
			Shape result = new ManualInputSymbol(Type, this.Template);
			result.CopyFrom(this);
			return result;
		}


		/// <override></override>
		protected override Rectangle CalculateBoundingRectangle(bool tight) {
			if (tight) {
				int left = X + (int)Math.Round(-Width / 2f);
				int top = Y + (int)Math.Round(-Height / 2f);
				int right = left + Width;
				int bottom = top + Height;
				float angle = Geometry.TenthsOfDegreeToDegrees(Angle);

				Point tl = Geometry.RotatePoint(X, Y, angle, left, top + (Height / 4));
				Point tr = Geometry.RotatePoint(X, Y, angle, right, top);
				Point bl = Geometry.RotatePoint(X, Y, angle, left, bottom);
				Point br = Geometry.RotatePoint(X, Y, angle, right, bottom);

				Rectangle result;
				Geometry.CalcBoundingRectangle(tl, tr, bl, br, out result);
				ShapeUtils.InflateBoundingRectangle(ref result, LineStyle);
				return result;
			} else return base.CalculateBoundingRectangle(tight);
		}


		protected internal ManualInputSymbol(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal ManualInputSymbol(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		/// <override></override>
		protected override bool CalculatePath() {
			if (base.CalculatePath()) {
				int left = (int)Math.Round(-Width / 2f);
				int top = (int)Math.Round(-Height / 2f);
				int right = left + Width;
				int bottom = top + Height;

				shapePoints[0].X = left;
				shapePoints[0].Y = top + (Height / 4);
				shapePoints[1].X = right;
				shapePoints[1].Y = top;
				shapePoints[2].X = right;
				shapePoints[2].Y = bottom;
				shapePoints[3].X = left;
				shapePoints[3].Y = bottom;

				Path.Reset();
				Path.StartFigure();
				Path.AddPolygon(shapePoints);
				Path.CloseFigure();
				return true;
			} else return false;
		}


		#region Fields
		private Point[] shapePoints = new Point[4];
		#endregion
	}


	public class CoreSymbol : FlowChartRectangleBase {

		/// <override></override>
		public override Shape Clone() {
			Shape result = new CoreSymbol(Type, this.Template);
			result.CopyFrom(this);
			return result;
		}


		protected internal CoreSymbol(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal CoreSymbol(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		/// <override></override>
		protected override bool CalculatePath() {
			if (base.CalculatePath()) {
				Path.Reset();

				int left = (int)Math.Round(-Width / 2f);
				int top = (int)Math.Round(-Height / 2f);
				int sideBarSize = CalcSideBarSize();
				Rectangle rect = Rectangle.Empty;

				Path.StartFigure();
				rect.X = left;
				rect.Y = top;
				rect.Width = Width;
				rect.Height = Height;
				Path.AddRectangle(rect);

				rect.Width = sideBarSize;
				Path.AddRectangle(rect);

				rect.Width = Width;
				rect.Height = sideBarSize;
				Path.AddRectangle(rect);
				Path.CloseFigure();
				Path.FillMode = System.Drawing.Drawing2D.FillMode.Winding;
				return true;
			}
			return false;
		}


		private int CalcSideBarSize() { return (int)Math.Round(Math.Min(Width / 8f, Height / 8f)); }
	}


	public class DisplaySymbol : FlowChartRectangleBase {

		/// <override></override>
		public override Shape Clone() {
			Shape result = new DisplaySymbol(Type, this.Template);
			result.CopyFrom(this);
			return result;
		}


		/// <override></override>
		protected override Rectangle CalculateBoundingRectangle(bool tight) {
			Rectangle result = Geometry.InvalidRectangle;
			if (tight) {
				int left = X + (int)Math.Round(-Width / 2f);
				int top = Y + (int)Math.Round(-Height / 2f);
				int right = left + Width;
				int bottom = top + Height;
				float angle = Geometry.TenthsOfDegreeToDegrees(Angle);
				int arcRadius = CalcArcRadius();

				Point tl = Geometry.RotatePoint(X, Y, angle, left + arcRadius, top);
				Point ml = Geometry.RotatePoint(X, Y, angle, left, Y);
				Point bl = Geometry.RotatePoint(X, Y, angle, left + arcRadius, bottom);
				Point arcCenter = Geometry.RotatePoint(X, Y, angle, right - arcRadius, Y);

				Rectangle leftBounds;
				Geometry.CalcBoundingRectangle(tl, ml, bl, arcCenter, out leftBounds);
				if (arcRadius > 0) {
					Rectangle rightBounds;
					Geometry.CalcBoundingRectangleEllipse(arcCenter.X, arcCenter.Y, arcRadius + arcRadius, Height, angle, out rightBounds);
					result = Geometry.UniteRectangles(leftBounds, rightBounds);
				} else {
					Point tr = Geometry.RotatePoint(X, Y, angle, right, top);
					Point br = Geometry.RotatePoint(X, Y, angle, right, bottom);
					result = Geometry.UniteRectangles(tr.X, tr.Y, br.X, br.Y, leftBounds);
				}
			} else result = base.CalculateBoundingRectangle(tight);
			if (Geometry.IsValid(result))
				ShapeUtils.InflateBoundingRectangle(ref result, LineStyle);
			return result;
		}


		protected internal DisplaySymbol(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal DisplaySymbol(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		/// <override></override>
		protected override bool CalculatePath() {
			if (base.CalculatePath()) {
				int left = (int)Math.Round(-Width / 2f);
				int top = (int)Math.Round(-Height / 2f);
				int right = left + Width;
				int bottom = top + Height;
				int arcRadius = CalcArcRadius();

				shapePoints[0].X = left;
				shapePoints[0].Y = 0;
				shapePoints[1].X = left + arcRadius;
				shapePoints[1].Y = top;
				shapePoints[2].X = right;
				shapePoints[2].Y = top;
				shapePoints[3].X = right;
				shapePoints[3].Y = bottom;
				shapePoints[4].X = left + arcRadius;
				shapePoints[4].Y = bottom;

				Path.Reset();
				Path.StartFigure();
				Path.AddLine(shapePoints[0], shapePoints[1]);
				Path.AddLine(shapePoints[1].X, shapePoints[1].Y, shapePoints[2].X - arcRadius, shapePoints[2].Y);
				if (arcRadius > 0)
					Path.AddArc(shapePoints[2].X - arcRadius - arcRadius, shapePoints[2].Y, arcRadius + arcRadius, Height, -90, 180);
				else Path.AddLine(shapePoints[2], shapePoints[3]);
				Path.AddLine(shapePoints[3].X - arcRadius, shapePoints[3].Y, shapePoints[4].X, shapePoints[4].Y);
				Path.AddLine(shapePoints[4].X, shapePoints[4].Y, shapePoints[0].X, shapePoints[0].Y);
				Path.CloseFigure();
				return true;
			}
			return false;
		}


		private int CalcArcRadius() {
			return (int)Math.Round(Math.Min(Width / 4f, Height / 4f));
		}


		#region Fields
		private Point[] shapePoints = new Point[5];
		#endregion
	}


	public class TapeSymbol : FlowChartRectangleBase {

		/// <override></override>
		public override Shape Clone() {
			Shape result = new TapeSymbol(Type, this.Template);
			result.CopyFrom(this);
			return result;
		}


		protected internal TapeSymbol(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal TapeSymbol(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		/// <override></override>
		protected override void InitializeToDefault(IStyleSet styleSet) {
			base.InitializeToDefault(styleSet);
			Width = 60;
			Height = 40;
		}


		/// <override></override>
		protected override void CalcCaptionBounds(int index, out Rectangle captionBounds) {
			if (index != 0) throw new ArgumentOutOfRangeException("index");
			base.CalcCaptionBounds(index, out captionBounds);
			int tearOffSize = CalcTearOffSize();
			captionBounds.Y += tearOffSize;
			captionBounds.Height -= (2 * tearOffSize);
		}


		/// <override></override>
		protected override bool CalculatePath() {
			if (base.CalculatePath()) {
				Path.Reset();
				int tearOffSize = CalcTearOffSize();
				int left = (int)Math.Round(-Width / 2f);
				int top = (int)Math.Round((-Height / 2f) + tearOffSize);
				int right = left + Width;
				int bottom = (int)Math.Round((-Height / 2f) + Height - tearOffSize);

				Path.StartFigure();
				Path.AddBezier(left, top,
					0, top + (3 * tearOffSize),
					0, top - (3 * tearOffSize),
					right, top);
				Path.AddLine(right, top, right, bottom);
				Path.AddBezier(right, bottom,
					0, bottom - (3 * tearOffSize),
					0, bottom + (3 * tearOffSize),
					left, bottom);
				Path.AddLine(left, bottom, left, top);
				Path.CloseFigure();
				return true;
			}
			return false;
		}


		private int CalcTearOffSize() {
			return (int)Math.Round(Math.Min(10, Height / 8f));
		}
	}


	public class ManualOperationSymbol : FlowChartRectangleBase {

		/// <override></override>
		public override Shape Clone() {
			Shape result = new ManualOperationSymbol(Type, this.Template);
			result.CopyFrom(this);
			return result;
		}


		/// <override></override>
		protected override Rectangle CalculateBoundingRectangle(bool tight) {
			if (tight) {
				int left = X + (int)Math.Round(-Width / 2f);
				int top = Y + (int)Math.Round(-Height / 2f);
				int right = left + Width;
				int bottom = top + Height;
				int edgeInset = CalcEdgeInset();
				float angle = Geometry.TenthsOfDegreeToDegrees(Angle);

				Point tl = Geometry.RotatePoint(X, Y, angle, left, top);
				Point tr = Geometry.RotatePoint(X, Y, angle, right, top);
				Point bl = Geometry.RotatePoint(X, Y, angle, left + edgeInset, bottom);
				Point br = Geometry.RotatePoint(X, Y, angle, right - edgeInset, bottom);

				Rectangle result;
				Geometry.CalcBoundingRectangle(tl, tr, bl, br, out result);
				ShapeUtils.InflateBoundingRectangle(ref result, LineStyle);
				return result;
			} else return base.CalculateBoundingRectangle(tight);
		}


		/// <override></override>
		protected internal ManualOperationSymbol(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		/// <override></override>
		protected internal ManualOperationSymbol(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		/// <override></override>
		protected override bool CalculatePath() {
			if (base.CalculatePath()) {
				int left = (int)Math.Round(-Width / 2f);
				int top = (int)Math.Round(-Height / 2f);
				int right = left + Width;
				int bottom = top + Height;
				int edgeInset = CalcEdgeInset();

				shapePoints[0].X = left;
				shapePoints[0].Y = top;
				shapePoints[1].X = right;
				shapePoints[1].Y = top;
				shapePoints[2].X = right - edgeInset;
				shapePoints[2].Y = bottom;
				shapePoints[3].X = left + edgeInset;
				shapePoints[3].Y = bottom;

				Path.Reset();
				Path.StartFigure();
				Path.AddPolygon(shapePoints);
				Path.CloseFigure();
				return true;
			}
			return false;
		}


		private int CalcEdgeInset() { return (int)Math.Round(Width / 4f); }


		#region Fields
		private Point[] shapePoints = new Point[4];
		#endregion
	}


	public class CollateSymbol : FlowChartRectangleBase {

		/// <override></override>
		public override Shape Clone() {
			Shape result = new CollateSymbol(Type, this.Template);
			result.CopyFrom(this);
			return result;
		}


		protected internal CollateSymbol(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal CollateSymbol(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		/// <override></override>
		protected override bool ContainsPointCore(int x, int y) {
			int left = (int)Math.Round(-Width / 2f);
			int top = (int)Math.Round(-Height / 2f);
			int right = left + Width;
			int bottom = top + Height;

			shapePointBuffer[0].X = left;
			shapePointBuffer[0].Y = top;
			shapePointBuffer[1].X = right;
			shapePointBuffer[1].Y = top;
			shapePointBuffer[2].X = right;
			shapePointBuffer[2].Y = bottom;
			shapePointBuffer[3].X = left;
			shapePointBuffer[3].Y = bottom;
			Matrix.Reset();
			Matrix.Translate(X, Y);
			if (Angle != 0) Matrix.RotateAt(Geometry.TenthsOfDegreeToDegrees(Angle), Center, MatrixOrder.Append);
			Matrix.TransformPoints(shapePointBuffer);

			if (Geometry.TriangleContainsPoint(shapePointBuffer[0], shapePointBuffer[1], Center, x, y)
				|| Geometry.TriangleContainsPoint(Center, shapePointBuffer[2], shapePointBuffer[3], x, y))
				return true;
			else return false;
		}


		/// <override></override>
		protected override void InitializeToDefault(IStyleSet styleSet) {
			base.InitializeToDefault(styleSet);
			Height = 40;
			Width = 40;
		}


		/// <override></override>
		protected override bool CalculatePath() {
			if (base.CalculatePath()) {
				Path.Reset();
				int left = (int)Math.Round(-Width / 2f);
				int top = (int)Math.Round(-Height / 2f);
				int right = left + Width;
				int bottom = top + Height;

				Path.AddLine(left, top, right, top);
				Path.AddLine(right, top, left, bottom);
				Path.AddLine(left, bottom, right, bottom);
				Path.AddLine(right, bottom, left, top);
				Path.CloseFigure();
				return true;
			}
			return false;
		}


		private Point[] shapePointBuffer = new Point[4];
	}


	public class CardSymbol : FlowChartRectangleBase {

		/// <override></override>
		public override Shape Clone() {
			Shape result = new CardSymbol(Type, this.Template);
			result.CopyFrom(this);
			return result;
		}


		protected override Rectangle CalculateBoundingRectangle(bool tight) {
			if (tight) {
				int left = X + (int)Math.Round(-Width / 2f);
				int top = Y + (int)Math.Round(-Height / 2f);
				int right = left + Width;
				int bottom = top + Height;
				int edgeSize = CalcEdgeSize();
				float angle = Geometry.TenthsOfDegreeToDegrees(Angle);

				Point itl = Geometry.RotatePoint(X, Y, angle, left + edgeSize, top);
				Point tr = Geometry.RotatePoint(X, Y, angle, right, top);
				Point ibl = Geometry.RotatePoint(X, Y, angle, left + edgeSize, bottom);
				Point br = Geometry.RotatePoint(X, Y, angle, right, bottom);

				Point otl = Geometry.RotatePoint(X, Y, angle, left, top + edgeSize);
				Point obl = Geometry.RotatePoint(X, Y, angle, left, bottom);

				Rectangle result;
				Geometry.CalcBoundingRectangle(itl, tr, ibl, br, out result);
				result = Geometry.UniteWithRectangle(otl, result);
				ShapeUtils.InflateBoundingRectangle(ref result, LineStyle);
				return Geometry.UniteWithRectangle(obl, result);
			} else return base.CalculateBoundingRectangle(tight);
		}


		protected internal CardSymbol(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal CardSymbol(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		private int CalcEdgeSize() { return (int)Math.Round(Math.Min(Width / 4f, Height / 4f)); }


		/// <override></override>
		protected override bool CalculatePath() {
			if (base.CalculatePath()) {
				Path.Reset();
				if (Width > 0 && Height > 0) {
					int left = (int)Math.Round(-Width / 2f);
					int top = (int)Math.Round(-Height / 2f);
					int right = left + Width;
					int bottom = top + Height;
					int edgeSize = CalcEdgeSize();

					Path.AddLine(left + edgeSize, top, right, top);
					Path.AddLine(right, top, right, bottom);
					Path.AddLine(right, bottom, left, bottom);
					Path.AddLine(left, bottom, left, top + edgeSize);
					Path.CloseFigure();
					return true;
				} else return false;
			} else return false;
		}
	}

}
