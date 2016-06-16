using System;
using System.Collections.Generic;
using System.Reflection;
using Dataweb.NShape;
using Dataweb.NShape.Advanced;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dataweb.NShape.FlowChartShapes;
using Dataweb.NShape.ElectricalShapes;
using Dataweb.NShape.GeneralShapes;
using Dataweb.NShape.SoftwareArchitectureShapes;


namespace NShapeTest {

	[TestClass]
	public class ShapeClassesTest {

		[TestMethod]
		public void ControlPointConstantsTest() {
			// -- Create a project --
			Project project = new Project();
			project.AutoLoadLibraries = true;
			project.Name = "ControlPointConstantsTest";
			project.Create();

			// Add Libraries:
			//
			// GeneralShapes
			project.AddLibrary(typeof(Dataweb.NShape.GeneralShapes.Circle).Assembly, true);
			// ElectricalShapes
			project.AddLibrary(typeof(Dataweb.NShape.ElectricalShapes.AutoDisconnectorSymbol).Assembly, true);
			// FlowChartShapes
			project.AddLibrary(typeof(Dataweb.NShape.FlowChartShapes.ProcessSymbol).Assembly, true);
			// SoftwareArchitectureShapes
			project.AddLibrary(typeof(Dataweb.NShape.SoftwareArchitectureShapes.CloudSymbol).Assembly, true);

			foreach (ShapeType shapeType in project.ShapeTypes) {
				// use var here to obtain the concrete shape type
				using (Shape shape = shapeType.CreateInstance()) {
					// Check the number of constants and their values
					//ControlPointsBaseTest(shape);

					// Get all control point id's defined by constants of the specific shape type
					// and check whether the point has the expected capabilities
					ControlPointsByTypeTest(shape);
				}
			}

			project.Close();
		}


		#region [Private] General Test Methods

		private void ControlPointsBaseTest(Shape shape) {
			List<ControlPointId> pointsFromShape = new List<ControlPointId>(shape.GetControlPointIds(ControlPointCapabilities.All));

			// Check whether control point constants match the control poins returned by the shape instance
			foreach (int ptConst in GetControlPointsFromType(shape.GetType())) {
				ControlPointId id = ptConst;
				bool removed = pointsFromShape.Remove(id);
				Assert.IsTrue(removed, "ControlPoint {0} retrieved from type constant was not returned by !", id, shape.Type.FullName);
			}
			//Assert.AreEqual<int>(pointsFromShape.Count, 0,
			//    "Not all control points returned by {0}.GetControlPointIds are defined as constants. {1} constants missing.",
			//    shape.Type.FullName, pointsFromShape.Count);

			if (shape is ShapeBase) {
				// Check whether the number of control point constants match the ControlPointCount property
				Assert.IsTrue(pointsFromShape.Count >= ((ShapeBase)shape).ControlPointCount,
					"{0}.ControlPointCount does not match the number of control points returned by {0}.GetControlPoints()!",
					shape.Type.FullName);
			}
		}


		private void ControlPointsByTypeTest(Shape shape) {
			List<ControlPointId> pointsFromShape = new List<ControlPointId>(shape.GetControlPointIds(ControlPointCapabilities.All));
			pointsFromShape.Add(ControlPointId.Reference);
			if (shape is ILinearShape) {
				pointsFromShape.Add(ControlPointId.FirstVertex);
				pointsFromShape.Add(ControlPointId.LastVertex);
			}

			if (shape == null)
				Assert.IsTrue(false);

			// ElectricalShapes
			else if (shape is BusBarSymbol)
				TestControlPointCapabilities((BusBarSymbol)shape, pointsFromShape);
			else if (shape is DisconnectorSymbol)
				TestControlPointCapabilities((DisconnectorSymbol)shape, pointsFromShape);
			else if (shape is AutoDisconnectorSymbol)
				TestControlPointCapabilities((AutoDisconnectorSymbol)shape, pointsFromShape);
			else if (shape is AutoSwitchSymbol)
				TestControlPointCapabilities((AutoSwitchSymbol)shape, pointsFromShape);
			else if (shape is SwitchSymbol)
				TestControlPointCapabilities((SwitchSymbol)shape, pointsFromShape);
			else if (shape is TransformerSymbol)
				TestControlPointCapabilities((TransformerSymbol)shape, pointsFromShape);
			else if (shape is EarthSymbol)
				TestControlPointCapabilities((EarthSymbol)shape, pointsFromShape);
			else if (shape is FeederSymbol)
				TestControlPointCapabilities((FeederSymbol)shape, pointsFromShape);
			else if (shape is RectifierSymbol)
				TestControlPointCapabilities((RectifierSymbol)shape, pointsFromShape);
			else if (shape is DisconnectingPoint)
				TestControlPointCapabilities((DisconnectingPoint)shape, pointsFromShape);

			// FlowChartShapes
			else if (shape is TerminatorSymbol)
				TestControlPointCapabilities((TerminatorSymbol)shape, pointsFromShape);
			else if (shape is ProcessSymbol)
				TestControlPointCapabilities((ProcessSymbol)shape, pointsFromShape);
			else if (shape is PredefinedProcessSymbol)
				TestControlPointCapabilities((PredefinedProcessSymbol)shape, pointsFromShape);
			else if (shape is InputOutputSymbol)
				TestControlPointCapabilities((InputOutputSymbol)shape, pointsFromShape);
			else if (shape is Dataweb.NShape.FlowChartShapes.DocumentSymbol)
				TestControlPointCapabilities((Dataweb.NShape.FlowChartShapes.DocumentSymbol)shape, pointsFromShape);
			else if (shape is OffpageConnectorSymbol)
				TestControlPointCapabilities((OffpageConnectorSymbol)shape, pointsFromShape);
			else if (shape is OnlineStorageSymbol)
				TestControlPointCapabilities((OnlineStorageSymbol)shape, pointsFromShape);
			else if (shape is DrumStorageSymbol)
				TestControlPointCapabilities((DrumStorageSymbol)shape, pointsFromShape);
			else if (shape is DiskStorageSymbol)
				TestControlPointCapabilities((DiskStorageSymbol)shape, pointsFromShape);
			else if (shape is PreparationSymbol)
				TestControlPointCapabilities((PreparationSymbol)shape, pointsFromShape);
			else if (shape is ManualInputSymbol)
				TestControlPointCapabilities((ManualInputSymbol)shape, pointsFromShape);
			else if (shape is CoreSymbol)
				TestControlPointCapabilities((CoreSymbol)shape, pointsFromShape);
			else if (shape is DisplaySymbol)
				TestControlPointCapabilities((DisplaySymbol)shape, pointsFromShape);
			else if (shape is TapeSymbol)
				TestControlPointCapabilities((TapeSymbol)shape, pointsFromShape);
			else if (shape is ManualOperationSymbol)
				TestControlPointCapabilities((ManualOperationSymbol)shape, pointsFromShape);
			else if (shape is CollateSymbol)
				TestControlPointCapabilities((CollateSymbol)shape, pointsFromShape);
			else if (shape is CardSymbol)
				TestControlPointCapabilities((CardSymbol)shape, pointsFromShape);
			else if (shape is ConnectorSymbol)
				TestControlPointCapabilities((ConnectorSymbol)shape, pointsFromShape);
			else if (shape is TapeStorageSymbol)
				TestControlPointCapabilities((TapeStorageSymbol)shape, pointsFromShape);
			else if (shape is DecisionSymbol)
				TestControlPointCapabilities((DecisionSymbol)shape, pointsFromShape);
			else if (shape is SortSymbol)
				TestControlPointCapabilities((SortSymbol)shape, pointsFromShape);
			else if (shape is CommLinkSymbol)
				TestControlPointCapabilities((CommLinkSymbol)shape, pointsFromShape);
			else if (shape is ExtractSymbol)
				TestControlPointCapabilities((ExtractSymbol)shape, pointsFromShape);
			else if (shape is MergeSymbol)
				TestControlPointCapabilities((MergeSymbol)shape, pointsFromShape);
			else if (shape is OfflineStorageSymbol)
				TestControlPointCapabilities((OfflineStorageSymbol)shape, pointsFromShape);

			// GeneralShapes
			else if (shape is Polyline)
				TestControlPointCapabilities((Polyline)shape, pointsFromShape);
			else if (shape is RectangularLine)
				TestControlPointCapabilities((RectangularLine)shape, pointsFromShape);
			else if (shape is CircularArc)
				TestControlPointCapabilities((CircularArc)shape, pointsFromShape);
			else if (shape is Text)
				TestControlPointCapabilities((Text)shape, pointsFromShape);
			else if (shape is Label)
				TestControlPointCapabilities((Label)shape, pointsFromShape);
			else if (shape is RegularPolygone)
				TestControlPointCapabilities((RegularPolygone)shape, pointsFromShape);
			else if (shape is FreeTriangle)
				TestControlPointCapabilities((FreeTriangle)shape, pointsFromShape);
			else if (shape is IsoscelesTriangle)
				TestControlPointCapabilities((IsoscelesTriangle)shape, pointsFromShape);
			else if (shape is Circle)
				TestControlPointCapabilities((Circle)shape, pointsFromShape);
			else if (shape is Ellipse)
				TestControlPointCapabilities((Ellipse)shape, pointsFromShape);
			else if (shape is Square)
				TestControlPointCapabilities((Square)shape, pointsFromShape);
			else if (shape is Box)
				TestControlPointCapabilities((Box)shape, pointsFromShape);
			else if (shape is RoundedBox)
				TestControlPointCapabilities((RoundedBox)shape, pointsFromShape);
			else if (shape is Diamond)
				TestControlPointCapabilities((Diamond)shape, pointsFromShape);
			else if (shape is ThickArrow)
				TestControlPointCapabilities((ThickArrow)shape, pointsFromShape);
			else if (shape is Picture)
				TestControlPointCapabilities((Picture)shape, pointsFromShape);

			// SoftwareArchitectureShapes
			else if (shape is DataFlowArrow)
				TestControlPointCapabilities((DataFlowArrow)shape, pointsFromShape);
			else if (shape is DependencyArrow)
				TestControlPointCapabilities((DependencyArrow)shape, pointsFromShape);
			else if (shape is DatabaseSymbol)
				TestControlPointCapabilities((DatabaseSymbol)shape, pointsFromShape);
			else if (shape is EntitySymbol)
				TestControlPointCapabilities((EntitySymbol)shape, pointsFromShape);
			else if (shape is AnnotationSymbol)
				TestControlPointCapabilities((AnnotationSymbol)shape, pointsFromShape);
			else if (shape is CloudSymbol)
				TestControlPointCapabilities((CloudSymbol)shape, pointsFromShape);
			else if (shape is ClassSymbol)
				TestControlPointCapabilities((ClassSymbol)shape, pointsFromShape);
			else if (shape is ComponentSymbol)
				TestControlPointCapabilities((ComponentSymbol)shape, pointsFromShape);
			else if (shape is Dataweb.NShape.SoftwareArchitectureShapes.DocumentSymbol)
				TestControlPointCapabilities((Dataweb.NShape.SoftwareArchitectureShapes.DocumentSymbol)shape, pointsFromShape);
			else if (shape is InterfaceSymbol)
				TestControlPointCapabilities((InterfaceSymbol)shape, pointsFromShape);
			else if (shape is InterfaceUsageSymbol)
				TestControlPointCapabilities((InterfaceUsageSymbol)shape, pointsFromShape);
			else if (shape is VectorImage)
				TestControlPointCapabilities((VectorImage)shape, pointsFromShape);
		}

