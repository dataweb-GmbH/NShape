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
using System.Drawing;

using Dataweb.NShape.Advanced;


namespace Dataweb.NShape.ElectricalShapes {

	public class DisconnectorSymbol : ElectricalCircleBase {

		/// <override></override>
		public override Shape Clone() {
			Shape result = new DisconnectorSymbol(Type, this.Template);
			result.CopyFrom(this);
			return result;
		}


		/// <override></override>
		protected override bool CalculatePath() {
			if (base.CalculatePath()) {
				int left = (int)Math.Round(-Diameter / 2f);
				int top = (int)Math.Round(-Diameter / 2f);

				Path.StartFigure();
				Path.AddEllipse(left, top, Diameter, Diameter);
				Path.CloseFigure();
				return true;
			}
			return false;
		}


		protected internal DisconnectorSymbol(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal DisconnectorSymbol(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}
	}


	public class AutoDisconnectorSymbol : ElectricalCircleBase {

		/// <override></override>
		public override Shape Clone() {
			Shape result = new AutoDisconnectorSymbol(Type, this.Template);
			result.CopyFrom(this);
			return result;
		}


		/// <override></override>
		protected override bool CalculatePath() {
			if (base.CalculatePath()) {
				Path.Reset();
				int left = (int)Math.Round(-Diameter / 2f);
				int top = (int)Math.Round(-Diameter / 2f);
				int right = left + Diameter;

				Path.StartFigure();
				Path.AddLine(left, top, right, top);
				Path.AddEllipse(left, top, Diameter, Diameter);
				Path.CloseFigure();
				return true;
			}
			return false;
		}


		/// <override></override>
		protected override Rectangle CalculateBoundingRectangle(bool tight) {
			if (tight) {
				Rectangle result = Rectangle.Empty;
				result.X = X - (int)Math.Round(Diameter / 2f);
				result.Y = Y - (int)Math.Round(Diameter / 2f);
				result.Width = result.Height = Diameter;
				// Rotate top line
				if (Angle != 0) {
					float angleDeg = Geometry.TenthsOfDegreeToDegrees(Angle);
					Point tl = Point.Empty;
					tl.Offset(result.Left, result.Top);
					tl = Geometry.RotatePoint(X, Y, angleDeg, tl);
					Point tr = Point.Empty;
					tr.Offset(result.Right, result.Top);
					tr = Geometry.RotatePoint(X, Y, angleDeg, tr);
					Geometry.UniteRectangles(tl.X, tl.Y, tr.X, tr.Y, result);
					ShapeUtils.InflateBoundingRectangle(ref result, LineStyle);
				}
				return result;
			} return base.CalculateBoundingRectangle(tight);
		}


		protected internal AutoDisconnectorSymbol(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal AutoDisconnectorSymbol(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}
	}

}
