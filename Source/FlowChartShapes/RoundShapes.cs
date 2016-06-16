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

	public class ConnectorSymbol : FlowChartCircleBase {

		/// <override></override>
		public override Shape Clone() {
			Shape result = new ConnectorSymbol(Type, this.Template);
			result.CopyFrom(this);
			return result;
		}


		/// <override></override>
		protected internal ConnectorSymbol(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		/// <override></override>
		protected internal ConnectorSymbol(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		/// <override></override>
		protected override bool CalculatePath() {
			if (base.CalculatePath()) {
				Path.Reset();
				int left = (int)Math.Round(-Diameter / 2f);
				int top = (int)Math.Round(-Diameter / 2f);
				Path.StartFigure();
				Path.AddEllipse(left, top, Diameter, Diameter);
				Path.CloseFigure();
				return true;
			} else return false;
		}
	}


	public class TapeStorageSymbol : FlowChartCircleBase {

		/// <override></override>
		public override Shape Clone() {
			Shape result = new TapeStorageSymbol(Type, this.Template);
			result.CopyFrom(this);
			return result;
		}


		/// <override></override>
		protected override Rectangle CalculateBoundingRectangle(bool tight) {
			if (tight) {
				Rectangle result = base.CalculateBoundingRectangle(tight);
				Rectangle r = Rectangle.Empty;
				r.Location = Point.Round(Geometry.RotatePoint(X, Y, Geometry.TenthsOfDegreeToDegrees(Angle), X + (DiameterInternal / 2f), Y + (DiameterInternal / 2f)));
				ShapeUtils.InflateBoundingRectangle(ref r, LineStyle);
				return Geometry.UniteRectangles(r, result);
			} else return base.CalculateBoundingRectangle(tight);
		}


		protected internal TapeStorageSymbol(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal TapeStorageSymbol(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		/// <override></override>
		protected override bool IntersectsWithCore(int x, int y, int width, int height) {
			if (base.IntersectsWithCore(x, y, width, height))
				return true;
			int bottom = (Y - (DiameterInternal / 2) + DiameterInternal);
			int right = (X - (DiameterInternal / 2) + DiameterInternal);
			if (Geometry.RectangleIntersectsWithLine(x, y, x + width, y + height, Center.X, bottom, right, bottom, true))
				return true;
			return false;
		}


		/// <override></override>
		protected override void CalcControlPoints() {
			base.CalcControlPoints();
			int right = (int)Math.Round(Diameter / 2f);
			int bottom = right;
			ControlPoints[7].X = right;
			ControlPoints[7].Y = bottom;
		}


		/// <override></override>
		protected override bool CalculatePath() {
			if (base.CalculatePath()) {
				Path.Reset();
				int left = (int)Math.Round(-Diameter / 2f);
				int top = (int)Math.Round(-Diameter / 2f);
				int right = left + Diameter;
				int bottom = top + Diameter;

				Path.StartFigure();
				Path.AddLine(0, bottom, right, bottom);
				Path.AddEllipse(left, top, Diameter, Diameter);
				Path.CloseFigure();
				return true;
			}
			return false;
		}

	}

}