		#endregion


		#region [Private] Shape Class Specific Test Methods - ElectricalShapes

		private void TestControlPointCapabilities(BusBarSymbol shape, IEnumerable<ControlPointId> pointsFromShape) {
			// Nothing to do here
		}


		private void TestControlPointCapabilities(DisconnectorSymbol shape, IEnumerable<ControlPointId> pointsFromShape) {
			// Check whether all points have the expected capabilities
			foreach (ControlPointId id in pointsFromShape) {
				switch (id) {
					case DisconnectorSymbol.ControlPointIds.TopCenterControlPoint:
					case DisconnectorSymbol.ControlPointIds.MiddleLeftControlPoint:
					case DisconnectorSymbol.ControlPointIds.MiddleRightControlPoint:
					case DisconnectorSymbol.ControlPointIds.BottomCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case DisconnectorSymbol.ControlPointIds.TopLeftControlPoint:
					case DisconnectorSymbol.ControlPointIds.TopRightControlPoint:
					case DisconnectorSymbol.ControlPointIds.BottomLeftControlPoint:
					case DisconnectorSymbol.ControlPointIds.BottomRightControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case DisconnectorSymbol.ControlPointIds.MiddleCenterControlPoint:
					case ControlPointId.Reference:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						break;
					default:
						Assert.Fail("Unexpected control point id in class '{0}': {1}", shape.Type.FullName, id);
						break;
				}
			}
		}


		private void TestControlPointCapabilities(AutoDisconnectorSymbol shape, IEnumerable<ControlPointId> pointsFromShape) {
			// Check whether all points have the expected capabilities
			foreach (ControlPointId id in pointsFromShape) {
				switch (id) {
					case AutoDisconnectorSymbol.ControlPointIds.TopCenterControlPoint:
					case AutoDisconnectorSymbol.ControlPointIds.MiddleLeftControlPoint:
					case AutoDisconnectorSymbol.ControlPointIds.MiddleRightControlPoint:
					case AutoDisconnectorSymbol.ControlPointIds.BottomCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case AutoDisconnectorSymbol.ControlPointIds.TopLeftControlPoint:
					case AutoDisconnectorSymbol.ControlPointIds.TopRightControlPoint:
					case AutoDisconnectorSymbol.ControlPointIds.BottomLeftControlPoint:
					case AutoDisconnectorSymbol.ControlPointIds.BottomRightControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case AutoDisconnectorSymbol.ControlPointIds.MiddleCenterControlPoint:
					case ControlPointId.Reference:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						break;
					default:
						Assert.Fail("Unexpected control point id in class '{0}': {1}", shape.Type.FullName, id);
						break;
				}
			}
		}


		private void TestControlPointCapabilities(AutoSwitchSymbol shape, IEnumerable<ControlPointId> pointsFromShape) {
			// Check whether all points have the expected capabilities
			foreach (ControlPointId id in pointsFromShape) {
				switch (id) {
					case AutoSwitchSymbol.ControlPointIds.TopLeftControlPoint:
					case AutoSwitchSymbol.ControlPointIds.TopCenterControlPoint:
					case AutoSwitchSymbol.ControlPointIds.TopRightControlPoint:
					case AutoSwitchSymbol.ControlPointIds.MiddleLeftControlPoint:
					case AutoSwitchSymbol.ControlPointIds.MiddleRightControlPoint:
					case AutoSwitchSymbol.ControlPointIds.BottomLeftControlPoint:
					case AutoSwitchSymbol.ControlPointIds.BottomCenterControlPoint:
					case AutoSwitchSymbol.ControlPointIds.BottomRightControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case AutoSwitchSymbol.ControlPointIds.MiddleCenterControlPoint:
					case ControlPointId.Reference:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						break;
					default:
						Assert.Fail("Unexpected control point id in class '{0}': {1}", shape.Type.FullName, id);
						break;
				}
			}
		}


		private void TestControlPointCapabilities(SwitchSymbol shape, IEnumerable<ControlPointId> pointsFromShape) {
			// Check whether all points have the expected capabilities
			foreach (ControlPointId id in pointsFromShape) {
				switch (id) {
					case SwitchSymbol.ControlPointIds.TopLeftControlPoint:
					case SwitchSymbol.ControlPointIds.TopCenterControlPoint:
					case SwitchSymbol.ControlPointIds.TopRightControlPoint:
					case SwitchSymbol.ControlPointIds.MiddleLeftControlPoint:
					case SwitchSymbol.ControlPointIds.MiddleRightControlPoint:
					case SwitchSymbol.ControlPointIds.BottomLeftControlPoint:
					case SwitchSymbol.ControlPointIds.BottomCenterControlPoint:
					case SwitchSymbol.ControlPointIds.BottomRightControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case SwitchSymbol.ControlPointIds.MiddleCenterControlPoint:
					case ControlPointId.Reference:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						break;
					default:
						Assert.Fail("Unexpected control point id in class '{0}': {1}", shape.Type.FullName, id);
						break;
				}
			}
		}


		private void TestControlPointCapabilities(TransformerSymbol shape, IEnumerable<ControlPointId> pointsFromShape) {
			// Check whether all points have the expected capabilities
			foreach (ControlPointId id in pointsFromShape) {
				switch (id) {
					case TransformerSymbol.ControlPointIds.TopCenterControlPoint:
					case TransformerSymbol.ControlPointIds.BottomCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case TransformerSymbol.ControlPointIds.TopLeftControlPoint:
					case TransformerSymbol.ControlPointIds.TopRightControlPoint:
					case TransformerSymbol.ControlPointIds.MiddleLeftControlPoint:
					case TransformerSymbol.ControlPointIds.MiddleRightControlPoint:
					case TransformerSymbol.ControlPointIds.BottomLeftControlPoint:
					case TransformerSymbol.ControlPointIds.BottomRightControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case TransformerSymbol.ControlPointIds.MiddleCenterControlPoint:
					case ControlPointId.Reference:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						break;
					default:
						Assert.Fail("Unexpected control point id in class '{0}': {1}", shape.Type.FullName, id);
						break;
				}
			}
		}


		private void TestControlPointCapabilities(EarthSymbol shape, IEnumerable<ControlPointId> pointsFromShape) {
			// Check whether all points have the expected capabilities
			foreach (ControlPointId id in pointsFromShape) {
				switch (id) {
					case EarthSymbol.ControlPointIds.TopCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case EarthSymbol.ControlPointIds.TopLeftControlPoint:
					case EarthSymbol.ControlPointIds.TopRightControlPoint:
					case EarthSymbol.ControlPointIds.MiddleLeftControlPoint:
					case EarthSymbol.ControlPointIds.MiddleRightControlPoint:
					case EarthSymbol.ControlPointIds.BottomLeftControlPoint:
					case EarthSymbol.ControlPointIds.BottomCenterControlPoint:
					case EarthSymbol.ControlPointIds.BottomRightControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case EarthSymbol.ControlPointIds.MiddleCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						break;
					case ControlPointId.Reference:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						break;
					default:
						Assert.Fail("Unexpected control point id in class '{0}': {1}", shape.Type.FullName, id);
						break;
				}
			}
		}


