/******************************************************************************
  Copyright 2009-2014 dataweb GmbH
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


namespace Dataweb.NShape.FlowChartShapes {

	public class DecisionSymbol : FlowChartDiamondBase {

		/// <override></override>
		public override Shape Clone() {
			Shape result = new DecisionSymbol(Type, this.Template);
			result.CopyFrom(this);
			return result;
		}


		/// <override></override>
		protected internal DecisionSymbol(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		/// <override></override>
		protected internal DecisionSymbol(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		#region Fields
		Point[] shapeBuffer = new Point[4];
		#endregion
	}


	public class SortSymbol : FlowChartDiamondBase {

		/// <override></override>
		public override Shape Clone() {
			Shape result = new SortSymbol(Type, this.Template);
			result.CopyFrom(this);
			return result;
		}


		protected internal SortSymbol(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal SortSymbol(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		/// <override></override>
		protected override void InitializeToDefault(IStyleSet styleSet) {
			base.InitializeToDefault(styleSet);
			Width = 40;
			Height = 40;
		}


		/// <override></override>
		protected override bool CalculatePath() {
			if (base.CalculatePath()) {
				Path.Reset();
				if (Width > 0 && Height > 0) {
					int left = (int)Math.Round(-Width / 2f);
					int top = (int)Math.Round(-Height / 2f);
					int right = left + Width;
					int bottom = top + Height;

					pointBuffer[0].X = 0;
					pointBuffer[0].Y = top;
					pointBuffer[1].X = right;
					pointBuffer[1].Y = 0;
					pointBuffer[2].X = 0;
					pointBuffer[2].Y = bottom;
					pointBuffer[3].X = left;
					pointBuffer[3].Y = 0;

					Path.StartFigure();
					Path.AddPolygon(pointBuffer);
					Path.CloseFigure();

					// refresh edge positions
					Path.StartFigure();
					Path.AddLine(left, 0, right, 0);
					Path.CloseFigure();
					Path.FillMode = System.Drawing.Drawing2D.FillMode.Winding;
					return true;
				} else return false;
			} else return false;
		}

	}


	public class CommLinkSymbol : FlowChartDiamondBase {


		/// <summary>
		/// Provides constants for the control point id's of the shape.
		/// </summary>
		new public class ControlPointIds {
			/// <summary>ControlPointId of the top control point.</summary>
			public const int TopCenterControlPoint = 1;
			/// <summary>ControlPointId of the left control point.</summary>
			public const int MiddleLeftControlPoint = 2;
			/// <summary>ControlPointId of the right control point.</summary>
			public const int MiddleRightControlPoint = 3;
			/// <summary>ControlPointId of the bottom control point.</summary>
			public const int BottomCenterControlPoint = 4;
			/// <summary>ControlPointId of the middle control point.</summary>
			public const int MiddleCenterControlPoint = 5;
		}		


		/// <override></override>
		public override Shape Clone() {
			Shape result = new CommLinkSymbol(Type, this.Template);
			result.CopyFrom(this);
			return result;
		}


		/// <override></override>
		public override bool HasControlPointCapability(ControlPointId controlPointId, ControlPointCapabilities controlPointCapability) {
			switch (controlPointId) {
				case ControlPointIds.TopCenterControlPoint:
				case ControlPointIds.MiddleLeftControlPoint:
				case ControlPointIds.MiddleRightControlPoint:
				case ControlPointIds.BottomCenterControlPoint:
					return ((controlPointCapability & ControlPointCapabilities.Resize) != 0);
				case ControlPointIds.MiddleCenterControlPoint:
					return ((controlPointCapability & ControlPointCapabilities.Reference) != 0
						|| (controlPointCapability & ControlPointCapabilities.Rotate) != 0
						|| ((controlPointCapability & ControlPointCapabilities.Connect) != 0 && IsConnectionPointEnabled(controlPointId)));
				case ControlPointId.Reference:
					return ((controlPointCapability & ControlPointCapabilities.Reference) != 0
						|| (controlPointCapability & ControlPointCapabilities.Rotate) != 0);
				default:
					return false;
			}
		}


		protected internal CommLinkSymbol(ShapeType shapeType, Template template)
			: base(shapeType, template) {
			pointBuffer = new Point[6];
		}


		protected internal CommLinkSymbol(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
			pointBuffer = new Point[6];
		}


		/// <override></override>
		protected override int ControlPointCount {
			get { return 5; }
		}


		/// <override></override>
		protected override void InitializeToDefault(IStyleSet styleSet) {
			base.InitializeToDefault(styleSet);
			pointBuffer = new Point[6];
			Height = 60;
			Width = 20;
		}


		/// <override></override>
		protected override bool MovePointByCore(ControlPointId pointId, float transformedDeltaX, float transformedDeltaY, float sin, float cos, ResizeModifiers modifiers) {
			bool result = true;
			int dx = 0, dy = 0;
			int width = Width;
			int height = Height;
			switch ((int)pointId) {
				case ControlPointIds.TopCenterControlPoint:
					result = (transformedDeltaX == 0);
					if (!Geometry.MoveRectangleTop(width, height, transformedDeltaX, transformedDeltaY, cos, sin, modifiers, out dx, out dy, out width, out height))
						result = false;
					break;
				case ControlPointIds.MiddleLeftControlPoint:
					result = (transformedDeltaY == 0);
					if (!Geometry.MoveRectangleLeft(width, height, transformedDeltaX, transformedDeltaY, cos, sin, modifiers, out dx, out dy, out width, out height))
						result = false;
					break;
				case ControlPointIds.MiddleRightControlPoint:
					result = (transformedDeltaY == 0);
					if (!Geometry.MoveRectangleRight(width, height, transformedDeltaX, transformedDeltaY, cos, sin, modifiers, out dx, out dy, out width, out height))
						result = false;
					break;
				case ControlPointIds.BottomCenterControlPoint:
					result = (transformedDeltaX == 0);
					if (!Geometry.MoveRectangleBottom(width, height, transformedDeltaX, transformedDeltaY, cos, sin, modifiers, out dx, out dy, out width, out height))
						result = false;
					break;
				default:
					break;
			}
			if (result) {
				Width = width;
				Height = height;
				X += dx;
				Y += dy;
			}
			ControlPointsHaveMoved();
			return result;
		}


		/// <override></override>
		protected override Rectangle CalculateBoundingRectangle(bool tight) {
			Rectangle result;
			CalculalteTranslatedShapePoints();
			Geometry.CalcBoundingRectangle(pointBuffer, out result);
			ShapeUtils.InflateBoundingRectangle(ref result, LineStyle);
			return result;
		}


		/// <override></override>
		protected override bool ContainsPointCore(int x, int y) {
			CalculalteTranslatedShapePoints();
			bool result = Geometry.PolygonContainsPoint(pointBuffer, x, y);
			if (!result) {
				pt.X = x;
				pt.Y = y;
				if (DisplayService != null)
					DisplayService.Invalidate(new Rectangle(short.MinValue, short.MinValue, short.MaxValue * 2, short.MaxValue * 2));
			} else pt = Point.Empty;
			return result;
		}


		/// <override></override>
		protected override void CalcControlPoints() {
			int left = (int)Math.Round(-Width / 2f);
			int top = (int)Math.Round(-Height / 2f);
			int right = left + Width;
			int bottom = top + Height;

			ControlPoints[0].X = 0;
			ControlPoints[0].Y = top;
			ControlPoints[1].X = left;
			ControlPoints[1].Y = 0;
			ControlPoints[2].X = right;
			ControlPoints[2].Y = 0;
			ControlPoints[3].X = 0;
			ControlPoints[3].Y = bottom;
			ControlPoints[4].X = 0;
			ControlPoints[4].Y = 0;
		}


		/// <override></override>
		protected override bool CalculatePath() {
			if (base.CalculatePath()) {
				CalculateShapePoints();

				Path.Reset();
				Path.StartFigure();
				Path.AddPolygon(pointBuffer);
				Path.CloseFigure();
				return true;
			}
			return false;
		}


		private void CalculateShapePoints() {
			int left = (int)Math.Round(-Width / 2f);
			int top = (int)Math.Round(-Height / 2f);
			int right = left + Width;
			int bottom = top + Height;
			int w = (int)Math.Round(Width / 8f);
			int h = (int)Math.Round(Height / 16f);

			pointBuffer[0].X = 0;
			pointBuffer[0].Y = top;
			pointBuffer[1].X = right;
			pointBuffer[1].Y = 0 + h;
			pointBuffer[2].X = 0 - w;
			pointBuffer[2].Y = 0 + (int)Math.Round(h / 2f);
			pointBuffer[3].X = 0;
			pointBuffer[3].Y = bottom;
			pointBuffer[4].X = left;
			pointBuffer[4].Y = 0 - h;
			pointBuffer[5].X = 0 + w;
			pointBuffer[5].Y = 0 - (int)Math.Round(h / 2f);
		}


		private void CalculalteTranslatedShapePoints() {
			CalculateShapePoints();
			Matrix.Reset();
			Matrix.Translate(X, Y, MatrixOrder.Prepend);
			Matrix.RotateAt(Geometry.TenthsOfDegreeToDegrees(Angle), Center, MatrixOrder.Append);
			Matrix.TransformPoints(pointBuffer);
		}


		// Field
		private Point pt;

	}

}
