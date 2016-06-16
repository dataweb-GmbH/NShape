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

using Dataweb.NShape.Advanced;


namespace Dataweb.NShape.FlowChartShapes {

	public abstract class FlowChartRectangleBase : BoxBase {

		/// <override></override>
		public override bool HasControlPointCapability(ControlPointId controlPointId, ControlPointCapabilities controlPointCapability) {
			// Customize the capabilities of the shape's control points
			switch (controlPointId) {
				case ControlPointIds.TopLeftControlPoint:
				case ControlPointIds.TopRightControlPoint:
				case ControlPointIds.BottomLeftControlPoint:
				case ControlPointIds.BottomRightControlPoint:
					// The connection capability of the corner points is removed herby
					return ((controlPointCapability & ControlPointCapabilities.Resize) != 0);
				case ControlPointIds.MiddleCenterControlPoint:
				    // The connection point capability of the center point (including the "to-Shape" connection capability) is removed herby
				    return ((controlPointCapability & ControlPointCapabilities.Rotate) != 0 
				        || (controlPointCapability & ControlPointCapabilities.Reference) != 0);
				default:
					return base.HasControlPointCapability(controlPointId, controlPointCapability);
			}
		}


		protected internal FlowChartRectangleBase(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal FlowChartRectangleBase(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}

	}


	public abstract class FlowChartEllipseBase : EllipseBase {

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
			// Customize the capabilities of the shape's control points
			switch (controlPointId) {
				case ControlPointIds.TopLeftControlPoint:
				case ControlPointIds.TopRightControlPoint:
				case ControlPointIds.BottomLeftControlPoint:
				case ControlPointIds.BottomRightControlPoint:
					// The connection capability of the corner points is removed herby
					return ((controlPointCapability & ControlPointCapabilities.Resize) != 0);
				case ControlPointIds.MiddleCenterControlPoint:
					// The connection point capability of the center point (including the "to-Shape" connection capability) is removed herby
					return ((controlPointCapability & ControlPointCapabilities.Rotate) != 0
						|| (controlPointCapability & ControlPointCapabilities.Reference) != 0);
				default:
					return base.HasControlPointCapability(controlPointId, controlPointCapability);
			}
		}