		private void TestControlPointCapabilities(FeederSymbol shape, IEnumerable<ControlPointId> pointsFromShape) {
			// Check whether all points have the expected capabilities
			foreach (ControlPointId id in pointsFromShape) {
				switch (id) {
					case FeederSymbol.ControlPointIds.TopCenterControlPoint:
					case FeederSymbol.ControlPointIds.BottomLeftControlPoint:
					case FeederSymbol.ControlPointIds.BottomCenterControlPoint:
					case FeederSymbol.ControlPointIds.BottomRightControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case FeederSymbol.ControlPointIds.BalancePointControlPoint:
					case ControlPointId.Reference:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						break;
					default:
						Assert.Fail("Unexpected control point id in class '{0}': {1}", shape.Type.FullName, id);
						break;
				}
			}
		}


		private void TestControlPointCapabilities(RectifierSymbol shape, IEnumerable<ControlPointId> pointsFromShape) {
			// Check whether all points have the expected capabilities
			foreach (ControlPointId id in pointsFromShape) {
				switch (id) {
					case RectifierSymbol.ControlPointIds.TopCenterControlPoint:
					case RectifierSymbol.ControlPointIds.BottomLeftControlPoint:
					case RectifierSymbol.ControlPointIds.BottomCenterControlPoint:
					case RectifierSymbol.ControlPointIds.BottomRightControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case RectifierSymbol.ControlPointIds.BalancePointControlPoint:
					case ControlPointId.Reference:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						break;
					default:
						Assert.Fail("Unexpected control point id in class '{0}': {1}", shape.Type.FullName, id);
						break;
				}
			}
		}


		private void TestControlPointCapabilities(DisconnectingPoint shape, IEnumerable<ControlPointId> pointsFromShape) {
			// Check whether all points have the expected capabilities
			foreach (ControlPointId id in pointsFromShape) {
				switch (id) {
					case DisconnectingPoint.ControlPointIds.MiddleLeftControlPoint:
					case DisconnectingPoint.ControlPointIds.MiddleRightControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case DisconnectingPoint.ControlPointIds.TopLeftControlPoint:
					case DisconnectingPoint.ControlPointIds.TopCenterControlPoint:
					case DisconnectingPoint.ControlPointIds.TopRightControlPoint:
					case DisconnectingPoint.ControlPointIds.BottomLeftControlPoint:
					case DisconnectingPoint.ControlPointIds.BottomCenterControlPoint:
					case DisconnectingPoint.ControlPointIds.BottomRightControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case DisconnectingPoint.ControlPointIds.MiddleCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						break;
					case ControlPointId.Reference:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						break;
					default:
						Assert.Fail("Unexpected control point id in class '{0}': {1}", shape.Type.FullName, id);
						break;
				}
			}
		}

		#endregion


		#region [Private] Shape Class Specific Test Methods - FlowChartShapes

		private void TestControlPointCapabilities(TerminatorSymbol shape, IEnumerable<ControlPointId> controlPointsFromShape) {
			// Check whether all points have the expected capabilities
			foreach (ControlPointId id in controlPointsFromShape) {
				switch (id) {
					case TerminatorSymbol.ControlPointIds.TopLeftControlPoint:
					case TerminatorSymbol.ControlPointIds.TopRightControlPoint:
					case TerminatorSymbol.ControlPointIds.BottomLeftControlPoint:
					case TerminatorSymbol.ControlPointIds.BottomRightControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case TerminatorSymbol.ControlPointIds.TopCenterControlPoint:
					case TerminatorSymbol.ControlPointIds.MiddleLeftControlPoint:
					case TerminatorSymbol.ControlPointIds.MiddleRightControlPoint:
					case TerminatorSymbol.ControlPointIds.BottomCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case TerminatorSymbol.ControlPointIds.MiddleCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						break;
					case ControlPointId.Reference:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						break;
					default:
						Assert.Fail("Unexpected control point id in class '{0}': {1}", shape.Type.FullName, id);
						break;
				}
			}
		}


		private void TestControlPointCapabilities(ProcessSymbol shape, IEnumerable<ControlPointId> controlPointsFromShape) {
			// Check whether all points have the expected capabilities
			foreach (ControlPointId id in controlPointsFromShape) {
				switch (id) {
					case ProcessSymbol.ControlPointIds.TopLeftControlPoint:
					case ProcessSymbol.ControlPointIds.TopRightControlPoint:
					case ProcessSymbol.ControlPointIds.BottomLeftControlPoint:
					case ProcessSymbol.ControlPointIds.BottomRightControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case ProcessSymbol.ControlPointIds.TopCenterControlPoint:
					case ProcessSymbol.ControlPointIds.MiddleLeftControlPoint:
					case ProcessSymbol.ControlPointIds.MiddleRightControlPoint:
					case ProcessSymbol.ControlPointIds.BottomCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case ProcessSymbol.ControlPointIds.TopLeftConnectionPoint:
					case ProcessSymbol.ControlPointIds.TopRightConnectionPoint:
					case ProcessSymbol.ControlPointIds.BottomLeftConnectionPoint:
					case ProcessSymbol.ControlPointIds.BottomRightConnectionPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						break;
					case ProcessSymbol.ControlPointIds.MiddleCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						break;
					case ControlPointId.Reference:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						break;
					default:
						Assert.Fail("Unexpected control point id in class '{0}': {1}", shape.Type.FullName, id);
						break;
				}
			}
		}


		private void TestControlPointCapabilities(PredefinedProcessSymbol shape, IEnumerable<ControlPointId> controlPointsFromShape) {
			// Check whether all points have the expected capabilities
			foreach (ControlPointId id in controlPointsFromShape) {
				switch (id) {
					case PredefinedProcessSymbol.ControlPointIds.TopLeftControlPoint:
					case PredefinedProcessSymbol.ControlPointIds.TopRightControlPoint:
					case PredefinedProcessSymbol.ControlPointIds.BottomLeftControlPoint:
					case PredefinedProcessSymbol.ControlPointIds.BottomRightControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case PredefinedProcessSymbol.ControlPointIds.TopCenterControlPoint:
					case PredefinedProcessSymbol.ControlPointIds.MiddleLeftControlPoint:
					case PredefinedProcessSymbol.ControlPointIds.MiddleRightControlPoint:
					case PredefinedProcessSymbol.ControlPointIds.BottomCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case PredefinedProcessSymbol.ControlPointIds.MiddleCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						break;
					case ControlPointId.Reference:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						break;
					default:
						Assert.Fail("Unexpected control point id in class '{0}': {1}", shape.Type.FullName, id);
						break;
				}
			}
		}


		private void TestControlPointCapabilities(InputOutputSymbol shape, IEnumerable<ControlPointId> controlPointsFromShape) {
			// Check whether all points have the expected capabilities
			foreach (ControlPointId id in controlPointsFromShape) {
				switch (id) {
					case InputOutputSymbol.ControlPointIds.TopLeftControlPoint:
					case InputOutputSymbol.ControlPointIds.TopRightControlPoint:
					case InputOutputSymbol.ControlPointIds.BottomLeftControlPoint:
					case InputOutputSymbol.ControlPointIds.BottomRightControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case InputOutputSymbol.ControlPointIds.TopCenterControlPoint:
					case InputOutputSymbol.ControlPointIds.MiddleLeftControlPoint:
					case InputOutputSymbol.ControlPointIds.MiddleRightControlPoint:
					case InputOutputSymbol.ControlPointIds.BottomCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case InputOutputSymbol.ControlPointIds.MiddleCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						break;
					case ControlPointId.Reference:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						break;
					default:
						Assert.Fail("Unexpected control point id in class '{0}': {1}", shape.Type.FullName, id);
						break;
				}
			}
		}


		private void TestControlPointCapabilities(Dataweb.NShape.FlowChartShapes.DocumentSymbol shape, IEnumerable<ControlPointId> controlPointsFromShape) {
			// Check whether all points have the expected capabilities
			foreach (ControlPointId id in controlPointsFromShape) {
				switch (id) {
					case Dataweb.NShape.FlowChartShapes.DocumentSymbol.ControlPointIds.TopLeftControlPoint:
					case Dataweb.NShape.FlowChartShapes.DocumentSymbol.ControlPointIds.TopRightControlPoint:
					case Dataweb.NShape.FlowChartShapes.DocumentSymbol.ControlPointIds.BottomLeftControlPoint:
					case Dataweb.NShape.FlowChartShapes.DocumentSymbol.ControlPointIds.BottomRightControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case Dataweb.NShape.FlowChartShapes.DocumentSymbol.ControlPointIds.TopCenterControlPoint:
					case Dataweb.NShape.FlowChartShapes.DocumentSymbol.ControlPointIds.MiddleLeftControlPoint:
					case Dataweb.NShape.FlowChartShapes.DocumentSymbol.ControlPointIds.MiddleRightControlPoint:
					case Dataweb.NShape.FlowChartShapes.DocumentSymbol.ControlPointIds.BottomCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case Dataweb.NShape.FlowChartShapes.DocumentSymbol.ControlPointIds.MiddleCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						break;
					case ControlPointId.Reference:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						break;
					default:
						Assert.Fail("Unexpected control point id in class '{0}': {1}", shape.Type.FullName, id);
						break;
				}
			}
		}


		private void TestControlPointCapabilities(OffpageConnectorSymbol shape, IEnumerable<ControlPointId> controlPointsFromShape) {
			// Check whether all points have the expected capabilities
			foreach (ControlPointId id in controlPointsFromShape) {
				switch (id) {
					case OffpageConnectorSymbol.ControlPointIds.TopLeftControlPoint:
					//case OffpageConnectorSymbol.ControlPointIds.TopRightControlPoint:
					case OffpageConnectorSymbol.ControlPointIds.BottomLeftControlPoint:
						//case OffpageConnectorSymbol.ControlPointIds.BottomRightControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case OffpageConnectorSymbol.ControlPointIds.TopCenterControlPoint:
					case OffpageConnectorSymbol.ControlPointIds.MiddleLeftControlPoint:
					case OffpageConnectorSymbol.ControlPointIds.MiddleRightControlPoint:
					case OffpageConnectorSymbol.ControlPointIds.BottomCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case OffpageConnectorSymbol.ControlPointIds.MiddleCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						break;
					case ControlPointId.Reference:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						break;
					default:
						Assert.Fail("Unexpected control point id in class '{0}': {1}", shape.Type.FullName, id);
						break;
				}
			}
		}


