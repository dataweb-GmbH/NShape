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


namespace Dataweb.NShape.ElectricalShapes {

	public static class NShapeLibraryInitializer {

		public static void Initialize(IRegistrar registrar) {
			registrar.RegisterLibrary(libraryName, preferredRepositoryVersion);
			registrar.RegisterShapeType(new ShapeType("BusBar", libraryName, libraryName,
				delegate(ShapeType shapeType, Template t) { return new BusBarSymbol(shapeType, t); },
				BusBarSymbol.GetPropertyDefinitions, Dataweb.NShape.ElectricalShapes.Properties.Resources.ShaperReferenceHorizontalBar));
			registrar.RegisterShapeType(new ShapeType("Disconnector", libraryName, libraryName,
				delegate(ShapeType shapeType, Template t) { return new DisconnectorSymbol(shapeType, t); },
				DisconnectorSymbol.GetPropertyDefinitions, Dataweb.NShape.ElectricalShapes.Properties.Resources.ShaperReferenceCircleWithBar));
			registrar.RegisterShapeType(new ShapeType("AutoDisconnector", libraryName, libraryName,
				delegate(ShapeType shapeType, Template t) { return new AutoDisconnectorSymbol(shapeType, t); },
				AutoDisconnectorSymbol.GetPropertyDefinitions, Dataweb.NShape.ElectricalShapes.Properties.Resources.ShaperReferenceCircleWithBar));
			registrar.RegisterShapeType(new ShapeType("AutoSwitch", libraryName, libraryName,
				delegate(ShapeType shapeType, Template t) { return new AutoSwitchSymbol(shapeType, t); },
				AutoSwitchSymbol.GetPropertyDefinitions, Dataweb.NShape.ElectricalShapes.Properties.Resources.ShaperReferenceQuadrangle));
			registrar.RegisterShapeType(new ShapeType("Switch", libraryName, libraryName,
				delegate(ShapeType shapeType, Template t) { return new SwitchSymbol(shapeType, t); },
				SwitchSymbol.GetPropertyDefinitions, Dataweb.NShape.ElectricalShapes.Properties.Resources.ShaperReferenceQuadrangle));
			registrar.RegisterShapeType(new ShapeType("Transformer", libraryName, libraryName,
				delegate(ShapeType shapeType, Template t) { return new TransformerSymbol(shapeType, t); },
				TransformerSymbol.GetPropertyDefinitions, Dataweb.NShape.ElectricalShapes.Properties.Resources.ShaperReferenceDoubleCircle));
			registrar.RegisterShapeType(new ShapeType("Earth", libraryName, libraryName,
				delegate(ShapeType shapeType, Template t) { return new EarthSymbol(shapeType, t); },
				EarthSymbol.GetPropertyDefinitions, Dataweb.NShape.ElectricalShapes.Properties.Resources.ShaperReferenceEarthSymbol));
			registrar.RegisterShapeType(new ShapeType("Feeder", libraryName, libraryName,
				delegate(ShapeType shapeType, Template t) { return new FeederSymbol(shapeType, t); },
				FeederSymbol.GetPropertyDefinitions, Dataweb.NShape.ElectricalShapes.Properties.Resources.ShaperReferenceEarthSymbol));
			registrar.RegisterShapeType(new ShapeType("Rectifier", libraryName, libraryName,
				delegate(ShapeType shapeType, Template t) { return new RectifierSymbol(shapeType, t); },
				RectifierSymbol.GetPropertyDefinitions, Dataweb.NShape.ElectricalShapes.Properties.Resources.ShaperReferenceEarthSymbol));
			registrar.RegisterShapeType(new ShapeType("DisconnectingPoint", libraryName, libraryName,
				delegate(ShapeType shapeType, Template t) { return new DisconnectingPoint(shapeType, t); },
				DisconnectingPoint.GetPropertyDefinitions, Dataweb.NShape.ElectricalShapes.Properties.Resources.ShaperReferenceEarthSymbol));
		}


		private const string libraryName = "ElectricalShapes";
		private const int preferredRepositoryVersion = 6;
	}

}
