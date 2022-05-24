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

using Dataweb.NShape.Advanced;


namespace Dataweb.NShape.ElectricalShapes {

	public class BusBarSymbol : PolylineBase {

		/// <override></override>
		public override Shape Clone() {
			Shape result = new BusBarSymbol(Type, this.Template);
			result.CopyFrom(this);
			return result;
		}


		/// <override></override>
		public override int MaxVertexCount {
			get { return 2; }
		}


		/// <override></override>
		public override bool HasControlPointCapability(ControlPointId controlPointId, ControlPointCapabilities controlPointCapability) {
			// BusBar has no glue points
			if (IsFirstVertex(controlPointId)) {
				return (controlPointCapability & ControlPointCapabilities.Reference) != 0
					|| (controlPointCapability & ControlPointCapabilities.Resize) != 0;
			} else if (IsLastVertex(controlPointId)) {
				return (controlPointCapability & ControlPointCapabilities.Resize) != 0;
			} else
				return base.HasControlPointCapability(controlPointId, controlPointCapability);
		}


		protected internal BusBarSymbol(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal BusBarSymbol(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}
	}

}
