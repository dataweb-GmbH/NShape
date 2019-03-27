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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;


namespace Dataweb.NShape.Advanced {

	/// <summary>
	/// Helper methods for calculating shapes
	/// </summary>
	public static class ShapeUtils {

		/// <summary>
		/// Calculates the cell for a shape map (spacial index) from the given coordinates.
		/// </summary>
		public static Point CalcCell(Point pos, int cellSize) {
			return CalcCell(pos.X, pos.Y, cellSize);
		}


		/// <summary>
		/// Calculates the cell for a shape map (spacial index) from the given coordinates.
		/// </summary>
		public static Point CalcCell(int x, int y, int cellSize) {
			// Optimization:
			// Use integer division for values >= 0 (>20 times faster than floored float divisions)
			// Use integer division and subtract 1 for values < 0 (otherwise calculating intersection with cell 
			// bounds will not work as expected as 0.5f / 1 is 0 instead of the expected result 1)
			Point cell = Point.Empty;
			cell.Offset(
				(x >= 0) ? (x / cellSize) : (x / cellSize) - 1,
				(y >= 0) ? (y / cellSize) : (y / cellSize) - 1
			);
			return cell;
		}


		/// <summary>
		/// Calculates the cell for a shape map (spacial index) from the given coordinates.
		/// </summary>
		public static void CalcCell(int x, int y, int cellSize, out int cellX, out int cellY) {
			// Optimization:
			// Use integer division for values >= 0 (>20 times faster than floored float divisions)
			// Use integer division and subtract 1 for values < 0 (otherwise calculating intersection with cell 
			// bounds will not work as expected as 0.5f / 1 is 0 instead of the expected result 1)
			cellX = (x >= 0) ? (x / cellSize) : (x / cellSize) - 1;
			cellY = (y >= 0) ? (y / cellSize) : (y / cellSize) - 1;
		}


		/// <summary>
		/// Inflates the bounding rectangle by the width of the given line style.
		/// </summary>
		public static void InflateBoundingRectangle(ref Rectangle boundingRectangle, ILineStyle lineStyle) {
			if (lineStyle == null) throw new ArgumentNullException("lineStyle");
			Geometry.AssertIsValid(boundingRectangle);
			if (lineStyle.LineWidth > 2) {
				int halfLineWidth = (int)Math.Ceiling(lineStyle.LineWidth / 2f) - 1;
				boundingRectangle.Inflate(halfLineWidth, halfLineWidth);
			}
		}



		#region Drawing Lines with LineCaps

		private static PointF CalcCapIntersectionWithLine(Point[] points, Pen pen, bool isStartCap) {
			int idx;
			return CalcCapIntersectionWithLine(points, pen, isStartCap, out idx);
		}


