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


namespace Dataweb.NShape.Advanced {

	/// <summary>
	/// Title shape.
	/// </summary>
	public abstract class TextBase : RectangleBase {

		/// <summary>ControlPointId of the top left control point.</summary>
		[Obsolete("Use TextBase.ControlPointIds instead")]
		public const int TopLeftControlPoint = 1;
		/// <summary>ControlPointId of the top center control point.</summary>
		[Obsolete("Use TextBase.ControlPointIds instead")]
		public const int TopCenterControlPoint = 2;
		/// <summary>ControlPointId of the top right control point.</summary>
		[Obsolete("Use TextBase.ControlPointIds instead")]
		public const int TopRightControlPoint = 3;
		/// <summary>ControlPointId of the middle left control point.</summary>
		[Obsolete("Use TextBase.ControlPointIds instead")]
		public const int MiddleLeftControlPoint = 4;
		/// <summary>ControlPointId of the middle right control point.</summary>
		[Obsolete("Use TextBase.ControlPointIds instead")]
		public const int MiddleRightControlPoint = 5;
		/// <summary>ControlPointId of the bottom left control point.</summary>
		[Obsolete("Use TextBase.ControlPointIds instead")]
		public const int BottomLeftControlPoint = 6;
		/// <summary>ControlPointId of the bottom center control point.</summary>
		[Obsolete("Use TextBase.ControlPointIds instead")]
		public const int BottomCenterControlPoint = 7;
		/// <summary>ControlPointId of the bottom right control point.</summary>
		[Obsolete("Use TextBase.ControlPointIds instead")]
		public const int BottomRightControlPoint = 8;
		/// <summary>ControlPointId of the center control point.</summary>
		[Obsolete("Use TextBase.ControlPointIds instead")]
		public const int MiddleCenterControlPoint = 9;


		#region [Public] Properties

		/// <override></override>
		public override string Text {
			get { return base.Text; }
			set {
				base.Text = value;
				if (AutoSize) {
					FitShapeToText();
					InvalidateDrawCache();
					Invalidate();
				}
			}
		}


		/// <override></override>
		public override ICharacterStyle CharacterStyle {
			get { return base.CharacterStyle; }
			set {
				base.CharacterStyle = value;
				if (AutoSize) {
					FitShapeToText();
					InvalidateDrawCache();
					Invalidate();
				}
			}
		}


