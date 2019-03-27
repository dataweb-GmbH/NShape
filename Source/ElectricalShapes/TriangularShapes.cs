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

using Dataweb.NShape.Advanced;


namespace Dataweb.NShape.ElectricalShapes {

	public class FeederSymbol : ElectricalTriangleBase {

		/// <override></override>
		public override Shape Clone() {
			Shape result = new FeederSymbol(Type, this.Template);
			result.CopyFrom(this);
			return result;
		}


		protected internal FeederSymbol(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal FeederSymbol(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}
	}


	public class RectifierSymbol : ElectricalTriangleBase {

		/// <override></override>
		public override Shape Clone() {
			Shape result = new RectifierSymbol(Type, this.Template);
			result.CopyFrom(this);
			return result;
		}


		/// <override></override>
		protected override bool CalculatePath() {
			if (base.CalculatePath()) {
				int left = (int)Math.Round(-Width * CenterPosFactorX);
				int top = (int)Math.Round(-Height * CenterPosFactorY);
				int right = left + Width;
				int bottom = top + Height;

				Path.StartFigure();
				Path.AddLine(left, top, right, top);
				Path.CloseFigure();
				return true;
			}
			return false;
		}


		protected override Rectangle CalculateBoundingRectangle(bool tight) {
			// Tight and loose bounding rectangles are equal
			Rectangle result = Geometry.InvalidRectangle;
			if (Width >= 0 && Height >= 0) {
				int left = (int)Math.Round(-Width / 2f);
				int top = (int)Math.Round(-Height * CenterPosFactorY);
				int right = left + Width;
				int bottom = (int)Math.Round(Height - (Height * CenterPosFactorY));

				if (Angle == 0 || Angle == 1800) {
					result.X = X + left;
					result.Y = (Angle == 0) ? Y + top : Y - bottom;
					result.Width = right - left;
					result.Height = bottom - top;
				} else if (Angle == 900 || Angle == 2700) {
					result.X = (Angle == 900) ? X - bottom : result.X = X + top;
					result.Y = Y + left;
					result.Width = bottom - top;
					result.Height = right - left;
				} else {
					float angleDeg = Geometry.TenthsOfDegreeToDegrees(Angle);
					int x1, y1, x2, y2, x3, y3, x4, y4;

					x1 = X + left; y1 = Y + top;
					x2 = X + right; y2 = Y + top;
					x3 = X + right; y3 = Y + bottom;
					x4 = X + left; y4 = Y + bottom;
					Geometry.RotatePoint(X, Y, angleDeg, ref x1, ref y1);
					Geometry.RotatePoint(X, Y, angleDeg, ref x2, ref y2);
					Geometry.RotatePoint(X, Y, angleDeg, ref x3, ref y3);
					Geometry.RotatePoint(X, Y, angleDeg, ref x4, ref y4);

					result.X = Math.Min(Math.Min(x1, x2), Math.Min(x3, x4));
					result.Y = Math.Min(Math.Min(y1, y2), Math.Min(y3, y4));
					result.Width = Math.Max(Math.Max(x1, x2), Math.Max(x3, x4)) - result.X;
					result.Height = Math.Max(Math.Max(y1, y2), Math.Max(y3, y4)) - result.Y;
				}
				ShapeUtils.InflateBoundingRectangle(ref result, LineStyle);
			}
			return result;
		}


		protected internal RectifierSymbol(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal RectifierSymbol(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}
	}

}