		private static PointF CalcCapIntersectionWithLine(Point[] points, Pen pen, bool isStartCap, out int destIdx) {
			int startIdx, step;
			float capInset = 0;
			if (isStartCap) {
				startIdx = 0;
				step = 1;
				capInset = GetLineCapInset(pen, true);
			} else {
				startIdx = points.Length - 1;
				step = -1;
				capInset = GetLineCapInset(pen, false);
			}

			// Store point where the line cap is located
			Point capPoint = points[startIdx];
			int maxIdx = points.Length - 1;
			destIdx = startIdx + step;
			// Find first point *outside* the cap's area
			while (Geometry.DistancePointPoint(capPoint, points[destIdx]) < capInset && (destIdx > 0 && destIdx < maxIdx)) {
				startIdx = destIdx;
				destIdx += step;
			}
			// Calculate intersection point between cap and line
			return Geometry.IntersectCircleWithLine(capPoint, capInset, points[startIdx], points[destIdx], true);
		}

	
		/// <summary>
		/// Calculates the angle of the line cap (this is usually the angle between the line's end vertex the its predecessor).
		/// If the predecessor of the line's end vertex is inside the line cap, the intersection point between the line cap and
		/// the rest of the line is calculated, like GDI+ does.
		/// </summary>
		public static float CalcLineCapAngle(Point[] points, ControlPointId pointId, Pen pen) {
			if (points == null) throw new ArgumentNullException("points");
			if (points.Length < 2) throw new ArgumentException("Parameter points must have at least 2 elements.");
			if (pointId != ControlPointId.FirstVertex && pointId != ControlPointId.LastVertex) throw new ArgumentException("pointId");
			float result = float.NaN;

			// Get required infos: Size of the cap, start point and point processing direction
			int startIdx;
			if (pointId == ControlPointId.FirstVertex)
				startIdx = 0;
			else if (pointId == ControlPointId.LastVertex)
				startIdx = points.Length - 1;
			else throw new NotSupportedException();

			// Calculate cap angle
			int lastIdx = points.Length - 1;
			Point origStartPoint = points[0];
			Point origEndPoint = points[lastIdx];
			try {
				Point safeStartPoint, safeEndPoint;
				if (LineHasInvalidCapIntersection(points, pen, out safeStartPoint, out safeEndPoint)) {
					points[0] = safeStartPoint;
					points[lastIdx] = safeEndPoint;
				}
				// Now calculate the cap angle
				int destIdx;
				PointF p = CalcCapIntersectionWithLine(points, pen, (pointId == ControlPointId.FirstVertex), out destIdx);
				if (Geometry.IsValid(p)) // && Geometry.LineContainsPoint(points[startIdx], points[destIdx], false, p.X, p.Y, 1))
					result = Geometry.RadiansToDegrees(Geometry.Angle(points[startIdx], p));
				if (float.IsNaN(result))
					result = Geometry.RadiansToDegrees(Geometry.Angle(points[startIdx], points[destIdx]));
			} finally {
				points[0] = origStartPoint;
				points[lastIdx] = origEndPoint;
			}
			Debug.Assert(!float.IsNaN(result));
			return result;
		}


		/// <summary>
		/// Draws a multi segment line including its caps.
		/// </summary>
		/// <remarks>
		/// This method avoids OutOfMemoryExceptions being thrown in case of intersecting line caps.
		/// </remarks>
		public static void DrawLinesSafe(Graphics gfx, Pen pen, Point[] shapePoints) {
			if (gfx == null) throw new ArgumentNullException("gfx");
			if (pen == null) throw new ArgumentNullException("pen");
			if (shapePoints == null) throw new ArgumentNullException("shapePoints");
			if (shapePoints.Length < 2) throw new ArgumentException("Not enough points for a line.");
			try {
				// GDI+ behaviour:
				// If the two end caps of a line intersect within the range of the insets and the whole 
				// line is covered by caps, GDI+ seems to have trouble finding the intersection point 
				// and throws an out of memory exception.
				// Workaround:
				// We detect that condition and move one endpoint of the line farther away such that 
				// the caps do not intersect anymore.
				int lastIdx = shapePoints.Length - 1;
				Point startPoint = shapePoints[0];
				Point endPoint = shapePoints[lastIdx];
				try {
					Point safeStartPoint, safeEndPoint;
					if (LineHasInvalidCapIntersection(shapePoints, pen, out safeStartPoint, out safeEndPoint)) {
						shapePoints[0] = safeStartPoint;
						shapePoints[lastIdx] = safeEndPoint;
					}
					gfx.DrawLines(pen, shapePoints);
				} finally {
					shapePoints[0] = startPoint;
					shapePoints[lastIdx] = endPoint;
				}
			} catch (OutOfMemoryException) {
				throw;
			}
		}


