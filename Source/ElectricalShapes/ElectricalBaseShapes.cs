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
using System.ComponentModel;
using System.Drawing;

using Dataweb.NShape.Advanced;


namespace Dataweb.NShape.ElectricalShapes {

	public abstract class ElectricalRectangleBase : BoxBase {

		/// <override></override>
		public override bool HasControlPointCapability(ControlPointId controlPointId, ControlPointCapabilities controlPointCapability) {
			switch (controlPointId) {
				case ControlPointIds.TopCenterControlPoint:
				case ControlPointIds.MiddleLeftControlPoint:
				case ControlPointIds.MiddleRightControlPoint:
				case ControlPointIds.BottomCenterControlPoint:
					return ((controlPointCapability & ControlPointCapabilities.Resize) != 0 
						|| (controlPointCapability & ControlPointCapabilities.Connect) != 0 && IsConnectionPointEnabled(controlPointId));
				case ControlPointIds.TopLeftControlPoint:
				case ControlPointIds.TopRightControlPoint:
				case ControlPointIds.BottomLeftControlPoint:
				case ControlPointIds.BottomRightControlPoint:
					return ((controlPointCapability & ControlPointCapabilities.Resize) != 0);
				case ControlPointIds.MiddleCenterControlPoint:
					return ((controlPointCapability & ControlPointCapabilities.Rotate) != 0
						|| (controlPointCapability & ControlPointCapabilities.Reference) != 0);
				default:
					return base.HasControlPointCapability(controlPointId, controlPointCapability);
			}
		}


		protected internal ElectricalRectangleBase(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal ElectricalRectangleBase(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		/// <override></override>
		protected override int ControlPointCount { get { return 9; } }

	}


	public abstract class ElectricalSquareBase : SquareBase {

		/// <override></override>
		public override bool HasControlPointCapability(ControlPointId controlPointId, ControlPointCapabilities controlPointCapability) {
			switch (controlPointId) {
				case ControlPointIds.TopCenterControlPoint:
				case ControlPointIds.MiddleLeftControlPoint:
				case ControlPointIds.MiddleRightControlPoint:
				case ControlPointIds.BottomCenterControlPoint:
					return ((controlPointCapability & ControlPointCapabilities.Resize) != 0 
						|| ((controlPointCapability & ControlPointCapabilities.Connect) != 0 && IsConnectionPointEnabled(controlPointId)));
				default:
					return base.HasControlPointCapability(controlPointId, controlPointCapability);
			}
		}


		protected internal ElectricalSquareBase(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal ElectricalSquareBase(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		/// <override></override>
		protected override int ControlPointCount { get { return 9; } }

	}


	public abstract class ElectricalEllipseBase : EllipseBase {

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
		}


		/// <override></override>
		public override bool HasControlPointCapability(ControlPointId controlPointId, ControlPointCapabilities controlPointCapability) {
			switch (controlPointId) {
				case ControlPointIds.TopLeftControlPoint:
				case ControlPointIds.TopRightControlPoint:
				case ControlPointIds.BottomLeftControlPoint:
				case ControlPointIds.BottomRightControlPoint:
					// No connect capability
					return ((controlPointCapability & ControlPointCapabilities.Resize) != 0);
				case EllipseBase.ControlPointIds.BottomLeftConnectionPoint:
				case EllipseBase.ControlPointIds.BottomRightConnectionPoint:
				case EllipseBase.ControlPointIds.TopLeftConnectionPoint:
				case EllipseBase.ControlPointIds.TopRightConnectionPoint:
					// Removed control point id's
					throw new ArgumentException("controlPointId");
				default:
					return base.HasControlPointCapability(controlPointId, controlPointCapability);
			}
		}


		/// <override></override>
		protected override int ControlPointCount { get { return 9; } }


		protected internal ElectricalEllipseBase(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal ElectricalEllipseBase(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}

	}


	public abstract class ElectricalCircleBase : CircleBase {

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
		}


		/// <override></override>
		public override bool HasControlPointCapability(ControlPointId controlPointId, ControlPointCapabilities controlPointCapability) {
			switch (controlPointId) {
				case ControlPointIds.TopLeftControlPoint:
				case ControlPointIds.TopRightControlPoint:
				case ControlPointIds.BottomLeftControlPoint:
				case ControlPointIds.BottomRightControlPoint:
					return ((controlPointCapability & ControlPointCapabilities.Resize) != 0);
				case CircleBase.ControlPointIds.BottomLeftConnectionPoint:
				case CircleBase.ControlPointIds.BottomRightConnectionPoint:
				case CircleBase.ControlPointIds.TopLeftConnectionPoint:
				case CircleBase.ControlPointIds.TopRightConnectionPoint:
					// Removed control point id's
					throw new ArgumentException("controlPointId");
				default:
					return base.HasControlPointCapability(controlPointId, controlPointCapability);
			}
		}


		protected internal ElectricalCircleBase(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal ElectricalCircleBase(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		/// <override></override>
		protected override int ControlPointCount { get { return 9; } }


		//// ControlPoint Id Constants
		//private const int TopLeftControlPoint = 1;
		//private const int TopRightControlPoint = 3;
		//private const int BottomLeftControlPoint = 6;
		//private const int BottomRightControlPoint = 8;
	}


	public abstract class ElectricalTriangleBase : IsoscelesTriangleBase {

		protected internal ElectricalTriangleBase(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal ElectricalTriangleBase(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		/// <override></override>
		protected override int ControlPointCount { get { return 5; } }

	}

}