		/// <override></override>
		public override IParagraphStyle ParagraphStyle {
			get { return base.ParagraphStyle; }
			set {
				base.ParagraphStyle = value;
				if (AutoSize) {
					FitShapeToText();
					InvalidateDrawCache();
					Invalidate();
				}
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		[CategoryLayout()]
		[LocalizedDisplayName("PropName_TextBase_AutoSize")]
		[LocalizedDescription("PropDesc_TextBase_AutoSize")]
		[RequiredPermission(Permission.Layout)]
		public bool AutoSize {
			get { return _autoSize; }
			set {
				Invalidate();
				_autoSize = value;
				if (AutoSize) {
					InvalidateDrawCache();
					Invalidate();
				}
			}
		}

		#endregion


		#region [Public] Methods

		/// <override></override>
		public override void CopyFrom(Shape source) {
			base.CopyFrom(source);
			if (source is TextBase)
				this.AutoSize = ((TextBase)source).AutoSize;
		}


		/// <override></override>
		protected override bool MovePointByCore(ControlPointId pointId, int deltaX, int deltaY, ResizeModifiers modifiers) {
			if (AutoSize && pointId != ControlPointId.Reference) 
				return false;
			else return base.MovePointByCore(pointId, deltaX, deltaY, modifiers);
		}


		/// <override></override>
		public override void SetCaptionCharacterStyle(int index, ICharacterStyle characterStyle) {
			base.SetCaptionCharacterStyle(index, characterStyle);
			if (AutoSize) FitShapeToText();
		}


		/// <override></override>
		public override void SetCaptionParagraphStyle(int index, IParagraphStyle paragraphStyle) {
			base.SetCaptionParagraphStyle(index, paragraphStyle);
			if (AutoSize) FitShapeToText();
		}


		/// <override></override>
		public override bool HasControlPointCapability(ControlPointId controlPointId, ControlPointCapabilities controlPointCapability) {
			switch (controlPointId) {
				case ControlPointIds.TopLeftControlPoint:
				case ControlPointIds.TopCenterControlPoint:
				case ControlPointIds.TopRightControlPoint:
				case ControlPointIds.MiddleLeftControlPoint:
				case ControlPointIds.MiddleRightControlPoint:
				case ControlPointIds.BottomLeftControlPoint:
				case ControlPointIds.BottomCenterControlPoint:
				case ControlPointIds.BottomRightControlPoint:
					return ((controlPointCapability & ControlPointCapabilities.Resize) != 0 && !AutoSize);
				case ControlPointId.Reference:
				case ControlPointIds.MiddleCenterControlPoint:
					return ((controlPointCapability & ControlPointCapabilities.Reference) != 0
						|| (controlPointCapability & ControlPointCapabilities.Rotate) != 0
						|| (controlPointCapability & ControlPointCapabilities.Connect) != 0);
				default:
					return base.HasControlPointCapability(controlPointId, controlPointCapability);
			}
		}


		/// <override></override>
		public override IEnumerable<ControlPointId> GetControlPointIds(ControlPointCapabilities controlPointCapability) {
			return base.GetControlPointIds(controlPointCapability);
		}


		/// <override></override>
		protected internal override bool NotifyStyleChanged(IStyle style) {
			if (base.NotifyStyleChanged(style)) {
				if (AutoSize && (style is ICharacterStyle || style is IParagraphStyle))
					FitShapeToText();
				return true;
			} else return false;
		}


		/// <override></override>
		public override void Draw(Graphics graphics) {
			if (graphics == null) throw new ArgumentNullException(nameof(graphics));
			DrawPath(graphics, LineStyle, FillStyle);
			DrawCaption(graphics);
		}


		/// <override></override>
		public override void DrawThumbnail(Image image, int margin, Color transparentColor) {
			AutoSize = false;
			Text = "ab";

			Size textSize = Size.Empty;

			// Create a ParameterStyle without padding
			ParagraphStyle paragraphStyle = new ParagraphStyle();
			paragraphStyle.Alignment = ContentAlignment.TopLeft;
			paragraphStyle.Padding = new TextPadding(0);
			paragraphStyle.Trimming = StringTrimming.None;
			paragraphStyle.WordWrap = false;
			ParagraphStyle = paragraphStyle;

			textSize = TextMeasurer.MeasureText(Text, ToolCache.GetFont(CharacterStyle), textSize, ParagraphStyle);

			Width = textSize.Width;
			Height = textSize.Height;

			// If the linestyle is transparent, modify margin in order to improve the text's readability
			int marginCorrection = (LineStyle.ColorStyle.Transparency == 100) ? 3 : 0;
			base.DrawThumbnail(image, margin - marginCorrection, transparentColor);
		}

		#endregion


		#region [Protected] Methods

		/// <override></override>
		protected internal TextBase(ShapeType shapeType, Template template)
			: base(shapeType, template) {
			TextParametersInitialized = true;
		}


		/// <override></override>
		protected internal TextBase(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
			TextParametersInitialized = true;
		}


		/// <override></override>
		protected internal override void InitializeToDefault(IStyleSet styleSet) {
			base.InitializeToDefault(styleSet);
			AutoSize = true;
			Text = "Text";
			// override standard Line- and FillStyles
			FillStyle = styleSet.FillStyles.Transparent;
			LineStyle = styleSet.LineStyles.None;
			TextParametersInitialized = true;
		}


		/// <override></override>
		protected override void CalcCaptionBounds(int index, out Rectangle captionBounds) {
			captionBounds = Rectangle.Empty;
			if (_autoSize) {
				captionBounds.Size = TextMeasurer.MeasureText(Text, CharacterStyle, Size.Empty, ParagraphStyle);
				captionBounds.Width += ParagraphStyle.Padding.Horizontal;
				captionBounds.Height += ParagraphStyle.Padding.Vertical;
				captionBounds.X = (int)Math.Round(-captionBounds.Width / 2f);
				captionBounds.Y = (int)Math.Round(-captionBounds.Height / 2f);
			} else 
				base.CalcCaptionBounds(index, out captionBounds);
		}


		/// <override></override>
		protected override bool IntersectsWithCore(int x, int y, int width, int height) {
			if (_autoSize && !string.IsNullOrEmpty(Text)) {
				Rectangle rectangle = Rectangle.Empty;
				rectangle.X = x;
				rectangle.Y = y;
				rectangle.Width = width;
				rectangle.Height = height;

				if (Angle % 900 == 0)
					return Geometry.RectangleIntersectsWithRectangle(rectangle, GetBoundingRectangle(true));
				else {
					float angleDeg = Geometry.TenthsOfDegreeToDegrees(Angle);
					Rectangle layoutRectangle = Rectangle.Empty;
					CalcCaptionBounds(0, out layoutRectangle);
					layoutRectangle.Offset(X, Y);

					if (Geometry.RectangleContainsPoint(layoutRectangle.X, layoutRectangle.Y, layoutRectangle.Width, layoutRectangle.Height, angleDeg, rectangle.Left, rectangle.Top, true)
						|| Geometry.RectangleContainsPoint(layoutRectangle.X, layoutRectangle.Y, layoutRectangle.Width, layoutRectangle.Height, angleDeg, rectangle.Right, rectangle.Top, true)
						|| Geometry.RectangleContainsPoint(layoutRectangle.X, layoutRectangle.Y, layoutRectangle.Width, layoutRectangle.Height, angleDeg, rectangle.Right, rectangle.Bottom, true)
						|| Geometry.RectangleContainsPoint(layoutRectangle.X, layoutRectangle.Y, layoutRectangle.Width, layoutRectangle.Height, angleDeg, rectangle.Left, rectangle.Bottom, true)
						) return true;
					else 
						return false;

					// Old version, did not detect when the rectangle was completely inside the shape!
					//Point tl = Point.Empty, tr = Point.Empty, br = Point.Empty, bl = Point.Empty;
					//Geometry.TransformRectangle(Center, Angle, layoutRectangle, out tl, out tr, out br, out bl);
					//if (Geometry.RectangleContainsPoint(rectangle, tl) 
					//    || Geometry.RectangleContainsPoint(rectangle, tr) 
					//    || Geometry.RectangleContainsPoint(rectangle, bl) 
					//    || Geometry.RectangleContainsPoint(rectangle, br))
					//    return true;
					//else {
					//    if (Geometry.RectangleIntersectsWithLine(rectangle, tl.X, tl.Y, tr.X, tr.Y, true))
					//        return true;
					//    if (Geometry.RectangleIntersectsWithLine(rectangle, tr.X, tr.Y, br.X, br.Y, true))
					//        return true;
					//    if (Geometry.RectangleIntersectsWithLine(rectangle, br.X, br.Y, bl.X, bl.Y, true))
					//        return true;
					//    if (Geometry.RectangleIntersectsWithLine(rectangle, bl.X, bl.Y, tl.X, tl.Y, true))
					//        return true;
					//}
					//return false;
				}
			} else
				return base.IntersectsWithCore(x, y, width, height);
		}


		/// <override></override>
		protected override void RecalcDrawCache() {
			// Fit shape to text *before* the draw cache is calculated
			if (_autoSize) FitShapeToText();
			base.RecalcDrawCache();
		}


		/// <override></override>
		protected override bool CalculatePath() {
			if (base.CalculatePath()) {
				Path.Reset();
				Rectangle shapeBuffer = Rectangle.Empty;
				shapeBuffer.X = (int)Math.Round(-Width / 2f);
				shapeBuffer.Y = (int)Math.Round(-Height / 2f);
				shapeBuffer.Width = Width;
				shapeBuffer.Height = Height;

				Path.Reset();
				Path.StartFigure();
				Path.AddRectangle(shapeBuffer);
				Path.CloseFigure();
				return true;
			}
			return false;
		}

		#endregion


		#region IEntity Members

		/// <summary>
		/// Retrieves the persistable properties of <see cref="T:Dataweb.NShape.Advanced.TextBase" />.
		/// </summary>
		new public static IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			foreach (EntityPropertyDefinition pi in RectangleBase.GetPropertyDefinitions(version))
				yield return pi;
			yield return new EntityFieldDefinition("AutoSize", typeof(bool));
		}


		/// <override></override>
		protected override void LoadFieldsCore(IRepositoryReader reader, int version) {
			base.LoadFieldsCore(reader, version);
			AutoSize = reader.ReadBool();
		}


		/// <override></override>
		protected override void SaveFieldsCore(IRepositoryWriter writer, int version) {
			base.SaveFieldsCore(writer, version);
			writer.WriteBool(AutoSize);
		}

		#endregion


		/// <summary>
		/// Fits the shape dimensions to the size of the caption(s).
		/// </summary>
		protected virtual void FitShapeToText() {
			if (!string.IsNullOrEmpty(Text) && TextParametersInitialized) {
				Size textSize = TextMeasurer.MeasureText(Text, CharacterStyle, Size.Empty, ParagraphStyle);
				Width = textSize.Width + ParagraphStyle.Padding.Horizontal;
				Height = textSize.Height + ParagraphStyle.Padding.Vertical;
			}
		}


		/// <summary>
		/// Returns true if the properties relevant for text size calculation (character style and paragraph style) are initialized.
		/// </summary>
		protected bool TextParametersInitialized {
			get { return _textParametersInitialized; }
			set { _textParametersInitialized = value; }
		}


		#region Fields
		
		private bool _autoSize = true;
		private bool _textParametersInitialized = false;

		#endregion
	}


