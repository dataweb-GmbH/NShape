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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;


namespace Dataweb.NShape.Advanced {

	/// <ToBeCompleted></ToBeCompleted>
	public static class Geometry {

		/// <summary>
		/// Returns true if both numbers are regarded as equal.
		/// </summary>
		public static bool Equals(double a, double b) {
			return Math.Abs(a - b) < EqualityDeltaDouble;
		}


		/// <summary>
		/// Returns true if both numbers are regarded as equal.
		/// </summary>
		public static bool Equals(float a, float b) {
			return Math.Abs(a - b) < EqualityDeltaFloat;
		}


		#region Calculations with Rectangles

		/// <summary>
		/// Returns the center point of the rectangle.
		/// </summary>
		public static Point GetCenter(this Rectangle rect) {
			Point result = Point.Empty;
			result.Offset((int)Math.Round(rect.X + rect.Width / 2f), (int)Math.Round(rect.X + rect.Height / 2f));
			return result;
		}


		/// <summary>
		/// Returns the center point of the rectangle.
		/// </summary>
		public static PointF GetCenter(this RectangleF rect) {
			PointF result = PointF.Empty;
			result.X = rect.X + rect.Width / 2f;
			result.Y = rect.X + rect.Height / 2f;
			return result;
		}


		/// <summary>
		/// Returns the center point of the rectangle.
		/// </summary>
		public static PointF GetCenterF(this Rectangle rect) {
			PointF result = PointF.Empty;
			result.X = rect.X + rect.Width / 2f;
			result.Y = rect.X + rect.Height / 2f;
			return result;
		}


		/// <summary>
		/// Merges two rectangles to a new rectangle that contains all the points of the two.
		/// </summary>
		public static Rectangle UniteRectangles(int left, int top, int right, int bottom, Rectangle rectangle) {
			AssertIsValidLTRB(left, top, right, bottom);
			AssertIsValid(rectangle);
			//
			Rectangle result = Rectangle.Empty;
			if (rectangle.IsEmpty) {
				result.X = left;
				result.Y = top;
				result.Width = right - left;
				result.Height = bottom - top;
			} else {
				result.X = Math.Min(left, rectangle.X);
				result.Y = Math.Min(top, rectangle.Y);
				result.Width = Math.Max(right, rectangle.Right) - result.X;
				result.Height = Math.Max(bottom, rectangle.Bottom) - result.Y;
			}
			return result;
		}


		///// <summary>
		///// Merges two rectangles to a new rectangle that contains all the points of the two.
		///// </summary>
		//public static Rectangle UniteRectangles(Rectangle a, Rectangle b) {
		//    const string excMsgFormat = "{0} is not a valid value for parameter '{1}'";
		//    if (!IsValid(a)) throw new ArgumentException(string.Format(excMsgFormat, a, "a"));
		//    if (!IsValid(b)) throw new ArgumentException(string.Format(excMsgFormat, b, "b"));
		//    //
		//    if (a.IsEmpty) return b;
		//    if (b.IsEmpty) return a;
		//    Rectangle result = Rectangle.Empty;
		//    result.X = Math.Min(a.X, b.X);
		//    result.Y = Math.Min(a.Y, b.Y);
		//    result.Width = Math.Max(a.Right, b.Right) - result.X;
		//    result.Height = Math.Max(a.Bottom, b.Bottom) - result.Y;
		//    return result;
		//}


		///// <summary>
		///// Merges two rectangles to a new rectangle that contains all the points of the two.
		///// </summary>
		//public static Rectangle UniteRectangles(RectangleF a, Rectangle b) {
		//    const string excMsgFormat = "{0} is not a valid value for parameter '{1}'";
		//    if (!IsValid(a)) throw new ArgumentException(string.Format(excMsgFormat, a, "a"));
		//    if (!IsValid(b)) throw new ArgumentException(string.Format(excMsgFormat, b, "b"));
		//    //
		//    if (a.IsEmpty) return b;
		//    if (b.IsEmpty) return Rectangle.Round(a);
		//    Rectangle result = Rectangle.Empty;
		//    result.X = Math.Min((int)Math.Floor(a.X), b.X);
		//    result.Y = Math.Min((int)Math.Floor(a.Y), b.Y);
		//    result.Width = Math.Max((int)Math.Ceiling(a.Right), b.Right) - result.X;
		//    result.Height = Math.Max((int)Math.Ceiling(a.Bottom), b.Bottom) - result.Y;
		//    return result;
		//}


		/// <summary>
		/// Merges two rectangles to a new rectangle that contains all the points of the two.
		/// </summary>
		public static Rectangle UniteRectangles(Rectangle a, Rectangle b) {
			if (!IsValid(a)) return b;
			if (!IsValid(b)) return a;
			Rectangle result = Rectangle.Empty;
			result.X = Math.Min(a.X, b.X);
			result.Y = Math.Min(a.Y, b.Y);
			result.Width = Math.Max(a.Right, b.Right) - result.X;
			result.Height = Math.Max(a.Bottom, b.Bottom) - result.Y;
			return result;
		}


		/// <summary>
		/// Merges two rectangles to a new rectangle that contains all the points of the two.
		/// </summary>
		public static Rectangle UniteRectangles(RectangleF a, Rectangle b) {
			if (!IsValid(a)) return b;
			if (!IsValid(b)) return Rectangle.Round(a);
			Rectangle result = Rectangle.Empty;
			result.X = Math.Min((int)Math.Floor(a.X), b.X);
			result.Y = Math.Min((int)Math.Floor(a.Y), b.Y);
			result.Width = Math.Max((int)Math.Ceiling(a.Right), b.Right) - result.X;
			result.Height = Math.Max((int)Math.Ceiling(a.Bottom), b.Bottom) - result.Y;
			return result;
		}


		/// <summary>
		/// Merges a point and a rectangle to a new rectangle that contains all the points of the two.
		/// </summary>
		public static Rectangle UniteWithRectangle(Point p, Rectangle rect) {
			AssertIsValid(p);
			AssertIsValid(rect);
			if (p.X < rect.X) {
				rect.Width += rect.X - p.X;
				rect.X = p.X;
			} else if (p.X > rect.Right)
				rect.Width = p.X - rect.X;
			if (p.Y < rect.Y) {
				rect.Height += rect.Y - p.Y;
				rect.Y = p.Y;
			} else if (p.Y > rect.Bottom)
				rect.Height = p.Y - rect.Y;
			return rect;
		}


		/// <summary>
		/// Enlarges the rectangle to include a given point.
		/// </summary>
		/// <param name="r"></param>
		/// <param name="p"></param>
		public static void IncludeRectanglePoint(ref Rectangle r, Point p) {
			if (p.X < r.Left) { r.Width = r.Right - p.X; r.X = p.X; }
			if (p.X > r.Right) r.Width = p.X - r.Left;
			if (p.Y < r.Top) { r.Height = r.Bottom - p.Y; r.Y = p.Y; }
			if (p.Y > r.Bottom) r.Height = p.Y - r.Top;
		}

		#endregion


		#region Helpers for moving and resizing shapes

		/// <summary>
		/// Transforms the mouse movement into the coordinate system of the (rotated) shape.
		/// </summary>
		/// <param name="deltaX">Mouse movement on X axis</param>
		/// <param name="deltaY">Mouse movement on Y axis</param>
		/// <param name="angleTenthsOfDeg">Angle of the rotated shape in tenths of degrees</param>
		/// <param name="modifiers">Movement/Resizing modifiers</param>
		/// <param name="divFactorX">Ensures that transformedDeltaX is dividable by this factor without remainder</param>
		/// <param name="divFactorY">Ensures that transformedDeltaY is dividable by this factor without remainder</param>
		/// <param name="transformedDeltaX">Transformed mouse movement on X axis</param>
		/// <param name="transformedDeltaY">Transformed mouse movement on Y axis</param>
		/// <param name="sin">Sinus value of the given angle</param>
		/// <param name="cos">Cosinus value of the given angle</param>
		[Obsolete("Use the other version of TransformMouseMovement in conjunction with AlignMovement instead.")]
		public static bool TransformMouseMovement(int deltaX, int deltaY, int angleTenthsOfDeg, ResizeModifiers modifiers, int divFactorX, int divFactorY,
			out float transformedDeltaX, out float transformedDeltaY, out float sin, out float cos) {
			TransformMouseMovement(deltaX, deltaY, angleTenthsOfDeg, out transformedDeltaX, out transformedDeltaY, out sin, out cos);
			return AlignMovement(modifiers, divFactorX, divFactorY, ref transformedDeltaX, ref transformedDeltaY);
		}


		/// <summary>
		/// Transforms the mouse movement into the coordinate system of the (rotated) shape.
		/// </summary>
		/// <param name="deltaX">Mouse movement on X axis</param>
		/// <param name="deltaY">Mouse movement on Y axis</param>
		/// <param name="angleTenthsOfDeg">Angle of the rotated shape in tenths of degrees</param>
		/// <param name="transformedDeltaX">Transformed mouse movement on X axis</param>
		/// <param name="transformedDeltaY">Transformed mouse movement on Y axis</param>
		/// <param name="sin">Sinus value of the given angle</param>
		/// <param name="cos">Cosinus value of the given angle</param>
		public static void TransformMouseMovement(int deltaX, int deltaY, int angleTenthsOfDeg, out float transformedDeltaX, out float transformedDeltaY, out float sin, out float cos) {
			// Rotate the mouse movement
			float ang = TenthsOfDegreeToRadians(angleTenthsOfDeg);
			cos = (float)Math.Cos(ang);
			sin = (float)Math.Sin(ang);
			transformedDeltaX = (float)((deltaX * cos) + (deltaY * sin));
			transformedDeltaY = (float)((deltaY * cos) - (deltaX * sin));
		}


		/// <summary>
		/// Aligns the (transformed) movement to pixels in order to ensure that the shape or its control points are moved to full integer coordinates.
		/// Returns false if the movement was modified.
		/// </summary>
		/// <param name="modifiers">Movement/Resizing modifiers</param>
		/// <param name="divFactorX">Ensures that transformedDeltaX is dividable by this factor without remainder</param>
		/// <param name="divFactorY">Ensures that transformedDeltaY is dividable by this factor without remainder</param>
		/// <param name="deltaX">Mouse movement on X axis</param>
		/// <param name="deltaY">Mouse movement on Y axis</param>
		public static bool AlignMovement(ResizeModifiers modifiers, int divFactorX, int divFactorY, ref float deltaX, ref float deltaY) {
			bool result = true;
			// Ensure that the mouse movement will be devidable without remainder 
			// when *not* resizing in both directions
			if ((modifiers & ResizeModifiers.MirroredResize) == 0) {
				if (deltaX % divFactorX != 0) {
					deltaX -= deltaX % divFactorX;
					result = false;
				}
				if (deltaY % divFactorY != 0) {
					deltaY -= deltaY % divFactorY;
					result = false;
				}
			}
			return result;
		}


		/// <summary>
		/// Corrects deltaX and deltaY in order to preserve the aspect ratio of the sources bounds when inflating by deltaX and deltaY.
		/// Return true if deltaX / deltaY were left unchanged or false if they were corrected.
		/// </summary>
		/// <param name="width">The current width</param>
		/// <param name="height">The current height</param>
		/// <param name="growDirectionX">Specifies if deltaX is added (1) or subtracted (-1) in order to grow the shape.</param>
		/// <param name="growDirectionY">Specifies if deltaY is added (1) or subtracted (-1) in order to grow the shape.</param>
		/// <param name="deltaX">Specifies the growth of the shape in X direction.</param>
		/// <param name="deltaY">Specifies the growth of the shape in Y direction.</param>
		public static bool MaintainAspectRatio(float width, float height, sbyte growDirectionX, sbyte growDirectionY, ref float deltaX, ref float deltaY) {
			if (width == 0) throw new ArgumentException("width");
			if (height == 0) throw new ArgumentException("height");
			if (!(growDirectionX == 1 || growDirectionX == -1)) throw new ArgumentException("growDirectionX has to be 1 or -1.");
			if (!(growDirectionY == 1 || growDirectionY == -1)) throw new ArgumentException("growDirectionX has to be 1 or -1.");
			bool result = true;
			// Calculate aspect ratio and resulting rectangles
			float aspectRatio = width / (float)height;
			int dstWidthX = (int)Math.Round(width + (deltaX * growDirectionX), MidpointRounding.ToEven);
			int dstHeightX = (int)Math.Round(dstWidthX / aspectRatio, MidpointRounding.ToEven);
			float dstHeightY = (int)Math.Round(height + (deltaY * growDirectionY), MidpointRounding.ToEven);
			float dstWidthY = (int)Math.Round(dstHeightY * aspectRatio, MidpointRounding.ToEven);
			// 
			if (dstWidthX <= dstWidthY && dstHeightX <= dstHeightY) {
				float newDeltaY = ((dstWidthX / aspectRatio) - height) * growDirectionY;
				result = (deltaY == newDeltaY);
				deltaY = newDeltaY;
			} else if (dstWidthY <= dstWidthX && dstHeightY <= dstHeightX) {
				float newDeltaX = ((dstHeightY * aspectRatio) - width) * growDirectionX;
				result = (deltaX == newDeltaX);
				deltaX = newDeltaX;
			} else { Debug.Fail("Undefined case!"); }

			//float dstWidth = width + (deltaX * growDirectionX);
			//float dstHeight = height + (deltaY * growDirectionY);
			//float newDeltaX = deltaX;
			//float newDeltaY = deltaY;
			//if (Math.Abs(deltaX) <= Math.Abs(deltaY)) {
			//    newDeltaY = ((dstWidth / aspectRatio) - height) * growDirectionY;
			//} else {
			//    newDeltaX = ((dstHeight * aspectRatio) - width) * growDirectionX;
			//}
			//deltaX = newDeltaX;
			//deltaY = newDeltaY;

			return result;
		}


		/// <summary>
		/// Moves the TopTeft corner of a (rotated) rectangle in order to resize it
		/// </summary>
		/// <param name="width">Width of the rectangle</param>
		/// <param name="height">Height of the rectangle</param>
		/// <param name="deltaX">Movement on X axis</param>
		/// <param name="deltaY">Movement on Y axis</param>
		/// <param name="cosAngle">Cosinus value of the rectangle's rotation angle</param>
		/// <param name="sinAngle">Sinus value of the rectangle's rotation angle</param>
		/// <param name="modifiers">Movement modifiers</param>
		/// <param name="centerOffsetX">Specifies the movement the rectangle's center has to perform for resizing</param>
		/// <param name="centerOffsetY">Specifies the movement the rectangle's center has to perform for resizing</param>
		/// <param name="newWidth">New width of the rectangle</param>
		/// <param name="newHeight">New height of the rectangle</param>
		/// <returns>Returns true if the movement could be performed as desired. 
		/// Returns false if the movement could not be performed completely because 
		/// of movement restrictions</returns>
		public static bool MoveRectangleTopLeft(int width, int height, float deltaX, float deltaY,
			float cosAngle, float sinAngle, ResizeModifiers modifiers, out int centerOffsetX,
			out int centerOffsetY, out int newWidth, out int newHeight) {
			int minWidth = 0, minHeight = 0;
			GetMinValues(width, height, modifiers, out minWidth, out minHeight);
			return MoveRectangleTopLeft(width, height, minWidth, minHeight, 0.5f, 0.5f, 2, 2, deltaX,
				deltaY, cosAngle, sinAngle, modifiers, out centerOffsetX, out centerOffsetY, out newWidth, out newHeight);
		}


		/// <summary>
		/// Moves the TopTeft corner of a (rotated) rectangle in order to resize it
		/// </summary>
		/// <param name="width">Width of the rectangle</param>
		/// <param name="height">Height of the rectangle</param>
		/// <param name="minValueX">Specifies the minimum width.</param>
		/// <param name="minValueY">Specifies the minimum height.</param>
		/// <param name="centerPosFactorX">Specifies where the center is located. Default value is 50% = 0.5</param>
		/// <param name="centerPosFactorY">Specifies where the center is located. Default value is 50% = 0.5</param>
		/// <param name="divFactorX">Specifies the factor through which the corrected and modified movement must be dividable without remainding.</param>
		/// <param name="divFactorY">Specifies the factor through which the corrected and modified movement must be dividable without remainding.</param>
		/// <param name="deltaX">Movement on X axis</param>
		/// <param name="deltaY">Movement on Y axis</param>
		/// <param name="cosAngle">Cosinus value of the rectangle's rotation angle</param>
		/// <param name="sinAngle">Sinus value of the rectangle's rotation angle</param>
		/// <param name="modifiers">Movement modifiers</param>
		/// <param name="centerOffsetX">Specifies the movement the rectangle's center has to perform for resizing</param>
		/// <param name="centerOffsetY">Specifies the movement the rectangle's center has to perform for resizing</param>
		/// <param name="newWidth">New width of the rectangle</param>
		/// <param name="newHeight">New height of the rectangle</param>
		/// <returns>Returns true if the movement could be performed as desired. 
		/// Returns false if the movement could not be performed completely because of movement restrictions</returns>
		public static bool MoveRectangleTopLeft(int width, int height, int minValueX, int minValueY,
			float centerPosFactorX, float centerPosFactorY, int divFactorX, int divFactorY,
			float deltaX, float deltaY, float cosAngle, float sinAngle, ResizeModifiers modifiers,
			out int centerOffsetX, out int centerOffsetY, out int newWidth, out int newHeight) {
			bool result = true;
			centerOffsetX = centerOffsetY = 0;
			newWidth = width;
			newHeight = height;
			// Maintain aspect (if needed)
			if ((modifiers & ResizeModifiers.MaintainAspect) != 0)
				result = MaintainAspectRatio(width, height, -1, -1, ref deltaX, ref deltaY);
			// Align to integer coordinates
			Geometry.AlignMovement(modifiers, divFactorX, divFactorY, ref deltaX, ref deltaY);

			if ((modifiers & ResizeModifiers.MirroredResize) != 0) {
				if (newWidth - deltaX - deltaX >= minValueX)
					newWidth -= (int)Math.Round(deltaX + deltaX);
				else {
					newWidth = minValueX;
					result = false;
				}
				if (newHeight - deltaY - deltaY >= minValueY)
					newHeight -= (int)Math.Round(deltaY + deltaY);
				else {
					newHeight = minValueY;
					result = false;
				}
			} else {
				if (newWidth - deltaX >= minValueX) {
					newWidth -= (int)Math.Round(deltaX);
				} else {
					deltaX = newWidth;
					newWidth = minValueX;
					result = false;
				}
				if (newHeight - deltaY >= minValueY) {
					newHeight -= (int)Math.Round(deltaY);
				} else {
					deltaY = newHeight;
					newHeight = minValueY;
					result = false;
				}
				centerOffsetX = (int)Math.Round((deltaX * centerPosFactorX * cosAngle) - (deltaY * (1 - centerPosFactorY) * sinAngle));
				centerOffsetY = (int)Math.Round((deltaX * centerPosFactorX * sinAngle) + (deltaY * (1 - centerPosFactorY) * cosAngle));
			}
			return result;
		}


		/// <summary>
		/// Moves the top side of a (rotated) rectangle in order to resize it
		/// </summary>
		/// <param name="width">Width of the rectangle</param>
		/// <param name="height">Height of the rectangle</param>
		/// <param name="deltaX">Movement on X axis</param>
		/// <param name="deltaY">Movement on Y axis</param>
		/// <param name="cosAngle">Cosinus value of the rectangle's rotation angle</param>
		/// <param name="sinAngle">Sinus value of the rectangle's rotation angle</param>
		/// <param name="modifiers">Movement modifiers</param>
		/// <param name="centerOffsetX">Specifies the movement the rectangle's center has to perform for resizing</param>
		/// <param name="centerOffsetY">Specifies the movement the rectangle's center has to perform for resizing</param>
		/// <param name="newWidth">New width of the rectangle</param>
		/// <param name="newHeight">New height of the rectangle</param>
		/// <returns>Returns true if the movement could be performed as desired. 
		/// Returns false if the movement could not be performed completely because of movement restrictions</returns>
		public static bool MoveRectangleTop(int width, int height, float deltaX, float deltaY, float cosAngle, float sinAngle, ResizeModifiers modifiers,
			out int centerOffsetX, out int centerOffsetY, out int newWidth, out int newHeight) {
			int minWidth = 0, minHeight = 0;
			GetMinValues(width, height, modifiers, out minWidth, out minHeight);
			return MoveRectangleTop(width, height, minHeight, 0.5f, 0.5f, 2, 2, deltaX, deltaY, cosAngle, sinAngle, modifiers, out centerOffsetX, out centerOffsetY, out newWidth, out newHeight);
		}


		/// <summary>
		/// Moves the top side of a (rotated) rectangle in order to resize it
		/// </summary>
		/// <param name="width">Width of the rectangle</param>
		/// <param name="height">Height of the rectangle</param>
		/// <param name="minValueY">Specifies the minimum height.</param>
		/// <param name="centerPosFactorX">Specifies where the center is located. Default value is 50% = 0.5</param>
		/// <param name="centerPosFactorY">Specifies where the center is located. Default value is 50% = 0.5</param>
		/// <param name="divFactorX">Specifies the factor through which the corrected and modified movement must be dividable without remainding.</param>
		/// <param name="divFactorY">Specifies the factor through which the corrected and modified movement must be dividable without remainding.</param>
		/// <param name="deltaX">Movement on X axis</param>
		/// <param name="deltaY">Movement on Y axis</param>
		/// <param name="cosAngle">Cosinus value of the rectangle's rotation angle</param>
		/// <param name="sinAngle">Sinus value of the rectangle's rotation angle</param>
		/// <param name="modifiers">Movement modifiers</param>
		/// <param name="centerOffsetX">Specifies the movement the rectangle's center has to perform for resizing</param>
		/// <param name="centerOffsetY">Specifies the movement the rectangle's center has to perform for resizing</param>
		/// <param name="newWidth">New width of the rectangle</param>
		/// <param name="newHeight">New height of the rectangle</param>
		/// <returns>Returns true if the movement could be performed as desired. 
		/// Returns false if the movement could not be performed completely because of movement restrictions</returns>
		public static bool MoveRectangleTop(int width, int height, int minValueY,
			float centerPosFactorX, float centerPosFactorY, int divFactorX, int divFactorY,
			float deltaX, float deltaY, double cosAngle, double sinAngle, ResizeModifiers modifiers,
			out int centerOffsetX, out int centerOffsetY, out int newWidth, out int newHeight) {
			bool result = true;
			centerOffsetX = centerOffsetY = 0;
			newWidth = width;
			newHeight = height;

			if ((modifiers & ResizeModifiers.MaintainAspect) != 0) {
				//MaintainAspectRatio(width, height, 1, -1, ref deltaX, ref deltaY);
				float aspectRatio = width / (float)height;
				deltaX = deltaY * aspectRatio;
			}
			// Align to integer coordinates
			Geometry.AlignMovement(modifiers, divFactorX, divFactorY, ref deltaX, ref deltaY);

			if ((modifiers & ResizeModifiers.MirroredResize) != 0) {
				if (newHeight - deltaY - deltaY >= minValueY)
					newHeight -= (int)Math.Round(deltaY + deltaY);
				else {
					newHeight = minValueY;
					result = false;
				}
			} else {
				if (newHeight - deltaY >= minValueY)
					newHeight -= (int)Math.Round(deltaY);
				else {
					deltaY = newHeight;
					newHeight = minValueY;
					result = false;
				}
				centerOffsetX = (int)Math.Round(-(deltaY * (1 - centerPosFactorY) * sinAngle));
				centerOffsetY = (int)Math.Round((deltaY * (1 - centerPosFactorY) * cosAngle));
			}
			if ((modifiers & ResizeModifiers.MaintainAspect) != 0)
				newWidth = (int)Math.Round(newHeight * (width / (float)height));
			return result;
		}


		/// <summary>
		/// Moves the TopRight corner of a (rotated) rectangle in order to resize it
		/// </summary>
		/// <param name="width">Width of the rectangle</param>
		/// <param name="height">Height of the rectangle</param>
		/// <param name="deltaX">Movement on X axis</param>
		/// <param name="deltaY">Movement on Y axis</param>
		/// <param name="cosAngle">Cosinus value of the rectangle's rotation angle</param>
		/// <param name="sinAngle">Sinus value of the rectangle's rotation angle</param>
		/// <param name="modifiers">Movement modifiers</param>
		/// <param name="centerOffsetX">Specifies the movement the rectangle's center has to perform for resizing</param>
		/// <param name="centerOffsetY">Specifies the movement the rectangle's center has to perform for resizing</param>
		/// <param name="newWidth">New width of the rectangle</param>
		/// <param name="newHeight">New height of the rectangle</param>
		/// <returns>Returns true if the movement could be performed as desired. 
		/// Returns false if the movement could not be performed completely because of movement restrictions</returns>
		public static bool MoveRectangleTopRight(int width, int height, float deltaX, float deltaY, float cosAngle, float sinAngle, ResizeModifiers modifiers,
			out int centerOffsetX, out int centerOffsetY, out int newWidth, out int newHeight) {
			int minWidth = 0, minHeight = 0;
			GetMinValues(width, height, modifiers, out minWidth, out minHeight);
			return MoveRectangleTopRight(width, height, minWidth, minHeight, 0.5f, 0.5f, 2, 2, deltaX, deltaY, cosAngle, sinAngle, modifiers, out centerOffsetX, out centerOffsetY, out newWidth, out newHeight);
		}


		/// <summary>
		/// Moves the TopRight corner of a (rotated) rectangle in order to resize it
		/// </summary>
		/// <param name="width">Width of the rectangle</param>
		/// <param name="height">Height of the rectangle</param>
		/// <param name="minValueX">Specifies the minimum width.</param>
		/// <param name="minValueY">Specifies the minimum height.</param>
		/// <param name="centerPosFactorX">Specifies where the center is located. Default value is 50% = 0.5</param>
		/// <param name="centerPosFactorY">Specifies where the center is located. Default value is 50% = 0.5</param>
		/// <param name="divFactorX">Specifies the factor through which the corrected and modified movement must be dividable without remainding.</param>
		/// <param name="divFactorY">Specifies the factor through which the corrected and modified movement must be dividable without remainding.</param>
		/// <param name="deltaX">Movement on X axis</param>
		/// <param name="deltaY">Movement on Y axis</param>
		/// <param name="cosAngle">Cosinus value of the rectangle's rotation angle</param>
		/// <param name="sinAngle">Sinus value of the rectangle's rotation angle</param>
		/// <param name="modifiers">Movement modifiers</param>
		/// <param name="centerOffsetX">Specifies the movement the rectangle's center has to perform for resizing</param>
		/// <param name="centerOffsetY">Specifies the movement the rectangle's center has to perform for resizing</param>
		/// <param name="newWidth">New width of the rectangle</param>
		/// <param name="newHeight">New height of the rectangle</param>
		/// <returns>Returns true if the movement could be performed as desired. 
		/// Returns false if the movement could not be performed completely because of movement restrictions</returns>
		public static bool MoveRectangleTopRight(int width, int height, int minValueX, int minValueY,
			float centerPosFactorX, float centerPosFactorY, int divFactorX, int divFactorY,
			float deltaX, float deltaY, float cosAngle, float sinAngle, ResizeModifiers modifiers,
			out int centerOffsetX, out int centerOffsetY, out int newWidth, out int newHeight) {
			bool result = true;
			centerOffsetX = centerOffsetY = 0;
			newWidth = width;
			newHeight = height;
			// Aspect maintainance can be combined with both MirroredResizing and normal resizing
			if ((modifiers & ResizeModifiers.MaintainAspect) != 0)
				MaintainAspectRatio(width, height, 1, -1, ref deltaX, ref deltaY);
			// Align to integer coordinates
			Geometry.AlignMovement(modifiers, divFactorX, divFactorY, ref deltaX, ref deltaY);

			if ((modifiers & ResizeModifiers.MirroredResize) != 0) {
				if (newWidth + deltaX + deltaX >= minValueX)
					newWidth += (int)Math.Round(deltaX + deltaX);
				else {
					newWidth = minValueX;
					result = false;
				}
				if (newHeight - deltaY - deltaY >= minValueY)
					newHeight -= (int)Math.Round(deltaY + deltaY);
				else {
					newHeight = minValueY;
					result = false;
				}
			} else {
				if (newWidth + deltaX >= minValueX)
					newWidth += (int)Math.Round(deltaX);
				else {
					deltaX = -newWidth;
					newWidth = minValueX;
					result = false;
				}
				if (newHeight - deltaY >= minValueY)
					newHeight -= (int)Math.Round(deltaY);
				else {
					deltaY = newHeight;
					newHeight = minValueY;
					result = false;
				}
				centerOffsetX = (int)Math.Round((deltaX * centerPosFactorX * cosAngle) - (deltaY * (1 - centerPosFactorY) * sinAngle));
				centerOffsetY = (int)Math.Round((deltaX * centerPosFactorX * sinAngle) + (deltaY * (1 - centerPosFactorY) * cosAngle));
			}
			return result;
		}


		/// <summary>
		/// Moves the left side of a (rotated) rectangle in order to resize it
		/// </summary>
		/// <param name="width">Width of the rectangle</param>
		/// <param name="height">Height of the rectangle</param>
		/// <param name="deltaX">Movement on X axis</param>
		/// <param name="deltaY">Movement on Y axis</param>
		/// <param name="cosAngle">Cosinus value of the rectangle's rotation angle</param>
		/// <param name="sinAngle">Sinus value of the rectangle's rotation angle</param>
		/// <param name="modifiers">Movement modifiers</param>
		/// <param name="centerOffsetX">Specifies the movement the rectangle's center has to perform for resizing</param>
		/// <param name="centerOffsetY">Specifies the movement the rectangle's center has to perform for resizing</param>
		/// <param name="newWidth">New width of the rectangle</param>
		/// <param name="newHeight">New height of the rectangle</param>
		/// <returns>Returns true if the movement could be performed as desired. Returns 
		/// false if the movement could not be performed completely because of movement 
		/// restrictions</returns>
		public static bool MoveRectangleLeft(int width, int height, float deltaX, float deltaY, float cosAngle, float sinAngle, ResizeModifiers modifiers,
			out int centerOffsetX, out int centerOffsetY, out int newWidth, out int newHeight) {
			int minWidth = 0, minHeight = 0;
			GetMinValues(width, height, modifiers, out minWidth, out minHeight);
			return MoveRectangleLeft(width, height, minWidth, 0.5f, 0.5f, 2, 2, deltaX, deltaY,
				cosAngle, sinAngle, modifiers, out centerOffsetX, out centerOffsetY, out newWidth, out newHeight);
		}


		/// <summary>
		/// Moves the left side of a (rotated) rectangle in order to resize it
		/// </summary>
		/// <param name="width">Width of the rectangle</param>
		/// <param name="height">Height of the rectangle</param>
		/// <param name="minValueX">Specifies the minimum width.</param>
		/// <param name="centerPosFactorX">Specifies where the center is located. Default value is 50% = 0.5</param>
		/// <param name="centerPosFactorY">Specifies where the center is located. Default value is 50% = 0.5</param>
		/// <param name="divFactorX">Specifies the factor through which the corrected and modified movement must be dividable without remainding.</param>
		/// <param name="divFactorY">Specifies the factor through which the corrected and modified movement must be dividable without remainding.</param>
		/// <param name="deltaX">Movement on X axis</param>
		/// <param name="deltaY">Movement on Y axis</param>
		/// <param name="cosAngle">Cosinus value of the rectangle's rotation angle</param>
		/// <param name="sinAngle">Sinus value of the rectangle's rotation angle</param>
		/// <param name="modifiers">Movement modifiers</param>
		/// <param name="centerOffsetX">Specifies the movement the rectangle's center has to perform for resizing</param>
		/// <param name="centerOffsetY">Specifies the movement the rectangle's center has to perform for resizing</param>
		/// <param name="newWidth">New width of the rectangle</param>
		/// <param name="newHeight">New height of the rectangle</param>
		/// <returns>Returns true if the movement could be performed as desired. 
		/// Returns false if the movement could not be performed completely because of movement restrictions</returns>
		public static bool MoveRectangleLeft(int width, int height, int minValueX,
			float centerPosFactorX, float centerPosFactorY, int divFactorX, int divFactorY,
			float deltaX, float deltaY, double cosAngle, double sinAngle, ResizeModifiers modifiers,
			out int centerOffsetX, out int centerOffsetY, out int newWidth, out int newHeight) {
			bool result = true;
			centerOffsetX = centerOffsetY = 0;
			newWidth = width;
			newHeight = height;
			// aspect maintainence can be combined with both MirroredResizing and normal resizing
			if ((modifiers & ResizeModifiers.MaintainAspect) != 0) {
				//MaintainAspectRatio(newWidth, height, -1, 1, ref deltaX, ref deltaY);
				float aspectRatio = width / (float)height;
				deltaY = deltaX / aspectRatio;
			}
			// Align to integer coordinates
			Geometry.AlignMovement(modifiers, divFactorX, divFactorY, ref deltaX, ref deltaY);

			if ((modifiers & ResizeModifiers.MirroredResize) != 0) {
				if (newWidth - deltaX - deltaX >= minValueX)
					newWidth -= (int)Math.Round(deltaX + deltaX);
				else {
					newWidth = minValueX;
					result = false;
				}
			} else {
				if (newWidth - deltaX >= minValueX)
					newWidth -= (int)Math.Round(deltaX);
				else {
					deltaX = newWidth;
					newWidth = minValueX;
					result = false;
				}
				centerOffsetX = (int)Math.Round(deltaX * centerPosFactorX * cosAngle);
				centerOffsetY = (int)Math.Round(deltaX * centerPosFactorX * sinAngle);
			}
			if ((modifiers & ResizeModifiers.MaintainAspect) != 0)
				newHeight = (int)Math.Round(newWidth / (width / (float)height));
			return result;
		}


		/// <summary>
		/// Moves the right side of a (rotated) rectangle in order to resize it
		/// </summary>
		/// <param name="width">Width of the rectangle</param>
		/// <param name="height">Height of the rectangle</param>
		/// <param name="deltaX">Movement on X axis</param>
		/// <param name="deltaY">Movement on Y axis</param>
		/// <param name="cosAngle">Cosinus value of the rectangle's rotation angle</param>
		/// <param name="sinAngle">Sinus value of the rectangle's rotation angle</param>
		/// <param name="modifiers">Movement modifiers</param>
		/// <param name="centerOffsetX">Specifies the movement the rectangle's center has to perform for resizing</param>
		/// <param name="centerOffsetY">Specifies the movement the rectangle's center has to perform for resizing</param>
		/// <param name="newWidth">New width of the rectangle</param>
		/// <param name="newHeight">New height of the rectangle</param>
		/// <returns>Returns true if the movement could be performed as desired. 
		/// Returns false if the movement could not be performed completely because of movement restrictions</returns>
		public static bool MoveRectangleRight(int width, int height, float deltaX, float deltaY, float cosAngle, float sinAngle, ResizeModifiers modifiers,
			out int centerOffsetX, out int centerOffsetY, out int newWidth, out int newHeight) {
			int minWidth = 0, minHeight = 0;
			GetMinValues(width, height, modifiers, out minWidth, out minHeight);
			return MoveRectangleRight(width, height, minWidth, 0.5f, 0.5f, 2, 2, deltaX, deltaY, cosAngle, sinAngle, modifiers, out centerOffsetX, out centerOffsetY, out newWidth, out newHeight);
		}


		/// <summary>
		/// Moves the right side of a (rotated) rectangle in order to resize it
		/// </summary>
		/// <param name="width">Width of the rectangle</param>
		/// <param name="height">Height of the rectangle</param>
		/// <param name="minValueX">Specifies the minimum width.</param>
		/// <param name="centerPosFactorX">Specifies where the center is located. Default value is 50% = 0.5</param>
		/// <param name="centerPosFactorY">Specifies where the center is located. Default value is 50% = 0.5</param>
		/// <param name="divFactorX">Specifies the factor through which the corrected and modified movement must be dividable without remainding.</param>
		/// <param name="divFactorY">Specifies the factor through which the corrected and modified movement must be dividable without remainding.</param>
		/// <param name="deltaX">Movement on X axis</param>
		/// <param name="deltaY">Movement on Y axis</param>
		/// <param name="cosAngle">Cosinus value of the rectangle's rotation angle</param>
		/// <param name="sinAngle">Sinus value of the rectangle's rotation angle</param>
		/// <param name="modifiers">Movement modifiers</param>
		/// <param name="centerOffsetX">Specifies the movement the rectangle's center has to perform for resizing</param>
		/// <param name="centerOffsetY">Specifies the movement the rectangle's center has to perform for resizing</param>
		/// <param name="newWidth">New width of the rectangle</param>
		/// <param name="newHeight">New height of the rectangle</param>
		/// <returns>Returns true if the movement could be performed as desired. 
		/// Returns false if the movement could not be performed completely because of movement restrictions</returns>
		public static bool MoveRectangleRight(int width, int height, int minValueX,
			float centerPosFactorX, float centerPosFactorY, int divFactorX, int divFactorY,
			float deltaX, float deltaY, double cosAngle, double sinAngle, ResizeModifiers modifiers,
			out int centerOffsetX, out int centerOffsetY, out int newWidth, out int newHeight) {
			bool result = true;
			centerOffsetX = centerOffsetY = 0;
			newWidth = width;
			newHeight = height;
			// Aspect maintainence can be combined with both MirroredResizing and normal resizing
			if ((modifiers & ResizeModifiers.MaintainAspect) != 0) {
				//MaintainAspectRatio(newWidth, height, 1, -1, ref deltaX, ref deltaY);
				float aspectRatio = width / (float)height;
				deltaY = -deltaX / aspectRatio;
			}
			// Align to integer coordinates
			Geometry.AlignMovement(modifiers, divFactorX, divFactorY, ref deltaX, ref deltaY);

			if ((modifiers & ResizeModifiers.MirroredResize) != 0) {
				if (newWidth + deltaX + deltaX >= minValueX)
					newWidth += (int)Math.Round(deltaX + deltaX);
				else {
					newWidth = minValueX;
					result = false;
				}
			} else {
				if (newWidth + deltaX >= minValueX)
					newWidth += (int)Math.Round(deltaX);
				else {
					deltaX = minValueX - newWidth;
					newWidth = minValueX;
					result = false;
				}
				centerOffsetX = (int)Math.Round(deltaX * centerPosFactorX * cosAngle);
				centerOffsetY = (int)Math.Round(deltaX * centerPosFactorX * sinAngle);
			}
			if ((modifiers & ResizeModifiers.MaintainAspect) != 0)
				newHeight = (int)Math.Round(newWidth / (width / (float)height));
			return result;
		}


