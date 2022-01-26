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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;

using Dataweb.NShape.Advanced;


namespace Dataweb.NShape.GeneralShapes {

	public class ThickArrow : RectangleBase {

		/// <summary>
		/// Provides constants for the control point id's of the shape.
		/// </summary>
		new public class ControlPointIds {
			/// <summary>ControlPointId at the tip of the arrow head.</summary>
			public const int ArrowTipControlPoint = 1;
			/// <summary>ControlPointId at the top of the arrow head.</summary>
			public const int ArrowTopControlPoint = 2;
			/// <summary>ControlPointId at the bottom of the arrow head.</summary>
			public const int ArrowBottomControlPoint = 3;
			/// <summary>ControlPointId at the top of the arrow's shaft.</summary>
			public const int BodyTopControlPoint = 4;
			/// <summary>ControlPointId at the bottom of the arrow's shaft.</summary>
			public const int BodyBottomControlPoint = 5;
			/// <summary>ControlPointId at the end of the arrow.</summary>
			public const int BodyEndControlPoint = 6;
			/// <summary>ControlPointId of the center control point.</summary>
			public const int MiddleCenterControlPoint = 7;
		}


		/// <override></override>
		protected override void InitializeToDefault(IStyleSet styleSet) {
			base.InitializeToDefault(styleSet);
			_bodyHeightRatio = 1d / 3d;
			_headWidth = (int)Math.Round(Width / 2f);
		}


		/// <override></override>
		public override void CopyFrom(Shape source) {
			base.CopyFrom(source);
			if (source is ThickArrow) {
				this._headWidth = ((ThickArrow)source)._headWidth;
				this._bodyHeightRatio = ((ThickArrow)source)._bodyHeightRatio;
			}
			InvalidateDrawCache();
		}


		/// <override></override>
		public override Shape Clone() {
			Shape result = new ThickArrow(Type, this.Template);
			result.CopyFrom(this);
			return result;
		}


		public int BodyWidth {
			get { return Width - _headWidth; }
		}


		[CategoryLayout()]
		[LocalizedDisplayName("PropName_ThickArrow_BodyHeight", typeof(Properties.Resources))]
		[LocalizedDescription("PropDesc_ThickArrow_BodyHeight", typeof(Properties.Resources))]
		[PropertyMappingId(PropertyIdBodyHeight)]
		[RequiredPermission(Permission.Layout)]
		public int BodyHeight {
			get { return (int)Math.Round(Height * _bodyHeightRatio); }
			set {
				Invalidate();
				if (value > Height) throw new ArgumentOutOfRangeException("BodyHeight");

				if (Height == 0) _bodyHeightRatio = 0;
				else _bodyHeightRatio = value / (float)Height;
				
				InvalidateDrawCache();
				Invalidate();
			}
		}


		[CategoryLayout()]
		[LocalizedDisplayName("PropName_ThickArrow_HeadWidth", typeof(Properties.Resources))]
		[LocalizedDescription("PropDesc_ThickArrow_HeadWidth", typeof(Properties.Resources))]
		[PropertyMappingId(PropertyIdHeadWidth)]
		[RequiredPermission(Permission.Layout)]
		public int HeadWidth {
			get { return _headWidth; }
			set {
				Invalidate();
				_headWidth = value;
				InvalidateDrawCache();
				Invalidate();
			}
		}


		/// <override></override>
		protected override int ControlPointCount {
			get { return 7; }
		}


		/// <override></override>
		public override bool HasControlPointCapability(ControlPointId controlPointId, ControlPointCapabilities controlPointCapability) {
			switch (controlPointId) {
				case ControlPointIds.ArrowTipControlPoint:
				case ControlPointIds.BodyEndControlPoint:
					// ToDo: Implement GluePoint behavior for ThickArrows
					return ((controlPointCapability & ControlPointCapabilities.Resize) != 0
						//|| (controlPointCapability & ControlPointCapabilities.Glue) != 0
						|| ((controlPointCapability & ControlPointCapabilities.Connect) != 0 && IsConnectionPointEnabled(controlPointId)));
				case ControlPointIds.ArrowTopControlPoint:
				case ControlPointIds.BodyTopControlPoint:
				case ControlPointIds.BodyBottomControlPoint:
				case ControlPointIds.ArrowBottomControlPoint:
					return ((controlPointCapability & ControlPointCapabilities.Resize) != 0
								|| ((controlPointCapability & ControlPointCapabilities.Connect) != 0
								&& IsConnectionPointEnabled(controlPointId)));
				case ControlPointIds.MiddleCenterControlPoint:
					return ((controlPointCapability & ControlPointCapabilities.Reference) != 0
								|| (controlPointCapability & ControlPointCapabilities.Rotate) != 0
								|| ((controlPointCapability & ControlPointCapabilities.Connect) != 0
									&& IsConnectionPointEnabled(controlPointId)));
				default:
					return base.HasControlPointCapability(controlPointId, controlPointCapability);
			}
		}