		private void TestControlPointCapabilities(OnlineStorageSymbol shape, IEnumerable<ControlPointId> controlPointsFromShape) {
			// Check whether all points have the expected capabilities
			foreach (ControlPointId id in controlPointsFromShape) {
				switch (id) {
					case OnlineStorageSymbol.ControlPointIds.TopLeftControlPoint:
					case OnlineStorageSymbol.ControlPointIds.TopRightControlPoint:
					case OnlineStorageSymbol.ControlPointIds.BottomLeftControlPoint:
					case OnlineStorageSymbol.ControlPointIds.BottomRightControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case OnlineStorageSymbol.ControlPointIds.TopCenterControlPoint:
					case OnlineStorageSymbol.ControlPointIds.MiddleLeftControlPoint:
					case OnlineStorageSymbol.ControlPointIds.MiddleRightControlPoint:
					case OnlineStorageSymbol.ControlPointIds.BottomCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case OnlineStorageSymbol.ControlPointIds.MiddleCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						break;
					case ControlPointId.Reference:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						break;
					default:
						Assert.Fail("Unexpected control point id in class '{0}': {1}", shape.Type.FullName, id);
						break;
				}
			}
		}


		private void TestControlPointCapabilities(DrumStorageSymbol shape, IEnumerable<ControlPointId> controlPointsFromShape) {
			// Check whether all points have the expected capabilities
			foreach (ControlPointId id in controlPointsFromShape) {
				switch (id) {
					case DrumStorageSymbol.ControlPointIds.TopLeftControlPoint:
					case DrumStorageSymbol.ControlPointIds.TopRightControlPoint:
					case DrumStorageSymbol.ControlPointIds.BottomLeftControlPoint:
					case DrumStorageSymbol.ControlPointIds.BottomRightControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case DrumStorageSymbol.ControlPointIds.TopCenterControlPoint:
					case DrumStorageSymbol.ControlPointIds.MiddleLeftControlPoint:
					case DrumStorageSymbol.ControlPointIds.MiddleRightControlPoint:
					case DrumStorageSymbol.ControlPointIds.BottomCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case DrumStorageSymbol.ControlPointIds.MiddleCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						break;
					case ControlPointId.Reference:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						break;
					default:
						Assert.Fail("Unexpected control point id in class '{0}': {1}", shape.Type.FullName, id);
						break;
				}
			}
		}


		private void TestControlPointCapabilities(DiskStorageSymbol shape, IEnumerable<ControlPointId> controlPointsFromShape) {
			// Check whether all points have the expected capabilities
			foreach (ControlPointId id in controlPointsFromShape) {
				switch (id) {
					case DiskStorageSymbol.ControlPointIds.TopLeftControlPoint:
					case DiskStorageSymbol.ControlPointIds.TopRightControlPoint:
					case DiskStorageSymbol.ControlPointIds.BottomLeftControlPoint:
					case DiskStorageSymbol.ControlPointIds.BottomRightControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case DiskStorageSymbol.ControlPointIds.TopCenterControlPoint:
					case DiskStorageSymbol.ControlPointIds.MiddleLeftControlPoint:
					case DiskStorageSymbol.ControlPointIds.MiddleRightControlPoint:
					case DiskStorageSymbol.ControlPointIds.BottomCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case DiskStorageSymbol.ControlPointIds.MiddleCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						break;
					case ControlPointId.Reference:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						break;
					default:
						Assert.Fail("Unexpected control point id in class '{0}': {1}", shape.Type.FullName, id);
						break;
				}
			}
		}


		private void TestControlPointCapabilities(PreparationSymbol shape, IEnumerable<ControlPointId> controlPointsFromShape) {
			// Check whether all points have the expected capabilities
			foreach (ControlPointId id in controlPointsFromShape) {
				switch (id) {
					case PreparationSymbol.ControlPointIds.TopLeftControlPoint:
					case PreparationSymbol.ControlPointIds.TopRightControlPoint:
					case PreparationSymbol.ControlPointIds.BottomLeftControlPoint:
					case PreparationSymbol.ControlPointIds.BottomRightControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case PreparationSymbol.ControlPointIds.TopCenterControlPoint:
					case PreparationSymbol.ControlPointIds.MiddleLeftControlPoint:
					case PreparationSymbol.ControlPointIds.MiddleRightControlPoint:
					case PreparationSymbol.ControlPointIds.BottomCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case PreparationSymbol.ControlPointIds.MiddleCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						break;
					case ControlPointId.Reference:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						break;
					default:
						Assert.Fail("Unexpected control point id in class '{0}': {1}", shape.Type.FullName, id);
						break;
				}
			}
		}


		private void TestControlPointCapabilities(ManualInputSymbol shape, IEnumerable<ControlPointId> controlPointsFromShape) {
			// Check whether all points have the expected capabilities
			foreach (ControlPointId id in controlPointsFromShape) {
				switch (id) {
					case ManualInputSymbol.ControlPointIds.TopLeftControlPoint:
					case ManualInputSymbol.ControlPointIds.TopRightControlPoint:
					case ManualInputSymbol.ControlPointIds.BottomLeftControlPoint:
					case ManualInputSymbol.ControlPointIds.BottomRightControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case ManualInputSymbol.ControlPointIds.TopCenterControlPoint:
					case ManualInputSymbol.ControlPointIds.MiddleLeftControlPoint:
					case ManualInputSymbol.ControlPointIds.MiddleRightControlPoint:
					case ManualInputSymbol.ControlPointIds.BottomCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case ManualInputSymbol.ControlPointIds.MiddleCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						break;
					case ControlPointId.Reference:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						break;
					default:
						Assert.Fail("Unexpected control point id in class '{0}': {1}", shape.Type.FullName, id);
						break;
				}
			}
		}


		private void TestControlPointCapabilities(CoreSymbol shape, IEnumerable<ControlPointId> controlPointsFromShape) {
			// Check whether all points have the expected capabilities
			foreach (ControlPointId id in controlPointsFromShape) {
				switch (id) {
					case CoreSymbol.ControlPointIds.TopLeftControlPoint:
					case CoreSymbol.ControlPointIds.TopRightControlPoint:
					case CoreSymbol.ControlPointIds.BottomLeftControlPoint:
					case CoreSymbol.ControlPointIds.BottomRightControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case CoreSymbol.ControlPointIds.TopCenterControlPoint:
					case CoreSymbol.ControlPointIds.MiddleLeftControlPoint:
					case CoreSymbol.ControlPointIds.MiddleRightControlPoint:
					case CoreSymbol.ControlPointIds.BottomCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case CoreSymbol.ControlPointIds.MiddleCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						break;
					case ControlPointId.Reference:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						break;
					default:
						Assert.Fail("Unexpected control point id in class '{0}': {1}", shape.Type.FullName, id);
						break;
				}
			}
		}


		private void TestControlPointCapabilities(DisplaySymbol shape, IEnumerable<ControlPointId> controlPointsFromShape) {
			// Check whether all points have the expected capabilities
			foreach (ControlPointId id in controlPointsFromShape) {
				switch (id) {
					case DisplaySymbol.ControlPointIds.TopLeftControlPoint:
					case DisplaySymbol.ControlPointIds.TopRightControlPoint:
					case DisplaySymbol.ControlPointIds.BottomLeftControlPoint:
					case DisplaySymbol.ControlPointIds.BottomRightControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case DisplaySymbol.ControlPointIds.TopCenterControlPoint:
					case DisplaySymbol.ControlPointIds.MiddleLeftControlPoint:
					case DisplaySymbol.ControlPointIds.MiddleRightControlPoint:
					case DisplaySymbol.ControlPointIds.BottomCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case DisplaySymbol.ControlPointIds.MiddleCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						break;
					case ControlPointId.Reference:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						break;
					default:
						Assert.Fail("Unexpected control point id in class '{0}': {1}", shape.Type.FullName, id);
						break;
				}
			}
		}


		private void TestControlPointCapabilities(TapeSymbol shape, IEnumerable<ControlPointId> controlPointsFromShape) {
			// Check whether all points have the expected capabilities
			foreach (ControlPointId id in controlPointsFromShape) {
				switch (id) {
					case TapeSymbol.ControlPointIds.TopLeftControlPoint:
					case TapeSymbol.ControlPointIds.TopRightControlPoint:
					case TapeSymbol.ControlPointIds.BottomLeftControlPoint:
					case TapeSymbol.ControlPointIds.BottomRightControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case TapeSymbol.ControlPointIds.TopCenterControlPoint:
					case TapeSymbol.ControlPointIds.MiddleLeftControlPoint:
					case TapeSymbol.ControlPointIds.MiddleRightControlPoint:
					case TapeSymbol.ControlPointIds.BottomCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case TapeSymbol.ControlPointIds.MiddleCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						break;
					case ControlPointId.Reference:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						break;
					default:
						Assert.Fail("Unexpected control point id in class '{0}': {1}", shape.Type.FullName, id);
						break;
				}
			}
		}