	/// <ToBeCompleted></ToBeCompleted>
	public abstract class LabelBase : TextBase {

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
			/// <summary>ControlPointId of the sticky control point used to attach the label to other shapes.</summary>
			public const int GlueControlPoint = 10;
		}


		#region [Public] Properties

		/// <ToBeCompleted></ToBeCompleted>
		[CategoryLayout()]
		[LocalizedDisplayName("PropName_LabelBase_MaintainOrientation")]
		[LocalizedDescription("PropDesc_LabelBase_MaintainOrientation")]
		[RequiredPermission(Permission.Layout)]
		public bool MaintainOrientation {
			get { return _maintainOrientation; }
			set {
				if (_maintainOrientation != value) {
					Invalidate();
					_maintainOrientation = value;
					InvalidateDrawCache();
					Invalidate();
				}
			}
		}

		///// <ToBeCompleted></ToBeCompleted>
		//[LayoutCategoryAttribute()]
		//[Description("If true, the label draws a line between the label's text bounds and the point it sticks to.")]
		//[RequiredPermission(Permission.Layout)]
		//public bool ShowGluePointLine {
		//    get { return showGluePointLine; }
		//    set {
		//        Invalidate();
		//        showGluePointLine = value;
		//        Invalidate();
		//    }
		//}

		#endregion


		#region [Public] Shape Members

		/// <override></override>
		public override void CopyFrom(Shape source) {
			// Prevent recalculating GluePointCalcInfo when moving shape while copying X/Y position
			IsGluePointFollowingConnectionPoint = true;
			base.CopyFrom(source);
			if (source is LabelBase) {
				LabelBase src = (LabelBase)source;
				this.MaintainOrientation = src.MaintainOrientation;
				this.ShowGluePointLine = src.ShowGluePointLine;
				this.GluePointPosition = src.GluePointPosition;
				this.LabelPosition = src.LabelPosition;
			}
			IsGluePointFollowingConnectionPoint = false;
		}


		/// <override></override>
		public override ControlPointId HitTest(int x, int y, ControlPointCapabilities controlPointCapabilities, int range) {
			if ((controlPointCapabilities & ControlPointCapabilities.Glue) != 0
				|| (controlPointCapabilities & ControlPointCapabilities.Resize) != 0) {
				if (Geometry.DistancePointPoint(x, y, GluePointPosition.X, GluePointPosition.Y) <= range)
					return ControlPointIds.GlueControlPoint;
				if (IsConnected(ControlPointIds.GlueControlPoint, null) == ControlPointId.None
					&& (_pinPath != null && _pinPath.IsVisible(x, y)))
					return ControlPointIds.GlueControlPoint;
			}
			return base.HitTest(x, y, controlPointCapabilities, range);
		}


		/// <override></override>
		public override bool HasControlPointCapability(ControlPointId controlPointId, ControlPointCapabilities controlPointCapability) {
			if (controlPointId == ControlPointIds.GlueControlPoint)
				return ((controlPointCapability & ControlPointCapabilities.Glue) != 0 
					|| (controlPointCapability & ControlPointCapabilities.Resize) != 0);
			else return base.HasControlPointCapability(controlPointId, controlPointCapability);
		}