		/// <summary>
		/// Moves the BottomLeft corner of a (rotated) rectangle in order to resize it
		/// </summary>
		/// <param name="width">Width of the rectangle</param>
		/// <param name="height">Height of the rectangle</param>
		/// <param name="deltaX">Movement on X axis</param>
		/// <param name="deltaY">Movement on Y axis</param>
		/// <param name="cosAngle">Cosinus value of the rectangle's rotation angle</param>
		/// <param name="sinAngle">Sinus value of the rectangle's rotation angle</param>
		/// <param name="modifiers">Movement modifiers</param>
		/// <param name="centerOffsetX">Specifies the movement the rectangle's center has to perform for resizing</param>
		/// <param name="centerOffsetY">Specifies the movement the rectangle's center has to perform for resizing</param>
		/// <param name="newWidth">New width of the rectangle</param>
		/// <param name="newHeight">New height of the rectangle</param>
		/// <returns>Returns true if the movement could be performed as desired. 
		/// Returns false if the movement could not be performed completely because of movement restrictions</returns>
		public static bool MoveRectangleBottomLeft(int width, int height, float deltaX, float deltaY, float cosAngle, float sinAngle, ResizeModifiers modifiers,
			out int centerOffsetX, out int centerOffsetY, out int newWidth, out int newHeight) {
			int minWidth = 0, minHeight = 0;
			GetMinValues(width, height, modifiers, out minWidth, out minHeight);
			return MoveRectangleBottomLeft(width, height, minWidth, minHeight, 0.5f, 0.5f, 2, 2, deltaX, deltaY, cosAngle, sinAngle, modifiers, out centerOffsetX, out centerOffsetY, out newWidth, out newHeight);
		}


		/// <summary>
		/// Moves the BottomLeft corner of a (rotated) rectangle in order to resize it
		/// </summary>
		/// <param name="width">Width of the rectangle</param>
		/// <param name="height">Height of the rectangle</param>
		/// <param name="minValueX">Specifies the minimum width.</param>
		/// <param name="minValueY">Specifies the minimum height.</param>
		/// <param name="centerPosFactorX">Specifies where the center is located. Default value is 50% = 0.5</param>
		/// <param name="centerPosFactorY">Specifies where the center is located. Default value is 50% = 0.5</param>
		/// <param name="divFactorX">Specifies the factor through which the corrected and modified movement must be dividable without remainding.</param>
		/// <param name="divFactorY">Specifies the factor through which the corrected and modified movement must be dividable without remainding.</param>
		/// <param name="deltaX">Movement on X axis</param>
		/// <param name="deltaY">Movement on Y axis</param>
		/// <param name="cosAngle">Cosinus value of the rectangle's rotation angle</param>
		/// <param name="sinAngle">Sinus value of the rectangle's rotation angle</param>
		/// <param name="modifiers">Movement modifiers</param>
		/// <param name="centerOffsetX">Specifies the movement the rectangle's center has to perform for resizing</param>
		/// <param name="centerOffsetY">Specifies the movement the rectangle's center has to perform for resizing</param>
		/// <param name="newWidth">New width of the rectangle</param>
		/// <param name="newHeight">New height of the rectangle</param>
		/// <returns>Returns true if the movement could be performed as desired. 
		/// Returns false if the movement could not be performed completely because of movement restrictions</returns>
		public static bool MoveRectangleBottomLeft(int width, int height, int minValueX, int minValueY,
			float centerPosFactorX, float centerPosFactorY, int divFactorX, int divFactorY,
			float deltaX, float deltaY, double cosAngle, double sinAngle, ResizeModifiers modifiers,
			out int centerOffsetX, out int centerOffsetY, out int newWidth, out int newHeight) {
			bool result = true;
			centerOffsetX = centerOffsetY = 0;
			newWidth = width;
			newHeight = height;

			// Aspect maintainence can be combined with both MirroredResizing and normal resizing
			if ((modifiers & ResizeModifiers.MaintainAspect) != 0)
				MaintainAspectRatio(newWidth, newHeight, -1, 1, ref deltaX, ref deltaY);
			// Align to integer coordinates
			Geometry.AlignMovement(modifiers, divFactorX, divFactorY, ref deltaX, ref deltaY);

			if ((modifiers & ResizeModifiers.MirroredResize) != 0) {
				if (newWidth - deltaX - deltaX >= minValueX)
					newWidth -= (int)Math.Round(deltaX + deltaX);
				else {
					newWidth = minValueX;
					result = false;
				}
				if (newHeight + deltaY + deltaY >= minValueY)
					newHeight += (int)Math.Round(deltaY + deltaY);
				else {
					newHeight = minValueY;
					result = false;
				}
			} else {
				if (newWidth - deltaX >= minValueX) {
					newWidth -= (int)Math.Round(deltaX);
				} else {
					deltaX = newWidth;
					newWidth = minValueX;
					result = false;
				}
				if (newHeight + deltaY >= minValueY) {
					newHeight += (int)Math.Round(deltaY);
				} else {
					deltaY = -newHeight;
					newHeight = minValueY;
					result = false;
				}
				centerOffsetX = (int)Math.Round((deltaX * centerPosFactorX * cosAngle) - (deltaY * centerPosFactorY * sinAngle));
				centerOffsetY = (int)Math.Round((deltaX * centerPosFactorX * sinAngle) + (deltaY * centerPosFactorY * cosAngle));
			}
			return result;
		}


		/// <summary>
		/// Moves the bottom side of a (rotated) rectangle in order to resize it
		/// </summary>
		/// <param name="width">Width of the rectangle</param>
		/// <param name="height">Height of the rectangle</param>
		/// <param name="deltaX">Movement on X axis</param>
		/// <param name="deltaY">Movement on Y axis</param>
		/// <param name="cosAngle">Cosinus value of the rectangle's rotation angle</param>
		/// <param name="sinAngle">Sinus value of the rectangle's rotation angle</param>
		/// <param name="modifiers">Movement modifiers</param>
		/// <param name="centerOffsetX">Specifies the movement the rectangle's center has to perform for resizing</param>
		/// <param name="centerOffsetY">Specifies the movement the rectangle's center has to perform for resizing</param>
		/// <param name="newWidth">New width of the rectangle</param>
		/// <param name="newHeight">New height of the rectangle</param>
		/// <returns>Returns true if the movement could be performed as desired. 
		/// Returns false if the movement could not be performed completely because of movement restrictions</returns>
		public static bool MoveRectangleBottom(int width, int height, float deltaX, float deltaY, float cosAngle, float sinAngle, ResizeModifiers modifiers, out int centerOffsetX,
			out int centerOffsetY, out int newWidth, out int newHeight) {
			int minWidth = 0, minHeight = 0;
			GetMinValues(width, height, modifiers, out minWidth, out minHeight);
			return MoveRectangleBottom(width, height, minHeight, 0.5f, 0.5f, 2, 2, deltaX, deltaY, cosAngle, sinAngle, modifiers, out centerOffsetX, out centerOffsetY, out newWidth, out newHeight);
		}


		/// <summary>
		/// Moves the bottom side of a (rotated) rectangle in order to resize it
		/// </summary>
		/// <param name="width">Width of the rectangle</param>
		/// <param name="height">Height of the rectangle</param>
		/// <param name="minValueY">Specifies the minimum height.</param>
		/// <param name="centerPosFactorX">Specifies where the center is located. Default value is 50% = 0.5</param>
		/// <param name="centerPosFactorY">Specifies where the center is located. Default value is 50% = 0.5</param>
		/// <param name="divFactorX">Specifies the factor through which the corrected and modified movement must be dividable without remainding.</param>
		/// <param name="divFactorY">Specifies the factor through which the corrected and modified movement must be dividable without remainding.</param>
		/// <param name="deltaX">Movement on X axis</param>
		/// <param name="deltaY">Movement on Y axis</param>
		/// <param name="cosAngle">Cosinus value of the rectangle's rotation angle</param>
		/// <param name="sinAngle">Sinus value of the rectangle's rotation angle</param>
		/// <param name="modifiers">Movement modifiers</param>
		/// <param name="centerOffsetX">Specifies the movement the rectangle's center has to perform for resizing</param>
		/// <param name="centerOffsetY">Specifies the movement the rectangle's center has to perform for resizing</param>
		/// <param name="newWidth">New width of the rectangle</param>
		/// <param name="newHeight">New height of the rectangle</param>
		/// <returns>Returns true if the movement could be performed as desired. 
		/// Returns false if the movement could not be performed completely because of movement restrictions</returns>
		public static bool MoveRectangleBottom(int width, int height, int minValueY,
			float centerPosFactorX, float centerPosFactorY, int divFactorX, int divFactorY,
			float deltaX, float deltaY, double cosAngle, double sinAngle, ResizeModifiers modifiers,
			out int centerOffsetX, out int centerOffsetY, out int newWidth, out int newHeight) {
			bool result = true;
			centerOffsetX = centerOffsetY = 0;
			newWidth = width;
			newHeight = height;

			// Aspect maintainence can be combined with both MirroredResizing and normal resizing
			if ((modifiers & ResizeModifiers.MaintainAspect) != 0) {
				//MaintainAspectRatio(width, newHeight, 1, 1, ref deltaX, ref deltaY);
				float aspectRatio = width / (float)height;
				deltaX = -deltaY * aspectRatio;
			}
			// Align to integer coordinates
			Geometry.AlignMovement(modifiers, divFactorX, divFactorY, ref deltaX, ref deltaY);

			if ((modifiers & ResizeModifiers.MirroredResize) != 0) {
				if (newHeight + deltaY + deltaY >= minValueY)
					newHeight += (int)Math.Round(deltaY + deltaY);
				else {
					newHeight = minValueY;
					result = false;
				}
			} else {
				if (newHeight + deltaY >= minValueY)
					newHeight += (int)Math.Round(deltaY);
				else {
					deltaY = -newHeight;
					newHeight = minValueY;
					result = false;
				}
				centerOffsetX = (int)Math.Round(-deltaY * centerPosFactorY * sinAngle);
				centerOffsetY = (int)Math.Round(deltaY * centerPosFactorY * cosAngle);
			}
			if ((modifiers & ResizeModifiers.MaintainAspect) != 0)
				newWidth = (int)Math.Round(newHeight * (width / (float)height));
			return result;
		}


		/// <summary>
		/// Moves the BottomRight corner of a (rotated) rectangle in order to resize it
		/// </summary>
		/// <param name="width">Width of the rectangle</param>
		/// <param name="height">Height of the rectangle</param>
		/// <param name="deltaX">Movement on X axis</param>
		/// <param name="deltaY">Movement on Y axis</param>
		/// <param name="cosAngle">Cosinus value of the rectangle's rotation angle</param>
		/// <param name="sinAngle">Sinus value of the rectangle's rotation angle</param>
		/// <param name="modifiers">Movement modifiers</param>
		/// <param name="centerOffsetX">Specifies the movement the rectangle's center has to perform for resizing</param>
		/// <param name="centerOffsetY">Specifies the movement the rectangle's center has to perform for resizing</param>
		/// <param name="newWidth">New width of the rectangle</param>
		/// <param name="newHeight">New height of the rectangle</param>
		/// <returns>Returns true if the movement could be performed as desired. 
		/// Returns false if the movement could not be performed completely because of movement restrictions</returns>
		public static bool MoveRectangleBottomRight(int width, int height, float deltaX, float deltaY, float cosAngle, float sinAngle, ResizeModifiers modifiers,
			out int centerOffsetX, out int centerOffsetY, out int newWidth, out int newHeight) {
			int minWidth = 0, minHeight = 0;
			GetMinValues(width, height, modifiers, out minWidth, out minHeight);
			return MoveRectangleBottomRight(width, height, minWidth, minHeight, 0.5f, 0.5f, 2, 2, deltaX, deltaY, cosAngle, sinAngle, modifiers, out centerOffsetX, out centerOffsetY, out newWidth, out newHeight);
		}


		/// <summary>
		/// Moves the BottomRight corner of a (rotated) rectangle in order to resize it
		/// </summary>
		/// <param name="width">Width of the rectangle</param>
		/// <param name="height">Height of the rectangle</param>
		/// <param name="minValueX">Specifies the minimum width.</param>
		/// <param name="minValueY">Specifies the minimum height.</param>
		/// <param name="centerPosFactorX">Specifies where the center is located. Default value is 50% = 0.5</param>
		/// <param name="centerPosFactorY">Specifies where the center is located. Default value is 50% = 0.5</param>
		/// <param name="divFactorX">Specifies the factor through which the corrected and modified movement must be dividable without remainding.</param>
		/// <param name="divFactorY">Specifies the factor through which the corrected and modified movement must be dividable without remainding.</param>
		/// <param name="deltaX">Movement on X axis</param>
		/// <param name="deltaY">Movement on Y axis</param>
		/// <param name="cosAngle">Cosinus value of the rectangle's rotation angle</param>
		/// <param name="sinAngle">Sinus value of the rectangle's rotation angle</param>
		/// <param name="modifiers">Movement modifiers</param>
		/// <param name="centerOffsetX">Specifies the movement the rectangle's center has to perform for resizing</param>
		/// <param name="centerOffsetY">Specifies the movement the rectangle's center has to perform for resizing</param>
		/// <param name="newWidth">New width of the rectangle</param>
		/// <param name="newHeight">New height of the rectangle</param>
		/// <returns>Returns true if the movement could be performed as desired. 
		/// Returns false if the movement could not be performed completely because of movement restrictions</returns>
		public static bool MoveRectangleBottomRight(int width, int height, int minValueX, int minValueY,
			float centerPosFactorX, float centerPosFactorY, int divFactorX, int divFactorY,
			float deltaX, float deltaY, double cosAngle, double sinAngle, ResizeModifiers modifiers,
			out int centerOffsetX, out int centerOffsetY, out int newWidth, out int newHeight) {
			bool result = true;
			centerOffsetX = centerOffsetY = 0;
			newWidth = width;
			newHeight = height;

			// Aspect maintainence can be combined with both MirroredResizing and normal resizing
			if ((modifiers & ResizeModifiers.MaintainAspect) != 0)
				MaintainAspectRatio(width, height, 1, 1, ref deltaX, ref deltaY);
			// Align to integer coordinates
			Geometry.AlignMovement(modifiers, divFactorX, divFactorY, ref deltaX, ref deltaY);

			if ((modifiers & ResizeModifiers.MirroredResize) != 0) {
				if (newWidth + deltaX + deltaX >= minValueX)
					newWidth += (int)Math.Round(deltaX + deltaX);
				else {
					newWidth = minValueX;
					result = false;
				}
				if (newHeight + deltaY + deltaY >= minValueY)
					newHeight += (int)Math.Round(deltaY + deltaY);
				else {
					newHeight = minValueY;
					result = false;
				}
			} else {
				if (newWidth + deltaX >= minValueX)
					newWidth += (int)Math.Round(deltaX);
				else {
					deltaX = -newWidth;
					newWidth = minValueX;
					result = false;
				}
				if (newHeight + deltaY >= minValueY)
					newHeight += (int)Math.Round(deltaY);
				else {
					deltaY = -newHeight;
					newHeight = minValueY;
					result = false;
				}
				centerOffsetX = (int)Math.Round((deltaX * centerPosFactorX * cosAngle) - (deltaY * centerPosFactorY * sinAngle));
				centerOffsetY = (int)Math.Round((deltaX * centerPosFactorX * sinAngle) + (deltaY * centerPosFactorY * cosAngle));
			}
			return result;
		}


		/// <summary>
		/// Moves the arrow tip of a rectangle based arrow
		/// </summary>
		public static bool MoveArrowPoint(Point center, Point movedPtPos, Point fixedPtPos, int angle, int minWidth, float centerPosFactorX, int untransformedDeltaX, int untransformedDeltaY, ResizeModifiers modifiers, out int centerOffsetX, out int centerOffsetY, out int newWidth, out int newAngle) {
			centerOffsetX = centerOffsetY = 0;
			newAngle = angle;
			newWidth = 0;

			// calculate new position of moved point
			Point newPtPos = movedPtPos;
			newPtPos.Offset(untransformedDeltaX, untransformedDeltaY);

			// calculate new shape location
			PointF newCenter = VectorLinearInterpolation((float)fixedPtPos.X, (float)fixedPtPos.Y, (float)newPtPos.X, (float)newPtPos.Y, centerPosFactorX);
			centerOffsetX = (int)Math.Round(newCenter.X - center.X);
			centerOffsetY = (int)Math.Round(newCenter.Y - center.Y);

			// calculate new angle
			float newAng = (360 + RadiansToDegrees(Geometry.Angle(fixedPtPos, newPtPos))) % 360;
			float oldAng = (360 + RadiansToDegrees(Geometry.Angle(fixedPtPos, movedPtPos))) % 360;
			newAngle = angle + DegreesToTenthsOfDegree(newAng - oldAng);

			// calculate new width
			newWidth = (int)Math.Round(DistancePointPoint(fixedPtPos, newPtPos));

			return (movedPtPos.X == newPtPos.X && movedPtPos.Y == newPtPos.Y);
		}

		#endregion


		#region Vector functions

		/// <summary>
		/// Calculates the dot product a * b
		/// </summary>
		[Obsolete("CAUTION: Arithmetic overflows with large coordinates.")]
		public static int VectorDotProduct(int aX, int aY, int bX, int bY) {
			return (int)((long)aX * bX + (long)aY * bY);
		}


		/// <summary>
		/// Calculates the dot product a * b
		/// </summary>
		internal static long VectorDotProductL(int aX, int aY, int bX, int bY) {
			// Not really slower than int version (100.000.000 calls):
			// int: 695 ms, long: 725 ms
			return (long)aX * bX + (long)aY * bY;
		}


		/// <summary>
		/// Calculates the dot product a * b
		/// </summary>
		public static float VectorDotProduct(float aX, float aY, float bX, float bY) {
			return aX * bX + aY * bY;
		}


		/// <summary>
		/// Calculates the dot product a * b
		/// </summary>
		[Obsolete("CAUTION: Arithmetic overflows with large coordinates.")]
		public static int VectorDotProduct(Point a, Point b) {
			return VectorDotProduct(a.X, a.Y, b.X, b.Y);
		}


		/// <summary>
		/// Calculates the dot product a * b
		/// </summary>
		public static float VectorDotProduct(PointF a, PointF b) {
			return VectorDotProduct(a.X, a.Y, b.X, b.Y);
		}


		/// <summary>
		/// Calculates the dot product ab * bc
		/// </summary>
		public static float VectorDotProduct(PointF a, PointF b, PointF c) {
			return VectorDotProduct(a.X, a.Y, b.X, b.Y, c.X, c.Y);
		}


		/// <summary>
		/// Calculates the dot product ab * bc
		/// </summary>
		public static float VectorDotProduct(float aX, float aY, float bX, float bY, float cX, float cY) {
			float abX = bX - aX;
			float abY = bY - aY;
			float bcX = cX - bX;
			float bcY = cY - bY;
			return abX * bcX + abY * bcY;
		}


		/// <summary>
		/// Calculates the dot product ab * bc
		/// </summary>
		[Obsolete("CAUTION: Arithmetic overflows with large coordinates.")]
		public static int VectorDotProduct(Point a, Point b, Point c) {
			return VectorDotProduct(a.X, a.Y, b.X, b.Y, c.X, c.Y);
		}


		/// <summary>
		/// Calculates the dot product ab * bc
		/// </summary>
		// Note: We should have calculated ba * bc instead, that would be more conformant to the expectations.
		// The result should be proportional to the cos of the angle between the two vectors.
		[Obsolete("CAUTION: Arithmetic overflows with large coordinates.")]
		public static int VectorDotProduct(int aX, int aY, int bX, int bY, int cX, int cY) {
			int abX = bX - aX;
			int abY = bY - aY;
			int bcX = cX - bX;
			int bcY = cY - bY;
			return (int)((long)abX * bcX + (long)abY * bcY);
		}


		/// <summary>
		/// Calculates the dot product ab * bc
		/// </summary>
		// Note: We should have calculated ba * bc instead, that would be more conformant to the expectations.
		// The result should be proportional to the cos of the angle between the two vectors.
		internal static long VectorDotProductL(int aX, int aY, int bX, int bY, int cX, int cY) {
			// Not really slower than int version (100.000.000 calls):
			// int: 693 ms, long: 731 ms
			int abX = bX - aX;
			int abY = bY - aY;
			int bcX = cX - bX;
			int bcY = cY - bY;
			return (long)abX * bcX + (long)abY * bcY;
		}


		/// <summary>
		/// Calculates the scalar/dot product between vectors A-B and C-D
		/// </summary>
		internal static long VectorDotProductL(int aX, int aY, int bX, int bY, int cX, int cY, int dX, int dY) {
			// Faster than int version (100.000.000 calls:
			// int: 1205 ms, long: 1104 ms
			return (long)(bX - aX) * (dX - cX) + (long)(bY - aY) * (dY - cY);
		}


		/// <summary>
		/// Calculates the scalar/dot product between vectors A-B and C-D
		/// </summary>
		public static float VectorDotProduct(float aX, float aY, float bX, float bY, float cX, float cY, float dX, float dY) {
			return (bX - aX) * (dX - cX) + (bY - aY) * (dY - cY);
		}


		/// <summary>
		/// Calculates the scalar/dot product between vectors A-B and C-D
		/// </summary>
		[Obsolete("CAUTION: Arithmetic overflows with large coordinates.")]
		public static int CalcScalarProduct(int aX, int aY, int bX, int bY, int cX, int cY, int dX, int dY) {
			return (int)((long)(bX - aX) * (dX - cX) + (long)(bY - aY) * (dY - cY));
		}


		/// <summary>
		/// Calculates the scalar/dot product between vectors A-B and C-D
		/// </summary>
		[Obsolete("Use 'VectorDotProduct' instead.")]
		public static float CalcScalarProduct(float aX, float aY, float bX, float bY, float cX, float cY, float dX, float dY) {
			return VectorDotProduct(aX, aY, bX, bY, cX, cY, dX, dY);
		}


		/// <summary>
		/// Calculates the cross product a x b
		/// </summary>
		/// <remarks>
		/// What we actually calculate here is the z coordinate of the cross product vector in R³
		/// with aZ and bZ assumed to be zero. Result is the size of the area of the parallelogram A B A' B'
		/// </remarks>
		[Obsolete("CAUTION: Arithmetic overflows with large coordinates.")]
		public static int VectorCrossProduct(int aX, int aY, int bX, int bY) {
			return aX * bY - aY * bX;
		}


		/// <summary>
		/// Calculates the cross product a x b
		/// </summary>
		/// <remarks>
		/// What we actually calculate here is the z coordinate of the cross product vector in R³
		/// with aZ and bZ assumed to be zero. Result is the size of the area of the parallelogram A B A' B'
		/// </remarks>
		internal static long VectorCrossProductL(int aX, int aY, int bX, int bY) {
			// Not really slower than int version (100.000.000 calls):
			// int: 694ms, long: 715ms
			return (long)aX * bY - (long)aY * bX;
		}


		/// <summary>
		/// Calculates the cross product ab x ac
		/// </summary>
		public static float VectorCrossProduct(float aX, float aY, float bX, float bY) {
			return aX * bY - aY * bX;
		}


		/// <summary>
		/// Calculates the cross product ab x ac
		/// </summary>
		[Obsolete("CAUTION: Arithmetic overflows with large coordinates.")]
		public static int VectorCrossProduct(Point a, Point b, Point c) {
			return VectorCrossProduct(a.X, a.Y, b.X, b.Y, c.X, c.Y);
		}


		/// <summary>
		/// Calculates the cross product ab x ac
		/// </summary>
		[Obsolete("CAUTION: Arithmetic overflows with large coordinates.")]
		public static int VectorCrossProduct(int aX, int aY, int bX, int bY, int cX, int cY) {
			int abX = bX - aX;
			int abY = bY - aY;
			int acX = cX - aX;
			int acY = cY - aY;
			return (int)((long)abX * acY - (long)abY * acX);
		}


		/// <summary>
		/// Calculates the cross product ab x ac
		/// </summary>
		internal static long VectorCrossProductL(int aX, int aY, int bX, int bY, int cX, int cY) {
			// Not really slower than int version (100.000.000 calls):
			// int:   704 ms, longt: 722 ms
			int abX = bX - aX;
			int abY = bY - aY;
			int acX = cX - aX;
			int acY = cY - aY;
			return (long)abX * acY - (long)abY * acX;
		}


		/// <summary>
		/// Calculates the cross product ab x ac
		/// </summary>
		public static float VectorCrossProduct(PointF a, PointF b, PointF c) {
			return VectorCrossProduct(a.X, a.Y, b.X, b.Y, c.X, c.Y);
		}


		/// <summary>
		/// Calculates the cross product ab x ac
		/// </summary>
		public static float VectorCrossProduct(float aX, float aY, float bX, float bY, float cX, float cY) {
			return ((aX - bX) * (aY - cY) - (aY - bY) * (aX - cX));
		}


		/// <summary>
		/// Computes a point on the line segment ab located db% away from a
		/// </summary>
		/// <param name="a">Point defining one end of the line segment</param>
		/// <param name="b">Point defining the other end of the line segment</param>
		/// <param name="t">Position of the calculated point. E.g. 0.5 returns the middle of a and b.</param>
		public static Point VectorLinearInterpolation(Point a, Point b, float t) {
			return VectorLinearInterpolation(a.X, a.Y, b.X, b.Y, t);
		}


		/// <summary>
		/// Computes a point on the line segment ab located db% away from a
		/// </summary>
		/// <param name="aX">X coordinate of the line's first point</param>
		/// <param name="aY">Y coordinate of the line's first point</param>
		/// <param name="bX">X coordinate of the line's second point</param>
		/// <param name="bY">Y coordinate of the line's second point</param>
		/// <param name="t">Position of the calculated point. E.g. 0.5 returns the middle of a and b.</param>
		public static Point VectorLinearInterpolation(int aX, int aY, int bX, int bY, float t) {
			Point result = Point.Empty;
			int dX = bX - aX;
			int dY = bY - aY;
			result.Offset(
				aX + (int)Math.Round(dX * t),
				aY + (int)Math.Round(dY * t)
			);
			return result;
		}


		/// <summary>
		/// Computes a point on the line segment ab located (t * 100)% away from a
		/// </summary>
		/// <param name="a">Point defining one end of the line segment</param>
		/// <param name="b">Point defining the other end of the line segment</param>
		/// <param name="t">Position of the calculated point. E.g. 0.5 returns the middle of a and b.</param>
		public static PointF VectorLinearInterpolation(PointF a, PointF b, float t) {
			return VectorLinearInterpolation(a.X, a.Y, b.X, b.Y, t);
		}


		/// <summary>
		/// Computes a point on the line segment ab located db% away from a
		/// </summary>
		/// <param name="aX">X coordinate of the line's first point</param>
		/// <param name="aY">Y coordinate of the line's first point</param>
		/// <param name="bX">X coordinate of the line's second point</param>
		/// <param name="bY">Y coordinate of the line's second point</param>
		/// <param name="t">Position of the calculated point. E.g. 0.5 returns the middle of a and b.</param>
		public static PointF VectorLinearInterpolation(float aX, float aY, float bX, float bY, float t) {
			PointF result = Point.Empty;
			float dX = bX - aX;
			float dY = bY - aY;
			result.X = aX + (dX * t);
			result.Y = aY + (dY * t);
			return result;
		}

		#endregion


		#region Calculation of normal vectors

		/// <summary>
		/// Calculates the normal vector of the line in foot F.
		/// </summary>
		/// <param name="a1X">x coordinate of first line definition point</param>
		/// <param name="a1Y">y coordinate of first line definition point</param>
		/// <param name="a2X">x coordinate of second line definition point</param>
		/// <param name="a2Y">y coordinate of second line definition point</param>
		/// <param name="fX">x coordinate of foot point</param>
		/// <param name="fY">y coordinate of foot point</param>
		/// <param name="vectorLength">desired length of normal vector</param>
		/// <param name="pX">x coordinate of normal vector end point</param>
		/// <param name="pY">y coordinate of normal vector end point</param>
		// The direction of the resulting vector should point towards the half-plane that does not contain the origin.
		public static void CalcNormalVectorOfLine(int a1X, int a1Y, int a2X, int a2Y, int fX, int fY, int vectorLength, out int pX, out int pY) {
			//// Original version: Works, but changes the normal's direction depending on the line's slope
			//int a, b, c;
			//CalcLine(x1, y1, x2, y2, out a, out b, out c);
			//double l;
			//if (a == 0 && b == 0) l = 1;
			//else l = Math.Sqrt(a * a + b * b);
			//// Since a, b, c is (almost) the Hesse normal form, (a, b) is the normal vector of A-B.
			//pX = (int)Math.Round(fX + vectorLength * a / l);
			//pY = (int)Math.Round(fY + vectorLength * b / l);

			// New Version
			Point normalFoot = Geometry.CalcPointOnLine(a1X, a1Y, a2X, a2Y, vectorLength);
			int dx = normalFoot.X - a1X;
			int dy = normalFoot.Y - a1Y;
			// The normals are (-dy, dx) and (dy, -dx).
			pX = fX + dy;
			pY = fY - dx;
		}


		/// <summary>
		/// Calculates the normal vector of a rectangle. 
		/// If the point p is not on the outline of the rectangle, the resulting normal vector will be translated to the outline.
		/// </summary>
		public static Point CalcNormalVectorOfRectangle(Rectangle rectangle, Point p, int vectorLength) {
			return CalcNormalVectorOfRectangle(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, p.X, p.Y, vectorLength);
		}


		/// <summary>
		/// Calculates the normal vector of a rectangle. 
		/// If the point p is not on the outline of the rectangle, the resulting notmal vector will be translated to the outline.
		/// </summary>
		public static Point CalcNormalVectorOfRectangle(Rectangle rectangle, int ptX, int ptY, int vectorLength) {
			return CalcNormalVectorOfRectangle(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, ptX, ptY, vectorLength);
		}


		/// <summary>
		/// Calculates the normal vector in (ptX, ptY) with respect to the bounds of a rectangle. 
		/// If the point (ptX,ptY) is not on the outline of the rectangle, the resulting normal vector will be translated to the outline.
		/// </summary>
		public static Point CalcNormalVectorOfRectangle(int rX, int rY, int rWidth, int rHeight, int ptX, int ptY, int vectorLength) {
			Point normalVector = Point.Empty;
			normalVector.Offset(ptX, ptY);
			if (rX <= ptX && ptX <= rX + rWidth && rY <= ptY && ptY <= rY + rHeight) {
				Point topLeft = Point.Empty;
				topLeft.Offset(rX, rY);
				Point bottomRight = Point.Empty;
				bottomRight.Offset(rX + rWidth, rY + rHeight);
				Point bottomLeft = Point.Empty;
				bottomLeft.Offset(topLeft.X, bottomRight.Y);
				Point topRight = Point.Empty;
				topRight.Offset(bottomRight.X, topLeft.Y);
				Point center = Point.Empty;
				center.Offset(rX + (rWidth / 2), rY + (rHeight / 2));
				Point p = Point.Empty;
				p.Offset(ptX, ptY);
				if (TriangleContainsPoint(topLeft, topRight, center, p)) {
					normalVector.X = ptX;
					normalVector.Y = rY - vectorLength;
				} else if (TriangleContainsPoint(bottomLeft, bottomRight, center, p)) {
					normalVector.X = ptX;
					normalVector.Y = rY + rHeight + vectorLength;
				} else if (TriangleContainsPoint(topRight, bottomRight, center, p)) {
					normalVector.X = rX + rWidth + vectorLength;
					normalVector.Y = ptY;
				} else if (TriangleContainsPoint(topLeft, bottomLeft, center, p)) {
					normalVector.X = rX - vectorLength;
					normalVector.Y = ptY;
				} else Debug.Fail(string.Format("Unable to calculate normal vector of {0}", new Point(ptX, ptY)));
			}
			return normalVector;
		}


		/// <summary>
		/// Calculates the normal vector of a circle. 
		/// If the point ptX/ptY is not on the outline of the circle, the resulting notmal vector will be translated to the outline.
		/// </summary>
		public static Point CalcNormalVectorOfCircle(int centerX, int centerY, int radius, int ptX, int ptY, int vectorLength) {
			Point normalVector = Point.Empty;
			normalVector.Offset(ptX, ptY);
			if (CircleContainsPoint(centerX, centerY, radius, ptX, ptY, 0)) {
				Point intersectionPt = IntersectCircleWithLine(centerX, centerY, radius, ptX, ptY, centerX, centerY, false);
				if (IsValid(intersectionPt)) {
					float d = (float)radius / vectorLength;
					normalVector = VectorLinearInterpolation(centerX, centerY, intersectionPt.X, intersectionPt.Y, d);
				} else Debug.Fail("No intersection between circle and line");
			}
			return normalVector;
		}

		#endregion


		#region Hit test functions

		/// <summary>
		/// Returns true, if point (x,y) is inside (including bounds) of given rectangle.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="p1x"></param>
		/// <param name="p1y"></param>
		/// <param name="p2x"></param>
		/// <param name="p2y"></param>
		/// <returns></returns>
		private static bool IsPointInRect(int x, int y, int p1x, int p1y, int p2x, int p2y)
		{
			if (p1x >= p2x && (x < p2x || x > p1x)) return false;
			if (p1x <= p2x && (x > p2x || x < p1x)) return false;
			if (p1y >= p2y && (y < p2y || y > p1y)) return false;
			if (p1y <= p2y && (y > p2y || y < p1y)) return false;
			return true;
		}


		/// <summary>
		/// Returns true, if point (x,y) is inside (including bounds) of given rectangle.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="p1x"></param>
		/// <param name="p1y"></param>
		/// <param name="p2x"></param>
		/// <param name="p2y"></param>
		/// <returns></returns>
		private static bool IsPointInRect(float x, float y, float p1x, float p1y, float p2x, float p2y) {
			if (p1x >= p2x && (x < p2x || x > p1x)) return false;
			if (p1x <= p2x && (x > p2x || x < p1x)) return false;
			if (p1y >= p2y && (y < p2y || y > p1y)) return false;
			if (p1y <= p2y && (y > p2y || y < p1y)) return false;
			return true;
		}


		/// <summary>
		/// Returns true if point p is inside or on the bounds of the triangle a/b/c
		/// </summary>
		/// <param name="a">First point of the triangle</param>
		/// <param name="b">Second point of the triangle</param>
		/// <param name="c">Third point of the triangle</param>
		/// <param name="p">The testee</param>
		/// <returns></returns>
		public static bool TriangleContainsPoint(Point a, Point b, Point c, Point p) {
			return TriangleContainsPoint(a.X, a.Y, b.X, b.Y, c.X, c.Y, p.X, p.Y);
		}


		/// <summary>
		/// Returns true if point x/y is inside or on the bounds of the triangle a/b/c
		/// </summary>
		public static bool TriangleContainsPoint(Point a, Point b, Point c, int x, int y) {
			return TriangleContainsPoint(a.X, a.Y, b.X, b.Y, c.X, c.Y, x, y);
		}


		/// <summary>
		/// Returns true if point x/y is inside or on the bounds of the triangle a/b/c
		/// </summary>
		public static bool TriangleContainsPoint(int aX, int aY, int bX, int bY, int cX, int cY, int x, int y) {
			if ((x == aX || x == bX || x == cX) && (y == aY || y == bY || y == cY))
				return true;
			// Faster than the float version: 1365 ms versus 1435 ms for the float version (100.000.000 calls, Release) 
			long ab = VectorCrossProductL(aX, aY, bX, bY, x, y);
			long bc = VectorCrossProductL(bX, bY, cX, cY, x, y);
			long ca = VectorCrossProductL(cX, cY, aX, aY, x, y);
			bool hasNegative = (ab < 0) || (bc < 0) || (ca < 0);
			bool hasPositive = (ab > 0) || (bc > 0) || (ca > 0);
			return !(hasNegative && hasPositive);
		}


		/// <summary>
		/// Returns true if point p is inside or on the bounds of the triangle a/b/c
		/// </summary>
		/// <param name="a">First point of the triangle</param>
		/// <param name="b">Second point of the triangle</param>
		/// <param name="c">Third point of the triangle</param>
		/// <param name="p">The testee</param>
		/// <returns></returns>
		public static bool TriangleContainsPoint(PointF a, PointF b, PointF c, PointF p) {
			return TriangleContainsPoint(a.X, a.Y, b.X, b.Y, c.X, c.Y, p.X, p.Y);
		}


		/// <summary>
		/// Returns true if point x/y is inside or on the bounds of the triangle a/b/c
		/// </summary>
		public static bool TriangleContainsPoint(float aX, float aY, float bX, float bY, float cX, float cY, float x, float y) {
			if ((Equals(x, aX) || Equals(x, bX) || Equals(x, cX)) && (Equals(y, aY) || Equals(y, bY) || Equals(y, cY)))
				return true;
			float ab = VectorCrossProduct(aX, aY, bX, bY, x, y);
			float bc = VectorCrossProduct(bX, bY, cX, cY, x, y);
			float ca = VectorCrossProduct(cX, cY, aX, aY, x, y);
			bool hasNegative = (ab < -EqualityDeltaFloat) || (bc < -EqualityDeltaFloat) || (ca < -EqualityDeltaFloat);
			bool hasPositive = (ab > EqualityDeltaFloat) || (bc > EqualityDeltaFloat) || (ca > EqualityDeltaFloat);
			return !(hasNegative && hasPositive);
		}


		/// <summary>
		/// Returns true if point x/y is inside or on the bounds of the quadrangle a/b/c/d
		/// </summary>
		/// <param name="a">First point of the quadrangle</param>
		/// <param name="b">Second point of the quadrangle</param>
		/// <param name="c">Third point of the quadrangle</param>
		/// <param name="d">Fourth point of the quadrangle</param>
		/// <param name="x">X coordinate to test.</param>
		/// <param name="y">Y coordinate to test.</param>
		public static bool QuadrangleContainsPoint(Point a, Point b, Point c, Point d, int x, int y) {
			return QuadrangleContainsPoint(a.X, a.Y, b.X, b.Y, c.X, c.Y, d.X, d.Y, x, y);
		}