		private void TestControlPointCapabilities(ManualOperationSymbol shape, IEnumerable<ControlPointId> controlPointsFromShape) {
			// Check whether all points have the expected capabilities
			foreach (ControlPointId id in controlPointsFromShape) {
				switch (id) {
					case ManualOperationSymbol.ControlPointIds.TopLeftControlPoint:
					case ManualOperationSymbol.ControlPointIds.TopRightControlPoint:
					case ManualOperationSymbol.ControlPointIds.BottomLeftControlPoint:
					case ManualOperationSymbol.ControlPointIds.BottomRightControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case ManualOperationSymbol.ControlPointIds.TopCenterControlPoint:
					case ManualOperationSymbol.ControlPointIds.MiddleLeftControlPoint:
					case ManualOperationSymbol.ControlPointIds.MiddleRightControlPoint:
					case ManualOperationSymbol.ControlPointIds.BottomCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case ManualOperationSymbol.ControlPointIds.MiddleCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						break;
					case ControlPointId.Reference:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						break;
					default:
						Assert.Fail("Unexpected control point id in class '{0}': {1}", shape.Type.FullName, id);
						break;
				}
			}
		}


		private void TestControlPointCapabilities(CollateSymbol shape, IEnumerable<ControlPointId> controlPointsFromShape) {
			// Check whether all points have the expected capabilities
			foreach (ControlPointId id in controlPointsFromShape) {
				switch (id) {
					case CollateSymbol.ControlPointIds.TopLeftControlPoint:
					case CollateSymbol.ControlPointIds.TopRightControlPoint:
					case CollateSymbol.ControlPointIds.BottomLeftControlPoint:
					case CollateSymbol.ControlPointIds.BottomRightControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case CollateSymbol.ControlPointIds.TopCenterControlPoint:
					case CollateSymbol.ControlPointIds.MiddleLeftControlPoint:
					case CollateSymbol.ControlPointIds.MiddleRightControlPoint:
					case CollateSymbol.ControlPointIds.BottomCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case CollateSymbol.ControlPointIds.MiddleCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						break;
					case ControlPointId.Reference:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						break;
					default:
						Assert.Fail("Unexpected control point id in class '{0}': {1}", shape.Type.FullName, id);
						break;
				}
			}
		}


		private void TestControlPointCapabilities(CardSymbol shape, IEnumerable<ControlPointId> controlPointsFromShape) {
			// Check whether all points have the expected capabilities
			foreach (ControlPointId id in controlPointsFromShape) {
				switch (id) {
					case CardSymbol.ControlPointIds.TopLeftControlPoint:
					case CardSymbol.ControlPointIds.TopRightControlPoint:
					case CardSymbol.ControlPointIds.BottomLeftControlPoint:
					case CardSymbol.ControlPointIds.BottomRightControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case CardSymbol.ControlPointIds.TopCenterControlPoint:
					case CardSymbol.ControlPointIds.MiddleLeftControlPoint:
					case CardSymbol.ControlPointIds.MiddleRightControlPoint:
					case CardSymbol.ControlPointIds.BottomCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case CardSymbol.ControlPointIds.MiddleCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						break;
					case ControlPointId.Reference:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						break;
					default:
						Assert.Fail("Unexpected control point id in class '{0}': {1}", shape.Type.FullName, id);
						break;
				}
			}
		}


		private void TestControlPointCapabilities(ConnectorSymbol shape, IEnumerable<ControlPointId> controlPointsFromShape) {
			// Check whether all points have the expected capabilities
			foreach (ControlPointId id in controlPointsFromShape) {
				switch (id) {
					case ConnectorSymbol.ControlPointIds.TopLeftControlPoint:
					case ConnectorSymbol.ControlPointIds.TopRightControlPoint:
					case ConnectorSymbol.ControlPointIds.BottomLeftControlPoint:
					case ConnectorSymbol.ControlPointIds.BottomRightControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case ConnectorSymbol.ControlPointIds.TopCenterControlPoint:
					case ConnectorSymbol.ControlPointIds.MiddleLeftControlPoint:
					case ConnectorSymbol.ControlPointIds.MiddleRightControlPoint:
					case ConnectorSymbol.ControlPointIds.BottomCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case ConnectorSymbol.ControlPointIds.MiddleCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						break;
					case ControlPointId.Reference:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						break;
					default:
						Assert.Fail("Unexpected control point id in class '{0}': {1}", shape.Type.FullName, id);
						break;
				}
			}
		}


		private void TestControlPointCapabilities(TapeStorageSymbol shape, IEnumerable<ControlPointId> controlPointsFromShape) {
			// Check whether all points have the expected capabilities
			foreach (ControlPointId id in controlPointsFromShape) {
				switch (id) {
					case TapeStorageSymbol.ControlPointIds.TopLeftControlPoint:
					case TapeStorageSymbol.ControlPointIds.TopRightControlPoint:
					case TapeStorageSymbol.ControlPointIds.BottomLeftControlPoint:
					case TapeStorageSymbol.ControlPointIds.BottomRightControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case TapeStorageSymbol.ControlPointIds.TopCenterControlPoint:
					case TapeStorageSymbol.ControlPointIds.MiddleLeftControlPoint:
					case TapeStorageSymbol.ControlPointIds.MiddleRightControlPoint:
					case TapeStorageSymbol.ControlPointIds.BottomCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case TapeStorageSymbol.ControlPointIds.MiddleCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						break;
					case ControlPointId.Reference:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						break;
					default:
						Assert.Fail("Unexpected control point id in class '{0}': {1}", shape.Type.FullName, id);
						break;
				}
			}
		}


		private void TestControlPointCapabilities(DecisionSymbol shape, IEnumerable<ControlPointId> controlPointsFromShape) {
			// Check whether all points have the expected capabilities
			foreach (ControlPointId id in controlPointsFromShape) {
				switch (id) {
					case DecisionSymbol.ControlPointIds.TopLeftControlPoint:
					case DecisionSymbol.ControlPointIds.TopRightControlPoint:
					case DecisionSymbol.ControlPointIds.BottomLeftControlPoint:
					case DecisionSymbol.ControlPointIds.BottomRightControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case DecisionSymbol.ControlPointIds.TopCenterControlPoint:
					case DecisionSymbol.ControlPointIds.MiddleLeftControlPoint:
					case DecisionSymbol.ControlPointIds.MiddleRightControlPoint:
					case DecisionSymbol.ControlPointIds.BottomCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case DecisionSymbol.ControlPointIds.MiddleCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						break;
					case ControlPointId.Reference:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						break;
					default:
						Assert.Fail("Unexpected control point id in class '{0}': {1}", shape.Type.FullName, id);
						break;
				}
			}
		}


		private void TestControlPointCapabilities(SortSymbol shape, IEnumerable<ControlPointId> controlPointsFromShape) {
			// Check whether all points have the expected capabilities
			foreach (ControlPointId id in controlPointsFromShape) {
				switch (id) {
					case SortSymbol.ControlPointIds.TopLeftControlPoint:
					case SortSymbol.ControlPointIds.TopRightControlPoint:
					case SortSymbol.ControlPointIds.BottomLeftControlPoint:
					case SortSymbol.ControlPointIds.BottomRightControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case SortSymbol.ControlPointIds.TopCenterControlPoint:
					case SortSymbol.ControlPointIds.MiddleLeftControlPoint:
					case SortSymbol.ControlPointIds.MiddleRightControlPoint:
					case SortSymbol.ControlPointIds.BottomCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case SortSymbol.ControlPointIds.MiddleCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						break;
					case ControlPointId.Reference:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						break;
					default:
						Assert.Fail("Unexpected control point id in class '{0}': {1}", shape.Type.FullName, id);
						break;
				}
			}
		}


		private void TestControlPointCapabilities(CommLinkSymbol shape, IEnumerable<ControlPointId> controlPointsFromShape) {
			// Check whether all points have the expected capabilities
			foreach (ControlPointId id in controlPointsFromShape) {
				switch (id) {
					case CommLinkSymbol.ControlPointIds.TopCenterControlPoint:
					case CommLinkSymbol.ControlPointIds.MiddleLeftControlPoint:
					case CommLinkSymbol.ControlPointIds.MiddleRightControlPoint:
					case CommLinkSymbol.ControlPointIds.BottomCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case CommLinkSymbol.ControlPointIds.MiddleCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						break;
					case ControlPointId.Reference:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						break;
					default:
						Assert.Fail("Unexpected control point id in class '{0}': {1}", shape.Type.FullName, id);
						break;
				}
			}
		}


		private void TestControlPointCapabilities(ExtractSymbol shape, IEnumerable<ControlPointId> controlPointsFromShape) {
			// Check whether all points have the expected capabilities
			foreach (ControlPointId id in controlPointsFromShape) {
				switch (id) {
					case ExtractSymbol.ControlPointIds.BottomLeftControlPoint:
					case ExtractSymbol.ControlPointIds.BottomRightControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case ExtractSymbol.ControlPointIds.TopCenterControlPoint:
					case ExtractSymbol.ControlPointIds.BottomCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case ExtractSymbol.ControlPointIds.LeftConnectionPoint:
					case ExtractSymbol.ControlPointIds.RightConnectionPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						break;
					case ExtractSymbol.ControlPointIds.BalancePointControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						break;
					case ControlPointId.Reference:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						break;
					default:
						Assert.Fail("Unexpected control point id in class '{0}': {1}", shape.Type.FullName, id);
						break;
				}
			}
		}