		/// <summary>
		/// Detects whether drawing the line would cause an OutOfMemoryException and therefore applying 
		/// the workaround for intersecting line caps is necessary or not.
		/// </summary>
		public static bool LineHasInvalidCapIntersection(Point[] shapePoints, Pen pen) {
			// GDI+ behaviour:
			// Line caps will be rotated to the intersection point between the line and its cap.
			// As long as line points are within the line cap's inset, these points will be ignored.
			// If the start cap and the end cap intersect each other, the intersection point between the line 
			// and the cap is no longer unique and therefore GDI+ will throw an OutOfMemoryException.
			float startCapInset = GetLineCapInset(pen, true);
			float endCapInset = GetLineCapInset(pen, false);
			if (startCapInset == 0 && endCapInset == 0)
				return false;

			bool result = true;
			if (shapePoints.Length == 2) {
				const float delta = 0.1f;
				result = (Math.Abs((startCapInset + endCapInset) - Geometry.DistancePointPoint(shapePoints[0], shapePoints[1])) <= delta);
			} else {
				// Due to the behaviour described above, we have to calculate the intersection point between the cap
				// and the line for each cap and check whether both intersection points are inside the opposite cap.
				int firstIdxOutsideStartCap, firstIdxOutsideEndCap;
				PointF startCapIntersectionPoint = CalcCapIntersectionWithLine(shapePoints, pen, true, out firstIdxOutsideStartCap);
				PointF endCapIntersectionPoint = CalcCapIntersectionWithLine(shapePoints, pen, false, out firstIdxOutsideEndCap); 
				// 
				int lastIdx = shapePoints.Length-1;
				result = (Geometry.DistancePointPoint(shapePoints[0], endCapIntersectionPoint) <= startCapInset
					&& Geometry.DistancePointPoint(shapePoints[lastIdx], startCapIntersectionPoint) <= endCapInset);
			}
			return result;
		}


		/// <summary>
		/// Detects whether drawing the line would cause an OutOfMemoryException and therefore applying 
		/// the workaround for intersecting line caps is necessary or not.
		/// </summary>
		public static bool LineHasInvalidCapIntersection(Point[] shapePoints, Pen pen, out Point safeStartPoint, out Point safeEndPoint) {
			int maxIdx = shapePoints.Length - 1;
			Point startPoint = shapePoints[0]; 
			Point endPoint = shapePoints[maxIdx];
			if (LineHasInvalidCapIntersection(shapePoints, pen)) {
				// Move the end point farther away
				float startCapInset = GetLineCapInset(pen, true);
				float endCapInset = GetLineCapInset(pen, false);
				float distanceStartEnd = Geometry.DistancePointPoint(startPoint, endPoint);
				float offset = (startCapInset + endCapInset) - distanceStartEnd;
				float angle = Geometry.RadiansToDegrees(Geometry.Angle(startPoint, endPoint));

				int delta = 1;
				int halfOffset = (int)Math.Ceiling(offset / 2f);
				safeStartPoint = Geometry.RotatePoint(startPoint, angle - 180, new Point(startPoint.X + halfOffset, startPoint.Y));
				safeEndPoint = Geometry.RotatePoint(endPoint, angle, new Point(endPoint.X + delta + halfOffset, endPoint.Y));
				return true;
			} else {
				safeStartPoint = startPoint;
				safeEndPoint = endPoint;
				return false;
			}
		}


		/// <summary>
		/// Returns the BaseInset of the pen's specified line cap.
		/// Returns 0 if the line has no such cap.
		/// </summary>
		public static float GetLineCapInset(Pen pen, bool isStartCap) {
			float result = 0;
			if (isStartCap) {
				if (pen.StartCap == LineCap.Custom) {
					Debug.Assert(pen.CustomStartCap != null);
					result = pen.Width * pen.CustomStartCap.BaseInset;
				}
			} else {
				if (pen.EndCap == LineCap.Custom) {
					Debug.Assert(pen.CustomEndCap != null);
					result = pen.Width * pen.CustomEndCap.BaseInset;
				}
			}
			return result;
		}

		#endregion

	}

}
