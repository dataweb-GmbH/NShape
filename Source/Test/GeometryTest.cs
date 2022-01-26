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
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace NShapeTest {

	[TestClass]
	public class GeometryTests : NShapeTestBase {

		[TestMethod]
		public void CalcDroppedPerpendicularFootTest() {
			int fX, fY;
			//
			Geometry.CalcDroppedPerpendicularFoot(0, 18, 5, 0, 12, 0, out fX, out fY);
			Assert.AreEqual(fX, 0);
			Assert.AreEqual(fY, 0);
			//
			Geometry.CalcDroppedPerpendicularFoot(-2, -2, 4, -17, 4, -16, out fX, out fY);
			Assert.AreEqual(fX, 4);
			Assert.AreEqual(fY, -2);
			//
			Geometry.CalcDroppedPerpendicularFoot(8, 2, -5, 1, 1, 7, out fX, out fY);
			Assert.AreEqual(fX, 2);
			Assert.AreEqual(fY, 8);
		}


		[TestMethod]
		public void CalcNormalVectorOfLineTest() {
			int pX, pY;
			//
			Geometry.CalcNormalVectorOfLine(-2, -4, 2, -4, 0, -4, 100, out pX, out pY);
			Assert.AreEqual(0, pX);
			Assert.AreEqual(-104, pY);
			//
			Geometry.CalcNormalVectorOfLine(-5, -12, -5, -14, -5, 100, 100, out pX, out pY);
			Assert.AreEqual(-105, pX);
			Assert.AreEqual(100, pY);
			//
			Geometry.CalcNormalVectorOfLine(0, 8, 6, 0, 3, 4, 100, out pX, out pY);
			Assert.AreEqual(-77, pX);
			Assert.AreEqual(-56, pY);
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

		}

	}

}