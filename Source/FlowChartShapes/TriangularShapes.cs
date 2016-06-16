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
using System.Drawing;

using Dataweb.NShape.Advanced;


namespace Dataweb.NShape.FlowChartShapes {

	public class ExtractSymbol : FlowChartTriangleBase {

		/// <override></override>
		public override Shape Clone() {
			Shape result = new ExtractSymbol(Type, this.Template);
			result.CopyFrom(this);
			return result;
		}


		protected internal ExtractSymbol(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal ExtractSymbol(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		/// <override></override>
		protected override void CalcControlPoints() {
			base.CalcControlPoints();
			
			// Add additional connection points
			int left = (int)Math.Round(-Width * CenterPosFactorX);
			int right = left + Width;
			int bottom = (int)Math.Round(Height * (1 - CenterPosFactorY));

			ControlPoints[5].X = left + (int)Math.Round(Width / 4f);
			ControlPoints[5].Y = bottom;
			ControlPoints[6].X = right - (int)Math.Round(Width / 4f);
			ControlPoints[6].Y = bottom;
		}

	}


	public class MergeSymbol : FlowChartTriangleBase {

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
			/// <summary>ControlPointId of the bottom center control point.</summary>
			public const int BottomCenterControlPoint = 4;
			/// <summary>ControlPointId of the center control point.</summary>
			public const int BalancePointControlPoint = 5;
			/// <summary>ControlPointId of the connection point on the left side of the base.</summary>
			public const int LeftConnectionPoint = 6;
			/// <summary>ControlPointId of the connection point on the right side of the base.</summary>
			public const int RightConnectionPoint = 7;
		}


		/// <override></override>
		public override Shape Clone() {
			Shape result = new MergeSymbol(Type, this.Template);
			result.CopyFrom(this);
			return result;
		}


		/// <override></override>
		public override bool HasControlPointCapability(ControlPointId controlPointId, ControlPointCapabilities controlPointCapability) {
			// As the id's of the inherited control points (topCenter and BottomCenter) are different from the base class' ids,
			// we have to re-implement the whole switch block.
			switch (controlPointId) {
				case ControlPointIds.BottomCenterControlPoint:
				case ControlPointIds.TopCenterControlPoint:
					return ((controlPointCapability & ControlPointCapabilities.Resize) != 0
						|| ((controlPointCapability & ControlPointCapabilities.Connect) != 0 && IsConnectionPointEnabled(controlPointId)));
				case ControlPointIds.LeftConnectionPoint:
				case ControlPointIds.RightConnectionPoint:
					return ((controlPointCapability & ControlPointCapabilities.Connect) != 0 && IsConnectionPointEnabled(controlPointId));
				case ControlPointIds.TopLeftControlPoint:
				case ControlPointIds.TopRightControlPoint:
					return ((controlPointCapability & ControlPointCapabilities.Resize) != 0);
				case ControlPointIds.BalancePointControlPoint:
					// The connection point capability of the center point (NOT including the "to-Shape" connection capability) is removed herby
					return ((controlPointCapability & ControlPointCapabilities.Rotate) != 0
						|| (controlPointCapability & ControlPointCapabilities.Reference) != 0);
				default:
					return base.HasControlPointCapability(controlPointId, controlPointCapability);
			}
		}


		protected internal MergeSymbol(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal MergeSymbol(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		/// <override></override>
		protected override bool MovePointByCore(ControlPointId pointId, float transformedDeltaX, float transformedDeltaY, float sin, float cos, ResizeModifiers modifiers) {
			bool result = true;
			int dx = 0, dy = 0;
			int width = Width;
			int height = Height;
			switch ((int)pointId) {
				case ControlPointIds.TopLeftControlPoint:
					if (!Geometry.MoveRectangleTopLeft(width, height, 0, 0, CenterPosFactorX, CenterPosFactorY, DivFactorX, DivFactorY,
													transformedDeltaX, transformedDeltaY, cos, sin, modifiers,
													out dx, out dy, out width, out height))
						result = false;
					break;

				case ControlPointIds.TopCenterControlPoint:
					result = (transformedDeltaX == 0);
					if (!Geometry.MoveRectangleTop(width, height, 0, CenterPosFactorX, CenterPosFactorY, DivFactorX, DivFactorY,
													transformedDeltaX, transformedDeltaY, cos, sin, modifiers,
													out dx, out dy, out width, out height))
						result = false;
					break;

				case ControlPointIds.TopRightControlPoint:
					if (!Geometry.MoveRectangleTopRight(width, height, 0, 0, CenterPosFactorX, CenterPosFactorY, DivFactorX, DivFactorY,
													transformedDeltaX, transformedDeltaY, cos, sin, modifiers,
													out dx, out dy, out width, out height))
						result = false;
					break;

				case ControlPointIds.BottomCenterControlPoint:
					result = (transformedDeltaX == 0);
					if (!Geometry.MoveRectangleBottom(width, height, 0, CenterPosFactorX, CenterPosFactorY,
													DivFactorX, DivFactorY, transformedDeltaX, transformedDeltaY, cos, sin, modifiers,
													out dx, out dy, out width, out height))
						result = false;
					break;

				default:
					break;
			}
			Width = width;
			Height = height;
			MoveByCore(dx, dy);
			ControlPointsHaveMoved();

			return result;
		}


		/// <override></override>
		protected override Rectangle CalculateBoundingRectangle(bool tight) {
			// tight and loose fitting bounding rectangles are equal
			if (Angle == 0 || Angle == 900 || Angle == 1800 || Angle == 2700)
				return base.CalculateBoundingRectangle(tight);
			else {
				// Calculate tight fitting bounding rectangle
				Rectangle result = Geometry.InvalidRectangle;
				float angleDeg = Geometry.TenthsOfDegreeToDegrees(Angle);
				int x1, y1, x2, y2, x3, y3;
				int left = (int)Math.Round(X - (Width / 2f));
				int top = Y - (int)Math.Round(Height * CenterPosFactorY);
				int right = left + Width;
				int bottom = top + Height;
				x1 = left; y1 = top;
				x2 = right; y2 = top;
				x3 = X; y3 = bottom;
				Geometry.RotatePoint(X, Y, angleDeg, ref x1, ref y1);
				Geometry.RotatePoint(X, Y, angleDeg, ref x2, ref y2);
				Geometry.RotatePoint(X, Y, angleDeg, ref x3, ref y3);

				result.X = Math.Min(Math.Min(x1, x2), Math.Min(x1, x3));
				result.Y = Math.Min(Math.Min(y1, y2), Math.Min(y1, y3));
				result.Width = Math.Max(Math.Max(x1, x2), Math.Max(x1, x3)) - result.X;
				result.Height = Math.Max(Math.Max(y1, y2), Math.Max(y1, y3)) - result.Y;
				ShapeUtils.InflateBoundingRectangle(ref result, LineStyle);
				return result;
			}
		}


		/// <override></override>
		protected override void CalcControlPoints() {
			int left = (int)Math.Round(-Width / 2f);
			int top = (int)Math.Round(-Height * CenterPosFactorY);
			int right = left + Width;
			int bottom = top + Height;

			ControlPoints[0].X = left;
			ControlPoints[0].Y = top;
			ControlPoints[1].X = 0;
			ControlPoints[1].Y = top;
			ControlPoints[2].X = right;
			ControlPoints[2].Y = top;
			ControlPoints[3].X = 0;
			ControlPoints[3].Y = bottom;
			ControlPoints[4].X = 0;
			ControlPoints[4].Y = 0;
			ControlPoints[5].X = left + (int)Math.Round(Width * (CenterPosFactorX / 2f));
			ControlPoints[5].Y = top;
			ControlPoints[6].X = right - (int)Math.Round(Width * (CenterPosFactorX / 2f));
			ControlPoints[6].Y = top;
		}


		/// <override></override>
		protected override void CalculateShapePoints() {
			int left = (int)Math.Round(-Width * CenterPosFactorX);
			int top = (int)Math.Round(-Height * CenterPosFactorY);
			int right = left + Width;
			int bottom = top + Height;

			shapePoints[0].X = left;
			shapePoints[0].Y = top;
			shapePoints[1].X = right;
			shapePoints[1].Y = top;
			shapePoints[2].X = 0;
			shapePoints[2].Y = bottom;
		}


		/// <override></override>
		protected override float CenterPosFactorY { get { return centerPosFactorY; } }


		#region Fields
		private const float centerPosFactorY = 0.3333333333f;

		Point[] shapeBuffer = new Point[3];

		#endregion
	}


	public class OfflineStorageSymbol : MergeSymbol {

		/// <override></override>
		public override Shape Clone() {
			Shape result = new OfflineStorageSymbol(Type, this.Template);
			result.CopyFrom(this);
			return result;
		}


		protected internal OfflineStorageSymbol(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal OfflineStorageSymbol(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		/// <override></override>
		protected override bool CalculatePath() {
			if (base.CalculatePath()) {
				int left = (int)Math.Round(-Width * CenterPosFactorX);
				int top = (int)Math.Round(-Height * CenterPosFactorY);
				int right = left + Width;
				int bottom = top + Height;

				shapePoints[0].X = left;
				shapePoints[0].Y = top;
				shapePoints[1].X = right;
				shapePoints[1].Y = top;
				shapePoints[2].X = 0;
				shapePoints[2].Y = bottom;

				int x1, x2, y1, y2;
				int a1, b1, c1, a2, b2, c2, a, b, c;
				Geometry.CalcLine(0, bottom, left, top, out a1, out b1, out c1);
				Geometry.CalcLine(0, bottom, right, top, out a2, out b2, out c2);
				Geometry.CalcLine(left, bottom - (Height / 3), right, bottom - (Height / 3), out a, out b, out c);
				Geometry.IntersectLines(a, b, c, a1, b1, c1, out x1, out y1);
				Geometry.IntersectLines(a, b, c, a2, b2, c2, out x2, out y2);

				Path.Reset();
				Path.StartFigure();
				Path.AddPolygon(shapePoints);
				Path.CloseFigure();
				Path.StartFigure();
				Path.AddLine(x1, y1, x2, y2);
				Path.CloseFigure();
				return true;
			} else return false;
		}
	}

}