		private void TestControlPointCapabilities(MergeSymbol shape, IEnumerable<ControlPointId> controlPointsFromShape) {
			// Check whether all points have the expected capabilities
			foreach (ControlPointId id in controlPointsFromShape) {
				switch (id) {
					case MergeSymbol.ControlPointIds.TopLeftControlPoint:
					case MergeSymbol.ControlPointIds.TopRightControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case MergeSymbol.ControlPointIds.TopCenterControlPoint:
					case MergeSymbol.ControlPointIds.BottomCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case MergeSymbol.ControlPointIds.BalancePointControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						break;
					case MergeSymbol.ControlPointIds.LeftConnectionPoint:
					case MergeSymbol.ControlPointIds.RightConnectionPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						break;
					case ControlPointId.Reference:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						break;
					default:
						Assert.Fail("Unexpected control point id in class '{0}': {1}", shape.Type.FullName, id);
						break;
				}
			}
		}


		private void TestControlPointCapabilities(OfflineStorageSymbol shape, IEnumerable<ControlPointId> controlPointsFromShape) {
			// Check whether all points have the expected capabilities
			foreach (ControlPointId id in controlPointsFromShape) {
				switch (id) {
					case OfflineStorageSymbol.ControlPointIds.TopLeftControlPoint:
					case OfflineStorageSymbol.ControlPointIds.TopRightControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case OfflineStorageSymbol.ControlPointIds.TopCenterControlPoint:
					case OfflineStorageSymbol.ControlPointIds.BottomCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case ExtractSymbol.ControlPointIds.LeftConnectionPoint:
					case ExtractSymbol.ControlPointIds.RightConnectionPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						break;
					case OfflineStorageSymbol.ControlPointIds.BalancePointControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						break;
					case ControlPointId.Reference:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						break;
					default:
						Assert.Fail("Unexpected control point id in class '{0}': {1}", shape.Type.FullName, id);
						break;
				}
			}
		}

		#endregion


		#region [Private] Shape Class Specific Test Methods - GeneralShapes

		private void TestControlPointCapabilities(Polyline shape, IEnumerable<ControlPointId> controlPointsFromShape) {
			// Nothing to do here
		}


		private void TestControlPointCapabilities(RectangularLine shape, IEnumerable<ControlPointId> controlPointsFromShape) {
			// Nothing to do here
		}


		private void TestControlPointCapabilities(CircularArc shape, IEnumerable<ControlPointId> controlPointsFromShape) {
			// Nothing to do here
		}


		private void TestControlPointCapabilities(Text shape, IEnumerable<ControlPointId> controlPointsFromShape) {
			// Check whether all points have the expected capabilities
			for (int i = 0; i <= 1; ++i) {
				// Test with and without AutoSize
				shape.AutoSize = (i != 0);

				foreach (ControlPointId id in controlPointsFromShape) {
					switch (id) {
						case Text.ControlPointIds.TopLeftControlPoint:
						case Text.ControlPointIds.TopCenterControlPoint:
						case Text.ControlPointIds.TopRightControlPoint:
						case Text.ControlPointIds.MiddleLeftControlPoint:
						case Text.ControlPointIds.MiddleRightControlPoint:
						case Text.ControlPointIds.BottomLeftControlPoint:
						case Text.ControlPointIds.BottomCenterControlPoint:
						case Text.ControlPointIds.BottomRightControlPoint:
							Assert.AreEqual<bool>(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize), shape.AutoSize);
							break;
						case Text.ControlPointIds.MiddleCenterControlPoint:
						case ControlPointId.Reference:
							Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
							Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
							Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
							break;
						default:
							Assert.Fail("Unexpected control point id in class '{0}': {1}", shape.Type.FullName, id);
							break;
					}
				}
			}
		}