		/// <override></override>
		public override void Connect(ControlPointId ownPointId, Shape otherShape, ControlPointId otherPointId) {
			if (otherShape == null) throw new ArgumentNullException(nameof(otherShape));
			// Calculate the relative position of the gluePoint on the other shape
			LabelPosition = CalcGluePointCalcInfo(ownPointId, otherShape, otherPointId);
			base.Connect(ownPointId, otherShape, otherPointId);
		}


		/// <override></override>
		public override void Disconnect(ControlPointId gluePointId) {
			base.Disconnect(gluePointId);
			LabelPosition = LabelPositionInfo.Empty;
		}


		/// <override></override>
		public override Point GetControlPointPosition(ControlPointId controlPointId) {
			if (controlPointId == ControlPointIds.GlueControlPoint)
				return GluePointPosition;
			else return base.GetControlPointPosition(controlPointId);
		}


		/// <override></override>
		public override void FollowConnectionPointWithGluePoint(ControlPointId gluePointId, Shape connectedShape, ControlPointId movedPointId) {
			if (connectedShape == null) throw new ArgumentNullException(nameof(connectedShape));

			Rectangle boundsBefore = GetBoundingRectangle(true);
			BeginResize();

			try {
				IsGluePointFollowingConnectionPoint = true;
				Debug.Assert(LabelPosition != LabelPositionInfo.Empty);

				Point currGluePtPos = GluePointPosition;
				Point newGluePtPos = Geometry.InvalidPoint;
				// If the connection is a point-to-shape connection, the shape calculates the new glue point position 
				// with the help of the connected shape. 
				if (movedPointId == ControlPointId.Reference)
					newGluePtPos = CalcGluePoint(gluePointId, connectedShape);
				else 
					newGluePtPos = connectedShape.GetControlPointPosition(movedPointId);
				// Ensure that the glue point is moved to a valid coordinate
				if (!Geometry.IsValid(newGluePtPos)) {
					//System.Diagnostics.Debug.Fail("Invalid glue point position");
					newGluePtPos = currGluePtPos;
				}

				// Calculate new target outline intersection point along with old and new anchor point position
				float shapeAngleDeg = CalculateAngleOfShape(connectedShape, newGluePtPos);
				float posAngleDeg = LabelPosition.Alpha + shapeAngleDeg;

				// Calculate new position of the GlueLabel (calculation method depends on the desired behavior)
				Point newCenter = Point.Round(Geometry.CalcPoint(newGluePtPos.X, newGluePtPos.Y, posAngleDeg, LabelPosition.Distance));
				// Move GlueLabel and update GluePointPos
				MoveTo(newCenter.X, newCenter.Y);
				GluePointPosition = newGluePtPos;

				// Rotate shape if MaintainOrientation is set to false
				if (!MaintainOrientation) {
					int newAngle = Geometry.DegreesToTenthsOfDegree(shapeAngleDeg) + LabelPosition.LabelAngle;
					if (Angle != newAngle) Angle = newAngle;
				}
			} finally {
				IsGluePointFollowingConnectionPoint = false;
			}

			Rectangle boundsAfter = GetBoundingRectangle(true);
			EndResize(boundsAfter.Width - boundsBefore.Width, boundsAfter.Height - boundsBefore.Height);
		}


		/// <summary>
		/// Calculates the angle of the given shape in degrees.
		/// For shapes that do not implement an 'Angle' property, the normal vector at the given position 
		/// is used to determine the angle of the shape (at that position).
		/// </summary>
		protected float CalculateAngleOfShape(Shape shape, Point gluePointPosition) {
			if (shape == null) throw new ArgumentNullException(nameof(shape));
			float shapeAngleDeg = 0;
			if (shape is IPlanarShape planarShape) {
				// Planar shapes: Use the shape's angle
				shapeAngleDeg = Geometry.TenthsOfDegreeToDegrees(planarShape.Angle);
			} else if (shape is ILinearShape linearShape) {
				// Calculate the angle of the tangent through the and use it as shape angle
				Point normalVector = linearShape.CalcNormalVector(gluePointPosition);
				shapeAngleDeg = Geometry.RadiansToDegrees(Geometry.Angle(gluePointPosition.X, gluePointPosition.Y, normalVector.X, normalVector.Y)) + 90;
			} else {
				// Other shapes: Calculate the shape's angle via normal vector at glue point's position
				Point normalVector = shape.CalculateNormalVector(gluePointPosition.X, gluePointPosition.Y);
				shapeAngleDeg = Geometry.RadiansToDegrees(Geometry.Angle(gluePointPosition.X, gluePointPosition.Y, normalVector.X, normalVector.Y)) + 90;
			}
			return shapeAngleDeg;
		}


		/// <override></override>
		public override void Invalidate() {
			if (!SuspendingInvalidation) {
				base.Invalidate();
				if (DisplayService != null && !SuspendingInvalidation) {
					// Invalidate line to GluePoint
					Point p = CalculateConnectionFoot(GluePointPosition.X, GluePointPosition.Y);
					Rectangle b = Rectangle.Empty;
					b.X = Math.Min(GluePointPosition.X, p.X);
					b.Y = Math.Min(GluePointPosition.Y, p.Y);
					b.Width = Math.Max(GluePointPosition.X, p.X) - b.X;
					b.Height = Math.Max(GluePointPosition.Y, p.Y) - b.Y;
					DisplayService.Invalidate(b);

					// Invalidate GluePoint 'Pin' hint
					int left = Math.Min(GluePointPosition.X - (int)Math.Round(PinSize / 2f), X);
					int right = Math.Max(GluePointPosition.X + (int)Math.Round(PinSize / 2f), X);
					int top = Math.Min(GluePointPosition.Y - PinSize, Y);
					int bottom = Math.Max(GluePointPosition.Y, Y);
					DisplayService.Invalidate(left, top, right - left, bottom - top);
				}
			}
		}