		/// <override></override>
		public override void Fit(int x, int y, int width, int height) {
			float headWidthRatio = this.HeadWidth / (float)Width;
			HeadWidth = (int)Math.Round(width * headWidthRatio);
			base.Fit(x, y, width, height);
		}


		/// <override></override>
		public override Point CalculateConnectionFoot(int startX, int startY) {
			CalcShapePoints();
			PointF rotationCenter = PointF.Empty;
			rotationCenter.X = X;
			rotationCenter.Y = Y;
			Matrix.Reset();
			Matrix.Translate(X, Y, MatrixOrder.Prepend);
			Matrix.RotateAt(Geometry.TenthsOfDegreeToDegrees(Angle), rotationCenter, MatrixOrder.Append);
			Matrix.TransformPoints(_shapePoints);
			Matrix.Reset();

			Point startPoint = Point.Empty;
			startPoint.X = startX;
			startPoint.Y = startY;
			Point result = Geometry.GetNearestPoint(startPoint, Geometry.IntersectPolygonLine(_shapePoints, startX, startY, X, Y, true));
			if (!Geometry.IsValid(result)) result = Center;
			return result;
		}


		#region IEntity Members

		/// <override></override>
		protected override void LoadFieldsCore(IRepositoryReader reader, int version) {
			base.LoadFieldsCore(reader, version);
			HeadWidth = reader.ReadInt32();
			BodyHeight = reader.ReadInt32();
		}


		/// <override></override>
		protected override void SaveFieldsCore(IRepositoryWriter writer, int version) {
			base.SaveFieldsCore(writer, version);
			writer.WriteInt32(HeadWidth);
			writer.WriteInt32(BodyHeight);
		}


		/// <summary>
		/// Retrieves the persistable properties of <see cref="T:Dataweb.NShape.Advanced.ThickArrow" />.
		/// </summary>
		new public static IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			foreach (EntityPropertyDefinition pi in RectangleBase.GetPropertyDefinitions(version))
				yield return pi;
			yield return new EntityFieldDefinition("HeadWidth", typeof(int));
			yield return new EntityFieldDefinition("BodyHeight", typeof(int));
		}

		#endregion


