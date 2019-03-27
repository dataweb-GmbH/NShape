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

using System.Reflection;

using Dataweb.NShape.Advanced;


namespace Dataweb.NShape.SoftwareArchitectureShapes {

	public static class NShapeLibraryInitializer {

		public static void Initialize(IRegistrar registrar) {
			// Register library
			registrar.RegisterLibrary(namespaceName, preferredRepositoryVersion);
			// Register shape types
			registrar.RegisterShapeType(new ShapeType("DataFlow", namespaceName, namespaceName,
				delegate(ShapeType shapeType, Template t) { return new DataFlowArrow(shapeType, t); },
				DataFlowArrow.GetPropertyDefinitions));
			registrar.RegisterShapeType(new ShapeType("Dependency", namespaceName, namespaceName,
				delegate(ShapeType shapeType, Template t) { return new DependencyArrow(shapeType, t); },
				DependencyArrow.GetPropertyDefinitions));
			registrar.RegisterShapeType(new ShapeType("Database", namespaceName, namespaceName,
				delegate(ShapeType shapeType, Template t) { return new DatabaseSymbol(shapeType, t); },
				DatabaseSymbol.GetPropertyDefinitions));
			registrar.RegisterShapeType(new ShapeType("Entity", namespaceName, namespaceName,
				delegate(ShapeType shapeType, Template t) { return new EntitySymbol(shapeType, t); },
				EntitySymbol.GetPropertyDefinitions));
			registrar.RegisterShapeType(new ShapeType("Annotation", namespaceName, namespaceName,
				delegate(ShapeType shapeType, Template t) { return new AnnotationSymbol(shapeType, t); },
				AnnotationSymbol.GetPropertyDefinitions));
			registrar.RegisterShapeType(new ShapeType("Cloud", namespaceName, namespaceName,
				delegate(ShapeType shapeType, Template t) { return new CloudSymbol(shapeType, t); },
				CloudSymbol.GetPropertyDefinitions));
			registrar.RegisterShapeType(new ShapeType("Class", namespaceName, namespaceName,
				delegate(ShapeType shapeType, Template t) { return new ClassSymbol(shapeType, t); },
				ClassSymbol.GetPropertyDefinitions));
			registrar.RegisterShapeType(new ShapeType("Component", namespaceName, namespaceName,
				delegate(ShapeType shapeType, Template t) { return new ComponentSymbol(shapeType, t); },
				ComponentSymbol.GetPropertyDefinitions));
			registrar.RegisterShapeType(new ShapeType("Document", namespaceName, namespaceName,
				delegate(ShapeType shapeType, Template t) { return new DocumentSymbol(shapeType, t); },
				DocumentSymbol.GetPropertyDefinitions));
			registrar.RegisterShapeType(new ShapeType("Interface", namespaceName, namespaceName,
				delegate(ShapeType shapeType, Template t) { return new InterfaceSymbol(shapeType, t); },
				InterfaceSymbol.GetPropertyDefinitions));
			registrar.RegisterShapeType(new ShapeType("InterfaceUsage", namespaceName, namespaceName,
				delegate(ShapeType shapeType, Template t) { return new InterfaceUsageSymbol(shapeType, t); },
				InterfaceUsageSymbol.GetPropertyDefinitions));
			registrar.RegisterShapeType(new ShapeType("Server", namespaceName, namespaceName,
				delegate(ShapeType shapeType, Template t) {
					VectorImage result = VectorImage.CreateInstance(shapeType, t,
						"Dataweb.NShape.SoftwareArchitectureShapes.Resources.Tower.emf", Assembly.GetExecutingAssembly());
					result.Text = "Server";
					return result;
				}, VectorImage.GetPropertyDefinitions));
			registrar.RegisterShapeType(new ShapeType("RTU", namespaceName, namespaceName,
				delegate(ShapeType shapeType, Template t) {
					VectorImage result = VectorImage.CreateInstance(shapeType, t,
						"Dataweb.NShape.SoftwareArchitectureShapes.Resources.RTU.emf", Assembly.GetExecutingAssembly());
					result.Text = "RTU";
					return result;
				}, VectorImage.GetPropertyDefinitions));
			registrar.RegisterShapeType(new ShapeType("Actor", namespaceName, namespaceName,
				delegate(ShapeType shapeType, Template t) {
					VectorImage result = VectorImage.CreateInstance(shapeType, t,
						"Dataweb.NShape.SoftwareArchitectureShapes.Resources.Actor.emf", Assembly.GetExecutingAssembly());
					result.Text = "Actor";
					return result;
				}, VectorImage.GetPropertyDefinitions));
			registrar.RegisterShapeType(new ShapeType("Monitor", namespaceName, namespaceName,
				delegate(ShapeType shapeType, Template t) {
					VectorImage result = VectorImage.CreateInstance(shapeType, t,
						"Dataweb.NShape.SoftwareArchitectureShapes.Resources.Monitor.emf", Assembly.GetExecutingAssembly());
					result.Text = "Monitor";
					return result;
				}, VectorImage.GetPropertyDefinitions));
			registrar.RegisterShapeType(new ShapeType("PC", namespaceName, namespaceName,
				delegate(ShapeType shapeType, Template t) {
					VectorImage result = VectorImage.CreateInstance(shapeType, t,
						"Dataweb.NShape.SoftwareArchitectureShapes.Resources.Desktop.emf", Assembly.GetExecutingAssembly());
					result.Text = "PC";
					return result;
				}, VectorImage.GetPropertyDefinitions));
			registrar.RegisterShapeType(new ShapeType("Tower", namespaceName, namespaceName,
				delegate(ShapeType shapeType, Template t) {
					VectorImage result = VectorImage.CreateInstance(shapeType, t,
						"Dataweb.NShape.SoftwareArchitectureShapes.Resources.Tower.emf", Assembly.GetExecutingAssembly());
					result.Text = "Tower";
					return result;
				}, VectorImage.GetPropertyDefinitions));
			registrar.RegisterShapeType(new ShapeType("Laptop", namespaceName, namespaceName,
				delegate(ShapeType shapeType, Template t) {
					VectorImage result = VectorImage.CreateInstance(shapeType, t,
						"Dataweb.NShape.SoftwareArchitectureShapes.Resources.Laptop.emf", Assembly.GetExecutingAssembly());
					result.Text = "Notebook";
					return result;
				}, VectorImage.GetPropertyDefinitions));
		}


		private const string namespaceName = "SoftwareArchitectureShapes";
		private const int preferredRepositoryVersion = 6;
	}

}