		/// <override></override>
		public override void Draw(Graphics graphics) {
			// Draw hint
			DrawHint(graphics);
			// Draw shape and text
			base.Draw(graphics);
			// Draw line to GluePoint
			if (ShowGluePointLine) 
				DrawGluePointLine(graphics, LineStyle, null);

#if DEBUG_DIAGNOSTICS
			//else {
			//    // Draw Line to GluePoint
			//    graphics.DrawLine(Pens.Green, gluePointPos, Center);
			//    GdiHelpers.DrawPoint(graphics, Pens.Green, gluePointPos.X, gluePointPos.Y, 3);

			//    ShapeConnectionInfo ci = GetConnectionInfo(GlueControlPoint, null);
			//    if (ci != ShapeConnectionInfo.Empty) {
			//        Point absPos = ci.OtherShape.CalculateAbsolutePosition(calcInfo.RelativePosition);
			//        GdiHelpers.DrawPoint(graphics, Pens.Red, absPos.X, absPos.Y, 3);
			//        System.Diagnostics.Debug.Print("Relative Position: A = {0}, B = {1}", calcInfo.RelativePosition.A, calcInfo.RelativePosition.B);
			//        System.Diagnostics.Debug.Print("Absolute Position: {0}", absPos);
			//    }

			//    //ShapeConnectionInfo ci = GetConnectionInfo(GlueControlPoint, null);
			//    float shapeAngle = 0;
			//    if (ci.OtherShape is ILinearShape) {
			//        ILinearShape line = (ILinearShape)ci.OtherShape;
			//        Point nv = line.CalcNormalVector(gluePointPos);
			//        graphics.DrawLine(Pens.Red, gluePointPos, nv);
			//        shapeAngle = Geometry.RadiansToTenthsOfDegree(Geometry.Angle(gluePointPos.X, gluePointPos.Y, nv.X, nv.Y)) - 900;
			//    } else if (ci.OtherShape is IPlanarShape) {
			//        shapeAngle = Geometry.TenthsOfDegreeToDegrees(((IPlanarShape)ci.OtherShape).Angle);
			//    }
			//    shapeAngle = shapeAngle / 10f;
			//    GdiHelpers.DrawAngle(graphics, Brushes.Red, gluePointPos, shapeAngle, 10);
			//    GdiHelpers.DrawAngle(graphics, Brushes.Purple, gluePointPos, shapeAngle, calcInfo.Alpha, 10);

			//    string debugStr = string.Format("Partner Angle: {0}°{1}Alpha: {2}°{1}Angle: {3}", shapeAngle, Environment.NewLine, calcInfo.Alpha, Geometry.TenthsOfDegreeToDegrees(Angle));
			//    using (Font font = new Font("Arial", 8))
			//        graphics.DrawString(debugStr, font, Brushes.Blue, gluePointPos);
			//}
#endif
		}


		/// <override></override>
		public override void DrawThumbnail(Image image, int margin, Color transparentColor) {
			GluePointPosition = new Point(X - Width / 2, Y - Height / 2);
			InvalidateDrawCache();
			UpdateDrawCache();
			base.DrawThumbnail(image, margin, transparentColor);
		}


		/// <override></override>
		public override void DrawOutline(Graphics graphics, Pen pen) {
			base.DrawOutline(graphics, pen);
			DrawGluePointLine(graphics, pen);
		}

		#endregion


		#region [Protected] Methods (Inherited)

