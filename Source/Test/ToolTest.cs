using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

using Dataweb.NShape;
using Dataweb.NShape.Advanced;
using Dataweb.NShape.Controllers;
using Dataweb.NShape.ElectricalShapes;
using Dataweb.NShape.FlowChartShapes;
using Dataweb.NShape.GeneralShapes;
using Dataweb.NShape.SoftwareArchitectureShapes;

using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace NShapeTest {

	[TestClass]
	public class ToolTest : NShapeTestBase {

		#region [Public] Planar Shape Tool Tests

		[TestMethod]
		public void TestPlanarShapeCreationToolCreateShape() {
			// Prepare test
			LoadProjectFile(TestContext.TestName, "Connected Shapes.nspj");
			// Create list of created shapes, register event handler 
			List<Shape> createdShapes = new List<Shape>();
			void Repository_ShapesInserted(object sender, RepositoryShapesEventArgs e) {
				createdShapes.AddRange(e.Shapes);
			}
			Project.Repository.ShapesInserted += Repository_ShapesInserted;
			// Register ToolExecuted event handler
			PlanarShapeCreationTool tool = ToolSetController.FindTool(Project.Repository.GetTemplate("Box")) as PlanarShapeCreationTool;
			Assert.IsNotNull(tool);
			bool toolExecuted = false;
			tool.ToolExecuted += (object sender, ToolExecutedEventArgs e) => {
				toolExecuted = true;
			};

			Assert.AreEqual(false, toolExecuted);
			// Simulate mouse movement, otherwise the tool does not start a new action!
			Point mousePos = new Point(100, 100);
			tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseMove, MouseButtonsDg.None, 0, 0, mousePos, KeysDg.None));
			Assert.AreEqual(true, tool.IsToolActionPending);
			// Simulate click: MouseDown and MouseUp
			tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseDown, MouseButtonsDg.Left, 0, 0, mousePos, KeysDg.None));
			tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseUp, MouseButtonsDg.Left, 0, 0, mousePos, KeysDg.None));
			// Check if the simulated click has executed the tool
			Assert.AreEqual(true, toolExecuted);
			Assert.AreEqual(false, tool.IsToolActionPending);
			// Validate expected shape position
			Assert.AreEqual(1, createdShapes.Count);
			Assert.AreEqual(100, createdShapes[0].X);
			Assert.AreEqual(100, createdShapes[0].Y);
			Rectangle shapeBounds = createdShapes[0].GetBoundingRectangle(true);
			Assert.AreEqual(70, shapeBounds.X);
			Assert.AreEqual(80, shapeBounds.Y);
			Assert.AreEqual(60, shapeBounds.Width);
			Assert.AreEqual(40, shapeBounds.Height);
		}


		[TestMethod]
		public void TestPlanarShapeCreationToolSnapToGrid() {
			// Prepare test
			LoadProjectFile(TestContext.TestName, "Connected Shapes.nspj");
			// Create list of created shapes, register event handler 
			List<Shape> createdShapes = new List<Shape>();
			void Repository_ShapesInserted(object sender, RepositoryShapesEventArgs e) {
				createdShapes.AddRange(e.Shapes);
			}
			Project.Repository.ShapesInserted += Repository_ShapesInserted;
			// Register ToolExecuted event handler
			PlanarShapeCreationTool tool = ToolSetController.FindTool(Project.Repository.GetTemplate("Box")) as PlanarShapeCreationTool;
			Assert.IsNotNull(tool);
			bool toolExecuted = false;
			tool.ToolExecuted += (object sender, ToolExecutedEventArgs e) => {
				toolExecuted = true;
			};

			// Add grid aligned shapes (therefore adjust the grid size and the 'Snap to Grid' distance)
			DiagramPresenter.GridSize = 20;
			DiagramPresenter.SnapDistance = 5;
			int baseX = 60;
			int baseY = 50;
			foreach (ContentAlignment alignment in Enum<ContentAlignment>.GetValues()) {
				int shapeIdx = createdShapes.Count;
				toolExecuted = false;
				// 
				// Determine shape movement and expected result
				Point mousePos = Point.Empty;
				Rectangle expectedShapeBounds = new Rectangle(0, 0, 60, 40);
				switch (alignment) {
					case ContentAlignment.TopLeft:
						mousePos = new Point(baseX - 6, baseY - 6);
						expectedShapeBounds.Location = new Point(20, 20);
						break;
					case ContentAlignment.TopCenter:
						mousePos = new Point(baseX, baseY - 6);
						expectedShapeBounds.Location = new Point(30, 20);
						break;
					case ContentAlignment.TopRight:
						mousePos = new Point(baseX + 6, baseY - 6);
						expectedShapeBounds.Location = new Point(40, 20);
						break;
					case ContentAlignment.MiddleLeft:
						mousePos = new Point(baseX - 6, baseY);
						expectedShapeBounds.Location = new Point(20, 30);
						break;
					case ContentAlignment.MiddleCenter:
						mousePos = new Point(baseX, baseY);
						expectedShapeBounds.Location = new Point(30, 30);
						break;
					case ContentAlignment.MiddleRight:
						mousePos = new Point(baseX + 6, baseY);
						expectedShapeBounds.Location = new Point(40, 30);
						break;
					case ContentAlignment.BottomLeft:
						mousePos = new Point(baseX - 6, baseY + 6);
						expectedShapeBounds.Location = new Point(20, 40);
						break;
					case ContentAlignment.BottomCenter:
						mousePos = new Point(baseX, baseY + 6);
						expectedShapeBounds.Location = new Point(30, 40);
						break;
					case ContentAlignment.BottomRight:
						mousePos = new Point(baseX + 6, baseY + 6);
						expectedShapeBounds.Location = new Point(40, 40);
						break;
					default: throw new NotImplementedException();
				}
				// Simulate mouse movement, otherwise the tool does not start a new action!
				tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseMove, MouseButtonsDg.None, 0, 0, mousePos, KeysDg.None));
				Assert.AreEqual(true, tool.IsToolActionPending);
				tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseDown, MouseButtonsDg.Left, 0, 0, mousePos, KeysDg.None));
				tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseUp, MouseButtonsDg.Left, 0, 0, mousePos, KeysDg.None));
				// Check if the simulated click has executed the tool
				Assert.AreEqual(true, toolExecuted);
				Assert.AreEqual(false, tool.IsToolActionPending);
				Assert.AreEqual(shapeIdx + 1, createdShapes.Count);
				// Validate expected shape position
				Rectangle shapeBounds = createdShapes[shapeIdx].GetBoundingRectangle(true);
				Assert.AreEqual(expectedShapeBounds.X, shapeBounds.X);
				Assert.AreEqual(expectedShapeBounds.Y, shapeBounds.Y);
				Assert.AreEqual(expectedShapeBounds.Width, shapeBounds.Width);
				Assert.AreEqual(expectedShapeBounds.Height, shapeBounds.Height);
			}
			DiagramPresenter.Draw();
			DiagramPresenter.Diagram = null;
		}

		#endregion


		#region [Public] Linear Shape Tool Tests

		[TestMethod]
		public void TestLinearShapeCreationToolCreateLines() {
			// Prepare test
			LoadProjectFile(TestContext.TestName, "Connected Shapes.nspj");
			// Create list of created shapes, register event handler 
			List<PolylineBase> createdShapes = new List<PolylineBase>();
			void Repository_ShapesInserted(object sender, RepositoryShapesEventArgs e) {
				foreach (Shape s in e.Shapes)
					createdShapes.Add((PolylineBase)s);
			}
			Project.Repository.ShapesInserted += Repository_ShapesInserted;
			// Register ToolExecuted event handler
			LinearShapeCreationTool tool = ToolSetController.FindTool(Project.Repository.GetTemplate("Polyline")) as LinearShapeCreationTool;
			Assert.IsNotNull(tool);
			bool toolExecuted = false;
			tool.ToolExecuted += (object sender, ToolExecutedEventArgs e) => {
				toolExecuted = true;
			};
			DiagramPresenter.ActiveTool = tool;

			// Create a bunch of poly lines:
			// - Un-/Connected single-segment line
			// - Un-/Connected multi-segment line
			Point startPoint;
			Point midPoint;
			Point endPoint;
			foreach (bool connectLine in EnumerationHelper.Enumerate(false, true)) {
				foreach (bool createMultipleSegments in EnumerationHelper.Enumerate(false, true)) {
					int shapeIdx = createdShapes.Count;
					toolExecuted = false;
					//
					if (connectLine) {
						int x = createMultipleSegments ? 600 : 420;
						startPoint = new Point(x, 320);
						midPoint = new Point(x + 200, 500);
						endPoint = new Point(x, 680);
					} else {
						startPoint = new Point(20, 20 + shapeIdx * 100);
						midPoint = new Point(50, 20 + shapeIdx * 50);
						endPoint = new Point(80, 20 + shapeIdx * 100);
					}

					Assert.IsFalse(tool.IsToolActionPending);
					tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseMove, MouseButtonsDg.None, 0, 0, startPoint, KeysDg.None));
					tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseDown, MouseButtonsDg.Left, 1, 0, startPoint, KeysDg.None));
					tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseUp, MouseButtonsDg.Left, 1, 0, startPoint, KeysDg.None));
					Assert.IsTrue(tool.IsToolActionPending);
					if (createMultipleSegments) {
						tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseMove, MouseButtonsDg.None, 0, 0, midPoint, KeysDg.None));
						tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseDown, MouseButtonsDg.Left, 1, 0, midPoint, KeysDg.None));
						tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseUp, MouseButtonsDg.Left, 1, 0, midPoint, KeysDg.None));
						Assert.IsTrue(tool.IsToolActionPending);
					}
					tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseMove, MouseButtonsDg.None, 0, 0, endPoint, KeysDg.None));
					tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseDown, MouseButtonsDg.Left, 1, 0, endPoint, KeysDg.None));
					tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseUp, MouseButtonsDg.Left, 1, 0, endPoint, KeysDg.None));
					if (connectLine) {
						// Line creation must be finished automatically by connecting to the partner shapes.
						Assert.IsFalse(tool.IsToolActionPending);
						Assert.IsTrue(toolExecuted);
					} else {
						// Line creation must be finished by a double-click not or a right click
						Assert.IsTrue(tool.IsToolActionPending);
						Assert.IsFalse(toolExecuted);
						// Double-Click
						tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseDown, MouseButtonsDg.Left, 2, 0, endPoint, KeysDg.None));
						tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseUp, MouseButtonsDg.Left, 2, 0, endPoint, KeysDg.None));
						Assert.IsFalse(tool.IsToolActionPending);
						Assert.IsTrue(toolExecuted);
					}
					// Validate shape vertexes and connections (if applicable)
					Assert.AreEqual(shapeIdx + 1, createdShapes.Count);
					PolylineBase line = createdShapes[shapeIdx];
					if (connectLine) {
						ShapeConnectionInfo startConn = line.GetConnectionInfo(ControlPointId.FirstVertex, null);
						Assert.IsFalse(startConn.IsEmpty);
						ShapeConnectionInfo endConn = line.GetConnectionInfo(ControlPointId.LastVertex, null);
						Assert.IsFalse(endConn.IsEmpty);
					} else {
						Assert.AreEqual(startPoint, line.GetControlPointPosition(ControlPointId.FirstVertex));
						if (createMultipleSegments)
							Assert.AreEqual(midPoint, line.GetControlPointPosition(3));
						Assert.AreEqual(endPoint, line.GetControlPointPosition(ControlPointId.LastVertex));
					}
				}
			}
			DiagramPresenter.Draw();
			DiagramPresenter.Diagram = null;
		}


		[TestMethod]
		public void TestLinearShapeCreationToolSnapToGrid() {
			// Prepare test
			LoadProjectFile(TestContext.TestName, "Connected Shapes.nspj");
			// Create list of created shapes, register event handler 
			List<PolylineBase> createdShapes = new List<PolylineBase>();
			void Repository_ShapesInserted(object sender, RepositoryShapesEventArgs e) {
				foreach (Shape s in e.Shapes)
					createdShapes.Add((PolylineBase)s);
			}
			Project.Repository.ShapesInserted += Repository_ShapesInserted;
			// Register ToolExecuted event handler
			LinearShapeCreationTool tool = ToolSetController.FindTool(Project.Repository.GetTemplate("Polyline")) as LinearShapeCreationTool;
			Assert.IsNotNull(tool);
			bool toolExecuted = false;
			tool.ToolExecuted += (object sender, ToolExecutedEventArgs e) => {
				toolExecuted = true;
			};
			DiagramPresenter.ActiveTool = tool;

			// Add grid aligned shapes (therefore adjust the grid size and the 'Snap to Grid' distance)
			DiagramPresenter.GridSize = 20;
			DiagramPresenter.SnapDistance = 5;
			const int BaseX = 60;
			const int BaseY = 60;
			foreach (ContentAlignment alignment in Enum<ContentAlignment>.GetValues()) {
				if (alignment == ContentAlignment.MiddleCenter)
					continue;

				int shapeIdx = createdShapes.Count;
				toolExecuted = false;
				// 
				// Determine shape movement and expected result
				// we have to move the start point i n order to avoid extending the previously created line!
				Point startPoint = Point.Empty;
				Point endPoint = Point.Empty;
				Point expected = Point.Empty;
				switch (alignment) {
					case ContentAlignment.TopLeft:
						startPoint = new Point(BaseX - 40, BaseY - 40);
						endPoint = new Point(startPoint.X - 16, startPoint.Y - 16);
						expected = new Point(startPoint.X - 20, startPoint.Y - 20);
						break;
					case ContentAlignment.TopCenter:
						startPoint = new Point(BaseX, BaseY - 40);
						endPoint = new Point(startPoint.X, startPoint.Y - 16);
						expected = new Point(startPoint.X, startPoint.Y - 20);
						break;
					case ContentAlignment.TopRight:
						startPoint = new Point(BaseX + 40, BaseY - 40);
						endPoint = new Point(startPoint.X + 16, startPoint.Y - 16);
						expected = new Point(startPoint.X + 20, startPoint.Y - 20);
						break;
					case ContentAlignment.MiddleLeft:
						startPoint = new Point(BaseX - 40, BaseY);
						endPoint = new Point(startPoint.X - 16, startPoint.Y);
						expected = new Point(startPoint.X - 20, startPoint.Y);
						break;
					case ContentAlignment.MiddleCenter:
						startPoint = new Point(BaseX, BaseY);
						endPoint = new Point(startPoint.X + 4, startPoint.Y + 4);
						expected = new Point(startPoint.X, startPoint.Y);
						break;
					case ContentAlignment.MiddleRight:
						startPoint = new Point(BaseX + 40, BaseY);
						endPoint = new Point(startPoint.X + 16, startPoint.Y);
						expected = new Point(startPoint.X + 20, startPoint.Y);
						break;
					case ContentAlignment.BottomLeft:
						startPoint = new Point(BaseX - 40, BaseY + 40);
						endPoint = new Point(startPoint.X - 16, startPoint.Y + 16);
						expected = new Point(startPoint.X - 20, startPoint.Y + 20);
						break;
					case ContentAlignment.BottomCenter:
						startPoint = new Point(BaseX, BaseY + 40);
						endPoint = new Point(startPoint.X, startPoint.Y + 16);
						expected = new Point(startPoint.X, startPoint.Y + 20);
						break;
					case ContentAlignment.BottomRight:
						startPoint = new Point(BaseX + 40, BaseY + 40);
						endPoint = new Point(startPoint.X + 16, startPoint.Y + 16);
						expected = new Point(startPoint.X + 20, startPoint.Y + 20);
						break;
					default: throw new NotImplementedException();
				}
				// Simulate mouse movement, otherwise the tool does not start a new action!
				Assert.IsFalse(tool.IsToolActionPending);
				tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseMove, MouseButtonsDg.None, 0, 0, startPoint, KeysDg.None));
				tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseDown, MouseButtonsDg.Left, 1, 0, startPoint, KeysDg.None));
				tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseUp, MouseButtonsDg.Left, 1, 0, startPoint, KeysDg.None));
				Assert.IsTrue(tool.IsToolActionPending);
				// Move mouse near grid line and perform a double-click
				tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseMove, MouseButtonsDg.None, 0, 0, endPoint, KeysDg.None));
				tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseDown, MouseButtonsDg.Left, 1, 0, endPoint, KeysDg.None));
				tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseUp, MouseButtonsDg.Left, 1, 0, endPoint, KeysDg.None));
				tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseDown, MouseButtonsDg.Left, 2, 0, endPoint, KeysDg.None));
				tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseUp, MouseButtonsDg.Left, 2, 0, endPoint, KeysDg.None));
				// Validate result
				Assert.AreEqual(true, toolExecuted);
				Assert.AreEqual(false, tool.IsToolActionPending);
				Assert.AreEqual(shapeIdx + 1, createdShapes.Count);
				Assert.AreEqual(expected, createdShapes[shapeIdx].GetControlPointPosition(ControlPointId.LastVertex), $"Snapping to '{alignment}' failed.");
			}

			DiagramPresenter.Draw();
			DiagramPresenter.Diagram = null;
		}


		[TestMethod]
		public void TestLinearShapeCreationToolSnapToPoints() {
			// Prepare test
			LoadProjectFile(TestContext.TestName, "Connected Shapes.nspj");
			// Create list of created shapes, register event handler 
			List<PolylineBase> createdShapes = new List<PolylineBase>();
			void Repository_ShapesInserted(object sender, RepositoryShapesEventArgs e) {
				foreach (Shape s in e.Shapes)
					createdShapes.Add((PolylineBase)s);
			}
			Project.Repository.ShapesInserted += Repository_ShapesInserted;
			// Register ToolExecuted event handler
			LinearShapeCreationTool tool = ToolSetController.FindTool(Project.Repository.GetTemplate("Polyline")) as LinearShapeCreationTool;
			Assert.IsNotNull(tool);
			bool toolExecuted = false;
			tool.ToolExecuted += (object sender, ToolExecutedEventArgs e) => {
				toolExecuted = true;
			};
			DiagramPresenter.ActiveTool = tool;

			// Snap to control points:
			// - Disable Snap-to-Grid
			// - enlarge the grip size (which defines the snap range)
			DiagramPresenter.GridSize = 20;
			DiagramPresenter.SnapDistance = 0;
			DiagramPresenter.GripSize = 5;
			//
			Ellipse targetShape = DiagramPresenter.Diagram.Shapes.FindShape(350, 440, ControlPointCapabilities.Reference, 0, null) as Ellipse;
			Assert.IsNotNull(targetShape);
			targetShape.MoveTo(200, 100);
			Rectangle targetShapeBounds = targetShape.GetBoundingRectangle(true);

			foreach (ContentAlignment alignment in Enum<ContentAlignment>.GetValues()) {
				int shapeIdx = createdShapes.Count;
				toolExecuted = false;
				// 
				// Determine shape movement and expected result
				// we have to move the start point i n order to avoid extending the previously created line!
				Point startPoint = Point.Empty;
				Point endPoint = Point.Empty;
				Point expected = Point.Empty;
				switch (alignment) {
					case ContentAlignment.TopLeft:
						expected = targetShape.GetControlPointPosition(Ellipse.ControlPointIds.TopLeftConnectionPoint);
						startPoint = new Point(targetShapeBounds.Left - 20, targetShapeBounds.Top - 20);
						endPoint = new Point(expected.X - 2, expected.Y - 2);
						break;
					case ContentAlignment.TopCenter:
						expected = targetShape.GetControlPointPosition(Ellipse.ControlPointIds.TopCenterControlPoint);
						startPoint = new Point(targetShapeBounds.GetCenter().X, targetShapeBounds.Top - 20);
						endPoint = new Point(expected.X, expected.Y - 2);
						break;
					case ContentAlignment.TopRight:
						expected = targetShape.GetControlPointPosition(Ellipse.ControlPointIds.TopRightConnectionPoint);
						startPoint = new Point(targetShapeBounds.Right + 20, targetShapeBounds.Top - 20);
						endPoint = new Point(expected.X + 2, expected.Y - 2);
						break;
					case ContentAlignment.MiddleLeft:
						expected = targetShape.GetControlPointPosition(Ellipse.ControlPointIds.MiddleLeftControlPoint);
						startPoint = new Point(targetShapeBounds.Left - 20, targetShapeBounds.GetCenter().Y);
						endPoint = new Point(expected.X - 2, expected.Y);
						break;
					case ContentAlignment.MiddleCenter:
						expected = targetShape.GetControlPointPosition(Ellipse.ControlPointIds.MiddleCenterControlPoint);
						startPoint = new Point(targetShapeBounds.Left - 30, targetShapeBounds.GetCenter().Y - 10);
						endPoint = new Point(expected.X - 2, expected.Y);
						break;
					case ContentAlignment.MiddleRight:
						expected = targetShape.GetControlPointPosition(Ellipse.ControlPointIds.MiddleRightControlPoint);
						startPoint = new Point(targetShapeBounds.Left + 20, targetShapeBounds.GetCenter().Y);
						endPoint = new Point(expected.X + 2, expected.Y);
						break;
					case ContentAlignment.BottomLeft:
						expected = targetShape.GetControlPointPosition(Ellipse.ControlPointIds.BottomLeftConnectionPoint);
						startPoint = new Point(targetShapeBounds.Left - 20, targetShapeBounds.Bottom + 20);
						endPoint = new Point(expected.X - 2, expected.Y + 2);
						break;
					case ContentAlignment.BottomCenter:
						expected = targetShape.GetControlPointPosition(Ellipse.ControlPointIds.BottomCenterControlPoint);
						startPoint = new Point(targetShapeBounds.GetCenter().X, targetShapeBounds.Bottom + 20);
						endPoint = new Point(expected.X, expected.Y + 2);
						break;
					case ContentAlignment.BottomRight:
						expected = targetShape.GetControlPointPosition(Ellipse.ControlPointIds.BottomRightConnectionPoint);
						startPoint = new Point(targetShapeBounds.Right + 20, targetShapeBounds.Bottom + 20);
						endPoint = new Point(expected.X + 2, expected.Y + 2);
						break;
					default: throw new NotImplementedException();
				}
				// Simulate mouse movement, otherwise the tool does not start a new action!
				Assert.IsFalse(tool.IsToolActionPending);
				tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseMove, MouseButtonsDg.None, 0, 0, startPoint, KeysDg.None));
				tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseDown, MouseButtonsDg.Left, 1, 0, startPoint, KeysDg.None));
				tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseUp, MouseButtonsDg.Left, 1, 0, startPoint, KeysDg.None));
				Assert.IsTrue(tool.IsToolActionPending);
				// Move mouse near grid line and perform a double-click
				tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseMove, MouseButtonsDg.None, 0, 0, endPoint, KeysDg.None));
				tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseDown, MouseButtonsDg.Left, 1, 0, endPoint, KeysDg.None));
				tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseUp, MouseButtonsDg.Left, 1, 0, endPoint, KeysDg.None));
				// Validate result
				Assert.AreEqual(true, toolExecuted);
				Assert.AreEqual(false, tool.IsToolActionPending);
				Assert.AreEqual(shapeIdx + 1, createdShapes.Count);
				Assert.AreEqual(expected, createdShapes[shapeIdx].GetControlPointPosition(ControlPointId.LastVertex), $"Snapping to '{alignment}' failed.");
			}

			DiagramPresenter.Draw();
			DiagramPresenter.Diagram = null;
		}


		[TestMethod]
		public void TestLinearShapeCreationToolExtendLine() {
			// Prepare test
			LoadProjectFile(TestContext.TestName, "Connected Shapes.nspj");
			// Create list of created shapes, register event handler 
			List<PolylineBase> createdShapes = new List<PolylineBase>();
			void Repository_ShapesInserted(object sender, RepositoryShapesEventArgs e) {
				foreach (Shape s in e.Shapes)
					createdShapes.Add((PolylineBase)s);
			}
			Project.Repository.ShapesInserted += Repository_ShapesInserted;
			// Register ToolExecuted event handler
			LinearShapeCreationTool tool = ToolSetController.FindTool(Project.Repository.GetTemplate("Polyline")) as LinearShapeCreationTool;
			Assert.IsNotNull(tool);
			bool toolExecuted = false;
			tool.ToolExecuted += (object sender, ToolExecutedEventArgs e) => {
				toolExecuted = true;
			};
			DiagramPresenter.ActiveTool = tool;

			DiagramPresenter.GridSize = 20;
			DiagramPresenter.SnapDistance = 5;
			DiagramPresenter.GripSize = 5;
			//
			Point startPos = new Point(100, 400);
			Point mousePos = startPos;
			// Start line
			Assert.IsFalse(tool.IsToolActionPending);
			tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseMove, MouseButtonsDg.None, 0, 0, mousePos, KeysDg.None));
			tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseDown, MouseButtonsDg.Left, 1, 0, mousePos, KeysDg.None));
			tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseUp, MouseButtonsDg.Left, 1, 0, mousePos, KeysDg.None));
			Assert.IsTrue(tool.IsToolActionPending);
			// Move mouse and finish line
			mousePos.Offset(20, 200);
			Assert.IsTrue(tool.IsToolActionPending);
			tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseMove, MouseButtonsDg.None, 0, 0, mousePos, KeysDg.None));
			tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseDown, MouseButtonsDg.Left, 1, 0, mousePos, KeysDg.None));
			tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseUp, MouseButtonsDg.Left, 1, 0, mousePos, KeysDg.None));
			tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseDown, MouseButtonsDg.Left, 2, 0, mousePos, KeysDg.None));
			tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseUp, MouseButtonsDg.Left, 2, 0, mousePos, KeysDg.None));
			Assert.IsFalse(tool.IsToolActionPending);
			// Extend line at start point
			mousePos = startPos;
			tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseMove, MouseButtonsDg.None, 0, 0, mousePos, KeysDg.None));
			tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseDown, MouseButtonsDg.Left, 1, 0, mousePos, KeysDg.None));
			tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseUp, MouseButtonsDg.Left, 1, 0, mousePos, KeysDg.None));
			// Move mouse and finish line (double-click)
			mousePos.Offset(20, -200);
			Assert.IsTrue(tool.IsToolActionPending);
			tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseMove, MouseButtonsDg.None, 0, 0, mousePos, KeysDg.None));
			tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseDown, MouseButtonsDg.Left, 1, 0, mousePos, KeysDg.None));
			tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseUp, MouseButtonsDg.Left, 1, 0, mousePos, KeysDg.None));
			tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseDown, MouseButtonsDg.Left, 2, 0, mousePos, KeysDg.None));
			tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseUp, MouseButtonsDg.Left, 2, 0, mousePos, KeysDg.None));
			Assert.IsFalse(tool.IsToolActionPending);
			Assert.IsTrue(toolExecuted);
			Assert.AreEqual(1, createdShapes.Count);

			DiagramPresenter.Draw();
			DiagramPresenter.Diagram = null;
		}

		#endregion


		#region [Public] Selection Tool Tests

		[TestMethod]
		public void TestSelectionToolMoveShapes() {
			// Prepare test
			LoadProjectFile(TestContext.TestName, "Connected Shapes.nspj");
			//
			// Register ToolExecuted event handler
			SelectionTool tool = ToolSetController.DefaultTool as SelectionTool;
			Assert.IsNotNull(tool);
			bool toolExecuted = false;
			tool.ToolExecuted += (object sender, ToolExecutedEventArgs e) => {
				toolExecuted = true;
			};

			// Add grid aligned shapes (therefore adjust the grid size and the 'Snap to Grid' distance)
			DiagramPresenter.GridSize = 20;
			DiagramPresenter.SnapDistance = 5;
			DiagramPresenter.ActiveTool = tool;
			{
				// In the "Connected Shapes" diagram, this is the position of the blue ellipse in the center
				int baseX = 510;
				int baseY = 440;
				// Get the elllipse shape and check if the shape is at the expected coordinates
				Ellipse ellipse = DiagramPresenter.Diagram.Shapes.FindShape(baseX, baseY, ControlPointCapabilities.Reference, 0, null) as Ellipse;
				Assert.IsNotNull(ellipse);
				Assert.AreEqual(ellipse.X, baseX);
				Assert.AreEqual(ellipse.Y, baseY);

				foreach (ContentAlignment alignment in Enum<ContentAlignment>.GetValues()) {
					if (alignment == ContentAlignment.MiddleCenter)
						continue;
					toolExecuted = false;

					Assert.AreEqual(ellipse.X, baseX);
					Assert.AreEqual(ellipse.Y, baseY);
					// 
					// Determine shape movement and expected result
					//Point basepos = new Point(baseX, baseY);
					Point dragStartPos = new Point(baseX, baseY);
					Point dragEndPos = Point.Empty;
					Rectangle expectedShapeBounds = ellipse.GetBoundingRectangle(true);
					switch (alignment) {
						case ContentAlignment.TopLeft:
							dragEndPos = new Point(baseX - 16, baseY - 16);
							expectedShapeBounds.Offset(-20, -20);
							break;
						case ContentAlignment.TopCenter:
							dragEndPos = new Point(baseX, baseY - 16);
							expectedShapeBounds.Offset(0, -20);
							break;
						case ContentAlignment.TopRight:
							dragEndPos = new Point(baseX + 16, baseY - 16);
							expectedShapeBounds.Offset(20, -20);
							break;
						case ContentAlignment.MiddleLeft:
							dragEndPos = new Point(baseX - 16, baseY);
							expectedShapeBounds.Offset(-20, 0);
							break;
						case ContentAlignment.MiddleRight:
							dragEndPos = new Point(baseX + 16, baseY);
							expectedShapeBounds.Offset(20, 0);
							break;
						case ContentAlignment.BottomLeft:
							dragEndPos = new Point(baseX - 16, baseY + 16);
							expectedShapeBounds.Offset(-20, 20);
							break;
						case ContentAlignment.BottomCenter:
							dragEndPos = new Point(baseX, baseY + 16);
							expectedShapeBounds.Offset(0, 20);
							break;
						case ContentAlignment.BottomRight:
							dragEndPos = new Point(baseX + 16, baseY + 16);
							expectedShapeBounds.Offset(20, 20);
							break;
						default: throw new NotImplementedException();
					}
					// Offset the mouse position, do not grab the shape in the center but offset a bit!
					dragStartPos.Offset(-15, -10);
					dragEndPos.Offset(-15, -10);
					//
					// Unselect all shapes - the shape will be selected when dragging
					DiagramPresenter.UnselectAll();
					// Move mouse in position and start a drag action.
					Assert.IsFalse(tool.IsToolActionPending);
					tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseMove, MouseButtonsDg.None, 0, 0, dragStartPos, KeysDg.None));
					tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseDown, MouseButtonsDg.Left, 1, 0, dragStartPos, KeysDg.None));
					// Drag shape (left mouse button pressed)
					foreach (Point mousePos in Interpolate(dragStartPos, dragEndPos, 8))
						tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseMove, MouseButtonsDg.Left, 1, 0, mousePos, KeysDg.None));
					tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseMove, MouseButtonsDg.Left, 1, 0, dragEndPos, KeysDg.None));
					Assert.IsTrue(tool.IsToolActionPending);
					// End drag action
					tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseUp, MouseButtonsDg.Left, 1, 0, dragEndPos, KeysDg.None));
					// Check if the simulated click has executed the tool
					Assert.IsFalse(tool.IsToolActionPending);
					Assert.IsTrue(toolExecuted);
					//
					// Validate expected shape position
					Rectangle shapeBounds = ellipse.GetBoundingRectangle(true);
					Assert.AreEqual(expectedShapeBounds.X, shapeBounds.X);
					Assert.AreEqual(expectedShapeBounds.Y, shapeBounds.Y);
					Assert.AreEqual(expectedShapeBounds.Width, shapeBounds.Width);
					Assert.AreEqual(expectedShapeBounds.Height, shapeBounds.Height);
					//
					DiagramPresenter.Draw($"{TestContext.TestName}_{alignment}.png");

					// Revert the changes
					if (Project.History.UndoCommandCount > 0)
						Project.History.Undo();
				}
			}

			// Additional test: Move an unselected line by just dragging it somewhere (without selecting it before)
			{
				Template template = Project.Repository.GetTemplate("Polyline");
				Polyline line = template.CreateShape<Polyline>();
				DiagramPresenter.InsertShape(line);

				line.MoveControlPointTo(ControlPointId.FirstVertex, 20, 100, ResizeModifiers.None);
				line.MoveControlPointTo(ControlPointId.LastVertex, 80, 100, ResizeModifiers.None);
				Assert.AreEqual(20, line.X);
				Assert.AreEqual(100, line.Y);

				Point mousePosFrom = new Point(60, 100);
				Point mousePosTo = new Point(60, 140);
				// Unselect all shapes - the shape will be selected when dragging
				DiagramPresenter.UnselectAll();
				// Move mouse in position and start a drag action.
				Assert.IsFalse(tool.IsToolActionPending);
				tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseMove, MouseButtonsDg.None, 0, 0, mousePosFrom, KeysDg.None));
				tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseDown, MouseButtonsDg.Left, 1, 0, mousePosFrom, KeysDg.None));
				// Drag shape (left mouse button pressed)
				foreach (Point mousePos in Interpolate(mousePosFrom, mousePosTo, 10))
					tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseMove, MouseButtonsDg.Left, 1, 0, mousePos, KeysDg.None));
				tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseMove, MouseButtonsDg.Left, 1, 0, mousePosTo, KeysDg.None));
				Assert.IsTrue(tool.IsToolActionPending);
				// End drag action
				tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseUp, MouseButtonsDg.Left, 1, 0, mousePosTo, KeysDg.None));
				// Check if the simulated click has executed the tool
				Assert.IsFalse(tool.IsToolActionPending);
				Assert.IsTrue(toolExecuted);
				//
				// Validate expected shape position
				Assert.AreEqual(20, line.X);
				Assert.AreEqual(140, line.Y);
				//
				DiagramPresenter.Draw();
			}

			DiagramPresenter.Diagram = null;
		}


		[TestMethod]
		public void TestSelectionToolResizeShapes() {
			// Prepare test
			LoadProjectFile(TestContext.TestName, "Connected Shapes.nspj");
			//
			// Register ToolExecuted event handler
			SelectionTool tool = ToolSetController.DefaultTool as SelectionTool;
			Assert.IsNotNull(tool);
			bool toolExecuted = false;
			tool.ToolExecuted += (object sender, ToolExecutedEventArgs e) => {
				toolExecuted = true;
			};

			// Add grid aligned shapes (therefore adjust the grid size and the 'Snap to Grid' distance)
			DiagramPresenter.GridSize = 20;
			DiagramPresenter.SnapDistance = 5;
			DiagramPresenter.ActiveTool = tool;
			//
			// In the "Connected Shapes" diagram, this is the position of the blue ellipse in the center
			int baseX = 510;
			int baseY = 440;
			// Get the elllipse shape and check if the shape is at the expected coordinates
			Ellipse ellipse = DiagramPresenter.Diagram.Shapes.FindShape(baseX, baseY, ControlPointCapabilities.Reference, 0, null) as Ellipse;
			Assert.IsNotNull(ellipse);
			Assert.AreEqual(ellipse.X, baseX);
			Assert.AreEqual(ellipse.Y, baseY);

			foreach (ContentAlignment alignment in Enum<ContentAlignment>.GetValues()) {
				if (alignment == ContentAlignment.MiddleCenter)
					continue;
				toolExecuted = false;

				Assert.AreEqual(ellipse.X, baseX);
				Assert.AreEqual(ellipse.Y, baseY);
				// 
				// Determine shape movement and expected result
				//Point basepos = new Point(baseX, baseY);
				Point dragStartPos = Point.Empty;
				Point dragEndPos = Point.Empty;
				Rectangle expectedShapeBounds = ellipse.GetBoundingRectangle(true);
				switch (alignment) {
					case ContentAlignment.TopLeft:
						dragStartPos = ellipse.GetControlPointPosition(Ellipse.ControlPointIds.TopLeftControlPoint);
						dragEndPos = new Point(dragStartPos.X - 16, dragStartPos.Y - 16);
						expectedShapeBounds.Offset(-20, -20);
						expectedShapeBounds.Width += 20;
						expectedShapeBounds.Height += 20;
						break;
					case ContentAlignment.TopCenter:
						dragStartPos = ellipse.GetControlPointPosition(Ellipse.ControlPointIds.TopCenterControlPoint);
						dragEndPos = new Point(dragStartPos.X, dragStartPos.Y - 16);
						expectedShapeBounds.Offset(0, -20);
						expectedShapeBounds.Height += 20;
						break;
					case ContentAlignment.TopRight:
						dragStartPos = ellipse.GetControlPointPosition(Ellipse.ControlPointIds.TopRightControlPoint);
						dragEndPos = new Point(dragStartPos.X + 16, dragStartPos.Y - 16);
						expectedShapeBounds.Offset(0, -20);
						expectedShapeBounds.Width += 20;
						expectedShapeBounds.Height += 20;
						break;
					case ContentAlignment.MiddleLeft:
						dragStartPos = ellipse.GetControlPointPosition(Ellipse.ControlPointIds.MiddleLeftControlPoint);
						dragEndPos = new Point(dragStartPos.X - 16, dragStartPos.Y);
						expectedShapeBounds.Offset(-20, 0);
						expectedShapeBounds.Width += 20;
						break;
					case ContentAlignment.MiddleRight:
						dragStartPos = ellipse.GetControlPointPosition(Ellipse.ControlPointIds.MiddleRightControlPoint);
						dragEndPos = new Point(dragStartPos.X + 16, dragStartPos.Y);
						expectedShapeBounds.Width += 20;
						break;
					case ContentAlignment.BottomLeft:
						dragStartPos = ellipse.GetControlPointPosition(Ellipse.ControlPointIds.BottomLeftControlPoint);
						dragEndPos = new Point(dragStartPos.X - 16, dragStartPos.Y + 16);
						expectedShapeBounds.Offset(-20, 0);
						expectedShapeBounds.Width += 20;
						expectedShapeBounds.Height += 20;
						break;
					case ContentAlignment.BottomCenter:
						dragStartPos = ellipse.GetControlPointPosition(Ellipse.ControlPointIds.BottomCenterControlPoint);
						dragEndPos = new Point(dragStartPos.X, dragStartPos.Y + 16);
						expectedShapeBounds.Height += 20;
						break;
					case ContentAlignment.BottomRight:
						dragStartPos = ellipse.GetControlPointPosition(Ellipse.ControlPointIds.BottomRightControlPoint);
						dragEndPos = new Point(dragStartPos.X + 16, dragStartPos.Y + 16);
						expectedShapeBounds.Width += 20;
						expectedShapeBounds.Height += 20;
						break;
					default: throw new NotImplementedException();
				}
				// Offset the mouse position, do not grab the shape handle in the middle but offset a bit!
				dragStartPos.Offset(-2, -2);
				dragEndPos.Offset(-2, -2);
				//
				// Select the shape - otherwise the handles would not be visible (and the tool does not consider resizing when starting the drag action)
				DiagramPresenter.SelectShape(ellipse);
				// Move mouse in position and start a drag action.
				Assert.IsFalse(tool.IsToolActionPending);
				tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseMove, MouseButtonsDg.None, 0, 0, dragStartPos, KeysDg.None));
				tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseDown, MouseButtonsDg.Left, 1, 0, dragStartPos, KeysDg.None));
				// Drag shape (left mouse button pressed)
				foreach (Point mousePos in Interpolate(dragStartPos, dragEndPos, 8))
					tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseMove, MouseButtonsDg.Left, 1, 0, mousePos, KeysDg.None));
				tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseMove, MouseButtonsDg.Left, 1, 0, dragEndPos, KeysDg.None));
				Assert.IsTrue(tool.IsToolActionPending);
				// End drag action
				tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseUp, MouseButtonsDg.Left, 1, 0, dragEndPos, KeysDg.None));
				// Check if the simulated click has executed the tool
				Assert.IsFalse(tool.IsToolActionPending);
				Assert.IsTrue(toolExecuted);
				//
				// Validate expected shape position
				Rectangle shapeBounds = ellipse.GetBoundingRectangle(true);
				Assert.AreEqual(expectedShapeBounds.X, shapeBounds.X);
				Assert.AreEqual(expectedShapeBounds.Y, shapeBounds.Y);
				Assert.AreEqual(expectedShapeBounds.Width, shapeBounds.Width);
				Assert.AreEqual(expectedShapeBounds.Height, shapeBounds.Height);
				//
				DiagramPresenter.Draw($"{TestContext.TestName}_{alignment}.png");

				// Revert the changes
				if (Project.History.UndoCommandCount > 0)
					Project.History.Undo();
			}
			DiagramPresenter.Diagram = null;
		}


		[TestMethod]
		public void TestSelectionToolRotateShapes() {
			// Prepare test
			LoadProjectFile(TestContext.TestName, "Connected Shapes.nspj");
			//
			// Register ToolExecuted event handler
			SelectionTool tool = ToolSetController.DefaultTool as SelectionTool;
			Assert.IsNotNull(tool);
			bool toolExecuted = false;
			tool.ToolExecuted += (object sender, ToolExecutedEventArgs e) => {
				toolExecuted = true;
			};

			// Add grid aligned shapes (therefore adjust the grid size and the 'Snap to Grid' distance)
			DiagramPresenter.GridSize = 20;
			DiagramPresenter.SnapDistance = 5;
			DiagramPresenter.ActiveTool = tool;
			//
			// In the "Connected Shapes" diagram, this is the position of the blue ellipse in the center
			int baseX = 510;
			int baseY = 440;
			// Get the elllipse shape and check if the shape is at the expected coordinates
			Ellipse ellipse = DiagramPresenter.Diagram.Shapes.FindShape(baseX, baseY, ControlPointCapabilities.Reference, 0, null) as Ellipse;
			Assert.IsNotNull(ellipse);
			Assert.AreEqual(ellipse.X, baseX);
			Assert.AreEqual(ellipse.Y, baseY);

			// Select the shape - otherwise the handles would not be visible (and the tool does not consider resizing when starting the drag action)
			DiagramPresenter.SelectShape(ellipse);
			// Move mouse in position and start a drag action.
			Assert.IsFalse(tool.IsToolActionPending);
			Point dragStartPos = new Point(baseX, baseY);
			Point dragEndPos = new Point(baseX + 100, baseY);
			tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseMove, MouseButtonsDg.None, 0, 0, dragStartPos, KeysDg.None));
			tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseDown, MouseButtonsDg.Left, 1, 0, dragStartPos, KeysDg.None));
			// Drag shape (left mouse button pressed)
			foreach (Point mousePos in Interpolate(dragStartPos, dragEndPos, 10))
				tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseMove, MouseButtonsDg.Left, 1, 0, mousePos, KeysDg.None));
			tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseMove, MouseButtonsDg.Left, 1, 0, dragEndPos, KeysDg.None));
			dragStartPos = dragEndPos;
			dragEndPos.Y += 100;
			foreach (Point mousePos in Interpolate(dragStartPos, dragEndPos, 10))
				tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseMove, MouseButtonsDg.Left, 1, 0, mousePos, KeysDg.None));
			tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseMove, MouseButtonsDg.Left, 1, 0, dragEndPos, KeysDg.None));
			dragStartPos = dragEndPos;
			dragEndPos.X -= 100;
			foreach (Point mousePos in Interpolate(dragStartPos, dragEndPos, 10))
				tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseMove, MouseButtonsDg.Left, 1, 0, mousePos, KeysDg.None));
			tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseMove, MouseButtonsDg.Left, 1, 0, dragEndPos, KeysDg.None));
			Assert.IsTrue(tool.IsToolActionPending);
			// End drag action
			tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseUp, MouseButtonsDg.Left, 1, 0, dragEndPos, KeysDg.None));
			// Check if the simulated click has executed the tool
			Assert.IsFalse(tool.IsToolActionPending);
			Assert.IsTrue(toolExecuted);
			//
			// Validate expected shape position
			Rectangle shapeBounds = ellipse.GetBoundingRectangle(true);
			Assert.AreEqual(ellipse.Height, shapeBounds.Width);
			Assert.AreEqual(ellipse.Width, shapeBounds.Height);
			Assert.AreEqual(ellipse.Angle, 900);
			//
			DiagramPresenter.Draw();

			DiagramPresenter.SelectShape(ellipse);
			DiagramPresenter.Draw();
			DiagramPresenter.Diagram = null;
		}


		[TestMethod]
		public void TestSelectionToolConnectLineToShape() {
			// Prepare test
			LoadProjectFile(TestContext.TestName, "Connected Shapes.nspj");
			//
			// Register ToolExecuted event handler
			SelectionTool tool = ToolSetController.DefaultTool as SelectionTool;
			Assert.IsNotNull(tool);
			bool toolExecuted = false;
			tool.ToolExecuted += (object sender, ToolExecutedEventArgs e) => {
				toolExecuted = true;
			};

			// Add grid aligned shapes (therefore adjust the grid size and the 'Snap to Grid' distance)
			DiagramPresenter.GridSize = 20;
			DiagramPresenter.SnapDistance = 5;
			DiagramPresenter.ActiveTool = tool;
			//
			// Strategy:
			// - disconnect a line from an ellipse.
			// - (re-)connect the line to another shape...
			// - ...via point-to-point or point-to-shape connection
			Point lineCoordinates = new Point(470, 380);
			ControlPointId gluePointId = ControlPointId.LastVertex;
			Point initialGluePointCoordinates = new Point(498, 422);
			Point blueEllipseCoordinates = new Point(510, 440);
			Point yellowEllipseCoordinates = new Point(510, 560);
			Point disconnectLineCoordinates = new Point(440, 400);
			Point snapToShapeCoordinates = new Point(519, 582);
			Point expectedShapeConnectionCoordinates = new Point(503, 540);
			//
			Polyline line = DiagramPresenter.Diagram.Shapes.FindShape(lineCoordinates.X, lineCoordinates.Y, ControlPointCapabilities.None, 0, null) as Polyline;
			Assert.IsNotNull(line);
			Ellipse blueEllipse = DiagramPresenter.Diagram.Shapes.FindShape(blueEllipseCoordinates.X, blueEllipseCoordinates.Y, ControlPointCapabilities.None, 0, null) as Ellipse;
			Assert.IsNotNull(blueEllipse);
			Ellipse yellowEllipse = DiagramPresenter.Diagram.Shapes.FindShape(yellowEllipseCoordinates.X, yellowEllipseCoordinates.Y, ControlPointCapabilities.None, 0, null) as Ellipse;
			Assert.IsNotNull(yellowEllipse);
			foreach (bool reconnectDirectly in EnumerationHelper.Enumerate(false, true)) {
				string testFailedMessage = reconnectDirectly ? "Reconnect shape directly failed" : "disconnect and reconnect shape failed";
				toolExecuted = false;
				// Check if the shapes are in the expected state
				Assert.AreEqual(initialGluePointCoordinates, line.GetControlPointPosition(gluePointId));
				Assert.AreEqual<ControlPointId>(ControlPointId.Reference, line.IsConnected(ControlPointId.Any, blueEllipse));

				// Disconnect the line:
				// Select the shape - otherwise the handles would not be visible (and the tool does not consider resizing when starting the drag action)
				DiagramPresenter.SelectShape(line);
				Assert.IsFalse(tool.IsToolActionPending, testFailedMessage);
				Point dragStartPos = initialGluePointCoordinates;
				Point dragEndPos = reconnectDirectly ? snapToShapeCoordinates : new Point(443, 402);
				// Move mouse in position and start a drag action.
				tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseMove, MouseButtonsDg.None, 0, 0, dragStartPos, KeysDg.None));
				tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseDown, MouseButtonsDg.Left, 1, 0, dragStartPos, KeysDg.None));
				// Drag mouse to target location and release mouse button
				foreach (Point mousePos in Interpolate(dragStartPos, dragEndPos, 4))
					tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseMove, MouseButtonsDg.Left, 1, 0, mousePos, KeysDg.None));
				tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseMove, MouseButtonsDg.Left, 1, 0, dragEndPos, KeysDg.None));
				tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseUp, MouseButtonsDg.Left, 1, 0, dragEndPos, KeysDg.None));
				Assert.IsFalse(tool.IsToolActionPending, testFailedMessage);
				Assert.IsTrue(toolExecuted, testFailedMessage);
				Point currentGluePointPos = line.GetControlPointPosition(gluePointId);
				if (reconnectDirectly == false) {
					Assert.AreNotEqual(dragEndPos, currentGluePointPos, testFailedMessage);
					Assert.AreEqual(disconnectLineCoordinates.X, currentGluePointPos.X, testFailedMessage);
					Assert.AreEqual(disconnectLineCoordinates.Y, currentGluePointPos.Y, testFailedMessage);

					dragStartPos = currentGluePointPos;
					dragEndPos = snapToShapeCoordinates;
					// Move mouse in position and start a drag action.
					toolExecuted = false;
					Assert.IsFalse(tool.IsToolActionPending, testFailedMessage);
					tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseMove, MouseButtonsDg.None, 0, 0, dragStartPos, KeysDg.None));
					tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseDown, MouseButtonsDg.Left, 1, 0, dragStartPos, KeysDg.None));
					Assert.IsTrue(tool.IsToolActionPending, testFailedMessage);
					// Drag mouse to target location and release mouse button
					foreach (Point mousePos in Interpolate(dragStartPos, dragEndPos, 4))
						tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseMove, MouseButtonsDg.Left, 1, 0, mousePos, KeysDg.None));
					tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseMove, MouseButtonsDg.Left, 1, 0, dragEndPos, KeysDg.None));
					tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseUp, MouseButtonsDg.Left, 1, 0, dragEndPos, KeysDg.None));
					Assert.IsFalse(tool.IsToolActionPending, testFailedMessage);
					Assert.IsTrue(toolExecuted, testFailedMessage);
					// Update current glue point position
					currentGluePointPos = line.GetControlPointPosition(gluePointId);
				}
				DiagramPresenter.Draw();
				// Validate the line's point-to-shape connection
				Assert.AreNotEqual(dragEndPos, currentGluePointPos, testFailedMessage);
				Assert.AreEqual<ControlPointId>(ControlPointId.Reference, line.IsConnected(ControlPointId.Any, yellowEllipse), testFailedMessage);
				Assert.AreEqual(expectedShapeConnectionCoordinates, currentGluePointPos, testFailedMessage);

				Assert.IsTrue(Project.History.UndoCommandCount > 0);
				Project.History.Undo(Project.History.UndoCommandCount);
				Assert.IsTrue(Project.History.UndoCommandCount == 0);
			}
			DiagramPresenter.Diagram = null;
		}


		[TestMethod]
		public void TestSelectionToolConnectLineToConnectionPoint() {
			// Prepare test
			LoadProjectFile(TestContext.TestName, "Connected Shapes.nspj");
			//
			// Register ToolExecuted event handler
			SelectionTool tool = ToolSetController.DefaultTool as SelectionTool;
			Assert.IsNotNull(tool);
			bool toolExecuted = false;
			tool.ToolExecuted += (object sender, ToolExecutedEventArgs e) => {
				toolExecuted = true;
			};

			// Add grid aligned shapes (therefore adjust the grid size and the 'Snap to Grid' distance)
			DiagramPresenter.GridSize = 20;
			DiagramPresenter.SnapDistance = 5;
			DiagramPresenter.ActiveTool = tool;
			//
			// Strategy:
			// - disconnect a line from an ellipse.
			// - (re-)connect the line to another shape...
			// - ...via point-to-point or point-to-shape connection
			Point lineCoordinates = new Point(470, 380);
			ControlPointId gluePointId = ControlPointId.LastVertex;
			Point initialGluePointCoordinates = new Point(498, 422);
			Point blueEllipseCoordinates = new Point(510, 440);
			Point yellowEllipseCoordinates = new Point(510, 560);
			Point disconnectLineCoordinates = new Point(440, 400);
			Point snapToConnectionPointCoordinates = new Point(488, 545);
			Point expectedShapeConnectionCoordinates = new Point(489, 546);
			//
			Polyline line = DiagramPresenter.Diagram.Shapes.FindShape(lineCoordinates.X, lineCoordinates.Y, ControlPointCapabilities.None, 0, null) as Polyline;
			Assert.IsNotNull(line);
			Ellipse blueEllipse = DiagramPresenter.Diagram.Shapes.FindShape(blueEllipseCoordinates.X, blueEllipseCoordinates.Y, ControlPointCapabilities.None, 0, null) as Ellipse;
			Assert.IsNotNull(blueEllipse);
			Ellipse yellowEllipse = DiagramPresenter.Diagram.Shapes.FindShape(yellowEllipseCoordinates.X, yellowEllipseCoordinates.Y, ControlPointCapabilities.None, 0, null) as Ellipse;
			Assert.IsNotNull(yellowEllipse);
			foreach (bool reconnectDirectly in EnumerationHelper.Enumerate(false, true)) {
				toolExecuted = false;
				// Check if the shapes are in the expected state
				Assert.AreEqual(initialGluePointCoordinates, line.GetControlPointPosition(gluePointId));
				Assert.AreEqual<ControlPointId>(ControlPointId.Reference, line.IsConnected(ControlPointId.Any, blueEllipse));

				// Disconnect the line:
				// Select the shape - otherwise the handles would not be visible (and the tool does not consider resizing when starting the drag action)
				DiagramPresenter.SelectShape(line);
				Assert.IsFalse(tool.IsToolActionPending);
				Point dragStartPos = initialGluePointCoordinates;
				Point dragEndPos = reconnectDirectly ? snapToConnectionPointCoordinates : new Point(443, 402);
				// Move mouse in position and start a drag action.
				tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseMove, MouseButtonsDg.None, 0, 0, dragStartPos, KeysDg.None));
				tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseDown, MouseButtonsDg.Left, 1, 0, dragStartPos, KeysDg.None));
				// Drag mouse to target location and release mouse button
				foreach (Point mousePos in Interpolate(dragStartPos, dragEndPos, 4))
					tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseMove, MouseButtonsDg.Left, 1, 0, mousePos, KeysDg.None));
				tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseMove, MouseButtonsDg.Left, 1, 0, dragEndPos, KeysDg.None));
				tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseUp, MouseButtonsDg.Left, 1, 0, dragEndPos, KeysDg.None));
				Assert.IsFalse(tool.IsToolActionPending);
				Assert.IsTrue(toolExecuted);
				Point currentGluePointPos = line.GetControlPointPosition(gluePointId);
				if (reconnectDirectly == false) {
					Assert.AreNotEqual(dragEndPos, currentGluePointPos);
					Assert.AreEqual(disconnectLineCoordinates.X, currentGluePointPos.X);
					Assert.AreEqual(disconnectLineCoordinates.Y, currentGluePointPos.Y);

					dragStartPos = currentGluePointPos;
					dragEndPos = snapToConnectionPointCoordinates;
					// Move mouse in position and start a drag action.
					toolExecuted = false;
					Assert.IsFalse(tool.IsToolActionPending);
					tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseMove, MouseButtonsDg.None, 0, 0, dragStartPos, KeysDg.None));
					tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseDown, MouseButtonsDg.Left, 1, 0, dragStartPos, KeysDg.None));
					Assert.IsTrue(tool.IsToolActionPending);
					// Drag mouse to target location and release mouse button
					foreach (Point mousePos in Interpolate(dragStartPos, dragEndPos, 4))
						tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseMove, MouseButtonsDg.Left, 1, 0, mousePos, KeysDg.None));
					tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseMove, MouseButtonsDg.Left, 1, 0, dragEndPos, KeysDg.None));
					tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseUp, MouseButtonsDg.Left, 1, 0, dragEndPos, KeysDg.None));
					Assert.IsFalse(tool.IsToolActionPending);
					Assert.IsTrue(toolExecuted);
					// Update current glue point position
					currentGluePointPos = line.GetControlPointPosition(gluePointId);
				}
				DiagramPresenter.Draw();
				// Validate the line's point-to-shape connection
				Assert.AreNotEqual(dragEndPos, currentGluePointPos);
				Assert.AreEqual(expectedShapeConnectionCoordinates, currentGluePointPos);
				Assert.AreEqual<ControlPointId>(Ellipse.ControlPointIds.TopLeftConnectionPoint, line.IsConnected(ControlPointId.Any, yellowEllipse));

				Assert.IsTrue(Project.History.UndoCommandCount > 0);
				Project.History.Undo(Project.History.UndoCommandCount);
				Assert.IsTrue(Project.History.UndoCommandCount == 0);
			}
			DiagramPresenter.Diagram = null;
		}


		[TestMethod]
		public void TestSelectionToolSelectPlanarShape() {
			// Prepare test
			LoadProjectFile(TestContext.TestName, "Connected Shapes.nspj");
			//
			// Register ToolExecuted event handler
			SelectionTool tool = ToolSetController.DefaultTool as SelectionTool;
			Assert.IsNotNull(tool);
			bool toolExecuted;
			tool.ToolExecuted += (object sender, ToolExecutedEventArgs e) => {
				toolExecuted = true;
			};

			// Add grid aligned shapes (therefore adjust the grid size and the 'Snap to Grid' distance)
			DiagramPresenter.GridSize = 20;
			DiagramPresenter.SnapDistance = 5;
			DiagramPresenter.ActiveTool = tool;

			// In the "Connected Shapes" diagram, this is the position of the blue ellipse in the center
			int baseX = 510;
			int baseY = 440;
			// Get the elllipse shape and check if the shape is at the expected coordinates
			Ellipse ellipse = DiagramPresenter.Diagram.Shapes.FindShape(baseX, baseY, ControlPointCapabilities.Reference, 0, null) as Ellipse;
			Assert.IsNotNull(ellipse);
			Assert.AreEqual(ellipse.X, baseX);
			Assert.AreEqual(ellipse.Y, baseY);

			// Test 1: Select shape with a click
			{
				toolExecuted = false;
				DiagramPresenter.UnselectAll();
				//
				Point mousePos = new Point(baseX, baseY);
				Assert.IsFalse(tool.IsToolActionPending);
				tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseMove, MouseButtonsDg.None, 0, 0, mousePos, KeysDg.None));
				tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseDown, MouseButtonsDg.Left, 1, 0, mousePos, KeysDg.None));
				tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseUp, MouseButtonsDg.Left, 1, 0, mousePos, KeysDg.None));
				Assert.IsFalse(tool.IsToolActionPending);
				Assert.IsTrue(toolExecuted);
				//
				Assert.AreEqual(1, DiagramPresenter.SelectedShapes.Count);
				Assert.AreEqual(ellipse, DiagramPresenter.SelectedShapes.TopMost);
			}

			// Test 2: Select shape with selection frame
			{
				toolExecuted = false;
				DiagramPresenter.UnselectAll();
				//
				Point dragStartPos = new Point(baseX - ellipse.Width, baseY - ellipse.Height);
				Point dragEndPos = new Point(baseX + ellipse.Width, baseY + ellipse.Height);
				Assert.IsFalse(tool.IsToolActionPending);
				tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseMove, MouseButtonsDg.None, 0, 0, dragStartPos, KeysDg.None));
				tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseDown, MouseButtonsDg.Left, 1, 0, dragStartPos, KeysDg.None));
				Assert.IsTrue(tool.IsToolActionPending);
				// Drag selection frame
				tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseMove, MouseButtonsDg.Left, 0, 0, dragStartPos, KeysDg.None));
				foreach (Point mousePos in Interpolate(dragStartPos, dragEndPos, 10))
					tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseMove, MouseButtonsDg.Left, 0, 0, mousePos, KeysDg.None));
				tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseMove, MouseButtonsDg.Left, 0, 0, dragEndPos, KeysDg.None));
				Assert.IsTrue(tool.IsToolActionPending);
				//
				tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseUp, MouseButtonsDg.Left, 1, 0, dragEndPos, KeysDg.None));
				Assert.IsFalse(tool.IsToolActionPending);
				Assert.IsTrue(toolExecuted);
				//
				Assert.AreEqual(1, DiagramPresenter.SelectedShapes.Count);
				Assert.AreEqual(ellipse, DiagramPresenter.SelectedShapes.TopMost);
			}
			DiagramPresenter.Diagram = null;
		}


		[TestMethod]
		public void TestSelectionToolSelectLinearShape() {
			// Prepare test
			LoadProjectFile(TestContext.TestName, "Connected Shapes.nspj");
			//
			// Register ToolExecuted event handler
			SelectionTool tool = ToolSetController.DefaultTool as SelectionTool;
			Assert.IsNotNull(tool);
			bool toolExecuted;
			tool.ToolExecuted += (object sender, ToolExecutedEventArgs e) => {
				toolExecuted = true;
			};

			// Add grid aligned shapes (therefore adjust the grid size and the 'Snap to Grid' distance)
			DiagramPresenter.GridSize = 20;
			DiagramPresenter.SnapDistance = 5;
			DiagramPresenter.ActiveTool = tool;

			// In the "Connected Shapes" diagram, this is the position of the blue ellipse in the center
			int baseX = 470;
			int baseY = 380;
			// Get the elllipse shape and check if the shape is at the expected coordinates
			Polyline line = DiagramPresenter.Diagram.Shapes.FindShape(baseX, baseY, ControlPointCapabilities.Reference, 0, null) as Polyline;
			Assert.IsNotNull(line);
			Assert.IsTrue(line.ContainsPoint(baseX, baseY));

			// Test 1: Select shape with a click
			{
				toolExecuted = false;
				DiagramPresenter.UnselectAll();
				//
				Point mousePos = new Point(baseX, baseY);
				Assert.IsFalse(tool.IsToolActionPending);
				tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseMove, MouseButtonsDg.None, 0, 0, mousePos, KeysDg.None));
				tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseDown, MouseButtonsDg.Left, 1, 0, mousePos, KeysDg.None));
				tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseUp, MouseButtonsDg.Left, 1, 0, mousePos, KeysDg.None));
				Assert.IsFalse(tool.IsToolActionPending);
				Assert.IsTrue(toolExecuted);
				//
				Assert.AreEqual(1, DiagramPresenter.SelectedShapes.Count);
				Assert.AreEqual(line, DiagramPresenter.SelectedShapes.TopMost);
			}
			{
				// Test 2: Select shape with selection frame
				toolExecuted = false;
				DiagramPresenter.UnselectAll();
				//
				Point dragStartPos = new Point(390, 320);
				Point dragEndPos = new Point(540, 450);
				Assert.IsFalse(tool.IsToolActionPending);
				tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseMove, MouseButtonsDg.None, 0, 0, dragStartPos, KeysDg.None));
				tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseDown, MouseButtonsDg.Left, 1, 0, dragStartPos, KeysDg.None));
				Assert.IsTrue(tool.IsToolActionPending);
				// Drag selection frame
				tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseMove, MouseButtonsDg.Left, 0, 0, dragStartPos, KeysDg.None));
				foreach (Point mousePos in Interpolate(dragStartPos, dragEndPos, 10))
					tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseMove, MouseButtonsDg.Left, 0, 0, mousePos, KeysDg.None));
				tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseMove, MouseButtonsDg.Left, 0, 0, dragEndPos, KeysDg.None));
				Assert.IsTrue(tool.IsToolActionPending);
				//
				tool.ProcessMouseEvent(DiagramPresenter, new MouseEventArgsDg(MouseEventType.MouseUp, MouseButtonsDg.Left, 1, 0, dragEndPos, KeysDg.None));
				Assert.IsFalse(tool.IsToolActionPending);
				Assert.IsTrue(toolExecuted);
				//
				Assert.AreEqual(1, DiagramPresenter.SelectedShapes.Count);
				Assert.AreEqual(line, DiagramPresenter.SelectedShapes.TopMost);
			}
			DiagramPresenter.Diagram = null;
		}

		#endregion


		#region [Private] Properties

		private Project Project { get; set; }
		private DiagramPresenterMock DiagramPresenter { get; set; }

		private ToolSetController ToolSetController { get; set; }

		#endregion


		#region [Private] Methods

		private void LoadProjectFile(string testName, string projectFileName) {
			// Create NShape components and load XML repository
			string projectsDirectory = DiagramHelper.GetProjectFilesDirectory();
			Project = DiagramHelper.CreateProject(Path.Combine(projectsDirectory, projectFileName));
			DiagramSetController diagramSetController = new DiagramSetController(Project);
			ToolSetController = new ToolSetController() {
				DiagramSetController = diagramSetController
			};
			DiagramPresenter = new DiagramPresenterMock(testName) {
				DiagramSetController = diagramSetController,
				Project = Project,
			};
			Project.Open();
			DiagramPresenter.Diagram = Project.Repository.GetDiagrams().First();
		}


		private static IEnumerable<Point> Interpolate(Point from, Point to, uint steps) {
			if (steps == 0) throw new ArgumentOutOfRangeException(nameof(steps));

			float deltaX = (to.X - from.X) / (float)steps;
			float deltaY = (to.Y - from.Y) / (float)steps;
			for (int i = 0; i < steps; ++i)
				yield return new Point(
					from.X + (int)Math.Round(i * deltaX),
					from.Y + (int)Math.Round(i * deltaY)
				);
		}


		#endregion

	}

}