		/// <summary>
		/// Returns true if point x/y is inside or on the bounds of the quadrangle a/b/c/d
		/// </summary>
		public static bool QuadrangleContainsPoint(int aX, int aY, int bX, int bY, int cX, int cY, int dX, int dY, int x, int y) {
			// Order of points does not matter as long as they form a polygon: "a, b, c, d" works as good as "c, d, a, b" or "d, c, b, a"
			if ((x == aX || x == bX || x == cX || x == dX) && (y == aY || y == bY || y == cY || y == dY))
				return true;
			// Faster than the float version: 1923 ms (long version) versus 2186 ms (float version) for 100.000.000 calls, Release
			long ab = VectorCrossProductL(aX, aY, bX, bY, x, y);
			long bc = VectorCrossProductL(bX, bY, cX, cY, x, y);
			long cd = VectorCrossProductL(cX, cY, dX, dY, x, y);
			long da = VectorCrossProductL(dX, dY, aX, aY, x, y);
			bool hasNegative = (ab < 0) || (bc < 0) || (cd < 0) || (da < 0);
			bool hasPositive = (ab > 0) || (bc > 0) || (cd > 0) || (da > 0);
			return !(hasNegative && hasPositive);
		}


		/// <summary>
		/// Returns true if point p is inside or on the bounds of the quadrangle a/b/c/d
		/// </summary>
		/// <param name="a">First point of the quadrangle</param>
		/// <param name="b">Second point of the quadrangle</param>
		/// <param name="c">Third point of the quadrangle</param>
		/// <param name="d">Fourth point of the quadrangle</param>
		/// <param name="x">X coordinate to test.</param>
		/// <param name="y">Y coordinate to test.</param>
		/// <returns></returns>
		public static bool QuadrangleContainsPoint(PointF a, PointF b, PointF c, PointF d, float x, float y) {
			return QuadrangleContainsPoint(a.X, a.Y, b.X, b.Y, c.X, c.Y, d.X, d.Y, x, y);
		}


		/// <summary>
		/// Returns true if point x/y is inside or on the bounds of the quadrangle a/b/c/d
		/// </summary>
		public static bool QuadrangleContainsPoint(float aX, float aY, float bX, float bY, float cX, float cY, float dX, float dY, float x, float y) {
			if ((Equals(x, aX) || Equals(x, bX) || Equals(x, cX) || Equals(x, dX)) && (Equals(y, aY) || Equals(y, bY) || Equals(y, cY) || Equals(y, dY)))
				return true;
			float ab = VectorCrossProduct(aX, aY, bX, bY, x, y);
			float bc = VectorCrossProduct(bX, bY, cX, cY, x, y);
			float cd = VectorCrossProduct(cX, cY, dX, dY, x, y);
			float da = VectorCrossProduct(dX, dY, aX, aY, x, y);
			bool hasNegative = (ab < -EqualityDeltaFloat) || (bc < -EqualityDeltaFloat) || (cd < -EqualityDeltaFloat) || (da < -EqualityDeltaFloat);
			bool hasPositive = (ab > EqualityDeltaFloat) || (bc > EqualityDeltaFloat) || (cd > -EqualityDeltaFloat) || (da > -EqualityDeltaFloat);
			return !(hasNegative && hasPositive);
		}


		/// <summary>
		/// Tests if point p is inside or on the bounds of the given rectangle.
		/// </summary>
		public static bool RectangleContainsPoint(int rX, int rY, int rWidth, int rHeight, int pX, int pY) {
			return RectangleContainsPoint(rX, rY, rWidth, rHeight, pX, pY, true);
		}


		/// <summary>
		/// Tests if point x/y is inside or on the bounds of the given rectangle.
		/// </summary>
		public static bool RectangleContainsPoint(Rectangle r, Point p) {
			return RectangleContainsPoint(r.X, r.Y, r.Width, r.Height, p.X, p.Y, true);
		}


		/// <summary>
		/// Tests if point x/y is inside or on the bounds of the given rectangle.
		/// </summary>
		public static bool RectangleContainsPoint(Rectangle r, int x, int y) {
			return RectangleContainsPoint(r.X, r.Y, r.Width, r.Height, x, y, true);
		}


		/// <summary>
		/// Tests if point p is inside the given rectangle. The bounds of the rectangle can be excluded.
		/// </summary>
		public static bool RectangleContainsPoint(int rectX, int rectY, int rectWidth, int rectHeight, 
			float rectRotationAngleDeg, int pX, int pY, bool withBounds) 
		{
			if (rectRotationAngleDeg != 0 && rectRotationAngleDeg % 180 != 0) {
				float x = pX;
				float y = pY;
				RotatePoint(rectX + (rectWidth / 2f), rectY + (rectHeight / 2f), -rectRotationAngleDeg, ref x, ref y);
				return RectangleContainsPoint(rectX, rectY, rectWidth, rectHeight, x, y);
			} else return RectangleContainsPoint(rectX, rectY, rectWidth, rectHeight, pX, pY);
		}


		/// <summary>
		/// Tests if point p is inside the given rectangle. The bounds of the rectangle can be excluded.
		/// </summary>
		public static bool RectangleContainsPoint(int rectX, int rectY, int rWidth, int rHeight, int pX, int pY, bool withBounds) {
			bool result = false;
			if (withBounds) {
				if ((pX >= rectX && pX <= rectX + rWidth) && (pY >= rectY && pY <= rectY + rHeight))
					result = true;
			} else {
				if ((pX > rectX && pX < rectX + rWidth) && (pY > rectY && pY < rectY + rHeight))
					result = true;
			}
			return result;
		}


		/// <summary>
		/// Tests if point p is inside the given rectangle
		/// </summary>
		public static bool RectangleContainsPoint(RectangleF r, PointF p) {
			return RectangleContainsPoint(r.X, r.Y, r.Width, r.Height, p.X, p.Y, true);
		}


		/// <summary>
		/// Tests if point p is inside the given rectangle
		/// </summary>
		public static bool RectangleContainsPoint(RectangleF r, float x, float y) {
			return RectangleContainsPoint(r.X, r.Y, r.Width, r.Height, x, y, true);
		}


		/// <summary>
		/// Tests if point p is inside the given rectangle
		/// </summary>
		public static bool RectangleContainsPoint(float rectX, float rectY, float rWidth, float rHeight, float pX, float pY) {
			return RectangleContainsPoint(rectX, rectY, rWidth, rHeight, pX, pY, true);
		}


		/// <summary>
		/// Tests if point p is inside the given rectangle. The bounds of the rectangle can be excluded.
		/// </summary>
		public static bool RectangleContainsPoint(float rectX, float rectY, float rWidth, float rHeight, float pX, float pY, bool withBounds) {
			bool result = false;
			if (withBounds) {
				if ((pX >= rectX && pX <= rectX + rWidth) && (pY >= rectY && pY <= rectY + rHeight))
					result = true;
			} else {
				if ((pX > rectX && pX < rectX + rWidth) && (pY > rectY && pY < rectY + rHeight))
					result = true;
			}
			return result;
		}


		/// <summary>
		/// Tests if the given point array is a convex polygon.
		/// </summary>
		public static bool PolygonIsConvex(Point[] points) {
			if (points == null) throw new ArgumentNullException(nameof(points));
			int convex = 0;
			int n = points.Length - 1;
			float ai = (float)Math.Atan2(points[n].X - points[0].X, points[n].Y - points[0].Y);
			for (int i = 0; i < n; ++i) {
				Point a = points[i];
				Point b = points[i + 1];
				float aj = (float)Math.Atan2(a.X - b.X, a.Y - b.Y);
				if (ai - aj < 0) convex++;
				ai = aj;
			}
			return (convex == 1 || convex == n - 1);
		}


		/// <summary>
		/// Tests if the given points define a convex polygon or not
		/// </summary>
		public static bool PolygonIsConvex(PointF[] points) {
			if (points == null) throw new ArgumentNullException(nameof(points));
			int convex = 0;
			int n = points.Length - 1;
			float ai = (float)Math.Atan2(points[n].X - points[0].X, points[n].Y - points[0].Y);
			for (int i = 0; i < n; ++i) {
				PointF a = points[i];
				PointF b = points[i + 1];
				float aj = (float)Math.Atan2(a.X - b.X, a.Y - b.Y);
				if (ai - aj < 0) convex++;
				ai = aj;
			}
			return (convex == 1 || convex == n - 1);
		}


		/// <summary>
		/// Tests if point x/y is inside or on the bounds of the given (convex) polygon
		/// </summary>
		public static bool ConvexPolygonContainsPoint(Point[] points, int x, int y) {
			if (points == null) throw new ArgumentNullException(nameof(points));
			// Store the cross product of the points.
			// If all the points have the same cross product (this means that they 
			// are on the same side), the point is inside the convex polygon.
			int maxIdx = points.Length - 1;
			// Faster than the float version: 1498 ms (long version) versus 2203 ms (float version) for 100.000.000 calls, Release
			bool z = VectorCrossProductL(points[maxIdx].X, points[maxIdx].Y, points[0].X, points[0].Y, x, y) < 0;
			for (int idxPt1 = 0; idxPt1 < maxIdx; ++idxPt1) {
				int idxPt2 = idxPt1 + 1;
				if (VectorCrossProductL(points[idxPt1].X, points[idxPt1].Y, points[idxPt2].X, points[idxPt2].Y, x, y) < 0 != z)
					return false;
			}
			return true;
		}


		/// <summary>
		/// Tests if point x/y is inside or on the bounds of the given (convex) polygon.
		/// PointF's of the polygon will be rounded to Point.
		/// </summary>
		public static bool ConvexPolygonContainsPoint(PointF[] points, int x, int y) {
			if (points == null) throw new ArgumentNullException(nameof(points));
			int maxIdx = points.Length - 1;
			if (points[0] == points[maxIdx]) --maxIdx;
			// Store the cross product of the points.
			// If all the points have the same cross product (this means that they 
			// are on the same side), the point is inside the convex polygon.
			PointF a = PointF.Empty, b = PointF.Empty;
			a = points[maxIdx];
			b = points[0];
			bool z = VectorCrossProduct(a.X, a.Y, b.X, b.Y, x, y) < 0;
			for (int i = 0; i < maxIdx; ++i) {
				a = points[i];
				b = points[i + 1];
				if (VectorCrossProduct(a.X, a.Y, b.X, b.Y, x, y) < 0 != z)
					return false;
			}
			return true;
		}


		/// <summary>
		/// Tests if point x/y is inside or on the bounds of the given (convex) polygon
		/// </summary>
		public static bool ConvexPolygonContainsPoint(PointF[] points, float x, float y) {
			if (points == null) throw new ArgumentNullException(nameof(points));
			// If vector a is less than 180 degrees clockwise from b, the value is positive
			// If all cross products are positive or all cross products are negative, this means 
			// that the point is always on the same side and this means that the point is inside
			int n = points.Length - 1;
			if (points[0] == points[n]) --n;
			bool z = VectorCrossProduct(points[n].X, points[n].Y, points[0].X, points[0].Y, x, y) < 0;
			int j;
			for (var i = 0; i < n; i++) {
				j = i + 1;
				if (VectorCrossProduct(points[i].X, points[i].Y, points[j].X, points[j].Y, x, y) < 0 != z)
					return false;
			}
			return true;
		}


		/// <summary>
		/// Tests if point x/y is inside or on the bounds of the given polygon
		/// </summary>
		public static bool PolygonContainsPoint(Point[] points, int x, int y) {
			int cnt = points.Length;
			if (cnt == 0) return false;
			else if (cnt == 1) return points[0].X == x && points[0].Y == y;
			else {
				// Test intersection of a line between the point and a point far, far away
				// If the number of intersections is even, the point must be outside, otherwise it is inside.
				int intersectionCnt = 0;
				// Instead of using a point far, far away we search for the max coordinates of the polygon's points
				CalcBoundingRectangle(points, out Rectangle boundingRect);
				//int x2 = int.MaxValue, y2 = int.MaxValue;
				int x2 = boundingRect.Right + 10, y2 = boundingRect.Top + 10;

				int j;
				for (int i = 0; i < cnt; ++i) {
					j = i + 1;
					Point a = points[i];
					Point b = (j == cnt) ? points[0] : points[j];
					if (DistancePointLine(x, y, a.X, a.Y, b.X, b.Y, true) == 0)
						return true;
					else if (LineSegmentIntersectsWithLineSegment(x, y, x2, y2, a.X, a.Y, b.X, b.Y))
						intersectionCnt++;
				}
				return (intersectionCnt % 2) != 0;
			}
		}


		/// <summary>
		/// Tests if point x/y is inside or on the bounds of the given polygon
		/// </summary>
		public static bool PolygonContainsPoint(PointF[] points, int x, int y) {
			int cnt = points.Length;
			if (cnt == 0) return false;
			else if (cnt == 1) return points[0].X == x && points[0].Y == y;
			else {
				// Test intersection of a line between the point and a point far, far away
				// If the number of intersections is even, the point must be outside, otherwise it is inside.
				int intersectionCnt = 0;
				float x2 = int.MaxValue, y2 = int.MaxValue;
				int j;
				for (int i = 0; i < cnt; ++i) {
					j = i + 1;
					PointF a = points[i];
					PointF b = (j == cnt) ? points[0] : points[j];
					if (DistancePointLine(x, y, a.X, a.Y, b.X, b.Y, true) == 0)
						return true;
					else if (LineSegmentIntersectsWithLineSegment(x, y, x2, y2, a.X, a.Y, b.X, b.Y))
						intersectionCnt++;
				}
				return (intersectionCnt % 2) != 0;
			}
		}


		#region Old version of ConvexPolygonContainsPoint (slower)
		//public static bool ConvexPolygonContainsPoint(PointF[] points, float x, float y) {
		//   // ist der Punkt immer auf der selben Seite der einzelnen Geraden, so ist er innerhalb der Figur
		//   int maxIdx = points.Length - 1;
		//   float r;
		//   float d = DistancePointLine2(x, y, points[0].X, points[0].Y, points[1].X, points[1].Y);
		//   for (int i = 1; i < maxIdx; ++i) {
		//      r = DistancePointLine2(x, y, points[i].X, points[i].Y, points[i + 1].X, points[i + 1].Y);
		//      if (d == 0) d = r;
		//      else if (d < 0) {
		//         if (r > 0) return false;
		//      }
		//      else if (d > 0) {
		//         if (r < 0) return false;
		//      }
		//   }
		//   r = DistancePointLine2(x, y, points[maxIdx].X, points[maxIdx].Y, points[0].X, points[0].Y);
		//   if (d < 0) { 
		//      if (r > 0) return false; 
		//   }
		//   else if (d > 0) { 
		//      if (r < 0) return false; 
		//   }
		//   return true;
		//}


		//public static bool ConvexPolygonContainsPoint(Point[] points, int x, int y) {
		//   // ist der Punkt immer auf der selben Seite der einzelnen Geraden, so ist er innerhalb der Figur
		//   float r;
		//   float d = DistancePointLine(x, y, points[0].X, points[0].Y, points[1].X, points[1].Y, false);
		//   for (int i = 1; i < points.Length - 1; ++i) {
		//      r = DistancePointLine(x, y, points[i].X, points[i].Y, points[i + 1].X, points[i + 1].Y, false);
		//      if (d == 0) d = r;
		//      else if (d < 0 && r > 0) 
		//         return false;
		//      else if (d > 0 && r < 0) 
		//         return false;
		//   }
		//   if (points[0] != points[points.Length - 1]) {
		//      r = DistancePointLine(x, y, points[points.Length - 1].X, points[points.Length - 1].Y, points[0].X, points[0].Y, false);
		//      if (d < 0 && r > 0) 
		//         return false;
		//      else if (d > 0 && r < 0) 
		//         return false;
		//   }
		//   return true;
		//}
		#endregion


		/// <summary>
		/// Determines if the point is on the line (segment).
		/// </summary>
		/// <param name="a1X">The x-coordinate of the first point that defines the line</param>
		/// <param name="a1Y">The y-coordinate of the first point that defines the line</param>
		/// <param name="a2X">The x-coordinate of the second point that defines the line</param>
		/// <param name="a2Y">The y-coordinate of the second point that defines the line</param>
		/// <param name="isSegment">Specifies if the line is a line or a line segment.</param>
		/// <param name="x">The x-coordinate of the point to test</param>
		/// <param name="y">The y-coordinate of the point to test</param>
		public static bool LineContainsPoint(int a1X, int a1Y, int a2X, int a2Y, bool isSegment, int x, int y) {
			return LineContainsPoint(a1X, a1Y, a2X, a2Y, isSegment, x, y, 0.5f);
		}


		/// <summary>
		/// Determines if the point is on the line (segment).
		/// </summary>
		/// <param name="a1">The first point that defines the line</param>
		/// <param name="a2">The second point that defines the line</param>
		/// <param name="isSegment">Specifies if the line is a line or a line segment</param>
		/// <param name="x">The x-coordinate of the point to test</param>
		/// <param name="y">The y-coordinate of the point to test</param>
		/// <param name="delta">Specifies the tolerance of the calculation</param>
		public static bool LineContainsPoint(Point a1, Point a2, bool isSegment, int x, int y, float delta) {
			return LineContainsPoint(a1.X, a1.Y, a2.X, a2.Y, isSegment, x, y, delta);
		}


		/// <summary>
		/// Determines if the point is on the line (segment).
		/// </summary>
		/// <param name="a1">The first point that defines the line</param>
		/// <param name="a2">The second point that defines the line</param>
		/// <param name="isSegment">Specifies if the line is a line or a line segment</param>
		/// <param name="p">The point to test</param>
		/// <param name="delta">Specifies the tolerance of the calculation</param>
		public static bool LineContainsPoint(Point a1, Point a2, bool isSegment, Point p, float delta) {
			return LineContainsPoint(a1.X, a1.Y, a2.X, a2.Y, isSegment, p.X, p.Y, delta);
		}


		/// <summary>
		/// Determines if the point is on the line (segment).
		/// </summary>
		/// <param name="a1">The first point that defines the line</param>
		/// <param name="a2">The second point that defines the line</param>
		/// <param name="isSegment">Specifies if the line is a line or a line segment</param>
		/// <param name="x">The x-coordinate of the point to test</param>
		/// <param name="y">The y-coordinate of the point to test</param>
		/// <param name="delta">Specifies the tolerance of the calculation</param>
		public static bool LineContainsPoint(PointF a1, PointF a2, bool isSegment, float x, float y, float delta) {
			return LineContainsPoint(a1.X, a1.Y, a2.X, a2.Y, isSegment, x, y, delta);
		}


		/// <summary>
		/// Determines if the point is on the line (segment).
		/// </summary>
		/// <param name="a1X">The x-coordinate of the first point that defines the line</param>
		/// <param name="a1Y">The y-coordinate of the first point that defines the line</param>
		/// <param name="a2X">The x-coordinate of the second point that defines the line</param>
		/// <param name="a2Y">The y-coordinate of the second point that defines the line</param>
		/// <param name="isSegment">Specifies if the line is a line or a line segment</param>
		/// <param name="x">The x-coordinate of the point to test</param>
		/// <param name="y">The y-coordinate of the point to test</param>
		/// <param name="delta">Specifies the tolerance of the calculation</param>
		public static bool LineContainsPoint(int a1X, int a1Y, int a2X, int a2Y, bool isSegment, int x, int y, float delta) {
			return Math.Abs(DistancePointLine(x, y, a1X, a1Y, a2X, a2Y, isSegment)) <= delta;
		}


		/// <summary>
		/// Determines if the point is on the line (segment).
		/// </summary>
		/// <param name="a1X">The x-coordinate of the first point that defines the line</param>
		/// <param name="a1Y">The y-coordinate of the first point that defines the line</param>
		/// <param name="a2X">The x-coordinate of the second point that defines the line</param>
		/// <param name="a2Y">The y-coordinate of the second point that defines the line</param>
		/// <param name="isSegment">Specifies if the line is a line or a line segment</param>
		/// <param name="x">The x-coordinate of the point to test</param>
		/// <param name="y">The y-coordinate of the point to test</param>
		public static bool LineContainsPoint(float a1X, float a1Y, float a2X, float a2Y, bool isSegment, float x, float y) {
			return LineContainsPoint(a1X, a1Y, a2X, a2Y, true, x, y, 0);
		}


		/// <summary>
		/// Determines if the point is on the line (segment).
		/// </summary>
		/// <param name="a1X">The x-coordinate of the first point that defines the line</param>
		/// <param name="a1Y">The y-coordinate of the first point that defines the line</param>
		/// <param name="a2X">The x-coordinate of the second point that defines the line</param>
		/// <param name="a2Y">The y-coordinate of the second point that defines the line</param>
		/// <param name="isSegment">Specifies if the line is a line or a line segment</param>
		/// <param name="x">The x-coordinate of the point to test</param>
		/// <param name="y">The y-coordinate of the point to test</param>
		/// <param name="delta">Specifies the tolerance of the calculation</param>
		public static bool LineContainsPoint(float a1X, float a1Y, float a2X, float a2Y, bool isSegment, float x, float y, float delta) {
			return Math.Abs(DistancePointLine(x, y, a1X, a1Y, a2X, a2Y, isSegment)) <= delta;

			// Works, but the version above is more than 2x faster...
			//if (p2x == p1x)
			//	return (p1x - delta <= x && x <= p1x + delta);
			//// check if point is inside the bounds of the line segment
			//if (isSegment) {
			//	if (!(Math.Min(p1x - delta, p2x - delta) <= x && x <= Math.Max(p1x + delta, p2x + delta)) ||
			//		 !(Math.Min(p1y - delta, p2y - delta) <= y && y <= Math.Max(p1y + delta, p2y + delta)))
			//		return false;
			//}
			//float m = (p2y - p1y) / (p2x - p1x);
			//float c1 = m * p1x - p1y;
			//float c = m * x - y;

			//return (c1 - delta <= c && c <= c1 + delta);
		}


		/// <summary>
		/// Returns true if rectangle rect1 contains rectangle rect2.
		/// </summary>
		public static bool RectangleContainsRectangle(Rectangle rect1, Rectangle rect2) {
			AssertIsValid(rect1);
			AssertIsValid(rect2);
			return rect2.Left >= rect1.Left
				&& rect2.Top >= rect1.Top
				&& rect2.Right <= rect1.Right
				&& rect2.Bottom <= rect1.Bottom;
		}


		/// <summary>
		/// Returns true if the rectanlge defined by x, y, width and height contains the given rectangle.
		/// </summary>
		public static bool RectangleContainsRectangle(int rX, int rY, int rWidth, int rHeight, Rectangle rectangle) {
			AssertIsValid(rX, rY, rWidth, rHeight);
			AssertIsValid(rectangle);
			return rectangle.Left >= rX && rectangle.Top >= rY && rectangle.Right <= rX + rWidth && rectangle.Bottom <= rY + rHeight;
		}


		/// <summary>
		/// Returns true if the given rectangle contains the rectanlge defined by x, y, width and height.
		/// </summary>
		public static bool RectangleContainsRectangle(Rectangle rectangle, int x, int y, int width, int height) {
			AssertIsValid(x, y, width, height);
			AssertIsValid(rectangle);
			return x >= rectangle.Left
				&& y >= rectangle.Top
				&& x + width <= rectangle.Right
				&& y + height <= rectangle.Bottom;
		}


		/// <summary>
		/// Returns true if rectangle rect1 contains rectangle rect2.
		/// </summary>
		public static bool RectangleContainsRectangle(int rect1X, int rect1Y, int rect1Width, int rect1Height, int rect2X, int rect2Y, int rect2Width, int rect2Height) {
			if (rect1Width < 0) throw new ArgumentException(string.Format("{0} is not a valid value.", rect1Width), "rect1Width");
			if (rect1Height < 0) throw new ArgumentException(string.Format("{0} is not a valid value.", rect1Height), "rect1Height");
			if (rect2Width < 0) throw new ArgumentException(string.Format("{0} is not a valid value.", rect2Width), "rect2Width");
			if (rect2Height < 0) throw new ArgumentException(string.Format("{0} is not a valid value.", rect2Height), "rect2Height");
			return rect2X >= rect1X
				&& rect2Y >= rect1Y
				&& rect2X + rect2Width <= rect1X + rect1Width
				&& rect2Y + rect2Height <= rect1Y + rect1Height;
		}


		/// <summary>
		/// Determines if a rotated ellipse contains a point
		/// /// </summary>
		/// <param name="ellipseCenter">The center point of the ellipse and the rotation center</param>
		/// <param name="ellipseWidth">The width of the ellipse, equal to the doubled length of the major half axis</param>
		/// <param name="ellipseHeight">The height of the ellipse, equal to the doubled length of the minor half axis</param>
		/// <param name="ellipseAngleDeg">The rotation angle of the ellipse in degrees</param>
		/// <param name="p">The point to test</param>
		/// <returns></returns>
		public static bool EllipseContainsPoint(PointF ellipseCenter, float ellipseWidth, float ellipseHeight, float ellipseAngleDeg, PointF p) {
			return EllipseContainsPoint(ellipseCenter.X, ellipseCenter.Y, ellipseWidth, ellipseHeight, ellipseAngleDeg, p.X, p.Y);
		}


		/// <summary>
		/// Determines if a rotated ellipse contains a point
		/// /// </summary>
		/// <param name="ellipseCenterX">The center point of the ellipse and the rotation center</param>
		/// <param name="ellipseCenterY">The center point of the ellipse and the rotation center</param>
		/// <param name="ellipseWidth">The width of the ellipse, equal to the doubled length of the major half axis</param>
		/// <param name="ellipseHeight">The height of the ellipse, equal to the doubled length of the minor half axis</param>
		/// <param name="ellipseAngleDeg">The rotation angle of the ellipse in degrees</param>
		/// <param name="x">The x-coordinate of the point to test</param>
		/// <param name="y">The y-coordinate of the point to test</param>
		/// <returns></returns>
		public static bool EllipseContainsPoint(float ellipseCenterX, float ellipseCenterY, float ellipseWidth, float ellipseHeight, float ellipseAngleDeg, float x, float y) {
			// Standard ellipse formula:
			// (x² / a²) + (y² / b²) = 1
			// Where a = radiusX and b = radiusY
			float radiusX = ellipseWidth / 2f;
			float radiusY = ellipseHeight / 2f;
			// instead of rotating the ellipse, we rotate the line to the opposite side and then rotate intersection points back
			if (ellipseAngleDeg != 0)
				RotatePoint(ellipseCenterX, ellipseCenterY, -ellipseAngleDeg, ref x, ref y);
			// transform x/y to the origin of the coordinate system
			float x0 = x - ellipseCenterX;
			float y0 = y - ellipseCenterY;
			return ((x0 * x0) / (radiusX * radiusX)) + ((y0 * y0) / (radiusY * radiusY)) <= 1;
		}


		/// <summary>
		/// Determines if a rotated ellipse contains a point
		/// /// </summary>
		/// <param name="ellipseCenter">The center point of the ellipse and the rotation center</param>
		/// <param name="ellipseWidth">The width of the ellipse, equal to the doubled length of the major half axis</param>
		/// <param name="ellipseHeight">The height of the ellipse, equal to the doubled length of the minor half axis</param>
		/// <param name="ellipseAngleDeg">The rotation angle of the ellipse in degrees</param>
		/// <param name="p">The point to test</param>
		/// <returns></returns>
		public static bool EllipseContainsPoint(Point ellipseCenter, int ellipseWidth, int ellipseHeight, float ellipseAngleDeg, Point p) {
			return EllipseContainsPoint(ellipseCenter.X, ellipseCenter.Y, ellipseWidth, ellipseHeight, ellipseAngleDeg, p.X, p.Y);
		}


		/// <summary>
		/// Determines if a rotated ellipse contains a point
		/// /// </summary>
		/// <param name="ellipseCenterX">The center point of the ellipse and the rotation center</param>
		/// <param name="ellipseCenterY">The center point of the ellipse and the rotation center</param>
		/// <param name="ellipseWidth">The width of the ellipse, equal to the doubled length of the major half axis</param>
		/// <param name="ellipseHeight">The height of the ellipse, equal to the doubled length of the minor half axis</param>
		/// <param name="ellipseAngleDeg">The rotation angle of the ellipse in degrees</param>
		/// <param name="x">The x-coordinate of the point to test</param>
		/// <param name="y">The y-coordinate of the point to test</param>
		/// <returns></returns>
		public static bool EllipseContainsPoint(int ellipseCenterX, int ellipseCenterY, int ellipseWidth, int ellipseHeight, float ellipseAngleDeg, int x, int y) {
			// Standard ellipse formula:
			// (x² / a²) + (y² / b²) = 1
			// Where a = radiusX and b = radiusY
			float radiusX = ellipseWidth / 2f;
			float radiusY = ellipseHeight / 2f;
			// instead of rotating the ellipse, we rotate the line to the opposite side and then rotate intersection points back
			if (ellipseAngleDeg != 0)
				RotatePoint(ellipseCenterX, ellipseCenterY, -ellipseAngleDeg, ref x, ref y);
			// transform x/y to the origin of the coordinate system
			float x0 = x - ellipseCenterX;
			float y0 = y - ellipseCenterY;
			return ((x0 * x0) / (radiusX * radiusX)) + ((y0 * y0) / (radiusY * radiusY)) <= 1;
		}


		/// <summary>
		/// Determines if a circle represented by a center point and a radius contains a point
		/// </summary>
		/// <param name="centerX">The x-coordinate of the circle's center</param>
		/// <param name="centerY">The y-coordinate of the circle's center</param>
		/// <param name="radius">The radius of the circle</param>
		/// <param name="pointX">The x-coordinate of the point to test</param>
		/// <param name="pointY">The y-coordinate of the point to test</param>
		/// <param name="delta">The tolerance of the calculation</param>
		public static bool CircleContainsPoint(float centerX, float centerY, float radius, float pointX, float pointY, float delta) {
			float pDistance = Math.Abs(DistancePointPoint(pointX, pointY, centerX, centerY));
			return pDistance <= radius + delta;
		}


		/// <summary>
		/// Determines if a circle represented by a center point and a radius contains a point
		/// </summary>
		/// <param name="centerX">The x-coordinate of the circle's center</param>
		/// <param name="centerY">The y-coordinate of the circle's center</param>
		/// <param name="radius">The radius of the circle</param>
		/// <param name="pointX">The x-coordinate of the point to test</param>
		/// <param name="pointY">The y-coordinate of the point to test</param>
		/// <param name="delta">The tolerance of the calculation</param>
		public static bool CircleContainsPoint(int centerX, int centerY, float radius, int pointX, int pointY, float delta) {
			float pDistance = Math.Abs(DistancePointPoint(pointX, pointY, centerX, centerY));
			return pDistance <= radius + delta;
		}


		/// <summary>
		/// Determines if a circle outline represented by a center point, a radius and the width of the outline contains a point
		/// </summary>
		/// <param name="centerX">The x-coordinate of the circle's center</param>
		/// <param name="centerY">The y-coordinate of the circle's center</param>
		/// <param name="radius">The radius of the circle</param>
		/// <param name="pX">The x-coordinate of the point to test</param>
		/// <param name="pY">The y-coordinate of the point to test</param>
		/// <param name="delta">The tolerance of the calculation</param>
		public static bool CircleOutlineContainsPoint(int centerX, int centerY, float radius, int pX, int pY, float delta) {
			float pDistance = Math.Abs(DistancePointPoint(pX, pY, centerX, centerY));
			if ((pDistance <= radius + delta) && (pDistance >= radius - delta))
				return true;
			else
				return false;
		}


		/// <summary>
		/// Returns true if the arc defined by arcCenter and the three points startPoint, endPoint and radiusPoint contains the given point.
		/// </summary>
		public static bool ArcContainsPoint(Point arcCenter, Point startPoint, Point endPoint, Point radiusPoint, Point point, float delta) {
			return ArcContainsPoint(arcCenter.X, arcCenter.Y, startPoint.X, startPoint.Y, endPoint.X, endPoint.Y, delta, point.X, point.Y);
		}


		/// <summary>
		/// Determines if an arc, defined by three points, contains a point
		/// </summary>
		/// <param name="startPoint">The point defining the beginning of the arc</param>
		/// <param name="radiusPoint">The point defining the radius of the arc</param>
		/// <param name="endPoint">The point defining end of the arc</param>
		/// <param name="point">The point to test</param>
		/// <param name="delta">The tolerance of the calculation</param>
		public static bool ArcContainsPoint(PointF startPoint, PointF radiusPoint, PointF endPoint, PointF point, float delta) {
			return ArcContainsPoint(startPoint.X, startPoint.Y, radiusPoint.X, radiusPoint.Y, endPoint.X, endPoint.Y, delta, point.X, point.Y);
		}


		/// <summary>
		/// Determines if an arc, defined by three points, contains a point
		/// </summary>
		/// <param name="startPointX">The x-coordinate of the point defining the beginning of the arc</param>
		/// <param name="startPointY">The y-coordinate of the point defining  the beginning of the arc</param>
		/// <param name="radiusPointX">The x-coordinate of the point defining the radius of the arc</param>
		/// <param name="radiusPointY">The y-coordinate of the point defining the radius of the arc</param>
		/// <param name="endPointX">The x-coordinate of the point defining end of the arc</param>
		/// <param name="endPointY">The x-coordinate of the point defining end of the arc</param>
		/// <param name="delta">The tolerance of the calculation</param>
		/// <param name="pointX">The x-coordinate of the point to test</param>
		/// <param name="pointY">The y-coordinate of the point to test</param>
		public static bool ArcContainsPoint(float startPointX, float startPointY, float radiusPointX, float radiusPointY, float endPointX, float endPointY, float delta, float pointX, float pointY) {
			float radius;
			PointF center;
			CalcCircumCircle(startPointX, startPointY, radiusPointX, radiusPointY, endPointX, endPointY, out center, out radius);
			return ArcContainsPoint(startPointX, startPointY, radiusPointX, radiusPointY, endPointX, endPointY, center.X, center.Y, radius, pointX, pointY, delta);
		}


		/// <summary>
		/// Determines if an arc contains a point
		/// </summary>
		/// <param name="arcCenterX">The x-coordinate of the arc's center point</param>
		/// <param name="arcCenterY">The y-coordinate of the arc's center point</param>
		/// <param name="startPointX">The x-coordinate of the point defining the beginning of the arc</param>
		/// <param name="startPointY">The y-coordinate of the point defining  the beginning of the arc</param>
		/// <param name="endPointX">The x-coordinate of the point defining end of the arc</param>
		/// <param name="endPointY">The x-coordinate of the point defining end of the arc</param>
		/// <param name="radiusPointX">The x-coordinate of the point defining the radius of the arc</param>
		/// <param name="radiusPointY">The y-coordinate of the point defining the radius of the arc</param>
		/// <param name="pointX">The x-coordinate of the point to test</param>
		/// <param name="pointY">The y-coordinate of the point to test</param>
		/// <param name="delta">The tolerance of the calculation</param>
		public static bool ArcContainsPoint(int arcCenterX, int arcCenterY, int startPointX, int startPointY, int endPointX, int endPointY, int radiusPointX, int radiusPointY, int pointX, int pointY, float delta) {
			//if the point is on the arc's circle...
			float distance = Math.Abs(DistancePointPoint(pointX, pointY, arcCenterX, arcCenterY));
			float radius = Math.Abs(DistancePointPoint(startPointX, startPointY, arcCenterX, arcCenterY));
			if (radius - delta <= distance && distance <= radius + delta) {
				// ... and if Point and RadiusPoint are on the same side -> arc contains point
				float distancePt = DistancePointLine(pointX, pointY, startPointX, startPointY, endPointX, endPointY, false);
				float distanceRadPt = DistancePointLine(radiusPointX, radiusPointY, startPointX, startPointY, endPointX, endPointY, false);
				if (distancePt < 0 && distanceRadPt < 0 || distancePt >= 0 && distanceRadPt >= 0)
					return true;
			}
			return false;
		}


		/// <summary>
		/// Determines if an arc contains a point. This function takes all parameters of the arc so it does not have to perform redundant calculations.
		/// </summary>
		/// <param name="startPointX">The x-coordinate of the point defining the beginning of the arc</param>
		/// <param name="startPointY">The y-coordinate of the point defining  the beginning of the arc</param>
		/// <param name="radiusPointX">The x-coordinate of the point defining the radius of the arc</param>
		/// <param name="radiusPointY">The y-coordinate of the point defining the radius of the arc</param>
		/// <param name="endPointX">The x-coordinate of the point defining end of the arc</param>
		/// <param name="endPointY">The x-coordinate of the point defining end of the arc</param>
		/// <param name="arcCenterX">The x-coordinate of the arc's center point</param>
		/// <param name="arcCenterY">The y-coordinate of the arc's center point</param>
		/// <param name="arcRadius">The radius of the arc</param>
		/// <param name="pointX">The x-coordinate of the point to test</param>
		/// <param name="pointY">The y-coordinate of the point to test</param>
		/// <param name="delta">The tolerance of the calculation</param>
		public static bool ArcContainsPoint(float startPointX, float startPointY, float radiusPointX, float radiusPointY, float endPointX, float endPointY, float arcCenterX, float arcCenterY, float arcRadius, float pointX, float pointY, float delta) {
			// check if the point is on the arc's circle
			float distance = Math.Abs(DistancePointPoint(pointX, pointY, arcCenterX, arcCenterY));
			if (arcRadius - delta <= distance && distance <= arcRadius + delta) {
				float twoPi = (float)(Math.PI + Math.PI);

				// First, sort the angles
				float startPtAngle = Angle(arcCenterX, arcCenterY, startPointX, startPointY);
				if (startPtAngle < 0) startPtAngle += twoPi;
				float radiusPtAngle = Angle(arcCenterX, arcCenterY, radiusPointX, radiusPointY);
				if (radiusPtAngle < 0) radiusPtAngle += twoPi;
				float endPtAngle = Angle(arcCenterX, arcCenterY, endPointX, endPointY);
				if (endPtAngle < 0) endPtAngle += twoPi;
				float pointAngle = Angle(arcCenterX, arcCenterY, pointX, pointY);
				if (pointAngle < 0) pointAngle += twoPi;

				// Then compare the point's angle with the sorted angles of the arc
				if (startPtAngle <= radiusPtAngle && radiusPtAngle <= endPtAngle) {
					if (startPtAngle <= pointAngle && pointAngle <= endPtAngle)
						return true;
					else return false;
				} else if (endPtAngle <= radiusPtAngle && radiusPtAngle <= startPtAngle) {
					if (endPtAngle <= pointAngle && pointAngle <= startPtAngle)
						return true;
					else return false;
				} else if (startPtAngle <= radiusPtAngle && endPtAngle <= radiusPtAngle) {
					if (startPtAngle < endPtAngle) {
						if (startPtAngle < pointAngle && pointAngle < endPtAngle)
							return false;
						else return true;
					} else if (endPtAngle < startPtAngle) {
						if (endPtAngle < pointAngle && pointAngle < startPtAngle)
							return false;
						else return true;
					}
				} else if (radiusPtAngle <= startPtAngle && radiusPtAngle <= endPtAngle) {
					if (startPtAngle < endPtAngle) {
						if (startPtAngle < pointAngle && pointAngle < endPtAngle)
							return false;
						else return true;
					} else if (endPtAngle < startPtAngle) {
						if (endPtAngle < pointAngle && pointAngle < startPtAngle)
							return false;
						else return true;
					}
				}
			}
			return false;
		}