		/// <override></override>
		protected internal LabelBase(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		/// <override></override>
		protected internal LabelBase(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
		}


		/// <override></override>
		protected internal override void InitializeToDefault(IStyleSet styleSet) {
			base.InitializeToDefault(styleSet);
			Point gluePointPos = Point.Empty;
			gluePointPos.X = (int)Math.Round(X - (Width / 2f)) - 10;
			gluePointPos.Y = (int)Math.Round(Y - (Height / 2f)) - 10;
			GluePointPosition = gluePointPos;
			LabelPosition = LabelPositionInfo.Empty;
			MaintainOrientation = true;
		}


		/// <override></override>
		protected internal override int ControlPointCount {
			get { return base.ControlPointCount + 1; }
		}


		/// <override></override>
		protected override Rectangle CalculateBoundingRectangle(bool tight) {
			Rectangle result = base.CalculateBoundingRectangle(tight);
			if (Geometry.IsValid(GluePointPosition)) {
				// Calculating the pin bounds even if they are not visible.
				// Otherwise, the number of cells calculated by CalculateCells() differs between connected 
				// and unconnected state and the (connected) label would not be removed completely from 
				// the diagram's spatial index.
				if (!tight) {
					_tl.X = _bl.X = GluePointPosition.X - PinSize / 2;
					_tr.X = _br.X = GluePointPosition.X + PinSize / 2;
					_tl.Y = _tr.Y = GluePointPosition.Y - PinSize;
					_bl.Y = _br.Y = GluePointPosition.Y;
					_tl = Geometry.RotatePoint(GluePointPosition, PinAngle, _tl);
					_tr = Geometry.RotatePoint(GluePointPosition, PinAngle, _tr);
					_bl = Geometry.RotatePoint(GluePointPosition, PinAngle, _bl);
					_br = Geometry.RotatePoint(GluePointPosition, PinAngle, _br);
					Rectangle pinBounds;
					Geometry.CalcBoundingRectangle(_tl, _tr, _br, _bl, out pinBounds);
					ShapeUtils.InflateBoundingRectangle(ref pinBounds, LineStyle);
					result = Geometry.UniteRectangles(result, pinBounds);
				} 
			}
			return result;
		}


		/// <override></override>
		protected override bool ContainsPointCore(int x, int y) {
			bool result = base.ContainsPointCore(x, y);
			if (!result) {
				if (ShowGluePointLine)
					result = Geometry.LineContainsPoint(GluePointPosition.X, GluePointPosition.Y, X, Y, true, x, y, (int)Math.Ceiling(LineStyle.LineWidth / 2f));
				if (IsConnected(ControlPointIds.GlueControlPoint, null) == ControlPointId.None) {
					if (_pinPath != null) result = _pinPath.IsVisible(x, y);
					else {
						// Fallback solution: Test on bounding rectangle of the 'pin'
						float halfPinSize = PinSize / 2;
						return (GluePointPosition.X - halfPinSize <= x && x <= GluePointPosition.X + halfPinSize
							&& GluePointPosition.Y - PinSize <= y && y <= GluePointPosition.Y);
					}
				}
			}
			return result;
		}


		/// <override></override>
		protected override bool IntersectsWithCore(int x, int y, int width, int height) {
			bool result = base.IntersectsWithCore(x, y, width, height);
			if (!result) {
				int boundsX = GluePointPosition.X - (PinSize / 2);
				int boundsY = GluePointPosition.Y - PinSize;
				result = Geometry.RectangleIntersectsWithRectangle(x, y, width, height, boundsX, boundsY, PinSize, PinSize)
					|| Geometry.RectangleContainsRectangle(x, y, width, height, boundsX, boundsY, PinSize, PinSize)
					|| Geometry.RectangleIntersectsWithLine(x, y, x + width, y + height, X, Y, GluePointPosition.X, GluePointPosition.Y, true);
			}
			return result;
		}


		/// <override></override>
		protected override bool MoveByCore(int deltaX, int deltaY) {
			bool result = base.MoveByCore(deltaX, deltaY);
			// If the glue point is not connected, move it with the shape
			ShapeConnectionInfo ci = GetConnectionInfo(ControlPointIds.GlueControlPoint, null);
			if (ci.IsEmpty) {
				// Do not set the property as it will invalidate the draw cache
				if (Geometry.IsValid(_gluePointPos))
					_gluePointPos.Offset(deltaX, deltaY);
			} else {
				// If the gluePoint is connected and the shape is not
				// following the connected shape, recalculate GluePointCalcInfo
				if (!IsGluePointFollowingConnectionPoint)
					LabelPosition = CalcGluePointCalcInfo(ci.OwnPointId, ci.OtherShape, ci.OtherPointId);
			}
			return result;
		}


		/// <override></override>
		protected override bool MovePointByCore(ControlPointId pointId, int origDeltaX, int origDeltaY, ResizeModifiers modifiers) {
			if (pointId == ControlPointIds.GlueControlPoint) {
				bool result = false;
				// If the glue ponit is connected, recalculate glue point calculation info
				ShapeConnectionInfo ci = GetConnectionInfo(ControlPointIds.GlueControlPoint, null);
				if (ci.IsEmpty) {
					// If the glue point is not connected, move the glue point to the desired position
					if (Geometry.IsValid(GluePointPosition)) {
						Point p = GluePointPosition;
						p.Offset(origDeltaX, origDeltaY);
						GluePointPosition = p;
						result = true;
					}
				} else {
					if (ci.OtherPointId != ControlPointId.Reference)
						CalcGluePoint(ControlPointIds.GlueControlPoint, ci.OtherShape);
				}
				return result;
			} else return base.MovePointByCore(pointId, origDeltaX, origDeltaY, modifiers);
		}


		/// <override></override>
		protected override bool RotateCore(int deltaAngle, int x, int y) {
			bool result = base.RotateCore(deltaAngle, x, y);
			if (!IsGluePointFollowingConnectionPoint) {
				ShapeConnectionInfo ci = GetConnectionInfo(ControlPointIds.GlueControlPoint, null);
				if (!ci.IsEmpty) {
					// If the gluePoint is connected, recalculate GluePointCalcInfo
					LabelPosition = CalcGluePointCalcInfo(ci.OwnPointId, ci.OtherShape, ci.OtherPointId);
				}
			}
			return result;
		}


		/// <override></override>
		protected override void InvalidateDrawCache() {
			base.InvalidateDrawCache();
			if (ObjectState == ObjectState.Normal) {
				if (_pinPath != null) {
					if (IsConnected(ControlPointIds.GlueControlPoint, null) != ControlPointId.None) {
						_pinPath.Dispose();
						_pinPath = null;
					} else
						_pinPath.Reset();
				}
			}
		}


		/// <override></override>
		protected override void TransformDrawCache(int deltaX, int deltaY, int deltaAngle, int rotationCenterX, int rotationCenterY) {
			base.TransformDrawCache(deltaX, deltaY, deltaAngle, rotationCenterX, rotationCenterY);
			if (_pinPath != null) {
				PinMatrix.Reset();
				PinMatrix.Translate(deltaX, deltaY);
				_pinPath.Transform(PinMatrix);
			}
		}


		/// <override></override>
		protected override bool CalculatePath() {
			if (base.CalculatePath()) {
				// The pin path 
				if (_pinPath == null) _pinPath = new GraphicsPath();
				if (_pinPath.PointCount <= 0) {
					int gpX = GluePointPosition.X - X;
					int gpY = GluePointPosition.Y - Y;

					float halfPinSize = PinSize / 2f;
					float quaterPinSize = PinSize / 4f;
					PointF[] pts = new PointF[3];
					pts[0] = Geometry.GetNearestPoint(gpX, gpY, Geometry.IntersectEllipseLine(gpX, gpY - PinSize + (halfPinSize / 2f), PinSize, halfPinSize, 0, gpX, gpY, gpX - quaterPinSize, gpY - PinSize, false));
					pts[1].X = gpX; pts[1].Y = gpY;
					pts[2] = Geometry.GetNearestPoint(gpX, gpY, Geometry.IntersectEllipseLine(gpX, gpY - PinSize + (halfPinSize / 2f), PinSize, halfPinSize, 0, gpX, gpY, gpX + quaterPinSize, gpY - PinSize, false));
					_pinPath.Reset();
					_pinPath.StartFigure();
					_pinPath.AddLines(pts);
					_pinPath.CloseFigure();
					_pinPath.StartFigure();
					_pinPath.AddEllipse(gpX - halfPinSize, gpY - PinSize, PinSize, halfPinSize);
					_pinPath.CloseFigure();

					PinMatrix.Reset();
					PinMatrix.RotateAt(PinAngle, pts[1]);
					_pinPath.Transform(PinMatrix);
					PinMatrix.Reset();
				}
				return true;
			} else return false;
		}


		/// <override></override>
		protected override void CalcControlPoints() {
			base.CalcControlPoints();
			ControlPoints[ControlPointCount - 1] = GluePointPosition;
		}


		/// <override></override>
		protected override Point CalcGluePoint(ControlPointId gluePointId, Shape shape) {
			InvalidateDrawCache();
			// Get current position of the GluePoint and the new position of the GluePoint
			Point result = Geometry.InvalidPoint;
			ControlPointId pointId = IsConnected(gluePointId, shape);
			Debug.Assert(LabelPosition.RelativePosition != RelativePosition.Empty, "RelativePosition not set!");

			if (pointId == ControlPointId.Reference)
				result = shape.CalculateAbsolutePosition(LabelPosition.RelativePosition);
			else result = shape.GetControlPointPosition(pointId);

			Debug.Assert(Geometry.IsValid(result));
			return result;
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected virtual void DrawGluePointLine(Graphics graphics, ILineStyle lineStyle, ICapStyle capStyle) {
			if (lineStyle == null) throw new ArgumentNullException(nameof(lineStyle));
			Pen pen = ToolCache.GetPen(LineStyle, null, capStyle);
			DrawGluePointLine(graphics, pen);
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected virtual void DrawGluePointLine(Graphics graphics, Pen pen) {
			if (graphics == null) throw new ArgumentNullException(nameof(graphics));
			Point p = CalculateConnectionFoot(GluePointPosition.X, GluePointPosition.Y);
			if (pen != null && Geometry.IsValid(p)) 
				graphics.DrawLine(pen, p, GluePointPosition);
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected virtual void DrawHint(Graphics graphics) {
			// Draw connection point hints
			if (IsConnected(ControlPointIds.GlueControlPoint, null) == ControlPointId.None) {
				if (DisplayService != null) {
					Pen foregroundPen = ToolCache.GetPen(DisplayService.HintForegroundStyle, null, null);
					Brush backgroundBrush = ToolCache.GetBrush(DisplayService.HintBackgroundStyle);

					DrawOutline(graphics, foregroundPen);
					graphics.FillPath(backgroundBrush, _pinPath);
					graphics.DrawPath(foregroundPen, _pinPath);
				}
			}
		}

		#endregion


		#region [Protected] Properties

		/// <summary>Specifies whether the shape shows the line between the shape's outline and its GluePoint</summary>
		protected bool ShowGluePointLine {
			get { return _showGluePointLine; }
			set {
				if (_showGluePointLine != value) {
					_showGluePointLine = value;
					Invalidate();
				}
			}
		}


		/// <summary>Position of the glue point (unrotated)</summary>
		protected Point GluePointPosition {
			get { return _gluePointPos; }
			set {
				if (_gluePointPos != value) {
					Invalidate();
					_gluePointPos = value;
					InvalidateDrawCache();
					Invalidate();
				}
			}
		}
		

		/// <summary>Contains parameters for calculating the </summary>
		protected LabelPositionInfo LabelPosition {
			get { return _labelPositionInfo; }
			set {
				if (_labelPositionInfo != value) {
					Invalidate();
					_labelPositionInfo = value;
					InvalidateDrawCache();
					Invalidate();
				}
			}
		}

		#endregion


		/// <ToBeCompleted></ToBeCompleted>
		protected struct LabelPositionInfo : IEquatable<LabelPositionInfo> {

			/// <ToBeCompleted></ToBeCompleted>
			public static readonly LabelPositionInfo Empty;

			/// <ToBeCompleted></ToBeCompleted>
			public static bool operator !=(LabelPositionInfo x, LabelPositionInfo y) { return !(x == y); }

			/// <ToBeCompleted></ToBeCompleted>
			public static bool operator ==(LabelPositionInfo x, LabelPositionInfo y) {
				return (
					((float.IsNaN(x.Alpha) == float.IsNaN(y.Alpha) && float.IsNaN(x.Alpha))
					 || (x.Alpha == y.Alpha && !float.IsNaN(x.Alpha)))
					&& ((float.IsNaN(x.Beta) == float.IsNaN(y.Beta) && float.IsNaN(x.Beta))
					 || (x.Beta == y.Beta && !float.IsNaN(x.Beta)))
					&& x.RelativePosition == y.RelativePosition
					&& ((float.IsNaN(x.Distance) && float.IsNaN(y.Distance) && float.IsNaN(x.Distance))
					|| (x.Distance == y.Distance && !float.IsNaN(x.Distance))));
			}

			/// <summary>
			/// Distance between the target shape's outline and the anchor point.
			/// </summary>
			public float Distance;

			/// <summary>
			/// The relative position of the GluePoint on the target shape. 
			/// The target shape can calculate the new absolute position of the gluepoint using the RelativePosition.
			/// </summary>
			public RelativePosition RelativePosition;

			/// <summary>
			/// Angle between the normal vector of the target shape's outline intersection and the line through GluePoint and X|Y
			/// </summary>
			public float Alpha;

			/// <summary>
			/// Angle between the normal vector of this Label shape's outline intersection and the line through GluePoint and X|Y
			/// </summary>
			public float Beta;

			/// <summary>
			/// The own rotation of the label
			/// </summary>
			public int LabelAngle;

			/// <override></override>
			public override bool Equals(object obj) {
				return obj is LabelPositionInfo && this == (LabelPositionInfo)obj;
			}

			/// <override></override>
			public bool Equals(LabelPositionInfo other) {
				return other == this;
			}

			/// <override></override>
			public override int GetHashCode() {
				return HashCodeGenerator.CalculateHashCode(Distance, RelativePosition, Alpha, Beta, LabelAngle);
			}

			static LabelPositionInfo() {
				Empty.Alpha = float.NaN;
				Empty.Beta = float.NaN;
				Empty.RelativePosition = RelativePosition.Empty;
				Empty.Distance = float.NaN;
				Empty.LabelAngle = 0;
			}

		}


		#region IEntity Members (Explicit Implementation)

		/// <summary>
		/// Retrieves the persistable properties of <see cref="T:Dataweb.NShape.Advanced.LabelBase" />.
		/// </summary>
		new public static IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			foreach (EntityPropertyDefinition pi in TextBase.GetPropertyDefinitions(version))
				yield return pi;
			yield return new EntityFieldDefinition("GluePointX", typeof(int));
			yield return new EntityFieldDefinition("GluePointY", typeof(int));
			if (version >= 3) yield return new EntityFieldDefinition("MaintainOrientation", typeof(bool));
		}


		/// <override></override>
		protected override void LoadFieldsCore(IRepositoryReader reader, int version) {
			base.LoadFieldsCore(reader, version);
			Point gluePointPos = Point.Empty;
			gluePointPos.X = reader.ReadInt32();
			gluePointPos.Y = reader.ReadInt32();
			GluePointPosition = gluePointPos;
			if (version >= 3) MaintainOrientation = reader.ReadBool();
		}


		/// <override></override>
		protected override void SaveFieldsCore(IRepositoryWriter writer, int version) {
			base.SaveFieldsCore(writer, version);
			writer.WriteInt32(GluePointPosition.X);
			writer.WriteInt32(GluePointPosition.Y);
			if (version >= 3) writer.WriteBool(MaintainOrientation);
		}

		#endregion


		private Matrix PinMatrix {
			get {
				if (_pinMatrix == null) _pinMatrix = new Matrix();
				return _pinMatrix;
			}
		}


		/// <summary>
		/// Calculates and returns the positioning of the label relative to its partner shape.
		/// </summary>
		protected virtual LabelPositionInfo CalcGluePointCalcInfo(ControlPointId gluePointId, Shape otherShape, ControlPointId otherPointId) {
			// Calculate GluePoint position and AnchorPoint position
			Point gluePtPos = GetControlPointPosition(gluePointId);
			Point labelPos = Point.Empty;
			labelPos.Offset(X, Y);

			// Calculate target shape's outline intersection point and the relative position of the gluePoint in/on the target shape
			float shapeAngleDeg = CalculateAngleOfShape(otherShape, gluePtPos);
			float posAngleDeg = Geometry.RadiansToDegrees(Geometry.Angle(gluePtPos, labelPos));

			// Calculate and store label's relative position info
			LabelPositionInfo position = LabelPositionInfo.Empty;
			position.Alpha = (360 + posAngleDeg - shapeAngleDeg) % 360;
			position.Beta = (360 + Geometry.RadiansToDegrees(Geometry.Angle(labelPos, gluePtPos))) % 360;
			position.LabelAngle = (Angle - Geometry.DegreesToTenthsOfDegree(shapeAngleDeg)) % 3600;
			position.Distance = Geometry.DistancePointPoint(gluePtPos, labelPos);
			position.RelativePosition = otherShape.CalculateRelativePosition(gluePtPos.X, gluePtPos.Y);

			Debug.Assert(position != LabelPositionInfo.Empty);
			Debug.Assert(position.RelativePosition != RelativePosition.Empty);

			return position;
		}


		#region Fields

		private Point _gluePointPos = Geometry.InvalidPoint;
		private LabelPositionInfo _labelPositionInfo = LabelPositionInfo.Empty;
		private bool _maintainOrientation;
		private bool _showGluePointLine = false;

		private const int PinSize = 12;
		private const int PinAngle = 45;
		private GraphicsPath _pinPath;
		private Matrix _pinMatrix;

		// Specifies if the movement of a connected label is due to repositioning or 
		// due to a "FollowConnectionPointWithGluePoint" call
		//private bool _followingConnectedShape = false;

		// Buffers
		private Point _tl = Point.Empty;
		private Point _tr = Point.Empty;
		private Point _bl = Point.Empty;
		private Point _br = Point.Empty;
		#endregion

	}

}
