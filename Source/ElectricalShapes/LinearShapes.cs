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
			if ((controlPointCapability & ControlPointCapabilities.Connect) != 0)
				return true;
			if ((controlPointCapability & ControlPointCapabilities.Glue) != 0) {
				// always false
			}
			if ((controlPointCapability & ControlPointCapabilities.Reference) != 0) {
				if (controlPointId == ControlPointId.Reference || controlPointId == 1) return true;
			}
			if ((controlPointCapability & ControlPointCapabilities.Rotate) != 0) {
				// always false
			}
			if ((controlPointCapability & ControlPointCapabilities.Resize) != 0)
				return true;
			return false;
		}


		protected internal BusBarSymbol(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal BusBarSymbol(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}
	}

}
