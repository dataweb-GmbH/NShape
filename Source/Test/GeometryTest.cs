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
using System.Diagnostics;
using System.Drawing;
using Dataweb.NShape.Advanced;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace NShapeTest {

	[TestClass]
	public class GeometryTests : NShapeTestBase {

		/// <summary>
		/// Factors for multiplying coordinates with (for testing coordinate ranges)
		/// </summary>
		public static readonly int[] DiagramSizeFactors = new int[] { 
			// Default range of diagram coordinates
			1, 10, 100, 1000, 2500, 
			// Extended range of diagram coordinates, arithmetic overflows can occur
			5000,
			10000, 
			50000, 
			100000,
			//// Extreme range of diagram coordinates, arithmetic overflows can occur
			//500000,
			//1000000,
			//5000000,
			//10000000,
			//50000000,
		};


		[TestMethod]
		public void CalcDroppedPerpendicularFootTest() {
			int fX, fY;
			Point p1 = Point.Empty, p2 = Point.Empty, p3 = Point.Empty;
			foreach (int sizeFactor in DiagramSizeFactors) {
				//
				// Coordinates would exceed targeted range of -1.000.000 to 1.000.000 with size factors > 50.000
				if (sizeFactor <= 50000) {
					p1.X = 0 * sizeFactor; p1.Y = 18 * sizeFactor;
					p2.X = 5 * sizeFactor; p2.Y = 0 * sizeFactor;
					p3.X = 12 * sizeFactor; p3.Y = 0 * sizeFactor;
					Geometry.CalcDroppedPerpendicularFoot(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, out fX, out fY);
					Assert.AreEqual(0, fX);
					Assert.AreEqual(0, fY);
					//
					p1.X = -2 * sizeFactor; p1.Y = -2 * sizeFactor;
					p2.X = 4 * sizeFactor; p2.Y = -17 * sizeFactor;
					p3.X = 4 * sizeFactor; p3.Y = -16 * sizeFactor;
					Geometry.CalcDroppedPerpendicularFoot(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, out fX, out fY);
					Assert.AreEqual(4 * sizeFactor, fX);
					Assert.AreEqual(-2 * sizeFactor, fY);
				}
				//
				p1.X = 8 * sizeFactor; p1.Y = 2 * sizeFactor;
				p2.X = -5 * sizeFactor; p2.Y = 1 * sizeFactor;
				p3.X = 1 * sizeFactor; p3.Y = 7 * sizeFactor;
				Geometry.CalcDroppedPerpendicularFoot(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, out fX, out fY);
				Assert.AreEqual(2 * sizeFactor, fX);
				Assert.AreEqual(8 * sizeFactor, fY);
				//
				// Special 1: Line is parallel to X- or Y-axis
				p1.X = 3 * sizeFactor; p1.Y = -3 * sizeFactor;
				p2.X = -4 * sizeFactor; p2.Y = -4 * sizeFactor;
				p3.X = -4 * sizeFactor; p3.Y = 4 * sizeFactor;
				Geometry.CalcDroppedPerpendicularFoot(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, out fX, out fY);
				Assert.AreEqual(-4 * sizeFactor, fX);
				Assert.AreEqual(-3 * sizeFactor, fY);
				//
				p1.X = -3 * sizeFactor; p1.Y = -3 * sizeFactor;
				p2.X = -4 * sizeFactor; p2.Y = 4 * sizeFactor;
				p3.X = 4 * sizeFactor; p3.Y = 4 * sizeFactor;
				Geometry.CalcDroppedPerpendicularFoot(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, out fX, out fY);
				Assert.AreEqual(-3 * sizeFactor, fX);
				Assert.AreEqual(4 * sizeFactor, fY);

				Trace.WriteLine($"SizeFactor {sizeFactor}: OK");
			}
		}


		[TestMethod]
		public void CalcNormalVectorOfLineTest() {
			int pX, pY;
			foreach (int sizeFactor in DiagramSizeFactors) {
				//
				Geometry.CalcNormalVectorOfLine(-2, -4, 2, -4, 0, -4, 100, out pX, out pY);
				Assert.AreEqual(0, pX);
				Assert.AreEqual(-104, pY);
				//
				// Coordinates would exceed targeted range of -1.000.000 to 1.000.000 with size factors > 50.000
				if (sizeFactor <= 50000) {
					Geometry.CalcNormalVectorOfLine(-5, -12, -5, -14, -5, 100, 100, out pX, out pY);
					Assert.AreEqual(-105, pX);
					Assert.AreEqual(100, pY);
				}
				//
				Geometry.CalcNormalVectorOfLine(0, 8, 6, 0, 3, 4, 100, out pX, out pY);
				Assert.AreEqual(-77, pX);
				Assert.AreEqual(-56, pY);

				Trace.WriteLine($"SizeFactor {sizeFactor}: OK");
			}
		}


		[TestMethod]
		public void CalcNearestPointOfLineSegmentTest() {
			int x, y;
			//
			Geometry.CalcNearestPointOfLineSegment(-4, 0, +4, 0, 0, 3, out x, out y);
			Assert.AreEqual(x, 0);
			Assert.AreEqual(y, 0);
			//
			Geometry.CalcNearestPointOfLineSegment(-4, 0, +4, 0, -8, 3, out x, out y);
			Assert.AreEqual(x, -4);
			Assert.AreEqual(y, 0);
			//
			Geometry.CalcNearestPointOfLineSegment(-4, 0, +4, 0, 8, 3, out x, out y);
			Assert.AreEqual(x, 4);
			Assert.AreEqual(y, 0);
			//
			Geometry.CalcNearestPointOfLineSegment(1, 1, 7, 7, 2, 8, out x, out y);
			Assert.AreEqual(x, 5);
			Assert.AreEqual(y, 5);
			//
			Geometry.CalcNearestPointOfLineSegment(1, 1, 7, 7, 2, 13, out x, out y);
			Assert.AreEqual(x, 7);
			Assert.AreEqual(y, 7);
			//
			Geometry.CalcNearestPointOfLineSegment(1, 1, 7, 7, 2, -1, out x, out y);
			Assert.AreEqual(x, 1);
			Assert.AreEqual(y, 1);
			//
		}


		[TestMethod]
		public void CalcNormalVectorOfRectangleTest() {
			System.Drawing.Point p;

			// Test each side (from point on the outline)
			// Left
			p = Geometry.CalcNormalVectorOfRectangle(-2, -4, 20, 10, -2, 0, 100);
			Assert.AreEqual(-102, p.X);
			Assert.AreEqual(0, p.Y);
			// Top
			p = Geometry.CalcNormalVectorOfRectangle(-2, -4, 20, 10, 0, -4, 100);
			Assert.AreEqual(0, p.X);
			Assert.AreEqual(-104, p.Y);
			// Right
			p = Geometry.CalcNormalVectorOfRectangle(-2, -4, 20, 10, 16, 0, 100);
			Assert.AreEqual(118, p.X);
			Assert.AreEqual(0, p.Y);
			// Bottom
			p = Geometry.CalcNormalVectorOfRectangle(-2, -4, 20, 10, 0, 6, 100);
			Assert.AreEqual(0, p.X);
			Assert.AreEqual(106, p.Y);

			// Test each side (from point inside)
			// Left
			p = Geometry.CalcNormalVectorOfRectangle(-2, -4, 20, 10, 0, 0, 100);
			Assert.AreEqual(-102, p.X);
			Assert.AreEqual(0, p.Y);
			// Top
			p = Geometry.CalcNormalVectorOfRectangle(-2, -4, 20, 10, 0, -3, 100);
			Assert.AreEqual(0, p.X);
			Assert.AreEqual(-104, p.Y);
			// Right
			p = Geometry.CalcNormalVectorOfRectangle(-2, -4, 20, 10, 15, 0, 100);
			Assert.AreEqual(118, p.X);
			Assert.AreEqual(0, p.Y);
			// Bottom
			p = Geometry.CalcNormalVectorOfRectangle(-2, -4, 20, 10, 0, 5, 100);
			Assert.AreEqual(0, p.X);
			Assert.AreEqual(106, p.Y);

			// Test corners (from outline)
			// Top Left
			p = Geometry.CalcNormalVectorOfRectangle(-2, -4, 20, 10, -2, -4, 100);
			Assert.AreEqual(-2, p.X);
			Assert.AreEqual(-104, p.Y);
			// Top Right
			p = Geometry.CalcNormalVectorOfRectangle(-2, -4, 20, 10, 18, -4, 100);
			Assert.AreEqual(18, p.X);
			Assert.AreEqual(-104, p.Y);
			// Bottom Left
			p = Geometry.CalcNormalVectorOfRectangle(-2, -4, 20, 10, -2, 6, 100);
			Assert.AreEqual(-2, p.X);
			Assert.AreEqual(106, p.Y);
			// Bottom Right
			p = Geometry.CalcNormalVectorOfRectangle(-2, -4, 20, 10, 18, 6, 100);
			Assert.AreEqual(18, p.X);
			Assert.AreEqual(106, p.Y);
		}


		[TestMethod]
		public void DistancePointLineTest() {
			float d;

			d = Geometry.DistancePointLine(0, +3, -4, 0, +4, 0, false);
			Assert.AreEqual(d, 3.0, 0.01);
			//
			d = Geometry.DistancePointLine(0, +3, -4, 0, +4, 0, true);
			Assert.AreEqual(d, 3.0, 0.01);

			d = Geometry.DistancePointLine(-8, +3, -4, 0, +4, 0, false);
			Assert.AreEqual(d, 3.0, 0.01);
			//
			d = Geometry.DistancePointLine(-8, +3, -4, 0, +4, 0, true);
			Assert.AreEqual(d, 5.0, 0.01);
		}


		[TestMethod]
		public void CalcLineTest() {
#pragma warning disable 0618
			Point p1 = Point.Empty, p2 = Point.Empty;
			int a, b, c;
			foreach (int sizeFactor in DiagramSizeFactors) {
				// CalcLine does not work with big coordinate values
				if (sizeFactor > 2500) break;

				p1.X = -1 * sizeFactor; p1.Y = -1 * sizeFactor;
				p2.X = 1 * sizeFactor; p2.Y = 1 * sizeFactor;
				Geometry.CalcLine(p1.X, p1.Y, p2.X, p2.Y, out a, out b, out c);
				Assert.AreEqual(-2 * sizeFactor, a);
				Assert.AreEqual(2 * sizeFactor, b);
				Assert.AreEqual(0, c);
				//
				// Big values for a and b
				p1.X = 10 * sizeFactor; p1.Y = 10 * sizeFactor;
				p2.X = -10 * sizeFactor; p2.Y = -10 * sizeFactor;
				Geometry.CalcLine(p1.X, p1.Y, p2.X, p2.Y, out a, out b, out c);
				Assert.AreEqual(20 * sizeFactor, a);
				Assert.AreEqual(-20 * sizeFactor, b);
				Assert.AreEqual(0, c);
				//
				// Huge value for c
				p1.X = 10 * sizeFactor; p1.Y = 1 * sizeFactor;
				p2.X = -10 * sizeFactor; p2.Y = -10 * sizeFactor;
				Geometry.CalcLine(p1.X, p1.Y, p2.X, p2.Y, out a, out b, out c);
				Assert.AreEqual(11 * sizeFactor, a);
				Assert.AreEqual(-20 * sizeFactor, b);
				Assert.AreEqual(-90 * (sizeFactor * sizeFactor), c);
				//
				// Special 1: Nearly horzontal/vertical lines
				p1.X = 0; p1.Y = 1 * sizeFactor;
				p2.X = 1; p2.Y = -1 * sizeFactor;
				Geometry.CalcLine(p1.X, p1.Y, p2.X, p2.Y, out a, out b, out c);
				Assert.AreEqual(2 * sizeFactor, a);
				Assert.AreEqual(1, b);
				Assert.AreEqual(-1 * sizeFactor, c);
				//
				p1.X = 1 * sizeFactor; p1.Y = 0;
				p2.X = -1 * sizeFactor; p2.Y = 1;
				Geometry.CalcLine(p1.X, p1.Y, p2.X, p2.Y, out a, out b, out c);
				Assert.AreEqual(1, a);
				Assert.AreEqual(2 * sizeFactor, b);
				Assert.AreEqual(-1 * sizeFactor, c);
				//
				// Special 2: Only one point
				p1.X = 1 * sizeFactor; p1.Y = 1;
				p2.X = 1 * sizeFactor; p2.Y = 1;
				Geometry.CalcLine(p1.X, p1.Y, p2.X, p2.Y, out a, out b, out c);
				Assert.AreEqual(0, a);
				Assert.AreEqual(0, b);
				Assert.AreEqual(0, c);

				Trace.WriteLine($"SizeFactor {sizeFactor}: OK");
			}
#pragma warning restore 0618
		}


		[TestMethod]
		public void CalcPerpendicularLineTest() {
#pragma warning disable 0618
			Point p1 = Point.Empty, p2 = Point.Empty;
			int a, b, c;
			foreach (int sizeFactor in DiagramSizeFactors) {
				// CalcLine does not work with big coordinate values
				if (sizeFactor > 1000) break;

				p1.X = -1 * sizeFactor; p1.Y = -1 * sizeFactor;
				p2.X = 1 * sizeFactor; p2.Y = 1 * sizeFactor;
				Geometry.CalcPerpendicularLine(p1.X, p1.Y, p2.X, p2.Y, out a, out b, out c);
				Assert.AreEqual(-2 * sizeFactor, a);
				Assert.AreEqual(-2 * sizeFactor, b);
				Assert.AreEqual(-4 * (sizeFactor * sizeFactor), c);
				//
				// Big values for a and b, huge value for c
				p1.X = 10 * sizeFactor; p1.Y = 10 * sizeFactor;
				p2.X = -10 * sizeFactor; p2.Y = -10 * sizeFactor;
				Geometry.CalcPerpendicularLine(p1.X, p1.Y, p2.X, p2.Y, out a, out b, out c);
				Assert.AreEqual(20 * sizeFactor, a);
				Assert.AreEqual(20 * sizeFactor, b);
				Assert.AreEqual(-400 * (sizeFactor * sizeFactor), c);
				//
				p1.X = 10 * sizeFactor; p1.Y = 1 * sizeFactor;
				p2.X = -10 * sizeFactor; p2.Y = -10 * sizeFactor;
				Geometry.CalcPerpendicularLine(p1.X, p1.Y, p2.X, p2.Y, out a, out b, out c);
				Assert.AreEqual(20 * sizeFactor, a);
				Assert.AreEqual(11 * sizeFactor, b);
				Assert.AreEqual(-211 * (sizeFactor * sizeFactor), c);
				//
				// Special 1: Nearly horzontal/vertical lines
				p1.X = 0; p1.Y = 1 * sizeFactor;
				p2.X = 1; p2.Y = -1 * sizeFactor;
				Geometry.CalcPerpendicularLine(p1.X, p1.Y, p2.X, p2.Y, out a, out b, out c);
				Assert.AreEqual(-1, a);
				Assert.AreEqual(2 * sizeFactor, b);
				Assert.AreEqual(-2 * (sizeFactor * sizeFactor), c);
				//
				p1.X = 1 * sizeFactor; p1.Y = 0;
				p2.X = -1 * sizeFactor; p2.Y = 1;
				Geometry.CalcPerpendicularLine(p1.X, p1.Y, p2.X, p2.Y, out a, out b, out c);
				Assert.AreEqual(2 * sizeFactor, a);
				Assert.AreEqual(-1, b);
				Assert.AreEqual(-2 * (sizeFactor * sizeFactor), c);
				//
				// Special 2: Only one point
				p1.X = 1 * sizeFactor; p1.Y = 1;
				p2.X = 1 * sizeFactor; p2.Y = 1;
				Geometry.CalcPerpendicularLine(p1.X, p1.Y, p2.X, p2.Y, out a, out b, out c);
				Assert.AreEqual(0, a);
				Assert.AreEqual(0, b);
				Assert.AreEqual(0, c);

				Trace.WriteLine($"SizeFactor {sizeFactor}: OK");
			}
#pragma warning restore 0618
		}


		[TestMethod]
		public void IntersectLineSegmentsTest() {
			Point p1 = Point.Empty, p2 = Point.Empty, p3 = Point.Empty, p4 = Point.Empty;
			Point r;
			//
			// Check if line segment ends bounds are respected (y axis)
			p1.X = 1000; p1.Y = 0;
			p2.X = 1000; p2.Y = 1000;
			p3.X = 250; p3.Y = 414;
			p4.X = 9077; p4.Y = 15238;
			r = Geometry.IntersectLineSegments(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
			Assert.AreEqual(Geometry.InvalidCoordinateValue, r.X);
			Assert.AreEqual(Geometry.InvalidCoordinateValue, r.Y);
			//
			// Check if line segment ends bounds are respected (x axis)
			p1.X = 0; p1.Y = 1000;
			p2.X = 1000; p2.Y = 1000;
			p3.X = 250; p3.Y = 414;
			p4.X = 15238; p4.Y = 9077;
			r = Geometry.IntersectLineSegments(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
			Assert.AreEqual(Geometry.InvalidCoordinateValue, r.X);
			Assert.AreEqual(Geometry.InvalidCoordinateValue, r.Y);
			//
			// Check if result is correct (x axis)
			p1.X = 0; p1.Y = 1000;
			p2.X = 1000; p2.Y = 1000;
			p3.X = 250; p3.Y = 414;
			p4.X = 9077; p4.Y = 15238;
			r = Geometry.IntersectLineSegments(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
			Assert.AreEqual(598, r.X);
			Assert.AreEqual(1000, r.Y);
			//
			r = Geometry.IntersectLineSegments(2, 0, 2, 2, 0, 0, 4, 2);
			Assert.AreEqual(2, r.X);
			Assert.AreEqual(1, r.Y);
			//
			// Check if result is correct (y axis)
			p1.X = 1000; p1.Y = 0;
			p2.X = 1000; p2.Y = 1000;
			p3.X = 250; p3.Y = 250;
			p4.X = 1750; p4.Y = 750;
			r = Geometry.IntersectLineSegments(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
			Assert.AreEqual(1000, r.X);
			Assert.AreEqual(500, r.Y);
			//
			// Check if result is correct
			p1.X = 680; p1.Y = 340;
			p2.X = 520; p2.Y = 380;
			p3.X = 620; p3.Y = 440;
			p4.X = 580; p4.Y = 280;
			r = Geometry.IntersectLineSegments(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
			Assert.AreEqual(600, r.X);
			Assert.AreEqual(360, r.Y);
			//
			p1.X = 301; p1.Y = 120;
			p2.X = 300; p2.Y = 440;
			p3.X = 500; p3.Y = 280;
			p4.X = 200; p4.Y = 160;
			r = Geometry.IntersectLineSegments(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
			Assert.AreEqual(300, r.X);
			Assert.AreEqual(200, r.Y);
			//
			foreach (int sizeFactor in DiagramSizeFactors) {
				p1.X = -1 * sizeFactor; p1.Y = -1 * sizeFactor;
				p2.X = 1 * sizeFactor; p2.Y = 1 * sizeFactor;
				p3.X = -1 * sizeFactor; p3.Y = 1 * sizeFactor;
				p4.X = 1 * sizeFactor; p4.Y = -1 * sizeFactor;
				r = Geometry.IntersectLineSegments(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
				Assert.AreEqual(0, r.X);
				Assert.AreEqual(0, r.Y);
				//
				// This test works only with 100 and 25 because the intersection point is not an integer value otherwise.
				// Ignore size factors that exceed 10.000 for the moment as coordinates would exceed 1.000.000 in this case.
				if (sizeFactor <= 10000) {
					p1.X = -100 * sizeFactor; p1.Y = 0 * sizeFactor;
					p2.X = 25 * sizeFactor; p2.Y = 100 * sizeFactor;
					p3.X = 100 * sizeFactor; p3.Y = 0 * sizeFactor;
					p4.X = -25 * sizeFactor; p4.Y = 100 * sizeFactor;
					r = Geometry.IntersectLineSegments(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
					Assert.AreEqual(0, r.X);
					Assert.AreEqual(80 * sizeFactor, r.Y);
				}
				//
				p1.X = -10 * sizeFactor; p1.Y = 1 * sizeFactor;
				p2.X = 10 * sizeFactor; p2.Y = 1 * sizeFactor;
				p3.X = -10 * sizeFactor; p3.Y = 0 * sizeFactor;
				p4.X = 10 * sizeFactor; p4.Y = 1 * sizeFactor;
				r = Geometry.IntersectLineSegments(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
				Assert.AreEqual(10 * sizeFactor, r.X);
				Assert.AreEqual(1 * sizeFactor, r.Y);
				//
				// Special 1: Orthogonal line segments
				p1.X = -1 * sizeFactor; p1.Y = 0 * sizeFactor;
				p2.X = -1 * sizeFactor; p2.Y = 10 * sizeFactor;
				p3.X = -10 * sizeFactor; p3.Y = 9 * sizeFactor;
				p4.X = 10 * sizeFactor; p4.Y = 9 * sizeFactor;
				r = Geometry.IntersectLineSegments(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
				Assert.AreEqual(-1 * sizeFactor, r.X);
				Assert.AreEqual(9 * sizeFactor, r.Y);
				//
				// Special 2.1: Parallel line segments (horizontal)
				p1.X = -10 * sizeFactor; p1.Y = 0 * sizeFactor;
				p2.X = 10 * sizeFactor; p2.Y = 0 * sizeFactor;
				p3.X = -10 * sizeFactor; p3.Y = 10 * sizeFactor;
				p4.X = 10 * sizeFactor; p4.Y = 10 * sizeFactor;
				r = Geometry.IntersectLineSegments(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
				Assert.AreEqual(Geometry.InvalidCoordinateValue, r.X);
				Assert.AreEqual(Geometry.InvalidCoordinateValue, r.Y);
				// Special 2.1: Parallel line segments (vertical)
				p1.X = 0 * sizeFactor; p1.Y = -10 * sizeFactor;
				p2.X = 0 * sizeFactor; p2.Y = 10 * sizeFactor;
				p3.X = 10 * sizeFactor; p3.Y = -10 * sizeFactor;
				p4.X = 10 * sizeFactor; p4.Y = 10 * sizeFactor;
				r = Geometry.IntersectLineSegments(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
				Assert.AreEqual(Geometry.InvalidCoordinateValue, r.X);
				Assert.AreEqual(Geometry.InvalidCoordinateValue, r.Y);
				// Special 2.1: Parallel line segments
				if (sizeFactor <= 50000) {
					p1.X = -16 * sizeFactor; p1.Y = -14 * sizeFactor;
					p2.X = 12 * sizeFactor; p2.Y = 13 * sizeFactor;
					p3.X = -p1.X; p3.Y = -p1.Y;
					p4.X = -p2.X; p4.Y = -p2.Y;
					r = Geometry.IntersectLineSegments(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
					Assert.AreEqual(Geometry.InvalidCoordinateValue, r.X);
					Assert.AreEqual(Geometry.InvalidCoordinateValue, r.Y);
				}
				// Special 3: Congruent and reverse segments
				if (sizeFactor <= 50000) {
					p1.X = -16 * sizeFactor; p1.Y = -14 * sizeFactor;
					p2.X = 12 * sizeFactor; p2.Y = 13 * sizeFactor;
					p3 = p2;
					p4 = p1;
					r = Geometry.IntersectLineSegments(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
					Assert.AreEqual(Geometry.InvalidCoordinateValue, r.X);
					Assert.AreEqual(Geometry.InvalidCoordinateValue, r.Y);
				}

				Trace.WriteLine($"SizeFactor {sizeFactor}: OK");
			}
		}


		[TestMethod]
		public void IntersectLineWithLineSegmentTest() {
			Point p1 = Point.Empty, p2 = Point.Empty, p3 = Point.Empty, p4 = Point.Empty;
			Point r;
			//
			// Check if line segment ends bounds are respected (y axis)
			p1.X = 1000; p1.Y = 0;
			p2.X = 1000; p2.Y = 1000;
			p3.X = 250; p3.Y = 414;
			p4.X = 9077; p4.Y = 15238;
			r = Geometry.IntersectLineWithLineSegment(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
			Assert.AreEqual(1000, r.X);
			Assert.AreEqual(1674, r.Y);
			r = Geometry.IntersectLineWithLineSegment(p3.X, p3.Y, p4.X, p4.Y, p1.X, p1.Y, p2.X, p2.Y);
			Assert.AreEqual(Geometry.InvalidCoordinateValue, r.X);
			Assert.AreEqual(Geometry.InvalidCoordinateValue, r.Y);
			//
			// Check if line segment ends bounds are respected (x axis)
			p1.X = 0; p1.Y = 1000;
			p2.X = 1000; p2.Y = 1000;
			p3.X = 250; p3.Y = 414;
			p4.X = 15238; p4.Y = 9077;
			r = Geometry.IntersectLineWithLineSegment(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
			Assert.AreEqual(1263, r.X);
			Assert.AreEqual(1000, r.Y);
			r = Geometry.IntersectLineWithLineSegment(p3.X, p3.Y, p4.X, p4.Y, p1.X, p1.Y, p2.X, p2.Y);
			Assert.AreEqual(Geometry.InvalidCoordinateValue, r.X);
			Assert.AreEqual(Geometry.InvalidCoordinateValue, r.Y);
			//
			// Check if result is correct (x axis)
			p1.X = 0; p1.Y = 1000;
			p2.X = 1000; p2.Y = 1000;
			p3.X = 250; p3.Y = 414;
			p4.X = 9077; p4.Y = 15238;
			r = Geometry.IntersectLineWithLineSegment(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
			Assert.AreEqual(598, r.X);
			Assert.AreEqual(1000, r.Y);
			//
			r = Geometry.IntersectLineWithLineSegment(2, 0, 2, 2, 0, 0, 4, 2);
			Assert.AreEqual(2, r.X);
			Assert.AreEqual(1, r.Y);
			//
			// Check if result is correct (y axis)
			p1.X = 1000; p1.Y = 0;
			p2.X = 1000; p2.Y = 1000;
			p3.X = 250; p3.Y = 250;
			p4.X = 1750; p4.Y = 750;
			r = Geometry.IntersectLineWithLineSegment(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
			Assert.AreEqual(1000, r.X);
			Assert.AreEqual(500, r.Y);
			//
			// Check if result is correct
			p1.X = 680; p1.Y = 340;
			p2.X = 520; p2.Y = 380;
			p3.X = 620; p3.Y = 440;
			p4.X = 580; p4.Y = 280;
			r = Geometry.IntersectLineWithLineSegment(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
			Assert.AreEqual(600, r.X);
			Assert.AreEqual(360, r.Y);
			//
			p1.X = 301; p1.Y = 120;
			p2.X = 300; p2.Y = 440;
			p3.X = 500; p3.Y = 280;
			p4.X = 200; p4.Y = 160;
			r = Geometry.IntersectLineSegments(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
			Assert.AreEqual(300, r.X);
			Assert.AreEqual(200, r.Y);
			//
			foreach (int sizeFactor in DiagramSizeFactors) {
				p1.X = -1 * sizeFactor; p1.Y = -1 * sizeFactor;
				p2.X = 1 * sizeFactor; p2.Y = 1 * sizeFactor;
				p3.X = -1 * sizeFactor; p3.Y = 1 * sizeFactor;
				p4.X = 1 * sizeFactor; p4.Y = -1 * sizeFactor;
				r = Geometry.IntersectLineWithLineSegment(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
				Assert.AreEqual(0, r.X);
				Assert.AreEqual(0, r.Y);
				//
				// This test works only with 100 and 25 because the intersection point is not an integer value otherwise.
				// Ignore size factors that exceed 10.000 for the moment as coordinates would exceed 1.000.000 in this case.
				if (sizeFactor <= 10000) {
					p1.X = -100 * sizeFactor; p1.Y = 0 * sizeFactor;
					p2.X = 25 * sizeFactor; p2.Y = 100 * sizeFactor;
					p3.X = 100 * sizeFactor; p3.Y = 0 * sizeFactor;
					p4.X = -25 * sizeFactor; p4.Y = 100 * sizeFactor;
					r = Geometry.IntersectLineWithLineSegment(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
					Assert.AreEqual(0, r.X);
					Assert.AreEqual(80 * sizeFactor, r.Y);
				}
				//
				p1.X = -10 * sizeFactor; p1.Y = 1 * sizeFactor;
				p2.X = 10 * sizeFactor; p2.Y = 1 * sizeFactor;
				p3.X = -10 * sizeFactor; p3.Y = 0 * sizeFactor;
				p4.X = 10 * sizeFactor; p4.Y = 1 * sizeFactor;
				r = Geometry.IntersectLineWithLineSegment(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
				Assert.AreEqual(10 * sizeFactor, r.X);
				Assert.AreEqual(1 * sizeFactor, r.Y);
				//
				// Special 1: Orthogonal line segments
				p1.X = -1 * sizeFactor; p1.Y = 0 * sizeFactor;
				p2.X = -1 * sizeFactor; p2.Y = 10 * sizeFactor;
				p3.X = -10 * sizeFactor; p3.Y = 9 * sizeFactor;
				p4.X = 10 * sizeFactor; p4.Y = 9 * sizeFactor;
				r = Geometry.IntersectLineWithLineSegment(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
				Assert.AreEqual(-1 * sizeFactor, r.X);
				Assert.AreEqual(9 * sizeFactor, r.Y);
				//
				// Special 2.1: Parallel line segments (horizontal)
				p1.X = -10 * sizeFactor; p1.Y = 0 * sizeFactor;
				p2.X = 10 * sizeFactor; p2.Y = 0 * sizeFactor;
				p3.X = -10 * sizeFactor; p3.Y = 10 * sizeFactor;
				p4.X = 10 * sizeFactor; p4.Y = 10 * sizeFactor;
				r = Geometry.IntersectLineWithLineSegment(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
				Assert.AreEqual(Geometry.InvalidCoordinateValue, r.X);
				Assert.AreEqual(Geometry.InvalidCoordinateValue, r.Y);
				// Special 2.1: Parallel line segments (vertical)
				p1.X = 0 * sizeFactor; p1.Y = -10 * sizeFactor;
				p2.X = 0 * sizeFactor; p2.Y = 10 * sizeFactor;
				p3.X = 10 * sizeFactor; p3.Y = -10 * sizeFactor;
				p4.X = 10 * sizeFactor; p4.Y = 10 * sizeFactor;
				r = Geometry.IntersectLineWithLineSegment(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
				Assert.AreEqual(Geometry.InvalidCoordinateValue, r.X);
				Assert.AreEqual(Geometry.InvalidCoordinateValue, r.Y);
				// Special 2.1: Parallel line segments
				if (sizeFactor <= 50000) {
					p1.X = -16 * sizeFactor; p1.Y = -14 * sizeFactor;
					p2.X = 12 * sizeFactor; p2.Y = 13 * sizeFactor;
					p3.X = -p1.X; p3.Y = -p1.Y;
					p4.X = -p2.X; p4.Y = -p2.Y;
					r = Geometry.IntersectLineWithLineSegment(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
					Assert.AreEqual(Geometry.InvalidCoordinateValue, r.X);
					Assert.AreEqual(Geometry.InvalidCoordinateValue, r.Y);
				}
				// Special 3: Congruent and reverse segments
				if (sizeFactor <= 50000) {
					p1.X = -16 * sizeFactor; p1.Y = -14 * sizeFactor;
					p2.X = 12 * sizeFactor; p2.Y = 13 * sizeFactor;
					p3 = p2;
					p4 = p1;
					r = Geometry.IntersectLineWithLineSegment(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
					Assert.AreEqual(Geometry.InvalidCoordinateValue, r.X);
					Assert.AreEqual(Geometry.InvalidCoordinateValue, r.Y);
				}

				Trace.WriteLine($"SizeFactor {sizeFactor}: OK");
			}
		}


		[TestMethod]
		public void IntersectLinesTest() {
			Point p1 = Point.Empty, p2 = Point.Empty, p3 = Point.Empty, p4 = Point.Empty;
			Point r;
			//
			// Intersect with line segment parallel to y axis
			p1.X = 1000; p1.Y = 0;
			p2.X = 1000; p2.Y = 1000;
			p3.X = 250; p3.Y = 414;
			p4.X = 9077; p4.Y = 15238;
			r = Geometry.IntersectLines(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
			Assert.AreEqual(1000, r.X);
			Assert.AreEqual(1674, r.Y);
			//
			// Intersect with line segment parallel to x axis
			r = Geometry.IntersectLines(0, 1, 2, 1, 0, 0, 2, 2);
			Assert.AreEqual(1, r.X);
			Assert.AreEqual(1, r.Y);
			//
			p1.X = 0; p1.Y = 1000;
			p2.X = 1000; p2.Y = 1000;
			p3.X = 250; p3.Y = 414;
			p4.X = 15238; p4.Y = 9077;
			r = Geometry.IntersectLines(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
			Assert.AreEqual(1263, r.X);
			Assert.AreEqual(1000, r.Y);
			//
			// Check if result is correct (x axis)
			p1.X = 0; p1.Y = 1000;
			p2.X = 1000; p2.Y = 1000;
			p3.X = 250; p3.Y = 414;
			p4.X = 9077; p4.Y = 15238;
			r = Geometry.IntersectLines(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
			Assert.AreEqual(598, r.X);
			Assert.AreEqual(1000, r.Y);
			//
			r = Geometry.IntersectLines(2, 0, 2, 2, 0, 0, 4, 2);
			Assert.AreEqual(2, r.X);
			Assert.AreEqual(1, r.Y);
			//
			// Check if result is correct (y axis)
			p1.X = 1000; p1.Y = 0;
			p2.X = 1000; p2.Y = 1000;
			p3.X = 250; p3.Y = 250;
			p4.X = 1750; p4.Y = 750;
			r = Geometry.IntersectLines(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
			Assert.AreEqual(1000, r.X);
			Assert.AreEqual(500, r.Y);
			//
			// Check if result is correct
			p1.X = 680; p1.Y = 340;
			p2.X = 520; p2.Y = 380;
			p3.X = 620; p3.Y = 440;
			p4.X = 580; p4.Y = 280;
			r = Geometry.IntersectLines(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
			Assert.AreEqual(600, r.X);
			Assert.AreEqual(360, r.Y);
			//
			// Check with almost vertical line
			p1.X = 301; p1.Y = 120;
			p2.X = 300; p2.Y = 440;
			p3.X = 500; p3.Y = 280;
			p4.X = 200; p4.Y = 160;
			r = Geometry.IntersectLines(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
			Assert.AreEqual(300, r.X);
			Assert.AreEqual(200, r.Y);
			//
			foreach (int sizeFactor in DiagramSizeFactors) {
				p1.X = -1 * sizeFactor; p1.Y = -1 * sizeFactor;
				p2.X = 1 * sizeFactor; p2.Y = 1 * sizeFactor;
				p3.X = -1 * sizeFactor; p3.Y = 1 * sizeFactor;
				p4.X = 1 * sizeFactor; p4.Y = -1 * sizeFactor;
				r = Geometry.IntersectLines(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
				Assert.AreEqual(0, r.X);
				Assert.AreEqual(0, r.Y);
				//
				// This test works only with 100 and 25 because the intersection point is not an integer value otherwise.
				// Ignore size factors that exceed 10.000 for the moment as coordinates would exceed 1.000.000 in this case.
				if (sizeFactor <= 10000) {
					p1.X = -100 * sizeFactor; p1.Y = 0 * sizeFactor;
					p2.X = 25 * sizeFactor; p2.Y = 100 * sizeFactor;
					p3.X = 100 * sizeFactor; p3.Y = 0 * sizeFactor;
					p4.X = -25 * sizeFactor; p4.Y = 100 * sizeFactor;
					r = Geometry.IntersectLines(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
					Assert.AreEqual(0, r.X);
					Assert.AreEqual(80 * sizeFactor, r.Y);
				}
				//
				// Special 1: Orthogonal lines
				p1.X = -1 * sizeFactor; p1.Y = 0 * sizeFactor;
				p2.X = -1 * sizeFactor; p2.Y = 10 * sizeFactor;
				p3.X = -10 * sizeFactor; p3.Y = 9 * sizeFactor;
				p4.X = 10 * sizeFactor; p4.Y = 9 * sizeFactor;
				r = Geometry.IntersectLines(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
				Assert.AreEqual(-1 * sizeFactor, r.X);
				Assert.AreEqual(9 * sizeFactor, r.Y);
				//
				// Special 2.1: Parallel line segments (horizontal)
				p1.X = -10 * sizeFactor; p1.Y = 0 * sizeFactor;
				p2.X = 10 * sizeFactor; p2.Y = 0 * sizeFactor;
				p3.X = -10 * sizeFactor; p3.Y = 10 * sizeFactor;
				p4.X = 10 * sizeFactor; p4.Y = 10 * sizeFactor;
				r = Geometry.IntersectLines(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
				Assert.AreEqual(Geometry.InvalidCoordinateValue, r.X);
				Assert.AreEqual(Geometry.InvalidCoordinateValue, r.Y);
				// Special 2.1: Parallel line segments (vertical)
				p1.X = 0 * sizeFactor; p1.Y = -10 * sizeFactor;
				p2.X = 0 * sizeFactor; p2.Y = 10 * sizeFactor;
				p3.X = 10 * sizeFactor; p3.Y = -10 * sizeFactor;
				p4.X = 10 * sizeFactor; p4.Y = 10 * sizeFactor;
				r = Geometry.IntersectLines(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
				Assert.AreEqual(Geometry.InvalidCoordinateValue, r.X);
				Assert.AreEqual(Geometry.InvalidCoordinateValue, r.Y);
				// Special 2.1: Parallel line segments
				if (sizeFactor <= 50000) {
					p1.X = -16 * sizeFactor; p1.Y = -14 * sizeFactor;
					p2.X = 12 * sizeFactor; p2.Y = 13 * sizeFactor;
					p3.X = -p1.X; p3.Y = -p1.Y;
					p4.X = -p2.X; p4.Y = -p2.Y;
					r = Geometry.IntersectLines(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
					Assert.AreEqual(Geometry.InvalidCoordinateValue, r.X);
					Assert.AreEqual(Geometry.InvalidCoordinateValue, r.Y);
				}
				// Special 3: Congruent and reverse segments
				if (sizeFactor <= 50000) {
					p1.X = -16 * sizeFactor; p1.Y = -14 * sizeFactor;
					p2.X = 12 * sizeFactor; p2.Y = 13 * sizeFactor;
					p3 = p2;
					p4 = p1;
					r = Geometry.IntersectLines(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
					Assert.AreEqual(Geometry.InvalidCoordinateValue, r.X);
					Assert.AreEqual(Geometry.InvalidCoordinateValue, r.Y);
				}
			}
			////
			//// Special 3: Nearly parallel lines
			//// What to do with such a case where the intersection point is outside the numeric range of coordinates?
			//try {
			//	p1.X = -100000; p1.Y = -10000;
			//	p2.X = 100000; p2.Y = -10000;
			//	p3.X = -100000; p3.Y = 10000;
			//	p4.X = 100000; p4.Y = 10001;
			//	r = Geometry.IntersectLines(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
			//	Assert.AreEqual(Geometry.InvalidCoordinateValue, r.X);
			//	Assert.AreEqual(Geometry.InvalidCoordinateValue, r.Y);
			//} catch (OverflowException exc) {
			//	Assert.Inconclusive(exc.Message);
			//}
		}


		[TestMethod]
		public void VectorCrossProductTest() {
#pragma warning disable 0618
			Point p1 = Point.Empty, p2 = Point.Empty, p3 = Point.Empty;
			long r;
			foreach (int sizeFactor in DiagramSizeFactors) {
				long sizeFactorSquare = (long)sizeFactor * sizeFactor;
				// Coordinates of the test are 3,4 and 6,8 for easier vector length calculation by brain:
				// a²+b²=c² -> 3²+4²=5² / 6²+8²=10²
				//
				// Coordinates would exceed targeted range of -1.000.000 to 1.000.000 with size factors > 50.000
				p1.X = 3 * sizeFactor; p1.Y = -4 * sizeFactor;
				p2.X = 6 * sizeFactor; p2.Y = 8 * sizeFactor;
				p3.X = -12 * sizeFactor; p3.Y = 0;
				if (sizeFactor < 5000)
					r = Geometry.VectorCrossProduct(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y);
				else
					r = Geometry.VectorCrossProductL(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y);
				Assert.AreEqual(192 * sizeFactorSquare, r);
				//
				// Special 1: Parallel vectors
				p1.X = 3 * sizeFactor; p1.Y = -4 * sizeFactor;
				p2.X = 6 * sizeFactor; p2.Y = -8 * sizeFactor;
				p3.X = 2 * p1.X; p3.Y = 2 * p1.Y;
				if (sizeFactor < 5000)
					r = Geometry.VectorCrossProduct(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y);
				else
					r = Geometry.VectorCrossProductL(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y);
				Assert.AreEqual(0 * sizeFactorSquare, r);
				//
				// Special 2: Orthogonal vectors
				p1.X = 0 * sizeFactor; p1.Y = -4 * sizeFactor;
				p2.X = 4 * sizeFactor; p2.Y = 0 * sizeFactor;
				p3.X = -2 * p1.X; p3.Y = -2 * p1.Y;
				if (sizeFactor < 5000)
					r = Geometry.VectorCrossProduct(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y);
				else
					r = Geometry.VectorCrossProductL(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y);
				Assert.AreEqual(48 * sizeFactorSquare, r);

				Trace.WriteLine($"SizeFactor {sizeFactor}: OK");
			}
#pragma warning restore 0618
		}


		[TestMethod]
		public void VectorDotProductTest() {
#pragma warning disable 0618
			Point p1 = Point.Empty, p2 = Point.Empty, p3 = Point.Empty, p4 = Point.Empty;
			long r;
			foreach (int sizeFactor in DiagramSizeFactors) {
				long sizeFactorSquare = (long)sizeFactor * sizeFactor;

				p1.X = 3 * sizeFactor; p1.Y = -4 * sizeFactor;
				p2.X = 6 * sizeFactor; p2.Y = 8 * sizeFactor;
				if (sizeFactor < 5000)
					r = Geometry.VectorDotProduct(p1.X, p1.Y, p2.X, p2.Y);
				else
					r = Geometry.VectorDotProductL(p1.X, p1.Y, p2.X, p2.Y);
				Assert.AreEqual(-14 * sizeFactorSquare, r);
				//
				p3.X = -p1.Y; p3.Y = -p1.X;
				if (sizeFactor < 5000)
					r = Geometry.VectorDotProduct(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y);
				else
					r = Geometry.VectorDotProductL(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y);
				Assert.AreEqual(-138 * sizeFactorSquare, r);
				//
				p4.X = -p2.Y; p4.Y = -p2.X;
				if (sizeFactor < 5000)
					r = Geometry.CalcScalarProduct(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
				else
					r = Geometry.VectorDotProductL(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
				Assert.AreEqual(-72 * sizeFactorSquare, r);
				//
				// Special 1: Parallel vectors
				p1.X = 3 * sizeFactor; p1.Y = -4 * sizeFactor;
				p2.X = 6 * sizeFactor; p2.Y = -8 * sizeFactor;
				if (sizeFactor < 5000)
					r = Geometry.VectorDotProduct(p1.X, p1.Y, p2.X, p2.Y);
				else
					r = Geometry.VectorDotProductL(p1.X, p1.Y, p2.X, p2.Y);
				Assert.AreEqual(50 * sizeFactorSquare, r);
				//
				p3.X = 2 * p1.X; p3.Y = 2 * p1.Y;
				if (sizeFactor < 5000)
					r = Geometry.VectorDotProduct(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y);
				else
					r = Geometry.VectorDotProductL(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y);
				Assert.AreEqual(0 * sizeFactorSquare, r);
				//
				p4.X = 2 * p2.X; p4.Y = 2 * p2.Y;
				if (sizeFactor < 5000)
					r = Geometry.CalcScalarProduct(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
				else
					r = Geometry.VectorDotProductL(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
				Assert.AreEqual(50 * sizeFactorSquare, r);
				//
				// Special 2: Orthogonal vectors
				p1.X = 0 * sizeFactor; p1.Y = -4 * sizeFactor;
				p2.X = 4 * sizeFactor; p2.Y = 0 * sizeFactor;
				if (sizeFactor < 5000)
					r = Geometry.VectorDotProduct(p1.X, p1.Y, p2.X, p2.Y);
				else
					r = Geometry.VectorDotProductL(p1.X, p1.Y, p2.X, p2.Y);
				Assert.AreEqual(0, r);
				//
				p3.X = -2 * p1.X; p3.Y = -2 * p1.Y;
				if (sizeFactor < 5000)
					r = Geometry.VectorDotProduct(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y);
				else
					r = Geometry.VectorDotProductL(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y);
				Assert.AreEqual(16 * sizeFactorSquare, r);
				//
				p4.X = -2 * p2.X; p4.Y = -2 * p2.Y;
				if (sizeFactor < 5000)
					r = Geometry.CalcScalarProduct(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
				else
					r = Geometry.VectorDotProductL(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
				Assert.AreEqual(-64 * sizeFactorSquare, r);

				Trace.WriteLine($"SizeFactor {sizeFactor}: OK");
			}
#pragma warning restore 0618
		}


		/// <summary>
		/// Tests for arithmetic overflow in geometric functions when using very large diagrams.
		/// </summary>
		[TestMethod]
		public void TestLargeCoordinates() {
			// For simplicity's sake, we only test methods with int parameters as type 'float' can store even int.MaxValue * int.MaxValue which should be enough.
			Point getPoint1(int factor) { return new Point(-16 * factor, -14 * factor); };
			Point getPoint2(int factor) { return new Point(12 * factor, 13 * factor); };

			foreach (int sizeFactor in DiagramSizeFactors) {
				int skippedFunctionsCount = 0;
				int deprecatedCount = 0;

				// Prepare test coordinates
				int resX, resY, aP, bP, cP;
				Rectangle resRect;
				Point p1 = getPoint1(sizeFactor);
				Point p2 = getPoint2(sizeFactor);
				Point p3 = new Point(p2.X, p1.Y);
				Point p4 = new Point(p1.X, p2.Y);
				Point[] points = new Point[] { p1, p3, p2, p4 };
				PointF[] pointFs = new PointF[] { p1, p3, p2, p4 };
				Rectangle rect = Rectangle.FromLTRB(p1.X, p1.Y, p2.X, p2.Y);
				int intA1 = 0, intB1 = 0, intC1 = 0;
				//
				//try {
#pragma warning disable 0612, 0618
				checked {
					Point center = Geometry.GetCenter(rect);

					Geometry.TransformMouseMovement(p2.X, p2.Y, 3500, out float transformedDeltaX, out float transformedDeltaY, out float sin, out float cos);
					Geometry.VectorLinearInterpolation(p1.X, p1.Y, p2.X, p2.Y, 1.75f);
					Geometry.CalcNormalVectorOfLine(p1.X, p1.Y, p2.X, p2.Y, (p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2, 100, out int pX, out int pY);
					Geometry.CalcNormalVectorOfRectangle(rect.X, rect.Y, rect.Width, rect.Height, p1.X, p1.Y, 100);
					Geometry.CalcNormalVectorOfCircle(center.X, center.Y, rect.Width / 2, center.X + rect.Width / 2, center.Y + rect.Height / 2, 100);
					Geometry.TriangleContainsPoint(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, center.X, center.Y);
					Geometry.QuadrangleContainsPoint(rect.Left, rect.Top, rect.Right, rect.Top, rect.Right, rect.Bottom, rect.Left, rect.Bottom, center.X, center.Y);
					Geometry.RectangleContainsPoint(rect.X, rect.Y, rect.Width, rect.Height, p1.X, p1.Y, true);
					Geometry.PolygonIsConvex(points);
					Geometry.ConvexPolygonContainsPoint(points, center.X, center.Y);
					Geometry.ConvexPolygonContainsPoint(pointFs, center.X, center.Y);
					Geometry.PolygonContainsPoint(points, center.X, center.Y);
					Geometry.PolygonContainsPoint(pointFs, center.X, center.Y);
					Geometry.EllipseContainsPoint(center.X, center.Y, rect.Width, rect.Height, 350, center.X, center.Y);
					Geometry.ArcContainsPoint(center.X, rect.Bottom, rect.Left, rect.Bottom, rect.Right, rect.Bottom, center.X, rect.Top, center.X, rect.Top, 0.5f);
					Geometry.PolygonIntersectsWithRectangle(points, rect.Left, rect.Top, rect.Right, rect.Bottom);
					Geometry.LineIntersectsWithLine(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
					Geometry.RectangleIntersectsWithLine(rect, p1.X, p1.Y, p2.X, p2.Y, true);
					Geometry.RectangleIntersectsWithLine(center.X, center.Y, rect.Width, rect.Height, 350, p1.X, p1.Y, p2.X, p2.Y, true);
					Geometry.ArcIntersectsWithRectangle(rect.Left, rect.Bottom, center.X, rect.Top, rect.Right, rect.Bottom, center.X, rect.Bottom, rect.Height, rect);
					Geometry.CircleIntersectsWithRectangle(rect, center.X, center.Y, rect.Height);
					Geometry.EllipseIntersectsWithRectangle(center.X, center.Y, rect.Width, rect.Height, 350, rect);
					Geometry.PolygonIntersectsWithEllipse(center.X, center.Y, rect.Width, rect.Height, 350, pointFs);
					Geometry.PolygonIntersectsWithEllipse(center.X, center.Y, rect.Width, rect.Height, 350, points);
					Geometry.CalcLine(p3.X, p3.Y, p4.X, p4.Y, out int _, out int _, out long _);
					Geometry.CalcLine(p1.X, p1.Y, p2.X, p2.Y, out float m, out float c);
					Geometry.IntersectLineWithRectangle(p1.X, p1.Y, p2.X, p2.Y, rect.Left, rect.Top, rect.Right, rect.Bottom);
					Geometry.IntersectLineSegments((long)p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
					Geometry.IntersectCircleWithLine(center.X, center.Y, rect.Height, p1.X, p1.Y, p2.X, p2.Y, true);
					Geometry.IntersectCircleWithLine(center.X, center.Y, rect.Height, p1.X, p1.Y, p2.X, p2.Y, false);
					Geometry.IntersectCircles(center.X, center.Y, rect.Height, p1.X, p1.Y, rect.Height);
					Geometry.IntersectCircleArc(center.X, center.Y, rect.Height, rect.Left, rect.Bottom, center.X, rect.Top, rect.Right, rect.Bottom, center.X, rect.Bottom, rect.Height);
					Geometry.IntersectArcLine(rect.Left, rect.Bottom, center.X, rect.Top, rect.Right, rect.Bottom, p1.X, p1.Y, p2.X, p2.Y, true);
					Geometry.IntersectPolygonLine(points, p1.X, p1.Y, p2.X, p2.Y, true);
					Geometry.IntersectEllipseLine(center.X, center.Y, rect.Width, rect.Height, 350, p1.X, p1.Y, p2.X, p2.Y, true);
					Geometry.IntersectRectangleLine(rect, 350, p1, p2, true);
					Geometry.DistancePointPoint(p1.X, p1.Y, p2.X, p2.Y);
					Geometry.DistancePointPoint(p1, p2);
					Geometry.DistancePointPointFast(p1.X, p1.Y, p2.X, p2.Y);
					Geometry.DistancePointPointFast(p1.X, p1.Y, p2);
					Geometry.DistancePointPointFast(p1, p2);
					Geometry.DistancePointLineSegment2(p3.X, p3.Y, p1.X, p1.Y, p2.X, p2.Y);
					Geometry.DistancePointLine(p3.X, p3.Y, p1.X, p1.Y, p2.X, p2.Y, true);
					Geometry.BezierPoint(p1, p4, p3, p2, rect.Width / 2);
					Geometry.CalcPolygonBalancePoint(points);
					Geometry.CalcPolygonBalancePoint(points, out resX, out resY);
					Geometry.CalcBoundingRectangle(points, p1.X, p1.Y, 350, out resRect);
					Geometry.CalcBoundingRectangle(points, out resRect);
					Geometry.CalcBoundingRectangle(pointFs, out resRect, true);
					Geometry.CalcBoundingRectangleEllipse(center.X, center.Y, rect.Width, rect.Height, 350, out resRect);
					Geometry.CalcArcTangentThroughPoint(rect.Left, rect.Bottom, center.X, rect.Top, rect.Right, rect.Bottom, rect.Right, rect.Bottom);
					Geometry.CalcArcTangentThroughPoint((float)rect.Left, rect.Bottom, center.X, rect.Top, rect.Right, rect.Bottom, rect.Right, rect.Bottom);
					Geometry.SolveLinear22System(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, out resX, out resY);
					if (sizeFactor < 1000) {
						Geometry.VectorDotProduct(p1.X, p1.Y, p2.X, p2.Y);
						Geometry.VectorDotProduct(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y);
						Geometry.CalcScalarProduct(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
						Geometry.VectorCrossProduct(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y);
						Geometry.CalcPerpendicularLine(p1.X, p1.Y, p2.X, p2.Y, out aP, out bP, out cP);
						Geometry.CalcPerpendicularLine(p1.X, p1.Y, intA1, intB1, intC1, out aP, out bP, out cP);
						Geometry.TranslateLine(intA1, intB1, intC1, p3, out int aT, out int bT, out int cT);
					} else {
						skippedFunctionsCount += 7;
						deprecatedCount += 7;
					}
					if (sizeFactor < 10000) {
						Geometry.CalcPerpendicularBisector(p1.X, p1.Y, p2.X, p2.Y, out aP, out bP, out cP);
						Geometry.CalcLine(p1.X, p1.Y, p2.X, p2.Y, out intA1, out intB1, out intC1);
						Geometry.CalcLine(p3.X, p3.Y, p4.X, p4.Y, out int intA2, out int intB2, out int intC2);
						Geometry.IntersectLineWithLineSegment(intA1, intB1, intC1, p1, p2);
						Geometry.IntersectLineWithLineSegment(intA1, intB1, intC1, p1, p2, out resX, out resY);
						Geometry.IntersectLineWithLineSegment(intA1, intB1, intC1, p1, p2, out float _resX, out float _resY);
						Geometry.IntersectLines(intA1, intB1, intC1, intA2, intB2, intC2, out resX, out resY);
						Geometry.IntersectLines(intA1, intB1, intC1, intA2, intB2, intC2, out int x, out int y);
						Geometry.IntersectLines(intA1, intB1, intC1, intA2, intB2, intC2);
						Geometry.IntersectLines(intA1, intB1, intC1, p3, p4);
					} else {
						skippedFunctionsCount += 10;
						deprecatedCount += 10;
					}
					if (sizeFactor < 100000) {
						Geometry.IntersectLineSegments(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
						Geometry.IntersectLineWithLineSegment(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
						Geometry.IntersectLines(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, p4.X, p4.Y);
						Geometry.CalcDroppedPerpendicularFoot(p3.X, p3.Y, p1.X, p1.Y, p2.X, p2.Y, out int fX, out int fY);
						Geometry.CalcNearestPointOfLineSegment(p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y, out resX, out resY);
					} else {
						skippedFunctionsCount += 5;
					}
				}
				string skippedFunctionsText = (skippedFunctionsCount > 0) ? $"\t\t{skippedFunctionsCount} Functions skipped: \t{deprecatedCount} deprecated, \t{skippedFunctionsCount - deprecatedCount} regular" : string.Empty;
				Trace.WriteLine($"SizeFactor {sizeFactor}: OK {skippedFunctionsText}");
#pragma warning restore 0612, 0618
				//} catch (Exception exc) {
				//	Assert.Fail($"Error whily testing size factor {sizeFactor} ({p1} to {p2}): '{exc.Message}'");
				//}
			}
		}


		// Test for reproducing and fixing special issues. Does not belong to the standard tests.
		// Comment out after fixing the issue!
		[TestMethod]
		public void SpecialTest() {
			Point r = Geometry.IntersectLineSegments(-1, -1, 1, 1, -1, 1, 1, -1);
			Assert.AreEqual(0, r.X);
			Assert.AreEqual(0, r.Y);
			//
			r = Geometry.IntersectLineSegments(-1000000, -1000000, 250000, 1000000, 1000000, -1000000, -250000, 1000000);
			Assert.AreEqual(0, r.X);
			Assert.AreEqual(600000, r.Y);
		}


		/// <summary>
		/// This method is only for testing and comparing speed of calculation functions.
		/// </summary>
		[TestMethod]
		public void SpeedTest() {
			int Iterations = 100000000;
			Stopwatch sw = Stopwatch.StartNew();

			Point getTestPoint1() {
				//return new Point(-1600, -1400);
				return new Point(-800000, -700000);
				//return new Point(-1600000, -1400000);
			};
			Point getTestPoint2() {
				//return new Point(1200, 1300);
				return new Point(600000, 650000);
				//return new Point(1200000, 1300000);
			};
#pragma warning disable 0612, 0618
			// Test integer versions
			{
				Point p1 = getTestPoint1();
				Point p2 = getTestPoint2();
				Geometry.CalcLine(p1.X, p1.Y, p2.X, p2.Y, out int a, out int b, out long c);
				Point[] points = new Point[] { new Point(p1.X, p1.Y), new Point(p2.X, p1.Y), new Point(p2.X, p2.Y), new Point(p1.X, p2.Y) };
				sw.Restart();
				for (int i = 0; i < Iterations; ++i) {
					Geometry.IntersectLines(p1.X, p1.Y, p2.X, p2.Y, -p1.X, -p1.Y, -p2.X, p2.Y, out long _, out long _);
				}
				sw.Stop();
			}
			string result1Text = $"int:   {sw.ElapsedMilliseconds} ms";

			// Test float versions
			{
				PointF p1 = getTestPoint1();
				PointF p2 = getTestPoint2();
				Geometry.CalcLine(p1.X, p1.Y, p2.X, p2.Y, out float a, out float b, out float c);
				PointF[] points = new PointF[] { new PointF(p1.X, p1.Y), new PointF(p2.X, p1.Y), new PointF(p2.X, p2.Y), new PointF(p1.X, p2.Y) };
				sw.Restart();
				for (int i = 0; i < Iterations; ++i) {
					Geometry.IntersectLines(p1.X, p1.Y, p2.X, p2.Y, -p1.X, -p1.Y, -p2.X, p2.Y);
				}
				sw.Stop();
			}
			string result2Text = $"float: {sw.ElapsedMilliseconds} ms";

			Trace.WriteLine(Environment.NewLine);
			Trace.WriteLine(result1Text);
			Trace.WriteLine(result2Text);
			Trace.WriteLine(Environment.NewLine);

			//System.Windows.Forms.MessageBox.Show(result1Text + Environment.NewLine + result2Text);
#pragma warning restore 0612, 0618
		}

	}

}