		protected internal FlowChartEllipseBase(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal FlowChartEllipseBase(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		protected override int ControlPointCount {
			get {
				// We do not want the additional points on the diagonal sides
				return 9;
			}
		}

	}


	public abstract class FlowChartCircleBase : CircleBase {

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
			// Customize the capabilities of the shape's control points
			switch (controlPointId) {
				case ControlPointIds.TopLeftControlPoint:
				case ControlPointIds.TopRightControlPoint:
				case ControlPointIds.BottomLeftControlPoint:
				case ControlPointIds.BottomRightControlPoint:
					// The connection capability of the corner points is removed herby
					return ((controlPointCapability & ControlPointCapabilities.Resize) != 0);
				case ControlPointIds.MiddleCenterControlPoint:
					// The connection point capability of the center point (including the "to-Shape" connection capability) is removed herby
					return ((controlPointCapability & ControlPointCapabilities.Rotate) != 0
						|| (controlPointCapability & ControlPointCapabilities.Reference) != 0);
				default:
					return base.HasControlPointCapability(controlPointId, controlPointCapability);
			}
		}


		protected internal FlowChartCircleBase(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal FlowChartCircleBase(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		protected override int ControlPointCount {
			get {
				// We do not want the additional points on the diagonal sides
				return 9;
			}
		}

	}


	public abstract class FlowChartSquareBase : SquareBase {

		/// <override></override>
		public override bool HasControlPointCapability(ControlPointId controlPointId, ControlPointCapabilities controlPointCapability) {
			// Customize the capabilities of the shape's control points
			switch (controlPointId) {
				case ControlPointIds.TopLeftControlPoint:
				case ControlPointIds.TopRightControlPoint:
				case ControlPointIds.BottomLeftControlPoint:
				case ControlPointIds.BottomRightControlPoint:
					// The connection capability of the corner points is removed herby
					return (controlPointCapability & ControlPointCapabilities.Resize) != 0;
				case ControlPointIds.MiddleCenterControlPoint:
					// The connection point capability of the center point (including the "to-Shape" connection capability) is removed herby
					return ((controlPointCapability & ControlPointCapabilities.Rotate) != 0
						|| (controlPointCapability & ControlPointCapabilities.Reference) != 0);
				default:
					return base.HasControlPointCapability(controlPointId, controlPointCapability);
			}
		}


		protected internal FlowChartSquareBase(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal FlowChartSquareBase(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}

	}


	public abstract class FlowChartDiamondBase : DiamondBase {

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
			// Customize the capabilities of the shape's control points
			switch (controlPointId) {
				case ControlPointIds.TopLeftControlPoint:
				case ControlPointIds.TopRightControlPoint:
				case ControlPointIds.BottomLeftControlPoint:
				case ControlPointIds.BottomRightControlPoint:
					// The connection capability of the corner points is removed herby
					return (controlPointCapability & ControlPointCapabilities.Resize) != 0;
				case ControlPointIds.MiddleCenterControlPoint:
					// The connection point capability of the center point (including the "to-Shape" connection capability) is removed herby
					return ((controlPointCapability & ControlPointCapabilities.Rotate) != 0
						|| (controlPointCapability & ControlPointCapabilities.Reference) != 0);
				default:
					return base.HasControlPointCapability(controlPointId, controlPointCapability);
			}
		}


		protected internal FlowChartDiamondBase(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal FlowChartDiamondBase(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		protected override int ControlPointCount {
			get {
				// We do not want the additional points on the diagonal sides
				return 9;
			}
		}

	}


	public abstract class FlowChartTriangleBase : IsoscelesTriangleBase {

		/// <summary>
		/// Provides constants for the control point id's of the shape.
		/// </summary>
		new public class ControlPointIds {
			/// <summary>ControlPointId of the top center control point.</summary>
			public const int TopCenterControlPoint = 1;
			/// <summary>ControlPointId of the bottom left control point.</summary>
			public const int BottomLeftControlPoint = 2;
			/// <summary>ControlPointId of the bottom center control point.</summary>
			public const int BottomCenterControlPoint = 4;
			/// <summary>ControlPointId of the bottom right control point.</summary>
			public const int BottomRightControlPoint = 3;
			/// <summary>ControlPointId of the center control point.</summary>
			public const int BalancePointControlPoint = 5;
			/// <summary>ControlPointId of the connection point on the left side of the base.</summary>
			public const int LeftConnectionPoint = 6;
			/// <summary>ControlPointId of the connection point on the right side of the base.</summary>
			public const int RightConnectionPoint = 7;
		}


		/// <override></override>
		public override bool HasControlPointCapability(ControlPointId controlPointId, ControlPointCapabilities controlPointCapability) {
			// Customize the capabilities of the shape's control points
			switch (controlPointId) {
				case ControlPointIds.BottomCenterControlPoint:
				case ControlPointIds.TopCenterControlPoint:
					return ((controlPointCapability & ControlPointCapabilities.Resize) != 0 
						|| ((controlPointCapability & ControlPointCapabilities.Connect) != 0 && IsConnectionPointEnabled(controlPointId)));
				case ControlPointIds.LeftConnectionPoint:
				case ControlPointIds.RightConnectionPoint:
					return ((controlPointCapability & ControlPointCapabilities.Connect) != 0 && IsConnectionPointEnabled(controlPointId));
				case ControlPointIds.BottomLeftControlPoint:
				case ControlPointIds.BottomRightControlPoint:
					return ((controlPointCapability & ControlPointCapabilities.Resize) != 0);
				case ControlPointIds.BalancePointControlPoint:
					// The connection point capability of the center point (NOT including the "to-Shape" connection capability) is removed herby
					return ((controlPointCapability & ControlPointCapabilities.Rotate) != 0
						|| (controlPointCapability & ControlPointCapabilities.Reference) != 0);
				default:
					return base.HasControlPointCapability(controlPointId, controlPointCapability);
			}
		}


		/// <override></override>
		protected override int ControlPointCount {
			get { return 7; }
		}


		protected internal FlowChartTriangleBase(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal FlowChartTriangleBase(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}

	}

}