		#endregion


		#region Intersection test functions

		/// <ToBeCompleted></ToBeCompleted>
		public static bool PolygonIntersectsWithRectangle(Point[] points, Rectangle rectangle) {
			return PolygonIntersectsWithRectangle(points, rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static bool PolygonIntersectsWithRectangle(Point[] points, int rectangleLeft, int rectangleTop, int rectangleRight, int rectangleBottom) {
			int left = int.MaxValue;
			int top = int.MaxValue;
			int right = int.MinValue;
			int bottom = int.MinValue;

			int rectangleWidth = rectangleRight - rectangleLeft;
			int rectangleHeight = rectangleBottom - rectangleTop;
			int maxIdx = points.Length - 1;
			for (int i = 0; i < maxIdx; ++i) {
				Point a = points[i];
				Point b = points[i + 1];
				if (a.X < left) left = a.X;
				if (a.X > right) right = a.X;
				if (a.Y < top) top = a.Y;
				if (a.Y > bottom) bottom = a.Y;

				// The polygon intersects the Rectangle if the rectangle contains one point of the polygon...
				if (RectangleContainsPoint(rectangleLeft, rectangleTop, rectangleWidth, rectangleHeight, a.X, a.Y))
					return true;
				if (RectangleContainsPoint(rectangleLeft, rectangleTop, rectangleWidth, rectangleHeight, b.X, b.Y))
					return true;

				// ... or if one side of the polygon intersects one side of the rectangle ...
				if (RectangleIntersectsWithLine(rectangleLeft, rectangleTop, rectangleRight, rectangleBottom, a.X, a.Y, b.X, b.Y, true))
					return true;
			}
			if (RectangleIntersectsWithLine(rectangleLeft, rectangleTop, rectangleRight, rectangleBottom, points[maxIdx].X, points[maxIdx].Y, points[0].X, points[0].Y, true))
				return true;

			// ... or if the rectangle is inside of the polygon...
			//if (left <= rectangle.left && top <= rectangle.top && right >= rectangle.right && bottom >= rectangle.bottom)
			//   return true;
			if (ConvexPolygonContainsPoint(points, rectangleLeft, rectangleTop) &&
				ConvexPolygonContainsPoint(points, rectangleRight, rectangleTop) &&
				ConvexPolygonContainsPoint(points, rectangleRight, rectangleBottom) &&
				ConvexPolygonContainsPoint(points, rectangleLeft, rectangleBottom))
				return true;
			return false;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static bool PolygonIntersectsWithRectangle(PointF[] pointFs, Rectangle rectangle) {
			return PolygonIntersectsWithRectangle(pointFs, (RectangleF)rectangle);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static bool PolygonIntersectsWithRectangle(PointF[] points, RectangleF rectangle) {
			if (points == null) throw new ArgumentNullException(nameof(points));
			if (points.Length <= 0) return false;
			int maxIdx = points.Length - 1;
			for (int i = 0; i < maxIdx; ++i) {
				PointF a = points[i];
				PointF b = points[i + 1];
				// The polygon intersects the Rectangle if the rectangle contains one point of the polygon...
				if (RectangleContainsPoint(rectangle, a.X, a.Y))
					return true;
				if (RectangleContainsPoint(rectangle, b.X, b.Y))
					return true;
				// ... or if one side of the polygon intersects one side of the rectangle ...
				if (RectangleIntersectsWithLine(rectangle, a.X, a.Y, b.X, b.Y, true))
					return true;
			}
			if (RectangleIntersectsWithLine(rectangle, points[maxIdx].X, points[maxIdx].Y, points[0].X, points[0].Y, true))
				return true;

			// ... or if the rectangle is inside of the polygon...
			if (ConvexPolygonContainsPoint(points, rectangle.Left, rectangle.Top) &&
				ConvexPolygonContainsPoint(points, rectangle.Right, rectangle.Top) &&
				ConvexPolygonContainsPoint(points, rectangle.Right, rectangle.Bottom) &&
				ConvexPolygonContainsPoint(points, rectangle.Left, rectangle.Bottom))
				return true;
			return false;
		}


		/// <summary>
		/// Bestimmt, ob die beiden Geraden sich schneiden.
		/// </summary>
		public static bool LineIntersectsWithLine(Point a1, Point a2, Point b1, Point b2) {
			return LineIntersectsWithLine(a1.X, a1.Y, a2.X, a2.Y, b1.X, b1.Y, b2.X, b2.Y);
		}


		/// <summary>
		/// Bestimmt, ob die beiden Geraden sich schneiden.
		/// </summary>
		public static bool LineIntersectsWithLine(Point a1, Point a2, int b1X, int b1Y, int b2X, int b2Y) {
			return LineIntersectsWithLine(a1.X, a1.Y, a2.X, a2.Y, b1X, b1Y, b2X, b2Y);
		}


		/// <summary>
		/// Bestimmt, ob die beiden Geraden sich schneiden.
		/// </summary>
		public static bool LineIntersectsWithLine(int a1X, int a1Y, int a2X, int a2Y, int b1X, int b1Y, int b2X, int b2Y) {
			// Richtungsvectoren der beiden Geraden berechnen			
			Point line1Vec = Point.Empty;               // Richtungsvektor Linie1
			line1Vec.Offset(a2X - a1X, a2Y - a1Y);
			Point line2Vec = Point.Empty;               // Richtungsvector Linie2
			line2Vec.Offset(b2X - b1X, b2Y - b1Y);

			// Determinante det berechnen
			double det = (double)line1Vec.Y * line2Vec.X - (double)line1Vec.X * line2Vec.Y;

			// Wenn det == 0 ist, sind die Linien parallel. 
			if (Equals(det, 0)) {
				// Wenn die Linien gegensinnig laufen, also die Vektoren die Beträge [10|10] und [-10|-10] haben, schneiden sie 
				// sich zwangsläufig - vorausgesetzt sie sind nicht parallel verschoben!
				if (line1Vec.X == -line2Vec.X && line1Vec.Y == -line2Vec.Y) {
					float m1, m2, c1, c2;
					Geometry.CalcLine(a1X, a1Y, a2X, a2Y, out m1, out c1);
					Geometry.CalcLine(b1X, b1Y, b2X, b2Y, out m2, out c2);
					return (Math.Abs(c1 - c2) < EqualityDeltaFloat);
				} else return false;
			}

			// Determinante det's berechnen 
			double ds = (b1Y - a1Y) * (double)line2Vec.X - (b1X - a1X) * (double)line2Vec.Y;

			// detdet / det berechnen und prüfen, ob s in den Grenzen liegt 
			double s = ds / det;
			if (s < 0.0f || s > 1.0f)
				return false;

			// dt und db berechnen.
			double dt = (b1Y - a1Y) * (double)line1Vec.X - (b1X - a1X) * (double)line1Vec.Y;
			double t = dt / det;
			if (t < 0.0f || t > 1.0f)
				return false;

			return true;
		}


		/// <summary>
		/// Bestimmt, ob die beiden Geraden sich schneiden.
		/// </summary>
		public static bool LineIntersectsWithLine(PointF a1, PointF a2, PointF b1, PointF b2) {
			return LineIntersectsWithLine(a1.X, a1.Y, a2.X, a2.Y, b1.X, b1.Y, b2.X, b2.Y);
		}


		/// <summary>
		/// Bestimmt, ob die beiden Geraden sich schneiden.
		/// </summary>
		public static bool LineIntersectsWithLine(float a1X, float a1Y, float a2X, float a2Y, float b1X, float b1Y, float b2X, float b2Y) {
			// Richtungsvectoren der beiden Geraden berechnen			
			PointF line1Vec = Point.Empty;              // Richtungsvektor Linie1
			line1Vec.X = a2X - a1X;
			line1Vec.Y = a2Y - a1Y;
			PointF line2Vec = Point.Empty;              // Richtungsvector Linie2
			line2Vec.X = b2X - b1X;
			line2Vec.Y = b2Y - b1Y;

			// Determinante det berechnen
			double det = line1Vec.Y * line2Vec.X - line1Vec.X * line2Vec.Y;

			// Wenn det == 0 ist, sind die Linien parallel. 
			if (Equals(det, 0)) {
				// Wenn die Linien gegensinnig laufen, also die Vektoren die Beträge [10|10] und [-10|-10] haben, schneiden sie 
				// sich zwangsläufig - vorausgesetzt sie sind nicht parallel verschoben!
				if (line1Vec.X == -line2Vec.X && line1Vec.Y == -line2Vec.Y) {
					float m1, m2, c1, c2;
					Geometry.CalcLine(a1X, a1Y, a2X, a2Y, out m1, out c1);
					Geometry.CalcLine(b1X, b1Y, b2X, b2Y, out m2, out c2);
					return (Math.Abs(c1 - c2) < EqualityDeltaFloat);
				} else return false;
			}

			// Determinante det's berechnen 
			double detdet = (b1Y - a1Y) * line2Vec.X - (b1X - a1X) * line2Vec.Y;

			// detdet / det berechnen und prüfen, ob das Ergebnis in den Grenzen liegt
			double s = detdet / det;
			if (s < 0.0f || s > 1.0f)
				return false;

			// dt und db berechnen.
			double dt = (b1Y - a1Y) * line1Vec.X - (b1X - a1X) * line1Vec.Y;
			double t = dt / det;
			if (t < 0.0f || t > 1.0f)
				return false;

			return true;
		}


		/// <summary>
		/// Bestimmt, ob die Gerade sich mit der Strecke schneidet.
		/// </summary>
		public static bool LineIntersectsWithLineSegment(PointF a1, PointF a2, float b1X, float b1Y, float b2X, float b2Y) {
			return LineIntersectsWithLineSegment(a1.X, a1.Y, a2.X, a2.Y, b1X, b1Y, b2X, b2Y);
		}


		/// <summary>
		/// Bestimmt, ob die Gerade sich mit der Strecke schneidet.
		/// </summary>
		public static bool LineIntersectsWithLineSegment(float a1X, float a1Y, float a2X, float a2Y, float b1X, float b1Y, float b2X, float b2Y) {
			return IsValid(IntersectLineWithLineSegment(a1X, a1Y, a2X, a2Y, b1X, b1Y, b2X, b2Y));
		}


		/// <summary>
		/// Bestimmt, ob die Gerade sich mit der Strecke schneidet.
		/// </summary>
		public static bool LineIntersectsWithLineSegment(Point a1, Point a2, Point b1, Point b2) {
			return LineIntersectsWithLineSegment(a1.X, a1.Y, a2.X, a2.Y, b1.X, b1.Y, b2.X, b2.Y);
		}


		/// <summary>
		/// Bestimmt, ob die Gerade sich mit der Strecke schneidet.
		/// </summary>
		public static bool LineIntersectsWithLineSegment(Point a1, Point a2, int b1X, int b1Y, int b2X, int b2Y) {
			return LineIntersectsWithLineSegment(a1.X, a1.Y, a2.X, a2.Y, b1X, b1Y, b2X, b2Y);
		}


		/// <summary>
		/// Bestimmt, ob die Gerade sich mit der Strecke schneidet.
		/// </summary>
		public static bool LineIntersectsWithLineSegment(int a1X, int a1Y, int a2X, int a2Y, int b1X, int b1Y, int b2X, int b2Y) {
			return IsValid(IntersectLineWithLineSegment(a1X, a1Y, a2X, a2Y, b1X, b1Y, b2X, b2Y));
		}


		/// <summary>
		/// Bestimmt, ob die beiden Strecken sich schneiden.
		/// </summary>
		public static bool LineSegmentIntersectsWithLineSegment(int a1X, int a1Y, int a2X, int a2Y, int b1X, int b1Y, int b2X, int b2Y) {
			return IsValid(IntersectLineSegments(a1X, a1Y, a2X, a2Y, b1X, b1Y, b2X, b2Y));
		}


		/// <summary>
		/// Bestimmt, ob die beiden Strecken sich schneiden.
		/// </summary>
		[Obsolete]
		public static bool LineSegmentIntersectsWithLineSegment(long a1X, long a1Y, long a2X, long a2Y, long b1X, long b1Y, long b2X, long b2Y) {
			return IsValid(IntersectLineSegments(a1X, a1Y, a2X, a2Y, b1X, b1Y, b2X, b2Y));
		}


		/// <summary>
		/// Bestimmt, ob die beiden Strecken sich schneiden.
		/// </summary>
		public static bool LineSegmentIntersectsWithLineSegment(float a1X, float a1Y, float a2X, float a2Y, float b1X, float b1Y, float b2X, float b2Y) {
			return IsValid(IntersectLineSegments(a1X, a1Y, a2X, a2Y, b1X, b1Y, b2X, b2Y));
		}


		/// <summary>
		/// Checks whether the rectangle area intersects with the line. 
		/// If isSegmnt is true, the line is treated as line segment.
		/// </summary>
		public static bool RectangleIntersectsWithLine(Rectangle rectangle, Point a1, Point a2, bool isSegment) {
			return RectangleIntersectsWithLine(rectangle, a1.X, a1.Y, a2.X, a2.Y, isSegment);
		}


		/// <summary>
		/// Checks whether the rectangle intersects with the line. 
		/// If isSegmnt is true, the line is treated as line segment.
		/// </summary>
		public static bool RectangleIntersectsWithLine(Rectangle rectangle, int a1X, int a1Y, int a2X, int a2Y, bool isSegment) {
			// Strecke schneidet das Rechteck, wenn das Recteck einen der Linien-Endpunkte enthält 
			// oder wenn sie eine der Seiten schneidet
			if (RectangleContainsPoint(rectangle, a1X, a1Y) || RectangleContainsPoint(rectangle, a2X, a2Y))
				return true;
			else {
				// sind beide Punkte der Strecke auf einer Seite des Rechtecks kann es keinen Schnittpunkt geben
				if (isSegment && ((a1X < rectangle.Left && a2X < rectangle.Left)
					|| (a1X > rectangle.Right && a2X > rectangle.Right)
					|| (a1Y < rectangle.Top && a2Y < rectangle.Top)
					|| (a1Y > rectangle.Bottom && a2Y > rectangle.Bottom)))
					return false;
				else {
					if (isSegment)
						return (LineSegmentIntersectsWithLineSegment(a1X, a1Y, a2X, a2Y, rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Top) ||
								LineSegmentIntersectsWithLineSegment(a1X, a1Y, a2X, a2Y, rectangle.Right, rectangle.Top, rectangle.Right, rectangle.Bottom) ||
								LineSegmentIntersectsWithLineSegment(a1X, a1Y, a2X, a2Y, rectangle.Left, rectangle.Bottom, rectangle.Right, rectangle.Bottom) ||
								LineSegmentIntersectsWithLineSegment(a1X, a1Y, a2X, a2Y, rectangle.Left, rectangle.Top, rectangle.Left, rectangle.Bottom));
					else
						return (LineIntersectsWithLineSegment(a1X, a1Y, a2X, a2Y, rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Top) ||
								LineIntersectsWithLineSegment(a1X, a1Y, a2X, a2Y, rectangle.Right, rectangle.Top, rectangle.Right, rectangle.Bottom) ||
								LineIntersectsWithLineSegment(a1X, a1Y, a2X, a2Y, rectangle.Left, rectangle.Bottom, rectangle.Right, rectangle.Bottom) ||
								LineIntersectsWithLineSegment(a1X, a1Y, a2X, a2Y, rectangle.Left, rectangle.Top, rectangle.Left, rectangle.Bottom));
				}
			}
		}


		/// <summary>
		/// Checks whether the rectangle intersects with the line. 
		/// If isSegmnt is true, the line is treated as line segment.
		/// </summary>
		public static bool RectangleIntersectsWithLine(RectangleF rectangle, PointF a1, PointF a2, bool isSegment) {
			return RectangleIntersectsWithLine(rectangle, a1.X, a1.Y, a2.X, a2.Y, isSegment);
		}


		/// <summary>
		/// Checks whether the rectangle intersects with the line. 
		/// If isSegmnt is true, the line is treated as line segment.
		/// </summary>
		public static bool RectangleIntersectsWithLine(RectangleF rectangle, float a1X, float a1Y, float a2X, float a2Y, bool isSegment) {
			// Strecke schneidet das Rechteck, wenn das Recteck einen der Linien-Endpunkte enthält 
			// oder wenn sie eine der Seiten schneidet
			if (RectangleContainsPoint(rectangle, a1X, a1Y) || RectangleContainsPoint(rectangle, a2X, a2Y))
				return true;
			else {
				// sind beide Punkte der Strecke auf einer Seite des Rectecks kann es keinen Schnittpunkt geben
				if (isSegment && ((a1X < rectangle.Left && a2X < rectangle.Left)
					|| (a1X > rectangle.Right && a2X > rectangle.Right)
					|| (a1Y < rectangle.Top && a2Y < rectangle.Top)
					|| (a1Y > rectangle.Bottom && a2Y > rectangle.Bottom)))
					return false;
				else {
					if (isSegment)
						return (LineSegmentIntersectsWithLineSegment(a1X, a1Y, a2X, a2Y, rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Top) ||
								LineSegmentIntersectsWithLineSegment(a1X, a1Y, a2X, a2Y, rectangle.Right, rectangle.Top, rectangle.Right, rectangle.Bottom) ||
								LineSegmentIntersectsWithLineSegment(a1X, a1Y, a2X, a2Y, rectangle.Left, rectangle.Bottom, rectangle.Right, rectangle.Bottom) ||
								LineSegmentIntersectsWithLineSegment(a1X, a1Y, a2X, a2Y, rectangle.Left, rectangle.Top, rectangle.Left, rectangle.Bottom));
					else
						return (LineIntersectsWithLineSegment(a1X, a1Y, a2X, a2Y, rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Top) ||
								LineIntersectsWithLineSegment(a1X, a1Y, a2X, a2Y, rectangle.Right, rectangle.Top, rectangle.Right, rectangle.Bottom) ||
								LineIntersectsWithLineSegment(a1X, a1Y, a2X, a2Y, rectangle.Left, rectangle.Bottom, rectangle.Right, rectangle.Bottom) ||
								LineIntersectsWithLineSegment(a1X, a1Y, a2X, a2Y, rectangle.Left, rectangle.Top, rectangle.Left, rectangle.Bottom));
				}
			}
		}


		/// <summary>
		/// Checks whether the rectangle intersects with the line. 
		/// If isSegmnt is true, the line is treated as line segment.
		/// </summary>
		public static bool RectangleIntersectsWithLine(int left, int top, int right, int bottom, int a1X, int a1Y, int a2X, int a2Y, bool isSegment) {
			return RectangleIntersectsWithLine(Rectangle.FromLTRB(left, top, right, bottom), a1X, a1Y, a2X, a2Y, isSegment);
		}


		/// <summary>
		/// Checks if there is an intersection between the given rectangle (rotated around its center point) and the given line.
		/// </summary>
		/// <param name="rectCenterX">X coordinate of the rectangle's center</param>
		/// <param name="rectCenterY">Y coordinate of the rectangle's center</param>
		/// <param name="rectWidth">Width of the rectangle</param>
		/// <param name="rectHeight">Height of the rectangle</param>
		/// <param name="rectRotationDeg">Rotation of the rectangle in degrees</param>
		/// <param name="a1X">X coordinate of the line's first point</param>
		/// <param name="a1Y">Y coordinate of the line's first point</param>
		/// <param name="a2X">X coordinate of the line's second point</param>
		/// <param name="a2Y">Y coordinate of the line's second point</param>
		/// <param name="isSegment">Specifies whether the line is a line segment</param>
		/// <returns></returns>
		public static bool RectangleIntersectsWithLine(int rectCenterX, int rectCenterY, int rectWidth, int rectHeight, float rectRotationDeg, int a1X, int a1Y, int a2X, int a2Y, bool isSegment) {
			// Instead of rotating the rectangle, we translate both rectangle and line to the origin of the coordinate 
			// system and rotate the line's points in the opposite direction:
			//
			// Calc unrotated but translated rectangle
			RectangleF rect = Rectangle.Empty;
			rect.X = -rectWidth / 2f;
			rect.Y = -rectHeight / 2f;
			rect.Width = rectWidth;
			rect.Height = rectHeight;
			// Calc rotated and translated first point of the line
			Point p1 = Point.Empty;
			p1.Offset(a1X - rectCenterX, a1Y - rectCenterY);
			p1 = RotatePoint(Point.Empty, -rectRotationDeg, p1);
			// Calc rotated and translated second point of the line
			Point p2 = Point.Empty;
			p2.Offset(a2X - rectCenterX, a2Y - rectCenterY);
			p2 = RotatePoint(Point.Empty, -rectRotationDeg, p2);
			// check for intersection
			return RectangleIntersectsWithLine(rect, p1, p2, isSegment);
		}


		/// <summary>
		/// Checks if there is an intersection between the given rectangle (rotated around its center point) and the given line.
		/// </summary>
		/// <param name="rectCenterX">X coordinate of the rectangle's center</param>
		/// <param name="rectCenterY">Y coordinate of the rectangle's center</param>
		/// <param name="rectWidth">Width of the rectangle</param>
		/// <param name="rectHeight">Height of the rectangle</param>
		/// <param name="rectRotationDeg">Rotation of the rectangle in degrees</param>
		/// <param name="a1X">X coordinate of the line's first point</param>
		/// <param name="a1Y">Y coordinate of the line's first point</param>
		/// <param name="a2X">X coordinate of the line's second point</param>
		/// <param name="a2Y">Y coordinate of the line's second point</param>
		/// <param name="isSegment">Specifies whether the line is a line segment</param>
		/// <returns></returns>
		public static bool RectangleIntersectsWithLine(float rectCenterX, float rectCenterY, float rectWidth, float rectHeight, float rectRotationDeg, float a1X, float a1Y, float a2X, float a2Y, bool isSegment) {
			// Instead of rotating the rectangle, we translate both rectangle and line to the origin of the coordinate 
			// system and rotate the line's points in the opposite direction:
			//
			// Calc unrotated but translated rectangle
			RectangleF rect = Rectangle.Empty;
			rect.X = -rectWidth / 2f;
			rect.Y = -rectHeight / 2f;
			rect.Width = rectWidth;
			rect.Height = rectHeight;
			// Calc rotated and translated first point of the line
			PointF p1 = Point.Empty;
			p1.X = a1X - rectCenterX;
			p1.Y = a1Y - rectCenterY;
			p1 = RotatePoint(PointF.Empty, -rectRotationDeg, p1);
			// Calc rotated and translated second point of the line
			PointF p2 = Point.Empty;
			p2.X = a2X - rectCenterX;
			p2.Y = a2Y - rectCenterY;
			p2 = RotatePoint(PointF.Empty, -rectRotationDeg, p2);
			// check for intersection
			return RectangleIntersectsWithLine(rect, p1, p2, isSegment);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static bool CircleIntersectsWithLine(PointF center, float radius, PointF a1, PointF a2, bool isSegment) {
			return Math.Abs(DistancePointLine(center.X, center.Y, a1.X, a1.Y, a2.X, a2.Y, isSegment)) <= radius;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static bool CircleIntersectsWithLine(float centerX, float centerY, float radius, float a1X, float a1Y, float a2X, float a2Y, bool isSegment) {
			return Math.Abs(DistancePointLine(centerX, centerY, a1X, a1Y, a2X, a2Y, isSegment)) <= radius;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static bool ArcIntersectsWithLine(float startPtX, float startPtY, float radiusPtX, float radiusPtY, float endPtX, float endPtY, float a1X, float a1Y, float a2X, float a2Y, bool isSegment) {
			foreach (PointF p in IntersectArcLine(startPtX, startPtY, radiusPtX, radiusPtY, endPtX, endPtY, a1X, a1Y, a2X, a2Y, isSegment))
				return true;
			return false;
		}


		/// <summary>
		/// Checks if an arc defined, by three points, intersects with a rectangle.
		/// </summary>
		/// <param name="startPt">The Arc's frist point</param>
		/// <param name="radiusPt">The arc's second point</param>
		/// <param name="endPt">The Arc's third point</param>
		/// <param name="rect">Rectangle to be checked for intersection</param>
		public static bool ArcIntersectsWithRectangle(PointF startPt, PointF radiusPt, PointF endPt, Rectangle rect) {
			return ArcIntersectsWithRectangle(startPt.X, startPt.Y, radiusPt.X, radiusPt.Y, endPt.X, endPt.Y, rect);
		}


		/// <summary>
		/// Checks if an arc defined, by three points, intersects with a (rotated) rectangle.
		/// </summary>
		/// <param name="startPtX">X Coordinate of the arc's first point</param>
		/// <param name="startPtY">Y Coordinate of the arc's first point</param>
		/// <param name="radiusPtX">X Coordinate of the arc's second point</param>
		/// <param name="radiusPtY">Y Coordinate of the arc's second point</param>
		/// <param name="endPtX">X Coordinate of the arc's third point</param>
		/// <param name="endPtY">Y Coordinate of the arc's third point</param>
		/// <param name="rect">Rectangle to be checked for intersection</param>
		/// <param name="angleDeg">Rotation angle of the rectangle in degrees</param>
		public static bool ArcIntersectsWithRectangle(float startPtX, float startPtY, float radiusPtX, float radiusPtY, float endPtX, float endPtY, Rectangle rect, float angleDeg) {
			// rotate the points defining the arc in the opposite direction, then do the intersection test
			PointF[] pts = new PointF[3];
			pts[0].X = startPtX;
			pts[0].Y = startPtY;
			pts[1].X = radiusPtX;
			pts[1].Y = radiusPtY;
			pts[2].X = endPtX;
			pts[2].Y = endPtY;
			PointF rotationCenter = PointF.Empty;
			rotationCenter.X = rect.X + (rect.Width / 2f);
			rotationCenter.Y = rect.Y + (rect.Height / 2f);
			matrix.Reset();
			matrix.RotateAt(DegreesToRadians(-angleDeg), rotationCenter);
			matrix.TransformPoints(pts);
			// perform intersection test
			return ArcIntersectsWithRectangle(pts[0].X, pts[0].Y, pts[1].X, pts[1].Y, pts[2].X, pts[2].Y, rect);
		}


		/// <summary>
		/// Checks if an arc defined, by three points, intersects with a rectangle.
		/// </summary>
		/// <param name="startPtX">X Coordinate of the arc's first point</param>
		/// <param name="startPtY">Y Coordinate of the arc's first point</param>
		/// <param name="radiusPtX">X Coordinate of the arc's second point</param>
		/// <param name="radiusPtY">Y Coordinate of the arc's second point</param>
		/// <param name="endPtX">X Coordinate of the arc's third point</param>
		/// <param name="endPtY">Y Coordinate of the arc's third point</param>
		/// <param name="rect">Rectangle to be checked for intersection</param>
		public static bool ArcIntersectsWithRectangle(float startPtX, float startPtY, float radiusPtX, float radiusPtY, float endPtX, float endPtY, Rectangle rect) {
			float radius;
			PointF center;
			CalcCircumCircle(startPtX, startPtY, radiusPtX, radiusPtY, endPtX, endPtY, out center, out radius);
			return ArcIntersectsWithRectangle(startPtX, startPtY, radiusPtX, radiusPtY, endPtX, endPtY, center.X, center.Y, radius, rect);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static bool ArcIntersectsWithRectangle(float startPtX, float startPtY, float radiusPtX, float radiusPtY, float endPtX, float endPtY, float centerX, float centerY, float radius, Rectangle rect) {
			float left = rect.Left;
			float top = rect.Top;
			float right = rect.Right;
			float bottom = rect.Bottom;
			// Check if the rectangle contains any of the arc's points...
			if (left <= startPtX && startPtX <= right && top <= startPtY && startPtY <= bottom)
				return true;
			if (left <= radiusPtX && radiusPtX <= right && top <= radiusPtY && radiusPtY <= bottom)
				return true;
			if (left <= endPtX && endPtX <= right && top <= endPtY && endPtY <= bottom)
				return true;
			// check the sides of the rectangle if one one of then intersects with the arc
			PointF p;
			p = IntersectCircleWithLine(centerX, centerY, radius, left, top, right, top, true);
			if (IsValid(p) && ArcContainsPoint(startPtX, startPtY, radiusPtX, radiusPtY, endPtX, endPtY, centerX, centerY, radius, p.X, p.Y, 0.01f))
				return true;
			p = IntersectCircleWithLine(centerX, centerY, radius, right, top, right, bottom, true);
			if (IsValid(p) && ArcContainsPoint(startPtX, startPtY, radiusPtX, radiusPtY, endPtX, endPtY, centerX, centerY, radius, p.X, p.Y, 0.01f))
				return true;
			p = IntersectCircleWithLine(centerX, centerY, radius, right, bottom, left, bottom, true);
			if (IsValid(p) && ArcContainsPoint(startPtX, startPtY, radiusPtX, radiusPtY, endPtX, endPtY, centerX, centerY, radius, p.X, p.Y, 0.01f))
				return true;
			p = IntersectCircleWithLine(centerX, centerY, radius, left, bottom, left, top, true);
			if (IsValid(p) && ArcContainsPoint(startPtX, startPtY, radiusPtX, radiusPtY, endPtX, endPtY, centerX, centerY, radius, p.X, p.Y, 0.01f))
				return true;
			return false;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static bool CircleIntersectsWithCircle(PointF center1, float radius1, PointF center2, float radius2) {
			float distance = DistancePointPoint(center1, center2);
			return (distance <= radius1 || distance <= radius2);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static bool CircleIntersectsWithRectangle(Rectangle rect, Point center, float radius) {
			return CircleIntersectsWithRectangle(rect, center.X, center.Y, radius);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static bool CircleIntersectsWithRectangle(Rectangle rect, int centerX, int centerY, float radius) {
			float radiusSq = radius * radius;

			// Translate coordinates, placing center at the origin.
			// Use float in order to avoid Arithmetic overflows (e.g. for right² + bottom², see below)
			float left = rect.Left - centerX;
			float top = rect.Top - centerY;
			float right = left + rect.Width;
			float bottom = top + rect.Height;

			// rect to left of circle center
			if (right < 0) {
				// rect in lower left corner
				if (bottom < 0)
					return ((right * right + bottom * bottom) < radiusSq);
				// rect in upper left corner
				else if (top > 0)
					return ((right * right + top * top) < radiusSq);
				// rect due West of circle
				else
					return (Math.Abs(right) < radius);
			}
			// rect to right of circle center
			else if (left > 0) {
				// rect in lower right corner
				if (bottom < 0)
					return ((left * left + bottom * bottom) < radiusSq);
				// rect in upper right corner
				else if (top > 0)
					return ((left * left + top * top) < radiusSq);
				// rect due East of circle
				else
					return (left < radius);
			}
			// rect on circle vertical centerline
			else
				// rect due South of circle
				if (bottom < 0)
				return (Math.Abs(bottom) < radius);
			// rect due North of circle
			else if (top > 0)
				return (top < radius);
			// rect contains circle centerpoint
			else
				return true;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static bool CircleIntersectsWithRectangle(Rectangle rect, PointF center, float radius) {
			return CircleIntersectsWithRectangle(rect, center.X, center.Y, radius);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static bool CircleIntersectsWithRectangle(Rectangle rect, float centerX, float centerY, float radius) {
			float radiusSq = radius * radius;

			// Translate coordinates, placing center at the origin.
			float left = rect.Left - centerX;
			float top = rect.Top - centerY;
			float right = left + rect.Width;
			float bottom = top + rect.Height;

			// rect to left of circle center
			if (right < 0) {
				// rect in lower left corner
				if (bottom < 0)
					return ((right * right + bottom * bottom) < radiusSq);
				// rect in upper left corner
				else if (top > 0)
					return ((right * right + top * top) < radiusSq);
				// rect due West of circle
				else
					return (Math.Abs(right) < radius);
			}
			// rect to right of circle center
			else if (left > 0) {
				// rect in lower right corner
				if (bottom < 0)
					return ((left * left + bottom * bottom) < radiusSq);
				// rect in upper right corner
				else if (top > 0)
					return ((left * left + top * top) < radiusSq);
				// rect due East of circle
				else
					return (left < radius);
			}
			// rect on circle vertical centerline
			else
				// rect due South of circle
				if (bottom < 0)
				return (Math.Abs(bottom) < radius);
			// rect due North of circle
			else if (top > 0)
				return (top < radius);
			// rect contains circle centerpoint
			else
				return true;
		}


		/// <summary>
		/// Check if the (rotated) rectangle intersects with the given circle
		/// </summary>
		public static bool CircleIntersectsWithRectangle(Rectangle rect, float angleDeg, Point center, float radius) {
			// rotate the circle in the opposite direction, then perform a check for unrotated objects
			PointF[] pt = new PointF[1] { center };
			PointF rotationCenter = PointF.Empty;
			rotationCenter.X = rect.X + (rect.Width / 2f);
			rotationCenter.Y = rect.Y + (rect.Height / 2f);
			matrix.Reset();
			matrix.RotateAt(DegreesToRadians(-angleDeg), rotationCenter);
			matrix.TransformPoints(pt);
			return CircleIntersectsWithRectangle(rect, pt[0], radius);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static bool EllipseIntersectsWithLine(Point center, int ellipseWidth, int ellipseHeight, Point a1, Point a2, bool isSegment) {
			return EllipseIntersectsWithLine(center.X, center.Y, ellipseWidth, ellipseHeight, 0, a1.X, a1.Y, a2.X, a2.Y, isSegment);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static bool EllipseIntersectsWithLine(Point center, int ellipseWidth, int ellipseHeight, float ellipseAngleDeg, Point a1, Point a2, bool isSegment) {
			return EllipseIntersectsWithLine(center.X, center.Y, ellipseWidth, ellipseHeight, ellipseAngleDeg, a1.X, a1.Y, a2.X, a2.Y, isSegment);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static bool EllipseIntersectsWithLine(int centerX, int centerY, int ellipseWidth, int ellipseHeight, int a1X, int a1Y, int a2X, int a2Y, bool isSegment) {
			return EllipseIntersectsWithLine(centerX, centerY, ellipseWidth, ellipseHeight, 0, a1X, a1Y, a2X, a2Y, isSegment);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static bool EllipseIntersectsWithLine(Point center, int ellipseWidth, int ellipseHeight, float ellipseAngleDeg, PointF a1, PointF a2, bool isSegment) {
			return EllipseIntersectsWithLine(center.X, center.Y, ellipseWidth, ellipseHeight, ellipseAngleDeg, a1.X, a1.Y, a2.X, a2.Y, isSegment);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static bool EllipseIntersectsWithLine(Point center, int ellipseWidth, int ellipseHeight, PointF a1, PointF a2, bool isSegment) {
			return EllipseIntersectsWithLine(center.X, center.Y, ellipseWidth, ellipseHeight, 0, a1.X, a1.Y, a2.X, a2.Y, isSegment);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static bool EllipseIntersectsWithLine(int centerX, int centerY, int ellipseWidth, int ellipseHeight, float a1X, float a1Y, float a2X, float a2Y, bool isSegment) {
			// instead of rotating the ellipse, we rotate the line in the opposite direction
			return EllipseIntersectsWithLine(centerX, centerY, ellipseWidth, ellipseHeight, 0, a1X, a1Y, a2X, a2Y, isSegment);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static bool EllipseIntersectsWithLine(int centerX, int centerY, int ellipseWidth, int ellipseHeight, float ellipseAngleDeg, float a1X, float a1Y, float a2X, float a2Y, bool isSegment) {
			Point result = Point.Empty;
			// Use only double or only float but do not mix! (Results in much better performance)
			double radiusX = ellipseWidth / 2f;
			double radiusY = ellipseHeight / 2f;
			double rrx = radiusX * radiusX;
			double rry = radiusY * radiusY;
			if (ellipseAngleDeg != 0) {
				RotatePoint(centerX, centerY, -ellipseAngleDeg, ref a1X, ref a1Y);
				RotatePoint(centerX, centerY, -ellipseAngleDeg, ref a2X, ref a2Y);
			}
			double x21 = a2X - a1X;
			double y21 = a2Y - a1Y;
			double x10 = a1X - centerX;
			double y10 = a1Y - centerY;
			double a = x21 * x21 / rrx + y21 * y21 / rry;
			double b = x21 * x10 / rrx + y21 * y10 / rry;
			double c = x10 * x10 / rrx + y10 * y10 / rry;
			double d = b * b - a * (c - 1);
			if (d < 0)
				return false;
			else {
				if (isSegment) {
					double rd = Math.Sqrt(d);
					double u1 = (-b - rd) / a;
					double u2 = (-b + rd) / a;
					// If the line has length 0 or the line starts at the center of the ellipse, u1 and/or u2 will be NaN.
					if (!double.IsNaN(u1) && (0 <= u1 && u1 <= 1))
						return true;
					else if (!double.IsNaN(u2) && (0 <= u2 && u2 <= 1))
						return true;
					else
						return false;
				} else
					return true;
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static bool EllipseIntersectsWithRectangle(int ellipseCenterX, int ellipseCenterY, int ellipseWidth, int ellipseHeight, float ellipseAngleDeg, Rectangle rectangle) {
			// At least one point of the rectangle is inside the ellipse
			if (EllipseContainsPoint(ellipseCenterX, ellipseCenterY, ellipseWidth, ellipseHeight, ellipseAngleDeg, rectangle.Left, rectangle.Top)
				|| EllipseContainsPoint(ellipseCenterX, ellipseCenterY, ellipseWidth, ellipseHeight, ellipseAngleDeg, rectangle.Right, rectangle.Top)
				|| EllipseContainsPoint(ellipseCenterX, ellipseCenterY, ellipseWidth, ellipseHeight, ellipseAngleDeg, rectangle.Right, rectangle.Bottom)
				|| EllipseContainsPoint(ellipseCenterX, ellipseCenterY, ellipseWidth, ellipseHeight, ellipseAngleDeg, rectangle.Left, rectangle.Bottom))
				return true;

			// At least one point of the ellipse is inside the rectangle
			Point ellipseLeft = Point.Empty, ellipseTop = Point.Empty,
				ellipseRight = Point.Empty, ellipseBottom = Point.Empty;
			ellipseLeft.X = (int)Math.Round(ellipseCenterX - (ellipseWidth / 2f));
			ellipseRight.X = ellipseLeft.X + ellipseWidth;
			ellipseLeft.Y = ellipseRight.Y = ellipseCenterY;
			ellipseTop.X = ellipseBottom.X = ellipseCenterX;
			ellipseTop.Y = (int)Math.Round(ellipseCenterY - (ellipseHeight / 2f));
			ellipseBottom.Y = ellipseTop.Y + ellipseHeight;
			if (ellipseAngleDeg % 180 != 0) {
				// No need to rotate any points if the ellipse is upside down or rotated by 360°
				Point ellipseCenter = Point.Empty;
				ellipseCenter.X = ellipseCenterX; ellipseCenter.Y = ellipseCenterY;
				ellipseLeft = RotatePoint(ellipseCenter, ellipseAngleDeg, ellipseLeft);
				ellipseTop = RotatePoint(ellipseCenter, ellipseAngleDeg, ellipseTop);
				ellipseRight = RotatePoint(ellipseCenter, ellipseAngleDeg, ellipseRight);
				ellipseBottom = RotatePoint(ellipseCenter, ellipseAngleDeg, ellipseBottom);
			}
			if (RectangleContainsPoint(rectangle, ellipseLeft) || RectangleContainsPoint(rectangle, ellipseRight)
				|| RectangleContainsPoint(rectangle, ellipseTop) || RectangleContainsPoint(rectangle, ellipseBottom))
				return true;

			PointF[] rectPoints = new PointF[4];
			rectPoints[0].X = rectangle.Left;
			rectPoints[0].Y = rectangle.Top;
			rectPoints[1].X = rectangle.Right;
			rectPoints[1].Y = rectangle.Top;
			rectPoints[2].X = rectangle.Right;
			rectPoints[2].Y = rectangle.Bottom;
			rectPoints[3].X = rectangle.Left;
			rectPoints[3].Y = rectangle.Bottom;
			return PolygonIntersectsWithEllipse(ellipseCenterX, ellipseCenterY, ellipseWidth, ellipseHeight, ellipseAngleDeg, rectPoints);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static bool PolygonIntersectsWithEllipse(int ellipseCenterX, int ellipseCenterY, int ellipseWidth, int ellipseHeight, float ellipseAngleDeg, PointF[] polygon) {
			if (ellipseAngleDeg != 0) {
				// instead of rotating the ellipse, we rotate the polygon in the opposite direction and calculate 
				// intersection of the unrotated ellipse with the rotated polygon
				for (int i = polygon.Length - 1; i >= 0; --i)
					polygon[i] = RotatePoint(ellipseCenterX, ellipseCenterY, -ellipseAngleDeg, polygon[i]);
			}

			int maxIdx = polygon.Length - 1;
			for (int i = 0; i < maxIdx; ++i) {
				PointF a = polygon[i];
				PointF b = polygon[i + 1];
				if (EllipseIntersectsWithLine(ellipseCenterX, ellipseCenterY, ellipseWidth, ellipseHeight, a.X, a.Y, b.X, b.Y, true))
					return true;
			}
			if (EllipseIntersectsWithLine(ellipseCenterX, ellipseCenterY, ellipseWidth, ellipseHeight, polygon[0].X, polygon[0].Y, polygon[maxIdx].X, polygon[maxIdx].Y, true))
				return true;

			return false;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static bool PolygonIntersectsWithEllipse(int ellipseCenterX, int ellipseCenterY, int ellipseWidth, int ellipseHeight, float ellipseAngleDeg, Point[] polygon) {
			if (ellipseAngleDeg != 0) {
				// instead of rotating the ellipse, we rotate the polygon in the opposite direction and calculate 
				// intersection of the unrotated ellipse with the rotated polygon
				for (int i = polygon.Length - 1; i >= 0; --i)
					polygon[i] = RotatePoint(ellipseCenterX, ellipseCenterY, -ellipseAngleDeg, polygon[i]);
			}

			int maxIdx = polygon.Length - 1;
			for (int i = 0; i < maxIdx; ++i) {
				PointF a = polygon[i];
				PointF b = polygon[i + 1];
				if (EllipseIntersectsWithLine(ellipseCenterX, ellipseCenterY, ellipseWidth, ellipseHeight, a.X, a.Y, b.X, b.Y, true))
					return true;
			}
			if (EllipseIntersectsWithLine(ellipseCenterX, ellipseCenterY, ellipseWidth, ellipseHeight, polygon[0].X, polygon[0].Y, polygon[maxIdx].X, polygon[maxIdx].Y, true))
				return true;
			return false;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static bool RectangleIntersectsWithRectangle(Rectangle rect1, Rectangle rect2) {
			return ((((rect2.X <= rect1.Right) && (rect1.X <= rect2.Right)) && (rect2.Y <= rect1.Bottom)) && (rect1.Y <= rect2.Bottom));
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static bool RectangleIntersectsWithRectangle(Rectangle rect, int x, int y, int width, int height) {
			Rectangle rect2 = Rectangle.Empty;
			rect2.Offset(x, y);
			rect2.Width = width;
			rect2.Height = height;
			return RectangleIntersectsWithRectangle(rect, rect2);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static bool RectangleIntersectsWithRectangle(int r1X, int r1Y, int r1Width, int r1Height, int r2X, int r2Y, int r2Width, int r2Height) {
			Rectangle r1 = Rectangle.Empty, r2 = Rectangle.Empty;
			r1.Offset(r1X, r1Y);
			r1.Width = r1Width;
			r1.Height = r1Height;
			r2.Offset(r2X, r2Y);
			r2.Width = r2Width;
			r2.Height = r2Height;
			return RectangleIntersectsWithRectangle(r1, r2);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static bool RectangleIntersectsWithRectangle(RectangleF rect1, RectangleF rect2) {
			return ((((rect2.X <= rect1.Right) && (rect1.X <= rect2.Right)) && (rect2.Y <= rect1.Bottom)) && (rect1.Y <= rect2.Bottom));
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static bool RectangleIntersectsWithRectangle(RectangleF rect, float x, float y, float width, float height) {
			RectangleF rect2 = RectangleF.Empty;
			rect2.Offset(x, y);
			rect2.Width = width;
			rect2.Height = height;
			return RectangleIntersectsWithRectangle(rect, rect2);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static bool RectangleIntersectsWithRectangle(float r1X, float r1Y, float r1Width, float r1Height, float r2X, float r2Y, float r2Width, float r2Height) {
			RectangleF r1 = RectangleF.Empty, r2 = RectangleF.Empty;
			r1.Offset(r1X, r1Y);
			r1.Width = r1Width;
			r1.Height = r1Height;
			r2.Offset(r2X, r2Y);
			r2.Width = r2Width;
			r2.Height = r2Height;
			return RectangleIntersectsWithRectangle(r1, r2);
		}

		#endregion


		#region Intersection calculation functions

		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("CAUTION: Arithmetic overflows with large coordinates, use version with two-point form instead.")]
		public static bool IntersectLineWithLineSegment(int aLine, int bLine, int cLine, Point p1, Point p2, out int x, out int y) {
			x = InvalidCoordinateValue; y = InvalidCoordinateValue;
			int aSegment, bSegment; long cSegment;
			CalcLine(p1.X, p1.Y, p2.X, p2.Y, out aSegment, out bSegment, out cSegment);
			if (IntersectLines(aLine, bLine, cLine, aSegment, bSegment, cSegment, out x, out y)) {
				return (Math.Min(p1.X, p2.X) <= x && x <= Math.Max(p1.X, p2.X)
					&& Math.Min(p1.Y, p2.Y) <= y && y <= Math.Max(p1.Y, p2.Y));
			} else return false;
		}


		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("CAUTION: Arithmetic overflows with large coordinates, use version with two-point form instead.")]
		public static bool IntersectLineWithLineSegment(float aLine, float bLine, float cLine, PointF p1, PointF p2, out float x, out float y) {
			float aSegment, bSegment, cSegment;
			CalcLine(p1.X, p1.Y, p2.X, p2.Y, out aSegment, out bSegment, out cSegment);
			if (IntersectLines(aLine, bLine, cLine, aSegment, bSegment, cSegment, out x, out y)) {
				return (Math.Min(p1.X, p2.X) <= x && x <= Math.Max(p1.X, p2.X)
					&& Math.Min(p1.Y, p2.Y) <= y && y <= Math.Max(p1.Y, p2.Y));
			} else return false;
		}


		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("CAUTION: Arithmetic overflows with large coordinates, use version with two-point form instead.")]
		public static bool IntersectLineWithLineSegment(float aLine, float bLine, float cLine, PointF p1, PointF p2, out int x, out int y) {
			x = y = InvalidCoordinateValue;
			float aSegment, bSegment, cSegment;
			CalcLine(p1.X, p1.Y, p2.X, p2.Y, out aSegment, out bSegment, out cSegment);
			if (IntersectLines(aLine, bLine, cLine, aSegment, bSegment, cSegment, out float _x, out float _y)) {
				x = (int)Math.Round(_x);
				y = (int)Math.Round(_y);
				if (Math.Min(p1.X, p2.X) <= x && x <= Math.Max(p1.X, p2.X)
					&& Math.Min(p1.Y, p2.Y) <= y && y <= Math.Max(p1.Y, p2.Y))
					return true;
				else {
					x = y = InvalidCoordinateValue;
					return false;
				}
			} else return false;
		}


		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("CAUTION: Arithmetic overflows with large coordinates, use version with two-point form instead.")]
		public static Point IntersectLineWithLineSegment(int aLine, int bLine, int cLine, Point p1, Point p2) {
			Point result = InvalidPoint;
			int x, y;
			int aSegment, bSegment; long cSegment;
			CalcLine(p1.X, p1.Y, p2.X, p2.Y, out aSegment, out bSegment, out cSegment);
			if (IntersectLines(aLine, bLine, cLine, aSegment, bSegment, cSegment, out x, out y)) {
				if (Math.Min(p1.X, p2.X) <= x && x <= Math.Max(p1.X, p2.X)
					&& Math.Min(p1.Y, p2.Y) <= y && y <= Math.Max(p1.Y, p2.Y)) {
					result.X = x;
					result.Y = y;
				}
			}
			return result;
		}


		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("CAUTION: Arithmetic overflows with large coordinates, use version with two-point form instead.")]
		public static PointF IntersectLineWithLineSegment(float aLine, float bLine, float cLine, PointF p1, PointF p2) {
			PointF result = InvalidPointF;
			float x, y, aSegment, bSegment, cSegment;
			CalcLine(p1.X, p1.Y, p2.X, p2.Y, out aSegment, out bSegment, out cSegment);
			if (IntersectLines(aLine, bLine, cLine, aSegment, bSegment, cSegment, out x, out y)) {
				if (Math.Min(p1.X, p2.X) <= x && x <= Math.Max(p1.X, p2.X)
					&& Math.Min(p1.Y, p2.Y) <= y && y <= Math.Max(p1.Y, p2.Y)) {
					result.X = x;
					result.Y = y;
				}
			}
			return result;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static Point IntersectLineSegments(Point p1, Point p2, Point p3, Point p4) {
			return IntersectLineSegments(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static Point IntersectLineSegments(int a1X, int a1Y, int a2X, int a2Y, int b1X, int b1Y, int b2X, int b2Y) {
			IntersectLines(a1X, a1Y, a2X, a2Y, b1X, b1Y, b2X, b2Y, out long x, out long y);
			// Now check, whether the intersection is on the segments.
			if (x < int.MinValue || x > int.MaxValue || y < int.MinValue || y > int.MaxValue)
				return InvalidPoint;
			Point result = Point.Empty;
			result.Offset((int)x, (int)y);
			if (!IsPointInRect(result.X, result.Y, a1X, a1Y, a2X, a2Y) || !IsPointInRect(result.X, result.Y, b1X, b1Y, b2X, b2Y))
				return InvalidPoint;
			return result;
		}


		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete]
		public static Point IntersectLineSegments(long a1X, long a1Y, long a2X, long a2Y, long b1X, long b1Y, long b2X, long b2Y) 
		{
			Point result = InvalidPoint;
			long x21 = a2X - a1X;
			long y21 = a2Y - a1Y;
			long x13 = a1X - b1X;
			long y13 = a1Y - b1Y;
			long x43 = b2X - b1X;
			long y43 = b2Y - b1Y;
			double d = y43 * x21 - x43 * y21;
			if (d != 0) {
				double u2 = ((x21 * y13 - y21 * x13) / d);
				if (!double.IsNaN(u2) && 0 <= u2 && u2 <= 1) {
					double u1 = (x43 * y13 - y43 * x13) / d;
					if (!double.IsNaN(u1) && 0 <= u1 && u1 <= 1) {
						result.X = (int)Math.Round(a1X + x21 * u1);
						result.Y = (int)Math.Round(a1Y + y21 * u1);
					}
				}
			}
			return result;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static PointF IntersectLineSegments(PointF a1, PointF a2, PointF b1, PointF b2) {
			return IntersectLineSegments(a1.X, a1.Y, a2.X, a2.Y, b1.X, b1.Y, b2.X, b2.Y);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static PointF IntersectLineSegments(float a1X, float a1Y, float a2X, float a2Y, float b1X, float b1Y, float b2X, float b2Y) {
			float x21 = a2X - a1X;
			float y21 = a2Y - a1Y;
			float x13 = a1X - b1X;
			float y13 = a1Y - b1Y;
			float x43 = b2X - b1X;
			float y43 = b2Y - b1Y;
			double d = y43 * x21 - x43 * y21;
			PointF result = InvalidPointF;
			if (d != 0) {
				double u2 = (x21 * y13 - y21 * x13) / d;
				if (!double.IsNaN(u2) && 0 <= u2 && u2 <= 1) {
					double u1 = (x43 * y13 - y43 * x13) / d;
					if (!double.IsNaN(u1) && 0 <= u1 && u1 <= 1) {
						result.X = (float)(a1X + x21 * u1);
						result.Y = (float)(a1Y + y21 * u1);
					}
				}
			}
			return result;
		}


		/// <summary>
		/// Intersects the line a1-a2 with the line segment b1-b2.
		/// Returns Geometry.InvalidPoint if there is no intersection.
		/// </summary>
		public static Point IntersectLineWithLineSegment(int a1X, int a1Y, int a2X, int a2Y, int b1X, int b1Y, int b2X, int b2Y)
		{
			IntersectLines(a1X, a1Y, a2X, a2Y, b1X, b1Y, b2X, b2Y, out long x, out long y);
			if (x < int.MinValue || x > int.MaxValue || y < int.MinValue || y > int.MaxValue)
				return InvalidPoint;
			Point result = Point.Empty;
			result.Offset((int)x, (int)y);
			if (!IsPointInRect(result.X, result.Y, b1X, b1Y, b2X, b2Y))
				return InvalidPoint;
			return result;
		}


		/// <summary>
		/// Calculates the intersection point of line a1-a2 and line segment b1-b2.
		/// Returns Geometry.InvalidPoint if there is no intersection.
		/// </summary>
		public static PointF IntersectLineWithLineSegment(float a1X, float a1Y, float a2X, float a2Y, float b1X, float b1Y, float b2X, float b2Y) {
			IntersectLines(a1X, a1Y, a2X, a2Y, b1X, b1Y, b2X, b2Y, out double x, out double y);
			if (x < int.MinValue || x > int.MaxValue || y < int.MinValue || y > int.MaxValue)
				return InvalidPointF;
			PointF result = PointF.Empty;
			result.X = (float)x;
			result.Y = (float)y;
			if (!IsPointInRect(result.X, result.Y, b1X, b1Y, b2X, b2Y))
				return InvalidPointF;
			return result;

			//float a1, b1, c1, a2, b2, c2;
			//CalcLine(a1X, a1Y, a2X, a2Y, out a1, out b1, out c1);
			//CalcLine(b1X, b1Y, b2X, b2Y, out a2, out b2, out c2);
			//float x, y;
			//if (IntersectLines(a1, b1, c1, a2, b2, c2, out x, out y)) {
			//	if (Math.Min(b1X, b2X) <= x && x <= Math.Max(b1X, b2X)
			//		&& Math.Min(b1Y, b2Y) <= y && y <= Math.Max(b1Y, b2Y)) {
			//		result.X = x;
			//		result.Y = y;
			//	}
			//}
			//return result;
		}


		/// <summary>
		/// Calculates the intersection point of two lines given in the form ax + bx + c = 0
		/// </summary>
		// Algorithm Explaination:
		// a1 * x + b1 * y = -c1   multiplied by b2
		// and
		// a2 * x + b2 * y = -c2   multiplied by b1
		// results in 
		// a1 b2 x + b1 b2 y = -(b2 c1)          [1]
		// a2 b1 x + b1 b2 y = -(b1 c2)          [2]
		// [1] - [2] results in
		// a1 b2 x - a2 b1 x = - b2 c1 - b1 c2   [3]
		// [3] divided through a1 b2 - a2 b1 results in the equation for x (analog for y)
		[Obsolete("CAUTION: Arithmetic overflows with large coordinates, use version with two-point form instead.")]
		public static bool IntersectLines(float a1, float b1, float c1, float a2, float b2, float c2, out int x, out int y) {
			double det = (double)a1 * b2 - (double)a2 * b1;
			if (Equals(det, 0)) {
				x = InvalidPoint.X; y = InvalidPoint.Y;
				return false;
			} else {
				x = (int)Math.Round(((double)b2 * -c1) / det - ((double)b1 * -c2) / det);
				y = (int)Math.Round(((double)a1 * -c2) / det - ((double)a2 * -c1) / det);
				return true;
			}
		}


		internal static bool IntersectLines(float a1X, float a1Y, float a2X, float a2Y, float b1X, float b1Y, float b2X, float b2Y, out double x, out double y) {
			if (a1X == a2X && b1X == b2X) {
				// Both line segments are vertical, no intersection point
				x = InvalidCoordinateValueL;
				y = InvalidCoordinateValueL;
				return false;
			} else if (a1X == a2X) {
				// First line segment is vertical
				x = a1X;
				y = x * (double)(b2Y - b1Y) / (b2X - b1X) + b1Y - (b2Y - b1Y) * (double)b1X / (b2X - b1X);
				return true;
			} else if (b1X == b2X) {
				// Second line segment is vertical
				x = b1X;
				y = x * (double)(a2Y - a1Y) / (a2X - a1X) + a1Y - (a2Y - a1Y) * (double)a1X / (a2X - a1X);
				return true;
			} else if ((double)(a2Y - a1Y) * (b2X - b1X) - (double)(b2Y - b1Y) * (a2X - a1X) == 0) {
				// No intersection point
				x = InvalidCoordinateValueL;
				y = InvalidCoordinateValueL;
				return false;
			} else {
				// We equal the y value of mx + t and solve for x.
				x = (double)(b1Y - a1Y) * (a2X - a1X) * (b2X - b1X) - (double)b1X * (b2Y - b1Y) * (a2X - a1X)
					+ (double)a1X * (a2Y - a1Y) * (b2X - b1X);
				x /= (double)(a2Y - a1Y) * (b2X - b1X) - (double)(b2Y - b1Y) * (a2X - a1X);
				// We cannot calculate y as the value of mx + t, because if the line is almost vertical,
				// there might be only two different values for x. Therefore we equal the x value of mx + t
				// for both lines and solve for y.
				y = -(double)b1Y * (b2X - b1X) * (a2Y - a1Y) + (double)(b2Y - b1Y) * (a2Y - a1Y) * b1X + (double)a1Y * (a2X - a1X) * (b2Y - b1Y) - (double)(a2Y - a1Y) * (b2Y - b1Y) * a1X;
				y /= (double)(b2Y - b1Y) * (a2X - a1X) - (double)(a2Y - a1Y) * (b2X - b1X);
				return true;
			}
		}


		/// <summary>
		/// Intersects two lines and returns the intersection point.
		/// Must not be published, because the intersection point can lay outside the valid coordinates.
		/// </summary>
		internal static bool IntersectLines(int a1X, int a1Y, int a2X, int a2Y, int b1X, int b1Y, int b2X, int b2Y, out long x, out long y)
		{
			if (a1X == a2X && b1X == b2X) {
				// Both line segments are vertical, no intersection point
				x = InvalidCoordinateValueL;
				y = InvalidCoordinateValueL;
				return false;
			} else if (a1X == a2X) {
				// First line segment is vertical
				x = a1X;
				y = x * (long)(b2Y - b1Y) / (b2X - b1X) + b1Y - (b2Y - b1Y) * (long)b1X / (b2X - b1X);
				return true;
			} else if (b1X == b2X) {
				// Second line segment is vertical
				x = b1X;
				y = x * (long)(a2Y - a1Y) / (a2X - a1X) + a1Y - (a2Y - a1Y) * (long)a1X / (a2X - a1X);
				return true;
			} else if ((long)(a2Y - a1Y) * (b2X - b1X) - (long)(b2Y - b1Y) * (a2X - a1X) == 0) {
				// No intersection point
				x = InvalidCoordinateValueL;
				y = InvalidCoordinateValueL;
				return false;
			} else {
				// We equal the y value of mx + t and solve for x.
				x = (long)(b1Y - a1Y) * (a2X - a1X) * (b2X - b1X) - (long)b1X * (b2Y - b1Y) * (a2X - a1X)
					+ (long)a1X * (a2Y - a1Y) * (b2X - b1X);
				x /= (long)(a2Y - a1Y) * (b2X - b1X) - (long)(b2Y - b1Y) * (a2X - a1X);
				// We cannot calculate y as the value of mx + t, because if the line is almost vertical,
				// there might be only two different values for x. Therefore we equal the x value of mx + t
				// for both lines and solve for y.
				y = -(long)b1Y * (b2X - b1X) * (a2Y - a1Y) + (long)(b2Y - b1Y) * (a2Y - a1Y) * b1X + (long)a1Y * (a2X - a1X) * (b2Y - b1Y) - (long)(a2Y - a1Y) * (b2Y - b1Y) * a1X;
				y /= (long)(b2Y - b1Y) * (a2X - a1X) - (long)(a2Y - a1Y) * (b2X - b1X);
				return true;
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("CAUTION: Arithmetic overflows with large coordinates, use IntersectLines with two-point form instead.")]
		public static bool IntersectLines(int a1, int b1, int c1, int a2, int b2, int c2, out int x, out int y) {
			x = InvalidPoint.X; y = InvalidPoint.Y;
			float det = (float)a1 * b2 - (float)a2 * b1;
			if (Equals(det, 0)) return false;
			else {
				x = (int)Math.Round(((float)b2 * -c1 - (float)b1 * -c2) / det);
				y = (int)Math.Round(((float)a1 * -c2 - (float)a2 * -c1) / det);
				return true;
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("CAUTION: Arithmetic overflows with large coordinates, use version with two-point form instead.")]
		public static bool IntersectLines(int a1, int b1, long c1, int a2, int b2, long c2, out int x, out int y) {
			// Use double for more precise atithmetics because performance is equal to float! (At least on a 'Core i9 9900K' CPU)
			// Performance inprovement of using double instead of long:
			// 1665 ms (all double) versus 3490 ms ('det' is double) versus 5251 ms (all long)
			x = InvalidPoint.X; y = InvalidPoint.Y;
			long _c1 = c1, _c2 = c2;
			double det = (long)a1 * b2 - (long)a2 * b1;
			if (Equals(det, 0)) return false;
			else {
				// Pure integer division delivers is not always correct
				x = (int)Math.Round((b2 * -_c1 - b1 * -_c2) / det);
				y = (int)Math.Round((a1 * -_c2 - a2 * -_c1) / det);
				return true;
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("CAUTION: Arithmetic overflows with large coordinates, use version with two-point form instead.")]
		public static bool IntersectLines(float a1, float b1, float c1, float a2, float b2, float c2, out float x, out float y) {
			// This is the fastest version (690 ms vs 820 ms for int version) because the datatype does not change.
			x = InvalidPointF.X; y = InvalidPointF.Y;
			float det = a1 * b2 - a2 * b1;
			if (Equals(det, 0)) return false;
			else {
				x = (b2 * -c1 - b1 * -c2) / det;
				y = (a1 * -c2 - a2 * -c1) / det;
				return true;
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("CAUTION: Arithmetic overflows with large coordinates, use version with two-point form instead.")]
		public static Point IntersectLines(int a1, int b1, int c1, int a2, int b2, int c2) {
			Point result = InvalidPoint;
			int x, y;
			if (IntersectLines(a1, b1, c1, a2, b2, c2, out x, out y)) {
				result.X = x;
				result.Y = y;
			}
			return result;
		}


		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("CAUTION: Arithmetic overflows with large coordinates, use version with two-point form instead.")]
		public static Point IntersectLines(int a1, int b1, int c1, Point p1, Point p2) {
			Point result = InvalidPoint;
			int a2, b2; long c2;
			CalcLine(p1.X, p1.Y, p2.X, p2.Y, out a2, out b2, out c2);
			int x, y;
			if (IntersectLines(a1, b1, c1, a2, b2, c2, out x, out y)) {
				result.X = x;
				result.Y = y;
			}
			return result;
		}


		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("CAUTION: Arithmetic overflows with large coordinates, use version with two-point form instead.")]
		public static PointF IntersectLines(float a1, float b1, float c1, PointF a, PointF b) {
			PointF result = InvalidPointF;
			float a2, b2, c2;
			CalcLine(a.X, a.Y, b.X, b.Y, out a2, out b2, out c2);
			float x, y;
			if (IntersectLines(a1, b1, c1, a2, b2, c2, out x, out y)) {
				result.X = x;
				result.Y = y;
			}
			return result;
		}


		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("CAUTION: Arithmetic overflows with large coordinates, use version with two-point form instead.")]
		public static PointF IntersectLines(float a1, float b1, float c1, float a2, float b2, float c2) {
			PointF result = InvalidPointF;
			float x, y;
			if (IntersectLines(a1, b1, c1, a2, b2, c2, out x, out y)) {
				result.X = x;
				result.Y = y;
			}
			return result;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static PointF IntersectLines(float a1X, float a1Y, float a2X, float a2Y, float b1X, float b1Y, float b2X, float b2Y) {
			IntersectLines(a1X, a1Y, a2X, a2Y, b1X, b1Y, b2X, b2Y, out double x, out double y);
			// Now check, whether the intersection is on the segments.
			if (x <= int.MinValue || x > int.MaxValue || y <= int.MinValue || y > int.MaxValue)
				return InvalidPointF;
			PointF result = PointF.Empty;
			result.X = (float)x;
			result.Y = (float)y;
			return result;

			//PointF result = InvalidPointF;
			//float a1, b1, c1, a2, b2, c2;
			//CalcLine(line1StartX, line1StartY, line1EndX, line1EndY, out a1, out b1, out c1);
			//CalcLine(line2StartX, line2StartY, line2EndX, line2EndY, out a2, out b2, out c2);
			//float x, y;
			//if (IntersectLines(a1, b1, c1, a2, b2, c2, out x, out y)) {
			//	result.X = x;
			//	result.Y = y;
			//}
			//return result;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static Point IntersectLines(int a1X, int a1Y, int a2X, int a2Y, int b1X, int b1Y, int b2X, int b2Y) {
			IntersectLines(a1X, a1Y, a2X, a2Y, b1X, b1Y, b2X, b2Y, out long x, out long y);
			// Now check, whether the intersection is on the segments.
			if (x <= int.MinValue || x > int.MaxValue || y <= int.MinValue || y > int.MaxValue)
				return InvalidPoint;
			Point result = Point.Empty;
			result.Offset((int)x, (int)y);
			return result;

			//// Not really slower than float version (100.000.000 calls):
			//// int/long: 2409 ms, long: 2399 ms
			//Point result = InvalidPoint;
			////CalcLine(p1X, p1Y, p2X, p2Y, out int a1, out int b1, out long c1);
			//CalcLine(p3X, p3Y, p4X, p4Y, out int a2, out int b2, out long c2);
			//int x, y;
			//if (IntersectLines(a1, b1, c1, a2, b2, c2, out x, out y)) {
			//	result.X = x;
			//	result.Y = y;
			//}
			//return result;
		}


		/// <summary>
		/// Calculates the intersection point of the line with the rectangle that is nearer to point 1 of the line.
		/// </summary>
		public static Point IntersectLineWithRectangle(int a1X, int a1Y, int a2X, int a2Y, int r1X, int r1Y, int r2X, int r2Y) {
			Point result = InvalidPoint;
			if (a1X == a2X && a1Y == a2Y)
				return result;

			if (a1Y <= r1Y) {
				if (a2Y > r1Y) {
					if (a1X <= r1X) {
						// links oben
						result = IntersectLineWithLineSegment(a1X, a1Y, a2X, a2Y, r1X, r1Y, r1X, r2Y);
						if (!IsValid(result))
							result = IntersectLineWithLineSegment(a1X, a1Y, a2X, a2Y, r1X, r1Y, r2X, r1Y);
					} else if (a1X >= r2X) {
						// rechts oben
						result = IntersectLineWithLineSegment(a1X, a1Y, a2X, a2Y, r2X, r1Y, r2X, r2Y);
						if (!IsValid(result))
							result = IntersectLineWithLineSegment(a1X, a1Y, a2X, a2Y, r1X, r1Y, r2X, r1Y);
					} else { // Mitte oben
						result = IntersectLineWithLineSegment(a1X, a1Y, a2X, a2Y, r1X, r1Y, r2X, r1Y);
					}
				}
			} else if (a1Y >= r2Y) {
				if (a2Y < r2Y) {
					if (a1X <= r1X) { // links unten
						result = IntersectLineWithLineSegment(a1X, a1Y, a2X, a2Y, r1X, r1Y, r1X, r2Y);
						if (!IsValid(result))
							result = IntersectLineWithLineSegment(a1X, a1Y, a2X, a2Y, r1X, r2Y, r2X, r2Y);
					} else if (a1X >= r2X) { // rechts unten
						result = IntersectLineWithLineSegment(a1X, a1Y, a2X, a2Y, r1X, r2Y, r2X, r2Y);
						if (!IsValid(result))
							result = IntersectLineWithLineSegment(a1X, a1Y, a2X, a2Y, r2X, r1Y, r2X, r2Y);
					} else { // Mitte unten
						result = IntersectLineWithLineSegment(a1X, a1Y, a2X, a2Y, r1X, r2Y, r2X, r2Y);
					}
				}
			} else if (a1X <= r1X) { // links mitte
				result = IntersectLineWithLineSegment(a1X, a1Y, a2X, a2Y, r1X, r1Y, r1X, r2Y);
			} else if (a1X >= r2X) { // rechts mitte
				result = IntersectLineWithLineSegment(a1X, a1Y, a2X, a2Y, r2X, r1Y, r2X, r2Y);
			} else if (a2Y <= r1Y || a2Y >= r2Y || a2X <= r1X || a2X >= r2X) {
				result = IntersectLineWithRectangle(a2X, a2Y, a1X, a1Y, r1X, r1Y, r2X, r2Y);
			} else { // Beide sind innen, wir verlängern die Gerade über X2, Y2 hinaus
				int newLineX1, newLineY1;
				if (Math.Abs(a2X - a1X) >= Math.Abs(a2Y - a1Y)) { // X in den Nenner, Y durch Rechteck bestimmen
					if (a2X >= a1X) newLineX1 = a1X + r2X - r1X;
					else newLineX1 = a1X + r1X - r2X;
					newLineY1 = a2Y + (newLineX1 - a2X) * (a1Y - a2Y) / (a1X - a2X);
				} else { // Y in den Nenner, X durch Rechteckbreite bestimmen
					if (a2Y >= a1Y) newLineY1 = a1Y + r2Y - r1Y;
					else newLineY1 = a1Y + r1Y - r2Y;
					newLineX1 = a2X + (newLineY1 - a2Y) * (a1X - a2X) / (a1Y - a2Y);
				}
				result = IntersectLineWithRectangle(newLineX1, newLineY1, a2X, a2Y, r1X, r1Y, r2X, r2Y);
			}
			return result;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static Point IntersectCircleWithLine(Point center, int radius, Point a1, Point a2, bool isSegment) {
			return IntersectCircleWithLine(center.X, center.Y, radius, a1.X, a1.Y, a2.X, a2.Y, isSegment);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static PointF IntersectCircleWithLine(PointF center, float radius, PointF a1, PointF a2, bool isSegment) {
			return IntersectCircleWithLine(center.X, center.Y, radius, a1.X, a1.Y, a2.X, a2.Y, isSegment);
		}


		/// <summary>
		/// Calculates and returns the intersection point of the given circle with the given line (segment) that is nearest to point x1/y1.
		/// </summary>
		/// <param name="centerX">X Coordinate of the circle's center</param>
		/// <param name="centerY">Y Coordinate of the circle's center</param>
		/// <param name="radius">Radius of the circle</param>
		/// <param name="a1X">X coordinate of the first point of the line (segment)</param>
		/// <param name="a1Y">Y coordinate of the first point of the line (segment)</param>
		/// <param name="a2X">X coordinate of the second point of the line (segment)</param>
		/// <param name="a2Y">Y coordinate of the second point of the line (segment)</param>
		/// <param name="isSegment">Specifies if the given line should be treated as a (endable) line segment instead of a (endless) line</param>
		public static Point IntersectCircleWithLine(int centerX, int centerY, int radius, int a1X, int a1Y, int a2X, int a2Y, bool isSegment) {
			Point result = InvalidPoint;
			if (a1X == a2X && a1Y == a2Y)
				return result;
			float rr = (float)radius * radius;
			float x21 = a2X - a1X;
			float y21 = a2Y - a1Y;
			float x10 = a1X - centerX;
			float y10 = a1Y - centerY;
			float a = (x21 * x21 + y21 * y21) / rr;
			float b = (x21 * x10 + y21 * y10) / rr;
			float c = (x10 * x10 + y10 * y10) / rr;
			float d = b * b - a * (c - 1);
			if (d >= 0) {
				float e = (float)Math.Sqrt(d);
				float u1 = (-b - e) / a;
				float u2 = (-b + e) / a;
				Point pt1 = InvalidPoint;
				Point pt2 = InvalidPoint;
				if (!isSegment || (!float.IsNaN(u1) && 0 <= u1 && u1 <= 1)) {
					Point p = Point.Empty;
					p.X = (int)Math.Round(a1X + x21 * u1);
					p.Y = (int)Math.Round(a1Y + y21 * u1);
					pt1 = p;
				}
				if (!isSegment || (!float.IsNaN(u2) && 0 <= u2 && u2 <= 1)) {
					Point p = Point.Empty;
					p.X = (int)Math.Round(a1X + x21 * u2);
					p.Y = (int)Math.Round(a1Y + y21 * u2);
					pt2 = p;
				}
				if (IsValid(pt1) && IsValid(pt2))
					result = GetNearestPoint(a1X, a1Y, pt1.X, pt1.Y, pt2.X, pt2.Y);
				else if (IsValid(pt1)) result = pt1;
				else if (IsValid(pt2)) result = pt2;
			}
			return result;
		}


		/// <summary>
		/// Calculates and returns the intersection point of the given circle with the given line (segment) that is nearest to point x1/y1.
		/// </summary>
		/// <param name="centerX">X Coordinate of the circle's center</param>
		/// <param name="centerY">Y Coordinate of the circle's center</param>
		/// <param name="radius">Radius of the circle</param>
		/// <param name="a1X">X coordinate of the first point of the line (segment)</param>
		/// <param name="a1Y">Y coordinate of the first point of the line (segment)</param>
		/// <param name="a2X">X coordinate of the second point of the line (segment)</param>
		/// <param name="a2Y">Y coordinate of the second point of the line (segment)</param>
		/// <param name="isSegment">Specifies if the given line should be treated as a (endable) line segment instead of a (endless) line</param>
		public static PointF IntersectCircleWithLine(float centerX, float centerY, float radius, float a1X, float a1Y, float a2X, float a2Y, bool isSegment) {
			PointF p = Point.Empty;
			p.X = a1X; p.Y = a1Y;
			return GetNearestPoint(p, GetAllCircleLineIntersections(centerX, centerY, radius, a1X, a1Y, a2X, a2Y, isSegment));
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static IEnumerable<PointF> GetAllCircleLineIntersections(PointF center, float radius, PointF a1, PointF a2, bool isSegment) {
			return GetAllCircleLineIntersections(center.X, center.Y, radius, a1.X, a1.Y, a2.X, a2.Y, isSegment);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static IEnumerable<PointF> GetAllCircleLineIntersections(float centerX, float centerY, float radius, float a1X, float a1Y, float a2X, float a2Y, bool isSegment) {
			PointF result = PointF.Empty;
			double rr = radius * radius;
			float x21 = a2X - a1X;
			float y21 = a2Y - a1Y;
			float x10 = a1X - centerX;
			float y10 = a1Y - centerY;
			double a = (x21 * x21 + y21 * y21) / rr;
			double b = (x21 * x10 + y21 * y10) / rr;
			double c = (x10 * x10 + y10 * y10) / rr;
			double d = b * b - a * (c - 1);
			if (d >= 0) {
				double e = Math.Sqrt(d);
				double u1 = (-b - e) / a;
				double u2 = (-b + e) / a;
				if (!isSegment || (0 <= u1 && u1 <= 1)) {
					result.X = (float)(a1X + x21 * u1);
					result.Y = (float)(a1Y + y21 * u1);
					yield return result;
				}
				if (!isSegment || (0 <= u2 && u2 <= 1)) {
					result.X = (float)(a1X + x21 * u2);
					result.Y = (float)(a1Y + y21 * u2);
					yield return result;
				}
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static IEnumerable<Point> GetAllCircleLineIntersections(Point center, float radius, Point a1, Point a2, bool isSegment) {
			return GetAllCircleLineIntersections(center.X, center.Y, radius, a1.X, a1.Y, a2.X, a2.Y, isSegment);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static IEnumerable<Point> GetAllCircleLineIntersections(int centerX, int centerY, float radius, int a1X, int a1Y, int a2X, int a2Y, bool isSegment) {
			foreach (PointF p in GetAllCircleLineIntersections((float)centerX, (float)centerY, (float)radius, (float)a1X, (float)a1Y, (float)a2X, (float)a2Y, isSegment))
				yield return Point.Round(p);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static IEnumerable<PointF> IntersectCircles(int center1X, int center1Y, int radius1, int center2X, int center2Y, int radius2) {
			PointF result = Point.Empty;
			double a1, a2, b1, b2, c1, c2, d, e, f;

			a1 = 2 * center1X;
			b1 = 2 * center1Y;
			c1 = (radius1 * radius1) - (center1X * center1X) - (center1Y * center1Y);

			a2 = 2 * center2X;
			b2 = 2 * center2Y;
			c2 = (radius2 * radius2) - (center2X * center2X) - (center2Y * center2Y);

			if ((a2 - a1) > (b2 - b1)) {
				d = (c1 - c2) / (a2 - a1);
				e = (b1 - b2) / (a2 - a1);
				f = ((2 * d * e) - b1 - (a1 * e)) / (2 * (e * e) + 2);

				result.Y = (float)Math.Round(Math.Sqrt((f * f) - (((d * d) - (a1 * d) - c1) / ((e * e) + 1))) - f, 6);
				result.X = (float)Math.Round(d + e * result.Y, 6);
				yield return result;

				result.Y = (float)Math.Round(-Math.Sqrt((f * f) - (((d * d) - (a1 * d) - c1) / ((e * e) + 1))) - f, 6);
				result.X = (float)Math.Round(d + e * result.Y);
				yield return result;
			} else {
				d = (c1 - c2) / (b2 - b1);
				e = (a1 - a2) / (b2 - b1);
				f = ((2 * d * e) - a1 - (b1 * e)) / (2 * (e * e) + 2);

				result.X = (float)Math.Round(Math.Sqrt((f * f) - (((d * d) - (b1 * d) - c1) / ((e * e) + 1))) - f, 6);
				result.Y = (float)Math.Round(d + e * result.X, 6);
				yield return result;

				result.X = (float)Math.Round(-Math.Sqrt((f * f) - (((d * d) - (b1 * d) - c1) / ((e * e) + 1))) - f, 6);
				result.Y = (float)Math.Round(d + e * result.X, 6);
				yield return result;
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static IEnumerable<PointF> IntersectCircles(float center1X, float center1Y, float radius1, float center2X, float center2Y, float radius2) {
			PointF result = Point.Empty;
			double a1, a2, b1, b2, c1, c2, d, e, f;

			a1 = 2 * center1X;
			b1 = 2 * center1Y;
			c1 = (radius1 * radius1) - (center1X * center1X) - (center1Y * center1Y);

			a2 = 2 * center2X;
			b2 = 2 * center2Y;
			c2 = (radius2 * radius2) - (center2X * center2X) - (center2Y * center2Y);

			if ((a2 - a1) > (b2 - b1) || (b2 - b1 == 0)) {
				d = (c1 - c2) / (a2 - a1);
				e = (b1 - b2) / (a2 - a1);
				f = ((2 * d * e) - b1 - (a1 * e)) / (2 * (e * e) + 2);

				result.Y = (float)(Math.Sqrt((f * f) - (((d * d) - (a1 * d) - c1) / ((e * e) + 1))) - f);
				result.X = (float)(d + e * result.Y);
				yield return result;

				result.Y = (float)(-Math.Sqrt((f * f) - (((d * d) - (a1 * d) - c1) / ((e * e) + 1))) - f);
				result.X = (float)(d + e * result.Y);
				yield return result;
			} else {
				d = (c1 - c2) / (b2 - b1);
				e = (a1 - a2) / (b2 - b1);
				f = ((2 * d * e) - a1 - (b1 * e)) / (2 * (e * e) + 2);

				result.X = (float)(Math.Sqrt((f * f) - (((d * d) - (b1 * d) - c1) / ((e * e) + 1))) - f);
				result.Y = (float)(d + e * result.X);
				yield return result;

				result.X = (float)(-Math.Sqrt((f * f) - (((d * d) - (b1 * d) - c1) / ((e * e) + 1))) - f);
				result.Y = (float)(d + e * result.X);
				yield return result;
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static IEnumerable<Point> IntersectCircleArc(int circleCenterX, int circleCenterY, int circleRadius, int arcStartPtX, int arcStartPtY, int arcRadiusPtX, int arcRadiusPtY, int arcEndPtX, int arcEndPtY) {
			float arcRadius;
			PointF arcCenter;
			CalcCircumCircle(arcStartPtX, arcStartPtY, arcRadiusPtX, arcRadiusPtY, arcEndPtX, arcEndPtY, out arcCenter, out arcRadius);
			return IntersectCircleArc(circleCenterX, circleCenterY, circleRadius, arcStartPtX, arcStartPtY, arcRadiusPtX, arcRadiusPtY, arcEndPtX, arcEndPtY, arcCenter.X, arcCenter.Y, arcRadius);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static IEnumerable<Point> IntersectCircleArc(int circleCenterX, int circleCenterY, int circleRadius, int arcStartPtX, int arcStartPtY, int arcRadiusPtX, int arcRadiusPtY, int arcEndPtX, int arcEndPtY, float arcCenterX, float arcCenterY, float arcRadius) {
			foreach (PointF p in IntersectCircles(circleCenterX, circleCenterY, circleRadius, arcCenterX, arcCenterY, arcRadius)) {
				if (ArcContainsPoint(arcStartPtX, arcStartPtY, arcRadiusPtX, arcRadiusPtY, arcEndPtX, arcEndPtY, arcCenterX, arcCenterY, arcRadius, p.X, p.Y, 0.1f))
					yield return Point.Round(p);
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static IEnumerable<PointF> IntersectCircleArc(float circleCenterX, float circleCenterY, float circleRadius, float arcStartPtX, float arcStartPtY, float arcRadiusPtX, float arcRadiusPtY, float arcEndPtX, float arcEndPtY) {
			float arcRadius;
			PointF arcCenter;
			CalcCircumCircle(arcStartPtX, arcStartPtY, arcRadiusPtX, arcRadiusPtY, arcEndPtX, arcEndPtY, out arcCenter, out arcRadius);
			return IntersectCircleArc(circleCenterX, circleCenterY, circleRadius, arcStartPtX, arcStartPtY, arcRadiusPtX, arcRadiusPtY, arcEndPtX, arcEndPtY, arcCenter.X, arcCenter.Y, arcRadius);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static IEnumerable<PointF> IntersectCircleArc(float circleCenterX, float circleCenterY, float circleRadius, float arcStartPtX, float arcStartPtY, float arcRadiusPtX, float arcRadiusPtY, float arcEndPtX, float arcEndPtY, float arcCenterX, float arcCenterY, float arcRadius) {
			foreach (PointF p in IntersectCircles(circleCenterX, circleCenterY, circleRadius, arcCenterX, arcCenterY, arcRadius)) {
				if (ArcContainsPoint(arcStartPtX, arcStartPtY, arcRadiusPtX, arcRadiusPtY, arcEndPtX, arcEndPtY, arcCenterX, arcCenterY, arcRadius, p.X, p.Y, 0.1f))
					yield return p;
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static IEnumerable<Point> IntersectArcLine(Point startPt, Point radiusPt, Point endPt, Point a1, Point a2, bool isSegment) {
			return IntersectArcLine(startPt.X, startPt.Y, radiusPt.X, radiusPt.Y, endPt.X, endPt.Y, a1.X, a1.Y, a2.X, a2.Y, isSegment);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static IEnumerable<Point> IntersectArcLine(int startPtX, int startPtY, int radiusPtX, int radiusPtY, int endPtX, int endPtY, int a1X, int a1Y, int a2X, int a2Y, bool isSegment) {
			// calculate center point and radius
			float radius;
			PointF center;
			CalcCircumCircle((float)startPtX, (float)startPtY, (float)radiusPtX, (float)radiusPtY, (float)endPtX, (float)endPtY, out center, out radius);
			foreach (PointF p in GetAllCircleLineIntersections(center.X, center.Y, radius, a1X, a1Y, a2X, a2Y, isSegment))
				if (ArcContainsPoint(startPtX, startPtY, radiusPtX, radiusPtY, endPtX, endPtY, center.X, center.Y, radius, p.X, p.Y, 0.1f))
					yield return Point.Round(p);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static IEnumerable<PointF> IntersectArcLine(PointF startPt, PointF radiusPt, PointF endPt, PointF a1, PointF a2, bool isSegment) {
			return IntersectArcLine(startPt.X, startPt.Y, radiusPt.X, radiusPt.Y, endPt.X, endPt.Y, a1.X, a1.Y, a2.X, a2.Y, isSegment);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static IEnumerable<PointF> IntersectArcLine(float startPtX, float startPtY, float radiusPtX, float radiusPtY, float endPtX, float endPtY, float a1X, float a1Y, float a2X, float a2Y, bool isSegment) {
			// calculate center point and radius
			float radius;
			PointF center;
			CalcCircumCircle(startPtX, startPtY, radiusPtX, radiusPtY, endPtX, endPtY, out center, out radius);
			foreach (PointF p in GetAllCircleLineIntersections(center.X, center.Y, radius, a1X, a1Y, a2X, a2Y, isSegment))
				if (ArcContainsPoint(startPtX, startPtY, radiusPtX, radiusPtY, endPtX, endPtY, center.X, center.Y, radius, p.X, p.Y, 0.1f))
					yield return p;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static IEnumerable<Point> IntersectPolygonLine(Point[] points, Point a1, Point a2, bool isSegment) {
			return IntersectPolygonLine(points, a1.X, a1.Y, a2.X, a2.Y, isSegment);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static IEnumerable<Point> IntersectPolygonLine(Point[] points, int a1X, int a1Y, int a2X, int a2Y, bool isSegment) {
			if (points == null) throw new ArgumentNullException(nameof(points));

			int maxIdx = points.Length - 1;
			for (int idxPt1 = 0; idxPt1 < maxIdx; ++idxPt1) {
				int idxPt2 = idxPt1 + 1;
				// Instead of checking if there is an intersection and then calculating the intersection point again, 
				// we calculate the intersection point in the first place and then check if it's valid
				Point r = isSegment ? IntersectLineSegments(a1X, a1Y, a2X, a2Y, points[idxPt1].X, points[idxPt1].Y, points[idxPt2].X, points[idxPt2].Y)
					: IntersectLineWithLineSegment(a1X, a1Y, a2X, a2Y, points[idxPt1].X, points[idxPt1].Y, points[idxPt2].X, points[idxPt2].Y);
				if (IsValid(r))
					yield return r;
			}
			if (points[0] != points[maxIdx]) {
				int idxPt1 = maxIdx, idxPt2 = 0;
				Point r = isSegment ? IntersectLineSegments(a1X, a1Y, a2X, a2Y, points[idxPt1].X, points[idxPt1].Y, points[idxPt2].X, points[idxPt2].Y)
					: IntersectLineWithLineSegment(a1X, a1Y, a2X, a2Y, points[idxPt1].X, points[idxPt1].Y, points[idxPt2].X, points[idxPt2].Y);
				if (IsValid(r))
					yield return r;
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static IEnumerable<Point> IntersectPolygonLine(PointF[] points, PointF a1, PointF a2, bool isSegment) {
			return IntersectPolygonLine(points, a1.X, a1.Y, a2.X, a2.Y, isSegment);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static IEnumerable<Point> IntersectPolygonLine(PointF[] points, float a1X, float a1Y, float a2X, float a2Y, bool isSegment) {
			if (points == null) throw new ArgumentNullException(nameof(points));
			Point result = Point.Empty;

			PointF polyPt1, polyPt2;
			int maxIdx = points.Length - 1;
			for (int i = 0; i < maxIdx; ++i) {
				polyPt1 = points[i];
				polyPt2 = points[i + 1];
				PointF intersection;
				if (isSegment)
					intersection = IntersectLineSegments(a1X, a1Y, a2X, a2Y, polyPt1.X, polyPt1.Y, polyPt2.X, polyPt2.Y);
				else 
					intersection = IntersectLineWithLineSegment(a1X, a1Y, a2X, a2Y, polyPt1.X, polyPt1.Y, polyPt2.X, polyPt2.Y);
				if (IsValid(intersection)) {
					yield return Point.Round(intersection);
				}
			}
			if (points[0] != points[maxIdx]) {
				polyPt1 = points[0];
				polyPt2 = points[maxIdx];
				PointF intersection;
				if (isSegment)
					intersection = IntersectLineSegments(a1X, a1Y, a2X, a2Y, polyPt1.X, polyPt1.Y, polyPt2.X, polyPt2.Y);
				else 
					intersection = IntersectLineWithLineSegment(a1X, a1Y, a2X, a2Y, polyPt1.X, polyPt1.Y, polyPt2.X, polyPt2.Y);
				if (IsValid(intersection))
					yield return Point.Round(intersection);
			}
		}


		/// <summary>
		/// Calculates the intersection points of the line segment from p1 to p2 with the ellipse.
		/// </summary>
		/// <param name="center">Center point of the ellipse.</param>
		/// <param name="ellipseWidth">Width of the ellipse.</param>
		/// <param name="ellipseHeight">Height of the ellipse.</param>
		/// <param name="ellipseAngleDeg">Rotation angle of the ellipse in degrees.</param>
		/// <param name="a1">First point of the line segment.</param>
		/// <param name="a2">Second point of the line segment.</param>
		/// <param name="isSegment">Specifies whether the tested line should be treated as line or as line segment.</param>
		/// <returns>Intersection points of the line with the ellipse.</returns>		
		public static IEnumerable<Point> IntersectEllipseLine(Point center, int ellipseWidth, int ellipseHeight, float ellipseAngleDeg, Point a1, Point a2, bool isSegment) {
			return IntersectEllipseLine(center.X, center.Y, ellipseWidth, ellipseHeight, ellipseAngleDeg, a1.X, a1.Y, a2.X, a2.Y, isSegment);
		}


		/// <summary>
		/// Calculates the intersection points of the line segment from p1 to p2 with the ellipse.
		/// </summary>
		/// <param name="center">Center point of the ellipse.</param>
		/// <param name="ellipseWidth">Width of the ellipse.</param>
		/// <param name="ellipseHeight">Height of the ellipse.</param>
		/// <param name="a1">First point of the line segment.</param>
		/// <param name="p2">Second point of the line segment.</param>
		/// <param name="isSegment">Specifies whether the tested line should be treated as line or as line segment.</param>
		/// <returns>Intersection points of the line with the ellipse.</returns>		
		public static IEnumerable<Point> IntersectEllipseLine(Point center, int ellipseWidth, int ellipseHeight, Point a1, Point p2, bool isSegment) {
			return IntersectEllipseLine(center.X, center.Y, ellipseWidth, ellipseHeight, 0, a1.X, a1.Y, p2.X, p2.Y, isSegment);
		}


		/// <summary>
		/// Calculates the intersection points of the line segment from p1 to p2 with the ellipse.
		/// </summary>
		/// <param name="centerX">X coordinate of the ellipse center point.</param>
		/// <param name="centerY">Y coordinate of the ellipse center point.</param>
		/// <param name="ellipseWidth">Width of the ellipse.</param>
		/// <param name="ellipseHeight">Height of the ellipse.</param>
		/// <param name="a1X">X coordinate of the line segments first point.</param>
		/// <param name="a1Y">Y coordinate of the line segments first point.</param>
		/// <param name="a2X">X coordinate of the line segments second point.</param>
		/// <param name="a2Y">Y coordinate of the line segments second point.</param>
		/// <param name="isSegment">Specifies whether the tested line should be treated as line or as line segment.</param>
		/// <returns>Intersection points of the line with the ellipse.</returns>		
		public static IEnumerable<Point> IntersectEllipseLine(int centerX, int centerY, int ellipseWidth, int ellipseHeight, int a1X, int a1Y, int a2X, int a2Y, bool isSegment) {
			return IntersectEllipseLine(centerX, centerY, ellipseWidth, ellipseHeight, 0, a1X, a1Y, a2X, a2Y, isSegment);
		}


		/// <summary>
		/// Calculates the intersection points of the line segment from p1 to p2 with the ellipse.
		/// </summary>
		/// <param name="centerX">X coordinate of the ellipse center point.</param>
		/// <param name="centerY">Y coordinate of the ellipse center point.</param>
		/// <param name="ellipseWidth">Width of the ellipse.</param>
		/// <param name="ellipseHeight">Height of the ellipse.</param>
		/// <param name="ellipseAngleDeg">Specifies the rotation angle of the ellipse in degrees.</param>
		/// <param name="a1X">X coordinate of the line segments first point.</param>
		/// <param name="a1Y">Y coordinate of the line segments first point.</param>
		/// <param name="a2X">X coordinate of the line segments second point.</param>
		/// <param name="a2Y">Y coordinate of the line segments second point.</param>
		/// <param name="isSegment">Specifies whether the tested line should be treated as line or as line segment.</param>
		/// <returns>Intersection points of the line with the ellipse.</returns>		
		public static IEnumerable<Point> IntersectEllipseLine(int centerX, int centerY, int ellipseWidth, int ellipseHeight, float ellipseAngleDeg, int a1X, int a1Y, int a2X, int a2Y, bool isSegment) {
			Point result = Point.Empty;
			float x, y;
			float radiusX = ellipseWidth / 2f;
			float radiusY = ellipseHeight / 2f;
			float rrx = radiusX * radiusX;
			float rry = radiusY * radiusY;
			// instead of rotating the ellipse, we rotate the line to the opposite side and then rotate intersection points back
			if (ellipseAngleDeg != 0) {
				RotatePoint(centerX, centerY, -ellipseAngleDeg, ref a1X, ref a1Y);
				RotatePoint(centerX, centerY, -ellipseAngleDeg, ref a2X, ref a2Y);
			}
			int dXLine = a2X - a1X;
			int dYLine = a2Y - a1Y;
			int dXCenter = a1X - centerX;
			int dYCenter = a1Y - centerY;
			float a = (((float)dXLine * (float)dXLine) / rrx) + ((float)dYLine * (float)dYLine / rry);
			float b = (((float)dXLine * (float)dXCenter) / rrx) + ((float)dYLine * (float)dYCenter / rry);
			float c = (((float)dXCenter * (float)dXCenter) / rrx) + ((float)dYCenter * (float)dYCenter / rry);
			float d = b * b - a * (c - 1);
			if (d >= 0) {
				float rd = (float)Math.Sqrt(d);
				float u1 = (-b - rd) / a;
				float u2 = (-b + rd) / a;
				if (!float.IsNaN(u1) && (0 <= u1 && u1 <= 1 || !isSegment)) {
					x = (float)(a1X + dXLine * u1);
					y = (float)(a1Y + dYLine * u1);
					if (ellipseAngleDeg != 0) RotatePoint(centerX, centerY, ellipseAngleDeg, ref x, ref y);
					result.X = (int)Math.Round(x);
					result.Y = (int)Math.Round(y);
					yield return result;
				}
				if (!float.IsNaN(u2) && (0 <= u2 && u2 <= 1 || !isSegment)) {
					x = (float)(a1X + dXLine * u2);
					y = (float)(a1Y + dYLine * u2);
					if (ellipseAngleDeg != 0) RotatePoint(centerX, centerY, ellipseAngleDeg, ref x, ref y);
					result.X = (int)Math.Round(x);
					result.Y = (int)Math.Round(y);
					yield return result;
				}
			}
		}


		/// <summary>
		/// Calculates the intersection points of the line segment from p1 to p2 with the ellipse.
		/// </summary>
		/// <param name="centerX">X coordinate of the ellipse center point.</param>
		/// <param name="centerY">Y coordinate of the ellipse center point.</param>
		/// <param name="ellipseWidth">Width of the ellipse.</param>
		/// <param name="ellipseHeight">Height of the ellipse.</param>
		/// <param name="ellipseAngleDeg">Rotation angle of the ellipse.</param>
		/// <param name="a1X">X coordinate of the line segments first point.</param>
		/// <param name="a1Y">Y coordinate of the line segments first point.</param>
		/// <param name="a2X">X coordinate of the line segments second point.</param>
		/// <param name="a2Y">Y coordinate of the line segments second point.</param>
		/// <param name="isSegment">Specifies whether the tested line should be treated as line or as line segment.</param>
		/// <returns>Intersection points of the line with the ellipse.</returns>		
		public static IEnumerable<PointF> IntersectEllipseLine(float centerX, float centerY, float ellipseWidth, float ellipseHeight, float ellipseAngleDeg, float a1X, float a1Y, float a2X, float a2Y, bool isSegment) {
			PointF result = PointF.Empty;
			float x, y;
			float radiusX = ellipseWidth / 2f;
			float radiusY = ellipseHeight / 2f;
			float rrx = radiusX * radiusX;
			float rry = radiusY * radiusY;
			// Instead of rotating the ellipse, we rotate the line to the opposite side and then rotate intersection points back
			if (ellipseAngleDeg != 0) {
				RotatePoint(centerX, centerY, -ellipseAngleDeg, ref a1X, ref a1Y);
				RotatePoint(centerX, centerY, -ellipseAngleDeg, ref a2X, ref a2Y);
			}
			float dXLine = a2X - a1X;
			float dYLine = a2Y - a1Y;
			float dXCenter = a1X - centerX;
			float dYCenter = a1Y - centerY;
			double a = ((dXLine * dXLine) / (double)rrx) + (dYLine * dYLine / (double)rry);
			double b = ((dXLine * dXCenter) / (double)rrx) + (dYLine * dYCenter / (double)rry);
			double c = ((dXCenter * dXCenter) / (double)rrx) + (dYCenter * dYCenter / (double)rry);
			double d = b * b - a * (c - 1);
			if (d >= 0) {
				double rd = Math.Sqrt(d);
				double u1 = (-b - rd) / a;
				double u2 = (-b + rd) / a;
				if (!double.IsNaN(u1) && (0 <= u1 && u1 <= 1 || !isSegment)) {
					x = (float)(a1X + dXLine * u1);
					y = (float)(a1Y + dYLine * u1);
					if (ellipseAngleDeg != 0) RotatePoint(centerX, centerY, ellipseAngleDeg, ref x, ref y);
					result.X = x;
					result.Y = y;
					yield return result;
				}
				if (!double.IsNaN(u2) && (0 <= u2 && u2 <= 1 || !isSegment)) {
					x = (float)(a1X + dXLine * u2);
					y = (float)(a1Y + dYLine * u2);
					if (ellipseAngleDeg != 0) RotatePoint(centerX, centerY, ellipseAngleDeg, ref x, ref y);
					result.X = x;
					result.Y = y;
					yield return result;
				}
			}
		}


		/// <summary>
		/// Calculates the intersection points of the line segment from p1 to p2 with the (rotated) rectangle.
		/// </summary>
		public static IEnumerable<Point> IntersectRectangleLine(Rectangle rectangle, float angleDeg, Point a1, Point a2, bool isSegment) {
			// Instead of rotating the rectangle and checking the 4 sides of the rectangle for intersection with the unrotated line,
			// we rotate the line around the rectangle's center in the opposite direction
			float centerX = rectangle.X + (rectangle.Width / 2f);
			float centerY = rectangle.Y + (rectangle.Height / 2f);

			Point tl = Point.Round(RotatePoint(centerX, centerY, angleDeg, rectangle.Left, rectangle.Top));
			Point tr = Point.Round(RotatePoint(centerX, centerY, angleDeg, rectangle.Right, rectangle.Top));
			Point bl = Point.Round(RotatePoint(centerX, centerY, angleDeg, rectangle.Left, rectangle.Bottom));
			Point br = Point.Round(RotatePoint(centerX, centerY, angleDeg, rectangle.Right, rectangle.Bottom));
			if (isSegment) {
				Point p;
				p = IntersectLineWithLineSegment(a1.X, a1.Y, a2.X, a2.Y, tl.X, tl.Y, tr.X, tr.Y);
				if (IsValid(p)) yield return Point.Round(p);
				p = IntersectLineWithLineSegment(a1.X, a1.Y, a2.X, a2.Y, tr.X, tr.Y, br.X, br.Y);
				if (IsValid(p)) yield return Point.Round(p);
				p = IntersectLineWithLineSegment(a1.X, a1.Y, a2.X, a2.Y, bl.X, bl.Y, br.X, br.Y);
				if (IsValid(p)) yield return Point.Round(p);
				p = IntersectLineWithLineSegment(a1.X, a1.Y, a2.X, a2.Y, tl.X, tl.Y, br.X, br.Y);
				if (IsValid(p)) yield return Point.Round(p);
			} else {
				Point p;
				p = IntersectLineSegments(a1.X, a1.Y, a2.X, a2.Y, tl.X, tl.Y, tr.X, tr.Y);
				if (IsValid(p)) yield return Point.Round(p);
				p = IntersectLineSegments(a1.X, a1.Y, a2.X, a2.Y, tr.X, tr.Y, br.X, br.Y);
				if (IsValid(p)) yield return Point.Round(p);
				p = IntersectLineSegments(a1.X, a1.Y, a2.X, a2.Y, bl.X, bl.Y, br.X, br.Y);
				if (IsValid(p)) yield return Point.Round(p);
				p = IntersectLineSegments(a1.X, a1.Y, a2.X, a2.Y, tl.X, tl.Y, br.X, br.Y);
				if (IsValid(p)) yield return Point.Round(p);
			}
		}

		#endregion


		#region Distance calculation functions

		/// <summary>
		/// Calculate the distance between the two points {x1 | y1} and {x2 | y2}
		/// </summary>
		public static float DistancePointPoint(int aX, int aY, int bX, int bY) {
			int d1 = aX - bX;
			int d2 = aY - bY;
			return (float)Math.Sqrt((double)d1 * d1 + (double)d2 * d2);
		}


		/// <summary>
		/// Calculate the distance between the two points a and b
		/// </summary>
		public static float DistancePointPoint(Point a, Point b) {
			int d1 = a.X - b.X;
			int d2 = a.Y - b.Y;
			return (float)Math.Sqrt((double)d1 * d1 + (double)d2 * d2);
		}


		/// <summary>
		/// Calculate the distance between the two points a and b
		/// </summary>
		public static float DistancePointPoint(float aX, float aY, float bX, float bY) {
			float d1 = aX - bX;
			float d2 = aY - bY;
			return (float)Math.Sqrt(d1 * d1 + d2 * d2);
		}


		/// <summary>
		/// Calculate the distance between the two points a and b
		/// </summary>
		public static float DistancePointPoint(PointF a, PointF b) {
			float d1 = a.X - b.X;
			float d2 = a.Y - b.Y;
			return (float)Math.Sqrt(d1 * d1 + d2 * d2);
		}


		/// <summary>
		/// Calculates the approximate distance between the two points {x1 | y1} and {x2 | y2}
		/// </summary>
		public static int DistancePointPointFast(int aX, int aY, int bX, int bY) {
			int dx = bX - aX;
			int dy = bY - aY;
			int min, max;
			if (dx < 0) dx = -dx;
			if (dy < 0) dy = -dy;

			if (dx < dy) {
				min = dx;
				max = dy;
			} else {
				min = dy;
				max = dx;
			}
			// coefficients equivalent to ( 123/128 * max ) and ( 51/128 * min )
			int result = (((max << 8) + (max << 3) - (max << 4) - (max << 1) +
					 (min << 7) - (min << 5) + (min << 3) - (min << 1)) >> 8);
			return result;
		}


		/// <summary>
		/// Calculates the approximate distance between the two points {x1 | y1} and {x2 | y2}
		/// </summary>
		public static int DistancePointPointFast(int aX, int aY, Point b) {
			return DistancePointPointFast(aX, aY, b.X, b.Y);
		}


		/// <summary>
		/// Calculates the approximate distance betwenn the two points.
		/// </summary>
		public static int DistancePointPointFast(Point a, Point b) {
			return DistancePointPointFast(a.X, a.Y, b.X, b.Y);
		}


		/// <summary>
		/// Calculates the distance of point p from the line through a and b. The result can be positive or negative.
		/// </summary>
		public static float DistancePointLine2(Point p, Point a1, Point a2) {
			return DistancePointLine2(p.X, p.Y, a1.X, a1.Y, a2.X, a2.Y);
		}


		/// <summary>
		/// Calculates the distance of point p from the line through a and b. The result can be positive or negative.
		/// </summary>
		public static float DistancePointLine2(int pX, int pY, int a1X, int a1Y, int a2X, int a2Y) {
			return DistancePointPoint(pX, pY, a1X, a1Y) * (float)Math.Sin(Angle(a1X, a1Y, pX, pY, a2X, a2Y));
		}


		/// <summary>
		/// Berechnet den Abstand des Punktes p von der Geraden linePt1 - linePt2
		/// </summary>
		public static float DistancePointLine2(float pX, float pY, float a1X, float a1Y, float a2X, float a2Y) {
			return DistancePointPoint(pX, pY, a1X, a1Y) * (float)Math.Sin(Angle(a1X, a1Y, pX, pY, a2X, a2Y));
		}


		/// <summary>
		/// Calculates the distance of point p from the line segment a to b. The result is always >= 0.
		/// </summary>
		public static float DistancePointLineSegment2(Point p, Point a1, Point a2) {
			return DistancePointLineSegment2(p.X, p.Y, a1.X, a1.Y, a2.X, a2.Y);
		}


		/// <summary>
		/// Calculates the distance of point p from the line segment a to b. The result is always >= 0.
		/// </summary>
		public static float DistancePointLineSegment2(int pX, int pY, int a1X, int a1Y, int a2X, int a2Y) {
			float result;
			// Liegt der Lotpunkt auf der Strecke?
			// Abstand des Lotpunktes von linePt1 in Richtung linePt2
			float d = DistancePointPoint(pX, pY, a1X, a1Y) * (float)Math.Cos(Angle(a1X, a1Y, pX, pY, a2X, a2Y));
			// Falls ja ist der Abstand gleich dem Abstand von der Geraden
			// Falls nein ist der Abstand der zum näher liegenden Strecken-Endpunkt.
			if (d < 0)
				result = DistancePointPoint(pX, pY, a1X, a1Y);
			else if (d > DistancePointPoint(a2X, a2Y, a1X, a1Y))
				result = DistancePointPoint(pX, pY, a2X, a2Y);
			else
				result = Math.Abs(DistancePointLine2(pX, pY, a1X, a1Y, a2X, a2Y));
			return result;
		}


		/// <summary>
		/// Calculate the distance from p to ab. 
		/// If isSegment is true, ab is not a line but a line segment. This also means that the calculated value is always >= 0.
		/// </summary>
		public static float DistancePointLine(Point p, Point a1, Point a2, bool isSegment) {
			return DistancePointLine(p.X, p.Y, a1.X, a1.Y, a2.X, a2.Y, isSegment);
		}


		/// <summary>
		/// Calculate the distance from p to ab. 
		/// If isSegment is true, ab is not a line but a line segment. This also means that the calculated value is always >= 0.
		/// </summary>
		public static float DistancePointLine(int pX, int pY, int a1X, int a1Y, int a2X, int a2Y, bool isSegment) {
			// Note on performance:
			// Lines:
			// Faster than the float version: 1811 ms versus 2032 ms for the float version (100.000.000 calls, Release)
			// Line Segments:
			// Slower than the float version: 2702 ms versus 1622 ms for the float version (100.000.000 calls, Release)

			if ((pX == a1X && pY == a1Y) || (pX == a2X && pY == a2Y)) return 0;
			if (isSegment) {
				float l = DistancePointPoint(a1X, a1Y, a2X, a2Y);
				if (Equals(l, 0)) return DistancePointPoint(pX, pY, a1X, a1Y);
			}
			// Vector cross product is the size of the parallelogram A, B, P, A'. Dived by the length of 
			// the base line, the result is the requested distance.
			float dist = VectorCrossProductL(a1X, a1Y, a2X, a2Y, pX, pY) / DistancePointPoint(a1X, a1Y, a2X, a2Y);
			if (!isSegment) return dist;
			else {
				// If one of the angles is larger than 90 degree, there is no perpendicular and we must calculate
				// the distance to the nearer point of the two.
				if (VectorDotProductL(a1X, a1Y, a2X, a2Y, pX, pY) > 0)
					return DistancePointPoint(a2X, a2Y, pX, pY);
				if (VectorDotProductL(a2X, a2Y, a1X, a1Y, pX, pY) > 0)
					return DistancePointPoint(a1X, a1Y, pX, pY);
				return Math.Abs(dist);
			}
		}


		/// <summary>
		/// Calculate the distance from p to ab. 
		/// If isSegment is true, ab is not a line but a line segment. This also means that the calculated value is always >= 0.
		/// </summary>
		public static float DistancePointLine(PointF p, PointF a1, PointF a2, bool isSegment) {
			return DistancePointLine(p.X, p.Y, a1.X, a1.Y, a2.X, a2.Y, isSegment);
		}


		///<summary>
		///Calculate the distance from p to ab. If isSegment is true, ab is not not a line but a line segment.
		///If isSegment is true, ab is not not a line but a line segment. This also means that the calculated value is always >= 0.
		///</summary>
		public static float DistancePointLine(float pX, float pY, float a1X, float a1Y, float a2X, float a2Y, bool isSegment) {
			if ((pX == a1X && pY == a1Y) || (pX == a2X && pY == a2Y))
				return 0;
			float dist = VectorCrossProduct(a1X, a1Y, a2X, a2Y, pX, pY) / DistancePointPoint(a1X, a1Y, a2X, a2Y);
			if (isSegment) return dist;
			else {
				if (VectorDotProduct(a1X, a1Y, a2X, a2Y, pX, pY) >= EqualityDeltaFloat)
					return DistancePointPoint(a2X, a2Y, pX, pY);
				if (VectorDotProduct(a2X, a2Y, a1X, a1Y, pX, pY) >= EqualityDeltaFloat)
					return DistancePointPoint(a1X, a1Y, pX, pY);
				return Math.Abs(dist);
			}
		}

		#endregion


		#region Rotation and angle calculation functions
		/// <summary>
		/// Berechnet den Winkel zwischen p1 und p2 mit Scheitel p0 in Radians
		/// </summary>
		public static float Angle(int p0X, int p0Y, int p1X, int p1Y, int p2X, int p2Y) {
			return (float)(Math.Atan2(p1X - p0X, p1Y - p0Y) - Math.Atan2(p2X - p0X, p2Y - p0Y));
		}


		/// <summary>
		/// Berechnet den Winkel zwischen p1 und p2 mit Scheitel p0 in Radians
		/// </summary>
		public static float Angle(float p0X, float p0Y, float p1X, float p1Y, float p2X, float p2Y) {
			return (float)(Math.Atan2(p1X - p0X, p1Y - p0Y) - Math.Atan2(p2X - p0X, p2Y - p0Y));
		}


		/// <summary>
		/// Berechnet den Winkel zwischen p1 und p2 mit Scheitel p0 in Radians
		/// </summary>
		public static float Angle(Point p0, Point p1, Point p2) {
			return Angle(p0.X, p0.Y, p1.X, p1.Y, p2.X, p2.Y);
		}


		/// <summary>
		/// Berechnet den Winkel (in Radians) von p1 zum Scheitelpunkt p0.
		/// </summary>
		public static float Angle(Point p0, Point p1) {
			return (float)Math.Atan2(p1.Y - p0.Y, p1.X - p0.X);
		}


		/// <summary>
		/// Berechnet den Winkel zwischen p1 und p2 mit Scheitel p0 in Radians
		/// </summary>
		public static float Angle(PointF p0, PointF p1, PointF p2) {
			return Angle(p0.X, p0.Y, p1.X, p1.Y, p2.X, p2.Y);
		}


		/// <summary>
		/// Calculates the angle (in radians) between p1 and the center point p0.
		/// </summary>
		public static float Angle(PointF p0, PointF p1) {
			return (float)Math.Atan2(p1.Y - p0.Y, p1.X - p0.X);
		}


		/// <summary>
		/// Calculates the angle (in radians) between p1 and the center point p0.
		/// </summary>
		public static float Angle(int p0X, int p0Y, int p1X, int p1Y) {
			return (float)Math.Atan2(p1Y - p0Y, p1X - p0X);
		}


		/// <summary>
		/// Calculates the angle (in radians) between p1 and the center point p0.
		/// </summary>
		public static float Angle(float p0X, float p0Y, float p1X, float p1Y) {
			return (float)Math.Atan2(p1Y - p0Y, p1X - p0X);
		}


		/// <summary>
		/// Performas a rotation of a point around a center point.
		/// </summary>
		public static PointF RotatePoint(PointF rotationCenter, float angleDeg, PointF point) {
			return RotatePoint(rotationCenter.X, rotationCenter.Y, angleDeg, point);
		}


		/// <summary>
		/// Performas a rotation of a point around a center point.
		/// </summary>
		public static PointF RotatePoint(float rotationCenterX, float rotationCenterY, float angleDeg, PointF point) {
			double a = angleDeg * RadiansFactor;
			double cos = Math.Cos(a);
			double sin = Math.Sin(a);

			float x = point.X - rotationCenterX;
			float y = point.Y - rotationCenterY;
			float dX = (float)((x * cos) - (y * sin));
			float dY = (float)((x * sin) + (y * cos));
			point.X = dX + rotationCenterX;
			point.Y = dY + rotationCenterY;

			return point;
		}


		/// <summary>
		/// Rotate the given Point around the given center
		/// </summary>
		public static Point RotatePoint(Point rotationCenter, float angleDeg, Point point) {
			return RotatePoint(rotationCenter.X, rotationCenter.Y, angleDeg, point);
		}


		/// <summary>
		/// Rotate the given Point around the given center
		/// </summary>
		public static Point RotatePoint(int rotationCenterX, int rotationCenterY, float angleDeg, int x, int y) {
			Point p = Point.Empty;
			p.Offset(x, y);
			return RotatePoint(rotationCenterX, rotationCenterY, angleDeg, p);
		}


		/// <summary>
		/// Rotate the given Point around the given center
		/// </summary>
		public static PointF RotatePoint(float rotationCenterX, float rotationCenterY, float angleDeg, float x, float y) {
			PointF p = PointF.Empty;
			p.X = x; p.Y = y;
			return RotatePoint(rotationCenterX, rotationCenterY, angleDeg, p);
		}


		/// <summary>
		/// Rotate the given Point around the given center
		/// </summary>
		public static Point RotatePoint(int rotationCenterX, int rotationCenterY, float angleDeg, Point point) {
			double a = angleDeg * RadiansFactor;
			double cos = Math.Cos(a);
			double sin = Math.Sin(a);

			int x = point.X - rotationCenterX;
			int y = point.Y - rotationCenterY;
			int dX = (int)Math.Round((x * cos) - (y * sin));
			int dY = (int)Math.Round((x * sin) + (y * cos));
			point.X = dX + rotationCenterX;
			point.Y = dY + rotationCenterY;

			return point;
		}


		/// <summary>
		/// Rotate the given Point around the given center
		/// </summary>
		public static void RotatePoint(float rotationCenterX, float rotationCenterY, float angleDeg, ref float x, ref float y) {
			double a = angleDeg * RadiansFactor;
			double cos = Math.Cos(a);
			double sin = Math.Sin(a);

			float _x = x - rotationCenterX;
			float _y = y - rotationCenterY;
			float dX = (float)((_x * cos) - (_y * sin));
			float dY = (float)((_x * sin) + (_y * cos));
			x = dX + rotationCenterX;
			y = dY + rotationCenterY;
		}


		/// <summary>
		/// Rotate the given Point around the given center
		/// </summary>
		public static void RotatePoint(int rotationCenterX, int rotationCenterY, float angleDeg, ref int x, ref int y) {
			double a = angleDeg * RadiansFactor;
			double cos = Math.Cos(a);
			double sin = Math.Sin(a);

			float _x = x - rotationCenterX;
			float _y = y - rotationCenterY;
			int dX = (int)Math.Round((_x * cos) - (_y * sin));
			int dY = (int)Math.Round((_x * sin) + (_y * cos));
			x = dX + rotationCenterX;
			y = dY + rotationCenterY;
		}


		/// <summary>
		/// Rotates the given line around the given center
		/// </summary>
		public static void RotateLine(Point center, float angleDeg, ref Point a1, ref Point a2) {
			RotateLine(center.X, center.Y, angleDeg, ref a1, ref a2);
		}


		/// <summary>
		/// Rotates the given line around the given center
		/// </summary>
		public static void RotateLine(int centerX, int centerY, float angleDeg, ref Point a1, ref Point a2) {
			a1 = RotatePoint(centerX, centerY, angleDeg, a1);
			a2 = RotatePoint(centerX, centerY, angleDeg, a2);
		}


		/// <summary>
		/// Rotates the given rectangle around the given center
		/// </summary>
		/// <param name="rectangle">Rectangle to rotate</param>
		/// <param name="rotationCenter">Center of rotation</param>
		/// <param name="angleDeg">Rotation angle</param>
		/// <param name="topLeft">Rotated top left corner of the rectangle</param>
		/// <param name="topRight">Rotated top right corner of the rectangle</param>
		/// <param name="bottomRight">Rotated bottom right corner of the rectangle</param>
		/// <param name="bottomLeft">Rotated bottom left corner of the rectangle</param>
		public static void RotateRectangle(Rectangle rectangle, Point rotationCenter, float angleDeg, out Point topLeft, out Point topRight, out Point bottomRight, out Point bottomLeft) {
			RotateRectangle(rectangle, rotationCenter.X, rotationCenter.Y, angleDeg, out topLeft, out topRight, out bottomRight, out bottomLeft);
		}


		/// <summary>
		/// Rotates the given rectangle around the given center
		/// </summary>
		/// <param name="rectangle">Rectangle to rotate</param>
		/// <param name="rotationCenterX">X coordinate of the rotation center</param>
		/// <param name="rotationCenterY">Y coordinate of the rotation center</param>
		/// <param name="angleDeg">Rotation angle</param>
		/// <param name="topLeft">Rotated top left corner of the rectangle</param>
		/// <param name="topRight">Rotated top right corner of the rectangle</param>
		/// <param name="bottomRight">Rotated bottom right corner of the rectangle</param>
		/// <param name="bottomLeft">Rotated bottom left corner of the rectangle</param>
		public static void RotateRectangle(Rectangle rectangle, int rotationCenterX, int rotationCenterY, float angleDeg, out Point topLeft, out Point topRight, out Point bottomRight, out Point bottomLeft) {
			topLeft = RotatePoint(rotationCenterX, rotationCenterY, angleDeg, rectangle.Left, rectangle.Top);
			topRight = RotatePoint(rotationCenterX, rotationCenterY, angleDeg, rectangle.Right, rectangle.Top);
			bottomRight = RotatePoint(rotationCenterX, rotationCenterY, angleDeg, rectangle.Right, rectangle.Bottom);
			bottomLeft = RotatePoint(rotationCenterX, rotationCenterY, angleDeg, rectangle.Left, rectangle.Bottom);
		}


		/// <summary>
		/// Rotates the given rectangle around the given center
		/// </summary>
		/// <param name="rectangle">Rectangle to rotate</param>
		/// <param name="rotationCenter">Center of rotation</param>
		/// <param name="angleDeg">Rotation angle</param>
		/// <param name="topLeft">Rotated top left corner of the rectangle</param>
		/// <param name="topRight">Rotated top right corner of the rectangle</param>
		/// <param name="bottomRight">Rotated bottom right corner of the rectangle</param>
		/// <param name="bottomLeft">Rotated bottom left corner of the rectangle</param>
		public static void RotateRectangle(RectangleF rectangle, PointF rotationCenter, float angleDeg, out PointF topLeft, out PointF topRight, out PointF bottomRight, out PointF bottomLeft) {
			RotateRectangle(rectangle, rotationCenter.X, rotationCenter.Y, angleDeg, out topLeft, out topRight, out bottomRight, out bottomLeft);
		}


		/// <summary>
		/// Rotates the given rectangle around the given center
		/// </summary>
		/// <param name="rectangle">Rectangle to rotate</param>
		/// <param name="rotationCenterX">X coordinate of the rotation center</param>
		/// <param name="rotationCenterY">Y coordinate of the rotation center</param>
		/// <param name="angleDeg">Rotation angle</param>
		/// <param name="topLeft">Rotated top left corner of the rectangle</param>
		/// <param name="topRight">Rotated top right corner of the rectangle</param>
		/// <param name="bottomRight">Rotated bottom right corner of the rectangle</param>
		/// <param name="bottomLeft">Rotated bottom left corner of the rectangle</param>
		public static void RotateRectangle(RectangleF rectangle, float rotationCenterX, float rotationCenterY, float angleDeg, out PointF topLeft, out PointF topRight, out PointF bottomRight, out PointF bottomLeft) {
			topLeft = RotatePoint(rotationCenterX, rotationCenterY, angleDeg, rectangle.Left, rectangle.Top);
			topRight = RotatePoint(rotationCenterX, rotationCenterY, angleDeg, rectangle.Right, rectangle.Top);
			bottomRight = RotatePoint(rotationCenterX, rotationCenterY, angleDeg, rectangle.Right, rectangle.Bottom);
			bottomLeft = RotatePoint(rotationCenterX, rotationCenterY, angleDeg, rectangle.Left, rectangle.Bottom);
		}


		/// <summary>
		/// Translates and rotates the given caption bounds.
		/// </summary>
		/// <param name="captionCenter">Coordinates of the caption's center.</param>
		/// <param name="angle">Rotation angle in tenths of degree.</param>
		/// <param name="captionBounds">Untransformed caption bounds (relative to origin of coordinates).</param>
		/// <param name="topLeft">Transformed top left corner of the given caption bounds.</param>
		/// <param name="topRight">Transformed top right corner of the given caption bounds.</param>
		/// <param name="bottomRight">Transformed bottom right corner of the given caption bounds.</param>
		/// <param name="bottomLeft">Transformed bottom left corner of the given caption bounds.</param>
		public static void TransformRectangle(Point captionCenter, int angle, Rectangle captionBounds,
			out Point topLeft, out Point topRight, out Point bottomRight, out Point bottomLeft) {
			// translate text bounds
			captionBounds.Offset(captionCenter.X, captionCenter.Y);
			// rotate text bounds
			RotateRectangle(captionBounds, captionCenter, Geometry.TenthsOfDegreeToDegrees(angle),
				out topLeft, out topRight, out bottomRight, out bottomLeft);
		}



		/// <summary>
		/// Translates and rotates the given caption bounds.
		/// </summary>
		/// <param name="captionCenter">Coordinates of the caption's center.</param>
		/// <param name="rotationCenter">Coordinates of the rotation center.</param>
		/// <param name="angle">Rotation angle in tenths of degree.</param>
		/// <param name="captionBounds">Untransformed caption bounds (relative to origin of coordinates).</param>
		/// <param name="topLeft">Transformed top left corner of the given caption bounds.</param>
		/// <param name="topRight">Transformed top right corner of the given caption bounds.</param>
		/// <param name="bottomRight">Transformed bottom right corner of the given caption bounds.</param>
		/// <param name="bottomLeft">Transformed bottom left corner of the given caption bounds.</param>
		public static void TransformRectangle(Point captionCenter, Point rotationCenter, int angle, Rectangle captionBounds,
			out Point topLeft, out Point topRight, out Point bottomRight, out Point bottomLeft) {
			// translate text bounds
			captionBounds.Offset(captionCenter.X, captionCenter.Y);
			// rotate text bounds
			RotateRectangle(captionBounds, rotationCenter, Geometry.TenthsOfDegreeToDegrees(angle),
				out topLeft, out topRight, out bottomRight, out bottomLeft);
		}



		/// <summary>
		/// Translates and rotates the given caption bounds.
		/// </summary>
		/// <param name="captionCenter">Coordinates of the caption's center.</param>
		/// <param name="angle">Rotation angle in tenths of degree.</param>
		/// <param name="captionBounds">Untransformed caption bounds (relative to origin of coordinates).</param>
		/// <param name="topLeft">Transformed top left corner of the given caption bounds.</param>
		/// <param name="topRight">Transformed top right corner of the given caption bounds.</param>
		/// <param name="bottomRight">Transformed bottom right corner of the given caption bounds.</param>
		/// <param name="bottomLeft">Transformed bottom left corner of the given caption bounds.</param>
		public static void TransformRectangle(Point captionCenter, int angle, Rectangle captionBounds,
			out PointF topLeft, out PointF topRight, out PointF bottomRight, out PointF bottomLeft) {
			// translate text bounds
			captionBounds.Offset(captionCenter.X, captionCenter.Y);
			// rotate text bounds
			RotateRectangle(captionBounds, captionCenter, Geometry.TenthsOfDegreeToDegrees(angle),
				out topLeft, out topRight, out bottomRight, out bottomLeft);
		}



		/// <summary>
		/// Translates and rotates the given caption bounds.
		/// </summary>
		/// <param name="captionCenter">Coordinates of the caption's center.</param>
		/// <param name="rotationCenter">Coordinates of the rotation center.</param>
		/// <param name="angle">Rotation angle in tenths of degree.</param>
		/// <param name="captionBounds">Untransformed caption bounds (relative to origin of coordinates).</param>
		/// <param name="topLeft">Transformed top left corner of the given caption bounds.</param>
		/// <param name="topRight">Transformed top right corner of the given caption bounds.</param>
		/// <param name="bottomRight">Transformed bottom right corner of the given caption bounds.</param>
		/// <param name="bottomLeft">Transformed bottom left corner of the given caption bounds.</param>
		public static void TransformRectangle(Point captionCenter, Point rotationCenter, int angle, Rectangle captionBounds,
			out PointF topLeft, out PointF topRight, out PointF bottomRight, out PointF bottomLeft) {
			// translate text bounds
			captionBounds.Offset(captionCenter.X, captionCenter.Y);
			// rotate text bounds
			RotateRectangle(captionBounds, rotationCenter, Geometry.TenthsOfDegreeToDegrees(angle),
				out topLeft, out topRight, out bottomRight, out bottomLeft);
		}



		/// <summary>
		/// Converts and angle measured in tenths of degrees to an angle measured in radians.
		/// </summary>
		/// <param name="angle">Angle measured in tenths of degree.</param>
		/// <returns>Angle measured in radians</returns>
		public static float TenthsOfDegreeToRadians(int angle) {
			return (float)(angle * (RadiansFactor / 10));
		}


		/// <summary>
		/// Converts and angle measured in tenths of degrees to an angle measured in degrees.
		/// </summary>
		/// <param name="angle">Angle measured in tenths of degree.</param>
		/// <returns>Angle measured in degrees</returns>
		public static float TenthsOfDegreeToDegrees(int angle) {
			return angle / 10f;
		}


		/// <summary>
		/// Converts and angle measured in degrees to an angle measured in radians.
		/// </summary>
		/// <param name="angle">Angle measured in degrees</param>
		/// <returns>Angle measured in radians</returns>
		public static float DegreesToRadians(float angle) {
			return (float)(angle * RadiansFactor);
		}


		/// <summary>
		/// Converts and angle measured in degrees to an angle measured in tenths of degree.
		/// </summary>
		/// <param name="angle">Angle measured in degrees</param>
		/// <returns>Angle measured in tenths of degree</returns>
		public static int DegreesToTenthsOfDegree(float angle) {
			return (int)Math.Round(angle * 10);
		}


		/// <summary>
		/// Converts and angle measured in radians to an angle measured in degrees.
		/// </summary>
		/// <param name="angle">Angle measured in radians</param>
		/// <returns>Angle measured in degrees</returns>
		public static float RadiansToDegrees(float angle) {
			return (float)(angle / RadiansFactor);
		}


		/// <summary>
		/// Converts and angle measured in radians to an angle measured in tenths of degree.
		/// </summary>
		/// <param name="angle">Angle measured in radians</param>
		/// <returns>Angle measured in tenths of degree</returns>
		public static int RadiansToTenthsOfDegree(float angle) {
			return (int)Math.Round(angle / (RadiansFactor / 10));
		}

		#endregion


		/// <summary>
		/// Calculates the scale factor that has to be applied in order to fit the source rectangle into the destination rectangle.
		/// </summary>
		public static float CalcScaleFactor(int srcWidth, int srcHeight, int dstWidth, int dstHeight) {
			return CalcScaleFactor(srcWidth, srcHeight, dstWidth, dstHeight, true);
		}


		/// <summary>
		/// Calculates the scale factor that has to be applied in order to fit the source rectangle into the destination rectangle.
		/// </summary>
		public static float CalcScaleFactor(float srcWidth, float srcHeight, float dstWidth, float dstHeight) {
			return CalcScaleFactor(srcWidth, srcHeight, dstWidth, dstHeight, true);
		}


		/// <summary>
		/// Calculates the scale factor that has to be applied in order to fit the source rectangle into the destination rectangle.
		/// </summary>
		public static float CalcScaleFactor(int srcWidth, int srcHeight, int dstWidth, int dstHeight, bool minimum) {
			return CalcScaleFactor((float)srcWidth, (float)srcHeight, (float)dstWidth, (float)dstHeight, minimum);
		}


		/// <summary>
		/// Calculates the scale factor that has to be applied in order to fit the source rectangle into the destination rectangle.
		/// </summary>
		public static float CalcScaleFactor(float srcWidth, float srcHeight, float dstWidth, float dstHeight, bool minimum) {
			float scaleX, scaleY;
			CalcScaleFactor(srcWidth, srcHeight, dstWidth, dstHeight, out scaleX, out scaleY);
			return minimum ? Math.Min(scaleX, scaleY) : Math.Max(scaleX, scaleY);
		}


		/// <summary>
		/// Calculates the scale factor that has to be applied in order to fit the source rectangle into the destination rectangle
		/// </summary>
		/// <param name="srcWidth">Width of the source rectangle</param>
		/// <param name="srcHeight">Height of the source rectangle</param>
		/// <param name="dstWidth">Width of the destinalion rectangle</param>
		/// <param name="dstHeight">Height of the destinalion rectangle</param>
		/// <param name="scaleX">Scale factor in X direction</param>
		/// <param name="scaleY">Scale factor in X direction</param>
		public static void CalcScaleFactor(int srcWidth, int srcHeight, int dstWidth, int dstHeight, out float scaleX, out float scaleY) {
			CalcScaleFactor((float)srcWidth, (float)srcHeight, (float)dstWidth, (float)dstHeight, out scaleX, out scaleY);
		}


		/// <summary>
		/// Calculates the scale factor that has to be applied in order to fit the source rectangle into the destination rectangle
		/// </summary>
		/// <param name="srcWidth">Width of the source rectangle</param>
		/// <param name="srcHeight">Height of the source rectangle</param>
		/// <param name="dstWidth">Width of the destinalion rectangle</param>
		/// <param name="dstHeight">Height of the destinalion rectangle</param>
		/// <param name="scaleX">Scale factor in X direction</param>
		/// <param name="scaleY">Scale factor in X direction</param>
		public static void CalcScaleFactor(float srcWidth, float srcHeight, float dstWidth, float dstHeight, out float scaleX, out float scaleY) {
			scaleX = (float)dstWidth / (float)(srcWidth != 0 ? srcWidth : 1);
			scaleY = (float)dstHeight / (float)(srcHeight != 0 ? srcHeight : 1);
		}


		/// <summary>
		/// Calculates a point on a cubic bezier curve
		/// </summary>
		/// <param name="A">The first interpolation point of the bezier curve</param>
		/// <param name="B">The second interpolation point of the bezier curve</param>
		/// <param name="C">The third interpolation point of the bezier curve</param>
		/// <param name="D">The fourth interpolation point of the bezier curve</param>
		/// <param name="distance">the distance of the point to calculate</param>
		public static PointF BezierPoint(PointF A, PointF B, PointF C, PointF D, float distance) {
			PointF p = Point.Empty;
			int n = 200;
			float a = distance * (1f / n);
			float b = 1.0f - a;
			p.X = A.X * a * a * a + B.X * 3 * a * a * b + C.X * 3 * a * b * b + D.X * b * b * b;
			p.Y = A.Y * a * a * a + B.Y * 3 * a * a * b + C.Y * 3 * a * b * b + D.Y * b * b * b;
			return p;
		}


		/// <summary>
		/// Calculates the balance point of a polygon
		/// </summary>
		public static PointF CalcPolygonBalancePoint(IEnumerable<PointF> points) {
			if (points == null) throw new ArgumentNullException(nameof(points));
			PointF result = PointF.Empty;
			int pointCnt = 0;
			foreach (PointF p in points) {
				result.X += p.X;
				result.Y += p.Y;
				++pointCnt;
			}
			result.X = result.X / pointCnt;
			result.Y = result.Y / pointCnt;
			return result;
		}


		/// <summary>
		/// Calculates the balance point of a polygon
		/// </summary>
		public static void CalcPolygonBalancePoint(IEnumerable<PointF> points, out float x, out float y) {
			if (points == null) throw new ArgumentNullException(nameof(points));
			x = 0; y = 0;
			int pointCnt = 0;
			foreach (PointF p in points) {
				x += p.X;
				y += p.Y;
				++pointCnt;
			}
			x = x / pointCnt;
			y = y / pointCnt;
		}


		/// <summary>
		/// Calculates the balance point of a polygon
		/// </summary>
		public static Point CalcPolygonBalancePoint(IEnumerable<Point> points) {
			if (points == null) throw new ArgumentNullException(nameof(points));
			Point result = Point.Empty;
			int pointCnt = 0;
			foreach (Point p in points) {
				result.Offset(p.X, p.Y);
				++pointCnt;
			}
			result.X = (int)Math.Round(result.X / (float)pointCnt);
			result.Y = (int)Math.Round(result.Y / (float)pointCnt);
			return result;
		}


		/// <summary>
		/// Calculates the balance point of a polygon
		/// </summary>
		public static void CalcPolygonBalancePoint(IEnumerable<Point> points, out int x, out int y) {
			if (points == null) throw new ArgumentNullException(nameof(points));
			x = 0; y = 0;
			int pointCnt = 0;
			foreach (Point p in points) {
				x += p.X;
				y += p.Y;
				++pointCnt;
			}
			x = (int)Math.Round(x / (float)pointCnt);
			y = (int)Math.Round(y / (float)pointCnt);
		}


		/// <summary>
		/// Returns the boundingRectangle of a rotated Point collection. 
		/// The Vertices in the collections are neither copied nor changed.
		/// </summary>
		public static void CalcBoundingRectangle(IEnumerable<Point> points, int rotationCenterX, int rotationCenterY, float angleDeg, out Rectangle rectangle) {
			if (points == null) throw new ArgumentNullException(nameof(points));
			rectangle = Rectangle.Empty;
			int left = int.MaxValue;
			int top = int.MaxValue;
			int right = int.MinValue;
			int bottom = int.MinValue;
			int pX, pY;
			foreach (Point p in points) {
				pX = p.X; pY = p.Y;
				if (angleDeg != 0) RotatePoint(rotationCenterX, rotationCenterY, angleDeg, ref pX, ref pY);

				if (pX < left) left = pX;
				if (pX > right) right = pX;
				if (pY < top) top = pY;
				if (pY > bottom) bottom = pY;
			}
			rectangle.X = left;
			rectangle.Y = top;
			rectangle.Width = right - left;
			rectangle.Height = bottom - top;
		}


		/// <summary>
		/// Returns the bounding rectangle of a point array.
		/// </summary>
		/// <param name="points">The point array</param>
		/// <param name="rectangle">The resulting bounding rectangle</param>
		public static void CalcBoundingRectangle(IEnumerable<Point> points, out Rectangle rectangle) {
			if (points == null) throw new ArgumentNullException(nameof(points));
			rectangle = Rectangle.Empty;
			int left = int.MaxValue;
			int top = int.MaxValue;
			int right = int.MinValue;
			int bottom = int.MinValue;
			foreach (Point p in points) {
				if (p.X < left) left = p.X;
				if (p.X > right) right = p.X;
				if (p.Y < top) top = p.Y;
				if (p.Y > bottom) bottom = p.Y;
			}
			rectangle.X = left;
			rectangle.Y = top;
			rectangle.Width = right - left;
			rectangle.Height = bottom - top;
		}


		/// <summary>
		/// Calculates the axis-aligned bounding rectangle of the given point collection. 
		/// </summary>
		/// <param name="points">Collection of points</param>
		/// <param name="rectangle">The resulting bounding rectangle</param>
		/// <param name="floor">Specifies if PointF values should be floored or ceiled</param>
		public static void CalcBoundingRectangle(IEnumerable<PointF> points, out Rectangle rectangle, bool floor) {
			if (points == null) throw new ArgumentNullException(nameof(points));
			rectangle = Rectangle.Empty;
			float left = int.MaxValue;
			float top = int.MaxValue;
			float right = int.MinValue;
			float bottom = int.MinValue;
			foreach (PointF p in points) {
				if (p.X < left) left = p.X;
				if (p.X > right) right = p.X;
				if (p.Y < top) top = p.Y;
				if (p.Y > bottom) bottom = p.Y;
			}
			rectangle.X = floor ? (int)Math.Ceiling(left) : (int)Math.Floor(left);
			rectangle.Y = floor ? (int)Math.Ceiling(top) : (int)Math.Floor(top);
			rectangle.Width = (floor ? (int)Math.Floor(right) : (int)Math.Ceiling(right)) - rectangle.Left;
			rectangle.Height = (floor ? (int)Math.Floor(bottom) : (int)Math.Ceiling(bottom)) - rectangle.Top;
		}


		/// <summary>
		/// Calculates the axis-aligned bounding rectangle of the given point collection
		/// </summary>
		public static void CalcBoundingRectangle(IEnumerable<PointF> points, out Rectangle rectangle) {
			CalcBoundingRectangle(points, out rectangle, false);
		}


		/// <summary>
		/// Calculates the x axis aligned bounding rectangle of a point array.
		/// </summary>
		/// <param name="p1">The top left corner of the rectangle.</param>
		/// <param name="p2">The top right corner of the rectangle.</param>
		/// <param name="p3">The bottom right corner of the rectangle.</param>
		/// <param name="p4">The bottom left corner of the rectangle.</param>
		/// <param name="rectangle">The resulting bounding rectangle</param>
		public static void CalcBoundingRectangle(Point p1, Point p2, Point p3, Point p4, out Rectangle rectangle) {
			int left = Math.Min(Math.Min(p1.X, p2.X), Math.Min(p3.X, p4.X));
			int top = Math.Min(Math.Min(p1.Y, p2.Y), Math.Min(p3.Y, p4.Y));
			int right = Math.Max(Math.Max(p1.X, p2.X), Math.Max(p3.X, p4.X));
			int bottom = Math.Max(Math.Max(p1.Y, p2.Y), Math.Max(p3.Y, p4.Y));
			rectangle = Rectangle.Empty;
			rectangle.Offset(left, top);
			rectangle.Width = right - left;
			rectangle.Height = bottom - top;
		}


		/// <summary>
		/// Calculates the x axis aligned bounding rectangle of a point array.
		/// </summary>
		/// <param name="p1">The top left corner of the rectangle.</param>
		/// <param name="p2">The top right corner of the rectangle.</param>
		/// <param name="p3">The bottom right corner of the rectangle.</param>
		/// <param name="p4">The bottom left corner of the rectangle.</param>
		/// <param name="rectangle">The resulting bounding rectangle</param>
		public static void CalcBoundingRectangle(PointF p1, PointF p2, PointF p3, PointF p4, out Rectangle rectangle) {
			float left = Math.Min(Math.Min(p1.X, p2.X), Math.Min(p3.X, p4.X));
			float top = Math.Min(Math.Min(p1.Y, p2.Y), Math.Min(p3.Y, p4.Y));
			float right = Math.Max(Math.Max(p1.X, p2.X), Math.Max(p3.X, p4.X));
			float bottom = Math.Max(Math.Max(p1.Y, p2.Y), Math.Max(p3.Y, p4.Y));
			rectangle = Rectangle.Empty;
			rectangle.Offset((int)Math.Floor(left), (int)Math.Floor(top));
			rectangle.Width = (int)Math.Ceiling(right) - rectangle.X;
			rectangle.Height = (int)Math.Ceiling(bottom) - rectangle.Y;
		}


		/// <summary>
		/// Calculates the x axis aligned bounding rectangle of a rotated ellipse.
		/// </summary>
		public static void CalcBoundingRectangleEllipse(int centerX, int centerY, int width, int height, float angleDeg, out Rectangle rectangle) {
			rectangle = Geometry.InvalidRectangle;
			// a is the major half axis
			// b is the minor half axis
			// phi is the ratation angle of the ellipse
			// t1/t2 are the angles where to find the maxima:
			// The formulas how to calculate the maxima:
			//	   x = centerX + a * cos(t) * cos(phi) - b * sin(t) * sin(phi)  [1]
			//	   y = centerY + b * sin(t) * cos(phi) + a * cos(t) * sin(phi)  [2]
			// The formula how to calculate the angle t:
			//    tan(t) = -b * tan(phi) / a   [3]
			//    tan(t) = b * cot(phi) / a  [4]
			float a = width / 2f;
			float b = height / 2f;
			float phi = Geometry.DegreesToRadians(angleDeg);
			double tanPhi = Math.Tan(phi);
			double sinPhi = Math.Sin(phi);
			double cosPhi = Math.Cos(phi);
			float t1 = (float)Math.Round(Math.Atan(-b * tanPhi / a), 7, MidpointRounding.ToEven);
			float t2 = (float)Math.Round(Math.Atan(b * (1 / tanPhi) / a), 7, MidpointRounding.ToEven);
			double sinT1 = Math.Sin(t1);
			double cosT1 = Math.Cos(t1);
			double sinT2 = Math.Sin(t2);
			double cosT2 = Math.Cos(t2);

			float x1 = (float)Math.Abs(a * cosT1 * cosPhi - b * sinT1 * sinPhi);
			float x2 = (float)Math.Abs(a * cosT2 * cosPhi - b * sinT2 * sinPhi);
			float y1 = (float)Math.Abs(b * sinT1 * cosPhi + a * cosT1 * sinPhi);
			float y2 = (float)Math.Abs(b * sinT2 * cosPhi + a * cosT2 * sinPhi);

			rectangle.X = (int)Math.Floor(centerX - Math.Max(x1, x2));
			rectangle.Y = (int)Math.Floor(centerY - Math.Max(y1, y2));
			rectangle.Width = (int)Math.Ceiling(centerX + Math.Max(x1, x2)) - rectangle.X;
			rectangle.Height = (int)Math.Ceiling(centerY + Math.Max(y1, y2)) - rectangle.Y;
		}


		/// <summary>
		/// Converts font size in points to font size in pixels
		/// </summary>
		public static int PointToPixel(float sizeInPoints, float dpiY) {
			return (int)Math.Ceiling((sizeInPoints / 72) * dpiY);
		}


		/// <summary>
		/// Converts font size in points to font size in pixels
		/// </summary>
		public static float PixelToPoint(int sizeInPixel, float dpiY) {
			return (sizeInPixel * 72) / dpiY;
		}


		/// <summary>
		/// Calculates the coordinates of a point from another point with angle and distance;
		/// </summary>
		public static Point CalcPoint(int x, int y, float angleDeg, float distance) {
			return Point.Round(CalcPoint((float)x, (float)y, angleDeg, distance));
		}


		/// <summary>
		/// Calculates the coordinates of a point from another point with angle and distance;
		/// </summary>
		public static PointF CalcPoint(float x, float y, float angleDeg, float distance) {
			float angle = DegreesToRadians(angleDeg);
			PointF result = PointF.Empty;
			result.X = (float)(x + distance * Math.Cos(angle));
			result.Y = (float)(y + distance * Math.Sin(angle));
			return result;
		}


		/// <summary>
		/// Calculates the position of the point on the line. 
		/// </summary>
		public static Point CalcPointOnLine(int startX, int startY, int endX, int endY, float distanceFromStart) {
			return Point.Round(CalcPointOnLine((float)startX, (float)startY, (float)endX, (float)endY, distanceFromStart));
		}


		/// <summary>
		/// Calculates the position of the point on the line.
		/// </summary>
		public static PointF CalcPointOnLine(float startX, float startY, float endX, float endY, float distanceFromStart) {
			//float angle = RadiansToDegrees(Angle(startX, startY, endX, endY));
			//float ptX = startX + distance;
			//float ptY = startY;
			//RotatePoint(startX, startY, angle, ref ptX, ref ptY);
			//PointF result = PointF.Empty;
			//result.X = ptX;
			//result.Y = ptY;
			//return result;
			float dist = DistancePointPoint(startX, startY, endX, endY);
			if (Equals(dist, 0)) return new PointF(startX, startY);
			return VectorLinearInterpolation(startX, startY, endX, endY, distanceFromStart / dist);
		}


		/// <summary>
		/// Calculates the parameters a, b and c of the line formula ax + by + c = 0 from two given points
		/// </summary>
		[Obsolete("CAUTION: Arithmetic overflows with large coordinates, use version with two-point form instead.")]
		public static void CalcLine(Point p1, Point p2, out int a, out int b, out int c) {
			CalcLine(p1.X, p1.Y, p2.X, p2.Y, out a, out b, out c);
		}


		/// <summary>
		/// Calculates the parameters a, b and c of the line formula ax + by + c = 0 from two given points.
		/// In order to be compatible with the Hesse normal form, c is always negative, -c is the distance 
		/// of the line from the origin. The vector A-B points from the origin towards the line.
		/// </summary>
		[Obsolete("CAUTION: Arithmetic overflows with large coordinates, use version with two-point form instead.")]
		public static void CalcLine(int x1, int y1, int x2, int y2, out int a, out int b, out int c) {
			a = y2 - y1;
			b = x2 - x1;
			c = (int)((long)a * x1 - (long)b * y1);
			if (c > 0) { b = -b; c = -c; } else a = -a;
		}


		// Calculation speed analysis for 100.000.000 function calls:
		// ~310ms for int and float version
		// ~820ms for version with "out long a, out long b, out long c".
		// ~330ms for version with "out int a, out int b, out long c".
		/// <summary>
		/// Calculates the parameters a, b and c of the line formula ax + by + c = 0 from two given points.
		/// In order to be compatible with the Hesse normal form, c is always negative, -c is the distance 
		/// of the line from the origin. The vector A-B points from the origin towards the line.
		/// </summary>
		[Obsolete("CAUTION: Arithmetic overflows with large coordinates, use version with two-point form instead.")]
		internal static void CalcLine(int x1, int y1, int x2, int y2, out int a, out int b, out long c) {
			// This version is nearly as fast as the pure int version, but's safe in respect of arithmetic overflow.
			a = y2 - y1;
			b = x2 - x1;
			c = (long)a * x1 - (long)b * y1;
			if (c > 0) { b = -b; c = -c; } else a = -a;
		}


		/// <summary>
		/// Calculates the parameters a, b and c of the line formula ax + by + c = 0 from two given points
		/// </summary>
		[Obsolete("CAUTION: Arithmetic overflows with large coordinates, use version with two-point form instead.")]
		public static void CalcLine(PointF p1, PointF p2, out float a, out float b, out float c) {
			CalcLine(p1.X, p1.Y, p2.X, p2.Y, out a, out b, out c);
		}


		/// <summary>
		/// Calculates the parameters a, b and c of the line formula ax + by + c = 0 from two given points
		/// In order to be compatible with the Hesse normal form, c is always negative, -c is the distance 
		/// of the line from the origin. The vector A-B points from the origin towards the line.
		/// </summary>
		[Obsolete("CAUTION: Arithmetic overflows with large coordinates, use version with two-point form instead.")]
		public static void CalcLine(float x1, float y1, float x2, float y2, out float a, out float b, out float c) {
			a = y2 - y1;
			b = x2 - x1;
			c = a * x1 - b * y1;
			if (c > 0) { b = -b; c = -c; } else a = -a;
		}


		/// <summary>
		/// Calculates the parameters m and c of the line formula y = mx + c from two given points
		/// </summary>
		public static void CalcLine(Point a1, Point a2, out float m, out float c) {
			CalcLine(a1.X, a1.Y, a2.X, a2.Y, out m, out c);
		}


		/// <summary>
		/// Calculates the parameters m and c of the line formula y = mx + c from two given points
		/// </summary>
		public static void CalcLine(int a1X, int a1Y, int a2X, int a2Y, out float m, out float c) {
			m = 0;
			c = 0;
			int m1 = a1Y - a2Y;
			int m2 = a1X - a2X;
			if (m2 != 0)
				m = m1 / (float)m2;
			else {
				if (m1 < 0) m = float.NegativeInfinity;
				else m = float.PositiveInfinity;
			}
			c = a1Y - (m * a1X);
		}


		/// <summary>
		/// Calculates the parameters m and c of the line formula y = mx + c from two given points
		/// </summary>
		public static void CalcLine(float a1X, float a1Y, float a2X, float a2Y, out float m, out float c) {
			m = 0;
			c = 0;
			float m1 = a1Y - a2Y;
			float m2 = a1X - a2X;
			if (m2 != 0)
				m = m1 / m2;
			else {
				if (m1 < 0) m = float.NegativeInfinity;
				else m = float.PositiveInfinity;
			}
			c = a1Y - (m * a1X);
		}


		/// <summary>
		/// Calculates the center point and the radius of an arc defined by start point, end point and a third point on the arc ('radius point')
		/// </summary>
		[Obsolete("Use Geometry.CalcCircumCircle instead.")]
		public static PointF CalcArcCenterAndRadius(PointF startPt, PointF radiusPt, PointF endPt, out float radius) {
			PointF result;
			CalcCircumCircle(startPt.X, startPt.Y, radiusPt.X, radiusPt.Y, endPt.X, endPt.Y, out result, out radius);
			return result;
		}


		/// <summary>
		/// Calculates the center point and the radius of an arc defined by start point, end point and a third point on the arc ('radius point')
		/// </summary>
		[Obsolete("Use Geometry.CalcCircumCircle instead.")]
		public static PointF CalcArcCenterAndRadius(float startPtX, float startPtY, float radiusPtX, float radiusPtY, float endPtX, float endPtY, out float radius) {
			PointF result;
			CalcCircumCircle(startPtX, startPtY, radiusPtX, radiusPtY, endPtX, endPtY, out result, out radius);
			return result;
		}


		/// <summary>
		/// Calculates the center point and the radius of an arc defined by start point, end point and a third point on the arc ('radius point')
		/// </summary>
		public static void CalcCircumCircle(PointF startPt, PointF radiusPt, PointF endPt, out PointF center, out float radius) {
			CalcCircumCircle(startPt.X, startPt.Y, radiusPt.X, radiusPt.Y, endPt.X, endPt.Y, out center, out radius);
		}


		/// <summary>
		/// Calculates the center point and the radius of an arc defined by start point, end point and a third point on the arc ('radius point')
		/// </summary>
		public static void CalcCircumCircle(float startPtX, float startPtY, float radiusPtX, float radiusPtY, float endPtX, float endPtY, out PointF center, out float radius) {
			center = Geometry.InvalidPointF;
			radius = 0;

			//// Geradengleichung der Kreissehne StartPoint/RadiusPoint
			//float a1s, b1s, c1s;
			//CalcLine(startPtX, startPtY, radiusPtX, radiusPtY, out a1s, out b1s, out c1s);
			//// Geradengleichung der Mittelsenkrechten der Kreisssehne StartPoint/RadiusPoint
			//float a1m, b1m, c1m;
			//CalcPerpendicularBisector(startPtX, startPtY, radiusPtX, radiusPtY, out a1m, out b1m, out c1m);
			//// Schnittpunkt der Mittelsenkrechten mit der Kreissehne berechnen berechnen
			//float pt1X, pt1Y;
			//IntersectLines(a1s, b1s, c1s, a1m, b1m, c1m, out pt1X, out pt1Y);

			//// Geradengleichung der Kreissehne EndPoint/RadiusPoint
			//float a2s, b2s, c2s;
			//CalcLine(endPtX, endPtY, radiusPtX, radiusPtY, out a2s, out b2s, out c2s);
			//// Geradengleichung der Mittelsenkrechten der Strecke EndPoint-RadiusPoint
			//float a2m, b2m, c2m;
			//CalcPerpendicularBisector(endPtX, endPtY, radiusPtX, radiusPtY, out a2m, out b2m, out c2m);
			//// Schnittpunkt der Mittelsenkrechten mit der Kreissehne berechnen berechnen
			//float pt2X, pt2Y;
			//IntersectLines(a2s, b2s, c2s, a2m, b2m, c2m, out pt2X, out pt2Y);

			//// Schnittpunkte der Mittelsenkrechten berechnen
			//float cX, cY;
			//IntersectLines(a1m, b1m, c1m, a2m, b2m, c2m, out cX, out cY);

			// Schnittpunkt der Kreissehne StartPoint/RadiusPoint mit ihrer Mittelsenkrechten berechnen
			float b1P1X, b1P1Y, b1P2X, b1P2Y;
			CalcPerpendicularBisector(startPtX, startPtY, radiusPtX, radiusPtY, out b1P1X, out b1P1Y, out b1P2X, out b1P2Y);
			// Schnittpunkt der Kreissehne EndPoint/RadiusPoint mit ihrer Mittelsenkrechten berechnen
			float b2P1X, b2P1Y, b2P2X, b2P2Y;
			CalcPerpendicularBisector(endPtX, endPtY, radiusPtX, radiusPtY, out b2P1X, out b2P1Y, out b2P2X, out b2P2Y);

			// Schnittpunkt der beiden Mittelsenkrechten berechnen
			center = IntersectLines(b1P1X, b1P1Y, b1P2X, b1P2Y, b2P1X, b2P1Y, b2P2X, b2P2Y);
			if (IsValid(center))
				radius = DistancePointPoint(center.X, center.Y, radiusPtX, radiusPtY);
		}


		/// <summary>Berechnet die Lotrechte durch den Punkt x1/y1</summary>
		public static void CalcPerpendicularLine(int a1X, int a1Y, int a2X, int a2Y, out int b1X, out int b1Y, out int b2X, out int b2Y) {
			// Steigung
			int dX = (int)Math.Round((a2X - a1X) / 2f);
			int dY = (int)Math.Round((a2Y - a1Y) / 2f);
			b1X = a1X + dY;
			b1Y = a1Y - dX;
			b2X = a1X - dY;
			b2Y = a1Y + dX;
		}


		/// <summary>Berechnet die Lotrechte durch den Punkt x1/y1</summary>
		public static void CalcPerpendicularLine(float a1X, float a1Y, float a2X, float a2Y, out float b1X, out float b1Y, out float b2X, out float b2Y) {
			// Steigung
			float dX = (a2X - a1X) / 2f;
			float dY = (a2Y - a1Y) / 2f;
			b1X = a1X + dY;
			b1Y = a1Y - dX;
			b2X = a1X - dY;
			b2Y = a1Y + dX;
		}


		/// <summary>Calculates the perpendicular line through p.</summary>
		public static void CalcPerpendicularLine(Point a1, Point a2, Point p, out Point p2) {
			CalcPerpendicularLine(a1.X, a1.Y, a2.X, a2.Y, p.X, p.Y, out int p2X, out int p2Y);
			p2 = Point.Empty;
			p2.Offset(p2X, p2Y);
		}


		/// <summary>Calculates the perpendicular line through p.</summary>
		public static void CalcPerpendicularLine(Point a1, Point a2, int pX, int pY, out int p2X, out int p2Y) {
			CalcPerpendicularLine(a1.X, a1.Y, a2.X, a2.Y, pX, pY, out p2X, out p2Y);
		}


		/// <summary>Calculates the perpendicular line through pX/pY</summary>
		public static void CalcPerpendicularLine(int a1X, int a1Y, int a2X, int aY2, int pX, int pY, out int p2X, out int p2Y) {
			// Steigung der Geraden
			int dX = a2X - a1X;
			int dY = aY2 - a1Y;
			//
			p2X = pX - dY;
			p2Y = pY + dX;
		}



		/// <summary>Calculates the perpendicular line through pX/pY</summary>
		public static void CalcPerpendicularLine(float a1X, float a1Y, float a2X, float a2Y, float pX, float pY, out float p2X, out float p2Y) {
			// Steigung der Geraden
			float dX = a2X - a1X;
			float dY = a2Y - a1Y;
			//
			p2X = pX - dY;
			p2Y = pY + dX;
		}


		/// <summary>
		/// Berechnet die Lotrechte durch den Punkt x1/y1 
		/// Das Ergebnis ist die Normalform der Geraden ax + by + c = 0
		/// </summary>
		[Obsolete("CAUTION: Arithmetic overflows with large coordinates, use version with two-point form instead.")]
		public static void CalcPerpendicularLine(int x1, int y1, int x2, int y2, out int a, out int b, out int c) {
			// Steigung
			a = x1 - x2;
			b = y1 - y2;
			// Konstante
			c = (int)((long)-a * x1 - (long)b * y1);
		}


		/// <summary>
		/// Berechnet die Lotrechte durch den Punkt x1/y1 
		/// Das Ergebnis ist die Normalform der Geraden ax + by + c = 0
		/// </summary>
		[Obsolete("CAUTION: Arithmetic overflows with large coordinates, use version with two-point form instead.")]
		public static void CalcPerpendicularLine(int x1, int y1, int x2, int y2, out int a, out int b, out long c) {
			// Steigung
			a = x1 - x2;
			b = y1 - y2;
			// Konstante
			c = ((long)-a * x1 - (long)b * y1);
		}


		/// <summary>
		/// Berechnet die Lotrechte durch den Punkt x1/y1 
		/// Das Ergebnis ist die Normalform der Geraden ax + by + c = 0
		/// </summary>
		[Obsolete]
		public static void CalcPerpendicularLine(float x1, float y1, float x2, float y2, out float a, out float b, out float c) {
			// Steigung
			a = x1 - x2;
			b = y1 - y2;
			// Konstante
			c = -a * x1 - b * y1;
		}


		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("CAUTION: Arithmetic overflows with large coordinates, use version with two-point form instead.")]
		public static void CalcPerpendicularLine(int x, int y, int a, int b, int c, out int aP, out int bP, out int cP) {
			aP = b;
			bP = -a;
			cP = (int)-((long)aP * x + (long)bP * y);
		}


		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("CAUTION: Arithmetic overflows with large coordinates, use version with two-point form instead.")]
		internal static void CalcPerpendicularLine(int x, int y, int a, int b, long c, out int aP, out int bP, out long cP) {
			aP = b;
			bP = -a;
			cP = -((long)aP * x + (long)bP * y);
		}


		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("CAUTION: Arithmetic overflows with large coordinates, use version with two-point form instead.")]
		public static void CalcPerpendicularLine(float x, float y, float a, float b, float c, out float aP, out float bP, out float cP) {
			aP = b;
			bP = -a;
			cP = -(aP * x + bP * y);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static void CalcPerpendicularBisector(int a1X, int a1Y, int a2X, int a2Y, out int b1X, out int b1Y, out int b2X, out int b2Y) {
			float dX = (a2X - a1X) / 2f;
			float dY = (a2Y - a1Y) / 2f;
			b1X = (int)Math.Round(a1X + dX - dY);
			b1Y = (int)Math.Round(a1Y + dX + dY);
			b2X = (int)Math.Round(a2X - dX + dY);
			b2Y = (int)Math.Round(a2Y - dX - dY);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static void CalcPerpendicularBisector(float a1X, float a1Y, float a2X, float a2Y, out float b1X, out float b1Y, out float b2X, out float b2Y) {
			float dX = (a2X - a1X) / 2f;
			float dY = (a2Y - a1Y) / 2f;
			b1X = a1X + dX - dY;
			b1Y = a1Y + dX + dY;
			b2X = a2X - dX + dY;
			b2Y = a2Y - dX - dY;
		}


		// Berechnet die Gerade (2D), welche im Mittelpunkt zwischen linePt1 und linePt2 senkrecht 
		// auf der Verbindungsstrecke steht. Das Ergebnis ist die Normalform der Geraden
		// ax + by + c = 0
		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("CAUTION: Arithmetic overflows with large coordinates, use version with two-point form instead")]
		public static void CalcPerpendicularBisector(int x1, int y1, int x2, int y2, out int a, out int b, out int c) {
			// Steigung
			a = x1 - x2;
			b = y1 - y2;
			// Konstante (Cast zu float wg Wertebereich und weil ohnehin durch 2f geteilt wird!)
			c = (int)Math.Round((-a * (float)(x1 + x2)) / 2f - (b * (float)(y1 + y2)) / 2f);
		}


		// Berechnet die Gerade (2D), welche im Mittelpunkt zwischen linePt1 und linePt2 senkrecht 
		// auf der Verbindungsstrecke steht. Das Ergebnis ist die Normalform der Geraden
		// ax + by + c = 0
		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("CAUTION: Arithmetic overflows with large coordinates, use version with two-point form instead")]
		public static void CalcPerpendicularBisector(int x1, int y1, int x2, int y2, out int a, out int b, out long c) {
			// Steigung
			a = x1 - x2;
			b = y1 - y2;
			// Konstante (Cast zu float wg Wertebereich und weil ohnehin durch 2f geteilt wird!)
			c = (long)Math.Round((-a * (float)(x1 + x2)) / 2f - (b * (float)(y1 + y2)) / 2f);
		}


		// Berechnet die Gerade (2D), welche im Mittelpunkt zwischen linePt1 und linePt2 senkrecht 
		// auf der Verbindungsstrecke steht. Das Ergebnis ist die Normalform der Geraden
		// ax + by + c = 0
		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("CAUTION: Arithmetic overflows with large coordinates, use version with two-point form instead.")]
		public static void CalcPerpendicularBisector(float x1, float y1, float x2, float y2, out float a, out float b, out float c) {
			// Steigung
			a = x1 - x2;
			b = y1 - y2;
			// Konstante
			c = (-a * (x1 + x2)) / 2f - (b * (y1 + y2)) / 2f;
		}


		/// <summary>
		/// Calculates the coordinates of the base point (fX, fY) of the perpendicular from point (pX, pY) 
		/// to line a1-a2.
		/// </summary>
		/// <remarks>The foot is the solution of equation (p - d) . (a - b) = 0</remarks>
		public static void CalcDroppedPerpendicularFoot(int pX, int pY, int a1X, int a1Y, int a2X, int a2Y, out int fX, out int fY) {
			if (a1X == a2X && a1Y == a2Y) {
				fX = a1X;
				fY = a1Y;
			} else {
				//// Calculate line formula parameters for the line a - b.
				//CalcLine(aX, aY, bX, bY, out int a1, out int b1, out long c1);
				//// Calculate perpendicular through p
				//int a2 = aX - bX;
				//int b2 = aY - bY;
				//long c2 = (long)bX * pX - (long)aX * pX + (long)bY * pY - (long)aY * pY;
				//// Now intersect the two lines
				//IntersectLines(a1, b1, c1, a2, b2, c2, out fX, out fY);

				// Calc perpendicular line through p
				CalcPerpendicularLine(a1X, a1Y, a2X, a2Y, pX, pY, out int toX, out int toY);
				// Intersect perpendicular line with line segment
				Point r = IntersectLines(pX, pY, toX, toY, a1X, a1Y, a2X, a2Y);
				fX = r.X;
				fY = r.Y;
			}
		}


		/// <summary>
		/// Calculates the coordinates of the base point (fX, fY) of the perpendicular from point (pX, pY) 
		/// to line a1-a2.
		/// </summary>
		/// <remarks>The foot is the solution of equation (p - d) . (a - b) = 0</remarks>
		public static void CalcDroppedPerpendicularFoot(float pX, float pY, float a1X, float a1Y, float a2X, float a2Y, out float fX, out float fY) {
			if (a1X == a2X && a1Y == a2Y) {
				fX = a1X;
				fY = a1Y;
			} else {
				// Calc perpendicular line through p
				CalcPerpendicularLine(a1X, a1Y, a2X, a2Y, pX, pY, out float toX, out float toY);
				// Intersect perpendicular line with line segment
				PointF r = IntersectLines(pX, pY, toX, toY, a1X, a1Y, a2X, a2Y);
				fX = r.X;
				fY = r.Y;
			}
		}


		/// <summary>
		/// Translates the given line in Hesse normal form to point p.
		/// </summary>
		[Obsolete("CAUTION: Arithmetic overflows with large coordinates.")]
		public static void TranslateLine(int a, int b, int c, Point p, out int aT, out int bT, out int cT) {
			aT = a; bT = b;
			cT = -(a * p.X + b * p.Y);
		}

		
		/// <summary>
		/// Translates the given line in Hesse normal form to point p.
		/// </summary>
		[Obsolete("CAUTION: Arithmetic overflows with large coordinates.")]
		public static void TranslateLine(int a, int b, long c, Point p, out int aT, out int bT, out long cT) {
			aT = a; bT = b;
			cT = -((long)a * p.X + (long)b * p.Y);
		}


		/// <summary>
		/// Translates the given line in Hesse normal form to point p.
		/// </summary>
		[Obsolete("CAUTION: Arithmetic overflows with large coordinates.")]
		public static void TranslateLine(float a, float b, float c, Point p, out float aT, out float bT, out float cT) {
			aT = a; bT = b;
			cT = -((a * p.X) + (b * p.Y));
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static IEnumerable<PointF> CalcCircleTangentThroughPoint(float centerX, float centerY, float radius, int ptX, int ptY) {
			float distance = (float)Math.Abs(DistancePointPoint(centerX, centerY, ptX, ptY));
			PointF p = VectorLinearInterpolation(centerX, centerY, ptX, ptY, 0.5f);
			return IntersectCircles(centerX, centerY, radius, p.X, p.Y, distance / 2f);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static IEnumerable<PointF> CalcArcTangentThroughPoint(float startPtX, float startPtY, float radiusPtX, float radiusPtY, float endPtX, float endPtY, int ptX, int ptY) {
			float radius;
			PointF center;
			CalcCircumCircle(startPtX, startPtY, radiusPtX, radiusPtY, endPtX, endPtY, out center, out radius);
			float distance = (float)Math.Abs(DistancePointPoint(center.X, center.Y, ptX, ptY));
			PointF pC = VectorLinearInterpolation(center.X, center.Y, ptX, ptY, 0.5f);
			foreach (PointF pT in IntersectCircles(center.X, center.Y, radius, pC.X, pC.Y, distance / 2f)) {
				if (ArcContainsPoint(startPtX, startPtY, radiusPtX, radiusPtY, endPtX, endPtY, center.X, center.Y, radius, pT.X, pT.Y, 0.1f))
					yield return pT;
				else {
					PointF result = PointF.Empty;
					float startPtDist = DistancePointPoint(pT.X, pT.Y, startPtX, startPtY);
					float endPtDist = DistancePointPoint(pT.X, pT.Y, endPtX, endPtY);
					if (startPtDist < endPtDist) {
						result.X = startPtX;
						result.Y = startPtY;
						yield return result;
					} else if (endPtDist < startPtDist) {
						result.X = endPtX;
						result.Y = endPtY;
						yield return result;
					} else {
						startPtDist = DistancePointLine(startPtX, startPtY, pT.X, pT.Y, ptX, ptY, true);
						endPtDist = DistancePointLine(endPtX, endPtY, pT.X, pT.Y, ptX, ptY, true);
						if (startPtDist < endPtDist) {
							result.X = startPtX;
							result.Y = startPtY;
							yield return result;
						} else if (endPtDist < startPtDist) {
							result.X = endPtX;
							result.Y = endPtY;
							yield return result;
						}
					}
				}
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static IEnumerable<Point> CalcArcTangentThroughPoint(int startPtX, int startPtY, int radiusPtX, int radiusPtY, int endPtX, int endPtY, int ptX, int ptY) {
			float radius;
			PointF center;
			CalcCircumCircle(startPtX, startPtY, radiusPtX, radiusPtY, endPtX, endPtY, out center, out radius);
			float distance = Math.Abs(DistancePointPoint(center.X, center.Y, ptX, ptY));
			PointF pC = VectorLinearInterpolation(center.X, center.Y, ptX, ptY, 0.5f);
			foreach (PointF pT in IntersectCircles(center.X, center.Y, radius, pC.X, pC.Y, distance / 2f)) {
				if (ArcContainsPoint(startPtX, startPtY, radiusPtX, radiusPtY, endPtX, endPtY, center.X, center.Y, radius, pT.X, pT.Y, 0.1f))
					yield return Point.Round(pT);
				else {
					Point result = Point.Empty;
					float startPtDist = DistancePointPoint(pT.X, pT.Y, startPtX, startPtY);
					float endPtDist = DistancePointPoint(pT.X, pT.Y, endPtX, endPtY);
					if (startPtDist < endPtDist) {
						result.X = startPtX;
						result.Y = startPtY;
						yield return result;
					} else if (endPtDist < startPtDist) {
						result.X = endPtX;
						result.Y = endPtY;
						yield return result;
					} else {
						startPtDist = DistancePointLine(startPtX, startPtY, pT.X, pT.Y, ptX, ptY, true);
						endPtDist = DistancePointLine(endPtX, endPtY, pT.X, pT.Y, ptX, ptY, true);
						if (startPtDist < endPtDist) {
							result.X = startPtX;
							result.Y = startPtY;
							yield return result;
						} else if (endPtDist < startPtDist) {
							result.X = endPtX;
							result.Y = endPtY;
							yield return result;
						}
					}
				}
			}
		}


		// Löst das lineare Gleichungssystem Ax + b = 0
		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("CAUTION: Arithmetic overflows with large coordinates.")]
		public static bool SolveLinear22System(int a11, int a12, int a21, int a22, int b1, int b2, out int x, out int y) {
			bool result = false;
			x = InvalidPoint.X; y = InvalidPoint.Y;
			long det = ((long)a11 * a22 - (long)a12 * a21);
			// if det == 0, there is no solution
			if (!Equals(det, 0)) {
				result = true;
				x = (int)Math.Round(((long)b2 * a12 - (long)b1 * a22) / (double)det);
				y = (int)Math.Round(((long)b1 * a21 - (long)b2 * a11) / (double)det);
			}
			return result;
		}


		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete]
		public static bool SolveLinear22System(float a11, float a12, float a21, float a22, float b1, float b2, out float x, out float y) {
			bool result = false;
			x = InvalidPointF.X; y = InvalidPointF.Y;
			// if det == 0, there is no solution
			float det = a11 * a22 - a12 * a21;
			if (!Equals(det, 0)) {
				result = true;
				x = (b2 * a12 - b1 * a22) / det;
				y = (b1 * a21 - b2 * a11) / det;
			}
			return result;
		}


		/// <summary>
		/// Returns the sign of the given value: -1, 0 (if value is 0) or +1
		/// </summary>
		public static sbyte Signum(int value) {
			return (value > 0) ? signumPositive : (value < 0) ? signumNegative : signumZero;
		}


		/// <summary>
		/// Returns the sign of the given value: -1, 0 (if value is 0) or +1
		/// </summary>
		public static sbyte Signum(long value) {
			return (value > 0) ? signumPositive : (value < 0) ? signumNegative : signumZero;
		}


		/// <summary>
		/// Returns the sign of the given value: -1, 0 (if value is 0) or +1
		/// </summary>
		public static sbyte Signum(float value) {
			return (value > 0) ? signumPositive : (value < 0) ? signumNegative : signumZero;
		}


		/// <summary>
		/// Returns the sign of the given value: -1, 0 (if value is 0) or +1
		/// </summary>
		public static sbyte Signum(double value) {
			return (value > 0) ? signumPositive : (value < 0) ? signumNegative : signumZero;
		}


		#region Functions to determine the orientation of points and lines

		/// <summary>
		/// Determines the quadrant within the coordinate system of point with respect to origin.
		/// </summary>
		// For symmetry and uniqueness, we include the right-sided axis into the respective quadrant.
		public static int CalcRelativeQuadrant(Point point, Point origin) {
			int result;
			// Not sure whether this special case is correct.
			if (point == origin)
				result = 1;
			else if (point.Y - origin.Y >= 0 && point.X - origin.X > 0)
				result = 1;
			else if (point.Y - origin.Y > 0 && point.X - origin.X <= 0)
				result = 2;
			else if (point.Y - origin.Y <= 0 && point.X - origin.X < 0)
				result = 3;
			else if (point.Y - origin.Y < 0 && point.X - origin.X >= 0)
				result = 4;
			else {
				Debug.Fail("Geometry.CalcRelativeQuadrant");
				result = 0;
			}
			return result;
		}

		#endregion


		#region Retrieving nearest / farest point and compare point distance methods

		/// <summary>
		/// Returns p1 or p2 dending on which one is nearer to p.
		/// </summary>
		public static Point GetNearestPoint(Point p, Point p1, Point p2) {
			return GetNearestPoint(p.X, p.Y, p1.X, p1.Y, p2.X, p2.Y);
		}


		/// <summary>
		/// Returns p1 or p2 dending on which one is nearer to p.
		/// </summary>
		public static Point GetNearestPoint(int pX, int pY, int p1X, int p1Y, int p2X, int p2Y) {
			Point result = InvalidPoint;
			//// VectorCrossProduct liefert leider manchmal abweichende Ergebnisse...
			//int d1 = Math.Abs(VectorCrossProduct(pX, pY, p1X, p1Y));
			//int d2 = Math.Abs(VectorCrossProduct(pX, pY, p2X, p2Y));

			float d1 = DistancePointPoint(pX, pY, p1X, p1Y);
			float d2 = DistancePointPoint(pX, pY, p2X, p2Y);
			if (d1 <= d2) {
				result.X = p1X;
				result.Y = p1Y;
			} else {
				result.X = p2X;
				result.Y = p2Y;
			}
			return result;
		}


		/// <summary>
		/// Returns the nearest point to p (if there is one) or null
		/// </summary>
		public static Point GetNearestPoint(Point p, IEnumerable<Point> points) {
			return GetNearestPoint(p.X, p.Y, points);
		}


		/// <summary>
		/// Returns the nearest point to x/y (if there is one) or null
		/// </summary>
		public static Point GetNearestPoint(int x, int y, IEnumerable<Point> points) {
			if (points == null) throw new ArgumentNullException(nameof(points));
			Point result = InvalidPoint;
			float d, lowest = float.MaxValue;
			foreach (Point point in points) {
				d = DistancePointPoint(x, y, point.X, point.Y);
				if (d < lowest) {
					lowest = d;
					result = point;
				}
			}
			return result;
		}


		/// <summary>
		/// Returns the nearest point to p (if there is one) or null
		/// </summary>
		public static PointF GetNearestPoint(PointF p, IEnumerable<PointF> points) {
			return GetNearestPoint(p.X, p.Y, points);
		}


		/// <summary>
		/// Returns the nearest point to x/y (if there is one) or null
		/// </summary>
		public static PointF GetNearestPoint(float x, float y, IEnumerable<PointF> points) {
			if (points == null) throw new ArgumentNullException(nameof(points));
			PointF result = InvalidPointF;
			float d, lowest = float.MaxValue;
			foreach (PointF point in points) {
				d = DistancePointPoint(x, y, point.X, point.Y);
				if (d < lowest) {
					lowest = d;
					result = point;
				}
			}
			return result;
		}


		/// <summary>
		/// Returns the furthest point from p (if one exists) or null.
		/// </summary>
		public static Point GetFurthestPoint(Point p, IEnumerable<Point> points) {
			if (points == null) throw new ArgumentNullException(nameof(points));
			Point result = InvalidPoint;
			float d, greatest = int.MinValue;
			foreach (Point point in points) {
				d = DistancePointPoint(p, point);
				if (d > greatest) {
					greatest = d;
					result = point;
				}
			}
			return result;
		}


		/// <summary>
		/// Returns the furthest point from p (if one exists) or null.
		/// </summary>
		public static PointF GetFurthestPoint(PointF p, IEnumerable<PointF> points) {
			if (points == null) throw new ArgumentNullException(nameof(points));
			PointF result = InvalidPointF;
			float d, greatest = int.MinValue;
			foreach (PointF point in points) {
				d = DistancePointPoint(p, point);
				if (d > greatest) {
					greatest = d;
					result = point;
				}
			}
			return result;
		}


		/// <summary>
		/// Returns Point p1 or Point p2 dpending on the distance to p.
		/// </summary>
		public static Point GetFurthestPoint(int pX, int pY, int p1X, int p1Y, int p2X, int p2Y) {
			Point result = Point.Empty;
			float d1 = DistancePointPoint(pX, pY, p1X, p1Y);
			float d2 = DistancePointPoint(pX, pY, p2X, p2Y);
			if (d1 >= d2) {
				result.X = p1X;
				result.Y = p1Y;
			} else {
				result.X = p2X;
				result.Y = p2Y;
			}
			return result;
		}


		/// <summary>
		/// Returns Point p1 or Point p2 dpending on the distance to p.
		/// </summary>
		public static Point GetFurthestPoint(Point p, Point p1, Point p2) {
			return GetFurthestPoint(p.X, p.Y, p1.X, p1.Y, p2.X, p2.Y);
		}


		/// <summary>
		/// Returns Point p1 or Point p2 dpending on the distance to p.
		/// </summary>
		public static PointF GetFurthestPoint(float pX, float pY, float p1X, float p1Y, float p2X, float p2Y) {
			PointF result = PointF.Empty;
			float d1 = DistancePointPoint(pX, pY, p1X, p1Y);
			float d2 = DistancePointPoint(pX, pY, p2X, p2Y);
			if (d1 >= d2) {
				result.X = p1X;
				result.Y = p1Y;
			} else {
				result.X = p2X;
				result.Y = p2Y;
			}
			return result;
		}


		/// <summary>
		/// Calculates the point on the line segment with the smallest absolute distance to p.
		/// </summary>
		/// <param name="aX"></param>
		/// <param name="aY"></param>
		/// <param name="bX"></param>
		/// <param name="bY"></param>
		/// <param name="pX"></param>
		/// <param name="pY"></param>
		/// <param name="nX"></param>
		/// <param name="nY"></param>
		public static void CalcNearestPointOfLineSegment(int aX, int aY, int bX, int bY, int pX, int pY, out int nX, out int nY) {
			// Caution! Does not work if the line goes from top right to bottom left and p is below and more left than the end point 'b'!
			//CalcDroppedPerpendicularFoot(pX, pY, aX, aY, bX, bY, out nX, out nY);
			//if (Math.Sign(nX - aX) != Math.Sign(bX - aX)) {
			//	// Point is on the other side of A
			//	nX = aX;
			//	nY = aY;
			//} else if (nX - aX >= bX - aX) {
			//	// Point is further away than B
			//	nX = bX;
			//	nY = bY;
			//}

			CalcPerpendicularLine(aX, aY, bX, bY, pX, pY, out int p2X, out int p2Y);
			Point nP = IntersectLines(pX, pY, p2X, p2Y, aX, aY, bX, bY);
			if (!LineContainsPoint(aX, aY, bX, bY, true, nP.X, nP.Y, 0.75f))
				nP = GetNearestPoint(nP.X, nP.Y, aX, aY, bX, bY);
			nX = nP.X;
			nY = nP.Y;
		}


		/// <summary>
		/// Returns Point p1 or Point p2 dpending on the distance to p.
		/// </summary>
		public static PointF GetFurthestPoint(PointF p, PointF p1, PointF p2) {
			return GetFurthestPoint(p.X, p.Y, p1.X, p1.Y, p2.X, p2.Y);
		}


		/// <summary>
		/// Compares the distances of originalPoint to p and comparedPoint to p.
		/// Returns true if comparedPoint is nearer to p than originalPoint
		/// </summary>
		public static bool IsNearer(PointF p, PointF originalPt, PointF comparedPt) {
			return DistancePointPoint(p, comparedPt) < DistancePointPoint(p, originalPt);
		}


		/// <summary>
		/// Compares the distances of originalPoint to p and comparedPoint to p.
		/// Returns true if comparedPoint is farer away from p than originalPoint
		/// </summary>
		public static bool IsFarther(PointF p, PointF originalPt, PointF comparedPt) {
			return DistancePointPoint(p, comparedPt) < DistancePointPoint(p, originalPt);
		}

		#endregion


		#region Check methods if a geometric struct has valid values


		/// <summary>
		/// Tests if the given point has valid coordinates.
		/// </summary>
		public static bool IsValid(Point p) {
			return IsValidCoordinate(p.X) && IsValidCoordinate(p.Y);
		}


		/// <summary>
		/// Tests if the given point has valid coordinates.
		/// </summary>
		public static bool IsValid(PointF p) {
			return IsValidCoordinate(p.X) && IsValidCoordinate(p.Y);
		}


		/// <summary>
		/// Tests if the given coordinates are considered as valid.
		/// </summary>
		public static bool IsValid(int x, int y) {
			return IsValidCoordinate(ref x) && IsValidCoordinate(ref y);
		}


		/// <summary>
		/// Tests if the given coordinates are considered as valid.
		/// </summary>
		public static bool IsValid(float x, float y) {
			return IsValidCoordinate(ref x) && IsValidCoordinate(ref y);
		}


		/// <summary>
		/// Tests if the given rectangle has valid coordinates and a valid size.
		/// </summary>
		public static bool IsValid(Rectangle r) {
			//return IsValidCoordinate(r.X) && IsValidCoordinate(r.Y) && IsValidSize(r.Width) && IsValidSize(r.Height);
			return IsValid(r.Location) && IsValid(r.Size);
		}


		/// <summary>
		/// Tests if the given rectangle has valid coordinates and a valid size.
		/// </summary>
		public static bool IsValid(RectangleF r) {
			//return IsValidCoordinate(r.X) && IsValidCoordinate(r.Y) && IsValidSize(r.Width) && IsValidSize(r.Height);
			return IsValid(r.Location) && IsValid(r.Size);
		}


		/// <summary>
		/// Tests if the given rectangle has valid coordinates and a valid size.
		/// </summary>
		public static bool IsValid(int x, int y, int width, int height) {
			return IsValidCoordinate(ref x) && IsValidCoordinate(ref y) && IsValidSize(ref width) && IsValidSize(ref height);
		}


		/// <summary>
		/// Tests if the given rectangle has valid coordinates and a valid size.
		/// </summary>
		public static bool IsValid(float x, float y, float width, float height) {
			return IsValidCoordinate(ref x) && IsValidCoordinate(ref y) && IsValidSize(ref width) && IsValidSize(ref height);
		}


		/// <summary>
		/// Tests if the given size is considered as valid.
		/// </summary>
		public static bool IsValid(Size s) {
			return IsValidSize(s.Width) && IsValidSize(s.Height);
		}


		/// <summary>
		/// Tests if the given size is considered as valid.
		/// </summary>
		public static bool IsValid(SizeF s) {
			return IsValidSize(s.Width) && IsValidSize(s.Height);
		}

		#endregion


		#region Check funktions (debug mode only)

		/// <summary>
		/// Tests if the given point has valid coordinates.
		/// </summary>
		[Conditional("DEBUG")]
		public static void AssertIsValid(Point p) {
			if (!IsValidCoordinate(p.X) || !IsValidCoordinate(p.Y))
				throw new ArgumentException(string.Format("{0} is not a valid point.", p));
		}


		/// <summary>
		/// Tests if the given point has valid coordinates.
		/// </summary>
		[Conditional("DEBUG")]
		public static void AssertIsValid(PointF p) {
			if (!IsValidCoordinate(p.X) || !IsValidCoordinate(p.Y))
				throw new ArgumentException(string.Format("{0} is not a valid point.", p));
		}


		/// <summary>
		/// Tests if the given coordinates are considered as valid.
		/// </summary>
		[Conditional("DEBUG")]
		public static void AssertIsValid(int x, int y) {
			if (!IsValidCoordinate(x)) throw new ArgumentException(string.Format("{0} is not a valid coordinate.", x));
			if (!IsValidCoordinate(y)) throw new ArgumentException(string.Format("{0} is not a valid coordinate.", y));
		}


		/// <summary>
		/// Tests if the given coordinates are considered as valid.
		/// </summary>
		[Conditional("DEBUG")]
		public static void AssertIsValid(float x, float y) {
			if (!IsValidCoordinate(x)) throw new ArgumentException(string.Format("{0} is not a valid coordinate.", x));
			if (!IsValidCoordinate(y)) throw new ArgumentException(string.Format("{0} is not a valid coordinate.", y));
		}


		/// <summary>
		/// Tests if the given rectangle has valid coordinates and a valid size.
		/// </summary>
		[Conditional("DEBUG")]
		public static void AssertIsValid(Rectangle r) {
			if (!IsValidCoordinate(r.X) || !IsValidCoordinate(r.Y) || !IsValidSize(r.Width) || !IsValidSize(r.Height))
				throw new ArgumentException(string.Format("{0} is not a valid rectangle.", r));
		}


		/// <summary>
		/// Tests if the given rectangle has valid coordinates and a valid size.
		/// </summary>
		[Conditional("DEBUG")]
		public static void AssertIsValid(RectangleF r) {
			if (!IsValidCoordinate(r.X) || !IsValidCoordinate(r.Y) || !IsValidSize(r.Width) || !IsValidSize(r.Height))
				throw new ArgumentException(string.Format("{0} is not a valid rectangle.", r));
		}


		/// <summary>
		/// Tests if the given rectangle has valid coordinates and a valid size.
		/// </summary>
		[Conditional("DEBUG")]
		public static void AssertIsValid(int x, int y, int width, int height) {
			if (!IsValidCoordinate(x) || !IsValidCoordinate(y) || !IsValidSize(width) || !IsValidSize(height))
				throw new ArgumentException(string.Format("{0} is not a valid rectangle.", new Rectangle(x, y, width, height)));
		}


		/// <summary>
		/// Tests if the given rectangle has valid coordinates and a valid size.
		/// </summary>
		[Conditional("DEBUG")]
		public static void AssertIsValid(float x, float y, float width, float height) {
			if (!IsValidCoordinate(x) || !IsValidCoordinate(y) || !IsValidSize(width) || !IsValidSize(height))
				throw new ArgumentException(string.Format("{0} is not a valid rectangle.", new RectangleF(x, y, width, height)));
		}


		/// <summary>
		/// Tests if the given rectangle has valid coordinates and a valid size.
		/// </summary>
		[Conditional("DEBUG")]
		public static void AssertIsValidLTRB(int left, int top, int right, int bottom) {
			if (left == InvalidRectangle.Left && top == InvalidRectangle.Top && right == InvalidRectangle.Right && bottom == InvalidRectangle.Bottom)
				throw new ArgumentException(string.Format("{0} is not a valid rectangle.", Rectangle.FromLTRB(left, top, right, bottom)));
		}


		/// <summary>
		/// Tests if the given rectangle has valid coordinates and a valid size.
		/// </summary>
		[Conditional("DEBUG")]
		public static void AssertIsValidLTRB(float left, float top, float right, float bottom) {
			if (left == InvalidRectangleF.Left && top == InvalidRectangleF.Top && right == InvalidRectangleF.Right && bottom == InvalidRectangleF.Bottom)
				throw new ArgumentException(string.Format("{0} is not a valid rectangle.", RectangleF.FromLTRB(left, top, right, bottom)));
		}


		/// <summary>
		/// Tests if the given size is considered as valid.
		/// </summary>
		[Conditional("DEBUG")]
		public static void AssertIsValid(Size s) {
			if (!IsValidSize(s.Width) || !IsValidSize(s.Height))
				throw new ArgumentException(string.Format("{0} is not a valid size.", s));
		}


		/// <summary>
		/// Tests if the given size is considered as valid.
		/// </summary>
		[Conditional("DEBUG")]
		public static void AssertIsValid(SizeF s) {
			if (!IsValidSize(s.Width) || !IsValidSize(s.Height))
				throw new ArgumentException(string.Format("{0} is not a valid size.", s));
		}

		#endregion


		#region Definitions for invalid geometric structs

		/// <summary>Defines a coordinate value considered as invalid.</summary>
		public const int InvalidCoordinateValue = int.MinValue;

		/// <summary>Defines a coordinate value considered as invalid.</summary>
		public const long InvalidCoordinateValueL = long.MinValue;

		/// <summary>Defines a coordinate value considered as invalid.</summary>
		public const int InvalidSizeValue = int.MinValue;

		/// <summary>Defines a point considered as invalid.</summary>
		public static readonly Point InvalidPoint;

		/// <summary>Defines a point considered as invalid.</summary>
		public static readonly PointF InvalidPointF;

		/// <summary>Defines a size considered as invalid.</summary>
		public static readonly Size InvalidSize;

		/// <summary>Defines a size considered as invalid.</summary>
		public static readonly SizeF InvalidSizeF;

		/// <summary>Defines a rectangle considered as invalid.</summary>
		public static readonly Rectangle InvalidRectangle;

		/// <summary>Defines a rectangle considered as invalid.</summary>
		public static readonly RectangleF InvalidRectangleF;

		#endregion


		#region Methods (private)

		static Geometry() {
			InvalidPoint.X =
			InvalidPoint.Y = InvalidCoordinateValue;

			InvalidPointF.X =
			InvalidPointF.Y = InvalidCoordinateValue;

			InvalidSize.Width =
			InvalidSize.Height = InvalidSizeValue;

			InvalidSizeF.Width =
			InvalidSizeF.Height = InvalidSizeValue;

			InvalidRectangle.Location = Point.Empty;
			InvalidRectangle.Width = -1;
			InvalidRectangle.Height = -1;

			InvalidRectangleF.Location = PointF.Empty;
			InvalidRectangleF.Width = -1;
			InvalidRectangleF.Height = -1;
		}


		/// <summary>
		/// Tests if the given coordinate is considered as valid.
		/// </summary>
		public static bool IsValidCoordinate(int c) {
			return c > InvalidCoordinateValue;
		}


		/// <summary>
		/// Tests if the given coordinate is considered as valid.
		/// </summary>
		public static bool IsValidCoordinate(float c) {
			return !float.IsNaN(c) && !float.IsInfinity(c) && c > InvalidCoordinateValue;
		}


		/// <summary>
		/// Tests if the given size is considered as valid.
		/// </summary>
		public static bool IsValidSize(int s) {
			return s >= 0;
		}


		/// <summary>
		/// Tests if the given size is considered as valid.
		/// </summary>
		public static bool IsValidSize(float s) {
			return !float.IsNaN(s) && !float.IsInfinity(s) && s >= 0;
		}


		/// <summary>
		/// Tests if the given coordinate is considered as valid.
		/// Parameter will not be modified, ref is for improved performance only.
		/// </summary>
		public static bool IsValidCoordinate(ref int c) {
			return c > InvalidCoordinateValue;
		}


		/// <summary>
		/// Tests if the given coordinate is considered as valid.
		/// Parameter will not be modified, ref is for improved performance only.
		/// </summary>
		private static bool IsValidCoordinate(ref float c) {
			return !float.IsNaN(c) && !float.IsInfinity(c) && c > InvalidCoordinateValue;
		}


		/// <summary>
		/// Tests if the given size is considered as valid.
		/// Parameter will not be modified, ref is for improved performance only.
		/// </summary>
		private static bool IsValidSize(ref int s) {
			return s >= 0;
		}


		/// <summary>
		/// Tests if the given size is considered as valid.
		/// Parameter will not be modified, ref is for improved performance only.
		/// </summary>
		private static bool IsValidSize(ref float s) {
			return !float.IsNaN(s) && !float.IsInfinity(s) && s >= 0;
		}


		/// <summary>
		/// Calculates the greatest common factor with the algorithm of Euklid.
		/// </summary>
		private static int CalcGreatestCommonFactor(int valueA, int valueB) {
			int result = 1;
			int rem;
			int a = Math.Max(valueA, valueB);
			int b = Math.Min(valueA, valueB);
			do {
				rem = a % b;
				if (rem != 0) b = rem;
			} while (rem != 0);
			Debug.Assert(valueA % b == 0 && valueB % b == 0);
			result = b;
			return result;
		}


		private static void GetMinValues(int width, int height, ResizeModifiers modifiers, out int minWidth, out int minHeight) {
			if ((modifiers & ResizeModifiers.MaintainAspect) != 0) {
				//if (width == height || width == 0 || height == 0) {
				//    minWidth = minHeight = 1;
				//} else {
				//    int gcf = CalcGreatestCommonFactor(width, height);
				//    minHeight = height / gcf;
				//    minWidth = width / gcf;
				//}

				// A simpler and less acurate but much faster alternative:
				float aspect = width / (float)height;
				minHeight = 1;
				minWidth = (int)Math.Round(1 * aspect);
			} else minWidth = minHeight = 0;
		}

		#endregion


		private static Matrix matrix = new Matrix();

		private const double RadiansFactor = 0.017453292519943295769236907684886d;  // = Math.PI / 180
		private const double EqualityDeltaDouble = 0.000001d;
		private const float EqualityDeltaFloat = 0.000001f;
		private const sbyte signumNegative = -1;
		private const sbyte signumPositive = 1;
		private const sbyte signumZero = 0;
	}

}