		private void TestControlPointCapabilities(Label shape, IEnumerable<ControlPointId> controlPointsFromShape) {
			// Check whether all points have the expected capabilities
			for (int i = 0; i <= 1; ++i) {
				// Test with and without AutoSize
				shape.AutoSize = (i != 0);

				foreach (ControlPointId id in controlPointsFromShape) {
					switch (id) {
						case Label.ControlPointIds.TopLeftControlPoint:
						case Label.ControlPointIds.TopCenterControlPoint:
						case Label.ControlPointIds.TopRightControlPoint:
						case Label.ControlPointIds.MiddleLeftControlPoint:
						case Label.ControlPointIds.MiddleRightControlPoint:
						case Label.ControlPointIds.BottomLeftControlPoint:
						case Label.ControlPointIds.BottomCenterControlPoint:
						case Label.ControlPointIds.BottomRightControlPoint:
							Assert.AreEqual<bool>(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize), shape.AutoSize);
							break;
						case Label.ControlPointIds.MiddleCenterControlPoint:
						case ControlPointId.Reference:
							Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
							Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
							Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
							break;
						case Label.ControlPointIds.GlueControlPoint:
							Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
							Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Glue));
							break;
						default:
							Assert.Fail("Unexpected control point id in class '{0}': {1}", shape.Type.FullName, id);
							break;
					}
				}
			}
		}


		private void TestControlPointCapabilities(RegularPolygone shape, IEnumerable<ControlPointId> controlPointsFromShape) {
			// Nothing to do here
		}


		private void TestControlPointCapabilities(FreeTriangle shape, IEnumerable<ControlPointId> controlPointsFromShape) {
			// Nothing to do here
		}


		private void TestControlPointCapabilities(IsoscelesTriangle shape, IEnumerable<ControlPointId> controlPointsFromShape) {
			// Check whether all points have the expected capabilities
			foreach (ControlPointId id in controlPointsFromShape) {
				switch (id) {
					case IsoscelesTriangle.ControlPointIds.TopCenterControlPoint:
					case IsoscelesTriangle.ControlPointIds.BottomLeftControlPoint:
					case IsoscelesTriangle.ControlPointIds.BottomCenterControlPoint:
					case IsoscelesTriangle.ControlPointIds.BottomRightControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case IsoscelesTriangle.ControlPointIds.BalancePointControlPoint:
					case ControlPointId.Reference:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						break;
					default:
						Assert.Fail("Unexpected control point id in class '{0}': {1}", shape.Type.FullName, id);
						break;
				}
			}
		}


		private void TestControlPointCapabilities(Circle shape, IEnumerable<ControlPointId> controlPointsFromShape) {
			// Check whether all points have the expected capabilities
			foreach (ControlPointId id in controlPointsFromShape) {
				switch (id) {
					case Circle.ControlPointIds.TopLeftControlPoint:
					case Circle.ControlPointIds.TopRightControlPoint:
					case Circle.ControlPointIds.BottomLeftControlPoint:
					case Circle.ControlPointIds.BottomRightControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case Circle.ControlPointIds.TopCenterControlPoint:
					case Circle.ControlPointIds.MiddleLeftControlPoint:
					case Circle.ControlPointIds.MiddleRightControlPoint:
					case Circle.ControlPointIds.BottomCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case Circle.ControlPointIds.TopLeftConnectionPoint:
					case Circle.ControlPointIds.TopRightConnectionPoint:
					case Circle.ControlPointIds.BottomLeftConnectionPoint:
					case Circle.ControlPointIds.BottomRightConnectionPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						break;
					case Circle.ControlPointIds.MiddleCenterControlPoint:
					case ControlPointId.Reference:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						break;
					default:
						Assert.Fail("Unexpected control point id in class '{0}': {1}", shape.Type.FullName, id);
						break;
				}
			}
		}


		private void TestControlPointCapabilities(Ellipse shape, IEnumerable<ControlPointId> controlPointsFromShape) {
			// Check whether all points have the expected capabilities
			foreach (ControlPointId id in controlPointsFromShape) {
				switch (id) {
					case Ellipse.ControlPointIds.TopLeftControlPoint:
					case Ellipse.ControlPointIds.TopRightControlPoint:
					case Ellipse.ControlPointIds.BottomLeftControlPoint:
					case Ellipse.ControlPointIds.BottomRightControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case Ellipse.ControlPointIds.TopCenterControlPoint:
					case Ellipse.ControlPointIds.MiddleLeftControlPoint:
					case Ellipse.ControlPointIds.MiddleRightControlPoint:
					case Ellipse.ControlPointIds.BottomCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case Ellipse.ControlPointIds.TopLeftConnectionPoint:
					case Ellipse.ControlPointIds.TopRightConnectionPoint:
					case Ellipse.ControlPointIds.BottomLeftConnectionPoint:
					case Ellipse.ControlPointIds.BottomRightConnectionPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						break;
					case Ellipse.ControlPointIds.MiddleCenterControlPoint:
					case ControlPointId.Reference:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						break;
					default:
						Assert.Fail("Unexpected control point id in class '{0}': {1}", shape.Type.FullName, id);
						break;
				}
			}
		}

		
		private void TestControlPointCapabilities(Square shape, IEnumerable<ControlPointId> controlPointsFromShape) {
			// Check whether all points have the expected capabilities
			foreach (ControlPointId id in controlPointsFromShape) {
				switch (id) {
					case Square.ControlPointIds.TopLeftControlPoint:
					case Square.ControlPointIds.TopCenterControlPoint:
					case Square.ControlPointIds.TopRightControlPoint:
					case Square.ControlPointIds.MiddleLeftControlPoint:
					case Square.ControlPointIds.MiddleRightControlPoint:
					case Square.ControlPointIds.BottomLeftControlPoint:
					case Square.ControlPointIds.BottomCenterControlPoint:
					case Square.ControlPointIds.BottomRightControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case Square.ControlPointIds.MiddleCenterControlPoint:
					case ControlPointId.Reference:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						break;
					default:
						Assert.Fail("Unexpected control point id in class '{0}': {1}", shape.Type.FullName, id);
						break;
				}
			}
		}


		private void TestControlPointCapabilities(Box shape, IEnumerable<ControlPointId> controlPointsFromShape) {
			// Check whether all points have the expected capabilities
			foreach (ControlPointId id in controlPointsFromShape) {
				switch (id) {
					case Box.ControlPointIds.TopLeftControlPoint:
					case Box.ControlPointIds.TopCenterControlPoint:
					case Box.ControlPointIds.TopRightControlPoint:
					case Box.ControlPointIds.MiddleLeftControlPoint:
					case Box.ControlPointIds.MiddleRightControlPoint:
					case Box.ControlPointIds.BottomLeftControlPoint:
					case Box.ControlPointIds.BottomCenterControlPoint:
					case Box.ControlPointIds.BottomRightControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case Box.ControlPointIds.MiddleCenterControlPoint:
					case ControlPointId.Reference:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						break;
					default:
						Assert.Fail("Unexpected control point id in class '{0}': {1}", shape.Type.FullName, id);
						break;
				}
			}
		}


		private void TestControlPointCapabilities(RoundedBox shape, IEnumerable<ControlPointId> controlPointsFromShape) {
			// Check whether all points have the expected capabilities
			foreach (ControlPointId id in controlPointsFromShape) {
				switch (id) {
					case RoundedBox.ControlPointIds.TopLeftControlPoint:
					case RoundedBox.ControlPointIds.TopCenterControlPoint:
					case RoundedBox.ControlPointIds.TopRightControlPoint:
					case RoundedBox.ControlPointIds.MiddleLeftControlPoint:
					case RoundedBox.ControlPointIds.MiddleRightControlPoint:
					case RoundedBox.ControlPointIds.BottomLeftControlPoint:
					case RoundedBox.ControlPointIds.BottomCenterControlPoint:
					case RoundedBox.ControlPointIds.BottomRightControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case RoundedBox.ControlPointIds.MiddleCenterControlPoint:
					case ControlPointId.Reference:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						break;
					default:
						Assert.Fail("Unexpected control point id in class '{0}': {1}", shape.Type.FullName, id);
						break;
				}
			}
		}


		private void TestControlPointCapabilities(Diamond shape, IEnumerable<ControlPointId> controlPointsFromShape) {
			// Check whether all points have the expected capabilities
			foreach (ControlPointId id in controlPointsFromShape) {
				switch (id) {
					case Diamond.ControlPointIds.TopLeftControlPoint:
					case Diamond.ControlPointIds.TopRightControlPoint:
					case Diamond.ControlPointIds.BottomLeftControlPoint:
					case Diamond.ControlPointIds.BottomRightControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case Diamond.ControlPointIds.TopCenterControlPoint:
					case Diamond.ControlPointIds.MiddleLeftControlPoint:
					case Diamond.ControlPointIds.MiddleRightControlPoint:
					case Diamond.ControlPointIds.BottomCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case Diamond.ControlPointIds.TopLeftConnectionPoint:
					case Diamond.ControlPointIds.TopRightConnectionPoint:
					case Diamond.ControlPointIds.BottomLeftConnectionPoint:
					case Diamond.ControlPointIds.BottomRightConnectionPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						break;
					case Diamond.ControlPointIds.MiddleCenterControlPoint:
					case ControlPointId.Reference:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						break;
					default:
						Assert.Fail("Unexpected control point id in class '{0}': {1}", shape.Type.FullName, id);
						break;
				}
			}
		}


		private void TestControlPointCapabilities(ThickArrow shape, IEnumerable<ControlPointId> controlPointsFromShape) {
			// Check whether all points have the expected capabilities
			foreach (ControlPointId id in controlPointsFromShape) {
				switch (id) {
					case ThickArrow.ControlPointIds.ArrowTipControlPoint:
					case ThickArrow.ControlPointIds.BodyEndControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case ThickArrow.ControlPointIds.ArrowTopControlPoint:
					case ThickArrow.ControlPointIds.ArrowBottomControlPoint:
					case ThickArrow.ControlPointIds.BodyTopControlPoint:
					case ThickArrow.ControlPointIds.BodyBottomControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case ThickArrow.ControlPointIds.MiddleCenterControlPoint:
					case ControlPointId.Reference:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						break;
					default:
						Assert.Fail("Unexpected control point id in class '{0}': {1}", shape.Type.FullName, id);
						break;
				}
			}
		}


		private void TestControlPointCapabilities(Picture shape, IEnumerable<ControlPointId> controlPointsFromShape) {
			// Check whether all points have the expected capabilities
			foreach (ControlPointId id in controlPointsFromShape) {
				switch (id) {
					case Picture.ControlPointIds.TopLeftControlPoint:
					case Picture.ControlPointIds.TopCenterControlPoint:
					case Picture.ControlPointIds.TopRightControlPoint:
					case Picture.ControlPointIds.MiddleLeftControlPoint:
					case Picture.ControlPointIds.MiddleRightControlPoint:
					case Picture.ControlPointIds.BottomLeftControlPoint:
					case Picture.ControlPointIds.BottomCenterControlPoint:
					case Picture.ControlPointIds.BottomRightControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case Picture.ControlPointIds.MiddleCenterControlPoint:
					case ControlPointId.Reference:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						break;
					default:
						Assert.Fail("Unexpected control point id in class '{0}': {1}", shape.Type.FullName, id);
						break;
				}
			}
		}

		#endregion


		#region [Private] Shape Class Specific Test Methods - SoftwareArchitectureShapes

		private void TestControlPointCapabilities(DataFlowArrow shape, IEnumerable<ControlPointId> controlPointsFromShape) {
			// Nothing to do here
		}

		private void TestControlPointCapabilities(DependencyArrow shape, IEnumerable<ControlPointId> controlPointsFromShape) {
			// Nothing to do here
		}

		private void TestControlPointCapabilities(DatabaseSymbol shape, IEnumerable<ControlPointId> controlPointsFromShape) {
			// Check whether all points have the expected capabilities
			foreach (ControlPointId id in controlPointsFromShape) {
				switch (id) {
					case DatabaseSymbol.ControlPointIds.TopLeftControlPoint:
					case DatabaseSymbol.ControlPointIds.TopCenterControlPoint:
					case DatabaseSymbol.ControlPointIds.TopRightControlPoint:
					case DatabaseSymbol.ControlPointIds.MiddleLeftControlPoint:
					case DatabaseSymbol.ControlPointIds.MiddleRightControlPoint:
					case DatabaseSymbol.ControlPointIds.BottomLeftControlPoint:
					case DatabaseSymbol.ControlPointIds.BottomCenterControlPoint:
					case DatabaseSymbol.ControlPointIds.BottomRightControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case DatabaseSymbol.ControlPointIds.MiddleCenterControlPoint:
					case ControlPointId.Reference:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						break;
					default:
						Assert.Fail("Unexpected control point id in class '{0}': {1}", shape.Type.FullName, id);
						break;
				}
			}
		}


		private void TestControlPointCapabilities(EntitySymbol shape, IEnumerable<ControlPointId> controlPointsFromShape) {
			// Check whether all points have the expected capabilities
			foreach (ControlPointId id in controlPointsFromShape) {
				switch (id) {
					case EntitySymbol.ControlPointIds.TopCenterControlPoint:
					case EntitySymbol.ControlPointIds.BottomCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case EntitySymbol.ControlPointIds.TopLeftControlPoint:
					case EntitySymbol.ControlPointIds.TopRightControlPoint:
					case EntitySymbol.ControlPointIds.MiddleLeftControlPoint:
					case EntitySymbol.ControlPointIds.MiddleRightControlPoint:
					case EntitySymbol.ControlPointIds.BottomLeftControlPoint:
					case EntitySymbol.ControlPointIds.BottomRightControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case EntitySymbol.ControlPointIds.MiddleCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						break;
					case ControlPointId.Reference:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						break;
					default:
						Assert.Fail("Unexpected control point id in class '{0}': {1}", shape.Type.FullName, id);
						break;
				}
			}
		}


		private void TestControlPointCapabilities(AnnotationSymbol shape, IEnumerable<ControlPointId> controlPointsFromShape) {
			// Check whether all points have the expected capabilities
			foreach (ControlPointId id in controlPointsFromShape) {
				switch (id) {
					case AnnotationSymbol.ControlPointIds.TopLeftControlPoint:
					case AnnotationSymbol.ControlPointIds.TopCenterControlPoint:
					case AnnotationSymbol.ControlPointIds.TopRightControlPoint:
					case AnnotationSymbol.ControlPointIds.MiddleLeftControlPoint:
					case AnnotationSymbol.ControlPointIds.MiddleRightControlPoint:
					case AnnotationSymbol.ControlPointIds.BottomLeftControlPoint:
					case AnnotationSymbol.ControlPointIds.BottomCenterControlPoint:
					case AnnotationSymbol.ControlPointIds.BottomRightControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case AnnotationSymbol.ControlPointIds.MiddleCenterControlPoint:
					case ControlPointId.Reference:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						break;
					default:
						Assert.Fail("Unexpected control point id in class '{0}': {1}", shape.Type.FullName, id);
						break;
				}
			}
		}


		private void TestControlPointCapabilities(CloudSymbol shape, IEnumerable<ControlPointId> controlPointsFromShape) {
			// Check whether all points have the expected capabilities
			foreach (ControlPointId id in controlPointsFromShape) {
				switch (id) {
					case CloudSymbol.ControlPointIds.TopLeftControlPoint:
					case CloudSymbol.ControlPointIds.TopCenterControlPoint:
					case CloudSymbol.ControlPointIds.TopRightControlPoint:
					case CloudSymbol.ControlPointIds.MiddleLeftControlPoint:
					case CloudSymbol.ControlPointIds.MiddleRightControlPoint:
					case CloudSymbol.ControlPointIds.BottomLeftControlPoint:
					case CloudSymbol.ControlPointIds.BottomCenterControlPoint:
					case CloudSymbol.ControlPointIds.BottomRightControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case CloudSymbol.ControlPointIds.TopLeftConnectionPoint:
					case CloudSymbol.ControlPointIds.TopRightConnectionPoint:
					case CloudSymbol.ControlPointIds.MiddleLeftConnectionPoint:
					case CloudSymbol.ControlPointIds.MiddleRightConnectionPoint:
					case CloudSymbol.ControlPointIds.BottomLeftConnectionPoint:
					case CloudSymbol.ControlPointIds.BottomRightConnectionPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						break;
					case CloudSymbol.ControlPointIds.MiddleCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						break;
					case ControlPointId.Reference:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						break;
					default:
						Assert.Fail("Unexpected control point id in class '{0}': {1}", shape.Type.FullName, id);
						break;
				}
			}
		}


		private void TestControlPointCapabilities(ClassSymbol shape, IEnumerable<ControlPointId> controlPointsFromShape) {
			// Check whether all points have the expected capabilities
			foreach (ControlPointId id in controlPointsFromShape) {
				switch (id) {
					case ClassSymbol.ControlPointIds.TopCenterControlPoint:
					case ClassSymbol.ControlPointIds.BottomCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case ClassSymbol.ControlPointIds.TopLeftControlPoint:
					case ClassSymbol.ControlPointIds.TopRightControlPoint:
					case ClassSymbol.ControlPointIds.MiddleLeftControlPoint:
					case ClassSymbol.ControlPointIds.MiddleRightControlPoint:
					case ClassSymbol.ControlPointIds.BottomLeftControlPoint:
					case ClassSymbol.ControlPointIds.BottomRightControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case ClassSymbol.ControlPointIds.MiddleCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						break;
					case ControlPointId.Reference:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						break;
					default:
						Assert.Fail("Unexpected control point id in class '{0}': {1}", shape.Type.FullName, id);
						break;
				}
			}
		}


		private void TestControlPointCapabilities(ComponentSymbol shape, IEnumerable<ControlPointId> controlPointsFromShape) {
			// Check whether all points have the expected capabilities
			foreach (ControlPointId id in controlPointsFromShape) {
				switch (id) {
					case ComponentSymbol.ControlPointIds.TopLeftControlPoint:
					case ComponentSymbol.ControlPointIds.BottomLeftControlPoint:
					case ComponentSymbol.ControlPointIds.MiddleLeftControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case ComponentSymbol.ControlPointIds.TopCenterControlPoint:
					case ComponentSymbol.ControlPointIds.TopRightControlPoint:
					case ComponentSymbol.ControlPointIds.MiddleRightControlPoint:
					case ComponentSymbol.ControlPointIds.BottomCenterControlPoint:
					case ComponentSymbol.ControlPointIds.BottomRightControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case ComponentSymbol.ControlPointIds.MiddleCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						break;
					case ControlPointId.Reference:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						break;
					case ComponentSymbol.ControlPointIds.TopLeftConnectionPoint:
					case ComponentSymbol.ControlPointIds.MiddleLeftConnectionPoint:
					case ComponentSymbol.ControlPointIds.BottomLeftConnectionPoint:
					case ComponentSymbol.ControlPointIds.UpperDockTopConnectionPoint:
					case ComponentSymbol.ControlPointIds.UpperDockMiddleConnectionPoint:
					case ComponentSymbol.ControlPointIds.UpperDockBottomConnectionPoint:
					case ComponentSymbol.ControlPointIds.LowerDockTopConnectionPoint:
					case ComponentSymbol.ControlPointIds.LowerDockMiddleConnectionPoint:
					case ComponentSymbol.ControlPointIds.LowerDockBottomConnectionPoint:
					case ComponentSymbol.ControlPointIds.TopConnectionPoint1:
					case ComponentSymbol.ControlPointIds.TopConnectionPoint2:
					case ComponentSymbol.ControlPointIds.TopConnectionPoint3:
					case ComponentSymbol.ControlPointIds.TopConnectionPoint4:
					case ComponentSymbol.ControlPointIds.TopConnectionPoint5:
					case ComponentSymbol.ControlPointIds.RightConnectionPoint1:
					case ComponentSymbol.ControlPointIds.RightConnectionPoint2:
					case ComponentSymbol.ControlPointIds.RightConnectionPoint3:
					case ComponentSymbol.ControlPointIds.RightConnectionPoint4:
					case ComponentSymbol.ControlPointIds.RightConnectionPoint5:
					case ComponentSymbol.ControlPointIds.RightConnectionPoint6:
					case ComponentSymbol.ControlPointIds.BottomConnectionPoint1:
					case ComponentSymbol.ControlPointIds.BottomConnectionPoint2:
					case ComponentSymbol.ControlPointIds.BottomConnectionPoint3:
					case ComponentSymbol.ControlPointIds.BottomConnectionPoint4:
					case ComponentSymbol.ControlPointIds.BottomConnectionPoint5:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						break;
					default:
						Assert.Fail("Unexpected control point id in class '{0}': {1}", shape.Type.FullName, id);
						break;
				}
			}
		}


		private void TestControlPointCapabilities(Dataweb.NShape.SoftwareArchitectureShapes.DocumentSymbol shape, IEnumerable<ControlPointId> controlPointsFromShape) {
			// Check whether all points have the expected capabilities
			foreach (ControlPointId id in controlPointsFromShape) {
				switch (id) {
					case Dataweb.NShape.SoftwareArchitectureShapes.DocumentSymbol.ControlPointIds.TopCenterControlPoint:
					case Dataweb.NShape.SoftwareArchitectureShapes.DocumentSymbol.ControlPointIds.MiddleLeftControlPoint:
					case Dataweb.NShape.SoftwareArchitectureShapes.DocumentSymbol.ControlPointIds.MiddleRightControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case Dataweb.NShape.SoftwareArchitectureShapes.DocumentSymbol.ControlPointIds.TopLeftControlPoint:
					case Dataweb.NShape.SoftwareArchitectureShapes.DocumentSymbol.ControlPointIds.TopRightControlPoint:
					case Dataweb.NShape.SoftwareArchitectureShapes.DocumentSymbol.ControlPointIds.BottomLeftControlPoint:
					case Dataweb.NShape.SoftwareArchitectureShapes.DocumentSymbol.ControlPointIds.BottomCenterControlPoint:
					case Dataweb.NShape.SoftwareArchitectureShapes.DocumentSymbol.ControlPointIds.BottomRightControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case Dataweb.NShape.SoftwareArchitectureShapes.DocumentSymbol.ControlPointIds.MiddleCenterControlPoint:
					case ControlPointId.Reference:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Rotate));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						break;
					default:
						Assert.Fail("Unexpected control point id in class '{0}': {1}", shape.Type.FullName, id);
						break;
				}
			}
		}


		private void TestControlPointCapabilities(InterfaceSymbol shape, IEnumerable<ControlPointId> controlPointsFromShape) {
			// Nothing to do here
		}


		private void TestControlPointCapabilities(InterfaceUsageSymbol shape, IEnumerable<ControlPointId> controlPointsFromShape) {
			// Nothing to do here
		}


		private void TestControlPointCapabilities(VectorImage shape, IEnumerable<ControlPointId> controlPointsFromShape) {
			// Check whether all points have the expected capabilities
			foreach (ControlPointId id in controlPointsFromShape) {
				switch (id) {
					case VectorImage.ControlPointIds.TopLeftControlPoint:
					case VectorImage.ControlPointIds.BottomCenterControlPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Resize));
						break;
					case VectorImage.ControlPointIds.BottomCenterConnectionPoint:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						break;
					case VectorImage.ControlPointIds.MiddleCenterControlPoint:
					case ControlPointId.Reference:
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Reference));
						Assert.IsTrue(shape.HasControlPointCapability(id, ControlPointCapabilities.Connect));
						break;
					default:
						Assert.Fail("Unexpected control point id in class '{0}': {1}", shape.Type.FullName, id);
						break;
				}
			}
		}

		#endregion


		#region [Private] Helper Methods

		private IEnumerable<int> GetControlPointsFromType(Type type) {
			// ToDo: This does not work yet -> Repair!
			System.Reflection.MemberInfo[] memberinfo = type.FindMembers(MemberTypes.NestedType, BindingFlags.Public, new System.Reflection.MemberFilter(SearchCriteria), "ControlPointIds");
			if (memberinfo != null) {
			}
			
			Type controlPointIdsType = type.GetNestedType("ControlPointIds", BindingFlags.Public | BindingFlags.FlattenHierarchy);
			if (controlPointIdsType != null) {
				foreach (FieldInfo fi in controlPointIdsType.GetFields(BindingFlags.Public | BindingFlags.GetField))
					if (fi.IsStatic && fi.IsInitOnly)
						yield return (int)fi.GetValue(null);
			}

		}


		public static bool SearchCriteria(MemberInfo objMemberInfo, Object objSearch) {
			if (objMemberInfo.Name == objSearch.ToString())
				return true;
			else
				return false;
		}

		#endregion

	}
}