		protected internal ThickArrow(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected internal ThickArrow(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		/// <override></override>
		protected override void CalcCaptionBounds(int index, out Rectangle captionBounds) {
			if (index != 0) throw new ArgumentOutOfRangeException("index");
			captionBounds = Rectangle.Empty;
			captionBounds.Width = (int)Math.Round(Width - (HeadWidth / 3f));
			captionBounds.Height = BodyHeight;
			captionBounds.X = -(int)Math.Round((Width / 2f) - (HeadWidth / 3f));
			captionBounds.Y = -(int)Math.Round(captionBounds.Height / 2f);
			if (ParagraphStyle != null) {
				captionBounds.X += ParagraphStyle.Padding.Left;
				captionBounds.Y += ParagraphStyle.Padding.Top;
				captionBounds.Width -= ParagraphStyle.Padding.Horizontal;
				captionBounds.Height -= ParagraphStyle.Padding.Vertical;
			}
		}


		/// <override></override>
		protected override bool CalculatePath() {
			if (base.CalculatePath()) {
				Path.Reset();
				CalcShapePoints();
				Path.StartFigure();
				Path.AddPolygon(_shapePoints);
				Path.CloseFigure();
				return true;
			} else return false;
		}


		/// <override></override>
		protected override bool ContainsPointCore(int x, int y) {
			if (base.ContainsPointCore(x, y)) {
				CalcShapePoints();
				// Transform points
				Matrix.Reset();
				Matrix.Translate(X, Y, MatrixOrder.Prepend);
				Matrix.RotateAt(Geometry.TenthsOfDegreeToDegrees(Angle), Center, MatrixOrder.Append);
				Matrix.TransformPoints(_shapePoints);

				return Geometry.PolygonContainsPoint(_shapePoints, x, y);
			} return false;
		}


		/// <override></override>
		protected override Rectangle CalculateBoundingRectangle(bool tight) {
			Rectangle result = Geometry.InvalidRectangle;
			if (Width >= 0 && Height >= 0) {
				CalcShapePoints();
				Geometry.CalcBoundingRectangle(_shapePoints, 0, 0, Geometry.TenthsOfDegreeToDegrees(Angle), out result);
				if (Geometry.IsValid(result)) {
					result.Offset(X, Y);
					ShapeUtils.InflateBoundingRectangle(ref result, LineStyle);
				}
			}
			return result;
		}


		/// <override></override>
		protected override int GetControlPointIndex(ControlPointId id) {
			switch (id) {
				case ControlPointIds.ArrowTipControlPoint: return 0;
				case ControlPointIds.ArrowTopControlPoint: return 1;
				case ControlPointIds.BodyTopControlPoint: return 2;
				case ControlPointIds.BodyEndControlPoint: return 3;
				case ControlPointIds.BodyBottomControlPoint: return 4;
				case ControlPointIds.ArrowBottomControlPoint: return 5;
				case ControlPointIds.MiddleCenterControlPoint: return 6;
				default:
					return base.GetControlPointIndex(id);
			}
		}


		/// <override></override>
		protected override void CalcControlPoints() {
			int left = -(int)Math.Round(Width / 2f);
			int right = left + Width;
			int top = -(int)Math.Round(Height / 2f);
			int bottom = top + Height;
			int halfBodyWidth = (int)Math.Round(BodyWidth / 2f);
			int halfBodyHeight = (int)Math.Round(BodyHeight / 2f);

			int i = 0;
			ControlPoints[i].X = left;
			ControlPoints[i].Y = 0;
			++i;
			ControlPoints[i].X = right - BodyWidth;
			ControlPoints[i].Y = top;
			++i;
			ControlPoints[i].X = right - halfBodyWidth;
			ControlPoints[i].Y = -halfBodyHeight;
			++i;
			ControlPoints[i].X = right;
			ControlPoints[i].Y = 0;
			++i;
			ControlPoints[i].X = right - halfBodyWidth;
			ControlPoints[i].Y = halfBodyHeight;
			++i;
			ControlPoints[i].X = right - BodyWidth;
			ControlPoints[i].Y = bottom;
			++i;
			ControlPoints[i].X = 0;
			ControlPoints[i].Y = 0;
		}


		/// <override></override>
		protected override bool MovePointByCore(ControlPointId pointId, int deltaX, int deltaY, ResizeModifiers modifiers) {
			if (pointId == ControlPointIds.ArrowTipControlPoint || pointId == ControlPointIds.BodyEndControlPoint) {
				bool result = true;
				int dx = 0, dy = 0;
				int width = Width;
				int angle = Angle;
				Point tipPt = GetControlPointPosition(ControlPointIds.ArrowTipControlPoint);
				Point endPt = GetControlPointPosition(ControlPointIds.BodyEndControlPoint);

				if (pointId == ControlPointIds.ArrowTipControlPoint)
					result = Geometry.MoveArrowPoint(Center, tipPt, endPt, angle, _headWidth, 0.5f, deltaX, deltaY, modifiers, out dx, out dy, out width, out angle);
				else
					result = Geometry.MoveArrowPoint(Center, endPt, tipPt, angle, _headWidth, 0.5f, deltaX, deltaY, modifiers, out dx, out dy, out width, out angle);

				RotateCore(angle - Angle, X, Y);
				MoveByCore(dx, dy);
				Width = width;
				return result;
			} else return base.MovePointByCore(pointId, deltaX, deltaY, modifiers);
		}


		/// <override></override>
		protected override bool MovePointByCore(ControlPointId pointId, float transformedDeltaX, float transformedDeltaY, float sin, float cos, ResizeModifiers modifiers) {
			bool result = true;
			int dx = 0, dy = 0;
			int width = Width;
			int height = Height;
			switch (pointId) {
				case ControlPointIds.ArrowTopControlPoint:
				case ControlPointIds.ArrowBottomControlPoint:
					if (pointId == ControlPointIds.ArrowTopControlPoint) {
						//result = (transformedDeltaX == 0);
						if (!Geometry.MoveRectangleTop(width, height, transformedDeltaX, transformedDeltaY, cos, sin, modifiers, out dx, out dy, out width, out height))
							result = false;
					} else {
						//result = (transformedDeltaX == 0);
						if (!Geometry.MoveRectangleBottom(width, height, transformedDeltaX, transformedDeltaY, cos, sin, modifiers, out dx, out dy, out width, out height))
							result = false;
					}
					int newHeadWidth = HeadWidth + (int)Math.Round(transformedDeltaX);
					if (newHeadWidth < 0) {
						newHeadWidth = 0;
						result = false;
					} else if (newHeadWidth > Width) {
						newHeadWidth = Width;
						result = false;
					}
					HeadWidth = newHeadWidth;
					break;
				case ControlPointIds.BodyTopControlPoint:
				case ControlPointIds.BodyBottomControlPoint:
					result = (transformedDeltaX == 0);
					int newBodyHeight = 0;
					if (pointId == ControlPointIds.BodyTopControlPoint)
						newBodyHeight = (int)Math.Round(BodyHeight - (transformedDeltaY * 2));
					else
						newBodyHeight = (int)Math.Round(BodyHeight + (transformedDeltaY * 2));
					if (newBodyHeight > Height) {
						newBodyHeight = Height;
						result = false;
					} else if (newBodyHeight < 0) {
						newBodyHeight = 0;
						result = false;
					}
					BodyHeight = newBodyHeight;
					break;
				default:
					return base.MovePointByCore(pointId, transformedDeltaX, transformedDeltaY, sin, cos, modifiers);
			}
			if (width < _headWidth) {
				width = _headWidth;
				result = false;
			}
			MoveByCore(dx, dy);
			Width = width;
			Height = height;
			return result;
		}


		/// <override></override>
		protected override void ProcessExecModelPropertyChange(IModelMapping propertyMapping) {
			switch (propertyMapping.ShapePropertyId) {
				case PropertyIdBodyHeight:
					BodyHeight = propertyMapping.GetInteger();
					break;
				case PropertyIdHeadWidth:
					HeadWidth = propertyMapping.GetInteger();
					break;
				default:
					base.ProcessExecModelPropertyChange(propertyMapping);
					break;
			}
		}
		
		
		private void CalcShapePoints() {
			int left = -(int)Math.Round(Width / 2f);
			int right = left + Width;
			int top = -(int)Math.Round(Height / 2f);
			int bottom = top + Height;
			int halfBodyHeight = (int)Math.Round(BodyHeight / 2f);

			// head tip
			_shapePoints[0].X = left;
			_shapePoints[0].Y = 0;

			// head side tip (top)
			_shapePoints[1].X = right - BodyWidth;
			_shapePoints[1].Y = top;

			// head / body connection point
			_shapePoints[2].X = right - BodyWidth;
			_shapePoints[2].Y = -halfBodyHeight;

			// body corner (top)
			_shapePoints[3].X = right;
			_shapePoints[3].Y = -halfBodyHeight;

			// body corner (bottom)
			_shapePoints[4].X = right;
			_shapePoints[4].Y = halfBodyHeight;

			// head / body connection point
			_shapePoints[5].X = right - BodyWidth;
			_shapePoints[5].Y = halfBodyHeight;

			// head side tip (bottom)
			_shapePoints[6].X = right - BodyWidth;
			_shapePoints[6].Y = bottom;
		}


		#region Fields

		protected const int PropertyIdBodyHeight = 9;
		protected const int PropertyIdHeadWidth = 10;

		private Point _newTipPos = Point.Empty;
		private Point _oldTipPos = Point.Empty;

		private Point[] _shapePoints = new Point[7];
		private int _headWidth;
		private double _bodyHeightRatio;
		#endregion
	}


	public class Picture : PictureBase {
		
		internal static Shape CreateInstance(ShapeType shapeType, Template template) {
			return new Picture(shapeType, template);
		}


		/// <override></override>
		public override Shape Clone() {
			Shape result = new Picture(Type, this.Template);
			result.CopyFrom(this);
			return result;
		}


		protected internal Picture(ShapeType shapeType, Template template)
			: base(shapeType, template) {
			Construct();
		}


		protected internal Picture(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
			Construct();
		}


		private void Construct() {
			//Image = new NamedImage();
		}

	}


	public class RegularPolygone : RegularPolygoneBase {
		
		/// <ToBeCompleted></ToBeCompleted>
		public static Shape CreateInstance(ShapeType shapeType, Template template) {
			return new RegularPolygone(shapeType, template);
		}


		public override Shape Clone() {
			Shape result = new RegularPolygone(Type, this.Template);
			result.CopyFrom(this);
			return result;
		}


		protected RegularPolygone(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		protected RegularPolygone(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}

	}

}